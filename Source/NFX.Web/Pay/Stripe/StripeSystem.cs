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
using System.Threading;

using NFX.Environment;
using NFX.Financial;

namespace NFX.Web.Pay.Stripe
{
  /// <summary>
  /// Represents Stripe (see https://stripe.com) payment service.
  /// Some functions which implement IPaySystem interface don't use all supplied parameters
  /// </summary>
  public sealed class StripeSystem : PaySystem
  {
    #region Consts

      public const string BASE_URI = "https://api.stripe.com/v1";

      private const string CHARGE_URI = BASE_URI + "/charges";
      private const string RETRIEVE_URI = CHARGE_URI + "/{0}";
      private const string CAPTURE_URI = CHARGE_URI + "/{0}/capture";
      private const string REFUND_URI = CHARGE_URI + "/{0}/refund";
      private const string RECIPIENT_URI = BASE_URI + "/recipients";
      private const string RECIPIENT_DELETE_URI = RECIPIENT_URI + "/{0}";
      private const string TRANSFER_URI = BASE_URI + "/transfers";

      private const string PRM_AMOUNT = "amount";
      private const string PRM_CURRENCY = "currency";
      private const string PRM_DESCRIPTION = "description";
      private const string PRM_CARD = "card";
      private const string PRM_CARD_NUMBER = PRM_CARD + "[number]";
      private const string PRM_CARD_EXPYEAR = PRM_CARD + "[exp_year]";
      private const string PRM_CARD_EXPMONTH = PRM_CARD + "[exp_month]";
      private const string PRM_CARD_CVC = PRM_CARD + "[cvc]";
      private const string PRM_CARD_ADDRESS_LINE_1 = PRM_CARD + "[address_line1]";
      private const string PRM_CARD_ADDRESS_LINE_2 = PRM_CARD + "[address_line2]";
      private const string PRM_CARD_ADDRESS_CITY = PRM_CARD + "[address_city]";
      private const string PRM_CARD_ADDRESS_ZIP = PRM_CARD + "[address_zip]";
      private const string PRM_CARD_ADDRESS_STATE = PRM_CARD + "[address_state]";
      private const string PRM_CARD_ADDRESS_COUNTRY = PRM_CARD + "[address_country]";

      private const string PRM_CAPTURE = "capture";

      private const string PRM_REASON = "reason";

      private const string PRM_NAME = "name";
      private const string PRM_EMAIL = "email";
      private const string PRM_TYPE = "type";

      private const string PRM_RECIPIENT = "recipient";
      private const string PRM_BANK_ACCOUNT = "bank_account";
      private const string PRM_COUNTRY = PRM_BANK_ACCOUNT + "[country]";
      private const string PRM_COUNTRY_VALUE = "US";
      private const string PRM_ROUTING_NUMBER = PRM_BANK_ACCOUNT + "[routing_number]";
      private const string PRM_ACCOUNT_NUMBER = PRM_BANK_ACCOUNT + "[account_number]";

    #endregion

    public StripeSystem(string name, IConfigSectionNode node) : base(name, node) {}
    public StripeSystem(string name, IConfigSectionNode node, object director) : base(name, node, director) {}

    public override string ComponentCommonName { get { return "stripepay"; }}

    #region PaySystem implementation
      protected override ConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
      { return ConnectionParameters.Make<StripeConnectionParameters>(paramsSection); }

      protected override PaySession DoStartSession(ConnectionParameters cParams, IPaySessionContext context = null)
      { return new StripeSession(this, (StripeConnectionParameters)cParams, context); }

      /// <summary>
      /// Transfers funds from current Stripe account to user account supplied in "to" parameter
      /// ("from" parameter is not used in Stripe implementation
      /// </summary>
      protected internal override Transaction DoTransfer(PaySession session, Account from, Account to, Amount amount, string description = null, object extraData = null)
      {
        var stripeSession = (StripeSession)session;

        var recipientID = createRecipient(stripeSession, to, description);
        try
        {
          var payment = doTransfer(stripeSession, recipientID, to, amount, description, extraData);

          return payment;
        }
        finally
        {
          deleteRecipient(stripeSession, recipientID);
        }
      }

      /// <summary>
      /// Charges funds from credit card to current stripe account.
      /// Parameter "to" is unused in stripe provider.
      /// </summary>
      protected internal override Transaction DoCharge(PaySession session, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
      { return doCharge((StripeSession)session, from, to, amount, capture, description, extraData); }

      protected internal override bool DoVoid(PaySession session, Transaction charge, string description = null, object extraData = null)
      { return doVoid((StripeSession)session, charge, description, extraData); }

      /// <summary>
      /// Captures the payment of this, uncaptured, charge.
      /// This is the second half og the two-step payment flow, where first you created a charge
      /// with the capture option set to false.
      /// Uncaptured payment expires exactly seven days after it is created.
      /// If a payment is not captured by that pooint of time, it will be marked as refunded and will no longer be capturable.
      /// If set the amount of capture must be less than or equal to the original amount.
      /// </summary>
      protected internal override bool DoCapture(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
      { return doCapture((StripeSession)session, charge, amount, description, extraData); }

      /// <summary>
      /// If reason/description parameter set, possible values are duplicate, fraudulent, and requested_by_customer
      /// Developers, don't call this method directly. Call Transaction.Refund instead.
      /// </summary>
      protected internal override bool DoRefund(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
      { return doRefund((StripeSession)session, charge, amount, description, extraData); }
    #endregion

    #region Pvt. impl.
      private Transaction doCharge(StripeSession session, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
      {
        var fromActualData = session.FetchAccountData(from);

        try
        {
          var bodyPrms = new Dictionary<string, string>() {
            {PRM_AMOUNT, ((int)((amount.Value * 100))).ToString()},
            {PRM_CURRENCY, amount.CurrencyISO.ToString().ToLower()},
            {PRM_DESCRIPTION, description},
            {PRM_CAPTURE, capture.ToString()}
          };

          fillBodyParametersFromAccount(bodyPrms, fromActualData);

          var prms = new WebClient.RequestParams(this) {
            UserName = session.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(new Uri(CHARGE_URI), prms);

          var created = ((long)obj.created).FromSecondsSinceUnixEpochStart();

          var taId = session.GenerateTransactionID(TransactionType.Charge);

          var ta = new Transaction(taId, TransactionType.Charge, TransactionStatus.Success, from, to, this.Name, obj.AsGDID, created, amount, description: description, extraData: extraData);

          if (capture)
            ta.__Apply(Transaction.Operation.Capture(TransactionStatus.Success, created, token: obj.AsGDID, description: description, amount: amount.Value, extraData: extraData));

          StatCharge(amount);

          return ta;
        }
        catch (Exception ex)
        {
          StatChargeError();

          var wex = ex as System.Net.WebException;

          if (wex != null)
          {
            var response = wex.Response as System.Net.HttpWebResponse;
            if (response != null)
            {
              string errorMessage = this.GetType().Name +
                ".Charge(money: {0}, card: {1}, amount: '{2}')".Args(amount.Value, fromActualData.AccountID, amount);
              var stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + this.GetType()
            + " .Capture(session='{0}', card='{1}', amount='{2}')".Args(session, from, amount), ex);
        }
      }

      private Transaction doTransfer(StripeSession session, string recipientID, Account customerAccount, Amount amount, string description, object extraData = null)
      {
        var actualAccountData = session.FetchAccountData(customerAccount);

        try
        {
          var prms = new WebClient.RequestParams(this)
          {
            UserName = session.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = new Dictionary<string, string>()
            {
              {PRM_RECIPIENT, recipientID},
              {PRM_AMOUNT, ((int)((amount.Value * 100))).ToString()},
              {PRM_CURRENCY, amount.CurrencyISO.ToLower()},
              {PRM_DESCRIPTION, description}
            }
          };

          dynamic obj = WebClient.GetJsonAsDynamic(new Uri(TRANSFER_URI), prms);

          var created = ((long)obj.created).FromSecondsSinceUnixEpochStart();

          var taId = session.GenerateTransactionID(TransactionType.Transfer);

          var ta = new Transaction(taId, TransactionType.Transfer, TransactionStatus.Success, Account.EmptyInstance, customerAccount, this.Name, obj.id, created, amount, description: description, extraData: extraData);

          StatTransfer(amount);

          return ta;
        }
        catch (Exception ex)
        {
          StatTransferError();

          var wex = ex as System.Net.WebException;
          if (wex != null)
          {
            var response = wex.Response as System.Net.HttpWebResponse;
            if (response != null)
            {
              string errorMessage = this.GetType().Name +
                        ".doTransfer(recipientID='{0}', customerAccount='{1}', amount='{2}')".Args(recipientID, actualAccountData, amount);
              PaymentStripeException stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              if (stripeEx != null) throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_TRANSFER_ERROR + this.GetType()
            + " .doTransfer(customerAccout='{0}')".Args(actualAccountData), ex);
        }
      }

      private bool doVoid(StripeSession session, Transaction charge, string description = null, object extraData = null)
      { throw new NotImplementedException(); }

      private bool doCapture(StripeSession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
      {
        try
        {
          var bodyPrms = new Dictionary<string, string>();

          if (amount.HasValue)
          {
            bodyPrms.Add(PRM_AMOUNT, ((int)((amount.Value * 100))).ToString());
          }

          var prms = new WebClient.RequestParams(this)
          {
            Method = HTTPRequestMethod.POST,
            UserName = session.SecretKey,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(new Uri(CAPTURE_URI.Args(charge.Token)), prms);

          StatCapture(charge, amount);
          return true;
        }
        catch (Exception ex)
        {
          StatCaptureError();

          var wex = ex as System.Net.WebException;
          if (wex != null)
          {
            var response = wex.Response as System.Net.HttpWebResponse;
            if (response != null)
            {
              string errorMessage = this.GetType().Name + ".Capture(id: {0})".Args(charge.ID);
              var stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_CAPTURE_CAPTURED_PAYMENT_ERROR + this.GetType()
            + " .Capture(session='{0}', charge='{1}')".Args(session, charge), ex);
        }
      }

      private bool doRefund(StripeSession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
      {
        var fromActualData = session.FetchAccountData(charge.From);

        var refundAmount = amount ?? charge.Amount.Value;

        try
        {
          var bodyPrms = new Dictionary<string, string>() {
            {PRM_AMOUNT, ((int)((refundAmount * 100))).ToString()}
          };

          if (description.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_REASON, description);

          var prms = new WebClient.RequestParams(this)
          {
            UserName = session.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(new Uri(REFUND_URI.Args(charge.Token)), prms);

          dynamic lastRefund = ((NFX.Serialization.JSON.JSONDataArray)obj.refunds.Data).First();

          var created = ((long)obj.created).FromSecondsSinceUnixEpochStart();

          StatRefund(charge, amount);

          return true;
        }
        catch (Exception ex)
        {
          StatRefundError();

          var wex = ex as System.Net.WebException;
          if (wex != null)
          {
            var response = wex.Response as System.Net.HttpWebResponse;
            if (response != null)
            {
              string errorMessage = this.GetType().Name +
                ".Refund(money: {0}, card: {1}, description: '{2}')"
                  .Args(charge.Amount, fromActualData.AccountID, description);
              PaymentStripeException stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              if (stripeEx != null) throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_CAPTURE_CAPTURED_PAYMENT_ERROR + this.GetType()
            + " .Refund(session='{0}', charge='{1}')".Args(session, charge), ex);
        }
      }

      /// <summary>
      /// Creates new recipient in Stripe system.
      /// Is used as a temporary entity to substitute recipient parameter in Transfer operation then deleted
      /// </summary>
      private string createRecipient(StripeSession session, Account recipientAccount, string description)
      {
        var recipientActualAccountData = session.FetchAccountData(recipientAccount);

        try
        {
          var bodyPrms = new Dictionary<string, string>()
          {
            {PRM_NAME, recipientActualAccountData.AccountTitle},
            {PRM_TYPE, recipientActualAccountData.AccountType == AccountType.Corporation ? "corporation" : "individual"},
            {PRM_EMAIL, recipientActualAccountData.BillingAddress.EMail}
          };

          fillBodyParametersFromAccount(bodyPrms, recipientActualAccountData);

          var prms = new WebClient.RequestParams(this)
          {
            UserName = session.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(new Uri(RECIPIENT_URI), prms);

          return obj.id;
        }
        catch (Exception ex)
        {
          var wex = ex as System.Net.WebException;
          if (wex != null)
          {
            var response = wex.Response as System.Net.HttpWebResponse;
            if (response != null)
            {
              string errorMessage = this.GetType().Name +
                ".createRecipient(customerAccout='{0}')".Args(recipientActualAccountData);
              PaymentStripeException stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              if (stripeEx != null) throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_CREATE_RECIPIENT_ERROR + this.GetType()
            + " .Refund(customerAccout='{0}')".Args(recipientActualAccountData), ex);
        }
      }

      /// <summary>
      /// Deletes existing recipient in Stripe system.
      /// It is used to delete temporary recipient created to perform Transfer operation
      /// </summary>
      private void deleteRecipient(StripeSession stripeSession, string recipientID)
      {
        try
        {
          var prms = new WebClient.RequestParams(this)
          {
            UserName = stripeSession.SecretKey,
            Method = HTTPRequestMethod.DELETE
          };

          dynamic obj = WebClient.GetJsonAsDynamic(new Uri(RECIPIENT_DELETE_URI.Args(recipientID)), prms);

          return;
        }
        catch (Exception ex)
        {
          var wex = ex as System.Net.WebException;
          if (wex != null)
          {
            var response = wex.Response as System.Net.HttpWebResponse;
            if (response != null)
            {
              string errorMessage = this.GetType().Name +
                        ".deleteRecipient(recipientID: '{0}')".Args(recipientID);
              PaymentStripeException stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              if (stripeEx != null) throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_CREATE_RECIPIENT_ERROR + this.GetType()
            + " .deleteRecipient(recipientID: '{0}')".Args(recipientID), ex);
        }
      }

      private void fillBodyParametersFromAccount(Dictionary<string, string> bodyPrms, IActualAccountData accountData)
      {
        if (!accountData.IsCard) // bank account
        {
          bodyPrms.Add(PRM_COUNTRY, accountData.BillingAddress.Country);
          bodyPrms.Add(PRM_ACCOUNT_NUMBER, accountData.AccountID.AsString());
          bodyPrms.Add(PRM_ROUTING_NUMBER, accountData.RoutingNumber);
        }
        else
        {
          bodyPrms.Add(PRM_CARD_NUMBER, accountData.AccountID.AsString());
          bodyPrms.Add(PRM_CARD_EXPYEAR, accountData.CardExpirationDate.Value.Year.ToString("####"));
          bodyPrms.Add(PRM_CARD_EXPMONTH, accountData.CardExpirationDate.Value.Month.ToString("##"));

          if (accountData.CardVC.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_CARD_CVC, accountData.CardVC);

          if (accountData.BillingAddress.Address1.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_CARD_ADDRESS_LINE_1, accountData.BillingAddress.Address1);

          if (accountData.BillingAddress.Address2.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_CARD_ADDRESS_LINE_2, accountData.BillingAddress.Address2);

          if (accountData.BillingAddress.City.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_CARD_ADDRESS_CITY, accountData.BillingAddress.City);

          if (accountData.BillingAddress.PostalCode.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_CARD_ADDRESS_ZIP, accountData.BillingAddress.PostalCode);

          if (accountData.BillingAddress.Region.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_CARD_ADDRESS_STATE, accountData.BillingAddress.Region);

          if (accountData.BillingAddress.Country.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_CARD_ADDRESS_COUNTRY, accountData.BillingAddress.Country);
        }
      }
    #endregion

  }
}
