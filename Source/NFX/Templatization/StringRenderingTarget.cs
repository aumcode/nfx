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

namespace NFX.Templatization
{
    /// <summary>
    /// Renders templates into string
    /// </summary>
    public class StringRenderingTarget : IRenderingTarget
    {
        private StringBuilder m_Buffer = new StringBuilder();

        public StringRenderingTarget()
        {

        }

        public StringRenderingTarget(bool encodeHtml)
        {
          EncodeHtml = encodeHtml;
        }

        public readonly bool EncodeHtml;


        /// <summary>
        /// Returns what has been written
        /// </summary>
        public override string ToString()
        {
          return Value;
        }

        /// <summary>
        /// Returns what has been written
        /// </summary>
        public string Value
        {
          get { return m_Buffer.ToString();}
        }

        public void Write(object value)
        {
            if (value!=null)
             m_Buffer.Append(value.ToString());
        }

        public void Write(string value)
        {
          if (value!=null)
             m_Buffer.Append(value);
        }

        public void WriteLine(object value)
        {
            if (value!=null)
             m_Buffer.AppendLine(value.ToString());
        }

        public void WriteLine(string value)
        {
          if (value!=null)
             m_Buffer.AppendLine(value);
        }



        public void Flush()
        {
        }

        public object Encode(object value)
        {
            if (!EncodeHtml) return value;

            if (value==null) return null;

            return System.Net.WebUtility.HtmlEncode(value.ToString());
        }
    }
}
