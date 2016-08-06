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
using System.Text;

using NFX.Parsing;

namespace NFX.CodeAnalysis.JSON
{

  /// <summary>
  /// Provides JSON string escape parsing
  /// </summary>
  public static class JSONStrings
  {

    public static string UnescapeString(string str)
    {
      if (str.IndexOf('\\')==-1) return str;//20131215 DKh 6.3% speed improvement in Integration test

      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < str.Length; i++)
      {
        char c = str[i];
        if ((i < str.Length - 1) && (c == '\\'))
        {
          i++;
          char n = str[i];

          switch (n)
          {
            case '\\': sb.Append('\\'); break;
            case '/': sb.Append('/'); break;
            case '0': sb.Append((char)CharCodes.Char0); break;
            case 'a': sb.Append((char)CharCodes.AlertBell); break;
            case 'b': sb.Append((char)CharCodes.Backspace); break;
            case 'f': sb.Append((char)CharCodes.Formfeed); break;
            case 'n': sb.Append((char)CharCodes.LF); break;
            case 'r': sb.Append((char)CharCodes.CR); break;
            case 't': sb.Append((char)CharCodes.Tab); break;
            case 'v': sb.Append((char)CharCodes.VerticalQuote); break;
            case 'u': //  \uFFFF
              string hex = string.Empty;
              int cnt = 0;
              //loop through UNICODE hex number chars
              while ((i < str.Length - 1) && (cnt < 4))
              {
                i++;
                hex += str[i];
                cnt++;
              }

              try
              {
                sb.Append(Char.ConvertFromUtf32(Convert.ToInt32(hex, 16)));
              }
              catch
              {
                throw new StringEscapeErrorException(hex);
              }

              break;

            default:
              throw new StringEscapeErrorException(String.Format("{0}{1}", c, n));
          }

        }
        else
          sb.Append(c);

      }//for

      return sb.ToString();
    }

  }



}