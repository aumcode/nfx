using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.IO.FileSystem;

namespace NFX.Financial.Market.SecDb
{
  /// <summary>
  /// Represents SecDB file. This class is thread-safe.
  /// See: https://github.com/saleyn/secdb/wiki/Data-Format
  /// </summary>
  public sealed partial class SecDBFileReader : DisposableObject
  {
      /// <summary>
      /// Contains data from the REQUIRED/system protion of the file header
      /// </summary>
      public struct FileRequiredHeader
      {
        internal FileRequiredHeader(string shebang, IDictionary<string, string> headers)
        {
          if (shebang.IsNullOrWhiteSpace())
             throw new FinancialException(StringConsts.SECDB_FILE_HEADER_ERROR + "No shebang");

          try
          {
            this.Shebang    = shebang;
            this.Version    = headers[HDR_VERSION].AsInt(handling: ConvertErrorHandling.Throw);

            var sdt = headers[HDR_DATE].Trim();
            try
            {
                var i = sdt.IndexOf('(');
                if (i<1 || i==sdt.Length-1) throw new FinancialException("missing '(' | empty");

                this.Date = sdt.Substring(0, i).AsDateTime(DateTime.MaxValue, handling: ConvertErrorHandling.Throw);
                sdt = sdt.Substring(i+1);
                i = sdt.IndexOf(' ');
                if (i<1 || i==sdt.Length-1) throw new FinancialException("missing 'time offset' | empty");
                var of = sdt.Substring(0, i);

                if (of.Length!=5) throw new FinancialException("offset must be ±HHMM");
                var pls = of[0]=='+';
                var hh = Int16.Parse(of.Substring(1, 2));
                var mm = Int16.Parse(of.Substring(3, 2));

                var ts = new TimeSpan(hh, mm, 00);

                this.OriginLocalTimeOffset = pls ? ts : -ts;


                sdt = sdt.Substring(i+1);

                if (!sdt.EndsWith(")")) throw new FinancialException("missing ')'");
                this.OriginLocalTimeName = sdt.TrimEnd(')');
            }
            catch(Exception de)
            {
              throw new FinancialException("Invalid date: '{0}': {1}".Args(sdt, de.ToMessageWithType()));
            }

            this.Exchange   = headers[HDR_EXCHANGE].AsNonNullOrEmptyString();
            this.Symbol     = headers[HDR_SYMBOL].AsNonNullOrEmptyString();
            this.Instrument = headers[HDR_INSTRUMENT].AsNonNullOrEmptyString();
            this.SecurityId = headers[HDR_SECURITYID].AsNonNullOrEmptyString();
            this.Depth      = headers[HDR_DEPTH].AsInt(handling: ConvertErrorHandling.Throw);
            this.PriceStep  = headers[HDR_PRICESTEP].AsFloat(handling: ConvertErrorHandling.Throw);
            this.UUID       = headers[HDR_UUID].AsGUID(Guid.Empty, handling: ConvertErrorHandling.Throw);
          }
          catch(Exception error)
          {
            throw new FinancialException(StringConsts.SECDB_FILE_HEADER_ERROR + error.ToMessageWithType());
          }
        }


        /// <summary>
        /// .NIX shebang line
        /// </summary>
        public readonly string   Shebang;

        /// <summary>
        /// File format version
        /// </summary>
        public readonly int      Version;

        /// <summary>
        /// UTC date of market data
        /// </summary>
        public readonly DateTime Date;


        /// <summary>
        /// Offset of the origin local time from UTC
        /// </summary>
        public readonly TimeSpan OriginLocalTimeOffset;

        /// <summary>
        /// The name of the origin local time zone/location
        /// </summary>
        public readonly string   OriginLocalTimeName;


        /// <summary>
        /// Exchange producing market data
        /// </summary>
        public readonly string   Exchange;

        /// <summary>
        /// Company-specific security name
        /// </summary>
        public readonly string   Symbol;

        /// <summary>
        /// Exchange-specific security name
        /// </summary>
        public readonly string   Instrument;

        /// <summary>
        /// Exchange-specific security ID
        /// </summary>
        public readonly string   SecurityId;

        /// <summary>
        /// Max depth of market quote levels
        /// </summary>
        public readonly int      Depth;

        /// <summary>
        /// Instrument's minimal price step
        /// </summary>
        public readonly float    PriceStep;

        /// <summary>
        /// Company-specific unique file ID
        /// </summary>
        public readonly Guid     UUID;
      }


      /// <summary>
      /// Denotes compression types. Used in StreamMeta
      /// </summary>
      public enum CompressionType
      {
        None  = 0,
        GZip  = 1
      }


      /// <summary>
      /// Denotes stream types (id). Used in StreamMeta
      /// </summary>
      public enum StreamID
      {
        Seconds = 0,
        Quotes  = 1,
        Trade   = 2,
        Order   = 3,
        Summary = 4,
        Info    = 5,
        MAX_ID = Info
      }


      public struct StreamMeta
      {
        internal StreamMeta(StreamID id) { ID = id;}
        public readonly StreamID ID;
      }

      public struct CandlesMeta
      {
        internal CandlesMeta(SecDBFileReader file, CandleHeader[] candles) { File = file; Candles = candles;}

        public readonly SecDBFileReader File;
        public readonly CandleHeader[] Candles;

        /// <summary>
        /// Returns resolution available in the file
        /// </summary>
        public IEnumerable<ushort> Resolutions
        {
          get { return Candles.Select( ch => ch.ResolutionSec );}
        }

      }

      public struct CandleHeader
      {
        internal CandleHeader(SecDBFileReader file,
                              ushort resolutionSec,
                              uint startTimeOffsetSec,
                              DateTime startTimeDt,
                              uint count,
                              uint offset)
        {
          File = file;
          ResolutionSec = resolutionSec;
          StartTimeMidnightOffsetSec = startTimeOffsetSec;
          StartTime = startTimeDt;
          CandleCount = count;
          DataOffset = offset;
        }

        /// <summary>
        /// File that the header is from
        /// </summary>
        public readonly SecDBFileReader File;

        /// <summary>
        /// True to indicate that struct instance is assigned meaningful data
        /// </summary>
        public bool IsAssigned{ get{ return File!=null;} }


        /// <summary>
        /// Size of candle in seconds
        /// </summary>
        public readonly ushort ResolutionSec;


        /// <summary>
        /// Offset since midnight in seconds
        /// </summary>
        public readonly uint StartTimeMidnightOffsetSec;

        /// <summary>
        /// Start time as calculated from StartTimeMidnightOffsetSec
        /// </summary>
        public readonly DateTime StartTime;


        /// <summary>
        /// Total candle samples at DataOffset
        /// </summary>
        public readonly uint CandleCount;

        /// <summary>
        /// Where candle data starts
        /// </summary>
        public readonly uint DataOffset;

        /// <summary>
        /// Returns how many seconds need to be skipped to arrive at the particular date.
        /// The negative return means that the supplied data is BEFORE the data start
        /// </summary>
        public int GetSkipSecondsUntil(DateTime utcDate)
        {
          if (utcDate.Kind!= DateTimeKind.Utc)
            throw new FinancialException(StringConsts.ARGUMENT_ERROR+"GetSkipSecondsUntil(utcDate is not UTC)");

          return (int)Math.Truncate((utcDate - StartTime).TotalSeconds);
        }

      }


      /// <summary>
      /// Represents OHLC sample from the file
      /// </summary>
      public struct CandleData
      {
        public const int BYTE_SIZE = (6 * 4) + 8;

        internal CandleData(SecDBFileReader file, int sampleNumber, DateTime timestamp, Stream stream)
        {
          File = file;

          SampleNumber = sampleNumber;
          TimeStamp = timestamp;

          OpenSteps  = SecDBPrimitives.ReadInt32( stream );
          HighSteps  = SecDBPrimitives.ReadInt32( stream );
          LowSteps   = SecDBPrimitives.ReadInt32( stream );
          CloseSteps = SecDBPrimitives.ReadInt32( stream );

          BuyVolume  = SecDBPrimitives.ReadInt32( stream );
          SellVolume = SecDBPrimitives.ReadInt32( stream );

          FirstStreamOffset = SecDBPrimitives.ReadUInt64( stream );
        }

        /// <summary>
        /// File that the header is from
        /// </summary>
        public readonly SecDBFileReader File;

        /// <summary>
        /// True to indicate that struct instance is assigned meaningful data
        /// </summary>
        public bool IsAssigned{ get{ return File!=null;} }


        public readonly int SampleNumber;
        public readonly DateTime TimeStamp;

        public readonly int OpenSteps;
        public readonly int HighSteps;
        public readonly int LowSteps;
        public readonly int CloseSteps;

        public float Open { get{ return OpenSteps  * File.SystemHeader.PriceStep;} }
        public float High { get{ return HighSteps  * File.SystemHeader.PriceStep;} }
        public float Low  { get{ return LowSteps   * File.SystemHeader.PriceStep;} }
        public float Close{ get{ return CloseSteps * File.SystemHeader.PriceStep;} }

        public readonly int BuyVolume;
        public readonly int SellVolume;

        public readonly ulong FirstStreamOffset;


        internal CandleSample ToCandleSample()
        {
          return new CandleSample(TimeStamp)
          {
             OpenPrice = this.Open,
             ClosePrice = this.Close,
             HighPrice = this.High,
             LowPrice = this.Low,
             BuyVolume = this.BuyVolume,
             SellVolume = this.SellVolume,
             //TimeSpanMs = Is not set here
          };
        }

      }

      /// <summary>
      /// Base class for all samples
      /// </summary>
      public abstract class StreamSample : TimeSeriesSampleBase
      {
        protected StreamSample(DateTime ts) : base(ts)
        {
        }
      }

      public sealed class SecondSample : StreamSample
      {
        internal SecondSample(DateTime ts) : base(ts) {}
      }


      public class QuoteSample : StreamSample
      {
            public struct PxLevel
            {
              public long PriceStep;
              public float Price;
              public long Quantity;
            }


        internal QuoteSample(SecDBFileReader file, QuoteSample ps, DateTime ts, Stream stream) : base(ts)
        {
          var bt = SecDBPrimitives.ReadByte(stream);

          BidCount =  bt & 0xf;
          AskCount =  (bt & 0xf0) >> 4;
          var cnt = BidCount + AskCount;

          if (cnt==0)
            throw new FinancialException(StringConsts.SECDB_FILE_HEADER_ERROR + "QuoteSample: no px levels");

          PriceLevels = new PxLevel[ cnt ];

          var pxStep = file.SystemHeader.PriceStep;

          var currentPrice = 0L;
          for(var i=0; i<cnt; i++)
          {
            var price = SecDBPrimitives.ReadSLEB128(stream);
            if (i==0)
            {
              if (ps==null)
               currentPrice = price;
              else
              {
                currentPrice = ps.PriceLevels[0].PriceStep;
                currentPrice += price;
              }
            }
            else
             currentPrice += price;

            var qty = SecDBPrimitives.ReadSLEB128(stream);

            var pl = new PxLevel{ PriceStep = currentPrice, Price = currentPrice * pxStep, Quantity = qty };
            PriceLevels[i] = pl;
          }
        }

        public readonly int BidCount;
        public readonly int AskCount;

        public readonly PxLevel[] PriceLevels;

        public IEnumerable<PxLevel> Bids{ get{ return PriceLevels.Take(BidCount);}}
        public IEnumerable<PxLevel> Asks{ get{ return PriceLevels.Skip(BidCount);}}

      }

      public class TradeSample : StreamSample
      {

        public enum AggressorType{Undef=0, Agressor=1, Passive=2}
        public enum SideType{Buy=0, Sell=1}

        internal TradeSample(SecDBFileReader file, TradeSample ps, DateTime ts, Stream stream) : base(ts)
        {
           var flags = SecDBPrimitives.ReadByte(stream);

           InternalTrade =    (flags & (1)) !=0;
           Aggressor = (AggressorType)((flags >> 1)&0x3);
           Side = (SideType)((flags >> 3)&0x1);

           IsQty =      (flags & (1 << 4)) !=0;
           IsTradeID =  (flags & (1 << 5)) !=0;
           IsOrderID =  (flags & (1 << 6)) !=0;

           var price = SecDBPrimitives.ReadSLEB128(stream);
           if (ps==null)
              PriceStep = price;
            else
              PriceStep = ps.PriceStep + price;

           Price = PriceStep * file.SystemHeader.PriceStep;

           if (IsQty)      Qty  = SecDBPrimitives.ReadSLEB128(stream);
           if (IsTradeID)  TradeID = SecDBPrimitives.ReadULEB128(stream);
           if (IsOrderID)  OrderID = SecDBPrimitives.ReadULEB128(stream);
        }


        public readonly bool InternalTrade;
        public readonly AggressorType Aggressor;
        public readonly SideType Side;


        public long PriceStep;
        public float Price;

        public readonly bool IsQty;
        public readonly bool IsTradeID;
        public readonly bool IsOrderID;

        public readonly long   Qty;
        public readonly ulong  TradeID;
        public readonly ulong   OrderID;

      }




      public class OrderSample : StreamSample
      {
        public enum SideType{Buy=0, Sell=1}

        internal OrderSample(SecDBFileReader file, OrderSample ps, DateTime ts, Stream stream) : base(ts)
        {
           var flags = SecDBPrimitives.ReadByte(stream);

           InternalOrder =    (flags & (1)) !=0;
           CancelAll =        (flags >> 1) !=0;

           if (!CancelAll)
           {
             IsActive =       (flags >> 2) !=0;
             IsReplacement =  (flags >> 3) !=0;
             Side = (SideType)((flags >> 4)&0x1);

             IsTakeProfit =   (flags >> 5) !=0;
             IsStopLoss    =   (flags >> 6) !=0;

             OrderID = SecDBPrimitives.ReadSLEB128(stream);

             if (IsActive)
             {
                 var price = SecDBPrimitives.ReadSLEB128(stream);
                 if (ps!=null)
                    PriceStep = price;
                  else
                    PriceStep = ps.PriceStep + price;

                 Price = PriceStep * file.SystemHeader.PriceStep;

                 Qty  = SecDBPrimitives.ReadSLEB128(stream);
             }

             if (IsReplacement)
              OldOrderID = SecDBPrimitives.ReadSLEB128(stream);
           }
        }


        public readonly bool InternalOrder;
        public readonly bool CancelAll;
        public readonly bool IsActive;
        public readonly bool IsReplacement;
        public readonly SideType Side;
        public readonly bool IsTakeProfit;
        public readonly bool IsStopLoss;

        public readonly long OrderID;

        public long PriceStep;
        public float Price;

        public readonly long Qty;
        public readonly long OldOrderID;
      }




      public class SummarySample : StreamSample
      {
        internal SummarySample(DateTime ts, Stream stream) : base(ts)
        {
           var flags = SecDBPrimitives.ReadByte(stream);

           IsBidQty =    (flags & (1)) !=0;
           IsAskQty =    (flags & (1 << 1)) !=0;
           IsPositions = (flags & (1 << 2)) !=0;
           IsRisk =      (flags & (1 << 3)) !=0;

           if (IsBidQty)    BidQty    = SecDBPrimitives.ReadULEB128(stream);
           if (IsAskQty)    AskQty    = SecDBPrimitives.ReadULEB128(stream);
           if (IsPositions) Positions = SecDBPrimitives.ReadSLEB128(stream);
           if (IsRisk)      RiskAmt   = SecDBPrimitives.ReadDouble(stream);
        }


        public readonly bool IsBidQty;
        public readonly bool IsAskQty;
        public readonly bool IsPositions;
        public readonly bool IsRisk;

        public readonly ulong  BidQty;
        public readonly ulong  AskQty;
        public readonly long   Positions;
        public readonly double RiskAmt;
      }

      public class InfoSample : StreamSample
      {
        internal InfoSample(DateTime ts, Stream stream) : base(ts)
        {
          var tp = SecDBPrimitives.ReadByte(stream);

          if (tp==0)
          {
            Body = SecDBPrimitives.ReadString(stream);
          }
          else
          {
            ResID = SecDBPrimitives.ReadULEB128(stream);
          }
        }

        public readonly string Body;
        public readonly ulong ResID;
      }



  }

}
