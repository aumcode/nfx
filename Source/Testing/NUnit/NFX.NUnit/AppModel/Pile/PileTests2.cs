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
  public class PileTests2
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
      public void ManyRow_PutThenRead()
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          var sw = Stopwatch.StartNew();
          while(ipile.SegmentCount<2)
          {
            var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)lst.Count));
            var pp = ipile.Put(rowIn);
            lst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );
          }

          var wms = sw.ElapsedMilliseconds;


          Console.WriteLine("Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, wms, lst.Count / (wms / 1000d)));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));

          foreach(var kvp in lst)
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          }
        }
      }


      [Test]
      public void ManyRow_PutReadDeleteRead()
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          int totalPut = 0;
          int totalDelete = 0;

          var sw = Stopwatch.StartNew();
          while(ipile.SegmentCount<4)
          {
            var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)lst.Count));
            var pp = ipile.Put(rowIn);
            totalPut++;
            lst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );

            var chance = ExternalRandomGenerator.Instance.NextRandomInteger > 1000000000; //periodically delete
            if (!chance) continue;

            var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, lst.Count-1);
            var kvp = lst[idx];
            lst.RemoveAt(idx);
            Aver.AreObjectsEqual(kvp.Value, ipile.Get(kvp.Key));

            Aver.IsTrue( ipile.Delete(kvp.Key) );
            totalDelete++;
          }

          var wms = sw.ElapsedMilliseconds;


          Console.WriteLine("Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalPut, wms, totalPut / (wms / 1000d)));
          Console.WriteLine("Read and deleted {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalDelete, wms, totalDelete / (wms / 1000d)));
          Console.WriteLine("Left {0:n0}".Args(lst.Count));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));


          Console.WriteLine("Will verify all rows...");
          foreach(var kvp in lst)
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          }
        }
      }


      [Test]
      public void ManyMixed_PutReadDeleteRead()
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          int totalBuff = 0;
          long totalBuffSz = 0;
          int totalPut = 0;
          int totalDelete = 0;

          var pBuff = PilePointer.Invalid;

          var sw = Stopwatch.StartNew();
          while(ipile.SegmentCount<16)
          {
            var chance = ExternalRandomGenerator.Instance.NextRandomInteger > 0;
            if (chance)
            {
              var fakeBuf = new byte[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 128*1024)];
              totalBuff++;
              totalBuffSz += fakeBuf.Length;
              if (pBuff.Valid && ExternalRandomGenerator.Instance.NextRandomInteger > 0) ipile.Delete(pBuff); //periodically delete buffers
              pBuff = ipile.Put(fakeBuf);
            }

            var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)lst.Count));
            var pp = ipile.Put(rowIn);
            totalPut++;
            lst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );

            chance = ExternalRandomGenerator.Instance.NextRandomInteger > 1000000000; //periodically delete rows
            if (!chance) continue;

            var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, lst.Count-1);
            var kvp = lst[idx];
            lst.RemoveAt(idx);
            Aver.AreObjectsEqual(kvp.Value, ipile.Get(kvp.Key));

            Aver.IsTrue( ipile.Delete(kvp.Key) );
            totalDelete++;
          }

          var wms = sw.ElapsedMilliseconds;

          Console.WriteLine("Buff Created {0:n0} size {1:n0} bytes".Args(totalBuff, totalBuffSz));
          Console.WriteLine("Row Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalPut, wms, totalPut / (wms / 1000d)));
          Console.WriteLine("Read and deleted {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalDelete, wms, totalDelete / (wms / 1000d)));
          Console.WriteLine("Left {0:n0}".Args(lst.Count));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));
          Console.WriteLine("Utilized bytes {0:n0}".Args(pile.UtilizedBytes));
          Console.WriteLine("Objects {0:n0}".Args(pile.ObjectCount));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));


          Console.WriteLine("Will verify all rows...");
          foreach(var kvp in lst)
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          }
        }
      }


      [TestCase(3)]
      [TestCase(7)]
      [TestCase(11)]
      [TestCase(39)]
      public void Parallel_ManyMixed_PutReadDeleteRead(int tcount)
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          int totalBuff = 0;
          long totalBuffSz = 0;
          int totalPut = 0;
          int totalDelete = 0;


          var sw = Stopwatch.StartNew();

          var tasks =new List<Task>();

          while(tasks.Count<tcount)
          {
              var task = Task.Factory.StartNew( ()=>
              {
                        var llst = new List<KeyValuePair<PilePointer, CheckoutRow>>();
                        var pBuff = PilePointer.Invalid;

                        while(ipile.SegmentCount<64)
                        {
                          var chance = ExternalRandomGenerator.Instance.NextRandomInteger > 0;
                          if (chance)
                          {
                            var fakeBuf = new byte[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 128*1024)];
                            Interlocked.Increment(ref totalBuff);
                            Interlocked.Add( ref totalBuffSz, fakeBuf.Length);
                            if (pBuff.Valid && ExternalRandomGenerator.Instance.NextRandomInteger > 0) ipile.Delete(pBuff); //periodically delete buffers
                            pBuff = ipile.Put(fakeBuf);
                          }

                          var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)llst.Count));
                          var pp = ipile.Put(rowIn);
                          Interlocked.Increment( ref totalPut);
                          llst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );

                          chance = ExternalRandomGenerator.Instance.NextRandomInteger > 1000000000; //periodically delete rows
                          if (!chance) continue;

                          var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, llst.Count-1);
                          var kvp = llst[idx];
                          llst.RemoveAt(idx);
                          Aver.AreObjectsEqual(kvp.Value, ipile.Get(kvp.Key));

                          Aver.IsTrue( ipile.Delete(kvp.Key) );
                          Interlocked.Increment(ref totalDelete);
                        }
                        lock(lst)
                         lst.AddRange(llst);
              });
              tasks.Add(task);
          }
          Task.WaitAll(tasks.ToArray());

          var wms = sw.ElapsedMilliseconds;

          Console.WriteLine("Buff Created {0:n0} size {1:n0} bytes".Args(totalBuff, totalBuffSz));
          Console.WriteLine("Row Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalPut, wms, totalPut / (wms / 1000d)));
          Console.WriteLine("Read and deleted {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalDelete, wms, totalDelete / (wms / 1000d)));
          Console.WriteLine("Left {0:n0}".Args(lst.Count));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));
          Console.WriteLine("Utilized bytes {0:n0}".Args(pile.UtilizedBytes));
          Console.WriteLine("Objects {0:n0}".Args(pile.ObjectCount));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));


          Console.WriteLine("Will verify all rows...");
          Parallel.ForEach(lst, kvp =>
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          });
        }
      }




  }
}
