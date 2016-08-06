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
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using NFX.Parsing;

namespace NFX.Serialization.JSON
{
    /// <summary>
    /// Writes primitive types, JSONDataObjects, JSONDynamicObjects, IEnumerable and IDictionary - implementers into string or stream.
    /// Can also write IJSONWritable-implementing types that directly serialize their state into JSON.
    /// This class does not serialize regular CLR types (that do not implement IJSONWritable), use JSONSerializer for full functionality
    /// </summary>
    public static class JSONWriter
    {

        /// <summary>
        /// Writes JSON data to the file
        /// </summary>
        public static void WriteToFile(object data, string  fileName, JSONWritingOptions options = null, Encoding encoding = null)
        {
            using(var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
               Write(data, fs, options, encoding);
        }


        /// <summary>
        /// Writes JSON data to the byte[]
        /// </summary>
        public static byte[] WriteToBuffer(object data, JSONWritingOptions options = null, Encoding encoding = null)
        {
            using(var ms = new MemoryStream())
            {
              Write(data, ms, options, encoding);
              return ms.ToArray();
            }
        }


        /// <summary>
        /// Writes JSON data to the stream
        /// </summary>
        public static void Write(object data, Stream stream, JSONWritingOptions options = null, Encoding encoding = null)
        {
            using(var writer = new StreamWriter(stream, encoding ?? UTF8Encoding.UTF8))
               Write(data, writer, options);
        }

        /// <summary>
        /// Writes JSON data to the string
        /// </summary>
        public static string Write(object data, JSONWritingOptions options = null, IFormatProvider formatProvider = null)
        {
            if (options==null) options = JSONWritingOptions.Compact;

            var sb = new StringBuilder(0xff);
            using( var wri =  formatProvider==null ?
                                  new StringWriter( sb ) :
                                  new StringWriter( sb, formatProvider ) )
            {
                writeAny(wri, data, 0, options);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Appends JSON data into the instance of StringBuilder
        /// </summary>
        public static void Write(object data, TextWriter wri, JSONWritingOptions options = null)
        {
            if (options==null) options = JSONWritingOptions.Compact;

            writeAny(wri, data, 0, options);
        }

        /// <summary>
        /// Appends JSON representation of a map(IDictionary)
        /// </summary>
        public static void WriteMap(TextWriter wri, IDictionary data, int level, JSONWritingOptions options = null)
        {
            if (options==null) options = JSONWritingOptions.Compact;

            writeMap(wri, data, level, options);
        }

        /// <summary>
        /// Appends JSON representation of a map(IEnumerable(DictionaryEntry))
        /// </summary>
        public static void WriteMap(TextWriter wri, IEnumerable<DictionaryEntry> data, int level, JSONWritingOptions options = null)
        {
            if (options==null) options = JSONWritingOptions.Compact;

            writeMap(wri, data, level, options);
        }

        /// <summary>
        /// Appends JSON representation of a map(IEnumerable(DictionaryEntry))
        /// </summary>
        public static void WriteMap(TextWriter wri, int level, JSONWritingOptions options, params DictionaryEntry[] data)
        {
            if (options==null) options = JSONWritingOptions.Compact;

            writeMap(wri, data, level, options);
        }

        /// <summary>
        /// Appends JSON representation of an IEnumerable
        /// </summary>
        public static void WriteArray(TextWriter wri, IEnumerable data, int level, JSONWritingOptions options)
        {
            if (options==null) options = JSONWritingOptions.Compact;

            writeArray(wri, data, level, options);
        }






        /// <summary>
        /// Writes a string in JSON format (a la "JSON encode string") - using quotes and escaping charecters that need it
        /// </summary>
        /// <param name="wri">TextWriter instance to append data into</param>
        /// <param name="data">Original string to encode as JSON</param>
        /// <param name="opt">JSONWriting options instance, if omitted then JSONWritingOptions.Compact is used</param>
        public static void EncodeString(TextWriter wri, string data, JSONWritingOptions opt = null)
        {
            if (data.IsNullOrEmpty())
            {
                wri.Write("\"\"");
                return;
            }

            if (opt==null)
                opt = JSONWritingOptions.Compact;

            wri.Write('"');

            for (int i = 0; i < data.Length; i++)
            {
                char c = data[i];
                if (c>127 && opt.ASCIITarget)
                {
                    wri.Write("\\u");
                    wri.Write(((int)c).ToString("x4"));
                    continue;
                }

                switch (c)
                {
                    case '\\':  { wri.Write(@"\\"); break; }
                    case '/':   { wri.Write(@"\/"); break; }
                    case (char)CharCodes.Char0:     { wri.Write(@"\u0000"); break; }
                    case (char)CharCodes.AlertBell: { wri.Write(@"\u"); ((int)c).ToString("x4"); break; }
                    case (char)CharCodes.Backspace: { wri.Write(@"\b"); break; }
                    case (char)CharCodes.Formfeed:  { wri.Write(@"\f"); break; }
                    case (char)CharCodes.LF:        { wri.Write(@"\n"); break; }
                    case (char)CharCodes.CR:        { wri.Write(@"\r"); break; }
                    case (char)CharCodes.Tab:       { wri.Write(@"\t"); break; }
                    case (char)CharCodes.VerticalQuote: { wri.Write(@"\u"); ((int)c).ToString("x4"); break; }

                    case '"':  { wri.Write(@"\"""); break; }

                    default: { wri.Write(c); break;}
                }

            }//for

            wri.Write('"');
        }

        /// <summary>
        /// Writes a string in JSON format (a la "JSON encode string") - using quotes and escaping charecters that need it
        /// </summary>
        /// <param name="wri">TextWriter instance to append data into</param>
        /// <param name="data">Original string to encode as JSON</param>
        /// <param name="opt">JSONWriting options instance, if omitted then JSONWritingOptions.Compact is used</param>
        /// <param name="utcOffset">UTC offset override. If not supplied then offset form local time zone is used</param>
        public static void EncodeDateTime(TextWriter wri, DateTime data, JSONWritingOptions opt = null, TimeSpan? utcOffset = null)
        {
            if (opt==null) opt = JSONWritingOptions.Compact;

            if (!opt.ISODates)
            {
                wri.Write("new Date({0})".Args( data.ToMillisecondsSinceUnixEpochStart() ));
                return;
            }

            wri.Write('"');
            var year = data.Year;
            if (year>999) wri.Write(year);
            else if (year>99) { wri.Write('0'); wri.Write(year); }
            else if (year>9) { wri.Write("00"); wri.Write(year); }
            else if (year>0) { wri.Write("000"); wri.Write(year); }

            wri.Write('-');

            var month = data.Month;
            if (month>9) wri.Write(month);
            else { wri.Write('0'); wri.Write(month); }

            wri.Write('-');

            var day = data.Day;
            if (day>9) wri.Write(day);
            else { wri.Write('0'); wri.Write(day); }

            wri.Write('T');

            var hour = data.Hour;
            if (hour>9) wri.Write(hour);
            else { wri.Write('0'); wri.Write(hour); }

            wri.Write(':');

            var minute = data.Minute;
            if (minute>9) wri.Write(minute);
            else { wri.Write('0'); wri.Write(minute); }

            wri.Write(':');

            var second = data.Second;
            if (second>9) wri.Write(second);
            else { wri.Write('0'); wri.Write(second); }

            var ms = data.Millisecond;
            if (ms>0)
            {
                wri.Write('.');

                if (ms>99) wri.Write(ms);
                else if (ms>9) { wri.Write('0'); wri.Write(ms); }
                else { wri.Write("00"); wri.Write(ms); }
            }

            if (data.Kind==DateTimeKind.Utc)
                wri.Write('Z');
            else
            {
                //var offset = utcOffset==null ? TimeZoneInfo.Local.BaseUtcOffset : utcOffset.Value;
                //dlat 2014/06/15
                var offset = utcOffset==null ? TimeZoneInfo.Local.GetUtcOffset(data) : utcOffset.Value;

                wri.Write( offset.Ticks<0 ? '-' : '+' );

                hour = Math.Abs(offset.Hours);
                if (hour>9) wri.Write(hour);
                else { wri.Write('0'); wri.Write(hour); }

                wri.Write(':');

                minute = Math.Abs(offset.Minutes);
                if (minute>9) wri.Write(minute);
                else { wri.Write('0'); wri.Write(minute); }
            }


            wri.Write('"');
        }


                #region .pvt .impl


                        private static void indent(TextWriter wri, int level, JSONWritingOptions opt)
                        {
                            if (opt.IndentWidth==0) return;

                            var total = level * opt.IndentWidth;
                            for(var i=0; i<total; i++)
                             wri.Write(' ');
                        }


                        private static void writeAny(TextWriter wri, object data, int level, JSONWritingOptions opt)
                        {
                            if (data==null)
                            {
                                wri.Write("null");
                                return;
                            }

                            if (level>opt.MaxNestingLevel)
                                throw new JSONSerializationException(StringConsts.JSON_SERIALIZATION_MAX_NESTING_EXCEEDED_ERROR.Args(opt.MaxNestingLevel));

                            if (data is string)
                            {
                                EncodeString(wri, (string)data, opt);
                                return;
                            }

                            if (data is bool)//the check is here for speed
                            {
                                wri.Write( ((bool)data) ? "true" : "false");//do NOT LOCALIZE!
                                return;
                            }

                            if (data is int || data is long)//the check is here for speed
                            {
                                wri.Write( ((IConvertible)data).ToString(System.Globalization.CultureInfo.InvariantCulture) );
                                //20140619 Dkh+Dlat wri.Write(data.ToString());
                                return;
                            }

                            if (data is double || data is float || data is decimal)//the check is here for speed
                            {
                                wri.Write( ((IConvertible)data).ToString(System.Globalization.CultureInfo.InvariantCulture) );
                                //20140619 Dkh+Dlat wri.Write(data.ToString());
                                return;
                            }

                            if (data is DateTime)
                            {
                                EncodeDateTime(wri, (DateTime)data, opt);
                                return;
                            }

                            if (data is TimeSpan)//20140619 Dlat
                            {
                                var ts = (TimeSpan)data;
                                wri.Write(ts.Ticks);
                                return;
                            }

                            if (data is IJSONWritable)//these types know how to directly write themselves
                            {
                                ((IJSONWritable)data).WriteAsJSON(wri, level, opt);
                                return;
                            }

                            if (data is Serialization.JSON.JSONDynamicObject)//unwrap dynamic
                            {
                                writeAny(wri, ((Serialization.JSON.JSONDynamicObject)data).Data, level, opt);
                                return;
                            }


                            if (data is IDictionary)//must be BEFORE IEnumerable
                            {
                                writeMap(wri, (IDictionary)data, level, opt);
                                return;
                            }

                            if (data is IEnumerable)
                            {
                                writeArray(wri, (IEnumerable)data, level, opt);
                                return;
                            }

                            var tdata = data.GetType();
                            if (tdata.IsPrimitive || tdata.IsEnum)
                            {
                                string val;
                                if (data is IConvertible)
                                  val = ((IConvertible)data).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                else
                                  val = data.ToString();

                                EncodeString(wri, val, opt);
                                return;
                            }

                            var fields = SerializationUtils.GetSerializableFields(tdata);

                            //20150620 DKh
                            //var dict = new Dictionary<string, object>();
                            //foreach(var f in fields)
                            //{
                            //    var name = f.Name;
                            //    var iop = name.IndexOf('<');
                            //    if (iop>=0)//handle anonymous type field name
                            //    {
                            //        var icl = name.IndexOf('>');
                            //        if (icl>iop+1)
                            //            name = name.Substring(iop+1, icl-iop-1);
                            //    }
                            //    dict.Add(name, f.GetValue(data));
                            //}

                            var dict = fields.Select(
                            f =>
                            {
                              var name = f.Name;
                              var iop = name.IndexOf('<');
                              if (iop>=0)//handle anonymous type field name
                              {
                                    var icl = name.IndexOf('>');
                                    if (icl>iop+1)
                                        name = name.Substring(iop+1, icl-iop-1);
                              }

                              return new DictionaryEntry(name, f.GetValue(data));
                            });//select


                            writeMap(wri, dict, level, opt);
                        }


                             //20150620 DKh
                             private struct dictEnumberable : IEnumerable<DictionaryEntry>
                             {
                               public dictEnumberable(IDictionary dict) { Dictionary = dict;}

                               private readonly IDictionary Dictionary;

                               public IEnumerator<DictionaryEntry> GetEnumerator()
                               {
                                 return new dictEnumerator(Dictionary.GetEnumerator());
                               }

                               IEnumerator IEnumerable.GetEnumerator()
                               {
                                 return Dictionary.GetEnumerator();
                               }
                             }

                             //20150620 DKh
                             private struct dictEnumerator : IEnumerator<DictionaryEntry>
                             {

                               public dictEnumerator(IDictionaryEnumerator enumerator) { Enumerator = enumerator;}

                               private readonly IDictionaryEnumerator Enumerator;

                               public DictionaryEntry Current { get { return (DictionaryEntry)Enumerator.Current; } }

                               public void Dispose() {}

                               object IEnumerator.Current { get { return Enumerator.Current; } }

                               public bool MoveNext() { return Enumerator.MoveNext(); }

                               public void Reset() { Enumerator.Reset();}
                             }


                        private static void writeMap(TextWriter wri, IDictionary data, int level, JSONWritingOptions opt)
                        {
                           writeMap(wri, new dictEnumberable(data), level, opt);
                        }

                        private static void writeMap(TextWriter wri, IEnumerable<DictionaryEntry> data, int level, JSONWritingOptions opt)
                        {
                            if (level>0) level++;

                            if (opt.ObjectLineBreak)
                            {
                              wri.WriteLine();
                              indent(wri, level, opt);
                            }

                            wri.Write('{');

                            var first = true;
                            foreach(DictionaryEntry entry in data)
                            {
                              //20160324 DKh
                              if (opt.MapSkipNulls && entry.Value==null) continue;

                              if (!first)
                                wri.Write(opt.SpaceSymbols ? ", " : ",");

                              if (opt.MemberLineBreak)
                              {
                                wri.WriteLine();
                                indent(wri, level+1, opt);
                              }
                              EncodeString(wri, entry.Key.ToString(), opt);
                              wri.Write(opt.SpaceSymbols ? ": " : ":");
                              writeAny(wri, entry.Value, level+1, opt);
                              first = false;
                            }

                            if (!first && opt.MemberLineBreak)
                            {
                              wri.WriteLine();
                              indent(wri, level, opt);
                            }

                            wri.Write('}');
                        }

                        private static void writeArray(TextWriter wri, IEnumerable data, int level, JSONWritingOptions opt)
                        {
                            wri.Write('[');

                            var first = true;
                            foreach(var elm in data)
                            {
                              if (!first)
                                wri.Write(opt.SpaceSymbols ? ", " : ",");
                              writeAny(wri, elm, level+1, opt);
                              first = false;
                            }

                            wri.Write(']');
                        }


                #endregion
    }
}
