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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;


using NFX.Environment;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;

using MySql.Data.MySqlClient;




namespace NFX.DataAccess.MySQL
{
  /// <summary>
  /// Implements MySQL general data store that auto-generates SQLs for record models and supports CRUD operations.
  /// This class IS thread-safe load/save/delete operations
  /// </summary>
  public class MySQLDataStore : MySQLDataStoreBase, ICRUDDataStoreImplementation
  {
    #region CONSTS
        public const string SCRIPT_FILE_SUFFIX = ".mys.sql";
    #endregion
    
    #region .ctor/.dctor

      public MySQLDataStore() : base()
      {
        m_QueryResolver = new QueryResolver(this);
      }

      public MySQLDataStore(string connectString) : base(connectString)
      {
        m_QueryResolver = new QueryResolver(this);
      }

    #endregion

    #region Fields

        private QueryResolver m_QueryResolver;

    #endregion

    #region ICRUDDataStore
        
        //WARNING!!!
        //The ASYNC versions of sync call now call TaskUtils.AsCompletedTask( sync_version )
        // which executes synchronously. Because of it CRUDOperationCallContext does not need to be captured
        // and passed along the async task chain.
        // Keep in mind: In future implementations, the true ASYNC versions of methods would need to capture
        // CRUDOperationCallContext and pass it along the call chain


        
        public CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted, 
                                                TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
        {
            var cnn = GetConnection();
            return new MySQLCRUDTransaction(this, cnn, iso, behavior);
        }

        public Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                                           TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
        {
            return TaskUtils.AsCompletedTask( () => this.BeginTransaction(iso, behavior) );
        }

        public bool SupportsTransactions
        {
            get { return true; }
        }

        public bool SupportsTrueAsynchrony
        {
            get { return false; }
        }

        public string ScriptFileSuffix
        {
            get { return SCRIPT_FILE_SUFFIX;}
        }

        public CRUDDataStoreType StoreType
        {
            get { return CRUDDataStoreType.Relational; }
        }


        public Schema GetSchema(Query query)
        {
            using (var cnn = GetConnection())
              return DoGetSchema(cnn, null, query);
        }

        public Task<Schema> GetSchemaAsync(Query query)
        {
            return TaskUtils.AsCompletedTask( () => this.GetSchema(query) ); 
        }

        public List<RowsetBase> Load(params Query[] queries)
        {
            using (var cnn = GetConnection())
              return DoLoad(cnn, null, queries);
        }

        public Task<List<RowsetBase>> LoadAsync(params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.Load(queries) );
        }

        public RowsetBase LoadOneRowset(Query query)
        {
            return Load(query).FirstOrDefault();
        }

        public Task<RowsetBase> LoadOneRowsetAsync(Query query)
        {
            return this.LoadAsync(query)
                       .ContinueWith( antecedent => antecedent.Result.FirstOrDefault());
        }

        public Row LoadOneRow(Query query)
        {
            RowsetBase rset = null;
            using (var cnn = GetConnection())
              rset = DoLoad(cnn, null, new Query[]{query}, true).FirstOrDefault();

            if (rset!=null) return rset.FirstOrDefault();
            return null;
        }

        public Task<Row> LoadOneRowAsync(Query query)
        {
            return this.LoadAsync(query)
                       .ContinueWith( antecedent =>
                        {
                          RowsetBase rset = antecedent.Result.FirstOrDefault();
                          if (rset!=null) return rset.FirstOrDefault();
                          return null;
                        });
        }

        public Cursor OpenCursor(Query query)
        {
          Cursor result = null;
          var cnn = GetConnection();
          try
          {
            result = DoOpenCursor(cnn, null, query);
          }
          catch
          {
             DisposableObject.DisposeAndNull(ref cnn);
             throw;
          }

          return result;
        }
        
        public Task<Cursor> OpenCursorAsync(Query query)
        {
           return TaskUtils.AsCompletedTask( () => this.OpenCursor(query) );
        }


        public int Save(params RowsetBase[] rowsets)
        {
           using (var cnn = GetConnection()) 
              return DoSave(cnn,  null, rowsets);
        }

        public Task<int> SaveAsync(params RowsetBase[] rowsets)
        {
           return TaskUtils.AsCompletedTask( () => this.Save(rowsets) ); 
        }

        public int Insert(Row row, FieldFilterFunc filter = null)
        {
            using (var cnn = GetConnection())
              return DoInsert(cnn,  null, row, filter);
        }

        public Task<int> InsertAsync(Row row, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Insert(row, filter) ); 
        }

        public int Upsert(Row row, FieldFilterFunc filter = null)
        {
            using (var cnn = GetConnection())
              return DoUpsert(cnn,  null, row, filter);
        }

        public Task<int> UpsertAsync(Row row, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Upsert(row, filter) );
        }

        public int Update(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
        {
            using (var cnn = GetConnection())
              return DoUpdate(cnn,  null, row, key, filter);
        }

        public Task<int> UpdateAsync(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Update(row, key, filter) );
        }

        public int Delete(Row row, IDataStoreKey key = null)
        {
            using (var cnn = GetConnection())
              return DoDelete(cnn,  null, row, key);
        }

        public Task<int> DeleteAsync(Row row, IDataStoreKey key = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Delete(row, key) );
        }

        public int ExecuteWithoutFetch(params Query[] queries)
        {
            using (var cnn = GetConnection())
              return DoExecuteWithoutFetch(cnn, null, queries);
        }

        public Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.ExecuteWithoutFetch(queries) );
        }


        public ICRUDQueryHandler MakeScriptQueryHandler(QuerySource querySource)
        {
            return new MySQLCRUDScriptQueryHandler(this, querySource);
        }


        public ICRUDQueryResolver QueryResolver
        {
            get { return m_QueryResolver; }
        }

    #endregion



    #region Protected + Overrides
       
        public override void Configure(IConfigSectionNode node)
        {
            m_QueryResolver.Configure(node);
            base.Configure(node);
        }

        /// <summary>
        ///  Performs CRUD load without fetching data only returning schema. Override to do custom Query interpretation
        /// </summary>
        protected internal virtual Schema DoGetSchema(MySqlConnection cnn, MySqlTransaction transaction, Query query)
        {
            if (query==null) return null;

            var handler = QueryResolver.Resolve(query);
            return handler.GetSchema( new MySQLCRUDQueryExecutionContext(this, cnn, transaction), query);
        }


        /// <summary>
        ///  Performs CRUD load. Override to do custom Query interpretation
        /// </summary>
        protected internal virtual List<RowsetBase> DoLoad(MySqlConnection cnn, MySqlTransaction transaction, Query[] queries, bool oneRow = false)
        {
            var result = new List<RowsetBase>();
            if (queries==null) return result;

            foreach(var query in queries)
            {
               var handler = QueryResolver.Resolve(query);
               var rowset = handler.Execute( new MySQLCRUDQueryExecutionContext(this, cnn, transaction), query, oneRow);
               result.Add( rowset );
            }

            return result;
            
        }


        /// <summary>
        ///  Performs CRUD load. Override to do custom Query interpretation
        /// </summary>
        protected internal virtual Cursor DoOpenCursor(MySqlConnection cnn, MySqlTransaction transaction, Query query)
        {
            var context = new MySQLCRUDQueryExecutionContext(this, cnn, transaction);
            var handler = QueryResolver.Resolve(query);
            return handler.OpenCursor( context, query);
        }

        /// <summary>
        ///  Performs CRUD execution of queries that do not return result set. Override to do custom Query interpretation
        /// </summary>
        protected internal virtual int DoExecuteWithoutFetch(MySqlConnection cnn, MySqlTransaction transaction, Query[] queries)
        {
            if (queries==null) return 0;

            var affected = 0;

            foreach(var query in queries)
            {
               var handler = QueryResolver.Resolve(query);
               affected += handler.ExecuteWithoutFetch( new MySQLCRUDQueryExecutionContext(this, cnn, transaction), query);
            }

            return affected;
        }

        /// <summary>
        /// Performs CRUD batch save. Override to do custom batch saving
        /// </summary>
        protected internal virtual int DoSave(MySqlConnection cnn, MySqlTransaction transaction, RowsetBase[] rowsets)
        {
            if (rowsets==null) return 0;

            var affected = 0;

            foreach(var rset in rowsets)
            {
                foreach(var change in rset.Changes)
                {
                    switch(change.ChangeType)
                    {
                        case RowChangeType.Insert: affected += DoInsert(cnn, transaction, change.Row); break;
                        case RowChangeType.Update: affected += DoUpdate(cnn, transaction, change.Row, change.Key); break;
                        case RowChangeType.Upsert: affected += DoUpsert(cnn, transaction, change.Row); break;
                        case RowChangeType.Delete: affected += DoDelete(cnn, transaction, change.Row, change.Key); break;
                    }
                }
            }
            

            return affected;
        }

        /// <summary>
        /// Performs CRUD row insert. Override to do custom insertion
        /// </summary>
        protected internal virtual int DoInsert(MySqlConnection cnn, MySqlTransaction transaction, Row row, FieldFilterFunc filter = null)
        {
             checkReadOnly(row.Schema, "insert");
             return CRUDGenerator.CRUDInsert(this, cnn, transaction, row, filter);
        }

        /// <summary>
        /// Performs CRUD row upsert. Override to do custom upsertion
        /// </summary>
        protected internal virtual int DoUpsert(MySqlConnection cnn, MySqlTransaction transaction, Row row, FieldFilterFunc filter = null)
        {         
            checkReadOnly(row.Schema, "upsert");
            return CRUDGenerator.CRUDUpsert(this, cnn, transaction, row, filter);
        }

        /// <summary>
        /// Performs CRUD row update. Override to do custom update
        /// </summary>
        protected internal virtual int DoUpdate(MySqlConnection cnn, MySqlTransaction transaction, Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
        {
            checkReadOnly(row.Schema, "update");
            return CRUDGenerator.CRUDUpdate(this, cnn, transaction, row, key, filter);
        }

        /// <summary>
        /// Performs CRUD row deletion. Override to do custom deletion
        /// </summary>
        protected internal virtual int DoDelete(MySqlConnection cnn, MySqlTransaction transaction, Row row, IDataStoreKey key = null)
        {
            checkReadOnly(row.Schema, "delete");
            return CRUDGenerator.CRUDDelete(this, cnn, transaction, row, key);
        }


    #endregion

    #region .pvt

        private void checkReadOnly(Schema schema, string operation)
        {
            if (schema.ReadOnly)
                throw new CRUDException(StringConsts.CRUD_READONLY_SCHEMA_ERROR.Args(schema.Name, operation));
        }


    #endregion

  }
}
