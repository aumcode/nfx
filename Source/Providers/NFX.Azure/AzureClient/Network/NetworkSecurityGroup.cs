using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFX.Serialization.JSON;
using NFX.DataAccess.CRUD;

namespace NFX.AzureClient.Network
{
  public enum SecurityRuleProtocol
  {
    Tcp,
    Udp,
    All
  }

  public enum SecurityRuleAccess
  {
    Allow,
    Deny
  }

  public enum SecurityRuleDirection
  {
    Inbound,
    Outbound
  }

  public class SecurityRulePropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "description")]
    public string Description { get; set; }
    [Field(backendName: "protocol")]
    public string ProtocolString { get; set; }
    public SecurityRuleProtocol? Protocol
    {
      get
      {
        if (ProtocolString == "*") return SecurityRuleProtocol.All;
        return ProtocolString.AsNullableEnum<SecurityRuleProtocol>();
      }
      set
      {
        if (value.HasValue)
          switch (value.Value)
          {
            case SecurityRuleProtocol.Tcp: ProtocolString = "Tcp"; return;
            case SecurityRuleProtocol.Udp: ProtocolString = "Udp"; return;
            case SecurityRuleProtocol.All: ProtocolString = "*"; return;
          }
        ProtocolString = null;
      }
    }
    [Field(backendName: "sourcePortRange")]
    public string SourcePortRange { get; set; }
    [Field(backendName: "destinationPortRange")]
    public string DestinationPortRange { get; set; }
    [Field(backendName: "sourceAddressPrefix")]
    public string SourceAddressPrefix { get; set; }
    [Field(backendName: "destinationAddressPrefix")]
    public string DestinationAddressPrefix { get; set; }
    [Field(backendName: "access")]
    public SecurityRuleAccess? Access { get; set; }
    [Field(backendName: "priority")]
    public int? Priority { get; set; }
    [Field(backendName: "direction")]
    public SecurityRuleDirection? Direction { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; set; }
  }

  public class SecurityRule : NamedSubResourceWithETagAndProperties<SecurityRulePropertiesFormat> {}

  public class NetworkSecurityGroupPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "securityRules")]
    public List<SecurityRule> SecurityRules { get; set; }
    [Field(backendName: "defaultSecurityRules")]
    public List<SecurityRule> DefaultSecurityRules { get; set; }
    [Field(backendName: "networkInterfaces")]
    public List<NetworkInterface> NetworkInterfaces { get; private set; }
    [Field(backendName: "subnets")]
    public List<Subnet> Subnets { get; private set; }
    [Field(backendName: "resourceGuid")]
    public string ResourceGuid { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; set; }
  }

  public class NetworkSecurityGroup : NamedResourceWithETagAndProperties<NetworkSecurityGroupPropertiesFormat> {}
}
