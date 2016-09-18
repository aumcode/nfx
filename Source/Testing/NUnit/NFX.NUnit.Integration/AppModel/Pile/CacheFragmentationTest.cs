/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.ApplicationModel.Pile;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.Integration.AppModel.Pile
{
  [TestFixture]
  class CacheFragmentationTest
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

    [TestCase(true,  30, 2, 1000, 3, true)]
    [TestCase(false, 30, 2, 1000, 3, true)]
    public void DeleteOne_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      using (var cache = new LocalCache())
      using (var pile = new DefaultPile(cache))
      {
        cache.Pile = pile;
        cache.PileAllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        cache.Start();

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var i = 0;
              var list = new List<CheckByteArray>();
              var tA = cache.GetOrCreateTable<GDID>("A");
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var payloadSize = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                var val = new byte[payloadSize];
                val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                var key = new GDID((uint)Thread.CurrentThread.ManagedThreadId, (ulong)i);
                tA.Put(key, val);

                list.Add(new CheckByteArray(key, payloadSize - 1, val[0], val[payloadSize - 1]));

                // delete ONE random element
                if (i > 0 && i % deleteFreq == 0)
                {
                  while (true && list.Count > 0)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    key = list[idx].Key;
                    var removed = tA.Remove(key);
                    list.RemoveAt(idx);
                    if (removed)
                      break;
                  }
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead && list.Count > 0; k++)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    var element = list[idx];
                    var buf = tA.Get(element.Key) as byte[];
                    if (buf == null)
                    {
                      list.RemoveAt(idx);
                      continue;
                    }
                    Assert.AreEqual(element.FirstByte, buf[0]);
                    Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
                  }

                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckByteArray>();
              }

              Console.WriteLine("Thread {0} is doing final read of {1} elements, tableCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, tA.Count));
              foreach (var element in list)
              {
                var buf = tA.Get(element.Key) as byte[];
                if (buf == null)
                  continue;
                Assert.AreEqual(element.FirstByte, buf[0]);
                Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 2, 1000, true)]
    [TestCase(false, 30, 2, 1000, true)]
    public void Chessboard_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      using (var cache = new LocalCache())
      using (var pile = new DefaultPile(cache))
      {
        cache.Pile = pile;
        cache.PileAllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        cache.Start();

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              var i = 0;
              var tA = cache.GetOrCreateTable<GDID>("A");
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var payloadSize = NFX.ExternalRandomGenerator
                                    .Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                var val = new byte[payloadSize];
                val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                var key = new GDID((uint)Thread.CurrentThread.ManagedThreadId, (ulong)i);
                tA.Put(key, val);

                var element = new CheckByteArray(key, payloadSize - 1, val[0], val[payloadSize - 1]);
                list.Add(element);

                // delete previous element
                if (list.Count > 1 && i % 2 == 0)
                {
                  key = list[list.Count - 2].Key;
                  tA.Remove(key);
                  list.RemoveAt(list.Count - 2);
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead && list.Count > 0; k++)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    element = list[idx];
                    var buf = tA.Get(element.Key) as byte[];
                    if (buf == null)
                    {
                      list.RemoveAt(idx);
                      continue;
                    }
                    Assert.AreEqual(element.FirstByte, buf[0]);
                    Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
                  }
                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckByteArray>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, tableCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, tA.Count));
              foreach (var element in list)
              {
                var buf = tA.Get(element.Key) as byte[];
                if (buf == null)
                  continue;
                Assert.AreEqual(element.FirstByte, buf[0]);
                Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 100, 200, 4, 2, 1000, true)]
    [TestCase(false, 30, 100, 200, 4, 2, 1000, true)]
    public void DeleteSeveral_ByteArray(bool speed, int durationSec, int putMin, int putMax, int delFactor, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      using (var cache = new LocalCache())
      using (var pile = new DefaultPile(cache))
      {
        cache.Pile = pile;
        cache.PileAllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        cache.Start();

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              var tA = cache.GetOrCreateTable<GDID>("A");
              ulong k = 0;
              var wlc = 0;

              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var putCount = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putMin, putMax);
                for (int i = 0; i < putCount; i++)
                {
                  var payloadSize = NFX.ExternalRandomGenerator
                                      .Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                  var val = new byte[payloadSize];
                  val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                  val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                  var key = new GDID((uint)Thread.CurrentThread.ManagedThreadId, k);

                  tA.Put(key, val);

                  list.Add(new CheckByteArray(key, payloadSize - 1, val[0], val[payloadSize - 1]));
                  k++;
                }

                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  while (true && list.Count > 0)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    var key = list[idx].Key;
                    var removed = tA.Remove(key);
                    list.RemoveAt(idx);
                    if (removed)
                      break;
                  }
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var j = 0; j < toRead && list.Count > 0; j++)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    var element = list[idx];
                    var buf = tA.Get(element.Key) as byte[];
                    if (buf == null)
                    {
                      list.RemoveAt(idx);
                      continue;
                    }
                    Assert.AreEqual(element.FirstByte, buf[0]);
                    Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
                  }
                }

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckByteArray>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, tableCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, tA.Count));
              foreach (var element in list)
              {
                var val = tA.Get(element.Key) as byte[];
                if (val == null)
                  continue;
                Assert.AreEqual(element.FirstByte, val[0]);
                Assert.AreEqual(element.LastByte, val[element.IdxLast]);
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 2, 1000, 100000, 200000)]
    [TestCase(false, 30, 2, 1000, 100000, 200000)]
    public void NoGrowth_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int countMin, int countMax)
    {
      using (var cache = new LocalCache())
      using (var pile = new DefaultPile(cache))
      {
        cache.Pile = pile;
        cache.PileAllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        cache.Start();

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (System.Environment.ProcessorCount - 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var tA = cache.GetOrCreateTable<GDID>("A");
              var list = new List<CheckByteArray>();
              bool put = true;

              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

                if (put)
                {
                  var cnt = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(countMin, countMax);
                  for (int i = 0; i < cnt; i++)
                  {
                    var payloadSize = NFX.ExternalRandomGenerator
                                        .Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                    var val = new byte[payloadSize];
                    val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                    val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                    var key = new GDID((uint)Thread.CurrentThread.ManagedThreadId, (ulong)i);

                    tA.Put(key, val);

                    var element = new CheckByteArray(key, payloadSize - 1, val[0], val[payloadSize - 1]);
                    list.Add(element);
                  }
                  Console.WriteLine("Thread {0} put {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  put = false;
                }
                else
                {
                  var i = 0;
                  for (var j = 0; j < list.Count; j++)
                  {
                    var element = list[j];
                    var buf = tA.Get(element.Key) as byte[];
                    if (buf != null)
                    {
                      Assert.AreEqual(element.FirstByte, buf[0]);
                      Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
                      tA.Remove(element.Key);
                      i++;
                    }
                  }
                  Console.WriteLine("Thread {0} deleted {1} objects".Args(Thread.CurrentThread.ManagedThreadId, i));
                  list.Clear();
                  put = true;
                }
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 2, 1000, 3, true)]
    [TestCase(false, 30, 2, 1000, 3, true)]
    public void DeleteOne_TwoTables_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      using (var cache = new LocalCache())
      using (var pile = new DefaultPile(cache))
      {
        cache.Pile = pile;
        cache.PileAllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        cache.Start();

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var i = 0;
              var list = new List<Tuple<int, GDID, int, byte, byte>>();
              var tA = cache.GetOrCreateTable<GDID>("A");
              var tB = cache.GetOrCreateTable<GDID>("B");
              var wlc = 0;

              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var payloadSize = NFX.ExternalRandomGenerator
                                    .Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                var val = new byte[payloadSize];
                val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                var tableId = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                var table = tableId == 0 ? tA : tB;
                var key = new GDID((uint)Thread.CurrentThread.ManagedThreadId, (ulong)i);

                table.Put(key, val);

                list.Add(new Tuple<int, GDID, int, byte, byte>(tableId, key, payloadSize - 1, val[0], val[payloadSize - 1]));

                // delete ONE random element
                if (i > 0 && i % deleteFreq == 0)
                {
                  while (true && list.Count > 0)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    var element = list[idx];

                    table = element.Item1 == 0 ? tA : tB;
                    key = element.Item2;

                    var removed = table.Remove(key);
                    list.RemoveAt(idx);

                    if (removed)
                      break;
                  }
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead));
                  for (var j = 0; j < toRead && list.Count > 0; j++)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    var element = list[idx];
                    table = element.Item1 == 0 ? tA : tB;
                    var buf = table.Get(element.Item2) as byte[];
                    if (buf == null)
                    {
                      list.RemoveAt(idx);
                      continue;
                    }
                    Assert.AreEqual(element.Item4, buf[0]);
                    Assert.AreEqual(element.Item5, buf[element.Item3]);
                  }

                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<Tuple<int, GDID, int, byte, byte>>();
              }

              Console.WriteLine("Thread {0} is doing final read of {1} elements"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count));
              foreach (var element in list)
              {
                var table = element.Item1 == 0 ? tA : tB;
                var buf = table.Get(element.Item2) as byte[];
                if (buf == null)
                  continue;
                Assert.AreEqual(element.Item4, buf[0]);
                Assert.AreEqual(element.Item5, buf[element.Item3]);
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 100, 200, 4, 2, 1000, true)]
    [TestCase(false, 30, 100, 200, 4, 2, 1000, true)]
    public void DeleteSeveral_TwoTables_ByteArray(bool speed, int durationSec, int putMin, int putMax, int delFactor, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      using (var cache = new LocalCache())
      using (var pile = new DefaultPile(cache))
      {
        cache.Pile = pile;
        cache.PileAllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        cache.Start();

        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<Tuple<int, GDID, int, byte, byte>>();
              var tA = cache.GetOrCreateTable<GDID>("A");
              var tB = cache.GetOrCreateTable<GDID>("B");
              ulong k = 0;
              var wlc = 0;

              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var putCount = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putMin, putMax);
                for (int i = 0; i < putCount; i++)
                {
                  var payloadSize = NFX.ExternalRandomGenerator
                                      .Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                  var val = new byte[payloadSize];
                  val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                  val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                  var tableId = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 1);
                  var table = tableId == 0 ? tA : tB;
                  var key = new GDID((uint)Thread.CurrentThread.ManagedThreadId, k);

                  table.Put(key, val);

                  list.Add(new Tuple<int, GDID, int, byte, byte>(tableId, key, payloadSize - 1, val[0], val[payloadSize - 1]));
                  k++;
                }

                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  while (true && list.Count > 0)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    var element = list[idx];
                    var table = element.Item1 == 0 ? tA : tB;
                    var key = element.Item2;

                    var removed = table.Remove(key);
                    list.RemoveAt(idx);
                    if (removed)
                      break;
                  }
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead));
                  for (var j = 0; j < toRead && list.Count > 0; j++)
                  {
                    var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                    var element = list[idx];
                    var table = element.Item1 == 0 ? tA : tB;
                    var key = element.Item2;

                    var buf = table.Get(key) as byte[];
                    if (buf == null)
                    {
                      list.RemoveAt(idx);
                      continue;
                    }
                    Assert.AreEqual(element.Item4, buf[0]);
                    Assert.AreEqual(element.Item5, buf[element.Item3]);
                  }
                }

                if (list.Count == Int32.MaxValue)
                  list = new List<Tuple<int, GDID, int, byte, byte>>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
              foreach (var element in list)
              {
                var table = element.Item1 == 0 ? tA : tB;
                var val = table.Get(element.Item2) as byte[];
                if (val == null)
                  continue;
                Assert.AreEqual(element.Item4, val[0]);
                Assert.AreEqual(element.Item5, val[element.Item3]);
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    public struct CheckByteArray
    {
      public CheckByteArray(GDID key, int il, byte fb, byte lb)
      {
        Key = key;
        IdxLast = il;
        FirstByte = fb;
        LastByte = lb;
      }
      public readonly GDID Key;
      public readonly int IdxLast;
      public readonly byte FirstByte;
      public readonly byte LastByte;
    }
  }
}
