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
using System.Linq;

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Web.Pay;
using NFX.Financial;
using NFX.Web.Pay.PayPal;
using System.Threading;

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
            name='PayPalAsync'
            type='NFX.Web.Pay.PayPal.PayPalSystem, NFX.Web'
            auto-start=true
            sync-mode=false

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

          pay-system
          {
            name='PayPalSync'
            type='NFX.Web.Pay.PayPal.PayPalSystem, NFX.Web'
            auto-start=true
            sync-mode=true

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
    public void AsyncPayoutTest()
    {
      var ps = PaySystemAsync;

      var amount = new Amount("USD", 1.0m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });

        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = PaySystemHost.PaypalValidAccount
        });

        var tran = session.Transfer(from, to, amount);
        Assert.IsNotNull(tran);
        Assert.IsNotNull(tran.ID);
        id = tran.ID;
        Assert.AreEqual(TransactionType.Transfer, tran.Type);
        Assert.AreEqual(amount, tran.Amount);
        Assert.AreEqual(from, tran.From);
        Assert.AreEqual(to, tran.To);
        Assert.AreEqual(TransactionStatus.Pending, tran.Status);

        Assert.IsNotNull(session.User.AuthToken.Data); // token occured on first call
        Assert.IsInstanceOf<PayPalOAuthToken>(session.User.AuthToken.Data);

        var token = session.User.AuthToken.Data as PayPalOAuthToken;
        Assert.IsTrue(token.ObtainTime > App.TimeSource.Now.AddMinutes(-1));
        Assert.IsTrue(token.ObtainTime < App.TimeSource.Now);
        Assert.AreEqual(3600, token.ExpirationMarginSec);
        Assert.IsNotNullOrEmpty(token.ApplicationID);
        Assert.IsTrue(token.ExpiresInSec > 0);
        Assert.IsNotNullOrEmpty(token.AccessToken);
        Assert.IsNotNullOrEmpty(token.Scope);
        Assert.IsNotNullOrEmpty(token.Nonce);
      }

      var transaction = PaySystemHost.FetchTransaction(id);

      Assert.IsNotNull(transaction);
      for (int i = 0; i < 5 && (i == 0 || !transaction.Refresh()); i++)
      {
        if (i != 0)
          Console.WriteLine("...try refresh #" + i);

        Assert.AreEqual(TransactionType.Transfer, transaction.Type);
        Assert.AreEqual(amount, transaction.Amount);
        Assert.AreEqual(from, transaction.From);
        Assert.AreEqual(to, transaction.To);
        Assert.AreEqual(TransactionStatus.Pending, transaction.Status);

        var sleep = ExternalRandomGenerator.Instance.NextScaledRandomInteger(5000, 10000);
        Console.WriteLine("Sleep {0} ms...".Args(sleep));
        Thread.Sleep(sleep);
      }

      Assert.IsNotNull(transaction);
      Assert.IsTrue(transaction.Token.AsString().Split(':').Length == 3);
      Assert.AreEqual(TransactionType.Transfer, transaction.Type);
      Assert.AreEqual(amount, transaction.Amount);
      Assert.AreEqual(from, transaction.From);
      Assert.AreEqual(to, transaction.To);
      Assert.AreEqual(TransactionStatus.Success, transaction.Status);
    }

    [Test]
    public void SyncPayoutTest()
    {
      var ps = PaySystemSync;

      var amount = new Amount("USD", 1.0m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });

        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = PaySystemHost.PaypalValidAccount
        });

        var tran = session.Transfer(from, to, amount);
        Assert.IsNotNull(tran);
        Assert.IsNotNull(tran.ID);
        Assert.IsTrue(tran.Token.AsString().Split(':').Length == 3);
        id = tran.ID;
        Assert.AreEqual(TransactionType.Transfer, tran.Type);
        Assert.AreEqual(amount, tran.Amount);
        Assert.AreEqual(from, tran.From);
        Assert.AreEqual(to, tran.To);
        Assert.AreEqual(TransactionStatus.Success, tran.Status);
      }

      var transaction = PaySystemHost.FetchTransaction(id);
      Assert.IsNotNull(transaction);
      Assert.AreEqual(TransactionType.Transfer, transaction.Type);
      Assert.AreEqual(amount, transaction.Amount);
      Assert.AreEqual(from, transaction.From);
      Assert.AreEqual(to, transaction.To);
      Assert.AreEqual(TransactionStatus.Success, transaction.Status);
    }

    [Test]
    public void SyncUnclaimedPayoutTest()
    {
      var ps = PaySystemSync;

      var amount = new Amount("USD", 1.0m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });

        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = "nfx@example.com"
        });

        var tran = session.Transfer(from, to, amount);
        Assert.IsNotNull(tran);
        Assert.IsNotNull(tran.ID);
        Assert.IsTrue(tran.Token.AsString().Split(':').Length == 3);
        id = tran.ID;
        Assert.AreEqual(TransactionType.Transfer, tran.Type);
        Assert.AreEqual(amount, tran.Amount);
        Assert.AreEqual(from, tran.From);
        Assert.AreEqual(to, tran.To);
        Assert.AreEqual(TransactionStatus.Unclaimed, tran.Status);
      }

      var transaction = PaySystemHost.FetchTransaction(id);
      Assert.IsNotNull(transaction);
      Assert.AreEqual(TransactionType.Transfer, transaction.Type);
      Assert.AreEqual(amount, transaction.Amount);
      Assert.AreEqual(from, transaction.From);
      Assert.AreEqual(to, transaction.To);
      Assert.AreEqual(TransactionStatus.Unclaimed, transaction.Status);

      var voided = transaction.Void();
      Assert.IsTrue(voided);
      Assert.AreEqual(TransactionType.Transfer, transaction.Type);
      Assert.AreEqual(amount, transaction.Amount);
      Assert.AreEqual(from, transaction.From);
      Assert.AreEqual(to, transaction.To);
      Assert.AreEqual(TransactionStatus.Refunded, transaction.Status);
    }

    [Test, ExpectedException(typeof(PaymentException), ExpectedMessage = "ITEM_INCORRECT_STATUS", MatchType = MessageMatch.Contains)]
    public void VoidSuccessedPayoutTest()
    {
      var ps = PaySystemSync;

      var amount = new Amount("USD", 1.0m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });

        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = PaySystemHost.PaypalValidAccount
        });

        var tran = session.Transfer(from, to, amount);
        Assert.IsNotNull(tran);
        Assert.IsNotNull(tran.ID);
        Assert.IsTrue(tran.Token.AsString().Split(':').Length == 3);
        id = tran.ID;
        Assert.AreEqual(TransactionType.Transfer, tran.Type);
        Assert.AreEqual(amount, tran.Amount);
        Assert.AreEqual(from, tran.From);
        Assert.AreEqual(to, tran.To);
        Assert.AreEqual(TransactionStatus.Success, tran.Status);
      }

      var transaction = PaySystemHost.FetchTransaction(id);
      Assert.IsNotNull(transaction);
      Assert.AreEqual(TransactionType.Transfer, transaction.Type);
      Assert.AreEqual(amount, transaction.Amount);
      Assert.AreEqual(from, transaction.From);
      Assert.AreEqual(to, transaction.To);
      Assert.AreEqual(TransactionStatus.Success, transaction.Status);

      transaction.Void();
    }

    [Test, ExpectedException(typeof(PaymentException), ExpectedMessage = "USER_BUSINESS_ERROR", MatchType = MessageMatch.Contains)]
    public void DuplicatePayoutTest()
    {
      var ps = PaySystemSync;

      var amount = new Amount("USD", 1.0m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });

        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = PaySystemHost.PaypalValidAccount
        });

        var tran = session.Transfer(from, to, amount);
        Assert.IsNotNull(tran);
        Assert.IsNotNull(tran.ID);
        Assert.IsTrue(tran.Token.AsString().Split(':').Length == 3);
        id = tran.ID;
        Assert.AreEqual(TransactionType.Transfer, tran.Type);
        Assert.AreEqual(amount, tran.Amount);
        Assert.AreEqual(from, tran.From);
        Assert.AreEqual(to, tran.To);
        Assert.AreEqual(TransactionStatus.Success, tran.Status);
      }

      PaySystemHost.SetNextTransactionID(id);

      using (var session = ps.StartSession())
        session.Transfer(from, to, amount);
    }


    private IPaySystem m_PaySystemSync;
    public IPaySystem PaySystemSync
    {
      get
      {
        if (m_PaySystemSync == null) { m_PaySystemSync = PaySystem.Instances["PayPalSync"]; }
        return m_PaySystemSync;
      }
    }

    private IPaySystem m_PaySystemAsync;
    public IPaySystem PaySystemAsync
    {
      get
      {
        if (m_PaySystemAsync == null) { m_PaySystemAsync = PaySystem.Instances["PayPalAsync"]; }
        return m_PaySystemAsync;
      }
    }

    internal FakePaySystemHost PaySystemHost
    {
      get
      {
        return PaySystem.PaySystemHost as FakePaySystemHost;
      }
    }
  }
}
