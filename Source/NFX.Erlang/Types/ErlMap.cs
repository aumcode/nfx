/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/List.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Erlang
{
  public class ErlMap : IErlObject<IDictionary<IErlObject, IErlObject>>, IDictionary<IErlObject, IErlObject>

  {
    #region Static

    public static readonly ErlMap Empty = new ErlMap();

    #endregion

    #region .ctor

    public ErlMap() : base() {
      m_Items = new Dictionary<IErlObject, IErlObject>();
    }

    /// <summary>
    /// Create an Erlang string from the given string
    /// </summary>
    public ErlMap(IDictionary<IErlObject, IErlObject> items, bool clone = true)
    {
      if (clone)
      {
        m_Items = items.ToDictionary(
            kvp => kvp.Key.Clone(), kvp => kvp.Value.Clone()
        );
      }
      else
      {
        m_Items = items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      }
    }

    public ErlMap(ErlMap map) : this(map.m_Items, true) { }

    #endregion

    #region Public

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlMap; } }

    /// <summary>
    /// Implicit conversion of atom to string
    /// </summary>
    public static implicit operator Dictionary<IErlObject, IErlObject>(ErlMap a)
    {
      return a.m_Items;
    }

    public static bool operator ==(ErlMap lhs, IErlObject rhs)
    {
      return (object)lhs == null ? (object)rhs == null : lhs.Equals(rhs);
    }
    public static bool operator !=(ErlMap lhs, IErlObject rhs) { return !(lhs == rhs); }

    public override bool Equals(object o)
    {
      return o is IErlObject && Equals((IErlObject)o);
    }

    /// <summary>
    /// Determine if two Erlang objects are equal
    /// </summary>
    public bool Equals(IErlObject o)
    {
      if (o is ErlMap)
        return Equals((ErlMap)o);
      return false;
    }

    /// <summary>
    /// Determine if two Erlang lists are equal
    /// </summary>
    public bool Equals(ErlMap o) { return m_Items.SequenceEqual(o.m_Items); }

    public override int GetHashCode()
    {
      return m_Items.GetHashCode();
    }

    public IErlObject Clone()
    {
      return new ErlMap(this);
    }

    public override string ToString()
    {
      var res = new StringBuilder();
      res.Append(OpenBracket);
      res.Append(" ");
      foreach (var kvp in m_Items)
      {
        res.Append(kvp.Key.ToString());
        res.Append(" => ");
        res.Append(kvp.Value.ToString());
        res.Append(", ");
      }
      res.Remove(res.Length - 2, 1);
      res.Append(CloseBracket);
      return res.ToString();
    }

    public bool Match(IErlObject pattern, ErlVarBind binding)
    {
      if (pattern is ErlVar)
        return pattern.Match(this, binding);

      var other = pattern as ErlMap;
      if (other == null)
        return false;

      if (m_Items.Count != other.m_Items.Count)
        return false;

      // TODO: Sensible matching for maps (like in Erlang)
      return false;
    }

    public ErlVarBind Match(IErlObject pattern)
    {
      var binding = new ErlVarBind();
      return Match(pattern, binding) ? binding : null;
    }

    public bool Matches(IErlObject pattern)
    {
      return pattern is ErlVar ? pattern.Matches(this) : Equals(pattern);
    }

    public bool Subst(ref IErlObject term, ErlVarBind binding)
    {
      bool changed = false;

      var r = m_Items.Select(o =>
      {
        IErlObject key = null, value = null;
        if (o.Key.Subst(ref key, binding)) { changed = true; }
        if (o.Value.Subst(ref value, binding)) { changed = true; }
        return new KeyValuePair<IErlObject, IErlObject>(
          key ?? o.Key,
          value ?? o.Value
        );
      }).ToList();

      if (!changed)
        return false;

      term = new ErlMap(r.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
      return true;
    }

    public TAccumulate Visit<TAccumulate>(TAccumulate acc, Func<TAccumulate, IErlObject, TAccumulate> fun)
    {
      foreach (var kvp in m_Items)
      {
        acc = fun(acc, new ErlTuple(kvp.Key, kvp.Value));
      }
      return acc;
    }

    public int CompareTo(object obj)
    {
      throw new NotImplementedException();
    }

    public int CompareTo(IErlObject other)
    {
      throw new NotImplementedException();
    }

    object ICloneable.Clone()
    {
      return new ErlMap(this);
    }

    public bool ContainsKey(IErlObject key)
    {
      return m_Items.ContainsKey(key);
    }

    public void Add(IErlObject key, IErlObject value)
    {
      m_Items.Add(key, value);
    }

    public bool Remove(IErlObject key)
    {
      return m_Items.Remove(key);
    }

    public bool TryGetValue(IErlObject key, out IErlObject value)
    {
      return m_Items.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<IErlObject, IErlObject> item)
    {
      Add(item.Key, item.Value);
    }

    public void Clear()
    {
      m_Items.Clear();
    }

    public bool Contains(KeyValuePair<IErlObject, IErlObject> item)
    {
      return m_Items.Contains(item);
    }

    public void CopyTo(KeyValuePair<IErlObject, IErlObject>[] array, int arrayIndex)
    {
      ICollection<KeyValuePair<IErlObject, IErlObject>> items = m_Items;
      items.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<IErlObject, IErlObject> item)
    {
      ICollection<KeyValuePair<IErlObject, IErlObject>> items = m_Items;
      return items.Remove(item);
    }

    public IEnumerator<KeyValuePair<IErlObject, IErlObject>> GetEnumerator()
    {
      return m_Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable)m_Items).GetEnumerator();
    }

    #endregion

    #region Protected

    protected string OpenBracket { get { return "#{"; } }
    protected string CloseBracket { get { return "}"; } }

    public IDictionary<IErlObject, IErlObject> Value
    {
      get
      {
        return m_Items;
      }
    }

    public bool IsScalar
    {
      get
      {
        return false;
      }
    }

    public object ValueAsObject
    {
      get
      {
        return (object)m_Items;
      }
    }

    public int ValueAsInt
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public long ValueAsLong
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public decimal ValueAsDecimal
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public DateTime ValueAsDateTime
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public TimeSpan ValueAsTimeSpan
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public double ValueAsDouble
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string ValueAsString
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public bool ValueAsBool
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public char ValueAsChar
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public byte[] ValueAsByteArray
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public ICollection<IErlObject> Keys
    {
      get
      {
        return m_Items.Keys;
      }
    }

    public ICollection<IErlObject> Values
    {
      get
      {
        return m_Items.Values;
      }
    }

    public int Count
    {
      get
      {
        return m_Items.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public IErlObject this[IErlObject key]
    {
      get
      {
        return m_Items[key];
      }

      set
      {
        m_Items[key] = value;
      }
    }

    #endregion

    #region Private

    Dictionary<IErlObject, IErlObject> m_Items;

    #endregion
  }
}