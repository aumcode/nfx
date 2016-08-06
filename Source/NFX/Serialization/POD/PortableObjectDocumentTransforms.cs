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
using System.Text;

namespace NFX.Serialization.POD
{
    /// <summary>
    /// Represents an entity that knows how to transform/apply CLR data types to PortableObjectDocument
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple=false, Inherited=false)]
    public abstract class PortableObjectDocumentSerializationTransform : Attribute
    {
        /// <summary>
        /// Override to provide custome seriallization of type marked with this attribute into custom name/value bag
        /// </summary>
        /// <param name="document">Document context that this operation executes under</param>
        /// <param name="source">Source native data to serialize / populate data from</param>
        /// <param name="data">Name/value bag to write data into</param>
        public abstract void SerializeCustomObjectData(PortableObjectDocument document, object source, Dictionary<string, CustomTypedEntry> data);
    }


    /// <summary>
    /// Represents an entity that knows how to transform/apply CLR data types from PortableObjectDocument
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple=false, Inherited=false)]
    public abstract class PortableObjectDocumentDeserializationTransform : Attribute
    {
        /// <summary>
        /// Override to construct new object instance from CompositeData.
        /// Return null to let the default implementatiom/ctor be invoked by the framework
        /// </summary>
        public abstract object ConstructObjectInstance(CompositeData data);

        /// <summary>
        /// Handles the deserialization of the object instance from CompoisteCustomData bag. Returns true to indicate that
        ///  deserialization was handled completely here and default framework implementation should not be called
        /// </summary>
        public abstract bool DeserializeFromCompositeCustomData(object instance, CompositeCustomData data);


        /// <summary>
        /// Handles the deserialization of the object instance from CompoisteReflectedData. Returns a set of fields that were handles by this implementation
        ///  so that framework code can skip them. Return null or empty set when method is not implemented
        /// </summary>
        public abstract HashSet<MetaComplexType.MetaField> DeserializeFromCompositeReflectedData(object instance, CompositeReflectedData data);

        /// <summary>
        /// Handles the assignemnt from ReflectedData into CLR fieldInfo. Override to make conversions, i.e. string to bool, int to string etc...
        /// Return true to indicate that default framework implementation should not be called
        /// </summary>
        public abstract bool SetFieldValue(ReadingStrategy readingStrategy, object instance, FieldInfo fieldInfo,  CompositeReflectedData data, MetaComplexType.MetaField mfield);

        /// <summary>
        /// Resolves a meta field definition into actual native field. Returns null wen resolution is not possible and field should be skipped
        /// </summary>
        public abstract FieldInfo ResolveField(Type nativeType, MetaComplexType.MetaField mfield);
    }
}
