using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NFX.Web.Messaging
{
  public class HttpBasicAuthenticationHelper
  {
    private const string AUTHORIZATION_HEADER = "Authorization";

    public HttpBasicAuthenticationHelper(string username, string password)
    {
      string token = Convert.ToBase64String(Encoding.UTF8.GetBytes("{0}:{1}".Args(username, password)));
      m_AuthHeader = "Basic {0}".Args(token);
    }

    private readonly string m_AuthHeader;

    public void AddAuthHeader(HttpWebRequest request)
    {
      if (request == null)
        throw new WebException("HttpBasicAuthenticationHelper.AddAuthentication(request=null)");

      if (request.Headers[HttpRequestHeader.Authorization] == null)
        request.Headers.Add(HttpRequestHeader.Authorization, m_AuthHeader);
    }

    public void AddAuthHeader(ref WebClient.RequestParams request)
    {
      if (request.Headers != null)
      {
        if (request.Headers.ContainsKey(AUTHORIZATION_HEADER)) return;
      }
      else
        request.Headers = new Dictionary<string, string>();

      request.Headers.Add(AUTHORIZATION_HEADER, m_AuthHeader);
    }
  }
}
