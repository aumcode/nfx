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

      public const string HEADER = "#!/usr/bin/env nfxbl\n";
      public static readonly byte[] HEADER_BUF = System.Text.Encoding.ASCII.GetBytes(HEADER);

    #endregion
    
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

    }

    private Stream m_Stream;
    private long m_StreamStart;

    #region .pvt

      private static void checkStream(Stream stream, string op)
      {
        if (stream==null)
          throw new BinLogException(StringConsts.BINLOG_STREAM_NULL_ERROR + op);
       
        if (!stream.CanSeek || !stream.CanWrite)
          throw new BinLogException(StringConsts.BINLOG_STREAM_CANT_SEEK_WRITE_ERROR + op);
      }

      private static Type readHeader(Stream stream)
      {
        var buf = Primitives.getBuff32();
        Primitives.readFromStream(stream, buf, HEADER_BUF.Length);
        for(var i=0; i<HEADER_BUF.Length; i++)
         if (buf[i]!=HEADER_BUF[i])
          throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR+"bad header");

        var tn = Primitives.ReadString(stream);
        if (tn.IsNullOrWhiteSpace())
          throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR+"bad type");

        var result = Type.GetType(tn, false);

        if (result==null || !typeof(LogReader).IsAssignableFrom(result))
          throw new BinLogException(StringConsts.BINLOG_BAD_READER_TYPE_ERROR + tn);

        return result;
      }

    #endregion
  }
}
