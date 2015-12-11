using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Pay.Tax.TaxJar
{
  public class TaxJarTaxCalc: TaxCalculator
  {
    public override ITaxCalculation Calc( IAddress wholesellerAddress, IAddress retailerAddress, IAddress shippingAddress)
    {
      throw new NotImplementedException();
    }
  }
}
