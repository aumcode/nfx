using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.AzureClient.Network
{
  public class BackendAddressPool : NamedSubResourceWithETagAndProperties<PropertiesFormat> { }

  public class InboundNatRule : NamedSubResourceWithETagAndProperties<PropertiesFormat> {}

  public class LoadBalancer : NamedResourceWithETagAndProperties<PropertiesFormat> {}
}
