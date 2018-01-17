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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Runtime.Serialization;

namespace NFX.DataAccess.MongoDB
{
  /// <summary>
  /// Thrown by MongoDB data access classes
  /// </summary>
  [Serializable]
  public class MongoDBDataAccessException : DataAccessException
  {
    public MongoDBDataAccessException() { }
    public MongoDBDataAccessException(string message) : base(message) { }
    public MongoDBDataAccessException(string message, Exception inner) : base(message, inner) { }
    public MongoDBDataAccessException(string message, KeyViolationKind kvKind, string keyViolation) : base(message, kvKind, keyViolation) { }
    public MongoDBDataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation) : base(message, inner, kvKind, keyViolation) { }
    protected MongoDBDataAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}