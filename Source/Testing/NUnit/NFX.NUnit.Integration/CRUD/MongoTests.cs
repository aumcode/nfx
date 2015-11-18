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

using NUnit.Framework;

using NFX;
using NFX.Glue;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.DataAccess.MongoDB;
using NFX.Serialization.JSON;

namespace NFX.NUnit.Integration.CRUD
{
  /// <summary>
  /// Mongo CRUD tests
  /// </summary>
  [TestFixture]
  public class MongoTests
  {
      private const string SCRIPT_ASM = "NFX.NUnit.Integration";

      private static readonly Node CONNECT_NODE = NFX.DataAccess.MongoDB.Connector.Connection.DEFAUL_LOCAL_NODE;
      private static readonly string CONNECT_STR = CONNECT_NODE.ConnectString;
      private const string DB_NAME = "nfxtest";


      private MongoDBDataStore store;

      [SetUp] public void BeforeTest()
      { 
         store = new MongoDBDataStore(CONNECT_STR, DB_NAME);
         store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
         store.QueryResolver.RegisterHandlerLocation("NFX.NUnit.Integration.CRUD.MongoSpecific, NFX.NUnit.Integration");
         clearAll();
      }

      [TearDown] public void AfterTest()
      {
         DisposableObject.DisposeAndNull(ref store);
      }

      private void clearAll()
      {
        using (var db = NFX.DataAccess.MongoDB.Connector.MongoClient.Instance[CONNECT_NODE][DB_NAME])
        {
          foreach( var cn in db.GetCollectionNames())
           db[cn].Drop();
        }
      }

      //==========================================================================================


      [Test]
      public void Insert()
      {
          var row = new MyPerzon
          {
             GDID = new GDID(1, 1, 100),
             Name = "Jeka Koshmar",
             Age = 89
          }; 

          store.Insert(row);
      }

      [Test]
      public void InsertAndLoad()
      {
          var row = new MyPerzon
          {
             GDID = new GDID(1, 1, 100),
             Name = "Jeka Koshmar",
             Age = 89
          }; 

          store.Insert(row);

          var row2 = store.LoadOneRow(new Query("CRUD.LoadPerzon", typeof(MyPerzon))
                           {
                             new Query.Param("id", row.GDID)
                           }) as MyPerzon; 
          Assert.IsNotNull( row2 );

          Assert.AreEqual(row.GDID, row2.GDID);
          Assert.AreEqual(row.Name, row2.Name);
          Assert.AreEqual(row.Age,  row2.Age);
      }


      [Test]
      public void InsertManyAndLoadMany()
      {
          for(var i=0; i<100; i++) 
          {
            var row = new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            }; 

            store.Insert(row);
          }

          var rs = store.LoadOneRowset(new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           }); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(4, rs.Count);

          Assert.AreEqual(14, rs.First()["Age"]);
          Assert.AreEqual(11, rs.Last()["Age"]);
      }


      [Test]
      public void InsertUpsertUpdate()
      {
          var row = new MyPerzon
          {
              GDID = new GDID(1, 1, 1),
              Name = "Eight Eightavich",
              Age = 8
          }; 

          Assert.AreEqual(1, store.Insert(row) );

          var qryBetween1015 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           };

          
          var rs = store.LoadOneRowset(qryBetween1015); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(0, rs.Count);

          row =  new MyPerzon
          {
              GDID = new GDID(1, 1, 2),
              Name = "T Twelver",
              Age = 12
          }; 

          Assert.AreEqual(0, store.Update(row) );//update did not find

          Assert.AreEqual(1, store.Upsert(row) );//upsert DID find

          row.Name="12-er-changed";
          Assert.AreEqual(1, store.Update(row) );//update DID find this time

          rs = store.LoadOneRowset(qryBetween1015); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(1, rs.Count);
          row = rs[0] as MyPerzon;
          Assert.AreEqual(new GDID(1,1,2), row.GDID);
          Assert.AreEqual("12-er-changed", row.Name);
      }


      [Test]
      public void InsertUpsert()
      {
          var row = new MyPerzon
          {
              GDID = new GDID(1, 1, 1),
              Name = "Eight Eightavich",
              Age = 8
          }; 

          Assert.AreEqual(1, store.Insert(row) );

          var qryBetween1015 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           };

          
          var rs = store.LoadOneRowset(qryBetween1015); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(0, rs.Count);

          row =  new MyPerzon
          {
              GDID = new GDID(1, 1, 2),
              Name = "T Twelver",
              Age = 12
          }; 

          Assert.AreEqual(1, store.Insert(row) );

          rs = store.LoadOneRowset(qryBetween1015); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(1, rs.Count);
          var cr = rs[0];
          Assert.AreEqual("T Twelver", cr["Name"]);

          cr["Name"] = "12-er";
          store.Upsert(cr);

          rs = store.LoadOneRowset(qryBetween1015); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(1, rs.Count);
          row = rs[0] as MyPerzon;
          Assert.AreEqual("12-er", row.Name);
      }


      [Test]
      public void InsertDelete()
      {
          for(var i=0; i<100; i++) 
          {
            var row = new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            }; 

            store.Insert(row);
          }

          var qryBetween1015 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           };

          var rs = store.LoadOneRowset(qryBetween1015); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(4, rs.Count);

          Assert.AreEqual(14, rs.First()["Age"]);
          Assert.AreEqual(11, rs.Last()["Age"]);

          
          Assert.AreEqual(1, store.Delete(rs.First()));//DELETE!!!!

          rs = store.LoadOneRowset(qryBetween1015); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(3, rs.Count);

          Assert.AreEqual(13, rs.First()["Age"]);
          Assert.AreEqual(11, rs.Last()["Age"]);
      }


      [Test]
      public void Save()
      {
          var rowset = new Rowset(Schema.GetForTypedRow(typeof(MyPerzon)));
          rowset.LogChanges = true;
          
          for(var i=0; i<100; i++) 
          {
            rowset.Insert( new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            }); 
          }

          var qryBetween5060 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 50),
                             new Query.Param("toAge", 60)
                           };

          var rs = store.LoadOneRowset(qryBetween5060); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(0, rs.Count);

          store.Save(rowset);
          rowset.PurgeChanges();

          rs = store.LoadOneRowset(qryBetween5060); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(9, rs.Count);

          rowset[55]["Age"] = 900;  //falls out of query
          rowset.Update(rowset[55]);
          rowset.Delete(rowset[59]); //physically deleted
          store.Save(rowset);

          rs = store.LoadOneRowset(qryBetween5060); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(7, rs.Count);
          Assert.AreEqual(58, rs.First()["Age"]);
          Assert.AreEqual(51, rs.Last()["Age"]);
      }

      [Test]
      public void GetSchema_ROW_JSON_ROW()
      {
         var row = 
           new MyPerzon
            {
               GDID = new GDID(1, 1, 980),
               Name = "Lenin Grib",
               Age = 100
            }; 

         store.Insert(row);

         var qry = new Query("CRUD.LoadPerzon", typeof(MyPerzon))
                           {
                             new Query.Param("id", new GDID(1,1,980))
                           };
         
         var schema = store.GetSchema(qry);
         
         Assert.IsNotNull(schema);
         Assert.AreEqual(3, schema.FieldCount);
         
         Assert.AreEqual(0, schema["_id"].Order); 
         Assert.AreEqual(1, schema["Name"].Order); 
         Assert.AreEqual(2, schema["Age"].Order); 
         
         var row2 = new DynamicRow(schema);//Notice: We are creating dynamic row with schema taken from Mongo

         row2["_id"] = new GDID(10,10,10);
         row2["Name"] = "Kozloff";
         row2["Age"] = "199";

         var json = row2.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);
         Console.WriteLine(json);

         var dyn = json.JSONToDynamic();

         Assert.AreEqual("10:10:10", dyn._id);
         Assert.AreEqual("Kozloff", dyn.Name);
         Assert.AreEqual("199", dyn.Age);
      }


      [Test]
      public void Count()
      {
        for(var i=0; i<100; i++) 
          {
            var row = new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            }; 

            store.Insert(row);
          }
          
          //Note!
          // this query is implemented in C# code
          var rs = store.LoadOneRowset(new Query("CountPerzons")
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 90)
                           }); 
          Assert.IsNotNull( rs );

          Assert.AreEqual(1, rs.Count);

          Assert.AreEqual(79, rs[0]["Count"]);
      }






        public class MyPerzon : TypedRow
        {
          [Field(backendName: "_id")]
          public GDID GDID { get; set;} 

          [Field]
          public string Name { get; set;} 

          [Field]
          public int Age { get; set;} 
        }
    
  }
}
