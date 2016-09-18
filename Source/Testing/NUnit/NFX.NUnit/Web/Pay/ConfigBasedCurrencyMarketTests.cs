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
using NUnit.Framework;
using NFX.Web.Pay;
using NFX.Financial;
using NFX.Environment;

namespace NFX.NUnit.Web.Pay
{
  [TestFixture]
  public class ConfigBasedCurrencyMarketTests
  {
    #region Consts

      private const string CURRENCY_USD = "usd";
      private const string CURRENCY_RUB = "rub";

      private const string NO_RATE_TABLE = null;
      private const string RATE_TABLE_SAFE = "safe";

      private const int RATE_USD_TO_RUB = 70;
      private readonly Amount USD1 = new Amount(CURRENCY_USD, 1M);

    #endregion    

    #region Public

      [Test]
      [ExpectedException(typeof(PaymentException))]
      public void CurrencyMarket_NoConfig()
      {
        var market = new ConfigBasedCurrencyMarket();
        market.ConvertCurrency(NO_RATE_TABLE, USD1, CURRENCY_RUB);
      }

      [Test]
      [ExpectedException(typeof(PaymentException))]
      public void CurrencyMarket_NoCurrencyMarketSection()
      {
        var config = "payment-processing {}".AsLaconicConfig();

        var market = new ConfigBasedCurrencyMarket(config);
        Convert(config, USD1, CURRENCY_RUB);
      }

      [Test]
      [ExpectedException(typeof(PaymentException))]
      public void CurrencyMarket_NoRequiredDefaultRate()
      {
        var config = "payment-processing { usd=eur { rate=1.1 } }".AsLaconicConfig();

        Convert(config, USD1, CURRENCY_RUB);
      }

      [Test]
      [ExpectedException(typeof(PaymentException))]
      public void CurrencyMarket_NoRequiredRateInTable()
      {
        var config = @"payment-processing { 
          usd=rub { rate=70 } 
          tables {
            safe { usd=eur { rate=1.1 } } 
          }
        }".AsLaconicConfig();

        Convert(config, RATE_TABLE_SAFE, USD1, CURRENCY_RUB);
      }

      [Test]
      public void CurrencyMarket_DefaultRequiredRate()
      {
        var config = "payment-processing { usd=rub { rate=70 } }".AsLaconicConfig();

        var result = Convert(config, USD1, CURRENCY_RUB);

        Assert.AreEqual(CURRENCY_RUB, result.CurrencyISO);
        Assert.AreEqual(USD1.Value * RATE_USD_TO_RUB, result.Value);
      }

      [Test]
      public void CurrencyMarket_FallbackToDefaultRate()
      {
        var config = @"payment-processing { 
          usd=rub { rate=70 } 
          tables {
            not-safe { usd=eur { rate=1.1 } } 
          }
        }".AsLaconicConfig();

        var result = Convert(config, RATE_TABLE_SAFE, USD1, CURRENCY_RUB);

        Assert.AreEqual(CURRENCY_RUB, result.CurrencyISO);
        Assert.AreEqual(USD1.Value * RATE_USD_TO_RUB, result.Value);
      }

    #endregion

    #region .pvt

      private Amount Convert(ConfigSectionNode config, Amount amount, string currencyISO)
      {
        return Convert(config, NO_RATE_TABLE, amount, currencyISO);
      }

      private Amount Convert(ConfigSectionNode config, string rateTable, Amount amount, string currencyISO)
      {
        var market = new ConfigBasedCurrencyMarket(config);
        return market.ConvertCurrency(rateTable, amount, currencyISO);
      }

    #endregion
  }
}
