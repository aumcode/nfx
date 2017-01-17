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

using NFX;
using NFX.Financial;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Denotes transaction types, such as: charges, refunds, transfers
  /// </summary>
  public enum TransactionType
  {
    /// <summary>
    /// Customer paid for some service (i.e. bought some product).
    /// Funds got withdrawn from customer account (i.e. credit card) into another account (i.e. system account)
    /// </summary>
    Charge,

    /// <summary>
    /// Funds were transfered between accounts.
    /// This mode is usually used to transfer funds between system-internal accounts (i.e. from sales GL account to bank account)
    /// </summary>
    Transfer
  }

  /// <summary>
  /// Denotes transaction status
  /// </summary>
  public enum TransactionStatus
  {
    Undefined = 0,
    /// <summary>
    /// The transaction was queued for async execution in future. Actual pay system implementation determines whether this status is supported.
    /// For example this is used for paypal payout where transaction is created instantly in the "promised" status and executed later by a call to Refresh()
    /// </summary>
    Promised,
    Pending,
    Success,
    Denied,
    Failed,
    Unclaimed,
    Refunded,
    Other
  }

  /// <summary>
  /// Denotes processing fee types, such as: included in amount and surcharged.
  /// </summary>
  public enum ProcessingFeeKind
  {
    /// <summary>
    /// Fees are included in amount charged from customers.
    /// Net amount = amount charged - fees.
    /// </summary>
    IncludedInAmount,

    /// <summary>
    /// Fees are added to amount charged from customers.
    /// </summary>
    Surcharged
  }

  /// <summary>
  /// Represents an abstraction of financial operation performed with pay system
  /// </summary>
  [Serializable]
  public sealed class Transaction
  {
    #region Inner
    public enum OperationType
    {
      Refresh,
      Void,
      Capture,
      Refund
    }

    public class Operation
    {
      public static Operation Refresh(TransactionStatus status, DateTime? date = null, object token = null, decimal? amount = null, decimal? fee = null, object extraData = null)
      { return new Operation(OperationType.Refresh, status, date, token: token, amount: amount, fee: fee, extraData: extraData); }
      public static Operation Void(TransactionStatus status, DateTime date, object token = null, string description = null, decimal? fee = null, object extraData = null)
      { return new Operation(OperationType.Void, status, date, token: token, description: description, amount: null, fee: fee, extraData: extraData); }
      public static Operation Capture(TransactionStatus status, DateTime date, object token = null, string description = null, decimal? amount = null, decimal? fee = null, object extraData = null)
      { return new Operation(OperationType.Capture, status, date, token: token, description: description, amount: amount, fee: fee, extraData: extraData); }
      public static Operation Refund(TransactionStatus status, DateTime date, object token = null, string description = null, decimal? amount = null, decimal? fee = null, object extraData = null)
      { return new Operation(OperationType.Refund, status, date, token: token, description: description, amount: amount, fee: fee, extraData: extraData); }

      public Operation(OperationType type, TransactionStatus status, DateTime? date = null, object token = null, string description = null, decimal? amount = null, decimal? fee = null, object extraData = null)
      {
        m_Type = type;
        m_Status = status;
        m_Date = date;
        m_Token = token;
        m_Description = description;
        m_Amount = amount;
        m_Fee = fee;
        m_ExtraData = extraData;
      }

      private OperationType m_Type;
      private TransactionStatus m_Status;
      private DateTime? m_Date;
      private object m_Token;
      private string m_Description;
      private decimal? m_Amount;
      private decimal? m_Fee;
      private object m_ExtraData;

      public OperationType Type { get { return m_Type; } }
      public TransactionStatus Status { get { return m_Status; } }
      public DateTime? Date { get { return m_Date; } }
      public object Token { get { return m_Token; } }
      public string Description { get { return m_Description; } }
      public decimal? Amount { get { return m_Amount; } }
      public decimal? Fee { get { return m_Fee; } }
      public object ExtraData { get { return m_ExtraData; } }
    }
    #endregion

    #region .ctor
    /// <summary>
    /// Framework-only method. Developer shouldn't call it.
    /// </summary>
    /// <param name="id">Trasaction ID</param>
    /// <param name="type">Trasaction type</param>
    /// <param name="status">Transaction Status</param>
    /// <param name="from">Source account</param>
    /// <param name="to">Destination account</param>
    /// <param name="processorName">Payment processor name which this trasaction belongs to</param>
    /// <param name="token">Payment processor trasaction of this trasaction</param>
    /// <param name="createDateUTC">Creation date of this trasaction</param>
    /// <param name="amount">Amount of this transaction</param>
    /// <param name="description">Description of this transaction (e.g. "Payment for CPU Intel i7 4470 SandyBridge")</param>
    /// <param name="fee">Service fee</param>
    /// <param name="extraData">Some extra data if needed</param>
    public Transaction(object id, TransactionType type, TransactionStatus status,
                       Account from, Account to,
                       string processorName, object token,
                       DateTime createDateUTC,
                       Amount amount,
                       decimal? fee = null,
                       string description = null,
                       object extraData = null)
      : this(id, type, processorName, token)
    {
      m_TransactionStatus = status;

      m_From = from;
      m_To = to;

      m_Date = createDateUTC;

      m_CurrencyISO = amount.CurrencyISO;
      m_Amount = amount.Value;

      m_Description = description;

      m_Fee = fee ?? 0.0M;
      m_ExtraData = extraData;
    }

    private Transaction(object id, TransactionType type, string processorName, object processorToken)
    {
      if (id == null) throw new PaymentException(StringConsts.ARGUMENT_ERROR + "TX.ctor(id == null)");
      if (processorName.IsNullOrWhiteSpace()) throw new PaymentException(StringConsts.ARGUMENT_ERROR + "TX.ctor(processorName == null)");
      if (processorName.IsNullOrWhiteSpace()) throw new PaymentException(StringConsts.ARGUMENT_ERROR + "TX.ctor(processorToken == null)");

      m_ID = id;
      m_TransactionType = type;

      m_Processor = processorName;
      m_Token = processorToken;
    }
    #endregion

    #region Fields
    private object m_ID;
    private TransactionType m_TransactionType;
    private TransactionStatus m_TransactionStatus;

    private Account m_From;
    private Account m_To;

    private string m_Processor;
    private object m_Token;
    private DateTime m_Date;

    private string m_Description;

    private string m_CurrencyISO;
    private decimal m_Amount;
    private decimal m_Fee;

    private object m_ExtraData;

    private List<Operation> m_Log;
    #endregion

    #region Properties
    /// <summary>
    /// Unique ID for this transaction instance
    /// </summary>
    public object ID { get { return m_ID; } }

    public TransactionType Type { get { return m_TransactionType; } }
    public TransactionStatus Status
    {
      get
      {
        if (!Log.Any()) return m_TransactionStatus;
        return Log.Last().Status;
      }
    }

    public Account From { get { return m_From; } }
    public Account To { get { return m_To; } }

    /// <summary>
    /// The name of pay system implementation system that processes this transaction
    /// </summary>
    public string Processor { get { return m_Processor; } }

    public IPaySystem PaySystem { get { return Pay.PaySystem.Instances[m_Processor]; } }

    /// <summary>
    /// A value assigned by a particular pay system implementation.
    /// This value is parsed and understood by issuing system
    /// </summary>
    public object Token
    {
      get
      {
        if (!Log.Any(l => l.Type == OperationType.Refresh)) return m_Token;
        return Log.Last(l => l.Type == OperationType.Refresh).Token ?? m_Token;
      }
    }

    /// <summary>
    /// UTC timestamp of this transaction creation
    /// </summary>
    public DateTime Date
    {
      get
      {
        if (!Log.Any(l => l.Type == OperationType.Refresh)) return m_Date;
        return Log.Last(l => l.Type == OperationType.Refresh).Date ?? m_Date;
      }
    }

    /// <summary>
    /// Textual transaction description
    /// </summary>
    public string Description { get { return m_Description ?? string.Empty; } }

    /// <summary>
    /// Transaction amount - monetary value and currency code.
    /// Depending on the transaction type can indicate charged amount, refunded amount, transfer amount
    /// </summary>
    public Amount Amount
    {
      get
      {
        var amount = m_Amount;
        if (Log.Any(l => l.Type == OperationType.Refresh))
          amount = Log.Last(l => l.Type == OperationType.Refresh).Amount ?? m_Amount;
        return new Amount(m_CurrencyISO, amount);
      }
    }

    public Amount Fee
    {
      get
      {
        var fee = m_Fee;
        if (Log.Any(l => l.Type == OperationType.Refresh))
          fee = Log.Last(l => l.Type == OperationType.Refresh).Fee ?? m_Fee;
        return new Amount(m_CurrencyISO, fee);
      }
    }

    /// <summary>
    /// Contains additional data
    /// </summary>
    public object ExtraData { get { return m_ExtraData; } }

    /// <summary>
    /// Contains log of operations performed on this transaction
    /// </summary>
    public IEnumerable<Operation> Log { get { return m_Log ?? Enumerable.Empty<Operation>(); } }

    /// <summary>
    /// True if transaction was ever voided
    /// </summary>
    public bool IsVoided { get { return m_Log != null && m_Log.Any(l => l.Type == OperationType.Void); } }

    public bool CanVoid { get { return m_Log == null || (!IsVoided && !m_Log.Any(l => l.Type == OperationType.Capture)); } }

    /// <summary>
    /// Charged amount - monetary value and currency code.
    /// For charge indicates captured amount in two-step charge scenario.
    /// Returns zero for uncaptured charges (created with captured flag set to false).
    /// Returns non-zero amount (typically is equal to original charge amount) for captured amount.
    /// Returns zero for all other transaction types
    /// </summary>
    public Amount AmountCaptured
    {
      get
      {
        if (m_Log == null || IsVoided) return new Amount(m_CurrencyISO, 0.0M);
        return new Amount(m_CurrencyISO, m_Log.Where(l => l.Type == OperationType.Capture).Sum(l => l.Amount ?? 0.0M));
      }
    }

    /// <summary>
    /// If this transaction can be captured by value less or equal to its initial amount
    /// </summary>
    public bool CanCapture { get { return m_TransactionType == TransactionType.Charge && !IsVoided && (m_Amount - AmountCaptured.Value > 0.0M); } }

    public Amount LeftToCapture { get { return CanCapture ? Amount - AmountCaptured : new Amount(m_CurrencyISO, 0.0M); } }

    /// <summary>
    /// Transaction amount - monetary value and currency code.
    /// Indicates refunded amount for charge transaction.
    /// Can be reflected in child refund transactions when explicit refund performed
    /// or can be just reminder of partial capture.
    /// For transactions types other than charge returns zero
    /// </summary>
    public Amount AmountRefunded
    {
      get
      {
        if (m_Log == null || IsVoided) return new Amount(m_CurrencyISO, 0.0M);
        return new Amount(m_CurrencyISO, m_Log.Where(l => l.Type == OperationType.Refund).Sum(l => l.Amount ?? 0.0M));
      }
    }

    /// <summary>
    /// If this transaction can be refunded by value less or equal to its initial amount
    /// </summary>
    public bool CanRefund { get { return m_TransactionType == TransactionType.Charge && !IsVoided && (AmountCaptured.Value - AmountRefunded.Value > 0.0M); } }

    public Amount LeftToRefund { get { return CanCapture ? AmountCaptured - AmountRefunded : new Amount(m_CurrencyISO, 0.0M); } }
    #endregion

    #region Public
    public bool Refresh(ConnectionParameters cParams = null, IPaySessionContext context = null)
    {
      using (var session = PaySystem.StartSession(cParams, context))
        return session.Refresh(this);
    }

    public bool Void(string description = null, object extraData = null, ConnectionParameters cParams = null, IPaySessionContext context = null)
    {
      using (var session = PaySystem.StartSession(cParams, context))
        return session.Void(this, description, extraData);
    }

    public bool Capture(decimal? amount = null, string description = null, object extraData = null, ConnectionParameters cParams = null, IPaySessionContext context = null)
    {
      using (var session = PaySystem.StartSession(cParams, context))
        return session.Capture(this, amount, description, extraData);
    }

    public bool Refund(decimal? amount = null, string description = null, object extraData = null, ConnectionParameters cParams = null, IPaySessionContext context = null)
    {
      using (var session = PaySystem.StartSession(cParams, context))
        return session.Refund(this, amount, description, extraData);
    }

    /// <summary>
    /// Must be call by PaySystem. Developers do not call internal method
    /// </summary>
    public void __Apply(Operation operation)
    {
      if (m_Log == null) m_Log = new List<Operation>();
      m_Log.Add(operation);
    }

    #region Object overrides
    public override string ToString()
    { return "TX[{0} {1} {2}]({3}, {4}) of {5} '{6}' -> '{7}'".Args(m_Processor, m_TransactionType, m_TransactionStatus, m_ID, Amount, Log.Count(), m_From, m_To); }

    public override int GetHashCode()
    { return m_ID.GetHashCode(); }
    #endregion
    #endregion
  }
}
