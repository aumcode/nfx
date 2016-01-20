using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Pay.Tax
{
  using NFX.Financial;
  using NFX.Security;

  /// <summary>
  /// Represents session of PaySystem.
  /// All PaySystem operation requires session as mandatory parameter
  /// </summary>
  public abstract class TaxSession : DisposableObject, INamed
  {
    #region ctor

      protected TaxSession(TaxCalculator taxCalculator, TaxConnectionParameters cParams)
      {
        if (taxCalculator == null || cParams == null)
          throw new TaxException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor(taxSystem is not null and cParams is not null)");

        m_TaxCalculator = taxCalculator;

        m_Name = cParams.Name;
        m_User = cParams.User;

        lock (m_TaxCalculator.m_Sessions)
          m_TaxCalculator.m_Sessions.Add(this);
      }

      protected override void Destructor()
      {
        lock (this.m_TaxCalculator.m_Sessions)
            this.m_TaxCalculator.m_Sessions.Remove(this);

        base.Destructor();
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private readonly TaxCalculator m_TaxCalculator;
      private readonly string m_Name;
      protected readonly User m_User;

    #endregion

    #region Properties

      protected TaxCalculator TaxCalculator { get { return m_TaxCalculator; } }
      public string Name { get { return m_Name; } }
      public User User { get { return m_User; } }

    #endregion

    #region Public

      public ITaxCalculation Calc(
        IAddress wholesellerAddress, 
        string[] wholesellerNexusStates,
        IAddress retailerAddress, 
        string[] retailerNexusStates,
        string[] retailerCertificateStates,
        IAddress shippingAddress, 
        Amount wholesalePrice, 
        Amount retailPrice, 
        Amount shippingPrice)
      {
        return m_TaxCalculator.Calc(this, wholesellerAddress, wholesellerNexusStates, retailerAddress, retailerNexusStates, retailerCertificateStates,
                                      shippingAddress, wholesalePrice, retailPrice, shippingPrice);
      }

    #endregion
  }
}
