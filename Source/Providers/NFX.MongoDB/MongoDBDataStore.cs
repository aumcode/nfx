using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using NFX;
using NFX.Environment;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB
{
  /// <summary>
  /// Implements MongoDB general data store that supports CRUD operations.
  /// This class IS thread-safe load/save/delete operations
  /// </summary>
  public class MongoDBDataStore : MongoDBDataStoreBase, ICRUDDataStoreImplementation
  {
    #region CONSTS
        public const string SCRIPT_FILE_SUFFIX = ".mon.json";
    #endregion

    #region .ctor/.dctor

      public MongoDBDataStore() : base()
      {
        m_QueryResolver = new QueryResolver(this);
        m_Converter = new RowConverter();
      }

      public MongoDBDataStore(string connectString, string dbName) : base(connectString, dbName)
      {
        m_QueryResolver = new QueryResolver(this);
        m_Converter = new RowConverter();
      }

    #endregion

    #region Fields

        private QueryResolver m_QueryResolver;
        private RowConverter m_Converter;

    #endregion


    #region ICRUDDataStore

        //WARNING!!!
        //The ASYNC versions of sync call now call TaskUtils.AsCompletedTask( sync_version )
        // which executes synchronously. Because of it CRUDOperationCallContext does not need to be captured
        // and passed along the async task chain.
        // Keep in mind: In future implementations, the true ASYNC versions of methods would need to capture
        // CRUDOperationCallContext and pass it along the call chain



        public virtual CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                                TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
        {
            throw new MongoDBDataAccessException(StringConsts.OP_NOT_SUPPORTED_ERROR.Args("BeginTransaction", GetType().Name));
        }

        public virtual Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                                           TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
        {
            throw new MongoDBDataAccessException(StringConsts.OP_NOT_SUPPORTED_ERROR.Args("BeginTransactionAsync", GetType().Name));
        }

        public virtual bool SupportsTransactions
        {
            get { return false; }
        }

        public virtual bool SupportsTrueAsynchrony
        {
            get { return false; }
        }

        public virtual string ScriptFileSuffix
        {
            get { return SCRIPT_FILE_SUFFIX;}
        }

        public virtual CRUDDataStoreType StoreType
        {
            get { return CRUDDataStoreType.Document; }
        }


        public virtual Schema GetSchema(Query query)
        {
            if (query==null) return null;

            var db = GetDatabase();

            var handler = QueryResolver.Resolve(query);
            return handler.GetSchema( new MongoDBCRUDQueryExecutionContext(this, db), query);
        }

        public virtual Task<Schema> GetSchemaAsync(Query query)
        {
            return TaskUtils.AsCompletedTask( () => this.GetSchema(query) );
        }

        public virtual List<RowsetBase> Load(params Query[] queries)
        {
            var db = GetDatabase();

            var result = new List<RowsetBase>();
            if (queries==null) return result;

            foreach(var query in queries)
            {
               var handler = QueryResolver.Resolve(query);
               var rowset = handler.Execute( new MongoDBCRUDQueryExecutionContext(this, db), query, false);
               result.Add( rowset );
            }

            return result;
        }

        public virtual Task<List<RowsetBase>> LoadAsync(params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.Load(queries) );
        }

        public virtual RowsetBase LoadOneRowset(Query query)
        {
            return Load(query).FirstOrDefault();
        }

        public virtual Task<RowsetBase> LoadOneRowsetAsync(Query query)
        {
            return this.LoadAsync(query)
                       .ContinueWith( antecedent => antecedent.Result.FirstOrDefault());
        }

        public virtual Row LoadOneRow(Query query)
        {
            RowsetBase rset = null;
            rset = Load(query).FirstOrDefault();

            if (rset!=null) return rset.FirstOrDefault();
            return null;
        }

        public virtual Task<Row> LoadOneRowAsync(Query query)
        {
            return this.LoadAsync(query)
                       .ContinueWith( antecedent =>
                        {
                          RowsetBase rset = antecedent.Result.FirstOrDefault();
                          if (rset!=null) return rset.FirstOrDefault();
                          return null;
                        });
        }

        public virtual Cursor OpenCursor(Query query)
        {
           var db = GetDatabase();

           var handler = QueryResolver.Resolve(query);
           var context = new MongoDBCRUDQueryExecutionContext(this, db);
           var result = handler.OpenCursor( context, query);
           return result;
        }

        public virtual Task<Cursor> OpenCursorAsync(Query query)
        {
          return TaskUtils.AsCompletedTask( () => this.OpenCursor(query) );
        }


        public virtual int Save(params RowsetBase[] rowsets)
        {
           if (rowsets==null) return 0;

           var db = GetDatabase();

           var affected = 0;

           foreach(var rset in rowsets)
           {
                foreach(var change in rset.Changes)
                {
                    switch(change.ChangeType)
                    {
                        case RowChangeType.Insert: affected += DoInsert(db, change.Row); break;
                        case RowChangeType.Update: affected += DoUpdate(db, change.Row, change.Key); break;
                        case RowChangeType.Upsert: affected += DoUpsert(db, change.Row); break;
                        case RowChangeType.Delete: affected += DoDelete(db, change.Row, change.Key); break;
                    }
                }
           }

           return affected;
        }

        public virtual Task<int> SaveAsync(params RowsetBase[] rowsets)
        {
           return TaskUtils.AsCompletedTask( () => this.Save(rowsets) );
        }

        public virtual int Insert(Row row, FieldFilterFunc filter = null)
        {
            var db = GetDatabase();
            return DoInsert(db, row, filter);
        }

        public virtual Task<int> InsertAsync(Row row, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Insert(row, filter) );
        }

        public virtual int Upsert(Row row, FieldFilterFunc filter = null)
        {
            var db = GetDatabase();
            return DoUpsert(db, row, filter);
        }

        public virtual Task<int> UpsertAsync(Row row, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Upsert(row, filter) );
        }

        public virtual int Update(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
        {
            var db = GetDatabase();
            return DoUpdate(db, row, key, filter);
        }

        public virtual Task<int> UpdateAsync(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Update(row, key, filter) );
        }

        public int Delete(Row row, IDataStoreKey key = null)
        {
            var db = GetDatabase();
            return DoDelete(db, row, key);
        }

        public virtual Task<int> DeleteAsync(Row row, IDataStoreKey key = null)
        {
            return TaskUtils.AsCompletedTask( () => this.Delete(row, key) );
        }

        public virtual int ExecuteWithoutFetch(params Query[] queries)
        {
            if (queries==null) return 0;

            var db = GetDatabase();
            var affected = 0;

            foreach(var query in queries)
            {
               var handler = QueryResolver.Resolve(query);
               affected += handler.ExecuteWithoutFetch( new MongoDBCRUDQueryExecutionContext(this, db), query);
            }

            return affected;
        }

        public virtual Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.ExecuteWithoutFetch(queries) );
        }


        public ICRUDQueryHandler MakeScriptQueryHandler(QuerySource querySource)
        {
            return new MongoDBCRUDScriptQueryHandler(this, querySource);
        }


        public ICRUDQueryResolver QueryResolver
        {
            get { return m_QueryResolver; }
        }

    #endregion


    #region Protected

        protected internal RowConverter Converter{ get{return m_Converter;} }

        public override void Configure(IConfigSectionNode node)
        {
            m_QueryResolver.Configure(node);
            m_Converter.Configure(node);
            base.Configure(node);
        }

        protected internal string GetCollectionName(Schema schema)
        {
          string tableName = schema.Name;

          if (schema.TypedRowType!=null)
            tableName = schema.TypedRowType.Name;//without namespace

          var tableAttr = schema.GetTableAttrForTarget(TargetName);
          if (tableAttr!=null && tableAttr.Name.IsNotNullOrWhiteSpace()) tableName = tableAttr.Name;
          return tableName;
        }


        protected virtual int DoInsert(Connector.Database db, Row row, FieldFilterFunc filter = null)
        {
          var doc = convertRowToBSONDocumentWith_ID(row, "insert", filter);

          var tname = GetCollectionName(row.Schema);

          var collection = db[tname];

          var result = collection.Insert(doc);

          checkCRUDResult(result, row.Schema.Name, "insert");

          return result.TotalDocumentsAffected;
        }

        protected virtual int DoUpsert(Connector.Database db, Row row, FieldFilterFunc filter = null)
        {
          var doc = convertRowToBSONDocumentWith_ID(row, "upsert", filter);

          var tname = GetCollectionName(row.Schema);

          var collection = db[tname];

          var result = collection.Save(doc);

          checkCRUDResult(result, row.Schema.Name, "upsert");

          return result.TotalDocumentsAffected;
        }

        protected virtual int DoUpdate(Connector.Database db, Row row, IDataStoreKey key, FieldFilterFunc filter = null)
        {
          var doc = convertRowToBSONDocumentWith_ID(row, "update", filter);
          var _id = doc[Connector.Protocol._ID];

          //20160212 spol
          if (filter != null)
          {
            doc.Delete(Connector.Protocol._ID);
            if (doc.Count == 0) return 0; // nothing to update
            var wrapDoc = new BSONDocument();
            wrapDoc.Set(new BSONDocumentElement(Connector.Protocol.SET, doc));
            doc = wrapDoc;
          }

          var tname = GetCollectionName(row.Schema);

          var collection = db[tname];

          var qry = new Connector.Query();
          qry.Set( _id );
          var upd = new Connector.UpdateEntry(qry, doc, false, false);

          var result = collection.Update( upd );

          checkCRUDResult(result, row.Schema.Name, "update");

          return result.TotalDocumentsAffected;
        }

        protected virtual int DoDelete(Connector.Database db, Row row, IDataStoreKey key)
        {
          var doc = convertRowToBSONDocumentWith_ID(row, "delete");

          var tname = GetCollectionName(row.Schema);

          var collection = db[tname];

          var qry = new Connector.Query();
          qry.Set( doc[Connector.Protocol._ID] );

          var result = collection.Delete( new Connector.DeleteEntry( qry, Connector.DeleteLimit.OnlyFirstMatch) );

          checkCRUDResult(result, row.Schema.Name, "delete");

          return result.TotalDocumentsAffected;
        }


    #endregion


    #region .pvt

      private BSONDocument convertRowToBSONDocumentWith_ID(Row row, string operation, FieldFilterFunc filter = null)
      {
        var result = m_Converter.RowToBSONDocument(row, this.TargetName, filter: filter);

        if (result[Connector.Protocol._ID]==null)
         throw new MongoDBDataAccessException(StringConsts.OP_ROW_NO_PK_ID_ERROR.Args(row.Schema.Name, Connector.Protocol._ID, operation));

        return result;
      }

      private void checkCRUDResult(Connector.CRUDResult result, string schema, string operation)
      {
        if (result.WriteErrors==null ||
            result.WriteErrors.Length==0) return;

        var dump = NFX.Serialization.JSON.JSONWriter.Write(result.WriteErrors, Serialization.JSON.JSONWritingOptions.PrettyPrint);

        string kv = null;
        KeyViolationKind kvKind = KeyViolationKind.Unspecified;

        if (result.WriteErrors[0].Code==11000)
        {
          kv = result.WriteErrors[0].Message;
          kvKind =  kv.IndexOf("_id")>0 ? KeyViolationKind.Primary : KeyViolationKind.Secondary;
        }


        throw new MongoDBDataAccessException(StringConsts.OP_CRUD_ERROR.Args(operation, schema, dump), kvKind, kv);
      }


    #endregion


  }
}
