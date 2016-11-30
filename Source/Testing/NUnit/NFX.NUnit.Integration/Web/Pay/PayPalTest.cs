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
using System.Linq;

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Web.Pay;
using NFX.Financial;
using NFX.Web.Pay.PayPal;

namespace NFX.NUnit.Integration.Web.Pay
{
  [TestFixture]
  public class PayPalTest
  {
    public const string LACONF = @"
    nfx
    {
      paypal-server-url='https://api.sandbox.paypal.com'

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
        service-point-manager
        {
          security-protocol=4032 // Tls|Tls11|Tls12 binary flag

          service-point { uri=$(/$paypal-server-url) expect-100-continue=true }

          policy
          {
            default-certificate-validation
            {
              case { uri=$(/$paypal-server-url) trusted=true}
            }
          }
        }

        payment-processing
        {
          pay-system-host
          {
            name='PayPalPrimary'
            type='NFX.NUnit.Integration.Web.Pay.FakePaySystemHost, NFX.NUnit.Integration'

            paypal-valid-account=$(~PAYPAL_SANDBOX_VALID_ACCOUNT)
          }

          pay-system
          {
            name='PayPal'
            type='NFX.Web.Pay.PayPal.PayPalSystem, NFX.Web'
            auto-start=true

            api-uri=$(/$paypal-server-url)

            payout-email-subject='Payout from NFX PayPalTest'
            payout-note='Thanks for using NFX PayPalTest'

            default-session-connect-params
            {
              name='PayPalPayoutsPrimary'
              type='NFX.Web.Pay.PayPal.PayPalConnectionParameters, NFX.Web'

              client-id=$(~PAYPAL_SANDBOX_CLIENT_ID)
              client-secret=$(~PAYPAL_SANDBOX_CLIENT_SECRET)
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
    public void GetAuthTokenTest()
    {
      var ps = PaySystem;

      using (var session = ps.StartSession())
      {
        Assert.IsNotNull(session);
        Assert.IsInstanceOf<PayPalSession>(session);
        Assert.IsNotNull(session.User);
        Assert.AreEqual(session.User.AuthenticationType, PayPalSystem.PAYPAL_REALM);
        Assert.IsNotNull(session.User.Credentials);
        Assert.IsInstanceOf<PayPalCredentials>(session.User.Credentials);

        // token occured on first api call
      }
    }

    [Test]
    public void SimplePayoutTest()
    {
      var ps = PaySystem;

      using (var session = ps.StartSession())
      {
        var to = FakePaySystemHost.PAYPAL_CORRECT_ACCOUNT;
        var amount = new Amount("USD", 1.0m);
        var transaction = session.Transfer(null, Account.EmptyInstance, to, amount);
        Assert.IsNotNull(transaction);
        Assert.AreEqual(TransactionType.Transfer, transaction.Type);
        Assert.AreEqual(amount, transaction.Amount);
        Assert.AreEqual(Account.EmptyInstance, transaction.From);
        Assert.AreEqual(to, transaction.To);

        Assert.IsNotNull(session.User.AuthToken.Data); // token occured on first call
        Assert.IsInstanceOf<PayPalOAuthToken>(session.User.AuthToken.Data);

        var token = session.User.AuthToken.Data as PayPalOAuthToken;

        Assert.IsTrue(token.ObtainTime > App.TimeSource.Now.AddMinutes(-1));
        Assert.IsTrue(token.ObtainTime < App.TimeSource.Now);
        Assert.AreEqual(3600, token.ExpirationMargin);
        Assert.IsNotNullOrEmpty(token.ApplicationID);
        Assert.IsTrue(token.ExpiresInSeconds > 0);
        Assert.IsNotNullOrEmpty(token.AccessToken);
        Assert.IsNotNullOrEmpty(token.Scope);
        Assert.IsNotNullOrEmpty(token.Nonce);
      }
    }

    [Test]
    [ExpectedException(typeof(PayPalPaymentException), ExpectedMessage = "Receiver is unregistered", MatchType = MessageMatch.Contains)]
    public void PayoutWithUngegisteredPPUserTest()
    {
      var ps = PaySystem;

      using (var session = ps.StartSession())
      {
        var to = FakePaySystemHost.PAYPAL_INCORRECT_ACCOUNT;
        var amount = new Amount("USD", 1.0m);
        session.Transfer(null, Account.EmptyInstance, to, amount);
      }
    }

    [Test]
    [ExpectedException(typeof(PayPalPaymentException))]
    public void PayoutLimitExceedPayoutTest()
    {
      var ps = PaySystem;

      using (var session = ps.StartSession())
      {
        var to = new Account("user", 211, 3000001);
        var amount = new Amount("USD", 100000.0m); // paypal payout limit is $10k
        var transaction = session.Transfer(null, Account.EmptyInstance, to, amount);
      }
    }

    private IPaySystem m_PaySystem;
    public IPaySystem PaySystem
    {
      get
      {
        if (m_PaySystem == null) { m_PaySystem = NFX.Web.Pay.PaySystem.Instances["PayPal"]; }
        return m_PaySystem;
      }
    }

  }
}
