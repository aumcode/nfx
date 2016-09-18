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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Financial.Market;


namespace NFX.WinForms.Controls.ChartKit.Temporal
{

  /// <summary>
  /// Stores Candle time series data
  /// </summary>
  public class CandleTimeSeries : TimeSeries<CandleSample>
  {
      public CandleTimeSeries(string name, int order, TimeSeries parent = null)
              : base(name, order, parent)
      {
      }
  }

}
