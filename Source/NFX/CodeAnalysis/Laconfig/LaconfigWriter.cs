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
using NFX.Environment;


namespace NFX.CodeAnalysis.Laconfig
{
    
    /// <summary>
    /// Specifies how Laconic configuration should be written as text. Use LaconfigWritingOptions.Compact or LaconfigWritingOptions.PrettyPrint
    ///  static properties for typical options
    /// </summary>
    public class LaconfigWritingOptions
    {
        private static LaconfigWritingOptions s_Compact = new LaconfigWritingOptions(0, false, false);
        private static LaconfigWritingOptions s_PrettyPrint = new LaconfigWritingOptions(2, true, true);

        /// <summary>
        /// Writes Laconfig without line breaks between members and no indenting. Suitable for data transmission
        /// </summary>
        public static LaconfigWritingOptions Compact { get { return s_Compact;} }

        /// <summary>
        /// Writes Laconfig suitable for printing/screen display
        /// </summary>
        public static LaconfigWritingOptions PrettyPrint { get { return s_PrettyPrint;} }


        public readonly int IndentWidth;
        public readonly bool SectionLineBreak;
        public readonly bool AttributeLineBreak;

        public LaconfigWritingOptions(int indent, bool sectionLineBreak, bool attrLineBreak)
        {
            IndentWidth = indent;
            SectionLineBreak = sectionLineBreak;
            AttributeLineBreak = attrLineBreak;
        }
    }
    
    
    
    /// <summary>
    /// Writes Laconic Configuration into a stream or string
    /// </summary>
    public static class LaconfigWriter
    {
        
        /// <summary>
        /// Writes LaconicConfiguration data to the stream
        /// </summary>
        public static void Write(Configuration data, Stream stream, LaconfigWritingOptions options = null, Encoding encoding = null)
        {
            Write(data.Root, stream, options, encoding);
        }

        /// <summary>
        /// Writes LaconicConfiguration data to the stream
        /// </summary>
        public static void Write(IConfigSectionNode data, Stream stream, LaconfigWritingOptions options = null, Encoding encoding = null)
        {
            using(var writer = new StreamWriter(stream, encoding ?? UTF8Encoding.UTF8))
                writer.Write( Write(data, options) );
        }

        
        /// <summary>
        /// Writes LaconicConfiguration data to the string
        /// </summary>
        public static string Write(Configuration data, LaconfigWritingOptions options = null)
        {
            return Write(data.Root, options);
        }

        /// <summary>
        /// Writes LaconicConfiguration data to the string
        /// </summary>
        public static string Write(IConfigSectionNode data, LaconfigWritingOptions options = null)
        {
            if (options==null) options = LaconfigWritingOptions.PrettyPrint;

            var sb = new StringBuilder();

            writeSection(sb, data, 0, options);

            return sb.ToString();
        }


        /// <summary>
        /// Appends LaconicConfiguration data into the instance of StringBuilder
        /// </summary>
        public static void Write(Configuration data, StringBuilder sb, LaconfigWritingOptions options = null)
        {
            Write(data.Root, sb, options);
        }

        /// <summary>
        /// Appends LaconicConfiguration data into the instance of StringBuilder
        /// </summary>
        public static void Write(IConfigSectionNode data, StringBuilder sb, LaconfigWritingOptions options = null)
        {
            if (options==null) options = LaconfigWritingOptions.PrettyPrint;

            writeSection(sb, data, 0, options);
        }


        /// <summary>
        /// Returns a padding string for specified level per set options
        /// </summary>
        public static string Indent(int level, LaconfigWritingOptions opt)
        {
            if (opt.IndentWidth>0) return string.Empty.PadLeft(level * opt.IndentWidth);

            return string.Empty;
        }


                    
                #region .pvt .impl
                    
                        private static void writeSection(StringBuilder sb, IConfigSectionNode section, int level, LaconfigWritingOptions opt)
                        {
                            if (opt.SectionLineBreak)
                            { 
                              sb.AppendLine();
                              sb.Append(Indent(level, opt));
                            }
                            else sb.Append(' ');
                           
                            writeString(sb, section.Name);   
                            var value = section.VerbatimValue;
                            if (value.IsNotNullOrWhiteSpace())
                            {
                                sb.Append("=");
                                writeString(sb, value);
                            }

                            if (opt.SectionLineBreak)
                            { 
                              sb.AppendLine();
                              sb.Append(Indent(level, opt));
                            }
                            sb.Append('{');

                            foreach(var anode in section.Attributes)
                            {
                                if (opt.AttributeLineBreak)
                                { 
                                  sb.AppendLine();
                                  sb.Append(Indent(level+1, opt));
                                }
                                else sb.Append(' ');

                                writeString(sb, anode.Name);   
                                sb.Append("=");
                                writeString(sb, anode.VerbatimValue);
                            }
                            sb.Append(' ');
                            foreach(var csect in section.Children)
                            {
                                writeSection(sb, csect, level+1, opt);
                            }

                            if (opt.SectionLineBreak)
                            { 
                              sb.AppendLine();
                              sb.Append(Indent(level, opt));
                            }
                            sb.Append('}');
                        }


                        private static void writeString(StringBuilder sb, string data)
                        {
                          if (data.IsNullOrEmpty())
                          {
                            sb.Append("''");
                            return;
                          }

                          var quotes = false;

                          StringBuilder buf = new StringBuilder();
                          for (int i = 0; i < data.Length; i++)
                          {
                                char c = data[i];
                                if (c>127)
                                {
                                  buf.Append("\\u");
                                  buf.Append(((int)c).ToString("x4"));
                                  quotes = true;
                                  continue;
                                }

                                switch (c)
                                {
                                    case '\\':  { buf.Append(@"\\"); quotes = true; break; }
                                    case (char)CharCodes.Char0:     { buf.Append(@"\0"); quotes = true; break; }
                                    case (char)CharCodes.AlertBell: { buf.Append(@"\a"); quotes = true; break; }
                                    case (char)CharCodes.Backspace: { buf.Append(@"\b"); quotes = true; break; }
                                    case (char)CharCodes.Formfeed:  { buf.Append(@"\f"); quotes = true; break; }
                                    case (char)CharCodes.LF:        { buf.Append(@"\n"); quotes = true; break; }
                                    case (char)CharCodes.CR:        { buf.Append(@"\r"); quotes = true; break; }
                                    case (char)CharCodes.Tab:       { buf.Append(@"\t"); quotes = true; break; }
                                    case (char)CharCodes.VerticalQuote: { buf.Append(@"\v"); quotes = true; break; }
                                    
                                    case '"':  { buf.Append(@"\"""); quotes = true; break; }

                                    case ' ':
                                    case '{':
                                    case '}':
                                    case '=': { buf.Append(c); quotes = true; break;}
                                    
                                    default: { buf.Append(c); break;}
                                }

                          }//for
                          
                          if (quotes)
                          {
                                sb.Append('"');
                                sb.Append(buf);
                                sb.Append('"');
                          }
                          else
                                sb.Append(buf);
                          
                        }





                #endregion
    }
}

