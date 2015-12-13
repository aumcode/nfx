using NFX.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Pay.Tax
{
  /// <summary>
  /// NOP implementation of ITaxCalc
  /// </summary>
  public class NOPTaxCalculator: TaxCalculator
  {
    public static readonly NOPTaxCalculator Instance = new NOPTaxCalculator();

    public NOPTaxCalculator(){}

    public NOPTaxCalculator(string name = null, IConfigSectionNode cfg = null)
      : base(name, cfg)
    {
    }

    public override ITaxCalculation Calc(IAddress wholesellerAddress, IAddress retailerAddress, IAddress shippingAddress)
    {
      return new TaxCalculation() { State = 0M, County = 0M, City = 0M, Special = 0M};
    }

    public void Configure(IConfigSectionNode node)
    {
    }

    public bool InstrumentationEnabled { get{ return false;} set{}}
   

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters
    {
      get { yield break; }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      yield break;
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      value = null;
      return false;
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      value = null;
      return false;
    }
  }
}
