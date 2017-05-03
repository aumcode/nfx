/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.ServiceModel;
using NFX.Log;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Represents a process-global entity that resolves account handle into actual account data
  /// and fetches existing transactions.
  /// This design provides an indirection level between pay systems (like Stripe, PayPal, Bank etc.) and
  /// particular application data store implementation as it decouples system-internal formats of transaction and
  /// account storage from provider-internal data (i.e. PayPal payment token string).
  /// The instance of implementor is a singleton accessible via PaySystem.PaySystemHost
  /// </summary>
  public interface IPaySystemHost: INamed
  {
    /// <summary>
    /// Returns a transaction with specified ID from storage or null
    /// </summary>
    Transaction FetchTransaction(object id);

    /// <summary>
    /// Returns actual data for supplied account object
    /// </summary>
    IActualAccountData FetchAccountData(Account account);

    /// <summary>
    /// Currency market
    /// </summary>
    ICurrencyMarket CurrencyMarket { get; }
  }

  /// <summary>
  /// Denotes an implementation of IPaySystemHost
  /// </summary>
  public interface IPaySystemHostImplementation : IPaySystemHost, IConfigurable {}

  /// <summary>
  /// Denotes a context of transaction execution.
  /// Can be used to provide additional information
  /// </summary>
  public interface IPaySessionContext { }

  public abstract class PaySystemHost : ServiceWithInstrumentationBase<object>, IPaySystemHostImplementation
  {
    #region CONST
    private const string LOG_TOPIC = "PaySystemHost";
    private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region .ctor
    protected PaySystemHost(string name, IConfigSectionNode node): this(name, node, null) { }

    protected PaySystemHost(string name, IConfigSectionNode node, object director): base(director)
    {
      if (node != null) Configure(node);
      if (name.IsNotNullOrWhiteSpace()) this.Name = name;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    public override bool InstrumentationEnabled { get { return false; } set { } }

    [Config(Default = DEFAULT_LOG_LEVEL)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_PAY)]
    public MessageType LogLevel { get; set; }
    #endregion

    public Transaction FetchTransaction(object id)
    { return DoFetchTransaction(null, id); }

    public IActualAccountData FetchAccountData(Account account)
    { return DoFetchAccountData(null, account); }

    public abstract ICurrencyMarket CurrencyMarket { get; }

    protected internal virtual IPaySessionContext GetDefaultTransactionContext() { return null; }

    protected internal abstract object DoGenerateTransactionID(PaySession session, TransactionType type);

    protected internal abstract Transaction DoFetchTransaction(PaySession session, object id);

    protected internal abstract IActualAccountData DoFetchAccountData(PaySession session, Account account);

    protected internal abstract void DoStoreTransaction(PaySession session, Transaction tran);

    protected internal abstract void DoStoreAccountData(PaySession session, IActualAccountData accoundData);

    protected internal virtual Guid Log(MessageType type, string from, string message, Exception error = null, Guid? relatedMessageID = null, string parameters = null)
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
  }
}
