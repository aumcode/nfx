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
using System.Threading.Tasks;

using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.IO;
using NFX.Serialization.Slim;

namespace NFX.NUnit.Integration.AppModel.Pile
{
  [TestFixture]
  public class PileTest
  {
    #region Tests

      [Test]
      public void SegmentAddedDeleted()
      {
        const int SEG_SIZE = DefaultPile.SEG_SIZE_MIN;

        using (var pile = new DefaultPile() { SegmentSize = SEG_SIZE})
        {
          pile.Start();

          var ipile = pile as IPile;

          var pp = PilePointer.Invalid;
          for(ulong i=0; i<SEG_SIZE && ipile.SegmentCount < 2; i++)
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

      [Test]
      public void Parallel_PutGetDelete_Random()
      {
        const int PUTTER_CNT  = 2, PUTTER_OP_CNT  = 2 * 10000;
        const int GETTER_CNT  = 6, GETTER_OP_CNT  = 2 * 30000;
        const int DELETER_CNT = 2, DELETER_OP_CNT = 2 * 10000;

        var data = new ConcurrentDictionary<PilePointer, string>();

        var getAccessViolations = new ConcurrentDictionary<int, int>();
        var deleteAccessViolations = new ConcurrentDictionary<int, int>();

        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          // putter tasks
          var putters = new Task[PUTTER_CNT];
          for (int it = 0; it < PUTTER_CNT; it++)
          {
            var task = new Task(() => {

              for (int i = 0; i < PUTTER_OP_CNT; i++)
              {
                var str = NFX.Parsing.NaturalTextGenerator.Generate();
                var pp = ipile.Put(str);
                data.TryAdd(pp, str);
              }

            });

            putters[it] = task;
          }

          // getter tasks
          var getters = new Task[GETTER_CNT];
          for (int it = 0; it < GETTER_CNT; it++)
          {
            var task = new Task(() => {

              for (int i = 0; i < GETTER_OP_CNT; i++)
              {
                if (data.Count == 0) {
                  System.Threading.Thread.Yield();
                  continue;
                }
                var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, data.Count-1);
                var kvp = data.ElementAt(idx);
                try
                {
                  
                  var str = ipile.Get(kvp.Key);
                  Assert.AreEqual(str, kvp.Value);
                }
                catch (PileAccessViolationException)
                {
                  getAccessViolations.AddOrUpdate(System.Threading.Thread.CurrentThread.ManagedThreadId, 1, (mid, val) => val + 1);
                }
              }
            });
            getters[it] = task;
          }

          // deleter tasks
          var deleters = new Task[DELETER_CNT];
          for (int it = 0; it < DELETER_CNT; it++)
          {
            var task = new Task(() => {

              for (int i = 0; i < DELETER_OP_CNT; i++)
              {
                if (data.Count == 0) {
                  System.Threading.Thread.Yield();
                  continue;
                }
                var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, data.Count-1);
                var kvp = data.ElementAt(idx);
                try
                {
                  ipile.Delete(kvp.Key);
                }
                catch (PileAccessViolationException)
                {
                  deleteAccessViolations.AddOrUpdate(System.Threading.Thread.CurrentThread.ManagedThreadId, 1, (mid, val) => val + 1);
                }
              }
            });
            deleters[it] = task;
          }


          foreach (var task in putters) task.Start();
          foreach (var task in getters) task.Start();
          foreach (var task in deleters) task.Start();


          Task.WaitAll(putters.Concat(getters).Concat(deleters).ToArray());

          foreach (var kvp in getAccessViolations)
            Console.WriteLine("Get thread '{0}' {1:n0} times accessed deleted pointer", kvp.Key, kvp.Value);

          foreach (var kvp in deleteAccessViolations)
            Console.WriteLine("Del thread '{0}' {1:n0} times accessed deleted pointer", kvp.Key, kvp.Value);
        }
      }


    #endregion
  }
}
