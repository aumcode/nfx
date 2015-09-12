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


using NFX.Serialization.JSON;

using NFX.RecordModel;

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Describes a schema for rows: TypedRows and DynamicRows.
    /// DynamicRows are "shaped" in memory from schema, whereas, TypedRows define schema.
    /// Schema for Typedrows is cached in static dictionary for speed
    /// </summary>
    [Serializable]
    public sealed class Schema : INamed, IEnumerable<Schema.FieldDef>, IJSONWritable
    {
        #region Inner Classes
            
            /// <summary>
            /// Provides a definition for a single field of a row
            /// </summary>
            [Serializable]
            public sealed class FieldDef : INamed, IOrdered, ISerializable, IJSONWritable
            {
                public FieldDef(string name, Type type, IEnumerable<FieldAttribute> attrs) 
                {
                    ctor(name, 0, type, attrs, null);
                }

                public FieldDef(string name, Type type, QuerySource.ColumnDef columnDef) 
                {
                    FieldAttribute attr;
                    if (columnDef!=null)
                        attr =  new FieldAttribute(targetName: TargetedAttribute.ANY_TARGET,
                                                  storeFlag: columnDef.StoreFlag,
                                                  required: columnDef.Required,
                                                  visible: columnDef.Visible,
                                                  key: columnDef.Key,
                                                  backendName: columnDef.BackendName,
                                                  description: columnDef.Description);
                    else
                       attr =  new FieldAttribute(targetName: TargetedAttribute.ANY_TARGET);
                    
                    var attrs = new FieldAttribute[1] { attr };
                    ctor(name, 0, type, attrs, null);
                }
                
                internal FieldDef(string name, int order, Type type, IEnumerable<FieldAttribute> attrs, PropertyInfo memberInfo = null)
                {
                    ctor(name, order, type, attrs, memberInfo);
                }

                private void ctor(string name, int order, Type type, IEnumerable<FieldAttribute> attrs, PropertyInfo memberInfo = null)
                {
                    if (name.IsNullOrWhiteSpace() || type==null || attrs==null)
                        throw new CRUDException(StringConsts.ARGUMENT_ERROR + "FieldDef.ctor(..null..)");
                   
                    m_Name = name;
                    m_Order = order;
                    m_Type = type;
                    m_Attrs = new List<FieldAttribute>(attrs);

                    if (m_Attrs.Count<1)
                     throw new CRUDException(StringConsts.CRUD_FIELDDEF_ATTR_MISSING_ERROR.Args(name));

                    //add ANY_TARGET attribute
                    if (!m_Attrs.Any(a => a.TargetName == TargetedAttribute.ANY_TARGET))
                      m_Attrs.Add( new FieldAttribute(FieldAttribute.ANY_TARGET));

                    m_MemberInfo = memberInfo;

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        m_NonNullableType = type.GetGenericArguments()[0];
                    else
                        m_NonNullableType = type;

                    m_AnyTargetKey = this[null].Key;
                }
                

                internal FieldDef(SerializationInfo info, StreamingContext context)
                {
                    m_Name = info.GetString("nm");
                    m_Order = info.GetInt32("o");
                    m_Type = Type.GetType( info.GetString("t"), true);
                    m_NonNullableType = Type.GetType( info.GetString("nnt"), true);
                    m_Attrs = info.GetValue("attrs", typeof(List<FieldAttribute>)) as List<FieldAttribute>;
                    m_AnyTargetKey = info.GetBoolean("atk");

                    var mtp = info.GetString("mtp");
                    if (mtp!=null)
                    {
                        var tp = Type.GetType( mtp, true);
                    
                        var mn = info.GetString("mn");
                        if (mn!=null)
                        {
                            m_MemberInfo = tp.GetProperty(mn);
                        }
                    }

                }

                public void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.AddValue("nm", m_Name);
                    info.AddValue("o", m_Order);
                    info.AddValue("t", m_Type.AssemblyQualifiedName);
                    info.AddValue("nnt", m_NonNullableType.AssemblyQualifiedName);
                    info.AddValue("attrs", m_Attrs);
                    info.AddValue("atk", m_AnyTargetKey);

                    if (m_MemberInfo==null)
                    {
                       info.AddValue("mtp", null);
                       info.AddValue("mn", null);
                    }
                    else
                    {
                       info.AddValue("mtp", m_MemberInfo.DeclaringType.AssemblyQualifiedName);
                       info.AddValue("mn", m_MemberInfo.Name);
                    }


                }


                private string m_Name;
                internal int m_Order;
                private Type m_Type;
                private Type m_NonNullableType;
                private List<FieldAttribute> m_Attrs;
                private PropertyInfo m_MemberInfo;
                private bool m_AnyTargetKey;

                /// <summary>
                /// Returns the name of the field
                /// </summary>
                public string                        Name  { get { return m_Name;}}
                
                /// <summary>
                /// Returns the field type
                /// </summary>
                public Type                          Type  { get { return m_Type;}}

                /// <summary>
                /// For nullable value types returns the field type regardless of nullability, it is the type argument of Nullable struct;
                /// For reference types returns the same type as Type property
                /// </summary>
                public Type                          NonNullableType  { get { return m_NonNullableType;}}
                
                /// <summary>
                /// Returns field attributes
                /// </summary>
                public IEnumerable<FieldAttribute>   Attrs { get { return m_Attrs;}}

                /// <summary>
                /// Gets absolute field order index in a row
                /// </summary>
                public int                           Order { get {return m_Order;} }

                /// <summary>
                /// For TypedRow-descendants returns a PropertyInfo object for the underlying property
                /// </summary>
                public PropertyInfo                  MemberInfo { get {return m_MemberInfo;} }

                /// <summary>
                /// Returns true when this field is attributed as being a key field in an attribute that targets ANY_TARGET
                /// </summary>
                public bool                          AnyTargetKey { get {return m_AnyTargetKey;} }


                /// <summary>
                /// Returns description from field attribute or parses it from field name
                /// </summary>
                public string Description
                {
                  get
                  {
                    var attr = this[null];
                    var result = attr!=null ? attr.Description : "";

                    if (result.IsNullOrWhiteSpace())
                     result = NFX.Parsing.Utils.ParseFieldNameToDescription(Name, true);

                    return result;
                  }
                }
                
                
                /// <summary>
                /// Returns true when at least one attribute was marked as NonUI - meaning that this field must not be serialized-to/deserialized-from client UI
                /// </summary>
                public bool  NonUI
                {
                  get { return m_Attrs.Any(a=>a.NonUI);}
                }

                
                       private Dictionary<string, FieldAttribute> m_TargetAttrsCache = new Dictionary<string, FieldAttribute>();
                
                /// <summary>
                /// Returns a FieldAttribute that matches the supplied targetName, or if one was not defined then
                ///  returns FieldAttribute which matches any target or null
                /// </summary>
                public FieldAttribute this[string targetName]
                {
                    get
                    {
                      if (targetName==null) targetName = FieldAttribute.ANY_TARGET;

                      FieldAttribute result = null;
                      if (!m_TargetAttrsCache.TryGetValue(targetName, out result))
                      {  
                        if (targetName.IsNotNullOrWhiteSpace())
                        {
                            result = m_Attrs.FirstOrDefault(a => targetName.EqualsIgnoreCase(a.TargetName));
                        }

                        if (result==null)
                         result = m_Attrs.FirstOrDefault(a => TargetedAttribute.ANY_TARGET.EqualsIgnoreCase(a.TargetName) );

                        var dict = new Dictionary<string, FieldAttribute>(m_TargetAttrsCache); 
                        dict[targetName] = result;
                        m_TargetAttrsCache = dict;//atomic
                      }

                      return result;
                    }
                }

                /// <summary>
                /// Returns the name of the field in backend that was possibly overriden for a particular target
                /// </summary>
                public string GetBackendNameForTarget(string targetName)
                {
                    var result = m_Name;
                    var fattr = this[targetName];
                    if (fattr!=null && fattr.BackendName.IsNotNullOrWhiteSpace()) result = fattr.BackendName;

                    return result;
                }

                public override string ToString()
                {
                    return "FieldDef(Name: '{0}', Type: '{1}', Order: {2})".Args(m_Name, m_Type.FullName, m_Order);
                }

                public override int GetHashCode()
                {
                    return m_Name.GetHashCodeOrdIgnoreCase() ^ m_Order;
                }

                public override bool Equals(object obj)
                {
                    var other = obj as FieldDef;
                    if (other==null) return false;
                    return
                       this.m_Name.EqualsOrdIgnoreCase(other.m_Name) &&
                       this.m_Order==other.m_Order &&
                       this.m_Type == other.m_Type &&
                       this.m_Attrs.SequenceEqual(other.m_Attrs);
                }


                /// <summary>
                /// Writes fielddef as JSON. Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
                /// </summary>
                public void WriteAsJSON(System.IO.TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
                {
                    var attr = this[null];

                    if (attr.NonUI) return;//nothing to write for NONUI

                    string tp;
                    if (m_NonNullableType.IsPrimitive)
                         tp = m_NonNullableType.Name;
                    else
                         tp = m_NonNullableType.FullName;
                   

                    var map = new Dictionary<string, object>
                    {
                      {"Name",  m_Name},       
                      {"Order", m_Order},       
                      {"Type",  tp},
                      {"Nullable", m_Type!=m_NonNullableType}
                    };

                    if (attr!=null)
                    {
                        map.Add("IsKey", attr.Key);
                        map.Add("IsRequired", attr.Required);
                        map.Add("Visible", attr.Visible);
                        if (attr.Default!=null)map.Add("Default", attr.Default);
                        if (attr.CharCase!=CharCase.AsIs) map.Add("CharCase", attr.CharCase);
                        if (attr.Kind!=DataKind.Text) map.Add("Kind", attr.Kind);
                        if (attr.MinLength!=0) map.Add("MinLen", attr.MinLength);
                        if (attr.MaxLength!=0) map.Add("MaxLen", attr.MaxLength);
                        if (attr.Min!=null) map.Add("Min", attr.Min);
                        if (attr.Max!=null) map.Add("Max", attr.Max);
                        if (attr.ValueList!=null) map.Add("ValueList", attr.ValueList);
                        if (attr.Description!=null)map.Add("Description", attr.Description);
                        //metadata content is in the internal format and not dumped
                    }

                    JSONWriter.WriteMap(wri, map, nestingLevel, options);
                } 

                
            }


        #endregion

        #region CONSTS
            

        #endregion
        
        #region .ctor / static

            /// <summary>
            /// Gets all property members of TypedRow that are tagged as [Field]
            /// </summary>
            public static IEnumerable<PropertyInfo> GetFieldMembers(Type type)
            {
                //20140926 DKh +DeclaredOnly
                var local = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                                .Where(pi => Attribute.IsDefined(pi, typeof(FieldAttribute)));
                if (type.BaseType==typeof(object)) return local;

                var bt = type.BaseType;//null for Object

                if (bt != null)
                    return GetFieldMembers(type.BaseType).Concat(local);
                else
                    return local;
            }
            
            
            private static Registry<Schema> s_TypedRegistry = new Registry<Schema>();

            
            /// <summary>
            /// Returns schema instance for the TypedRow instance by fetching schema object from cache or
            ///  creating it if it has not been cached yet
            /// </summary>
            public static Schema GetForTypedRow(TypedRow row)
            {
                return GetForTypedRow(row.GetType());
            }


            /// <summary>
            /// Returns schema instance for the TypedRow instance by fetching schema object from cache or
            ///  creating it if it has not been cached yet
            /// </summary>
            public static Schema GetForTypedRow(Type trow)
            {
                if (!typeof(TypedRow).IsAssignableFrom(trow))
                    throw new CRUDException(StringConsts.CRUD_TYPE_IS_NOT_DERIVED_FROM_TYPED_ROW_ERROR.Args(trow.FullName));
                
                var name = trow.AssemblyQualifiedName;

                var schema = s_TypedRegistry[name];

                if (schema!=null) return schema;

                lock(s_TypedRegistry)
                {
                    schema = s_TypedRegistry[name];
                    if (schema!=null) return schema;
                    schema = new Schema(trow);
                    return schema;
                }
            }

            private static HashSet<Type> s_TypeLatch = new HashSet<Type>();

            private Schema(Type trow)
            {
                lock(s_TypeLatch)
                {
                  if (s_TypeLatch.Contains(trow))
                   throw new CRUDException(StringConsts.CRUD_TYPED_ROW_RECURSIVE_FIELD_DEFINITION_ERROR.Args(trow.FullName));
                
                  s_TypeLatch.Add(trow);
                  try
                  {
                      m_Name = trow.AssemblyQualifiedName;
                
                
                      var tattrs = trow.GetCustomAttributes(typeof(TableAttribute), false).Cast<TableAttribute>();
                      m_TableAttrs = new List<TableAttribute>( tattrs );


                      m_FieldDefs = new OrderedRegistry<FieldDef>();
                      var props = GetFieldMembers(trow);
                      var order = 0;
                      foreach(var prop in props)
                      {
                          var fattrs = prop.GetCustomAttributes(typeof(FieldAttribute), false).Cast<FieldAttribute>();
                          var fdef = new FieldDef(prop.Name, order, prop.PropertyType, fattrs, prop);
                          m_FieldDefs.Register(fdef);

                          order++;
                      }
                      s_TypedRegistry.Register(this);
                      m_TypedRowType = trow;
                  }
                  finally
                  {
                    s_TypeLatch.Remove(trow);
                  }
                }//lock
            }

            public Schema(string name, params FieldDef[] fieldDefs) : this(name, false, fieldDefs, null)
            {

            }

            public Schema(string name, bool readOnly, params FieldDef[] fieldDefs) : this(name, readOnly, fieldDefs, null)
            {

            }

            public Schema(string name, bool readOnly, IEnumerable<TableAttribute> tableAttributes, params FieldDef[] fieldDefs) : this(name, readOnly, fieldDefs, tableAttributes)
            {

            }

            public Schema(string name, bool readOnly, IEnumerable<FieldDef> fieldDefs, IEnumerable<TableAttribute> tableAttributes = null)
            {
                if (name.IsNullOrWhiteSpace())
                    throw new CRUDException(StringConsts.ARGUMENT_ERROR + "CRUD.Schema.ctor(name==null|empty)");

                if (fieldDefs==null || !fieldDefs.Any())
                    throw new CRUDException(StringConsts.ARGUMENT_ERROR + "CRUD.Schema.ctor(fieldDefs==null|empty)");

                m_Name = name;
                m_ReadOnly = readOnly;
                if (tableAttributes==null)
                    m_TableAttrs = new List<TableAttribute>();
                else
                    m_TableAttrs = new List<TableAttribute>( tableAttributes );

                m_FieldDefs = new OrderedRegistry<FieldDef>();
                int order = 0;
                foreach(var fd in fieldDefs)
                {
                    fd.m_Order = order;
                    m_FieldDefs.Register( fd );
                    order++;
                }

            }



        #endregion

        #region Fields
            
            private string m_Name;
            private bool m_ReadOnly;
            private Type m_TypedRowType;

            private List<TableAttribute> m_TableAttrs;
            private OrderedRegistry<FieldDef> m_FieldDefs;

        #endregion

        #region Properties
            
            /// <summary>
            /// For TypedRows, returns a unique fully-qualified row type name, whichs is the global identifier of this schema instance
            /// </summary>
            public string Name
            {
                get { return m_Name; }
            }

            /// <summary>
            /// Specifies that target that this schema represents (i.e. db table) is not updatable so DataStore will not be able to save row changes made in ram
            /// </summary>
            public bool ReadOnly
            {
                get { return m_ReadOnly;}
            }
            
            /// <summary>
            /// Returns a type of TypedRow if schema was created for TypedRow, or null 
            /// </summary>
            public Type TypedRowType
            {
                get { return m_TypedRowType;}
            }

            /// <summary>
            /// Returns table-level attributes
            /// </summary>
            public IEnumerable<TableAttribute> TableAttrs { get { return m_TableAttrs;}}
            
            /// <summary>
            /// Returns FieldDefs in their order within rows that this schema describes
            /// </summary>
            public IEnumerable<FieldDef> FieldDefs { get { return m_FieldDefs.OrderedValues;}}


            /// <summary>
            /// Returns FieldDefs in their order within rows that are declared as key fields in any target
            /// </summary>
            public IEnumerable<FieldDef> AnyTargetKeyFieldDefs { get { return m_FieldDefs.Where(fd => fd.AnyTargetKey);}}



            /// <summary>
            /// Returns a field definition by a unique case-insensitive field name within schema
            /// </summary>
            public FieldDef this[string name]
            {
                get{ return m_FieldDefs[name];}
            }

            /// <summary>
            /// Returns a field definition by a positional index within the row
            /// </summary>
            public FieldDef this[int index]
            {
                get{ return m_FieldDefs[index];}
            }

            /// <summary>
            /// Returns field count
            /// </summary>
            public int FieldCount {get { return m_FieldDefs.Count;}}

        #endregion


        #region Public

            /// <summary>
            /// Finds fielddef by name or throws if name is not found
            /// </summary>
            public FieldDef GetFieldDefByIndex(int index)
            {
                var result = this[index];
                if (result==null) throw new CRUDException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args("["+index+"]", Name));
                return result;
            }

            /// <summary>
            /// Finds fielddef by name or throws if name is not found
            /// </summary>
            public FieldDef GetFieldDefByName(string fieldName)
            {
                var result = this[fieldName];
                if (result==null) throw new CRUDException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args(fieldName, Name));
                return result;
            }


            /// <summary>
            /// Returns FieldDefs in their order within rows that are declared as key fields for particular target
            /// </summary>
            public IEnumerable<FieldDef> GetKeyFieldDefsForTarget(string targetName)
            {
                 foreach( var fd in m_FieldDefs)
                 {
                    var fattr = fd[targetName];
                    if (fattr!=null && fattr.Key) yield return fd;
                 }
            }


            public IEnumerator<Schema.FieldDef> GetEnumerator()
            {
                return FieldDefs.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return FieldDefs.GetEnumerator();
            }


            /// <summary>
            /// Returns a TableAttribute that matches the supplied targetName, or if one was not defined then
            ///  returns TableAttribute which matches any target or null
            /// </summary>
            public TableAttribute GetTableAttrForTarget(string targetName)
            {
                if (targetName.IsNotNullOrWhiteSpace())
                {
                    var atr = m_TableAttrs.FirstOrDefault(a => targetName.EqualsIgnoreCase(a.TargetName));
                    if (atr!=null) return atr;
                }
                return m_TableAttrs.FirstOrDefault(a => TargetedAttribute.ANY_TARGET.EqualsIgnoreCase(a.TargetName));
            }


            public override string ToString()
            {
                return "Schema(Name: '{0}', Count: {1})".Args(Name, m_FieldDefs.Count);
            }
            

            /// <summary>
            /// Performs logical equivalence testing of two schemas
            /// </summary>
            public bool IsEquivalentTo(Schema other)
            {
                if (other==null) return false;
                if (object.ReferenceEquals(this, other)) return true;

                if (!Name.EqualsIgnoreCase(other.Name)) return false;
                
                if (this.m_TableAttrs.Count != other.m_TableAttrs.Count ||
                    this.m_FieldDefs.Count != other.m_FieldDefs.Count) return false;

                var cnt = this.m_TableAttrs.Count;
                for(var i=0; i<cnt; i++)
                  if (!this.m_TableAttrs[i].Equals(other.m_TableAttrs[i])) return false;

                cnt = this.m_FieldDefs.Count;
                for(var i=0; i<cnt; i++)
                  if (!this.m_FieldDefs[i].Equals(other.m_FieldDefs[i])) return false;

                return true;
            }


        #endregion

        #region IJSONWritable

            /// <summary>
            /// Writes schema as JSON. Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
            /// </summary>
            public void WriteAsJSON(System.IO.TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
            {
                var map = new Dictionary<string, object>
                {
                  {"Name", "JSON"+m_Name.GetHashCode()},       
                  {"FieldDefs", m_FieldDefs}
                };
                JSONWriter.WriteMap(wri, map, nestingLevel, options);
            } 


        #endregion


        #region Comparer
          

          /// <summary>
          /// Returns an instance of IEqualityComparer(Schema) that performs logical equivalence testing
          /// </summary>
          public static IEqualityComparer<Schema> SchemaEquivalenceEqualityComparer{ get{ return new schemaEquivalenceEqualityComparer();} }


            private struct schemaEquivalenceEqualityComparer : IEqualityComparer<Schema>
            {

              public bool Equals(Schema x, Schema y)
              {
                if (x==null) return false;
                return x.IsEquivalentTo(y);
              }

              public int GetHashCode(Schema obj)
              {
                if (obj==null) return 0;
                return obj.Name.GetHashCodeIgnoreCase() ^ obj.FieldCount;
              }
            }
        #endregion
            
    }



   




}
