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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NFX.Environment;

namespace NFX.Serialization.POD
{

    /// <summary>
    /// Represents an graph of arbitrary CLR objects as a graph of well known POD-objects which are based on primitive-only types.
    /// This class acts as a Document-Object-Model container that hosts comlex CLR types that may change their structure
    ///  but need to be deserialized even if deserialization is partial / requires transform
    /// </summary>
    [Serializable]
    public sealed class PortableObjectDocument
    {
        #region .ctor

            //20140717 DKh commented. Why was this .ctor EVER needed?
            //it started failing after deserializer started to call not only pub ctor but also private
            //as needed to init field assignments
            ////////internal PortableObjectDocument()
            ////////{
            ////////    MetaType.GetExistingOrNewMetaTypeIndex(this, typeof(object));
            ////////}


            /// <summary>
            /// Creates a new instance of object document from the graph of arbitrary CLR objects serializing them into well-known POD object types
            /// </summary>
            public PortableObjectDocument(object graph, DateTime? creationDate = null, string notes = null)
            {
                try
                {
                    m_BuildInfo = BuildInformation.ForFramework;
                    m_CreationDate = creationDate ?? App.LocalizedTime;
                    m_Notes = notes;
                    m_Types = new List<MetaType>();
                    m_CompositeData = new List<CompositeData>();
                    m_StreamingContext = new StreamingContext();

                    MetaType.GetExistingOrNewMetaTypeIndex(this, typeof(object));

                    if (graph!=null)
                        m_Root = nativeDataToPortableData( graph, out m_RootMetaTypeIndex );
                }
                finally
                {
                    purgeCaches();//drop all temp objects
                }
            }
        #endregion

        #region Fields

            private BuildInformation m_BuildInfo;
            private DateTime m_CreationDate;
            private string m_Notes;

            internal List<MetaType> m_Types;
            internal List<CompositeData> m_CompositeData;


            private object m_Root;
            private int m_RootMetaTypeIndex;

            [NonSerialized]
            internal Dictionary<Type, int> m_TypesDict;

            [NonSerialized]
            internal Dictionary<object, int> m_CompositeDataDict;

            [NonSerialized]
            internal StreamingContext m_StreamingContext;

        #endregion

        #region Properties

            /// <summary>
            /// Returns build information for framework that contains the PortableObjectDocuemnt type
            /// </summary>
            public BuildInformation BuildInformation { get { return m_BuildInfo; } }

            /// <summary>
            /// Returns timestamp when this doc was created
            /// </summary>
            public DateTime CreationDate { get { return m_CreationDate; } }

            /// <summary>
            /// Returns notes supplied when document was created
            /// </summary>
            public string Notes { get { return m_Notes ?? string.Empty; } }

            /// <summary>
            /// Returns meta types that this document contains
            /// </summary>
            public IEnumerable<MetaType> MetaTypes { get { return m_Types;}}

            /// <summary>
            /// Returns graph root potable object that this document represents, not a native object
            /// </summary>
            public object Root { get { return m_Root;}}

            /// <summary>
            /// Returns graph root object meta type index, if root data is null then 0 is returned which is an index of MetaCompositeType(Object) type
            /// </summary>
            public int RootMetaTypeIndex { get { return m_RootMetaTypeIndex ;}}

            /// <summary>
            /// Returns graph root object meta type index, if root is null them MetaCompositeType(Object) is returned
            /// </summary>
            public MetaType RootMetaType { get { return m_Types[ m_RootMetaTypeIndex ];}}

        #endregion

        #region Public

            /// <summary>
            /// Deserializes PortableObjectDocument into original graph of arbitrary CLR objects trying to preserve/convert as much data as possible
            ///  using optionally supplied strategy
            /// </summary>
            public object ToOriginalObject(ReadingStrategy strategy = null)
            {
                if (strategy==null) strategy = ReadingStrategy.Default;

                if (m_Root==null) return null;

                m_StreamingContext = new StreamingContext();

                try
                {
                    var result = PortableDataToNativeData(strategy, m_Root);


                    if (m_CompositeData!=null)
                    {
                        foreach(var cd in m_CompositeData)
                        {
                            var obj = cd.__CLRObject;
                            if (obj==null) continue;

                            //invoke all IDeserializationCallback-implementors
                            if (obj is IDeserializationCallback)
                                ((IDeserializationCallback)obj).OnDeserialization(this);

                            //invoke all OnDeserialized
                            List<MethodInfo> methodsOnDeserialized = SerializationUtils.FindSerializationAttributedMethods( obj.GetType(), typeof(OnDeserializedAttribute));
                            if (methodsOnDeserialized!=null)
                                 SerializationUtils.InvokeSerializationAttributedMethods(methodsOnDeserialized, obj, m_StreamingContext);
                        }
                    }


                    return result;
                }
                finally
                {
                    purgeCaches();//drop all temp objects
                }
            }

            /// <summary>
            /// Obtains new or existing index of MetaType that represents a Type in this document instance.
            /// If this document instance already has this type registered, then existing index is returned, otherwise
            ///  the new MetaType instance that represents the supplied CLR Type is created and registered under the document-unique index
            /// </summary>
            public int GetExistingOrNewMetaTypeIndex(Type type)
            {
                return MetaType.GetExistingOrNewMetaTypeIndex(this, type);
            }

            /// <summary>
            /// Obtains new or existing MetaType instance that represents a Type in this document instance.
            /// If this document instance already has this type registered, then existing MetaType instance is returned, otherwise
            ///  the new MetaType instance that represents the supplied CLR Type is created and registered under the document-unique index
            /// </summary>
            public MetaType GetExistingOrNewMetaType(Type type)
            {
                return m_Types[ MetaType.GetExistingOrNewMetaTypeIndex(this, type) ];
            }


            /// <summary>
            /// Returns MetaType by index
            /// </summary>
            public MetaType GetMetaTypeFromIndex(int index)
            {
                return m_Types[ index ];
            }


            /// <summary>
            /// Transforms a native value, such as object, primitive, struct etc.. into a value that can be stored in the PortableObjectDocument.
            /// The complex types are stored as CompositeData, primitives are stored as-is (boxed)
            /// </summary>
            public object NativeDataToPortableData(object data)
            {
                int mtpi;
                return nativeDataToPortableData(data, out mtpi);
            }

            /// <summary>
            /// Transforms a portable data value, such as object, primitive, struct etc.. into a CLR
            /// </summary>
            public object PortableDataToNativeData(ReadingStrategy strategy, object data)
            {
                if (data==null) return null;
                if (! (data is CompositeData)) return data;
                var cd = (CompositeData)data;
                return strategy.CompositeToNative(cd);
            }

        #endregion


        #region .pvt

            private void purgeCaches()
            {
                 if (m_Types!=null)
                    foreach(var tp in m_Types)
                    {
                        tp.__CLRType = null;
                        if (tp is MetaComplexType)
                            foreach(var f in ((MetaComplexType)tp).m_Fields)
                                f.m_FieldInfo = null;
                    }

                 if (m_CompositeData!=null)
                    foreach(var cd in m_CompositeData)
                        cd.__CLRObject = null;
            }



            private object nativeDataToPortableData(object data, out int metaTypeIndex)
            {
                metaTypeIndex = 0;
                if (data==null) return null;// null is kept as is

                var t = data.GetType();
                metaTypeIndex = GetExistingOrNewMetaTypeIndex(t);
                var mtp =  m_Types[ metaTypeIndex ];

                if (mtp is MetaPrimitiveType)
                     return data;//as-is (box)
                else
                {
                   var mct = (MetaComplexType)mtp;

                   if (mct.m_MethodsOnSerializing!=null)
                    SerializationUtils.InvokeSerializationAttributedMethods(mct.m_MethodsOnSerializing, data, m_StreamingContext);

                   try
                   {
                       if (data is ISerializable)
                       {
                           return new CompositeCustomData(this, (ISerializable)data, metaTypeIndex);
                       }
                       else if (data is Array)
                       {
                           return new CompositeArrayData(this, (Array)data, metaTypeIndex);
                       }
                       else if (Attribute.IsDefined( t, typeof(PortableObjectDocumentSerializationTransform), false))
                       {
                           return new CompositeCustomData(this, data, metaTypeIndex);
                       }
                       return new CompositeReflectedData(this, data, metaTypeIndex);
                   }
                   finally
                   {
                       if (mct.m_MethodsOnSerialized!=null)
                        SerializationUtils.InvokeSerializationAttributedMethods(mct.m_MethodsOnSerialized, data, m_StreamingContext);
                   }
                }
            }


        #endregion



    }
}
