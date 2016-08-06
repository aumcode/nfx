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

      public override IPayWebTerminal WebTerminal { get { throw new NotImplementedException(); } }

      protected override PaySession DoStartSession(PayConnectionParameters cParams = null)
      {
        var sessionParams = cParams ?? DefaultSessionConnectParams;
        return this.StartSession((StripeConnectionParameters)sessionParams);
      }

      public StripeSession StartSession(StripeConnectionParameters cParams)
      {
        return new StripeSession(this, cParams);
      }

      /// <summary>
      /// The method is not implemented bacause there is no such function in Stripe
      /// </summary>
      public override PaymentException VerifyPotentialTransaction(PaySession session, ITransactionContext context, bool transfer, IActualAccountData from, IActualAccountData to, Amount amount)
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Charges funds from credit card to current stripe account.
      /// Parameter "to" is unused in stripe provider.
      /// </summary>
      public override Transaction Charge(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
      {
        var stripeSession = (StripeSession)session;
        var ta = Charge(stripeSession, context, from, to, amount, capture, description, extraData);

        return ta;
      }

      /// <summary>
      /// Overload of Charge method with Stripe-typed session parameter
      /// </summary>
      public Transaction Charge(StripeSession session, ITransactionContext context, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
      {
        var fromActualData = PaySystemHost.AccountToActualData(context, from);

        try
        {
          var bodyPrms = new Dictionary<string, string>() {
            {PRM_AMOUNT, ((int)((amount.Value * 100))).ToString()},
            {PRM_CURRENCY, amount.CurrencyISO.ToString().ToLower()},
            {PRM_DESCRIPTION, description},
            {PRM_CAPTURE, capture.ToString()}
          };

          fillBodyParametersFromAccount(bodyPrms, fromActualData);

          var prms = new WebClient.RequestParams() {
            Uri = new Uri(CHARGE_URI),
            Caller = this,
            UName = session.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(prms);

          var created = ((long)obj.created).FromSecondsSinceUnixEpochStart();

          var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Charge);

          var ta = Transaction.Charge(taId, this.Name, obj.AsGDID, from, to, amount, created, description);

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
                ".Charge(money: {0}, card: {1}, amount: '{2}')".Args(amount.Value, fromActualData.AccountNumber, amount);
              var stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_CHARGE_PAYMENT_ERROR + this.GetType()
            + " .Capture(session='{0}', card='{1}', amount='{2}')".Args(session, from, amount), ex);
        }
      }

      /// <summary>
      /// Captures the payment of this, uncaptured, charge.
      /// This is the second half og the two-step payment flow, where first you created a charge
      /// with the capture option set to false.
      /// Uncaptured payment expires exactly seven days after it is created.
      /// If a payment is not captured by that pooint of time, it will be marked as refunded and will no longer be capturable.
      /// If set the amount of capture must be less than or equal to the original amount.
      /// Developers, don't call this method directly. Call Transaction.Capture instead.
      /// </summary>
      public override void Capture(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = null, string description = null, object extraData = null)
      {
        Capture((StripeSession)session, context, ref charge, amount, description, extraData);
      }

      /// <summary>
      /// Overload of Capture method with Stripe-typed session parameter
      /// </summary>
      public void Capture(StripeSession session, ITransactionContext context, ref Transaction charge, Amount? amount = null, string description = null, object extraData = null)
      {
        try
        {
          var bodyPrms = new Dictionary<string, string>();

          if (amount.HasValue)
          {
            bodyPrms.Add(PRM_AMOUNT, ((int)((amount.Value.Value * 100))).ToString());
          }

          var prms = new WebClient.RequestParams()
          {
            Uri = new Uri(CAPTURE_URI.Args(charge.ProcessorToken)),
            Caller = this,
            Method = HTTPRequestMethod.POST,
            UName = session.SecretKey,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(prms);

          StatCapture(charge, amount);
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

      /// <summary>
      /// If reason/description parameter set, possible values are duplicate, fraudulent, and requested_by_customer
      /// Developers, don't call this method directly. Call Transaction.Refund instead.
      /// </summary>
      public override Transaction Refund(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = null, string description = null, object extraData = null)
      {
        return Refund((StripeSession)session, context, ref charge, amount, description, extraData);
      }

      /// <summary>
      /// Overload of Refund method with Stripe-typed session parameter
      /// Developers, don't call this method directly. Call Transaction.Refund instead.
      /// </summary>
      public Transaction Refund(StripeSession session, ITransactionContext context, ref Transaction charge, Amount? amount = null, string description = null, object extraData = null)
      {
        var fromActualData = PaySystemHost.AccountToActualData(context, charge.From);

        var refundAmount = amount ?? charge.Amount;

        try
        {
          var bodyPrms = new Dictionary<string, string>() {
            {PRM_AMOUNT, ((int)((refundAmount.Value * 100))).ToString()}
          };

          if (description.IsNotNullOrWhiteSpace())
            bodyPrms.Add(PRM_REASON, description);

          var prms = new WebClient.RequestParams()
          {
            Uri = new Uri(REFUND_URI.Args(charge.ProcessorToken)),
            Caller = this,
            UName = session.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(prms);

          dynamic lastRefund = ((NFX.Serialization.JSON.JSONDataArray)obj.refunds.Data).First();

          var created = ((long)obj.created).FromSecondsSinceUnixEpochStart();

          var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Refund);

          var refundTA = Transaction.Refund(taId, this.Name,
            lastRefund["id"], Account.EmptyInstance, charge.From, refundAmount, created, description, relatedTransactionID: charge.ID);

          StatRefund(charge, amount);

          return refundTA;
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
                  .Args(charge.Amount, fromActualData.AccountNumber, description);
              PaymentStripeException stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              if (stripeEx != null) throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_CAPTURE_CAPTURED_PAYMENT_ERROR + this.GetType()
            + " .Refund(session='{0}', charge='{1}')".Args(session, charge), ex);
        }
      }


      /// <summary>
      /// Transfers funds from current Stripe account to user account supplied in "to" parameter
      /// ("from" parameter is not used in Stripe implementation
      /// </summary>
      public override Transaction Transfer(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, string description = null, object extraData = null)
      {
        var stripeSession = (StripeSession)session;

        var recipientID = createRecipient(stripeSession, context, to, description);
        try
        {
          var payment = transfer(stripeSession, context, recipientID, to, amount, description);

          return payment;
        }
        finally
        {
          deleteRecipient(stripeSession, recipientID);
        }
      }

      protected override PayConnectionParameters MakeDefaultSessionConnectParams(Environment.IConfigSectionNode paramsSection)
      {
        return PayConnectionParameters.Make<StripeConnectionParameters>(paramsSection);
      }

    #endregion

    #region Pvt. impl.

      /// <summary>
      /// Transfers funds to customerAccount from current stripe account
      /// (which credentials is supplied in current session)
      /// </summary>
      private Transaction transfer(StripeSession stripeSession, ITransactionContext context, string recipientID, Account customerAccount, Amount amount, string description)
      {
        var actualAccountData = PaySystemHost.AccountToActualData(context, customerAccount);

        try
        {
          var prms = new WebClient.RequestParams()
          {
            Uri = new Uri(TRANSFER_URI),
            Caller = this,
            UName = stripeSession.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = new Dictionary<string, string>()
            {
              {PRM_RECIPIENT, recipientID},
              {PRM_AMOUNT, ((int)((amount.Value * 100))).ToString()},
              {PRM_CURRENCY, amount.CurrencyISO.ToLower()},
              {PRM_DESCRIPTION, description}
            }
          };

          dynamic obj = WebClient.GetJsonAsDynamic(prms);

          var created = ((long)obj.created).FromSecondsSinceUnixEpochStart();

          var taId = PaySystemHost.GenerateTransactionID(stripeSession, context, TransactionType.Transfer);

          var ta = Transaction.Transfer(taId, this.Name, obj.id, Account.EmptyInstance, customerAccount, amount, created, description);

          StatTransfer(amount);

          return ta;
        }
        catch (Exception ex)
        {
          StatTransferError();

          var wex = ex as System.Net.WebException;
          if (wex == null)
          {
            var response = wex.Response as System.Net.HttpWebResponse;
            if (response != null)
            {
              string errorMessage = this.GetType().Name +
                        ".transfer(recipientID='{0}', customerAccount='{1}', amount='{2}')".Args(recipientID, actualAccountData, amount);
              PaymentStripeException stripeEx = PaymentStripeException.Compose(response, errorMessage, wex);
              if (stripeEx != null) throw stripeEx;
            }
          }

          throw new PaymentStripeException(StringConsts.PAYMENT_CANNOT_TRANSFER_ERROR + this.GetType()
            + " .transfer(customerAccout='{0}')".Args(actualAccountData), ex);
        }
      }

      /// <summary>
      /// Creates new recipient in Stripe system.
      /// Is used as a temporary entity to substitute recipient parameter in Transfer operation then deleted
      /// </summary>
      private string createRecipient(StripeSession stripeSession, ITransactionContext context, Account recipientAccount, string description)
      {
        var recipientActualAccountData = PaySystemHost.AccountToActualData(context, recipientAccount);

        try
        {
          var bodyPrms = new Dictionary<string, string>()
          {
            {PRM_NAME, recipientActualAccountData.AccountTitle},
            {PRM_TYPE, recipientActualAccountData.AccountType == AccountType.Corporation ? "corporation" : "individual"},
            {PRM_EMAIL, recipientActualAccountData.BillingAddress.EMail}
          };

          fillBodyParametersFromAccount(bodyPrms, recipientActualAccountData);

          var prms = new WebClient.RequestParams()
          {
            Uri = new Uri(RECIPIENT_URI),
            Caller = this,
            UName = stripeSession.SecretKey,
            Method = HTTPRequestMethod.POST,
            BodyParameters = bodyPrms
          };

          dynamic obj = WebClient.GetJsonAsDynamic(prms);

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
          var prms = new WebClient.RequestParams()
          {
            Uri = new Uri(RECIPIENT_DELETE_URI.Args(recipientID)),
            Caller = this,
            UName = stripeSession.SecretKey,
            Method = HTTPRequestMethod.DELETE
          };

          dynamic obj = WebClient.GetJsonAsDynamic(prms);

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
          bodyPrms.Add(PRM_ACCOUNT_NUMBER, accountData.AccountNumber);
          bodyPrms.Add(PRM_ROUTING_NUMBER, accountData.RoutingNumber);
        }
        else
        {
          bodyPrms.Add(PRM_CARD_NUMBER, accountData.AccountNumber);
          bodyPrms.Add(PRM_CARD_EXPYEAR, accountData.CardExpirationYear.ToString("####"));
          bodyPrms.Add(PRM_CARD_EXPMONTH, accountData.CardExpirationMonth.ToString("##"));

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
