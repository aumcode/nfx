/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
// Author: Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-09-06
// This code is derived from:
// https://github.com/saleyn/otp.net/blob/master/Otp/OtpOutputStream.cs
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace NFX.Erlang
{
  /// <summary>
  /// Provides a stream for encoding Erlang terms to external format for
  /// transmission or storage
  /// </summary>
  public class ErlOutputStream : Stream
  {
  #region CONSTS

    /// <summary>
    /// The default initial size of the stream
    /// </summary>
    private const int DEFAULT_INITIAL_SIZE = 2048;

    /// <summary>
    /// The default increment used when growing the stream
    /// </summary>
    private const int DEFAULT_INCREMENT = 2048;

  #endregion

  #region .ctor

    /// <summary>
    /// Create a stream containing the serialized Erlang term.
    /// Optionally include in the beginning Erlang protocol version byte
    /// </summary>
    public ErlOutputStream(IErlObject o = null, bool writeVersion = true, bool writePktSize = false,
        int capacity = DEFAULT_INITIAL_SIZE)
    {
      m_Buffer = new byte[capacity];
      m_Capacity = capacity;
      m_Position = 0;

      if (writePktSize)
        Write4BE(0); // make space for length data, but final value is not yet known

      if (o == null)
      {
        if (writeVersion)
          write(ErlExternalTag.Version);
        return;
      }

      encodeObject(o, writeVersion);

      if (writePktSize)
        Poke4BE(0, m_Position - 4);
    }

  #endregion

  #region Fields

    private byte[] m_Buffer;
    private int m_Capacity;
    private int m_Position;

  #endregion

  #region Props

    /// <summary>
    /// Get the current capacity of the stream. As bytes are added the
    /// capacity of the stream is increased automatically, however this
    /// method returns the current size
    /// </summary>
    public int Capacity { get { return m_Capacity; } }

    /// <summary>
    /// Get the current position in the stream
    /// </summary>
    public override long Position { get { return m_Position; } set { m_Position = (int)value; } }

    /// <summary>
    /// Get the number of bytes in the stream
    /// </summary>
    public override long Length { get { return m_Position; } }

    public override bool CanRead    { get { return true;  } }
    public override bool CanSeek    { get { return false; } }
    public override bool CanWrite   { get { return true;  } }

  #endregion

  #region Public

    public ErlBinary ToBinary()
    {
      byte[] tmp = new byte[m_Position];
      Array.Copy(m_Buffer, 0, tmp, 0, m_Position);
      return new ErlBinary(tmp);
    }

    /// <summary>
    /// Convert stream content to printable binary string (i.e. &lt;&lt;131,10,...>>)
    /// </summary>
    /// <returns></returns>
    public string ToBinaryString()
    {
      return ErlBinary.ToBinaryString(m_Buffer, 0, m_Position);
    }

    /// <summary>
    /// Reset the stream so that it can be reused
    /// </summary>
    public void Reset()
    {
      m_Position = 0;
    }

    /// <summary>
    /// Get internal buffer
    /// </summary>
    public byte[] GetBuffer() { return m_Buffer; }

    public override void Flush() { return; }

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
      if (value > m_Capacity)
        ensureCapacity((int)value - m_Position);

      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write an arbitrary Erlang term to the stream.
    /// </summary>
    public void Write(IErlObject o)
    {
      switch (o.TypeOrder)
      {
        case ErlTypeOrder.ErlAtom:      WriteAtom((ErlAtom)o); break;
        case ErlTypeOrder.ErlBinary:    WriteBinary((ErlBinary)o); break;
        case ErlTypeOrder.ErlBoolean:   WriteBoolean((ErlBoolean)o); break;
        case ErlTypeOrder.ErlByte:      WriteByte((ErlByte)o); break;
        case ErlTypeOrder.ErlDouble:    WriteDouble((ErlDouble)o); break;
        case ErlTypeOrder.ErlLong:      WriteLong((ErlLong)o); break;
        case ErlTypeOrder.ErlList:      WriteList((ErlList)o); break;
        case ErlTypeOrder.ErlPid:       WritePid((ErlPid)o); break;
        case ErlTypeOrder.ErlPort:      WritePort((ErlPort)o); break;
        case ErlTypeOrder.ErlRef:       WriteRef((ErlRef)o); break;
        case ErlTypeOrder.ErlString:    WriteString((ErlString)o); break;
        case ErlTypeOrder.ErlTuple:     WriteTuple((ErlTuple)o); break;
        default:
          throw new ErlException(
              StringConsts.ERL_UNSUPPORTED_ELEMENT_TYPE_ERROR,
                  o.GetType().Name, o.TypeOrder.ToString());
      }
    }

    /// <summary>
    /// Write one byte to the stream
    /// </summary>
    public void Write(byte b)
    {
      ensureCapacity(1);
      m_Buffer[m_Position++] = b;
    }

    /// <summary>
    /// Write the contents of the stream to an OutputStream
    /// </summary>
    public void WriteTo(Stream os)
    {
      os.Write(m_Buffer, 0, m_Position);
      os.Flush();
    }

    /// <summary>
    /// Write atom to the stream
    /// </summary>
    public void WriteAtom(ErlAtom atom, Encoding encoding = null)
    {
      WriteAtom(atom.Value, encoding);
    }

    /// <summary>
    /// Write string value as atom to the stream
    /// </summary>
    public void WriteAtom(string atom, Encoding encoding = null)
    {
      write(ErlExternalTag.Atom);
      Write2BE(atom.Length);

      var b = (encoding ?? Encoding.ASCII).GetBytes(atom);
      Write(b);
    }

    public static int EncodeSize(IErlObject o)
    {
      switch (o.TypeOrder)
      {
        case ErlTypeOrder.ErlAtom:
          return 1 + 2 + o.ValueAsString.Length;
        case ErlTypeOrder.ErlBinary:
          return 5 + o.ValueAsByteArray.Length;
        case ErlTypeOrder.ErlBoolean:
          return 1 + 2 + (o.ValueAsBool ? ErlConsts.TRUE.Length : ErlConsts.FALSE.Length);
        case ErlTypeOrder.ErlByte:
          return 1 + 1;
        case ErlTypeOrder.ErlDouble:
          return 9;
        case ErlTypeOrder.ErlLong:
          long l1 = o.ValueAsLong;
          if ((l1 & 0xff) == l1) return 2;
          else if ((ErlConsts.ERL_INT_MIN <= l1) && (l1 <= ErlConsts.ERL_INT_MAX)) return 5;
          return longArity(l1);
        case ErlTypeOrder.ErlList:
          var l2 = (ErlList)o;
          if (l2.Count == 0) return 1;
          return 5 + l2.Value.Sum(obj1 => EncodeSize(obj1));
        case ErlTypeOrder.ErlPid:
          var p1 = (ErlPid)o;
          return 1 + (1 + 2 + p1.Node.Length) + 4 + 4 + 1;
        case ErlTypeOrder.ErlPort:
          var p2 = (ErlPort)o;
          return 1 + (1 + 2 + p2.Node.Length) + 4 + 1;
        case ErlTypeOrder.ErlRef:
          var p3 = (ErlRef)o;
          return 1 + (1 + 2 + p3.Node.Length) + 1 + 4 * p3.Ids.Length;
        case ErlTypeOrder.ErlString:
          var l3 = o.ValueAsString;
          if (l3.Length == 0) return 1;
          if (l3.Length < 0xffff) return 2 + l3.Length;
          return 1 + 4 + 2 * l3.Length;
        case ErlTypeOrder.ErlTuple:
          var l4 = (ErlTuple)o;
          int sz = 1 + (l4.Count < 0xff ? 1 : 4);
          return sz + l4.Value.Sum(obj2 => EncodeSize(obj2));
        default:
          throw new ErlException(StringConsts.ERL_UNSUPPORTED_ELEMENT_TYPE_ERROR,
              o.GetType().Name, o.TypeOrder.ToString());
      }
    }

    /// <summary>
    /// Write an array of bytes to the stream as an Erlang binary
    /// </summary>
    public void WriteBinary(ErlBinary bin)
    {
      write(ErlExternalTag.Bin);
      Write4BE(bin.Length);
      Write(bin.Value);
    }

    /// <summary>
    /// Write a boolean value to the stream as the Erlang atom 'true' or 'false'
    /// </summary>
    public void WriteBoolean(ErlBoolean b)
    {
      WriteAtom(b.Value ? ErlAtom.True : ErlAtom.False);
    }

    /// <summary>
    /// Write a single byte to the stream as an Erlang integer
    /// </summary>
    public void WriteByte(ErlByte b)
    {
      writeLong(b.ValueAsLong, false);
    }

    /// <summary>
    /// Write a character to the stream as an Erlang integer
    /// </summary>
    public void WriteChar(char c)
    {
      writeLong(c, false);
    }

    /// <summary>
    /// Write a double value to the stream
    /// </summary>
    public void WriteDouble(ErlDouble d)
    {
      write(ErlExternalTag.NewFloat);

      var data = BitConverter.GetBytes(d);
      if (BitConverter.IsLittleEndian)
        Array.Reverse(data);
      Write(data);
    }

    /// <summary>
    /// Write an Erlang long to the stream
    /// </summary>
    public void WriteLong(ErlLong l) { writeLong(l, l < 0L); }

    /// <summary>
    /// Write a long to stream
    /// </summary>
    public void WriteLong(long l) { writeLong(l, l < 0L); }

    /// <summary>
    /// Write an integer to the stream
    /// </summary>
    public void WriteInt(int i)
    {
      writeLong(i, i < 0);
    }

    /// <summary>
    /// Write Erlang list to stream
    /// </summary>
    public void WriteList(ErlList list)
    {
      int n = list.Count;
      if (n == 0)
        WriteNil();
      else
      {
        var isStr = n < 256 && list.All(i =>
        {
          if (i.TypeOrder != ErlTypeOrder.ErlLong && i.TypeOrder != ErlTypeOrder.ErlByte)
            return false;
          var k = i.ValueAsInt; return k >= 0 && k <= 255;
        });
        WriteListHead(list.Count, isStr);
        if (isStr)
          list.ForEach(t => Write1(t.ValueAsLong));
        else
        {
          list.ForEach(t => Write(t));
          WriteNil();
        }
      }
    }

    /// <summary>
    /// Write an Erlang list header to the stream. After calling this
    /// method, you must write 'arity' elements to the stream followed by
    /// nil, or it will not be possible to decode it later.
    /// </summary>
    public void WriteListHead(int arity, bool isStr = false)
    {
      if (arity == 0)
        WriteNil();
      else if (isStr)
      {
        write(ErlExternalTag.String);
        Write2BE(arity);
      }
      else
      {
        write(ErlExternalTag.List);
        Write4BE(arity);
      }
    }

    /// <summary>
    /// Write an empty Erlang list to the stream
    /// </summary>
    public void WriteNil()
    {
      write(ErlExternalTag.Nil);
    }

    /// <summary>
    /// Write Erlang tuple to stream
    /// </summary>
    public void WriteTuple(ErlTuple tup)
    {
      WriteTupleHead(tup.Count);
      foreach (var t in tup)
        Write(t);
    }

    /// <summary>
    /// Write an Erlang tuple header to the stream. After calling this
    /// method, you must write 'arity' elements to the stream or it will
    /// not be possible to decode it later.
    /// </summary>
    public void WriteTupleHead(int arity)
    {
      if (arity < 0xff)
      {
        write(ErlExternalTag.SmallTuple);
        Write((byte)arity);
      }
      else
      {
        write(ErlExternalTag.LargeTuple);
        Write4BE(arity);
      }
    }

    /// <summary>
    /// Write Erlang tuple to stream
    /// </summary>
    public void WriteTrace(ErlTrace tup)
    {
      WriteTuple(tup);
    }

    /// <summary>
    /// Write an Erlang PID to the stream
    /// </summary>
    public void WritePid(ErlPid pid)
    {
      Debug.Assert(pid != ErlPid.Null);
      write(ErlExternalTag.Pid);
      WriteAtom(pid.Node);
      Write4BE(pid.Id & 0x7fff); // 15 bits
      Write4BE(pid.Serial & 0x1fff); // 13 bits
      Write1((byte)(pid.Creation & 0x3)); // 2 bits
    }

    /// <summary>
    /// Write an Erlang port to the stream
    /// </summary>
    public void WritePort(ErlPort p)
    {
      Debug.Assert(p != ErlPort.Null);
      write(ErlExternalTag.Port);
      WriteAtom(p.Node);
      Write4BE(p.Id & 0x0fffFFFF); // 28 bits
      Write((byte)(p.Creation & 0x3)); // 2 bits
    }

    /// <summary>
    /// Write a new style (R6 and later) Erlang ref to the stream
    /// </summary>
    public void WriteRef(ErlRef r)
    {
      Debug.Assert(r != ErlRef.Null);
      Debug.Assert(r.Ids.Length == 3);
      int arity = (int)(r.Ids.Length);

      // r6 ref
      write(ErlExternalTag.NewRef);

      // how many id values
      Write2BE(arity);

      WriteAtom(r.Node);

      // note: creation BEFORE id in r6 ref
      Write1(r.Creation & 0x3); // 2 bits

      // first int gets truncated to 18 bits
      Write4BE(r.Ids[0]);
      Write4BE(r.Ids[1]);
      Write4BE(r.Ids[2]);
    }

    /// <summary>
    /// Write a string to the stream
    /// </summary>
    public void WriteString(ErlString s, Encoding encoding = null)
    {
      WriteString(s.Value, encoding);
    }

    /// <summary>
    /// Write a string to the stream
    /// </summary>
    public void WriteString(string s, Encoding encoding = null)
    {
      int len = s.Length;

      if (len == 0)
        WriteNil();
      else
      {
        var bytebuf = (encoding ?? Encoding.ASCII).GetBytes(s);
        if (bytebuf.Length < 0xffff)
        {
          write(ErlExternalTag.String);
          Write2BE(len);
          Write(bytebuf);
        }
        else
        {
          WriteListHead(len);
          foreach (var b in bytebuf)
            Write(b);
          WriteNil();
        }
      }
    }

    /// <summary>
    /// Write the low byte of a value to the stream
    /// </summary>
    internal void Write1(long n)
    {
      Write((byte)(n & 0xff));
    }

    /// <summary>
    /// Write the low two bytes of a value to the stream in big endian order
    /// </summary>
    internal void Write2BE(long n)
    {
      ensureCapacity(2);
      m_Buffer[m_Position++] = (byte)((n & 0xff00) >> 8);
      m_Buffer[m_Position++] = (byte)(n & 0xff);
    }

    /// <summary>
    /// Write the low four bytes of a value to the stream in big endian order
    /// </summary>
    internal void Write4BE(long n)
    {
      ensureCapacity(4);
      m_Buffer[m_Position++] = (byte)((n & 0xff000000) >> 24);
      m_Buffer[m_Position++] = (byte)((n & 0xff0000) >> 16);
      m_Buffer[m_Position++] = (byte)((n & 0xff00) >> 8);
      m_Buffer[m_Position++] = (byte)(n & 0xff);
    }

    /// <summary>
    /// Write an array of bytes to the stream
    /// </summary>
    internal void Write(byte[] buf)
    {
      ensureCapacity(buf.Length);
      Array.Copy(buf, 0, this.m_Buffer, m_Position, buf.Length);
      m_Position += (int)buf.Length;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public ErlInputStream InputStream(int offset)
    {
      return new ErlInputStream(m_Buffer, offset, m_Position - offset, false);
    }

  #endregion

  #region .pvt

    /// <summary>
    /// Write the low four bytes of a value to the stream in bif endian
    /// order, at the specified position. If the position specified is
    /// beyond the end of the stream, this method will have no effect
    /// </summary>
    /// <remarks>
    /// Normally this method should be used in conjunction with
    /// <see cref="Position"/>, when is is necessary to insert data into
    /// the stream before it is known what the actual value should be.
    /// For example:
    ///
    /// <pre>
    /// int pos = s.Position;
    /// s.Write4BE(0); // make space for length data,
    /// // but final value is not yet known
    ///
    /// [ ...more write statements...]
    ///
    /// // later... when we know the length value
    /// s.Poke4BE(pos, length);
    /// </pre>
    /// </remarks>
    public void Poke4BE(int offset, int n)
    {
      if (offset >= m_Position - 4)
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR, offset);
      m_Buffer[offset + 0] = ((byte)((n & 0xff000000) >> 24));
      m_Buffer[offset + 1] = ((byte)((n & 0xff0000) >> 16));
      m_Buffer[offset + 2] = ((byte)((n & 0xff00) >> 8));
      m_Buffer[offset + 3] = ((byte)(n & 0xff));
    }

    private void encodeObject(IErlObject o, bool writeVersion)
    {
      if (writeVersion)
        write(ErlExternalTag.Version);
      Write(o);
    }

    private void ensureCapacity(int n)
    {
      if (m_Position + n > m_Capacity)
      {
        byte[] tmp = new byte[m_Capacity + n + DEFAULT_INCREMENT];
        Array.Copy(m_Buffer, 0, tmp, 0, m_Position);
        m_Capacity += n + DEFAULT_INCREMENT;
        m_Buffer = tmp;
      }

    }

    private void write(ErlExternalTag tag)
    {
      Write((byte)tag);
    }

    private void writeLong(long l, bool isNegative)
    {
      if ((l & 0xff) == l)
      {
        // will fit in one byte
        write(ErlExternalTag.SmallInt);
        Write((byte)l);
      }
      else if ((ErlConsts.ERL_INT_MIN <= l) && (l <= ErlConsts.ERL_INT_MAX))
      {
        write(ErlExternalTag.Int);
        Write4BE(l);
      }
      else
      {
        int neg = isNegative ? 1 : 0;
        ulong v = (ulong)(isNegative ? -l : l);
        byte arity = 0;
        write(ErlExternalTag.SmallBigInt);
        int arity_pos = m_Position;
        Write((byte)0);  // Fill in later
        Write((byte)neg); // sign
        while (v != 0)
        {
          Write((byte)(v & 0xff));  // write lowest byte
          v >>= 8;                // shift unsigned
          arity++;
        }
        m_Buffer[arity_pos] = arity;
      }
    }

    private static int longArity(long l)
    {
      int sz = 3; /* Type, arity and sign */
      for (var v = (ulong)(l < 0 ? -l : l); v != 0; v >>= 8)
        sz++;
      return sz;
    }

  #endregion
  }
}