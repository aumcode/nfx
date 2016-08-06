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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Boolean.cs
using System;

namespace NFX.Erlang
{
  /// <summary>
  /// Provides a C# representation of Erlang floats and doubles. Erlang
  /// defines only one floating point numeric type
  /// </summary>
  public struct ErlBoolean : IErlObject<bool>
  {
  #region .ctor

    /// <summary>
    /// Create an Erlang boolean from the given boolean value.
    /// </summary>
    public ErlBoolean(bool b)
    {
      m_Value = b;
    }

  #endregion

  #region Fields

    private bool m_Value;

  #endregion

  #region Props

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlBoolean; } }

    public bool IsScalar { get { return true; } }

    public bool Value { get { return m_Value; } }

    public object      ValueAsObject   { get { return m_Value; } }
    public int         ValueAsInt      { get { return m_Value ? 1 : 0; } }
    public long        ValueAsLong     { get { return m_Value ? 1 : 0; } }
    public decimal     ValueAsDecimal  { get { return Convert.ToDecimal(m_Value); } }
    public DateTime    ValueAsDateTime { get { throw new ErlIncompatibleTypesException(this, typeof(DateTime)); } }
    public TimeSpan    ValueAsTimeSpan { get { throw new ErlIncompatibleTypesException(this, typeof(TimeSpan)); } }
    public double      ValueAsDouble   { get { return m_Value ? 1.0 : 0.0; } }
    public string      ValueAsString   { get { return m_Value.ToString(); } }
    public bool        ValueAsBool     { get { return m_Value; } }
    public char        ValueAsChar     { get { return m_Value ? 'T' : 'F'; } }
    public byte[]      ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

  #endregion

  #region Public

    /// <summary>
    /// Implicit conversion of ErlBoolean to bool
    /// </summary>
    public static implicit operator bool (ErlBoolean t) { return t.Value; }
    /// <summary>
    /// Implicit conversion of bool to ErlBoolean
    /// </summary>
    public static implicit operator ErlBoolean(bool a) { return new ErlBoolean(a); }

    public static bool operator ==(ErlBoolean lhs, ErlBoolean rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ErlBoolean lhs, ErlBoolean rhs) { return !lhs.Equals(rhs); }

    public override string ToString()
    {
      return m_Value ? ErlConsts.TRUE : ErlConsts.FALSE;
    }

    /// <summary>
    /// Determine if this atom equals to the object
    /// </summary>
    public override bool Equals(object o)
    {
      return o is IErlObject && Equals((IErlObject)o);
    }

    /// <summary>
    /// Determine if two atoms are equal
    /// </summary>
    public bool Equals(IErlObject o)
    {
      return o is ErlBoolean ? Equals((ErlBoolean)o) : false;
    }

    /// <summary>
    /// Determine if two atoms are equal
    /// </summary>
    public bool Equals(ErlBoolean o) { return Value == o.Value; }

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return 1;
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
      if (!(obj is ErlBoolean))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      var rhs = (ErlBoolean)obj;
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