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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

using NFX;
using NFX.DataAccess.CRUD;

namespace NFX.DataAccess.MySQL
{
    /// <summary>
    /// Represents MySQL CRUD transaction
    /// </summary>
    public sealed class MySQLCRUDTransaction : CRUDTransaction
    {
        #region .ctor/.dctor

            internal MySQLCRUDTransaction(MySQLDataStore store, MySqlConnection cnn, IsolationLevel iso, TransactionDisposeBehavior disposeBehavior) : base (store, disposeBehavior)
            {
                m_Connection = cnn;
                m_Transaction = cnn.BeginTransaction(iso); 
            }

            protected override void Destructor()
            {
                base.Destructor();
                m_Connection.Dispose();
            }

        #endregion

        #region Fields

            private MySqlConnection m_Connection;
            private MySqlTransaction m_Transaction;

        #endregion  
        
        #region Properties

            internal MySQLDataStore Store
            {
                get { return m_Store as MySQLDataStore; }
            }

            /// <summary>
            /// Returns the underlying MySQL connection that this transaction works through
            /// </summary>
            public MySqlConnection Connection { get {return m_Connection;} }

            /// <summary>
            /// Returns the underlying MySQL transaction that this instance represents. Do not call Commit/Rollback method on this property directly
            /// </summary>
            public MySqlTransaction Transaction { get {return m_Transaction;} }
            

        #endregion
        

        protected override Schema DoGetSchema(Query query)
        {
            return Store.DoGetSchema(m_Connection, m_Transaction, query);
        }

        protected override Task<Schema> DoGetSchemaAsync(Query query)
        {
            return TaskUtils.AsCompletedTask( () => this.DoGetSchema(query) );
        }
        
        protected override List<RowsetBase> DoLoad(bool oneRow, params Query[] queries)
        {
            return Store.DoLoad(m_Connection, m_Transaction, queries, oneRow);
        }

        protected override Task<List<RowsetBase>> DoLoadAsync(bool oneRow, params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.DoLoad(oneRow, queries) );
        }

        protected override Cursor DoOpenCursor(Query query)
        {
            return Store.DoOpenCursor(m_Connection, m_Transaction, query);
        }

        protected override Task<Cursor> DoOpenCursorAsync(Query query)
        {
            return TaskUtils.AsCompletedTask( () => this.DoOpenCursor(query) );
        }

        protected override int DoExecuteWithoutFetch(params Query[] queries)
        {
            return Store.DoExecuteWithoutFetch(m_Connection, m_Transaction, queries);
        }

        protected override Task<int> DoExecuteWithoutFetchAsync(params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.DoExecuteWithoutFetch(queries) );
        }

        protected override int DoSave(params RowsetBase[] rowsets)
        {
            return Store.DoSave(m_Connection, m_Transaction, rowsets);
        }

        protected override Task<int> DoSaveAsync(params RowsetBase[] rowsets)
        {
            return TaskUtils.AsCompletedTask( () => this.DoSave(rowsets) );
        }

        protected override int DoInsert(Row row, FieldFilterFunc filter = null)
        {
            return Store.DoInsert(m_Connection, m_Transaction, row, filter);
        }

        protected override Task<int> DoInsertAsync(Row row, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.DoInsert(row, filter) );
        }

        protected override int DoUpsert(Row row, FieldFilterFunc filter = null)
        {
            return Store.DoUpsert(m_Connection, m_Transaction, row, filter);
        }

        protected override Task<int> DoUpsertAsync(Row row, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.DoUpsert(row, filter) );
        }

        protected override int DoUpdate(Row row, IDataStoreKey key, FieldFilterFunc filter = null)
        {
            return Store.DoUpdate(m_Connection, m_Transaction, row, key, filter);
        }

        protected override Task<int> DoUpdateAsync(Row row, IDataStoreKey key, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.DoUpdate(row, key, filter) );
        }

        protected override int DoDelete(Row row, IDataStoreKey key)
        {
            return Store.DoDelete(m_Connection, m_Transaction, row, key);
        }

        protected override Task<int> DoDeleteAsync(Row row, IDataStoreKey key)
        {
            return TaskUtils.AsCompletedTask( () => this.DoDelete(row, key) );
        }

        protected override void DoCommit()
        {
            m_Transaction.Commit();
        }

        protected override void DoRollback()
        {
            m_Transaction.Rollback();
        }
    }
}
