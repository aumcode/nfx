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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Long.cs
using System;

namespace NFX.Erlang
{

  /// <summary>
  /// Provides a C# representation of Erlang integer types
  /// </summary>
  public struct ErlLong : IErlObject<long>
  {
  #region .ctor

    /// <summary>
    /// Create an Erlang integer from the given value
    /// </summary>
    public ErlLong(long val)
    {
      m_Value = val;
    }

    /*
    * Create an Erlang integer from a stream containing an integer
    * encoded in Erlang external format.
    *
    * @param buf the stream containing the encoded value.
    *
    * @exception DecodeException if the buffer does not
    * contain a valid external representation of an Erlang integer.
    **/
    public ErlLong(byte[] buf, ref int offset, int count)
    {
      int idx = offset;
      var tag = (ErlExternalTag)buf[idx++];
      int arity;

      switch (tag)
      {
        case ErlExternalTag.SmallInt:
          if (idx + 1 >= count) throw new NotEnoughDataException();
          m_Value = buf[idx++];
          break;
        case ErlExternalTag.Int:
          if (idx + 4 >= count) throw new NotEnoughDataException();
          m_Value = buf.ReadBEInt32(ref idx);
          break;
        case ErlExternalTag.SmallBigInt:
          if (idx + 2 >= count) throw new NotEnoughDataException();
          arity = buf[idx++];
          goto DECODE_BIG;
        case ErlExternalTag.LargeBigInt:
          if (idx + 5 >= count) throw new NotEnoughDataException();
          arity = buf.ReadBEInt32(ref idx);
        DECODE_BIG:
          int sign = buf[idx++];
          if (idx + arity >= count) throw new NotEnoughDataException();
          if (arity > 8)
            throw new ErlException(StringConsts.ERL_VALUE_TOO_LARGE_FOR_TYPE_ERROR, "long", arity);

          long val = 0;
          for (int i = 0; i < arity; i++)
            val |= (long)buf[idx++] << (i * 8);

          m_Value = sign == 0 ? val : -val; // should deal with overflow
          break;
        default:
          throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR, "int", tag);
      }

      offset = idx;
    }

  #endregion

  #region Fields

    private readonly long m_Value;

  #endregion

  #region Props

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlLong; } }

    public bool IsScalar { get { return true; } }

    public long Value { get { return m_Value; } }

    public object      ValueAsObject   { get { return m_Value; } }
    public int         ValueAsInt      { get { return (int)m_Value; } }
    public long        ValueAsLong     { get { return m_Value; } }
    public decimal     ValueAsDecimal  { get { return m_Value; } }
    public DateTime    ValueAsDateTime { get { return m_Value.FromMicrosecondsSinceUnixEpochStart(); } }
    public TimeSpan    ValueAsTimeSpan { get { return new TimeSpan(0, 0, (int)m_Value); } }
    public double      ValueAsDouble   { get { return m_Value; } }
    public string      ValueAsString   { get { return m_Value.ToString(); } }
    public bool        ValueAsBool     { get { return m_Value != 0; } }
    public char        ValueAsChar     { get { if (m_Value >= 0 && m_Value <= 255) return Convert.ToChar(m_Value);
                                                throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR, m_Value);
                                              } }
    public byte[]      ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

  #endregion

  #region Public

    /// <summary>
    /// Implicit conversion of ErlLong to int
    /// </summary>
    public static explicit operator int  (ErlLong t) { return (int)t.Value; }
    public static implicit operator long (ErlLong t) { return t.Value; }
    public static explicit operator ErlLong(int  t)  { return new ErlLong(t); }
    public static implicit operator ErlLong(long t)  { return new ErlLong(t); }

    public static bool operator ==(ErlLong lhs, ErlLong rhs) { return lhs.Value == rhs.Value; }
    public static bool operator !=(ErlLong lhs, ErlLong rhs) { return lhs.Value != rhs.Value; }

    public static bool operator ==(ErlLong lhs, long d) { return lhs.Value == d; }
    public static bool operator ==(ErlLong lhs, int d)  { return lhs.Value == d; }
    public static bool operator ==(ErlLong lhs, byte d) { return lhs.Value == d; }
    public static bool operator ==(long d, ErlLong lhs) { return lhs.Value == d; }
    public static bool operator ==(int d,  ErlLong lhs) { return lhs.Value == d; }
    public static bool operator ==(byte d, ErlLong lhs) { return lhs.Value == d; }

    public static bool operator !=(ErlLong lhs, long d) { return lhs.Value != d; }
    public static bool operator !=(ErlLong lhs, int d)  { return lhs.Value != d; }
    public static bool operator !=(ErlLong lhs, byte d) { return lhs.Value != d; }
    public static bool operator !=(long d, ErlLong lhs) { return lhs.Value != d; }
    public static bool operator !=(int d,  ErlLong lhs) { return lhs.Value != d; }
    public static bool operator !=(byte d, ErlLong lhs) { return lhs.Value != d; }

    public override string ToString()
    {
      return Value.ToString();
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
      return (o is ErlLong && Equals((ErlLong)o))
          || (o is ErlByte && Equals((ErlByte)o));
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(ErlLong o) { return Value == o.Value; }
    public bool Equals(ErlByte o) { return Value == o.Value; }

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return (int)Value;
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
      if (!(obj is ErlLong))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      ErlLong rhs = (ErlLong)obj;
      return Value.CompareTo(rhs.Value);
    }

    /// <summary>
    /// Clone an instance of the object (non-scalar immutable objects are copied by reference)
    /// </summary>
    public IErlObject Clone() { return this; }  // Scalar Value is immutable
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