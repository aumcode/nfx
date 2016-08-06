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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;


using NFX.Time;

namespace NFX.Environment
{

    /// <summary>
    /// Represents an entity that runs config var macros
    /// </summary>
    public interface IMacroRunner
    {
        /// <summary>
        /// Runs macro
        /// </summary>
        string Run(IConfigSectionNode node, string inputValue, string macroName, IConfigSectionNode macroParams, object context = null);
    }


    /// <summary>
    /// Provides default implementation for configuration variable macros.
    /// NOTE: When serialized a new instance is created which will not equal by reference to static.Instance property
    /// </summary>
    [Serializable]
    public class DefaultMacroRunner : IMacroRunner
    {
        #region CONSTS
            public const string AS_PREFIX = "as-";

        #endregion

        #region STATIC
            private static DefaultMacroRunner s_Instance = new DefaultMacroRunner();

            private DefaultMacroRunner()
            {

            }

            /// <summary>
            /// Returns a singleton class instance
            /// </summary>
            public static DefaultMacroRunner Instance
            {
              get { return s_Instance; }
            }

            /// <summary>
            /// Returns a string value converted to desired type with optional default and format
            /// </summary>
            /// <param name="value">String value to convert</param>
            /// <param name="type">A type to convert string value into i.e. "decimal"</param>
            /// <param name="dflt">Default value which is used when conversion of original value can not be made</param>
            /// <param name="fmt">Format string that formats the converted value. Example: 'Goods: {0}'. The '0' index is the value</param>
            /// <returns>Converted value to desired type then back to string, using optional formatting and default if conversion did not succeed</returns>
            public static string GetValueAs(string value, string type, string dflt = null, string fmt = null)
            {
              var mi = typeof(StringValueConversion).GetMethod("As"+type.CapitalizeFirstChar(), BindingFlags.Public | BindingFlags.Static);

              object result;
              if (!string.IsNullOrWhiteSpace(dflt))
              {
                  var dval = mi.Invoke(null, new object[] { dflt, null});

                  result = mi.Invoke(null, new object[] { value, dval});
              }
              else
                result = mi.Invoke(null, new object[] { value, null});


              if (result==null) return string.Empty;

              if (!string.IsNullOrWhiteSpace(fmt))
                return string.Format(fmt, result);
              else
                return result.ToString();
            }

        #endregion


        public string Run(IConfigSectionNode node, string inputValue, string macroName, IConfigSectionNode macroParams, object context = null)
        {

            if (macroName.StartsWith(AS_PREFIX, StringComparison.InvariantCultureIgnoreCase) && macroName.Length > AS_PREFIX.Length)
            {
               var type = macroName.Substring(AS_PREFIX.Length);

               return GetValueAs(inputValue,
                                 type,
                                 macroParams.Navigate("$dflt|$default").Value,
                                 macroParams.Navigate("$fmt|$format").Value);

            }
            else if (string.Equals(macroName, "now", StringComparison.InvariantCultureIgnoreCase))
            {
               var utc = macroParams.AttrByName("utc").ValueAsBool(false);

               var fmt = macroParams.Navigate("$fmt|$format").ValueAsString();

               var valueAttr = macroParams.AttrByName("value");


               DateTime now;

               if (utc)
                    now = App.TimeSource.UTCNow;
               else
               {
                    ILocalizedTimeProvider timeProvider = App.Instance;

                    if (context is ILocalizedTimeProvider)
                        timeProvider = (ILocalizedTimeProvider) context;

                    now = timeProvider.LocalizedTime;
               }

               // We inspect the "value" param that may be provided for testing purposes
               if (valueAttr.Exists)
                   now = valueAttr.Value.AsDateTimeFormat(now, fmt,
                            utc ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal);

               return fmt == null ? now.ToString() : now.ToString(fmt);
            }
            else if (string.Equals(macroName, "ctx-name", StringComparison.InvariantCultureIgnoreCase))
            {
               if (context is INamed)
                return ((INamed)context).Name;
            }


            return inputValue;
        }
    }


}
