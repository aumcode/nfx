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

using NFX;

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Represents query source code with pre-processed pragmas
    /// </summary>
    /// <example>
    ///#pragma
    ///modify=tbl_patient
    ///key=counter,ssn
    ///ignore=doctor_phone,doctor_id
    ///load=
    ///store=
    ///@last_name=lname
    ///@first_name=fname
    ///.doctor_id=This is description for column
    ///
    ///select
    /// t1.ssn,
    /// t1.lname as last_name,
    /// t1.fname as first_name,
    /// t1.c_doctor,
    /// t2.phone as doctor_phone,
    /// t2.NPI	as doctor_id
    ///from
    /// tbl_patient t1
    ///  left outer join tbl_doctor t2 on t1.c_doctor = t2.counter
    ///where
    /// t1.lname like ?LN
    /// </example>
    public sealed class QuerySource : INamed
    {
        #region Inner classes
                /// <summary>
                /// Provides column definition in QuerySource
                /// </summary>
                public class ColumnDef : INamed
                {
                    internal ColumnDef(string name)
                    {
                        m_Name = name;

                        //Defaults
                        m_StoreFlag = StoreFlag.LoadAndStore;
                        m_Visible = true;
                    }

                    private string m_Name;
                    private bool m_Key;
                    private bool m_Required;
                    private bool m_Visible;

                    private StoreFlag m_StoreFlag;
                    private string m_BackendName;
                    private string m_Description;

                    public string Name { get{return m_Name;}}
                    public StoreFlag StoreFlag { get{ return m_StoreFlag;} internal set{ m_StoreFlag = value;}}
                    public bool Key { get{ return m_Key;} internal set{ m_Key = value;}}
                    public bool Required { get{ return m_Required;} internal set{ m_Required = value;}}
                    public bool Visible { get{ return m_Visible;} internal set{ m_Visible = value;}}
                    public string BackendName { get{ return m_BackendName ?? string.Empty;} internal set{ m_BackendName = value;}}
                    public string Description { get{ return m_Description ?? string.Empty;} internal set{ m_Description = value;}}

                }
        #endregion

        #region CONSTS

            public const string PRAGMA = "#pragma";
            public const string TABLE_MODIFY_SECTION = "modify";
            public const string COLUMN_KEY_SECTION = "key";
            public const string COLUMN_IGNORE_SECTION = "ignore";
            public const string COLUMN_LOAD_SECTION = "load";
            public const string COLUMN_STORE_SECTION = "store";
            public const string COLUMN_REQUIRED_SECTION = "required";
            public const string COLUMN_INVISIBLE_SECTION = "invisible";

            public const string COLUMN_ALIAS_PREFIX = "@";
            public const string COLUMN_DESCRIPTION_PREFIX = ".";

        #endregion

        #region .ctor
           public QuerySource(string name, string source)
           {
                if (name.IsNullOrWhiteSpace() || source.IsNullOrWhiteSpace())
                    throw new CRUDException(StringConsts.ARGUMENT_ERROR + "QuerySource.ctor(name | source = null|empty)");

                m_Name = name;
                m_OriginalSource = source;
                m_StatementSource = m_OriginalSource;
                m_Columns = new Registry<ColumnDef>();
                var lines = source.SplitLines();

                if (lines.Length<4) return;// pragma, 1 line, space, statement = 4

                if (lines[0]!=PRAGMA) return;

                m_HasPragma = true;

                var pragma = true;
                var sb = new StringBuilder();
                for(var i=1; i<lines.Length; i++)
                {
                    if (!pragma)
                    {
                        sb.AppendLine( lines[i] );
                        continue;
                    }

                    var line = lines[i].TrimStart();
                    if (line.Length==0)
                    {
                        pragma = false;
                        continue;
                    }
                    var ieq = line.IndexOf('=');
                    if (ieq<=0 || ieq==line.Length-1)
                       throw new CRUDException(StringConsts.CRUD_QUERY_SOURCE_PRAGMA_ERROR.Args(i, line, "syntax"));

                    var pname = line.Substring(0, ieq).Trim();
                    var pvalue = line.Substring(ieq+1);
                    parseLine(i, line, pname, pvalue);
                }
                m_StatementSource = sb.ToString();
           }
        #endregion

        #region Field

            private string m_Name;
            private string m_OriginalSource;
            private string m_StatementSource;

            private bool m_HasPragma;
            private string m_ModifyTarget;

            private Registry<ColumnDef> m_Columns;

        #endregion

        #region Properties


            /// <summary>
            /// Rerurns name of query source
            /// </summary>
            public string Name { get{ return m_Name;} }

            /// <summary>
            /// Returns original source of query including pragma text (if any)
            /// </summary>
            public string OriginalSource { get { return m_OriginalSource; } }

            /// <summary>
            /// Returns source of query excluding pragma text (if any was present in the original)
            /// </summary>
            public string StatementSource { get { return m_StatementSource; } }

            /// <summary>
            /// Returns true when #pragma was defined in source
            /// </summary>
            public bool HasPragma { get { return m_HasPragma; } }

            /// <summary>
            /// Returns true when pragma does not specify any modification target (table name to insert/update/delete against)
            /// </summary>
            public bool ReadOnly { get { return m_ModifyTarget.IsNullOrWhiteSpace() ; } }

            /// <summary>
            /// Returns modification target (table name to insert/update/delete against) if query is not read-only
            /// </summary>
            public string ModifyTarget { get{ return m_ModifyTarget ?? string.Empty;}}

            /// <summary>
            /// Returns column definitions defined by #pragma
            /// </summary>
            public IRegistry<ColumnDef> ColumnDefs { get{ return m_Columns;} }

        #endregion

        #region .pvt

            private void parseLine(int no, string line, string name, string value)
            {
                if (TABLE_MODIFY_SECTION.EqualsOrdIgnoreCase(name))
                {
                    m_ModifyTarget = value;
                    return;
                }

                if (COLUMN_KEY_SECTION.EqualsOrdIgnoreCase(name))
                {
                    var keys = value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var key in keys)
                        getDef(key).Key = true;

                    return;
                }

                if (COLUMN_REQUIRED_SECTION.EqualsOrdIgnoreCase(name))
                {
                    var keys = value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var key in keys)
                        getDef(key).Required = true;

                    return;
                }

                if (COLUMN_INVISIBLE_SECTION.EqualsOrdIgnoreCase(name))
                {
                    var keys = value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var key in keys)
                        getDef(key).Visible = false;

                    return;
                }

                if (COLUMN_LOAD_SECTION.EqualsOrdIgnoreCase(name))
                {
                    var columns = value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var column in columns)
                        getDef(column).StoreFlag = StoreFlag.OnlyLoad;
                    return;
                }

                if (COLUMN_STORE_SECTION.EqualsOrdIgnoreCase(name))
                {
                    var columns = value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var column in columns)
                        getDef(column).StoreFlag = StoreFlag.OnlyStore;
                    return;
                }

                if (COLUMN_IGNORE_SECTION.EqualsOrdIgnoreCase(name))
                {
                    var columns = value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var column in columns)
                        getDef(column).StoreFlag = StoreFlag.None;
                    return;
                }

                if (name.StartsWith(COLUMN_ALIAS_PREFIX))
                {
                    name = name.Substring(1);
                    getDef(name).BackendName = value.Trim();
                    return;
                }

                if (name.StartsWith(COLUMN_DESCRIPTION_PREFIX))
                {
                    name = name.Substring(1);
                    getDef(name).Description = value;
                    return;
                }

                throw new CRUDException(StringConsts.CRUD_QUERY_SOURCE_PRAGMA_ERROR.Args(no, line, StringConsts.CRUD_QUERY_SOURCE_PRAGMA_LINE_ERROR.Args(name, value)));
            }

            private ColumnDef getDef(string name)
            {
                var result = m_Columns[name];
                if (result==null)
                {
                     result = new ColumnDef(name);
                     m_Columns.Register(result);
                }
                return result;
            }

        #endregion
    }



}
