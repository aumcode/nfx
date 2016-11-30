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
using System.Globalization;
using System.Net;
using System.Text;

using NFX.Environment;
using NFX.Financial;
using NFX.Security;
using NFX.Serialization.JSON;
using NFX.Log;
using NFX.Serialization;

namespace NFX.Web.Pay.PayPal
{
  /// <summary>
  /// Represents PayPal (see https://www.paypal.com/) payment service.
  ///
  /// NOTE: Before use WebClient, please set in your configuration file
  /// 1. ServicePoint's Expect100Continue to 'false' (100-continue header can reduce performace and is not supported by some web accellerator),
  /// 2. HttpRequest timeout and any other ServicePoint's settings if needeed.
  /// </summary>
  public class PayPalSystem : PaySystem
  {
    #region CONSTS

    public const string PAYPAL_REALM = "paypal";

    private const string URI_API = "https://api.paypal.com";
    private const string URI_API_SANDBOX = "https://api.sandbox.paypal.com";
    private const string URI_GET_OAUTH_TOKEN = "/v1/oauth2/token";
    private const string URI_PAYOUTS = "/v1/payments/payouts";
    private const string URI_CANCEL_UNCLAIMED_PAYOUT = "/v1/payments/payouts-item/{0}/cancel";

    private const string HDR_AUTHORIZATION = "Authorization";
    private const string HDR_AUTHORIZATION_BASIC = "Basic {0}";

    private const string EMAIL_RECIPIENT_TYPE = "EMAIL";

    private const string PRM_GRANT_TYPE = "grant_type";
    private const string PRM_CLIENT_CREDENTIALS = "client_credentials";
    private const string PRM_SYNC_MODE = "sync_mode";

    private const string DEFAULT_API_URI = URI_API_SANDBOX;
    private const int    DEFAULT_TOKEN_EXPIRATION_MARGIN = 3600; // 1 hour default margin to check oauth token expiration;
    private const string DEFAULT_PAYOUT_EMAIL_SUBJECT = "Payout from NFX";
    private const string DEFAULT_PAYOUT_NOTE = "Thanks for using NFX!";
    private const bool   DEFAULT_SYNC_MODE = true;

    private const string RESPONSE_BATCH_HEADER = "batch_header";
    private const string RESPONSE_BATCH_STATUS = "batch_status";
    private const string RESPONSE_BATCH_ID = "payout_batch_id";
    private const string RESPONSE_SUCCESS = "SUCCESS";
    private const string RESPONSE_ERRORS = "errors";
    private const string RESPONSE_ITEMS = "items";
    private const string RESPONSE_ERRORS_NAME = "name";
    private const string RESPONSE_ERRORS_MESSAGE = "message";
    private const string RESPONSE_ERRORS_DETAILS = "details";
    private const string RESPONSE_ITEMS_ITEMID = "payout_item_id";
    private const string RESPONSE_TIME_COMPLETED = "time_completed";
    private const string RESPONSE_ITEM_TRAN_STATUS = "transaction_status";
    private const string RESPONSE_ITEM_UNCLAIMED = "UNCLAIMED";
    private const string RESPONSE_ITEM_SUCCESS = "SUCCESS";
    private const string RESPONSE_ITEM_ERRORS = "errors";
    private const string RESPONSE_ITEM_ERRORS_NAME = "name";
    private const string RESPONSE_ITEM_ERRORS_MESSAGE = "message";

    private const string BASIC_AUTH_FORMAT = "{0}:{1}";
    private const string CURRENCY_FORMAT = "{0:0.00}";
    private const string EMPTY = "-";

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

    #region PaySystem implementation

    public override IPayWebTerminal WebTerminal { get { throw new NotImplementedException(); } }

    /// <summary>
    /// The operation is not supported in PayPal payout action.
    /// </summary>
    public override Transaction Charge(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// The operation is not supported in PayPal payout action.
    /// </summary>
    public override Transaction Capture(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = default(Amount?), string description = null, object extraData = null)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// The operation is not supported in PayPal payout action.
    /// </summary>
    public override Transaction Refund(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = default(Amount?), string description = null, object extraData = null)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// The operation is not supported in PayPal payout action.
    /// </summary>
    public override PaymentException VerifyPotentialTransaction(PaySession session, ITransactionContext context, bool transfer, IActualAccountData from, IActualAccountData to, Amount amount)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Transfers funds from current application's business account to PayPal user account.
    /// 'from' account isn't used as current application's business account is used instead.
    /// </summary>
    public override Transaction Transfer(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, string description = null, object extraData = null)
    {
      try
      {
        return doTransfer(session, context, from, to, amount, description, extraData);
      }
      catch (System.Net.WebException ex)
      {
        var status = ((HttpWebResponse)ex.Response).StatusCode;
        if (status == HttpStatusCode.Unauthorized)
        {
          Log(MessageType.Error, "Transfer()", StringConsts.PAYPAL_UNAUTHORIZED_MESSAGE);
          var payPalSession = (PayPalSession)session;
          refreshOAuthToken(payPalSession);
          return doTransfer(payPalSession, context, from, to, amount, description, extraData);
        }

        throw;
      }
    }

    protected override PaySession DoStartSession(ConnectionParameters cParams = null)
    {
      var connectionParameters = (PayPalConnectionParameters)(cParams ?? DefaultSessionConnectParams);
      return new PayPalSession(this, connectionParameters);
    }

    protected override ConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
    {
      return ConnectionParameters.Make<PayPalConnectionParameters>(paramsSection);
    }

    #endregion

    #region .pvt

    private void ensureCurrentOAuthToken(PayPalSession session)
    {
      var token = session.AuthorizationToken;
      if (token == null || token.IsCloseToExpire())
        refreshOAuthToken(session);
    }

    private void refreshOAuthToken(PayPalSession session)
    {
      var id = Log(MessageType.Info, "refreshOAuthToken()", StringConsts.PAYPAL_REFRESH_TOKEN_MESSAGE);

      try
      {
        var user = session.User;

        var request = new WebClient.RequestParams
        {
          Caller = this,
          Uri = new Uri(m_ApiUri, URI_GET_OAUTH_TOKEN),
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.FORM_URL_ENCODED,
          Headers = new Dictionary<string, string>
          {
            { HDR_AUTHORIZATION, HDR_AUTHORIZATION_BASIC.Args(getBaseAuthString(user.Credentials)) }
          },
          BodyParameters = new Dictionary<string, string>
          {
            { PRM_GRANT_TYPE, PRM_CLIENT_CREDENTIALS }
          }
        };

        var response = WebClient.GetJson(request);
        Log(MessageType.Info, "refreshOAuthToken()", response.ToJSON(), relatedMessageID: id);

        var oauthToken = new PayPalOAuthToken(response, m_OAuthExpirationMargin);
        var token = new AuthenticationToken(PAYPAL_REALM, oauthToken);
        session.User = new User(user.Credentials, token, user.Name, user.Rights);

        Log(MessageType.Info, "refreshOAuthToken()", StringConsts.PAYPAL_TOKEN_REFRESHED_MESSAGE, relatedMessageID: id);
      }
      catch (Exception ex)
      {
        var message = StringConsts.PAYPAL_REFRESH_TOKEN_ERROR.Args(ex.ToMessageWithType());
        var error = PayPalPaymentException.ComposeError(message, ex);
        Log(MessageType.Error, "refreshOAuthToken()", error.Message, ex, relatedMessageID: id);

        throw error;
      }
    }

    private Transaction doTransfer(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, string description = null, object extraData = null)
    {
      var id = Log(MessageType.Info, "doTransfer()", StringConsts.PAYPAL_PAYOUT_MESSAGE.Args(to, amount));

      try
      {
        var actualAccountData = PaySystemHost.AccountToActualData(context, to);
        var payPalSession = session as PayPalSession;
        ensureCurrentOAuthToken(payPalSession);

        var request = new WebClient.RequestParams
        {
          Caller = this,
          Uri = new Uri(m_ApiUri, URI_PAYOUTS),
          QueryParameters = new Dictionary<string, string>
          {
              { PRM_SYNC_MODE, m_SyncMode.AsString() }
          },
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
          {
              { HDR_AUTHORIZATION, payPalSession.AuthorizationToken.AuthorizationHeader },
          },
          Body = getPayoutJSONBody(actualAccountData, amount, description)
        };

        var response = WebClient.GetJson(request);
        Log(MessageType.Info, "doTransfer()", response.ToJSON(), null, id);

        checkPayoutStatus(response, payPalSession);

        var transaction = createPayoutTransaction(session, context, response, to, amount, description);

        StatTransfer(amount);

        return transaction;
      }
      catch (Exception ex)
      {
        StatTransferError();

        var message = StringConsts.PAYPAL_PAYOUT_ERROR.Args(to, amount, ex.ToMessageWithType());
        var error = PayPalPaymentException.ComposeError(message, ex);
        Log(MessageType.Error, "doTransfer()", error.Message, ex, id);

        throw error;
      }
    }

    private string getPayoutJSONBody(IActualAccountData to, Amount amount, string description)
    {
      return new
      {
        sender_batch_header = new
        {
          sender_batch_id = generateBatchID(),  // maximum length: 30.
          email_subject = m_PayoutEmailSubject, // maximum of 255 single-byte alphanumeric characters.
          recipient_type = EMAIL_RECIPIENT_TYPE // EMAIL, PHONE, PAYPAL_ID
        },
        items = new[] {
          new {
            recipient_type = EMAIL_RECIPIENT_TYPE,
            amount = new {
              currency = amount.CurrencyISO.ToUpperInvariant(),
              value = string.Format(CURRENCY_FORMAT, amount.Value, CultureInfo.InvariantCulture)
            },
            note = description.IsNullOrWhiteSpace() ? m_PayoutNote : description,
            receiver = to.PrimaryEMail,
            sender_item_id = 1  // maximum length: 30.
          }
        }
      }.ToJSON(JSONWritingOptions.Compact);
    }

    private void checkPayoutStatus(JSONDataMap response, PayPalSession payPalSession)
    {
      var batchStatus = response.GetNodeByPath(RESPONSE_BATCH_HEADER, RESPONSE_BATCH_STATUS).AsString();
      if (!batchStatus.EqualsIgnoreCase(RESPONSE_SUCCESS))
      {
        throwPaymentException(response.GetNodeByPath(RESPONSE_ERRORS, RESPONSE_ERRORS_NAME).AsString(),
                              response.GetNodeByPath(RESPONSE_ERRORS, RESPONSE_ERRORS_MESSAGE).AsString(),
                              response.GetNodeByPath(RESPONSE_ERRORS, RESPONSE_ERRORS_DETAILS).AsString());
      }

      var items = response[RESPONSE_ITEMS] as JSONDataArray;
      if (items == null || items.Count <= 0) return;

      // for the moment there is only one possible payment in a batch
      var item = (JSONDataMap)items[0];
      var status = item[RESPONSE_ITEM_TRAN_STATUS].AsString();

      // cancel unclaimed transaction
      if (status.EqualsIgnoreCase(RESPONSE_ITEM_UNCLAIMED))
      {
        var itemID = item[RESPONSE_ITEMS_ITEMID].AsString();
        cancelUnclaimedPayout(itemID, payPalSession);
        throwPaymentException(item.GetNodeByPath(RESPONSE_ITEM_ERRORS, RESPONSE_ITEM_ERRORS_NAME).AsString(),
                              item.GetNodeByPath(RESPONSE_ITEM_ERRORS, RESPONSE_ITEM_ERRORS_MESSAGE).AsString(),
                              EMPTY);
      }

      // todo: other statuses https://developer.paypal.com/docs/api/payments.payouts-batch#payouts_get
      if (!status.EqualsIgnoreCase(RESPONSE_ITEM_SUCCESS))
      {
        throwPaymentException(item.GetNodeByPath(RESPONSE_ITEM_ERRORS, RESPONSE_ITEM_ERRORS_NAME).AsString(),
                              item.GetNodeByPath(RESPONSE_ITEM_ERRORS, RESPONSE_ITEM_ERRORS_MESSAGE).AsString(),
                              status);
      }
    }

    private void cancelUnclaimedPayout(string itemID, PayPalSession payPalSession)
    {
      try
      {
        ensureCurrentOAuthToken(payPalSession);

        var request = new WebClient.RequestParams
        {
          Caller = this,
          Uri = new Uri(m_ApiUri, URI_CANCEL_UNCLAIMED_PAYOUT.Args(itemID)),
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
          {
            { HDR_AUTHORIZATION, payPalSession.AuthorizationToken.AuthorizationHeader },
          }
        };

        var response = WebClient.GetJson(request);
        Log(MessageType.Info, "cancelUnclaimedPayout()", response.ToJSON());
      }
      catch (Exception ex)
      {
        var message = StringConsts.PAYPAL_PAYOUT_CANCEL_ERROR.Args(ex.ToMessageWithType());
        var error = PayPalPaymentException.ComposeError(message, ex);
        Log(MessageType.Error, "cancelUnclaimedPayout()", error.Message, ex);

        throw error;
      }
    }

    private Transaction createPayoutTransaction(PaySession session, ITransactionContext context, JSONDataMap response, Account to, Amount amount, string description = null)
    {
      var batchID = response.GetNodeByPath(RESPONSE_BATCH_HEADER, RESPONSE_BATCH_ID);
      object itemID = null;
      var items = response.GetNodeByPath(RESPONSE_ITEMS) as JSONDataArray;
      if (items != null && items.Count > 0)
      {
        // for the moment there is only one possible payment in a batch
        itemID = ((JSONDataMap)items[0]).GetNodeByPath(RESPONSE_ITEMS_ITEMID);
      }

      var processorToken = new { batchID = batchID.AsString(), itemID = itemID.AsString() };
      var transactionDate = response.GetNodeByPath(RESPONSE_BATCH_HEADER, RESPONSE_TIME_COMPLETED)
                                    .AsDateTime(App.TimeSource.UTCNow);

      var transactionID = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Transfer);

      return Transaction.Transfer(transactionID,
                             this.Name, processorToken,
                             Account.EmptyInstance,
                             to,
                             amount,
                             transactionDate,
                             description);
    }

    private static string getBaseAuthString(Credentials credentials)
    {
      var payPalCredentials = credentials as PayPalCredentials;
      var bytes = Encoding.UTF8.GetBytes(BASIC_AUTH_FORMAT.Args(payPalCredentials.ClientID, payPalCredentials.ClientSecret));
      return Convert.ToBase64String(bytes);
    }

    private static string generateBatchID()
    {
      return Guid.NewGuid().ToString("N").Substring(0, 30);
    }

    private static void throwPaymentException(string name, string message, string description)
    {
      var text = StringConsts.PAYPAL_PAYOUT_NOT_SUCCEEDED_MESSAGE.Args(name ?? EMPTY,
                                                                       message ?? EMPTY,
                                                                       description ?? EMPTY);
      throw new PayPalPaymentException(text);
    }

    #endregion
  }
}
