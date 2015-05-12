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
using System.Linq;
using System.Text;
using System.Diagnostics;
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

namespace NFX.NUnit.AppModel.Pile
{
  [TestFixture]
  public class PileTest
  {
      [TestFixtureSetUp]
      public void RigSetup()
      {
        var ms = NFX.OS.Computer.GetMemoryStatus();

        const ulong MIN = 64ul * 1000ul * 1000ul * 1000ul;

        var has = ms.TotalPhysicalBytes;
        if (has < MIN)
           Assert.Ignore("The machine has to have at least {0:n0} bytes of ram for this test, but it only has {1:n0} bytes".Args(MIN, has));
      }

      [SetUp]
      public void SetUp()
      {
        GC.Collect();
      }

      [TearDown]
      public void TearDown()
      {
        GC.Collect();
      }


      [Test]
      public void Initial()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(0, ipile.UtilizedBytes);
          Assert.AreEqual(0, ipile.OverheadBytes);
          Assert.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Test]
      public void PutWOStart()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;

          var row = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Assert.IsFalse(pp.Valid);

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(0, ipile.UtilizedBytes);
          Assert.AreEqual(0, ipile.OverheadBytes);
          Assert.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Test]
      public void GetWOStart()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;
          var obj = ipile.Get(PilePointer.Invalid);
          Assert.IsNull(obj);
        }
      }

      [Test]
      public void PutOne()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var row = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Assert.AreEqual(1, ipile.ObjectCount);
          Assert.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(1, ipile.SegmentCount);
        }
      } 

      [Test]
      public void PutTwo()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var rowIn1 = Charge.MakeFake(new GDID(0, 1));
          var rowIn2 = Charge.MakeFake(new GDID(0, 2));

          var pp1 = ipile.Put(rowIn1);
          var pp2 = ipile.Put(rowIn2);

          Assert.AreEqual(2, ipile.ObjectCount);
          Assert.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(1, ipile.SegmentCount);

          var rowOut1 = pile.Get(pp1) as Charge;
          var rowOut2 = pile.Get(pp2) as Charge;

          Assert.AreEqual(rowIn1, rowOut1);
          Assert.AreEqual(rowIn2, rowOut2);
        }
      } 

      [Test]
      public void PutGetObject()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          var rowOut = ipile.Get(pp) as Charge;

          Assert.AreEqual(rowIn, rowOut);
        }
      } 


      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void GetNoObject()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Get(PilePointer.Invalid);
        }
      } 

      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void DeleteInvalid()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Delete(PilePointer.Invalid);
        }
      } 

      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void DeleteExisting()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Delete(pp);

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(0, ipile.UtilizedBytes);
          Assert.AreEqual(0, ipile.OverheadBytes);
          Assert.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);
        }
      } 

      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void Purge()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Purge();

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);


        }
      } 

      [Test]
      public void PutCheckerboardPattern2()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 100;

          var ppp = new Tuple<PilePointer, Charge>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var ch = Charge.MakeFake(new GDID(0, i));
            ppp[i] = new Tuple<PilePointer,Charge>( ipile.Put(ch), ch);
          }

          Assert.AreEqual(CNT, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            var ch = ipile.Get(ppp[i].Item1);
            Assert.AreEqual(ch, ppp[i].Item2);
          }

          for(ulong i = 0; i < CNT; i+=2)
            ipile.Delete(ppp[i].Item1);

          Assert.AreEqual(CNT/2, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 2 == 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Assert.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Assert.AreEqual(ch, ppp[i].Item2);
            }
          }
        }  
      }

      [Test]
      public void PutCheckerboardPattern3()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 123;

          var ppp = new Tuple<PilePointer, string>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var str = NFX.Parsing.NaturalTextGenerator.Generate(179);
            ppp[i] = new Tuple<PilePointer,string>( ipile.Put(str), str);
          }

          Assert.AreEqual(CNT, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
              ipile.Delete(ppp[i].Item1);
          }

          Assert.AreEqual(CNT/3, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Assert.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Assert.AreEqual(ch, ppp[i].Item2);
            }
          }

          ////Console.WriteLine("ObjectCount: {0}", ipile.ObjectCount);
          ////Console.WriteLine("AllocatedMemoryBytes: {0}", ipile.AllocatedMemoryBytes);
          ////Console.WriteLine("UtilizedBytes: {0}", ipile.UtilizedBytes);
          ////Console.WriteLine("OverheadBytes: {0}", ipile.OverheadBytes);
          ////Console.WriteLine("SegmentCount: {0}", ipile.SegmentCount);
        }  
      }


           private class dummy
           {
             public byte[] bin;
           }

      
      [TestCase(false, 1000000, 0, 40, true)]
      [TestCase(false,  100000, 0, 50000, true)]
      [TestCase(false,   10000, 70000, 150000, true)]
      [TestCase(false,   50000, 0, 150000, true)]

      [TestCase(true, 1000000, 0, 40, true)]
      [TestCase(true,  100000, 0, 50000, true)]
      [TestCase(true,   10000, 70000, 150000, true)]
      [TestCase(true,   50000, 0, 150000, true)]

      [TestCase(false, 1000000, 0, 40, false)]
      [TestCase(false,  100000, 0, 50000, false)]
      [TestCase(false,   10000, 70000, 150000, false)]
      [TestCase(false,   50000, 0, 150000, false)]

      [TestCase(true, 1000000, 0, 40, false)]
      [TestCase(true,  100000, 0, 50000, false)]
      [TestCase(true,   10000, 70000, 150000, false)]
      [TestCase(true,   50000, 0, 150000, false)]
      public void VarSizes_Checkboard(bool isParallel, int cnt, int minSz, int maxSz, bool speed)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
          
          var tasks = new List<Task>();
          for(var t=0; t < (isParallel? (System.Environment.ProcessorCount - 1) : 1); t++)
           tasks.Add(
                  Task.Run( () =>
                  {
                     var dict = new Dictionary<PilePointer, dummy>();
                     var priorOdd = PilePointer.Invalid;
                     for(var i=0; i<cnt; i++)
                     {
                       var even = (i&0x01)==0;
                       var data = new dummy{ bin = new byte[12 + NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(minSz, maxSz)]};
                       data.bin.WriteBEInt32(0, ExternalRandomGenerator.Instance.NextRandomInteger);
                       data.bin.WriteBEInt32(data.bin.Length-4, ExternalRandomGenerator.Instance.NextRandomInteger);
                       var ptr = pile.Put(data);
                       Assert.IsTrue( ptr.Valid );
               
                       if (even)
                        dict.Add(ptr, data);
                       else
                       {
                        if (priorOdd.Valid)
                         Assert.IsTrue( pile.Delete(priorOdd) );
                        priorOdd = ptr;
                       }


                       if (i%1000==0)
                        Console.WriteLine("Thread{0} did {1}; allocated {2} bytes, utilized {3} bytes by {4} objects {5} bytes/obj. ",
                                          Thread.CurrentThread.ManagedThreadId,
                                           i,
                                           pile.AllocatedMemoryBytes,
                                           pile.UtilizedBytes,
                                           pile.ObjectCount,
                                           pile.UtilizedBytes / pile.ObjectCount);

                     }
                     Console.WriteLine("Thread {0} Population done, now checking the buffers... {1}",Thread.CurrentThread.ManagedThreadId, DateTime.Now);

                     foreach(var entry in dict)
                       Assert.IsTrue( NFX.IOMiscUtils.MemBufferEquals(entry.Value.bin, (pile.Get(entry.Key) as dummy).bin));

                     Console.WriteLine("Thread {0} DONE. {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                  })
            );//add
            
          Task.WaitAll( tasks.ToArray() );            
        }

      }


      [TestCase(false, 1000000, 0, 256, false, true)]
      [TestCase(false,  250000, 0, 8000, false, true)]
      [TestCase(false,  150000, 0, 24000, false, true)]
      [TestCase(false,   21000, 65000, 129000, false, true)]

      [TestCase(true, 1000000, 0, 256, false, true)]
      [TestCase(true,  250000, 0, 8000, false, true)]
      [TestCase(true,  150000, 0, 24000, false, true)]
      [TestCase(true,   21000, 65000, 129000, false, true)]


      [TestCase(false, 1000000, 0, 256, true, true)]
      [TestCase(false,  250000, 0, 8000, true, true)]
      [TestCase(false,  150000, 0, 24000, true, true)]
      [TestCase(false,   21000, 65000, 129000, true, true)]

      [TestCase(true, 1000000, 0, 256, true, true)]
      [TestCase(true,  250000, 0, 8000, true, true)]
      [TestCase(true,  150000, 0, 24000, true, true)]
      [TestCase(true,   21000, 65000, 129000, true, true)]



      [TestCase(false, 1000000, 0, 256, false, false)]
      [TestCase(false,  250000, 0, 8000, false, false)]
      [TestCase(false,  150000, 0, 24000, false, false)]
      [TestCase(false,   12000, 65000, 129000, false, false)]

      [TestCase(true, 1000000, 0, 256, false, false)]
      [TestCase(true,  250000, 0, 8000, false, false)]
      [TestCase(true,  150000, 0, 24000, false, false)]
      [TestCase(true,   12000, 65000, 129000, false, false)]


      [TestCase(false, 1000000, 0, 256, true, false)]
      [TestCase(false,  250000, 0, 8000, true, false)]
      [TestCase(false,  150000, 0, 24000, true, false)]
      [TestCase(false,   12000, 65000, 129000, true, false)]

      [TestCase(true, 1000000, 0, 256, true, false)]
      [TestCase(true,  250000, 0, 8000, true, false)]
      [TestCase(true,  150000, 0, 24000, true, false)]
      [TestCase(true,   12000, 65000, 129000, true, false)]
      public void VarSizes_Increasing_Random(bool isParallel, int cnt, int minSz, int maxSz, bool speed, bool rnd)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;

          var sw = Stopwatch.StartNew();
          var tasks = new List<Task>();
          for(var t=0; t < (isParallel? (System.Environment.ProcessorCount - 1) : 1); t++)
           tasks.Add(
                  Task.Run( () =>
                  {
                     var dict = new Dictionary<PilePointer, dummy>();
                     var lst = new List<PilePointer>();
                     var priorOdd = PilePointer.Invalid;
                     for(var i=0; i<cnt; i++)
                     {
                       var buf = new byte[12 + 
                                          minSz+ 
                                          (
                                          rnd
                                           ? (NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, (int)(maxSz*(i/(double)cnt))))
                                           : (int)(maxSz*(i/(double)cnt))
                                          ) 
                                         ];
                       var data = new dummy{ bin = buf};
                       data.bin.WriteBEInt32(0, ExternalRandomGenerator.Instance.NextRandomInteger);
                       data.bin.WriteBEInt32(data.bin.Length-4, ExternalRandomGenerator.Instance.NextRandomInteger);
                       var ptr = pile.Put(data);
                       Assert.IsTrue( ptr.Valid );
               
                       dict.Add(ptr, data);
                       lst.Add(ptr);
                       
                       if (i>cnt/3)
                       {
                         if (ExternalRandomGenerator.Instance.NextRandomInteger > 0)
                         {
                           var ri = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, lst.Count-1);
                           var pp = lst[ri];
                           if (!pp.Valid) continue;

                           Assert.IsTrue( pile.Delete(pp) );
                           dict.Remove( pp );
                           lst[ri] = PilePointer.Invalid;
                           
                         }
                       }


                       if (i%1000==0)
                        Console.WriteLine("Thread{0} did {1}; allocated {2} bytes, utilized {3} bytes by {4} objects {5} bytes/obj. ",
                                          Thread.CurrentThread.ManagedThreadId,
                                           i,
                                           pile.AllocatedMemoryBytes,
                                           pile.UtilizedBytes,
                                           pile.ObjectCount,
                                           pile.UtilizedBytes / pile.ObjectCount);

                     }
                     Console.WriteLine("Thread {0} Population done, now checking the buffers... {1}",Thread.CurrentThread.ManagedThreadId, DateTime.Now);

                     foreach(var entry in dict)
                       Assert.IsTrue( NFX.IOMiscUtils.MemBufferEquals(entry.Value.bin, (pile.Get(entry.Key) as dummy).bin));

                     Console.WriteLine("Thread {0} DONE. {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                  })
            );//add
            
          Task.WaitAll( tasks.ToArray() ); 
          var el = sw.ElapsedMilliseconds;
          var gt = cnt * tasks.Count;
          Console.WriteLine("Total objects: {0:n0} in {1:n0} ms at {2:n0} obj/sec".Args(gt, el, gt / (el / 1000d)));
        }
      }



      [Test]
      public void Configuration()
      {
        var conf = @"
 app
 {
   memory-management
   {
     pile
     {
       alloc-mode=favorspeed
       free-list-size=100000
       max-segment-limit=79
       segment-size=395313143 //will be rounded to 16 byte boundary: 395,313,152
       max-memory-limit=123666333000

       free-chunk-sizes='128, 256, 512, 1024, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 16000, 32000, 64000,  256000'
     } 
     
     pile
     {
       name='specialNamed'
       free-list-size=99000
       max-segment-limit=73
       segment-size=395313147 //will be rounded to 16 byte boundary: 395,313,152
       max-memory-limit=127666333000

       free-chunk-sizes='77, 124, 180, 190, 200, 210, 220, 230, 1000, 2000, 3000, 4000, 5000, 32000, 64000,  257000'
     }   
   }     
 }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

        using(var app = new ServiceBaseApplication(null, conf))
        {
          using (var pile = new DefaultPile())
          {
            pile.Configure(null);
          
            Assert.AreEqual(AllocationMode.FavorSpeed, pile.AllocMode);
            Assert.AreEqual(100000, pile.FreeListSize);
            Assert.AreEqual(79, pile.MaxSegmentLimit);
            Assert.AreEqual(395313152, pile.SegmentSize);
            Assert.AreEqual(123666333000, pile.MaxMemoryLimit);
            
            Assert.AreEqual(128, pile.FreeChunkSizes[00]);
            Assert.AreEqual(256, pile.FreeChunkSizes[01]);
            Assert.AreEqual(512, pile.FreeChunkSizes[02]);
            Assert.AreEqual(1024, pile.FreeChunkSizes[03]);
            Assert.AreEqual(2000, pile.FreeChunkSizes[04]);
            Assert.AreEqual(3000, pile.FreeChunkSizes[05]);
            Assert.AreEqual(4000, pile.FreeChunkSizes[06]);
            Assert.AreEqual(5000, pile.FreeChunkSizes[07]);
            Assert.AreEqual(6000, pile.FreeChunkSizes[08]);
            Assert.AreEqual(7000, pile.FreeChunkSizes[09]);
            Assert.AreEqual(8000, pile.FreeChunkSizes[10]);
            Assert.AreEqual(9000, pile.FreeChunkSizes[11]);
            Assert.AreEqual(16000, pile.FreeChunkSizes[12]);
            Assert.AreEqual(32000, pile.FreeChunkSizes[13]);
            Assert.AreEqual(64000, pile.FreeChunkSizes[14]);
            Assert.AreEqual(256000, pile.FreeChunkSizes[15]);

            pile.Start();//just to test that it starts ok
          }
          
          using (var pile = new DefaultPile("specialNamed"))
          {
            pile.Configure(null);
           
            Assert.AreEqual(AllocationMode.ReuseSpace, pile.AllocMode);
            Assert.AreEqual(99000, pile.FreeListSize);
            Assert.AreEqual(73, pile.MaxSegmentLimit);
            Assert.AreEqual(395313152, pile.SegmentSize);
            Assert.AreEqual(127666333000, pile.MaxMemoryLimit);
            
            Assert.AreEqual(77, pile.FreeChunkSizes[00]);
            Assert.AreEqual(124, pile.FreeChunkSizes[01]);
            Assert.AreEqual(180, pile.FreeChunkSizes[02]);
            Assert.AreEqual(190, pile.FreeChunkSizes[03]);
            Assert.AreEqual(200, pile.FreeChunkSizes[04]);
            Assert.AreEqual(210, pile.FreeChunkSizes[05]);
            Assert.AreEqual(220, pile.FreeChunkSizes[06]);
            Assert.AreEqual(230, pile.FreeChunkSizes[07]);
            Assert.AreEqual(1000, pile.FreeChunkSizes[08]);
            Assert.AreEqual(2000, pile.FreeChunkSizes[09]);
            Assert.AreEqual(3000, pile.FreeChunkSizes[10]);
            Assert.AreEqual(4000, pile.FreeChunkSizes[11]);
            Assert.AreEqual(5000, pile.FreeChunkSizes[12]);
            Assert.AreEqual(32000, pile.FreeChunkSizes[13]);
            Assert.AreEqual(64000, pile.FreeChunkSizes[14]);
            Assert.AreEqual(257000, pile.FreeChunkSizes[15]);

            pile.Start();//just to test that it starts ok
          

          }

        }//using app
      }




  }
}
