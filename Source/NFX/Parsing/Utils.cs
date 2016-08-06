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
 * Revision: NFX 1.0  2011.02.03
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NFX.Parsing
{
  public static class Utils
  {
    private const char SPACE = ' ';
    private static readonly char[] FIELD_NAME_DELIMETERS = new char[] {' ', '-', '_'};

    /// <summary>
    /// Parses database field names (column names) and converts parts to human-readable description
    ///  like:
    ///  "FIRST_NAME" -> "First Name",
    ///  "FirstName" -> "First Name",
    ///  "CHART_OF_ACCOUNTS" -> "Chart of Accounts"
    /// </summary>
    public static string ParseFieldNameToDescription(this string fieldName, bool capitalize)
    {
      if (fieldName.IsNullOrWhiteSpace()) return string.Empty;

      var builder = new StringBuilder();
      char prev = fieldName[0];
      builder.Append(prev);

      var length = fieldName.Length;
      for (int i = 1; i < length; i++)
      {
        var curr = fieldName[i];
        if (
            !FIELD_NAME_DELIMETERS.Contains(prev) &&
            !FIELD_NAME_DELIMETERS.Contains(curr) &&
            (charCaseTransition(prev, curr) || charDigitTransition(prev, curr))
           )
        {
            builder.Append(SPACE);
        }

        builder.Append(curr);
        prev = curr;
      }

      var name = builder.ToString();
      var segs = name.Split(FIELD_NAME_DELIMETERS, StringSplitOptions.RemoveEmptyEntries);
      var result = capitalize ?
                      segs.Select(s => s.Trim().ToLowerInvariant().CapitalizeFirstChar()).Aggregate((s1,s2) => s1+SPACE+s2) :
                      segs.Select(s => s.Trim().ToLowerInvariant()).Aggregate((s1,s2) => s1+SPACE+s2);

      return result;
    }


    private static Regex urlCheckRegex;

    /// <summary>
    /// Checks URL string for validity
    /// </summary>
    public static bool IsURLValid(this string url)
    {
      if (string.IsNullOrEmpty(url)) return false;


      lock (typeof(Utils))
        if (urlCheckRegex == null)
        {
          string pattern = @"^(http|https)\://[a-zA-Z0-9\-\.]+(:[a-zA-Z0-9]*)?(/[a-zA-Z0-9\-\._]*)*$";
          urlCheckRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

      Match m = urlCheckRegex.Match(url);
      return m.Success;
    }

    /// <summary>
    /// Puts every sentence on a separate line
    /// </summary>
    public static string MakeSentenceLines(this string text)
    {
      StringBuilder result = new StringBuilder();

      for (int i = 0; i < text.Length; i++)
      {
        Char c = text[i];
        result.Append(c);
        if ((c == '.') ||
            (c == ';') ||
            (c == '?') ||
            (c == '!'))
          result.Append("\n");
      }

      return result.ToString();
    }



    /// <summary>
    /// Returns a captured wildcard segment from string. Pattern uses '*' for match capture by default and may contain a single capture
    /// </summary>
    public static string CapturePatternMatch(this string str,     //     Pages/Dima/Welcome
                                                  string pattern,
                                                  char wc ='*',
                                                  StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)

    {
       var i = pattern.IndexOf(wc);
       if (i<0) return string.Empty;

       var pleft = pattern.Substring(0, i);
       var pright = (i+1<pattern.Length)? pattern.Substring(i+1) : string.Empty;

       if (pleft.Length>0)
       {
         if (!str.StartsWith(pleft, comparisonType)) return string.Empty;
         str = str.Substring(pleft.Length);
       }

       if (pright.Length>0)
       {
         if (!str.EndsWith(pright, comparisonType)) return string.Empty;
         str = str.Substring(0, str.Length - pright.Length);
       }

       return str;
    }


    /// <summary>
    /// Returns true if supplied string matches pattern that can contain up to one * wildcard and multiple ? wildcards
    /// </summary>
    public static bool MatchPattern(this string str,     //     some address
                                         string pattern, //     some*e?s
                                         char wc ='*',
                                         char wsc ='?',
                                         bool senseCase = false)
    {
       if (str.IsNullOrWhiteSpace() && pattern.IsNullOrWhiteSpace()) return true;

       if (pattern.IsNullOrWhiteSpace()) return false;

       var iwc = pattern.IndexOf(wc);
       if (iwc>=0)
       {
          var iwc2 = pattern.LastIndexOf(wc);
          if (iwc2>iwc)
            throw new NFXException(StringConsts.TEXT_PATTERN_MULTI_WC_ERROR.Args(pattern, wc));
       }

       int istr=0, ipat=0;
       for(; istr<str.Length && ipat<pattern.Length; istr++,ipat++)
       {
          var pc = pattern[ipat];
          if (pc==wsc)//?
            continue;

          if (pc==wc)//*
          {
            var leftcnt = pattern.Length-1-ipat;
            if (leftcnt<=0) return true;//match whatever left * at the end
            if (istr>=str.Length-1) return false;//nothing left in string but something left in pattern
            pattern = pattern.Substring(ipat+1);
            return MatchPattern(str.Substring(str.Length-pattern.Length), pattern, wc, wsc, senseCase);
          }

          var sc = str[istr];
          if (!charEqual(sc, pc, senseCase)) return false;
       }

       return str.Length==pattern.Length;
    }

    private static bool charEqual(char a, char b, bool senseCase)
    {
      return senseCase ? a==b : Char.ToUpperInvariant(a)==Char.ToUpperInvariant(b);
    }

    private static bool charCaseTransition(char prev, char curr)
    {
      return  char.IsLower(prev) & char.IsUpper(curr);
    }

    private static bool charDigitTransition(char prev, char curr)
    {
      return char.IsDigit(prev) ^ char.IsDigit(curr);
    }
  }
}
