using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Instrumentation;

namespace NFX.Web.Pay.Tax
{
  /// <summary>
  /// Represents taxation result
  /// </summary>
  public interface ITaxCalculation
  {
    /// <summary>
    /// Who must pay tax to Recipient 
    /// </summary>
    TaxCollector Collector { get; }

    /// <summary>
    /// Tax recipient (state in US or contry in Europe).
    /// </summary>
    string Recipient { get; }

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
  }

  /// <summary>
  /// Who pays collected money to state
  /// </summary>
  public enum TaxCollector
  {
    None = 0x00,

    /// <summary>
    /// Wholeseller (retailer must collect tax from byuer and then transfer it to wholesaler which must pay its nexus state)
    /// </summary>
    Wholeseller = 0x01,

    /// <summary>
    /// Retailer (retailer collects tax from buyer and then pays it to its nexus state)
    /// </summary>
    Retailer = 0x02
  }

  /// <summary>
  /// Primitive (maybe temporary) ITaxCalcResult implementation
  /// </summary>
  public struct TaxCalculation: ITaxCalculation
  {
    /// <summary>
    /// Who must pay tax to Recipient 
    /// </summary>
    public TaxCollector Collector { get; set; }

    /// <summary>
    /// Tax recipient (state in US or contry in Europe).
    /// </summary>
    public string Recipient { get; set; }

    /// <summary>
    /// Gets/sets state tax if present
    /// </summary>
    public decimal State { get; set; }

    /// <summary>
    /// Gets/sets county tax if present
    /// </summary>
    public decimal County { get; set; }

    /// <summary>
    /// Gets/sets city tax if present
    /// </summary>
    public decimal City { get; set; }

    /// <summary>
    /// Gets/sets special tax if present
    /// </summary>
    public decimal Special { get; set; }
  }

  /// <summary>
  /// Represents a taxation calculator
  /// </summary>
  public interface ITaxCalculator
  {
    ITaxCalculation Calc(IAddress wholesellerAddress, IAddress retailerAddress, IAddress shippingAddress);
  }

  public interface ITaxCalculatorImplementation: ITaxCalculator, IConfigurable, IInstrumentable {}

}

