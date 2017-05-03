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
using NFX.Environment;
using NFX.Web;
using NFX.Web.Pay;
using NFX.Web.Pay.Stripe;

namespace NFX.NUnit.Integration.Web.Pay
{
  [TestFixture]
  public class StripeTest : NFX.NUnit.Integration.ExternalCfg
  {
    [Test]
    public void Charge()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCommon(sess);
        }
      }
    }

    [Test, ExpectedException(typeof(PaymentStripeException), ExpectedMessage="declined", MatchType=MessageMatch.Contains)]
    public void ChargeCardDeclined()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardDeclined(sess);
        }
      }
    }

    [Test, ExpectedException(typeof(PaymentStripeException), ExpectedMessage = "card number is incorrect", MatchType = MessageMatch.Contains)]
    public void ChargeCardLuhnErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardLuhnErr(sess);
        }
      }
    }

    [Test, ExpectedException(typeof(PaymentStripeException), ExpectedMessage = "expiration year is invalid", MatchType = MessageMatch.Contains)]
    public void ChargeCardExpYearErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardExpYearErr(sess);
        }
      }
    }

    [Test, ExpectedException(typeof(PaymentStripeException), ExpectedMessage = "expiration month is invalid", MatchType = MessageMatch.Contains)]
    public void ChargeCardExpMonthErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardExpMonthErr(sess);
        }
      }
    }

    [Test, ExpectedException(typeof(PaymentStripeException), ExpectedMessage = "security code is invalid", MatchType = MessageMatch.Contains)]
    public void ChargeCardVCErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardVCErr(sess);
        }
      }
    }

    // dlatushkin 20141201:
    //   refund reason
    //   actualaccountdata: +zip
    //   charge: + other address attributes
    [Test]
    public void ChargeWithBillingAddressInfo()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeWithBillingAddressInfo(sess);
        }
      }
    }

    [Test]
    public void CaptureImplicitTotal()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.CaptureImplicitTotal(pss);
        }
      }
    }

    [Test]
    public void CaptureExplicitTotal()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.CaptureExplicitTotal(pss);
        }
      }
    }

    [Test]
    public void CapturePartial()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.CapturePartial(pss);
        }
      }
    }

    [Test]
    public void RefundFullImplicit()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.RefundFullImplicit(pss);
        }
      }
    }

    [Test]
    public void RefundFullExplicit()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.RefundFullExplicit(pss);
        }
      }
    }

    [Test]
    public void RefundFullTwoParts()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.RefundFullTwoParts(pss);
        }
      }
    }

    [Test]
    public void TransferToBank()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.TransferToBank(pss);
        }
      }
    }

    [Test]
    public void TransferToCard()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.TransferToCard(pss);
        }
      }
    }

    [Test]
    public void TransferToCardWithBillingAddressInfo()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.TransferToCardWithBillingAddressInfo(pss);
        }
      }
    }

    private PaySystem getPaySystem()
    {
      var paymentSection = LACONF.AsLaconicConfig()[WebSettings.CONFIG_WEBSETTINGS_SECTION][NFX.Web.Pay.PaySystem.CONFIG_PAYMENT_PROCESSING_SECTION];

      var stripeSection = paymentSection.Children.First(p => p.AttrByName("name").Value == "Stripe");

      var ps = PaySystem.Make<StripeSystem>(null, stripeSection);

      return ps;
    }
  }
}
