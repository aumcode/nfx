/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
    /// Executes MongoDB CRUD script-based queries
    /// </summary>
    public sealed class MongoDBCRUDScriptQueryHandler : MongoDBCRUDQueryHandlerBase
    {
        public const string QUERY_PARAM_SKIP_COUNT  = "__SKIP_COUNT";
        public const string QUERY_PARAM_FETCH_BY    = "__FETCH_BY";
        public const string QUERY_PARAM_FETCH_LIMIT = "__FETCH_LIMIT";

        public MongoDBCRUDScriptQueryHandler(MongoDBDataStore store, QuerySource source) : base(store, source)
        {
        }


        public override Schema GetSchema(ICRUDQueryExecutionContext context, Query query)
        {
          var ctx = (MongoDBCRUDQueryExecutionContext)context;

          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, Source, out collection );

          var doc = collection.FindOne(qry);
          if (doc==null) return null;

          return Store.Converter.InferSchemaFromBSONDocument(doc);
        }


        public override RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
        {
          var ctx = (MongoDBCRUDQueryExecutionContext)context;

          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, Source, out collection );

          Schema schema = null;
          var rtp = query.ResultRowType;
          if (rtp!=null && typeof(TypedRow).IsAssignableFrom(rtp))
            schema = Schema.GetForTypedRow(query.ResultRowType);


          Rowset result = null;
          if (schema!=null)
            result = new Rowset(schema);

          var p = query[QUERY_PARAM_SKIP_COUNT];
          var skipCount  = p!=null ? p.Value.AsInt(0) : 0;

          p = query[QUERY_PARAM_FETCH_BY];
          var fetchBy    = p!=null ? p.Value.AsInt(0) : 0;

          p = query[QUERY_PARAM_FETCH_LIMIT];
          var fetchLimit = p!=null ? p.Value.AsInt(-1) : -1;

          using(var cursor = collection.Find(qry, skipCount, oneRow ? 1: fetchBy))
            foreach(var doc in cursor)
            {
              if (schema==null)
              {
                schema = Store.Converter.InferSchemaFromBSONDocument(doc);
                result = new Rowset(schema);
              }

              var row = Row.MakeRow(schema, query.ResultRowType);
              Store.Converter.BSONDocumentToRow(doc, row, Store.TargetName);
              result.Add( row );

              if (fetchLimit>0 && result.Count>=fetchLimit) break;
            }

          return result;
        }

        public override Cursor OpenCursor(ICRUDQueryExecutionContext context, Query query)
        {
          var ctx = (MongoDBCRUDQueryExecutionContext)context;

          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, Source, out collection );

          Schema schema = null;
          var rtp = query.ResultRowType;
          if (rtp!=null && typeof(TypedRow).IsAssignableFrom(rtp))
            schema = Schema.GetForTypedRow(query.ResultRowType);


          var p = query[QUERY_PARAM_SKIP_COUNT];
          var skipCount  = p!=null ? p.Value.AsInt(0) : 0;

          p = query[QUERY_PARAM_FETCH_BY];
          var fetchBy    = p!=null ? p.Value.AsInt(0) : 0;

          var mcursor = collection.Find(qry, skipCount, fetchBy);
          var enumerable = enumOpenCursor(schema, query, mcursor);

          return new MongoDBCursor( mcursor, enumerable );
        }

            private IEnumerable<Row> enumOpenCursor(Schema schema, Query query, Connector.Cursor mcursor)
            {
              using(mcursor)
                foreach(var doc in mcursor)
                {
                  if (schema==null)
                  {
                    schema = Store.Converter.InferSchemaFromBSONDocument(doc);
                  }

                  var row = Row.MakeRow(schema, query.ResultRowType);
                  Store.Converter.BSONDocumentToRow(doc, row, Store.TargetName);
                  yield return row;
                }
            }


        public override int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query)
        {
            var ctx = (MongoDBCRUDQueryExecutionContext)context;

            var qry = MakeQuery(query, Source);

            return ctx.Database.RunCommand( qry ) != null ? 1 : 0;
        }
    }
}
