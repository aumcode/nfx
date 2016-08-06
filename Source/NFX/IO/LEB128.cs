using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NFX.IO
{
  /// <summary>
    /// Facilitates int64/uint64 LEB128 encode/decode
    /// See https://en.wikipedia.org/wiki/LEB128
    /// See http://llvm.org/docs/doxygen/html/LEB128_8h_source.html
    /// </summary>
    public static class LEB128
    {

      public static void WriteSLEB128(this byte[] buf, long value, int idxStart = 0, int padding = 0)
      {
        int dummy;
        WriteSLEB128(buf, value, out dummy, idxStart, padding);
      }
      public static void WriteSLEB128(this byte[] buf, long value, out int count,  int idxStart = 0, int padding = 0)
      {
        var origIdx = idxStart;
        bool more;
        do
        {
          byte bt = (byte)((byte)value & 0x7f);
          value >>= 7;
          more = !((((value == 0 ) && ((bt & 0x40) == 0)) ||
                    ((value == -1) && ((bt & 0x40) != 0))));
          if (more)
            bt |= 0x80; // Mark this byte to show that more bytes will follow.

          buf[idxStart] = bt;
          idxStart++;
        }
        while (more);

        // Pad with 0x80 and emit a null byte at the end.
         if (padding != 0)
         {
           for (; padding != 1; --padding)
           {
             buf[idxStart] = 0x80;
             idxStart++;
           }
           buf[idxStart] = 0x00;
           idxStart++;
         }

        count = idxStart - origIdx;
      }



      public static void WriteULEB128(this byte[] buf, ulong value, int idxStart = 0, uint padding = 0)
      {
        int dummy;
        WriteULEB128(buf, value, out dummy, idxStart, padding);
      }
      public static void WriteULEB128(this byte[] buf, ulong value, out int count, int idxStart = 0, uint padding = 0)
      {
         var origIdx = idxStart;
         do
         {
           byte bt = (byte)((byte)value & 0x7f);
           value >>= 7;
           if (value != 0 || padding != 0)
             bt |= 0x80; // Mark this byte to show that more bytes will follow.

           buf[idxStart] = bt;
           idxStart++;
         }
         while (value != 0);

         // Pad with 0x80 and emit a null byte at the end.
         if (padding != 0)
         {
           for (; padding != 1; --padding)
           {
             buf[idxStart] = 0x80;
             idxStart++;
           }
           buf[idxStart] = 0x00;
           idxStart++;
         }
         count = idxStart - origIdx;
      }



      public static void WriteSLEB128(this Stream stream, long value, int padding = 0)
      {
        int dummy;
        WriteSLEB128(stream, value, out dummy, padding);
      }
      public static void WriteSLEB128(this Stream stream, long value, out int count, int padding = 0)
      {
        count = 0;
        bool more;
        do
        {
          byte bt = (byte)((byte)value & 0x7f);
          value >>= 7;
          more = !((((value == 0 ) && ((bt & 0x40) == 0)) ||
                    ((value == -1) && ((bt & 0x40) != 0))));
          if (more)
            bt |= 0x80; // Mark this byte to show that more bytes will follow.

          stream.WriteByte( bt );
          count++;
        }
        while (more);

        // Pad with 0x80 and emit a null byte at the end.
         if (padding != 0)
         {
           for (; padding != 1; --padding)
           {
             stream.WriteByte( 0x80 );
             count++;
           }
           stream.WriteByte( 0x00 );
           count++;
         }
      }


      public static void WriteULEB128(this Stream stream, ulong value, uint padding = 0)
      {
        int dummy;
        WriteULEB128(stream, value, out dummy, padding);
      }
      public static void WriteULEB128(this Stream stream, ulong value, out int count, uint padding = 0)
      {
         count = 0;
         do
         {
           byte bt = (byte)((byte)value & 0x7f);
           value >>= 7;
           if (value != 0 || padding != 0)
             bt |= 0x80; // Mark this byte to show that more bytes will follow.

           stream.WriteByte( bt ); count++;
         }
         while (value != 0);

         // Pad with 0x80 and emit a null byte at the end.
         if (padding != 0)
         {
           for (; padding != 1; --padding)
           {
             stream.WriteByte( 0x80 );
             count++;
           }
           stream.WriteByte( 0x00 );
           count++;
         }
      }


      public static long ReadSLEB128(this byte[] buf, int idxStart = 0)
      {
         int dummy;
         return ReadSLEB128(buf, out dummy, idxStart);
      }

      public static long ReadSLEB128(this byte[] buf, out int count, int idxStart = 0)
      {
         var origIdx = idxStart;
         long value = 0;
         int shift = 0;

         byte bt;
         do
         {
           bt = buf[idxStart];
           idxStart++;

           value |= ((long)(bt & 0x7f) << shift);
           shift += 7;
         }
         while (bt >= 128);

         // Sign extend negative numbers.
         if ((bt & 0x40)!=0)
          value |= (-1L) << shift;

         count = idxStart - origIdx;
         return value;
      }


      public static ulong ReadULEB128(this byte[] buf, int idxStart = 0)
      {
        int dummy;
        return ReadULEB128(buf, out dummy, idxStart);
      }

      public static ulong ReadULEB128(this byte[] buf, out int count, int idxStart = 0)
      {
        var origIdx = idxStart;
        ulong value = 0;
        int shift = 0;
        while(true)
        {
          var bt = buf[idxStart];
          idxStart++;

          value += (ulong)(bt & 0x7f) << shift;

          if (bt<128) break;

          shift += 7;
        }
        count = idxStart - origIdx;
        return value;
      }

      public static long ReadSLEB128(this Stream stream)
      {
        int dummy;
        return ReadSLEB128(stream, out dummy);
      }
      public static long ReadSLEB128(this Stream stream, out int count)
      {
        long value = 0;
        count = 0;
        int shift = 0;

        byte bt;
        do
        {
          var ibt = stream.ReadByte();

          if (ibt<0) throw new NFXIOException("LEB128.ReadSLEB128(premature stream end)");

          bt = (byte)ibt;

          count++;

          value |= ((long)(bt & 0x7f) << shift);
          shift += 7;
        }
        while (bt >= 128);

        // Sign extend negative numbers.
        if ((bt & 0x40)!=0)
         value |= (-1L) << shift;

        return value;
      }



      public static ulong ReadULEB128(this Stream stream)
      {
        int dummy;
        return ReadULEB128(stream, out dummy);
      }
      public static ulong ReadULEB128(this Stream stream, out int count)
      {
        ulong value = 0;
        count = 0;
        int shift = 0;
        while(true)
        {
          var bt = stream.ReadByte();

          if (bt<0) throw new NFXIOException("LEB128.ReadULEB128(premature stream end)");

          count++;

          value += (ulong)(bt & 0x7f) << shift;

          if (bt<128) break;

          shift += 7;
        }
        return value;
      }

    }
}
