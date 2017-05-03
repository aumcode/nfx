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

        var ta = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new NFX.Financial.Amount("usd", 15.75M), true, "test payment");

        Assert.IsNotNull(ta);
      }

      public static void ChargeCardDeclined(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_DECLINED, Account.EmptyInstance,
          new NFX.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardLuhnErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_LUHN_ERR, Account.EmptyInstance,
          new NFX.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardExpYearErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_EXP_YEAR_ERR, Account.EmptyInstance,
          new NFX.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardExpMonthErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_EXP_MONTH_ERR, Account.EmptyInstance,
            new NFX.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardVCErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_CVC_ERR, Account.EmptyInstance,
            new NFX.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeWithBillingAddressInfo(PaySession sess)
      {
        Assert.IsNotNull(sess);

        var ta = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS, Account.EmptyInstance,
          new NFX.Financial.Amount("usd", 15.75M), true, "test payment");

        Assert.IsNotNull(ta);
      }

      public static void CaptureImplicitTotal(PaySession sess)
      {
        var amount = new NFX.Financial.Amount("usd", 17.25M);
        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT,
          amount, false, "test payment");

        Assert.AreEqual(new NFX.Financial.Amount("usd", .0M), charge.AmountCaptured);

        sess.Capture(charge);

        Assert.AreEqual(amount, charge.AmountCaptured);
      }

      public static void CaptureExplicitTotal(PaySession sess)
      {
        var amount = new NFX.Financial.Amount("usd", 17.25M);

        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, amount, false, "test payment");

        Assert.AreEqual(new NFX.Financial.Amount("usd", .0M), charge.AmountCaptured);

        sess.Capture(charge, amount.Value);

        Assert.AreEqual(amount, charge.AmountCaptured);
      }

      public static void CapturePartial(PaySession sess)
      {
        var chargeAmount = new NFX.Financial.Amount("usd", 17.25M);

        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT,
          chargeAmount, false, "test payment");

        Assert.AreEqual(new NFX.Financial.Amount("usd", .0M), charge.AmountCaptured);

        var captureAmount = 10.00M;
        sess.Capture(charge, amount: captureAmount);

        Assert.AreEqual(captureAmount, charge.AmountCaptured);
      }

      public static void RefundFullImplicit(PaySession sess)
      {
        var amountToRefund = new NFX.Financial.Amount("usd", 17.25M);

        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance, amountToRefund, true, "test payment");

        Assert.AreEqual(new NFX.Financial.Amount("usd", .0M), charge.AmountRefunded);

        sess.StoreTransaction(charge);

        sess.Refund(charge);

        Assert.AreEqual(amountToRefund, charge.AmountRefunded);
      }

      public static void RefundFullExplicit(PaySession sess)
      {
        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new NFX.Financial.Amount("usd", 20.00M), true, "Refund Full Explicit Charge");

        sess.Refund(charge, 20.00M);
      }

      public static void RefundFullTwoParts(PaySession sess)
      {
        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new NFX.Financial.Amount("usd", 20.00M), true, "Refund Full Explicit Charge");
        sess.Refund(charge, 15.00M, "fraudulent");
        sess.Refund(charge, 5.00M, "requested_by_customer");
      }

      public static void TransferToBank(PaySession sess)
      {
        var transferTA = sess.Transfer(Account.EmptyInstance, FakePaySystemHost.BANK_ACCOUNT_STRIPE_CORRECT, new NFX.Financial.Amount("usd", 183.90M));

        Assert.IsNotNull(transferTA);
      }

      public static void TransferToCard(PaySession sess)
      {
          var transferTA = sess.Transfer(Account.EmptyInstance, FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT,
            new NFX.Financial.Amount("usd", 27.00M));

          Assert.IsNotNull(transferTA);
      }

      public static void TransferToCardWithBillingAddressInfo(PaySession sess)
      {
        var transferTA = sess.Transfer(Account.EmptyInstance, FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS,
          new NFX.Financial.Amount("usd", 55.00M));

        Assert.IsNotNull(transferTA);
      }

    #endregion
  }
}
