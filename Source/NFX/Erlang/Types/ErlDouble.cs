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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Double.cs
using System;

namespace NFX.Erlang
{
  /// <summary>
  /// Provides a C# representation of Erlang floats and doubles. Erlang
  /// defines only one floating point numeric type
  /// </summary>
  public struct ErlDouble : IErlObject<double>
  {
  #region .ctor

    /// <summary>
    /// Create an Erlang float from the given double value.
    /// </summary>
    public ErlDouble(double d)
    {
      m_Value = d;
    }

  #endregion

  #region Fields

    private double m_Value;

  #endregion

  #region Props

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlDouble; } }

    public bool IsScalar { get { return true; } }

    public double Value { get { return m_Value; } }

    public object      ValueAsObject   { get { return m_Value; } }
    public int         ValueAsInt      { get { return (int)m_Value; } }
    public long        ValueAsLong     { get { return (long)m_Value; } }
    public decimal     ValueAsDecimal  { get { return Convert.ToDecimal(m_Value); } }
    public DateTime    ValueAsDateTime { get { return ((long)m_Value).FromMicrosecondsSinceUnixEpochStart(); } }
    public TimeSpan    ValueAsTimeSpan { get { return new TimeSpan(0, 0, (int)m_Value, (int)(m_Value - (int)m_Value)); } }
    public double      ValueAsDouble   { get { return m_Value; } }
    public string      ValueAsString   { get { return m_Value.ToString(); } }
    public bool        ValueAsBool     { get { return m_Value != 0; } }
    public char        ValueAsChar     { get { throw new ErlIncompatibleTypesException(this, typeof(char)); } }
    public byte[]      ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

  #endregion

  #region Public

    /// <summary>
    /// Implicit conversion of ErlLong to double
    /// </summary>
    public static implicit operator double (ErlDouble t) { return t.Value; }
    public static implicit operator ErlDouble(double t) { return new ErlDouble(t); }

    public static bool operator ==(ErlDouble lhs, ErlDouble rhs) { return lhs.Value == rhs.Value; }
    public static bool operator !=(ErlDouble lhs, ErlDouble rhs) { return lhs.Value != rhs.Value; }

    public static bool operator ==(ErlDouble lhs, double rhs) { return lhs.Value == rhs; }
    public static bool operator ==(double lhs, ErlDouble rhs) { return lhs == rhs.Value; }
    public static bool operator !=(ErlDouble lhs, double rhs) { return lhs.Value != rhs; }
    public static bool operator !=(double lhs, ErlDouble rhs) { return lhs != rhs.Value; }

    public override string ToString()
    {
      return m_Value.ToString();
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
      return o is ErlDouble ? Equals((ErlDouble)o) : false;
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(ErlDouble o, double precision = 0d)
    {
      return Math.Abs(Value - o.Value) <= precision;
    }

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return (int)m_Value;
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
      if (!(obj is ErlDouble))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      var rhs = (ErlDouble)obj;
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