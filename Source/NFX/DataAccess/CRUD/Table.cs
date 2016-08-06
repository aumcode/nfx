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

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Implements a master table. Tables are rowsets that are pre-sorted by keys.
    /// They are used in scenarios when in-memory data replication is needed.
    /// A table supports efficient FindKey() operation but does not support sorting.
    /// This class is not thread-safe.
    /// </summary>
    [Serializable]
    public class Table : RowsetBase
    {
        #region .ctor
            /// <summary>
            /// Creates an empty table
            /// </summary>
            public Table(Schema schema) : base(schema)
            {
                m_List =  new List<Row>();
            }

            /// <summary>
            /// Creates a shallow copy from another table, optionally applying a filter
            /// </summary>
            public Table(Table other, Func<Row, bool> filter = null) : base(other.Schema)
            {
              if (filter==null)
                m_List =  new List<Row>(other.m_List);
              else
                m_List = other.Where(filter).ToList();
            }

            /// <summary>
            /// Creates a shallow copy from another rowset resorting data per schema key definition, optionally applying a filter
            /// </summary>
            public Table(Rowset other, Func<Row, bool> filter = null) : base(other.Schema)
            {
              m_List =  new List<Row>();

              var src = filter==null ? other : other.Where(filter);

              foreach(var row in src)
                 Insert(row);
            }

        #endregion

        #region Protected

            /// <summary>
            /// Performs binary search on a sorted table
            /// </summary>
            protected override int SearchForRow(Row row, out int index)
            {
                int top = 0;
                int bottom = m_List.Count - 1;

                index = 0;

                while (top <= bottom)
                {

                    index = top + ((bottom - top) / 2);

                    int cmpResult  = Compare(row, m_List[index]);

                    if (cmpResult == 0)
                    {
                      return index;
                    }
                    if (cmpResult > 0)
                    {
                      top = index + 1;
                      index = top;
                    }
                    else
                    {
                      bottom = index - 1;
                    }
                }

                return -1;
            }


        #endregion

        #region IComparer<Row> Members

          /// <summary>
          /// Compares two rows based on their key fields. Always compares in ascending direction
          /// </summary>
          public override int Compare(Row rowA, Row rowB)
          {
              return CompareRows(m_Schema, rowA, rowB);
          }

          /// <summary>
          /// Compares two rows based on their key fields. Always compares in ascending direction
          /// </summary>
          public static int CompareRows(Schema schema, Row rowA, Row rowB)
          {
              if (rowA==null && rowB!=null) return -1;

              if (rowA!=null && rowB==null) return 1;

              if (rowA==null && rowB==null) return 0;

              if (object.ReferenceEquals(rowA, rowB)) return 0;

              if (rowA.Schema!=rowB.Schema) return 1;


              foreach(var fld in schema.AnyTargetKeyFieldDefs)
              {
                var obj1 = rowA[fld.Order] as IComparable;
                var obj2 = rowB[fld.Order] as IComparable;

                if (obj1==null && obj2==null) continue;
                if (obj1==null) return -1;
                if (obj2==null) return +1;

                var result = obj1.CompareTo(obj2);
                if (result!=0) return result;
              }

              return 0;
          }

         #endregion


         #region IList


           /// <summary>
           /// This is IList member implementation, index is ignored
           /// </summary>
           public override void Insert(int index, Row item)
           {
               Insert(item);
           }

           /// <summary>
           /// This method does not support setting rows in table
           /// </summary>
           public override Row this[int index]
           {
               get
               {
                   return m_List[index];
               }
               set
               {
                   throw new CRUDException(StringConsts.CRUD_OPERATION_NOT_SUPPORTED_ERROR+"Table[idx].set()");
               }
           }

        #endregion



    }




}
