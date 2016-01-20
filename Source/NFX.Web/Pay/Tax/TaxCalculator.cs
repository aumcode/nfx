/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;

using NFX.Environment;
using NFX.ServiceModel;
using NFX.ApplicationModel;
using NFX.Financial;
using NFX.Log;

namespace NFX.Web.Pay.Tax
{
  

  /// <summary>
  /// Base class for ITaxCalcImplementation implementation.
  /// Instances of descendants of this class is typically created and configured in WebSettings class.
  /// Then particular tax calculator can be obtained from TaxCalculator.Instances[TAX_CALCULATOR_NAME] indexer
  /// </summary>
  public abstract class TaxCalculator : ServiceWithInstrumentationBase<object>, ITaxCalculatorImplementation, INamed, IWebClientCaller
  {
    #region consts
    
      public const string CONFIG_AUTO_START_ATTR = "auto-start";

      private const string LOG_TOPIC = "Tax.Calculation";

      private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;

      private static readonly TimeSpan INSTR_INTERVAL = TimeSpan.FromMilliseconds(3911);

    #endregion

    #region Static

      private static Registry<TaxCalculator> s_Instances = new Registry<TaxCalculator>();

      /// <summary>
      /// Returns the read-only registry view of tax calculator currently present
      /// </summary>
      public static IRegistry<TaxCalculator> Instances { get { return s_Instances; } }

      public static void AutoStartCalculators()
      {
        App.Instance.RegisterAppFinishNotifiable(TaxFinisher.Instance);

        WebSettings.RequireInitializedSettings();

        foreach (var taxNode in App.ConfigRoot[WebSettings.CONFIG_WEBSETTINGS_SECTION][WebSettings.CONFIG_TAX_SECTION]
                                                          .Children.Where(cn => cn.IsSameName(WebSettings.CONFIG_TAX_CALCULATOR_SECTION)))
        {
          var name = taxNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;

          if (!taxNode.AttrByName(CONFIG_AUTO_START_ATTR).ValueAsBool()) continue;

          var taxCalculator = FactoryUtils.MakeAndConfigure<TaxCalculator>(taxNode, typeof(TaxCalculator), new object[] { null, taxNode});

          if (s_Instances[taxCalculator.Name] != null) continue; // already started

          taxCalculator.Start();
        }
      }

      public static TTaxCalculator Make<TTaxCalculator>(string name, IConfigSectionNode node) where TTaxCalculator: TaxCalculator
      {
        WebSettings.RequireInitializedSettings();

        return FactoryUtils.MakeAndConfigure<TTaxCalculator>(node, typeof(TTaxCalculator), new object[] {name, node});
      }

      public static TTaxCalculator Make<TTaxCalculator>(string name, string cfgStr, string format = Configuration.CONFIG_LACONIC_FORMAT) where TTaxCalculator: TaxCalculator
      {
        WebSettings.RequireInitializedSettings();

        var cfg = Configuration.ProviderLoadFromString(cfgStr, format);
        return Make<TTaxCalculator>(name, cfg.Root);
      }

    #endregion

    #region Inner classes

      private class TaxFinisher: IApplicationFinishNotifiable
      {
        internal static readonly TaxFinisher Instance = new TaxFinisher();

        public string Name { get { return GetType().FullName; } }

        public void ApplicationFinishBeforeCleanup(IApplication application)
        {
          foreach (var taxCalculator in s_Instances)
            taxCalculator.WaitForCompleteStop();
        }

        public void ApplicationFinishAfterCleanup(IApplication application) {}
      }

    #endregion

    #region ctor

      protected TaxCalculator(string name, IConfigSectionNode cfg): this(name, cfg, null) {}

      protected TaxCalculator(string name, IConfigSectionNode cfg, object director): base(director)
      {
        var calculatorName = name;
          
        if (cfg != null)
        {
          Configure(cfg);
          calculatorName = cfg.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        }

        if (name.IsNotNullOrWhiteSpace()) calculatorName = name;

        if (calculatorName.IsNullOrWhiteSpace()) calculatorName = GetType().Name;

        Name = calculatorName;

        m_Sessions = new List<TaxSession>();
      }

      protected override void Destructor()
      {
        DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
        base.Destructor();
      } 

    #endregion

    #region Pvt/Prot/Int Fields

      private bool m_InstrumentationEnabled;
      private Time.Event m_InstrumentationEvent;

      private IConfigSectionNode m_DefaultSesssionConnParamsCfg;
      private TaxConnectionParameters m_DefaultSessionConnectParams;

      protected internal readonly List<TaxSession> m_Sessions;

      private int m_WebServiceCallTimeoutMs;

      private long m_stat_TaxCount, m_stat_TaxErrorCount;
      private ConcurrentDictionary<string, decimal> m_stat_RetailerTaxAmounts = new ConcurrentDictionary<string,decimal>();
      private ConcurrentDictionary<string, decimal> m_stat_WholesellerTaxAmounts = new ConcurrentDictionary<string,decimal>();

    #endregion

    #region Properties

      [Config("default-session-connect-params")]
      public IConfigSectionNode DefaultSesssionConnectParamsCfg
      {
        get { return m_DefaultSesssionConnParamsCfg; }
        set
        {
          m_DefaultSessionConnectParams = MakeDefaultSessionConnectParams(value);
          m_DefaultSesssionConnParamsCfg = value;
        }
      }

    #endregion

    #region Public

      public ITaxCalculation Calc(
        TaxSession session,
        IAddress wholesellerAddress, 
        string[] wholesellerNexusStates,
        IAddress retailerAddress, 
        string[] retailerNexusStates,
        string[] retailerCertificateStates,
        IAddress shippingAddress, 
        Amount wholesalePrice, 
        Amount retailPrice, 
        Amount shipping)
      {
        if (!wholesalePrice.CurrencyISO.EqualsOrdIgnoreCase(retailPrice.CurrencyISO) ||
            !wholesalePrice.CurrencyISO.EqualsOrdIgnoreCase(shipping.CurrencyISO) ||
            !retailPrice.CurrencyISO.EqualsOrdIgnoreCase(shipping.CurrencyISO))
        {
          throw new TaxException(GetType().FullName + StringConsts.TAX_CALC_DIFFERENT_CURRENCIES_ERROR + 
            ".Calc(wholesalePrice='{0}', retailPrice='{1}', shipping='{2}')".Args(wholesalePrice, retailPrice, shipping));
        }

        var currencyISO = retailPrice.CurrencyISO;

        var wholesellerHasNexus = wholesellerNexusStates.Any(s => s.EqualsOrdIgnoreCase(shippingAddress.Region));
        var retailerHasNexus = retailerNexusStates.Any(s => s.EqualsOrdIgnoreCase(shippingAddress.Region));
        var retailerHasCertificate = retailerCertificateStates.Any(cs => cs.EqualsOrdIgnoreCase(shippingAddress.Region));

        if (wholesellerHasNexus)
        {
          if (retailerHasNexus)
          {
            if (retailerHasCertificate)
            {
              var retailerTax = DoCalc(session, retailerAddress, shippingAddress, retailPrice, shipping);
              return new TaxCalculation() { CurrencyISO = currencyISO, Recipient = shippingAddress.Region, RetailerTax = retailerTax};
            }
            else
            {
              var wholesellerTax = DoCalc(session, wholesellerAddress, shippingAddress, wholesalePrice, shipping);
              var retailerTax = DoCalc(session, retailerAddress, shippingAddress, retailPrice, shipping);
              return new TaxCalculation() { CurrencyISO = currencyISO, Recipient = shippingAddress.Region, WholesellerTax = wholesellerTax, RetailerTax = retailerTax};
            }
          }
          else
          {
            if (retailerHasCertificate)
            {
              return TaxCalculation.NoneInstance;
            }
            else
            {
              var wholesellerTax = DoCalc(session, wholesellerAddress, shippingAddress, wholesalePrice, shipping);
              return new TaxCalculation() { CurrencyISO = currencyISO, Recipient = shippingAddress.Region, WholesellerTax = wholesellerTax};
            }
          }
        }
        else
        {
          if (retailerHasNexus)
          {
            var retailerTax = DoCalc(session, retailerAddress, shippingAddress, retailPrice, shipping);
            return new TaxCalculation() { CurrencyISO = currencyISO, Recipient = shippingAddress.Region, RetailerTax = retailerTax};
          }
          else
          {
            return TaxCalculation.NoneInstance;
          }
        }
      }

      public abstract ITaxStructured DoCalc(
        TaxSession session,
        IAddress fromAddress, 
        IAddress toAddress, 
        Amount amount, 
        Amount shipping);

      /// <summary>
      /// Starts new tax session of system-specific type
      /// </summary>
      public TaxSession StartSession(TaxConnectionParameters cParams = null)
      {
        return DoStartSession(cParams);
      }

      protected abstract TaxSession DoStartSession(TaxConnectionParameters cParams = null);

    #endregion

    #region IWebClientCaller

      [Config(Default = 20000)]
      public int WebServiceCallTimeoutMs
      {
        get { return m_WebServiceCallTimeoutMs; }
        set { m_WebServiceCallTimeoutMs = value < 0 ? 0 : value; }
      }

      [Config(Default = true)]
      public bool KeepAlive { get; set; }

      [Config(Default = true)]
      public bool Pipelined { get; set; }

    #endregion

    #region IInstrumentable

      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_TAX)]
      public override bool InstrumentationEnabled
      {
        get
        {
          return m_InstrumentationEnabled;
        }
        set
        {
          m_InstrumentationEnabled = value;
          if (m_InstrumentationEvent==null)
          {
            if (!value) return;
            resetStats();
            m_InstrumentationEvent = new Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTR_INTERVAL);
          }
          else
          {
            if (value) return;
            DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
          }
        }
      }

    #endregion

    #region Stat

      protected void StatTaxError()
      {
        Interlocked.Increment(ref m_stat_TaxErrorCount);
      }

      protected void StatTax(ITaxCalculation taxCalculation)
      {
        Interlocked.Increment(ref m_stat_TaxCount);

        if (taxCalculation.RetailerTax != null)
          m_stat_RetailerTaxAmounts.AddOrUpdate(taxCalculation.RetailerTax.CurrencyISO, taxCalculation.RetailerTax.Total, 
            (k, v) => v + taxCalculation.RetailerTax.Total);

        if (taxCalculation.WholesellerTax != null)
          m_stat_WholesellerTaxAmounts.AddOrUpdate(taxCalculation.WholesellerTax.CurrencyISO, taxCalculation.RetailerTax.Total, 
            (k, v) => v + taxCalculation.WholesellerTax.Total);
      }

    #endregion

    #region Protected

      protected TaxConnectionParameters DefaultSessionConnectParams
      {
        get { return m_DefaultSessionConnectParams; }
      }

      protected override void DoStart()
      {
        if (!s_Instances.Register(this))
          throw new TaxException(StringConsts.TAX_CALCULATOR_DUPLICATE_NAME_ERROR.Args(GetType().FullName, Name));
      }

      protected override void DoWaitForCompleteStop()
      {
        s_Instances.Unregister(this);
      }

      protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
      {
        dumpStats();
      }

      protected abstract TaxConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection);

    #endregion

    #region .pvt .impl

      private void dumpStats()
      {
        var src = this.Name;

        Instrumentation.TaxCaclulationCount.Record(src, m_stat_TaxCount);
        m_stat_TaxCount = 0;

        Instrumentation.TaxCaclulcationErrorCount.Record(src, m_stat_TaxErrorCount);
        m_stat_TaxErrorCount = 0;

        foreach (var a in m_stat_RetailerTaxAmounts)
        {
          string key = a.Key;
          decimal val = a.Value;

          while (true)
          {
            if (m_stat_RetailerTaxAmounts.TryUpdate(key, 0, val)) break;
            m_stat_RetailerTaxAmounts.TryGetValue(key, out val); // never fails because keys (currency ISO) are never removed from dictionary
          }

          Instrumentation.RetailerTaxAmount.Record(src, new Amount(key, val));
        } 

        foreach (var a in m_stat_WholesellerTaxAmounts)
        {
          string key = a.Key;
          decimal val = a.Value;

          while (true)
          {
            if (m_stat_WholesellerTaxAmounts.TryUpdate(key, 0, val)) break;
            m_stat_WholesellerTaxAmounts.TryGetValue(key, out val); // never fails because keys (currency ISO) are never removed from dictionary
          }

          Instrumentation.WholesellerTaxAmount.Record(src, new Amount(key, val));
        } 
      }

      private void resetStats()
      {
        m_stat_TaxCount = m_stat_TaxErrorCount = 0;
        m_stat_RetailerTaxAmounts.Clear();
        m_stat_WholesellerTaxAmounts.Clear();
      }

    #endregion
  }
}
