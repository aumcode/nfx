using System;
using System.Linq;

using NFX.Web;
using NFX.Web.Pay;
using NFX.Web.Pay.PayPal;
using NUnit.Framework;
using NFX.Financial;
using NFX.ApplicationModel;

namespace NFX.NUnit.Integration.Web.Pay
{
    [TestFixture]
    public class PayPalTest : ExternalCfg
    {
        [Test]
        public void GetAuthTokenTest()
        {
            var conf = LACONF.AsLaconicConfig();
            var paySystem = getPaySystem();

            using (var app = new ServiceBaseApplication(null, conf))
            using (var session = paySystem.StartSession())
            {
                Assert.IsNotNull(session);
                Assert.IsInstanceOf<PayPalSession>(session);
                Assert.IsNotNull(session.User);
                Assert.AreEqual(session.User.AuthenticationType, PayPalSystem.PAYPAL_REALM);
                Assert.IsNotNull(session.User.Credentials);
                Assert.IsInstanceOf<PayPalCredentials>(session.User.Credentials);
                Assert.IsNotNull(session.User.AuthToken.Data);
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
        public void SimplePayoutTest()
        {
            var conf = LACONF.AsLaconicConfig();
            var paySystem = getPaySystem();

            using (var app = new ServiceBaseApplication(null, conf))
            using (var session = paySystem.StartSession() as PayPalSession)
            {
                var to = new Account("user", 211, 3000001);
                var amount = new Amount("USD", 1.0m);
                var transaction = session.Transfer(null, Account.EmptyInstance, to, amount);
                Assert.IsNotNull(transaction);
                Assert.AreEqual(TransactionType.Transfer, transaction.Type);
                Assert.AreEqual(amount, transaction.Amount);
                Assert.AreEqual(Account.EmptyInstance, transaction.From);
                Assert.AreEqual(to, transaction.To);
            }
        }

        [Test]
        [ExpectedException(typeof(PayPalPaymentException))]
        public void PayoutLimitExceedPayoutTest()
        {
            var conf = LACONF.AsLaconicConfig();
            var paySystem = getPaySystem();

            using (var app = new ServiceBaseApplication(null, conf))
            using (var session = paySystem.StartSession() as PayPalSession)
            {
                var to = new Account("user", 211, 3000001);
                var amount = new Amount("USD", 100000.0m); // paypal payout limit is $10k
                var transaction = session.Transfer(null, Account.EmptyInstance, to, amount);
            }
        }

        private PaySystem getPaySystem()
        {
            var paymentSection = LACONF.AsLaconicConfig()[WebSettings.CONFIG_WEBSETTINGS_SECTION][PaySystem.CONFIG_PAYMENT_PROCESSING_SECTION];
            var ppSection = paymentSection.Children.First(p => p.AttrByName("name").Value == "PayPal");

            var ps = PaySystem.Make<PayPalSystem>(null, ppSection);

            return ps;
        }
    }
}
