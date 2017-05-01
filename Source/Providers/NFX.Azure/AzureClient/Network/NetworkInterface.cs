using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.DataAccess.CRUD;

namespace NFX.AzureClient.Network
{
  public enum IPAllocationMethod
  {
    Static,
    Dynamic
  }

  public enum IPVersion
  {
    IPv4,
    IPv6
  }

  public class NetworkInterfaceIPConfigurationPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "applicationGatewayBackendAddressPools")]
    public List<ApplicationGatewayBackendAddressPool> ApplicationGatewayBackendAddressPools { get; set; }
    [Field(backendName: "loadBalancerBackendAddressPools")]
    public List<BackendAddressPool> LoadBalancerBackendAddressPools { get; set; }
    [Field(backendName: "loadBalancerInboundNatRules")]
    public List<InboundNatRule> LoadBalancerInboundNatRules { get; set; }
    [Field(backendName: "privateIPAddress")]
    public string privateIPAddress { get; set; }
    [Field(backendName: "privateIPAllocationMethod")]
    public IPAllocationMethod? PrivateIPAllocationMethod { get; set; }
    [Field(backendName: "privateIPAddressVersion")]
    public IPVersion? PrivateIPAddressVersion { get; set; }
    [Field(backendName: "subnet")]
    public Subnet Subnet { get; set; }
    [Field(backendName: "primary")]
    public bool? Primary { get; set; }
    [Field(backendName: "publicIPAddress")]
    public PublicIPAddress PublicIPAddress { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; set; }
  }

  public class NetworkInterfaceIPConfiguration : NamedSubResourceWithETagAndProperties<NetworkInterfaceIPConfigurationPropertiesFormat> {}

  public class NetworkInterfaceDnsSettings : PropertiesFormat
  {
    [Field(backendName: "dnsServers")]
    public List<string> DNSServers { get; set; }
    [Field(backendName: "appliedDnsServers")]
    public List<string> AppliedDNSServers { get; set; }
    [Field(backendName: "internalDnsNameLabel")]
    public string InternalDNSNameLabel { get; set; }
    [Field(backendName: "internalFqdn")]
    public string InternalFQDN { get; set; }
    [Field(backendName: "internalDomainNameSuffix")]
    public string InternalDomainNameSuffix { get; set; }
  }

  public class NetworkInterfacePropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "virtualMachine")]
    public SubResource VirtualMachine { get; set; }
    [Field(backendName: "networkSecurityGroup")]
    public NetworkSecurityGroup NetworkSecurityGroup { get; set; }
    [Field(backendName: "ipConfigurations")]
    public List<NetworkInterfaceIPConfiguration> IPConfigurations { get; set; }
    [Field(backendName: "dnsSettings")]
    public NetworkInterfaceDnsSettings DNSSettings { get; set; }
    [Field(backendName: "macAddress")]
    public string MACAddress { get; set; }
    [Field(backendName: "primary")]
    public bool? Primary { get; set; }
    [Field(backendName: "enableAcceleratedNetworking")]
    public bool? EnableAcceleratedNetworking { get; set; }
    [Field(backendName: "enableIPForwarding")]
    public bool? EnableIPForwarding { get; set; }
    [Field(backendName: "resourceGuid")]
    public string ResourceGuid { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; set; }
  }

  public class NetworkInterface : NamedResourceWithETagAndProperties<NetworkInterfacePropertiesFormat> {}

  public class IPConfigurationPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "privateIPAddress")]
    public string PrivateIPAddress { get; set; }
    [Field(backendName: "privateIPAllocationMethod")]
    public IPAllocationMethod? PrivateIPAllocationMethod { get; set; }
    [Field(backendName: "subnet")]
    public Subnet Subnet { get; set; }
    [Field(backendName: "publicIPAddress")]
    public PublicIPAddress PublicIPAddress { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; set; }
  }

  public class IPConfiguration : NamedSubResourceWithETagAndProperties<IPConfigurationPropertiesFormat> {}
}
