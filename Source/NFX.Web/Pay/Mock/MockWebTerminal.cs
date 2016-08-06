using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Pay.Mock
{
  public class MockWebTerminal : PayWebTerminal
  {
    public MockWebTerminal(MockSystem paySystem)
      : base(paySystem) { }

    public override object GetPayInit()
    {
      throw new NotImplementedException();
    }
  }
}
