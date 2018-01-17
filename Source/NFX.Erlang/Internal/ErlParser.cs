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
// Author: Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Format.cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using System.Globalization;

namespace NFX.Erlang.Internal
{
  /// <summary>
  /// Provides compilation of strings to Erlang terms
  /// </summary>
  internal class Parser
  {
    /// <summary>
    /// Substitute variables in a string contained in args[0]. The
    /// substitution values begin with args[1]
    /// </summary>
    internal static string Format(ErlList args)
    {
      string fmt = args.Count == 0 ? string.Empty : ((ErlString)args[0]).ToString(false);
      return internalFormat(fmt, args.Value.Skip(1).ToList<object>());
    }

    /// <summary>
    /// Substitute variables in a fmt string
    /// </summary>
    /// <param name="fmt">Format string containing substitution variables (e.g. '~w')</param>
    /// <param name="args">List of arguments</param>
    internal static string Format(string fmt, ErlList args)
    {
      return internalFormat(fmt, args.Value.ToList<object>());
    }

    /// <summary>
    /// Substitute variables in a fmt string. The following variables are supported:
    /// <dl>
    /// <dt>~v</dt><dd>Take next argument as string.
    ///                 If the argument is ErlAtom, it's name is used. If the argument is
    ///                 ErlVar it's name is used with the type if the fmt string doesn't have
    ///                 the variable type specification (e.g. A::integer()), or the name
    ///                 is used verbatim, if the variable type is present in the format
    ///                 string.  Finally if the argument is not ErlAtom and ErlVar, it's
    ///                 inserted as string verbatim</dd>
    /// <dt>~w</dt><dd>Format next argument as string</dd>
    /// <dt>~i</dt><dd>Skip next argument</dd>
    /// <dt>~c</dt><dd>Format next argument as the character</dd>
    /// </dl>
    /// </summary>
    /// <param name="fmt">Format string containing substitution variables (e.g. '~w')</param>
    /// <param name="args">List of arguments</param>
    internal static string Format(string fmt, params object[] args)
    {
      return internalFormat(fmt, args.ToList());
    }

    /// <summary>
    /// Parses a string representation of an Erlang function call in the form
    /// <code>Module:Function(Arg1, ..., Arg)</code> or
    /// <code>Module:Function(Arg1, ..., Arg).</code>
    /// </summary>
    /// <param name="fmt">string to parse</param>
    /// <param name="pos">starting index</param>
    /// <param name="argc">argument number</param>
    /// <param name="args">optional arguments</param>
    /// <returns>A 3-element tuple containing Module, Function, Arguments</returns>
    internal static Tuple<ErlAtom, ErlAtom, ErlList>
        ParseMFA(string fmt, ref int pos, ref int argc, params object[] args)
    {
      int end = fmt.Length;

      pos = skipWSAndComments(fmt, pos);

      var i = fmt.IndexOf(':', pos);

      if (i < 0)
        throw new ErlException(StringConsts.ERL_INVALID_MFA_FORMAT_ERROR);

      var module = parseAtom(fmt.Substring(pos, i - pos), ref argc, args);

      pos = fmt.IndexOf('(', i + 1);

      if (pos < 0)
        throw new ErlException(StringConsts.ERL_INVALID_MFA_FORMAT_ERROR);

      var function = parseAtom(fmt.Substring(i + 1, pos - i - 1), ref argc, args);

      if (++pos >= end)
        throw new ErlException(StringConsts.ERL_INVALID_MFA_FORMAT_ERROR);

      pos = skipWSAndComments(fmt, pos);

      if (pos == end)
        throw new ErlException(StringConsts.ERL_INVALID_MFA_FORMAT_ERROR);

      var arguments = new List<IErlObject>();

      if (fmt[pos] == ')')
        pos = skipWSAndComments(fmt, ++pos);
      else
        while (true)
        {
          var arg = Parse(fmt, ref pos, ref argc, args);
          arguments.Add(arg);

          pos = skipWSAndComments(fmt, pos);

          if (pos == end)
            throw new ErlException(StringConsts.ERL_INVALID_MFA_FORMAT_ERROR);

          var c = fmt[pos++];
          if (c == ')')
            break;
          if (c != ',')
            throw new ErlException(StringConsts.ERL_INVALID_MFA_FORMAT_ERROR);
        }

      pos = skipWSAndComments(fmt, pos);

      // Consume trailing '.' if present
      if (pos < end)
      {
        if (fmt[pos] == '.')
          pos = skipWSAndComments(fmt, ++pos);
      }

      if (pos != end)
        throw new ErlException(StringConsts.ERL_INVALID_MFA_FORMAT_ERROR);

      return Tuple.Create(module, function, new ErlList(arguments, false));
    }

    /// <summary>
    /// Compile a string fmt into an Erlang term
    /// </summary>
    internal static IErlObject Parse(
        string fmt, ref int pos, ref int argc, params object[] args)
    {
      var items = new List<IErlObject>();
      IErlObject result = null;
      pos = skipWSAndComments(fmt, pos);

      if (pos < fmt.Length)
      {
        switch (fmt[pos++])
        {
          case '{':
            if (State.Ok != pTuple(fmt, ref pos, ref items, ref argc, args))
              throw new ErlException(StringConsts.ERL_PARSING_AT_ERROR, "tuple", pos);
            result = new ErlTuple(items, false);
            break;

          case '[':
            if (fmt[pos] == ']')
            {
              result = new ErlList();
              pos++;
              break;
            }
            else if (State.Ok == pList(fmt, ref pos, ref items, ref argc, args))
            {
              result = new ErlList(items, false);
              break;
            }
            throw new ErlException(StringConsts.ERL_PARSING_AT_ERROR, "list", pos);

          case '<':
            if (pos < fmt.Length - 1 && fmt[pos] == '<')
            {
              var i = ++pos;
              var str = fmt[i] == '"';
              if (str) pos++;
              for (; i < fmt.Length && fmt[i - 1] != '>' && fmt[i] != '>'; ++i);
              if (i == fmt.Length)
                break;
              var end   = ++i - (str ? 3 : 2);
              var len   = end - pos + 1;
              byte[] bytes;
              if (str)
              {
                var cnt = Encoding.UTF8.GetByteCount(fmt.ToCharArray(), pos, len);
                bytes = new byte[cnt];
                Encoding.UTF8.GetBytes(fmt, pos, len, bytes, 0);
              }
              else
              {
                var beg = pos - 2;
                bytes   = fmt.Substring(pos, len)
                             .Split(new char[] {',', ' '},
                                    StringSplitOptions.RemoveEmptyEntries)
                             .Select(s =>
                             {
                               var n = int.Parse(s);
                               if (n < 0 || n > 255)
                                 throw new ErlException
                                  ("Invalid binary in format string: {0}".Args(fmt.Substring(beg, len+4)));
                               return (byte)n;
                             })
                             .ToArray();
              }
              result = new ErlBinary(bytes, 0, bytes.Length);
              pos = i+1;
            }
            break;
          case '$': /* char-value? */
            result = new ErlByte(Convert.ToByte(fmt[pos++]));
            break;

          case '~':
            if (State.Ok != pFormat(fmt, ref pos, ref items, ref argc, args))
              throw new ErlException(StringConsts.ERL_PARSING_AT_ERROR, "term", pos);
            result = items[0];
            break;

          default:
            char c = fmt[--pos];
            if (char.IsLower(c))
            {         /* atom  ? */
              string s = pAtom(fmt, ref pos);
              result = createAtom(s);
            }
            else if (char.IsUpper(c) || c == '_')
            {
              result = pVariable(fmt, ref pos);
            }
            else if (char.IsDigit(c) || c == '-')
            {    /* integer/float ? */
              string s = pDigit(fmt, ref pos);
              if (s.IndexOf('.') < 0)
                result = new ErlLong(long.Parse(s, CultureInfo.InvariantCulture));
              else
                result = new ErlDouble(double.Parse(s, CultureInfo.InvariantCulture));
            }
            else if (c == '"')
            {      /* string ? */
              string s = pString(fmt, ref pos);
              result = new ErlString(s);
            }
            else if (c == '\'')
            {     /* quoted atom ? */
              string s = pQuotedAtom(fmt, ref pos);
              result = createAtom(s);
            }
            break;
        }
      }

      if (result == null)
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR.Args(fmt));

      return result;
    }

  #region .pvt

    private static object advance(ref IEnumerator args)
    {
      while (args.MoveNext())
        return args.Current;
      throw new ErlException(StringConsts.ERL_INVALID_NUMBER_OF_ARGS_ERROR);
    }

    private static string internalFormat(string fmt, List<object> args)
    {
      IEnumerator argsIter = args.GetEnumerator();

      var sb = new StringBuilder();
      for (int i = 0, end = fmt.Length; i < end; i++)
      {
        if (fmt[i] == '~')
          i++;
        else
        {
          sb.Append(fmt[i]);
          continue;
        }

        var n = i;

        if (i < end && fmt[i] == '~')
        {
          sb.Append(fmt[i]);
          continue;
        }

        for (; i < end && (fmt[i] == '.' || (fmt[i] >= '0' && fmt[i] <= '9')); ++i) ;

        if (i < end)
          switch (fmt[i])
          {
            case 'v': /* Append next argument verbatim */
              {
                var a = advance(ref argsIter);
                if (a is ErlAtom) sb.Append(((ErlAtom)a).Value);
                else if (a is ErlString) sb.Append(((ErlString)a).Value);
                else if (a is ErlVar)
                  sb.Append(
                     (i < end - 2 && fmt[i + 1] == ':' && fmt[i + 2] == ':')
                     ? ((ErlVar)a).Name.Value : ((ErlVar)a).ToString()
                  );
                else sb.Append(a);
                break;
              }
            case 'w':
            case 'p':
            case 'f':
            case 'g':
            case 'e':
            case 's':
            case 'W':
            case 'B':
            case 'P':
            case '#':
            case 'b':
            case 'x':
            case '+':
              sb.Append(advance(ref argsIter));
              break;
            case 'n':
              sb.Append('\n');
              break;
            case 'i':
              advance(ref argsIter);
              break;
            case 'c':
              var c = advance(ref argsIter);
              if (c is ErlByte)
                sb.Append(((ErlByte)c).ValueAsChar);
              else
                sb.Append(c.ToString());
              break;
            default:
              throw new ErlException(
                  StringConsts.ERL_INVALID_FORMATTING_CHAR_ERROR, fmt[i], fmt);
          }
        else
          throw new ErlException(
              StringConsts.ERL_INVALID_FORMATTING_CHAR_ERROR, fmt[n], fmt);
      }
      return sb.ToString();
    }

    private enum State
    {
      Ok = 0
                , FmtError = -1
                , MaxEntries = 255  /* Max entries in a tuple/list term */
                , MaxNameLen = 255  /* Max length of variable names */
    };

    private static int skipWSAndComments(string fmt, int pos)
    {
      bool insideComment = false;
      for (int n = fmt.Length; pos < n; pos++)
      {
        char c = fmt[pos];
        if (insideComment)
        {
          if (c == '\n') insideComment = false;
          continue;
        }
        if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
          continue;
        else if (c == '%')
        {
          insideComment = true;
          continue;
        }
        break;
      }
      return pos;
    }

    private static ErlVar pVariable(string fmt, ref int pos)
    {
      int start = pos;

      for (pos = skipWSAndComments(fmt, pos); pos < fmt.Length; pos++)
      {
        char c = fmt[pos];
        if (char.IsLetterOrDigit(c) || (c == '_'))
          continue;
        break;
      }

      ErlTypeOrder type = ErlTypeOrder.ErlObject;
      int i = pos;
      int end = pos;

      // TODO: Add recursive type checking (i.e. A :: [{atom(), [integer() | string() | double() | tuple()]}])
      if (fmt.Length > i + 1 && fmt[i] == ':' && fmt[i + 1] == ':')
      {
        i = pos + 2;
        int tps = i;

        for (char c = fmt[i]; char.IsLetter(c) && i < fmt.Length - 1; c = fmt[++i]) ;

        if (fmt[i] == '(' && i < fmt.Length - 1 && fmt[i + 1] == ')')
        {
          pos = i + 2;

          string tp = fmt.Substring(tps, i - tps);

          switch (tp)
          {
            case "int":
            case "long":
            case "integer":     type = ErlTypeOrder.ErlLong;    break;
            case "str":
            case "string":      type = ErlTypeOrder.ErlString;  break;
            case "atom":        type = ErlTypeOrder.ErlAtom;    break;
            case "float":
            case "double":      type = ErlTypeOrder.ErlDouble;  break;
            case "binary":      type = ErlTypeOrder.ErlBinary;  break;
            case "bool":
            case "boolean":     type = ErlTypeOrder.ErlBoolean; break;
            case "byte":        type = ErlTypeOrder.ErlByte;    break;
            case "char":        type = ErlTypeOrder.ErlByte;    break;
            case "list":        type = ErlTypeOrder.ErlList;    break;
            case "tuple":       type = ErlTypeOrder.ErlTuple;   break;
            case "pid":         type = ErlTypeOrder.ErlPid;     break;
            case "ref":
            case "reference":   type = ErlTypeOrder.ErlRef;     break;
            case "port":        type = ErlTypeOrder.ErlPort;    break;
            default:
                throw new ErlException(StringConsts.ERL_UNSUPPORTED_TERM_TYPE_ERROR, tp);
          }
        }
        else
          throw new ErlException(StringConsts.ERL_INVALID_VARIABLE_TYPE_ERROR,
              fmt.Substring(start, pos - start));
      }
      else
        type = ErlTypeOrder.ErlObject;

      int len = end - start;
      return new ErlVar(fmt.Substring(start, len), type);
    }

    private static IErlObject createAtom(string s)
    {
      switch (s)
      {
        case ErlConsts.TRUE:
          return new ErlBoolean(true);
        case ErlConsts.FALSE:
          return new ErlBoolean(false);
        default:
          return new ErlAtom(s);
      }
    }

    private static string pAtom(string fmt, ref int pos)
    {
      int start = pos;

      for (pos = skipWSAndComments(fmt, pos); pos < fmt.Length; pos++)
      {
        char c = fmt[pos];
        if (char.IsLetterOrDigit(c) || (c == '_') || (c == '@'))
          continue;
        else
          break;
      }
      int len = pos - start;
      return fmt.Substring(start, len);
    }

    private static string pDigit(string fmt, ref int pos)
    {
      int start = pos;
      bool dotp = false;

      for (pos = skipWSAndComments(fmt, pos); pos < fmt.Length; pos++)
      {
        char c = fmt[pos];
        if (char.IsDigit(c) || c == '-')
          continue;
        else if (!dotp && c == '.')
        {
          dotp = true;
          continue;
        }
        else
          break;
      }
      int len = pos - start;
      return fmt.Substring(start, len);
    }

    private static string pString(string fmt, ref int pos)
    {
      int start = ++pos; // skip the first double quote

      for (; pos < fmt.Length; pos++)
      {
        char c = fmt[pos];
        if (c == '"')
        {
          if (fmt[pos - 1] == '\\')
            continue;
          else
            break;
        }
      }
      int len = pos++ - start;
      return fmt.Substring(start, len);
    }

    private static string pQuotedAtom(string fmt, ref int pos)
    {
      int start = ++pos; // skip first quote

      for (pos = skipWSAndComments(fmt, pos); pos < fmt.Length; pos++)
      {
        char c = fmt[pos];
        if (c == '\'')
        {
          if (fmt[pos - 1] == '\\')
            continue;
          else
            break;
        }
      }
      int len = pos++ - start;
      return fmt.Substring(start, len);
    }

    private static State pTuple(string fmt, ref int pos,
        ref List<IErlObject> items, ref int argc, params object[] args)
    {
      int start = pos;
      State rc = State.FmtError;

      if (pos < fmt.Length)
      {
        pos = skipWSAndComments(fmt, pos);
        switch (fmt[pos++])
        {
          case '}':
            rc = State.Ok;
            break;

          case ',':
            rc = pTuple(fmt, ref pos, ref items, ref argc, args);
            break;

          default:
            {
              --pos;
              IErlObject obj = Parse(fmt, ref pos, ref argc, args);
              items.Add(obj);
              rc = pTuple(fmt, ref pos, ref items, ref argc, args);
              break;
            }
        }
      }
      return rc;
    }

    private static State pList(string fmt, ref int pos,
        ref List<IErlObject> items, ref int argc, params object[] args)
    {
      State rc = State.FmtError;

      if (pos < fmt.Length)
      {
        pos = skipWSAndComments(fmt, pos);

        switch (fmt[pos++])
        {
          case ']':
            rc = State.Ok;
            break;

          case ',':
            rc = pList(fmt, ref pos, ref items, ref argc, args);
            break;

          case '|':
            pos = skipWSAndComments(fmt, pos);
            if (char.IsUpper(fmt[pos]) || fmt[pos] == '_')
            {
              var v = pVariable(fmt, ref pos);
              items.Add(v);
              pos = skipWSAndComments(fmt, pos);
              if (fmt[pos] == ']')
                rc = State.Ok;
              break;
            }
            break;

          default:
            {
              --pos;
              IErlObject obj = Parse(fmt, ref pos, ref argc, args);
              items.Add(obj);
              rc = pList(fmt, ref pos, ref items, ref argc, args);
              break;
            }

        }
      }
      return rc;
    }

    private static State pFormat(string fmt, ref int pos,
        ref List<IErlObject> items, ref int argc, params object[] args)
    {
      pos = skipWSAndComments(fmt, pos);

      char c = fmt[pos++];

      if (argc >= args.Length)
        throw new ErlException(StringConsts.ERL_MISSING_VALUE_FOR_ARGUMENT_ERROR, argc, c);

      object o = args[argc++];

      switch (c)
      {
        case 'i':
        case 'd': items.Add(new ErlLong(Convert.ToInt64(o))); break;
        case 'f': items.Add(new ErlDouble((double)o));  break;
        case 's': items.Add(new ErlString((string)o));  break;
        case 'b': items.Add(new ErlBoolean((bool)o));   break;
        case 'c': items.Add(new ErlLong((char)o));      break;
        case 'p':
        case 'w': items.Add(o.ToErlObject());           break;
        default:
          throw new ErlException(
              StringConsts.ERL_WRONG_VALUE_FOR_ARGUMENT_ERROR, argc - 1, c);
      }
      return State.Ok;
    }

    private static ErlAtom parseAtom(string s, ref int argc, object[] args)
    {
      if (s != "~w")
        return new ErlAtom(s);

      if (argc >= args.Length)
        throw new ErlException(StringConsts.ERL_MISSING_VALUE_FOR_ARGUMENT_ERROR, argc, s);

      var value = args[argc++];

      if (value is string) return new ErlAtom((string)value);
      else if (value is ErlAtom) return (ErlAtom)value;
      else
        throw new ErlException(
       StringConsts.ERL_INVALID_TERM_TYPE_ERROR, typeof(ErlAtom).Name, value.GetType().Name);
    }

  #endregion
  }
}
