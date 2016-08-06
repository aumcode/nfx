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

using System.Collections.Generic;
using System.Text;
using System;

using NFX.CodeAnalysis.Source;

namespace NFX.CodeAnalysis
{
      /// <summary>
      /// Provides language-agnostic token classification
      /// </summary>
      public enum TokenKind
      {
         Other = 0,

         BOF,
         EOF,
         Directive,
         Comment,
         Literal,
         Identifier,
         Keyword,
         Operator,
         Symbol
      }



      /// <summary>
      /// Represents a lexical token of the language. This is an abstract class that particular language implementations must extend
      ///  to define language-specific token types
      /// </summary>
      public abstract class Token
      {
        public readonly ILexer Lexer;

        public readonly SourcePosition StartPosition;
        public readonly SourcePosition EndPosition;

        public readonly string Text;
        public readonly object Value;


        private Token()
        {
        }

        public Token(ILexer lexer, SourcePosition startPos, SourcePosition endPos, string text, object value)
        {
          Lexer = lexer;
          StartPosition = startPos;
          EndPosition = endPos;
          Text = text;
          Value = value;
        }

        public Token(ILexer lexer, SourcePosition startPos, SourcePosition endPos, string text) :
          this(lexer, startPos, endPos, text, null)
        {

        }

        #region Properties
          /// <summary>
          /// Returns language that this token is a part of
          /// </summary>
          public abstract Language Language { get; }

          /// <summary>
          /// Provides language-agnostic classification for token type
          /// </summary>
          public abstract TokenKind Kind { get; }


          /// <summary>
          /// Returns true for tokens that are primary part of the language, not control, metadata, directive, comment etc...
          /// For example BOF,EOF markers are not primary part of the language, compiler directives, comments are not either.
          /// This property is useful for pattern searches, when comments and other non-primary tokens need to be quickly skipped
          /// </summary>
          public abstract bool IsPrimary { get; }


          /// <summary>
          /// This property is needed due to the fact that language lexers may be used to analyze special supersets of regular
          ///  language grammars i.e. for pattern matches, template parser etc. Code compilers may elect to throw errors when this property is true.
          /// Returns true for tokens that are not part of the valid language grammar, however they exist for other reasons,
          /// for example - for pattern capture match analysis, or for template processing
          /// </summary>
          public abstract bool IsNonLanguage { get; }

          /// <summary>
          /// Returns token type as a grammar-agnostic ordinal
          /// </summary>
          public abstract int OrdinalType { get; }



          /// <summary>
          /// Indicates whether this token indicates an BEGINNING-OF-FILE condition
          /// </summary>
          public bool IsBOF { get { return Kind==TokenKind.BOF;  } }

          /// <summary>
          /// Indicates whether this token indicates an END-OF-FILE condition
          /// </summary>
          public bool IsEOF { get { return Kind==TokenKind.EOF;  } }

          /// <summary>
          /// Indicates whether this token represents a directive
          /// </summary>
          public bool IsDirective { get { return Kind==TokenKind.Directive;  } }

          /// <summary>
          /// Indicates whether this token represents a comment
          /// </summary>
          public bool IsComment { get { return Kind==TokenKind.Comment;  } }

          /// <summary>
          /// Indicates whether this token represents a literal
          /// </summary>
          public bool IsLiteral { get { return Kind==TokenKind.Literal;  } }

          /// <summary>
          /// Indicates whether this token represents a literal, which is a string or a character sequence.
          /// This flag is useful for pattern searches that examine comments and strings for sub-patterns
          /// </summary>
          public abstract bool IsTextualLiteral { get; }

          /// <summary>
          /// Indicates whether this token represents a literal, which is a numeric literal (i.e. int, double)
          /// This flag is useful for pattern searches that look for particular constant values in numbers (i.e. year = 2000)
          /// </summary>
          public abstract bool IsNumericLiteral { get; }


          /// <summary>
          /// Indicates whether this token represents an identifier
          /// </summary>
          public bool IsIdentifier { get { return Kind==TokenKind.Identifier;  } }

          /// <summary>
          /// Indicates whether this token represents a language keyword
          /// </summary>
          public bool IsKeyword { get { return Kind==TokenKind.Keyword;  } }


          /// <summary>
          /// Indicates whether this token represents an operator
          /// </summary>
          public bool IsOperator { get { return Kind==TokenKind.Operator;  } }

          /// <summary>
          /// Indicates whether this token represents a symbol
          /// </summary>
          public bool IsSymbol { get { return Kind==TokenKind.Symbol;  } }

        #endregion


        public override string ToString()
        {
          return "[{0} :: {1}]  '{2}' ({3})".Args(StartPosition, EndPosition, Text, Value);
        }

      }//Token



      /// <summary>
      /// A list of tokens
      /// </summary>
      public class TokenList<TToken> : List<TToken> where TToken : Token
      {

        public TokenList()
        {

        }

        public TokenList(IEnumerable<TToken> other) : base(other)
        {

        }

        public new TToken this[int idx]
        {
          get
          {
            if (idx>=base.Count) idx=base.Count-1;
            return base[idx];
          }
        }


        public override string ToString()
        {
          StringBuilder sb = new StringBuilder();
          foreach (Token tok in this)
            sb.AppendLine(tok.ToString());

          return sb.ToString();
        }
      }

      /// <summary>
      /// Provides read-only view over TokenList
      /// </summary>
      public class Tokenized<TToken> : IEnumerable<TToken> where TToken : Token
      {
            public Tokenized(TokenList<TToken> list)
            {
              m_List = list;
            }

            private TokenList<TToken> m_List;


            public TToken this[int idx]{ get { return m_List[idx]; } }

            public int Count
            {
              get { return m_List.Count; }
            }

            public IEnumerator<TToken> GetEnumerator()
            {
                return m_List.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return m_List.GetEnumerator();
            }

            public override string ToString()
            {
              StringBuilder sb = new StringBuilder();
              foreach (Token tok in this)
                sb.AppendLine(tok.ToString());

              return sb.ToString();
            }
      }


}//namespace