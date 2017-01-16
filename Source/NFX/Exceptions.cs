/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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

using NFX.ApplicationModel;

namespace NFX
{

  /// <summary>
  /// Base exception thrown by the framework
  /// </summary>
  [Serializable]
  public class NFXException : Exception
  {
    public NFXException()
    {
    }

    public NFXException(string message)
      : base(message)
    {
    }

    public NFXException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected NFXException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }


    /// <summary>
    /// Provides general-purpose error code
    /// </summary>
    public int Code { get; set;}

  }


  /// <summary>
  /// Thrown by Debug class to indicate assertion failures
  /// </summary>
  [Serializable]
  public sealed class DebugAssertionException : NFXException
  {
    private string m_From;

    public string From
    {
      get { return m_From ?? string.Empty; }
    }


    public DebugAssertionException()
    {
    }

    public DebugAssertionException(string message, string from = null) : base(message)
    {
       m_From = from;
    }

    public DebugAssertionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {

    }

  }


  /// <summary>
  /// Marshalls exception details
  /// </summary>
  [Serializable]
  public sealed class WrappedExceptionData
  {

    /// <summary>
    /// Initializes instance form local exception
    /// </summary>
    public WrappedExceptionData(Exception error, bool captureStack = true)
    {
      var tp = error.GetType();
      m_TypeName = tp.FullName;
      m_Message = error.Message;
      if (error is NFXException)
        m_Code = ((NFXException)error).Code;

      m_ApplicationName = ExecutionContext.Application.Name;

      m_Source = error.Source;
      if (captureStack)
        m_StackTrace = error.StackTrace;

      if (error.InnerException != null)
        m_InnerException = new WrappedExceptionData(error.InnerException);
    }


    private string m_TypeName;
    private string m_Message;
    private int m_Code;
    private string m_ApplicationName;
    private string m_Source;
    private string m_StackTrace;
    private WrappedExceptionData m_InnerException;

    /// <summary>
    /// Returns the name of remote exception type
    /// </summary>
    public string TypeName { get { return m_TypeName ?? CoreConsts.UNKNOWN; } }

    /// <summary>
    /// Returns the message of remote exception
    /// </summary>
    public string Message { get { return m_Message ?? string.Empty; } }

    /// <summary>
    /// Returns the code of remote NFX exception
    /// </summary>
    public int Code { get { return m_Code; } }


    /// <summary>
    /// Name of the object that caused the error
    /// </summary>
    public string Source { get { return m_Source ?? string.Empty; } }

    /// <summary>
    /// Returns stack trace
    /// </summary>
    public string StackTrace { get { return m_StackTrace ?? string.Empty; } }


    /// <summary>
    /// Returns the name of remote application
    /// </summary>
    public string ApplicationName { get { return m_ApplicationName ?? CoreConsts.UNKNOWN; } }


    /// <summary>
    /// Returns the inner remote exception if any
    /// </summary>
    public WrappedExceptionData InnerException { get { return m_InnerException; } }


    public override string ToString()
    {
      return string.Format("[{0}:{1}:{2}] {3}", TypeName, Code, ApplicationName, Message);
    }
  }

  /// <summary>
  /// Represents exception that contains data about causing exception with all of it's chain
  /// </summary>
  [Serializable]
  public sealed class WrappedException : NFXException
  {
    private WrappedExceptionData m_Wrapped;

    /// <summary>
    /// Returns wrapped exception data
    /// </summary>
    public WrappedExceptionData Wrapped
    {
      get { return m_Wrapped; }
    }


    public WrappedException(WrappedExceptionData data) : base(data.ToString())
    {
      m_Wrapped = data;
    }

    public WrappedException(string message, WrappedExceptionData data)
      : base(message)
    {
      m_Wrapped = data;
    }

    public WrappedException(string message, WrappedExceptionData data, Exception inner)
      : base(message, inner)
    {
      m_Wrapped = data;
    }

    internal WrappedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}