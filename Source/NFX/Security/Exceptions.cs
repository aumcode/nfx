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

namespace NFX.Security
{
  /// <summary>
  /// Base exception thrown by the NFX Security framework
  /// </summary>
  [Serializable]
  public class SecurityException : NFXException, ISecurityException
  {
    public SecurityException() { }
    public SecurityException(string message) : base(message) { }
    public SecurityException(string message, Exception inner) : base(message, inner) { }
    protected SecurityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by NFX security to indicate the authorization problems, such as permission access denial
  /// </summary>
  [Serializable]
  public class AuthorizationException : SecurityException
  {
    public AuthorizationException() { }
    public AuthorizationException(string message) : base(message) { }
    public AuthorizationException(string message, Exception inner) : base(message, inner) { }
    protected AuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }


    /// <summary>
    /// Returns true when the specified exception is AuthorizationException or AuthorizationException exists
    /// in the chain of InnerException/s of the specified exception
    /// </summary>
    public static bool IsDenotedBy(Exception error)
    {
      while(error!=null)
      {
        if (error is AuthorizationException) return true;
        error = error.InnerException;
      }

      return false;
    }

  }
}
