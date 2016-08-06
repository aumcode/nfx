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

using NFX.CodeAnalysis.Source;

namespace NFX.CodeAnalysis.Laconfig
{
      /// <summary>
      /// Represents a token of Laconfig language
      /// </summary>
      public class LaconfigToken : Token
      {
        public readonly LaconfigTokenType Type;


        public LaconfigToken(LaconfigLexer lexer, LaconfigTokenType type, SourcePosition startPos, SourcePosition endPos, string text, object value = null) :
                           base(lexer, startPos, endPos, text, value)
        {
          Type = type;
        }

        public override Language Language
        {
            get { return LaconfigLanguage.Instance;}
        }

        public override string ToString()
        {
          return "{0} {1}".Args(Type, base.ToString());
        }

        public override TokenKind Kind
        {
            get
            {
                if (Type > LaconfigTokenType.LITERALS_START && Type < LaconfigTokenType.LITERALS_END)
                     return TokenKind.Literal;

                if (Type > LaconfigTokenType.SYMBOLS_START && Type < LaconfigTokenType.SYMBOLS_END)
                     return TokenKind.Symbol;

                switch(Type)
                {

                    case LaconfigTokenType.tBOF:          return TokenKind.BOF;
                    case LaconfigTokenType.tEOF:          return TokenKind.EOF;
                    case LaconfigTokenType.tDirective:    return TokenKind.Directive;
                    case LaconfigTokenType.tComment:      return TokenKind.Comment;
                    case LaconfigTokenType.tIdentifier:   return TokenKind.Identifier;
                    case LaconfigTokenType.tEQ:           return TokenKind.Operator;

                    default: return TokenKind.Other;
                }
            }
        }

        public override bool IsPrimary
        {
            get { return !IsNonLanguage && Type!=LaconfigTokenType.tComment; }
        }

        public override bool IsNonLanguage
        {
            get { return Type > LaconfigTokenType.NONLANG_START && Type < LaconfigTokenType.NONLANG_END; }
        }

        public override int OrdinalType
        {
            get { return (int)Type; }
        }

        public override bool IsTextualLiteral
        {
            get { return Type == LaconfigTokenType.tStringLiteral; }
        }

        public override bool IsNumericLiteral
        {
            get { return false; } //not supported
        }
      }




}
