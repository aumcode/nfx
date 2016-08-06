using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NFX.Log.Bin
{
  /// <summary>
  /// Provides abstraction for binary object log readers. Use Open(stream method) to obtain the
  /// instance of the reader type appropriate for particular content.
  /// This class is NOT thread safe
  /// </summary>
  public abstract class LogReader
  {
    #region CONSTS

    #endregion

    #region .ctor
      /// <summary>
      /// Opens the appropriate reader type from the stream
      /// </summary>
      public static LogReader Open(Stream stream)
      {
        checkStream(stream, "LogReader.Open()");

        var pos = stream.Position;
        var type = readHeader(stream);
        stream.Seek(pos, SeekOrigin.Begin);
        LogReader result;
        try
        {
          result = (LogReader)Activator.CreateInstance(type, stream);
        }
        catch(Exception error)
        {
          throw new BinLogException(StringConsts.BINLOG_READER_FACTORY_ERROR+error.ToMessageWithType(), error);
        }
        return result;
      }


      protected LogReader(Stream stream)
      {
        checkStream(stream, "{0}.ctor(stream)".Args(GetType().Name));
        m_Stream = stream;
        m_StreamStart = stream.Position;
        var type = readHeader(stream);
        if (type!=GetType())
         throw new BinLogException(StringConsts.BINLOG_READER_TYPE_MISMATCH_ERROR.Args(GetType().Name, type.Name));
        m_DataStart = stream.Position;//already alligned after header
      }
    #endregion

    #region Fields
      private Stream m_Stream;
      private long m_StreamStart;
      private long m_DataStart;
    #endregion

    #region Properties

      /// <summary>
      /// Returns the underlying stream
      /// </summary>
      public Stream Stream{ get{ return m_Stream;}}

    #endregion


    #region Public

      /// <summary>
      /// Returns the lazy enumerable that fetches data from the very log start, optionally including time stamps
      /// </summary>
      public IEnumerable<object> ReadFromStart(bool includeMetadata = true)
      {
        m_Stream.Seek(m_DataStart, SeekOrigin.Begin);
        return read(includeMetadata);
      }

      /// <summary>
      /// Returns the lazy enumerable that fetches data from the point in time, optionally including time stamps
      /// </summary>
      public IEnumerable<object> ReadFromAround(DateTime startUTC, bool includeMetadata = true)
      {
        seekForTimeStamp(startUTC);
        return read(includeMetadata);
      }

    #endregion

    #region .pvt

      public void seekForTimeStamp(DateTime startUTC)
      {
        m_Stream.Seek(m_DataStart, SeekOrigin.Begin);

      }

      public IEnumerable<object> read(bool includeMetadata)
      {
        DateTime current = MiscUtils.UNIX_EPOCH_START_DATE;
        while(App.Active)
        {
            while(App.Active)
            {
                var h = Format.TryReadPageHeader(m_Stream, ref current);
                if (h==Format.PageHeaderReadStatus.OK) break;
                if (h==Format.PageHeaderReadStatus.EOF) yield break;
                var pos = Stream.Position;
                var newPos = IntMath.Align16( pos );
                var delta = newPos - pos;

                var eof = true;
                try
                {
                  if (delta>0) Stream.Seek(delta, SeekOrigin.Current);
                  eof = false;
                }
                catch{}
                if (eof) yield break;
            }//page

            while(App.Active)
            {
                byte rec = Format.REC_INVALID;
                try{ rec = Format.ReadByte(m_Stream); } catch{}

                if (rec==Format.REC_EOF) break;
                else if (rec==Format.REC_TIMESTAMP)
                {
                  DateTime ts = DateTime.MinValue;
                  try{ ts = Format.ReadTimeStamp(m_Stream);} catch{}

                  if (ts<=MiscUtils.UNIX_EPOCH_START_DATE || ts<current)//corruption
                  {
                    if (includeMetadata) yield return LogCorruption.Instance;
                    break;
                  }
                  current = ts;
                  if (includeMetadata) yield return new LogUTCTimeStamp(ts);
                }else if (rec==Format.REC_SERIALIZER)
                {
                  //todo deserialize

                }else
                {
                  if (includeMetadata) yield return LogCorruption.Instance;
                  break;
                }
            }//records
        }//outer
      }

      private static void checkStream(Stream stream, string op)
      {
        if (stream==null)
          throw new BinLogException(StringConsts.BINLOG_STREAM_NULL_ERROR + op);

        if (!stream.CanSeek || !stream.CanWrite)
          throw new BinLogException(StringConsts.BINLOG_STREAM_CANT_SEEK_WRITE_ERROR + op);
      }

      private static Type readHeader(Stream stream)
      {
        var buf = Format.getBuff32();
        Format.readFromStream(stream, buf, Format.HEADER_BUF.Length);
        for(var i=0; i<Format.HEADER_BUF.Length; i++)
         if (buf[i]!=Format.HEADER_BUF[i])
          throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR+"bad header");

        var tn = Format.ReadString(stream);
        if (tn.IsNullOrWhiteSpace())
          throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR+"bad type");

        var pos = stream.Position;
        var start = IntMath.Align16(pos);
        while(pos<start)
         if (Format.ReadByte(stream)!=0)
          throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR+"bad header pad");

        var result = Type.GetType(tn, false);

        if (result==null || !typeof(LogReader).IsAssignableFrom(result))
          throw new BinLogException(StringConsts.BINLOG_BAD_READER_TYPE_ERROR + tn);

        return result;
      }

    #endregion
  }
}
