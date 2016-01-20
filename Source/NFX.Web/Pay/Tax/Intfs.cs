using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Instrumentation;
using NFX.Financial;

namespace NFX.Web.Pay.Tax
{
  /// <summary>
  /// Represents taxation result
  /// </summary>
  public interface ITaxCalculation
  {
    /// <summary>
    /// Tax recipient (state in US or country in Europe).
    /// </summary>
    string Recipient { get; }

    /// <summary>
    /// Paid to state by retailer
    /// </summary>
    ITaxStructured RetailerTax { get; }

    /// <summary>
    /// Paid to state by wholeseller
    /// </summary>
    ITaxStructured WholesellerTax { get; }

    /// <summary>
    /// Is this none instance (so no tax must be paid)
    /// </summary>
    bool IsNone { get; }
  }

  public interface ITaxStructured
  {
    /// <summary>
    /// Currency of fields above
    /// </summary>
    string CurrencyISO { get; }

    /// <summary>
    /// Returns total tax if present.
    /// Represents sum of State, County, City and Special properties if they are known.
    /// Otherwise State, County, City and Special properties are zero.
    /// </summary>
    decimal Total { get; }

    /// <summary>
    /// Returns state tax if present
    /// </summary>
    decimal State { get; }

    /// <summary>
    /// Returns county tax if present
    /// </summary>
    decimal County { get; }

    /// <summary>
    /// Returns city tax if present
    /// </summary>
    decimal City { get; }

    /// <summary>
    /// Returns special tax if present
    /// </summary>
    decimal Special { get; }

    /// <summary>
    /// Is this none instance (so no tax must be paid)
    /// </summary>
    bool IsNone { get; }
  }

  public struct TaxStructured: ITaxStructured
  {
    private bool m_IsNone;

    public bool IsNone { get { return m_IsNone; } }

    public static readonly TaxStructured NoneInstance = new TaxStructured() { m_IsNone = true };


    /// <summary>
    /// Currency of fields above
    /// </summary>
    public string CurrencyISO { get; set; }

    /// <summary>
    /// Returns total tax if present.
    /// Represents sum of State, County, City and Special properties if they are known.
    /// Otherwise State, County, City and Special properties are zero.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Returns state tax if present
    /// </summary>
    public decimal State { get; set; }

    /// <summary>
    /// Returns county tax if present
    /// </summary>
    public decimal County { get; set; }

    /// <summary>
    /// Returns city tax if present
    /// </summary>
    public decimal City { get; set; }

    /// <summary>
    /// Returns special tax if present
    /// </summary>
    public decimal Special { get; set; }
  }

  /// <summary>
  /// Primitive (maybe temporary) ITaxCalcResult implementation
  /// </summary>
  public struct TaxCalculation: ITaxCalculation
  {
    private bool m_IsNone;

    public bool IsNone { get { return m_IsNone; } }

    public static readonly TaxCalculation NoneInstance = new TaxCalculation() { m_IsNone = true };

    public string Recipient { get; set; }

    public string CurrencyISO { get; set; }

    public ITaxStructured RetailerTax { get; set; }

    public ITaxStructured WholesellerTax { get; set; }
  }

  /// <summary>
  /// Represents a taxation calculator
  /// </summary>
  public interface ITaxCalculator
  {
    ITaxCalculation Calc(TaxSession session, 
      IAddress wholesellerAddress, 
      string[] wholesellerNexusStates,
      IAddress retailerAddress, 
      string[] retailerNexusStates,
      string[] retailerCertificateStates,
      IAddress shippingAddress, 
      Amount wholesalePrice, 
      Amount retailPrice, 
      Amount shippingPrice);
  }

  public interface ITaxCalculatorImplementation: ITaxCalculator, IConfigurable, IInstrumentable {}

}

