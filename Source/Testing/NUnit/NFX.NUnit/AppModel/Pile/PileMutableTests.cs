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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.DataAccess.CRUD;
using NFX.IO;
using NFX.Serialization.Slim;
using NFX.Serialization.JSON;


namespace NFX.NUnit.AppModel.Pile
{
  [TestFixture]
  public class PileMutableTests
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
      public void FitPreallocate()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 4000);

          Aver.AreEqual(4000,  pile.SizeOf(p1));

          var obj2 = new Payload{ID = 2, Name = "2", Data = new byte []{1,2,3,4,5,6,7,8}};
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          var got = pile.Get(p1) as Payload;

          Aver.AreEqual(2, got.ID);
          Aver.AreEqual("2", got.Name);
          Aver.IsNotNull(got.Data);
          Aver.AreEqual(8, got.Data.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }

      [Test]
      public void FitPreallocateString()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = "abcdefgh";

          var p1 = pile.Put(obj1, preallocateBlockSize: 4000);
          var got = pile.Get(p1) as string;
          Aver.AreEqual("abcdefgh", got);

          Aver.AreEqual(4000,  pile.SizeOf(p1));

          var obj2 = "abcdefghijklmnopqrst0912345";
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as string;

          Aver.AreEqual("abcdefghijklmnopqrst0912345", got);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }

      [Test]
      public void LinkStringNoPreallocate()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = "abcdefgh";

          var p1 = pile.Put(obj1);
          var got = pile.Get(p1) as string;
          Aver.AreEqual("abcdefgh", got);

          var obj2 = "abcdefghijklmnopqrst0912345";
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as string;

          Aver.AreEqual("abcdefghijklmnopqrst0912345", got);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }


      [Test]
      public void FitPreallocateByteArray()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = new byte[3];

          var p1 = pile.Put(obj1, preallocateBlockSize: 4000);
          var got = pile.Get(p1) as byte[];
          Aver.AreEqual(3, got.Length);

          Aver.AreEqual(4000,  pile.SizeOf(p1));

          var obj2 = new byte[571];
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as byte[];

          Aver.AreEqual(571, got.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }


      [Test]
      public void LinkByteArrayNoPreallocate()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = new byte[12];

          var p1 = pile.Put(obj1);
          var got = pile.Get(p1) as byte[];
          Aver.AreEqual(12, got.Length);

          var obj2 = new byte[389];
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as byte[];

          Aver.AreEqual(389, got.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }



      [Test]
      public void LinkNoPreallocate()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1);

          Aver.IsTrue( pile.SizeOf(p1) < 128);

          var obj2 = new Payload{ID = 2, Name = "2", Data = new byte [128]};
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          var got = pile.Get(p1) as Payload;

          Aver.AreEqual(2, got.ID);
          Aver.AreEqual("2", got.Name);
          Aver.IsNotNull(got.Data);
          Aver.AreEqual(128, got.Data.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }

      [Test]
      public void LinkPreallocate()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 2000);

          Aver.AreEqual(2000,  pile.SizeOf(p1));

          var obj2 = new Payload{ID = 2, Name = "2", Data = new byte [3000]};
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          var got = pile.Get(p1) as Payload;

          Aver.AreEqual(2, got.ID);
          Aver.AreEqual("2", got.Name);
          Aver.IsNotNull(got.Data);
          Aver.AreEqual(3000, got.Data.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }




      [Test]
      public void Reallocate_BackOriginal()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 2000);

          Aver.AreEqual(2000,  pile.SizeOf(p1));
          var a = pile.AllocatedMemoryBytes;
          var u = pile.UtilizedBytes;
          var o = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a, u, o);

          Aver.AreEqual(2000, u);

          Aver.IsTrue( pile.Put(p1, new Payload{Data=new byte[8000]}) );

          var a2 = pile.AllocatedMemoryBytes;
          var u2 = pile.UtilizedBytes;
          var o2 = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);

          Aver.IsTrue( u2 > 10000);
          Aver.IsTrue( u2 > u);
          pile.Put(p1, obj1);

          u2 = pile.UtilizedBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);
          Aver.AreEqual(2000, u2);

        }
      }


      [Test]
      public void Reallocate_Delete()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 2000);

          Aver.AreEqual(2000,  pile.SizeOf(p1));
          var a = pile.AllocatedMemoryBytes;
          var u = pile.UtilizedBytes;
          var o = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a, u, o);

          Aver.AreEqual(2000, u);

          Aver.IsTrue( pile.Put(p1, new Payload{Data=new byte[8000]}) );

          var a2 = pile.AllocatedMemoryBytes;
          var u2 = pile.UtilizedBytes;
          var o2 = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);

          Aver.IsTrue( u2 > 10000);
          Aver.IsTrue( u2 > u);
          pile.Delete(p1);

          u2 = pile.UtilizedBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);
          Aver.AreEqual(0, u2);

        }
      }



      //todo test
      //delete, size of, compact, deadlocks/multithreaded
      //add statistics fro LINKS count?

      //todo serializer format for string
      //todo serialier format for byte[]
      //todo Is PilePointer supported by slim format? Pilepointer[]?


      public class Payload
      {
        public int ID;
        public string Name;
        public byte[] Data;
      }

      [TestCase(50000, 2, 8)]
      [TestCase(50000, 3, 8)]
      [TestCase(50000, 5, 8)]
      public void StringCorrectess(int len, int deleteEvery, int parallel)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var bag = new ConcurrentBag<PilePointer>();
          var deleted = 0;
          Parallel.For(0, len, new ParallelOptions{ MaxDegreeOfParallelism=parallel},
          (i) =>
          {
            var str = new string('a', i);

            PilePointer pp = PilePointer.Invalid;
            if (i%2==0)
             if (bag.TryTake(out pp))
              pile.Put(pp, str);

            pp = pile.Put(str, preallocateBlockSize: i+100);

            var got = pile.Get(pp) as string;

            Aver.AreEqual(str, got);

            if (i%deleteEvery==0)
            {
             PilePointer dp;
             if (bag.TryTake(out dp))
             {
               if (pile.Delete(dp, false))
                Interlocked.Increment(ref deleted);
             }
             bag.Add(pp);
            }
          });

          Console.WriteLine("Deleted {0:n0}", deleted);
        }
      }


      [TestCase(300)]
      public void ReallocateInPlace(int len)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var lst = new List<PilePointer>();
          for(var i=0; i<len; i++)
          {
             var ub = i * 10;
             var ptr = pile.Put(new byte[0], preallocateBlockSize: ub);

             lst.Add(ptr);
             for(var j=0; j<ub+(ub/2); j++)
             {
               Aver.IsTrue( pile.Put(ptr, new byte[j]) );
             }
          }

          Aver.AreEqual(lst.Count(), pile.ObjectCount);
          foreach(var pp in lst)
           Aver.IsTrue( pile.Delete(pp) );

          Aver.AreEqual(0, pile.AllocatedMemoryBytes);
          Aver.AreEqual(0, pile.ObjectCount);
          Aver.AreEqual(0, pile.ObjectLinkCount);
        }
      }

      [TestCase(1, 8, 20)]
      [TestCase(12, 8, 20)]
      [TestCase(256, 8, 30)]
      [TestCase(1024, 8, 30)]
      [TestCase(1024*1024, 12, 60)]
      public void TestThreadSafety(int cnt, int tcount, int seconds)
      {
        using (var pile = new DefaultPile())
        {
         // pile.SegmentSize = 64 * 1024 * 1024;
          pile.Start();

          var data = new PilePointer[cnt];
          for(var i=0; i<cnt; i++)
            data[i] = pile.Put(new byte[0], preallocateBlockSize: PilePointer.RAW_BYTE_SIZE + (137 * (i%17)));

          var lst = new List<Task>();

          long statRead = 0;
          long statPut = 0;
          long statDelete = 0;

          var sw = Stopwatch.StartNew();
          var sd = DateTime.UtcNow;
          var deleteLock = new object();
          for(var i=0; i<tcount; i++)
           lst.Add( Task.Factory.StartNew( () =>
           {
             while(true)
             {
               var now = DateTime.UtcNow;
               if ((now - sd).TotalSeconds>seconds) break;

               var it = ExternalRandomGenerator.Instance.NextScaledRandomInteger(10,100);
               for(var j=0; j<it; j++)
               {
                 var pp = data[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, cnt-1)];
                 Aver.IsTrue( pile.Get(pp) is byte[] );
                 Interlocked.Increment(ref statRead);
               }

               it = ExternalRandomGenerator.Instance.NextScaledRandomInteger(10,100);
               for(var j=0; j<it; j++)
               {
                 var pp = data[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, cnt-1)];
                 Aver.IsTrue( pile.Put(pp, new byte[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,3791)], link: true) );
                 Interlocked.Increment(ref statPut);
               }

               if (ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,100)>50 && Monitor.TryEnter(deleteLock))
               try
               {
                 var newData = (PilePointer[])data.Clone();
                 it = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 10 + (cnt / 2));
                 var toDelete = new List<PilePointer>();
                 for(var j=0; j<it; j++)
                 {
                   var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, cnt-1);
                   toDelete.Add( newData[idx] );
                   newData[idx] = pile.Put(new byte[12], preallocateBlockSize: ExternalRandomGenerator.Instance.NextScaledRandomInteger(24, 1024));
                 }
                 data = newData;//atomic;
                 Thread.Sleep(1000);
                 foreach(var pp in toDelete)
                 {
                   Aver.IsTrue( pile.Delete(pp) );
                   Interlocked.Increment(ref statDelete);
                 }
               }
               finally
               {
                 Monitor.Exit(deleteLock);
               }

             }
           }, TaskCreationOptions.LongRunning));

          Task.WaitAll(lst.ToArray());

          var el = sw.ElapsedMilliseconds;

          Console.WriteLine("Read {0:n0} at {1:n0} ops/sec".Args(statRead, statRead /(el / 1000d)));
          Console.WriteLine("Put {0:n0} at {1:n0} ops/sec".Args(statPut, statPut /(el / 1000d)));
          Console.WriteLine("Deleted {0:n0} at {1:n0} ops/sec".Args(statDelete, statDelete /(el / 1000d)));

          for(var i=0; i<data.Length; i++)
           Aver.IsTrue( pile.Delete(data[i]) );


          Aver.AreEqual(0, pile.ObjectCount);
          Aver.AreEqual(0, pile.ObjectLinkCount);
          Aver.AreEqual(0, pile.AllocatedMemoryBytes);

        }
      }


  }
}
