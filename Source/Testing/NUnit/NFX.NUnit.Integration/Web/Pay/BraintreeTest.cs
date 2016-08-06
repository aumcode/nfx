/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2016 Aum Code LLC
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
using System.Linq;

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Web.Pay;
using NFX.Environment;
using NFX.Financial;

namespace NFX.NUnit.Integration.Web.Pay
{
  [TestFixture]
  public class BraintreeTest
  {
    // Valid nonces
    public const string NONCE = "fake-valid-nonce";                                    // A valid nonce that can be used to create a transaction
    public const string VISA_NONCE = "fake-valid-visa-nonce";                          // A nonce representing a valid Visa card request
    public const string AMEX_NONCE = "fake-valid-amex-nonce";                          // A nonce representing a valid American Express card request
    public const string MASTERCARD_NONCE = "fake-valid-mastercard-nonce";              // A nonce representing a valid Mastercard request
    public const string DISCOVER_NONCE = "fake-valid-discover-nonce";                  // A nonce representing a valid Discover card request
    public const string JCB_NONCE = "fake-valid-jcb-nonce";                            // A nonce representing a valid JCB card request
    public const string MAESTRO_NONCE = "fake-valid-maestro-nonce";                    // A nonce representing a valid Maestro card request
    public const string DINERSCLUB_NONCE = "fake-valid-dinersclub-nonce";              // A nonce representing a valid Diners Club card request
    public const string PREPAID_NONCE = "fake-valid-prepaid-nonce";                    // A nonce representing a valid prepaid card request
    public const string COMMERCIAL_NONCE = "fake-valid-commercial-nonce";              // A nonce representing a valid commercial card request
    public const string DURBIN_REGULATED_NONCE = "fake-valid-durbin-regulated-nonce";  // A nonce representing a valid Durbin regulated card request
    public const string HEALTHCARE_NONCE = "fake-valid-healthcare-nonce";              // A nonce representing a valid healthcare card request
    public const string DEBIT_NONCE = "fake-valid-debit-nonce";                        // A nonce representing a valid debit card request
    public const string PAYROLL_NONCE = "fake-valid-payroll-nonce";                    // A nonce representing a valid payroll card request

    // Valid nonces with card info
    public const string NO_INDICATORS_NONCE = "fake-valid-no-indicators-nonce";                          // A nonce representing a request for a valid card with no indicators
    public const string UNKNOWN_INDICATORS_NONCE = "fake-valid-unknown-indicators-nonce";                // A nonce representing a request for a valid card with unknown indicators
    public const string COUNTRY_OF_ISSUANCE_USA_NONCE = "fake-valid-country-of-issuance-usa-nonce";      // A nonce representing a request for a valid card issued in the USA
    public const string COUNTRY_OF_ISSUANCE_CAD_NONCE = "fake-valid-country-of-issuance-cad-nonce";      // A nonce representing a request for a valid card issued in Canada
    public const string ISSUING_BANK_NETWORK_ONLY_NONCE = "fake-valid-issuing-bank-network-only-nonce";  // A nonce representing a request for a valid card with the message 'Network Only' from the issuing bank

    // Processor rejected nonces
    public const string PROCESSOR_DECLINED_VISA_NONCE = "fake-processor-declined-visa-nonce";             // A nonce representing a request for a Visa card that was declined by the processor
    public const string PROCESSOR_DECLINED_MASTERCARD_NONCE = "fake-processor-declined-mastercard-nonce"; // A nonce representing a request for a Mastercard that was declined by the processor
    public const string PROCESSOR_DECLINED_AMEX_NONCE = "fake-processor-declined-amex-nonce";             // A nonce representing a request for a American Express card that was declined by the processor
    public const string PROCESSOR_DECLINED_DISCOVER_NONCE = "fake-processor-declined-discover-nonce";     // A nonce representing a request for a Discover card that was declined by the processor
    public const string PROCESSOR_FAILURE_JCB_NONCE = "fake-processor-failure-jcb-nonce";                 // A nonce representing a request for a JCB card that was declined by the processor

    // Gateway rejected nonces
    public const string LUHN_INVALID_NONCE = "fake-luhn-invalid-nonce";                      // A nonce representing a Luhn-invalid card
    public const string CONSUMED_NONCE = "fake-consumed-nonce";                              // A nonce that has already been consumed
    public const string GATEWAY_REJECTED_FRAUD_NONCE = "fake-gateway-rejected-fraud-nonce";  // A fraudulent nonce

    public const string LACONF = @"
    nfx
    {
      starters
      {
        starter
        {
          name='Pay Systems'
          type='NFX.Web.Pay.PaySystemStarter, NFX.Web'
          application-start-break-on-exception=true
        }
      }

      web-settings
      {
        payment-processing
        {
          pay-system-host
          {
            name='BraintreePrimary'
            type='NFX.NUnit.Integration.Web.Pay.FakeBraintreeSystemHost, NFX.NUnit.Integration'
          }

          pay-system
          {
            name='Braintree'
            type='NFX.Web.Pay.Braintree.BraintreeSystem, NFX.Web'
            auto-start=true

            api-uri='https://api.sandbox.braintreegateway.com:443'

            default-session-connect-params
            {
              name='BraintreePrimary'
              type='NFX.Web.Pay.Braintree.BraintreeConnectionParameters, NFX.Web'

              merchant-id=$(~BRAINTREE_SANDBOX_MERCHANT_ID)
              public-key=$(~BRAINTREE_SANDBOX_PUBLIC_KEY)
              private-key=$(~BRAINTREE_SANDBOX_PRIVATE_KEY)
            }
          }
        }
      }
    }";

    private ServiceBaseApplication m_App;

    [TestFixtureSetUp]
    public void SetUp()
    {
      var config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new ServiceBaseApplication(null, config);
    }

    [TestFixtureTearDown]
    public void TearDown() { DisposableObject.DisposeAndNull(ref m_App); }

    [Test]
    public void ValidNonce()
    {
      var ps = PaySystem;

      var wt = ps.WebTerminal;
      var acc = new Account(wt, NONCE);
      var ctx = new FakeBraintreeTransactionContext { IsNewCustomer = true, OrderId = "TEST" };
      Transaction tran = null;
      using (var session = ps.StartSession())
        tran = session.Charge(ctx, acc, Account.EmptyInstance, new Amount("USD", 100M), capture: false);

      var split = tran.ProcessorToken.AsString().Split(':');

      ctx.IsNewCustomer = false;
      ctx.CustomerId = split[0];
      acc = new Account("Existing", "User", split[1]);
      using (var session = ps.StartSession())
        session.Charge(ctx, acc, Account.EmptyInstance, new Amount("USD", 1000M), capture: true);
    }

    private IPaySystem m_PaySystem;
    public IPaySystem PaySystem
    {
      get
      {
        if(m_PaySystem == null) { m_PaySystem = NFX.Web.Pay.PaySystem.Instances["Braintree"]; }
        return m_PaySystem;
      }
    }
  }

  public class FakeBraintreeSystemHost : PaySystemHost
  {
    public struct AccountData
    {
      public AccountData(
        string firstName = null,
        string middleName = null,
        string lastName = null,
        string email = null)
      {
        m_FirstName = firstName ?? string.Empty;
        m_MiddleName = middleName ?? string.Empty;
        m_LastName = lastName ?? string.Empty;
        m_EMail = email ?? string.Empty;
      }

      private readonly string m_FirstName;
      private readonly string m_MiddleName;
      private readonly string m_LastName;
      private readonly string m_EMail;

      public string FirstName { get { return m_FirstName; } }
      public string MiddleName { get { return m_MiddleName; } }
      public string LastName { get { return m_LastName; } }
      public string PrimaryEMail { get { return m_EMail; } }

      public string AccountTitle { get { return string.Join(" ", (new[] { m_FirstName, m_LastName, m_MiddleName }).Where(s => s.IsNullOrWhiteSpace())); } }
    }

    public struct AddressData : IAddress
    {
      public AddressData(
        string address1 = null,
        string address2 = null,
        string city = null,
        string region = null,
        string country = null,
        string postalCode = null,
        string company = null,
        string email = null,
        string phone = null)
      {
        m_Address1 = address1 ?? string.Empty;
        m_Address2 = address2 ?? string.Empty;
        m_City = city ?? string.Empty;
        m_Region = region ?? string.Empty;
        m_Country = country ?? string.Empty;
        m_PostalCode = postalCode ?? string.Empty;
        m_EMail = email ?? string.Empty;
        m_Phone = phone ?? string.Empty;
        m_Company = company ?? string.Empty;
      }

      private readonly string m_Address1;
      private readonly string m_Address2;
      private readonly string m_City;
      private readonly string m_Region;
      private readonly string m_Country;
      private readonly string m_PostalCode;
      private readonly string m_Company;
      private readonly string m_EMail;
      private readonly string m_Phone;

      public string Address1 { get { return m_Address1; } }
      public string Address2 { get { return m_Address2; } }
      public string City { get { return m_City; } }
      public string Region { get { return m_Region; } }
      public string Country { get { return m_Country; } }
      public string PostalCode { get { return m_PostalCode; } }
      public string Company { get { return m_Company; } }
      public string EMail { get { return m_EMail; } }
      public string Phone { get { return m_Phone; } }
    }

    public struct ActualAccountData : IActualAccountData
    {
      public ActualAccountData(Account account, AccountData data, AddressData address)
      {
        m_Account = account;
        m_Data = data;
        m_Address = address;
      }

      private readonly Account m_Account;
      private readonly AccountData m_Data;
      private readonly AddressData m_Address;

      public Account Account { get { return m_Account; } }

      public string FirstName { get { return m_Data.FirstName; } }
      public string LastName { get { return m_Data.LastName; } }
      public string MiddleName { get { return m_Data.MiddleName; } }
      public string PrimaryEMail { get { return m_Data.PrimaryEMail; } }

      public IAddress BillingAddress { get { return m_Address; } }

      public string AccountNumber { get { throw new NotImplementedException(); } }
      public string AccountTitle { get { throw new NotImplementedException(); } }
      public AccountType AccountType { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
      public int CardExpirationMonth { get { throw new NotImplementedException(); } }
      public int CardExpirationYear { get { throw new NotImplementedException(); } }
      public string CardVC { get { throw new NotImplementedException(); } }
      public bool HadSuccessfullTransactions { get { throw new NotImplementedException(); } }
      public bool IsCard { get { throw new NotImplementedException(); } }
      public string IssuerEMail { get { throw new NotImplementedException(); } }
      public string IssuerID { get { throw new NotImplementedException(); } }
      public string IssuerName { get { throw new NotImplementedException(); } }
      public string IssuerPhone { get { throw new NotImplementedException(); } }
      public string RoutingNumber { get { throw new NotImplementedException(); } }
      public IAddress ShippingAddress { get { throw new NotImplementedException(); } }
    }

    public readonly AccountData[] Accounts = {
      new AccountData(
        firstName: "John",
        middleName: "James",
        lastName: "Smith",
        email: "jjs@example.com")
    };

    public readonly AddressData[] Addresses = {
      new AddressData(
        address1: "1600 Amphitheatre Parkway",
        city: "Mountain View",
        region: "CA",
        country: "USA",
        postalCode: "94043",
        company: "Google Inc.",
        phone: "+16502530000"
      )
    };

    public FakeBraintreeSystemHost(string name, IConfigSectionNode node) : base(name, node) { }

    public override ICurrencyMarket CurrencyMarket { get { throw new NotImplementedException(); } }

    public override IActualAccountData AccountToActualData(ITransactionContext context, Account account)
    {
      return new ActualAccountData(account, Accounts[0], Addresses[0]);
    }

    public override Transaction FetchTransaction(ITransactionContext context, object id)
    {
      throw new NotImplementedException();
    }

    public override object GenerateTransactionID(PaySession callerSession, ITransactionContext context, TransactionType type)
    {
      return 0;
    }
  }

  public class FakeBraintreeTransactionContext : IOrderTransactionContext
  {
    public bool IsNewCustomer { get; set; }
    public object CustomerId { get; set; }
    public object OrderId { get; set; }
    public object VendorId { get; set; }
  }
}
