using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Pay
{
  public abstract class PayWebTerminal : IPayWebTerminal
  {
    protected PayWebTerminal(PaySystem paySystem)
    {
      PaySystem = paySystem;
    }
    public IPaySystem PaySystem { get; private set; }

    public abstract object GetPayInit();
  }
}
