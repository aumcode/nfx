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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NFX;
using NFX.Environment;
using NFX.Financial;
using NFX.Security;
using NFX.Serialization.JSON;
using System.Xml.Linq;

namespace NFX.Web.Pay.Braintree
{
  public sealed class BraintreeSystem : PaySystem
  {
    #region CONSTS
    public const string MERCHANTS_URI = "/merchants/{0}";

    private const string HDR_AUTHORIZATION = "Authorization";
    private const string HDR_AUTHORIZATION_BASIC = "Basic {0}";
    private const string HDR_AUTHORIZATION_OAUTH = "Bearer {0}";

    private const string BASIC_AUTH_FORMAT = "{0}:{1}";

    private const string HDR_X_API_VERSION = "X-ApiVersion";
    public const string API_VERSION = "4";

    public Uri URI_ClientToken(string merchantID) { return new Uri(m_ApiUri, MERCHANTS_URI.Args(merchantID) + "/client_token"); }
    public Uri URI_Transactions(string merchantID) { return new Uri(m_ApiUri, MERCHANTS_URI.Args(merchantID) + "/transactions"); }
    public Uri URI_SubmitForSettlement(string merchantID, string id)
    {
      return new Uri(m_ApiUri, MERCHANTS_URI.Args(merchantID) + "/transactions/{0}/submit_for_settlement".Args(id));
    }
    public Uri URI_Void(string merchantID, string id)
    {
      return new Uri(m_ApiUri, MERCHANTS_URI.Args(merchantID) + "/transactions/{0}/void".Args(id));
    }
    public Uri URI_Customer(string merchantID, string customerID) { return new Uri(m_ApiUri, MERCHANTS_URI.Args(merchantID) + "/customers/" + customerID); }
    #endregion

    #region .ctor
    public BraintreeSystem(string name, IConfigSectionNode node) : this((Uri)null, name, node, null) { }
    public BraintreeSystem(string name, IConfigSectionNode node, object director) : this((Uri)null, name, node, director) { }
    public BraintreeSystem(string apiUri, string name, IConfigSectionNode node, object director) : this(apiUri.IsNotNullOrWhiteSpace() ? new Uri(apiUri) : null, name, node, director) {}
    public BraintreeSystem(Uri apiUri, string name, IConfigSectionNode node, object director) : base(name, node, director)
    { if (apiUri != null) m_ApiUri = apiUri; }
    #endregion

    #region fields
    private BraintreeWebTerminal m_WebTerminal;
    [Config]
    private Uri m_ApiUri;
    #endregion


    #region Properties
    public override string ComponentCommonName { get { return "braintreepayments"; } }

    public override IPayWebTerminal WebTerminal
    {
      get
      {
        if (m_WebTerminal == null)
          m_WebTerminal = new BraintreeWebTerminal(this);
        return m_WebTerminal;
      }
    }
    #endregion

    public object GenerateClientToken(PaySession session)
    {
      return GenerateClientToken((BraintreeSession)session);
    }

    public object GenerateClientToken(BraintreeSession session)
    {

      var response = getResponse(session, URI_ClientToken(session.MerchantID), new XDocument(new XElement("client-token", new XElement("version", 2))));
      return response.Element("client-token").Element("value").Value;
    }

    public override Transaction Charge(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
    {
      return Charge((BraintreeSession)session, context, from, to, amount, capture, description, extraData);
    }

    public Transaction Charge(BraintreeSession session, ITransactionContext context, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
    {
      var orderContex = context as IOrderTransactionContext;

      var fromActualData = PaySystemHost.AccountToActualData(context, from);

      try
      {
        var isWeb = fromActualData.Account.IsWebTerminalToken;
        var transaction = new XElement("transaction",
                            new XElement("type", "sale"),
                            new XElement("amount", amount.Value),
                            new XElement(isWeb ? "payment-method-nonce" : "payment-method-token", fromActualData.Account.AccountID));
        if (orderContex != null)
        {
          transaction.Add(new XElement("order-id", orderContex.OrderId));
          if (isWeb)
          {
            if (orderContex.IsNewCustomer)
              transaction.Add(new XElement("customer", new XElement("id", orderContex.CustomerId)));
            else
              try
              {
                getResponse(session, URI_Customer(session.MerchantID, orderContex.CustomerId.AsString()), method: HTTPRequestMethod.GET);
                transaction.Add(new XElement("customer-id", orderContex.CustomerId));
              }
              catch
              {
                transaction.Add(new XElement("customer", new XElement("id", orderContex.CustomerId)));
              }
          }
        }
        if (isWeb)
        {
          var billing = new XElement("billing");

          if (fromActualData.FirstName.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("first-name", fromActualData.FirstName));
          if (fromActualData.LastName.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("last-name", fromActualData.LastName));
          if (fromActualData.BillingAddress.Address1.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("street-address", fromActualData.BillingAddress.Address1));
          if (fromActualData.BillingAddress.Address2.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("extended-address", fromActualData.BillingAddress.Address2));
          if (fromActualData.BillingAddress.City.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("locality", fromActualData.BillingAddress.City));
          if (fromActualData.BillingAddress.Country.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("country-code-alpha3", fromActualData.BillingAddress.Country));
          if (fromActualData.BillingAddress.Region.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("region", fromActualData.BillingAddress.Region));
          if (fromActualData.BillingAddress.PostalCode.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("postal-code", fromActualData.BillingAddress.PostalCode));
          if (fromActualData.BillingAddress.Company.IsNotNullOrWhiteSpace())
            billing.Add(new XElement("company", fromActualData.BillingAddress.Company));

          transaction.Add(billing);
        }

        var options = new XElement("options", new XElement("submit-for-settlement", capture));
        if (orderContex != null && isWeb)
          options.Add(new XElement("store-in-vault-on-success", true));

        transaction.Add(options);

        var response = getResponse(session, URI_Transactions(session.MerchantID), new XDocument(transaction));

        transaction = response.Root;

        var transactionID = transaction.Element("id").Value;
        var status = transaction.Element("status").Value;
        var captured = status.EqualsOrdIgnoreCase("submitted_for_settlement");
        var created = transaction.Element("created-at").Value.AsDateTime();
        var customerID = transaction.Element("customer").Element("id").Value;
        var token = transaction.Element("credit-card").Element("token").Value;

        var amountAuth = new Amount(amount.CurrencyISO, transaction.Element("amount").Value.AsDecimal());
        var amountCaptured = captured ? (Amount?)amountAuth : null;

        var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Charge);
        var ta = Transaction.Charge(taId, Name,
                                    "{0}:{1}:{2}".Args(customerID, token, transactionID),
                                    from, to,
                                    amountAuth, created, description,
                                    amountCaptured: amountCaptured);

        StatCharge(amount);

        return ta;
      }
      catch (Exception ex)
      {
        StatChargeError();
        if (ex is PaymentException) throw ex;
        throw new PaymentException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + GetType().Name
          + " .Charge(session='{0}', card='{1}', amount='{2}')".Args(session, from, amount), ex);
      }
    }

    public override Transaction Capture(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = default(Amount?), string description = null, object extraData = null)
    {
      return Capture((BraintreeSession)session, context, ref charge, amount, description, extraData);
    }

    public Transaction Capture(BraintreeSession session, ITransactionContext context, ref Transaction charge, Amount? amount = default(Amount?), string description = null, object extraData = null)
    {
      if (!charge.ProcessorName.EqualsIgnoreCase(Name))
        throw new PaymentException(StringConsts.PAYMENT_INVALID_PAYSYSTEM_ERROR + GetType().Name
          + ".Capture(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount.Value));

      if (amount.HasValue && !charge.Amount.CurrencyISO.EqualsOrdIgnoreCase(amount.Value.CurrencyISO))
        throw new PaymentException(StringConsts.PAYMENT_CAPTURE_CURRENCY_MUST_MATCH_CHARGE_ERROR + GetType().Name
          + ".Capture(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount.Value));

      var orderContex = context as IOrderTransactionContext;

      try
      {
        var splitTran = charge.ProcessorToken.AsString().Split(':');

        var transaction = new XElement("transaction");
        if (amount.HasValue) transaction.Add(new XElement("amount", amount.Value.Value));
        if (orderContex != null) transaction.Add(new XElement("order-id", orderContex.OrderId));

        var response = getResponse(session, URI_SubmitForSettlement(session.MerchantID, splitTran[2]), new XDocument(transaction), HTTPRequestMethod.PUT);

        transaction = response.Root;
        var transactionID = transaction.Element("id").Value;
        var created = transaction.Element("created-at").Value.AsDateTime();
        var customerID = transaction.Element("customer").Element("id").Value;
        var token = transaction.Element("credit-card").Element("token").Value;

        var amountCaptured = new Amount(charge.Amount.CurrencyISO, transaction.Element("amount").Value.AsDecimal());

        charge = Transaction.Charge(charge.ID, Name,
                                    "{0}:{1}:{2}".Args(customerID, token, transactionID),
                                    charge.From, charge.To,
                                    charge.Amount, created, description,
                                    amountCaptured);

        StatCapture(charge, amount);
        return charge;
      }
      catch (Exception ex)
      {
        StatChargeError();
        if (ex is PaymentException) throw ex;
        throw new PaymentException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + GetType().Name
          + " .Capture(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount), ex);
      }
    }

    public override Transaction Refund(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = default(Amount?), string description = null, object extraData = null)
    {
      return Refund((BraintreeSession)session, context, ref charge, amount, description, extraData);
    }

    public Transaction Refund(BraintreeSession session, ITransactionContext context, ref Transaction charge, Amount? amount = default(Amount?), string description = null, object extraData = null)
    {
      if (!charge.ProcessorName.EqualsIgnoreCase(Name))
        throw new PaymentException(StringConsts.PAYMENT_INVALID_PAYSYSTEM_ERROR + GetType().Name
          + ".Refund(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount.Value));

      if (amount.HasValue && !charge.Amount.CurrencyISO.EqualsOrdIgnoreCase(amount.Value.CurrencyISO))
        throw new PaymentException(StringConsts.PAYMENT_REFUND_CURRENCY_MUST_MATCH_CHARGE_ERROR + GetType().Name
          + ".Refund(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount.Value));

      var orderContex = context as IOrderTransactionContext;

      try
      {
        var splitTran = charge.ProcessorToken.AsString().Split(':');

        if (charge.AmountCaptured.Value != 0m) throw new NotImplementedException();

        var response = getResponse(session, URI_Void(session.MerchantID, splitTran[2]), method: HTTPRequestMethod.PUT);

        var transaction = response.Root;
        var transactionID = transaction.Element("id").Value;
        var created = transaction.Element("created-at").Value.AsDateTime();
        var customerID = transaction.Element("customer").Element("id").Value;
        var token = transaction.Element("credit-card").Element("token").Value;

        var amountRefunded = new Amount(charge.Amount.CurrencyISO, transaction.Element("amount").Value.AsDecimal());

        charge = Transaction.Refund(charge.ID, Name,
                                    "{0}:{1}:{2}".Args(customerID, token, transactionID),
                                    charge.From, charge.To,
                                    charge.Amount, created, description,
                                    amountRefunded, charge.ID);

        StatRefund(charge, charge.AmountCaptured);
        return charge;
      }
      catch (Exception ex)
      {
        StatRefundError();
        if (ex is PaymentException) throw ex;
        throw new PaymentException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + GetType().Name
          + " .Refund(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount), ex);
      }
    }

    public override Transaction Transfer(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, string description = null, object extraData = null)
    {
      throw new NotImplementedException();
    }

    public override PaymentException VerifyPotentialTransaction(PaySession session, ITransactionContext context, bool transfer, IActualAccountData from, IActualAccountData to, Amount amount)
    {
      throw new NotImplementedException();
    }

    protected override PaySession DoStartSession(PayConnectionParameters cParams = null)
    {
      return new BraintreeSession(this, (BraintreeConnectionParameters)(cParams ?? DefaultSessionConnectParams));
    }

    protected override PayConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
    {
      return PayConnectionParameters.Make<BraintreeConnectionParameters>(paramsSection);
    }

    #region .pvt
    private XDocument getResponse(BraintreeSession session, Uri uri, XDocument body = null, HTTPRequestMethod method = HTTPRequestMethod.POST)
    {
      if (!session.IsValid)
        throw new PaymentException("Braintree: " + StringConsts.PAYMENT_BRAINTREE_SESSION_INVALID.Args(this.GetType().Name + ".getResponse"));

      var prms = new WebClient.RequestParams()
      {
        Caller = this,
        Uri = uri,
        Method = method,
        AcceptType = ContentType.XML_APP,
        ContentType = ContentType.XML_APP,
        Headers = new Dictionary<string, string>()
        {
          { HDR_AUTHORIZATION, getAuthHeader(session.User.Credentials) },
          { HDR_X_API_VERSION, API_VERSION }
        },
        Body = body != null ? new XDeclaration("1.0", "UTF-8", null).ToString() + body.ToString(SaveOptions.DisableFormatting) : null
      };

      try {
        return WebClient.GetXML(prms);
      }
      catch (System.Net.WebException ex)
      {
        StatChargeError();
        var resp = (System.Net.HttpWebResponse)ex.Response;

        if (resp != null && resp.StatusCode == (System.Net.HttpStatusCode)422)
        {
          using (var sr = new StreamReader(resp.GetResponseStream()))
          {
            var respStr = sr.ReadToEnd();
            var response = respStr.IsNotNullOrWhiteSpace() ? XDocument.Parse(respStr) : null;
            if (response != null)
            {
              var apiErrorResponse = response.Element("api-error-response");
              throw new PaymentException(apiErrorResponse.Element("message").Value, ex);
            }
          }
        }
        throw;
      }
      catch
      {
        throw;
      }
    }

    private string getAuthHeader(Credentials credentials)
    {
      if (credentials is BraintreeCredentials)
        return HDR_AUTHORIZATION_BASIC.Args(getBasicAuthString((credentials as BraintreeCredentials)));
      if (credentials is BraintreeAuthCredentials)
        return HDR_AUTHORIZATION_OAUTH.Args((credentials as BraintreeAuthCredentials).AccessToken);
      throw new NFXException("{0}.getAuthHeader({1})".Args(GetType().Name, credentials.GetType().Name));
    }

    private string getBasicAuthString(BraintreeCredentials braintreeCredentials)
    {
      var bytes = Encoding.UTF8.GetBytes(BASIC_AUTH_FORMAT.Args(braintreeCredentials.PublicKey, braintreeCredentials.PrivateKey));
      return Convert.ToBase64String(bytes);
    }
    #endregion
  }
}
