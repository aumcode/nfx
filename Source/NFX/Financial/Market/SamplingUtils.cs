using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Financial.Market.SecDb;

namespace NFX.Financial.Market
{
  public static class SamplingUtils
  {

    /// <summary>
    /// Synthesizes a stream of candle samples from Quote and Trade samples coming from the market (i.e SecDB file)
    /// </summary>
    /// <param name="source">Source of market data</param>
    /// <param name="secSamplingPeriod">The output sampling period</param>
    /// <param name="funcQuote">Aggregation func for Quote, if null default is used which aggregates best bid</param>
    /// <param name="funcTrade">Aggregation func for Quote, if null default is used which aggregates buy and sell volumes</param>
    /// <returns>Synthesized candle stream</returns>
    public static IEnumerable<CandleSample> SynthesizeCandles(this IEnumerable<ITimeSeriesSample> source,
                                                              uint secSamplingPeriod,
                                                              Action<CandleSample, SecDBFileReader.QuoteSample, int> funcQuote = null,
                                                              Action<CandleSample, SecDBFileReader.TradeSample, int> funcTrade = null
                                                             )
    {
      if (source==null) yield break;

      if (funcQuote==null)
        funcQuote = (cs, qs, i) =>
        {
           var bestBid = qs.Bids.LastOrDefault();
           if (bestBid.Price!=0)
           {
             if (i==0) cs.OpenPrice = bestBid.Price;

             cs.HighPrice = Math.Max(cs.HighPrice, bestBid.Price);

             cs.LowPrice  = cs.LowPrice!=0f ? Math.Min( cs.LowPrice , bestBid.Price) : bestBid.Price;

             cs.ClosePrice = bestBid.Price;
           }
        };

      if (funcTrade==null)
        funcTrade = (cs, ts, i) =>
        {
          if (ts.IsQty)
          {
            if (ts.Side==SecDBFileReader.TradeSample.SideType.Buy)
              cs.BuyVolume += ts.Qty;
            else
              cs.SellVolume += ts.Qty;
          }
        };


      CandleSample emit = null;

      var filteredSamples = source.Where( s => s is SecDBFileReader.QuoteSample ||
                                               s is SecDBFileReader.TradeSample);

      var aggregateCount = 0;
      foreach(var sample in filteredSamples)
      {
        if (emit!=null && (sample.TimeStamp - emit.TimeStamp).TotalSeconds > secSamplingPeriod)
        {
          emit.TimeSpanMs = (long)(sample.TimeStamp - emit.TimeStamp).TotalMilliseconds;

          yield return emit;
          emit = null;
        }

        if (emit==null)
        {
          emit = new CandleSample(sample.TimeStamp);
          aggregateCount = 0;
        }

        var qts = sample as SecDBFileReader.QuoteSample;
        if (qts!=null)
        {
          funcQuote(emit, qts, aggregateCount);
        }

        var tds = sample as SecDBFileReader.TradeSample;
        if (tds!=null)
        {
          funcTrade(emit, tds, aggregateCount);
        }

        aggregateCount++;
      }

      if (emit!=null)
        yield return emit;
    }

    /// <summary>
    /// Aggregates source stream of the normally equidistant samples of the same type by the specified factor
    /// </summary>
    /// <param name="source">Source stream</param>
    /// <param name="times">Factor of aggergation, i.e. 4x means aggregate 4 samples into one</param>
    /// <param name="samplingRateVariationPct">
    ///  The allowed variation in timing between samples, once this variation is exceeded the system emits new aggregate
    /// </param>
    /// <returns>Aggregated sample stream</returns>
    public static IEnumerable<ITimeSeriesSample> AggregateHomogeneousSamples(this IEnumerable<ITimeSeriesSample> source, uint times, float samplingRateVariationPct = 1f)
    {
      if (source==null) yield break;

      ITimeSeriesSample prior = null;
      ITimeSeriesSample emit = null;
      float prate = 0f;
      var cnt = 0;
      foreach(var sample in source)
      {
        if (prior!=null && prate!=0)
        {
          var rate = (sample.TimeStamp - prior.TimeStamp).Ticks;
          if ( (Math.Abs(rate-prate) / Math.Max(rate, prate)) > samplingRateVariationPct)
          {
              if (emit!=null)
              {
                emit.SummarizeAggregation();
                yield return emit;
                emit = null;
              }
              prior = null;
              prate = 0;
          }
          else
           prate = rate;
        }

        if (emit==null)
        {
          emit = sample.MakeAggregateInstance();
          prior = sample;
          continue;
        }

        emit.AggregateSample(sample);
        cnt++;
        if (cnt>=times)
        {
          emit.SummarizeAggregation();
          yield return emit;
          emit = null;
        }

        prior = sample;
      }

      if (emit==null) yield break;
      emit.SummarizeAggregation();

      yield return emit;
    }

  }
}
