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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Atom.cs
using System;
using System.Text;

namespace NFX.Erlang
{
  public struct ErlAtom : IErlObject<string>
  {
  #region Static

    public static readonly ErlAtom Null      = new ErlAtom(AtomTable.EMPTY_INDEX);
    public static readonly ErlAtom True      = new ErlAtom(AtomTable.TRUE_INDEX);
    public static readonly ErlAtom False     = new ErlAtom(AtomTable.FALSE_INDEX);
    public static readonly ErlAtom Undefined = new ErlAtom(AtomTable.UNDEFINED_INDEX);
    public static readonly ErlAtom Normal    = new ErlAtom("normal");

  #endregion

  #region .ctor

    public ErlAtom(ErlAtom atom)
    {
      Index = atom.Index;
    }

    private ErlAtom(int atomIndex)
    {
      if (atomIndex < 0 || (AtomTable.Initialized && atomIndex >= AtomTable.Instance.Count))
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR, atomIndex);
      Index = atomIndex;
    }
    /// <summary>
    /// Create an atom from the given string
    /// </summary>
    public ErlAtom(string atom)
    {
      if (atom == null)
        throw new ErlException(StringConsts.ERL_VALUE_MUST_NOT_BE_NULL_ERROR);

      Index = AtomTable.Instance[atom];
    }

    public ErlAtom(byte[] buf, ref int offset, int count)
    {
      int idx = offset;
      var tag = (ErlExternalTag)buf[idx++];
      int len;
      Encoding encoding;

      switch (tag)
      {
        case ErlExternalTag.Atom:
          if (idx + 2 >= count) throw new NotEnoughDataException();
          len = buf.ReadBEShort(ref idx);
          encoding = Encoding.ASCII;
          break;
        case ErlExternalTag.SmallAtom:
          if (idx + 1 >= count) throw new NotEnoughDataException();
          len = buf[idx++];
          encoding = Encoding.ASCII;
          break;
        case ErlExternalTag.AtomUtf8:
          if (idx + 2 >= count) throw new NotEnoughDataException();
          len = buf.ReadBEShort(ref idx);
          encoding = Encoding.UTF8;
          break;
        case ErlExternalTag.SmallAtomUtf8:
          if (idx + 1 >= count) throw new NotEnoughDataException();
          len = buf[idx++];
          encoding = Encoding.UTF8;
          break;
        default:
          throw new ErlException(StringConsts.ERL_INVALID_TERM_TYPE_ERROR, "atom", (int)tag);
      }

      string s = encoding.GetString(buf, idx, len);
      idx += len;
      int n = Math.Min(len, AtomTable.MAX_ATOM_LEN);
      if (n != len)
        s = s.Substring(0, n);
      offset = idx;

      Index = AtomTable.Instance[s];
    }

  #endregion

  #region Fields

    /// <summary>
    /// The index of this atom in the global atom table
    /// </summary>
    public readonly int Index;

  #endregion

  #region Props

    /// <summary>
    /// Returns true if this instance equals ErlAtom.Null
    /// </summary>
    public bool Empty { get { return Equals(Null); } }

    /// <summary>
    /// Get the actual string contained in this object
    /// </summary>
    public string Value { get { return AtomTable.Instance[Index]; } }

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlAtom; } }

    /// <summary>
    /// Determines whether the underlying type is scalar or complex (i.e. tuple, list)
    /// </summary>
    public bool IsScalar { get { return true; } }

    public object       ValueAsObject   { get { return Value; } }
    public int          ValueAsInt      { get { throw new ErlIncompatibleTypesException(this, typeof(int)); } }
    public long         ValueAsLong     { get { throw new ErlIncompatibleTypesException(this, typeof(long)); } }
    public decimal      ValueAsDecimal  { get { throw new ErlIncompatibleTypesException(this, typeof(decimal)); } }
    public DateTime     ValueAsDateTime { get { throw new ErlIncompatibleTypesException(this, typeof(DateTime)); } }
    public TimeSpan     ValueAsTimeSpan { get { throw new ErlIncompatibleTypesException(this, typeof(TimeSpan)); } }
    public double       ValueAsDouble   { get { throw new ErlIncompatibleTypesException(this, typeof(double)); } }
    public string       ValueAsString   { get { return Value; } }
    public bool         ValueAsBool     { get { return Index == AtomTable.TRUE_INDEX; } }
    public char         ValueAsChar     { get { var s = Value;
                                                if (s.Length != 1) throw new ErlException(StringConsts.ERL_INVALID_VALUE_LENGTH_ERROR, s.Length);
                                                return s[0];
                                              } }
    public byte[]       ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

    /// <summary>
    /// Return the length of the atom string
    /// </summary>
    public int Length { get { return Value.Length; } }

  #endregion

  #region Public

    /// <summary>
    /// Implicit conversion of atom to string
    /// </summary>
    public static implicit operator string (ErlAtom a)
    {
      return a.Value;
    }

    /// <summary>
    /// Implicit conversion of string to atom
    /// </summary>
    public static implicit operator ErlAtom(string a)
    {
      return new ErlAtom(a);
    }

    public static bool operator ==(ErlAtom lhs, ErlAtom rhs) { return lhs.Index == rhs.Index; }
    public static bool operator !=(ErlAtom lhs, ErlAtom rhs) { return lhs.Index != rhs.Index; }

    /// <summary>
    /// Get the printname of the atom represented by this object. The
    /// difference between this method and {link #atomValue atomValue()}
    /// is that the printname is quoted and escaped where necessary,
    /// according to the Erlang rules for atom naming
    /// </summary>
    public override string ToString()
    {
      string s = AtomTable.Instance[Index];
      return atomNeedsQuoting(s) ? string.Format("'{0}'", escapeSpecialChars(s)) : s;
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
      return o is ErlAtom && Index == ((ErlAtom)o).Index;
    }

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return Index;
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
      if (!(obj is ErlAtom))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      var rhs = (ErlAtom)obj;
      return Value.CompareTo(rhs.Value);
    }

    /// <summary>
    /// Clone an instance of the object (non-scalar immutable objects are copied by reference)
    /// </summary>
    public IErlObject Clone() { return this; }  // Scalar Value is immutable
    object ICloneable.Clone() { return Clone(); }

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

  #endregion

  #region .pvt

    private bool isErlangDigit(char c) { return (c >= '0' && c <= '9'); }
    private bool isErlangUpper(char c) { return ((c >= 'A' && c <= 'Z') || (c == '_')); }
    private bool isErlangLower(char c) { return (c >= 'a' && c <= 'z'); }
    private bool isErlangLetter(char c) { return isErlangLower(c) || isErlangUpper(c); }

    // true if the atom should be displayed with quotation marks
    private bool atomNeedsQuoting(System.String s)
    {
      char c;

      if (s.Length == 0 || !isErlangLower(s[0])) return true;

      int len = s.Length;
      for (int i = 1; i < len; i++)
      {
        c = s[i];

        if (!isErlangLetter(c) && !isErlangDigit(c) && c != '@')
          return true;
      }
      return false;
    }

    /*Get the atom string, with special characters escaped. Note that
    * this function currently does not consider any characters above
    * 127 to be printable.
    */
    private string escapeSpecialChars(System.String s)
    {
      var so = new StringBuilder();

      for (int i = 0, len = s.Length; i < len; i++)
      {
        char c = s[i];

        /*note that some of these escape sequences are unique to
        * Erlang, which is why the corresponding 'case' values use
        * octal. The resulting string is, of course, in Erlang format.
        */

        switch (c)
        {
          case '\b':
            so.Append("\\b");
            break;

          case (char)(127):
            so.Append("\\d");
            break;

          case (char)(27):
            so.Append("\\e");
            break;

          case '\f':
            so.Append("\\f");
            break;

          case '\n':
            so.Append("\\n");
            break;

          case '\r':
            so.Append("\\r");
            break;

          case '\t':
            so.Append("\\t");
            break;

          case (char)(11):
            so.Append("\\v");
            break;

          case '\\':
            so.Append("\\\\");
            break;

          case '\'':
            so.Append("\\'");
            break;

          case '\"':
            so.Append("\\\"");
            break;

          default:
            // some other character classes
            if (c < 23)
            {
              // control chars show as "\^@", "\^A" etc
              so.Append("\\^" + (char)(('A' - 1) + c));
            }
            else if (c > 126)
            {
              // 8-bit chars show as \345 \344 \366 etc
              so.Append("\\" + Convert.ToString(c, 8));
            }
            else
            {
              // character is printable without modification!
              so.Append(c);
            }
            break;

        }
      }
      return so.ToString();
    }

  #endregion
  }
}