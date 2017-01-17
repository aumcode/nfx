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

using NFX.Environment;
using NFX.Financial;
using NFX.Web.Pay;
using NFX.Web.Pay.Mock;

namespace NFX.NUnit.Integration.Web.Pay
{
  internal class FakePaySystemHost : PaySystemHost
  {
    #region Consts
    public const string BRAINTREE_WEB_TERM = "fake-web-terminal";

    // Valid nonces
    public const string BRAINTREE_NONCE = "fake-valid-nonce";                                    // A valid nonce that can be used to create a transaction
    public const string BRAINTREE_VISA_NONCE = "fake-valid-visa-nonce";                          // A nonce representing a valid Visa card request
    public const string BRAINTREE_AMEX_NONCE = "fake-valid-amex-nonce";                          // A nonce representing a valid American Express card request
    public const string BRAINTREE_MASTERCARD_NONCE = "fake-valid-mastercard-nonce";              // A nonce representing a valid Mastercard request
    public const string BRAINTREE_DISCOVER_NONCE = "fake-valid-discover-nonce";                  // A nonce representing a valid Discover card request
    public const string BRAINTREE_JCB_NONCE = "fake-valid-jcb-nonce";                            // A nonce representing a valid JCB card request
    public const string BRAINTREE_MAESTRO_NONCE = "fake-valid-maestro-nonce";                    // A nonce representing a valid Maestro card request
    public const string BRAINTREE_DINERSCLUB_NONCE = "fake-valid-dinersclub-nonce";              // A nonce representing a valid Diners Club card request
    public const string BRAINTREE_PREPAID_NONCE = "fake-valid-prepaid-nonce";                    // A nonce representing a valid prepaid card request
    public const string BRAINTREE_COMMERCIAL_NONCE = "fake-valid-commercial-nonce";              // A nonce representing a valid commercial card request
    public const string BRAINTREE_DURBIN_REGULATED_NONCE = "fake-valid-durbin-regulated-nonce";  // A nonce representing a valid Durbin regulated card request
    public const string BRAINTREE_HEALTHCARE_NONCE = "fake-valid-healthcare-nonce";              // A nonce representing a valid healthcare card request
    public const string BRAINTREE_DEBIT_NONCE = "fake-valid-debit-nonce";                        // A nonce representing a valid debit card request
    public const string BRAINTREE_PAYROLL_NONCE = "fake-valid-payroll-nonce";                    // A nonce representing a valid payroll card request

    // Valid nonces with card info
    public const string BRAINTREE_NO_INDICATORS_NONCE = "fake-valid-no-indicators-nonce";                          // A nonce representing a request for a valid card with no indicators
    public const string BRAINTREE_UNKNOWN_INDICATORS_NONCE = "fake-valid-unknown-indicators-nonce";                // A nonce representing a request for a valid card with unknown indicators
    public const string BRAINTREE_COUNTRY_OF_ISSUANCE_USA_NONCE = "fake-valid-country-of-issuance-usa-nonce";      // A nonce representing a request for a valid card issued in the USA
    public const string BRAINTREE_COUNTRY_OF_ISSUANCE_CAD_NONCE = "fake-valid-country-of-issuance-cad-nonce";      // A nonce representing a request for a valid card issued in Canada
    public const string BRAINTREE_ISSUING_BANK_NETWORK_ONLY_NONCE = "fake-valid-issuing-bank-network-only-nonce";  // A nonce representing a request for a valid card with the message 'Network Only' from the issuing bank

    // Processor rejected nonces
    public const string BRAINTREE_PROCESSOR_DECLINED_VISA_NONCE = "fake-processor-declined-visa-nonce";             // A nonce representing a request for a Visa card that was declined by the processor
    public const string BRAINTREE_PROCESSOR_DECLINED_MASTERCARD_NONCE = "fake-processor-declined-mastercard-nonce"; // A nonce representing a request for a Mastercard that was declined by the processor
    public const string BRAINTREE_PROCESSOR_DECLINED_AMEX_NONCE = "fake-processor-declined-amex-nonce";             // A nonce representing a request for a American Express card that was declined by the processor
    public const string BRAINTREE_PROCESSOR_DECLINED_DISCOVER_NONCE = "fake-processor-declined-discover-nonce";     // A nonce representing a request for a Discover card that was declined by the processor
    public const string BRAINTREE_PROCESSOR_FAILURE_JCB_NONCE = "fake-processor-failure-jcb-nonce";                 // A nonce representing a request for a JCB card that was declined by the processor

    // Gateway rejected nonces
    public const string BRAINTREE_LUHN_INVALID_NONCE = "fake-luhn-invalid-nonce";                      // A nonce representing a Luhn-invalid card
    public const string BRAINTREE_CONSUMED_NONCE = "fake-consumed-nonce";                              // A nonce that has already been consumed
    public const string BRAINTREE_GATEWAY_REJECTED_FRAUD_NONCE = "fake-gateway-rejected-fraud-nonce";  // A fraudulent nonce

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

    #region Accounts hardcoded
    private static List<Transaction> s_TransactionList = new List<Transaction>();

    private static readonly IDictionary<string, AccountData> s_MockAccountDatas = new Dictionary<string, AccountData> {
        { BRAINTREE_WEB_TERM , new AccountData {

        } },
        { BRAINTREE_NONCE, new AccountData {
          FirstName = "Martin",
          LastName = "Kaleigh",

          BillingAddress1 = "272 Keslar School Rd",
          BillingCity = "Acme",
          BillingRegion = "PA",
          BillingPostalCode = "15610-1067",
          BillingCountry = "USA"
        } }
      };

    private static List<IActualAccountData> s_MockActualAccountDatas = new List<IActualAccountData> {
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

    #region .ctor
    public FakePaySystemHost() : base(typeof(FakePaySystemHost).Name, null) { }
    public FakePaySystemHost(string name, IConfigSectionNode node) : base(name, node) { }
    public FakePaySystemHost(string name, IConfigSectionNode node, object director) : base(name, node, director) {}
    #endregion

    #region Pvt. fields
    private ICurrencyMarket m_Market = new ConfigBasedCurrencyMarket();
    private object m_NextTransactionID = null;
    #endregion

    #region Properties
    [Config]
    public string PaypalValidAccount { get; set; }
    #endregion

    #region Public
    public void SetNextTransactionID(object id) { m_NextTransactionID = id; }
    #endregion

    #region IPaySystemHostImplementation
    protected override Transaction DoFetchTransaction(PaySession session, object id)
    {
      return s_TransactionList.SingleOrDefault(ta => ta.ID == id);
    }

    protected override IActualAccountData DoFetchAccountData(PaySession session, Account account)
    {
      return s_MockActualAccountDatas.SingleOrDefault(m => m.Account == account);
    }

    public override ICurrencyMarket CurrencyMarket { get { return m_Market; } }

    protected override object DoGenerateTransactionID(PaySession session, TransactionType type)
    {
      if (m_NextTransactionID != null)
      {
        var id = m_NextTransactionID;
        m_NextTransactionID = null;
        return id;
      }
      var eLink = new ELink((((ulong)ExternalRandomGenerator.Instance.NextRandomInteger) << 32) + ((ulong)ExternalRandomGenerator.Instance.NextRandomInteger), null);
      return eLink.Link;
    }

    protected override void DoStoreTransaction(PaySession session, Transaction tran)
    {
      lock (s_TransactionList)
      {
        s_TransactionList.RemoveAll(tr => tr.ID.Equals(tran.ID));
        s_TransactionList.Add(tran);
      }
    }

    protected override void DoStoreAccountData(PaySession session, IActualAccountData accoundData)
    {
      lock (s_MockActualAccountDatas)
      {
        s_MockActualAccountDatas.RemoveAll(ad => ad.Account == accoundData.Account);
        s_MockActualAccountDatas.Add(accoundData);
      }
    }
    #endregion
  }
}
