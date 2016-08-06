using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.IO.FileSystem;

namespace NFX.Financial.Market.SecDb
{
  /// <summary>
  /// Denotes stream types (id). Used in StreamMeta
  /// </summary>
  public enum StreamID
  {
    Seconds = 1,
    Quotes  = 2,
    Trade   = 3,
    Order   = 4,
    Summary = 5,
    Message = 6
  }


  /// <summary>
  /// Represents SecDB file. This class is thread-safe.
  /// See: https://github.com/saleyn/secdb/wiki/Data-Format
  /// </summary>
  public sealed partial class SecDBFileReader : DisposableObject
  {
    #region CONSTS

      public const string HDR_VERSION     = "version";
      public const string HDR_DATE        = "utc-date";
      public const string HDR_EXCHANGE    = "exchange";
      public const string HDR_SYMBOL      = "symbol";
      public const string HDR_INSTRUMENT  = "instr";
      public const string HDR_SECURITYID  = "secid";
      public const string HDR_DEPTH       = "depth";
      public const string HDR_PRICESTEP   = "px-step";
      public const string HDR_UUID        = "uuid";

    #endregion

    #region .ctor

      public SecDBFileReader(FileSystem fs, FileSystemSessionConnectParams conParams, string fileName)
      {
        if (fs==null ||
            conParams==null ||
            fileName.IsNullOrWhiteSpace())
          throw new FinancialException(StringConsts.ARGUMENT_ERROR+"SecDBFileReader.ctor(fs==null | conParams==null | fileName==null)");


        if (!fs.InstanceCapabilities.SupportsStreamSeek)
          throw new FinancialException(StringConsts.SECDB_FS_SEEK_STREAM_ERROR.Args("{0}('{1}')".Args(fs.GetType().FullName, fs.Name)));


        m_FS = fs;
        m_FSConnect = conParams;
        m_FileName = fileName;

        workWithFile<bool, bool>( (_, file) => { parseHeadersAndMetadata(file); return true; }, true);
      }

    #endregion

    #region .pvt

      private FileSystem m_FS;
      private FileSystemSessionConnectParams m_FSConnect;
      private string m_FileName;

      private FileRequiredHeader m_SystemHeader;

      private Dictionary<string, string> m_Headers = new Dictionary<string, string>(StringComparer.Ordinal);

      private CompressionType m_Streams_CompressionType;
      private uint            m_Streams_DataOffset;
      private StreamMeta[]    m_Streams_Metas;

      private CandlesMeta     m_Candles_Meta;

    #endregion

    #region Properties

     /// <summary>
     /// Returns the required/system portion of the file header
     /// </summary>
     public FileRequiredHeader SystemHeader { get { return m_SystemHeader;} }

     /// <summary>
     /// File headers dictionary, including the required ones
     /// </summary>
     public IReadOnlyDictionary<string, string> Headers { get{ return m_Headers;} }


     public CompressionType StreamsCompressionType { get{ return m_Streams_CompressionType;}}

     public uint         StreamsDataOffset { get{ return m_Streams_DataOffset;}}
     public StreamMeta[] StreamsMetas      { get{ return m_Streams_Metas;}}


     /// <summary>
     /// Returns metadata for candles in the file - collection of candle headers
     /// </summary>
     public CandlesMeta CandlesMetadata { get{ return m_Candles_Meta;} }

    #endregion


    #region Pub

      /// <summary>
      /// Tries to find a CandleHeader with the exact specified resolution or unassigned header
      /// </summary>
      public CandleHeader GetCandleHeader(uint resolutionSec)
      {
        return m_Candles_Meta.Candles.FirstOrDefault( ch => ch.ResolutionSec == resolutionSec);
      }

      /// <summary>
      /// Tries to find a candle stream with the specified exact resolution skipping the specified number of seconds and returns it, or
      /// returns an empty enumerable if resolution is not available
      /// </summary>
      public IEnumerable<CandleData> GetCandleData(uint resolutionSec, int skipSeconds = 0)
      {
        var hdr = GetCandleHeader(resolutionSec);
        if (!hdr.IsAssigned) return Enumerable.Empty<CandleData>();
        return GetCandleData(hdr, skipSeconds);
      }

      /// <summary>
      /// Tries to find a candle stream with the specified exact resolution and returns it starting from the specified start time, or
      /// returns an empty enumerable if resolution is not available
      /// </summary>
      public IEnumerable<CandleData> GetCandleData(uint resolutionSec, DateTime startTimeUTC)
      {
        var hdr = GetCandleHeader(resolutionSec);
        if (!hdr.IsAssigned) return Enumerable.Empty<CandleData>();
        return GetCandleData(hdr, hdr.GetSkipSecondsUntil(startTimeUTC));
      }


      /// <summary>
      /// Returns candle data at the specified resolution starting at the specified start time
      /// </summary>
      public IEnumerable<CandleData> GetCandleData(CandleHeader header, DateTime startTimeUTC)
      {
        return GetCandleData(header, header.GetSkipSecondsUntil(startTimeUTC));
      }


      /// <summary>
      /// Returns candle data at the specified resolution skipping the first specified number of seconds
      /// </summary>
      public IEnumerable<CandleData> GetCandleData(CandleHeader header, int skipSeconds = 0)
      {
        if (!header.IsAssigned || header.File!=this)
          throw new FinancialException(StringConsts.ARGUMENT_ERROR+"GetCandleData(!header.IsAssigned | wrong)");


        return workWithFile<enumerateCandleDataArgs, IEnumerable<CandleData>>(
                                 enumerateCandleData,
                                 new enumerateCandleDataArgs
                                 {
                                   Header = header,
                                   SkipSeconds = skipSeconds
                                 });
      }

       /// <summary>
      /// Tries to find a candle stream returned as CandleSamples with the specified exact resolution starting at the specified start time and returns it, or
      /// returns an empty enumerable if resolution is not available
      /// </summary>
      public IEnumerable<CandleSample> GetCandleDataAsCandleSamples(uint resolutionSec, DateTime startTimeUTC)
      {
        var hdr = GetCandleHeader(resolutionSec);
        if (!hdr.IsAssigned) return Enumerable.Empty<CandleSample>();
        return this.GetCandleData(hdr, hdr.GetSkipSecondsUntil(startTimeUTC))
                   .Select( cd =>
                           {
                            var cs = cd.ToCandleSample();
                            cs.TimeSpanMs = hdr.ResolutionSec * 1000;
                            return cs;
                           });
      }


      /// <summary>
      /// Tries to find a candle stream returned as CandleSamples with the specified exact resolution skipping the specofoed number of seconds and returns it, or
      /// returns an empty enumerable if resolution is not available
      /// </summary>
      public IEnumerable<CandleSample> GetCandleDataAsCandleSamples(uint resolutionSec, int skipSeconds = 0)
      {
        var hdr = GetCandleHeader(resolutionSec);
        if (!hdr.IsAssigned) return Enumerable.Empty<CandleSample>();
        return this.GetCandleData(hdr, skipSeconds)
                   .Select( cd =>
                           {
                            var cs = cd.ToCandleSample();
                            cs.TimeSpanMs = hdr.ResolutionSec * 1000;
                            return cs;
                           });
      }


      /// <summary>
      /// Returns candle data at the specified resolution starting at the specified start time
      /// </summary>
      public IEnumerable<CandleSample> GetCandleDataAsCandleSamples(CandleHeader header, DateTime startTimeUTC)
      {
        return this.GetCandleDataAsCandleSamples(header, header.GetSkipSecondsUntil(startTimeUTC));
      }

      /// <summary>
      /// Returns candle data at the specified resolution skipping the first specified number of seconds
      /// </summary>
      public IEnumerable<CandleSample> GetCandleDataAsCandleSamples(CandleHeader header, int skipSeconds = 0)
      {
        return this.GetCandleData(header, skipSeconds)
                   .Select( cd =>
                           {
                             var cs = cd.ToCandleSample();
                             cs.TimeSpanMs = header.ResolutionSec * 1000;
                             return cs;
                           });
      }


      /// <summary>
      /// Tries to find a candle stream by resolution and returns stream data starting from the specified UTC start time
      /// </summary>
      public IEnumerable<StreamSample> GetStreamData(uint resolutionSec, DateTime startTimeUTC)
      {
        var hdr = GetCandleHeader(resolutionSec);
        if (!hdr.IsAssigned) return Enumerable.Empty<StreamSample>();
        return this.GetStreamData(hdr, hdr.GetSkipSecondsUntil(startTimeUTC));
      }


      /// <summary>
      /// Tries to find a candle stream by resolution and return stream data skipping the specified seconds count from data start
      /// </summary>
      public IEnumerable<StreamSample> GetStreamData(uint resolutionSec, int skipSeconds = 0)
      {
        var hdr = GetCandleHeader(resolutionSec);
        if (!hdr.IsAssigned) return Enumerable.Empty<StreamSample>();
        return this.GetStreamData(hdr, skipSeconds);
      }


      /// <summary>
      /// Gets stream data per candle header index starting from the specified UTC start time
      /// </summary>
      public IEnumerable<StreamSample> GetStreamData(CandleHeader header, DateTime startTimeUTC)
      {
        return GetStreamData(header, header.GetSkipSecondsUntil(startTimeUTC));
      }

      /// <summary>
      /// Gets stream data per candle header index skipping the specified seconds count from data start
      /// </summary>
      public IEnumerable<StreamSample> GetStreamData(CandleHeader header, int skipSeconds = 0)
      {
        var candle = GetCandleData(header, skipSeconds).FirstOrDefault();
        if (!candle.IsAssigned) return Enumerable.Empty<StreamSample>();

        return workWithFile<enumerateStreamDataArgs, IEnumerable<StreamSample>>(
                                 enumerateStreamData,
                                 new enumerateStreamDataArgs
                                 {
                                   Candle = candle
                                 });
      }


      /// <summary>
      /// Gets all stream data
      /// </summary>
      public IEnumerable<StreamSample> GetAllStreamData()
      {
        return workWithFile<enumerateStreamDataArgs, IEnumerable<StreamSample>>(
                                 enumerateStreamData,
                                 new enumerateStreamDataArgs
                                 {
                                   Candle = new CandleData()
                                 });
      }


    #endregion

    #region .pvt

      private TResult workWithFile<TContext, TResult>(Func<TContext, FileSystemFile, TResult> body, TContext context)
      {
         using(var session = m_FS.StartSession(m_FSConnect))
         {
           var file = session[m_FileName] as FileSystemFile;
           if (file==null)
            throw new FinancialException(StringConsts.SECDB_FILE_NOT_FOUND_ERROR.Args(m_FileName, "{0}('{1}')".Args(m_FS.GetType().FullName, m_FS.Name)));

           using(file)
             return body(context, file);
         }
      }


      private void scrollStreamToHeadersEnd(Stream stream)
      {
        stream.Position = 0;
        int pbt = 0;
        while(true)
        {
          var bt = stream.ReadByte();
          if (bt==-1)
            throw new FinancialException(StringConsts.SECDB_FILE_HEADER_ERROR + "eof before expected in scrollStreamToHeadersEnd");

          if (bt=='\n' && pbt=='\n') break;
          pbt = bt;
        }
      }


      private void parseHeadersAndMetadata(FileSystemFile file)
      {
        string shebang = null;
        try
        {
          using(var fileStream = file.FileStream)
          {
             using(var reader = new StreamReader( new NFX.IO.NonClosingStreamWrap(fileStream), Encoding.UTF8))//Warning. Doc seays ASCII! But better use UTF8
             {
                while(!reader.EndOfStream)
                {
                  var line = reader.ReadLine();
                  if (line.IsNullOrWhiteSpace()) break;

                  if (shebang==null)
                  {
                    shebang = line;
                    continue;
                  }

                  var i = line.IndexOf(':');
                  if (i<=0)
                    throw new FinancialException("Header line syntax '{0}'".Args(line));

                  var key = line.Substring(0, i);
                  var value = i<line.Length-1 ? line.Substring(i+1) : string.Empty;

                  if (m_Headers.ContainsKey(key))
                    throw new FinancialException("Duplicate header key '{0}'".Args(key));

                  m_Headers.Add(key, value.TrimStart());
                }

             }//reader

             m_SystemHeader = new FileRequiredHeader(shebang, m_Headers);

             scrollStreamToHeadersEnd(fileStream);
             parseMetadata(fileStream);
          }
        }
        catch(Exception error)
        {
          throw new FinancialException(StringConsts.SECDB_FILE_HEADER_ERROR + error.ToMessageWithType());
        }


      }


      private void parseMetadata(Stream stream)
      {
        var bt = SecDBPrimitives.ReadByte(stream);
        if (bt!=0x01)//0x1 - Streams header
          throw new FinancialException("StreamsMeta.Header != 0x01");

        //Compression
        bt = SecDBPrimitives.ReadByte(stream);
        if (bt==0) m_Streams_CompressionType = CompressionType.None;
        else if (bt==1) m_Streams_CompressionType = CompressionType.GZip;
        else throw new FinancialException("StreamsMeta.Compression != 0 | 1");

        //DataOffset
        m_Streams_DataOffset = SecDBPrimitives.ReadUInt32(stream);

        //Stream Count
        var scount = SecDBPrimitives.ReadByte(stream);

        m_Streams_Metas = new StreamMeta[ scount+1 ];
        m_Streams_Metas[0] = new StreamMeta(StreamID.Seconds);

        //StreamMeta
        for(var i=1; i<=scount; i++) //<= as scount does not include zeros element which is mandatory
        {
          bt = SecDBPrimitives.ReadByte(stream);
          if (bt!=0x02)//0x2 - Stream header
            throw new FinancialException("StreamMeta.Header != 0x02");

          //StreamID
          bt = SecDBPrimitives.ReadByte(stream);
          if (bt>(byte)StreamID.MAX_ID)
            throw new FinancialException("StreamMeta.StreamID {0} = invalid".Args(bt));

          var sid = (StreamID)bt;
          m_Streams_Metas[i] = new StreamMeta(sid);
        }

        //CandlesMeta
        parseCandlesMeta( stream );

        //Verify data start
        //Beginning of Stream Data Marker
        stream.Seek(m_Streams_DataOffset, SeekOrigin.Begin);
        var magic = SecDBPrimitives.ReadUInt32(stream);
        if (magic!=0xABBABABA)
           throw new FinancialException("StreamDataMarker != 0xABBABABA");

      }

      private void parseCandlesMeta(Stream stream)
      {
        var bt = SecDBPrimitives.ReadByte(stream);
        if (bt!=0x03)//0x3 -  Candles metadata header
          throw new FinancialException("CandlesMeta.Header != 0x03");

        //Filler
        bt = SecDBPrimitives.ReadByte(stream);

        //HdrCount
        var icount = SecDBPrimitives.ReadUInt16(stream);

        var chdrs = new CandleHeader[icount];
        //CandleHeader
        for(var i=0; i<icount; i++)
        {
           bt = SecDBPrimitives.ReadByte(stream);
           if (bt!=0x04)//0x4 -  Candle Header
             throw new FinancialException("CandleHeader.Header != 0x04");

           //Filler
           bt = SecDBPrimitives.ReadByte(stream);

           var resolution = SecDBPrimitives.ReadUInt16(stream);
           var starttime  = SecDBPrimitives.ReadUInt32(stream);
           var count =  SecDBPrimitives.ReadUInt32(stream);
           var offset = SecDBPrimitives.ReadUInt32(stream);


           if (resolution==0)
             throw new FinancialException("CandleHeader.Resolution == 0x00");

           var sdt = new DateTime(m_SystemHeader.Date.Year,
                                  m_SystemHeader.Date.Month,
                                  m_SystemHeader.Date.Day,
                                  0, 0, 0, DateTimeKind.Utc).AddSeconds( starttime );

           var ch = new CandleHeader(this, resolution, starttime, sdt, count, offset);
           chdrs[i] = ch;
        }
        m_Candles_Meta = new CandlesMeta(this, chdrs);


      }


      private struct enumerateCandleDataArgs{ public CandleHeader Header; public int SkipSeconds; }
      private IEnumerable<CandleData> enumerateCandleData(enumerateCandleDataArgs args, FileSystemFile file)
      {
        var header = args.Header;
        var skipSec = args.SkipSeconds;

        using(var stream = file.FileStream)
        {
          //Candle data start
          stream.Seek(header.DataOffset, SeekOrigin.Begin);

          DateTime ts = header.StartTime;
          var i = 0;

          if (skipSec>0)
          {
            var skipSamples = skipSec / header.ResolutionSec;//int division. Resolution was already checked for 0

            if (skipSamples>=header.CandleCount) yield break;

            i = skipSamples;
            stream.Seek(skipSamples * CandleData.BYTE_SIZE, SeekOrigin.Current);
            ts.AddSeconds(skipSamples * header.ResolutionSec);
          }

          for(; i<header.CandleCount; i++)
          {
            yield return new CandleData(this, i, ts, stream);
            ts = ts.AddSeconds(header.ResolutionSec);
          }
        }//using stream
      }


      private static readonly Func<SecDBFileReader, StreamSample, DateTime, Stream, StreamSample>[] STREAM_SAMPLE_FACTORIES =
                                  new Func<SecDBFileReader, StreamSample, DateTime, Stream, StreamSample>[]{
           (file, ps, ts, stream) => new SecondSample (ts),
           (file, ps, ts, stream) => new QuoteSample  (file, ps as QuoteSample, ts, stream),
           (file, ps, ts, stream) => new TradeSample  (file, ps as TradeSample, ts, stream),
           (file, ps, ts, stream) => new OrderSample  (file, ps as OrderSample, ts, stream),
           (file, ps, ts, stream) => new SummarySample(ts, stream),
           (file, ps, ts, stream) => new InfoSample   (ts, stream)
      };

      private struct enumerateStreamDataArgs{ public CandleData Candle; }
      private IEnumerable<StreamSample> enumerateStreamData(enumerateStreamDataArgs args, FileSystemFile file)
      {
        var candle = args.Candle;

        using(var stream = file.FileStream)
        {
          //Stream data start per candle
          if (candle.IsAssigned)
           stream.Seek((long)candle.FirstStreamOffset, SeekOrigin.Begin);
          else
           stream.Seek((long)(m_Streams_DataOffset+sizeof(int)), SeekOrigin.Begin);

          DateTime currentTS = candle.TimeStamp;

          StreamSample[] prior = new StreamSample[(int)StreamID.MAX_ID + 1];
          while(true)
          {
            var hdr = stream.ReadByte();
            if (hdr==-1) yield break;//END OF FILE

            var isDelta = (hdr & 0x80)!=0;
            var sid = hdr & 0x7f;
            if ((StreamID)sid>StreamID.MAX_ID || sid>=STREAM_SAMPLE_FACTORIES.Length)
              throw new FinancialException(StringConsts.SECDB_STREAM_CORRUPTED_ERROR+"enumerateStreamData(sid>StreamIS.MAX_ID)");

            if (sid==(int)StreamID.Seconds)
            {
              int ts =  SecDBPrimitives.ReadMidnightSecond(stream);
              currentTS = new DateTime(m_SystemHeader.Date.Year,
                                     m_SystemHeader.Date.Month,
                                     m_SystemHeader.Date.Day,
                                     0, 0, 0, DateTimeKind.Utc).AddSeconds( ts );
              //Second resets all other
              for(var i=0; i<prior.Length; i++)
                prior[i] = null;
            }
            else
            {
              var mcsDiff =  SecDBPrimitives.ReadDiffTimeMcs(stream);
              var ticks = mcsDiff * 10;
              currentTS = currentTS.AddTicks((long)ticks);
            }


            var priorSampleOfThisType = prior[sid];
            if (priorSampleOfThisType==null && isDelta)
               throw new FinancialException(StringConsts.SECDB_STREAM_CORRUPTED_ERROR+"prior sample of '{0}' type is null, but IsDelta = 1".Args((StreamID)sid));

            var priorSecond = prior[0] as SecondSample;
            if (sid!=0 && priorSecond==null)
               throw new FinancialException(StringConsts.SECDB_STREAM_CORRUPTED_ERROR+"missing SecondSample before '{0}' sample".Args((StreamID)sid));


            var sample = STREAM_SAMPLE_FACTORIES[sid](this, priorSampleOfThisType, currentTS, stream);
            yield return sample;
            prior[sid] = sample;
          }
        }//using stream
      }


    #endregion

  }

}
