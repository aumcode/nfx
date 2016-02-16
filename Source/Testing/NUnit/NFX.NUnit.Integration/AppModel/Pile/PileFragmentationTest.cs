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
  class PileFragmentationTest
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

    [TestCase(100000, 30, true, 8000, 3, true)]
   // [TestCase(100000, 30, false, 200, 10, true)]
   // [TestCase(100000, 30, true, 200, 2, true)]     // chessboard
   // [TestCase(100000, 30, false, 200, 2, true)]
    public void Put_RandomDelete_ByteArray(int cnt, int durationSec, bool speed, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Run(() =>
            {
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

               // int curBound = 0;
                var dict = new Dictionary<int, Tuple<PilePointer, int, byte, byte>>();


                Console.WriteLine("Starting a batch of {0}".Args(cnt));
                for (int i = 0; i < cnt; i++)
                {
                  var payloadSize = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(2, payloadSizeMax);
                  var val = new byte[payloadSize];
                  val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                  val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                  
                  var ptr = pile.Put(val);
                  
                  var element = new Tuple<PilePointer, int, byte, byte>(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                  dict.Add(i, element);

                  if (dict.Count > 0 && i % deleteFreq == 0)
                  {
                    while(true)
                    {
                      var idx = i - NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, i);
                    
                      Tuple<PilePointer, int, byte, byte> stored;
                      if (dict.TryGetValue(idx, out stored))
                      {
                        ptr = stored.Item1;
                        pile.Delete(ptr);
                        dict.Remove(idx);
                     //   curBound = i + 1;
                        break;
                      }
                    }
                  }

                  if (NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100)>98)
                  {
                    var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}".Args(Thread.CurrentThread.ManagedThreadId, toRead, dict.Count));
                    for(var k=0; k<toRead; k++)
                    {
                      var kvp = dict.Skip(NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, dict.Count-1)).First();
                      var buf = pile.Get(kvp.Value.Item1) as byte[];
                      Assert.AreEqual(kvp.Value.Item3, buf[0]);
                      Assert.AreEqual(kvp.Value.Item4, buf[kvp.Value.Item2]);
                    }
                  }
                }
               
                Console.WriteLine("Thread {0} is doing final read of {1} elements".Args(Thread.CurrentThread.ManagedThreadId, dict.Count));
                foreach (var kvp in dict)
                {
                  var buf = pile.Get(kvp.Value.Item1) as byte[];
                  Assert.AreEqual(kvp.Value.Item3, buf[0]);
                  Assert.AreEqual(kvp.Value.Item4, buf[kvp.Value.Item2]);
                }
              }
            }));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true, true, 100000, 30, 1)]
    [TestCase(false, true, 100000, 30, 1)]
    [TestCase(true, true, 100000, 30, 4)]
    [TestCase(false, true, 100000, 30, 4)]
    public void Put_RandomDelete_Row(bool speed, bool isParallel, int cnt, int duration, int deleteFreq)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Run(() =>
            {
              var objects = new List<Tuple<PilePointer, GDID>>();
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= duration) return;

                int curBound = 0;
                var dict = new Dictionary<int, Tuple<PilePointer, GDID>>();

                for (int i = 0; i < cnt; i++)
                {
                  var val = PersonRow.MakeFake(new GDID());
                  var ptr = pile.Put(val);
                  var element = new Tuple<PilePointer, GDID>(ptr, val.ID);
                  dict.Add(i, element);

                  if (curBound + deleteFreq == i)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(curBound, i - 1);
                    ptr = dict[idx].Item1;
                    pile.Delete(ptr);
                    dict.Remove(idx);
                    curBound = i + 1;
                  }
                }

                objects.Concat(dict.Values.ToList());

                foreach (var element in objects)
                {
                  var val = pile.Get(element.Item1) as PersonRow;
                  Assert.AreEqual(element.Item2, val.ID);
                }
              }
            }));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true, true, 30, 100, 200, 4, 200)]
    [TestCase(false, true, 30, 100, 200, 4, 200)]
    public void RandomCountPutDelete_ByteArray(bool speed, bool isParallel, int duration, int putMin, int putMax, int delFactor, int payloadSizeMax)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Run(() =>
            {
              var objects = new List<Tuple<PilePointer, int, byte, byte>>();
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= duration) return;

                var putCount = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putMin, putMax);
                for (int i = 0; i < putCount; i++)
                {
                  var payloadSize = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(100, payloadSizeMax);
                  var val = new byte[payloadSize];
                  val[0] = 1;
                  val[payloadSize - 1] = 2;
                  var ptr = pile.Put(val);
                  objects.Add(new Tuple<PilePointer, int, byte, byte>(ptr, payloadSize - 1, val[0], val[payloadSize - 1]));
                }

                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, objects.Count - 1);
                  var ptr = objects[idx].Item1;
                  pile.Delete(ptr);
                  objects.RemoveAt(idx);
                }

                foreach (var element in objects)
                {
                  var val = pile.Get(element.Item1) as byte[];
                  Assert.AreEqual(element.Item3, val[0]);
                  Assert.AreEqual(element.Item4, val[element.Item2]);
                }
              }
            }));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true, true, 30, 100, 200, 4)]
    [TestCase(false, true, 30, 100, 200, 4)]
    public void RandomCountPutDelete_Row(bool speed, bool isParallel, int duration, int putMin, int putMax, int delFactor)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Run(() =>
            {
              var objects = new List<Tuple<PilePointer, GDID>>();
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= duration) return;

                var putCount = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putMin, putMax);
                for (int i = 0; i < putCount; i++)
                {
                  var val = PersonRow.MakeFake(new GDID());
                  var ptr = pile.Put(val);
                  objects.Add(new Tuple<PilePointer, GDID>(ptr, val.ID));
                }

                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, objects.Count - 1);
                  var ptr = objects[idx].Item1;
                  pile.Delete(ptr);
                  objects.RemoveAt(idx);
                }

                foreach (var element in objects)
                {
                  var val = pile.Get(element.Item1) as PersonRow;
                  Assert.AreEqual(element.Item2, val.ID);
                }
              }
            }));
        Task.WaitAll(tasks.ToArray());
      }
    }
  }
}
