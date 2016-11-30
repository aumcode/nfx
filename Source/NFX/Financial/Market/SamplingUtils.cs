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
                                                      Action<CandleSample, SecDBFileReader.TradeSample, int> funcTrade = null)
    {
      if (source==null) yield break;

      if (funcQuote==null)
      {
        funcQuote = (cs, qs, i) =>
        {
          var bestBid = qs.Bids.LastOrDefault();
          if (Math.Abs(bestBid.Price) < float.Epsilon) return;

          if (i==0) cs.OpenPrice = bestBid.Price;

          cs.HighPrice  = Math.Max(cs.HighPrice, bestBid.Price);
          cs.LowPrice   = Math.Abs(cs.LowPrice) > float.Epsilon ? Math.Min( cs.LowPrice, bestBid.Price) : bestBid.Price;
          cs.ClosePrice = bestBid.Price;
        };
      }

      if (funcTrade==null)
      {
        funcTrade = (cs, ts, i) =>
        {
          if (!ts.IsQty) return;

          if (ts.Side==SecDBFileReader.TradeSample.SideType.Buy)
            cs.BuyVolume += ts.Qty;
          else
            cs.SellVolume += ts.Qty;
        };
      }

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
    /// Synthesizes a stream of candle samples from Quote and Trade samples coming from
    /// a market data source (i.e SecDB file)
    /// </summary>
    /// <param name="source">Source of market data</param>
    /// <param name="secSamplingPeriod">The output sampling period</param>
    /// <param name="funcQuote">Aggregation func for Quote, if null default is used which aggregates best bid</param>
    /// <param name="funcTrade">Aggregation func for Quote, if null default is used which aggregates buy and sell volumes</param>
    /// <returns>Synthesized candle stream</returns>
    public static IEnumerable<T> SynthesizeCandles<T>
    (
      this IEnumerable<ITimeSeriesSample>         source,
      uint                                        secSamplingPeriod,
      Action<T, SecDBFileReader.QuoteSample, int> funcQuote,
      Action<T, SecDBFileReader.TradeSample, int> funcTrade
    )
      where T : ITimedSample
    {
      if (source==null) yield break;

      var emit = default(T);

      var filteredSamples = source.Where( s => s is SecDBFileReader.QuoteSample ||
                                               s is SecDBFileReader.TradeSample);

      var aggregateCount = 0;
      foreach(var sample in filteredSamples)
      {
        if (emit!=null && (sample.TimeStamp - emit.TimeStamp).TotalSeconds > secSamplingPeriod)
        {
          yield return emit;
          emit = default(T);
        }

        if (emit==null)
        {
          emit = (T)Activator.CreateInstance(typeof(T));
          emit.TimeStamp = sample.TimeStamp;
          aggregateCount = 0;
        }

        var qts = sample as SecDBFileReader.QuoteSample;
        if (qts!=null)
          funcQuote(emit, qts, aggregateCount);

        var tds = sample as SecDBFileReader.TradeSample;
        if (tds!=null)
          funcTrade(emit, tds, aggregateCount);

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
