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
      public void InsertManyAndLoadCursor()
      {
          const int CNT = 1000;
          for(var i=0; i<CNT; i++)
          {
            var row = new MyData
            {
               ID = i,
               Data = "i is "+i.ToString()
            };

            store.Insert(row);
          }

          {
            var cursor = store.OpenCursor(  new Query("CRUD.LoadAllMyData", typeof(MyData))  );
            Assert.IsNotNull( cursor );
            var lst = new List<Row>();
            foreach(var row in cursor)
             lst.Add(row);

            Assert.AreEqual(CNT, lst.Count);
            Assert.IsTrue( cursor.Disposed );

            Console.WriteLine(lst[0].ToJSON());
            Console.WriteLine("..............................");
            Console.WriteLine(lst[lst.Count-1].ToJSON());

            Assert.AreEqual(0, lst[0]["ID"]);
            Assert.AreEqual(CNT-1, lst[lst.Count-1]["ID"]);
          }
 Console.WriteLine("A");
          {
            Cursor cursor;
            var lst = new List<Row>();

            using(cursor = store.OpenCursor(  new Query("CRUD.LoadAllMyData", typeof(MyData))  ))
            {
              Assert.IsNotNull( cursor );
              foreach(var row in cursor)
               lst.Add(row);

              try
              {
                foreach(var row in cursor)
                  lst.Add(row);
                Assert.Fail("Can not iterate the second time");
              }
              catch(Exception error)
              {
                Console.WriteLine("Expected and got: "+error.ToMessageWithType());
              }
            }
            Assert.AreEqual(CNT, lst.Count);
            Assert.IsTrue( cursor.Disposed );
          }
Console.WriteLine("B");
          {
            Cursor cursor;

            using(cursor = store.OpenCursor(  new Query("CRUD.LoadAllMyData", typeof(MyData))  ))
            {
              Assert.IsNotNull( cursor );
              var en = cursor.GetEnumerator();
              Assert.IsTrue(en.MoveNext());
              Assert.IsTrue(en.MoveNext());
              Assert.IsTrue(en.MoveNext());
              //Notice, We DO NOT iterate to the very end
              //... not till the end
            }
            Assert.IsTrue( cursor.Disposed );
          }
Console.WriteLine("C");
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

      [Test]
      public void SubDocuments_InsertAndLoad()
      {
          var row = new MyInvoice
          {
             GDID = new GDID(1, 1, 100),
             Name = "Dimon Khachaturyan",
             Date = new DateTime(1990, 08, 15, 14, 0, 0, DateTimeKind.Utc),
             Lines = new MyInvoiceLine[]
             {
              new MyInvoiceLine{ LineNo = 1, Description = "Sosiki", Amount = 12.67m },
              new MyInvoiceLine{ LineNo = 2, Description = "Mylo", Amount = 8.50m },
              new MyInvoiceLine{ LineNo = 3, Description = "Туалетная Бумага 'Зева'", Amount = 9.75m },
              new MyInvoiceLine{ LineNo = 4, Description = "Трусы Мужские", Amount = 3.72m }
             }
          };

          store.Insert(row);

          var row2 = store.LoadRow(new Query<MyInvoice>("CRUD.LoadInvoice")
                           {
                             new Query.Param("id", row.GDID)
                           });
          Assert.IsNotNull( row2 );

          Assert.AreEqual(row.GDID, row2.GDID);
          Assert.AreEqual(row.Name, row2.Name);
          Assert.AreEqual(row.Date, row2.Date);

          Assert.IsNotNull( row2.Lines );
          Assert.AreEqual(4, row2.Lines.Length );

          Assert.AreEqual(1, row2.Lines[0].LineNo );
          Assert.AreEqual("Sosiki", row2.Lines[0].Description );
          Assert.AreEqual(12.67m, row2.Lines[0].Amount );

          Assert.AreEqual(2, row2.Lines[1].LineNo );
          Assert.AreEqual("Mylo", row2.Lines[1].Description );
          Assert.AreEqual(8.50m, row2.Lines[1].Amount );

          Assert.AreEqual(3, row2.Lines[2].LineNo );
          Assert.AreEqual("Туалетная Бумага 'Зева'", row2.Lines[2].Description );
          Assert.AreEqual(9.75m, row2.Lines[2].Amount );

          Assert.AreEqual(4, row2.Lines[3].LineNo );
          Assert.AreEqual("Трусы Мужские", row2.Lines[3].Description );
          Assert.AreEqual(3.72m, row2.Lines[3].Amount );
      }


      [Test]
      public void SubDocuments_MuchData()
      {
          const int CNT = 1000;

          var row = new MuchData
          {
            Address1 = "1782 Zhabovaja Uliza # 2",
            Address2 = "Kv. # 18",
            AddressCity = "Odessa",
            AddressState = "Zhopinsk",
            AddressCountry = "Russia",

            Mother = new MyPerzon{ Name = "Alla Pugacheva", Age = 56, GDID = new GDID(1,12,456), Data = new byte[]{1,7,6,2,4}  },
            Father = new MyPerzon{ Name = "Tsoi Korolenko", Age = 52, GDID = new GDID(21,2,2456), Data = new byte[]{1,2,3,4,5} },

            Decimals = new decimal[]{ 23m, 234m, 12m, 90m, 234m},
            Double   = new double[] { 12.3d, 89.2d, 90d },
            Ints     = new int[]    { 23, 892,33,423 },
            Phone    = "22-3-22",

            Invoices = new MyInvoice[]
            {
                new MyInvoice
                {
                    GDID = new GDID(1, 1, 100),
                    Name = "Dimon Khachaturyan",
                    Date = new DateTime(1990, 08, 15, 14, 0, 0, DateTimeKind.Utc),
                    Lines = new MyInvoiceLine[]
                    {
                    new MyInvoiceLine{ LineNo = 1, Description = "Sosiki", Amount = 12.67m },
                    new MyInvoiceLine{ LineNo = 2, Description = "Mylo", Amount = 8.50m },
                    new MyInvoiceLine{ LineNo = 3, Description = "Туалетная Бумага 'Зева'", Amount = 9.75m },
                    new MyInvoiceLine{ LineNo = 4, Description = "Трусы Мужские", Amount = 3.72m }
                    }
                },

                new MyInvoice
                {
                    GDID = new GDID(1, 1, 200),
                    Name = "Karen Matnazarov",
                    Date = new DateTime(1990, 08, 15, 14, 0, 0, DateTimeKind.Utc),
                    Lines = new MyInvoiceLine[]
                    {
                    new MyInvoiceLine{ LineNo = 1, Description = "Sol", Amount = 2.67m },
                    new MyInvoiceLine{ LineNo = 2, Description = "Gazeta", Amount = 4.50m },
                    }
                }
            }
          };

          var sw = System.Diagnostics.Stopwatch.StartNew();

          row.GDID =  new GDID(10, 0);
          store.Insert(row);

          for(var i=1; i<CNT; i++)
          {
            row.GDID =  new GDID(10, (ulong)i);
            store.Insert(row);
          }

          var elp = sw.ElapsedMilliseconds;

          Console.WriteLine("Did {0} in {1} ms at {2} ops/sec".Args(CNT, elp, CNT / (elp / 1000d)));


          var row2 = store.LoadRow(new Query<MuchData>("CRUD.LoadMuchData")
                           {
                             new Query.Param("id", row.GDID)
                           });
          Assert.IsNotNull( row2 );

          Assert.AreEqual(row.Address1,    row2.Address1);
          Assert.AreEqual(row.Address2,    row2.Address2);
          Assert.AreEqual(row.AddressCity, row2.AddressCity);
          Assert.AreEqual(row.AddressState, row2.AddressState);
          Assert.AreEqual(row.AddressCountry, row2.AddressCountry);
          Assert.AreEqual(row.Invoices.Length, row2.Invoices.Length);
      }


      [Test]
      public void Key_Violation()
      {
        var data1 = new MyData{ ID = 1, Data = "My data string 1"};
        var data2 = new MyData{ ID = 2, Data = "My data string 2"};
        var data1again  = new MyData{ ID = 1, Data = "My data string 1 again"};

        store.Insert(data1);
        store.Insert(data2);

        try
        {
          store.Insert(data1again);
          Assert.Fail("No key violation");
        }
        catch(Exception error)
        {
          var dae = error as MongoDBDataAccessException;
          Assert.IsNotNull( dae );
          Assert.IsNotNull( dae.KeyViolation);
          Assert.IsTrue( dae.KeyViolationKind == NFX.DataAccess.KeyViolationKind.Primary);
          Console.WriteLine(error.ToMessageWithType());

          Console.WriteLine("Key violation is: "+dae.KeyViolation);
        }

        var rowset = store.LoadOneRowset(  new Query("CRUD.LoadAllMyData", typeof(MyData))  );
        Assert.IsNotNull( rowset );

        Assert.AreEqual(2, rowset.Count);

        Assert.AreEqual(1, rowset[0][0]);
        Assert.AreEqual(2, rowset[1][0]);
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

        public class MyData : TypedRow
        {
          [Field(backendName: "_id")]
          public long ID { get; set;}

          [Field]
          public string Data { get; set;}
        }



        public class MyInvoice : TypedRow
        {
          [Field(backendName: "_id")]
          public GDID GDID { get; set;}

          [Field(backendName: "name")]
          public string Name { get; set;}

          [Field(backendName: "dt")]
          public DateTime Date { get; set;}

          [Field(backendName: "lns")]
          public MyInvoiceLine[] Lines {get; set; }
        }

        public class MyInvoiceLine : TypedRow
        {
          [Field(backendName: "ln")]
          public int LineNo { get; set;}

          [Field(backendName: "d")]
          public string Description { get; set;}

          [Field(backendName: "a")]
          public decimal Amount { get; set;}
        }



        public class MuchData : TypedRow
        {
          [Field(backendName: "_id")]
          public GDID GDID { get; set;}

          [Field(backendName: "adr1")]
          public string Address1{ get; set;}

          [Field(backendName: "adr2")]
          public string Address2{ get; set;}

          [Field(backendName: "adrCity")]
          public string AddressCity{ get; set;}

          [Field(backendName: "adrState")]
          public string AddressState{ get; set;}

          [Field(backendName: "adrCountry")]
          public string AddressCountry{ get; set;}

          [Field(backendName: "phones")]
          public string Phone{ get; set;}

          [Field(backendName: "ints")]
          public int[] Ints{ get; set;}

          [Field(backendName: "doubles")]
          public double[] Double{ get; set;}

          [Field(backendName: "decimals")]
          public decimal[] Decimals{ get; set;}

          [Field(backendName: "m")]
          public MyPerzon Mother{ get; set;}

          [Field(backendName: "f")]
          public MyPerzon Father{ get; set;}

          [Field(backendName: "inv")]
          public MyInvoice[] Invoices{ get; set;}

          [Field(backendName: "i1")]
          public int Int1{ get; set; }

          [Field(backendName: "i2")]
          public int Int2{ get; set; }

          [Field(backendName: "i3")]
          public int Int3{ get; set; }

          [Field(backendName: "L1")]
          public long Long1{ get; set; }

          [Field(backendName: "L2")]
          public long Long2{ get; set; }

          [Field(backendName: "B1")]
          public bool Bool1{ get; set; }

          [Field(backendName: "B2")]
          public bool Bool2{ get; set; }
        }

  }
}
