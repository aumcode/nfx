/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
// Author:   Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/String.cs
using System;
using System.Linq;
using System.Text;

namespace NFX.Erlang
{
  public struct ErlString : IErlObject<string>
  {
  #region .ctor

    /// <summary>
    /// Create an Erlang string from the given string
    /// </summary>
    public ErlString(string val)
    {
      if (val == null)
        throw new ErlException(StringConsts.ERL_VALUE_MUST_NOT_BE_NULL_ERROR);

      m_Value = val;
    }

  #endregion

  #region Fields

    private readonly string m_Value;

  #endregion

  #region Props

    /// <summary>
    /// Get the actual string contained in this object
    /// </summary>
    public string Value { get { return m_Value; } }

    /// <summary>
    /// Returns true if Value is a non-empty string
    /// </summary>
    public bool Empty { get { return m_Value.Length == 0; } }

    /// <summary>
    /// Length of the string
    /// </summary>
    public int Length { get { return m_Value.Length; } }

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlString; } }

    /// <summary>
    /// Determines whether the underlying type is scalar or complex (i.e. tuple, list)
    /// </summary>
    public bool IsScalar { get { return true; } }

    public object       ValueAsObject   { get { return Value; } }
    public int          ValueAsInt      { get { return int.Parse(m_Value); } }
    public long         ValueAsLong     { get { return long.Parse(m_Value); } }
    public decimal      ValueAsDecimal  { get { return decimal.Parse(m_Value); } }
    public DateTime     ValueAsDateTime { get { return DateTime.Parse(m_Value); } }
    public TimeSpan     ValueAsTimeSpan { get { return TimeSpan.Parse(m_Value); } }
    public double       ValueAsDouble   { get { return double.Parse(m_Value); } }
    public string       ValueAsString   { get { return Value; } }
    public bool         ValueAsBool     { get { return m_Value == ErlConsts.TRUE; } }
    public char         ValueAsChar     { get { var s = Value;
                                                if (s.Length != 1) throw new ErlException(StringConsts.ERL_INVALID_VALUE_LENGTH_ERROR, s.Length);
                                                return s[0];
                                              } }
    public byte[]       ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

  #endregion

  #region Public

    /// <summary>
    /// Implicit conversion of ErlString to string
    /// </summary>
    public static implicit operator string (ErlString a) { return a.Value; }
    /// <summary>
    /// Implicit conversion of string to ErlString
    /// </summary>
    public static implicit operator ErlString(string a) { return new ErlString(a); }


    public static bool operator ==(ErlString lhs, ErlString rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ErlString lhs, ErlString rhs) { return !lhs.Equals(rhs); }

    public static bool operator ==(ErlString lhs, string rhs) { return lhs.Value == rhs; }
    public static bool operator ==(string rhs, ErlString lhs) { return lhs.Value == rhs; }

    public static bool operator !=(ErlString lhs, string rhs) { return lhs.Value != rhs; }
    public static bool operator !=(string rhs, ErlString lhs) { return lhs.Value != rhs; }

    /// <summary>
    /// Get the printname of the atom represented by this object. The
    /// difference between this method and {link #atomValue atomValue()}
    /// is that the printname is quoted and escaped where necessary,
    /// according to the Erlang rules for atom naming
    /// </summary>
    public override string ToString()
    {
      return ToString(true);
    }

    public string ToString(bool withQuotes)
    {
      var n = m_Value.Length-1;
      var printable = m_Value.All(c => (byte)c >= 10);
      if (!printable)
      {
        var s = new StringBuilder();
        s.Append('[');
        m_Value.ForEach((c, i) => { s.Append((byte)c); if (i != n) s.Append(','); });
        s.Append(']');
        return s.ToString();
      }
      return withQuotes ? string.Format("\"{0}\"", m_Value) : m_Value;
    }

    /// <summary>
    /// Determine if this atom equals to the object
    /// </summary>
    public override bool Equals(object o)
    {
      return (o is ErlString) && Equals((ErlString)o);
    }

    /// <summary>
    /// Determine if two Erlang objects are equal
    /// </summary>
    public bool Equals(IErlObject o)
    {
      return (o is ErlString) && Equals((ErlString)o);
    }

    /// <summary>
    /// Determine if two Erlang objects are equal
    /// </summary>
    public bool Equals(ErlString rhs)
    {
      return string.Equals(m_Value, rhs.Value, StringComparison.InvariantCulture);
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
    /// Negative value means that the atom is less than obj, positive - greater than the obj
    /// </summary>
    public int CompareTo(object obj)
    {
      IErlObject o = obj as IErlObject;
      return o == null ? -1 : CompareTo(o);
    }

    /// <summary>
    /// Compare this instance to the IErlObject.
    /// Negative value means that the atom is less than obj, positive - greater than the obj
    /// </summary>
    public int CompareTo(IErlObject obj)
    {
      if (!(obj is ErlString))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      var rhs = (ErlString)obj;
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