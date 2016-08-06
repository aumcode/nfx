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
    /// Provides information about data types stored in Portable Object Document
    /// </summary>
    [Serializable]
    public abstract class MetaType
    {
        #region Static

            /// <summary>
            /// Obtains new or existing instance of MetaType that represents a Type in the document instance
            /// </summary>
            internal static int GetExistingOrNewMetaTypeIndex(PortableObjectDocument document, Type type)
            {
                var dict = document.m_TypesDict;
                if (dict==null)
                {
                    dict = new Dictionary<Type, int>();
                    document.m_TypesDict = dict;
                }

                int result;
                if (dict.TryGetValue(type, out result)) return result;

                MetaType mtp;
                if (type.IsPrimitive || type==typeof(string)|| type==typeof(DateTime)|| type==typeof(TimeSpan))
                    mtp = new MetaPrimitiveType(document, type);
                else
                    mtp = new MetaComplexType(document, type);

                //1. add to dict so no infinite loop happens during recursive call to this func
                result = document.m_Types.Count;
                document.m_Types.Add(mtp);
                dict.Add(type, result);

                //2. Build the type inner structures
                mtp.Build();

                return result;
            }

        #endregion

        #region .ctor
            internal MetaType() {}

            internal MetaType(PortableObjectDocument document, Type type)
            {
                __CLRType = type;
                m_Document = document;
                m_Name = type.Name;
                m_Namespace = type.Namespace;
                m_AssemblyQualifiedName = type.AssemblyQualifiedName;
            }
        #endregion

        #region Fields
            internal PortableObjectDocument m_Document;

            [NonSerialized]
            internal Type __CLRType; //this is set during conversion POD->CLR, this is a transient internal value

            private string m_Name;
            private string m_Namespace;
            private string m_AssemblyQualifiedName;
        #endregion

        #region Properties

            /// <summary>
            /// Returns document instance that contains this type
            /// </summary>
            public PortableObjectDocument Document { get { return m_Document;} }


            /// <summary>
            /// Returns Type.Name
            /// </summary>
            public string Name { get { return m_Name; }}

            /// <summary>
            /// Returns Type.Namespace
            /// </summary>
            public string Namespace { get { return m_Namespace; }}

            /// <summary>
            /// Returns Type.AssemblyQualifiedName
            /// </summary>
            public string AssemblyQualifiedName { get { return m_AssemblyQualifiedName; }}
       #endregion

       #region Protected

            protected virtual void Build()
            {

            }


       #endregion

    }

    /// <summary>
    /// Represents primitive built-in types in the framework that are stored in Portable Object Document
    /// </summary>
    [Serializable]
    public sealed class MetaPrimitiveType : MetaType
    {
        internal MetaPrimitiveType() {}

        internal MetaPrimitiveType(PortableObjectDocument document, Type type) : base(document, type)
        {
        }
    }


    /// <summary>
    /// Represents information about the composite type of data that is stored in Portable Object Document
    /// </summary>
    [Serializable]
    public sealed class MetaComplexType : MetaType, IEnumerable<MetaComplexType.MetaField>, IDeserializationCallback
    {
        #region Inner Classes

            /// <summary>
            /// Represents an information about a field of same type
            /// </summary>
            public class MetaField
            {
                internal MetaField() {}

                internal MetaField(MetaComplexType declaringType, FieldInfo fieldInfo)
                {
                    m_DeclaringType = declaringType;
                    m_FieldInfo = fieldInfo;
                    m_FieldName = fieldInfo.Name;
                    m_FieldMetaTypeIndex = MetaType.GetExistingOrNewMetaTypeIndex(declaringType.Document, fieldInfo.FieldType);
                }

                [NonSerialized]
                internal MetaComplexType m_DeclaringType;

                [NonSerialized]
                internal int m_Index;

                [NonSerialized]
                internal FieldInfo m_FieldInfo;

                private string   m_FieldName;
                private int m_FieldMetaTypeIndex;

                public MetaComplexType DeclaringType { get { return m_DeclaringType;} }
                public MetaType FieldType { get { return m_DeclaringType.Document.m_Types[m_FieldMetaTypeIndex]; } }

                public int               Index         { get { return m_Index;} }
                public string            FieldName     { get { return m_FieldName;} }
                public int               FieldMetaTypeIndex     { get { return m_FieldMetaTypeIndex;} }
            }

        #endregion


        #region .ctor

            internal MetaComplexType() {}

            internal MetaComplexType(PortableObjectDocument document,  Type type) : base(document, type)
            {

            }

            protected override void Build()
            {
                var type = this.__CLRType;

                m_BuildInformation = new BuildInformation(type.Assembly, throwError: false);
                m_IsValueType = type.IsValueType;
                m_IsVisible = type.IsVisible;
                m_IsArray = type.IsArray;
                m_IsAbstract = type.IsAbstract;

                if (m_IsArray)
                {
                    m_ArrayRank = type.GetArrayRank();
                    m_ArrayElementTypeIndex = MetaType.GetExistingOrNewMetaTypeIndex(m_Document, type.GetElementType());
                }

                m_Fields = new List<MetaField>();

                var fields = SerializationUtils.GetSerializableFields(type);
                foreach(var fld in fields)
                {
                  var mf = new MetaField(this, fld);
                  m_Fields.Add( mf );
                  mf.m_Index = m_Fields.Count-1;
                }

                m_MethodsOnSerializing = SerializationUtils.FindSerializationAttributedMethods(type, typeof(OnSerializingAttribute));
                m_MethodsOnSerialized  = SerializationUtils.FindSerializationAttributedMethods(type, typeof(OnSerializedAttribute));
            }

        #endregion


        #region Fields
            private BuildInformation m_BuildInformation;
            internal List<MetaField> m_Fields;

            private bool m_IsValueType;
            private bool m_IsVisible;
            private bool m_IsArray;
            private bool m_IsAbstract;

            private int m_ArrayRank;
            private int m_ArrayElementTypeIndex;

            [NonSerialized]
            internal List<MethodInfo> m_MethodsOnSerializing;
            [NonSerialized]
            internal List<MethodInfo> m_MethodsOnSerialized;
        #endregion

        #region Properties

            /// <summary>
            /// Returns assembly build information
            /// </summary>
            public BuildInformation BuildInfo { get { return m_BuildInformation;} }

            public bool IsValueType { get {return m_IsValueType;}}
            public bool IsVisible   { get {return m_IsVisible;}}
            public bool IsArray     { get {return m_IsArray;}}
            public bool IsAbstract  { get {return m_IsAbstract;}}

            /// <summary>
            /// Returns number of dimensions for arrays
            /// </summary>
            public int ArrayRank  { get {return m_ArrayRank;}}

            /// <summary>
            /// Returns index of the meta type of array element
            /// </summary>
            public int ArrayElementTypeIndex  { get {return m_ArrayElementTypeIndex;}}

            /// <summary>
            /// Returns MetaType of array element
            /// </summary>
            public MetaType ArrayElementType  { get {return m_Document.GetMetaTypeFromIndex( m_ArrayElementTypeIndex);}}

            /// <summary>
            /// Returns the serializable field count described by this type
            /// </summary>
            public int FieldCount { get { return m_Fields.Count; } }


        #endregion


        #region Public

            public void OnDeserialization(object sender)
            {
                for(var i=0; i<m_Fields.Count; i++)
                {
                    var f = m_Fields[i];
                    f.m_Index = i;
                    f.m_DeclaringType = this;
                }
            }

            public IEnumerator<MetaComplexType.MetaField> GetEnumerator()
            {
               return m_Fields.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
               return m_Fields.GetEnumerator();
            }

        #endregion


    }
}
