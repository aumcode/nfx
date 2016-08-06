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
    /// Implements a rowset that supports row change logging and CRUD operations.
    /// Rowsets are not optimal for data replication as they perform linear search which is slow, however
    /// Rowset class supports sorting. In contrast, Tables are kind of rowsets that keep data pre-sorted by key
    /// thus facilitating quick searching
    /// </summary>
    [Serializable]
    public class Rowset : RowsetBase
    {
        #region .ctor
            /// <summary>
            /// Creates an empty rowset
            /// </summary>
            public Rowset(Schema schema) : base(schema)
            {
                m_List =  new List<Row>();
                m_SortFieldList = new List<string>();
            }


            /// <summary>
            /// Creates a shallow copy from another rowset, optionally applying a filter
            /// </summary>
            public Rowset(RowsetBase other, Func<Row, bool> filter = null) : base(other.Schema)
            {
              if (filter==null)
                m_List =  new List<Row>(other.m_List);
              else
                m_List = other.Where(filter).ToList();

              m_SortFieldList = new List<string>();
            }


        #endregion

        #region Fields

            private string m_SortDefinition;
            internal List<string> m_SortFieldList;

        #endregion

        #region Properties

            /// <summary>
            /// Sort definition is a comma-separated field name list where every field may optionally be prefixed with
            /// `+` for ascending or `-` for descending sort order specifier. Example: "FirstName,-DOB"
            /// </summary>
            public string SortDefinition
            {
               get{ return m_SortDefinition ?? string.Empty; }
               set
               {
                   if (m_SortDefinition!=value)
                   {
                         m_SortDefinition = value;
                         m_SortFieldList.Clear();

                         if (m_SortDefinition!=null)
                         {
                            m_SortFieldList.AddRange(m_SortDefinition.Split(','));
                            m_SortFieldList.RemoveAll(s=>s.Trim().Length==0);
                            for(int i=0; i<m_SortFieldList.Count; i++)
                             m_SortFieldList[i] = m_SortFieldList[i].Trim();

                            m_List.Sort(this);
                         }
                   }
               }
            }


        #endregion


        #region IComparer<Row> Members

          public override int Compare(Row rowA, Row rowB)
          {
            if (rowA==null && rowB!=null) return -1;

            if (rowA!=null && rowB==null) return 1;

            if (rowA==null && rowB==null) return 0;

            if (object.ReferenceEquals(rowA, rowB)) return 0;

            if (rowA.Schema!=rowB.Schema) return 1;


            foreach(var sortDef in m_SortFieldList)
            {
                var sfld = sortDef.Trim();

                var desc = false;
                if (sfld.StartsWith("+"))
                  sfld = sfld.Remove(0,1);

                if (sfld.StartsWith("-"))
                {
                  sfld = sfld.Remove(0,1);
                  desc = true;
                }

                var fld = m_Schema[sfld];
                if (fld==null) return 1;//safeguard

                var obj1 = rowA[fld.Order] as IComparable;
                var obj2 = rowB[fld.Order] as IComparable;

                if (obj1==null && obj2==null) continue;
                if (obj1==null) return desc?1:-1;
                if (obj2==null) return desc?-1:1;

                var result = desc?-obj1.CompareTo(obj2):obj1.CompareTo(obj2);
                if (result!=0) return result;
            }
            return 0;
          }

        #endregion


        #region Protected

          protected override int DoInsert(Row row)
          {
              m_List.Add(row);
              return m_List.Count-1;
          }

        #endregion

    }
}
