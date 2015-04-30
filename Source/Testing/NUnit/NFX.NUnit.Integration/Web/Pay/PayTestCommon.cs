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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Web.Pay;

namespace NFX.NUnit.Integration.Web.Pay
{
  public class PayTestCommon
  {
    #region Tests

      public static void ChargeCommon(PaySession sess)
      {
        Assert.IsNotNull(sess);

        var ta = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Financial.Amount("usd", 15.75M), true, "test payment");

        Assert.IsNotNull(ta);
      }

      public static void ChargeCardDeclined(PaySession sess)
      {
        var ta = sess.Charge(null, FakePaySystemHost.CARD_DECLINED, Account.EmptyInstance,
          new Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardLuhnErr(PaySession sess)
      {
        var ta = sess.Charge(null, FakePaySystemHost.CARD_LUHN_ERR, Account.EmptyInstance,
          new Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardExpYearErr(PaySession sess)
      {
        var ta = sess.Charge(null, FakePaySystemHost.CARD_EXP_YEAR_ERR, Account.EmptyInstance,
          new Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardExpMonthErr(PaySession sess)
      {
        var ta = sess.Charge(null, FakePaySystemHost.CARD_EXP_MONTH_ERR, Account.EmptyInstance,
            new Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardVCErr(PaySession sess)
      {
        var ta = sess.Charge(null, FakePaySystemHost.CARD_CVC_ERR, Account.EmptyInstance,
            new Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeWithBillingAddressInfo(PaySession sess)
      {
        Assert.IsNotNull(sess);

        var ta = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS, Account.EmptyInstance,
          new Financial.Amount("usd", 15.75M), true, "test payment");

        Assert.IsNotNull(ta);
      }

      public static void CaptureImplicitTotal(PaySession sess)
      {
        var charge = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT,
          new Financial.Amount("usd", 17.25M), false, "test payment");

        Assert.IsFalse(charge.IsCaptured);

        charge.Capture(null);

        Assert.IsTrue(charge.IsCaptured);
      }

      public static void CaptureExplicitTotal(PaySession sess)
      {
        var amount = new Financial.Amount("usd", 17.25M);

        var charge = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, amount, false, "test payment");

        Assert.IsFalse(charge.IsCaptured);

        charge.Capture(null, amount);

        Assert.IsTrue(charge.IsCaptured);
      }

      public static void CapturePartial(PaySession sess)
      {
        var charge = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT,
          new Financial.Amount("usd", 17.25M), false, "test payment");

        Assert.IsFalse(charge.IsCaptured);

        charge.Capture(null, amount: new Financial.Amount("usd", 10.00M));

        Assert.IsTrue(charge.IsCaptured);
      }

      public static void RefundFullImplicit(PaySession sess)
      {
        var charge = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Financial.Amount("usd", 17.25M), true, "test payment");

        Assert.IsFalse(charge.IsRefunded);

        FakePaySystemHost.Instance.SaveTransaction(charge);

        var refund = charge.Refund(null);

        Assert.IsTrue(charge.IsRefunded);
        Assert.AreEqual(charge.ID, refund.ParentTransactionID);
      }

      public static void RefundFullExplicit(PaySession sess)
      {
        var charge = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Financial.Amount("usd", 20.00M), true, "Refund Full Explicit Charge");

        FakePaySystemHost.Instance.SaveTransaction(charge);

        var refundTA = charge.Refund(null, new Financial.Amount("usd", 20.00M));

        Assert.IsNotNull(refundTA);
      }

      public static void RefundFullTwoParts(PaySession sess)
      {
        var charge = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Financial.Amount("usd", 20.00M), true, "Refund Full Explicit Charge");

        FakePaySystemHost.Instance.SaveTransaction(charge);

        var refund1 = charge.Refund(null, new Financial.Amount("usd", 15.00M), "fraudulent");

        Assert.IsNotNull(refund1);
        Assert.AreEqual(charge.ID, refund1.ParentTransactionID);

        FakePaySystemHost.Instance.SaveTransaction(refund1);

        var refund2 = charge.Refund(null, new Financial.Amount("usd", 5.00M), "requested_by_customer");

        Assert.IsNotNull(refund2);
        Assert.AreEqual(refund2.ParentTransactionID, charge.ID);
      }

      public static void RefundDifferentCurrency(PaySession sess)
      {
        var chargeTA = sess.Charge(null, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Financial.Amount("usd", 20.00M), true, "Refund Full Explicit Charge");

        var refundTA = chargeTA.Refund(null, new Financial.Amount("eur", 15.00M), "duplicate");
      }

      public static void TransferToBank(PaySession sess)
      {
        var transferTA = sess.Transfer(null, Account.EmptyInstance, FakePaySystemHost.BANK_ACCOUNT_STRIPE_CORRECT, new Financial.Amount("usd", 183.90M));

        Assert.IsNotNull(transferTA);
      }

      public static void TransferToCard(PaySession sess)
      {
          var transferTA = sess.Transfer(null, Account.EmptyInstance, FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT, 
            new Financial.Amount("usd", 27.00M));

          Assert.IsNotNull(transferTA);
      }

      public static void TransferToCardWithBillingAddressInfo(PaySession sess)
      {
        var transferTA = sess.Transfer(null, Account.EmptyInstance, FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS,
          new Financial.Amount("usd", 55.00M));

        Assert.IsNotNull(transferTA);
      }

    #endregion
  }
}
