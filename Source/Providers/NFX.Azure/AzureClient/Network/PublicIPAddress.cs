using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.DataAccess.CRUD;

namespace NFX.AzureClient.Network
{
  public class PublicIPAddressDnsSettings : PropertiesFormat
  {
    [Field(backendName: "domainNameLabel")]
    public string DomainNameLabel { get; set; }
    [Field(backendName: "fqdn")]
    public string FQDN { get; set; }
    [Field(backendName: "reverseFqdn")]
    public string ReverseFQDN { get; set; }
  }

  public class PublicIPAddressPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "publicIPAllocationMethod")]
    public IPAllocationMethod? PublicIPAllocationMethod { get; set; }
    [Field(backendName: "publicIPAddressVersion")]
    public IPVersion? PublicIPAddressVersion { get; set; }
    [Field(backendName: "ipConfiguration")]
    public IPConfiguration IPConfiguration { get; set; }
    [Field(backendName: "dnsSettings")]
    public PublicIPAddressDnsSettings DNSSettings { get; set; }
    [Field(backendName: "ipAddress")]
    public string IPAddress { get; set; }
    [Field(backendName: "idleTimeoutInMinutes")]
    public int? IdleTimeoutInMinutes { get; set; }
    [Field(backendName: "resourceGuid")]
    public string ResourceGUID { get; set; }
    [Field(backendName: "provisioningState")]
    public string ProvisioningState { get; set; }
  }

  public class PublicIPAddress : NamedResourceWithETagAndProperties<PublicIPAddressPropertiesFormat> {}
}
