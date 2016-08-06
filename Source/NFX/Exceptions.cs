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




}