using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Web;

namespace NFX.AzureClient.Compute
{
  public class ComputeClient
  {
    public ComputeClient(IWebClientCaller caller, string subscriptionId, AzureOAuth2Token token)
    {

    }

    public VirtualMachine CreateOrUpdateVirtualMachine(string group, string id, VirtualMachine virtualMachine)
    {
      throw new NotImplementedException();
    }
  }
}
