/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Linq;
using System.Text;
using NUnit.Framework;


using NFX.Environment;
using NFX.ApplicationModel;
using NFX.DataAccess.Cache;



namespace NFX.NUnit.Cache
{
    [TestFixture]
    public class StoreTests
    {
 private const string CONF_SRC =@"
 nfx
 {
   cache
   {
     store //this is default store
     {
       parallel-sweep=true
       instrumentation-enabled=true
       table{ name='doctor' bucket-count=1234567 rec-per-page=7 lock-count=19 max-age-sec=193 parallel-sweep=true}
       table{ name='patient' bucket-count=451000000 rec-per-page=17 lock-count=1025 max-age-sec=739 parallel-sweep=true}
     }
     store
     {
       name='banking'
       table{ name='account' bucket-count=789001 rec-per-page=23 lock-count=149 max-age-sec=12000 parallel-sweep=true}
       table{ name='balance' bucket-count=1023 rec-per-page=3 lock-count=11 max-age-sec=230000}

     }    
   }
 }
 ";  
       
        [TestCase]
        public void Configuration_NamedStore()
        {
       
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                using(var store = new CacheStore("BANKING"))
                {
                  store.Configure(null);
            
                  Assert.AreEqual(789001, store.TableOptions["Account"].BucketCount);
                  Assert.AreEqual(23,     store.TableOptions["Account"].RecPerPage);
                  Assert.AreEqual(149,    store.TableOptions["Account"].LockCount);
                  Assert.AreEqual(12000,  store.TableOptions["Account"].MaxAgeSec);
                  Assert.AreEqual(true,   store.TableOptions["Account"].ParallelSweep);

                  Assert.AreEqual(1023,   store.TableOptions["BaLaNCE"].BucketCount);
                  Assert.AreEqual(3,      store.TableOptions["BaLaNCE"].RecPerPage);
                  Assert.AreEqual(11,     store.TableOptions["BaLaNCE"].LockCount);
                  Assert.AreEqual(230000, store.TableOptions["BaLaNCE"].MaxAgeSec);
                  Assert.AreEqual(false,  store.TableOptions["BaLaNCE"].ParallelSweep);

                  var tbl = store["AccoUNT"];
                  Assert.AreEqual(789001,    tbl.BucketCount);
                  Assert.AreEqual(23,        tbl.RecPerPage);
                  Assert.AreEqual(789001*23, tbl.Capacity);
                  Assert.AreEqual(149,       tbl.LockCount);
                  Assert.AreEqual(12000,     tbl.MaxAgeSec);
                  Assert.AreEqual(true,      tbl.ParallelSweep);
                }
            }
        }

        [TestCase]
        public void Configuration_UnNamedStore()
        {
       
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                using(var store = new CacheStore("SomeStoreThat will be configured from default store without name"))
                {
                  store.Configure(null);
            
                  Assert.AreEqual(true, store.ParallelSweep);
                  Assert.AreEqual(true, store.InstrumentationEnabled);

                  Assert.AreEqual(1234567, store.TableOptions["doctor"].BucketCount);
                  Assert.AreEqual(7,       store.TableOptions["doctor"].RecPerPage);
                  Assert.AreEqual(19,      store.TableOptions["doctor"].LockCount);
                  Assert.AreEqual(193,     store.TableOptions["doctor"].MaxAgeSec);
                  Assert.AreEqual(true,    store.TableOptions["doctor"].ParallelSweep);

                  Assert.AreEqual(451000000, store.TableOptions["PATIENT"].BucketCount);
                  Assert.AreEqual(17,        store.TableOptions["PATIENT"].RecPerPage);
                  Assert.AreEqual(1025,      store.TableOptions["PATIENT"].LockCount);
                  Assert.AreEqual(739,       store.TableOptions["PATIENT"].MaxAgeSec);
                  Assert.AreEqual(true,      store.TableOptions["PATIENT"].ParallelSweep);

                }
            }
        }


        [TestCase]
        public void Basic_Put_Get()
        {
            using(var store = new CacheStore(null))
            {
              var tbl1 = store["t1"];
            
              for(int i=0; i<1000; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Froemson-{0}".Args(i), IsGood = i%7==0};
                  Assert.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Assert.AreEqual( 1000, tbl1.Count);

              Assert.AreEqual( "Froemson-12",  tbl1.Get(12)     .ValueAs<businessObject>().Name );
              Assert.AreEqual( "Froemson-115",  store["t1", 115].ValueAs<businessObject>().Name );
              Assert.AreEqual( "Froemson-999", tbl1.Get(999)    .ValueAs<businessObject>().Name );
              Assert.AreEqual( true, tbl1.Get(7).ValueAs<businessObject>().IsGood );
            }
        }

        [TestCase]
        public void Basic_Put_Get_HitCount()
        {
            using(var store = new CacheStore(null))
            {
              var tbl1 = store["t1"];
            
              for(int i=0; i<100; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Froemson-{0}".Args(i), IsGood = i%7==0};
                  Assert.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Assert.AreEqual( 100, tbl1.Count);

              Assert.AreEqual( "Froemson-12",  tbl1.Get(12)     .ValueAs<businessObject>().Name );
              Assert.AreEqual( "Froemson-12",  store["t1", 12]  .ValueAs<businessObject>().Name );
              Assert.AreEqual( true, tbl1.Get(7).ValueAs<businessObject>().IsGood );

              Assert.AreEqual(1, tbl1.Get(0).HitCount);
              Assert.AreEqual(3, tbl1.Get(12).HitCount);
              Assert.AreEqual(2, tbl1.Get(7).HitCount);
            }
        }

        [TestCase]
        public void Basic_Put_Get_Remove()
        {
            using(var store = new CacheStore(null))
            {
              var tbl1 = store["t1"];
            
              for(int i=0; i<1000; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Xroemson-{0}".Args(i), IsGood = i%7==0};
                  Assert.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Assert.AreEqual( 1000, tbl1.Count);

              Assert.IsTrue( tbl1.Remove(115) );

               Assert.AreEqual( 999, tbl1.Count);

              Assert.AreEqual( "Xroemson-12",  tbl1.Get(12)  .ValueAs<businessObject>().Name );
              Assert.AreEqual( null,  tbl1.Get(115) );
              Assert.AreEqual( "Xroemson-999", tbl1.Get(999) .ValueAs<businessObject>().Name );
              Assert.AreEqual( true, tbl1.Get(7).ValueAs<businessObject>().IsGood );
            }
        }


        [TestCase]
        public void Put_Expire_Get()
        {
            using( var store = new CacheStore(null))
            {
              var tbl1 = store["t1"];
            
              for(int i=0; i<1000; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Suvorov-{0}".Args(i), IsGood = i%7==0};
                  Assert.IsTrue( tbl1.Put((ulong)i, bo, i<500 ? 5 : 100 ) );
              }

              Assert.AreEqual( 1000, tbl1.Count);

              //warning: this timeout depends on how fast the store will purge garbage
              for(var spin=0; spin<20 && tbl1.Count!=500; spin++)
              {
                  System.Threading.Thread.Sleep( 1000 );  
              }

              Assert.AreEqual( 500, tbl1.Count);

              Assert.AreEqual( null,  tbl1.Get(1) );
              Assert.AreEqual( null,  tbl1.Get(499) );
              Assert.AreEqual( "Suvorov-500",  tbl1.Get(500).ValueAs<businessObject>().Name );
              Assert.AreEqual( "Suvorov-999",  tbl1.Get(999).ValueAs<businessObject>().Name );
            }
        }

        [TestCase]
        public void Get_Does_Not_See_Expired()
        {
            using(var store = new CacheStore(null))
            {
              var tbl1 = store["t1"];
            
              for(int i=0; i<200; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Suvorov-{0}".Args(i), IsGood = i%7==0};
                  Assert.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Assert.AreEqual( 200, tbl1.Count);

              Assert.IsNotNull( tbl1.Get(123, 3) );//3 sec old
            
              System.Threading.Thread.Sleep( 15000 );// this timeout depends on store sweep thread speed, 15 sec span should be enough  

              Assert.IsNull( tbl1.Get(123, 3) );//3 sec old
              Assert.AreEqual( "Suvorov-123",  tbl1.Get(123, 25).ValueAs<businessObject>().Name ); //25 sec old
            }
        }


        [TestCase]
        public void Get_Does_Not_See_AbsoluteExpired()
        {
            using(var store = new CacheStore(null))
            {
              var tbl1 = store["t1"];
            
              var utcNow = App.TimeSource.UTCNow;
              var utcExp = utcNow.AddSeconds(5);


              var bo1 = new businessObject { ID = 1, Name = "Suvorov-1", IsGood = true};
              Assert.IsTrue( tbl1.Put(1, bo1, maxAgeSec: int.MaxValue, absoluteExpirationUTC: utcExp) );

              var bo2 = new businessObject { ID = 2, Name = "Meduzer-2", IsGood = false};
              Assert.IsTrue( tbl1.Put(2, bo2) );



              Assert.AreEqual( 2, tbl1.Count);
 
              System.Threading.Thread.Sleep( 15000 );// this timeout depends on store sweep thread speed, 15 sec span should be enough  

              Assert.AreEqual( 1, tbl1.Count);

              Assert.IsNull( tbl1.Get(1) );// expired
              Assert.AreEqual( "Meduzer-2",  tbl1.Get(2).ValueAs<businessObject>().Name ); //still there
            }
        }


        [TestCase]
        public void Collision()
        {
            using(var store = new CacheStore(null))
            {
              store.TableOptions.Register( new TableOptions("t1",3 ,7) );
              var tbl1 = store["t1"];
            
              Assert.AreEqual( 21, tbl1.Capacity);

              var obj1 = new businessObject { ID = 0, Name = "Suvorov-1", IsGood = true};
              var obj2 = new businessObject { ID = 21, Name = "Kozloff-21", IsGood = false};
            
              //because of the table size 3 x 7 both 0 and 21 map onto the same slot causing collision
              Assert.IsTrue( tbl1.Put(0, obj1) );
              Assert.IsFalse( tbl1.Put(21, obj2) );

              Assert.IsNull( tbl1.Get(0) );
              Assert.AreEqual( "Kozloff-21",  tbl1.Get(21).ValueAs<businessObject>().Name ); 
            }
        }

        [TestCase]
        public void Collision_Prevented_Priority()
        {
            using(var store = new CacheStore(null))
            {
              store.TableOptions.Register( new TableOptions("t1",3 ,7) );
              var tbl1 = store["t1"];
            
              Assert.AreEqual( 21, tbl1.Capacity);

              //because of the table size 3 x 7 both 0 and 21 map onto the same slot causing collision
              var obj1 = new businessObject { ID = 0,  Name = "Suvorov-1", IsGood = true};
              var obj2 = new businessObject { ID = 21, Name = "Kozloff-21", IsGood = false};
              Assert.IsTrue( tbl1.Put(0, obj1, priority: 10) ); //Suvorov has higher priority than Kozloff
              Assert.IsFalse( tbl1.Put(21, obj2) );             //so collision does not happen

              Assert.AreEqual( "Suvorov-1",  tbl1.Get(0).ValueAs<businessObject>().Name ); 
              Assert.IsNull( tbl1.Get(21) );
            }
        }


        [TestCase]
        public void ComplexKeyHashingStrategy_1()
        {
            using(var store = new CacheStore(null))
            {
              var strat = new ComplexKeyHashingStrategy(store);

              strat.Put("t1", "A", new businessObject { ID = 000,  Name = "Adoyan",   IsGood = true});
              strat.Put("t1", "B", new businessObject { ID = 234,  Name = "Borisenko", IsGood = false});
              strat.Put("t1", "C", new businessObject { ID = 342,  Name = "Carov",    IsGood = true});
              strat.Put("t2", "A", new businessObject { ID = 2000,  Name = "2Adoyan",   IsGood = true});
              strat.Put("t2", "B", new businessObject { ID = 2234,  Name = "2Borisenko", IsGood = false});
              strat.Put("t2", "C", new businessObject { ID = 2342,  Name = "2Carov",    IsGood = true});
            
              Assert.AreEqual("Adoyan",    ((businessObject)strat.Get("t1", "A")).Name);
              Assert.AreEqual("Borisenko", ((businessObject)strat.Get("t1", "B")).Name);
              Assert.AreEqual("Carov",     ((businessObject)strat.Get("t1", "C")).Name);

              Assert.AreEqual("2Adoyan",    ((businessObject)strat.Get("t2", "A")).Name);
              Assert.AreEqual("2Borisenko", ((businessObject)strat.Get("t2", "B")).Name);
              Assert.AreEqual("2Carov",     ((businessObject)strat.Get("t2", "C")).Name);
            }
        }

        [TestCase(1000, 0.1d)]
        [TestCase(10000, 0.25d)]
        [TestCase(100000, 0.5d)]
        public void ComplexKeyHashingStrategy_2(int CNT, double k)
        {
            using(var store = new CacheStore(null))
            {

              var strat = new ComplexKeyHashingStrategy(store);

              for(var i=0; i<CNT; i++)
                strat.Put("t1", "Key-"+i, new businessObject { ID = i,  Name = "Gagarin-"+i,   IsGood = true});
            

              var collision = 0;
              for(var i=0; i<CNT; i++)
              {
                Assert.IsNull( strat.Get("t1", "KeI-"+i) ); 
                Assert.IsNull( strat.Get("t1", "Ke y-"+i) ); 
                Assert.IsNull( strat.Get("t1", "Key "+i) ); 

                var o = strat.Get("t1", "Key-"+i);
                if (o!=null)
                 Assert.AreEqual(i, ((businessObject)o).ID);
                else
                 collision++;
              }

              Console.WriteLine("Did {0}, collision {1}", CNT, collision);
              Assert.IsTrue(collision < (int)(CNT*k));
            }
        }

        

        [TestCase(1000, 0.1d)]
        [TestCase(10000, 0.25d)]
        [TestCase(100000, 0.5d)]
        public void ComplexKeyHashingStrategy_3(int CNT, double k)
        {
            using(var store = new CacheStore(null))
            {
              var strat = new ComplexKeyHashingStrategy(store);

              for(var i=0; i<CNT; i++)
                strat.Put("t1", "ThisIsAKeyLongerThanEight-"+i, new businessObject { ID = i,  Name = "Gagarin-"+i,   IsGood = true});
            
              var collision = 0;
              for(var i=0; i<CNT; i++)
              {
                Assert.IsNull( strat.Get("t1", "ThisAKeyLongerThanEight-"+i) ); 
                Assert.IsNull( strat.Get("t1", "ThusIsAKeyLongerThanEight-"+i) ); 
                Assert.IsNull( strat.Get("t1", "ThisIsAKeyLongerEightThan-"+i) ); 
                var o =  strat.Get("t1", "ThisIsAKeyLongerThanEight-"+i); 

                if (o!=null)
                {
                  Assert.AreEqual(i, ((businessObject)strat.Get("t1", "ThisIsAKeyLongerThanEight-"+i)).ID);
                  Assert.AreEqual(i, ((businessObject)strat.Get("t1", "ThisIsAKeyLongerThanEight-"+i)).ID);
                }
                else
                 collision++;
              }

              Console.WriteLine("Did {0}, collision {1}", CNT, collision);
              Assert.IsTrue(collision < (int)(CNT*k));

            }
        }

        [TestCase]
        public void Hashing_StringKey_1()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("123");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("223");
           Console.WriteLine( h2 );
        }

        [TestCase]
        public void Hashing_StringKey_2()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("abc");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("忁曨曣");
           Console.WriteLine( h2 ); 
           Assert.AreNotEqual(h1, h2);

           var h3 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("忁曣曣");
           Console.WriteLine( h3 ); 
           Assert.AreNotEqual(h3, h2);

           var h4 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("Abc");
           Console.WriteLine( h4 ); 
           Assert.AreNotEqual(h1, h4);
        }



        [TestCase]
        public void Hashing_StringKey_3()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("    012345678");
           Console.WriteLine( h1 );
          
           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Console.WriteLine( h2 ); 
           Assert.AreNotEqual(h1, h2);
        }

        [TestCase]
        public void Hashing_StringKey_4()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Console.WriteLine( h1 );
          
           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Assert.AreEqual(h1, h2);
        }

        [TestCase]
        public void Hashing_StringKey_5()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678 and also some text follows that makes this much longer than others in the same test group");
           Console.WriteLine( h1 );
          
           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Console.WriteLine( h2 ); 
           Assert.AreNotEqual(h1, h2);
        }

        [TestCase]
        public void Hashing_StringKey_6()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678 and also some text follows that makes this much longer than others in the same test group");
           Console.WriteLine( h1 );
          
           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678 and also some text follows that makes this much longer than others in the same test group");
           Assert.AreEqual(h1, h2);
        }



                private class businessObject
                {
                    public int ID;
                    public string Name;
                    public bool IsGood;
                }


    }

                

}