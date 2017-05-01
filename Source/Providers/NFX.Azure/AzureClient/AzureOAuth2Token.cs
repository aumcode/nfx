using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.Log;
using NFX.ApplicationModel;

namespace NFX.AzureClient
{
  public struct AzureOAuth2Token
  {
    public AzureOAuth2Token(string type, string token, string resource, int expires_in, int ext_expires_in, DateTime expires_on, DateTime not_before)
    {
      Type = type;
      Token = token;
      Resource = resource;
      ExpiresIn = expires_in;
      ExtExpiresIn = ext_expires_in;
      ExpiresOn = expires_on;
      NotBefore = not_before;
    }

    public readonly string Type;
    public readonly string Token;
    public readonly string Resource;
    public readonly int ExpiresIn;
    public readonly int ExtExpiresIn;
    public readonly DateTime ExpiresOn;
    public readonly DateTime NotBefore;

    public bool IsCloseToExpire { get { return (App.TimeSource.UTCNow >= NotBefore); } }

    public string AuthorizationHeader { get { return "{0} {1}".Args(Type, Token); } }
  }
}
