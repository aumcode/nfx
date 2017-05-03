using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.Environment;

namespace NFX.Web.Cloud.Azure
{
  public class AzureConnectionParameters : CloudConnectionParameters
  {
    #region STATIC
    public const string CONFIG_TENANT_ID_ATTR = "tenant-id";
    public const string CONFIG_CLIENT_ID_ATTR = "client-id";
    public const string CONFIG_CLIENT_SECRET_ATTR = "client-secret";
    public const string CONFIG_SUBSCRIPTION_ID_ATTR = "subscription-id";
    #endregion

    #region .ctor
    public AzureConnectionParameters() : base() { }
    public AzureConnectionParameters(IConfigSectionNode node) : base(node) { }
    public AzureConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) { }
    public AzureConnectionParameters(AzureCredentials credentials, string subscriptionID) { m_Credentials = credentials; m_SubscriptionID = subscriptionID; }
    #endregion

    #region Fields
    private AzureCredentials m_Credentials;
    [Config]
    private string m_SubscriptionID;
    #endregion

    #region Public
    public string SubscriptionID { get { return m_SubscriptionID; } }
    public AzureCredentials Credentials { get { return m_Credentials; } }
    #endregion

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var tenantID = node.AttrByName(CONFIG_TENANT_ID_ATTR).ValueAsString();
      if (tenantID.IsNullOrWhiteSpace()) return;

      var clientID = node.AttrByName(CONFIG_CLIENT_ID_ATTR).ValueAsString();
      var clientSecret = node.AttrByName(CONFIG_CLIENT_SECRET_ATTR).ValueAsString();
      if (clientID.IsNullOrWhiteSpace() || clientSecret.IsNullOrWhiteSpace()) return;

      m_Credentials = new AzureCredentials(tenantID, clientID, clientSecret);
    }
  }
}
