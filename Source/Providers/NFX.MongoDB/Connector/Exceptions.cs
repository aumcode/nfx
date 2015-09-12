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
using System.Runtime.Serialization;

using NFX.DataAccess;
using NFX.DataAccess.MongoDB;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Thrown by MongoDB data access classes
  /// </summary>
  [Serializable]
  public class MongoDBConnectorException : MongoDBDataAccessException
  {
    public MongoDBConnectorException() {}

    public MongoDBConnectorException(string message) : base(message){}

    public MongoDBConnectorException(string message, Exception inner) : base(message, inner){}

    public MongoDBConnectorException(string message, KeyViolationKind kvKind, string keyViolation)
      : base(message, kvKind, keyViolation){}

    public MongoDBConnectorException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation)
      : base(message, inner, kvKind, keyViolation){}

    protected MongoDBConnectorException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }


  /// <summary>
  /// Thrown by MongoDB data access classes related to protocol
  /// </summary>
  [Serializable]
  public class MongoDBConnectorProtocolException : MongoDBConnectorException
  {
    public MongoDBConnectorProtocolException() {}

    public MongoDBConnectorProtocolException(string message) : base(message){}

    public MongoDBConnectorProtocolException(string message, Exception inner) : base(message, inner){}

    protected MongoDBConnectorProtocolException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }

  /// <summary>
  /// Thrown by MongoDB data access classes related to server errors
  /// </summary>
  [Serializable]
  public class MongoDBConnectorServerException : MongoDBConnectorException
  {
    public MongoDBConnectorServerException() {}

    public MongoDBConnectorServerException(string message) : base(message){}

    public MongoDBConnectorServerException(string message, Exception inner) : base(message, inner){}

    protected MongoDBConnectorServerException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }

  /// <summary>
  /// Thrown in case of query compile error
  /// </summary>
  [Serializable]
  public class MongoDBQueryException : MongoDBConnectorException
  {
    public MongoDBQueryException() {}

    public MongoDBQueryException(string message) : base(message){}

    public MongoDBQueryException(string message, Exception inner) : base(message, inner){}

    protected MongoDBQueryException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }

}