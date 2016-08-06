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
using System.Collections.Generic;
using System.Linq;

namespace NFX.Erlang
{
  /// <summary>
  /// Special class used for passing tracing information in Erlang distributed
  /// messages SEND_TT, EXIT_TT, REG_SEND_TT, EXIT2_TT
  /// </summary>
  /// <remarks>
  /// See
  /// </remarks>
  public class ErlTrace : ErlTuple
  {
  #region .ctor

    public ErlTrace() { }

    public ErlTrace(ErlTuple list) : base((ErlTupleBase)list) { }

    /// <summary>
    /// Create an Erlang trace from the given arguments
    /// </summary>
    public ErlTrace(int flags, int label, int serial, ErlPid from, int prev)
        : base(flags, label, serial, from, prev)
    { }

  #endregion

  #region Public

    public override ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlTuple; } }

    public int Flags { get { return m_Items[0].ValueAsInt; } }
    public int Label { get { return m_Items[1].ValueAsInt; } }
    public int Serial { get { return m_Items[2].ValueAsInt; } }
    public ErlPid From { get { return (ErlPid)m_Items[3]; } }
    public int Prev { get { return m_Items[4].ValueAsInt; } }

    /// <summary>
    /// Implicit conversion of atom to string
    /// </summary>
    public static implicit operator List<IErlObject>(ErlTrace a)
    {
      return a.m_Items;
    }

    public static bool operator ==(ErlTrace lhs, IErlObject rhs) { return lhs == null ? rhs == null : lhs.Equals(rhs); }
    public static bool operator !=(ErlTrace lhs, IErlObject rhs) { return lhs == null ? rhs != null : !lhs.Equals(rhs); }

    public override bool Equals(object o)
    {
      return o is IErlObject && Equals((IErlObject)o);
    }

    /// <summary>
    /// Determine if two Erlang objects are equal
    /// </summary>
    public override bool Equals(IErlObject o)
    {
      return o is ErlTrace ? Equals((ErlTrace)o) : false;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Determine if two Erlang tuples are equal
    /// </summary>
    public bool Equals(ErlTrace o) { return m_Items.SequenceEqual(o.m_Items); }

  #endregion

  #region Protected

    protected override char OpenBracket { get { return '{'; } }
    protected override char CloseBracket { get { return '}'; } }

    protected override ErlTupleBase MakeInstance() { return new ErlTrace(); }

  #endregion
  }
}
