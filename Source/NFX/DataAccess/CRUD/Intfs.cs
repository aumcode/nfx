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

using NFX.Environment;

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Denotes types of CRUD stores
    /// </summary>
    public enum CRUDDataStoreType
    {
        Relational,
        Document,
        KeyValue,
        Hybrid
    }


    /// <summary>
    /// Provides filter predicate for CRUD operations. Return true to include the specified field
    /// </summary>
    /// <param name="row">Row instance that filetring is performed on</param>
    /// <param name="key">If not null, the override key passed to Update() (if any)</param>
    /// <param name="fdef">A field that filtering is done for</param>
    public delegate bool FieldFilterFunc(Row row, IDataStoreKey key, Schema.FieldDef fdef);

    /// <summary>
    /// Describes an entity that performs single (not in transaction/batch)CRUD operations
    /// </summary>
    public interface ICRUDOperations
    {
        /// <summary>
        /// Returns true when backend supports true asynchronous operations, such as the ones that do not create extra threads/empty tasks
        /// </summary>
        bool SupportsTrueAsynchrony { get;}

        Schema GetSchema(Query query);
        Task<Schema> GetSchemaAsync(Query query);

        List<RowsetBase> Load(params Query[] queries);
        Task<List<RowsetBase>> LoadAsync(params Query[] queries);

        RowsetBase LoadOneRowset(Query query);
        Task<RowsetBase> LoadOneRowsetAsync(Query query);

        Row        LoadOneRow(Query query);
        Task<Row>  LoadOneRowAsync(Query query);

        Cursor OpenCursor(Query query);
        Task<Cursor> OpenCursorAsync(Query query);

        int Save(params RowsetBase[] rowsets);
        Task<int> SaveAsync(params RowsetBase[] rowsets);

        int ExecuteWithoutFetch(params Query[] queries);
        Task<int> ExecuteWithoutFetchAsync(params Query[] queries);

        int Insert(Row row, FieldFilterFunc filter = null);
        Task<int> InsertAsync(Row row, FieldFilterFunc filter = null);

        int Upsert(Row row, FieldFilterFunc filter = null);
        Task<int> UpsertAsync(Row row, FieldFilterFunc filter = null);

        int Update(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null);
        Task<int> UpdateAsync(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null);

        int Delete(Row row, IDataStoreKey key = null);
        Task<int> DeleteAsync(Row row, IDataStoreKey key = null);
    }

    /// <summary>
    /// Describes an entity that performs single (not in transaction/batch)CRUD operations
    /// </summary>
    public interface ICRUDTransactionOperations
    {
       /// <summary>
        /// Returns true when backend supports transactions. Even if false returned, CRUDDatastore supports CRUDTransaction return from BeginTransaction()
        ///  in which case statements may not be sent to destination until a call to Commit()
        /// </summary>
        bool SupportsTransactions { get;}

       /// <summary>
       /// Returns a transaction object for backend. Even if backend does not support transactions internally, CRUDTransactions save changes
       ///  into the store on commit only
       /// </summary>
       CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose);

       /// <summary>
       /// Returns a transaction object for backend. Even if backend does not support transactions internally, CRUDTransactions save changes
       ///  into the store on commit only
       /// </summary>
       Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose);
    }


    /// <summary>
    /// Represents a DataStore that supports CRUD operations
    /// </summary>
    public interface ICRUDDataStore : ICRUDOperations, ICRUDTransactionOperations
    {
        /// <summary>
        /// Returns default script file suffix, which some providers may use to locate script files
        ///  i.e. for MySql:  ".my.sql" which gets added to script files like so:  name.[suffix].[script ext (i.e. sql)].
        /// This name should uniquely identify the provider
        /// </summary>
        string ScriptFileSuffix { get; }

        /// <summary>
        /// Provides classification for the underlying store
        /// </summary>
        CRUDDataStoreType StoreType { get;}

        /// <summary>
        /// Reolver that turns query into handler
        /// </summary>
        ICRUDQueryResolver QueryResolver { get; }
    }


    public interface ICRUDDataStoreImplementation : ICRUDDataStore, IDataStoreImplementation
    {
        ICRUDQueryHandler MakeScriptQueryHandler(QuerySource querySource);
    }

    /// <summary>
    /// Represents a class that resolves Query into suitable handler that can execute it
    /// </summary>
    public interface ICRUDQueryResolver : IConfigurable
    {
        /// <summary>
        /// Retrieves a handler for supplied query. The implementation must be thread-safe
        /// </summary>
        ICRUDQueryHandler Resolve(Query query);

        string ScriptAssembly { get; set; }

        IList<string> HandlerLocations { get; }

        IRegistry<ICRUDQueryHandler> Handlers { get; }

        /// <summary>
        /// Registers handler location.
        /// The Resolver must be not started yet. This method is NOT thread safe
        /// </summary>
        void RegisterHandlerLocation(string location);

        /// <summary>
        /// Unregisters handler location returning true if it was found and removed.
        /// The Resolve must be not started yet. This method is NOT thread safe
        /// </summary>
        bool UnregisterHandlerLocation(string location);

    }

    /// <summary>
    /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public interface ICRUDQueryHandler : INamed
    {
        /// <summary>
        /// Store instance that handler is under
        /// </summary>
        ICRUDDataStore Store { get;}


        /// <summary>
        /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
        /// </summary>
        Schema GetSchema(ICRUDQueryExecutionContext context, Query query);

        /// <summary>
        /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
        /// </summary>
        Task<Schema> GetSchemaAsync(ICRUDQueryExecutionContext context, Query query);

        /// <summary>
        /// Executes query. The implementation may be called by multiple threads and must be safe
        /// </summary>
        RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false);

        /// <summary>
        /// Executes query. The implementation may be called by multiple threads and must be safe
        /// </summary>
        Task<RowsetBase> ExecuteAsync(ICRUDQueryExecutionContext context, Query query, bool oneRow = false);

        /// <summary>
        /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
        /// </summary>
        Cursor OpenCursor(ICRUDQueryExecutionContext context, Query query);

        /// <summary>
        /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
        /// </summary>
        Task<Cursor> OpenCursorAsync(ICRUDQueryExecutionContext context, Query query);

        /// <summary>
        /// Executes query that dows not return results. The implementation may be called by multiple threads and must be safe.
        /// Returns rows affected
        /// </summary>
        int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query);

        /// <summary>
        /// Executes query that dows not return results. The implementation may be called by multiple threads and must be safe.
        /// Returns rows affected
        /// </summary>
        Task<int> ExecuteWithoutFetchAsync(ICRUDQueryExecutionContext context, Query query);
    }

    /// <summary>
    /// Represents a context (such as Sql Server connection + transaction scope, or Hadoop connect string etc.) for query execution.
    /// This is a marker interface implemented by particular providers
    /// </summary>
    public interface ICRUDQueryExecutionContext
    {

    }

    /// <summary>
    /// Denotes an entity that supports validation
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validates entity state per particular named target, for performance reasons returns validation exception (instead of throwing)
        /// </summary>
        Exception Validate(string targetName);
    }


    /// <summary>
    /// Denotes an entity, which is typically a row-derivative, that has extra data fields that are not
    /// defined by particular schema and get represented as {name:value} map instead (schema-less data).
    /// This interface is usually implemented by rows that support version changing between releases, i.e. when
    /// structured storage (such as Mongo DB) stores more fields than are currently declared in the row the extra fields will be placed
    ///  in the AmorphousData collection. This interface also provides hook BeforeSave()/AfterLoad() that allow for transforms between
    ///  Amorphous and "hard-schema" data models
    /// </summary>
    public interface IAmorphousData
    {
        /// <summary>
        /// When true, enabled amorphous data behaviour, i.e. copying of amorphous data between rows.
        /// When false, the amorphous data is ignored as-if the type did not implement this interface
        /// This is needed for security, i.e. on the web returning false will prevent injection via posted forms
        /// </summary>
        bool AmorphousDataEnabled { get;}


        /// <summary>
        /// Returns data that does not comply with known schema (dynamic data).
        /// The field names are NOT case-sensitive
        /// </summary>
        IDictionary<string, object> AmorphousData{ get;}

        /// <summary>
        /// Invoked to allow the entity (such as a row) to transform its state into AmorphousData bag.
        /// For example, this may be usefull to store extra data that is not a part of established business schema.
        /// The operation is performed per particular targetName (name of physical backend). Simply put, this method allows
        ///  business code to "specify what to do before object gets saved in THE PARTICULAR TARGET backend store"
        /// </summary>
        void BeforeSave(string targetName);

        /// <summary>
        /// Invoked to allow the entity (such as a row) to hydrate its fields/state from AmorphousData bag.
        /// For example, this may be used to reconstruct some temporary object state that is not stored as a part of established business schema.
        /// The operation is performed per particular targetName (name of physical backend).
        /// Simply put, this method allows business code to "specify what to do after object gets loaded from THE PARTICULAR TARGET backend store".
        /// An example: suppose current MongoDB collection stores 3 fields for name, and we want to collapse First/Last/Middle name fields into one field.
        /// If we change rowschema then it will only contain 1 field which is not present in the database, however those 'older' fields will get populated
        /// into AmorphousData giving us an option to merge older 3 fields into 1 within AfterLoad() implementation
        /// </summary>
        void AfterLoad(string targetName);
    }



    /// <summary>
    /// Supplies caching params
    /// </summary>
    public interface ICacheParams
    {
        /// <summary>
        /// If greater than 0 then would allow reading a cached result for up-to the specified number of seconds.
        /// If =0 uses cache's default span.
        /// Less than 0 does not try to read from cache
        /// </summary>
        int ReadCacheMaxAgeSec{ get; }

        /// <summary>
        /// If greater than 0 then writes to cache with the expiration.
        /// If =0 uses cache's default life span.
        /// Less than 0 does not write to cache
        /// </summary>
        int WriteCacheMaxAgeSec{ get; }

        /// <summary>
        /// Relative cache priority which is used when WriteCacheMaxAgeSec>=0
        /// </summary>
        int WriteCachePriority{ get; }

        /// <summary>
        /// When true would cache the instance of AbsentData to signify the absence of data in the backend for key
        /// </summary>
        bool CacheAbsentData { get;}
    }

}
