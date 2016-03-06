using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.IO;

namespace NFX.Log.Bin
{
  /// <summary>
  /// Facilittates reading primitive values from stream
  /// </summary>
  internal static class Primitives
  {
    public static byte ReadByte(Stream stream)
    {
      var b = stream.ReadByte();
      if (b<0) throw new BinLogException(StringConsts.SECDB_STREAM_CORRUPTED_ERROR + "ReadByte(): eof");
      return (byte)b;
    }

    public static string ReadString(Stream stream)
    {
      var sz = stream.ReadBEInt32();
      const long MAX_STR = 128 * 1024;

      if (sz>MAX_STR)
        throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR + "ReadString(): sz={0}>max={1}".Args(sz, MAX_STR));

      var buf = new byte[sz];
      readFromStream(stream, buf, (int)sz);

      return Encoding.UTF8.GetString(buf);
    }


    #region buf readers
      [ThreadStatic] private static byte[] ts_Buff32;
                       
      [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

      [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
      internal static int readFromStream(Stream stream, byte[] buffer, int count)
      {
        if (count<=0) return 0;
        var total = 0;
        do
        {
          var got = stream.Read(buffer, total, count-total);
          if (got==0) //EOF
          throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR +
                                        "readFromStream(Need: {0}; Got: {1})".Args(count, total));
          total += got;
        }while(total<count);

        return total;
      } 
    #endregion

  }
}
