using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Financial.Market.SecDb;

namespace NFX.Financial.Market
{
   /// <summary>
  /// Represents data for a candle sample
  /// </summary>
  public class CandleSample : TimeSeriesSampleBase
  {
      public CandleSample(DateTime timeStamp) : base(timeStamp) { }

      /// <summary>
      /// Timespan in MS that this candle covers, may not be always needed
      /// </summary>
      public long TimeSpanMs { get; set;}

      public float LowPrice { get; set;}
      public float HighPrice { get; set;}

      public float OpenPrice { get; set;}  
      public float ClosePrice { get; set;}

      public long BuyVolume { get; set;}
      public long SellVolume { get; set;}



      public override ITimeSeriesSample MakeAggregateInstance()
      {
        return new CandleSample(this.TimeStamp)
        {
           TimeSpanMs = this.TimeSpanMs,
           LowPrice = this.LowPrice,
           HighPrice = this.HighPrice,
           OpenPrice = this.OpenPrice,
           ClosePrice = this.ClosePrice,
           BuyVolume = this.BuyVolume,
           SellVolume = this.SellVolume
        };
      }

      public override void AggregateSample(ITimeSeriesSample sample)
      {
        if (sample==null) return;

        if (sample is CandleSample) this.AggregateSample( sample as CandleSample );
        else
         throw new FinancialException("{0}.AggergateSample({1}) unsupported".Args(GetType().Name, sample.GetType().Name));
      }

      public void AggregateSample(CandleSample sample)
      {
          if (sample==null) return;

          this.TimeSpanMs += sample.TimeSpanMs;

          if (sample.LowPrice<this.LowPrice) this.LowPrice = sample.LowPrice;
          if (sample.HighPrice>this.HighPrice) this.HighPrice = sample.HighPrice;

          this.BuyVolume += sample.BuyVolume;
          this.SellVolume += sample.SellVolume;

          this.ClosePrice = sample.ClosePrice;
      }

      /// <summary>
      /// Generates random market candle stream
      /// </summary>
      public static CandleSample[] GenerateRandom(int count,
                                                  DateTime startDate, 
                                                  int msInterval,
                                                  int msIntervalDeviation,

                                                  int priceDirChangeEvery,
                                                  int priceChangeAccel,

                                                  float currentMidPrice)
     {
        if (count<=0) count = 1;
        if (msInterval==0) msInterval = 1000;
        if (priceDirChangeEvery<=0) priceDirChangeEvery = 11;
        if (priceChangeAccel==0) priceChangeAccel = 8;

        var result = new CandleSample[count];

        var dt = startDate;
        var deltaT = msInterval;

        var priceVelocity = -1.0f + (2.0f * (float)ExternalRandomGenerator.Instance.NextRandomDouble);
        var priceSteps = 0;

        var price = currentMidPrice;
        for(var i=0; i<count; i++)
        {
          var sample = new CandleSample(dt);
          dt = dt.AddMilliseconds( deltaT );
          if (msIntervalDeviation!=0)
          {
           deltaT += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-msIntervalDeviation, msIntervalDeviation);
           if (deltaT==0) deltaT = msInterval;
           if (i%8==0) deltaT = msInterval;
          }


          priceSteps++;
          if (priceSteps>=ExternalRandomGenerator.Instance.NextScaledRandomInteger(priceDirChangeEvery-4, priceDirChangeEvery+4))
          {
            var accel = (float)ExternalRandomGenerator.Instance.NextScaledRandomInteger(1, priceChangeAccel);
            priceVelocity = -accel + (2.0f * accel * (float)ExternalRandomGenerator.Instance.NextRandomDouble);
            priceSteps = 0;
          }

          price += priceVelocity;

          var pSample = i>0 ? result[i-1] : null;

          sample.OpenPrice = pSample!=null ? pSample.ClosePrice : price;
          sample.ClosePrice = price + (float)ExternalRandomGenerator.Instance.NextScaledRandomDouble(-0.08f*currentMidPrice, +0.08f*currentMidPrice);
          sample.LowPrice = Math.Min(sample.OpenPrice, sample.ClosePrice) - (float)ExternalRandomGenerator.Instance.NextScaledRandomDouble(0, +0.05f*currentMidPrice);
          sample.HighPrice = Math.Max(sample.OpenPrice, sample.ClosePrice) + (float)ExternalRandomGenerator.Instance.NextScaledRandomDouble(0, +0.05f*currentMidPrice);

          result[i] = sample;
        }

        return result;

     }





  }

}
