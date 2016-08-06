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
using System.Collections;
using System.Text;

namespace NFX.CodeAnalysis.JSON
{
  /// <summary>
  /// Provides JSON keyword resolution services, this class is thread safe
  /// </summary>
  public static class JSONKeywords
  {
    private static Dictionary<string, JSONTokenType> s_KeywordList = new Dictionary<string, JSONTokenType>();

    static JSONKeywords()
    {
       s_KeywordList["true"]  = JSONTokenType.tTrue;
       s_KeywordList["false"] = JSONTokenType.tFalse;
       s_KeywordList["null"]  = JSONTokenType.tNull;

       s_KeywordList["+"]  = JSONTokenType.tPlus;
       s_KeywordList["-"]  = JSONTokenType.tMinus;

       s_KeywordList["{"] = JSONTokenType.tBraceOpen;
       s_KeywordList["}"] = JSONTokenType.tBraceClose;
       s_KeywordList["["] = JSONTokenType.tSqBracketOpen;
       s_KeywordList["]"] = JSONTokenType.tSqBracketClose;
       s_KeywordList[","] = JSONTokenType.tComma;
       s_KeywordList[":"] = JSONTokenType.tColon;
    }

    /// <summary>
    /// Resolves a JSON keyword - this method IS thread safe
    /// </summary>
    public static JSONTokenType Resolve(string str)
    {
      JSONTokenType tt;

      s_KeywordList.TryGetValue(str, out tt);

      return (tt!=JSONTokenType.tUnknown) ? tt : JSONTokenType.tIdentifier;
    }

  }



}

