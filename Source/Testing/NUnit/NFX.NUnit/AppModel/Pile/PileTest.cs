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
      public void PutGetRawObject()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var buf = new byte[]{1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};

          var pp = ipile.Put(buf);

          byte svr;
          var buf2 = ipile.GetRawBuffer(pp, out svr); //main point: we dont get any exceptions
          
          Assert.IsTrue(buf2.Length >= buf.Length);
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

      [TestCase(false, 100000, 0, 40, true)]
      [TestCase(false,  10000, 0, 50000, true)]
      [TestCase(false,   1000, 70000, 150000, true)]
      [TestCase(false,   5000, 0, 150000, true)]

      [TestCase(true, 100000, 0, 40, true)]
      [TestCase(true,  10000, 0, 50000, true)]
      [TestCase(true,   1000, 70000, 150000, true)]
      [TestCase(true,   5000, 0, 150000, true)]

      [TestCase(false, 100000, 0, 40, false)]
      [TestCase(false,  10000, 0, 50000, false)]
      [TestCase(false,   1000, 70000, 150000, false)]
      [TestCase(false,   5000, 0, 150000, false)]

      [TestCase(true, 100000, 0, 40, false)]
      [TestCase(true,  10000, 0, 50000, false)]
      [TestCase(true,   1000, 70000, 150000, false)]
      [TestCase(true,   5000, 0, 150000, false)]
      public void VarSizes_Checkboard(bool isParallel, int cnt, int minSz, int maxSz, bool speed)
      {
        PileCacheTestCore.VarSizes_Checkboard(isParallel, cnt, minSz, maxSz, speed);
      }

      [TestCase(false, 100000, 0, 256, false, true)]
      [TestCase(false,  25000, 0, 8000, false, true)]
      [TestCase(false,  15000, 0, 24000, false, true)]
      [TestCase(false,   2100, 65000, 129000, false, true)]

      [TestCase(true, 100000, 0, 256, false, true)]
      [TestCase(true,  25000, 0, 8000, false, true)]
      [TestCase(true,  15000, 0, 24000, false, true)]
      [TestCase(true,   2100, 65000, 129000, false, true)]


      [TestCase(false, 100000, 0, 256, true, true)]
      [TestCase(false,  25000, 0, 8000, true, true)]
      [TestCase(false,  15000, 0, 24000, true, true)]
      [TestCase(false,   2100, 65000, 129000, true, true)]

      [TestCase(true, 100000, 0, 256, true, true)]
      [TestCase(true,  25000, 0, 8000, true, true)]
      [TestCase(true,  15000, 0, 24000, true, true)]
      [TestCase(true,   2100, 65000, 129000, true, true)]



      [TestCase(false, 100000, 0, 256, false, false)]
      [TestCase(false,  25000, 0, 8000, false, false)]
      [TestCase(false,  15000, 0, 24000, false, false)]
      [TestCase(false,   1200, 65000, 129000, false, false)]

      [TestCase(true, 100000, 0, 256, false, false)]
      [TestCase(true,  25000, 0, 8000, false, false)]
      [TestCase(true,  15000, 0, 24000, false, false)]
      [TestCase(true,   1200, 65000, 129000, false, false)]


      [TestCase(false, 100000, 0, 256, true, false)]
      [TestCase(false,  25000, 0, 8000, true, false)]
      [TestCase(false,  15000, 0, 24000, true, false)]
      [TestCase(false,   1200, 65000, 129000, true, false)]

      [TestCase(true, 100000, 0, 256, true, false)]
      [TestCase(true,  25000, 0, 8000, true, false)]
      [TestCase(true,  15000, 0, 24000, true, false)]
      [TestCase(true,   1200, 65000, 129000, true, false)]
      public void VarSizes_Increasing_Random(bool isParallel, int cnt, int minSz, int maxSz, bool speed, bool rnd)
      {
        PileCacheTestCore.VarSizes_Increasing_Random(isParallel, cnt, minSz, maxSz, speed, rnd);
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
