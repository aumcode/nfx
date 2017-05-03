/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NFX.IO.FileSystem.S3.V4
{
  public static class S3V4HttpHelpers
  {
    public static IDictionary<string, string> FilterHeaders(this IDictionary<string, string> headers)
    {
      headers.Remove("Host");
      headers.Remove("content-type");
      headers.Remove("content-length");

      return headers;
    }

    public static HttpWebRequest ConstructWebRequest(this Uri uri, string method, Stream contentStream, IDictionary<string, string> headers,
      int timeout = 0)
    {
      if (headers != null)
        headers.FilterHeaders();

      HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

      var t = timeout == 0 ? NFX.Web.WebSettings.WebDavDefaultTimeoutMs : timeout;
      if (t > 0) request.Timeout = t;

      request.Method = method;
      request.ContentType = "text/plain";

      if (method == "PUT")
        request.ContentLength = contentStream.Length;

      foreach (var kvp in headers)
        request.Headers.Add(kvp.Key, kvp.Value);

      if (method == "PUT")
      {
        using (Stream requestStream = request.GetRequestStream())
        {
          contentStream.Position = 0;
          contentStream.CopyTo(requestStream);

          requestStream.Flush();
          requestStream.Close();
        }
      }

      return request;
    }

    public static string GetResponseStr(this HttpWebRequest request)
    {
      string responseBody = string.Empty;

      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          using (var responseStream = response.GetResponseStream())
          {
            using (var reader = new StreamReader(responseStream))
            {
              responseBody = reader.ReadToEnd();
            }
          }
        }
      }

      return responseBody;
    }

    public static void GetResponseBytes(this HttpWebRequest request, Stream stream)
    {
      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          using (var responseStream = response.GetResponseStream())
          {
            responseStream.CopyTo(stream);
          }
        }
        else
          throw new Exception( string.Format("Response status code={0} description=\"{1}\"", response.StatusCode, response.StatusDescription));
      }
    }

    public static IDictionary<string, string> GetHeaders(this HttpWebRequest request, HttpStatusCode successStatus = HttpStatusCode.OK)
    {
      string responseBody = string.Empty;

      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {
        if (response.StatusCode == successStatus)
        {
          IDictionary<string, string> headers = response.Headers.AllKeys.ToDictionary(k => k, k => response.Headers[k]);
          return headers;
        }

        throw new NFXException(NFX.Web.StringConsts.DELETE_MODIFY_ERROR + typeof(S3V4) + ".GetHeaders: folder couldn't be deleted");
      }
    }

  }
}
