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
using System.Web;
using System.Globalization;

using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Web
{
  /// <summary>
  /// Provides various utilities for web technologies
  /// </summary>
  public static class Utils
  {

    /// <summary>
    /// Converts UTC date timne string suitable for use as Cookie expiration filed
    /// </summary>
    public static string DateTimeToHTTPCookieDateTime(this DateTime utcDateTime)
    {
	    return utcDateTime.ToString("ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'", DateTimeFormatInfo.InvariantInfo);
    }


    /// <summary>
    /// Escapes JS literal, replacing / \ \r \n " ' &lt; &gt; &amp; chars with their hex codes
    /// </summary>
    public static string EscapeJSLiteral(this string value)
    {
      if (value==null) return null;
      if (value.Length==0) return string.Empty;

      var sb = new StringBuilder();
      for(var i=0; i<value.Length; i++)
      {
        var c = value[i];
        if (c < 0x20 || //space
            c=='\'' || c=='"' ||
            c=='/' || c=='\\' ||
            c=='&' || c=='<'  || c=='>')
        {
          sb.Append(@"\x");
          var nibble = (c >> 4) & 0x0f; //this works faster than int to string hex
          sb.Append((char)(nibble<=9 ? '0'+nibble : 'A'+(nibble-10)));
          nibble =  c & 0x0f;
          sb.Append((char)(nibble<=9 ? '0'+nibble : 'A'+(nibble-10)));
          continue;
        }

        sb.Append(c);
      }
      return sb.ToString();
    }

  }
}
