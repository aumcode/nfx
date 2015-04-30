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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Pid.cs
using System;
using System.IO;

namespace NFX.Erlang
{
	
	/// <summary>
	/// Provides a C# representation of Erlang integer types
    /// </summary>
    public struct ErlPid : IErlObject<ErlPid>
    {
        #region Static
            
            /// <summary>
            /// Special value non-existing Pid used for "null" pid comparison
            /// </summary>
            public static readonly ErlPid Null = new ErlPid(ErlAtom.Null, 0, 0);

        #endregion

        #region .ctor

            /// <summary>
            /// Create an Erlang pid from the given values
            /// </summary>
            public ErlPid(string node, int id, int serial, int creation)
                : this(new ErlAtom(node), id, serial, creation)
            {}

            /// <summary>
            /// Create an Erlang pid from the given values
            /// </summary>
		    public ErlPid(ErlAtom node, int id, int serial, int creation)
		    {
                // TODO: check compatibility with latest version of Erlang supporting 28bit pids
			    Node    = node;
                // m_Num is 15+13+2 bits
                m_Num   = (((id & 0x7fff) << 15)
                        | ((serial & 0x1fff) << 2)
                        | (creation & 0x3)) & 0x3fffFFFF;
		    }

            /// <summary>
            /// Create an Erlang pid from a monotonically increasing integer in range
            /// [1 ... 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="id"></param>
            /// <param name="creation"></param>
            internal ErlPid(ErlAtom node, int id, int creation)
                : this(node, (id >> 13) & 0x7fff, id & 0x1fff, creation)
            {
                Debug.Assert(id >= 0 && id < (1 << 28), "id >= 0 && id <= (1 << 28)");
            }

        #endregion

        #region Fields

            public readonly ErlAtom Node;
            private readonly int    m_Num;

        #endregion

        #region Props

            public int Id       { get { return (int)((m_Num >> (13+2)) & 0x7fff); /* 15 bits */ } }
            public int Serial   { get { return (int)((m_Num >> 2) & 0x1fff); /* 13 bits */ } }
            public int Creation { get { return (int)(m_Num & 0x3); /* 2 bits */ } }
            internal int Num    { get { return m_Num; } }

            public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlPid; } }

            public bool IsScalar { get { return true; } }

            public bool Empty   { get { return Num == 0; } }

            public ErlPid Value { get { return this; } }

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

            public static bool operator ==(ErlPid lhs, ErlPid rhs) { return  lhs.Equals(rhs); }
            public static bool operator !=(ErlPid lhs, ErlPid rhs) { return !lhs.Equals(rhs); }

            public override string ToString()
		    {
                return string.Format("#Pid<{0}.{1}.{2}>", Node, Id, Serial);
		    }

            /// <summary>
            /// Determine if this instance equals to the object
            /// </summary>
            public override bool Equals(object o)
            {
                return (o is IErlObject) && Equals((IErlObject)o);
            }

            /// <summary>
            /// Determine if two instances are equal
            /// </summary>
            public bool Equals(IErlObject o)
            {
                return (o is ErlPid) && Equals((ErlPid)o);
            }

            /// <summary>
            /// Determine if two instances are equal
            /// </summary>
            public bool Equals(ErlPid pid)
            {
                return (m_Num == pid.m_Num) && Node.Equals(pid.Node);
            }

            /// <summary>
            /// Get internal hash code
            /// </summary>
            public override int GetHashCode()
            {
                return Node.GetHashCode() * Id;
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
                if (!(obj is ErlPid))
                    return TypeOrder < obj.TypeOrder ? -1 : 1;

                var rhs = (ErlPid)obj;
                int n = Node.CompareTo(rhs.Node);
                if (n != 0) return n;
                return m_Num.CompareTo(rhs.m_Num);
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