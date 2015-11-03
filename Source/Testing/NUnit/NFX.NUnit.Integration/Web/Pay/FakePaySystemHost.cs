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

using NFX.Web.Pay;
using NFX.Web.Pay.Mock;

namespace NFX.NUnit.Integration.Web.Pay
{
  internal class FakePaySystemHost : IPaySystemHostImplementation
  {
    #region Consts

    public readonly static Account CARD_ACCOUNT_STRIPE_CORRECT = new Account("user", 111, 1000001);
    public readonly static Account CARD_DECLINED = new Account("user", 111, 1000100);
    public readonly static Account CARD_LUHN_ERR = new Account("user", 111, 1000101);
    public readonly static Account CARD_EXP_YEAR_ERR = new Account("user", 111, 1000102);
    public readonly static Account CARD_EXP_MONTH_ERR = new Account("user", 111, 1000103);
    public readonly static Account CARD_CVC_ERR = new Account("user", 111, 1000104);

    public readonly static Account CARD_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS = new Account("user", 111, 1000002);
    public readonly static Account CARD_DEBIT_ACCOUNT_STRIPE_CORRECT = new Account("user", 111, 1000003);
    public readonly static Account CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS = new Account("user", 111, 1000004);
    public readonly static Account BANK_ACCOUNT_STRIPE_CORRECT = new Account("user", 111, 2000001);

    #endregion

    #region Static

      private static Lazy<FakePaySystemHost> s_Instance = new Lazy<FakePaySystemHost>(() => new FakePaySystemHost());

      public static FakePaySystemHost Instance { get { return s_Instance.Value; } } 

    #endregion

    #region Accounts hardcoded

    public readonly MockActualAccountData[] MockActualAccountDatas = new MockActualAccountData[] { 
        new MockActualAccountData() {
          Account = CARD_ACCOUNT_STRIPE_CORRECT,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_DECLINED,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4000000000000002",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_LUHN_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",
            AccountNumber = "4242424242424241",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_EXP_YEAR_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 1970,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_EXP_MONTH_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 13,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_CVC_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "99"
          }
        },

        new MockActualAccountData() {
          Account = CARD_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123",

            BillingAddress1= "5844 South Oak Street",
            BillingAddress2 = "1234 Lemon Street",
            BillingCountry = "US",
            BillingCity = "Chicago",
            BillingPostalCode = "60667",
            BillingRegion = "IL",
            BillingEmail = "vpupkin@mail.com",
            BillingPhone = "(309) 123-4567"
          }
        },

        new MockActualAccountData() {
          Account = BANK_ACCOUNT_STRIPE_CORRECT,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "000123456789",
            RoutingNumber = "110000000",

            BillingCountry = "US"
          }
        },

        new MockActualAccountData() {
          Account = CARD_DEBIT_ACCOUNT_STRIPE_CORRECT,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4000056655665556",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4000056655665556",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123",

            BillingAddress1= "5844 South Oak Street",
            BillingAddress2 = "1234 Lemon Street",
            BillingCountry = "US",
            BillingCity = "Chicago",
            BillingPostalCode = "60667",
            BillingRegion = "IL",
            BillingEmail = "vpupkin@mail.com",
            BillingPhone = "(309) 123-4567"
          }
        },
      };

    #endregion

    #region Pvt. fields

      private static List<Transaction> m_TransactionList = new List<Transaction>();
      private ICurrencyMarket m_Market = new ConfigBasedCurrencyMarket();

    #endregion

    #region Public

      /// <summary>
      /// Persists transaction in memory
      /// </summary>
      public void SaveTransaction(Transaction ta)
      {
        lock (m_TransactionList)
          m_TransactionList.Add(ta);
      }

    #endregion

    #region IPaySystemHostImplementation

      public object GenerateTransactionID(PaySession callerSession, ITransactionContext context, TransactionType type)
      {
        return generateUniqueID();
      }

      /// <summary>
      /// In this implementation returns transaction from memory list by id
      /// </summary>
      public Transaction FetchTransaction(ITransactionContext context, object id)
      {
        return m_TransactionList.FirstOrDefault(ta => ta.ID == id);
      }

      public IActualAccountData AccountToActualData(ITransactionContext context, Account account)
      {
        return MockActualAccountDatas.FirstOrDefault(m => m.Account == account);
      }

      public void Configure(Environment.IConfigSectionNode node)
      {
        NFX.Environment.ConfigAttribute.Apply(this, node);
      }

      public string Name { get { return this.GetType().Name; } }

      public ICurrencyMarket CurrencyMarket { get{ return m_Market;}}

    #endregion

    #region pvt./impl

      private string generateUniqueID()
      {
        ulong id = (((ulong)ExternalRandomGenerator.Instance.NextRandomInteger) << 32) + ((ulong)ExternalRandomGenerator.Instance.NextRandomInteger);
        var eLink = new ELink(id, new byte[] { });
        return eLink.Link;
      }

    #endregion

  }
}
