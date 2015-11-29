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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Tuple.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace NFX.Erlang
{
  public class ErlTuple : ErlTupleBase
  {
  #region .ctor

    private ErlTuple() { }

    /// <summary>
    /// Create an Erlang tuple from the given list of items
    /// </summary>
    public ErlTuple(IErlObject[] items) : base(items) { }

    public ErlTuple(List<IErlObject> items, bool clone = true) : base(items, clone) { }

    public ErlTuple(ErlTuple list) : base((ErlTupleBase)list) { }

    public ErlTuple(params object[] items) : base(items) { }

    public static ErlTuple Create(params object[] items)
    {
      return new ErlTuple(items);
    }

  #endregion

  #region Public

    public override ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlTuple; } }

    /// <summary>
    /// Implicit conversion of atom to string
    /// </summary>
    public static implicit operator List<IErlObject>(ErlTuple a)
    {
      return a.m_Items;
    }

    public static bool operator ==(ErlTuple lhs, IErlObject rhs) { return (object)lhs == null ? (object)rhs == null : lhs.Equals(rhs); }
    public static bool operator !=(ErlTuple lhs, IErlObject rhs) { return !(lhs == rhs); }

    /// <summary>
    /// Returns tuple value as DateTime in UTC if the tuple is in the form
    /// <code>{MegaSec, Sec, MicroSec}</code>
    /// </summary>
    public override DateTime ValueAsDateTime
    {
      get
      {
        if (Count == 3 && m_Items[0] is ErlLong && m_Items[1].IsInt() && m_Items[2].IsInt())
        {
          long s = m_Items[0].ValueAsLong * 1000000 + m_Items[1].ValueAsLong;
          long us = m_Items[2].ValueAsLong;
          return MiscUtils.UNIX_EPOCH_START_DATE.AddSeconds(s + (double)us / 1000000);
        }
        throw new ErlIncompatibleTypesException(this, typeof(DateTime));
      }
    }

    public override bool Equals(object o)
    {
      return base.Equals(o);
    }

    /// <summary>
    /// Determine if two Erlang objects are equal
    /// </summary>
    public override bool Equals(IErlObject o)
    {
      return o is ErlTuple ? Equals((ErlTuple)o) : false;
    }

    /// <summary>
    /// Determine if two Erlang tuples are equal
    /// </summary>
    public bool Equals(ErlTuple o) { return m_Items.SequenceEqual(o.m_Items); }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

  #endregion

  #region Protected

    protected override char OpenBracket { get { return '{'; } }
    protected override char CloseBracket { get { return '}'; } }

    protected override ErlTupleBase MakeInstance() { return new ErlTuple(); }

  #endregion
  }
}