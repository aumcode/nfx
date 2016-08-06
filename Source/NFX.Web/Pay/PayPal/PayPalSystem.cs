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

          public const string CFG_SYNC_MODE = "sync-mode";
          public const string CFG_TOKEN_EXPIRATION_MARGIN = "oauth-expiration-margin";
          public const string CFG_API_URI = "api-uri";
          public const string CFG_PAYOUT_EMAIL_SUBJECT = "payout-email-subject";
          public const string CFG_PAYOUT_NOTE = "payout-note";

          private const string URI_API_BASE = "https://api.paypal.com";
          private const string URI_API_SANDBOX_BASE = "https://api.sandbox.paypal.com";
          private const string URI_GET_OAUTH_TOKEN = "/v1/oauth2/token";
          private const string URI_PAYOUTS = "/v1/payments/payouts";

          private const string HDR_AUTHORIZATION = "Authorization";
          private const string HDR_AUTHORIZATION_BASIC = "Basic {0}";
          private const string HDR_AUTHORIZATION_OAUTH = "Bearer {0}";

          private const string EMAIL_RECIPIENT_TYPE = "EMAIL";

          private const string PRM_GRANT_TYPE = "grant_type";
          private const string PRM_CLIENT_CREDENTIALS = "client_credentials";
          private const string PRM_SYNC_MODE = "sync_mode";

          private const bool   DEFAULT_SYNC_MODE = true;
          private const int    DEFAULT_TOKEN_EXPIRATION_MARGIN = 3600; // 1 hour default margin to check oauth token expiration;
          private const string DEFAULT_API_URI = URI_API_BASE;
          private const string DEFAULT_PAYOUT_EMAIL_SUBJECT = "Payout from SL.RS";
          private const string DEFAULT_PAYOUT_NOTE = "Thanks for using SL.RS!";

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

          private const string BASIC_AUTH_FORMAT = "{0}:{1}";
          private const string CURRENCY_FORMAT = "{0:0.00}";
          private const string EMPTY = "-";

        #endregion

        #region .ctor

          public PayPalSystem(string name, IConfigSectionNode node) : base(name, node)
          {
          }

          public PayPalSystem(string name, IConfigSectionNode node, object director) : base(name, node, director)
          {
          }

        #endregion

        #region Fields

          private string m_ApiUri;
          private int m_OAuthTokenExpirationMargin;
          private string m_PayoutEmailSubject;
          private string m_PayoutNote;
          private bool m_SyncMode;

        #endregion

        #region PaySystem implementation

          public override IPayWebTerminal WebTerminal { get { throw new NotImplementedException(); } }

          /// <summary>
          /// The operation is not supported in PayPal payout action.
          /// </summary>
          public override void Capture(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = default(Amount?), string description = null, object extraData = null)
          {
              throw new NotSupportedException();
          }

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
                      refreshOAuthToken(((PayPalSession)session).ConnectionParameters);
                  }

                  return doTransfer(session, context, from, to, amount, description, extraData);
              }
          }

          protected override PaySession DoStartSession(PayConnectionParameters cParams = null)
          {
              var connectionParameters = cParams ?? DefaultSessionConnectParams;
              return StartSession((PayPalConnectionParameters)connectionParameters);
          }

          public PayPalSession StartSession(PayPalConnectionParameters cParams)
          {
              ensureCurrentOAuthToken(cParams);
              return new PayPalSession(this, cParams);
          }

          protected override PayConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
          {
              return PayConnectionParameters.Make<PayPalConnectionParameters>(paramsSection);
          }

        #endregion

        protected override void DoConfigure(IConfigSectionNode node)
        {
            base.DoConfigure(node);

            m_ApiUri = node.AttrByName(CFG_API_URI).ValueAsString(DEFAULT_API_URI);
            m_OAuthTokenExpirationMargin = node.AttrByName(CFG_TOKEN_EXPIRATION_MARGIN).ValueAsInt(DEFAULT_TOKEN_EXPIRATION_MARGIN);
            m_PayoutEmailSubject = node.AttrByName(CFG_PAYOUT_EMAIL_SUBJECT).ValueAsString(DEFAULT_PAYOUT_EMAIL_SUBJECT);
            m_PayoutNote = node.AttrByName(CFG_PAYOUT_NOTE).ValueAsString(DEFAULT_PAYOUT_NOTE);
            m_SyncMode = node.AttrByName(CFG_SYNC_MODE).ValueAsBool(DEFAULT_SYNC_MODE);
        }

        #region .pvt

          private void ensureCurrentOAuthToken(PayPalConnectionParameters connectParameters)
          {
              var token = connectParameters.User.AuthToken.Data as PayPalOAuthToken;
              if (token == null || token.IsCloseToExpire())
                  refreshOAuthToken(connectParameters);
          }

          private void refreshOAuthToken(PayPalConnectionParameters connectParameters)
          {
              try
              {
                  Log(MessageType.Info, "refreshOAuthToken()", StringConsts.PAYPAL_REFRESH_TOKEN_MESSAGE);

                  var user = connectParameters.User;

                  var request = new WebClient.RequestParams
                  {
                      Caller = this,
                      Uri = new Uri(m_ApiUri + URI_GET_OAUTH_TOKEN),
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
                  Log(MessageType.Info, "refreshOAuthToken()", response.ToJSON());

                  var oauthToken = new PayPalOAuthToken(response, m_OAuthTokenExpirationMargin);
                  var token = new AuthenticationToken(PAYPAL_REALM, oauthToken);
                  connectParameters.User = new User(user.Credentials, token, user.Name, user.Rights);

                  Log(MessageType.Info, "refreshOAuthToken()", StringConsts.PAYPAL_TOKEN_REFRESHED_MESSAGE);
              }
              catch (Exception ex)
              {
                  var message = StringConsts.PAYPAL_REFRESH_TOKEN_ERROR.Args(ex.ToMessageWithType());
                  var error = PayPalPaymentException.ComposeError(message, ex);
                  Log(MessageType.Error, "refreshOAuthToken()", error.Message, ex);

                  throw error;
              }
          }

          private Transaction doTransfer(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, string description = null, object extraData = null)
          {
              var id = Log(MessageType.Info, "doTransfer()", StringConsts.PAYPAL_PAYOUT_MESSAGE.Args(to, amount));

              try
              {
                  var payPalSession = session as PayPalSession;
                  var actualAccountData = PaySystemHost.AccountToActualData(context, to);

                  var request = new WebClient.RequestParams
                  {
                      Caller = this,
                      Uri = new Uri(m_ApiUri + URI_PAYOUTS),
                      QueryParameters = new Dictionary<string, string>
                          {
                              { PRM_SYNC_MODE, m_SyncMode.AsString() }
                          },
                      Method = HTTPRequestMethod.POST,
                      ContentType = ContentType.JSON,
                      Headers = new Dictionary<string, string>
                          {
                              { HDR_AUTHORIZATION, HDR_AUTHORIZATION_OAUTH.Args(payPalSession.AuthorizationToken.AccessToken) },
                          },
                      Body = getPayoutJSONBody(actualAccountData, amount, description)
                  };

                  var response = WebClient.GetJson(request);
                  Log(MessageType.Info, "doTransfer()", response.ToJSON(), null, id);
                  checkPayoutStatus(response);

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
              return new {
                       sender_batch_header = new {
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

          private static void checkPayoutStatus(JSONDataMap response)
          {
              var batchStatus = response.GetNodeByPath(RESPONSE_BATCH_HEADER, RESPONSE_BATCH_STATUS);
              if (batchStatus.AsString() != RESPONSE_SUCCESS)
              {
                  var errorName = response.GetNodeByPath(RESPONSE_ERRORS, RESPONSE_ERRORS_NAME);
                  var errorMessage = response.GetNodeByPath(RESPONSE_ERRORS, RESPONSE_ERRORS_MESSAGE);
                  var errorDescription = response.GetNodeByPath(RESPONSE_ERRORS, RESPONSE_ERRORS_DETAILS);
                  var message = StringConsts.PAYPAL_PAYOUT_DENIED_MESSAGE.Args(errorName ?? EMPTY,
                                                                               errorMessage ?? EMPTY,
                                                                               errorDescription ?? EMPTY);

                  throw new PayPalPaymentException(message);
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

        #endregion
    }
}
