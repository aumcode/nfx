
/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
 * Revision: NFX 1.0  1/14/2014 2:55:08 PM
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
using NFX.Web.IO;

namespace NFX.Web.Social
{
  public partial class Twitter : SocialNetwork
  {
    #region CONSTS

    #endregion

    #region Inner Types

    #endregion

    #region Static

      public static string GetRawOAuthHeaderStr(Dictionary<string, string> dictionary)
      {
        StringBuilder b = new StringBuilder();
        bool firstValue = true;

        foreach (var item in GetEncodedPairs(dictionary))
        {
          if (firstValue)
            firstValue = false;
          else
            b.Append('&');

          b.AppendFormat("{0}={1}", item.Item1, item.Item2);
        }

        return b.ToString();
      }

      public static string GetOAuthHeaderString(Dictionary<string, string> dictionary)
      {
        StringBuilder b = new StringBuilder();
        bool firstValue = true;

        foreach (var item in GetEncodedPairs(dictionary))
        {
          if (firstValue)
            firstValue = false;
          else
            b.Append(", ");

          b.AppendFormat("{0}=\"{1}\"", item.Item1, item.Item2);
        }

        return b.ToString();
      }

      private static IEnumerable<Tuple<string, string>> GetEncodedPairs(Dictionary<string, string> dictionary)
      {
        foreach (var kvp in dictionary.OrderBy(kvp => kvp.Key))
        {
          string encodedKey = RFC3986.Encode(kvp.Key);
          string encodedValue = RFC3986.Encode(kvp.Value);

          yield return new Tuple<string, string>(encodedKey, encodedValue);
        }
      }

      public static string AddMethodAndBaseURL(string headerStr, HTTPRequestMethod method, string baseURL)
      {
        StringBuilder b = new StringBuilder();

        b.Append(method.ToString()); b.Append('&');
        b.Append(RFC3986.Encode(baseURL)); b.Append('&');
        b.Append(RFC3986.Encode(headerStr));

        return b.ToString();
      }

      public static string CalculateSignature(string src, string secretKey, string tokenSecret = null)
      {
        TwitterCryptor cryptor = new TwitterCryptor( secretKey, tokenSecret);
        string signature = cryptor.GetHashString(src);
        return signature;
      }

    #endregion

  } //TwitterRequestExtensions

}
