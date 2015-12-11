using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Pay.Tax
{
  /// <summary>
  /// Represents session of PaySystem.
  /// All PaySystem operation requires session as mandatory parameter
  /// </summary>
  public abstract class TaxSession : DisposableObject, INamed
  {
    #region ctor

      public TaxSession(TaxCalculator taxCalculator, TaxConnectionParameters cParams)
      {
        if (taxCalculator == null || cParams == null)
          throw new TaxException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor(taxSystem is not null and cParams is not null)");

        this.mTaxCalculator = taxCalculator;

        m_Name = cParams.Name;
      }

      protected override void Destructor()
      {
        lock (this.mTaxCalculator.m_Sessions)
            this.mTaxCalculator.m_Sessions.Remove(this);

        base.Destructor();
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private readonly TaxCalculator mTaxCalculator;
      private readonly string m_Name;

    #endregion

    public string Name
    {
      get { throw new NotImplementedException(); }
    }
  }
}
