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

using NFX;

namespace NFX.DataAccess.CRUD
{
      /// <summary>
      /// Thrown by CRUD data access classes
      /// </summary>
      [Serializable]
      public class CRUDException : DataAccessException
      {
            public CRUDException() {}

            public CRUDException(string message) : base(message){}

            public CRUDException(string message, Exception inner) : base(message, inner){}

            public CRUDException(string message, KeyViolationKind kvKind, string keyViolation)
              : base(message, kvKind, keyViolation){}

            public CRUDException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation)
              : base(message, inner, kvKind, keyViolation){}

            protected CRUDException(SerializationInfo info, StreamingContext context)
              : base(info, context) {}
      }

      /// <summary>
      /// Thrown by CRUD data access classes when validation does not pass
      /// </summary>
      [Serializable]
      public class CRUDValidationException : CRUDException
      {
            public CRUDValidationException() {}

            public CRUDValidationException(string message) : base(message){}

            public CRUDValidationException(string message, Exception inner) : base(message, inner){}

            public CRUDValidationException(string message, KeyViolationKind kvKind, string keyViolation)
              : base(message, kvKind, keyViolation){}

            public CRUDValidationException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation)
              : base(message, inner, kvKind, keyViolation){}

            protected CRUDValidationException(SerializationInfo info, StreamingContext context)
              : base(info, context) {}
      }

      /// <summary>
      /// Thrown by CRUD data access classes when field validation does not pass
      /// </summary>
      [Serializable]
      public class CRUDRowValidationException : CRUDValidationException
      {
            public const string WHAT = "Schema: '{0}'; ";

            public readonly string SchemaName;

            public CRUDRowValidationException(string schemaName)
              : base(WHAT.Args(schemaName))
            {
                SchemaName = schemaName;
            }

            public CRUDRowValidationException(string schemaName, string message)
              : base(WHAT.Args(schemaName) + message)
            {
                SchemaName = schemaName;
            }

            public CRUDRowValidationException(string schemaName, string message, Exception inner)
              : base(WHAT.Args(schemaName) + message, inner)
            {
                SchemaName = schemaName;
            }

            protected CRUDRowValidationException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }

      }


      /// <summary>
      /// Thrown by CRUD data access classes when field validation does not pass
      /// </summary>
      [Serializable]
      public class CRUDFieldValidationException : CRUDValidationException
      {
            public const string WHAT = "Schema field: '{0}'.{1}; ";

            public readonly string SchemaName;
            public readonly string FieldName;
            public readonly string ClientMessage;


            public CRUDFieldValidationException(Row row, string fieldName, string message)
              : this(row.NonNull(text: "row").Schema.Name,
                     row.Schema[fieldName].NonNull(text: "field {0} not found in schema".Args(fieldName)).Name, message)
            {
            }

            public CRUDFieldValidationException(string schemaName, string fieldName)
              : base(WHAT.Args(schemaName, fieldName))
            {
                SchemaName = schemaName;
                FieldName = fieldName;
                ClientMessage = "Validation error";
            }

            public CRUDFieldValidationException(string schemaName, string fieldName, string message)
              : base(WHAT.Args(schemaName, fieldName) + message)
            {
                SchemaName = schemaName;
                FieldName = fieldName;
                ClientMessage = message;
            }

            public CRUDFieldValidationException(string schemaName, string fieldName, string message, Exception inner)
              : base(WHAT.Args(schemaName, fieldName) + message, inner)
            {
                SchemaName = schemaName;
                FieldName = fieldName;
                ClientMessage = message;
            }

            protected CRUDFieldValidationException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }

      }

 }