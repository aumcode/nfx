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

using NFX.Environment;
using NFX.DataAccess.Distributed;

namespace NFX
{
    /// <summary>
    /// Specifies how to handle errors during object value conversion
    /// </summary>
    public enum ConvertErrorHandling { ReturnDefault=0, Throw}

    /// <summary>
    /// Provides extension methods for converting object values to different scalar types
    /// </summary>
    public static class ObjectValueConversion
    {
        public const string RADIX_BIN = "0b";
        public const string RADIX_HEX = "0x";


         public static string AsString(this object val, string dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                return Convert.ToString(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static string AsNonNullOrEmptyString(this object val)
         {
              try
              {
                if (val==null) throw new NFXException("arg = null");

                var result = Convert.ToString(val);

                if (result.IsNullOrWhiteSpace())
                  throw new NFXException("result = null|empty");

                return result;
              }
              catch(Exception error)
              {
                throw new NFXException("AsNonNullOrEmptyString({0})".Args(error.ToMessageWithType()));
              }
         }


         public static ConfigSectionNode AsLaconicConfig(this object val, ConfigSectionNode dflt = null, string wrapRootName = "nfx", ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              string content;
              try
              {
                if (val==null) return dflt;
                content = val.ToString();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }

              try
              {
                return LaconicConfiguration.CreateFromString(content).Root;
              }
              catch
              {
                if (wrapRootName.IsNotNullOrWhiteSpace())
                  try
                  {
                    return LaconicConfiguration.CreateFromString(wrapRootName + "\n{\n" + content + "\n}").Root;
                  }
                  catch
                  {
                    if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                    return dflt;
                  }

                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static ConfigSectionNode AsJSONConfig(this object val, ConfigSectionNode dflt = null, string wrapRootName = "nfx", ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              string content;
              try
              {
                if (val==null) return dflt;
                content = val.ToString();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }

              try
              {
                return JSONConfiguration.CreateFromJSON(content).Root;
              }
              catch
              {
                if (wrapRootName.IsNotNullOrWhiteSpace())
                  try
                  {
                    return JSONConfiguration.CreateFromJSON("{'"+ wrapRootName +"':\n" + content + "\n}").Root;
                  }
                  catch
                  {
                    if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                    return dflt;
                  }

                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static ConfigSectionNode AsXMLConfig(this object val, ConfigSectionNode dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                var content = val.ToString();

                return XMLConfiguration.CreateFromXML(content).Root;
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }



         public static char AsChar(this object val, char dflt = (char)0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
           try
           {
             if (val == null) return dflt;
             if (val is string)
             {
                  var sval = (string)val;
                  return (sval.Length>0) ? sval[0] : (char)0;
             }
             return Convert.ToChar(val);
           }
           catch
           {
             if (handling != ConvertErrorHandling.ReturnDefault) throw;
             return dflt;
           }
         }


         public static char? AsNullableChar(this object val, char? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
           try
           {
             if (val == null) return null;
             if (val is string)
             {
                  var sval = (string)val;
                  return (sval.Length>0) ? sval[0] : (char)0;
             }
             return Convert.ToChar(val);
           }
           catch
           {
             if (handling != ConvertErrorHandling.ReturnDefault) throw;
             return dflt;
           }
         }



         public static byte AsByte(this object val, byte dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
           try
           {
             if (val == null) return dflt;
             if (val is string)
             {
                  var sval = ((string)val).Trim();
                  if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToByte(sval.Substring(2), 2);
                  if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToByte(sval.Substring(2), 16);
             }
             return Convert.ToByte(val);
           }
           catch
           {
             if (handling != ConvertErrorHandling.ReturnDefault) throw;
             return dflt;
           }
         }


         public static byte? AsNullableByte(this object val, byte? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
           try
           {
             if (val == null) return null;
             if (val is string)
             {
                  var sval = ((string)val).Trim();
                  if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToByte(sval.Substring(2), 2);
                  if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToByte(sval.Substring(2), 16);
             }
             return Convert.ToByte(val);
           }
           catch
           {
             if (handling != ConvertErrorHandling.ReturnDefault) throw;
             return dflt;
           }
         }

         public static sbyte AsSByte(this object val, sbyte dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
           try
           {
             if (val == null) return dflt;
             return Convert.ToSByte(val);
           }
           catch
           {
             if (handling != ConvertErrorHandling.ReturnDefault) throw;
             return dflt;
           }
         }


         public static sbyte? AsNullableSByte(this object val, sbyte? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
           try
           {
             if (val == null) return null;
             return Convert.ToSByte(val);
           }
           catch
           {
             if (handling != ConvertErrorHandling.ReturnDefault) throw;
             return dflt;
           }
         }

         public static short AsShort(this object val, short dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt16(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt16(sval.Substring(2), 16);
                }
                return Convert.ToInt16(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static short? AsNullableShort(this object val, short? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt16(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt16(sval.Substring(2), 16);
                }
                return Convert.ToInt16(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }





         public static ushort AsUShort(this object val, ushort dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt16(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt16(sval.Substring(2), 16);
                }
                return Convert.ToUInt16(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static ushort? AsNullableUShort(this object val, ushort? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt16(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt16(sval.Substring(2), 16);
                }
                return Convert.ToUInt16(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }




         public static int AsInt(this object val, int dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt32(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt32(sval.Substring(2), 16);
                }
                return Convert.ToInt32(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static int? AsNullableInt(this object val, int? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt32(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt32(sval.Substring(2), 16);
                }
                return Convert.ToInt32(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }







         public static uint AsUInt(this object val, uint dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt32(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt32(sval.Substring(2), 16);
                }
                return Convert.ToUInt32(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static uint? AsNullableUInt(this object val, uint? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt32(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt32(sval.Substring(2), 16);
                }
                return Convert.ToUInt32(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }




         public static long AsLong(this object val, long dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt64(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt64(sval.Substring(2), 16);
                }
                return Convert.ToInt64(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static long? AsNullableLong(this object val, long? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt64(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToInt64(sval.Substring(2), 16);
                }
                return Convert.ToInt64(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }




         public static ulong AsULong(this object val, ulong dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt64(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt64(sval.Substring(2), 16);
                }
                return Convert.ToUInt64(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static ulong? AsNullableULong(this object val, ulong? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                if (val is string)
                {
                   var sval = ((string)val).Trim();
                   if (sval.StartsWith(RADIX_BIN, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt64(sval.Substring(2), 2);
                   if (sval.StartsWith(RADIX_HEX, StringComparison.InvariantCultureIgnoreCase)) return Convert.ToUInt64(sval.Substring(2), 16);
                }
                return Convert.ToUInt64(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }





         public static double AsDouble(this object val, double dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                return Convert.ToDouble(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static double? AsNullableDouble(this object val, double? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return Convert.ToDouble(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static float AsFloat(this object val, float dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                return (float)Convert.ToDouble(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static float? AsNullableFloat(this object val, float? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return (float)Convert.ToDouble(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static decimal AsDecimal(this object val, decimal dflt = 0, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                return Convert.ToDecimal(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static decimal? AsNullableDecimal(this object val, decimal? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return Convert.ToDecimal(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         /// <summary>
         /// A "hack" enum used to provide tri-state checkbox functionality in some systems, i.e. HTML checkmarks
         /// do not understand "nulls". This is a surrogate type not used in server-side programming
         /// </summary>
         public enum TriStateBool { Unspecified = 0, False = 1, True = 2}

         public static bool? AsNullableBool(this TriStateBool val)
         {
           return val==TriStateBool.Unspecified ? null : val==TriStateBool.True ? (bool?)true : (bool?)false;
         }


         public static bool AsBool(this object val, bool dflt = false, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;

                if (val is bool) return (bool)val;
                else if (val is string)
                {
                    var sval = ((string)val).Trim();

                    if (string.Equals("true", sval, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals("yes",  sval, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals("t",    sval, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals("y",    sval, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals("on",   sval, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals("ok",   sval, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals("pass", sval, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals("1", sval, StringComparison.InvariantCultureIgnoreCase)
                       ) return true;

                    long ival;
                    if (long.TryParse(sval, out ival)) return ival!=0;

                    double dval;
                    if (double.TryParse(sval, out dval)) return dval!=0;

                    decimal dcval;
                    if (decimal.TryParse(sval, out dcval)) return dcval!=0;
                }
                else if (val is TriStateBool) { return ((TriStateBool)val)==TriStateBool.True; }
                else if (val is char)   { var c = (char)val; if ( c=='T' || c=='t' || c=='Y' || c=='y' || c=='1')  return true; }
                else if (val is int)    { if ( (int)val != 0)     return true; }
                else if (val is double) { if ( (double)val != 0)  return true; }
                else if (val is decimal){ if ( (decimal)val != 0) return true; }
                else if (val is TimeSpan){ if ( ((TimeSpan)val).Ticks != 0) return true; }
                else if (val is DateTime){ if ( ((DateTime)val).Ticks != 0) return true; }


                return Convert.ToBoolean(val);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static bool? AsNullableBool(this object val, bool? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;

                if (val is TriStateBool) return ((TriStateBool)val).AsNullableBool();

                return val.AsBool(false, ConvertErrorHandling.Throw);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }











         public static Guid AsGUID(this object val, Guid dflt, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;

                if (val is string)
                {
                    var sval = (string)val;

                    return Guid.Parse(sval);
                }
                else if (val is byte[])
                {
                    var arr = (byte[])val;
                    return new Guid(arr);
                }
                else
                    return (Guid)val;
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static Guid? AsNullableGUID(this object val, Guid? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return val.AsGUID(dflt ?? Guid.Empty, ConvertErrorHandling.Throw);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }















         public static DateTime AsDateTime(this object val)
         {
            if (val is string)
            {
                    var sval = ((string)val).Trim();


                    long ival;
                    if (long.TryParse(sval, out ival)) return new DateTime(ival);

                    double dval;
                    if (double.TryParse(sval, out dval)) return new DateTime((long)dval);

                    decimal dcval;
                    if (decimal.TryParse(sval, out dcval)) return new DateTime((long)dcval);
            }

            if (val is int) { return new DateTime((int)val); }
            if (val is long) { return new DateTime((long)val); }
            if (val is double) { return new DateTime((long)((double)val)); }
            if (val is float) { return new DateTime((long)((float)val)); }
            if (val is decimal) { return new DateTime((long)((decimal)val)); }
            return Convert.ToDateTime(val);
         }


         public static DateTime AsDateTime(this object val, DateTime dflt, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                return val.AsDateTime();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static DateTime? AsNullableDateTime(this object val, DateTime? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return val.AsDateTime();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static GDID AsGDID(this object val)
         {
            if (val==null) return GDID.Zero;

            if (val is GDID) return (GDID)val;

            if (val is ELink) return ((ELink)val).GDID;

            if (val is string)
            {
                var sval = ((string)val).Trim();

                GDID gdid;
                if (GDID.TryParse(sval, out gdid)) return gdid;

                //20160109 DKH also interpret string via ELink
                try
                {
                  var elink = new ELink(sval);
                  return elink.GDID;
                }
                catch{}
            }

            if (val is ulong) { return new GDID(0,(ulong)val); }
            if (val is byte[]) { return new GDID((byte[])val); }
            return new GDID(0, Convert.ToUInt64(val));
         }

         public static GDID AsGDID(this object val, GDID dflt, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                return val.AsGDID();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static GDID? AsNullableGDID(this object val, GDID? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return val.AsGDID();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         //20160622 DKh
         public static GDIDSymbol AsGDIDSymbol(this object val)
         {
            if (val==null) return new GDIDSymbol();

            if (val is GDIDSymbol) return (GDIDSymbol)val;
            if (val is GDID) return new GDIDSymbol((GDID)val, val.ToString());

            if (val is ELink) return ((ELink)val).AsGDIDSymbol;

            if (val is string)
            {
                var sval = ((string)val).Trim();

                GDID gdid;
                if (GDID.TryParse(sval, out gdid)) return new GDIDSymbol(gdid, sval);

                try
                {
                  var elink = new ELink(sval);
                  return elink.AsGDIDSymbol;
                }
                catch{}
            }

            if (val is ulong) { return new GDIDSymbol( new GDID(0,(ulong)val), val.ToString()); }
            if (val is byte[])
            {
              var buf = (byte[])val;
              return new GDIDSymbol( new GDID(buf), buf.ToDumpString(DumpFormat.Hex));
            }
            return new GDIDSymbol(new GDID(0, Convert.ToUInt64(val)), val.ToString());
         }


         public static GDIDSymbol AsGDIDSymbol(this object val, GDIDSymbol dflt, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;
                return val.AsGDIDSymbol();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static GDIDSymbol? AsNullableGDIDSymbol(this object val, GDIDSymbol? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return val.AsGDIDSymbol();
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }



         public static TimeSpan AsTimeSpan(this object val)
         {
            return val.AsTimeSpan(TimeSpan.FromSeconds(0), ConvertErrorHandling.Throw);
         }

         public static TimeSpan AsTimeSpan(this object val, TimeSpan dflt, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return dflt;

                if (val is string)
                {
                    var sval = (string)val;

                    long ival;
                    if (long.TryParse(sval, out ival)) return new TimeSpan(ival);

                    double dval;
                    if (double.TryParse(sval, out dval)) return new TimeSpan((long)dval);

                    decimal dcval;
                    if (decimal.TryParse(sval, out dcval)) return new TimeSpan((long)dcval);

                    TimeSpan tsval;
                    if (TimeSpan.TryParse(sval, out tsval)) return tsval;
                }

                var ticks = Convert.ToInt64(val);

                return new TimeSpan(ticks);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static TimeSpan? AsNullableTimeSpan(this object val, TimeSpan? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
              try
              {
                if (val==null) return null;
                return val.AsTimeSpan(TimeSpan.FromSeconds(0), ConvertErrorHandling.Throw);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static TEnum AsEnum<TEnum>(this object val, TEnum dflt, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         where TEnum : struct
         {
              try
              {
                if (val==null) return dflt;

                if (val is string)
                {
                    var sval = (string)val;

                    return (TEnum)Enum.Parse(typeof(TEnum), sval, true);
                }

                val = Convert.ToInt32(val);
                return (TEnum)val;
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }


         public static TEnum? AsNullableEnum<TEnum>(this object val, TEnum? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         where TEnum : struct
         {
              try
              {
                if (val==null) return null;
                return val.AsEnum( default(TEnum), ConvertErrorHandling.Throw);
              }
              catch
              {
                if (handling!=ConvertErrorHandling.ReturnDefault) throw;
                return dflt;
              }
         }

         public static Uri AsUri(this object val, Uri dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
         {
           try
           {
             if (val == null) return null;
             if (val is Uri) return (Uri)val;
             if (val is string)
             {
               var uri = (string)val;
               if (uri.IsNullOrWhiteSpace()) return null;
               return new Uri(uri);
             }
             throw new NFXException("{0}.AsUri".Args(val.GetType().Name));
           }
           catch
           {
             if (handling!=ConvertErrorHandling.ReturnDefault) throw;
             return dflt;
           }
         }
    }
}
