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
using System.Runtime.Serialization;
using System.Text;

namespace NFX.Serialization.POD
{

    /// <summary>
    /// Used in custom data bags to specify the type of the object contained in Data property if it is intended to be re-interpreted
    /// </summary>
    public struct CustomTypedEntry
    {
        public int TypeIndex;
        public object Data;
    }


    /// <summary>
    /// Represents a composite (non primitive) data stored in Portable Object Document.
    /// This data is obtained from native types using ISerializable interface or running PortableObjectDocumentTransform attribute.
    /// This class is NOT used for native types that perform default reflection-based serialization,
    ///  for that CompositeReflectedData is used
    /// </summary>
    [Serializable]
    public sealed class CompositeCustomData : CompositeData
    {
       #region .ctor

            internal CompositeCustomData(PortableObjectDocument document, ISerializable data, int metaTypeIndex = -1)
                      : base(document, data, metaTypeIndex)
            {
                if (!ExistingReference)
                    serializeFromISerializable(data);
            }

            internal CompositeCustomData(PortableObjectDocument document, object data, int metaTypeIndex = -1)
                      : base(document, data, metaTypeIndex)
            {
                if (!ExistingReference)
                    serializeFromTransform(data);
            }

        #endregion


        #region Fields

            private Dictionary<string, CustomTypedEntry> m_CustomData;


        #endregion

        #region Properties
            /// <summary>
            /// Returns custom data that this instance contains, or null if this instance is a reference to another object
            /// </summary>
            public Dictionary<string, CustomTypedEntry> CustomData { get { return m_CustomData;} }


        #endregion


        #region .pvt

                private void serializeFromISerializable(ISerializable data)
                {
                    m_CustomData = new Dictionary<string,CustomTypedEntry>();

                    var info = new SerializationInfo(data.GetType(), new FormatterConverter());
                    StreamingContext streamingContext = new StreamingContext(StreamingContextStates.Persistence);
                    data.GetObjectData(info, streamingContext);

                    var senum = info.GetEnumerator();
                    while(senum.MoveNext())
                    {
                        var value = new CustomTypedEntry();
                        value.TypeIndex = MetaType.GetExistingOrNewMetaTypeIndex( m_Document, senum.ObjectType );
                        value.Data = m_Document.NativeDataToPortableData( senum.Value );
                        m_CustomData[senum.Name] = value;
                    }

                }

                private void serializeFromTransform(object data)
                {
                    m_CustomData = new Dictionary<string,CustomTypedEntry>();

                    var t = data.GetType();

                    var attr = (PortableObjectDocumentSerializationTransform)t.GetCustomAttributes(typeof(PortableObjectDocumentSerializationTransform), false).First();

                    attr.SerializeCustomObjectData(m_Document, data, m_CustomData);
                }

        #endregion
    }
}
