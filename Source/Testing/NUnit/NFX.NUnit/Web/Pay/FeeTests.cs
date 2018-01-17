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

using NFX.Web.Pay;
using NFX.Web.Pay.Mock;

namespace NFX.NUnit.Web.Pay
{
  [TestFixture]
  public class FeeTests
  {
    private const string CURRENCY_USD = "usd";
    private const string CURRENCY_EUR = "eur";
    private const string CURRENCY_RUB = "rub";

    private const string NO_CURRENCIES_LACONF = @"
      pay-system
      {
        name='Mock'
        type='NFX.Web.Pay.Mock.MockSystem, NFX.Web'
        auto-start=true
      }
    ";

    private const string LACONF = @"
      pay-system
      {
        name='Mock'
        type='NFX.Web.Pay.Mock.MockSystem, NFX.Web'
        auto-start=true
        currencies 
        { 
          /* For all transaction type */
          usd { fee-flat='0.30' fee-pct='2.8'                     }

          /* All transaction type have the same fee except refund */
          rub { fee-flat='1.5'  fee-pct='2.5'                     }
          rub { fee-flat='5'    fee-pct='10'  tran-type='Refund' }

          /* Only charge is supported */
          eur { fee-flat='0.30' fee-pct='3'   tran-type='Charge' }
        }
      }
    ";

    private const string CHARGE_ONLY_LACONF = @"
      pay-system
      {
        name='Mock'
        type='NFX.Web.Pay.Mock.MockSystem, NFX.Web'
        auto-start=true
        currencies 
        { 
          eur { fee-flat='0.30' fee-pct='3' tran-type='Charge' }
        }
      }
    ";

    [Test]
    public void Fee_NoSupportedCurrencies_ByDefault()
    {
      var ps = getPaySystem(NO_CURRENCIES_LACONF);

      var currencies = ps.SupportedCurrencies;

      Assert.IsNotNull(currencies);
      Assert.AreEqual(0, currencies.Count());
    }

    [Test]
    public void Fee_SupportedCurrencies()
    {
      var ps = getPaySystem(LACONF);

      var currencies = ps.SupportedCurrencies;

      Assert.IsNotNull(currencies);
      Assert.AreEqual(3, currencies.Count());

      Assert.Contains(CURRENCY_USD, currencies.ToArray());
      Assert.Contains(CURRENCY_EUR, currencies.ToArray());
      Assert.Contains(CURRENCY_RUB, currencies.ToArray());
    }

    #warning Should be rewritten
    /*
    [Test]
    [ExpectedException(typeof(PaymentException))]
    public void Fee_Flat_UnsupportedCurrency()
    {
      var ps = getPaySystem(NO_CURRENCIES_LACONF);
      ps.GetTransactionFee(CURRENCY_USD, TransactionType.Charge);
    }

    [Test]
    [ExpectedException(typeof(PaymentException))]
    public void Fee_Pct_UnsupportedCurrency()
    {
      var ps = getPaySystem(NO_CURRENCIES_LACONF);
      ps.GetTransactionPct(CURRENCY_USD, TransactionType.Charge);
    }

    [Test]
    public void Fee_Flat_Implicit_Lookup()
    {
      var ps = getPaySystem(LACONF);
      var flat = ps.GetTransactionFee(CURRENCY_USD, TransactionType.Charge);

      Assert.AreEqual(CURRENCY_USD, flat.CurrencyISO);
      Assert.AreEqual(0.30M, flat.Value);
    }

    [Test]
    public void Fee_Pct_Implicit_Lookup()
    {
      var ps = getPaySystem(LACONF);
      var pct = ps.GetTransactionPct(CURRENCY_USD, TransactionType.Charge);

      Assert.AreEqual(2.8 * 10000, pct);
    }

    [Test]
    public void Fee_Explicit_Lookup()
    {
      var ps = getPaySystem(LACONF);

      var type = TransactionType.Refund;
      var fee = ps.GetTransactionFee(CURRENCY_RUB, type);
      var pct = ps.GetTransactionPct(CURRENCY_RUB, type);

      Assert.AreEqual(CURRENCY_RUB, fee.CurrencyISO);
      Assert.AreEqual(5, fee.Value);
      Assert.AreEqual(10 * 10000, pct);
    }

    [Test]
    public void Fee_Implicit_Lookup()
    {
      var ps = getPaySystem(LACONF);

      var type = TransactionType.Charge;
      var fee = ps.GetTransactionFee(CURRENCY_RUB, type);
      var pct = ps.GetTransactionPct(CURRENCY_RUB, type);

      Assert.AreEqual(CURRENCY_RUB, fee.CurrencyISO);
      Assert.AreEqual(1.5, fee.Value);
      Assert.AreEqual(2.5 * 10000, pct);
    }
    */
    [Test]
    public void Fee_TranType_NoSupportedTypes_ByDefault()
    {
      var ps = getPaySystem(NO_CURRENCIES_LACONF);

      var result = ps.IsTransactionTypeSupported(TransactionType.Charge);

      Assert.IsFalse(result);
    }

    [Test]
    public void Fee_TranType_Explicit_WithCurrency()
    {
      var ps = getPaySystem(LACONF);

      var result = ps.IsTransactionTypeSupported(TransactionType.Charge, CURRENCY_EUR);

      Assert.IsTrue(result);
    }

    [Test]
    public void Fee_TranType_Implicit_WithCurrency()
    {
      var ps = getPaySystem(LACONF);

      var result = ps.IsTransactionTypeSupported(TransactionType.Charge, CURRENCY_USD);

      Assert.IsTrue(result);
    }

    [Test]
    public void Fee_TranType_Explicit_NoCurrency()
    {
      var ps = getPaySystem(LACONF);

      var result = ps.IsTransactionTypeSupported(TransactionType.Charge);

      Assert.IsTrue(result);
    }

    [Test]
    public void Fee_TranType_Implicit_NoCurrency()
    {
      var ps = getPaySystem(LACONF);

      var result = ps.IsTransactionTypeSupported(TransactionType.Transfer);

      Assert.IsTrue(result);
    }

    [Test]
    public void Fee_TranType_Unsupported_WithCurrency()
    {
      var ps = getPaySystem(CHARGE_ONLY_LACONF);

      var result = ps.IsTransactionTypeSupported(TransactionType.Transfer, CURRENCY_EUR);

      Assert.IsFalse(result);
    }

    [Test]
    public void Fee_TranType_Unsupported_NoCurrency()
    {
      var ps = getPaySystem(CHARGE_ONLY_LACONF);

      var result = ps.IsTransactionTypeSupported(TransactionType.Transfer);

      Assert.IsFalse(result);
    }

    private PaySystem getPaySystem(string laconf)
    {
      return PaySystem.Make<MockSystem>(null, laconf.AsLaconicConfig());
    }
  }
}
