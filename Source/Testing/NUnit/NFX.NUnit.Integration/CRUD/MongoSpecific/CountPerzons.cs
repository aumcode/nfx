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

    public CountPerzons(MongoDBDataStore store) : base(store, null)
    {
      m_Source = new QuerySource(GetType().FullName, 
        @"#pragma
modify=MyPerzon

{'Age': {'$gt': '$$fromAge', '$lt': '$$toAge'}}");
    }
    
    
    public override RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
    {
      var ctx = (MongoDBCRUDQueryExecutionContext)context;

      NFX.DataAccess.MongoDB.Connector.Collection collection;
      var qry = MakeQuery(ctx.Database, query, out collection);

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
