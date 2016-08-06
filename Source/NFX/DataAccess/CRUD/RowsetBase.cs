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

using NFX.Serialization.JSON;

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Provides base for rowset implementation.
    /// Rowsets are mutable lists of rows where all rows must have the same schema, however a rowset may contain a mix of
    ///  dynamic and typed rows as long as they have the same schema.
    /// Rowsets are not thread-safe
    /// </summary>
    [Serializable]
    public abstract class RowsetBase : IList<Row>, IComparer<Row>, IJSONWritable, IValidatable
    {
        #region .ctor/static

            /// <summary>
            /// Reads either Table or Rowset from JSON created by WriteAsJSON. Metadata must be present
            /// </summary>
            public static RowsetBase FromJSON(string json, bool schemaOnly = false, bool readOnlySchema = false)
            {
              return FromJSON( JSONReader.DeserializeDataObject(json) as JSONDataMap, schemaOnly, readOnlySchema );
            }


            /// <summary>
            /// Reads either Table or Rowset from JSON created by WriteAsJSON. Metadata must be present
            /// </summary>
            public static RowsetBase FromJSON(JSONDataMap jsonMap, bool schemaOnly = false, bool readOnlySchema = false)
            {
              bool dummy;
              return FromJSON(jsonMap, out dummy, schemaOnly, readOnlySchema);
            }

            /// <summary>
            /// Reads either Table or Rowset from JSON created by WriteAsJSON. Metadata must be present.
            /// allMatched==false when some data did not match schema (i.e. too little fields or extra fields supplied)
            /// </summary>
            public static RowsetBase FromJSON(JSONDataMap jsonMap,
                                              out bool allMatched,
                                              bool schemaOnly = false,
                                              bool readOnlySchema = false,
                                              SetFieldFunc setFieldFunc = null)
            {
              if (jsonMap == null || jsonMap.Count == 0)
                throw new CRUDException(StringConsts.ARGUMENT_ERROR + "RowsetBase.FromJSON(jsonMap=null)");

              var schMap = jsonMap["Schema"] as JSONDataMap;
              if (schMap==null)
                throw new CRUDException(StringConsts.ARGUMENT_ERROR + "RowsetBase.FromJSON(jsonMap!schema)");

              var schema = Schema.FromJSON(schMap, readOnlySchema);
              var isTable = jsonMap["IsTable"].AsBool();

              allMatched = true;
              var result = isTable ? (RowsetBase)new Table(schema) : (RowsetBase)new Rowset(schema);
              if (schemaOnly) return result;

              var rows = jsonMap["Rows"] as JSONDataArray;
              if (rows==null) return result;


              foreach(var jrow in rows)
              {
                var jdo = jrow as IJSONDataObject;
                if (jdo==null)
                {
                  allMatched = false;
                  continue;
                }

                var row = new DynamicRow(schema);

                if (!Row.TryFillFromJSON(row, jdo, setFieldFunc)) allMatched = false;

                result.Add( row );
              }

              return result;
            }



            /// <summary>
            /// Creates an empty rowset
            /// </summary>
            protected RowsetBase(Schema schema)
            {
                m_Schema = schema;
                m_InstanceGUID = Guid.NewGuid();
            }

        #endregion

        #region Fields

            private Guid m_InstanceGUID;
            protected Schema m_Schema;
            internal List<Row> m_List;
            internal List<RowChange> m_Changes;

            private JSONDynamicObject m_DataContext;
        #endregion


        #region Properties

            /// <summary>
            /// Returns globaly-unique instance ID.
            /// This ID is useful as a key for storing rowsets in object stores and posting data back from web client to server.
            /// </summary>
            public Guid InstanceGUID { get { return m_InstanceGUID;} }


            /// <summary>
            /// Returns a schema for rows that this rowset contains
            /// </summary>
            public Schema Schema { get { return m_Schema;} }

            /// <summary>
            /// Returns row count in this rowset
            /// </summary>
            public int Count { get { return m_List.Count; }}


            /// <summary>
            /// Returns data as non-generic readonly IList
            /// </summary>
            public System.Collections.IList AsReadonlyIList{ get{ return new iListReadOnly(m_List);}}

            /// <summary>
            /// Gets/Sets whether this rowset keeps track of all modifications done to it.
            /// This property must be set to true to be able to save changes into ICRUDDataStore
            /// </summary>
            public bool LogChanges
            {
                get { return m_Changes != null; }
                set
                {
                    if (value && m_Changes==null) m_Changes = new List<RowChange>();
                    else
                    if (!value && m_Changes!=null) m_Changes = null;
                }
            }

            /// <summary>
            /// Returns accumulated modifications performed on the rowset, or empty enumerator if no modifications have been made or
            ///  LogModifications = false
            /// </summary>
            public IEnumerable<RowChange> Changes
            {
                get { return m_Changes ?? Enumerable.Empty<RowChange>(); }
            }

            /// <summary>
            /// Returns a count of accumulated modifications performed on the rowset, or zero when no modifications have been made or
            ///  LogModifications = false
            ///  </summary>
            public int ChangeCount
            {
                get { return m_Changes!=null?m_Changes.Count : 0; }
            }


            /// <summary>
            /// Provides dynamic view as JSONDataMap of rowset's data context - attributes applicable to the whole rowset
            /// </summary>
            public JSONDataMap ContextMap
            {
                get
                {
                  var data = this.Context;//laizily created if needed

                  return m_DataContext.Data as JSONDataMap;
                }
            }

            /// <summary>
            /// Provides dynamic view of rowset's data context - attributes applicable to the whole rowset
            /// </summary>
            public dynamic Context
            {
               get
               {
                  if (m_DataContext==null)
                    m_DataContext = new JSONDynamicObject(JSONDynamicObjectKind.Map, false);

                  return m_DataContext;
               }
            }

        #endregion

        #region Public

            /// <summary>
            /// Inserts the row. Returns insertion index
            /// </summary>
            public int Insert(Row row)
            {
                Check(row);
                var idx = DoInsert(row);
                if (idx>=0 && m_Changes!=null) m_Changes.Add( new RowChange(RowChangeType.Insert, row, null) );
                return idx;
            }

            /// <summary>
            /// Updates the row, Returns the row index or -1
            /// </summary>
            public int Update(Row row, IDataStoreKey key = null)
            {
                Check(row);
                var idx = DoUpdate(row, key);
                if (idx>=0 && m_Changes!=null) m_Changes.Add( new RowChange(RowChangeType.Update, row, key) );
                return idx;
            }


            /// <summary>
            /// Tries to find a row for update and if found, updates it and returns true,
            ///  otherwise inserts the row (if schemas match) and returns false. Optionally pass updateWhere condition
            ///   that may check whether update needs to be performed
            /// </summary>
            public bool Upsert(Row row, Func<Row, bool> updateWhere = null)
            {
               Check(row);
               var update = DoUpsert(row, updateWhere);
               if (m_Changes!=null) m_Changes.Add( new RowChange(RowChangeType.Upsert, row, null) );
               return update;
            }

            /// <summary>
            /// Tries to find a row with the same set of key fields in this table and if found, deletes it and returns its index, otherwise -1
            /// </summary>
            public int Delete(Row row, IDataStoreKey key = null)
            {
               Check(row);
               var idx = DoDelete(row, key);
               if (idx>=0 && m_Changes!=null) m_Changes.Add( new RowChange(RowChangeType.Delete, row, null) );
               return idx;
            }

            /// <summary>
            /// Tries to find a row with the same set of key fields in this table and if found, deletes it and returns its index, otherwise -1
            /// </summary>
            public int Delete(params object[] keys)
            {
               return Delete(KeyRowFromValues(keys));
            }

            /// <summary>
            /// Deletes all rows from table without logging the deleted modifications even when LogModifications=true
            /// </summary>
            public void Purge()
            {
               m_List.Clear();
               m_List.TrimExcess();
            }

            /// <summary>
            /// Deletes all rows from table. This method is similar to Purge() but does logging (when enabled)
            /// </summary>
            public void DeleteAll()
            {
               if (m_Changes!=null)
                   foreach(var row in m_List)
                     m_Changes.Add( new RowChange(RowChangeType.Delete, row, null) );

               Purge();
            }

            /// <summary>
            /// Clears modifications accumulated by this instance
            /// </summary>
            public void PurgeChanges()
            {
                if (m_Changes!=null) m_Changes.Clear();
            }

            /// <summary>
            /// Creates key row out of field values for keys
            /// </summary>
            public Row KeyRowFromValues(params object[] keys)
            {
               var krow = new DynamicRow(Schema);
               var idx = 0;
               foreach(var kdef in Schema.AnyTargetKeyFieldDefs)
               {
                    if (idx>keys.Length-1) throw new CRUDException(StringConsts.CRUD_FIND_BY_KEY_LENGTH_ERROR);
                    krow[kdef.Order] = keys[idx];
                    idx++;
               }

               return krow;
            }


            /// <summary>
            /// Tries to find a row by specified keyset and returns it or null if not found
            /// </summary>
            public Row FindByKey(Row keyRow)
            {
               return FindByKey(keyRow, null);
            }

            /// <summary>
            /// Tries to find a row by specified keyset and returns it or null if not found.
            /// This method does not perform well on Rowsets instances as a rowset is unordered list which does linear search.
            /// In contrast, Tables are always ordered and perform binary search instead
            /// </summary>
            public Row FindByKey(params object[] keys)
            {
               return FindByKey(KeyRowFromValues(keys), null);
            }


            /// <summary>
            /// Tries to find a row by specified keyset and extra WHERE clause and returns it or null if not found.
            /// This method does not perform well on Rowsets instances as a rowset is unordered list which does linear search.
            /// In contrast, Tables are always ordered and perform binary search instead
            /// </summary>
            public Row FindByKey(Row row, Func<Row, bool> extraWhere)
            {
               Check(row);

               int insertIndex;
               int idx = SearchForRow(row, out insertIndex);
               if (idx<0)
                  return null;

               if (extraWhere!=null)
               {
                 if (!extraWhere(m_List[idx])) return null;//where did notmatch
               }

               return m_List[idx];
            }

            /// <summary>
            /// Tries to find a row by specified keyset and extra WHERE clause and returns it or null if not found
            /// </summary>
            public Row FindByKey(Func<Row, bool> extraWhere, params object[] keys)
            {
               return FindByKey(KeyRowFromValues(keys), extraWhere);
            }


            /// <summary>
            /// Retrievs a change by index or null if index is out of bounds or changes are not logged
            /// </summary>
            public RowChange? GetChangeAt(int idx)
            {
              if (m_Changes==null) return null;

              if (idx>=m_Changes.Count) return null;

              return m_Changes[idx];
            }

            /// <summary>
            /// Validates all rows in this rowset.
            /// Override to perform custom validations.
            /// The method is not expected to throw exception in case of failed validation, rather return exception instance because
            ///  throwing exception really hampers validation performance when many rows need to be validated
            /// </summary>
            public virtual Exception Validate(string targetName)
            {
                foreach(var row in this)
                {
                    var error = row.Validate(targetName);
                    if (error!=null) return error;
                }

                return null;
            }


        #endregion

        #region IComparer<Row> Members

          /// <summary>
          /// Compares two rows
          /// </summary>
          public abstract int Compare(Row rowA, Row rowB);
        #endregion


        #region IEnumerable Members

            public IEnumerator<Row> GetEnumerator()
            {
              return m_List.GetEnumerator();
            }


            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
              return m_List.GetEnumerator();
            }
         #endregion


         #region IList

           public int IndexOf(Row row)
           {
               Check(row);
               return m_List.IndexOf(row);
           }

           /// <summary>
           /// Inserts row at index
           /// </summary>
           public virtual void Insert(int index, Row row)
           {
               Check(row);
               m_List.Insert(index, row);
               if (m_Changes!=null) m_Changes.Add( new RowChange(RowChangeType.Insert, row, null) );
           }

           /// <summary>
           /// Deletes row
           /// </summary>
           public void RemoveAt(int index)
           {
               Delete( m_List[index] );
           }

           /// <summary>
           /// This method performs update on set
           /// </summary>
           public virtual Row this[int index]
           {
               get
               {
                   return m_List[index];
               }
               set
               {
                   Check(value);
                   var existing = m_List[index];
                   if (object.ReferenceEquals(existing, value)) return;
                   m_List[index] = value;
                   if (m_Changes!=null)
                   {
                     m_Changes.Add( new RowChange(RowChangeType.Delete, existing, null) );
                     m_Changes.Add( new RowChange(RowChangeType.Insert, value, null) );
                   }
               }
           }

           /// <summary>
           /// Inserts a row
           /// </summary>
           public void Add(Row row)
           {
               Insert(row);
           }

           /// <summary>
           /// Purges table
           /// </summary>
           public void Clear()
           {
               Purge();
           }

           public bool Contains(Row row)
           {
               return m_List.Contains(row);
           }

           public void CopyTo(Row[] array, int arrayIndex)
           {
               m_List.CopyTo(array, arrayIndex);
           }

           public bool IsReadOnly
           {
               get { return false; }
           }

           /// <summary>
           /// Performs row delete
           /// </summary>
           public bool Remove(Row item)
           {
               return Delete(item)>=0;
           }
        #endregion


        #region IJSONWritable

            /// <summary>
            /// Writes rowset as JSON including schema information. Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
            /// </summary>
            public void WriteAsJSON(System.IO.TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
            {
                var tp = GetType();

                var metadata = options!=null && options.RowsetMetadata;

                var map = new Dictionary<string, object>//A root JSON object is needed for security, as some browsers can EXECUTE a valid JSON root array
                {
                  {"Rows", m_List}
                };

                if (metadata)
                {
                    map.Add("Instance", m_InstanceGUID.ToString("D"));
                    map.Add("Type", tp.FullName);
                    map.Add("IsTable", typeof(Table).IsAssignableFrom( tp ));
                    map.Add("Schema", m_Schema);
                }

                JSONWriter.WriteMap(wri, map, nestingLevel, options);
            }


        #endregion


         #region .Protected

           /// <summary>
           /// Checks argument for being non-null and of the same schema with this rowset
           /// </summary>
           protected void Check(Row row)
           {
             if (row==null || m_Schema!=row.Schema) throw new CRUDException(StringConsts.CRUD_ROWSET_OPERATION_ROW_IS_NULL_OR_SCHEMA_MISMATCH_ERROR);
           }


           /// <summary>
           /// Provides rowsearching. Override to do binary search in sorted rowsets
           /// </summary>
           /// <param name="row">A row to search for</param>
           /// <param name="index">An index where search collapsed without finding the match. Used for sorted insertions</param>
           protected virtual int SearchForRow(Row row, out int index)
           {
              for(int i=0; i<m_List.Count; i++)
              {
                if (m_List[i].Equals(row))
                {
                     index = i;
                     return i;
                }
              }

              index = m_List.Count;
              return -1;
           }


           /// <summary>
            /// Tries to insert a row. If another row with the same set of key fields already in the table returns -1, otherwise
            ///  returns insertion index
            /// </summary>
            protected virtual int DoInsert(Row row)
            {
                int idx = 0;
                if (SearchForRow(row, out idx) >=0) return -1;

                m_List.Insert(idx, row);

                return idx;
            }

            /// <summary>
            /// Tries to find a row with the same set of key fields in this table and if found, replaces it and returns its index,
            ///  otherwise returns -1
            /// </summary>
            protected virtual int DoUpdate(Row row, IDataStoreKey key = null)
            {
               int dummy;
               int idx = SearchForRow(row, out dummy);
               if (idx<0) return -1;

               m_List[idx] = row;

               return idx;
            }


            /// <summary>
            /// Tries to find a row with the same set of key fields in this table and if found, replaces it and returns true,
            ///  otherwise inserts the row (if schemas match) and returns false. Optionally pass updateWhere condition
            ///   that may check whether update needs to be performed
            /// </summary>
            protected virtual bool DoUpsert(Row row, Func<Row, bool> updateWhere)
            {
               int insertIndex;
               int idx = SearchForRow(row, out insertIndex);
               if (idx<0)
               {
                  m_List.Insert(insertIndex, row);
                  return false;
               }

               if (updateWhere!=null)
               {
                 if (!updateWhere(m_List[idx])) return false;//where did not match
               }

               m_List[idx] = row;
               return true;
            }

            /// <summary>
            /// Tries to find a row with the same set of key fields in this table and if found, deletes it and returns its index, otherwise -1
            /// </summary>
            protected virtual int DoDelete(Row row, IDataStoreKey key = null)
            {
               int dummy;
               int idx = SearchForRow(row, out dummy);
               if (idx<0) return -1;

               m_List.RemoveAt(idx);
               return idx;
            }


         #endregion





    }






}
