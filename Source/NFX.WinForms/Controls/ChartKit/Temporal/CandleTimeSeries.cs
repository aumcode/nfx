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
