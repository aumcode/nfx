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

using NFX.Web;
using NFX.Log;
using NFX.Collections;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.ApplicationModel;
using NFX.Serialization.JSON;
using NFX.Web.GeoLookup;
using NFX.DataAccess.CRUD;

namespace NFX.Wave
{
  /// <summary>
  /// Represents a context for request/response server processing in WAVE framework
  /// </summary>
  public class WorkContext : DisposableObject
  {
    #region .ctor/.dctor
      internal WorkContext(WaveServer server, HttpListenerContext listenerContext)
      {
        m_ID = Guid.NewGuid();
        m_Server = server;
        m_ListenerContext = listenerContext;
        m_Response = new Response(this, listenerContext.Response);

        ApplicationModel.ExecutionContext.__SetThreadLevelContext(this, m_Response, null);
        Interlocked.Increment(ref m_Server.m_Stat_WorkContextCtor);
      }

      /// <summary>
      /// Warning: if overridden, must call base otherwise semaphore will not get released
      /// </summary>
      protected override void Destructor()
      {
        if (m_Server.m_InstrumentationEnabled)
        {
          Interlocked.Increment(ref m_Server.m_Stat_WorkContextDctor);
          if (m_Aborted) Interlocked.Increment(ref m_Server.m_Stat_WorkContextAborted);
          if (m_Handled) Interlocked.Increment(ref m_Server.m_Stat_WorkContextHandled);
          if (m_NoDefaultAutoClose) Interlocked.Increment(ref m_Server.m_Stat_WorkContextNoDefaultClose);
        }

        ApplicationModel.ExecutionContext.__SetThreadLevelContext(null, null, null);
        ReleaseWorkSemaphore();
        m_Response.Dispose();
      }
    #endregion

    #region Fields
      private Guid m_ID;
      private WaveServer m_Server;
      private bool m_WorkSemaphoreReleased;

      private HttpListenerContext m_ListenerContext;
      private Response m_Response;

      internal Filters.SessionFilter m_SessionFilter;
      internal WaveSession m_Session;

      internal Filters.PortalFilter m_PortalFilter;
      internal Portal m_Portal;
      internal Theme m_PortalTheme;
      internal WorkMatch m_PortalMatch;
      internal JSONDataMap m_PortalMatchedVars;


      private object m_ItemsLock = new object();
      private ConcurrentDictionary<object, object> m_Items;

      internal WorkHandler m_Handler;

      private WorkMatch m_Match;
      private JSONDataMap m_MatchedVars;
                     /// <summary>
                     /// Internal method. Developers do not call
                     /// </summary>
                     internal void ___SetWorkMatch(WorkMatch match, JSONDataMap vars){m_Match = match; m_MatchedVars = vars;}

      private bool m_HasParsedRequestBody;
      private JSONDataMap m_RequestBodyAsJSONDataMap;
      private JSONDataMap m_WholeRequestAsJSONDataMap;

      internal bool m_Handled;
      private bool m_Aborted;

      private bool m_NoDefaultAutoClose;

      private GeoEntity m_GeoEntity;

    #endregion

    #region Properties


      /// <summary>
      /// Uniquely identifies the request
      /// </summary>
      public Guid ID{ get{ return m_ID;} }

      /// <summary>
      /// Returns the server this context is under
      /// </summary>
      public WaveServer Server { get { return m_Server;} }

      /// <summary>
      /// Returns true to indicate that work semaphore has been already released.
      /// It is not necessary to use this property or ReleaseWorkSemaphore() method as the framework does it
      ///  automatically in 99% cases. ReleaseWorkSemaphore() may need to be called from special places like HTTP streaming
      ///   servers that need to keep WorkContext instances open for a long time
      /// </summary>
      public bool WorkSemaphoreReleased { get{ return m_WorkSemaphoreReleased;}}


      /// <summary>
      /// Returns HttpListenerRequest object for this context
      /// </summary>
     //todo Wrap in Wave.Request object (just like Response)
      public HttpListenerRequest Request { get { return m_ListenerContext.Request;} }

      /// <summary>
      /// Returns Response object for this context
      /// </summary>
      public Response Response { get { return m_Response;} }

      /// <summary>
      /// Returns session that this context is linked with or null
      /// </summary>
      public WaveSession Session { get {return m_Session;} }

      /// <summary>
      /// Returns the first session filter which was injected in the processing line.
      /// It is the filter that manages the session state for this context
      /// </summary>
      public Filters.SessionFilter SessionFilter {get{ return m_SessionFilter;}}

      /// <summary>
      /// Returns true when the context was configured to support SessionFilter so Session can be injected
      /// </summary>
      public bool SupportsSession { get{ return m_SessionFilter!=null;}}

      /// <summary>
      /// Returns portal object for this request or null if no portal was injected
      /// </summary>
      public Portal Portal { get { return m_Portal;} }

               /// <summary>
               /// DEVELOPERS do not use!
               /// A hack method needed in some VERY RARE cases, like serving an error page form the filter which is out of portal scope.
               /// </summary>
               public void ___InternalInjectPortal(Portal portal = null,
                                                   Theme theme = null,
                                                   WorkMatch match = null,
                                                   JSONDataMap matchedVars = null)
                                                   {
                                                     m_Portal = portal;
                                                     m_PortalTheme = theme;
                                                     m_PortalMatch = match;
                                                     m_PortalMatchedVars = matchedVars;
                                                   }

      /// <summary>
      /// Returns the first portal filter which was injected in the processing line.
      /// It is the filter that manages the portals for this context
      /// </summary>
      public Filters.PortalFilter PortalFilter {get{ return m_PortalFilter;}}

      /// <summary>
      /// Returns matched that was made by portal filter or null
      /// </summary>
      public WorkMatch PortalMatch {get{ return m_PortalMatch;}}

      /// <summary>
      /// Gets/sets portal theme. This may be null as this is just a holder variable
      /// </summary>
      public Theme PortalTheme
      {
        get{ return m_PortalTheme ?? (m_Portal!=null ? m_Portal.DefaultTheme :  null);}
        set{ m_PortalTheme = value;}
      }


      /// <summary>
      /// Returns variables that have been extracted by WorkMatch when PortalFilter assigned portal.
      /// Returns null if no portal was matched
      /// </summary>
      public JSONDataMap PortalMatchedVars{  get { return m_PortalMatchedVars;}}


      /// <summary>
      /// Returns the work match instances that was made for this requested work or null if nothing was matched yet
      /// </summary>
      public WorkMatch Match {get{ return m_Match;}}

      /// <summary>
      /// Returns variables that have been extracted by WorkMatch when dispatcher assigned request to WorkHandler.
      /// If variables have not been assigned yet returns empty object
      /// </summary>
      public JSONDataMap MatchedVars
      {
        get
        {
          if (m_MatchedVars==null)
            m_MatchedVars = new JSONDataMap(false);

          return m_MatchedVars;
        }
      }

      /// <summary>
      /// Returns dynamic object that contains variables that have been extracted by WorkMatch when dispatcher assigned request to WorkHandler.
      /// If variables have not been assigned yet returns empty object
      /// </summary>
      public dynamic Matched{ get { return new JSONDynamicObject(MatchedVars);} }



      /// <summary>
      /// Fetches request body: multipart content, url encoded content, or JSON body into one JSONDataMap bag,
      /// or null if there is no body.
      /// The property does caching
      /// </summary>
      public JSONDataMap RequestBodyAsJSONDataMap
      {
        get
        {
          if (!m_HasParsedRequestBody)
          {
            m_RequestBodyAsJSONDataMap = ParseRequestBodyAsJSONDataMap();
            m_HasParsedRequestBody = true;
          }
          return m_RequestBodyAsJSONDataMap;
        }
      }

      /// <summary>
      /// Fetches matched vars, multipart content, url encoded content, or JSON body into one JSONDataMap bag.
      /// The property does caching
      /// </summary>
      public JSONDataMap WholeRequestAsJSONDataMap
      {
        get
        {
          if (m_WholeRequestAsJSONDataMap==null)
            m_WholeRequestAsJSONDataMap = GetWholeRequestAsJSONDataMap();
          return m_WholeRequestAsJSONDataMap;
        }
      }


      /// <summary>
      /// Provides a thread-safe dictionary of items. The underlying collection is lazily allocated
      /// </summary>
      public ConcurrentDictionary<object, object> Items
      {
          get
          {
            if (m_Items==null)
                lock(m_ItemsLock)
                {
                  if (m_Items==null)
                    m_Items = new ConcurrentDictionary<object,object>(4, 16);
                }
            return m_Items;
          }
      }

      /// <summary>
      /// Returns the work handler instance that was matched to perform work on this context or null if the match has not been made yet
      /// </summary>
      public WorkHandler Handler {get{ return m_Handler;}}


      /// <summary>
      /// Returns true when the work has been executed by the WorkHandler instance
      /// </summary>
      public bool Handled {get{return m_Handled;}}

      /// <summary>
      /// Indicates whether the work context is logically finished and its nested processing (i.e. through Filters/Handlers) should stop.
      /// For example, when some filter detects a special condition (judging by the request) and generates the response
      ///  and needs to abort the work request so it does no get filtered/processed anymore, it can set this property to true.
      /// This mechanism performs much better than throwing exceptions
      /// </summary>
      public bool Aborted
      {
        get {return m_Aborted;}
        set {m_Aborted = value;}
      }


      /// <summary>
      /// Generates short context description
      /// </summary>
      public string About
      {
        get
        {
          return "Work('{0}'@'{1}' -> {2} '{3}')".Args(Request.UserAgent, Request.RemoteEndPoint, Request.HttpMethod, Request.Url);
        }
      }

      /// <summary>
      /// Indicates whether the default dispatcher should close the WorkContext upon completion of async processing.
      /// This property may ONLY be set to TRUE IF Response.Buffered = false (chunked transfer) and Response has already been written to.
      /// When this property is set to true the WorkDispatcher will not auto dispose this WorkContext instance.
      /// This may be needed for a server that streams chat messages and some other thread manages the lifetime of this WorkContext.
      /// Keep in mind that alternative implementations of WorkDispatcher (derived classes that implement alternative threading/lifecycle)
      ///  may disregard this flag altogether
      /// </summary>
      public bool NoDefaultAutoClose
      {
        get { return m_NoDefaultAutoClose;}
        set
        {
          if ( value && (Response.Buffered==true || !Response.WasWrittenTo))
            throw new WaveException(StringConsts.WORK_NO_DEFAULT_AUTO_CLOSE_ERROR);

          m_NoDefaultAutoClose = value;
        }
      }

      /// <summary>
      /// Captures last error
      /// </summary>
      public Exception LastError{get; set;}

      /// <summary>
      /// Gets sets geo location information as detected by GeoLookupHandler.
      /// If Session context is injected then get/set passes through into session object
      /// </summary>
      public GeoEntity GeoEntity
      {
        get { return m_Session==null? m_GeoEntity : m_Session.GeoEntity;}
        set { if (m_Session==null)  m_GeoEntity = value; else  m_Session.GeoEntity = value;}
      }


         private bool? m_RequestedJSON;
      /// <summary>
      /// Returns true if client indicated in response that "application/json" is accepted
      /// </summary>
      public bool RequestedJSON
      {
        get
        {
          if (!m_RequestedJSON.HasValue)
            m_RequestedJSON = Request.AcceptTypes.Any(at => ContentType.JSON.EqualsOrdIgnoreCase(at));

          return m_RequestedJSON.Value;
        }
      }

      /// <summary>
      /// Indicates that request method id POST
      /// </summary>
      public bool IsPOST { get{ return Request.HttpMethod.EqualsOrdIgnoreCase("POST");}}

      /// <summary>
      /// Indicates that request method id GET
      /// </summary>
      public bool IsGET { get{ return Request.HttpMethod.EqualsOrdIgnoreCase("GET");}}

      /// <summary>
      /// Indicates that request method id PUT
      /// </summary>
      public bool IsPUT { get{ return Request.HttpMethod.EqualsOrdIgnoreCase("PUT");}}

      /// <summary>
      /// Indicates that request method id DELETE
      /// </summary>
      public bool IsDELETE { get{ return Request.HttpMethod.EqualsOrdIgnoreCase("DELETE");}}

      /// <summary>
      /// Indicates that request method id PATCH
      /// </summary>
      public bool IsPATCH { get{ return Request.HttpMethod.EqualsOrdIgnoreCase("PATCH");}}


    #endregion

    #region Public

      /// <summary>
      /// Releases work semaphore that throttles the processing of WorkContext instances.
      /// The WorkContext is released automatically in destructor, however there are cases when the semaphore release
      /// may be needed sooner, i.e. in a HTTP streaming application where work context instances are kept open indefinitely
      /// it may not be desirable to consider long-living work context instances as a throtteling factor.
      /// Returns true if semaphore was released, false if it was not released during this call as it was already released before
      /// </summary>
      public bool ReleaseWorkSemaphore()
      {
        if (m_Server!=null && m_Server.Running && !m_WorkSemaphoreReleased)
        {
          var workCount = m_Server.m_WorkSemaphore.Release();
          m_WorkSemaphoreReleased = true;
          if (m_Server.m_InstrumentationEnabled)
          {
            Interlocked.Increment(ref m_Server.m_Stat_WorkContextWorkSemaphoreRelease);
            Thread.VolatileWrite(ref m_Server.m_Stat_ServerWorkSemaphoreCount, workCount);
          }
          return true;
        }
        return false;
      }


      /// <summary>
      /// Ensures that session is injected if session filter is present in processing chain.
      /// If session is already available (Session!=null) then does nothing, otherwise
      /// fills Session poroperty with either NEW session (if onlyExisting=false(default)) if user supplied no session token,
      /// OR gets session from session store as defined by the first SessionFilter in the chain
      /// </summary>
      public WaveSession NeedsSession(bool onlyExisting = false)
      {
        if (m_Session!=null) return m_Session;

        Interlocked.Increment(ref m_Server.m_Stat_WorkContextNeedsSession);

        if (m_SessionFilter!=null)
          m_SessionFilter.FetchExistingOrMakeNewSession(this, onlyExisting);
        else
          throw new WaveException(StringConsts.SESSION_NOT_AVAILABLE_ERROR.Args(About));

        return m_Session;
      }


      /// <summary>
      /// Facilitates context-aware logging
      /// </summary>
      public void Log(MessageType type, string text, string from = null, Exception error = null, string pars = null, Guid? related = null)
      {
        var msg = new Message
        {
          Type = type,
          Topic = SysConsts.WAVE_LOG_TOPIC,
          From = from.IsNotNullOrWhiteSpace() ? from : About,
          Text = text,
          Exception = error ?? LastError,
          Parameters = pars
        };

        if (related.HasValue)
          msg.RelatedTo = related.Value;
        else
          msg.RelatedTo = this.m_ID;

        App.Log.Write(msg);
      }

      /// <summary>
      /// Returns true if the whole request (body or matched vars) contains any names matching any field names of the specified row
      /// </summary>
      public bool HasAnyVarsMatchingFieldNames(Row row)
      {
        if (row==null) return false;

        foreach(var fdef in row.Schema)
         if (WholeRequestAsJSONDataMap.ContainsKey(fdef.Name)) return true;

        return false;
      }

      public override string ToString()
      {
        return About;
      }
    #endregion


    #region Protected

      /// <summary>
      /// Converts request body and MatchedVars into a single JSONDataMap. Users should call WholeRequestAsJSONDataMap.get() as it caches the result
      /// </summary>
      protected virtual JSONDataMap GetWholeRequestAsJSONDataMap()
      {
        var body = this.RequestBodyAsJSONDataMap;

        if (body==null) return MatchedVars;

        var result = new JSONDataMap(false);
        result.Append(MatchedVars)
              .Append(body);
        return result;
      }

      /// <summary>
      /// This method is called only once as it touches the input streams
      /// </summary>
      protected virtual JSONDataMap ParseRequestBodyAsJSONDataMap()
      {
        if (!Request.HasEntityBody) return null;

        JSONDataMap result = null;

        var ctp = Request.ContentType;

        //Multipart
        if (ctp.IndexOf(ContentType.FORM_MULTIPART_ENCODED)>=0)
        {
          var boundary = Multipart.ParseContentType(ctp);
          var mp = Multipart.ReadFromStream(Request.InputStream, ref boundary, Request.ContentEncoding);
          result =  mp.ToJSONDataMap();
          // Multipart.ToJSONDataMap(Request.InputStream, ctp,  Request.ContentEncoding);
        }
        else //Form URL encoded
        if (ctp.IndexOf(ContentType.FORM_URL_ENCODED)>=0)
          result = JSONDataMap.FromURLEncodedStream(new NFX.IO.NonClosingStreamWrap(Request.InputStream),
                                                  Request.ContentEncoding);
        else//JSON
        if (ctp.IndexOf(ContentType.JSON)>=0)
          result = JSONReader.DeserializeDataObject(new NFX.IO.NonClosingStreamWrap(Request.InputStream),
                                                  Request.ContentEncoding) as JSONDataMap;

        return result;
      }

    #endregion

  }





}
