using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Financial;

namespace NFX.Web.Pay.Tax.NOP
{
  using NFX.Web.Pay.Mock;

  /// <summary>
  /// NOP implementation of ITaxCalc
  /// </summary>
  public class NOPTaxCalculator: TaxCalculator
  {
    public static readonly NOPTaxCalculator Instance = new NOPTaxCalculator();

    private NOPTaxCalculator(): base(null, null) {}

    public NOPTaxCalculator(string name, IConfigSectionNode node) : base(name, node) {}

    public NOPTaxCalculator(string name, IConfigSectionNode node, object director) : base(name, node, director) {}

    public override ITaxStructured DoCalc(
        TaxSession session,
        IAddress fromAddress, 
        IAddress toAddress, 
        Amount amount, 
        Amount shipping)
    {
      return TaxStructured.NoneInstance;
    }

    protected override TaxConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
    {
      return TaxConnectionParameters.Make<NOPConnectionParameters>(paramsSection);
    }

    protected override TaxSession DoStartSession(TaxConnectionParameters cParams = null)
    {
      TaxConnectionParameters sessionParams = cParams ?? DefaultSessionConnectParams;
      return this.StartSession((NOPConnectionParameters)sessionParams);
    }

    public NOPSession StartSession(NOPConnectionParameters cParams)
    {
      return new NOPSession(this, cParams);
    }
  }
}
