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

#pragma warning disable 1570
    /// <summary>
    /// Parses query string (e.g. "id=457&name=zelemhan") into dictionary (e.g. {{"id", "457"}, {"name", "zelemhan"}})
    /// </summary>
#pragma warning restore 1570
    public static JSONDataMap ParseQueryString(string query)
    {
      if (query.IsNullOrWhiteSpace()) return new JSONDataMap();

      var dict = new JSONDataMap();
      int queryLen = query.Length;

      int startIdx = 0;
      while (startIdx < queryLen)
      {
        int ampIdx = query.IndexOf('&', startIdx);
        int kvLen = (ampIdx != -1) ? ampIdx - startIdx : queryLen - startIdx;

        if (kvLen < 1)
        {
          startIdx = ampIdx + 1;
          continue;
        }

        int eqIdx = query.IndexOf('=', startIdx, kvLen);
        if (eqIdx == -1)
        {
          var key = Uri.UnescapeDataString(query.Substring(startIdx, kvLen));
          dict.Add(key, null);
        }
        else
        {
          int keyLen = eqIdx - startIdx;
          if (keyLen > 0)
          {
            string key = Uri.UnescapeDataString(query.Substring(startIdx, keyLen));
            string val = null;
            int valLen = kvLen - (eqIdx - startIdx) - 1;
            if (valLen > 0)
              val = Uri.UnescapeDataString(query.Substring(eqIdx + 1, kvLen - (eqIdx - startIdx) - 1));
            dict.Add(key, val);
          }
        }

        startIdx += kvLen + 1;
      }

      return dict;
    }

  }
}
