/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Tuple.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Erlang
{
  /*
  public interface IVector : IErlObject<List<IErlObject>>
  {
      IErlObject this[int index] { get; }
      int Count { get; }
  }
  */

  public abstract class ErlTupleBase : IErlObject<List<IErlObject>>, IEnumerable<IErlObject>
  {
  #region .ctor

    protected ErlTupleBase() { }

    /// <summary>
    /// Create an Erlang string from the given string
    /// </summary>
    public ErlTupleBase(IErlObject[] items, bool clone = true)
    {
      m_Items = clone ? items.Select(o => o.Clone()).ToList() : new List<IErlObject>(items);
    }

    public ErlTupleBase(List<IErlObject> items, bool clone = true)
    {
      m_Items = clone ? ErlTupleBase.clone(items) : items;
    }

    public ErlTupleBase(ErlTupleBase term)
    {
      m_Items = clone(term.Value);
    }

    public ErlTupleBase(params object[] items)
    {
      if (items == null)
      {
        m_Items = new List<IErlObject>();
        return;
      }

      m_Items = new List<IErlObject>(items.Length);
      foreach (var o in items)
        m_Items.Add(o.ToErlObject());
    }

  #endregion

  #region Fields

    /// <summary>
    /// The index of this atom in the global atom table
    /// </summary>
    protected List<IErlObject> m_Items;

  #endregion

  #region Props

    /// <summary>
    /// Get the list of Erlang terms contained in this object
    /// </summary>
    public List<IErlObject> Value { get { return m_Items; } internal set { m_Items = value; } }

    public abstract ErlTypeOrder TypeOrder { get; }

    /// <summary>
    /// Determines whether the underlying type is scalar or complex (i.e. tuple, list)
    /// </summary>
    public bool IsScalar { get { return false; } }

    public virtual object   ValueAsObject   { get { return m_Items; } }
    public virtual int      ValueAsInt      { get { throw new ErlIncompatibleTypesException(this, typeof(int     )); } }
    public virtual long     ValueAsLong     { get { throw new ErlIncompatibleTypesException(this, typeof(long    )); } }
    public virtual decimal  ValueAsDecimal  { get { throw new ErlIncompatibleTypesException(this, typeof(decimal )); } }
    public virtual DateTime ValueAsDateTime { get { throw new ErlIncompatibleTypesException(this, typeof(DateTime)); } }
    public virtual TimeSpan ValueAsTimeSpan { get { throw new ErlIncompatibleTypesException(this, typeof(TimeSpan)); } }
    public virtual double   ValueAsDouble   { get { throw new ErlIncompatibleTypesException(this, typeof(double  )); } }
    public virtual string   ValueAsString   { get { throw new ErlIncompatibleTypesException(this, typeof(string  )); } }
    public virtual bool     ValueAsBool     { get { throw new ErlIncompatibleTypesException(this, typeof(bool    )); } }
    public virtual char     ValueAsChar     { get { throw new ErlIncompatibleTypesException(this, typeof(char    )); } }
    public virtual byte[]   ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[]  )); } }

  #endregion

  #region Public

    /// <summary>
    /// Return index'th element from the container
    /// </summary>
    public IErlObject this[int index] { get { return m_Items[index]; } }

    /// <summary>
    /// Return number of elements in the container
    /// </summary>
    public int Count { get { return m_Items == null ? 0 : m_Items.Count; } }

    /// <summary>
    /// Return index'th element cast to the given type
    /// </summary>
    public T Cast<T>(int index) { return (T)m_Items[index]; }

    /// <summary>
    /// Add an item to a mutable list. This method must be used only during
    /// list construction phase, since it treats the list as a mutable entity
    /// </summary>
    public void Add(IErlObject term) { m_Items.Add(term); }

    /// <summary>
    /// Get the string representation of the list.
    /// </summary>
    public override string ToString()
    {
      return ToString(false);
    }

    public string ToString(bool noBrakets)
    {
      StringBuilder s = new StringBuilder();
      int arity = m_Items == null ? 0 : m_Items.Count;

      if (!noBrakets)
        s.Append(OpenBracket);

      if (arity > 0)
        s.Append(m_Items[0].ToString());

      for (int i = 1; i < arity; i++)
        s.AppendFormat(",{0}", m_Items[i].ToString());

      if (!noBrakets)
        s.Append(CloseBracket);

      return s.ToString();
    }

    /// <summary>
    /// Determine if this instance is equal to the object
    /// </summary>
    public override bool Equals(object o)
    {
      return (o is IErlObject) && Equals((IErlObject)o);
    }

    /// <summary>
    /// Determine if two Erlang objects are equal
    /// </summary>
    public abstract bool Equals(IErlObject o);

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return m_Items.GetHashCode();
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
      var rhs = obj as ErlTupleBase;

      if (rhs == null)
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      int n = Math.Min(m_Items.Count, rhs.Count);

      for (int i = 0; i < n; i++)
      {
        int m = m_Items[i].CompareTo(rhs[i]);
        if (m != 0)
          return m;
      }

      return m_Items.Count.CompareTo(rhs.Count);
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
      if (pattern is ErlVar)
        return pattern.Match(this, binding);

      var rhs = pattern as ErlTupleBase;

      if (rhs == null || GetType() != pattern.GetType() || Count != rhs.Count)
        return false;

      return !m_Items.SkipWhile((o, i) => o.Match(rhs[i], binding)).Any();
    }

    /// <summary>
    /// Perform pattern match on this Erlang term without binding any variables
    /// </summary>
    public bool Matches(IErlObject pattern)
    {
      return pattern is ErlVar ? pattern.Matches(this) : Equals(pattern);
    }

    public bool Subst(ref IErlObject term, ErlVarBind binding)
    {
      bool changed = false;

      if (m_Items == null)
        return false;

      var r = m_Items.Select(o =>
      {
        IErlObject obj = null;
        if (o.Subst(ref obj, binding)) { changed = true; return obj; }
        return o;
      }).ToList();

      if (!changed)
        return false;

      term = makeInstance(r);
      return true;
    }

    /// <summary>
    /// Execute fun for every nested term
    /// </summary>
    public TAccumulate Visit<TAccumulate>(TAccumulate acc, Func<TAccumulate, IErlObject, TAccumulate> fun)
    {
      return m_Items == null ? fun(acc, this) : m_Items.Aggregate(acc, (a, o) => o.Visit(a, fun));
    }

    /// <summary>
    /// Clone an instance of the object (non-scalar immutable objects are copied by reference)
    /// </summary>
    public IErlObject Clone()
    {
      var r = m_Items.Select(o => o.Clone()).ToList();
      return makeInstance(r);
    }

    object ICloneable.Clone() { return Clone(); }

    IEnumerator<IErlObject> IEnumerable<IErlObject>.GetEnumerator()
    {
      return ((IEnumerable<IErlObject>)m_Items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_Items.GetEnumerator();
    }

  #endregion

  #region Protected

    protected abstract char OpenBracket { get; }
    protected abstract char CloseBracket { get; }

    protected abstract ErlTupleBase MakeInstance();

  #endregion

  #region .pvt

    private ErlTupleBase makeInstance(List<IErlObject> items)
    {
      var result = MakeInstance();
      result.Value = items;
      return result;
    }

    private static List<IErlObject> clone(IList<IErlObject> list)
    {
      return list.Select(o => o.Clone()).ToList();
    }

  #endregion
  }
}