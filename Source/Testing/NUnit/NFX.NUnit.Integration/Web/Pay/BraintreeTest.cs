/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
    public const string LACONF = @"
    nfx
    {
      braintree-server-url='https://api.sandbox.braintreegateway.com'

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

          service-point { uri=$(/$braintree-server-url) expect-100-continue=true }

          policy
          {
            default-certificate-validation
            {
              case { uri=$(/$braintree-server-url) trusted=true}
            }
          }
        }

        payment-processing
        {
          pay-system-host
          {
            name='BraintreePrimary'
            type='NFX.NUnit.Integration.Web.Pay.FakePaySystemHost, NFX.NUnit.Integration'
          }

          pay-system
          {
            name='Braintree'
            type='NFX.Web.Pay.Braintree.BraintreeSystem, NFX.Web'
            auto-start=true

            api-uri=$(/$braintree-server-url)

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
      #warning Should be rewritten
      /*
      var ps = PaySystem;

      var acc = new Account("user", FakePaySystemHost.BRAINTREE_WEB_TERM, FakePaySystemHost.BRAINTREE_NONCE);
      Transaction tran = null;
      using (var session = ps.StartSession())
        tran = session.Charge(ctx, acc, Account.EmptyInstance, new Amount("usd", 99M), capture: false);

      var split = tran.ProcessorToken.AsString().Split(':');

      tran.Capture(ctx);

      ctx.IsNewCustomer = false;
      ctx.CustomerId = split[0];
      acc = new Account("user", acc.AccountID, split[1]);
      FakePaySystemHost.SaveAccount(acc);
      using (var session = ps.StartSession())
        tran = session.Charge(ctx, acc, Account.EmptyInstance, new Amount("usd", 1000M), capture: false);

      tran.Refund(ctx);
      */
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
}
