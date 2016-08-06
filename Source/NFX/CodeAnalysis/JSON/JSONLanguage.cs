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
    /// Represents JSON language
    /// </summary>
    public sealed class JSONLanguage : Language
    {
        public static readonly JSONLanguage Instance = new JSONLanguage();

        private JSONLanguage() : base() {}


        public override LanguageFamily Family
        {
            get { return LanguageFamily.JavaScript; }
        }

        public override IEnumerable<string> FileExtensions
        {
            get
            {
                yield return "jsn";
                yield return "json";
            }
        }

        public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
        {
            return new JSONLexer(context, srcRef, source, messages, throwErrors);
        }



    }
}
