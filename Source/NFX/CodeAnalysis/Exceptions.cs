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
using System.Runtime.Serialization;

namespace NFX.CodeAnalysis
{
  /// <summary>
  /// Base exception thrown by the framework
  /// </summary>
  [Serializable]
  public class CodeAnalysisException : NFXException
  {
    public CodeAnalysisException() { }
    public CodeAnalysisException(string message) : base(message) { }
    public CodeAnalysisException(string message, Exception inner) : base(message, inner) { }
    protected CodeAnalysisException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by code processors such as lexers, parsers ,  symantic analyzers, compilers etc...
  /// </summary>
  [Serializable]
  public class CodeProcessorException : CodeAnalysisException, IWrappedExceptionDataSource
  {
    public CodeProcessorException(ICodeProcessor codeProcessor) { CodeProcessor = codeProcessor; }
    public CodeProcessorException(ICodeProcessor codeProcessor, string message) : base(message) { CodeProcessor = codeProcessor; }
    public CodeProcessorException(ICodeProcessor codeProcessor, string message, Exception inner) : base(message, inner) { CodeProcessor = codeProcessor; }

    [NonSerialized]
    public readonly ICodeProcessor CodeProcessor;

    public string GetWrappedData()
    {
      return "{0}({1}{2}){3}".Args(CodeProcessor.GetType().Name, CodeProcessor.Language,
        CodeProcessor.Context != null ? ", context=" + CodeProcessor.Context.GetType().Name : string.Empty,
        CodeProcessor.Messages != null ? ": " + CodeProcessor.Messages.ToString() : string.Empty);
    }
  }

  [Serializable]
  public class StringEscapeErrorException : CodeAnalysisException
  {
    public const string ERRORED_ESCAPE_FLD_NAME = "SEEE-EE";

    private StringEscapeErrorException() { }
    public StringEscapeErrorException(string escape) { ErroredEscape = escape; }
    protected StringEscapeErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      ErroredEscape = info.GetString(ERRORED_ESCAPE_FLD_NAME);
    }

    public readonly string ErroredEscape;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(ERRORED_ESCAPE_FLD_NAME, ErroredEscape);
      base.GetObjectData(info, context);
    }
  }
}
