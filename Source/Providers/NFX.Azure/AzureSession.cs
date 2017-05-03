using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.Log;
using NFX.ApplicationModel;
using NFX.AzureClient;

namespace NFX.Web.Cloud.Azure
{
  public class AzureSession : CloudSession
  {
    public AzureSession(CloudSystem cloudSystem, CloudConnectionParameters cParams) : base(cloudSystem, cParams) {}

    private AzureOAuth2Token? m_AccessToken;

    public AzureOAuth2Token AccessToken
    {
      get
      {
        if (!m_AccessToken.HasValue || m_AccessToken.Value.IsCloseToExpire)
          m_AccessToken = ((AzureSystem)CloudSystem).getAccessToken(((AzureConnectionParameters)ConnectionParameters).Credentials);

        return m_AccessToken.Value;
      }
    }

    public string SubscriptionID { get { return ((AzureConnectionParameters)ConnectionParameters).SubscriptionID; } }
  }
}
