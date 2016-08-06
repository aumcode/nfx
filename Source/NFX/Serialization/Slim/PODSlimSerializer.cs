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

using NFX.Serialization.POD;

namespace NFX.Serialization.Slim
{
    /// <summary>
    /// Serializes CLR object graphs using PortableObjectDocument container and Slim binary serialization algorithm.
    /// This class is far less performant than SlimSerializer, however it serializes types using document model that
    ///  allows to transform/change data during serialization/deserialization.
    /// This class is needed for upgrades, when object metadata may change but need to be read (maybe partially) back into the new type structure
    /// </summary>
    public class PODSlimSerializer : ISlimSerializer
    {
        #region .ctor

            public PODSlimSerializer()
            {
                m_Serializer = new SlimSerializer( TypeRegistry.PODTypes );
            }
        #endregion

        #region Fields

            private SlimSerializer m_Serializer;

        #endregion

        #region Properties

           /// <summary>
           /// This serializer always uses "PerCall" setting. Setting this property has no effect
           /// </summary>
           TypeRegistryMode ISlimSerializer.TypeMode {get{ return TypeRegistryMode.PerCall;} set {}}

           /// <summary>
           /// This serializer is thread-safe
           /// </summary>
           bool ISerializer.IsThreadSafe { get{return true;}}

           /// <summary>
           /// Not supported here
           /// </summary>
           bool ISlimSerializer.BatchTypesAdded{ get{ return false;}}

        #endregion

        #region Public

            /// <summary>
            /// This method does nothig in this class
            /// </summary>
            void ISlimSerializer.ResetCallBatch()
            {

            }

            /// <summary>
            /// Serializes a graph of arbitrary CLR objects into stream using PortableObjectDocument container
            /// </summary>
            /// <param name="stream">Target stream</param>
            /// <param name="root">CLR object graph</param>
            public void Serialize(Stream stream, object root)
            {
              Serialize(stream, root, null, null);
            }


            /// <summary>
            /// Serializes a graph of arbitrary CLR objects into stream using PortableObjectDocument container,
            ///  optionally taking document creation attributes
            /// </summary>
            /// <param name="stream">Target stream</param>
            /// <param name="root">CLR object graph</param>
            /// <param name="documentCreationDate">Optional document creation attribute</param>
            /// <param name="documentNotes">Optional document creation attribute</param>
            public void Serialize(Stream stream, object root, DateTime? documentCreationDate = null, string documentNotes = null)
            {
              var document = new PortableObjectDocument(root, documentCreationDate, documentNotes);
              m_Serializer.Serialize(stream, document);
            }

            /// <summary>
            /// Desirializes a graph of arbitrary CLR objects that was serialized before
            /// </summary>
            /// <param name="stream">Source data stream in Slim binary format</param>
            /// <param name="readingStrategy">Optional reading strategy</param>
            /// <returns>CLR object graph which is deserialized from possibly transformed PortableObjectDocument container</returns>
            public object Deserialize(Stream stream, ReadingStrategy readingStrategy = null)
            {
               var document = DeserializeDocument(stream);
               return document.ToOriginalObject(readingStrategy);
            }

            /// <summary>
            /// Desirializes a graph of arbitrary CLR objects that was serialized before
            /// </summary>
            /// <param name="stream">Source data stream in Slim binary format</param>
            /// <returns>CLR object graph which is deserialized from possibly transformed PortableObjectDocument container</returns>
            public object Deserialize(Stream stream)
            {
               return this.Deserialize(stream, null);
            }


            /// <summary>
            /// Deserializes a PortableObjectDocument container instance
            /// </summary>
            /// <param name="stream">Source data stream in Slim binary format</param>
            /// <returns>PortableObjectDocument instance</returns>
            public PortableObjectDocument DeserializeDocument(Stream stream)
            {
                return (PortableObjectDocument)m_Serializer.Deserialize(stream);
            }

        #endregion
    }
}
