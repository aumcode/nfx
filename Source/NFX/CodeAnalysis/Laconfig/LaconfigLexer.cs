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
    /// Performs lexical analysis on source supplied in Laconfig syntax.
    /// This class supports lazy analysis that happens gradually as result tokens are consumed through IEnumerable interface.
    /// </summary>
    public sealed class LaconfigLexer : Lexer<LaconfigToken>
    {
        public LaconfigLexer(ISourceText source, MessageList messages = null, bool throwErrors = false) :
            base( source, messages, throwErrors)
        {
             m_FSM = new FSM(this);
        }

        public LaconfigLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false) :
            base(context, srcRef, source, messages, throwErrors)
        {
             m_FSM = new FSM(this);
        }

        public override Language Language
        {
            get { return LaconfigLanguage.Instance;}
        }

        public override string MessageCodeToString(int code)
        {
            return ((LaconfigMsgCode)code).ToString();
        }



        private FSM m_FSM;
        private IEnumerator<bool> m_Work;

        protected override bool DoLexingChunk()
        {
            if (m_AllAnalyzed) return true;

            if (m_Work==null)
            {
              m_Work = m_FSM.Run().GetEnumerator();
            }

            return !m_Work.MoveNext();
        }






    private class FSM
    {
      public FSM(LaconfigLexer lex)
      {
        lexer = lex;
        source = lexer.Source;
        tokens = lexer.m_Tokens;
        srcRef = lexer.SourceCodeReference;
      }

      private readonly LaconfigLexer lexer;
      private readonly ISourceText source;
      private readonly TokenList<LaconfigToken> tokens;
      private readonly SourceCodeRef srcRef;

      char chr, nchr;
      bool isCommentBlock;
      char commentBlockEnding;
      bool isCommentLine;
      bool isDirective;
      bool freshLine = true;

      bool isString;
      char stringEnding;
      bool isVerbatim;

      int posLine = 1;
      int posCol = 0;
      int posChar = 0;

      SourcePosition tagStartPos, tagEndPos;

      StringBuilder buffer = new StringBuilder();



      private void moveNext()
      {
        posChar++;
        posCol++;
        chr = source.ReadChar();
        nchr = source.PeekChar();
      }

      private SourcePosition srcPos()
      {
        return new SourcePosition(posLine, posCol, posChar);
      }


      private void bufferAdd(char c)
      {
        if (buffer.Length == 0) tagStartPos = srcPos();
        buffer.Append(c);
        tagEndPos = srcPos();
      }



      public IEnumerable<bool> Run()
      {
        const int YIELD_BATCH = 5;
        var prevTokenCount = 0;


        tokens.Add(new LaconfigToken(
                                lexer,
                                LaconfigTokenType.tBOF,
                                srcPos(),
                                srcPos(),
                                String.Empty));

        #region Main walk
        //=======================================================================================================================
        while (!source.EOF)
        {
          moveNext();

          #region CRLF
          if ((chr == '\n') || (chr == '\r'))
          {
            if ((isString) && (!isVerbatim))
            {
              lexer.EmitMessage(MessageType.Error, (int)LaconfigMsgCode.eUnterminatedString, srcPos());
              yield break;//no further parsing
            }


            if ((isString && isVerbatim) || (isCommentBlock))
              bufferAdd(chr);


            if (chr == '\n')
            {
              if ((!isString) && (!isCommentBlock))
              {
                flush();
                if (isCommentLine)
                {
                  isCommentLine = false;
                  isDirective = false;
                }
                freshLine = true;
              }
              posLine++;
            }
            posCol = 0;


            continue;
          }

          #endregion


          if (isString)
          {
            #region Inside String

            if (isVerbatim || (chr != '\\') || (nchr != '\\'))//take care of 'c:\\dir\\';
            {
                  //turn off strings
                  if (
                       ((isVerbatim) && (chr == stringEnding) && (nchr == stringEnding)) ||
                       ((!isVerbatim) && (chr == '\\') && (nchr == stringEnding))
                      )
                  {
                    //Verbatim: eat one extra:   $"string ""test"" syntax" == string "test" syntax
                    //Regular: eat "\" escape:    "string \"test\" syntax" == string "test" syntax
                    moveNext();

                    if (source.EOF)
                    {
                      lexer.EmitMessage(MessageType.Error, (int)LaconfigMsgCode.eUnterminatedString, srcPos());
                      yield break;//stop further processing, as string did not terminate but EOF reached
                    }
                  }
                  else if (chr == stringEnding)
                  {
                    flush();
                    isString = false;
                    continue; // eat terminating string char
                  }

              }
              else//take care of 'c:\\dir\\'
              {
                bufferAdd(chr); //preserve  \
                moveNext();
              }
            #endregion
          }//in string
          else
          {
            #region Not Inside String

            if (!isCommentLine)
            {
              if (!isCommentBlock)
              {
                #region Not inside comments

                #region Turn On Comments
                //turn on comment block
                if (((chr == '/') || (chr == '|')) && (nchr == '*'))
                {
                  flush();
                  isCommentBlock = true;
                  commentBlockEnding = chr;
                  moveNext();
                  continue;
                }

                //turn on comment line
                if ((chr == '/') && (nchr == '/'))
                {
                  flush();
                  isCommentLine = true;
                  moveNext();
                  continue;
                }

                //turn on comment line mode for directive
                //directives MUST be the first non-white char on the line
                if (freshLine && chr == '#')
                {
                  flush();
                  isCommentLine = true;
                  isDirective = true;
                  continue;
                }


                #endregion

                #region Turn On Strings
                if ((chr == '$') && ((nchr == '"') || (nchr == '\'')))
                {
                  flush();
                  isString = true;
                  isVerbatim = true;
                  stringEnding = nchr;
                  moveNext();
                  continue;
                }
                if ((chr == '"') || (chr == '\''))
                {
                  flush();
                  isString = true;
                  isVerbatim = false;
                  stringEnding = chr;
                  continue;
                }

                #endregion

                #region Syntactic Separators - Space, colons and Symbols

                if ((chr == ' ') || (chr == '\t')) //space or TAB
                {
                  flush();
                  continue; //eat it
                }

                if (
                    (chr == '{') ||
                    (chr == '}') ||
                    (chr == '=')
                   )
                 {
                   flush();
                   bufferAdd(chr);
                   flush();
                   continue;
                 }

                #endregion

                #endregion
              }
              else
              {
                #region Turn Off Comment Block
                if ((chr == '*') && (nchr == commentBlockEnding))
                {
                  flush();
                  isCommentBlock = false;
                  moveNext();
                  continue;
                }
                #endregion
              }//block comments off

            }//NOT CommentLine

            #endregion
          }//not in string

          bufferAdd(chr);
          freshLine = false;

          //yield the batch of new tokens
          if (tokens.Count>prevTokenCount+YIELD_BATCH)
          {
                prevTokenCount = tokens.Count;
                yield return true;
          }

        }//while
        //=======================================================================================================================
        #endregion


        flush(); //flush any remains

        #region Post-walk check
            if (tokens.Count < 2)
               lexer.EmitMessage(MessageType.Error, (int)LaconfigMsgCode.ePrematureEOF, srcPos());


            if (isCommentBlock)
               lexer.EmitMessage(MessageType.Error, (int)LaconfigMsgCode.eUnterminatedComment, srcPos());


            if (isString)
               lexer.EmitMessage(MessageType.Error, (int)LaconfigMsgCode.eUnterminatedString, srcPos());


        #endregion

            tokens.Add(new LaconfigToken(lexer,
                                   LaconfigTokenType.tEOF,
                                   new SourcePosition(posLine, posCol, posChar),
                                   new SourcePosition(posLine, posCol, posChar),
                                   String.Empty));
        yield return true;
        yield break;
      }//Run



      private void flush()
      {
        if (
            (!isString) &&
            (!isCommentBlock) &&
            (!isCommentLine) &&
            (buffer.Length == 0)
           ) return;

        string text = buffer.ToString();

        buffer.Length = 0;

        var type = LaconfigTokenType.tUnknown;

        if (isString)
        {
          type = LaconfigTokenType.tStringLiteral;


          if (!isVerbatim)
          {
            try //expand escapes
            {
              text = CSharp.CSStrings.UnescapeString(text);//laconfig strings are the same as in C#
            }
            catch (StringEscapeErrorException err)
            {
              lexer.EmitMessage(MessageType.Error, (int)LaconfigMsgCode.eInvalidStringEscape, tagStartPos, null, err.ErroredEscape);
              return;
            }
          }
        }
        else if (isCommentLine && isDirective)//directives treated similar to line comments
        {
          type = LaconfigTokenType.tDirective;
        }
        else if (isCommentBlock || isCommentLine)
        {
          type = LaconfigTokenType.tComment;
        }
        else
        {

            type = LaconfigKeywords.Resolve(text);

        }//not comment


        tokens.Add(new LaconfigToken(lexer, type, tagStartPos, tagEndPos, text, text));
      }


    }//FSM










    }//Laconfig Lexer
}

