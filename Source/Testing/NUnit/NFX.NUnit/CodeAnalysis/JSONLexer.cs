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


using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using NFX.CodeAnalysis;
using NFX.CodeAnalysis.Source;
using NFX.CodeAnalysis.JSON;
using JL=NFX.CodeAnalysis.JSON.JSONLexer;
using System.Reflection;

namespace NFX.NUnit.CodeAnalysis
{
    [TestFixture]   
    public class JSONLexer
    {

        [TestCase]
        public void BOF_EOF()
        {
          var src = @"a";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]{JSONTokenType.tBOF, JSONTokenType.tIdentifier, JSONTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="ePrematureEOF", MatchType=MessageMatch.Contains)]
        public void ePrematureEOF_Thrown()
        {
          var src = @"";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        public void ePrematureEOF_Logged()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new JL(new StringSource(src), msgs);
          lxr.AnalyzeAll();

          Assert.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)JSONMsgCode.ePrematureEOF));
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="ePrematureEOF", MatchType=MessageMatch.Contains)]
        public void ePrematureEOF_CouldLogButThrown()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new JL(new StringSource(src), msgs, throwErrors: true);
          lxr.AnalyzeAll();

          Assert.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)JSONMsgCode.ePrematureEOF));
        }


        [TestCase]
        public void TokenClassifications()
        {
          var src = @"a 'string' : 12 //comment";

          var tokens = new JL(new StringSource(src)).Tokens;

          Assert.IsTrue( tokens[0].IsBOF);
          Assert.IsTrue( tokens[0].IsNonLanguage);
          Assert.IsFalse( tokens[0].IsPrimary);
          Assert.AreEqual(TokenKind.BOF, tokens[0].Kind);

          Assert.AreEqual( JSONTokenType.tIdentifier, tokens[1].Type);
          Assert.IsFalse( tokens[1].IsNonLanguage);
          Assert.IsTrue( tokens[1].IsPrimary);
          Assert.AreEqual(TokenKind.Identifier, tokens[1].Kind);

          Assert.AreEqual( JSONTokenType.tStringLiteral, tokens[2].Type);
          Assert.IsFalse( tokens[2].IsNonLanguage);
          Assert.IsTrue( tokens[2].IsPrimary);
          Assert.IsTrue( tokens[2].IsTextualLiteral);
          Assert.AreEqual(TokenKind.Literal, tokens[2].Kind);

          Assert.AreEqual( JSONTokenType.tColon, tokens[3].Type);
          Assert.IsFalse( tokens[3].IsNonLanguage);
          Assert.IsTrue( tokens[3].IsPrimary);
          Assert.IsTrue( tokens[3].IsOperator);
          Assert.AreEqual(TokenKind.Operator, tokens[3].Kind);


          Assert.AreEqual( JSONTokenType.tIntLiteral, tokens[4].Type);
          Assert.IsFalse( tokens[4].IsNonLanguage);
          Assert.IsTrue( tokens[4].IsPrimary);
          Assert.IsTrue( tokens[4].IsNumericLiteral);
          Assert.AreEqual(TokenKind.Literal, tokens[4].Kind);

          Assert.AreEqual( JSONTokenType.tComment, tokens[5].Type);
          Assert.IsFalse( tokens[5].IsNonLanguage);
          Assert.IsFalse( tokens[5].IsPrimary);
          Assert.IsTrue( tokens[5].IsComment);
          Assert.AreEqual(TokenKind.Comment, tokens[5].Kind);

        }


        [TestCase]
        public void BasicTokensWithIdentifierAndIntLiteral()
        {
          var src = @"{a: 2}";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]
          { 
           JSONTokenType.tBOF, JSONTokenType.tBraceOpen,
           JSONTokenType.tIdentifier, JSONTokenType.tColon, JSONTokenType.tIntLiteral,
           JSONTokenType.tBraceClose, JSONTokenType.tEOF};

          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

         [TestCase]
        public void BasicTokensWithIdentifierAndDoubleLiteral()
        {
          var src = @"{a: 2.2}";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]
          { 
           JSONTokenType.tBOF, JSONTokenType.tBraceOpen,
           JSONTokenType.tIdentifier, JSONTokenType.tColon, JSONTokenType.tDoubleLiteral,
           JSONTokenType.tBraceClose, JSONTokenType.tEOF};

          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [TestCase]
        public void BasicTokens2()
        {
          var src = @"{a: 2, b: true, c: false, d: null, e: ['a','b','c']}";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]
          { 
           JSONTokenType.tBOF, JSONTokenType.tBraceOpen,
           JSONTokenType.tIdentifier, JSONTokenType.tColon, JSONTokenType.tIntLiteral, JSONTokenType.tComma,
           JSONTokenType.tIdentifier, JSONTokenType.tColon, JSONTokenType.tTrue, JSONTokenType.tComma,
           JSONTokenType.tIdentifier, JSONTokenType.tColon, JSONTokenType.tFalse, JSONTokenType.tComma,
           JSONTokenType.tIdentifier, JSONTokenType.tColon, JSONTokenType.tNull, JSONTokenType.tComma,
           JSONTokenType.tIdentifier, JSONTokenType.tColon, JSONTokenType.tSqBracketOpen, JSONTokenType.tStringLiteral, JSONTokenType.tComma,
                                                                                          JSONTokenType.tStringLiteral, JSONTokenType.tComma,
                                                                                          JSONTokenType.tStringLiteral,
                                                            JSONTokenType.tSqBracketClose,
           JSONTokenType.tBraceClose, JSONTokenType.tEOF};

           //lxr.AnalyzeAll();
           //Console.Write(lxr.ToString());
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [TestCase]
        public void IntLiterals()
        {
           Assert.AreEqual(12,  new JL(new StringSource(@"12")).Tokens.First(t=>t.IsPrimary).Value);
           Assert.AreEqual(2,   new JL(new StringSource(@"0b10")).Tokens.First(t=>t.IsPrimary).Value);
           Assert.AreEqual(16,  new JL(new StringSource(@"0x10")).Tokens.First(t=>t.IsPrimary).Value);
           Assert.AreEqual(8,   new JL(new StringSource(@"0o10")).Tokens.First(t=>t.IsPrimary).Value);
        }

        [TestCase]
        public void DoubleLiterals()
        {
           Assert.AreEqual(12.7,  new JL(new StringSource(@"12.7")).Tokens.First(t=>t.IsPrimary).Value);
           Assert.AreEqual(4e+8,  new JL(new StringSource(@"4e+8")).Tokens.First(t=>t.IsPrimary).Value);
        }



        [TestCase]
        public void Comments1()
        {
          var src = @"{
           //'string'}
          }
          ";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]{JSONTokenType.tBOF, JSONTokenType.tBraceOpen, JSONTokenType.tComment, JSONTokenType.tBraceClose, JSONTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [TestCase]
        public void Comments2()
        {
          var src = @"{
           /*'string'}*/
          }
          ";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]{JSONTokenType.tBOF, JSONTokenType.tBraceOpen, JSONTokenType.tComment, JSONTokenType.tBraceClose, JSONTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [TestCase]
        public void Comments3()
        {
          var src = @"{/*
                     
          'string //'}//inner
          */
          }
          ";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]{JSONTokenType.tBOF, JSONTokenType.tBraceOpen, JSONTokenType.tComment, JSONTokenType.tBraceClose, JSONTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [TestCase]
        public void Comments4()
        {
          var src = @"{
           //comment text
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual("comment text", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments5()
        {
          var src = @"{
          /* //comment text */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(" //comment text ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments6()
        {
          var src = @"{
          /* //comment text "+"\n"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(" //comment text \n ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments7()
        {
          var src = @"{
          /* //comment text "+"\r"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(" //comment text \r ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments8()
        {
          var src = @"{
          /* //comment text "+"\r\n"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(" //comment text \r\n ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments9()
        {
          var src = @"{
          /* //comment text "+"\n\r"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(" //comment text \n\r ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments10()
        {
          var src = @"{       
          |* /* //comment text "+"\n\r"+@" */ *|
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(" /* //comment text \n\r */ ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments11withStrings()
        {
          var src = @"{       
          $'|* /* //comment text "+"\n\r"+@" */ *|'
          }
          ";

          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(JSONTokenType.tStringLiteral, lxr.ElementAt(2).Type);
          Assert.AreEqual("|* /* //comment text \n\r */ *|", lxr.ElementAt(2).Text);
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void Comments12withStrings()
        {
          //string is opened but line break
          var src = @"{       
          '|* /* //comment text "+"\n\r"+@" */ *|'
          }
          ";

          var lxr = new JL(new StringSource(src));

          lxr.AnalyzeAll();
        }


        [TestCase]
        public void Comments13withStrings()
        {
          var src = @"{       
          |*'aaaa /* //comment""text "+"\n\r"+@" */ *|
          }
          ";
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(JSONTokenType.tComment, lxr.ElementAt(2).Type);
          Assert.AreEqual("'aaaa /* //comment\"text \n\r */ ", lxr.ElementAt(2).Text);
        }



        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedComment", MatchType=MessageMatch.Contains)]
        public void eUnterminatedComment1()
        {
          var src = @"a: /*aa
          
          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedComment", MatchType=MessageMatch.Contains)]
        public void eUnterminatedComment2()
        {
          var src = @"a: |*aa
          
          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [TestCase]
        public void String1()
        {
          var src = @"{'string'}";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]{JSONTokenType.tBOF, JSONTokenType.tBraceOpen, JSONTokenType.tStringLiteral, JSONTokenType.tBraceClose, JSONTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [TestCase]
        public void String2()
        {
          var src = @"{""string""}";

          var lxr = new JL(new StringSource(src));

          var expected = new JSONTokenType[]{JSONTokenType.tBOF, JSONTokenType.tBraceOpen, JSONTokenType.tStringLiteral, JSONTokenType.tBraceClose, JSONTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString1()
        {
          var src = @"a: 'aaaa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString2()
        {
          var src = @"a: ""aaaa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString3_Verbatim()
        {
          var src = @"a: $""aa
          
          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString4_Verbatim()
        {
          var src = @"a: $'aa
          
          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [TestCase]
        public void String_Escapes1()
        {
          var src = @"{""str\""ing""}";
                                              
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(@"str""ing", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes2()
        {
          var src = @"{'str\'ing'}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(@"str'ing", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes2_1()
        {
          var src = @"{a: -2, 'str\'ing\'': 2}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(@"str'ing'", lxr.ElementAt(7).Text);
        }

        [TestCase]
        public void String_Escapes2_2()
        {
          var src = @"{a: -2, 'string\'': 2}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(@"string'", lxr.ElementAt(7).Text);
        }

        [TestCase]
        public void String_Escapes3()
        {
          var src = @"{'str\n\rring'}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual("str\n\rring", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes4_Unicode()
        {
          var src = @"{'str\u8978ring'}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes5_Unicode()
        {
          var src = @"{""str\u8978ring""}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes6_ForwardSlash()
        {
          var src = @"{'male\/female'}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual("male/female", lxr.ElementAt(2).Text);
        }


        [TestCase]
        public void PlusInt()
        {
          var src = @"{+12}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(JSONTokenType.tPlus, lxr.ElementAt(2).Type);
          Assert.AreEqual(12, lxr.ElementAt(3).Value);
        }

        [TestCase]
        public void MinusInt()
        {
          var src = @"{-12}";
                                               
          var lxr = new JL(new StringSource(src));

          Assert.AreEqual(JSONTokenType.tMinus, lxr.ElementAt(2).Type);
          Assert.AreEqual(12, lxr.ElementAt(3).Value);
        }



        [TestCase]
        public void PatternSearch()
        {
          var src = @"{a: 2, b: 'Znatoki', c: false, d: null, e: ['a','b','c']}";

          var lxr = new JL(new StringSource(src));

                    
          var bvalue = lxr.LazyFSM(
                 (s,t) => t.LoopUntilAny("b"),
                 (s,t) => t.IsAnyOrAbort(JSONTokenType.tColon),
                 (s,t) => FSMI.TakeAndComplete
             );
          
          Assert.AreEqual( "Znatoki", bvalue.Text );
        }

        [TestCase]
        public void PatternSearch2()
        {
          var src = @"{a: 2, b: 'Znatoki', c: false}";

          var lxr = new JL(new StringSource(src));

                    
          var bvalue = lxr.LazyFSM(
                 (s,t) => t.LoopUntilAny("b"),
                 (s,t) => t.IsAnyOrAbort(JSONTokenType.tColon),
                 (s,t) => FSMI.Take,
                 (s,t) => t.IsAnyOrAbort(JSONTokenType.tComma)
             );
          
          Assert.AreEqual( "Znatoki", bvalue.Text );
        }

        [TestCase]
        public void PatternSearch3()
        {
          var src = @"{a: 2, b: 'Znatoki'}";

          var lxr = new JL(new StringSource(src));

                    
          var bvalue = lxr.LazyFSM(
                 (s,t) => t.LoopUntilAny("b"),
                 (s,t) => t.IsAnyOrAbort(JSONTokenType.tColon),
                 (s,t) => FSMI.Take,
                 (s,t) => t.IsAnyOrAbort(JSONTokenType.tComma)
             );
          
          Assert.AreEqual( null, bvalue );
        }

        [TestCase]
        public void PatternSearch4_LoopUntilMatch()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q: 2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilMatch(
                                            (ss, tk) => tk.LoopUntilAny(JSONTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => s.Skip(3),
                 (s,t) => t.LoopUntilAny(JSONTokenType.tStringLiteral),
                 (s,t) => t.IsAnyOrAbort(JSONTokenType.tColon),
                 (s,t) => FSMI.Take,
                 (s,t) => t.IsAnyOrAbort(JSONTokenType.tBraceClose)
             );
        
          Assert.IsNotNull( capture );
     
          Assert.AreEqual( JSONTokenType.tIntLiteral, capture.Type );
          Assert.AreEqual( 127, capture.Value );
        }

        [TestCase]
        public void PatternSearch5_Skip()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q:2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(
                 (s,t) => s.Skip(1),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tIdentifier, capture.Type );
          Assert.AreEqual( "a", capture.Text );

          capture = lxr.LazyFSM(
                 (s,t) => s.Skip(9),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tIdentifier, capture.Type );
          Assert.AreEqual( "q", capture.Text );
        }

        [TestCase]
        public void PatternSearch6_LoopUntilMatch()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q: 2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tStringLiteral, capture.Type );
          Assert.AreEqual( "Name", capture.Value );
        }


        [TestCase]
        public void PatternSearch7_LoopUntilAfterMatch()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q: 2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tComma, capture.Type );
        }

        [TestCase]
        public void PatternSearch8_LoopUntilAfterMatch()
        {
          var src = @"'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tNull, capture.Type );
        }

        [TestCase]
        public void PatternSearch8_LoopUntilAfterMatch_AnyToken()
        {
          var src = @"'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(false,
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tNull, capture.Type );
        }

        [TestCase]
        public void PatternSearch9_LoopUntilAfterMatch()
        {
          var src = @"'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.LoopUntilAny(JSONTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tNull, capture.Type );
        }


        [TestCase]
        public void PatternSearch10_LoopUntilAfterMatch()
        {
          var src = @"1,2,3,4,5,6,7,8,9 : 'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));

                    
          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.LoopUntilAny(JSONTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );
        
          Assert.IsNotNull( capture );
          Assert.AreEqual( JSONTokenType.tNull, capture.Type );
        }


        [JSONPersonMatch]
        [TestCase]
        public void JSONPatternMatchAttribute1()
        {
          var src = @"1,2,3,4,5,6,7,8,9 : 'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));

          var match = JSONPatternMatchAttribute.Check(MethodBase.GetCurrentMethod(), lxr);          
         
        
          Assert.IsFalse( match );
        }

        [JSONPersonMatch]
        [TestCase]
        public void JSONPatternMatchAttribute2()
        {
          var src = @"{ code: 1121982, 'FirstName': 'Alex', DOB: null}";

          var lxr = new JL(new StringSource(src));

          var match = JSONPatternMatchAttribute.Check(MethodBase.GetCurrentMethod(), lxr);          
         
        
          Assert.IsTrue( match );
        }

        [JSONPersonMatch]
        [TestCase]
        public void JSONPatternMatchAttribute3()
        {
          var src = @"{ code: 1121982, color: red, 'first_name': 'Alex', DOB: null}";

          var lxr = new JL(new StringSource(src));

          var match = JSONPatternMatchAttribute.Check(MethodBase.GetCurrentMethod(), lxr);          
         
        
          Assert.IsTrue( match );
        }

    }


                        public class JSONPersonMatchAttribute : JSONPatternMatchAttribute
                        {
                            public override bool Match(NFX.CodeAnalysis.JSON.JSONLexer content)
                            {
                              return content.LazyFSM(
                                     (s,t) => s.LoopUntilMatch(
                                                                (ss, tk) => tk.LoopUntilAny("First-Name","FirstName","first_name"),
                                                                (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tColon),
                                                                (ss, tk) => tk.IsAnyOrAbort(JSONTokenType.tStringLiteral),
                                                                (ss, tk) => FSMI.TakeAndComplete
                                                              ),
                                     (s,t) => FSMI.Take
                                 ) != null;  
                            }
                        }


}
