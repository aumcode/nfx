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

namespace NFX.Serialization.JSON
{
  /// <summary>
  /// Base exception thrown by the JSON serialization format
  /// </summary>
  [Serializable]
  public class JSONException : NFXSerializationException
  {
    public JSONException() { }
    public JSONException(string message) : base(message) { }
    public JSONException(string message, Exception inner) : base(message, inner) { }
    protected JSONException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Base exception thrown by the JSON when serializing objects
  /// </summary>
  [Serializable]
  public class JSONSerializationException : JSONException
  {
    public JSONSerializationException() { }
    public JSONSerializationException(string message) : base(message) { }
    public JSONSerializationException(string message, Exception inner) : base(message, inner) { }
    protected JSONSerializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Base exception thrown by the JSON when deserializing objects
  /// </summary>
  [Serializable]
  public class JSONDeserializationException : JSONException
  {
    public JSONDeserializationException() { }
    public JSONDeserializationException(string message) : base(message) { }
    public JSONDeserializationException(string message, Exception inner) : base(message, inner) { }
    protected JSONDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}