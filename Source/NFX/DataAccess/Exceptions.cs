/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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

namespace NFX.DataAccess
{

  /// <summary>
  /// Specifies the sub-type of key violation
  /// </summary>
  public enum KeyViolationKind { Unspecified = 0, Primary, Secondary }

  /// <summary>
  /// Thrown by data access classes
  /// </summary>
  [Serializable]
  public class DataAccessException : NFXException
  {
    public const string KEY_VIOLATION_KIND_FLD_NAME = "DAE-KVK";
    public const string KEY_VIOLATION_FLD_NAME = "DAE-KV";

    public DataAccessException() { }
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception inner) : base(message, inner) { }
    public DataAccessException(string message, KeyViolationKind kvKind, string keyViolation)
      : base(message)
    {
      KeyViolationKind = kvKind;
      KeyViolation = keyViolation;
    }

    public DataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation)
      : base(message, inner)
    {
      KeyViolationKind = kvKind;
      KeyViolation = keyViolation;
    }

    protected DataAccessException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      KeyViolationKind = (KeyViolationKind)info.GetInt32(KEY_VIOLATION_KIND_FLD_NAME);
      KeyViolation = info.GetString(KEY_VIOLATION_FLD_NAME);
    }

    /// <summary>
    /// Spcifies the sub-type of key violation
    /// </summary>
    public readonly KeyViolationKind KeyViolationKind;

    /// <summary>
    /// Provides the name of entity/index/field that was violated and resulted in this exception
    /// </summary>
    public readonly string KeyViolation;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(KEY_VIOLATION_KIND_FLD_NAME, KeyViolationKind);
      info.AddValue(KEY_VIOLATION_FLD_NAME, KeyViolation);
      base.GetObjectData(info, context);
    }
  }
}