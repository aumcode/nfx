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
    public const string BRAINTREE_REALM = "braintree";

    private const string URI_API         = "https://api.braintreegateway.com";
    private const string URI_API_SANDBOX = "https://api.sandbox.braintreegateway.com";

    private const string DEFAULT_API_URI = URI_API_SANDBOX;

    private const string HDR_AUTHORIZATION = "Authorization";
    private const string HDR_X_API_VERSION = "X-ApiVersion";
    private const string API_VERSION = "4";

    private const string URI_MERCHANTS = "/merchants/{0}";

    public Uri URI_ClientToken(string merchantID) { return new Uri(m_ApiUri, URI_MERCHANTS.Args(merchantID) + "/client_token"); }
    public Uri URI_Transactions(string merchantID) { return new Uri(m_ApiUri, URI_MERCHANTS.Args(merchantID) + "/transactions"); }
    public Uri URI_Find(string merchantID, string id)
    {
      return new Uri(m_ApiUri, URI_MERCHANTS.Args(merchantID) + "/transactions/{0}".Args(id));
    }
    public Uri URI_SubmitForSettlement(string merchantID, string id)
    {
      return new Uri(m_ApiUri, URI_MERCHANTS.Args(merchantID) + "/transactions/{0}/submit_for_settlement".Args(id));
    }
    public Uri URI_Void(string merchantID, string id)
    {
      return new Uri(m_ApiUri, URI_MERCHANTS.Args(merchantID) + "/transactions/{0}/void".Args(id));
    }
    public Uri URI_Refund(string merchantID, string id)
    {
      return new Uri(m_ApiUri, URI_MERCHANTS.Args(merchantID) + "/transactions/{0}/refund".Args(id));
    }
    public Uri URI_Customer(string merchantID, string customerID) { return new Uri(m_ApiUri, URI_MERCHANTS.Args(merchantID) + "/customers/" + customerID); }
    #endregion

    #region .ctor
    public BraintreeSystem(string name, IConfigSectionNode node) : this((Uri)null, name, node, null) { }
    public BraintreeSystem(string name, IConfigSectionNode node, object director) : this((Uri)null, name, node, director) { }
    public BraintreeSystem(string apiUri, string name, IConfigSectionNode node, object director) : this(apiUri.IsNotNullOrWhiteSpace() ? new Uri(apiUri) : null, name, node, director) {}
    public BraintreeSystem(Uri apiUri, string name, IConfigSectionNode node, object director) : base(name, node, director) { if (apiUri != null) m_ApiUri = apiUri; }
    #endregion

    #region Fields
    private BraintreeWebTerminal m_WebTerminal;
    [Config(Default = DEFAULT_API_URI)]
    private Uri m_ApiUri;
    #endregion

    #region Properties
    public override string ComponentCommonName { get { return "braintreepayments"; } }

    public override IPayWebTerminal WebTerminal
    {
      get
      {
        if (m_WebTerminal == null) m_WebTerminal = new BraintreeWebTerminal(this);
        return m_WebTerminal;
      }
    }
    #endregion

    public object GenerateClientToken(PaySession session) { return doGenerateClientToken((BraintreeSession)session); }

    protected override ConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
    { return ConnectionParameters.Make<BraintreeConnectionParameters>(paramsSection); }

    protected override PaySession DoStartSession(ConnectionParameters cParams, IPaySessionContext context = null)
    { return new BraintreeSession(this, (BraintreeConnectionParameters)cParams, context); }

    protected internal override Transaction DoTransfer(PaySession session, Account from, Account to, Amount amount, string description = null, object extraData = null)
    { throw new NotSupportedException(); }

    protected internal override Transaction DoCharge(PaySession session, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
    { return doCharge((BraintreeSession)session, from, to, amount, capture, description, extraData); }

    protected internal override bool DoVoid(PaySession session, Transaction charge, string description = null, object extraData = null)
    { return doVoid((BraintreeSession)session, charge, description, extraData); }

    protected internal override bool DoCapture(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
    { return doCapture((BraintreeSession)session, charge, amount, description, extraData); }

    protected internal override bool DoRefund(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
    { return doRefund((BraintreeSession)session, charge, amount, description, extraData); }

    #region Private
    private object doGenerateClientToken(BraintreeSession session)
    {
      var response = getResponse(session, URI_ClientToken(session.MerchantID), new XDocument(new XElement("client-token", new XElement("version", 2))));
      return response.Element("client-token").Element("value").Value;
    }

    private Transaction doCharge(BraintreeSession session, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
    {
      var fromActualData = session.FetchAccountData(from);
      var toActualData = session.FetchAccountData(to);

      try
      {
        var transaction = new XElement("transaction",
                            new XElement("type", "sale"),
                            new XElement("amount", amount.Value),
                            new XElement(fromActualData.IsWebTerminal ? "payment-method-nonce" : "payment-method-token", fromActualData.AccountID));
        if (toActualData != null)
          transaction.Add(new XElement("order-id", "{0}:{1}:{2}".Args(toActualData.Identity, toActualData.IdentityID, toActualData.AccountID)));

        if (fromActualData.IsWebTerminal)
        {
          if (fromActualData.IsNew)
            transaction.Add(new XElement("customer", new XElement("id", fromActualData.IdentityID)));
          else
            try
            {
              getResponse(session, URI_Customer(session.MerchantID, fromActualData.IdentityID.ToString()), method: HTTPRequestMethod.GET);
              transaction.Add(new XElement("customer-id", fromActualData.IdentityID));
            }
            catch
            {
              transaction.Add(new XElement("customer", new XElement("id", fromActualData.IdentityID)));
            }
        }

        if (fromActualData.IsWebTerminal)
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

        var descriptorName = buildDescriptor(toActualData.IssuerName, description);
        if (descriptorName.IsNotNullOrWhiteSpace())
        {
          var descriptor = new XElement("descriptor");
          descriptor.Add(new XElement("name", descriptorName));
          if (toActualData.IssuerUri.IsNotNullOrWhiteSpace())
            descriptor.Add(new XElement("url", toActualData.IssuerUri.TakeFirstChars(13)));
          if (toActualData.IssuerPhone.IsNotNullOrWhiteSpace())
            descriptor.Add(new XElement("phone", toActualData.IssuerPhone));
          transaction.Add(descriptor);
        }

        var options = new XElement("options", new XElement("submit-for-settlement", capture));
        if (fromActualData.IsWebTerminal)
          options.Add(new XElement("store-in-vault-on-success", true));

        transaction.Add(options);

        var response = getResponse(session, URI_Transactions(session.MerchantID), new XDocument(transaction));

        transaction = response.Root;

        var transactionID = transaction.Element("id").Value;
        var status = transaction.Element("status").Value;
        var captured = status.EqualsOrdIgnoreCase("submitted_for_settlement");
        var createdAt = transaction.Element("created-at").Value.AsDateTime();
        var creditCard = transaction.Element("credit-card");
        var token = creditCard.Element("token").Value;
        var cardBin = creditCard.Element("bin").Value;
        var cardLast4 = creditCard.Element("last-4").Value;
        var cardType = creditCard.Element("card-type").Value;
        var cardHolder = creditCard.Element("cardholder-name").Value;
        var cardExpMonth = creditCard.Element("expiration-month").Value.AsNullableInt();
        var cardExpYear = creditCard.Element("expiration-year").Value.AsNullableInt();
        var cardExpDate = cardExpMonth.HasValue && cardExpYear.HasValue ? new DateTime(cardExpYear.Value, cardExpMonth.Value, 1) : (DateTime?)null;
        var cardMaskedName = "{2}{0}..{1}".Args(cardType, cardLast4, cardExpMonth.HasValue && cardExpYear.HasValue ? "{0}/{1} ".Args(cardExpMonth, cardExpYear) : string.Empty);

        var amountAuth = transaction.Element("amount").Value.AsDecimal();

        var taId = session.GenerateTransactionID(TransactionType.Charge);
        var ta = new Transaction(taId, TransactionType.Charge, TransactionStatus.Success, from, to, Name, transactionID,
                                 createdAt, new Amount(amount.CurrencyISO, amountAuth), description: description, extraData: extraData);
        if (captured)
          ta.__Apply(Transaction.Operation.Capture(TransactionStatus.Success, createdAt, token: transactionID, description: description, amount: amountAuth, extraData: extraData));


        session.StoreAccountData(new ActualAccountData(fromActualData.Account)
        {
          IdentityID = fromActualData.IdentityID,
          AccountID = token,
          AccountType = fromActualData.AccountType,

          CardHolder = cardHolder,
          CardMaskedName = cardMaskedName,
          CardExpirationDate = cardExpDate,

          FirstName = fromActualData.FirstName,
          MiddleName = fromActualData.MiddleName,
          LastName = fromActualData.LastName,

          Phone = fromActualData.Phone,
          EMail = fromActualData.EMail,

          BillingAddress = fromActualData.BillingAddress,
          IsNew = fromActualData.IsNew,
          IsWebTerminal = false,

          IssuerID = fromActualData.IssuerID,
          IssuerName = fromActualData.IssuerName,
          IssuerPhone = fromActualData.IssuerPhone,
          IssuerEMail = fromActualData.IssuerEMail,
          IssuerUri = fromActualData.IssuerUri,

          HadSuccessfullTransactions = true,

          // TODO: useless fields
          ShippingAddress = fromActualData.ShippingAddress,
          CardVC = fromActualData.CardVC,
          RoutingNumber = fromActualData.RoutingNumber,
        });

        StatCharge(amount);
        if (capture) StatCapture(ta, amount.Value);
        return ta;
      }
      catch (Exception ex)
      {
        StatChargeError();
        if (ex is PaymentException) throw ex;
        throw new PaymentException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + GetType().Name
          + ".Charge(session='{0}', card='{1}', amount='{2}')".Args(session, from, amount), ex);
      }
    }

    private bool doVoid(BraintreeSession session,Transaction charge, string description = null, object extraData = null)
    {
      if (!charge.Processor.EqualsIgnoreCase(Name))
        throw new PaymentException(StringConsts.PAYMENT_INVALID_PAYSYSTEM_ERROR + GetType().Name
          + ".Void(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, charge.Amount));

      if (!charge.CanVoid)
        return false;

      try
      {
        var response = getResponse(session, URI_Void(session.MerchantID, charge.Token.AsString()), method: HTTPRequestMethod.PUT);

        var transaction = response.Root;
        var transactionID = transaction.Element("id").Value;
        var updatedAt = transaction.Element("updated-at").Value.AsDateTime();
        var amountVoided = transaction.Element("amount").Value.AsDecimal();

        charge.__Apply(Transaction.Operation.Void(TransactionStatus.Refunded, updatedAt, token: transactionID, description: description, extraData: extraData));

        StatVoid(charge);

        return true;
      }
      catch (Exception ex)
      {
        StatVoidError();
        if (ex is PaymentException)
          throw ex;
        throw new PaymentException(StringConsts.PAYMENT_CANNOT_VOID_ERROR + GetType().Name
          + ".Void(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, charge.Amount), ex);
      }
    }

    private bool doCapture(BraintreeSession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
    {
      if (!charge.Processor.EqualsIgnoreCase(Name))
        throw new PaymentException(StringConsts.PAYMENT_INVALID_PAYSYSTEM_ERROR + GetType().Name
          + ".Capture(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, charge.Amount));

      if (!charge.CanCapture)
        return false;

      try
      {
        var transaction = new XElement("transaction");
        if (amount.HasValue) transaction.Add(new XElement("amount", amount.Value));

        var response = getResponse(session, URI_SubmitForSettlement(session.MerchantID, charge.Token.AsString()), new XDocument(transaction), HTTPRequestMethod.PUT);

        transaction = response.Root;
        var transactionID = transaction.Element("id").Value;
        var updatedAt = transaction.Element("updated-at").Value.AsDateTime();
        var amountCaptured = transaction.Element("amount").Value.AsDecimal();

        charge.__Apply(Transaction.Operation.Capture(TransactionStatus.Success, updatedAt, token: transactionID, description: description, amount: amountCaptured, extraData: extraData));

        StatCapture(charge, amountCaptured);
        return true;
      }
      catch (Exception ex)
      {
        StatCaptureError();
        if (ex is PaymentException) throw ex;
        throw new PaymentException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + GetType().Name
          + ".Capture(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount), ex);
      }
    }

    private bool doRefund(BraintreeSession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
    {
      if (!charge.Processor.EqualsIgnoreCase(Name))
        throw new PaymentException(StringConsts.PAYMENT_INVALID_PAYSYSTEM_ERROR + GetType().Name
          + ".Refund(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, charge.Amount));

      if (!charge.CanRefund)
        return false;

      try
      {
        var useVoid = false;

        var transaction = new XElement("transaction");
        if (amount.HasValue) transaction.Add(new XElement("amount", amount.Value));
        else
        {
          var resp = getResponse(session, URI_Find(session.MerchantID, charge.Token.AsString()), method: HTTPRequestMethod.GET);
          var tran = resp.Root;
          var status = tran.Element("status").Value;
          useVoid = status.EqualsOrdIgnoreCase("submitted_for_settlement");
        }
        var response = useVoid ?
          getResponse(session, URI_Void(session.MerchantID, charge.Token.AsString()), new XDocument(transaction), HTTPRequestMethod.PUT) :
          getResponse(session, URI_Refund(session.MerchantID, charge.Token.AsString()), new XDocument(transaction), HTTPRequestMethod.POST);

        transaction = response.Root;
        var transactionID = transaction.Element("id").Value;
        var updatedAt = transaction.Element("updated-at").Value.AsDateTime();
        var amountRefunded = transaction.Element("amount").Value.AsDecimal();

        charge.__Apply(Transaction.Operation.Refund(charge.LeftToRefund.Value >= amountRefunded ? TransactionStatus.Refunded : TransactionStatus.Success, updatedAt, token: transactionID, description: description, amount: amountRefunded, extraData: extraData));

        StatRefund(charge, amountRefunded);
        return true;
      }
      catch (Exception ex)
      {
        StatRefundError();
        if (ex is PaymentException) throw ex;
        throw new PaymentException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + GetType().Name
          + ".Refund(session='{0}', charge='{1}', amount='{2}')".Args(session, charge, amount), ex);
      }
    }

    private string buildDescriptor(string issuerName, string description)
    {
      if (description.IsNullOrWhiteSpace()) return null;
      if (issuerName.IsNullOrWhiteSpace()) issuerName = "NFX";

      var sb = new StringBuilder();
      foreach (var ch in description)
      {
        if ( (ch >= '0' && ch <= '9')
          || (ch >= 'A' && ch <= 'Z')
          || (ch >= 'a' && ch <= 'z')
          || ch == ' ' || ch == '.'
          || ch == '-' || ch == '+')
          sb.Append(ch);
      }

      return issuerName.TakeFirstChars(3).PadRight(3, ' ') + "*" + sb.ToString().TakeFirstChars(18, "...");
    }

    private XDocument getResponse(BraintreeSession session, Uri uri, XDocument body = null, HTTPRequestMethod method = HTTPRequestMethod.POST)
    {
      var prms = new WebClient.RequestParams(this)
      {
        Method = method,
        AcceptType = ContentType.XML_APP,
        ContentType = ContentType.XML_APP,
        Headers = new Dictionary<string, string>()
        {
          { HDR_AUTHORIZATION, ((BraintreeCredentials)session.User.Credentials).AuthorizationHeader },
          { HDR_X_API_VERSION, API_VERSION }
        },
        Body = body != null ? new XDeclaration("1.0", "UTF-8", null).ToString() + body.ToString(SaveOptions.DisableFormatting) : null
      };

      try {
        return WebClient.GetXML(uri, prms);
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
    }
    #endregion
  }
}
