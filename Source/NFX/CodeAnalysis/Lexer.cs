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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.CodeAnalysis.Source;

namespace NFX.CodeAnalysis
{

    /// <summary>
    /// Performs lexical analysis of the source code in particular language
    /// Allows to enumerate over source as typed Token stream - depending on implementation enumeration may do
    /// lexical analysis token-by-token or in chunks with every enumerator advance
    /// </summary>
    public abstract class Lexer<TToken> : SourceRefCommonCodeProcessor, ILexer, IEnumerable<TToken>  where TToken : Token
    {
        protected Lexer(ISourceText source, MessageList messages = null, bool throwErrors = false) :
          this(null, new SourceCodeRef(source.Name ?? CoreConsts.UNNAMED_MEMORY_BUFFER), source, messages, throwErrors)
        {

        }

        protected Lexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false) :
          base(context, srcRef, messages, throwErrors)

        {
              m_Source = source;
        }

        private ISourceText   m_Source;


        /// <summary>
        /// References source code text that was lexed
        /// </summary>
        public ISourceText Source{ get { return m_Source; } }



        protected TokenList<TToken> m_Tokens = new TokenList<TToken>();


        /// <summary>
        /// Returns true when the whole input source has been analyzed. This property is always true for lexers that do not support lazy analysis
        /// </summary>
        public bool AllAnalyzed { get { return m_AllAnalyzed; } }


        /// <summary>
        /// Returns tokenized source as indexable list.
        /// Accessing this property causes lexical analysis to complete on the whole source if it has not been completed yet
        /// </summary>
        public Tokenized<TToken> Tokens
        {
             get
             {
                analyzeAll();
                return new Tokenized<TToken>(m_Tokens);
             }
        }


        /// <summary>
        /// Enumerates Token stream - depending on implementation enumeration may do
        /// lexical analysis token-by-token or in chunks with every enumerator advance
        /// </summary>
        public IEnumerable<Token> TokenStream
        {
            get
            {
               return this;
            }
        }


        public IEnumerator<TToken> GetEnumerator()
        {
            return new tokenEnumerator(this);
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        /// <summary>
        /// If lexer supports lazy analysis, forces analysis of the whole source
        /// </summary>
        public void AnalyzeAll()
        {
            analyzeAll();
        }

        public override string ToString()
        {
          StringBuilder buff = new StringBuilder();

          if (m_AllAnalyzed)
            buff.AppendFormat("Token count:{0}, finished\n ", m_Tokens.Count);
          else
            buff.AppendFormat("Current token count:{0}, not finished\n ", m_Tokens.Count);

          foreach (Token tok in m_Tokens)
          {
            buff.AppendLine(tok.ToString());
          }

          return buff.ToString();
        }


        /// <summary>
        /// Lexes more source and returns true when EOF has been reached.
        /// Depending on particular implementation, a chunk may contain more than one token.
        /// Tokens are added into m_Tokens
        /// </summary>
        protected abstract bool DoLexingChunk();



                        private void analyzeAll()
                        {
                            if (m_AllAnalyzed) return;
                            var e = this.GetEnumerator();
                            e.Reset();
                            while(e.MoveNext()) ;
                        }

                        protected bool m_AllAnalyzed;



                        private class tokenEnumerator : IEnumerator<TToken>
                        {

                            private int m_Index = -1;

                            public tokenEnumerator(Lexer<TToken> lexer)
                            {
                              m_Lexer = lexer;
                            }

                            private Lexer<TToken> m_Lexer;


                            public TToken Current
                            {
                                get { return m_Lexer.m_Tokens[m_Index]; }
                            }

                            public void Dispose()
                            {

                            }

                            object IEnumerator.Current
                            {
                                get { return this.Current; }
                            }

                            public bool MoveNext()
                            {
                               if (m_Index==m_Lexer.m_Tokens.Count-1)
                               {
                                  if (m_Lexer.m_AllAnalyzed) return false;
                                  m_Lexer.m_AllAnalyzed = m_Lexer.DoLexingChunk();
                                  if (m_Index==m_Lexer.m_Tokens.Count-1) return false;
                               }

                               m_Index++;
                               return true;
                            }

                            public void Reset()
                            {
                                m_Index = -1;
                            }
                        }
    }



}
