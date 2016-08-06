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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using NFX.DataAccess.Distributed;

namespace NFX
{
    /// <summary>
    /// Provides extension methods for converting string values to different scalar types
    /// </summary>
    public static class StringValueConversion
    {

         /// <summary>
         /// Used by env var macros evaluator do not remove
         /// </summary>
         public static string AsStringWhenNullOrEmpty(this string val, string dflt = "")
         {
              return val.AsString(dflt);
         }

         /// <summary>
         /// Used by env var macros evaluator do not remove
         /// </summary>
         public static string AsString(this string val, string dflt = "")
         {
              if (string.IsNullOrEmpty(val))
                return dflt;
              else
                return val;
         }


         public static readonly char[] BYTE_ARRAY_SPLIT_CHARS = new char[]{',',';'};

         public static byte[] AsByteArray(this string val, byte[] dflt = null)
         {
              if (val==null) return dflt;
              try
              {
                var result = new List<byte>();
                var segs = val.Split(BYTE_ARRAY_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
                foreach(var seg in segs)
                 result.Add( byte.Parse(seg, NumberStyles.HexNumber));

                return result.ToArray();
              }
              catch
              {
                return dflt;
              }
         }


         public static GDID AsGDID(this string val, GDID? dflt = null)
         {
              if (dflt.HasValue)
                return ObjectValueConversion.AsGDID(val, dflt.Value);
              else
                return ObjectValueConversion.AsGDID(val);
         }

         public static GDID? AsNullableGDID(this string val, GDID? dflt = null)
         {
              return ObjectValueConversion.AsNullableGDID(val, dflt);
         }

         public static GDIDSymbol AsGDIDSymbol(this string val, GDIDSymbol? dflt = null)
         {
              if (dflt.HasValue)
                return ObjectValueConversion.AsGDIDSymbol(val, dflt.Value);
              else
                return ObjectValueConversion.AsGDIDSymbol(val);
         }

         public static GDIDSymbol? AsNullableGDIDSymbol(this string val, GDIDSymbol? dflt = null)
         {
              return ObjectValueConversion.AsNullableGDIDSymbol(val, dflt);
         }


         public static byte AsByte(this string val, byte dflt = 0)
         {
              return ObjectValueConversion.AsByte(val, dflt);
         }

         public static byte? AsNullableByte(this string val, byte? dflt = 0)
         {
              return ObjectValueConversion.AsNullableByte(val, dflt);
         }

         public static sbyte AsSByte(this string val, sbyte dflt = 0)
         {
              return ObjectValueConversion.AsSByte(val, dflt);
         }

         public static sbyte? AsNullableSByte(this string val, sbyte? dflt = 0)
         {
              return ObjectValueConversion.AsNullableSByte(val, dflt);
         }


         public static short AsShort(this string val, short dflt = 0)
         {
              return ObjectValueConversion.AsShort(val, dflt);
         }

         public static short? AsNullableShort(this string val, short? dflt = 0)
         {
              return ObjectValueConversion.AsNullableShort(val, dflt);
         }


         public static ushort AsUShort(this string val, ushort dflt = 0)
         {
              return ObjectValueConversion.AsUShort(val, dflt);
         }

         public static ushort? AsNullableUShort(this string val, ushort? dflt = 0)
         {
              return ObjectValueConversion.AsNullableUShort(val, dflt);
         }


         public static int AsInt(this string val, int dflt = 0)
         {
              return ObjectValueConversion.AsInt(val, dflt);
         }

         public static int? AsNullableInt(this string val, int? dflt = 0)
         {
              return ObjectValueConversion.AsNullableInt(val, dflt);
         }

         public static uint AsUInt(this string val, uint dflt = 0)
         {
              return ObjectValueConversion.AsUInt(val, dflt);
         }

         public static uint? AsNullableUInt(this string val, uint? dflt = 0)
         {
              return ObjectValueConversion.AsNullableUInt(val, dflt);
         }


         public static long AsLong(this string val, long dflt = 0)
         {
              return ObjectValueConversion.AsLong(val, dflt);
         }

         public static long? AsNullableLong(this string val, long? dflt = 0)
         {
             return ObjectValueConversion.AsNullableLong(val, dflt);
         }

         public static ulong AsULong(this string val, ulong dflt = 0)
         {
              return ObjectValueConversion.AsULong(val, dflt);
         }

         public static ulong? AsNullableULong(this string val, ulong? dflt = 0)
         {
             return ObjectValueConversion.AsNullableULong(val, dflt);
         }


         public static double AsDouble(this string val, double dflt = 0d)
         {
             return ObjectValueConversion.AsDouble(val, dflt);
         }

         public static double? AsNullableDouble(this string val, double? dflt = 0d)
         {
             return ObjectValueConversion.AsNullableDouble(val, dflt);
         }


         public static float AsFloat(this string val, float dflt = 0f)
         {
             return ObjectValueConversion.AsFloat(val, dflt);
         }

         public static float? AsNullableFloat(this string val, float? dflt = 0f)
         {
            return ObjectValueConversion.AsNullableFloat(val, dflt);
         }


         public static decimal AsDecimal(this string val, decimal dflt = 0m)
         {
            return ObjectValueConversion.AsDecimal(val, dflt);
         }


         public static decimal? AsNullableDecimal(this string val, decimal? dflt = 0m)
         {
            return ObjectValueConversion.AsNullableDecimal(val, dflt);
         }


         public static bool AsBool(this string val, bool dflt = false)
         {
            return ObjectValueConversion.AsBool(val, dflt);
         }

         public static bool? AsNullableBool(this string val, bool? dflt = false)
         {
            return ObjectValueConversion.AsNullableBool(val, dflt);
         }


         public static Guid AsGUID(this string val, Guid dflt)
         {
            return ObjectValueConversion.AsGUID(val, dflt);
         }

         public static Guid? AsNullableGUID(this string val, Guid? dflt = null)
         {
            return ObjectValueConversion.AsNullableGUID(val, dflt);
         }

         public static DateTime AsDateTimeOrThrow(this string val)
         {
            return ObjectValueConversion.AsDateTime(val);
         }

         public static DateTime AsDateTime(this string val, DateTime dflt)
         {
            return ObjectValueConversion.AsDateTime(val, dflt);
         }

         public static DateTime AsDateTimeFormat(this string val, DateTime dflt, string fmt, DateTimeStyles fmtStyles = DateTimeStyles.None)
         {
            DateTime result;
            return DateTime.TryParseExact(val, fmt, null, fmtStyles, out result) ? result : dflt;
         }

         public static DateTime? AsNullableDateTime(this string val, DateTime? dflt = null)
         {
            return ObjectValueConversion.AsNullableDateTime(val, dflt);
         }


         public static TimeSpan AsTimeSpanOrThrow(this string val)
         {
            return ObjectValueConversion.AsTimeSpan(val, TimeSpan.FromSeconds(0), ConvertErrorHandling.Throw);
         }

         public static TimeSpan AsTimeSpan(this string val, TimeSpan dflt)
         {
            return ObjectValueConversion.AsTimeSpan(val, dflt);
         }

         public static TimeSpan? AsNullableTimeSpan(this string val, TimeSpan? dflt = null)
         {
            return ObjectValueConversion.AsNullableTimeSpan(val, dflt);
         }

         public static TEnum AsEnum<TEnum>(this string val, TEnum dflt = default(TEnum)) where TEnum : struct
         {
            return ObjectValueConversion.AsEnum<TEnum>(val, dflt);
         }

         public static TEnum? AsNullableEnum<TEnum>(this string val, TEnum? dflt = null) where TEnum : struct
         {
            return ObjectValueConversion.AsNullableEnum<TEnum>(val, dflt);
         }

         public static Uri AsUri(this string val, Uri dflt = null)
         {
            return ObjectValueConversion.AsUri(val, dflt);
         }


              private static Dictionary<Type, Func<string, bool, object>> s_CONV = new Dictionary<Type,Func<string,bool,object>>
              {
                   {typeof(object)   , (val, strict) => val },
                   {typeof(string)   , (val, strict) => val },
                   {typeof(int)      , (val, strict) => strict ? int.Parse(val)   : AsInt(val)  },
                   {typeof(uint)     , (val, strict) => strict ? uint.Parse(val)  : AsUInt(val)  },
                   {typeof(long)     , (val, strict) => strict ? long.Parse(val)  : AsLong(val) },
                   {typeof(ulong)    , (val, strict) => strict ? ulong.Parse(val) : AsULong(val) },
                   {typeof(short)    , (val, strict) => strict ? short.Parse(val) : AsShort(val)},
                   {typeof(ushort)   , (val, strict) => strict ? ushort.Parse(val): AsUShort(val)},
                   {typeof(byte)     , (val, strict) => strict ? byte.Parse(val)  : AsByte(val) },
                   {typeof(sbyte)    , (val, strict) => strict ? sbyte.Parse(val) : AsSByte(val) },
                   {typeof(bool)     , (val, strict) => strict ? bool.Parse(val)  : AsBool(val) },
                   {typeof(float)    , (val, strict) => strict ? float.Parse(val)    : AsFloat(val) },
                   {typeof(double)   , (val, strict) => strict ? double.Parse(val)   : AsDouble(val) },
                   {typeof(decimal)  , (val, strict) => strict ? decimal.Parse(val)  : AsDecimal(val) },
                   {typeof(TimeSpan) , (val, strict) => strict ? TimeSpan.Parse(val) : AsTimeSpanOrThrow(val) },
                   {typeof(DateTime) , (val, strict) => strict ? DateTime.Parse(val) : AsDateTimeOrThrow(val) },
                   {typeof(GDID)     , (val, strict) => strict ? GDID.Parse(val) : AsGDID(val) },
                   {typeof(GDIDSymbol),
                                         (val, strict) =>
                                         {
                                           if (strict)
                                           {
                                             var gdid = GDID.Parse(val);
                                             return new GDIDSymbol(gdid, val);
                                           }
                                           return  AsGDIDSymbol(val);
                                         }},
                   {typeof(Guid)     , (val, strict) => strict ? Guid.Parse(val) : AsGUID(val, Guid.Empty) },
                   {typeof(byte[])   , (val, strict) => AsByteArray(val)},

                   {typeof(int?),      (val, strict) => string.IsNullOrWhiteSpace(val) ? (int?)null      : (strict ? int.Parse(val)   : AsInt(val)) },
                   {typeof(uint?),     (val, strict) => string.IsNullOrWhiteSpace(val) ? (uint?)null     : (strict ? uint.Parse(val)  : AsUInt(val)) },
                   {typeof(long?),     (val, strict) => string.IsNullOrWhiteSpace(val) ? (long?)null     : (strict ? long.Parse(val)  : AsLong(val)) },
                   {typeof(ulong?),    (val, strict) => string.IsNullOrWhiteSpace(val) ? (ulong?)null    : (strict ? ulong.Parse(val) : AsULong(val)) },
                   {typeof(short?),    (val, strict) => string.IsNullOrWhiteSpace(val) ? (short?)null    : (strict ? short.Parse(val) : AsShort(val)) },
                   {typeof(ushort?),   (val, strict) => string.IsNullOrWhiteSpace(val) ? (ushort?)null   : (strict ? ushort.Parse(val): AsUShort(val)) },
                   {typeof(byte?),     (val, strict) => string.IsNullOrWhiteSpace(val) ? (byte?)null     : (strict ? byte.Parse(val)  : AsByte(val)) },
                   {typeof(sbyte?),    (val, strict) => string.IsNullOrWhiteSpace(val) ? (sbyte?)null    : (strict ? sbyte.Parse(val) : AsSByte(val)) },
                   {typeof(bool?),     (val, strict) => string.IsNullOrWhiteSpace(val) ? (bool?)null     : (strict ? bool.Parse(val)  : AsBool(val)) },
                   {typeof(float?),    (val, strict) => string.IsNullOrWhiteSpace(val) ? (float?)null    : (strict ? float.Parse(val)    : AsFloat(val)) },
                   {typeof(double?),   (val, strict) => string.IsNullOrWhiteSpace(val) ? (double?)null   : (strict ? double.Parse(val)   : AsDouble(val)) },
                   {typeof(decimal?),  (val, strict) => string.IsNullOrWhiteSpace(val) ? (decimal?)null  : (strict ? decimal.Parse(val)  : AsDecimal(val)) },
                   {typeof(TimeSpan?), (val, strict) => string.IsNullOrWhiteSpace(val) ? (TimeSpan?)null : (strict ? TimeSpan.Parse(val) : AsNullableTimeSpan(val)) },
                   {typeof(DateTime?), (val, strict) => string.IsNullOrWhiteSpace(val) ? (DateTime?)null : (strict ? DateTime.Parse(val) : AsNullableDateTime(val)) },
                   {typeof(GDID?),     (val, strict) => string.IsNullOrWhiteSpace(val) ? (GDID?)null     : (strict ? GDID.Parse(val) : AsGDID(val)) },
                   {typeof(GDIDSymbol?),
                                       (val, strict) =>
                                       {
                                         if (string.IsNullOrWhiteSpace(val)) return (GDIDSymbol?)null;
                                         if (strict)
                                         {
                                           var gdid = GDID.Parse(val);
                                           return new GDIDSymbol(gdid, val);
                                         }
                                         return AsGDIDSymbol(val);
                                        }},
                   {typeof(Guid?),     (val, strict) => string.IsNullOrWhiteSpace(val) ? (Guid?)null     : (strict ? Guid.Parse(val) : AsGUID(val, Guid.Empty)) },
                   {typeof(Uri),       (val, strict) => string.IsNullOrWhiteSpace(val) ? (Uri)null       : (strict ? new Uri(val) : AsUri(val)) }
              };




              /// <summary>
              /// Tries to get a string value as specified type.
              /// When 'strict=false', tries to do some inference like return "true" for numbers that dont equal to zero etc.
              /// When 'strict=true' throws an exception if deterministic conversion is not possible
              /// </summary>
              public static object AsType(this string val, Type tp, bool strict = true)
              {
                try
                {
                    Func<string, bool, object> func = null;
                    if (s_CONV.TryGetValue(tp, out func)) return func(val, strict);

                    if (tp.IsEnum)
                    {
                      return Enum.Parse(tp, val, true);
                    }

                    if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                          var v = val;
                          if (string.IsNullOrWhiteSpace(v)) return null;


                          var gargs = tp.GetGenericArguments();
                          if (gargs.Length==1)
                          {
                                var gtp = gargs[0];
                                if (gtp.IsEnum)
                                {
                                    return Enum.Parse(gtp, v, true);
                                }
                          }
                    }

                }
                catch(Exception error)
                {
                  throw new NFXException(string.Format(StringConsts.STRING_VALUE_COULD_NOT_BE_GOTTEN_AS_TYPE_ERROR,
                                                        val ?? StringConsts.NULL_STRING, tp.FullName), error);
                }

                throw new NFXException(string.Format(StringConsts.STRING_VALUE_COULD_NOT_BE_GOTTEN_AS_TYPE_ERROR,
                                                        val ?? StringConsts.NULL_STRING, tp.FullName));
              }


    }
}
