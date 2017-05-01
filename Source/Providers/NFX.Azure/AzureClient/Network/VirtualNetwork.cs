using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.DataAccess.CRUD;

namespace NFX.AzureClient.Network
{
  public class ResourceNavigationLinkFormat : PropertiesFormat
  {
    [Field(backendName: "linkedResourceType")]
    public string LinkedResourceType { get; set; }
    [Field(backendName: "link")]
    public string Link { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; private set; }
  }

  public class ResourceNavigationLink : NamedSubResourceWithETagAndProperties<ResourceNavigationLinkFormat> {}

  public class SubnetPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "addressPrefix")]
    public string AddressPrefix { get; set; }
    [Field(backendName: "networkSecurityGroup")]
    public NetworkSecurityGroup NetworkSecurityGroup { get; set; }
    [Field(backendName: "routeTable")]
    public RouteTable RouteTable { get; set; }
    [Field(backendName: "ipConfigurations")]
    public List<IPConfiguration> IPConfigurations { get; set; }
    [Field(backendName: "resourceNavigationLinks")]
    public List<ResourceNavigationLink> ResourceNavigationLinks { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; set; }
  }

  public class Subnet : NamedSubResourceWithETagAndProperties<SubnetPropertiesFormat> {}
}
