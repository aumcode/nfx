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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace NFX.Serialization
{

    /// <summary>
    /// Denotes ser/deser operations
    /// </summary>
    public enum SerializationOperation
    {
      /// <summary>
      /// Serializing object to stream
      /// </summary>
      Serializing,

      /// <summary>
      /// Deserializing object from stream
      /// </summary>
      Deserializing
    };


    /// <summary>
    /// Describes an entity that can serialize and deserialize objects
    /// </summary>
    public interface ISerializer
    {
       void Serialize(Stream stream, object root);
       object Deserialize(Stream stream);

       /// <summary>
       /// Indicates whether Serialize/Deserialize may be called by multiple threads at the same time
       /// </summary>
       bool IsThreadSafe{get;}
    }


    /// <summary>
    /// Describes an entity that can serialize and deserialize objects and can be disposed
    /// </summary>
    public interface IDisposableSerializer : ISerializer, IDisposable
    {

    }




}
