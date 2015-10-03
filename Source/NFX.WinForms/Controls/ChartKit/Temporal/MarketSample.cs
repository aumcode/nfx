using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NFX.WinForms.Controls.ChartKit.Temporal
{
  
  /// <summary>
  /// Represents data for a candle sample
  /// </summary>
  public class CandleSample : TimeSeries.Sample
  {
      public CandleSample(DateTime timeStamp) : base(timeStamp) { }

      public float LowPrice { get; set;}
      public float HighPrice { get; set;}

      public float OpenPrice { get; set;}  
      public float ClosePrice { get; set;}

      public int BuyVolume { get; set;}
      public int SellVolume { get; set;} 
  }


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
