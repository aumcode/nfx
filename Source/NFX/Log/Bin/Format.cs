/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Text;
using System.IO;

using NFX.IO;

namespace NFX.Log.Bin
{
  /// <summary>
  /// Facilittates reading primitive values from stream
  /// </summary>
  internal static class Format
  {
    public const string HEADER = "#!/usr/bin/env nfxbl\n";
    public static readonly byte[] HEADER_BUF = System.Text.Encoding.ASCII.GetBytes(HEADER);

    public const ulong PAGE_HEADER = 0x504147452d2d2d3e; //PAGE--->

    public const byte REC_INVALID    = 0xFF;
    public const byte REC_EOF        = 0x00;
    public const byte REC_SERIALIZER = 0xAA;
    public const byte REC_TIMESTAMP  = 0x55;

    public const int PAGE_HEADER_SZ = 16;

    public enum PageHeaderReadStatus{OK = 0, EOF, Corruption}

    public static void WritePageHeader(Stream stream, DateTime ts)
    {
      stream.WriteBEUInt64(PAGE_HEADER);
      WriteTimeStamp(stream, ts);
    }

    public static PageHeaderReadStatus TryReadPageHeader(Stream stream, ref DateTime current)
    {
      try
      {
        var h = stream.ReadBEUInt64();
        if (h!=PAGE_HEADER) return PageHeaderReadStatus.Corruption;
        var nix = (long)stream.ReadBEUInt64();
        var sts = nix.FromMicrosecondsSinceUnixEpochStart();
        if (sts<current) return PageHeaderReadStatus.Corruption;
        current = sts;
      }
      catch
      {
        return PageHeaderReadStatus.EOF;
      }
      return PageHeaderReadStatus.OK;
    }


    public static void WriteTimeStamp(Stream stream, DateTime ts)
    {
      var nix = ts.ToMicrosecondsSinceUnixEpochStart();
      stream.WriteBEUInt64((ulong)nix);
    }

    public static DateTime ReadTimeStamp(Stream stream)
    {
      var nix = (long)stream.ReadBEUInt64();
      return nix.FromMicrosecondsSinceUnixEpochStart();
    }


    public static byte ReadByte(Stream stream)
    {
      var b = stream.ReadByte();
      if (b<0) throw new BinLogException(StringConsts.BINLOG_STREAM_CORRUPTED_ERROR + "ReadByte(): eof");
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
