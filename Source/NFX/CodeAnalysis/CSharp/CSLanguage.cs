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

namespace NFX.CodeAnalysis.CSharp
{
    /// <summary>
    /// Represents C# language
    /// </summary>
    public sealed class CSLanguage : Language
    {
        public static readonly CSLanguage Instance = new CSLanguage();

        private CSLanguage() : base() {}


        public override LanguageFamily Family
        {
            get { return LanguageFamily.CSharp; }
        }

        public override IEnumerable<string> FileExtensions
        {
            get
            {
                yield return "cs";
            }
        }

        public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
        {
            return new CSLexer(context, srcRef, source, messages, throwErrors);
        }



    }
}
