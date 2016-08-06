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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.3  2009.10.12
 */
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.CodeDom.Compiler;

namespace NFX.Templatization
{

  /// <summary>
  /// Base exception thrown by the templatization-related functionality
  /// </summary>
  [Serializable]
  public class TemplatizationException : NFXException
  {
    public TemplatizationException()
    {
    }

    public TemplatizationException(string message)
      : base(message)
    {
    }

    public TemplatizationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected TemplatizationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  }



  /// <summary>
  /// Base exception thrown by the template compilers
  /// </summary>
  [Serializable]
  public class TemplateCompilerException : TemplatizationException
  {
    public TemplateCompilerException()
    {
    }

    public TemplateCompilerException(string message)
      : base(message)
    {
    }

    public TemplateCompilerException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected TemplateCompilerException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  }


  /// <summary>
  /// Indicates template source parsing exception
  /// </summary>
  [Serializable]
  public class TemplateParseException : TemplateCompilerException
  {
    public TemplateParseException()
    {
    }

    public TemplateParseException(string message)
      : base(message)
    {
    }

    public TemplateParseException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected TemplateParseException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  }


  /// <summary>
  /// Thrown by  template code compilers
  /// </summary>
  [Serializable]
  public class TemplateCodeCompilerException : TemplateCompilerException
  {
    public readonly CompilerError Error;

    public TemplateCodeCompilerException(CompilerError err) : base(err.ErrorText)
    {
      Error = err;
    }

    public override string ToString()
    {
        return string.Format("#{0} {1} Warn: {2} Line: {3} Column: {4} File: \"{5}\"",
                             Error.ErrorNumber,
                             Error.ErrorText,
                             Error.IsWarning,
                             Error.Line,
                             Error.Column,
                             Error.FileName);
    }

  }




}