
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  1/14/2014 12:28:08 AM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Collections.Specialized;

using NFX;
using NFX.Environment;
using NFX.Serialization.JSON;
using System.Xml.Linq;

namespace NFX.Web
{
  public enum HTTPRequestMethod { GET, HEAD, POST, PUT, DELETE, CONNECT, OPTIONS, TRACE, PATCH};

  /// <summary>
  /// Stipulates contract for an entity that executes calls via WebClient
  /// </summary>
  public interface IWebClientCaller
  {
    /// <summary>
    /// Specifies timeout for web call. If zero then default is used
    /// </summary>
    int WebServiceCallTimeoutMs { get; }

    /// <summary>
    /// Specifies keepalive option for web request
    /// </summary>
    bool KeepAlive { get; }

    /// <summary>
    /// Specifies if pipelining is used for web request
    /// </summary>
    bool Pipelined { get; }
  }

  /// <summary>
  /// Facilitates working with external services provided via HTTP
  /// </summary>
  public static partial class WebClient
  {
    #region Inner classes

      /// <summary>
      /// Provides request parameters for making WebClient calls
      /// </summary>
      public sealed class RequestParams
      {
        #region Pub params

          public Uri Uri;
          public HTTPRequestMethod Method = HTTPRequestMethod.GET;
          public IDictionary<string, string> QueryParameters;
          public IDictionary<string, string> BodyParameters;
          public string Body;
          public IDictionary<string, string> Headers;
          public string ContentType = NFX.Web.ContentType.FORM_URL_ENCODED;
          public string AcceptType;
          public string UName, UPwd;
          public IWebClientCaller Caller;

          public bool HasCredentials { get { return UName.IsNotNullOrWhiteSpace(); }}

        #endregion

        #region Public

          public void Validate()
          {
            if (BodyParameters != null && BodyParameters.Count != 0 && Body != null)
              throw new NFXException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".Validate: BodyParameters<>null and Body<>null");
          }

        #endregion
      }

    #endregion

    #region Public

      public static string GetString(Uri uri,
        IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        string body = null,
        IDictionary<string, string> headers = null)
      {
        RequestParams request = new RequestParams()
        {
          Uri = uri,
          Caller = caller,
          Method = method,
          QueryParameters = queryParameters,
          BodyParameters = bodyParameters,
          Body = body,
          Headers = headers
        };

        return GetString(request);
      }

      public static byte[] GetData(Uri uri,
                                   IWebClientCaller caller, out string contentType,
                                   IDictionary<string, string> queryParameters = null,
                                   IDictionary<string, string> headers = null)
      {
        byte[] data = null;
        string localContentType = string.Empty;
        GetDataObject((c) => {
          data = c.DownloadData(uri);
          localContentType = c.ResponseHeaders[HttpResponseHeader.ContentType];
        }, uri, caller, queryParameters, headers);
        contentType = localContentType;
        return data;
      }

      public static void GetFile(string file,
                                 Uri uri,
                                 IWebClientCaller caller,
                                 IDictionary<string, string> queryParameters = null,
                                 IDictionary<string, string> headers = null)
      {
        GetDataObject((c) => { c.DownloadFile(uri, file); }, uri, caller, queryParameters, headers);
      }

      public static void GetDataObject(Action<System.Net.WebClient> action,
                                       Uri uri,
                                       IWebClientCaller caller,
                                       IDictionary<string, string> queryParameters = null,
                                       IDictionary<string, string> headers = null)
      {
        NameValueCollection queryParams;
        if (queryParameters != null)
        {
          queryParams = new NameValueCollection(queryParameters.Count);
          queryParameters.ForEach(kvp => queryParams.Add(kvp.Key, kvp.Value));
        }
        else
        {
          queryParams = new NameValueCollection();
        }

        var headerParams = new WebHeaderCollection();
        if (headers != null)
          headers.ForEach(kvp => headerParams.Add(kvp.Key, kvp.Value));

        using (var client = new WebClientTimeouted(caller))
        {
          client.Headers = headerParams;
          client.QueryString = queryParams;
          action(client);
        }
      }

      public static string GetString(RequestParams request)
      {
        if (request.Method != HTTPRequestMethod.GET && request.Method != HTTPRequestMethod.POST && request.Method != HTTPRequestMethod.PUT
           && request.Method != HTTPRequestMethod.DELETE)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(WebClient).Name + ".GetString(method=GET|POST)");

        NameValueCollection queryParams;
        if (request.QueryParameters != null)
        {
          queryParams = new NameValueCollection(request.QueryParameters.Count);
          request.QueryParameters.ForEach(kvp => queryParams.Add(kvp.Key, kvp.Value));
        }
        else
        {
          queryParams = new NameValueCollection();
        }

        NameValueCollection bodyParams;
        if (request.BodyParameters != null)
        {
          bodyParams = new NameValueCollection(request.BodyParameters.Count);
          request.BodyParameters.ForEach(kvp => bodyParams.Add(kvp.Key, kvp.Value));
        }
        else
        {
          bodyParams = new NameValueCollection();
        }

        WebHeaderCollection headerParams = prepareRequestHeaders(request);

        string responseStr;
        using (var client = new WebClientTimeouted(request.Caller))
        {
          if (request.HasCredentials)
            client.Credentials = new NetworkCredential(request.UName, request.UPwd);

          client.Headers = headerParams;
          client.QueryString = queryParams;
          if (request.Method == HTTPRequestMethod.GET)
          {
            responseStr = Uri.UnescapeDataString(client.DownloadString(request.Uri));
          }
          else if (request.Method == HTTPRequestMethod.PUT || request.Method == HTTPRequestMethod.DELETE)
          {
            responseStr = Uri.UnescapeDataString(client.UploadString(request.Uri, request.Method.ToString(), request.Body ?? string.Empty));
          }
          else
          {
            var body = request.Body ??
                       (request.BodyParameters != null ?
                        string.Join("&", request.BodyParameters.Select(p => p.Key + "=" + p.Value)) :
                        string.Empty);
            responseStr = Uri.UnescapeDataString(client.UploadString(request.Uri, body));
          }
        }

        return responseStr;
      }

      public static string GetString(string uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        return GetString(new Uri(uri), caller, method, queryParameters: queryParameters, bodyParameters: bodyParameters, headers: headers);
      }

      public static JSONDynamicObject GetJsonAsDynamic(Uri uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        string responseStr = GetString(uri, caller, method, queryParameters: queryParameters, bodyParameters: bodyParameters, headers: headers);

        return string.IsNullOrWhiteSpace(responseStr) ? null : responseStr.JSONToDynamic();
      }

      public static JSONDynamicObject GetJsonAsDynamic(string uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        return GetJsonAsDynamic(new Uri(uri), caller, method, queryParameters, bodyParameters, headers);
      }

      public static JSONDynamicObject GetJsonAsDynamic(Uri uri, IWebClientCaller caller, string body, IDictionary<string, string> headers = null)
      {
        string responseStr = GetString(uri, caller, HTTPRequestMethod.POST, body: body, headers: headers);

        return string.IsNullOrWhiteSpace(responseStr) ? null : responseStr.JSONToDynamic();
      }

      public static JSONDynamicObject GetJsonAsDynamic(string uri, IWebClientCaller caller, string body, IDictionary<string, string> headers = null)
      {
        return GetJsonAsDynamic(new Uri(uri), caller, body, headers);
      }

      public static JSONDynamicObject GetJsonAsDynamic(RequestParams prms)
      {
        string responseStr = GetString(prms);
        return responseStr.IsNotNullOrWhiteSpace() ? responseStr.JSONToDynamic() : null;
      }

      public static JSONDataMap GetJson(Uri uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        string responseStr = GetString(uri, caller, method, queryParameters: queryParameters, bodyParameters: bodyParameters, headers: headers);

        return string.IsNullOrWhiteSpace(responseStr) ? null : responseStr.JSONToDataObject() as JSONDataMap;
      }

      public static JSONDataMap GetJson(string uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        return GetJson(new Uri(uri), caller, method, queryParameters, bodyParameters, headers);
      }

      public static JSONDataMap GetJson(Uri uri, IWebClientCaller caller, string body, IDictionary<string, string> headers = null)
      {
        string responseStr = GetString(uri, caller, HTTPRequestMethod.POST, body: body, headers: headers);

        return string.IsNullOrWhiteSpace(responseStr) ? null : responseStr.JSONToDataObject() as JSONDataMap;
      }

      public static JSONDataMap GetJson(string uri, IWebClientCaller caller, string body, IDictionary<string, string> headers = null)
      {
        return GetJson(new Uri(uri), caller, body, headers);
      }

      public static JSONDataMap GetJson(RequestParams prms)
      {
        string responseStr = GetString(prms);
        return responseStr.IsNullOrWhiteSpace() ? null : responseStr.JSONToDataObject() as JSONDataMap;
      }

      public static JSONDataMap GetValueMap(Uri uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        string responseStr = GetString(uri, caller, method, queryParameters: queryParameters, bodyParameters: bodyParameters, headers: headers);
        var dict = JSONDataMap.FromURLEncodedString(responseStr);
        return dict;
      }

      public static JSONDataMap GetValueMap(string uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        return GetValueMap(new Uri(uri), caller, method, queryParameters, bodyParameters, headers);
      }

      public static XDocument GetXML(Uri uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        string responseStr = GetString(uri, caller, method, queryParameters: queryParameters, bodyParameters: bodyParameters, headers: headers);

        return string.IsNullOrWhiteSpace(responseStr) ? null : XDocument.Parse(responseStr);
      }

      public static XDocument GetXML(string uri, IWebClientCaller caller,
        HTTPRequestMethod method = HTTPRequestMethod.GET,
        IDictionary<string, string> queryParameters = null,
        IDictionary<string, string> bodyParameters = null,
        IDictionary<string, string> headers = null)
      {
        return GetXML(new Uri(uri), caller, method, queryParameters, bodyParameters, headers);
      }

      public static XDocument GetXML(Uri uri, IWebClientCaller caller, IDictionary<string, string> queryParameters, string body)
      {
        string responseStr = GetString(uri, caller, HTTPRequestMethod.POST, body: body, queryParameters: queryParameters);

        return string.IsNullOrWhiteSpace(responseStr) ? null : XDocument.Parse(responseStr);
      }

      public static XDocument GetXML(string uri, IWebClientCaller caller, IDictionary<string, string> queryParameters, string body)
      {
        return GetXML(new Uri(uri), caller, queryParameters, body);
      }

      public static XDocument GetXML(RequestParams prms)
      {
        string response = GetString(prms);
        return response.IsNotNullOrWhiteSpace() ? XDocument.Parse(response) : null;
      }

      #endregion

      #region .pvt

        private static WebHeaderCollection prepareRequestHeaders(RequestParams request)
        {
          var result = new WebHeaderCollection();
          if (request.Headers != null)
            request.Headers.ForEach(kvp => result.Add(kvp.Key, kvp.Value));

          if (request.ContentType.IsNotNullOrWhiteSpace())
              result.Add(HttpRequestHeader.ContentType, request.ContentType);
          if (request.AcceptType.IsNotNullOrWhiteSpace())
              result.Add(HttpRequestHeader.Accept, request.AcceptType);

          return result;
        }

      #endregion

    } //WebClient

}
