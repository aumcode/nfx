/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

using NFX.IO;
using NFX.Log;
using NFX.Web;
using NFX.Collections;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.ApplicationModel;
using NFX.Serialization.JSON;

namespace NFX.Wave
{
  /// <summary>
  /// Represents Response object used to generate web responses to client
  /// </summary>
  public sealed class Response : DisposableObject
  {
    #region CONSTS

      public const int DEFAULT_DOWNLOAD_BUFFER_SIZE = 128*1024;
      public const int MIN_DOWNLOAD_BUFFER_SIZE = 1*1024;
      public const int MAX_DOWNLOAD_BUFFER_SIZE = 1024*1024;


      public const int MAX_DOWNLOAD_FILE_SIZE = 32 * 1024 * 1024;
    #endregion


    #region .ctor
      internal Response(WorkContext work, HttpListenerResponse netResponse)
      {
        Work = work;
        m_NetResponse = netResponse;
        m_NetResponse.Headers[HttpResponseHeader.Server] = Work.Server.Name;
      }

      protected override void Destructor()
      {
         try
         {
           try
           {
             var srv = Work.Server;
             var ie = srv.m_InstrumentationEnabled;

             if (ie && m_WasWrittenTo)
              Interlocked.Increment(ref srv.m_Stat_WorkContextWrittenResponse);

             stowClientVars();

             if (m_Buffer!=null)
             {
                var sz = m_Buffer.Position;
                m_NetResponse.ContentLength64 = sz;
                m_NetResponse.OutputStream.Write(m_Buffer.GetBuffer(), 0, (int)sz);
                m_Buffer = null;

                if (ie)
                {
                  Interlocked.Increment(ref srv.m_Stat_WorkContextBufferedResponse);
                  Interlocked.Add(ref srv.m_Stat_WorkContextBufferedResponseBytes, sz);
                }
             }
           }
           finally
           {
             m_NetResponse.OutputStream.Close();
 	           m_NetResponse.Close();
           }
         }
         catch(HttpListenerException lerror)
         {
           if (lerror.ErrorCode!=64)//specified net name no longer available
            Work.Log(MessageType.Error, lerror.ToMessageWithType(), "Response.dctor()", lerror);
         }
      }
    #endregion

    #region Fields
      private HttpListenerResponse m_NetResponse;
      private bool m_WasWrittenTo;
      private MemoryStream m_Buffer;

      private Dictionary<string, string> m_ClientVars;
      private bool m_ClientVarsChanged;

      public readonly WorkContext Work;
    #endregion

    #region Properties
      /// <summary>
      /// Returns true if some output has been performed
      /// </summary>
      public bool WasWrittenTo { get{ return m_WasWrittenTo;}}

      /// <summary>
      /// Determines whether the content is buffered locally. This property can not be set after
      ///  the response has been written to
      /// </summary>
      public bool Buffered
      {
        get {return !m_NetResponse.SendChunked;}
        set
        {
          if (m_WasWrittenTo)
            throw new WaveException(StringConsts.RESPONSE_WAS_WRITTEN_TO_ERROR+".Buffered.set()");
          m_NetResponse.SendChunked = !value;
        }
      }

      /// <summary>
      /// Gets/sets content encoding
      /// </summary>
      public Encoding Encoding { get { return m_NetResponse.ContentEncoding ?? Encoding.UTF8; } set {m_NetResponse.ContentEncoding = value;}}

      /// <summary>
      /// Http status code
      /// </summary>
      public int StatusCode
      {
        get { return m_NetResponse.StatusCode;}
        set { m_NetResponse.StatusCode = value;}
      }

      /// <summary>
      /// Http status description
      /// </summary>
      public string StatusDescription
      {
        get { return m_NetResponse.StatusDescription;}
        set { m_NetResponse.StatusDescription = value;}
      }

      /// <summary>
      /// Http content type
      /// </summary>
      public string ContentType
      {
        get { return m_NetResponse.ContentType;}
        set { m_NetResponse.ContentType = value;}
      }

      /// <summary>
      /// Returns http headers of the response
      /// </summary>
      public WebHeaderCollection Headers { get { return m_NetResponse.Headers;} }


      /// <summary>
      /// Returns true if current request processing cycle has changed the client var
      /// </summary>
      public bool ClientVarsChanged { get{ return m_ClientVarsChanged;} }


    #endregion

    #region Public

      /// <summary>
      /// Writes an object into response as string
      /// </summary>
      public void Write(object content)
      {
        if (content==null) return;

        Write(content.ToString());
      }

      /// <summary>
      /// Writes an object into response as string
      /// </summary>
      public void WriteLine(object content)
      {
        if (content==null) return;

        WriteLine(content.ToString());
      }

      /// <summary>
      /// Writes a string into response
      /// </summary>
      public void Write(string content)
      {
        if (content==null) return;

        setWasWrittenTo();
        byte[] buffer = Encoding.GetBytes(content);
        getStream().Write(buffer, 0, buffer.Length);
      }

      /// <summary>
      /// Writes a string into response
      /// </summary>
      public void WriteLine(string content)
      {
        Write((content??string.Empty)+"\n");
      }

      /// <summary>
      /// Writes an object as JSON. Does nothing if object is null
      /// </summary>
      public void WriteJSON(object data, JSONWritingOptions options = null)
      {
        if (data==null) return;
        setWasWrittenTo();
        m_NetResponse.ContentType = NFX.Web.ContentType.JSON;
        JSONWriter.Write(data, new NonClosingStreamWrap( getStream() ), options, Encoding);
      }

      /// <summary>
      /// Write the file to the client so client can download it. May set Buffered=false to use chunked encoding for big files
      /// </summary>
      public void WriteFile(string localFileName, int bufferSize = DEFAULT_DOWNLOAD_BUFFER_SIZE, bool attachment = false)
      {
        if (localFileName.IsNullOrWhiteSpace())
          throw new WaveException(StringConsts.ARGUMENT_ERROR+"Response.WriteFile(localFileName==null|empty)");

        var fi = new FileInfo(localFileName);

        if (!fi.Exists)
          throw new WaveException(StringConsts.RESPONSE_WRITE_FILE_DOES_NOT_EXIST_ERROR.Args(localFileName));

        var fsize = fi.Length;
        if (Buffered && fsize>MAX_DOWNLOAD_FILE_SIZE)
          throw new WaveException(StringConsts.RESPONSE_WRITE_FILE_OVER_MAX_SIZE_ERROR.Args(localFileName, MAX_DOWNLOAD_FILE_SIZE));

        var ext = Path.GetExtension(localFileName);
        setWasWrittenTo();
        m_NetResponse.ContentType = NFX.Web.ContentType.ExtensionToContentType(ext, NFX.Web.ContentType.BINARY);

        if (attachment)
          m_NetResponse.Headers.Add(WebConsts.HTTP_HDR_CONTENT_DISPOSITION, "attachment; filename={0}".Args(fi.Name));

        if (bufferSize<MIN_DOWNLOAD_BUFFER_SIZE) bufferSize=MIN_DOWNLOAD_BUFFER_SIZE;
        else if (bufferSize>MAX_DOWNLOAD_BUFFER_SIZE) bufferSize=MAX_DOWNLOAD_BUFFER_SIZE;

        using(var fs = new FileStream(localFileName, FileMode.Open, FileAccess.Read))
        {

          var dest = getStream();
          if (dest is MemoryStream)
          {
            var ms = ((MemoryStream)dest);
            if (ms.Position==0)
            {
              ms.SetLength(fsize);//pre-allocate memory stream
              ms.Position = ms.Length;
              fs.Read(ms.GetBuffer(), 0, (int)ms.Position);
              return;
            }
          }

          fs.CopyTo(dest, bufferSize);
        }
      }


      /// <summary>
      /// Write the contents of the stream to the client so client can download it. May set Buffered=false to use chunked encoding for big files
      /// </summary>
      public void WriteStream(Stream stream, int bufferSize = DEFAULT_DOWNLOAD_BUFFER_SIZE, string attachmentName = null)
      {
        if (stream==null)
          throw new WaveException(StringConsts.ARGUMENT_ERROR+"Response.WriteStream(stream==null)");

        setWasWrittenTo();

        if (attachmentName.IsNotNullOrWhiteSpace())
          m_NetResponse.Headers.Add(WebConsts.HTTP_HDR_CONTENT_DISPOSITION, "attachment; filename={0}".Args(attachmentName));

        if (bufferSize<MIN_DOWNLOAD_BUFFER_SIZE) bufferSize=MIN_DOWNLOAD_BUFFER_SIZE;
        else if (bufferSize>MAX_DOWNLOAD_BUFFER_SIZE) bufferSize=MAX_DOWNLOAD_BUFFER_SIZE;


        var dest = getStream();
        stream.CopyTo(dest, bufferSize);
      }


      /// <summary>
      /// Returns output stream for direct output and marks response as beeing written into
      /// </summary>
      public Stream GetDirectOutputStreamForWriting()
      {
        setWasWrittenTo();
        return getStream();
      }

      /// <summary>
      /// Cancels the buffered content. Throws if the response is not Buffered
      /// </summary>
      public void CancelBuffered()
      {
        if (!WasWrittenTo) return;
        if (!Buffered)
          throw new WaveException(StringConsts.RESPONSE_CANCEL_NON_BUFFERED_ERROR);
        m_Buffer = null;
        m_WasWrittenTo = false;
      }

      /// <summary>
      /// RESERVED FOR FUTURE USE. Flushes the internally buffered content
      /// </summary>
      public void Flush()
      {
       //There is currently (2014.03.26) no way to flush the HttpListenerResponse.OutputStream
      }


      /// <summary>
      /// Configures response with redirect status and headers. This method DOES NOT ABORT the work pipeline,so
      ///  the processing of filters and handlers continues unless 'work.Aborted = true' is issued in code.
      ///  See also 'RedirectAndAbort(url)'
      /// </summary>
      public void Redirect(string url, WebConsts.RedirectCode code = WebConsts.RedirectCode.Found_302)
      {
        //20160707 DKh m_NetResponse.Redirect(url);
        m_NetResponse.Headers.Set(HttpResponseHeader.Location, url);
        m_NetResponse.StatusCode        = WebConsts.GetRedirectStatusCode(code);
        m_NetResponse.StatusDescription = WebConsts.GetRedirectStatusDescription(code);
      }

      /// <summary>
      /// Configures response with redirect status and headers. This method also aborts the work pipeline,so
      ///  the processing of filters and handlers does not continue. See also 'Redirect(url)'
      /// </summary>
      public void RedirectAndAbort(string url, WebConsts.RedirectCode code = WebConsts.RedirectCode.Found_302)
      {
        this.Redirect(url, code);
        Work.Aborted = true;
      }

      /// <summary>
      /// Adds Http header
      /// </summary>
      public void AddHeader(string name, string value)
      {
        m_NetResponse.AddHeader(name, value);
      }

      /// <summary>
      /// Appends cookie to the response
      /// </summary>
      public void AppendCookie(Cookie cookie)
      {
        m_NetResponse.AppendCookie(cookie);
      }

      /// <summary>
      /// Sets headers so all downstream, layers (browsers, proxies) do not cache response.
      /// If Force==true(default) then overrides existing headers with no cache.
      /// Returns true when headers were set
      /// </summary>
      public bool SetNoCacheHeaders(bool force = true)
      {
        if (!force && m_NetResponse.Headers[HttpResponseHeader.CacheControl].IsNotNullOrWhiteSpace())
          return false;

        m_NetResponse.Headers[HttpResponseHeader.CacheControl] = "no-cache, no-store, must-revalidate";
        m_NetResponse.Headers[HttpResponseHeader.Pragma] = "no-cache";
        m_NetResponse.Headers[HttpResponseHeader.Expires] = "0";
        m_NetResponse.Headers[HttpResponseHeader.Vary] = "*";
        return true;
      }

      /// <summary>
      /// Sets max-age private cache header
      /// </summary>
      public void SetPrivateMaxAgeCacheHeader(int maxAgeSec, string vary = null)
      {
        m_NetResponse.Headers[System.Net.HttpResponseHeader.CacheControl] = "private, max-age="+maxAgeSec+", must-revalidate";

        if (vary.IsNotNullOrWhiteSpace()) //20160602 Opan+Dkh
          m_NetResponse.Headers[HttpResponseHeader.Vary] = vary;
      }

      /// <summary>
      /// Provides access to client state object which gets persisted as a cookie.
      /// Client states need to be used instead of cookies because of some HttpListener+Browser limitations
      /// that can not parse multiple cookies set into one Set-Cookie header
      /// </summary>
      public IEnumerable<string> GetClientVarNames()
      {
         loadClientVars();
         return m_ClientVars.Keys;
      }

      /// <summary>
      /// Provides access to client state object which gets persisted as a cookie.
      /// Client states need to be used instead of cookies because of some HttpListener+Browser limitations
      /// that can not parse multiple cookies set into one Set-Cookie header
      /// </summary>
      public string GetClientVar(string key)
      {
          if (key==null) return string.Empty;
          loadClientVars();
          string result;
          if (m_ClientVars.TryGetValue(key, out result)) return result;
          return string.Empty;
      }

      /// <summary>
      /// Provides access to client state object which gets persisted as a cookie.
      /// Client states need to be used instead of cookies because of some HttpListener+Browser limitations
      /// that can not parse multiple cookies set into one Set-Cookie header.
      /// Pass null for value to delete the var form the collection. The values are generally expected to be base64 encoded by the caller
      /// </summary>
      public void SetClientVar(string key, string value)
      {
          if (m_WasWrittenTo && !Buffered)
            throw new WaveException(StringConsts.RESPONSE_WAS_WRITTEN_TO_ERROR+".SetClientVar()");

          if (key==null) key = string.Empty;


          if (key.IndexOf(KEY_DELIMITER)>=0)
            throw new WaveException(StringConsts.ARGUMENT_ERROR+".SetClientVar(key has "+KEY_DELIMITER+")");



          if (value!=null && value.IndexOf(VAR_DELIMITER)>=0)
            throw new WaveException(StringConsts.ARGUMENT_ERROR+".SetClientVar(value has "+VAR_DELIMITER+")");


          loadClientVars();

          if (value==null)
          {
            m_ClientVarsChanged = m_ClientVarsChanged | m_ClientVars.Remove(key);
            return;
          }

          string existing;
          if (m_ClientVars.TryGetValue(key, out existing))
           if ( string.Equals(existing, value, StringComparison.OrdinalIgnoreCase)) return;
          m_ClientVars[key] = value;
          m_ClientVarsChanged = true;
      }


    #endregion


    #region .pvt
      private Stream getStream()
      {
        if (m_Buffer!=null) return m_Buffer;
        if (Buffered)
        {
          m_Buffer = new MemoryStream();
          return m_Buffer;
        }
        return m_NetResponse.OutputStream;
      }

      private void setWasWrittenTo()
      {
        m_WasWrittenTo = true;
        if (!Buffered) stowClientVars();
      }


      private const char KEY_DELIMITER = '~';
      private const char VAR_DELIMITER = '|';
      private const int MAX_COOKIE_LENGTH = 4020;

      private void loadClientVars()
      {
        if (m_ClientVars!=null) return;

        m_ClientVars = new Dictionary<string,string>();

        var cookie = Work.Request.Cookies[Work.Server.ClientVarsCookieName];

        if (cookie==null)  return;

        //Format:   name»value´name2=value2
        var cv = cookie.Value;
        if (cv.IsNullOrWhiteSpace()) return;


        var segs = cv.Split(VAR_DELIMITER);
        foreach(var seg in segs)
        {
          if (seg.Length==0) continue;
          var i = seg.IndexOf(KEY_DELIMITER);
          if (i<=0 || i==seg.Length)
            m_ClientVars[seg] = string.Empty;
          else
            m_ClientVars[seg.Substring(0, i)] = seg.Substring(i+1);
        }
      }

      private void stowClientVars()
      {
        if (m_ClientVars==null || !m_ClientVarsChanged) return;

        m_ClientVarsChanged = false;

        var sb = new StringBuilder();
        var first = true;
        foreach(var kvp in m_ClientVars)
        {
         if (!first) sb.Append(VAR_DELIMITER);
         sb.Append(kvp.Key);
         sb.Append(KEY_DELIMITER);
         sb.Append(kvp.Value);
         first = false;
        }

        var cookieName = Work.Server.ClientVarsCookieName;
        var cv = sb.ToString();

        var total = cookieName.Length + cv.Length;
        if (total > MAX_COOKIE_LENGTH)
         throw new WaveException(StringConsts.CLIENT_VARS_LENGTH_OVER_LIMIT_ERROR.Args(total, MAX_COOKIE_LENGTH));

        AddHeader(WebConsts.HTTP_SET_COOKIE,
                             "{0}={1};path=/;expires={2};HttpOnly"
                           .Args(cookieName, cv, App.TimeSource.UTCNow.AddYears(100).DateTimeToHTTPCookieDateTime() ));
      }


    #endregion


  }
}
