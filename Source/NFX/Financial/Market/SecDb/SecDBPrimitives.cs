using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.IO;

namespace NFX.Financial.Market.SecDb
{
  /// <summary>
  /// Facilittates reading SecDB primitive values
  /// see:  https://github.com/saleyn/secdb/wiki/Data-Format
  /// </summary>
  public static class SecDBPrimitives
  {
    public static byte ReadByte(Stream stream)
    {
      var b = stream.ReadByte();
      if (b<0) throw new FinancialException(StringConsts.SECDB_STREAM_CORRUPTED_ERROR + "ReadByte(): eof");
      return (byte)b;
    }

    public static short ReadInt16(Stream stream)
    {
      var buf = getBuff32();
      readFromStream(stream, buf, 2);
      return (short)((int)buf[0] | ((int)buf[1] << 8));
    }

    public static ushort ReadUInt16(Stream stream)
    {
      var buf = getBuff32();
      readFromStream(stream, buf, 2);
      return (ushort)((int)buf[0] | ((int)buf[1] << 8));
    }

    public static int ReadInt32(Stream stream)
    {
      var buf = getBuff32();
      readFromStream(stream, buf, 4);
      return  (int)buf[0] |
             ((int)buf[1] << 8) |
             ((int)buf[2] << 16) |
             ((int)buf[3] << 24);
    }

    public static uint ReadUInt32(Stream stream)
    {
      var buf = getBuff32();
      readFromStream(stream, buf, 4);
      return  (uint)buf[0] |
             ((uint)buf[1] << 8) |
             ((uint)buf[2] << 16) |
             ((uint)buf[3] << 24);
    }

    public static long ReadInt64(Stream stream)
    {
      var buf = getBuff32();
      readFromStream(stream, buf, 8);
      return  (long)buf[0] |
             ((long)buf[1] << 8) |
             ((long)buf[2] << 16) |
             ((long)buf[3] << 24) |
             ((long)buf[4] << 32) |
             ((long)buf[5] << 40) |
             ((long)buf[6] << 48) |
             ((long)buf[7] << 56);
    }

    public static ulong ReadUInt64(Stream stream)
    {
      var buf = getBuff32();
      readFromStream(stream, buf, 8);
      return  (ulong)buf[0] |
             ((ulong)buf[1] << 8) |
             ((ulong)buf[2] << 16) |
             ((ulong)buf[3] << 24) |
             ((ulong)buf[4] << 32) |
             ((ulong)buf[5] << 40) |
             ((ulong)buf[6] << 48) |
             ((ulong)buf[7] << 56);
    }

    public unsafe static double ReadDouble(Stream stream)
    {
      var buf = getBuff32();
      readFromStream(stream, buf, 8);

      uint seg1 =  (uint)((int)buf[0] |
                          (int)buf[1] << 8 |
                          (int)buf[2] << 16 |
                          (int)buf[3] << 24);

	    uint seg2 =  (uint)((int)buf[4] |
                          (int)buf[5] << 8 |
                          (int)buf[6] << 16 |
                          (int)buf[7] << 24);

	    ulong core = (ulong)seg2 << 32 | (ulong)seg1;

	    return *(double*)(&core);
    }


    public static long ReadSLEB128(Stream stream)
    {
      return LEB128.ReadSLEB128(stream);
    }

    public static ulong ReadULEB128(Stream stream)
    {
      return LEB128.ReadULEB128(stream);
    }

    public static int ReadMidnightSecond(Stream stream)
    {
      return (int)LEB128.ReadSLEB128(stream);
    }


    //public static ulong ReadTimestampMcs(Stream stream)
    //{
    //  var microseconds = ReadUInt64(stream);
    //  return microseconds;
    //}

    //public static DateTime ReadTimestampMcsAsDateTime(Stream stream)
    //{
    //  var microseconds = ReadUInt64(stream);
    //  return MiscUtils.FromMicrosecondsSinceUnixEpochStart( microseconds );
    //}

    public static ulong ReadDiffTimeMcs(Stream stream)
    {
      var microseconds = LEB128.ReadULEB128(stream);
      return microseconds;
    }

    //public static TimeSpan ReadDiffTimeMcsAsTimeSpan(Stream stream)
    //{
    //  var microseconds = ReadDiffTimeMcs(stream);
    //  var ticks = (long)microseconds * 10;
    //  return new TimeSpan( ticks );
    //}

    public static string ReadString(Stream stream)
    {
      var sz = LEB128.ReadULEB128(stream);
      const long MAX_STR = 128 * 1024;

      if (sz>MAX_STR)
        throw new FinancialException(StringConsts.SECDB_STREAM_CORRUPTED_ERROR + "ReadString(): sz={0}>max={1}".Args(sz, MAX_STR));

      var buf = new byte[sz];
      readFromStream(stream, buf, (int)sz);

      return Encoding.UTF8.GetString(buf);
    }


    #region buf readers
      [ThreadStatic] private static byte[] ts_Buff32;

      [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
      )]
      internal static byte[] getBuff32()
      {
        var result = ts_Buff32;
        if (result==null)
        {
          result = new byte[32];
          ts_Buff32 = result;
        }
        return result;
      }

      [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
      )]
      internal static int readFromStream(Stream stream, byte[] buffer, int count)
      {
        if (count<=0) return 0;
        var total = 0;
        do
        {
          var got = stream.Read(buffer, total, count-total);
          if (got==0) //EOF
          throw new FinancialException(StringConsts.SECDB_STREAM_CORRUPTED_ERROR +
                                        "readFromStream(Need: {0}; Got: {1})".Args(count, total));
          total += got;
        }while(total<count);

        return total;
      }
    #endregion

  }
}
