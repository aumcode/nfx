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
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Object.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NFX.Erlang
{
  /// <summary>
  /// All Erlang terms implement this interface
  /// </summary>
  public interface IErlObject : IComparable, IComparable<IErlObject>, IEquatable<IErlObject>, ICloneable
  {
    /// <summary>
    /// Defines order of the Erlang term for comparison purposes
    /// </summary>
    ErlTypeOrder TypeOrder { get; }

    /// <summary>
    /// Clone an instance of Erlang object by performing deep copy of non-scalar terms
    /// and shallow copy of scalar terms
    /// </summary>
    new IErlObject Clone();

    /// <summary>
    /// Perform pattern match on this Erlang term, storing matched variables
    /// found in the pattern into the binding.
    /// </summary>
    bool Match(IErlObject pattern, ErlVarBind binding);

    /// <summary>
    /// Perform pattern match on this Erlang term returning null if match fails
    /// or a dictionary of matched variables bound in the pattern
    /// </summary>
    ErlVarBind Match(IErlObject pattern);

    /// <summary>
    /// Perform pattern match on this Erlang term without binding any variables
    /// </summary>
    bool Matches(IErlObject pattern);

    /// <summary>
    /// Substitute variables in a given Erlang term provided a dictionary
    /// of bound values
    /// </summary>
    bool Subst(ref IErlObject term, ErlVarBind binding);

    /// <summary>
    /// Execute fun for every nested term
    /// </summary>
    TAccumulate Visit<TAccumulate>(TAccumulate acc, Func<TAccumulate, IErlObject, TAccumulate> fun);

    /// <summary>
    /// Determines whether the underlying type is scalar or complex (i.e. tuple, list)
    /// </summary>
    bool IsScalar { get; }

    object      ValueAsObject   { get; }
    int         ValueAsInt      { get; }
    long        ValueAsLong     { get; }
    decimal     ValueAsDecimal  { get; }
    DateTime    ValueAsDateTime { get; }
    TimeSpan    ValueAsTimeSpan { get; }
    double      ValueAsDouble   { get; }
    string      ValueAsString   { get; }
    bool        ValueAsBool     { get; }
    char        ValueAsChar     { get; }
    byte[]      ValueAsByteArray{ get; }
  }

  public interface IErlObject<T> : IErlObject
  {
    T Value { get; }
  }

  /// <summary>
  /// Class defines extension methods on types implementing IErlObject interface
  /// and static methods dealing with IErlObject
  /// </summary>
  public static class ErlObject
  {
    /// <summary>
    /// Determines if Erlang object is NULL - that is ErlAtom.Undefined or null reference
    /// </summary>
    public static bool IsNull(this IErlObject o)
    {
      if (o==null) return true;

      return (o is ErlAtom) && ((ErlAtom) o) == ErlAtom.Undefined;
    }

    /// <summary>
    /// Determines if Erlang object is of type ErlByte or ErlLong
    /// </summary>
    public static bool IsInt(this IErlObject o)
    {
      return o is ErlLong || o is ErlByte;
    }

    /// <summary>
    /// Substibute all variables in the given object using provided binding
    /// and return the resulting object
    /// </summary>
    public static IErlObject Subst(this IErlObject o, ErlVarBind binding)
    {
      IErlObject term = null;
      return o.Subst(ref term, binding) ? term : o;
    }

    /// <summary>
    /// Execute fun for every nested term
    /// </summary>
    public static TAccumulate Visit<TAccumulate>(this IErlObject o, TAccumulate acc,
                                                 Func<TAccumulate,  IErlObject, TAccumulate> fun)
    {
      return o.Visit(acc, fun);
    }

    /// <summary>
    /// Convert C# string to ErlString
    /// </summary>
    public static ErlString ToErlString(this string str)
    {
      return new ErlString(str);
    }

    /// <summary>
    /// Convert C# string to ErlString
    /// </summary>
    public static string ErlArgs(this string str, params object[] args)
    {
      return Internal.Parser.Format(str, args);
    }

    /// <summary>
    /// Substitute variables in the args[0] string
    /// </summary>
    public static string Format(ErlList args)
    {
      return Internal.Parser.Format(args);
    }

    /// <summary>
    /// Parse a string into an Erlang term
    /// </summary>
    public static IErlObject Parse(string fmt, params object[] args)
    {
      int pos = 0, argc = 0;
      return Internal.Parser.Parse(fmt, ref pos, ref argc, args);
    }

    /// <summary>
    /// Parse a string into an Erlang term
    /// </summary>
    public static T Parse<T>(string fmt, params object[] args) where T : IErlObject
    {
      return (T)Parse(fmt, args);
    }

    /// <summary>
    /// Parse a string representation of an Erlang function call
    /// "Module:Function(Arg1, ..., ArgN)" to a 3-element (Module, Function, Arguments) tuple
    /// </summary>
    public static Tuple<ErlAtom, ErlAtom, ErlList> ParseMFA(string fmt, params object[] args)
    {
      int pos = 0, argc = 0;
      return Internal.Parser.ParseMFA(fmt, ref pos, ref argc, args);
    }

    /// <summary>
    /// String extension method to parse a string into an Erlang term
    /// </summary>
    public static IErlObject ToErlObject(this string fmt, params object[] args)
    {
      int pos = 0, argc = 0;
      return Internal.Parser.Parse(fmt, ref pos, ref argc, args);
    }

    /// <summary>
    /// Parse a string into an Erlang term
    /// </summary>
    public static T To<T>(this string fmt, params object[] args) where T : IErlObject
    {
      return (T)Parse(fmt, args);
    }

    /// <summary>
    /// Parse a string representation of an Erlang function call
    /// "Module:Function(Arg1, ..., ArgN)" to a 3-element (Module, Function, Arguments) tuple
    /// </summary>
    public static Tuple<ErlAtom, ErlAtom, ErlList> ToErlMFA(this string fmt, params object[] args)
    {
      int pos = 0, argc = 0;
      return Internal.Parser.ParseMFA(fmt, ref pos, ref argc, args);
    }

    /// <summary>
    /// Tries to convert an Erlang term as specified native type.
    /// Throw exception if conversion is not possible
    /// </summary>
    public static object AsType(this IErlObject val, Type t)
    {
      try
      {
        if (typeof(IErlObject).IsAssignableFrom(t)) return val;
        if (t == typeof(string))    return val.ValueAsString;
        if (t == typeof(int))       return val.ValueAsInt;
        if (t == typeof(long))      return val.ValueAsLong;
        if (t == typeof(short))     return (short)val.ValueAsInt;
        if (t == typeof(bool))      return val.ValueAsBool;
        if (t == typeof(float))     return (float)val.ValueAsDouble;
        if (t == typeof(double))    return val.ValueAsDouble;
        if (t == typeof(decimal))   return val.ValueAsDecimal;
        if (t == typeof(TimeSpan))  return val.ValueAsTimeSpan;
        if (t == typeof(DateTime))  return val.ValueAsDateTime;
        if (t == typeof(object))    return val;

        if (t.IsEnum) return Enum.Parse(t, val.ValueAsString, true);

        bool empty = string.IsNullOrEmpty(val.ValueAsString);
        if (t == typeof(int?))      return empty ? (int?)null       : val.ValueAsInt;
        if (t == typeof(long?))     return empty ? (long?)null      : val.ValueAsLong;
        if (t == typeof(short?))    return empty ? (short?)null     : (short)val.ValueAsInt;
        if (t == typeof(bool?))     return empty ? (bool?)null      : val.ValueAsBool;
        if (t == typeof(float?))    return empty ? (float?)null     : (float)val.ValueAsDouble;
        if (t == typeof(double?))   return empty ? (double?)null    : val.ValueAsDouble;
        if (t == typeof(decimal?))  return empty ? (decimal?)null   : val.ValueAsDecimal;
        if (t == typeof(TimeSpan?)) return empty ? (TimeSpan?)null  : val.ValueAsTimeSpan;
        if (t == typeof(DateTime?)) return empty ? (DateTime?)null  : val.ValueAsDateTime;

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
          var v = val.ToString();
          if (string.IsNullOrWhiteSpace(v)) return null;

          var gargs = t.GetGenericArguments();
          if (gargs.Length == 1)
          {
            var gtp = gargs[0];
            if (gtp.IsEnum)
                return Enum.Parse(gtp, v, true);
          }
        }
      }
      catch (Exception e)
      {
        throw new NFXException(StringConsts.ERL_CANNOT_CONVERT_TYPES_ERROR.Args(val.GetType().Name, t.FullName), e);
      }

      throw new ErlException(StringConsts.ERL_CANNOT_CONVERT_TYPES_ERROR, val.GetType().Name, t.FullName);
    }

    [ThreadStatic] private static HashSet<object> ts_Refs;

    /// <summary>
    /// Try to convert .NET native object type to corresponding Erlang term.
    /// Throw exception if conversion is not possible
    /// </summary>
    public static IErlObject ToErlObject(this object o)
    {
      if (o == null)
        return ErlAtom.Undefined;

      var t = o.GetType();
      if (t.IsValueType || o is string)
        return coreToErlObject(o);

      if (ts_Refs == null)
        ts_Refs = new HashSet<object>(NFX.ReferenceEqualityComparer<object>.Instance);
      if (ts_Refs.Contains(o))
        throw new ErlException(StringConsts.ERL_CANNOT_CONVERT_TYPES_CYCLE_ERROR,
            o.GetType().FullName, typeof(IErlObject).Name);

      ts_Refs.Add(o);

      try     { return coreToErlObject(o); }
      finally { ts_Refs.Remove(o);         }
    }

    /// <summary>
    /// Try to convert .NET native object type to corresponding Erlang term of given type.
    /// Throw exception if conversion is not possible
    /// </summary>
    public static IErlObject ToErlObject(this object o, ErlTypeOrder etp, bool strict = true)
    {
      if (o == null)
        return ErlAtom.Undefined;

      var t = o.GetType();
      if (t.IsValueType || o is string)
        return coreToErlObject(o, etp, strict);

      if (ts_Refs == null)
        ts_Refs = new HashSet<object>(NFX.ReferenceEqualityComparer<object>.Instance);
      if (ts_Refs.Contains(o))
        throw new ErlException(StringConsts.ERL_CANNOT_CONVERT_TYPES_CYCLE_ERROR,
            o.GetType().FullName, typeof(IErlObject).Name);

      try { return coreToErlObject(o, etp, strict); }
      finally { ts_Refs.Remove(o); }
    }

    private static IErlObject coreToErlObject(object o)
    {
      // Erlang terms
      if      (o is IErlObject) return (IErlObject)o;
      // Native types
      if      (o is int)      return new ErlLong((int)o);
      else if (o is uint)     return new ErlLong((uint)o);
      else if (o is long)     return new ErlLong((long)o);
      else if (o is ulong)    return new ErlLong((long)(ulong)o);
      else if (o is double)   return new ErlDouble((double)o);
      else if (o is float)    return new ErlDouble((float)o);
      else if (o is string)   return new ErlString((string)o);
      else if (o is bool)     return new ErlBoolean((bool)o);
      else if (o is char)     return new ErlByte((char)o);
      else if (o is byte)     return new ErlByte((byte)o);
      else if (o is short)    return new ErlLong((short)o);
      else if (o is ushort)   return new ErlLong((ushort)o);
      else if (o is decimal)  return new ErlDouble((double)(decimal)o);
      else if (o is DateTime)
      {
        var ts = ((DateTime)o).ToMicrosecondsSinceUnixEpochStart();
        var s = ts / 1000000;
        var us = ts - (s * 1000000);
        return (IErlObject)new ErlTuple(s / 1000000, s % 1000000, us);
      }
      else if (o is byte[])   return new ErlBinary((byte[])o, true);
      // TODO: Add support for IDictionary
      else if (o is IEnumerable)
      {
        var list = new ErlList();
        foreach (var item in (IEnumerable)o)
          list.Add(item.ToErlObject());
        return list;
      }
      else
        throw new ErlException(StringConsts.ERL_CANNOT_CONVERT_TYPES_ERROR,
                               o.GetType().FullName, typeof(IErlObject).Name);
    }

    private static IErlObject coreToErlObject(object o, ErlTypeOrder etp, bool strict)
    {
      if (o == null) return ErlAtom.Undefined;

      var eh = strict ? ConvertErrorHandling.Throw : ConvertErrorHandling.ReturnDefault;

      var e = o as IErlObject;
      if (e != null) return e;

      try
      {
        switch (etp)
        {
          case ErlTypeOrder.ErlObject:
          case ErlTypeOrder.ErlAtom:      return new ErlAtom(o.ToString());
          case ErlTypeOrder.ErlBinary:    return new ErlBinary((byte[])o);
          case ErlTypeOrder.ErlBoolean:   return new ErlBoolean(o.AsBool(handling: eh));
          case ErlTypeOrder.ErlByte:      return new ErlByte(o.AsByte(handling: eh));
          case ErlTypeOrder.ErlDouble:    return new ErlDouble(o.AsDouble(handling: eh));
          case ErlTypeOrder.ErlLong:      return new ErlLong(o.AsLong(handling: eh));
          case ErlTypeOrder.ErlList:
          {
            var list = new ErlList();
            foreach (var item in (IEnumerable)o)
              list.Add(item.ToErlObject());
            return list;
          }
          case ErlTypeOrder.ErlString:    return new ErlString(o.AsString(handling: eh));
          case ErlTypeOrder.ErlTuple:     return new ErlTuple((object[])o);
          case ErlTypeOrder.ErlPid:
          case ErlTypeOrder.ErlPort:
          case ErlTypeOrder.ErlRef:
          case ErlTypeOrder.ErlVar:
          default:
            throw new Exception();
        }
      }
      catch (Exception)
      {
        throw new ErlException
          (StringConsts.ERL_CANNOT_CONVERT_TYPES_ERROR, o.GetType().ToString(), etp.ToString());
      }
    }
  }
}