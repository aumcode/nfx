using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.Log;
using NFX.ApplicationModel;
using NFX.Security;

namespace NFX.Web.Cloud.Azure
{
  public class AzureCredentials : Credentials
  {
    public AzureCredentials(string tenantID, string clientID, string clientSecret)
    {
      TenantID = tenantID;
      ClientID = clientID;
      ClientSecret = clientSecret;
    }

    public readonly string TenantID;
    public readonly string ClientID;
    public readonly string ClientSecret;
  }
}
