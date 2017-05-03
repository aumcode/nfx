/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.IO;
using NFX.Serialization.Slim;
using NFX.Environment;


namespace NFX.NUnit.AppModel.Pile
{
  [TestFixture]
  public class CacheTest
  {
      [Test]
      public void T010_MainOperations()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A");
          var tB = cache.GetOrCreateTable<string>("B");

          Assert.IsNotNull( tA );
          Assert.IsNotNull( tB );

          Assert.AreEqual(2, cache.Tables.Count );

          Assert.AreEqual( 0, tA.Count);
          Assert.AreEqual( 0, tB.Count);

          Assert.AreEqual(PutResult.Inserted, tA.Put("key1", "avalue1"));
          Assert.AreEqual(PutResult.Inserted, tA.Put("key2", "avalue2"));
          Assert.AreEqual(PutResult.Inserted, tB.Put("key1", "bvalue1"));

          Assert.AreEqual( 3, cache.Count);
          Assert.AreEqual( 2, tA.Count);
          Assert.AreEqual( 1, tB.Count);

          Assert.IsTrue( tA.ContainsKey("key1") );
          Assert.IsTrue( tA.ContainsKey("key2") );
          Assert.IsFalse( tA.ContainsKey("key3 that was never put") );

          Assert.IsTrue( tB.ContainsKey("key1") );
          Assert.IsFalse( tB.ContainsKey("key2") );


          Assert.AreEqual("avalue1", tA.Get("key1"));
          Assert.AreEqual("avalue2", tA.Get("key2"));
          Assert.AreEqual(null, tA.Get("key3"));

          Assert.AreEqual("bvalue1", tB.Get("key1"));

          Assert.AreEqual("avalue1", cache.GetTable<string>("A").Get("key1"));
          Assert.AreEqual("bvalue1", cache.GetTable<string>("B").Get("key1"));

          Assert.IsTrue( tA.Remove("key1"));
          Assert.IsFalse( tA.Remove("key2342341"));

          Assert.AreEqual( 2, cache.Count);
          Assert.AreEqual( 1, tA.Count);
          Assert.AreEqual( 1, tB.Count);

          cache.PurgeAll();
          Assert.AreEqual( 0, cache.Count);
          Assert.AreEqual( 0, tA.Count);
          Assert.AreEqual( 0, tB.Count);

          Assert.AreEqual( 0, cache.Pile.UtilizedBytes );
          Assert.AreEqual( 0, cache.Pile.ObjectCount );
        }
      }


      [Test]
      public void T020_Comparers()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A", StringComparer.Ordinal);
          var tB = cache.GetOrCreateTable<string>("B", StringComparer.OrdinalIgnoreCase);

          Assert.AreEqual(PutResult.Inserted, tA.Put("key1", "avalue1"));
          Assert.AreEqual(PutResult.Inserted, tA.Put("Key1", "avalue2"));
          Assert.AreEqual(PutResult.Inserted, tB.Put("key1", "bvalue1"));
          Assert.AreEqual(PutResult.Replaced, tB.Put("Key1", "bvalue2"));

          Assert.AreEqual( 2, tA.Count);
          Assert.AreEqual( 1, tB.Count);

          Assert.AreEqual("avalue1", tA.Get("key1"));
          Assert.AreEqual("avalue2", tA.Get("Key1"));
          Assert.AreEqual(null, tA.Get("key3"));

          Assert.AreEqual("bvalue2", tB.Get("key1"));
          Assert.AreEqual("bvalue2", tB.Get("Key1"));
        }
      }

      [Test]
      [ExpectedException(typeof(PileCacheException), ExpectedMessage="comparer mismatch", MatchType=MessageMatch.Contains)]
      public void T030_Comparers()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A", StringComparer.Ordinal);
          var tB = cache.GetOrCreateTable<string>("A", StringComparer.OrdinalIgnoreCase);
        }
      }

      [Test]
      public void T040_PileOwnership()
      {
        var cache = new LocalCache();
        cache.Pile = new DefaultPile(cache);
        cache.Configure(null);
        cache.Start();


        var tA = cache.GetOrCreateTable<string>("A");
        tA.Put("aaa", "avalue");
        Assert.AreEqual("avalue", tA.Get("aaa"));

        cache.WaitForCompleteStop();
        Assert.AreEqual(NFX.ServiceModel.ControlStatus.Inactive, cache.Status);
        Assert.AreEqual(NFX.ServiceModel.ControlStatus.Inactive, cache.Pile.Status);
      }

      [Test]
      [ExpectedException(typeof(PileCacheException), ExpectedMessage="has not been started", MatchType=MessageMatch.Contains)]
      public void T050_PileNonOwnershipErrorStart()
      {
        var pile = new DefaultPile();

        try
        {
          var cache = new LocalCache();
          cache.Pile = pile;
          cache.Configure(null);
          cache.Start();  //can not start cache that uses inactive pile which is not managed by this cache
        }
        finally
        {
          pile.Dispose();
        }

      }


      [Test]
      public void T060_PileNonOwnership()
      {
        var pile = new DefaultPile();
        pile.Start();
        try
        {
              var cache = new LocalCache();
              cache.Pile = pile;
              cache.Configure(null);
              cache.Start();


              var tA = cache.GetOrCreateTable<string>("A");
              tA.Put("aaa", "avalue");
              tA.Put("bbb", "bvalue");
              Assert.AreEqual("avalue", tA.Get("aaa"));

              Assert.AreEqual(2, cache.Count);
              Assert.AreEqual(2, pile.ObjectCount);

              cache.WaitForCompleteStop();
              Assert.AreEqual(NFX.ServiceModel.ControlStatus.Inactive, cache.Status);

              Assert.AreEqual(NFX.ServiceModel.ControlStatus.Active, pile.Status);
              Assert.AreEqual(0, pile.ObjectCount);

              cache = new LocalCache();
              cache.Pile = pile;
              cache.Configure(null);
              cache.Start();

              var tAbc = cache.GetOrCreateTable<string>("Abc");
              tAbc.Put("aaa", "avalue");
              tAbc.Put("bbb", "bvalue");
              tAbc.Put("ccc", "cvalue");
              tAbc.Put("ddd", "cvalue");

              Assert.AreEqual(4, pile.ObjectCount);

              var cache2 = new LocalCache();
              cache2.Pile = pile;
              cache2.Configure(null);
              cache2.Start();


              var t2 = cache2.GetOrCreateTable<string>("A");
              t2.Put("aaa", "avalue");
              t2.Put("bbb", "bvalue");

              Assert.AreEqual(2, cache2.Count);
              Assert.AreEqual(6, pile.ObjectCount);

              cache.WaitForCompleteStop();
              Assert.AreEqual(NFX.ServiceModel.ControlStatus.Active, pile.Status);
              Assert.AreEqual(2, pile.ObjectCount);

              cache2.WaitForCompleteStop();
              Assert.AreEqual(NFX.ServiceModel.ControlStatus.Active, pile.Status);
              Assert.AreEqual(0, pile.ObjectCount);

              pile.WaitForCompleteStop();
              Assert.AreEqual(NFX.ServiceModel.ControlStatus.Inactive, pile.Status);
        }
        finally
        {
          pile.Dispose();
        }
      }


      [Test]
      public void T080_PutGetWithoutMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
       ////   tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
           // Console.WriteLine("{0} -> {1}", i, pr);
            Assert.IsTrue( pr == PutResult.Inserted);//no overwrite because table keeps growing
          }

          for(var i=0;i<CNT; i++)
          {
            var v = tA.Get(i);
           // Console.WriteLine("{0} -> {1}", i, v);
            Assert.IsTrue( v.Equals("value"+i.ToString()));
          }
        }
      }

      [Test]
      public void T090_PutGetWithMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
          tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          var lst = new List<PutResult>();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Assert.IsTrue( pr == PutResult.Inserted || pr == PutResult.Overwritten);//the table is capped, so some values will be overwritten

            lst.Add(pr);
          }

          Assert.IsTrue( lst.Count(r => r==PutResult.Inserted)>0 );
          Assert.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );


          for(var i=0;i<CNT; i++)
          {
            var v = tA.Get(i);
          //  Console.WriteLine("{0} -> {1}", i, v);
            Assert.IsTrue( v==null || v.Equals("value"+i.ToString()));
          }
        }
      }

      [Test]
      public void T100_OverwriteWithMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
          tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          var lst = new List<PutResult>();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Assert.IsTrue( pr == PutResult.Inserted || pr == PutResult.Overwritten);//the table is capped, so some values will be overwritten
            lst.Add(pr);
          }

          Assert.IsTrue( lst.Count(r => r==PutResult.Inserted)>0 );
          Assert.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );

          lst.Clear();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Assert.IsTrue( pr == PutResult.Replaced || pr==PutResult.Overwritten  );
            lst.Add(pr);
          }


          Assert.IsTrue( lst.Count(r => r==PutResult.Replaced)>0 );
          Assert.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );

          Assert.IsTrue( lst.Count(r => r==PutResult.Overwritten)>lst.Count(r => r==PutResult.Replaced) );

        }
      }


      [Test]
      public void T110_PriorityCollideWithMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
          tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          var lst = new List<PutResult>();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);//PRIORITY +10!
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Assert.IsTrue( pr == PutResult.Inserted || pr == PutResult.Overwritten);//the table is capped, so some values will be overwritten
            lst.Add(pr);
          }

          Assert.IsTrue( lst.Count(r => r==PutResult.Inserted)>0 );
          Assert.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );

          lst.Clear();
          for(var i=CNT;i<2*CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: -10);//only collision, because priority is lowe r than what already exists
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Assert.IsTrue( pr==PutResult.Collision  );
            lst.Add(pr);
          }
        }
      }



      [TestCase(100)]
      public void T130_KeyInt_ManyPutGet(int cnt)
      {
        PileCacheTestCore.KeyInt_ManyPutGet(cnt);
      }


      [TestCase(100)]
      public void T140_KeyGDID_ManyPutGet(int cnt)
      {
        PileCacheTestCore.KeyGDID_ManyPutGet(cnt);
      }


      [TestCase(100)]
      public void T150_KeyString_ManyPutGet(int cnt)
      {
        PileCacheTestCore.KeyString_ManyPutGet(cnt);
      }

      [TestCase(25,    10000, 4*1024)]
      [TestCase(10,   300000, 16)]
      [TestCase(10,   100, 1024*1024)]
      [TestCase(10,   20, 8*1024*1024)]
      public void T160_ResizeTable(int cnt, int rec, int payload)
      {
        PileCacheTestCore.ResizeTable(cnt, rec, payload);
      }

      [Test]
      public void T170_Config()
      {
        var conf1 =
@"
store
{
        default-table-options
        {
          initial-capacity=1000000
          detailed-instrumentation=true
        }

        table
        {
          name='A'
          minimum-capacity=800000
          maximum-capacity=987654321
          initial-capacity=780000
          growth-factor=2.3
          shrink-factor=0.55
          load-factor-lwm=0.1
          load-factor-hwm=0.9
          default-max-age-sec=145
          detailed-instrumentation=true
        }

        table
        {
          name='B'
          maximum-capacity=256000
          detailed-instrumentation=false
        }

}
";
        var c1 = conf1.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        using (var cache = PileCacheTestCore.MakeCache(c1))
        {
          var tA = cache.GetOrCreateTable<int>("A");

          var topt = cache.DefaultTableOptions;
          Assert.AreEqual(1000000,    topt.InitialCapacity);
          Assert.AreEqual(true,       topt.DetailedInstrumentation);

          topt = cache.TableOptions["a"];
          Assert.AreEqual(800000,    topt.MinimumCapacity);
          Assert.AreEqual(987654321, topt.MaximumCapacity);
          Assert.AreEqual(780000,    topt.InitialCapacity);
          Assert.AreEqual(2.3d,      topt.GrowthFactor);
          Assert.AreEqual(0.55d,     topt.ShrinkFactor);
          Assert.AreEqual(0.1d,      topt.LoadFactorLWM);
          Assert.AreEqual(0.9d,      topt.LoadFactorHWM);
          Assert.AreEqual(145,       topt.DefaultMaxAgeSec);
          Assert.AreEqual(true,      topt.DetailedInstrumentation);

          topt = cache.GetOrCreateTable<int>("A").Options;
          Assert.AreEqual(800000,    topt.MinimumCapacity);
          Assert.AreEqual(987654321, topt.MaximumCapacity);
          Assert.AreEqual(780000,    topt.InitialCapacity);
          Assert.AreEqual(2.3d,      topt.GrowthFactor);
          Assert.AreEqual(0.55d,     topt.ShrinkFactor);
          Assert.AreEqual(0.1d,      topt.LoadFactorLWM);
          Assert.AreEqual(0.9d,      topt.LoadFactorHWM);
          Assert.AreEqual(145,       topt.DefaultMaxAgeSec);
          Assert.AreEqual(true,      topt.DetailedInstrumentation);

          cache.GetOrCreateTable<int>("A").Options.DefaultMaxAgeSec = 197;
          Assert.AreEqual(197,       cache.GetOrCreateTable<int>("A").Options.DefaultMaxAgeSec);
          Assert.AreEqual(145,       cache.TableOptions["a"].DefaultMaxAgeSec);

          topt = cache.GetOrCreateTable<int>("b").Options;
          Assert.AreEqual(256000, topt.MaximumCapacity);
          Assert.AreEqual(false,  topt.DetailedInstrumentation);
        }
      }



      [Test]
      public void T180_GetOrPut()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");

          tA.Put(1, "value 1");
          tA.Put(122, "value 122");

          PutResult? pResult;
          var v = tA.GetOrPut(2, (t, k, _) => "value "+k.ToString(), null, out pResult);
          Assert.AreEqual( "value 2", v);
          Assert.IsTrue( pResult.HasValue );
          Assert.AreEqual( PutResult.Inserted, pResult.Value);

          Assert.AreEqual(3, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "value 2");
          tA.Put(122, "value 122");


          v = tA.GetOrPut(1, (t, k, _) => "value "+k.ToString(), null, out pResult);
          Assert.AreEqual( "value 1", v);
          Assert.IsFalse( pResult.HasValue );

          Assert.AreEqual(3, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "value 2");
          tA.Put(122, "value 122");

          v = tA.GetOrPut(777, (t, k, _) => "value "+k.ToString(), null, out pResult);
          Assert.AreEqual( "value 777", v);
          Assert.IsTrue( pResult.HasValue );
          Assert.AreEqual( PutResult.Inserted, pResult.Value);


          Assert.AreEqual(4, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "value 2");
          tA.Put(122, "value 122");
          tA.Put(777, "value 777");

          pResult = tA.Put(2, "mod value 2");
          Assert.AreEqual( PutResult.Replaced, pResult.Value);


          Assert.AreEqual(4, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "mod value 2");
          tA.Put(122, "value 122");
          tA.Put(777, "value 777");
        }
      }

      [TestCase(100000, 1)]
      [TestCase(100000, 16)]
      [TestCase(100000, 512)]
      public void T190_FID_PutGetCorrectness(int cnt, int tbls)
      {
        PileCacheTestCore.FID_PutGetCorrectness(cnt, tbls);
      }

      [TestCase(5,  7,    100,  2)]
      [TestCase(16, 7,   2500,  4)]

      [TestCase(5,  20, 50000, 2)]
      [TestCase(16, 20, 15000, 4)]
      public void T9000000_ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
      {
        PileCacheTestCore.ParalellGetPutRemove(workers, tables, putCount, durationSec);
      }

  }
}
