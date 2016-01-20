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
  public class CacheTest : HighMemoryLoadTest
  {
    #region Tests

      [Test]
      public void T010_MainOperations()
      {
        using (var cache = makeCache())
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
        using (var cache = makeCache())
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
        using (var cache = makeCache())
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
      public void T070_DoesNotSeeAgedOrExpired()
      {
        using (var cache = makeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A");
          var tB = cache.GetOrCreateTable<string>("B");
          var tC = cache.GetOrCreateTable<string>("C");
          tC.Options.DefaultMaxAgeSec = 4;
          var tD = cache.GetOrCreateTable<string>("D");

          Assert.AreEqual(PutResult.Inserted, tA.Put("key1", "value1")); //does not expire by itself
          Assert.AreEqual(PutResult.Inserted, tB.Put("key1", "value1", 7));//will expire in 7 seconds
          Assert.AreEqual(PutResult.Inserted, tC.Put("key1", "value1"));//will expire in Options.DefaultMaxAgeSec
          Assert.AreEqual(PutResult.Inserted, tD.Put("key1", "value1", absoluteExpirationUTC: DateTime.UtcNow.AddSeconds(4) ));//will expire at specific time


          Assert.AreEqual("value1", tA.Get("key1"));
          Assert.AreEqual("value1", tA.Get("key1", 3));

          Assert.AreEqual("value1", tB.Get("key1"));

          Assert.AreEqual("value1", tC.Get("key1"));
          
          Assert.AreEqual("value1", tD.Get("key1"));


          Thread.Sleep(20000);// wait long enough to cover a few swep cycles (that may be 5+sec long)


          Assert.AreEqual("value1", tA.Get("key1"));
          Assert.AreEqual(null, tA.Get("key1", 3)); //did not expire, but aged over get limit
          Assert.AreEqual(null, tB.Get("key1")); //expired because of put with time limit
          Assert.AreEqual(null, tC.Get("key1"));//expired because of Options
          Assert.AreEqual(null, tD.Get("key1"));//expired because of absolute expiration on put
        }
      }

      [Test]
      public void T080_PutGetWithoutMaxCap()
      {
        using (var cache = makeCache())
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
        using (var cache = makeCache())
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
        using (var cache = makeCache())
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
        using (var cache = makeCache())
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




      [Test]
      public void T120_Rejuvenate()
      {
        using (var cache = makeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A");

          Assert.AreEqual(PutResult.Inserted, tA.Put("key1", "value1", 12));
          Assert.AreEqual(PutResult.Inserted, tA.Put("key2", "value2", 12));
          Assert.AreEqual(PutResult.Inserted, tA.Put("key3", "value3", 12));



          Assert.AreEqual("value1", tA.Get("key1"));
          Assert.AreEqual("value2", tA.Get("key2"));
          Assert.AreEqual("value3", tA.Get("key3"));

          for(var i=0; i<30; i++)
          {
            Thread.Sleep(1000);
            Console.WriteLine("Second {0}   Load Factor {1}", i, tA.LoadFactor);
            Assert.IsTrue( tA.Rejuvenate("key2") );
          }
          
          Assert.AreEqual(null, tA.Get("key1"));
          Assert.AreEqual("value2", tA.Get("key2"));//this is still here because it got rejuvenated
          Assert.AreEqual(null, tA.Get("key3"));

          Thread.Sleep(30000);
          Assert.AreEqual(null, tA.Get("key2"));//has died too
          Assert.AreEqual(0, tA.Count);
        }
      }

      [TestCase(100)]
      [TestCase(10000)]
      [TestCase(1000000)]
      public void T130_KeyInt_ManyPutGet(int cnt)
      {
        using (var cache = makeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");

          for(var i=0 ; i<cnt;i++)
           tA.Put(i, "value-"+i.ToString());

          for(var i=0 ; i<cnt;i++)
           Assert.AreEqual( "value-"+i.ToString(), tA.Get( i ));
        }
      }


      [TestCase(100)]
      [TestCase(10000)]
      [TestCase(1000000)]
      public void T140_KeyGDID_ManyPutGet(int cnt)
      {
        using (var cache = makeCache())
        {
          var tA = cache.GetOrCreateTable<GDID>("A");

          for(var i=0 ; i<cnt;i++)
           tA.Put( new GDID(0, (ulong)i), "value-"+i.ToString());

          for(var i=0 ; i<cnt;i++)
           Assert.AreEqual( "value-"+i.ToString(), tA.Get( new GDID(0, (ulong)i) ));
        }
      }


      [TestCase(100)]
      [TestCase(10000)]
      [TestCase(1000000)]
      public void T150_KeyString_ManyPutGet(int cnt)
      {
        using (var cache = makeCache())
        {
          cache.TableOptions.Register( new TableOptions("A")
          {
            InitialCapacity = cnt//*2,
         //   MaximumCapacity = cnt*2
          });
         
          var tA = cache.GetOrCreateTable<string>("A", StringComparer.Ordinal);

         

          for(var i=0 ; i<cnt;i++)
          {
           tA.Put( i.ToString(), "value-"+i.ToString());
          }
          
          var hit = 0;
          var miss = 0;
          for(var i=0 ; i<cnt;i++)
          {
            var d = tA.Get( i.ToString() );
            if (d==null) miss++;
            else
            {
              hit++;
              Assert.AreEqual( "value-"+i.ToString(), d);
            }
          }
          Console.WriteLine("Did {0:n0} ->  hits: {1:n0} ({2:n0}%)  ;  misses: {3:n0} ({4:n0}%)", cnt, hit, (hit/(double)cnt)*100d, miss, (miss/(double)cnt)*100d);

          Assert.IsTrue(hit > miss);
          Assert.IsTrue((miss/(double)cnt) <= 0.1d);//misses <=10%
        }
      }





      [TestCase(25,    10000, 4*1024)]
      [TestCase(10,   300000, 16)]
      [TestCase(10,   100, 1024*1024)]
      [TestCase(10,   20, 8*1024*1024)]
      public void T160_ResizeTable(int cnt, int rec, int payload)
      {
        using (var cache = makeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");

          Parallel.For(0, cnt,
          (c) =>
          {
              var put = ExternalRandomGenerator.Instance.NextScaledRandomInteger(1, rec);
              for(var i=0; i<put; i++)
               tA.Put(i, new byte[ExternalRandomGenerator.Instance.NextScaledRandomInteger(1, payload)]);

              var remove = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, rec);
              for(var i=0; i<remove; i++)
               tA.Remove(i);

              Console.WriteLine("{0}% done", (100*c) / cnt);
          });
         
          for(var i=0; i<rec; i++)
               tA.Remove(i);

          Assert.AreEqual(0, tA.Count);
          Assert.AreEqual(0, cache.Pile.UtilizedBytes);
          Assert.AreEqual(0, cache.Pile.ObjectCount);
        }
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
        using (var cache = makeCache(c1))
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
        using (var cache = makeCache())
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



      [TestCase( 1000000, 1)]
      [TestCase( 1000000, 16)]
      [TestCase( 1000000, 512)]
      [TestCase(10000000, 1)]
      [TestCase(10000000, 16)]
      [TestCase(10000000, 512)]
      public void T190_FID_PutGetCorrectness(int cnt, int tbls)
      {
        
        var sw = new System.Diagnostics.Stopwatch();
        var dicts = new ConcurrentDictionary<FID, string>[tbls];
        for(var i = 0; i<dicts.Length; i++) dicts[i] = new ConcurrentDictionary<FID, string>();
        var notInserted = 0;
        using (var cache = makeCache())
        {
            sw.Start();
            Parallel.For(0, cnt, (i) =>
            {
               var t = i % tbls;
               var tbl = cache.GetOrCreateTable<FID>(t.ToString());

               var key = FID.Generate();
               var data = NFX.Parsing.NaturalTextGenerator.Generate(0);

               var pr = tbl.Put(key, data);
               dicts[t].TryAdd(key, data);
               if (pr!=PutResult.Inserted)
                 Interlocked.Increment(ref notInserted);
            });
        

            var elapsed = sw.ElapsedMilliseconds;
            var inserted = cnt - notInserted;
            Console.WriteLine("Population of {0:n0} in {1:n0} msec at {2:n0}ops/sec", cnt, elapsed, cnt /  (elapsed / 1000d) );
            Console.WriteLine("  inserted: {0:n0} ({1:n0}%)", inserted, (int) (100 * (inserted / (double)cnt) ) );
            Console.WriteLine("  not-inserted: {0:n0} ({1:n0}%)", notInserted, (int) (100 * (notInserted) / (double)cnt));
            sw.Restart();

             var found = 0;
             var notfound = 0;

            for(var i=0; i< tbls; i++)
            {
             var tbl = cache.GetOrCreateTable<FID>(i.ToString()); 
             var dict = dicts[i];
            
             Parallel.ForEach(dict, (kvp)=>
             {
                var data = tbl.Get( kvp.Key );
                if (data!=null)
                {
                  Assert.AreEqual(data, kvp.Value );
                  Interlocked.Increment(ref found);
                }
                else
                  Interlocked.Increment(ref notfound);
             });
            }
            
            elapsed = sw.ElapsedMilliseconds;
            var totalGot = found + notfound;
            Console.WriteLine("Got of {0:n0} in {1:n0} msec at {2:n0}ops/sec", totalGot, elapsed, totalGot /  (elapsed / 1000d) );
            Console.WriteLine("  found: {0:n0} ({1:n0}%)", found, (int) (100 * (found / (double)totalGot) ) );
            Console.WriteLine("  not-found: {0:n0} ({1:n0}%)", notfound, (int) (100 * (notfound) / (double)totalGot));

            Assert.IsTrue( (found / (double)inserted) > 0.9d);
        }//using cache
      }




      [TestCase(5,  7,    1000,  20)]
      [TestCase(16, 7,   25000,  40)]

      [TestCase(5,  20,  50000, 20)]
      [TestCase(16, 20, 150000, 40)]
      public void T9000000_ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
      {
        var start =DateTime.UtcNow;

        using (var cache = makeCache())
        {
          var tasks = new Task[workers];

          var totalGet = 0;
          var totalPut = 0;
          var totalRem = 0;

          var getFound = 0;
          var putInsert = 0;
          var removed = 0;

          for(var i=0; i<workers; i++)
             tasks[i] = 
              Task.Factory.StartNew( 
               ()=>
               {
                 while(true)
                 {
                   var now = DateTime.UtcNow;
                   if ((now-start).TotalSeconds>=durationSec) return;

                   var t = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, tables);
                   var tbl = cache.GetOrCreateTable<string>("tbl_"+t);

                   var get = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putCount*2, putCount * 4);
                   var put = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putCount / 2, putCount);
                   var remove = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, putCount);

                   Interlocked.Add(ref totalGet, get);
                   Interlocked.Add(ref totalPut, put);
                   Interlocked.Add(ref totalRem, remove);

                   for(var j=0; j<get; j++) 
                    if (null!=tbl.Get( NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 1000000).ToString() )) Interlocked.Increment(ref getFound);

                   for(var j=0; j<put; j++) 
                    if (PutResult.Inserted==tbl.Put( 
                                                     NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 1000000).ToString(),
                                                     NFX.Parsing.NaturalTextGenerator.Generate()                     
                                                   )) Interlocked.Increment(ref putInsert);
                  
                   for(var j=0; j<remove; j++) 
                    if (tbl.Remove( 
                                   NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 1000000).ToString()
                                  )) Interlocked.Increment(ref removed);


                 }
               }
              , TaskCreationOptions.LongRunning );

          Task.WaitAll( tasks );

          Console.WriteLine("{0} workers, {1} tables, {2} put, {3} sec duration", workers, tables, putCount, durationSec);
          Console.WriteLine("-----------------------------------------------------------");

          Console.WriteLine("Total Gets: {0:n0}, found {1:n0}", totalGet, getFound);
          Console.WriteLine("Total Puts: {0:n0}, inserted {1:n0}", totalPut, putInsert);
          Console.WriteLine("Total Removes: {0:n0}, removed {1:n0}", totalRem, removed);

          Console.WriteLine("Pile utilized bytes: {0:n0}", cache.Pile.UtilizedBytes);
          Console.WriteLine("Pile object count: {0:n0}", cache.Pile.ObjectCount);

          cache.PurgeAll();

          Assert.AreEqual(0, cache.Count);
          Assert.AreEqual(0, cache.Pile.UtilizedBytes);
          Assert.AreEqual(0, cache.Pile.ObjectCount);


          Console.WriteLine();
        }
      }




    #endregion

        private LocalCache makeCache(IConfigSectionNode conf = null)
        {
           var cache = new LocalCache();
           cache.Pile = new DefaultPile(cache);
           cache.Configure( conf );
           cache.Start();
           return cache;
        }


  }
}
