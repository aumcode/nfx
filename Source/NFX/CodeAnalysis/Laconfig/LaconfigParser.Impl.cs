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

using NFX.Environment;
using NFX.CodeAnalysis.Source;

namespace NFX.CodeAnalysis.Laconfig
{
    public sealed partial class LaconfigParser : Parser<LaconfigLexer>
    {
            private class abortException: Exception {}


            private IEnumerator<LaconfigToken> tokens;
            private LaconfigToken token;

            private void abort() { throw new abortException(); }

            private void errorAndAbort(LaconfigMsgCode code)
            {
               EmitMessage(MessageType.Error, (int)code, Lexer.SourceCodeReference,token: token);
               throw new abortException();
            }

            private void fetch()
            {
                if (!tokens.MoveNext())
                   errorAndAbort(LaconfigMsgCode.ePrematureEOF);

                token = tokens.Current;
            }

            private void fetchPrimary()
            {
                do fetch();
                while(!token.IsPrimary);
            }

            private void fetchPrimaryOrEOF()
            {
                do fetch();
                while(!token.IsPrimary && token.Type!=LaconfigTokenType.tEOF);
            }


            private void doRoot(Configuration config)
            {
               if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                 errorAndAbort(LaconfigMsgCode.eSectionNameExpected);

               config.Create(token.Text);
               fetchPrimary();
               if (token.Type==LaconfigTokenType.tEQ)
               {
                    fetchPrimary();
                    if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                          errorAndAbort(LaconfigMsgCode.eSectionOrAttributeValueExpected);
                    config.Root.Value = token.Text;
                    fetchPrimary();
               }

               populateSection(config.Root);

               if (token.Type!=LaconfigTokenType.tEOF)
                 errorAndAbort(LaconfigMsgCode.eContentPastRootSection);
            }


            private void populateSection(ConfigSectionNode section)
            {
                if (token.Type!=LaconfigTokenType.tBraceOpen)
                      errorAndAbort(LaconfigMsgCode.eSectionOpenBraceExpected);

                fetchPrimary();//skip {  section started

                while(true)
                {
                    if (token.Type==LaconfigTokenType.tBraceClose)
                    {
                      fetchPrimaryOrEOF();//skip }  section ended
                      return;
                    }

                    if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                      errorAndAbort(LaconfigMsgCode.eSectionOrAttributeNameExpected);

                    var name = token.Text;
                    fetchPrimary();

                    if (token.Type==LaconfigTokenType.tBraceOpen)//section w/o value
                    {
                       var subsection = section.AddChildNode(name, null);
                       populateSection(subsection);
                    }else if (token.Type==LaconfigTokenType.tEQ)//section with value or attribute
                    {
                       fetchPrimary();
                       if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                          errorAndAbort(LaconfigMsgCode.eSectionOrAttributeValueExpected);

                       var value = token.Text;
                       fetchPrimary();//skip value

                       if (token.Type==LaconfigTokenType.tBraceOpen)//section with value
                       {
                         var subsection = section.AddChildNode(name, value);
                         populateSection(subsection);
                       }
                       else
                        section.AddAttributeNode(name, value);

                    }else
                       errorAndAbort(LaconfigMsgCode.eSyntaxError);
               }
            }

    }
}
