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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFX.Environment;

using NFX.Serialization.JSON;

namespace NFX.DataAccess.CRUD
{
    
    /// <summary>
    /// Base class for any CRUD row. This class has two direct subtypes - DynamicRow and TypedRow.
    /// Rows are NOT THREAD SAFE by definition
    /// </summary>
    [Serializable]
    public abstract class Row : IConfigurable, IConfigurationPersistent, IEquatable<Row>, IEnumerable<Object>, IValidatable, IJSONWritable
    {

        #region Static

            /// <summary>
            /// Factory method that makes an appropriate row type.For performance purposes,
            ///  this method does not check passed type for Row-derivation and returns null instead if type was invalid
            /// </summary>
            /// <param name="schema">Schema, which is used for creation of DynamicRows and their derivatives</param>
            /// <param name="tRow">
            /// A type of row to create, if the type is TypedRow-descending then a parameterless .ctor is called, 
            /// otherwise a type must have a .ctor that takes schema as a sole argument
            /// </param>
            /// <returns>
            /// Row instance or null if wrong type was passed. For performance purposes,
            ///  this method does not check passed type for Row-derivation and returns null instead if type was invalid
            /// </returns>
            public static Row MakeRow(Schema schema, Type tRow = null)
            {
                if (tRow!=null)
                {
                    if (typeof(TypedRow).IsAssignableFrom(tRow))
                        return Activator.CreateInstance(tRow) as Row;
                    else
                        return Activator.CreateInstance(tRow, schema) as Row;
                }
                
                return new DynamicRow(schema);
            }


        #endregion


       
        #region Properties

            /// <summary>
            /// References a schema for a table that this row is part of
            /// </summary>
            public abstract Schema Schema { get; }

            /// <summary>
            /// Gets/sets field values by name 
            /// </summary>
            public object this[string fieldName]
            {
                get
                {
                    try
                    {
                        return GetFieldValue( Schema.GetFieldDefByName(fieldName) );       
                    }
                    catch(Exception error)
                    {
                        throw new CRUDException(StringConsts.CRUD_FIELD_VALUE_GET_ERROR.Args(fieldName, error.ToMessageWithType()), error);
                    }
                }
                set
                {
                    try
                    {
                        SetFieldValue( Schema.GetFieldDefByName(fieldName), value);
                    }
                    catch(Exception error)
                    {
                        throw new CRUDException(StringConsts.CRUD_FIELD_VALUE_SET_ERROR.Args(fieldName, error.ToMessageWithType()), error);
                    }
                }
            }

            /// <summary>
            /// Gets/sets field values by positional index(Order)
            /// </summary>
            public object this[int fieldIdx]
            {
                get
                {          
                    try
                    {
                        return GetFieldValue( Schema.GetFieldDefByIndex( fieldIdx ) );       
                    }
                    catch(Exception error)
                    {
                        throw new CRUDException(StringConsts.CRUD_FIELD_VALUE_GET_ERROR.Args("["+fieldIdx+"]", error.ToMessageWithType()), error);
                    }
                }
                set
                {
                    try
                    {
                        SetFieldValue(Schema.GetFieldDefByIndex(fieldIdx), value);
                    }
                    catch(Exception error)
                    {
                        throw new CRUDException(StringConsts.CRUD_FIELD_VALUE_SET_ERROR.Args("["+fieldIdx+"]", error.ToMessageWithType()), error);
                    }
                }
            }


            /// <summary>
            /// Returns values for fields that represent row's primary key
            /// </summary>
            public IDataStoreKey GetDataStoreKey(string targetName = null)
            {
                var result = new NameValueDataStoreKey();
                foreach(var kdef in Schema.GetKeyFieldDefsForTarget(targetName))
                    result.Add(kdef.GetBackendNameForTarget(targetName), this[kdef.Order]);

                return result;
            }

        #endregion
        
        #region Public

            /// <summary>
            /// In base class applies Config attribute. Useful for typed rows
            /// </summary>
            public virtual void Configure(IConfigSectionNode node)
            {
                ConfigAttribute.Apply(this, node);
            }

            /// <summary>
            /// The base class does not implement this method. Override to persist row fields into config node
            /// </summary>
            public virtual void PersistConfiguration(ConfigSectionNode node)
            {
                throw new NotImplementedException();
            }
            
            /// <summary>
            /// Performs validation of data in the row returning exception object that provides description
            /// in cases when validation does not pass. Validation is performed not targeting any particular backend
            /// </summary>
            public Exception Validate()
            {
                return Validate(null);
            }
            
            
            /// <summary>
            /// Validates row using row schema and supplied field definitions.
            /// Override to perform custom validations,
            /// i.e. TypeRows may directly access properties and write some validation type-safe code
            /// The method is not expected to throw exception in case of failed validation, rather return exception instance because
            ///  throwing exception really hampers validation performance when many rows need to be validated
            /// </summary>
            public virtual Exception Validate(string targetName)
            {
                foreach(var fd in Schema)
                {
                    var error = ValidateField(targetName, fd);
                    if (error!=null) return error;
                }   

                return null;
            }

            /// <summary>
            /// Override to perform custom row equality comparison.
            /// Default implementation equates rows using their key fields
            /// </summary>
            public virtual bool Equals(Row other)
            {
                if (other==null) return false;
                if (!this.Schema.IsEquivalentTo(other.Schema)) return false;
        
                foreach(var fdef in Schema.AnyTargetKeyFieldDefs)
                {
                     var obj1 = this[fdef.Order];
                     var obj2 = other[fdef.Order];
                     if (obj1==null && obj2==null) continue; 
                     if (! obj1.Equals(obj2) ) return false;
                } 
        
                return true;
            }

            /// <summary>
            /// Object override - sealed. Override Equals(row) instead
            /// </summary>
            public sealed override bool Equals(object obj)
            {
                return this.Equals(obj as Row);
            }

            /// <summary>
            /// Object override - gets hash code from key fields
            /// </summary>
            public override int GetHashCode()
            {
                var result = 0;
                foreach(var fdef in Schema.AnyTargetKeyFieldDefs)
                {
                     var val = this[fdef.Order];
                     if (val!=null) result+= val.GetHashCode();
                } 
                return result;
            }


            /// <summary>
            /// Returns true if this row satisfies simple filter - it contains the supplied filter string.
            /// The filter pattern may start or end with "*" char that denotes a wildcard. A wildcard is permitted on both sides of the filter value
            /// </summary>
            public bool SimpleFilterPredicate(string filter, bool caseSensitive = false)
            {
              if (filter==null) return false;
      
              if (!caseSensitive)
                filter = filter.ToUpperInvariant();
              
              var sany = false;
              var eany = false;
      
              if (filter.StartsWith("*"))
              {
                sany = true;
                filter = filter.Remove(0, 1);
              }
      
              if (filter.EndsWith("*"))
              {
                eany = true;
                filter = filter.Remove(filter.Length-1, 1);
              }
      
              foreach(var val in this)
              {
                 if (val==null) continue;
         
                 var sval = val.ToString();
                 
                 if (!caseSensitive)
                    sval = sval.ToUpperInvariant();
         
                 if (sany && eany && sval.Contains(filter)) return true;
         
                 if (!sany && eany && sval.StartsWith(filter)) return true;
         
                 if (sany && !eany && sval.EndsWith(filter)) return true;
         
                 if (!sany && !eany && sval==filter) return true;
              }
      
              return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new rowFieldValueEnumerator(this);
            }

            public IEnumerator<Object> GetEnumerator()
            {
                return new rowFieldValueEnumerator(this);
            }


            /// <summary>
            /// Gets value of the field, for typerows it accesses property using reflection; for dynamic rows it reads data from
            ///  row buffer array using field index(order)
            /// </summary>
            public abstract object GetFieldValue(Schema.FieldDef fdef);

            /// <summary>
            /// Sets value of the field, for typerows it accesses property using reflection; for dynamic rows it sets data into
            ///  row buffer array using field index(order)
            /// </summary>
            public abstract void SetFieldValue(Schema.FieldDef fdef, object value);


            /// <summary>
            /// Converts field value to the type specified by Schema.FieldDef. For example converts GDID->ulong or ulong->GDID.
            /// This method can be overridden to perform custom handling of types,
            ///  for example one can assign bool field as "Si" that would convert to TRUE.
            /// This method is called by SetFieldValue(...) before assigning actual field buffer 
            /// </summary>
            /// <param name="fdef">Field being converted</param>
            /// <param name="value">Value to convert</param>
            /// <returns>Converted value before assignment to field buffer</returns>
            public virtual object ConvertFieldValueToDef(Schema.FieldDef fdef, object value)
            {
              if (value==DBNull.Value) value = null;

                if (value != null)
                {
                    var tv = value.GetType();

                    if (tv != fdef.NonNullableType && !fdef.NonNullableType.IsAssignableFrom(tv))
                    {
                      // 20150224 DKh, addedEra to GDID. Only GDIDS with ERA=0 can be converted to/from INT64
                      if (fdef.NonNullableType==typeof(NFX.DataAccess.Distributed.GDID))
                        value = new Distributed.GDID(0, (UInt64)Convert.ChangeType(value, typeof(UInt64)));
                      else
                       if (tv==typeof(Distributed.GDID))
                       {
                         var gdid = (Distributed.GDID)value;
                         if (gdid.Era!=0)
                           throw new CRUDException(StringConsts.CRUD_GDID_ERA_CONVERSION_ERROR.Args(fdef.Name, fdef.NonNullableType.Name));
                         value = gdid.ID;
                       }

                      //else
                      value = Convert.ChangeType(value, fdef.NonNullableType);
                    }
                }   

              return value;
            }

            /// <summary>
            /// Writes default values specified in schema into fields.
            /// Pass overwrite=true to force defaults over non-null existing values (false by default)
            /// </summary>
            public void ApplyDefaultFieldValues(string targetName = null, bool overwrite = false)
            {
              foreach(var fdef in  Schema)
              {
                var attr = fdef[targetName];
                if (attr==null) continue;
                if (attr.Default!=null)
                 if (overwrite || this.GetFieldValue(fdef)==null)
                  this.SetFieldValue(fdef, attr.Default);
              }
            }


            /// <summary>
            /// Copies fields from this row into another row/form
            /// </summary>
            public void CopyFields(Row other, bool includeAmorphousData = true, bool invokeAmorphousAfterLoad = true)
            {
              if (other==null) return;
              var oad = includeAmorphousData ? other as IAmorphousData : null;

              if (oad!=null && this is IAmorphousData)
              {
               var athis = (IAmorphousData)this;
               if (oad.AmorphousDataEnabled && athis.AmorphousDataEnabled)//only copy IF amorphous behavior is enabled here and in the destination
               {
                 foreach(var kvp in athis.AmorphousData)
                 {
                   var ofd = other.Schema[kvp.Key];
                   if (ofd!=null)
                   {
                     try 
                     {
                       other.SetFieldValue(ofd, kvp.Value); 
                       continue;
                     }catch{} //this may be impossible, then we assign to amorphous in other
                   }
                   oad.AmorphousData[kvp.Key] = kvp.Value;
                 }
               }
              }

              foreach(var fdef in this.Schema)
              {
                var atr = fdef[null];
                var stored = (atr==null) ? true : (atr.StoreFlag == StoreFlag.LoadAndStore || atr.StoreFlag == StoreFlag.OnlyStore);
                if (!stored) continue;

                var ofd = other.Schema[fdef.Name];
                if (ofd==null) 
                {
                  if (oad!=null && oad.AmorphousDataEnabled)// IF amorphous behavior is enabled in destination
                  {
                    oad.AmorphousData[fdef.Name] = GetFieldValue(fdef); 
                  }
                  continue;
                }

                other.SetFieldValue(ofd, GetFieldValue(fdef));
              }//foreach

              if (oad!=null && oad.AmorphousDataEnabled && invokeAmorphousAfterLoad)
                oad.AfterLoad( this is FormModel ? ((FormModel)this).DataStoreTargetName : string.Empty);
            }


        #endregion

         #region IJSONWritable

            /// <summary>
            /// Writes row as JSON either as an array or map depending on JSONWritingOptions.RowsAsMap setting.
            /// Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
            /// </summary>
            public void WriteAsJSON(System.IO.TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
            {
                if (options==null || !options.RowsAsMap)
                {
                  JSONWriter.WriteArray(wri, this, nestingLevel, options);
                  return;
                }

                var map = new Dictionary<string, object>();
                foreach(var fd in Schema)
                  map.Add(fd.Name, GetFieldValue(fd));
                
                if (this is IAmorphousData)
                {
                  var amorph = (IAmorphousData)this;
                  if (amorph.AmorphousDataEnabled)
                    foreach(var kv in amorph.AmorphousData)
                    {
                      var key = kv.Key;
                      while(map.ContainsKey(key)) key+="_";
                      map.Add(key, kv.Value);
                    }
                }

                JSONWriter.WriteMap(wri, map, nestingLevel, options);
            } 


        #endregion

        #region Protected

            
            
            
            /// <summary>
            /// Validates row field using Schema.FieldDef settings.
            /// This method is invoked by base Validate() implementation.
            /// The method is not expected to throw exception in case of failed validation, rather return exception instance because
            ///  throwing exception really hampers validation performance when many rows need to be validated
            /// </summary>
            protected virtual Exception ValidateField(string targetName, Schema.FieldDef  fdef)
            {
                var atr = fdef[targetName];
                if (atr==null) return null;
                
                var value = GetFieldValue(fdef);
                
                if (value==null || (value is string && ((string)value).IsNullOrWhiteSpace()))
                {
                   if (atr.Required) 
                    return new CRUDFieldValidationException(Schema.Name, fdef.Name, StringConsts.CRUD_FIELD_VALUE_REQUIRED_ERROR);
                   
                   return null;
                }

                if (value is IValidatable)
                   return (((IValidatable)value).Validate(targetName));

                if (value is IEnumerable<IValidatable>)//List<IValidatable>, IValidatable[]
                {
                   foreach(var v in (IEnumerable<IValidatable>)value)
                   {
                     if (v==null) continue;
                     var error = v.Validate(targetName);
                     if (error!=null) return error;
                   }
                   return null; 
                }

                if (value is IEnumerable<KeyValuePair<string, IValidatable>>)//Dictionary<string, IValidatable>
                {
                   foreach(var kv in (IEnumerable<KeyValuePair<string, IValidatable>>)value)
                   {
                     var v = kv.Value;
                     if (v==null) continue;
                     var error = v.Validate(targetName);
                     if (error!=null) return error;
                   }
                   return null; 
                }

                if (atr.ValueList.IsNotNullOrWhiteSpace())//check dictionary
                {
                    var parsed = atr.ParseValueList();
                    if (!parsed.ContainsKey(value.ToString()))
                        return new CRUDFieldValidationException(Schema.Name, fdef.Name, StringConsts.CRUD_FIELD_VALUE_IS_NOT_IN_LIST_ERROR);
                }

                if (atr.MinLength>0)
                    if (value.ToString().Length<atr.MinLength)
                       return new CRUDFieldValidationException(Schema.Name, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_LENGTH_ERROR);

                if (atr.MaxLength>0)
                    if (value.ToString().Length>atr.MaxLength)
                       return new CRUDFieldValidationException(Schema.Name, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_LENGTH_ERROR);
                
                if (atr.Kind==DataKind.ScreenName)
                    if (!NFX.Parsing.DataEntryUtils.CheckScreenName(value.ToString()))
                       return new CRUDFieldValidationException(Schema.Name, fdef.Name, StringConsts.CRUD_FIELD_VALUE_SCREEN_NAME_ERROR);

                if (value is IComparable)
                {
                    var error = CheckMinMax(atr, fdef.Name, (IComparable)value);
                    if (error!=null) return error;
                }
                     
                return null;
            }

            protected Exception CheckMinMax(FieldAttribute atr, string fName, IComparable val)
            {
               if (atr.Min != null)
               {
                    var bound = atr.Min as IComparable;
                    if (bound != null)
                    {
                        var tval = val.GetType();

                        bound = Convert.ChangeType(bound, tval) as IComparable;

                        if (val.CompareTo(bound)<0) 
                            return new CRUDFieldValidationException(Schema.Name, fName, StringConsts.CRUD_FIELD_VALUE_MIN_BOUND_ERROR);
                    }
               }

               if (atr.Max != null)
               {
                    var bound = atr.Max as IComparable;
                    if (bound != null)
                    {
                        var tval = val.GetType();

                        bound = Convert.ChangeType(bound, tval) as IComparable;

                        if (val.CompareTo(bound)>0) 
                            return new CRUDFieldValidationException(Schema.Name, fName, StringConsts.CRUD_FIELD_VALUE_MAX_BOUND_ERROR);
                    }
               }

               return null;
            }

        #endregion


      
        #region .pvt

            private class rowFieldValueEnumerator : IEnumerator<object>
            {
                
                internal rowFieldValueEnumerator(Row row)
                {
                    m_Row = row;
                }
                
                public void Dispose()
                {

                }

                private int m_Index = -1;
                private Row m_Row;



                public object Current
                {
                    get { return m_Row[m_Index]; }
                }

                public bool MoveNext()
                {
                    if (m_Index==m_Row.Schema.FieldCount-1) return false;
                    m_Index++;
                    return true;
                }

                public void Reset()
                {
                    m_Index = -1;
                }
            }

        #endregion

           
    }


   
}
