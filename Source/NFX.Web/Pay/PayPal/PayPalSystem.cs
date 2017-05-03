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
using System.Globalization;
using System.Net;
using System.Text;

using NFX.DataAccess.CRUD;
using NFX.Environment;
using NFX.Financial;
using NFX.Serialization.JSON;
using NFX.Log;
using System.Linq;
using System.IO;

namespace NFX.Web.Pay.PayPal
{
  /// <summary>
  /// Represents PayPal (see https://www.paypal.com/) payment service.
  ///
  /// NOTE: Before use WebClient, please set in your configuration file
  /// 1. ServicePoint's Expect100Continue to 'false' (100-continue header can reduce performace and is not supported by some web accellerator),
  /// 2. HttpRequest timeout and any other ServicePoint's settings if needeed.
  /// </summary>
  public partial class PayPalSystem : PaySystem
  {
    #region CONSTS
    public const string PAYPAL_REALM = "paypal";

    private const string URI_API = "https://api.paypal.com";
    private const string URI_API_SANDBOX = "https://api.sandbox.paypal.com";

    private const string HDR_AUTHORIZATION = "Authorization";

    private const string PRM_GRANT_TYPE = "grant_type";
    private const string PRM_CLIENT_CREDENTIALS = "client_credentials";
    private const string PRM_SYNC_MODE = "sync_mode";

    private const string DEFAULT_API_URI = URI_API_SANDBOX;
    private const int    DEFAULT_TOKEN_EXPIRATION_MARGIN = 3600; // 1 hour default margin to check oauth token expiration;
    private const string DEFAULT_PAYOUT_EMAIL_SUBJECT = "Payout from NFX";
    private const string DEFAULT_PAYOUT_NOTE = "Thanks for using NFX!";
    private const bool   DEFAULT_SYNC_MODE = true;
    #endregion

    #region .ctor
    public PayPalSystem(string name, IConfigSectionNode node) : base(name, node) {}
    public PayPalSystem(string name, IConfigSectionNode node, object director) : base(name, node, director) { }
    public PayPalSystem(string apiUri, string name, IConfigSectionNode node, object director) : this(apiUri.IsNotNullOrWhiteSpace() ? new Uri(apiUri) : null, name, node, director) { }
    public PayPalSystem(Uri apiUri, string name, IConfigSectionNode node, object director) : base(name, node, director) { if (apiUri != null) m_ApiUri = apiUri; }
    #endregion

    #region Fields
    [Config(Default = DEFAULT_API_URI)]
    private Uri m_ApiUri = new Uri(DEFAULT_API_URI);
    [Config(Default = DEFAULT_TOKEN_EXPIRATION_MARGIN)]
    private int m_OAuthExpirationMargin = DEFAULT_TOKEN_EXPIRATION_MARGIN;
    [Config(Default = DEFAULT_PAYOUT_EMAIL_SUBJECT)]
    private string m_PayoutEmailSubject = DEFAULT_PAYOUT_EMAIL_SUBJECT;
    [Config(Default = DEFAULT_PAYOUT_NOTE)]
    private string m_PayoutNote = DEFAULT_PAYOUT_NOTE;
    [Config(Default = DEFAULT_SYNC_MODE)]
    private bool m_SyncMode = DEFAULT_SYNC_MODE;
    #endregion

    #region PaySystem Implementation
    protected override ConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
    { return ConnectionParameters.Make<PayPalConnectionParameters>(paramsSection); }

    protected override PaySession DoStartSession(ConnectionParameters cParams, IPaySessionContext context = null)
    { return new PayPalSession(this, (PayPalConnectionParameters)cParams, context); }

    protected internal override Transaction DoCharge(PaySession session, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
    { throw new NotSupportedException(); }

    protected internal override bool DoVoid(PaySession session, Transaction transaction, string description = null, object extraData = null)
    {
      var ps = (PayPalSession)session;
      bool retry = true;
      while (true)
        try
        {
          var authToken = ps.AuthorizationToken;
          return doVoid(ps, authToken, transaction, description, extraData);
        }
        catch (System.Net.WebException ex)
        {
          var status = ((HttpWebResponse)ex.Response).StatusCode;
          if (status == HttpStatusCode.Unauthorized && retry)
          {
            Log(MessageType.Warning, "DoRefresh()", "Unauthorized payment. Trying to refresh OAuth token.");
            retry = false;
            ps.ResetAuthorizationToken();
            continue;
          }
          throw;
        }
    }

    protected internal override bool DoCapture(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
    { throw new NotSupportedException(); }

    protected internal override bool DoRefund(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
    { throw new NotSupportedException(); }

    protected internal override bool DoRefresh(PaySession session, Transaction transaction, object extraData = null)
    {
      var ps = (PayPalSession)session;
      bool retry = true;
      while (true)
        try
        {
          var authToken = ps.AuthorizationToken;
          if (transaction.Status == TransactionStatus.Promised)
            return doTransfer(ps, authToken, transaction, extraData);
          return doRefresh(ps, authToken, transaction, extraData);
        }
        catch (System.Net.WebException ex)
        {
          var status = ((HttpWebResponse)ex.Response).StatusCode;
          if (status == HttpStatusCode.Unauthorized && retry)
          {
            Log(MessageType.Warning, "DoRefresh()", "Unauthorized payment. Trying to refresh OAuth token.");
            retry = false;
            ps.ResetAuthorizationToken();
            continue;
          }
          throw;
        }
    }

    protected internal override Transaction DoTransfer(PaySession session, Account from, Account to, Amount amount, string description = null, object extraData = null)
    {
      if (description.IsNullOrWhiteSpace())
        description = m_PayoutNote;

      return new Transaction(
        session.GenerateTransactionID(TransactionType.Transfer), TransactionType.Transfer, TransactionStatus.Promised,
        from, to, Name, null, App.TimeSource.UTCNow, amount, description: description, extraData: extraData);
    }
    #endregion

    #region Private
    private bool doTransfer(PayPalSession session, PayPalOAuthToken authToken, Transaction transaction, object extraData = null)
    {
      var logID = Log(MessageType.Info, "doTransfer()", "Transfer {0}".Args(transaction.Amount));

      var fromActualData = session.FetchAccountData(transaction.From);
      var toActualData = session.FetchAccountData(transaction.To);

      var payouts = createPayouts(
        new payout
        {
          SenderBatchHeader = new payoutSenderBatchHeader
          {
            SenderBatchID = transaction.ID.AsString(),
            EmailSubject = m_PayoutEmailSubject,
            RecipientType = payoutRecipientType.EMAIL
          },
          Items = new List<payoutItem>
          {
            new payoutItem
            {
              SenderItemId = transaction.ID.AsString(),
              RecipientType = payoutRecipientType.EMAIL,
              Amount = transaction.Amount,
              Note = transaction.Description,
              Receiver = toActualData.AccountID.AsString()
            }
          }
        }, authToken, logID);

      switch (payouts.BatchHeader.BatchStatus)
      {
        case payoutBatchStatus.NEW:
        case payoutBatchStatus.PENDING:
        case payoutBatchStatus.PROCESSING:
          transaction.__Apply(Transaction.Operation.Refresh(TransactionStatus.Pending, App.TimeSource.UTCNow, payouts.BatchHeader.PayoutBatchID, transaction.Amount.Value, extraData: extraData));
          return true;
        case payoutBatchStatus.SUCCESS:
          var payoutItem = payouts.Items.FirstOrAnyOrDefault(item => item.PayoutItem.SenderItemId.Equals(transaction.ID.ToString()));
          if (payoutItem == null)
            throw new PaymentException();

          var transactionStatus = mapTransactionStatus(payoutItem.TransactionStatus);
          switch (transactionStatus)
          {
            case TransactionStatus.Pending:
            case TransactionStatus.Unclaimed:
            case TransactionStatus.Refunded:
              break;
            case TransactionStatus.Success:
              StatTransfer(transaction.Amount);
              break;
            default:
              throw new PaymentException();
          }
          transaction.__Apply(Transaction.Operation.Refresh(transactionStatus, payouts.BatchHeader.TimeCreated.Value, payoutItem.Token, transaction.Amount.Value, payoutItem.PayoutItemFee.Value, extraData: extraData));
          return true;
      }
      throw new PaymentException();
    }

    private bool doRefresh(PayPalSession session, PayPalOAuthToken authToken, Transaction transaction, object extraData = null)
    {
      var logID = Log(MessageType.Info, "doRefresh()", "Refresh transaction");

      payoutItemDetails payoutItem = null;
      var payoutItemID = getPayoutItemID(transaction);
      if (payoutItemID.IsNullOrWhiteSpace())
      {
        var payouts = getPayouts(getBatchID(transaction), authToken, logID);
        switch (payouts.BatchHeader.BatchStatus)
        {
          case payoutBatchStatus.NEW:
          case payoutBatchStatus.PENDING:
          case payoutBatchStatus.PROCESSING:
            return false;
        }
        if (!payouts.Items.Any()) return false;
        payoutItem = payouts.Items.FirstOrDefault(item => item.PayoutItem.SenderItemId.Equals(transaction.ID.AsString()));
      }
      else payoutItem = getPayoutItem(payoutItemID, authToken, logID);
      if (payoutItem == null) return false;

      var transactionStatus = mapTransactionStatus(payoutItem.TransactionStatus);
      if (transaction.Status == transactionStatus)
        return false;

      transaction.__Apply(Transaction.Operation.Refresh(transactionStatus, payoutItem.TimeProcessed, payoutItem.Token, payoutItem.Amount, payoutItem.PayoutItemFee.Value, extraData));
      return true;
    }

    private bool doVoid(PayPalSession session, PayPalOAuthToken authToken, Transaction transaction, string description = null, object extraData = null)
    {
      var logID = Log(MessageType.Info, "doVoid()", "Void transaction");

      var payoutItemID = getPayoutItemID(transaction);
      if (payoutItemID.IsNullOrWhiteSpace())
      {
        if (doRefresh(session, authToken, transaction, extraData))
        {
          session.StoreTransaction(transaction);
          payoutItemID = getPayoutItemID(transaction);
        }
        else return false;
      }

      var payoutItem = cancelPayoutItem(payoutItemID, authToken, logID);
      if (payoutItem.TransactionStatus != payoutTransactionStatus.RETURNED)
        return false;

      transaction.__Apply(Transaction.Operation.Void(TransactionStatus.Refunded, payoutItem.TimeProcessed.Value, payoutItem.Token, description, extraData: extraData));
      return true;
    }
    #endregion
  }
}
