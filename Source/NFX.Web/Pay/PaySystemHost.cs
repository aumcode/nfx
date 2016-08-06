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
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.ServiceModel;

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
    /// Generates new transaction ID for desired pay session and transaction type (Charge, Transfer, Refund).
    /// Context supplies host specific information about this transation i.e. user id
    /// </summary>
    object GenerateTransactionID(PaySession callerSession, ITransactionContext context, TransactionType type);

    /// <summary>
    /// Returns a transaction with specified ID from storage or null
    /// </summary>
    Transaction FetchTransaction(ITransactionContext context, object id);

    /// <summary>
    /// Returns actual data for supplied account object
    /// </summary>
    IActualAccountData AccountToActualData(ITransactionContext context, Account account);

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
  public interface ITransactionContext { }

  public interface IOrderTransactionContext : ITransactionContext
  {
    object CustomerId { get; }
    object OrderId { get; }
    object VendorId { get; }
    bool IsNewCustomer { get; }
  }

  public abstract class PaySystemHost : ServiceWithInstrumentationBase<object>, IPaySystemHostImplementation
  {


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
    public override bool InstrumentationEnabled
    {
      get { return false; }
      set { }
    }
    #endregion

    public abstract ICurrencyMarket CurrencyMarket { get; }

    public abstract IActualAccountData AccountToActualData(ITransactionContext context, Account account);

    public abstract Transaction FetchTransaction(ITransactionContext context, object id);

    public abstract object GenerateTransactionID(PaySession callerSession, ITransactionContext context, TransactionType type);
  }
}
