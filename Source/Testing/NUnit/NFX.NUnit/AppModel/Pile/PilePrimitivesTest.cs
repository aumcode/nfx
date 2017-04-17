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
  public class PilePrimitiveTests
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


      [TestCase(25000, 2    ,1 )]
      [TestCase(25000, 3    ,1 )]
      [TestCase(25000, 4    ,1 )]
      [TestCase(25000, 5    ,1 )]
      [TestCase(25000, 6    ,1 )]
      [TestCase(25000, 7    ,1 )]
      [TestCase(25000, 8    ,1 )]
      [TestCase(25000, 9    ,1 )]
      [TestCase(25000, 10   ,1 )]
      [TestCase(25000, 15   ,1 )]
      [TestCase(25000, 16   ,1 )]
      [TestCase(65*1024, 3  ,1 )]
      [TestCase(65*1024, 7  ,1 )]
      [TestCase(65*1024, 8  ,1 )]
      [TestCase(65*1024, 15 ,1 )]
      [TestCase(65*1024, 16 ,1 )]

      [TestCase(25000, 2    ,8 )]
      [TestCase(25000, 3    ,8 )]
      [TestCase(25000, 4    ,8 )]
      [TestCase(25000, 5    ,8 )]
      [TestCase(25000, 6    ,8 )]
      [TestCase(25000, 7    ,8 )]
      [TestCase(25000, 8    ,8 )]
      [TestCase(25000, 9    ,8 )]
      [TestCase(25000, 10   ,8 )]
      [TestCase(25000, 15   ,8 )]
      [TestCase(25000, 16   ,8 )]
      [TestCase(65*1024, 3  ,8 )]
      [TestCase(65*1024, 7  ,8 )]
      [TestCase(65*1024, 8  ,8 )]
      [TestCase(65*1024, 15 ,8 )]
      [TestCase(65*1024, 16 ,8 )]
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
            var str = new string('a', len-i);

            var pp = pile.Put(str);

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



      [TestCase(25000, 2    ,1 )]
      [TestCase(25000, 3    ,1 )]
      [TestCase(25000, 4    ,1 )]
      [TestCase(25000, 5    ,1 )]
      [TestCase(25000, 6    ,1 )]
      [TestCase(25000, 7    ,1 )]
      [TestCase(25000, 8    ,1 )]
      [TestCase(25000, 9    ,1 )]
      [TestCase(25000, 10   ,1 )]
      [TestCase(25000, 15   ,1 )]
      [TestCase(25000, 16   ,1 )]
      [TestCase(65*1024, 3  ,1 )]
      [TestCase(65*1024, 7  ,1 )]
      [TestCase(65*1024, 8  ,1 )]
      [TestCase(65*1024, 15 ,1 )]
      [TestCase(65*1024, 16 ,1 )]

      [TestCase(25000, 2    ,8 )]
      [TestCase(25000, 3    ,8 )]
      [TestCase(25000, 4    ,8 )]
      [TestCase(25000, 5    ,8 )]
      [TestCase(25000, 6    ,8 )]
      [TestCase(25000, 7    ,8 )]
      [TestCase(25000, 8    ,8 )]
      [TestCase(25000, 9    ,8 )]
      [TestCase(25000, 10   ,8 )]
      [TestCase(25000, 15   ,8 )]
      [TestCase(25000, 16   ,8 )]
      [TestCase(65*1024, 3  ,8 )]
      [TestCase(65*1024, 7  ,8 )]
      [TestCase(65*1024, 8  ,8 )]
      [TestCase(65*1024, 15 ,8 )]
      [TestCase(65*1024, 16 ,8 )]
      public void ByteCorrectess(int len, int deleteEvery, int parallel)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var bag = new ConcurrentBag<PilePointer>();
          var deleted = 0;
          Parallel.For(0, len, new ParallelOptions{ MaxDegreeOfParallelism=parallel},
          (i) =>
          {
            var original = new byte[len-i];

            var pp = pile.Put(original);

            var got = pile.Get(pp) as byte[];

            Aver.AreEqual(original.Length, got.Length);

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




      [TestCase(1, 1, 1)]//warmup
      [TestCase(2000000,  32, 1)] //1 threads put 2,000,000 strings of 32 bytes in 1,143 msec at 1,749,781 ops/sec
                                  //1 threads got 2,000,000 strings of 32 bytes in 1,167 msec at 1,713,796 ops/sec
                                  //-------------------------------------------------------------------------------
                                  //1 threads put 2,000,000 strings of 32 bytes in 446 msec at 4,484,305 ops/sec
                                  //1 threads got 2,000,000 strings of 32 bytes in 349 msec at 5,730,659 ops/sec


      [TestCase(32000000, 32, 8)] //8 threads put 32,000,000 strings of 32 bytes in 5,198 msec at 6,156,214 ops/sec
                                  //8 threads got 32,000,000 strings of 32 bytes in 4,309 msec at 7,426,317 ops/sec
                                  //---------------------------------------------------------------------------------
                                  //8 threads put 32,000,000 strings of 32 bytes in 3,421 msec at 9,353,990 ops/sec
                                  //8 threads got 32,000,000 strings of 32 bytes in 2,072 msec at 15,444,015 ops/sec

      [TestCase(32000000, 32, 12)] //12 threads put 32,000,000 strings of 32 bytes in 4,536 msec at 7,054,674 ops/sec
                                   //12 threads got 32,000,000 strings of 32 bytes in 3,910 msec at 8,184,143 ops/sec
                                   //--------------------------------------------------------------------------------
                                   //12 threads put 32,000,000 strings of 32 bytes in 2,947 msec at 10,858,500 ops/sec
                                   //12 threads got 32,000,000 strings of 32 bytes in 1,818 msec at 17,601,760 ops/sec


      public void Strings(int CNT, int size, int parallel)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var str = new string('a', size);
          var pp = pile.Put(str);

          var sw = Stopwatch.StartNew();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Put(str);
          });

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads put {1:n0} strings of {2:n0} bytes in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));


          sw.Restart();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Get(pp);
          });

          el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads got {1:n0} strings of {2:n0} bytes in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));

        }
      }


      [TestCase(1, 1, 1)]//warmup
      [TestCase(2000000,  32, 1)] //1 threads put 2,000,000 byte[32] in 1,307 msec at 1,530,222 ops/sec
                                  //1 threads got 2,000,000 byte[32] in 1,152 msec at 1,736,111 ops/sec
                                  //-------------------------------------------------------------------------------
                                  //1 threads put 2,000,000 byte[32] in 348 msec at 5,747,126 ops/sec
                                  //1 threads got 2,000,000 byte[32] in 235 msec at 8,510,638 ops/sec


      [TestCase(32000000, 32, 8)] //8 threads put 32,000,000 byte[32] in 5,741 msec at 5,573,942 ops/sec
                                  //8 threads got 32,000,000 byte[32] in 4,091 msec at 7,822,048 ops/sec
                                  //---------------------------------------------------------------------------------
                                  //8 threads put 32,000,000 byte[32] in 3,273 msec at 9,776,963 ops/sec
                                  //8 threads got 32,000,000 byte[32] in 2,450 msec at 13,061,224 ops/sec

      [TestCase(32000000, 32, 12)] //12 threads put 32,000,000 byte[32] in 5,008 msec at 6,389,776 ops/sec
                                   //12 threads got 32,000,000 byte[32] in 3,787 msec at 8,449,960 ops/sec
                                   //--------------------------------------------------------------------------------
                                   //12 threads put 32,000,000 byte[32] in 2,963 msec at 10,799,865 ops/sec
                                   //12 threads got 32,000,000 byte[32] in 2,253 msec at 14,203,285 ops/sec


      public void ByteBuf(int CNT, int size, int parallel)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var str = new byte[size];
          var pp = pile.Put(str);

          var sw = Stopwatch.StartNew();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Put(str);
          });

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads put {1:n0} byte[{2:n0}] in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));


          sw.Restart();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Get(pp);
          });

          el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads got {1:n0} byte[{2:n0}] in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));

        }
      }


  }
}
