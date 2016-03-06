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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NFX.Erlang
{
    /// <summary>
    /// Provides a stream for decoding Erlang terms from external format
    /// </summary>
    public class ErlInputStream : Stream
    {
    #region .ctor

      public ErlInputStream(byte[] buf, bool checkVersion = true, Encoding encoding = null)
          : this(buf, 0, buf.Length, checkVersion, encoding)
      {}

      public ErlInputStream(byte[] buf, int offset, int length,
          bool checkVersion = true, Encoding encoding = null)
      {
        m_Buffer    = buf;
        m_Origin    = offset;
        m_Position  = offset;
        m_End       = offset + length;
        m_Encoding  = encoding ?? Encoding.ASCII;

        Debug.Assert(length > 0);

        if (checkVersion)
            CheckVersion();
      }

    #endregion

    #region Fields

      private readonly byte[]     m_Buffer;
      private readonly int        m_Origin;
      private readonly int        m_End;
      private int                 m_Position;
      private readonly Encoding   m_Encoding;

    #endregion

    #region Props

      public override bool CanRead    { get { return true; } }
      public override bool CanSeek    { get { return false; } }
      public override bool CanWrite   { get { return false; } }

      public override void Flush()    { return; }

      public override long Length     { get { return m_End - m_Origin; } }
      public override long Position
      {
        get { return m_Position; }
        set { m_Position = IntMath.MinMax(m_Origin, (int)value, m_End); }
      }

    #endregion

    #region Public / Internal

      internal void CheckVersion()
      {
        checkSpace(1);
        if ((ErlExternalTag)m_Buffer[m_Position] != ErlExternalTag.Version)
          throw new ErlException(
              StringConsts.ERL_INVALID_DATA_FORMAT_ERROR, m_Buffer[m_Position]);
        m_Position++;
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
        int n = Math.Min(count, m_End - m_Position);
        Buffer.BlockCopy(m_Buffer, m_Position, buffer, offset, n);
        m_Position += n;
        return n;
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        throw new NotImplementedException();
      }

      public override void SetLength(long value)
      {
        throw new NotImplementedException();
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Read arbitraty Erlang term from the stream, optionally checking protocol
      /// version byte
      /// </summary>
      public IErlObject Read(bool checkVersion)
      {
        if (checkVersion)
          CheckVersion();
        return Read();
      }

      /// <summary>
      /// Read arbitraty Erlang term from the stream
      /// </summary>
      public IErlObject Read()
      {
        checkSpace(1);
        switch ((ErlExternalTag)m_Buffer[m_Position])
        {
          case ErlExternalTag.SmallAtom:
          case ErlExternalTag.SmallAtomUtf8:
          case ErlExternalTag.Atom:
          case ErlExternalTag.AtomUtf8:
            return ReadAtomOrBool();
          case ErlExternalTag.Bin:
            return ReadBinary();
          case ErlExternalTag.SmallInt:
          case ErlExternalTag.Int:
          case ErlExternalTag.SmallBigInt:
          case ErlExternalTag.LargeBigInt:
            return ReadByteOrLong();
          case ErlExternalTag.String:
            return ReadString();
          case ErlExternalTag.Float:
          case ErlExternalTag.NewFloat:
            return ReadDouble();
          case ErlExternalTag.List:
          case ErlExternalTag.Nil:
            return ReadList();
          case ErlExternalTag.SmallTuple:
          case ErlExternalTag.LargeTuple:
            return ReadTuple();
          case ErlExternalTag.Ref:
          case ErlExternalTag.NewRef:
            return ReadRef();
          case ErlExternalTag.Port:
            return ReadPort();
          case ErlExternalTag.Pid:
            return ReadPid();
          default:
            throw new ErlException(
                StringConsts.ERL_UNSUPPORTED_TERM_TYPE_ERROR,
                    m_Buffer[m_Position]);
        }
      }

      /// <summary>
      /// Read an Erlang byte from the stream
      /// </summary>
      public new ErlByte ReadByte()
      {
        readTag(ErlExternalTag.SmallInt);
        return new ErlByte((byte)Read1());
      }

      /// <summary>
      /// Read an Erlang atom from the stream and interpret the value as a boolean
      /// </summary>
      public ErlBoolean ReadBoolean()
      {
        var a = ReadAtom();
        switch (a.Index)
        {
          case AtomTable.TRUE_INDEX: return new ErlBoolean(true);
          case AtomTable.FALSE_INDEX: return new ErlBoolean(false);
          default:
            throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR,
                a.Value);
        }
      }

      /// <summary>
      /// Read an Erlang atom from the stream
      /// </summary>
      public ErlAtom ReadAtom()
      {
        var tag = readTag();
        int len;
        Encoding enc;

        switch (tag)
        {
          case ErlExternalTag.Atom:
            len = Read2BE();
            enc = Encoding.ASCII;
            break;
          case ErlExternalTag.SmallAtom:
            len = Read1();
            enc = Encoding.ASCII;
            break;
          case ErlExternalTag.AtomUtf8:
            len = Read2BE();
            enc = Encoding.UTF8;
            break;
          case ErlExternalTag.SmallAtomUtf8:
            len = Read1();
            enc = Encoding.UTF8;
            break;
          default:
            throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR,
                "atom", tag.ToString());
        }

        checkSpace(len);
        int n = Math.Min(len, AtomTable.MAX_ATOM_LEN);
        string s = enc.GetString(m_Buffer, m_Position, n);
        m_Position += len;
        return new ErlAtom(s);
      }

      public IErlObject ReadAtomOrBool()
      {
        var a = ReadAtom();
        switch (a.Index)
        {
          case AtomTable.TRUE_INDEX: return new ErlBoolean(true);
          case AtomTable.FALSE_INDEX: return new ErlBoolean(false);
          default: return a;
        }
      }

      /// <summary>
      /// Read an Erlang binary from the stream
      /// </summary>
      public ErlBinary ReadBinary()
      {
        readTag(ErlExternalTag.Bin);

        int len = Read4BE();
        checkSpace(len);
        var bin = new ErlBinary(m_Buffer, m_Position, len);
        m_Position += len;
        return bin;
      }

      /// <summary>
      /// Read an Erlang float from the stream
      /// </summary>
      public ErlDouble ReadDouble()
      {
        var tag = readTag();
        double value;

        switch (tag)
        {
          case ErlExternalTag.NewFloat:
            checkSpace(8);
            // IEEE 754 decoder
            byte[] b;
            int offset;
            if (BitConverter.IsLittleEndian)
            {
              b = new byte[8];
              offset = 0;
              Array.Copy(m_Buffer, m_Position, b, 0, b.Length);
              Array.Reverse(b);
            }
            else
            {
              b = m_Buffer;
              offset = m_Position;
            }
            value = BitConverter.ToDouble(b, offset);
            m_Position += 8;
            break;

          case ErlExternalTag.Float:
            checkSpace(31);
            var s = m_Encoding.GetString(m_Buffer, m_Position, 31);
            if (!double.TryParse(s, out value))
              throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR, s);
            m_Position += 31;
            break;

          default:
            throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR,
                "double", tag.ToString());
        }

        return new ErlDouble(value);
      }

      public IErlObject ReadByteOrLong()
      {
        var t = ReadLong();
        return t.ValueAsLong >= 0 && t.ValueAsLong < 256
            ? (IErlObject)(new ErlByte((byte)t.ValueAsLong))
            : t;
      }

      public ErlLong ReadLong()
      {
        var tag = readTag();
        int arity;
        long value;
        switch (tag)
        {
          case ErlExternalTag.SmallInt:
            value = Read1();
            break;

          case ErlExternalTag.Int:
            value = Read4BE();
            break;

          case ErlExternalTag.SmallBigInt:
            arity = Read1();
            goto DECODE_BIG;

          case ErlExternalTag.LargeBigInt:
            arity = Read4BE();

          DECODE_BIG:
            var sign = Read1();
            if (arity > 8)
              throw new ErlException
                (StringConsts.ERL_VALUE_TOO_LARGE_FOR_TYPE_ERROR, "long", arity);
            checkSpace(arity);
            ulong v = 0;

            for (int i = 0; i < arity; i++)
            {
              if (i < 8)
                v |= (ulong)m_Buffer[m_Position++] << (i * 8);
              else if (m_Buffer[m_Position++] != 0)
                throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR,
                    m_Buffer[m_Position - 1]);
            }
            // check for overflow
            if (sign == 1)
            {
              //if (v > 0x8000000000000000UL)
              //  throw new ErlException(StringConsts.ERL_BIG_INTEGER_OVERFLOW_ERROR);
              value = -(long)v;
            }
            else
            {
              //if (v > 0x7FFFFFFFFFFFFFFFUL)
              //  throw new ErlException(StringConsts.ERL_BIG_INTEGER_OVERFLOW_ERROR);
              value = (long)v;
            }
            break;

          default:
            throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR,
                tag.ToString());
        }

        return new ErlLong(value);
      }

      /// <summary>
      /// Read Erlang list from the stream
      /// </summary>
      public ErlList ReadList()
      {
        int arity = readListHead();
        if (arity == 0)
          return new ErlList();

        var list = new List<IErlObject>(arity);
        for (int i = 0; i < arity; i++)
          list.Add(Read());

        var next = Peek();
        if (next.Item1 && (ErlExternalTag)next.Item2 == ErlExternalTag.Nil)
          readTag(ErlExternalTag.Nil);

        return new ErlList(list, false);
      }

      /// <summary>
      /// Read Erlang tuple from the stream
      /// </summary>
      public ErlTuple ReadTuple()
      {
        int arity = readTupleHead();
        var list = new List<IErlObject>(arity);
        for (int i = 0; i < arity; i++)
          list.Add(Read());
        var res = new ErlTuple();
        res.Value = list;
        return res;
      }

      /// <summary>
      /// Read Erlang pid from the stream
      /// </summary>
      public ErlPid ReadPid()
      {
        readTag(ErlExternalTag.Pid);
        var node    = ReadAtom();
        var id      = Read4BE() & 0x7fff; // 15 bits
        var serial  = Read4BE() & 0x1fff; // 13 bits
        var creation= Read1()   & 0x03;   // 2 bits

        return new ErlPid(node, id, serial, creation);
      }

      /// <summary>
      /// Read Erlang port from the stream
      /// </summary>
      public ErlPort ReadPort()
      {
        readTag(ErlExternalTag.Port);
        var node    = ReadAtom();
        var id      = Read4BE() & 0x0fffffff; // 28 bits
        var creation= Read1() & 0x03; // 2 bits

        return new ErlPort(node, id, creation);
      }

      /// <summary>
      /// Read Erlang reference from the stream
      /// </summary>
      public ErlRef ReadRef()
      {
        var tag = readTag();

        switch (tag)
        {
          case ErlExternalTag.Ref:
            var node1       = ReadAtom();
            var id          = Read4BE() & 0x3ffff; // 18 bits
            var creation1   = Read1() & 0x03; // 2 bits
            return new ErlRef(node1, id, 0, 0, creation1);

          case ErlExternalTag.NewRef:
            var arity       = Read2BE();
            var node2       = ReadAtom();
            var creation2   = Read1() & 0x03; // 2 bits
            var ids = new int[arity];
            for(int i=0; i < arity; i++)
                ids[i] = Read4BE();
            return new ErlRef(node2, ids, creation2);

          default:
            throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR,
              "ref", tag.ToString());
        }
      }

      /// <summary>
      /// Read Erlang string or list from the stream
      /// </summary>
      /// <returns></returns>
      public ErlString ReadString()
      {
        var tag = readTag();
        string s;

        switch (tag)
        {
          case ErlExternalTag.String:
            var len   = Read2BE();
            s = m_Encoding.GetString(m_Buffer, m_Position, len);
            m_Position += len;
            break;

          case ErlExternalTag.Nil:
            s = string.Empty;
            break;

          case ErlExternalTag.List:
            var n = Read4BE();
            var b = new byte[n];
            for (int i = 0; i < n; i++)
              b[i] = Read1();
            readTag(ErlExternalTag.Nil);
            s = m_Encoding.GetString(b);
            break;

          default:
            throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR,
                "string", tag.ToString());
        }
        return new ErlString(s);
      }

      public override string ToString()
      {
        return "byte<{0}>: {1}".Args(Length, ToBinaryString(0, (int)Length, 256));
      }

      public string ToBinaryString(int offset = 0, int count = -1, int maxLen = 0)
      {
        return m_Buffer.ToDumpString(DumpFormat.Decimal, offset,
            (int)(count < 0 ? Length - offset : Math.Min(count, Length - offset)), maxLen: maxLen);
      }

    #endregion

    #region Internal

      internal Tuple<bool, byte> Peek()
      {
        return m_Position < m_End ? Tuple.Create(true, m_Buffer[m_Position]) : Tuple.Create(false, (byte)0);
      }

      internal byte Read1()
      {
        checkSpace(1);
        return m_Buffer[m_Position++];
      }

      internal int Read2BE()
      {
        checkSpace(2);
        return ((((int)m_Buffer[m_Position++] << 8) & 0xff00)
             + ((int)m_Buffer[m_Position++] & 0xff));
      }

      internal int Read4BE()
      {
        checkSpace(4);
        return (int)((((int)m_Buffer[m_Position++] << 24) & 0xff000000)
                   + (((int)m_Buffer[m_Position++] << 16) & 0xff0000)
                   + (((int)m_Buffer[m_Position++] << 8) & 0xff00)
                   + ((int)m_Buffer[m_Position++] & 0xff));
      }

      internal void ReadN(byte[] buf)
      {
        if (Read(buf, 0, buf.Length) != buf.Length)
          throw new ErlException(StringConsts.ERL_CANNOT_READ_FROM_STREAM_ERROR);
      }

      internal string ReadBytesAsString(int len = 0, Encoding encoding = null)
      {
        if (len == 0)
          len = m_End - m_Position;
        checkSpace(len);
        var s = (encoding ?? Encoding.ASCII).GetString(m_Buffer, m_Position, len);
        m_Position += len;
        return s;
      }

  #endregion

  #region .pvt

    /// <summary>
    /// Reads list head from the stream
    /// </summary>
    /// <returns>Arity of the list</returns>
    private int readListHead()
    {
      var tag = readTag();

      switch (tag)
      {
        case ErlExternalTag.Nil:
          return 0;
        case ErlExternalTag.String:
          return Read2BE();
        case ErlExternalTag.List:
          return Read4BE();
        default:
          throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR,
              "list", tag.ToString());
      }
    }

    /// <summary>
    /// Read tuple header from stream
    /// </summary>
    /// <returns>Arity of the tuple</returns>
    private int readTupleHead()
    {
      var tag = readTag();

      switch (tag)
      {
        case ErlExternalTag.SmallTuple:
          return Read1();
        case ErlExternalTag.LargeTuple:
          return Read4BE();
        default:
          throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR,
              "tuple", tag.ToString());
      }
    }

    private void readTag(ErlExternalTag tag)
    {
      var t = (ErlExternalTag)Read1();
      if (t != tag)
        throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR,
            tag.ToString(), t.ToString());
    }

    private ErlExternalTag readTag()
    {
      return (ErlExternalTag)Read1();
    }

    private void checkSpace(int n)
    {
      if (m_Position + n > m_End)
        throw new NotEnoughDataException();
    }

  #endregion
  }
}
