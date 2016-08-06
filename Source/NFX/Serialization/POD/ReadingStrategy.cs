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

namespace NFX.Serialization.POD
{
    /// <summary>
    /// Represents logic that resolves POD data types and field layouts to CLR types/layouts.
    /// Extend this type and pass its instance into PortableObjectDocument.ToOriginalObject(ReadingStrategy)
    /// </summary>
    public class ReadingStrategy
    {
        #region .ctor/static

            private static ReadingStrategy s_DefaultInstance = new ReadingStrategy();

            /// <summary>
            /// Returns an instance of default strategy
            /// </summary>
            public static ReadingStrategy Default { get {return s_DefaultInstance;}}


            protected ReadingStrategy()
            {

            }
        #endregion



        /// <summary>
        /// Resolves a MetaType instance from a particular document into CLR Type, i.e. an obsolete class named "ABC" may be resolved into
        ///  newer class "ABCX"
        /// </summary>
        public virtual Type ResolveType(MetaType metaType)
        {
            if (metaType.__CLRType!=null) return metaType.__CLRType;

            var result = Type.GetType(metaType.AssemblyQualifiedName, true, false);
            metaType.__CLRType = result;

            return result;
        }

        /// <summary>
        /// Constructs object out of CompositeData. This implementation calls ResolveType then tries to invoke attribute constructor first
        /// then create instance using default ctor
        /// </summary>
        public virtual object ConstructObject(CompositeData data)
        {
            object result = null;

            var clrType =  ResolveType( data.Type );

            //1 check for attribute ---------------
            var attr = (PortableObjectDocumentDeserializationTransform)clrType
                                                                       .GetCustomAttributes(typeof(PortableObjectDocumentDeserializationTransform), false)
                                                                       .FirstOrDefault();

            if (attr!=null)
                result = attr.ConstructObjectInstance(data);

            if (result==null)
            {
                //2 call the dfault .ctor
                if (data is CompositeArrayData)
                    result = MakeNewArrayInstance( (CompositeArrayData)data );
                else
                    result = MakeNewObjectInstanceUsingDefaultCtor( clrType );
            }

            //3 call OnDeserializing
            if (result!=null)
            {
                List<MethodInfo> methodsOnDeserializing = SerializationUtils.FindSerializationAttributedMethods( result.GetType(), typeof(OnDeserializingAttribute));
                if (methodsOnDeserializing!=null)
                    SerializationUtils.InvokeSerializationAttributedMethods(methodsOnDeserializing, result, data.Document.m_StreamingContext);
            }

            return result;
        }



        /// <summary>
        /// Creates an object using its default .ctor. This implementation uses "magic" to create uninit buffer first
        /// </summary>
        public virtual object MakeNewObjectInstanceUsingDefaultCtor(Type clrType)
        {
            return SerializationUtils.MakeNewObjectInstance(clrType);
        }

        /// <summary>
        /// Creates an array instance described by the CompositeArrayData instance
        /// </summary>
        public virtual object MakeNewArrayInstance(CompositeArrayData arrayData)
        {
            var clrElementType = ResolveType( arrayData.Type.ArrayElementType );
            return Array.CreateInstance(clrElementType, arrayData.ArrayDims);
        }


        /// <summary>
        /// Resolves composite data into CLR object
        /// </summary>
        public virtual object CompositeToNative(CompositeData data)
        {
            if (data==null) return null;
            if (data.ExistingReference)
               return CompositeToNative(data.Referenced);

            var result = data.__CLRObject;

            if (result==null)
            {
                //1. Construct
                result = ConstructObject(data);
                data.__CLRObject = result;

                //2. Fill with data
                if (data is CompositeCustomData)
                    DeserializeObjectFromCompositeCustomData(result, (CompositeCustomData)data);
                else if (data is CompositeArrayData)
                    DeserializeArray((Array)result, (CompositeArrayData)data);
                else
                    DeserializeObjectFromCompositeReflectedData(result, (CompositeReflectedData)data);
            }

            return result;
        }


        public virtual void DeserializeObjectFromCompositeCustomData(object instance, CompositeCustomData data)
        {
            var t = instance.GetType();
            //1 check for attribute ---------------
            var attr = (PortableObjectDocumentDeserializationTransform)t
                                                                       .GetCustomAttributes(typeof(PortableObjectDocumentDeserializationTransform), false)
                                                                       .FirstOrDefault();

            if (attr!=null)
               if (attr.DeserializeFromCompositeCustomData(instance, data)) return;

            if (instance is ISerializable)
            {
                var scontext = new StreamingContext();
                var iser = (ISerializable)instance;

                //rebuild ISerializationInfo object
                var info = DeserializeSerializationInfo(t, data, scontext);

                //Get ctor and invoke
                var ctor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                             null,
                                             new Type[] { typeof(SerializationInfo), typeof(StreamingContext)},
                                             null);
                 if (ctor==null)
                  throw new PODDeserializationException(StringConsts.POD_ISERIALIZABLE_MISSING_CTOR_ERROR + t.FullName);
                 ctor.Invoke(iser, new object[]{ info, scontext} );
            }
            else
                throw new PODDeserializationException(StringConsts.POD_DONT_KNOW_HOWTO_DESERIALIZE_FROM_CUSTOM_DATA.Args( t.FullName ));
        }

        public virtual void DeserializeObjectFromCompositeReflectedData(object instance, CompositeReflectedData data)
        {
            var t = instance.GetType();
            //1 check for attribute ---------------
            var attr = (PortableObjectDocumentDeserializationTransform)t
                                                                       .GetCustomAttributes(typeof(PortableObjectDocumentDeserializationTransform), false)
                                                                       .FirstOrDefault();
            HashSet<MetaComplexType.MetaField> alreadyHandled = null;
            if (attr!=null)
               alreadyHandled = attr.DeserializeFromCompositeReflectedData(instance, data);

            //2 read data
            var mfields = data.Type;
            foreach(var mfld in mfields)
            {
                if (alreadyHandled!=null && alreadyHandled.Contains(mfld)) continue;

                //find field
                if (mfld.m_FieldInfo==null)
                {
                    //1. try attr
                    if (attr!=null) mfld.m_FieldInfo = attr.ResolveField(t, mfld);

                    //2. try built in method
                    if (mfld.m_FieldInfo==null) mfld.m_FieldInfo = ResolveField(t, mfld);

                    //3. skip this field altogether if it can not be read
                    if (mfld.m_FieldInfo==null) continue;
                }

                var handled = false;
                if (attr!=null)
                    handled = attr.SetFieldValue(this, instance, mfld.m_FieldInfo, data, mfld);
                if (!handled)
                    SetFieldData(instance, mfld.m_FieldInfo, data, mfld);
            }
        }


        public virtual void DeserializeArray(Array array, CompositeArrayData data)
        {
            var idx = 0;
            SerializationUtils.WalkArrayRead(array, () => data.ArrayData[idx++] );
        }


        /// <summary>
        /// Performs the assignment of portable data into native field
        /// </summary>
        public virtual void SetFieldData(object instance, FieldInfo fieldInfo, CompositeReflectedData data, MetaComplexType.MetaField mfield)
        {
            var nativeData = data.Document.PortableDataToNativeData(this, data.m_FieldData[mfield.m_Index]);
            fieldInfo.SetValue(instance, nativeData);
        }

        /// <summary>
        /// Resolves a meta field definition into actual native field. Returns null wen resolution is not possible and field should be skipped
        /// </summary>
        public virtual FieldInfo ResolveField(Type nativeType, MetaComplexType.MetaField mfield)
        {
           var fname = mfield.FieldName;
           var fi = nativeType.GetField(fname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
           if (fi.IsNotSerialized) return null;
           return fi;
        }


        /// <summary>
        /// Performs deserialization of SerializationInfo from CompositeCustomData bag
        /// </summary>
        protected virtual SerializationInfo DeserializeSerializationInfo(Type objType, CompositeCustomData data, StreamingContext context)
        {
            var info = new SerializationInfo(objType, new FormatterConverter());

            foreach(var pair in data.CustomData)
            {
                var name = pair.Key;
                var type = ResolveType(  data.Document.GetMetaTypeFromIndex( pair.Value.TypeIndex )  );
                var obj = data.Document.PortableDataToNativeData(this, pair.Value.Data);

                info.AddValue(name, obj, type);
            }

            return info;
        }


    }
}
