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

using NFX;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.Log;
using System.Threading;

namespace NFX.Web.Pay.Mock
{
  /// <summary>
  /// Represents mock payment service (can be used for testing).
  /// Mock provider is driven by card pool configured in 'accounts" section (see sample config in ExternalCfg.LACONF)
  /// </summary>
  public sealed class MockSystem : PaySystem
  {
    #region Consts

      public const string CONFIG_ACCOUNTS_SECTION = "accounts";
      public const string CONFIG_ACCOUNT_DATA_NODE = "account-data";

      //public const string CONFIG_EMAIL_ATTR = "email";

      //public const string CONFIG_CARD_NUMBER_ATTR = "number";
      //public const string CONFIG_CARD_EXPYEAR_ATTR = "exp-year";
      //public const string CONFIG_CARD_EXPMONTH_ATTR = "exp-month";
      //public const string CONFIG_CARD_CVC_ATTR = "cvc";

      //public const string CONFIG_ACCOUNT_NUMBER_ATTR = "account-number";
      //public const string CONFIG_ROUTING_NUMBER_ATTR = "routing-number";
      //public const string CONFIG_ACCOUNT_TYPE_ATTR = "account-type";

    #endregion

    #region Nested classes

      private class Accounts: IConfigurable
      {
        #region Consts

          public const string CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_SECTION = "credit-card-correct";
          public const string CONFIG_ACCOUNTS_CREDIT_CARD_DECLINED_SECTION = "credit-card-declined";

          public const string CONFIG_ACCOUNTS_CREDIT_CARD_LUHN_ERROR_SECTION = "credit-card-luhn-error";
          public const string CONFIG_ACCOUNTS_CREDIT_CARD_CVC_ERROR_SECTION = "credit-card-cvc-error";
          public const string CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_WITH_ADDR_SECTION = "credit-card-correct-with-addr";

          public const string CONFIG_ACCOUNTS_DEBIT_BANK_CORRECT_SECTION = "debit-bank-correct";
          public const string CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_SECTION = "debit-card-correct";
          public const string CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_WITH_ADDR_SECTION = "debit-card-correct-with-address";

        #endregion

        #region .pvt/fields

          private List<AccountData> m_CreditCardsCorrect;
          private List<AccountData> m_CreditCardDeclined;
          private List<AccountData> m_CreditCardLuhnError;
          private List<AccountData> m_CreditCardCvcError;
          private List<AccountData> m_CreditCardCorrectWithAddr;

          private List<AccountData> m_DebitBankCorrect;
          private List<AccountData> m_DebitCardCorrect;
          private List<AccountData> m_DebitCardCorrectWithAddr;

        #endregion

        #region Properties

          public List<AccountData> CreditCardsCorrect { get { return m_CreditCardsCorrect != null ? m_CreditCardsCorrect : m_CreditCardsCorrect = new List<AccountData>(); } }
          public List<AccountData> CreditCardDeclined { get { return m_CreditCardDeclined != null ? m_CreditCardDeclined : m_CreditCardDeclined = new List<AccountData>(); } }

          public List<AccountData> CreditCardLuhnError { get { return m_CreditCardLuhnError != null ? m_CreditCardLuhnError : m_CreditCardLuhnError = new List<AccountData>(); } }
          public List<AccountData> CreditCardCvcError { get { return m_CreditCardCvcError != null ? m_CreditCardCvcError : m_CreditCardCvcError = new List<AccountData>(); } }
          public List<AccountData> CreditCardCorrectWithAddr { get { return m_CreditCardCorrectWithAddr != null ? m_CreditCardCorrectWithAddr : m_CreditCardCorrectWithAddr = new List<AccountData>(); } }

          public List<AccountData> DebitBankCorrect { get { return m_DebitBankCorrect != null ? m_DebitBankCorrect : m_DebitBankCorrect = new List<AccountData>(); } }
          public List<AccountData> DebitCardCorrect { get { return m_DebitCardCorrect != null ? m_DebitCardCorrect : m_DebitCardCorrect = new List<AccountData>(); } }
          public List<AccountData> DebitCardCorrectWithAddr { get { return m_DebitCardCorrectWithAddr != null ? m_DebitCardCorrectWithAddr : m_DebitCardCorrectWithAddr = new List<AccountData>(); } }

        #endregion

        #region Config

          public void Configure(IConfigSectionNode node)
          {
            m_CreditCardsCorrect = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_SECTION]);
            m_CreditCardDeclined = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_DECLINED_SECTION]);

            m_CreditCardLuhnError = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_LUHN_ERROR_SECTION]);
            m_CreditCardCvcError = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_CVC_ERROR_SECTION]);
            m_CreditCardCorrectWithAddr = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_WITH_ADDR_SECTION]);

            m_DebitBankCorrect = createAccounts(node[CONFIG_ACCOUNTS_DEBIT_BANK_CORRECT_SECTION]);
            m_DebitCardCorrect = createAccounts(node[CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_SECTION]);
            m_DebitCardCorrectWithAddr = createAccounts(node[CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_WITH_ADDR_SECTION]);
          }

          private List<AccountData> createAccounts(IConfigSectionNode node)
          {
            var accounts = new List<AccountData>();

            foreach (var nAcc in node.Children.Where(ch => ch.IsSameName(CONFIG_ACCOUNT_DATA_NODE)))
            {
              var acc = FactoryUtils.MakeAndConfigure<AccountData>(nAcc, typeof(AccountData));
              accounts.Add(acc);
            }

            return accounts;
          }

        #endregion
      }

    #endregion


    #region ctor

      public MockSystem(string name, IConfigSectionNode node) : base(name, node) { }

      public MockSystem(string name, IConfigSectionNode node, object director) : base(name, node, director) { }

    #endregion

    #region .pvt/fields

      private IConfigSectionNode m_AccountsCfg;
      private Accounts m_Accounts;
      private MockWebTerminal m_WebTerminal;

    #endregion


    #region Properties

      public override string ComponentCommonName { get { return "mockpay"; }}

      [Config(CONFIG_ACCOUNTS_SECTION)]
      public IConfigSectionNode AccountsCfg
      {
        get { return m_AccountsCfg; }
        set
        {
          m_Accounts = FactoryUtils.MakeAndConfigure<Accounts>(value, typeof(Accounts));
          m_AccountsCfg = value;
        }
      }

      public override IPayWebTerminal WebTerminal
      {
        get
        {
          if (m_WebTerminal == null)
            m_WebTerminal = new MockWebTerminal(this);
          return m_WebTerminal;
        }
      }

    #endregion

    #region PaySystem implementation

      protected override PaySession DoStartSession(PayConnectionParameters cParams = null)
      {
        PayConnectionParameters sessionParams = cParams ?? DefaultSessionConnectParams;
        return this.StartSession(sessionParams as MockConnectionParameters);
      }

      public MockSession StartSession(MockConnectionParameters cParams)
      {
        return new MockSession(this, cParams);
      }

      public override PaymentException VerifyPotentialTransaction(PaySession session, ITransactionContext context, bool transfer, IActualAccountData from, IActualAccountData to, Financial.Amount amount)
      {
        throw new NotImplementedException();
      }

      public override Transaction Charge(PaySession session, ITransactionContext context, Account from, Account to, Financial.Amount amount, bool capture = true, string description = null, object extraData = null)
      {
        var fromActualData = PaySystemHost.AccountToActualData(context, from);

        if (fromActualData == null)
        {
          StatChargeError();
          throw new PaymentMockException(StringConsts.PAYMENT_UNKNOWN_ACCOUNT_ERROR.Args(from) + this.GetType().Name + ".Charge");
        }

        if (m_Accounts.CreditCardDeclined.Any(c => c.AccountNumber == fromActualData.AccountNumber))
        {
          StatChargeError();
          throw new PaymentMockException(this.GetType().Name + ".Charge: card '{0}' declined".Args(fromActualData));
        }

        if (m_Accounts.CreditCardLuhnError.Any(c => c.AccountNumber == fromActualData.AccountNumber))
        {
          StatChargeError();
          throw new PaymentMockException(this.GetType().Name + ".Charge: card number '{0}' is incorrect".Args(fromActualData));
        }


        AccountData foundAccount = null;

        foundAccount = m_Accounts.CreditCardsCorrect.FirstOrDefault(c => c.AccountNumber == fromActualData.AccountNumber);

        if (foundAccount != null)
        {
          if (foundAccount.CardExpirationYear != fromActualData.CardExpirationYear)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationYear, fromActualData.CardExpirationMonth) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardExpirationMonth != fromActualData.CardExpirationMonth)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationYear, fromActualData.CardExpirationMonth) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardVC != fromActualData.CardVC)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_CVC_ERROR + this.GetType().Name + ".Charge");
          }

          var created = DateTime.UtcNow;

          var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Charge);

          var ta = Transaction.Charge(taId, this.Name, taId, from, to, amount, created, description);

          StatCharge(amount);

          return ta;
        }

        foundAccount = m_Accounts.CreditCardCorrectWithAddr.FirstOrDefault(c => c.AccountNumber == fromActualData.AccountNumber);

        if (foundAccount != null)
        {
          if (foundAccount.CardExpirationYear != fromActualData.CardExpirationYear)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationYear, fromActualData.CardExpirationMonth) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardExpirationMonth != fromActualData.CardExpirationMonth)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationYear, fromActualData.CardExpirationMonth) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardVC != fromActualData.CardVC)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_CVC_ERROR.Args(fromActualData.CardVC) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.BillingAddress1 != fromActualData.BillingAddress.Address1 ||
              foundAccount.BillingAddress2 != fromActualData.BillingAddress.Address2 ||
              foundAccount.BillingCountry != fromActualData.BillingAddress.Country ||
              foundAccount.BillingCity != fromActualData.BillingAddress.City ||
              foundAccount.BillingPostalCode != fromActualData.BillingAddress.PostalCode ||
              foundAccount.BillingRegion != fromActualData.BillingAddress.Region ||
              foundAccount.BillingEmail != fromActualData.BillingAddress.EMail ||
              foundAccount.BillingPhone != fromActualData.BillingAddress.Phone)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_ADDR_ERROR + this.GetType().Name + ".Charge");
          }

          var created = DateTime.UtcNow;

          var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Charge);

          var ta = Transaction.Charge(taId, this.Name, taId, from, to, amount, created, description);

          StatCharge(amount);

          return ta;
        }

        throw new PaymentException(StringConsts.PAYMENT_INVALID_CARD_NUMBER_ERROR + this.GetType().Name + ".Charge");
      }

      public override void Capture(PaySession session, ITransactionContext context, ref Transaction charge, Financial.Amount? amount = null, string description = null, object extraData = null)
      {
        StatCapture(charge, amount);
      }

      public override Transaction Refund(PaySession session, ITransactionContext context, ref Transaction charge, Financial.Amount? amount = null, string description = null, object extraData = null)
      {
        var refundAmount = amount ?? charge.Amount;

        var created = DateTime.UtcNow;

        var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Refund);

        var refundTA = Transaction.Refund(taId, this.Name, taId, Account.EmptyInstance, charge.From, refundAmount, created, description, relatedTransactionID: charge.ID);

        StatRefund(charge, amount);

        return refundTA;
      }

      public override Transaction Transfer(PaySession session, ITransactionContext context, Account from, Account to, Financial.Amount amount, string description = null, object extraData = null)
      {
        var actualAccountData = PaySystemHost.AccountToActualData(context, to);

        if (actualAccountData == null)
        {
          StatTransferError();
          throw new PaymentMockException(StringConsts.PAYMENT_UNKNOWN_ACCOUNT_ERROR.Args(from) + this.GetType().Name + ".Transfer");
        }

        AccountData accountData = null;

        accountData = m_Accounts.DebitBankCorrect.FirstOrDefault(c => c.AccountNumber == actualAccountData.AccountNumber
                                                && c.CardExpirationYear == actualAccountData.CardExpirationYear
                                                && c.CardExpirationMonth == actualAccountData.CardExpirationMonth
                                                && c.CardVC == actualAccountData.CardVC);

        if (accountData != null)
        {
          var created = DateTime.Now;

          var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Transfer);

          var ta = Transaction.Transfer(taId, this.Name, taId, Account.EmptyInstance, to, amount, created, description, extraData);

          StatTransfer(amount);

          return ta;
        }

        accountData = m_Accounts.DebitCardCorrect.FirstOrDefault(c => c.AccountNumber == actualAccountData.AccountNumber
                                                && c.CardExpirationYear == actualAccountData.CardExpirationYear
                                                && c.CardExpirationMonth == actualAccountData.CardExpirationMonth
                                                && c.CardVC == actualAccountData.CardVC);

        if (accountData != null)
        {
          var created = DateTime.Now;

          var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Transfer);

          var ta = Transaction.Transfer(taId, this.Name, taId, Account.EmptyInstance, to, amount, created, description, extraData);

          StatTransfer(amount);

          return ta;
        }

        accountData = m_Accounts.DebitCardCorrectWithAddr.FirstOrDefault(c => c.AccountNumber == actualAccountData.AccountNumber
                                                && c.CardExpirationYear == actualAccountData.CardExpirationYear
                                                && c.CardExpirationMonth == actualAccountData.CardExpirationMonth
                                                && c.CardVC == actualAccountData.CardVC
                                                && c.BillingAddress1 != actualAccountData.BillingAddress.Address1
                                                && c.BillingAddress2 != actualAccountData.BillingAddress.Address2
                                                && c.BillingCountry != actualAccountData.BillingAddress.Country
                                                && c.BillingCity != actualAccountData.BillingAddress.City
                                                && c.BillingPostalCode != actualAccountData.BillingAddress.PostalCode
                                                && c.BillingRegion != actualAccountData.BillingAddress.Region
                                                && c.BillingEmail != actualAccountData.BillingAddress.EMail
                                                && c.BillingPhone != actualAccountData.BillingAddress.Phone);

        if (accountData != null)
        {
          var created = DateTime.Now;

          var taId = PaySystemHost.GenerateTransactionID(session, context, TransactionType.Transfer);

          var ta = Transaction.Transfer(taId, this.Name, taId, Account.EmptyInstance, to, amount, created, description, extraData);

          StatTransfer(amount);

          return ta;
        }

        StatTransferError();
        throw new PaymentException(StringConsts.PAYMENT_INVALID_CARD_NUMBER_ERROR + this.GetType().Name + ".Transfer");
      }

      protected override PayConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
      {
        return PayConnectionParameters.Make<MockConnectionParameters>(paramsSection);
      }

    #endregion
  }
}
