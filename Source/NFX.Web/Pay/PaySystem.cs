/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using NFX.Environment;
using NFX.Financial;
using NFX.ServiceModel;
using NFX.ApplicationModel;
using NFX.Log;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Base class for IPaySystemImplementation implementation.
  /// Instances of descendants of this class is typically created and configured in PaySystemStarter class.
  /// Then particular pay system can be obtained from PaySystem.Instances[PAY_SYSTEM_NAME] indexer
  /// </summary>
  public abstract class PaySystem : ServiceWithInstrumentationBase<object>, IWebClientCaller, IPaySystemImplementation
  {
    #region CONSTS
      public const string CONFIG_PAYMENT_PROCESSING_SECTION = "payment-processing";
      public const string CONFIG_CURRENCY_MARKET_SECTION = "currency-market";
      public const string CONFIG_CURRENCIES_SECTION = "currencies";
      public const string CONFIG_PAY_SYSTEM_HOST_SECTION = "pay-system-host";
      public const string CONFIG_PAY_SYSTEM_SECTION = "pay-system";
      public const string CONFIG_AUTO_START_ATTR = "auto-start";
      public const string CONFIG_FEE_FLAT_ATTR = "fee-flat";
      public const string CONFIG_FEE_PCT_ATTR = "fee-pct";
      public const string CONFIG_FEE_TRAN_TYPE_ATTR = "tran-type";

      private const string LOG_TOPIC = "Payment.Processing";

      private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;

      private static readonly TimeSpan INSTR_INTERVAL = TimeSpan.FromMilliseconds(4015);
    #endregion

    #region Static
      private static IPaySystemHost s_PaySystemHost;

      /// <summary>
      /// Returns process-global pay system host used to resolve accounts and transactions
      /// or throws if host is not set. Check IsPaySystemHost to see if host is set.
      /// This design provides an indirection level between pay systems (like Stripe, PayPal, Bank etc.) and
      /// particular application data store implementation as it decouples system-internal formats of transaction and
      /// account storage from provider-internal data (i.e. PayPal payment token string)
      /// </summary>
      public static IPaySystemHost PaySystemHost
      {
        get
        {
          var result = s_PaySystemHost;

          if (result == null)
            throw new PaymentException(StringConsts.PAYMENT_SYSTEM_HOST_NULL_ERROR);

          return result;
        }
      }

      public static bool IsPaySystemHost { get { return s_PaySystemHost != null; } }

      /// <summary>
      /// Sets process-global pay system host used to resolve accounts and transactions.
      /// If PayStarter is used then the host may be auto-injected from configuration if it is there.
      /// Developers: Do not call this method as it is used for dependency injection from system code
      /// </summary>
      public static void ___SetPaySystemHost(IPaySystemHostImplementation host)
      {
        s_PaySystemHost = host;
      }


      private static Registry<PaySystem> s_Instances = new Registry<PaySystem>();

      /// <summary>
      /// Returns the read-only registry view of payment systems currently activated
      /// </summary>
      public static IRegistry<IPaySystem> Instances { get { return s_Instances; } }

      /// <summary>
      /// Automatically starts systems designated in config with auto-start attribute
      /// </summary>
      public static void AutoStartSystems()
      {
        App.Instance.RegisterAppFinishNotifiable(PayProcessingFinisher.Instance);

        WebSettings.RequireInitializedSettings();

        var pHost = App.ConfigRoot[WebSettings.CONFIG_WEBSETTINGS_SECTION][CONFIG_PAYMENT_PROCESSING_SECTION][CONFIG_PAY_SYSTEM_HOST_SECTION];
        if (pHost.Exists)
        {
          var host = FactoryUtils.MakeAndConfigure<PaySystemHost>(pHost, typeof(PaySystemHost), new object[] { null, pHost });
          host.Start();
          ___SetPaySystemHost(host);
        }

        foreach (var psNode in App.ConfigRoot[WebSettings.CONFIG_WEBSETTINGS_SECTION][CONFIG_PAYMENT_PROCESSING_SECTION]
                                  .Children
                                  .Where(cn => cn.IsSameName(CONFIG_PAY_SYSTEM_SECTION)))
        {
          var name = psNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;

          if (!psNode.AttrByName(CONFIG_AUTO_START_ATTR).ValueAsBool()) continue;

          var system = FactoryUtils.MakeAndConfigure<PaySystem>(psNode, typeof(PaySystem), new object[] { null, psNode });

          if (s_Instances[system.Name] != null) // already started
            throw new PaymentException("AutoStart: " + StringConsts.PAYMENT_SYSTEM_DUPLICATE_NAME_ERROR.Args(system.GetType().FullName, system.Name));

          system.Start();
        }
      }

      public static TPaySystem Make<TPaySystem>(string name, IConfigSectionNode node) where TPaySystem: PaySystem
      {
        WebSettings.RequireInitializedSettings();

        return FactoryUtils.MakeAndConfigure<TPaySystem>(node, typeof(TPaySystem), new object[] {name, node});
      }

      public static TPaySystem Make<TPaySystem>(string name, string cfgStr, string format = Configuration.CONFIG_LACONIC_FORMAT) where TPaySystem: PaySystem
      {
        WebSettings.RequireInitializedSettings();

        var cfg = Configuration.ProviderLoadFromString(cfgStr, format);
        return Make<TPaySystem>(name, cfg.Root);
      }
    #endregion

    #region Inner
      private class PayProcessingFinisher: IApplicationFinishNotifiable
      {
        internal static readonly PayProcessingFinisher Instance = new PayProcessingFinisher();

        public string Name { get { return GetType().FullName; } }

        public void ApplicationFinishBeforeCleanup(IApplication application)
        {
          foreach (var paySystem in s_Instances)
            paySystem.WaitForCompleteStop();
        }

        public void ApplicationFinishAfterCleanup(IApplication application) {}
      }
    #endregion

    #region ctor
      protected PaySystem(string name, IConfigSectionNode node): this(name, node, null) {}

      protected PaySystem(string name, IConfigSectionNode node, object director): base(director)
      {
        LogLevel = DEFAULT_LOG_LEVEL;
        KeepAlive = true;
        Pipelined = true;

        if (node != null)
        {
          Configure(node);
          name = node.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        }

        if (name.IsNullOrWhiteSpace()) name = GetType().Name;

        this.Name = name;

        Sessions = new List<PaySession>();
      }

      protected override void Destructor()
      {
        DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
        base.Destructor();
      }
    #endregion

    #region Fields
      private IConfigSectionNode m_DefaultSessionConnParamsCfg;
      private ConnectionParameters m_DefaultSessionConnectParams;


      private int m_WebServiceCallTimeoutMs;

      private bool m_InstrumentationEnabled;
      private Time.Event m_InstrumentationEvent;

      #region Stat
        private long m_stat_ChargeCount, m_stat_ChargeErrorCount;
        private ConcurrentDictionary<string, decimal> m_stat_ChargeAmounts = new ConcurrentDictionary<string, decimal>();

        private long m_stat_VoidCount, m_stat_VoidErrorCount;
        private ConcurrentDictionary<string, decimal> m_stat_VoidAmounts = new ConcurrentDictionary<string, decimal>();

        private long m_stat_CaptureCount, m_stat_CaptureErrorCount;
        private ConcurrentDictionary<string, decimal> m_stat_CaptureAmounts = new ConcurrentDictionary<string, decimal>();

        private long m_stat_RefundCount, m_stat_RefundErrorCount;
        private ConcurrentDictionary<string, decimal> m_stat_RefundAmounts = new ConcurrentDictionary<string, decimal>();

        private long m_stat_TransferCount, m_stat_TransferErrorCount;
        private ConcurrentDictionary<string, decimal> m_stat_TransferAmounts = new ConcurrentDictionary<string, decimal>();
      #endregion
    #endregion

    #region Properties
      protected internal readonly List<PaySession> Sessions;

      public virtual IPayWebTerminal WebTerminal { get { return null; } }

      public virtual ProcessingFeeKind ChargeFeeKind { get { return ProcessingFeeKind.IncludedInAmount; } }
      public virtual ProcessingFeeKind TransferFeeKind { get { return ProcessingFeeKind.Surcharged; } }

      public virtual IEnumerable<string> SupportedCurrencies
      {
        get { return CurrenciesCfg.Children.Select(c => c.Name).Distinct().ToList(); }
      }

      /// <summary>
      /// Implements IInstrumentable
      /// </summary>
      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_PAY)]
      public override bool InstrumentationEnabled
      {
        get { return m_InstrumentationEnabled;}
        set
        {
          m_InstrumentationEnabled = value;
          if (m_InstrumentationEvent==null)
          {
            if (!value) return;
            m_InstrumentationEvent = new Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTR_INTERVAL);
          }
          else
          {
            if (value) return;
            DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
          }
        }
      }

      [Config("default-session-connect-params")]
      public IConfigSectionNode DefaultSessionConnectParamsCfg
      {
        get { return m_DefaultSessionConnParamsCfg; }
        set
        {
          m_DefaultSessionConnectParams = MakeDefaultSessionConnectParams(value);
          m_DefaultSessionConnParamsCfg = value;
        }
      }

      [Config(CONFIG_CURRENCIES_SECTION)]
      public IConfigSectionNode CurrenciesCfg { get; set; }

      /// <summary>
      /// Specifies the log level for operations performed by Pay System.
      /// </summary>
      [Config(Default = DEFAULT_LOG_LEVEL)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_PAY)]
      public MessageType LogLevel { get; set; }

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
    #endregion

    #region Public
      /// <summary>
      /// Starts new pay session of system-specific type
      /// </summary>
      public PaySession StartSession(ConnectionParameters cParams = null, IPaySessionContext context = null) { return DoStartSession(cParams ?? DefaultSessionConnectParams, context); }

      public virtual bool IsTransactionTypeSupported(TransactionType type, string currencyISO = null)
      {
        // 1. Get currencies.
        var currencies = currencyISO.IsNotNullOrEmpty()
                       ? CurrenciesCfg.Children.Where(c => c.IsSameName(currencyISO))
                       : CurrenciesCfg.Children;

        // 2. Get all tran-type attributes.
        var types = currencies.Select(c => c.AttrByName(CONFIG_FEE_TRAN_TYPE_ATTR));

        // 3. Check explicit tran-types first then implicit ones.
        return types.Any(t => t.Exists && t.Value.EqualsOrdIgnoreCase(type.ToString()))
            || types.Any(t => !t.Exists);
      }
    #endregion

    #region Protected
      protected ConnectionParameters DefaultSessionConnectParams { get { return m_DefaultSessionConnectParams; } }
      protected abstract ConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection);

      protected abstract PaySession DoStartSession(ConnectionParameters cParams, IPaySessionContext context = null);

      protected internal virtual PaymentException DoVerifyPotentialTransaction(PaySession session, TransactionType type, Account from, Account to, Amount amount) { return null; }
      protected internal virtual bool DoRefresh(PaySession session, Transaction transaction, object extraData = null) { return false; }

      protected internal abstract Transaction DoTransfer(PaySession session, Account from, Account to, Amount amount, string description = null, object extraData = null);
      protected internal abstract Transaction DoCharge(PaySession session, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null);
      protected internal abstract bool DoVoid(PaySession session, Transaction charge, string description = null, object extraData = null);
      protected internal abstract bool DoCapture(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null);
      protected internal abstract bool DoRefund(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null);

      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node == null)
        {
          node = App.ConfigRoot[CONFIG_PAYMENT_PROCESSING_SECTION];
          if (!node.Exists) return;

          //1 try to find the server with the same name as this instance
          var snode = node.Children.FirstOrDefault(cn => cn.IsSameName(CONFIG_PAY_SYSTEM_SECTION) && cn.IsSameNameAttr(Name));

          //2 try to find a server without a name
          if (snode == null)
            snode = node.Children.FirstOrDefault(cn => cn.IsSameNameAttr(CONFIG_PAY_SYSTEM_SECTION) && cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.IsNullOrWhiteSpace());

          if (snode == null) return;

          node = snode;
        }

        ConfigAttribute.Apply(this, node);
      }

      protected override void DoStart()
      {
        if (!s_Instances.Register(this))
          throw new PaymentException(StringConsts.PAYMENT_SYSTEM_DUPLICATE_NAME_ERROR.Args(GetType().FullName, Name));
      }

      protected override void DoWaitForCompleteStop()
      {
        s_Instances.Unregister(this);
      }

      protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
      {
        dumpStats();
      }

      protected Guid Log(MessageType type,
                         string from,
                         string message,
                         Exception error = null,
                         Guid? relatedMessageID = null,
                         string parameters = null)
      {
        if (type < LogLevel) return Guid.Empty;

        var logMessage = new Message
        {
          Topic = LOG_TOPIC,
          Text = message ?? string.Empty,
          Type = type,
          From = "{0}.{1}".Args(this.GetType().Name, from),
          Exception = error,
          Parameters = parameters
        };
        if (relatedMessageID.HasValue) logMessage.RelatedTo = relatedMessageID.Value;

        App.Log.Write(logMessage);

        return logMessage.Guid;
      }

      #region Stat
        protected void StatChargeError()
        {
          Interlocked.Increment(ref m_stat_ChargeErrorCount);
        }

        protected void StatCharge(Amount amount)
        {
          Interlocked.Increment(ref m_stat_ChargeCount);
          m_stat_ChargeAmounts.AddOrUpdate(amount.CurrencyISO, amount.Value, (k, v) => v + amount.Value);
        }

        protected void StatCaptureError()
        {
          Interlocked.Increment(ref m_stat_CaptureErrorCount);
        }

        protected void StatCapture(Transaction charge, decimal? amount)
        {
          Interlocked.Increment(ref m_stat_CaptureCount);
          var instrAmount = amount ?? charge.Amount.Value;
          m_stat_CaptureAmounts.AddOrUpdate(charge.Amount.CurrencyISO, instrAmount, (k, v) => v + instrAmount);
        }

        protected void StatRefundError()
        {
          Interlocked.Increment(ref m_stat_RefundErrorCount);
        }

        protected void StatRefund(Transaction charge, decimal? amount)
        {
          Interlocked.Increment(ref m_stat_VoidCount);
          var instrAmount = amount ?? charge.Amount.Value;
          m_stat_RefundAmounts.AddOrUpdate(charge.Amount.CurrencyISO, instrAmount, (k, v) => v + instrAmount);
        }

        protected void StatVoidError()
        {
          Interlocked.Increment(ref m_stat_VoidErrorCount);
        }

        protected void StatVoid(Transaction charge)
        {
          Interlocked.Increment(ref m_stat_RefundCount);
          var instrAmount = charge.Amount;
          m_stat_VoidAmounts.AddOrUpdate(instrAmount.CurrencyISO, instrAmount.Value, (k, v) => v + instrAmount.Value);
        }

        protected void StatTransferError()
        {
          Interlocked.Increment(ref m_stat_TransferErrorCount);
        }

        protected void StatTransfer(Amount amount)
        {
          Interlocked.Increment(ref m_stat_TransferCount);
          m_stat_TransferAmounts.AddOrUpdate(amount.CurrencyISO, amount.Value, (k, v) => v + amount.Value);
        }
      #endregion
    #endregion

    #region Private
      #region Stat
        private void dumpStats()
        {
          var src = this.Name;

          #region Charge

          Instrumentation.ChargeCount.Record(src, m_stat_ChargeCount);
          m_stat_ChargeCount = 0;

          Instrumentation.ChargeErrorCount.Record(src, m_stat_ChargeErrorCount);
          m_stat_ChargeErrorCount = 0;

          foreach (var a in m_stat_ChargeAmounts)
          {
            string key = a.Key;
            decimal val = a.Value;

            while (true)
            {
              if (m_stat_ChargeAmounts.TryUpdate(key, 0, val)) break;
              m_stat_ChargeAmounts.TryGetValue(key, out val); // never fails because keys (currency ISO) are never removed from dictionary
            }

            Instrumentation.ChargeAmount.Record(src, new Amount(key, val));
          }

          #endregion

          #region Capture

          Instrumentation.CaptureCount.Record(src, m_stat_CaptureCount);
          m_stat_CaptureCount = 0;

          Instrumentation.CaptureErrorCount.Record(src, m_stat_CaptureErrorCount);
          m_stat_CaptureErrorCount = 0;

          foreach (var a in m_stat_CaptureAmounts)
          {
            string key = a.Key;
            decimal val = a.Value;

            while (true)
            {
              if (m_stat_CaptureAmounts.TryUpdate(key, 0, val)) break;
              m_stat_CaptureAmounts.TryGetValue(key, out val); // never fails because keys (currency ISO) are never removed from dictionary
            }

            Instrumentation.CaptureAmount.Record(src, new Amount(key, val));
          }

          #endregion

          #region Refund

          Instrumentation.RefundCount.Record(src, m_stat_RefundCount);
          m_stat_RefundCount = 0;

          Instrumentation.RefundErrorCount.Record(src, m_stat_RefundErrorCount);
          m_stat_RefundErrorCount = 0;

          foreach (var a in m_stat_RefundAmounts)
          {
            string key = a.Key;
            decimal val = a.Value;

            while (true)
            {
              if (m_stat_RefundAmounts.TryUpdate(key, 0, val)) break;
              m_stat_RefundAmounts.TryGetValue(key, out val); // never fails because keys (currency ISO) are never removed from dictionary
            }

            Instrumentation.RefundAmount.Record(src, new Amount(key, val));
          }

          #endregion

          #region Transfer

          Instrumentation.TransferCount.Record(src, m_stat_TransferCount);
          m_stat_TransferCount = 0;

          Instrumentation.TransferErrorCount.Record(src, m_stat_TransferErrorCount);
          m_stat_TransferErrorCount = 0;

          foreach (var a in m_stat_TransferAmounts)
          {
            string key = a.Key;
            decimal val = a.Value;

            while (true)
            {
              if (m_stat_TransferAmounts.TryUpdate(key, 0, val)) break;
              m_stat_TransferAmounts.TryGetValue(key, out val); // never fails because keys (currency ISO) are never removed from dictionary
            }

            Instrumentation.TransferAmount.Record(src, new Amount(key, val));
          }

          #endregion
        }

        private void resetStats()
        {
          m_stat_ChargeCount = m_stat_ChargeErrorCount = 0;
          m_stat_ChargeAmounts.Clear();

          m_stat_CaptureCount = m_stat_CaptureErrorCount = 0;
          m_stat_CaptureAmounts.Clear();

          m_stat_RefundCount = m_stat_RefundErrorCount = 0;
          m_stat_RefundAmounts.Clear();

          m_stat_TransferCount = m_stat_TransferErrorCount = 0;
          m_stat_TransferAmounts.Clear();
        }
      #endregion
    #endregion
  }

  public abstract class PaySystemWithStaticFees : PaySystem
  {
    private struct Fee
    {
      public decimal Flat;
      public decimal Pct;
    }

    protected PaySystemWithStaticFees(string name, IConfigSectionNode node): this(name, node, null) { }
    protected PaySystemWithStaticFees(string name, IConfigSectionNode node, object director) : base(name, node, director) { }

    private Dictionary<string, Fee> m_Fees = new Dictionary<string, Fee>(StringComparer.OrdinalIgnoreCase);

    protected Amount GetTransactionFee(string currencyISO, TransactionType type)
    {
      var fee = getCurrencyFee(currencyISO, type);
      return new Amount(currencyISO, fee.Flat);
    }

    protected int GetTransactionPct(string currencyISO, TransactionType type)
    {
      var fee = getCurrencyFee(currencyISO, type);
      return (fee.Pct * 10000).AsInt();
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      m_Fees = new Dictionary<string, Fee>(StringComparer.OrdinalIgnoreCase);

      foreach (var currency in CurrenciesCfg.Children)
      {
        var flat = currency.AttrByName(CONFIG_FEE_FLAT_ATTR);
        var pct = currency.AttrByName(CONFIG_FEE_PCT_ATTR);
        var type = currency.AttrByName(CONFIG_FEE_TRAN_TYPE_ATTR);

        var fee = new Fee();
        fee.Flat = flat.ValueAsDecimal();
        fee.Pct = pct.ValueAsDecimal();

        m_Fees[currency.Name + type.Value] = fee;
      }
    }

    /// <summary>
    /// Returns fee for specified currency and transaction type.
    /// </summary>
    private Fee getCurrencyFee(string currencyISO, TransactionType type)
    {
      // 1. Look for specific transaction type.
      var key = currencyISO + type;
      if (m_Fees.ContainsKey(key))
        return m_Fees[key];

      // 2. Look for general settings.
      if (m_Fees.ContainsKey(currencyISO))
        return m_Fees[currencyISO];

      throw new PaymentException(StringConsts.PAYMENT_CURRENCY_NOT_SUPPORTED_ERROR.Args(currencyISO, GetType().FullName));
    }
  }
}
