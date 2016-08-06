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

namespace NFX.CodeAnalysis.Laconfig
{
    /// <summary>
    /// Parses Laconfig lexer output into laconic configuration node graph
    /// </summary>
    public sealed partial class LaconfigParser : Parser<LaconfigLexer>
    {

        public LaconfigParser(LaconfigData context, LaconfigLexer input,  MessageList messages = null, bool throwErrors = false) :
            base(context, new LaconfigLexer[]{ input }, messages, throwErrors)
        {
            m_Lexer = Input.First();
        }

        private LaconfigLexer m_Lexer;

        public LaconfigLexer Lexer { get { return m_Lexer;} }

        public LaconfigData ResultContext { get{ return Context as LaconfigData;} }

        public override Language Language
        {
            get { return LaconfigLanguage.Instance; }
        }

        public override string MessageCodeToString(int code)
        {
            return ((LaconfigMsgCode)code).ToString();
        }




        protected override void DoParse()
        {
            try
            {
                var config = ResultContext.ResultObject;

                tokens = Lexer.GetEnumerator();
                fetchPrimary();
                doRoot( config );

                if (config.Root.Exists)
                   config.Root.ResetModified();

            }
            catch(abortException)
            {

            }
        }
    }
}
