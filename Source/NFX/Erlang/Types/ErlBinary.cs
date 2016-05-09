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
// Author:  Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Binary.cs
using System;
using System.Linq;
using System.Text;

namespace NFX.Erlang
{

  /// <summary>
  /// Provides a C# representation of Erlang integer types
  /// </summary>
  public struct ErlBinary : IErlObject<byte[]>
  {
  #region .ctor

    /// <summary>
    /// Create an Erlang binary from the given value
    /// </summary>
    public ErlBinary(byte[] val, bool copy = true)
    {
      Debug.Assert(val != null);
      m_Value = copy ? (byte[])val.Clone() : val;
    }

    /// <summary>
    /// Create an Erlang binary by copying its value from the given buffer
    /// </summary>
    public ErlBinary(byte[] buf, int offset, int count)
    {
      m_Value = new byte[count];
      Array.Copy(buf, offset, m_Value, 0, count);
    }

  #endregion

  #region Fields

    private readonly byte[] m_Value;

  #endregion

  #region Props

    /// <summary>
    /// Return length of the binary byte array
    /// </summary>
    public int Length { get { return m_Value.Length; } }

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlBinary; } }

    public bool IsScalar { get { return false; } }

    public byte[] Value { get { return m_Value; } }

    public object      ValueAsObject   { get { return m_Value; } }
    public int         ValueAsInt      { get { throw new ErlIncompatibleTypesException(this, typeof(int)); } }
    public long        ValueAsLong     { get { throw new ErlIncompatibleTypesException(this, typeof(long)); } }
    public decimal     ValueAsDecimal  { get { throw new ErlIncompatibleTypesException(this, typeof(decimal)); } }
    public DateTime    ValueAsDateTime { get { throw new ErlIncompatibleTypesException(this, typeof(DateTime)); } }
    public TimeSpan    ValueAsTimeSpan { get { throw new ErlIncompatibleTypesException(this, typeof(TimeSpan)); } }
    public double      ValueAsDouble   { get { throw new ErlIncompatibleTypesException(this, typeof(double)); } }
    public string      ValueAsString   { get { return ToString(Encoding.UTF8); } }
    public bool        ValueAsBool     { get { return m_Value.Length > 0; } }
    public char        ValueAsChar     { get { throw new ErlIncompatibleTypesException(this, typeof(char)); } }
    public byte[]      ValueAsByteArray{ get { return m_Value; } }

  #endregion

  #region Public

    /// <summary>
    /// Implicit conversion of ErlBinary to byte[]
    /// </summary>
    public static implicit operator byte[] (ErlBinary t)
    {
      return t.Value;
    }

    public static bool operator ==(ErlBinary lhs, ErlBinary rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ErlBinary lhs, ErlBinary rhs) { return !lhs.Equals(rhs); }

    public override string ToString()
    {
      //return "#Bin<" + m_Value.Length + ">";
      return ToPrintableString();
    }

    public string ToString(Encoding encoding)
    {
      if (m_Value==null) return string.Empty;
      if (encoding==null) encoding = Encoding.UTF8;
      return encoding.GetString(m_Value);
    }

    /// <summary>
    /// Returns binary representation of the string encoded as &lt;&lt;...&gt;&gt;
    /// </summary>
    /// <returns></returns>
    public string ToBinaryString(int maxLen = 0)
    {
      return ToBinaryString(m_Value, 0, m_Value.Length, maxLen);
    }

    /// <summary>
    /// Returns printable binary representation of the string encoded as &lt;&lt;"abc..."&gt;&gt;
    /// </summary>
    /// <returns></returns>
    public string ToPrintableString(int maxLen = 0)
    {
      return ToPrintableString(m_Value, 0, m_Value.Length, maxLen);
    }

    /// <summary>
    /// Convert a byte buffer to printable binary representation (i.e. &lt;&lt;131,15,12,...>>)
    /// </summary>
    /// <param name="buf">Buffer to convert</param>
    /// <param name="offset">Offset in the buffer</param>
    /// <param name="count">Number of bytes to convert</param>
    /// <param name="maxLen">Maximum allowed length of returned string. If given, and
    /// resulting string exceeds this value, it'll be trimmed to this length ending
    /// with "...>>"</param>
    /// <returns></returns>
    public static string ToBinaryString(byte[] buf, int offset, int count, int maxLen = 0)
    {
      /*
      int n = m_Value.Length;
      System.Text.StringBuilder s = new System.Text.StringBuilder();
      s.Append("<<");
      if (n > 0) s.Append(m_Value[0]);
      for (int i = 1; i < n; i++)
          s.AppendFormat(",{0}", m_Value[i]);
      s.Append(">>");
      return s.ToString();
      */
      return buf.ToDumpString(DumpFormat.Decimal, offset, count, maxLen: maxLen);
    }

    /// <summary>
    /// Convert a byte buffer to printable binary representation (i.e. &lt;&lt;"abc...">>)
    /// </summary>
    public static string ToPrintableString(byte[] buf, int offset, int count, int maxLen = 0)
    {
      var printable = true;
      var n = maxLen == 0 ? offset + count : offset + Math.Min(count, maxLen);
      for (int i = offset; i < n; ++i)
      {
        var c = buf[i];
        if (c > 31 && c != 127 || c == 8 || c == 10 || c == 13)
          continue;
        printable = false;
        break;
      }
      return printable
           ? "<<\"{0}\">>".Args(buf.ToDumpString(DumpFormat.Printable, offset, count, maxLen: maxLen))
           : ToBinaryString(buf, offset, count, maxLen);
    }

    /// <summary>
    /// Determine if this instance equals to the object
    /// </summary>
    public override bool Equals(object o)
    {
      return o is IErlObject && Equals((IErlObject)o);
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(IErlObject o)
    {
      return (o is ErlBinary) && Equals((ErlBinary)o);
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(ErlBinary rhs)
    {
      return m_Value.SequenceEqual(rhs.m_Value);
    }

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return m_Value.GetHashCode();
    }

    /// <summary>
    /// Compare this instance to the object.
    /// Negative value means that the value is less than obj, positive - greater than the obj
    /// </summary>
    public int CompareTo(object obj)
    {
      IErlObject o = obj as IErlObject;
      return o == null ? -1 : CompareTo(o);
    }

    /// <summary>
    /// Compare this instance to the IErlObject.
    /// Negative value means that the value is less than obj, positive - greater than the obj
    /// </summary>
    public int CompareTo(IErlObject obj)
    {
      if (!(obj is ErlBinary))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      var rhs = (ErlBinary)obj;
      var n   = m_Value.Length.CompareTo(rhs.Value.Length);
      if (n != 0) return n;

      for (int i = 0; i < m_Value.Length; i++)
        if (m_Value[i] != rhs.Value[i])
          return m_Value[i] < rhs.Value[i] ? -i : i;
      return 0;
    }

    /// <summary>
    /// Clone an instance of the object (non-scalar immutable objects are copied by reference)
    /// </summary>
    public IErlObject Clone() { return new ErlBinary(Value, true); }
    object ICloneable.Clone() { return Clone(); }

    public bool Subst(ref IErlObject term, ErlVarBind binding)
    {
      return false;
    }

    /// <summary>
    /// Execute fun for every nested term
    /// </summary>
    public TAccumulate Visit<TAccumulate>(TAccumulate acc, Func<TAccumulate, IErlObject, TAccumulate> fun)
    {
      return fun(acc, this);
    }

    /// <summary>
    /// Perform pattern match on this Erlang term returning null if match fails
    /// or a dictionary of matched variables bound in the pattern
    /// </summary>
    public ErlVarBind Match(IErlObject pattern)
    {
      return pattern is ErlVar
          ? pattern.Match(this)
          : Equals(pattern) ? new ErlVarBind() : null;
    }

    /// <summary>
    /// Perform pattern match on this Erlang term, storing matched variables
    /// found in the pattern into the binding.
    /// </summary>
    public bool Match(IErlObject pattern, ErlVarBind binding)
    {
      return pattern is ErlVar ? pattern.Match(this, binding) : Equals(pattern);
    }

    /// <summary>
    /// Perform pattern match on this Erlang term without binding any variables
    /// </summary>
    public bool Matches(IErlObject pattern)
    {
      return pattern is ErlVar ? pattern.Matches(this) : Equals(pattern);
    }

  #endregion
  }
}