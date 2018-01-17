/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
  /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be safe
  /// </summary>
  public abstract class CRUDQueryHandler : INamed
  {

    protected CRUDQueryHandler(ICRUDDataStore store, string name)
    {
      if (store == null)
        throw new CRUDException(StringConsts.ARGUMENT_ERROR + "CRUDQueryHandler.ctor(store == null)");
      if (name.IsNullOrWhiteSpace())
        throw new CRUDException(StringConsts.ARGUMENT_ERROR + "CRUDQueryHandler.ctor(name.IsNullOrWhiteSpace())");
      m_Store = store;
      m_Name = name;
    }

    protected CRUDQueryHandler(ICRUDDataStore store, QuerySource source) : this(store, source.NonNull().Name)
    {
      Source = source;
    }

    private string m_Name;
    private ICRUDDataStore m_Store;
    protected readonly QuerySource Source;

    /// <summary>
    /// Returns query name that this handler handles
    /// </summary>
    public string Name { get { return m_Name; } }

    /// <summary>
    /// Store instance that handler is under
    /// </summary>
    public ICRUDDataStore Store { get { return m_Store; } }

    /// <summary>
    /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Schema GetSchema(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<Schema> GetSchemaAsync(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false);

    /// <summary>
    /// Executes query. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<RowsetBase> ExecuteAsync(ICRUDQueryExecutionContext context, Query query, bool oneRow = false);

    /// <summary>
    /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Cursor OpenCursor(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<Cursor> OpenCursorAsync(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query that dows not return results. The implementation may be called by multiple threads and must be safe.
    /// Returns rows affected
    /// </summary>
    public abstract int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query that dows not return results. The implementation may be called by multiple threads and must be safe.
    /// Returns rows affected
    /// </summary>
    public abstract Task<int> ExecuteWithoutFetchAsync(ICRUDQueryExecutionContext context, Query query);
  }

  /// <summary>
  /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be safe
  /// </summary>
  public abstract class CRUDQueryHandler<TStore> : CRUDQueryHandler where TStore : ICRUDDataStore
  {
    protected CRUDQueryHandler(TStore store, string name) : base(store, name) {}
    protected CRUDQueryHandler(TStore store, QuerySource source) : base(store, source) { }

    /// <summary>
    /// Store instance that handler is under
    /// </summary>
    public new TStore Store { get { return (TStore)base.Store; } }
  }
}
