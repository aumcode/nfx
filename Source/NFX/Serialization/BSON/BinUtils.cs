using System;
using System.IO;
using System.Text;

namespace NFX.Serialization.BSON
{
  /// <summary>
  /// Facilitates read.writing binary BSON primitives. Developers shopuld not normallu use this class
  /// as it is used by serializers and socket low-level code
  /// </summary>
  public static class BinUtils
  {
    #region CONST
      public const byte TERMINATOR = 0x00;
      private const int BUFFER_LENGTH = 512;
      private const int MAX_CHARS_IN_BUFFER = BUFFER_LENGTH;
    #endregion

    #region Fields
      [ThreadStatic] private static byte[] ts_Buffer;
      private static readonly UTF8Encoding s_UTF8Encoding = new UTF8Encoding(false, false);
    #endregion

    public static UTF8Encoding UTF8Encoding { get { return s_UTF8Encoding; } }

    #region Pub Methods
      /// <summary>
      /// Writes terminator 0x00 symbol to stream
      /// </summary>
      public static void WriteTerminator(Stream stream)
      {
        stream.WriteByte(TERMINATOR);
      }

      /// <summary>
      ///  Writes BSON cstring to stream
      /// </summary>
      public static void WriteCString(Stream stream, string value)
      {
        var buffer = ensureBuffer();

        var maxByteCount = value.Length * 2; // UTF8 string length in UNICODE16 pairs
        if (maxByteCount < BUFFER_LENGTH)
        {
          var actual = s_UTF8Encoding.GetBytes(value, 0, value.Length, buffer, 0);
          stream.Write(buffer, 0, actual);
        }
        else
        {
          int charCount;
          var totalCharsWritten = 0;
          while (totalCharsWritten < value.Length)
          {
            charCount = Math.Min(MAX_CHARS_IN_BUFFER, value.Length - totalCharsWritten);
            var count = s_UTF8Encoding.GetBytes(value, totalCharsWritten, charCount, buffer, 0);
            stream.Write(buffer, 0, count);
            totalCharsWritten += charCount;
          }
        }

        WriteTerminator(stream);
      }

      //this method is needed for various pre-computations. it is not efficient for tight calls
      public static byte[] WriteCStringToBuffer(string value)
      {
        using(var ms = new MemoryStream(32 + (value.Length*2)))
        {
          WriteCString(ms, value);
          var buf = new byte[ms.Length];
          Buffer.BlockCopy(ms.GetBuffer(), 0, buf, 0, buf.Length);
          return buf;
        }
      }


      /// <summary>
      /// Writes byte to stream
      /// </summary>
      public static void WriteByte(Stream stream, byte value)
      {
        stream.WriteByte(value);
      }

      public static void WriteBinary(Stream stream, byte[] value)
      {
        stream.Write(value, 0 , value.Length);
      }

      /// <summary>
      /// Writes double to stream
      /// </summary>
      public unsafe static void WriteDouble(Stream stream, double value)
      {
        ulong core = *(ulong*)(&value);

        stream.WriteByte((byte)core);
        stream.WriteByte((byte)(core >> 8));
        stream.WriteByte((byte)(core >> 16));
        stream.WriteByte((byte)(core >> 24));
        stream.WriteByte((byte)(core >> 32));
        stream.WriteByte((byte)(core >> 40));
        stream.WriteByte((byte)(core >> 48));
        stream.WriteByte((byte)(core >> 56));
      }

      /// <summary>
      /// Writes 16-bit int to stream
      /// </summary>
      public static void WriteInt16(Stream stream, short value)
      {
        stream.WriteByte((byte)value);
        stream.WriteByte((byte)(value >> 8));
      }

      /// <summary>
      /// Writes 16-bit int to byte array
      /// </summary>
      public static void WriteInt16(byte[] buffer, short value, int startIdx = 0)
      {
        buffer[startIdx] = (byte)value;
        buffer[startIdx + 1] = (byte)(value >> 8);
      }

      /// <summary>
      /// Writes unsigned 16-bit int to stream
      /// </summary>
      public static void WriteUInt16(Stream stream, ushort value)
      {
        WriteInt16(stream, (short)value);
      }

      /// <summary>
      /// Writes unsigned 16-bit int to byte array
      /// </summary>
      public static void WriteUInt16(byte[] buffer, ushort value, int startIdx = 0)
      {
        WriteInt16(buffer, (short)value, startIdx);
      }

      /// <summary>
      /// Writes 32-bit int to stream
      /// </summary>
      public static void WriteInt32(Stream stream, int value)
      {
        stream.WriteByte((byte)value);
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 24));
      }

      /// <summary>
      /// Writes 32bit int to byte array
      /// </summary>
      public static void WriteInt32(byte[] buffer, int value, int startIdx = 0)
      {
        buffer[startIdx] = (byte)value;
        buffer[startIdx + 1] = (byte)(value >> 8);
        buffer[startIdx + 2] = (byte)(value >> 16);
        buffer[startIdx + 3] = (byte)(value >> 24);
      }

      /// <summary>
      /// Writes unsigned 24bit int (less than 2^24) to byte array
      /// </summary>
      public static void WriteUInt24(byte[] buffer, uint value, int startIdx = 0)
      {
        buffer[startIdx] = (byte)value;
        buffer[startIdx + 1] = (byte)(value >> 8);
        buffer[startIdx + 2] = (byte)(value >> 16);
      }

      /// <summary>
      /// Writes unsigned 32-bit int to stream
      /// </summary>
      public static void WriteUInt32(Stream stream, uint value)
      {
        WriteInt32(stream, (int)value);
      }

      /// <summary>
      /// Writes unsigned 32bit int to byte array
      /// </summary>
      public static void WriteUInt32(byte[] buffer, uint value, int startIdx = 0)
      {
        WriteInt32(buffer, (int)value, startIdx);
      }

      /// <summary>
      /// Writes 64-bit int to stream
      /// </summary>
      public static void WriteInt64(Stream stream, long value)
      {
        stream.WriteByte((byte)value);
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 32));
        stream.WriteByte((byte)(value >> 40));
        stream.WriteByte((byte)(value >> 48));
        stream.WriteByte((byte)(value >> 56));
      }

      /// <summary>
      /// Writes 64-bit int to byte array
      /// </summary>
      public static void WriteInt64(byte[] buffer, long value, int startIdx = 0)
      {
        buffer[startIdx] = (byte)value;
        buffer[startIdx + 1] = (byte)(value >> 8);
        buffer[startIdx + 2] = (byte)(value >> 16);
        buffer[startIdx + 3] = (byte)(value >> 24);
        buffer[startIdx + 4] = (byte)(value >> 32);
        buffer[startIdx + 5] = (byte)(value >> 40);
        buffer[startIdx + 6] = (byte)(value >> 48);
        buffer[startIdx + 7] = (byte)(value >> 56);
      }

      /// <summary>
      /// Writes unsigned 64-bit int to stream
      /// </summary>
      public static void WriteUInt64(Stream stream, ulong value)
      {
        WriteInt64(stream, (long)value);
      }

      /// <summary>
      /// Writes unsigned 64-bit int to byte array
      /// </summary>
      public static void WriteUInt64(byte[] buffer, ulong value, int startIdx = 0)
      {
         WriteInt64(buffer, (long)value, startIdx);
      }

      /// <summary>
      /// Reads BSON element type from stream
      /// </summary>
      public static BSONElementType ReadElementType(Stream stream)
      {
        var b = stream.ReadByte();
        if (b==0)
          throw new BSONException(StringConsts.BSON_READ_PREMATURE_EOF_ERROR.Args("ReadElementType"));

        //The method CAN return bad JSON type, but the later logic will not be able to map it and will throw
        return (BSONElementType)b;
      }

      /// <summary>
      /// Reads 16-bit int from stream
      /// </summary>
      public static Int16 ReadInt16(Stream stream)
      {
        var buf = readFromStream(stream, 2, "ReadInt16");
        return (short)(buf[0] | (buf[1] << 8));
      }

      /// <summary>
      /// Reads 16-bit int from byte array
      /// </summary>
      public static Int16 ReadInt16(byte[] bytes, int startIdx = 0)
      {
        return (short)(bytes[startIdx] | (bytes[startIdx + 1] << 8));
      }

      /// <summary>
      /// Reads unsigned 16-bit int from stream
      /// </summary>
      public static UInt16 ReadUInt16(Stream stream)
      {
        return (ushort)ReadInt16(stream);
      }

      /// <summary>
      /// Reads unsigned 16-bit int from byte array
      /// </summary>
      public static UInt16 ReadUInt16(byte[] bytes, int startIdx = 0)
      {
        return (ushort)ReadInt16(bytes, startIdx);
      }

      /// <summary>
      /// Reads 32-bit int from stream
      /// </summary>
      public static Int32 ReadInt32(Stream stream)
      {
        var buf = readFromStream(stream, 4, "ReadInt32");
        return buf[0] |
               (buf[1] << 8) |
               (buf[2] << 16) |
               (buf[3] << 24);
      }

      /// <summary>
      /// Reads 32-bit int from byte array
      /// </summary>
      public static Int32 ReadInt32(byte[] bytes, int startIdx = 0)
      {
        return bytes[startIdx] |
               (bytes[startIdx + 1] << 8) |
               (bytes[startIdx + 2] << 16) |
               (bytes[startIdx + 3] << 24);
      }

      /// <summary>
      /// Reads 24-bit int (less than 2^24) from byte array
      /// </summary>
      public static UInt32 ReadUInt24(byte[] bytes, int startIdx = 0)
      {
        return (uint)(bytes[startIdx] |
                     (bytes[startIdx + 1] << 8) |
                     (bytes[startIdx + 2] << 16));
      }

      /// <summary>
      /// Reads unsigned 32-bit int from stream
      /// </summary>
      public static UInt32 ReadUInt32(Stream stream)
      {
        return (UInt32)ReadInt32(stream);
      }

      /// <summary>
      /// Reads unsigned 32-bit int from byte array
      /// </summary>
      public static UInt32 ReadUInt32(byte[] bytes, int startIdx = 0)
      {
        return (UInt32)ReadInt32(bytes, startIdx);
      }

      /// <summary>
      /// Reads 64-bit int from stream
      /// </summary>
      public static Int64 ReadInt64(Stream stream)
      {
        var buf = readFromStream(stream, 8, "ReadInt64");
        return (long)buf[0] |
               ((long)buf[1] << 8) |
               ((long)buf[2] << 16) |
               ((long)buf[3] << 24) |
               ((long)buf[4] << 32) |
               ((long)buf[5] << 40) |
               ((long)buf[6] << 48) |
               ((long)buf[7] << 56);
      }

      /// <summary>
      /// Reads 64-bit int from byte array
      /// </summary>
      public static Int64 ReadInt64(byte[] bytes, int startIdx = 0)
      {
        return (long)bytes[startIdx] |
               ((long)bytes[startIdx + 1] << 8) |
               ((long)bytes[startIdx + 2] << 16) |
               ((long)bytes[startIdx + 3] << 24) |
               ((long)bytes[startIdx + 4] << 32) |
               ((long)bytes[startIdx + 5] << 40) |
               ((long)bytes[startIdx + 6] << 48) |
               ((long)bytes[startIdx + 7] << 56);
      }

      /// <summary>
      /// Reads unsigned 64-bit int from stream
      /// </summary>
      public static UInt64 ReadUInt64(Stream stream)
      {
        return (UInt64)ReadInt64(stream);
      }

      /// <summary>
      /// Reads unsigned 64-bit int from byte array
      /// </summary>
      public static UInt64 ReadUInt64(byte[] bytes, int startIdx = 0)
      {
        return (UInt64)ReadInt64(bytes, startIdx);
      }

      public static byte ReadByte(Stream stream)
      {
        return (byte)stream.ReadByte();
      }

      /// <summary>
      ///  Reads BSON cstring from stream
      /// </summary>
      public static string ReadCString(Stream stream, int length)
      {
        if (length == 0)
          return string.Empty;

        if (length < 0)
          throw new BSONException(StringConsts.BSON_INCORRECT_STRING_LENGTH_ERROR.Args(length));

        var buffer = new byte[length - 1];
        stream.Read(buffer, 0, length - 1);

        var terminator = BinUtils.ReadByte(stream);
        if (terminator != TERMINATOR)
          throw new BSONException(StringConsts.BSON_UNEXPECTED_END_OF_STRING_ERROR);

        return s_UTF8Encoding.GetString(buffer, 0, length - 1);
      }

      /// <summary>
      ///  Reads BSON cstring from stream
      /// </summary>
      public static string ReadCString(Stream stream)
      {
        int size = 1;
        long position = stream.Position;
        var buffer = readFromStream(stream, 1, "ReadCString");
        do
        {
          stream.Read(buffer, 0, 1);
          size++;
        }
        while (buffer[0] != TERMINATOR);

        stream.Position = position;

        return ReadCString(stream, size);
      }

      /// <summary>
      /// Reads double from stream
      /// </summary>
      public unsafe static double ReadDouble(Stream stream)
      {
        var buffer = readFromStream(stream, 8, "ReadDouble");
        uint seg1 = (uint)((int)buffer[0] |
                           (int)buffer[1] << 8 |
                           (int)buffer[2] << 16 |
                           (int)buffer[3] << 24);

	      uint seg2 = (uint)((int)buffer[4] |
                           (int)buffer[5] << 8 |
                           (int)buffer[6] << 16 |
                           (int)buffer[7] << 24);

	      ulong core = (ulong)seg2 << 32 | (ulong)seg1;

	      return *(double*)(&core);
      }

      /// <summary>
      /// Returns number of digits in 32-bit integer value
      /// </summary>
      public static short GetIntDigitCount(int value)
      {
         short d = 0;
         do
         {
           value = value / 10;
           d++;
         } while (value != 0);

         return d;
      }

    #endregion

    #region .pvt

      [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
      private static byte[] ensureBuffer(int size=0)
      {
        if (size>BUFFER_LENGTH) return new byte[size];

        var buffer = ts_Buffer;
        if (buffer == null)
        {
          buffer = new byte[BUFFER_LENGTH];
          ts_Buffer = buffer;
        }
        return buffer;
      }

      private static byte[] readFromStream(Stream stream, int count, string op)
      {
        var buffer = ensureBuffer(count);

        var total = 0;
        do
        {
          var got = stream.Read(buffer, total, count - total);
          if (got == 0) //EOF
            throw new BSONException(StringConsts.BSON_READ_PREMATURE_EOF_ERROR.Args(op));
          total += got;
        } while (total < count);

        return buffer;
      }

    #endregion .pvt
  }
}
