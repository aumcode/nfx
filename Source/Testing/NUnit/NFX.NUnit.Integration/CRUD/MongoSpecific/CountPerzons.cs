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
using System.Linq;
using System.Text;
using System.Diagnostics;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.MongoDB;

namespace NFX.NUnit.Integration.CRUD.MongoSpecific
{
  /// <summary>
  /// Performs a server-side cursor count
  /// </summary>
  public class CountPerzons : MongoDBCRUDQueryHandlerBase
  {

    public CountPerzons(MongoDBDataStore store, string name) : base(store, new QuerySource(name,
        @"#pragma
modify=MyPerzon

{'Age': {'$gt': '$$fromAge', '$lt': '$$toAge'}}"))
    { }


    public override RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
    {
      var ctx = (MongoDBCRUDQueryExecutionContext)context;

      NFX.DataAccess.MongoDB.Connector.Collection collection;
      var qry = MakeQuery(ctx.Database, query, Source, out collection);

      var rrow = new TResult();

      var sw = Stopwatch.StartNew();

      rrow.Count = collection.Count(qry);//Performs server-side count over query

      rrow.Interval = sw.Elapsed;

      var result = new Rowset(Schema.GetForTypedRow(typeof(TResult)));
      result.Add(rrow);
      return result;
    }

    public class TResult : TypedRow
    {
      [Field]
      public long Count{ get; set;}

      [Field]
      public TimeSpan Interval { get; set;}
    }


  }
}
