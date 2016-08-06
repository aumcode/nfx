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
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Runtime.Serialization;

using NFX.DataAccess;

namespace NFX.DataAccess.Erlang
{
  /// <summary>
  /// Thrown by Erlang data access classes
  /// </summary>
  [Serializable]
  public class ErlDataAccessException : DataAccessException
  {

    public ErlDataAccessException() {}

    public ErlDataAccessException(string message) : base(message){}

    public ErlDataAccessException(string message, Exception inner) : base(message, inner){}

    public ErlDataAccessException(string message, KeyViolationKind kvKind, string keyViolation)
      : base(message, kvKind, keyViolation){}

    public ErlDataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation)
      : base(message, inner, kvKind, keyViolation){}

    protected ErlDataAccessException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }

  /// <summary>
  /// Thrown by Erlang data schema map when server has changed schema. The client needs to restart or
  ///  kill all cached data and refetch schema
  /// </summary>
  [Serializable]
  public class ErlServerSchemaChangedException : ErlDataAccessException
  {

    public ErlServerSchemaChangedException() {}

    protected ErlServerSchemaChangedException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }

}