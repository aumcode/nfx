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
    /// Represents a type-safe row of data when schema is known at compile-time.
    /// Typed rows store data in instance fields, providing better performance and schema definition compile-time checking than DynamicRows
    /// at the expense of inability to define schema at runtime
    /// </summary>
    [Serializable]
    public abstract class TypedRow : Row
    {
        #region .ctor
            protected TypedRow()
            {
            }
        #endregion

        #region Fields

            [NonSerialized]
            private Schema m_CachedSchema;

        #endregion

        #region Properties

            /// <summary>
            /// References a schema for a table that this row is a part of
            /// </summary>
            public sealed override Schema Schema
            {
                get
                {
                    if (m_CachedSchema==null)
                        m_CachedSchema = Schema.GetForTypedRow(this);

                    return m_CachedSchema;
                }
            }

        #endregion

        #region Public

            public override object GetFieldValue(Schema.FieldDef fdef)
            {
                var pinf = fdef.MemberInfo;
                var result = pinf.GetValue(this, null);

                if (result==DBNull.Value) result = null;

                return result;
            }

            public override void SetFieldValue(Schema.FieldDef fdef, object value)
            {
                var pinf = fdef.MemberInfo;

                value = ConvertFieldValueToDef(fdef, value);

                pinf.SetValue(this, value, null);
            }

        #endregion

    }


    /// <summary>
    /// Represents a type-safe row of data when schema is known at compile-time that also implements IAmorphousData
    /// interface that allows this row to store "extra" data that does not comply with the current schema.
    /// Typed rows store data in instance fields, providing better performance and schema definition compile-time checking than DynamicRows
    /// at the expense of inability to define schema at runtime
    /// </summary>
    [Serializable]
    public abstract class AmorphousTypedRow : TypedRow, IAmorphousData
    {
        #region .ctor
            protected AmorphousTypedRow()
            {
            }
        #endregion

        #region Fields

            private Dictionary<string, object> m_AmorphousData;

        #endregion

        #region Properties

            /// <summary>
            /// True by default for rows
            /// </summary>
            public virtual bool AmorphousDataEnabled { get{return true;}}

            /// <summary>
            /// Returns data that does not comply with known schema (dynamic data). The field names are NOT case-sensitive
            /// </summary>
            public IDictionary<string, object> AmorphousData
            {
              get
              {
                if (m_AmorphousData==null)
                  m_AmorphousData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                return m_AmorphousData;
              }
            }

        #endregion


        #region Public
            /// <summary>
            /// Invoked to allow the row to transform its state into AmorphousData bag.
            /// For example, this may be usefull to store extra data that is not a part of established business schema.
            /// The operation is performed per particular targetName (name of physical backend). Simply put, this method allows
            ///  business code to "specify what to do before object gets saved in THE PARTICULAR TARGET backend store"
            /// </summary>
            public virtual void BeforeSave(string targetName)
            {

            }

            /// <summary>
            /// Invoked to allow the row to hydrate its fields/state from AmorphousData bag.
            /// For example, this may be used to reconstruct some temporary object state that is not stored as a part of established business schema.
            /// The operation is performed per particular targetName (name of physical backend).
            /// Simply put, this method allows business code to "specify what to do after object gets loaded from THE PARTICULAR TARGET backend store".
            /// An example: suppose current MongoDB collection stores 3 fields for name, and we want to collapse First/Last/Middle name fields into one field.
            /// If we change rowschema then it will only contain 1 field which is not present in the database, however those 'older' fields will get populated
            /// into AmorphousData giving us an option to merge older 3 fields into 1 within AfterLoad() implementation
            /// </summary>
            public virtual void AfterLoad(string targetName)
            {

            }
        #endregion
    }





}
