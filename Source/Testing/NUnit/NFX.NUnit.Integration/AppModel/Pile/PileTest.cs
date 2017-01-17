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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.ApplicationModel.Pile;
using NFX.DataAccess.Distributed;
using NFX.NUnit.AppModel.Pile;

namespace NFX.NUnit.Integration.AppModel.Pile
{
  [TestFixture]
  public class PileTest
  {
    #region Tests

      [Test]
      public void SegmentAddedDeleted()
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE})
        {
          pile.Start();

          var ipile = pile as IPile;

          var pp = PilePointer.Invalid;
          for(ulong i=0; i<PileCacheTestCore.SEG_SIZE && ipile.SegmentCount < 2; i++)
          {
            var ch = Charge.MakeFake(new GDID(1, i));
            pp = ipile.Put(ch);
          }

          Assert.AreEqual(2, ipile.SegmentCount);

          ipile.Delete(pp);

          Assert.AreEqual(1, ipile.SegmentCount);
        }
      }

      
      [Test]
      public void Parallel_PutGet()
      {
        const int CNT = 1000000;
        var tuples = new Tuple<PilePointer, Charge>[CNT];

        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          Parallel.For(0, CNT, i => {
            var ch = Charge.MakeFake(new GDID(0, (ulong)i));
            var pp = ipile.Put(ch);
            tuples[i] = new Tuple<PilePointer,Charge>(pp, ch);
          });

          Assert.AreEqual(CNT, ipile.ObjectCount);

          Parallel.ForEach(tuples, t => {
            Assert.AreEqual(t.Item2, ipile.Get(t.Item1));
          });
        }  
      }

      [Test]
      public void Parallel_PutDeleteGet_Checkerboard()
      {
        const int CNT = 1002030;//1000203;
        var tuples = new Tuple<PilePointer, Charge>[CNT];

        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          Parallel.For(0, CNT, i => {
            var ch = Charge.MakeFake(new GDID(0, (ulong)i));
            var pp = ipile.Put(ch);
            tuples[i] = new Tuple<PilePointer,Charge>(pp, ch);
          });

          Assert.AreEqual(CNT, ipile.ObjectCount);

          Parallel.For(0, CNT, i => {
            if (i % 3 != 0)
              ipile.Delete(tuples[i].Item1);
          });

          Assert.AreEqual(CNT/3, ipile.ObjectCount);

          var deletedHits = new ConcurrentDictionary<int, int>();

          Parallel.For(0, CNT, i => {
            if (i % 3 != 0)
            {
              try
              {
                deletedHits.AddOrUpdate(System.Threading.Thread.CurrentThread.ManagedThreadId, 1, (threadId, val) => val + 1);
                ipile.Get(tuples[i].Item1);
                Assert.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(tuples[i].Item1);
              Assert.AreEqual(ch, tuples[i].Item2);
            }
          });

          foreach (var kvp in deletedHits)
          {
            Console.WriteLine("Thread '{0}' {1:n0} times accessed deleted pointer", kvp.Key, kvp.Value);
          }
        }  
      }

      [TestCase(128,      64,128,134,  152,  170,180,190,200,250,512,1024,2000,3000,4000,5000,6000)]
      [TestCase(32000,    64,88,128,134,170,180,190,200,250,512,1024,2000,3000,4000,5000,  32024)]  
      public void PileSmallObjects(int payloadSize, params int[] freeChunkSizes)
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE, AllocMode = AllocationMode.ReuseSpace })
        {
          pile.FreeChunkSizes = freeChunkSizes;

          pile.Start();

          var pps = new List<PilePointer>();
          while (pile.SegmentCount < 2)
            pps.Add(pile.Put(generatePayload(payloadSize)));

          pile.Delete(pps.Last());
          pps.RemoveAt(pps.Count-1);

          Console.WriteLine("just removed the last added payload and segment should be 1 now, real segment count is {0}", pile.SegmentCount);

          var objectsInFirstSegment = pps.Count;

          Console.WriteLine("put {0:N0} objects in first segment, pile.ObjectCount {1:N0}", objectsInFirstSegment, pile.ObjectCount);

          var deletedObjectCount = 0;
          for (int i = 0; i < pps.Count; i+=2, deletedObjectCount++)
            Assert.IsTrue(pile.Delete(pps[i]));

          Console.WriteLine("deleted {0:N0} objects, pile.ObjectCount {1:N0}", deletedObjectCount, pile.ObjectCount);

          var crawlStatus = pile.Crawl(false);
          Console.WriteLine("crawl: {0}", crawlStatus);

          var pps1 = new List<PilePointer>();
          var c = 0;
          while (pile.SegmentCount < 3)
          {
            pps1.Add(pile.Put(generatePayload(payloadSize)));
            if (c%20000 ==0 )pile.Crawl(true); //we do crawl because otherwise the 25000 free index xlots get exhausted AND
            c++;                               //this unit tests does not run long enough to cause Crawl within allocator (5+ seconds)
          }                                    //so we induce Crawl by hand to rebiild indexes

          pile.Delete(pps1.Last());
          pps1.RemoveAt(pps1.Count-1);

          Console.WriteLine("again just removed the last added payload and segment should be 2 now, real segment count is {0}", pile.SegmentCount);

          var objectsInSecondSegment = pps1.Count;

          Console.WriteLine("put {0:N0} objects in second segment, pile.ObjectCount {1:N0}", objectsInSecondSegment, pile.ObjectCount);

          Assert.AreEqual(objectsInFirstSegment * 2, pile.ObjectCount, "Object count in pile with two full segments must be equal to double object count in full first segment!");
        }
      }

      [TestCase(200)]
      public void PileDeleteInLastSegment(int payloadSize)
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE})
        {
          pile.Start();

          var pps = new List<PilePointer>();
          while (pile.SegmentCount < 2)
            pps.Add(pile.Put(generatePayload(payloadSize)));

          pile.Delete(pps.Last());
          pps.RemoveAt(pps.Count-1);

          Console.WriteLine("segment count: {0}, segment total count: {1}", pile.SegmentCount, pile.SegmentTotalCount);

          Assert.AreEqual(1, pile.SegmentCount);
          Assert.AreEqual(1, pile.SegmentTotalCount);

        }   
      }

      [TestCase(200)]
      public void PileDeleteInMiddleSegment(int payloadSize)
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE})
        {
          pile.Start();

          var pps = new List<PilePointer>();
          while (pile.SegmentCount < 4)
            pps.Add(pile.Put(generatePayload(payloadSize)));

          pile.Delete(pps.Last());
          pps.RemoveAt(pps.Count-1);

          var objectsInsegmentCount = pps.Count / 3;

          Console.WriteLine("{0:N0} object in pile, {1:N0} object per segment", pile.ObjectCount, objectsInsegmentCount);

          Assert.AreEqual(3, pile.SegmentCount);
          Assert.AreEqual(3, pile.SegmentTotalCount);

          for (int i = objectsInsegmentCount; i < 2 * objectsInsegmentCount; i++)
          {
            pile.Delete(pps[i]);
          }

          Console.WriteLine("{0:N0} object in pile, {1:N0} segments, {2:N0} segments total", pile.ObjectCount, pile.SegmentCount, pile.SegmentTotalCount);

          Assert.AreEqual(2, pile.SegmentCount);
          Assert.AreEqual(3, pile.SegmentTotalCount);
        }   
      }

      [TestCase(32, 16000, 300, 500)]
      public void PutGetDelete_Sequential(int fromSize, int toSize, int fromObjCount, int toObjCount)
      {
        PileCacheTestCore.PutGetDelete_Sequential(fromSize, toSize, fromObjCount, toObjCount);
      }

      [TestCase(32, 3200, 1, 20, 4)]
      [TestCase(32, 12800, 1, 50, 8)]
      public void PutGetDelete_Parallel(int fromSize, int toSize, int fromObjCount, int toObjCount, int taskCount)
      {
        PileCacheTestCore.PutGetDelete_Parallel(fromSize, toSize, fromObjCount, toObjCount, taskCount);
      }

              private object generatePayload(int size = 8)
              {
                return new byte[size];
              }

    #endregion
  }
}
