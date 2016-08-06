
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
 * Originated: 2006.01
 * Revision: NFX 1.0  2/17/2014 12:32:35 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;

namespace NFX.Web.IO
{
  public static class RFC3986
  {
    #region CONSTS

      private const string     ESCAPE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
      private const string ESCAPE_URL = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~/:";

    #endregion

    #region Static

      /// <summary>
      /// Encodes URL string according to RFC3986
      /// </summary>
      public static string URLEncode(string input)
      {
        return Encode(input, ESCAPE_URL);
      }

      /// <summary>
      /// Encodes string according to RFC3986
      /// </summary>
      public static string Encode(string input)
      {
        return Encode(input, ESCAPE);
      }

      private static string Encode(string input, string escape)
      {
        if (string.IsNullOrEmpty(input))
          return string.Empty;

        StringBuilder b = new StringBuilder(input.Length * 2); // approximate estimation
        byte[] bb = Encoding.UTF8.GetBytes(input);

        foreach(char ch in bb)
        {
          if (escape.IndexOf(ch) != -1)
          {
            b.Append((char)ch);
          }
          else
          {
            b.Append('%');
            b.Append(Convert.ToString(ch, 16).ToUpper());
          }
        }
        return b.ToString();
      }

    #endregion

  } //RFC3986

}
