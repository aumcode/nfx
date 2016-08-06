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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/VarBind.cs
using System.Collections.Generic;
using System.Collections;

namespace NFX.Erlang
{
  /// <summary>
  /// Dictionary of variables bound to terms
  /// </summary>
  public class ErlVarBind : IEnumerable<KeyValuePair<ErlAtom, IErlObject>>, IEnumerable
  {
  #region Fields

    private Dictionary<ErlAtom, IErlObject> m_Dict;

    private static readonly Dictionary<ErlAtom, IErlObject>.Enumerator
        s_EmptyDictEnum = new Dictionary<ErlAtom, IErlObject>().GetEnumerator();

  #endregion

  #region Public

    public IErlObject this[string name] { get { return this[new ErlAtom(name)]; } }
    public IErlObject this[ErlVar name] { get { return this[name.Name]; } }

    public IErlObject this[ErlAtom name]
    {
      get { if (Empty) return null; IErlObject o; return m_Dict.TryGetValue(name, out o) ? o : null; }
      set { Add(name, value); }
    }

    public T Cast<T>(ErlAtom name) where T : IErlObject
    {
      Debug.Assert(!Empty);
      IErlObject obj = m_Dict[name];
      if (obj == null)
        throw new ErlException(string.Format(StringConsts.ERL_VARIABLE_NOT_FOUND_ERROR, name));
      return (T)obj;
    }

    public void Merge(ErlVarBind other)
    {
      init();
      foreach (var kv in other.m_Dict)
        m_Dict[kv.Key] = kv.Value;
    }

    public void Clear()
    {
      if (m_Dict != null)
        m_Dict.Clear();
      m_Dict = null;
    }

    /// <summary>
    /// Add a variable binding associating value with variable name
    /// </summary>
    /// <param name="name">Name of the variable</param>
    /// <param name="value">Value to associate with name</param>
    public void Add(ErlAtom name, IErlObject value)
    {
      init();
      m_Dict.Add(name, value);
    }

    /// <summary>
    /// Add a variable binding converting an object to variable by name
    /// </summary>
    /// <param name="var">Name of the variable</param>
    /// <param name="o">Value to associate with name</param>
    public void Add(ErlVar var, object o)
    {
      var eo = o.ToErlObject(var.ValueType);
      Add(var.Name, eo);
    }

    /// <summary>
    /// Add a variable binding associating value with variable name
    /// </summary>
    /// <param name="name">Name of the variable</param>
    /// <param name="et">Erlang type to use for conversion of the given CLR type</param>
    /// <param name="o">Value to associate with name</param>
    public void Add(ErlAtom name, ErlTypeOrder et, object o)
    {
      Add(name, o.ToErlObject(et));
    }

    public override string ToString()
    {
      var builder = new System.Text.StringBuilder();
      if (!Empty)
        foreach (var kv in m_Dict)
          builder.AppendFormat("{0}{1}={2}", builder.Length == 0 ? string.Empty : ", ", kv.Key, kv.Value);
      return builder.ToString();
    }

    public int Count { get { return Empty ? 0 : m_Dict.Count; } }
    public bool Empty { get { return m_Dict == null || m_Dict.Count == 0; } }

    public IEnumerator<KeyValuePair<ErlAtom, IErlObject>> GetEnumerator()
    {
      return Empty ? s_EmptyDictEnum : m_Dict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return Empty ? s_EmptyDictEnum : m_Dict.GetEnumerator();
    }

    private void init()
    {
      if (m_Dict != null) return;
      m_Dict = new Dictionary<ErlAtom, IErlObject>(8);
    }

  #endregion
  }
}
