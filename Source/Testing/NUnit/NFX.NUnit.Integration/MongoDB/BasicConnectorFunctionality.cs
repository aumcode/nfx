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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using NUnit.Framework;

using NFX.Serialization.BSON;
using NFX.DataAccess.MongoDB;
using NFX.DataAccess.MongoDB.Connector;

namespace NFX.NUnit.Integration.MongoDB
{
  [TestFixture]
  public class BasicConnectorFunctionality
  {


    [Test]
    public void AllocClient()
    {
      using(var client= new MongoClient("My Test"))
      {
        var server = client.DefaultLocalServer;
        Assert.IsNotNull( server );
        Assert.IsTrue( object.ReferenceEquals(server, client[server.Node]) );

        var db = server["db1"];
        Assert.IsNotNull( db );
        Assert.IsTrue( object.ReferenceEquals(db, server[db.Name]) );

        var collection = db["t1"];
        Assert.IsNotNull( collection );
        Assert.IsTrue( object.ReferenceEquals(collection, db[collection.Name]) );

        var query = new Query();
        var result = collection.FindOne( query );
      }
    }


    [Test]
    public void CollectionLifecycle()
    {

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();
        db["t2"].Drop();
        db["t3"].Drop();

        Assert.AreEqual(1, db.GetCollectionNames().Length);

        Assert.AreEqual(1, db["t1"].Insert( new BSONDocument().Set( new BSONInt32Element("_id", 1))
                                                              .Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        Assert.AreEqual(1, db["t2"].Insert( new BSONDocument().Set( new BSONInt32Element("_id", 1))
                                                              .Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        var collections = db.GetCollectionNames();

        Assert.AreEqual(3, collections.Length);
        Assert.IsTrue( collections.Contains("t1"));
        Assert.IsTrue( collections.Contains("t2"));
      }
    }


    [Test]
    public void InsertDelete()
    {

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        Assert.AreEqual(1, t1.Insert( new BSONDocument().Set( new BSONInt32Element("_id", 1))
                                                        .Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        Assert.AreEqual(1, t1.Insert( new BSONDocument().Set( new BSONInt32Element("_id", 2))
                                                        .Set( new BSONStringElement("val", "two"))
                           ).TotalDocumentsAffected);

        Assert.AreEqual(2, t1.Count());
        Assert.AreEqual( 1, t1.DeleteOne(Query.ID_EQ_Int32(1)).TotalDocumentsAffected );

        Assert.AreEqual(1, t1.Count());
        Assert.AreEqual( 1, t1.DeleteOne(Query.ID_EQ_Int32(2)).TotalDocumentsAffected );

        Assert.AreEqual(0, t1.Count());
      }

    }

    [Test]
    public void InsertWithoutID()
    {

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        Assert.AreEqual(1, t1.Insert( new BSONDocument().Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        Assert.AreEqual(1, t1.Count());
      }

    }

    [Test]
    public void CollectionDrop()
    {

      using(var client= new MongoClient("My Test"))
      {
        var collection = client.DefaultLocalServer["db1"]["ToDrop"];
        var doc1 = new BSONDocument();

        doc1.Set( new BSONStringElement("_id", "id1"))
            .Set( new BSONStringElement("val", "My value"))
            .Set( new BSONInt32Element("age", 125));

        var r = collection.Insert(doc1);
        Assert.AreEqual(1, r.TotalDocumentsAffected);

        collection.Drop();
        Assert.IsTrue( collection.Disposed );
      }

    }

    [Test]
    public void Ping()
    {

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];
        db.Ping();
      }
    }


    [Test]
    public void Ping_Parallel()
    {
      const int CNT = 123000;

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        var sw = Stopwatch.StartNew();
        Parallel.For(0, CNT, (i) => db.Ping());
        var e =sw.ElapsedMilliseconds;

        Console.WriteLine("Ping Parallel: {0:n0} times in {1:n0} ms at {2:n0} ops/sec".Args(CNT, e, CNT / (e/1000d)) );
      }
    }

    [TestCase(0)]
    [TestCase(2)]
    public void Insert_FindOne_Parallel(int maxConnections)
    {
      const int CNT = 75000;

      using(var client= new MongoClient("My Test"))
      {
        var server = client.DefaultLocalServer;
        server.MaxConnections = maxConnections;
        var db = server["db1"];
        db["t1"].Drop();
        var t1 = db["t1"];


        var sw = Stopwatch.StartNew();
        Parallel.For(0, CNT, (i) =>
        {
          Assert.AreEqual(1, db["t1"].Insert( new BSONDocument().Set( new BSONInt32Element("_id", i))
                                                                .Set( new BSONStringElement("val", "num-"+i.ToString()))
                           ).TotalDocumentsAffected);
        });

        var e1 =sw.ElapsedMilliseconds;

        Assert.AreEqual(CNT, t1.Count());

        Console.WriteLine("Insert Parallel: {0:n0} times in {1:n0} ms at {2:n0} ops/sec   MAX CONNECTIONS={3} ".Args(CNT, e1, CNT / (e1/1000d), maxConnections ));

        sw.Restart();
        Parallel.For(0, CNT, (i) =>
        {
          var got = db["t1"].FindOne(Query.ID_EQ_Int32(i));
          Assert.IsNotNull( got );
          Assert.AreEqual("num-"+i.ToString(), got["val"].AsString());
        });

        var e2 =sw.ElapsedMilliseconds;

        Console.WriteLine("FindOne Parallel: {0:n0} times in {1:n0} ms at {2:n0} ops/sec   MAX CONNECTIONS={3} ".Args(CNT, e2, CNT / (e2/1000d), maxConnections ));
      }
    }

    [Test]
    public void Insert_Find_PrimitiveTypes()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        var item1 = new BSONDocument().Set(new BSONInt32Element("int", int.MaxValue));
        var item2 = new BSONDocument().Set(new BSONStringElement("string", "min"));
        var item3 = new BSONDocument().Set(new BSONBooleanElement("bool", true));
        var item4 = new BSONDocument().Set(new BSONDateTimeElement("datetime", new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc)));
        var item5 = new BSONDocument().Set(new BSONNullElement("null"));
        var item6 = new BSONDocument().Set(new BSONArrayElement("array",
                                              new BSONElement[]
                                              {
                                                new BSONInt32Element(int.MaxValue),
                                                new BSONInt32Element(int.MinValue)
                                              }));
        var item7 = new BSONDocument().Set(new BSONBinaryElement("binary", new BSONBinary(BSONBinaryType.UserDefined, Encoding.UTF8.GetBytes("Hello world"))));
        var item8 = new BSONDocument().Set(new BSONDocumentElement("document",
                                              new BSONDocument().Set(new BSONInt64Element("innerlong", long.MinValue))));
        var item9 = new BSONDocument().Set(new BSONDoubleElement("double", -123.456D));
        var item10 = new BSONDocument().Set(new BSONInt64Element("long", long.MaxValue));
        var item11 = new BSONDocument().Set(new BSONJavaScriptElement("js", "function(a){var x = a;return x;}"));
        var item12 = new BSONDocument().Set(new BSONJavaScriptWithScopeElement("jswithscope",
                                              new BSONCodeWithScope("function(a){var x = a;return x+z;}",
                                                                     new BSONDocument().Set(new BSONInt32Element("z", 12)))));
        var item13 = new BSONDocument().Set(new BSONMaxKeyElement("maxkey"));
        var item14 = new BSONDocument().Set(new BSONMinKeyElement("minkey"));
        var item15 = new BSONDocument().Set(new BSONObjectIDElement("oid", new BSONObjectID(1, 2, 3, 400)));
        var item16 = new BSONDocument().Set(new BSONRegularExpressionElement("regex",
                                              new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M)));
        var item17 = new BSONDocument().Set(new BSONTimestampElement("timestamp", new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345)));

        Assert.AreEqual(1, collection.Insert(item1).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item2).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item3).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item4).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item5).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item6).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item7).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item8).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item9).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item10).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item11).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item12).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item13).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item14).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item15).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item16).TotalDocumentsAffected);
        Assert.AreEqual(1, collection.Insert(item17).TotalDocumentsAffected);

        var all = collection.Find(new Query());

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONInt32Element)all.Current["int"]).Value, int.MaxValue);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONStringElement)all.Current["string"]).Value, "min");

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONBooleanElement)all.Current["bool"]).Value, true);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONDateTimeElement)all.Current["datetime"]).Value, new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc));

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.IsInstanceOf<BSONNullElement>(all.Current["null"]);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        var array = ((BSONArrayElement)all.Current["array"]).Value;
        Assert.AreEqual(array.Length, 2);
        Assert.AreEqual(((BSONInt32Element)array[0]).Value, int.MaxValue);
        Assert.AreEqual(((BSONInt32Element)array[1]).Value, int.MinValue);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        var binary = ((BSONBinaryElement)all.Current["binary"]).Value;
        Assert.AreEqual(binary.Data, Encoding.UTF8.GetBytes("Hello world"));
        Assert.AreEqual(binary.Type, BSONBinaryType.UserDefined);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        var doc = ((BSONDocumentElement)all.Current["document"]).Value;
        Assert.AreEqual(doc.Count, 1);
        Assert.AreEqual(((BSONInt64Element)doc["innerlong"]).Value, long.MinValue);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONDoubleElement)all.Current["double"]).Value, -123.456D);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONInt64Element)all.Current["long"]).Value, long.MaxValue);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONJavaScriptElement)all.Current["js"]).Value, "function(a){var x = a;return x;}");

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        var jsScope = ((BSONJavaScriptWithScopeElement)all.Current["jswithscope"]).Value;
        Assert.AreEqual(jsScope.Code, "function(a){var x = a;return x+z;}");
        Assert.AreEqual(jsScope.Scope.Count, 1);
        Assert.AreEqual(((BSONInt32Element)jsScope.Scope["z"]).Value, 12);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.IsInstanceOf<BSONMaxKeyElement>(all.Current["maxkey"]);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.IsInstanceOf<BSONMinKeyElement>(all.Current["minkey"]);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        var oid = ((BSONObjectIDElement)all.Current["oid"]).Value;
        Assert.AreEqual(oid.Bytes, new BSONObjectID(1, 2, 3, 400).Bytes);

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONRegularExpressionElement)all.Current["regex"]).Value,
                        new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M));

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 2);
        Assert.AreEqual(((BSONTimestampElement)all.Current["timestamp"]).Value,
                        new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345));


        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Insert_Find_PrimitiveTypesSingleEntry()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        var item = new BSONDocument().Set(new BSONInt32Element("int", int.MaxValue))
                                     .Set(new BSONStringElement("string", "min"))
                                     .Set(new BSONBooleanElement("bool", true))
                                     .Set(new BSONDateTimeElement("datetime", new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc)))
                                     .Set(new BSONNullElement("null"))
                                     .Set(new BSONArrayElement("array",
                                              new BSONElement[]
                                              {
                                                new BSONInt32Element(int.MaxValue),
                                                new BSONInt32Element(int.MinValue)
                                              }))
                                     .Set(new BSONBinaryElement("binary", new BSONBinary(BSONBinaryType.UserDefined, Encoding.UTF8.GetBytes("Hello world"))))
                                     .Set(new BSONDocumentElement("document",
                                            new BSONDocument().Set(new BSONInt64Element("innerlong", long.MinValue))))
                                     .Set(new BSONDoubleElement("double", -123.456D))
                                     .Set(new BSONInt64Element("long", long.MaxValue))
                                     .Set(new BSONJavaScriptElement("js", "function(a){var x = a;return x;}"))
                                     .Set(new BSONJavaScriptWithScopeElement("jswithscope",
                                              new BSONCodeWithScope("function(a){var x = a;return x+z;}",
                                                                     new BSONDocument().Set(new BSONInt32Element("z", 12)))))
                                     .Set(new BSONMaxKeyElement("maxkey"))
                                     .Set(new BSONMinKeyElement("minkey"))
                                     .Set(new BSONObjectIDElement("oid", new BSONObjectID(1, 2, 3, 400)))
                                     .Set(new BSONRegularExpressionElement("regex",
                                           new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M)))
                                     .Set(new BSONTimestampElement("timestamp", new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345)));

        Assert.AreEqual(1, collection.Insert(item).TotalDocumentsAffected);

        var all = collection.Find(new Query());

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 18);
        Assert.AreEqual(((BSONInt32Element)all.Current["int"]).Value, int.MaxValue);
        Assert.AreEqual(((BSONStringElement)all.Current["string"]).Value, "min");
        Assert.AreEqual(((BSONBooleanElement)all.Current["bool"]).Value, true);
        Assert.AreEqual(((BSONDateTimeElement)all.Current["datetime"]).Value, new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc));
        Assert.IsInstanceOf<BSONNullElement>(all.Current["null"]);
        var array = ((BSONArrayElement)all.Current["array"]).Value;
        Assert.AreEqual(array.Length, 2);
        Assert.AreEqual(((BSONInt32Element)array[0]).Value, int.MaxValue);
        Assert.AreEqual(((BSONInt32Element)array[1]).Value, int.MinValue);
        var binary = ((BSONBinaryElement)all.Current["binary"]).Value;
        Assert.AreEqual(binary.Data, Encoding.UTF8.GetBytes("Hello world"));
        Assert.AreEqual(binary.Type, BSONBinaryType.UserDefined);
        var doc = ((BSONDocumentElement)all.Current["document"]).Value;
        Assert.AreEqual(doc.Count, 1);
        Assert.AreEqual(((BSONInt64Element)doc["innerlong"]).Value, long.MinValue);
        Assert.AreEqual(((BSONDoubleElement)all.Current["double"]).Value, -123.456D);
        Assert.AreEqual(((BSONInt64Element)all.Current["long"]).Value, long.MaxValue);
        Assert.AreEqual(((BSONJavaScriptElement)all.Current["js"]).Value, "function(a){var x = a;return x;}");
        var jsScope = ((BSONJavaScriptWithScopeElement)all.Current["jswithscope"]).Value;
        Assert.AreEqual(jsScope.Code, "function(a){var x = a;return x+z;}");
        Assert.AreEqual(jsScope.Scope.Count, 1);
        Assert.AreEqual(((BSONInt32Element)jsScope.Scope["z"]).Value, 12);
        Assert.IsInstanceOf<BSONMaxKeyElement>(all.Current["maxkey"]);
        Assert.IsInstanceOf<BSONMinKeyElement>(all.Current["minkey"]);
        var oid = ((BSONObjectIDElement)all.Current["oid"]).Value;
        Assert.AreEqual(oid.Bytes, new BSONObjectID(1, 2, 3, 400).Bytes);
        Assert.AreEqual(((BSONRegularExpressionElement)all.Current["regex"]).Value,
                        new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M));
        Assert.AreEqual(((BSONTimestampElement)all.Current["timestamp"]).Value,
                        new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345));

        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Insert_Find_UnicodeStings()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        var item = new BSONDocument().Set(new BSONStringElement("eng", "hello"))
          .Set(new BSONStringElement("rus", "привет"))
          .Set(new BSONStringElement("chi", "你好"))
          .Set(new BSONStringElement("jap", "こんにちは"))
          .Set(new BSONStringElement("gre", "γεια σας"))
          .Set(new BSONStringElement("alb", "përshëndetje"))
          .Set(new BSONStringElement("arm", "բարեւ Ձեզ"))
          .Set(new BSONStringElement("vie", "xin chào"))
          .Set(new BSONStringElement("por", "Olá"))
          .Set(new BSONStringElement("ukr", "Привіт"))
          .Set(new BSONStringElement("ger", "wünsche"));

        Assert.AreEqual(1, collection.Insert(item).TotalDocumentsAffected);

        var all = collection.Find(new Query());

        all.MoveNext();
        Assert.AreEqual(all.Current.Count, 12);
        Assert.AreEqual(((BSONStringElement)all.Current["eng"]).Value, "hello");
        Assert.AreEqual(((BSONStringElement)all.Current["rus"]).Value, "привет");
        Assert.AreEqual(((BSONStringElement)all.Current["chi"]).Value, "你好");
        Assert.AreEqual(((BSONStringElement)all.Current["jap"]).Value, "こんにちは");
        Assert.AreEqual(((BSONStringElement)all.Current["gre"]).Value, "γεια σας");
        Assert.AreEqual(((BSONStringElement)all.Current["alb"]).Value, "përshëndetje");
        Assert.AreEqual(((BSONStringElement)all.Current["arm"]).Value, "բարեւ Ձեզ");
        Assert.AreEqual(((BSONStringElement)all.Current["vie"]).Value, "xin chào");
        Assert.AreEqual(((BSONStringElement)all.Current["por"]).Value, "Olá");
        Assert.AreEqual(((BSONStringElement)all.Current["ukr"]).Value, "Привіт");
        Assert.AreEqual(((BSONStringElement)all.Current["ger"]).Value, "wünsche");

        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Find_Comparison_Int32()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        foreach (var age in Enumerable.Range(1, 100))
        {
          Assert.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", Guid.NewGuid().ToString()))
                                    .Set(new BSONInt32Element("age", age)))
                                       .TotalDocumentsAffected);
        }

        var lt1 = collection.Find(new Query(@"{ age: { $lt: 1}}", false));
        var lt5 = collection.Find(new Query(@"{ age: { '$lt': 5}}", false));
        var gte93 = collection.Find(new Query(@"{ age: { '$gte': 93}}", false));

        lt1.MoveNext();
        Assert.AreEqual(true, lt1.EOF);

        for (int i = 1; i < 5; i++)
        {
          lt5.MoveNext();
          Assert.AreEqual(lt5.Current.Count, 3);
          Assert.IsInstanceOf<BSONStringElement>(lt5.Current["name"]);
          Assert.AreEqual(((BSONInt32Element)lt5.Current["age"]).Value, i);
        }
        lt5.MoveNext();
        Assert.AreEqual(true, lt5.EOF);

        for (int i = 93; i <= 100; i++)
        {
          gte93.MoveNext();
          Assert.AreEqual(gte93.Current.Count, 3);
          Assert.IsInstanceOf<BSONStringElement>(gte93.Current["name"]);
          Assert.AreEqual(((BSONInt32Element)gte93.Current["age"]).Value, i);
        }
        gte93.MoveNext();
        Assert.AreEqual(true, gte93.EOF);
      }
    }

    [Test]
    public void Update_SimpleStringInt23Entries()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        foreach (var age in Enumerable.Range(1, 10))
        {
          Assert.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", "People" + age))
                                    .Set(new BSONInt32Element("age", age)))
                                       .TotalDocumentsAffected);
        }

        var result1 = collection.Update(new UpdateEntry(new Query("{ name: 'People1'}", false), new Query("{age: 100}", false), false, false));
        var update1 = collection.Find(new Query(@"{ age: 100}", false));

        var result2 = collection.Update(new UpdateEntry(new Query("{ age: { '$lt': 3 }}", false), new Query("{name: 'update2'}", false), false, false));
        var update2 = collection.Find(new Query(@"{name: 'update2'}", false));

        var result3 = collection.Update(new UpdateEntry(new Query("{ '$and': [ { age: { '$gte': 3 }}, { age: { '$lt': 6 }} ]}", false), new Query("{ '$set': {name: 'update3'}}", false), true, false));
        var update3 = collection.Find(new Query(@"{name: 'update3'}", false));

        var result4 = collection.Update(new UpdateEntry(new Query("{ age: -1}", false), new Query("{ '$set': {name: 'update4'}}", false), true, false));
        var update4 = collection.Find(new Query(@"{name: 'update4'}", false));

        var result5 = collection.Update(new UpdateEntry(new Query("{ age: -1}", false), new Query("{ '$set': {name: 'update5'}}", false), true, true));
        var update5 = collection.Find(new Query(@"{name: 'update5'}", false));

        var result6 = collection.Update(new UpdateEntry(new Query("{ '$or': [ {age: 6}, {age: 7}, {age: 8} ]}", false), new Query("{ '$set': {name: 'update6'}}", false), true, false));
        var update6 = collection.Find(new Query(@"{name: 'update6'}", false));

        Assert.AreEqual(result1.TotalDocumentsUpdatedAffected, 1);
        Assert.AreEqual(result1.TotalDocumentsAffected, 1);
        Assert.AreEqual(result1.Upserted, null);
        Assert.AreEqual(result1.WriteErrors, null);
        update1.MoveNext();
        Assert.AreEqual(update1.Current.Count, 2);
        Assert.AreEqual(((BSONInt32Element)update1.Current["age"]).Value,100);
        update1.MoveNext();
        Assert.AreEqual(true, update1.EOF);

        Assert.AreEqual(result2.TotalDocumentsUpdatedAffected, 1);
        Assert.AreEqual(result2.TotalDocumentsAffected, 1);
        Assert.AreEqual(result2.Upserted, null);
        Assert.AreEqual(result2.WriteErrors, null);
        update2.MoveNext();
        Assert.AreEqual(update2.Current.Count, 2);
        Assert.AreEqual(((BSONStringElement)update2.Current["name"]).Value, "update2");
        update2.MoveNext();
        Assert.AreEqual(true, update2.EOF);

        Assert.AreEqual(result3.TotalDocumentsUpdatedAffected, 3);
        Assert.AreEqual(result3.TotalDocumentsAffected, 3);
        Assert.AreEqual(result3.Upserted, null);
        Assert.AreEqual(result3.WriteErrors, null);
        for (int i = 3; i < 6; i++)
        {
          update3.MoveNext();
          Assert.AreEqual(update3.Current.Count, 3);
          Assert.AreEqual(((BSONStringElement)update3.Current["name"]).Value, "update3");
          Assert.AreEqual(((BSONInt32Element)update3.Current["age"]).Value, i);
        }
        update3.MoveNext();
        Assert.AreEqual(true, update3.EOF);

        Assert.AreEqual(result4.TotalDocumentsUpdatedAffected, 0);
        Assert.AreEqual(result4.TotalDocumentsAffected, 0);
        Assert.AreEqual(result4.Upserted, null);
        Assert.AreEqual(result4.WriteErrors, null);
        update4.MoveNext();
        Assert.AreEqual(true, update4.EOF);

        Assert.AreEqual(result5.TotalDocumentsUpdatedAffected, 0);
        Assert.AreEqual(result5.TotalDocumentsAffected, 1);
        Assert.AreEqual(result5.Upserted.Length, 1);
        Assert.AreEqual(result5.WriteErrors, null);
        update5.MoveNext();
        Assert.AreEqual(update5.Current.Count, 3);
        Assert.AreEqual(((BSONStringElement)update5.Current["name"]).Value, "update5");
        Assert.AreEqual(((BSONInt32Element)update5.Current["age"]).Value, -1);
        update5.MoveNext();
        Assert.AreEqual(true, update5.EOF);

        Assert.AreEqual(result6.TotalDocumentsUpdatedAffected, 3);
        Assert.AreEqual(result6.TotalDocumentsAffected, 3);
        Assert.AreEqual(result6.Upserted, null);
        Assert.AreEqual(result6.WriteErrors, null);
        for (int i = 6; i <= 8; i++)
        {
          update6.MoveNext();
          Assert.AreEqual(update6.Current.Count, 3);
          Assert.AreEqual(((BSONStringElement)update6.Current["name"]).Value, "update6");
          Assert.AreEqual(((BSONInt32Element)update6.Current["age"]).Value, i);
        }
        update6.MoveNext();
        Assert.AreEqual(true, update6.EOF);
      }
    }

    [Test]
    public void Update_Parallel_SimpleStringInt23Entries()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        const int CNT = 75000;
        const int CHUNK = 1500;

        foreach (var value in Enumerable.Range(0, CNT))
        {
          Assert.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", "People" + value))
                                    .Set(new BSONInt32Element("value", value)))
                                       .TotalDocumentsAffected);
        }

        Parallel.For(0, CNT/CHUNK, i =>
        {
          var result = collection.Update(new UpdateEntry(new Query("{ '$and': [ { value: {'$gte':"+i*CHUNK+"}}, { value: {'$lt':"+(i+1)*CHUNK+"}} ]}", false),
                                                         new Query("{ '$set': {name: 'updated'}}", false), true, false));
          Assert.AreEqual(result.TotalDocumentsUpdatedAffected, CHUNK);
          Assert.AreEqual(result.TotalDocumentsAffected, CHUNK);
          Assert.AreEqual(result.Upserted, null);
          Assert.AreEqual(result.WriteErrors, null);
        });

        var updated = collection.Find(new Query(@"{name: 'updated'}", false));
        for (int i = 0; i < CNT; i++)
        {
          updated.MoveNext();
          Assert.AreEqual(updated.Current.Count, 3);
          Assert.AreEqual(((BSONStringElement)updated.Current["name"]).Value, "updated");
        }
        updated.MoveNext();
        Assert.AreEqual(true, updated.EOF);
      }
    }

    [Test]
    public void Save_AsInsert()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<10; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                                                         .Set(new BSONStringElement("name", "People"+i)));
          Assert.AreEqual(1, result.TotalDocumentsAffected);
          Assert.AreEqual(0, result.TotalDocumentsUpdatedAffected);
          Assert.AreEqual(1, result.Upserted.Length);
          Assert.AreEqual(null, result.WriteErrors);
        }

        var all = collection.Find(new Query(@"{}", false));
        for (int i=0; i<10; i++)
        {
          all.MoveNext();
          Assert.AreEqual(all.Current.Count, 2);
          Assert.AreEqual(((BSONStringElement)all.Current["name"]).Value, "People"+i);
          Assert.AreEqual(((BSONObjectIDElement)all.Current["_id"]).Value.Bytes, new BSONObjectID(1,2,3, (uint)i).Bytes);
        }
        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Save_AsInsertAndUpdate()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<10; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                                                         .Set(new BSONStringElement("name", "People"+i)));
          Assert.AreEqual(1, result.TotalDocumentsAffected);
          Assert.AreEqual(0, result.TotalDocumentsUpdatedAffected);
          Assert.AreEqual(1, result.Upserted.Length);
          Assert.AreEqual(null, result.WriteErrors);
        }

        for (int i=5; i<10; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                                                         .Set(new BSONStringElement("name", "saved")));
          Assert.AreEqual(1, result.TotalDocumentsAffected);
          Assert.AreEqual(1, result.TotalDocumentsUpdatedAffected);
          Assert.AreEqual(null, result.Upserted);
          Assert.AreEqual(null, result.WriteErrors);
        }

        for (int i=10; i<15; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3,(uint)i)))
                                                         .Set(new BSONStringElement("name", "saved")));
          Assert.AreEqual(1, result.TotalDocumentsAffected);
          Assert.AreEqual(0, result.TotalDocumentsUpdatedAffected);
          Assert.AreEqual(1, result.Upserted.Length);
          Assert.AreEqual(null, result.WriteErrors);
        }

        var all = collection.Find(new Query(@"{}", false));
        for (int i=0; i<15; i++)
        {
          all.MoveNext();
          Assert.AreEqual(all.Current.Count, 2);
          Assert.AreEqual(((BSONObjectIDElement)all.Current["_id"]).Value.Bytes, new BSONObjectID(1,2,3,(uint)i).Bytes);
          Assert.AreEqual(((BSONStringElement)all.Current["name"]).Value, i<5 ? "People" + i : "saved");
        }
        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Save_Parallel_AsInsertAndUpdate()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        const int CNT = 50000;
        const int SAVE_CNT = 75000;
        const int CHUNK = 2000;

        for (int i=0; i<CNT; i++)
        {
          Assert.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                  .Set(new BSONStringElement("name", "People" + i)))
                                       .TotalDocumentsAffected);
        }

        Parallel.For(0, SAVE_CNT/CHUNK, i =>
        {
          for (int j = 0; j < CHUNK; j++)
          {
            var increment = CHUNK*i+j;
            var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1, 2, 3, (uint)increment)))
                                                           .Set(new BSONStringElement("name", "saved")));
            Assert.AreEqual(result.TotalDocumentsAffected, 1);
            Assert.AreEqual(result.WriteErrors, null);
            if (increment < CNT)
            {
              Assert.AreEqual(result.Upserted, null);
              Assert.AreEqual(result.TotalDocumentsUpdatedAffected, 1);
            }
            else
            {
              Assert.AreEqual(result.Upserted.Length, 1);
              Assert.AreEqual(result.TotalDocumentsUpdatedAffected, 0);
            }
          }
        });

        var all = collection.Find(new Query(@"{}", false));
        for (int i=0; i<SAVE_CNT; i++)
        {
          all.MoveNext();
          Assert.AreEqual(all.Current.Count, 2);
          Assert.AreEqual(((BSONStringElement)all.Current["name"]).Value, "saved");
        }
        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Delete_NoLimit()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<100; i++)
        {
          collection.Insert(new BSONDocument().Set(new BSONStringElement("name", "People" + i))
                                              .Set(new BSONInt32Element("value", i)));
        }

        var result = collection.Delete(new DeleteEntry(
                                         new Query("{ '$or': [{ name: 'People0' }, { '$and': [{ value: { '$gte': 1 }}, { value: { '$lt': 50 }}] }] }", false),
                                         DeleteLimit.None));
        var all = collection.Find(new Query(@"{}", false));

        Assert.AreEqual(50, result.TotalDocumentsAffected);
        Assert.AreEqual(0, result.TotalDocumentsUpdatedAffected);
        Assert.AreEqual(null, result.Upserted);
        Assert.AreEqual(null, result.WriteErrors);
        for (int i=50; i<100; i++)
        {
          all.MoveNext();
          Assert.AreEqual(all.Current.Count, 3);
          Assert.AreEqual(((BSONStringElement)all.Current["name"]).Value, "People"+i);
          Assert.AreEqual(((BSONInt32Element)all.Current["value"]).Value, i);
        }
        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Delete_OnlyFirstMatch()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<100; i++)
        {
          collection.Insert(new BSONDocument().Set(new BSONStringElement("name", "People" + i))
                                              .Set(new BSONInt32Element("value", i)));
        }

        var result1 = collection.Delete(new DeleteEntry(
                                         new Query("{ value: { '$gte': 0 }}", false),
                                         DeleteLimit.OnlyFirstMatch));
        var result2 = collection.Delete(new DeleteEntry(
                                         new Query("{ value: { '$lt': 1000 }}", false),
                                         DeleteLimit.OnlyFirstMatch));
        var all = collection.Find(new Query(@"{}", false));

        Assert.AreEqual(1, result1.TotalDocumentsAffected);
        Assert.AreEqual(0, result1.TotalDocumentsUpdatedAffected);
        Assert.AreEqual(null, result1.Upserted);
        Assert.AreEqual(null, result1.WriteErrors);
        Assert.AreEqual(1, result2.TotalDocumentsAffected);
        Assert.AreEqual(0, result2.TotalDocumentsUpdatedAffected);
        Assert.AreEqual(null, result2.Upserted);
        Assert.AreEqual(null, result2.WriteErrors);
        for (int i=2; i<100; i++)
        {
          all.MoveNext();
          Assert.AreEqual(all.Current.Count, 3);
          Assert.AreEqual(((BSONStringElement)all.Current["name"]).Value, "People"+i);
          Assert.AreEqual(((BSONInt32Element)all.Current["value"]).Value, i);
        }
        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }

    [Test]
    public void Delete_Parallel()
    {
      using (var client = new MongoClient("My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        const int CNT = 100000;
        const int CHUNK = 5000;

        foreach (var value in Enumerable.Range(0, CNT))
        {
          Assert.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", "People" + value))
                                    .Set(new BSONInt32Element("value", value)))
                                       .TotalDocumentsAffected);
        }

        Parallel.For(0, CNT/CHUNK, i =>
        {
          var result = collection.Delete(new DeleteEntry(new Query("{ '$and': [ { value: {'$gte':"+i*CHUNK+"}}, { value: {'$lt':"+(i+1)*CHUNK+"}} ]}", false),
                                                        DeleteLimit.None));
          Assert.AreEqual(result.TotalDocumentsUpdatedAffected, 0);
          Assert.AreEqual(result.TotalDocumentsAffected, CHUNK);
          Assert.AreEqual(result.Upserted, null);
          Assert.AreEqual(result.WriteErrors, null);
        });

        var all = collection.Find(new Query(@"{}", false));
        all.MoveNext();
        Assert.AreEqual(true, all.EOF);
      }
    }
  }
}
