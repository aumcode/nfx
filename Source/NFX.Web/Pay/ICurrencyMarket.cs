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

using NFX.ApplicationModel;
using NFX.Instrumentation;
using NFX.Financial;
using NFX.Environment;



namespace NFX.Web.Pay
{
  /// <summary>
  /// Represents a market that can convert buy/sell currencies
  /// </summary>
  public interface ICurrencyMarket
  {
     /// <summary>
     /// Returns conversion rate for source->target conversion.
     /// rateTable is the name of the rates set, if omitted or not found then default conv rates will be used
     /// </summary>
     Amount ConvertCurrency(string rateTable, Amount from, string targetCurrencyISO);
  }

  public interface ICurrencyMarketImplementation : ICurrencyMarket, IApplicationComponent, IConfigurable
  {

  }

}
