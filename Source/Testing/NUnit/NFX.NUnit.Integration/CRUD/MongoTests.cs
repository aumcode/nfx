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

          var affected = store.Insert(row);
          Assert.AreEqual(1, affected);
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
      public void InsertAndLoadRowIntoDynamicRow()
      {
          var row = new MyPerzon
          {
             GDID = new GDID(2, 2, 200),
             Name = "Zalup Marsoedov",
             Age = 89
          }; 

          store.Insert(row);
                                  
          var row2 = store.LoadRow(new Query<DynamicRow>("CRUD.LoadPerzon")
                           {
                             new Query.Param("id", row.GDID)
                           }); 
          Assert.IsNotNull( row2 );

          Assert.AreEqual(row.GDID, row2["_id"].AsGDID());
          Assert.AreEqual(row.Name, row2["Name"]);
          Assert.AreEqual(row.Age,  row2["Age"]);
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
         var data = new byte[] { 0x00, 0x79, 0x14 };
         var row = 
           new MyPerzon
            {
               GDID = new GDID(1, 1, 980),
               Name = "Lenin Grib",
               Age = 100,
               Data = data
            }; 

         store.Insert(row);

         var qry = new Query("CRUD.LoadPerzon", typeof(MyPerzon))
                           {
                             new Query.Param("id", new GDID(1,1,980))
                           };
         
         var schema = store.GetSchema(qry);
         
         Assert.IsNotNull(schema);
         Assert.AreEqual(4, schema.FieldCount);
         
         Assert.AreEqual(0, schema["_id"].Order); 
         Assert.AreEqual(1, schema["Name"].Order); 
         Assert.AreEqual(2, schema["Age"].Order); 
         Assert.AreEqual(3, schema["Data"].Order); 
         
         var row2 = new DynamicRow(schema);//Notice: We are creating dynamic row with schema taken from Mongo

         row2["_id"] = new GDID(10,10,10);
         row2["Name"] = "Kozloff";
         row2["Age"] = "199";
         row2["Data"] = data;

         var json = row2.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);
         Console.WriteLine(json);

         var dyn = json.JSONToDynamic();

         Assert.AreEqual(4, dyn.Data.Count);
         Assert.AreEqual("10:10:10", dyn._id);
         Assert.AreEqual("Kozloff", dyn.Name);
         Assert.AreEqual("199", dyn.Age);
         //todo: how to check dynamic row with 'Data' name? dyn.Data is the collection of all kvp ((JSONDataMap)dyn.Data)["Data"] is JSONDataArray
         //Assert.AreEqual(data, dyn.Data);
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

        [Test]
        public void InsertWithPredicate()
        {
            var person = new MyPerzon
            {
                GDID = new GDID(1, 1, 1),
                Name = "Jack London",
                Age = 23
            };

            var affected = store.Insert(person, (r, k, f) => f.Name != "Age");
            Assert.AreEqual(1, affected);

            var query = new Query<MyPerzon>("CRUD.LoadPerzon") 
                           {
                             new Query.Param("id", person.GDID)
                           };
            var persisted = store.LoadRow(query);
            Assert.AreEqual(person.Name, persisted.Name);
            Assert.AreEqual(0, persisted.Age);
        }

        [Test]
        public void UpdateWithPredicate()
        {
            var person = new MyPerzon
            {
                GDID = new GDID(1, 1, 1),
                Name = "Jack London",
                Age = 23
            };

            store.Insert(person);
            var query = new Query<MyPerzon>("CRUD.LoadPerzon") 
                           {
                             new Query.Param("id", person.GDID)
                           };
            var persisted = store.LoadRow(query);
            persisted.Name = "Ivan";
            persisted.Age = 56;
            
            var affected = store.Update(persisted, null, (r, k, f) => f.Name != "Name");
            var updated = store.LoadRow(query);

            Assert.AreEqual(1, affected);
            Assert.AreEqual(person.Name, updated.Name);
            Assert.AreEqual(persisted.Age, updated.Age);
        }

        [Test]
        public void UpsertWithPredicate()
        {
            var person = new MyPerzon
            {
                GDID = new GDID(1, 1, 1),
                Name = "Jack London",
                Age = 23
            };

            store.Insert(person);
            var query = new Query<MyPerzon>("CRUD.LoadPerzon") 
                           {
                             new Query.Param("id", person.GDID)
                           };
            var persisted = store.LoadRow(query);
            persisted.Name = "Ivan";
            persisted.Age = 56;
            
            var affected = store.Upsert(persisted, (r, k, f) => f.Name != "Name");
            var upserted = store.LoadRow(query);

            Assert.AreEqual(1, affected);
            Assert.AreEqual(null, upserted.Name);
            Assert.AreEqual(persisted.Age, upserted.Age);
        }

        [Test]
        public void ExecuteWithoutFetch_InsertRows()
        {
            var id1 = new GDID(0, 0, 1);
            var id2 = new GDID(0, 0, 2);
            var id3 = new GDID(0, 0, 3);
            var data = new byte[] { 0x00, 0x79, 0x14 };
            var query = new Query<MyPerzon>("CRUD.InsertPerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
                new Query.Param("data", data)
            };

            var affected = store.ExecuteWithoutFetch(query);
            Assert.AreEqual(1, affected);

            var c = NFX.DataAccess.MongoDB.Connector.MongoClient.Instance.DefaultLocalServer["nfxtest"]["MyPerzon"];
            var entries = c.FindAndFetchAll(new NFX.DataAccess.MongoDB.Connector.Query());
            Assert.AreEqual(3, entries.Count);

            var query1 = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id1) };
            var person1 = store.LoadRow(query1);
            Assert.IsNotNull(person1);
            Assert.AreEqual(id1, person1.GDID);
            Assert.AreEqual("Jack London", person1.Name);
            Assert.AreEqual(32, person1.Age);
            Assert.AreEqual(data, person1.Data);

            var query2 = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id2) };
            var person2 = store.LoadRow(query2);
            Assert.IsNotNull(person2);
            Assert.AreEqual(id2, person2.GDID);
            Assert.AreEqual("Ivan Poddubny", person2.Name);
            Assert.AreEqual(41, person2.Age);

            var query3 = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id3) };
            var person3 = store.LoadRow(query3);
            Assert.IsNotNull(person3);
            Assert.AreEqual(id3, person3.GDID);
            Assert.AreEqual("Anna Smith", person3.Name);
            Assert.AreEqual(28, person3.Age);
        }

        [Test]
        public void ExecuteWithoutFetch_UpdateRows()
        {
            var id1 = new GDID(0, 0, 1);
            var id2 = new GDID(0, 0, 2);
            var id3 = new GDID(0, 0, 3);
            var data = new byte[] { 0x00, 0x79, 0x14 };
            var query = new Query<MyPerzon>("CRUD.InsertPerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
                new Query.Param("data", data)
            };
            store.ExecuteWithoutFetch(query);

            query = new Query<MyPerzon>("CRUD.UpdatePerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
            };
            var affected = store.ExecuteWithoutFetch(query);
            Assert.AreEqual(1, affected);

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id1) };
            var person1 = store.LoadRow(query);
            Assert.IsNotNull(person1);
            Assert.AreEqual(id1, person1.GDID);
            Assert.AreEqual("Jack London", person1.Name);
            Assert.AreEqual(56, person1.Age);

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id2) };
            var person2 = store.LoadRow(query);
            Assert.IsNotNull(person2);
            Assert.AreEqual(id2, person2.GDID);
            Assert.AreEqual("John", person2.Name);
            Assert.AreEqual(0, person2.Age); // update without $set removed Age field

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id3) };
            var person3 = store.LoadRow(query);
            Assert.IsNotNull(person3);
            Assert.AreEqual(id3, person3.GDID);
            Assert.AreEqual("Anna Smith", person3.Name);
            Assert.AreEqual(23, person3.Age);
        }

        [Test]
        public void ExecuteWithoutFetch_Multiquering()
        {
            var id1 = new GDID(0, 0, 1);
            var id2 = new GDID(0, 0, 2);
            var id3 = new GDID(0, 0, 3);
            var data = new byte[] { 0x00, 0x79, 0x14 };
            var query1 = new Query<MyPerzon>("CRUD.InsertPerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
                new Query.Param("data", data)
            };
            var query2 = new Query<MyPerzon>("CRUD.UpdatePerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3)
            };

            var affected = store.ExecuteWithoutFetch(query1, query2);
            Assert.AreEqual(2, affected);

            var query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id1) };
            var person1 = store.LoadRow(query);
            Assert.IsNotNull(person1);
            Assert.AreEqual(id1, person1.GDID);
            Assert.AreEqual("Jack London", person1.Name);
            Assert.AreEqual(56, person1.Age);

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id2) };
            var person2 = store.LoadRow(query);
            Assert.IsNotNull(person2);
            Assert.AreEqual(id2, person2.GDID);
            Assert.AreEqual("John", person2.Name);
            Assert.AreEqual(0, person2.Age); // update without $set removed Age field

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id3) };
            var person3 = store.LoadRow(query);
            Assert.IsNotNull(person3);
            Assert.AreEqual(id3, person3.GDID);
            Assert.AreEqual("Anna Smith", person3.Name);
            Assert.AreEqual(23, person3.Age);
        }

        public class MyPerzon : TypedRow
        {
          [Field(backendName: "_id")]
          public GDID GDID { get; set;} 

          [Field]
          public string Name { get; set;} 

          [Field]
          public int Age { get; set;} 

          [Field]
          public byte[] Data {get; set; }
        }
  }
}
