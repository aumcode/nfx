using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.Environment;
using NFX.Log;
using NFX.Serialization.JSON;
using NFX.AzureClient;
using NFX.AzureClient.Network;
using NFX.AzureClient.Compute;

namespace NFX.Web.Cloud.Azure
{
  public class AzureSystem : CloudSystem
  {
    #region CONSTS
    public const string CONFIG_AD_URI_ATTR = "ad-uri";
    public const string CONFIG_RESOURCE_ATTR = "resource";

    public const string DEFAULT_AD_URI = "https://login.windows.net";
    public const string DEFAULT_RESOURCE = "https://management.azure.com/";

    private const string PRM_RESOURCE = "resource";
    private const string PRM_CLIENT_ID = "client_id";
    private const string PRM_CLIENT_SECRET = "client_secret";
    private const string PRM_GRANT_TYPE = "grant_type";
    private const string PRM_CLIENT_CREDENTIALS = "client_credentials";

    #endregion

    #region .ctor
    public AzureSystem(string name, IConfigSectionNode node) : this(name, node, null) { }
    public AzureSystem(string name, IConfigSectionNode node, object director) : base(name, node, director) { }
    #endregion

    #region Fields
    [Config(CONFIG_AD_URI_ATTR, Default = DEFAULT_AD_URI)]
    private string m_ADUri = DEFAULT_AD_URI;

    public Uri ADUri { get { return new Uri(m_ADUri); } }

    [Config(CONFIG_RESOURCE_ATTR, Default = DEFAULT_RESOURCE)]
    private string m_Resource = DEFAULT_RESOURCE;
    #endregion

    #region Protected
    protected override CloudConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
    { return CloudConnectionParameters.Make<AzureConnectionParameters>(paramsSection); }

    protected override CloudTemplate MakeTemplate(IConfigSectionNode node)
    { return CloudTemplate.Make<AzureTemplate>(this, node); }

    protected override CloudSession DoStartSession(CloudConnectionParameters cParams)
    { return new AzureSession(this, (AzureConnectionParameters)cParams); }

    protected override void DoDeploy(CloudSession session, string id, CloudTemplate template, IConfigSectionNode customData)
    {
      var aSession = (AzureSession)session;
      doDeploy(aSession.SubscriptionID, aSession.AccessToken, id, (AzureTemplate)template, customData);
    }
    #endregion

    #region Private
    private Uri URI_GetAccessToken(string tenantID) { return new Uri(ADUri, tenantID + "/oauth2/token"); }


    internal AzureOAuth2Token getAccessToken(AzureCredentials credentials)
    {
      var logID = Log(MessageType.Info, "getAccessToken()", "Get Access Token");

      try
      {
        var request = new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.FORM_URL_ENCODED,
          BodyParameters = new Dictionary<string, string>
          {
            { PRM_GRANT_TYPE, PRM_CLIENT_CREDENTIALS },
            { PRM_RESOURCE, Uri.EscapeDataString(m_Resource) },
            { PRM_CLIENT_ID, credentials.ClientID },
            { PRM_CLIENT_SECRET, credentials.ClientSecret }
          }
        };

        var response = WebClient.GetJson(URI_GetAccessToken(credentials.TenantID), request);
        Log(MessageType.Trace, "generateOAuthToken()", response.ToJSON(), relatedMessageID: logID);
        Log(MessageType.Info, "generateOAuthToken()", "OAuth token generated", relatedMessageID: logID);
        return new AzureOAuth2Token(
          response["token_type"].AsString(),
          response["access_token"].AsString(),
          response["resource"].AsString(),
          response["expires_in"].AsInt(),
          response["ext_expires_in"].AsInt(),
          response["expires_on"].AsLong().FromSecondsSinceUnixEpochStart(),
          response["not_before"].AsLong().FromSecondsSinceUnixEpochStart());
      }
      catch (Exception error)
      {
        Log(MessageType.Error, "generateOAuthToken()", error.Message, error, relatedMessageID: logID);
        throw error;
      }
    }

    private void doDeploy(string subscriptionID, AzureOAuth2Token token, string id, AzureTemplate template, IConfigSectionNode customData)
    {
      var nicName = id;

      var networkClient = new NetworkClient(this, DEFAULT_RESOURCE, subscriptionID, token);

      string securityGroup = null;
      NetworkInterface networkInterface = null;
      List<NetworkInterfaceIPConfiguration> ipConfigurations = null;
      List<NetworkInterface> networkInterfaces = null;

      foreach (var ni in template.NetworkInterfaces.OrderBy(ni => ni.SecurityGroup))
      {
        if (networkInterfaces == null) networkInterfaces = new List<NetworkInterface>();
        var subnet = networkClient.GetSubnet(template.Group, ni.VNet, ni.Subnet);
        var publicIP = ni.PublicIP.IsNotNullOrWhiteSpace() ? networkClient.GetPublicIPAddress(template.Group, ni.PublicIP) : null;

        if (securityGroup != ni.SecurityGroup)
        {
          securityGroup = ni.SecurityGroup;
          if (networkInterface != null) networkInterfaces.Add(networkClient.CreateOrUpdateNetworkInterface(template.Group, nicName, networkInterface));
          networkInterface = null;
        }

        if (networkInterface == null)
        {
          var nsg = ni.SecurityGroup.IsNotNullOrWhiteSpace() ? networkClient.GetNetworkSecurityGroup(template.Group, ni.SecurityGroup) : null;

          ipConfigurations = new List<NetworkInterfaceIPConfiguration>();

          networkInterface = new NetworkInterface
          {
            Location = template.Location,
            Properties = new NetworkInterfacePropertiesFormat
            {
              IPConfigurations = ipConfigurations,
              NetworkSecurityGroup = nsg
            }
          };
        }

        ipConfigurations.Add(new NetworkInterfaceIPConfiguration
        {
          Properties = new NetworkInterfaceIPConfigurationPropertiesFormat
          {
            Subnet = subnet,
            PublicIPAddress = publicIP
          }
        });
      }

      if (networkInterface != null) networkInterfaces.Add(networkClient.CreateOrUpdateNetworkInterface(template.Group, nicName, networkInterface));

      var computeClient = new ComputeClient(this, subscriptionID, token);
      computeClient.CreateOrUpdateVirtualMachine(template.Group, id, new VirtualMachine
      {
        Location = template.Location,
        Properties = new VirtualMachinePropertiesFormat
        {
          hardwareProfile = new HardwareProfile { vmSize = template.HardwareProfile },
          storageProfile = new StorageProfile
          {
            osDisk = new OSDisk
            {
              osType = OperatingSystemTypes.Windows,
              name = id + ".SYSTEM",
              createOption = DiskCreateOptionTypes.fromImage,
              image = new VirtualHardDisk { uri = template.ImageUri },
              vhd = new VirtualHardDisk { uri = template.OSDiskUri.Args(id + ".SYSTEM") },
              caching = CachingTypes.ReadWrite
            }
          },
          osProfile = new OSProfile
          {
            computerName = id,
            adminUserName = template.AdminUsername,
            adminPassword = template.AdminPassword
          },
          networkProfile = new NetworkProfile
          {
            networkInterfaces = networkInterfaces.Select(ni => new NetworkInterfaceReference(ni)).ToList()
          }
        }
      });

      throw new NotImplementedException();
    }
    #endregion
  }
}
