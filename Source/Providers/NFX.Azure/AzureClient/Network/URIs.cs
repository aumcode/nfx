using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.AzureClient.Network
{
  public static class URIs
  {
    public const string ApiVersion = "2017-03-01";

    private const string Network = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Network";
    private const string NetworkInterfaces = Network + "/networkInterfaces";
    private const string NetworkInterface = NetworkInterfaces + "/{2}";
    private const string VirtualNetworks = Network + "/virtualNetworks";
    private const string VirtualNetwork = VirtualNetworks + "/{2}";
    private const string Subnets = VirtualNetwork + "/subnets";
    private const string Subnet = Subnets + "/{3}";
    private const string PublicIPAddresses = Network + "/publicIPAddresses";
    private const string PublicIPAddress = PublicIPAddresses + "/{2}";
    private const string NetworkSecurityGroups = Network + "/networkSecurityGroups";
    private const string NetworkSecurityGroup = NetworkSecurityGroups + "/{2}";

    public static string NetworkInterfaces_List(string subscriptionId, string resourceGroupName, string apiVersion = ApiVersion)
    {
      const string NetworkInterfaces_List = NetworkInterfaces;
      return NetworkInterfaces_List.Args(subscriptionId, resourceGroupName) + "?api-version=" + apiVersion;
    }

    public static string NetworkInterfaces_Get(string subscriptionId, string resourceGroupName, string networkInterfaceName, string apiVersion = ApiVersion)
    {
      const string NetworkInterfaces_Get = NetworkInterface;
      return NetworkInterfaces_Get.Args(subscriptionId, resourceGroupName, networkInterfaceName) + "?api-version=" + apiVersion;
    }

    public static string NetworkInterfaces_CreateOrUpdate(string subscriptionId, string resourceGroupName, string networkInterfaceName, string apiVersion = ApiVersion)
    {
      const string NetworkInterfaces_CreateOrUpdate = NetworkInterface;
      return NetworkInterfaces_CreateOrUpdate.Args(subscriptionId, resourceGroupName, networkInterfaceName) + "?api-version=" + apiVersion;
    }

    public static string NetworkInterfaces_Delete(string subscriptionId, string resourceGroupName, string networkInterfaceName, string apiVersion = ApiVersion)
    {
      const string NetworkInterfaces_Delete = NetworkInterface;
      return NetworkInterfaces_Delete.Args(subscriptionId, resourceGroupName, networkInterfaceName) + "?api-version=" + apiVersion;
    }

    public static string NetworkInterfaces_ListAll(string subscriptionId, string apiVersion = ApiVersion)
    {
      const string NetworkInterfaces_ListAll = "/subscriptions/{0}/providers/Microsoft.Network/networkInterfaces";
      return NetworkInterfaces_ListAll.Args(subscriptionId) + "?api-version=" + apiVersion;
    }

    public static string NetworkInterfaces_GetEffectiveRouteTable(string subscriptionId, string resourceGroupName, string networkInterfaceName, string apiVersion = ApiVersion)
    {
      const string NetworkInterfaces_GetEffectiveRouteTable = NetworkInterface + "/effectiveRouteTable";
      return NetworkInterfaces_GetEffectiveRouteTable.Args(subscriptionId, resourceGroupName, networkInterfaceName) + "?api-version=" + apiVersion;
    }

    public static string NetworkInterfaces_ListEffectiveNetworkSecurityGroups(string subscriptionId, string resourceGroupName, string networkInterfaceName, string apiVersion = ApiVersion)
    {
      const string NetworkInterfaces_ListEffectiveNetworkSecurityGroups = NetworkInterface + "/effectiveNetworkSecurityGroups";
      return NetworkInterfaces_ListEffectiveNetworkSecurityGroups.Args(subscriptionId, resourceGroupName, networkInterfaceName) + "?api-version=" + apiVersion;
    }

    public static string VirtualNetworks_Get(string subscriptionId, string resourceGroupName, string virtualNetworkName, string apiVersion = ApiVersion)
    {
      const string VirtualNetworks_Get = VirtualNetwork;
      return VirtualNetworks_Get.Args(subscriptionId, resourceGroupName, virtualNetworkName) + "?api-version=" + apiVersion;
    }

    public static string Subnets_Get(string subscriptionId, string resourceGroupName, string virtualNetworkName, string subnetName, string apiVersion = ApiVersion)
    {
      const string Subnets_Get = Subnet;
      return Subnets_Get.Args(subscriptionId, resourceGroupName, virtualNetworkName, subnetName) + "?api-version=" + apiVersion;
    }

    public static string PublicIPAddresses_Get(string subscriptionId, object resourceGroupName, string publicIPAddressName, string apiVersion = ApiVersion)
    {
      const string PublicIPAddresses_Get = PublicIPAddress;
      return PublicIPAddresses_Get.Args(subscriptionId, resourceGroupName, publicIPAddressName) + "?api-version=" + apiVersion;
    }

    public static string NetworkSecurityGroups_Get(string subscriptionId, string resourceGroupName, string securityGroupName, string apiVersion = ApiVersion)
    {
      const string NetworkSecurityGroups_Get = NetworkSecurityGroup;
      return NetworkSecurityGroups_Get.Args(subscriptionId, resourceGroupName, securityGroupName) + "?api-version=" + apiVersion;
    }
  }
}
