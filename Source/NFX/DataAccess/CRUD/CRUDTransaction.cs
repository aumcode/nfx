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
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD
{

    /// <summary>
    /// Specifies how transaction scope behaves on scope exit
    /// </summary>
    public enum TransactionDisposeBehavior
    {
        CommitOnDispose = 0,
        RollbackOnDispose
    }

    /// <summary>
    /// Denotes transaction statuses
    /// </summary>
    public enum TransactionStatus
    {
        Open = 0,
        Comitted,
        RolledBack
    }

    /// <summary>
    /// Represents an abstract base for CRUDTransactions that perform particular backend CRUD work in overriden classes
    /// </summary>
    public abstract class CRUDTransaction : DisposableObject, ICRUDOperations
    {
        #region .ctor/.dctor

            protected CRUDTransaction(
                 ICRUDDataStoreImplementation store,
                 TransactionDisposeBehavior disposeBehavior = TransactionDisposeBehavior.CommitOnDispose)
            {
                m_Store = store;
                m_DisposeBahavior = disposeBehavior;
            }


            protected override void Destructor()
            {
               if (m_Status==TransactionStatus.Open)
               {
                   if (m_DisposeBahavior==TransactionDisposeBehavior.CommitOnDispose)
                      Commit();
                   else
                      Rollback();
               }
            }

        #endregion

        #region Fields

            protected ICRUDDataStoreImplementation m_Store;
            private TransactionStatus m_Status;
            private TransactionDisposeBehavior m_DisposeBahavior;

        #endregion


        #region Properties


            /// <summary>
            /// References the store instance that started this transaction
            /// </summary>
            public ICRUDDataStore DataStore
            {
                get { return m_Store;}
            }

            /// <summary>
            /// Returns current transaction status
            /// </summary>
            public TransactionStatus Status { get{ return m_Status; } }

            /// <summary>
            /// Specifies how transaction should be finalized on dispose: comitted or rolledback if it is still open
            /// </summary>
            public TransactionDisposeBehavior DisposeBehavior { get{ return m_DisposeBahavior; } }


            /// <summary>
            /// Returns true when backend supports true asynchronous operations, such as the ones that do not create extra threads/empty tasks
            /// </summary>
            public bool SupportsTrueAsynchrony{ get{ return m_Store.SupportsTrueAsynchrony;}}

        #endregion

        #region Public

            #region ICRUDOperations

                public Schema GetSchema(Query query)
                {
                   CheckOpenStatus("GetSchema");
                   return DoGetSchema(query);
                }

                public Task<Schema> GetSchemaAsync(Query query)
                {
                   CheckOpenStatus("GetSchema");
                   return DoGetSchemaAsync(query);
                }

                public List<RowsetBase> Load(params Query[] queries)
                {
                    CheckOpenStatus("Load");
                    return DoLoad(false, queries);
                }

                public Task<List<RowsetBase>> LoadAsync(params Query[] queries)
                {
                    CheckOpenStatus("Load");
                    return DoLoadAsync(false, queries);
                }


                public int ExecuteWithoutFetch(params Query[] queries)
                {
                    CheckOpenStatus("ExecuteWithoutFetch");
                    return DoExecuteWithoutFetch(queries);
                }

                public Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
                {
                    CheckOpenStatus("ExecuteWithoutFetch");
                    return DoExecuteWithoutFetchAsync(queries);
                }

                public RowsetBase LoadOneRowset(Query query)
                {
                    return Load(query).FirstOrDefault();
                }

                public Task<RowsetBase> LoadOneRowsetAsync(Query query)
                {
                    return LoadAsync(query).ContinueWith( antecedent => antecedent.Result.FirstOrDefault());
                }

                public Row LoadOneRow(Query query)
                {
                    RowsetBase rset = DoLoad(true, query).FirstOrDefault();
                    if (rset!=null) return rset.FirstOrDefault();
                    return null;
                }

                public Task<Row> LoadOneRowAsync(Query query)
                {
                    return DoLoadAsync(true, query).ContinueWith(
                         antecedent =>
                         {
                           var rset = antecedent.Result.FirstOrDefault();
                           if (rset!=null) return rset.FirstOrDefault();
                            return null;
                         });
                }

                public Cursor OpenCursor(Query query)
                {
                   return DoOpenCursor(query);
                }

                public Task<Cursor> OpenCursorAsync(Query query)
                {
                   return DoOpenCursorAsync(query);
                }

                public int Save(params RowsetBase[] tables)
                {
                    CheckOpenStatus("Save");
                    return DoSave(tables);
                }

                public Task<int> SaveAsync(params RowsetBase[] tables)
                {
                    CheckOpenStatus("Save");
                    return DoSaveAsync(tables);
                }

                public int Insert(Row row, FieldFilterFunc filter = null)
                {
                    CheckOpenStatus("Insert");
                    return DoInsert(row, filter);
                }

                public Task<int> InsertAsync(Row row, FieldFilterFunc filter = null)
                {
                    CheckOpenStatus("Insert");
                    return DoInsertAsync(row, filter);
                }


                public int Upsert(Row row, FieldFilterFunc filter = null)
                {
                    CheckOpenStatus("Upsert");
                    return DoUpsert(row, filter);
                }

                public Task<int> UpsertAsync(Row row, FieldFilterFunc filter = null)
                {
                    CheckOpenStatus("Upsert");
                    return DoUpsertAsync(row, filter);
                }


                public int Update(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
                {
                    CheckOpenStatus("Update");
                    return DoUpdate(row, key, filter);
                }

                public Task<int> UpdateAsync(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
                {
                    CheckOpenStatus("Update");
                    return DoUpdateAsync(row, key, filter);
                }


                public int Delete(Row row, IDataStoreKey key = null)
                {
                    CheckOpenStatus("Delete");
                    return DoDelete(row, key);
                }

                public Task<int> DeleteAsync(Row row, IDataStoreKey key = null)
                {
                    CheckOpenStatus("Delete");
                    return DoDeleteAsync(row, key);
                }


            #endregion

            public void Commit()
            {
                CheckOpenStatus("Commit");
                DoCommit();
                m_Status = TransactionStatus.Comitted;
            }

            public void Rollback()
            {
                CheckOpenStatus("Rollback");
                DoRollback();
                m_Status = TransactionStatus.RolledBack;
            }
        #endregion

        #region Protected


            protected void CheckOpenStatus(string operation)
            {
                if (m_Status!=TransactionStatus.Open)
                    throw new CRUDException(StringConsts.CRUD_TRANSACTION_IS_NOT_OPEN_ERROR.Args(operation, m_Status));
            }


            protected abstract Schema DoGetSchema(Query query);
            protected abstract Task<Schema> DoGetSchemaAsync(Query query);

            protected abstract List<RowsetBase> DoLoad(bool oneRow, params Query[] queries);
            protected abstract Task<List<RowsetBase>> DoLoadAsync(bool oneRow, params Query[] queries);

            protected abstract Cursor DoOpenCursor(Query query);
            protected abstract Task<Cursor> DoOpenCursorAsync(Query query);

            protected abstract int DoExecuteWithoutFetch(params Query[] queries);
            protected abstract Task<int> DoExecuteWithoutFetchAsync(params Query[] queries);

            protected abstract int  DoSave(params RowsetBase[] tables);
            protected abstract Task<int>  DoSaveAsync(params RowsetBase[] tables);

            protected abstract int  DoInsert(Row row, FieldFilterFunc filter = null);
            protected abstract Task<int>  DoInsertAsync(Row row, FieldFilterFunc filter = null);

            protected abstract int  DoUpsert(Row row, FieldFilterFunc filter = null);
            protected abstract Task<int>  DoUpsertAsync(Row row, FieldFilterFunc filter = null);

            protected abstract int  DoUpdate(Row row, IDataStoreKey key, FieldFilterFunc filter = null);
            protected abstract Task<int>  DoUpdateAsync(Row row, IDataStoreKey key, FieldFilterFunc filter = null);

            protected abstract int  DoDelete(Row row, IDataStoreKey key);
            protected abstract Task<int>  DoDeleteAsync(Row row, IDataStoreKey key);

            protected abstract void  DoCommit();

            protected abstract void  DoRollback();

        #endregion
    }
}
