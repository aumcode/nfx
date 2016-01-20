using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Security;

namespace NFX.Web.Pay.Tax.NOP
{
  public class NOPSession: TaxSession
  {
    public NOPSession(TaxCalculator taxCalculator, TaxConnectionParameters cParams): base(taxCalculator, cParams)
    {
    }
  }
}
