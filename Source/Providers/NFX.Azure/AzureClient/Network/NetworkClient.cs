using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Web;
using NFX.Log;
using NFX.Serialization.JSON;
using NFX.DataAccess.CRUD;

namespace NFX.AzureClient.Network
{
  public class NetworkClient : BaseClient
  {
    public NetworkClient(IWebClientCaller caller, string host, string subscriptionId, AzureOAuth2Token token)
      : base(caller, host, subscriptionId, token) { }

    public Subnet GetSubnet(string resourceGroupName, string virtualNetworkName, string subnetName)
    {
      return GetResource<Subnet>(URIs.Subnets_Get(SubscriptionId, resourceGroupName, virtualNetworkName, subnetName));
    }

    public PublicIPAddress GetPublicIPAddress(string resourceGroupName, string publicIPAddressName)
    {
      return GetResource<PublicIPAddress>(URIs.PublicIPAddresses_Get(SubscriptionId, resourceGroupName, publicIPAddressName));
    }

    public NetworkSecurityGroup GetNetworkSecurityGroup(string resourceGroupName, string securityGroupName)
    {
      return GetResource<NetworkSecurityGroup>(URIs.NetworkSecurityGroups_Get(SubscriptionId, resourceGroupName, securityGroupName));
    }

    public NetworkInterface CreateOrUpdateNetworkInterface(string resourceGroupName, string networkInterfaceName, NetworkInterface networkInterface)
    {
      NetworkInterface result;
      PutResource(URIs.NetworkInterfaces_CreateOrUpdate(SubscriptionId, resourceGroupName, networkInterfaceName), networkInterface, out result);
      return result;
    }
  }
}
