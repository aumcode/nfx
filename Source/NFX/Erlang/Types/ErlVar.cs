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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Var.cs
using System;

namespace NFX.Erlang
{
  /// <summary>
  /// Provides a C# representation of an Erlang variable
  /// </summary>
  public struct ErlVar : IErlObject
  {
  #region .ctor

    public static readonly ErlVar Any = new ErlVar(ConstAtoms.ANY);

    /// <summary>
    /// Create an Erlang named variable
    /// </summary>
    /// <param name="name">Variable name</param>
    public ErlVar(string name) : this(name, ErlTypeOrder.ErlObject) { }

    /// <summary>
    /// Create an Erlang typed named variable
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="type">Value type</param>
    public ErlVar(string name, ErlTypeOrder type)
        : this(name == null ? ConstAtoms.ANY : new ErlAtom(name), type)
    { }

    /// <summary>
    /// Create an Erlang typed named variable using ErlAtom as name
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="type">Value type</param>
    public ErlVar(ErlAtom name, ErlTypeOrder type = ErlTypeOrder.ErlObject)
    {
      m_Name = name;
      ValueType = type;
    }

  #endregion

  #region Fields

    private readonly ErlAtom m_Name;

  #endregion

  #region Props

    /// <summary>
    /// Variable name
    /// </summary>
    public ErlAtom Name { get { return m_Name; } }

    /// <summary>
    /// Type of value stored in this variable
    /// </summary>
    public readonly ErlTypeOrder ValueType;

    public bool IsAny { get { return Name.Equals(ConstAtoms.ANY); } }

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlVar; } }

    public bool IsScalar { get { return true; } }


    public object      ValueAsObject   { get { throw new ErlIncompatibleTypesException(this, typeof(object)); } }
    public int         ValueAsInt      { get { throw new ErlIncompatibleTypesException(this, typeof(int)); } }
    public long        ValueAsLong     { get { throw new ErlIncompatibleTypesException(this, typeof(long)); } }
    public decimal     ValueAsDecimal  { get { throw new ErlIncompatibleTypesException(this, typeof(decimal)); } }
    public DateTime    ValueAsDateTime { get { throw new ErlIncompatibleTypesException(this, typeof(DateTime)); } }
    public TimeSpan    ValueAsTimeSpan { get { throw new ErlIncompatibleTypesException(this, typeof(TimeSpan)); } }
    public double      ValueAsDouble   { get { throw new ErlIncompatibleTypesException(this, typeof(double)); } }
    public string      ValueAsString   { get { throw new ErlIncompatibleTypesException(this, typeof(string)); } }
    public bool        ValueAsBool     { get { throw new ErlIncompatibleTypesException(this, typeof(bool)); } }
    public char        ValueAsChar     { get { throw new ErlIncompatibleTypesException(this, typeof(char)); } }
    public byte[]      ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

  #endregion

  #region Public

    /// <summary>
    /// Get the printable version of the string contained in this object
    /// </summary>
    public override string ToString()
    {
      string tp;
      switch (ValueType)
      {
        case ErlTypeOrder.ErlAtom:    tp = "::atom()";   break;
        case ErlTypeOrder.ErlBinary:  tp = "::binary()"; break;
        case ErlTypeOrder.ErlBoolean: tp = "::bool()";   break;
        case ErlTypeOrder.ErlByte:    tp = "::byte()";   break;
        case ErlTypeOrder.ErlDouble:  tp = "::double()"; break;
        case ErlTypeOrder.ErlLong:    tp = "::int()";    break;
        case ErlTypeOrder.ErlList:    tp = "::list()";   break;
        case ErlTypeOrder.ErlPid:     tp = "::pid()";    break;
        case ErlTypeOrder.ErlPort:    tp = "::port()";   break;
        case ErlTypeOrder.ErlRef:     tp = "::ref()";    break;
        case ErlTypeOrder.ErlString:  tp = "::string()"; break;
        case ErlTypeOrder.ErlTuple:   tp = "::tuple()";  break;
        case ErlTypeOrder.ErlVar:     tp = "::var()";    break;
        default: tp = string.Empty; break;
      }
      return tp == string.Empty
          ? string.Format("{0}", Name.Value)
          : string.Format("{0}{1}", Name.Value, tp);
    }

    public bool Subst(ref IErlObject obj, ErlVarBind binding)
    {
      if (IsAny || binding == null || binding.Empty)
        throw new ErlException(StringConsts.ERL_UNBOUND_VARIABLE_ERROR);

      IErlObject term = binding[Name];
      if (term == null)
        //throw new ErlException(StringConsts.VARIABLE_NOT_FOUND_ERROR, Name);
        return false;
      if (!checkType(term))
        throw new ErlException(StringConsts.ERL_VARIABLE_INVALID_VALUE_TYPE_ERROR,
            Name, term.GetType().Name, ValueType);
      obj = term;
      return true;
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
      var binding = new ErlVarBind();
      return Match(pattern, binding) ? binding : null;
    }

    /// <summary>
    /// Perform pattern match on this Erlang term, storing matched variables
    /// found in the pattern into the binding.
    /// </summary>
    public bool Match(IErlObject pattern, ErlVarBind binding)
    {
      if (binding == null)
        return false;
      IErlObject value = binding[Name];
      if (value != null)
        return checkType(value) ? value.Match(pattern, binding) : false;
      if (!checkType(pattern))
        return false;
      IErlObject term = null;
      binding[Name] = pattern.Subst(ref term, binding) ? term : pattern;
      return true;
    }

    /// <summary>
    /// Match this variable with given pattern merely by performing type check
    /// </summary>
    public bool Matches(IErlObject pattern)
    {
      return pattern.TypeOrder != ErlTypeOrder.ErlVar && checkType(pattern);
    }

    /// <summary>
    /// Determine if this var equals to the object
    /// </summary>
    public override bool Equals(object o)
    {
      return false; // Must always be false in order for pattern matching to work, don't change!
    }

    /// <summary>
    /// Determine if two vars are equal
    /// </summary>
    public bool Equals(IErlObject o)
    {
      return false; // Must always be false in order for pattern matching to work, don't change!
    }

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
      return -1;
    }

    /// <summary>
    /// Compare this instance to the IErlObject.
    /// Negative value means that the atom is less than obj, positive - greater than the obj
    /// </summary>
    public int CompareTo(IErlObject obj)
    {
      return -1;
    }

    /// <summary>
    /// Clone an instance of the object (non-scalar immutable objects are copied by reference)
    /// </summary>
    public IErlObject Clone() { throw new ErlException(StringConsts.ERL_CANNOT_CLONE_INSTANCE_ERROR, GetType().Name); }
    object ICloneable.Clone() { return Clone(); }

  #endregion

  #region .pvt

    private bool checkType(IErlObject value)
    {
      var vt = value.TypeOrder;
      return ValueType == ErlTypeOrder.ErlObject || 
             value.TypeOrder == ErlTypeOrder.ErlAtom && (ErlAtom)value == ErlAtom.Undefined ||
             sameType(vt, ValueType);
    }

    private bool sameType(ErlTypeOrder tp1, ErlTypeOrder tp2)
    {
      return tp1 == tp2
          || tp1 == ErlTypeOrder.ErlLong && tp2 == ErlTypeOrder.ErlByte
          || tp1 == ErlTypeOrder.ErlByte && tp2 == ErlTypeOrder.ErlLong;
    }
  #endregion
  }
}
