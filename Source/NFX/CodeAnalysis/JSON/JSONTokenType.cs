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

namespace NFX.CodeAnalysis.JSON
{
  /// <summary>
  /// Denotes JSON token types.
  /// NOTE: Although called JSON, this is really a JSON superset implementation that includes extra features:
  ///  comments, directives, verbatim strings(start with $), ' or " string escapes, unquoted object key names
  /// </summary>
  public enum JSONTokenType
  {
    tUnknown = 0,
        NONLANG_START,
            tBOF,
            tEOF,
            tDirective,
        NONLANG_END,

            tComment,

        SYMBOLS_START,
            tComma,

            tBraceOpen,
            tBraceClose,

            tSqBracketOpen,
            tSqBracketClose,
            tDot,
        SYMBOLS_END,

            tIdentifier,

            tPlus,
            tMinus,
            tColon,


        LITERALS_START,
            NUMLITERALS_START,
                tIntLiteral,
                tLongIntLiteral,
                tDoubleLiteral,
            NUMLITERALS_END,

                tStringLiteral,

                tTrue,
                tFalse,

                tNull,
        LITERALS_END
  }
}
