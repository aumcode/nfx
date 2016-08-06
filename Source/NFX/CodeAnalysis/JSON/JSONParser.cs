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

namespace NFX.CodeAnalysis.JSON
{
    /// <summary>
    /// Parses JSON lexer output into object graphs.
    /// NOTE: Although called JSON, this is really a JSON superset implementation that includes extra features:
    ///  comments, directives, verbatim strings(start with $), ' or " string escapes, unquoted object key names
    /// </summary>
    public sealed partial class JSONParser : Parser<JSONLexer>
    {

        public JSONParser(JSONLexer input,  MessageList messages = null, bool throwErrors = false, bool caseSensitiveMaps = true) :
            base(new JSONData(), new JSONLexer[]{ input }, messages, throwErrors)
        {
            m_Lexer = Input.First();
            m_CaseSensitiveMaps = caseSensitiveMaps;
        }


        public JSONParser(JSONData context, JSONLexer input,  MessageList messages = null, bool throwErrors = false, bool caseSensitiveMaps = true) :
            base(context, new JSONLexer[]{ input }, messages, throwErrors)
        {
            m_Lexer = Input.First();
            m_CaseSensitiveMaps = caseSensitiveMaps;
        }

        private JSONLexer m_Lexer;

        private bool m_CaseSensitiveMaps;

        public JSONLexer Lexer { get { return m_Lexer;} }

        public JSONData ResultContext { get{ return Context as JSONData;} }

        public override Language Language
        {
            get { return JSONLanguage.Instance; }
        }

        public override string MessageCodeToString(int code)
        {
            return ((JSONMsgCode)code).ToString();;
        }




        protected override void DoParse()
        {
            try
            {
                tokens = Lexer.GetEnumerator();
                fetchPrimary();
                var root = doAny();
                ResultContext.setData( root );
            }
            catch(abortException)
            {

            }
        }
    }
}
