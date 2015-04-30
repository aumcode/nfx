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
using System.Runtime.Serialization;
using System.Text;

namespace NFX.CodeAnalysis
{
      /// <summary>
      /// Base exception thrown by the framework
      /// </summary>
      [Serializable]
      public class CodeAnalysisException : NFXException
      {
        public CodeAnalysisException()
        {
        }

        public CodeAnalysisException(string message)
          : base(message)
        {
        }

        public CodeAnalysisException(string message, Exception inner)
          : base(message, inner)
        {
        }

        protected CodeAnalysisException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {

        }

      }


      /// <summary>
      /// Thrown by code processors such as lexers, parsers ,  symantic analyzers, compilers etc...
      /// </summary>
      public class CodeProcessorException : CodeAnalysisException
      {
        public readonly ICodeProcessor CodeProcessor;

        public CodeProcessorException(ICodeProcessor codeProcessor)
        {
            CodeProcessor = codeProcessor;
        }

        public CodeProcessorException(ICodeProcessor codeProcessor, string message) : base(message)
        {
            CodeProcessor = codeProcessor;
        }

        public CodeProcessorException(ICodeProcessor codeProcessor, string message, Exception inner)
          : base(message, inner)
        {
            CodeProcessor = codeProcessor;
        }

      }


      public class StringEscapeErrorException : CodeAnalysisException
      {
        public readonly string ErroredEscape;


        private StringEscapeErrorException()
        {
        }

        public StringEscapeErrorException(string escape)
        {
          ErroredEscape = escape;
        }

      }


}
