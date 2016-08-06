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
using System.Linq;
using System.Collections.Generic;

using NFX.CodeAnalysis.Source;

namespace NFX.CodeAnalysis.CSharp
{
      /// <summary>
      /// Represents a C# token
      /// </summary>
      public class CSToken : Token
      {
        public readonly CSTokenType Type;


            public CSToken(CSLexer lexer, CSTokenType type, SourcePosition startPos, SourcePosition endPos, string text, object value = null) :
                               base(lexer, startPos, endPos, text, value)
            {
              Type = type;
            }

            public override Language Language
            {
                get { return CSLanguage.Instance;}
            }

            public override string ToString()
            {
              return "{0} {1}".Args(Type, base.ToString());
            }


            public override TokenKind Kind
            {
                get
                {
                    if (Type > CSTokenType.LITERALS_START && Type < CSTokenType.LITERALS_END)
                         return TokenKind.Literal;

                    if (Type > CSTokenType.SYMBOLS_START && Type < CSTokenType.SYMBOLS_END)
                         return TokenKind.Symbol;

                    if (Type > CSTokenType.OPERATORS_START && Type < CSTokenType.OPERATORS_END)
                         return TokenKind.Operator;

                    if (Type > CSTokenType.KEYWORDS_START && Type < CSTokenType.KEYWORDS_END)
                         return TokenKind.Keyword;

                    switch(Type)
                    {

                        case CSTokenType.tBOF:          return TokenKind.BOF;
                        case CSTokenType.tEOF:          return TokenKind.EOF;
                        case CSTokenType.tDirective:    return TokenKind.Directive;
                        case CSTokenType.tComment:      return TokenKind.Comment;
                        case CSTokenType.tIdentifier:   return TokenKind.Identifier;


                        default: return TokenKind.Other;
                    }
                }
            }

            public override bool IsPrimary
            {
                get { return !IsNonLanguage && Type!=CSTokenType.tComment; }
            }

            public override bool IsNonLanguage
            {
                get { return Type > CSTokenType.NONLANG_START && Type < CSTokenType.NONLANG_END; }
            }

            public override int OrdinalType
            {
                get { return (int)Type; }
            }

            public override bool IsTextualLiteral
            {
                get { return Type == CSTokenType.tStringLiteral; }
            }

            public override bool IsNumericLiteral
            {
                get { return Type > CSTokenType.NUMLITERALS_START && Type < CSTokenType.NUMLITERALS_END; }
            }


      }



}
