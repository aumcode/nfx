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


/* NFX by ITAdapter
 * Author: Andrey Kolbasov <andrey@kolbasov.com>
 */
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

using NFX.Serialization.JSON;
using NFX.IO.FileSystem.GoogleDrive.V2;

namespace NFX.IO.FileSystem.GoogleDrive
{
  static class Extensions
  {
    public static Uri FormatUri(this string template, params object[] args)
    {
      return new Uri(string.Format(CultureInfo.InvariantCulture, template, args), UriKind.RelativeOrAbsolute);
    }

    public static string GetString(this HttpWebResponse res)
    {
      using (var stream = res.GetResponseStream())
      {
        using (var reader = new StreamReader(stream))
        {
          return reader.ReadToEnd();
        }
      }
    }

    public static dynamic GetJsonAsDynamic(this HttpWebResponse res)
    {
      var str = res.GetString();
      return str.IsNotNullOrEmpty() ? str.JSONToDynamic() : null;
    }

    public static string ToFormEncoded(this GoogleDriveRequestBody data)
    {
      var parameters = data.Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value.ToString()));
      return string.Join("&", parameters);
    }

    public static Stream ToStream(this string value)
    {
      return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }
  }
}
