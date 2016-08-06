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

using NFX.DataAccess.CRUD;
using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB
{
    /// <summary>
    /// A base for ICRUDQueryHandler-derivatives for mongo
    /// </summary>
    public abstract class MongoDBCRUDQueryHandlerBase : ICRUDQueryHandler
    {
        #region .ctor
            public MongoDBCRUDQueryHandlerBase(MongoDBDataStore store, QuerySource source)
            {
                m_Store = store;
                m_Source = source;
            }
        #endregion

        #region Fields
            protected MongoDBDataStore m_Store;
            protected QuerySource m_Source;
        #endregion

        #region ICRUDQueryHandler
            public string Name
            {
                get { return m_Source.Name; }
            }

            public ICRUDDataStore Store
            {
               get { return m_Store;}
            }


            public virtual Schema GetSchema(ICRUDQueryExecutionContext context, Query query)
            {
              throw new NotImplementedException();
            }

            public virtual Task<Schema> GetSchemaAsync(ICRUDQueryExecutionContext context, Query query)
            {
              return TaskUtils.AsCompletedTask( () => this.GetSchema(context, query));
            }


            public virtual RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
            {
              throw new NotImplementedException();
            }

            public virtual Task<RowsetBase> ExecuteAsync(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
            {
              return TaskUtils.AsCompletedTask( () => this.Execute(context, query, oneRow));
            }


            public virtual Cursor OpenCursor(ICRUDQueryExecutionContext context, Query query)
            {
              throw new NotImplementedException();
            }

            public virtual Task<Cursor> OpenCursorAsync(ICRUDQueryExecutionContext context, Query query)
            {
              return TaskUtils.AsCompletedTask( () => this.OpenCursor(context, query) );
            }

            public virtual int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query)
            {
              throw new NotImplementedException();
            }

            public virtual Task<int> ExecuteWithoutFetchAsync(ICRUDQueryExecutionContext context, Query query)
            {
              return TaskUtils.AsCompletedTask( () => this.ExecuteWithoutFetch(context, query));
            }
        #endregion


        #region protected utility

            protected  Connector.Query MakeQuery(Connector.Database db, Query query, out Connector.Collection collection)
            {
              if (m_Source.ModifyTarget.IsNullOrWhiteSpace())
                 throw new MongoDBDataAccessException(StringConsts.QUERY_MODIFY_TARGET_MISSING_ERROR + "\n" + m_Source.OriginalSource);

              collection = db[m_Source.ModifyTarget];

              return MakeQuery(query);
            }

            protected  Connector.Query MakeQuery(Query query)
            {
              var args = query.Select( p =>
                                      {
                                        var elm = m_Store.Converter.ConvertCLRtoBSON(p.Name, p.Value, m_Store.TargetName);
                                        return new TemplateArg(p.Name, elm.ElementType, elm.ObjectValue);
                                      }).ToArray();

              return new Connector.Query(m_Source.StatementSource, true, args);
            }


        #endregion

    }





}
