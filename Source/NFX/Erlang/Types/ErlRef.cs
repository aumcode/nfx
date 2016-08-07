/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Pid.cs
using System;
using System.Linq;

namespace NFX.Erlang
{
  /// <summary>
  /// Provides a C# representation of Erlang integer types
  /// </summary>
  public struct ErlRef : IErlObject<ErlRef>
  {
  #region Static

    /// <summary>
    /// Special value non-existing Pid used for "null" ref comparison
    /// </summary>
    public static readonly ErlRef Null = new ErlRef(string.Empty, 0, 0, 0, 0);

  #endregion

  #region .ctor

    public ErlRef(string node, int id1, int id2, int id3, int creation)
      : this(new ErlAtom(node), new uint[] { (uint)id1, (uint)id2, (uint)id3 }, creation)
    { }

    public ErlRef(ErlAtom node, int id1, int id2, int id3, int creation)
      : this(node, new uint[] { (uint)id1, (uint)id2, (uint)id3 }, creation)
    { }

    /// <summary>
    /// Create an Erlang reference from the given values
    /// </summary>
    public ErlRef(string node, int[] ids, int creation)
      : this(new ErlAtom(node), ids, creation)
    { }

    /// <summary>
    /// Create an Erlang reference from the given values
    /// </summary>
    public ErlRef(ErlAtom node, int[] ids, int creation)
      : this(node, ids.Select(i => (uint)i).ToArray(), creation)
    { }

    /// <summary>
    /// Create an Erlang reference from the given values
    /// </summary>
    public ErlRef(ErlAtom node, uint[] ids, int creation)
    {
      Node = node;
      Creation = creation & 0x3;
      if (ids.Length == 3)
        Ids = ids;
      else
      {
        Ids = new uint[3];
        int i = 0;
        for (int n = Math.Min(ids.Length, 3); i < n; i++)
          Ids[i] = ids[i];
        for (; i < 3; i++)
          Ids[i] = 0;
      }
    }

  #endregion

  #region Fields

    public readonly ErlAtom Node;
    public readonly uint[] Ids;
    public readonly int Creation;

  #endregion

  #region Props

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlRef; } }

    public bool IsScalar { get { return true; } }

    public bool Empty { get { return Equals(Null); } }

    public ErlRef Value { get { return this; } }

    public object      ValueAsObject   { get { return this; } }
    public int         ValueAsInt      { get { throw new ErlIncompatibleTypesException(this, typeof(int)); } }
    public long        ValueAsLong     { get { throw new ErlIncompatibleTypesException(this, typeof(long)); } }
    public decimal     ValueAsDecimal  { get { throw new ErlIncompatibleTypesException(this, typeof(decimal)); } }
    public DateTime    ValueAsDateTime { get { throw new ErlIncompatibleTypesException(this, typeof(DateTime)); } }
    public TimeSpan    ValueAsTimeSpan { get { throw new ErlIncompatibleTypesException(this, typeof(TimeSpan)); } }
    public double      ValueAsDouble   { get { throw new ErlIncompatibleTypesException(this, typeof(double)); } }
    public string      ValueAsString   { get { return ToString(); } }
    public bool        ValueAsBool     { get { throw new ErlIncompatibleTypesException(this, typeof(bool)); } }
    public char        ValueAsChar     { get { throw new ErlIncompatibleTypesException(this, typeof(char)); } }
    public byte[]      ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

  #endregion

  #region Public

    public static bool operator ==(ErlRef lhs, ErlRef rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ErlRef lhs, ErlRef rhs) { return !lhs.Equals(rhs); }


    public static ErlRef Parse(string eref)
    {
      ErlRef result;
      if (TryParse(eref, out result)) return result;

      throw new ErlException(StringConsts.ARGUMENT_ERROR + "ErlRef.Parse('{0}')".Args(eref));
    }

    public static bool TryParse(string eref, out ErlRef erlRef)
    {
      erlRef = ErlRef.Null;
      if (eref.IsNullOrWhiteSpace() || eref.Length < 7) return false;

      if (eref[0] == '#') eref = eref.Substring(5, eref.Length - 5 - 1);

      var segs = eref.Split('.');
      if (segs.Length < 5) return false;

      var node = segs[0];
      if (node.IsNullOrWhiteSpace()) return false;

      int id1;
      if (!Int32.TryParse(segs[1], out id1)) return false;

      int id2;
      if (!Int32.TryParse(segs[2], out id2)) return false;

      int id3;
      if (!Int32.TryParse(segs[3], out id3)) return false;

      int creation;
      if (!Int32.TryParse(segs[4], out creation)) return false;

      erlRef = new ErlRef(node, id1, id2, id3, creation);
      return true;
    }

    public override string ToString()
    {
      return string.Format("#Ref<{0}.{1}.{2}.{3}.{4}>", Node, Ids[0], Ids[1], Ids[2], Creation);
    }

    /// <summary>
    /// Determine if this instance equals to the object
    /// </summary>
    public override bool Equals(object o)
    {
      return (o is ErlRef) && Equals((ErlRef)o);
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(IErlObject o)
    {
      return (o is ErlRef) && Equals((ErlRef)o);
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(ErlRef rhs)
    {
      if (Creation != rhs.Creation) return false;
      for (int i = 0; i < Ids.Length; i++)
        if (Ids[i] != rhs.Ids[i]) return false;

      return Node.Equals(rhs.Node);
    }

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return Node.GetHashCode() * Ids.GetHashCode();
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
      if (!(obj is ErlRef))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      var rhs = (ErlRef)obj;
      int n = Node.CompareTo(rhs.Node);
      if (n != 0) return n;
      for (int i = 0; i < Ids.Length; i++)
      {
        int m = Ids[i].CompareTo(rhs.Ids[i]);
        if (m != 0) return m;
      }
      return Creation.CompareTo(rhs.Creation);
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