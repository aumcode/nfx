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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Web;
using NFX.Web.Pay;
using NFX.Web.Pay.Mock;

namespace NFX.NUnit.Integration.Web.Pay
{
  [TestFixture]
  public class MockTest: NFX.NUnit.Integration.ExternalCfg
  {
    #region Tests

      //[Test]
      //public void MakePaySystem()
      //{
      //  var paymentSection = Configuration.ProviderLoadFromString(LACONF, Configuration.CONFIG_LACONIC_FORMAT)
      //    .Root.Navigate("/web-settings/payment-processing") as ConfigSectionNode;

      //  var mockSection = paymentSection.Children.First(p => p.AttrByName("name").Value == "Mock");

      //  Console.WriteLine(mockSection);

      //  var ps = PaySystem.Make<MockSystem>(null, mockSection);

      //  Console.WriteLine(ps);
      //}

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

      [Test, ExpectedException(typeof(PaymentMockException), ExpectedMessage = "declined", MatchType = MessageMatch.Contains)]
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

      [Test, ExpectedException(typeof(PaymentMockException), ExpectedMessage = "is incorrect", MatchType = MessageMatch.Contains)]
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

      [Test, ExpectedException(typeof(PaymentMockException), ExpectedMessage = "Invalid card expiration", MatchType = MessageMatch.Contains)]
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

      [Test, ExpectedException(typeof(PaymentMockException), ExpectedMessage = "Invalid card expiration", MatchType = MessageMatch.Contains)]
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

      [Test, ExpectedException(typeof(PaymentMockException), ExpectedMessage = "Invalid card CVC", MatchType = MessageMatch.Contains)]
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
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
          var ps = PaySystem.Instances["mock"];
          using (var pss = ps.StartSession())
          {
            PayTestCommon.TransferToCardWithBillingAddressInfo(pss);
          }
        }
      }


    #endregion
    #region .pvt/implementation

      private PaySystem getPaySystem()
      {
        var paymentSection = LACONF.AsLaconicConfig()[WebSettings.CONFIG_WEBSETTINGS_SECTION][NFX.Web.Pay.PaySystem.CONFIG_PAYMENT_PROCESSING_SECTION];

        var stripeSection = paymentSection.Children.First(p => p.AttrByName("name").Value == "Mock");

        var ps = PaySystem.Make<MockSystem>(null, stripeSection);

        return ps;
      }

    #endregion

  }
}
