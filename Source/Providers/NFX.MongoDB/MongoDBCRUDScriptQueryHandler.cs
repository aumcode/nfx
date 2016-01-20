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
    /// Executes MySql CRUD script-based queries
    /// </summary>
    public sealed class MongoDBCRUDScriptQueryHandler : MongoDBCRUDQueryHandlerBase   
    {
      public MongoDBCRUDScriptQueryHandler(MongoDBDataStore store, QuerySource source) : base(store, source)
      {
      }


        public override Schema GetSchema(ICRUDQueryExecutionContext context, Query query)
        {
          var ctx = (MongoDBCRUDQueryExecutionContext)context;
              
          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, out collection );

          var doc = collection.FindOne(qry);
          if (doc==null) return null;
              
          return m_Store.Converter.InferSchemaFromBSONDocument(doc);
        }


        public override RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
        {
          var ctx = (MongoDBCRUDQueryExecutionContext)context;
              
          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, out collection );
                            
          Schema schema = null;
          var rtp = query.ResultRowType;
          if (rtp!=null && typeof(TypedRow).IsAssignableFrom(rtp))
            schema = Schema.GetForTypedRow(query.ResultRowType);
              

          Rowset result = null;
          if (schema!=null)
            result = new Rowset(schema);


          var skipCount = query["SKIP_COUNT"].AsInt(0);

          using(var cursor = collection.Find(qry, skipCount, oneRow ? 1: 0))
            foreach(var doc in cursor)
            {
              if (schema==null)
              {
                schema = m_Store.Converter.InferSchemaFromBSONDocument(doc);
                result = new Rowset(schema);
              }
                  
              var row = Row.MakeRow(schema, query.ResultRowType);
              m_Store.Converter.BSONDocumentToRow(doc, row, m_Store.TargetName);
              result.Add( row );
            }

          return result;
        }

        public override int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query)
        {
            var ctx = (MongoDBCRUDQueryExecutionContext)context;
              
            var qry = MakeQuery(query);

            return ctx.Database.RunCommand( qry ) != null ? 1 : 0;
        }


    }

    
    


}
