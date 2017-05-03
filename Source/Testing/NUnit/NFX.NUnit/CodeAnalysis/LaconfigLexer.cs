/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using NFX.CodeAnalysis.Laconfig;
using LL=NFX.CodeAnalysis.Laconfig.LaconfigLexer;
using System.Reflection;

namespace NFX.NUnit.CodeAnalysis
{
    [TestFixture]   
    public class LaconfigLexer
    {

        [TestCase]
        public void BOF_EOF()
        {
          var src = @"a";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tIdentifier, LaconfigTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="ePrematureEOF", MatchType=MessageMatch.Contains)]
        public void ePrematureEOF_Thrown()
        {
          var src = @"";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        public void ePrematureEOF_Logged()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new LL(new StringSource(src), msgs);
          lxr.AnalyzeAll();

          Assert.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)LaconfigMsgCode.ePrematureEOF));
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="ePrematureEOF", MatchType=MessageMatch.Contains)]
        public void ePrematureEOF_CouldLogButThrown()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new LL(new StringSource(src), msgs, throwErrors: true);
          lxr.AnalyzeAll();

          Assert.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)LaconfigMsgCode.ePrematureEOF));
        }


        [TestCase]
        public void TokenClassifications()
        {
          var src = @"a 'string' = 12 {} //comment";

          var tokens = new LL(new StringSource(src)).Tokens;

          Assert.IsTrue( tokens[0].IsBOF);
          Assert.IsTrue( tokens[0].IsNonLanguage);
          Assert.IsFalse( tokens[0].IsPrimary);
          Assert.AreEqual(TokenKind.BOF, tokens[0].Kind);

          Assert.AreEqual( LaconfigTokenType.tIdentifier, tokens[1].Type);
          Assert.IsFalse( tokens[1].IsNonLanguage);
          Assert.IsTrue( tokens[1].IsPrimary);
          Assert.AreEqual(TokenKind.Identifier, tokens[1].Kind);

          Assert.AreEqual( LaconfigTokenType.tStringLiteral, tokens[2].Type);
          Assert.IsFalse( tokens[2].IsNonLanguage);
          Assert.IsTrue( tokens[2].IsPrimary);
          Assert.IsTrue( tokens[2].IsTextualLiteral);
          Assert.AreEqual(TokenKind.Literal, tokens[2].Kind);

          Assert.AreEqual( LaconfigTokenType.tEQ, tokens[3].Type);
          Assert.IsFalse( tokens[3].IsNonLanguage);
          Assert.IsTrue( tokens[3].IsPrimary);
          Assert.IsTrue( tokens[3].IsOperator);
          Assert.AreEqual(TokenKind.Operator, tokens[3].Kind);


          Assert.AreEqual( LaconfigTokenType.tIdentifier, tokens[4].Type);
          Assert.IsFalse( tokens[4].IsNonLanguage);
          Assert.IsTrue( tokens[4].IsPrimary);
          Assert.AreEqual(TokenKind.Identifier, tokens[4].Kind);

          Assert.AreEqual( LaconfigTokenType.tBraceOpen, tokens[5].Type);
          Assert.IsFalse( tokens[5].IsNonLanguage);
          Assert.IsTrue( tokens[5].IsPrimary);
          Assert.AreEqual(TokenKind.Symbol, tokens[5].Kind);

          Assert.AreEqual( LaconfigTokenType.tBraceClose, tokens[6].Type);
          Assert.IsFalse( tokens[6].IsNonLanguage);
          Assert.IsTrue( tokens[6].IsPrimary);
          Assert.AreEqual(TokenKind.Symbol, tokens[6].Kind);

          Assert.AreEqual( LaconfigTokenType.tComment, tokens[7].Type);
          Assert.IsFalse( tokens[7].IsNonLanguage);
          Assert.IsFalse( tokens[7].IsPrimary);
          Assert.IsTrue( tokens[7].IsComment);
          Assert.AreEqual(TokenKind.Comment, tokens[7].Kind);

        }

        [TestCase]
        public void Comments1()
        {
          var src = @"{
           //'string'}
          }
          ";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tComment, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [TestCase]
        public void Comments2()
        {
          var src = @"{
           /*'string'}*/
          }
          ";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tComment, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
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

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tComment, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [TestCase]
        public void Comments4()
        {
          var src = @"{
           //comment text
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual("comment text", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments5()
        {
          var src = @"{
          /* //comment text */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(" //comment text ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments6()
        {
          var src = @"{
          /* //comment text "+"\n"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(" //comment text \n ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments7()
        {
          var src = @"{
          /* //comment text "+"\r"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(" //comment text \r ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments8()
        {
          var src = @"{
          /* //comment text "+"\r\n"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(" //comment text \r\n ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments9()
        {
          var src = @"{
          /* //comment text "+"\n\r"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(" //comment text \n\r ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments10()
        {
          var src = @"{       
          |* /* //comment text "+"\n\r"+@" */ *|
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(" /* //comment text \n\r */ ", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void Comments11withStrings()
        {
          var src = @"{       
          $'|* /* //comment text "+"\n\r"+@" */ *|'
          }
          ";

          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(LaconfigTokenType.tStringLiteral, lxr.ElementAt(2).Type);
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

          var lxr = new LL(new StringSource(src));

          lxr.AnalyzeAll();
        }


        [TestCase]
        public void Comments13withStrings()
        {
          var src = @"{       
          |*'aaaa /* //comment""text "+"\n\r"+@" */ *|
          }
          ";
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(LaconfigTokenType.tComment, lxr.ElementAt(2).Type);
          Assert.AreEqual("'aaaa /* //comment\"text \n\r */ ", lxr.ElementAt(2).Text);
        }



        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedComment", MatchType=MessageMatch.Contains)]
        public void eUnterminatedComment1()
        {
          var src = @"a: /*aa
          
          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedComment", MatchType=MessageMatch.Contains)]
        public void eUnterminatedComment2()
        {
          var src = @"a: |*aa
          
          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [TestCase]
        public void String1()
        {
          var src = @"{'string'}";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tStringLiteral, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [TestCase]
        public void String2()
        {
          var src = @"{""string""}";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tStringLiteral, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Assert.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString1()
        {
          var src = @"a: 'aaaa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString2()
        {
          var src = @"a: ""aaaa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString3_Verbatim()
        {
          var src = @"a: $""aa
          
          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedString", MatchType=MessageMatch.Contains)]
        public void eUnterminatedString4_Verbatim()
        {
          var src = @"a: $'aa
          
          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [TestCase]
        public void String_Escapes1()
        {
          var src = @"{""str\""ing""}";
                                              
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(@"str""ing", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes2()
        {
          var src = @"{'str\'ing'}";
                                               
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(@"str'ing", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes2_1()
        {
          var src = @"a{ n='str\'ing\''}";
                                               
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(@"str'ing'", lxr.ElementAt(5).Text);
        }

        [TestCase]
        public void String_Escapes2_2()
        {
          var src = @"a{ n = 'string\''}";
                                               
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual(@"string'", lxr.ElementAt(5).Text);
        }

        [TestCase]
        public void String_Escapes3()
        {
          var src = @"{'str\n\rring'}";
                                               
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual("str\n\rring", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes4_Unicode()
        {
          var src = @"{'str\u8978ring'}";
                                               
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }

        [TestCase]
        public void String_Escapes5_Unicode()
        {
          var src = @"{""str\u8978ring""}";
                                               
          var lxr = new LL(new StringSource(src));

          Assert.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }


    }


}
