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

namespace NFX.NUnit.Integration.AppModel.Pile
{
  [TestFixture]
  public class PileFragmentationTest
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

    [TestCase(100000, 30, true,  2, 8000, 3, true)]
    [TestCase(100000, 30, false, 2, 200,  10, true)]
    public static void Put_RandomDelete_ByteArray(int cnt, int durationSec, bool speed, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

                var dict = new Dictionary<int, CheckByteArray>();


                Console.WriteLine("Starting a batch of {0}".Args(cnt));
                for (int i = 0; i < cnt; i++)
                {
                  var payloadSize = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                  var val = new byte[payloadSize];
                  val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                  val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                  var ptr = pile.Put(val);

                  var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                  dict.Add(i, element);

                  if (dict.Count > 0 && i % deleteFreq == 0)
                  {
                    while (true)
                    {
                      var idx = i - NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, i);

                      CheckByteArray stored;
                      if (dict.TryGetValue(idx, out stored))
                      {
                        ptr = stored.Ptr;
                        pile.Delete(ptr);
                        dict.Remove(idx);
                        break;
                      }
                    }
                  }

                  if (dict.Count > 16 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                  {
                    var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                    wlc++;
                    if (wlc % 125 == 0)
                      Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                        .Args(Thread.CurrentThread.ManagedThreadId, toRead, dict.Count));
                    for (var k = 0; k < toRead; k++)
                    {
                      var kvp = dict.Skip(NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, dict.Count - 1)).First();
                      var buf = pile.Get(kvp.Value.Ptr) as byte[];
                      Assert.AreEqual(kvp.Value.FirstByte, buf[0]);
                      Assert.AreEqual(kvp.Value.LastByte, buf[kvp.Value.IdxLast]);
                    }
                  }
                }

                Console.WriteLine("Thread {0} is doing final read of {1} elements".Args(Thread.CurrentThread.ManagedThreadId, dict.Count));
                foreach (var kvp in dict)
                {
                  var buf = pile.Get(kvp.Value.Ptr) as byte[];
                  Assert.AreEqual(kvp.Value.FirstByte, buf[0]);
                  Assert.AreEqual(kvp.Value.LastByte, buf[kvp.Value.IdxLast]);
                }
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 2, 1000, 3, true)]
    [TestCase(false, 30, 2, 1000, 3, true)]
    public static void DeleteOne_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var payloadSize = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                var val = new byte[payloadSize];
                val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                var ptr = pile.Put(val);

                var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                list.Add(element);

                // delete ONE random element
                if (i > 0 && i % deleteFreq == 0)
                {
                  var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                  ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as byte[];
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
              Console.WriteLine("Thread {0} is doing final read of {1} elements, ObjectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as byte[];
                Assert.AreEqual(element.FirstByte, buf[0]);
                Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 3, true)]
    [TestCase(false, 30, 3, true)]
    public static void DeleteOne_TRow(bool speed, int durationSec, int deleteFreq, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var val = PersonRow.MakeFake(new GDID(0, (ulong)i));

                var ptr = pile.Put(val);

                var element = new CheckTRow(ptr, val.ID, val.Address1);
                list.Add(element);

                // delete ONE random element
                if (i > 0 && i % deleteFreq == 0)
                {
                  var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                  ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Assert.IsTrue(element.Id.Equals(buf.ID));
                    Assert.IsTrue(element.Address.Equals(buf.Address1));
                  }
                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckTRow>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, ObjectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as PersonRow;
                Assert.IsTrue(element.Id.Equals(buf.ID));
                Assert.IsTrue(element.Address.Equals(buf.Address1));
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 2, 1000, true)]
    [TestCase(false, 30, 2, 1000, true)]
    public static void Chessboard_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var payloadSize = NFX.ExternalRandomGenerator
                                    .Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                var val = new byte[payloadSize];
                val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                var ptr = pile.Put(val);

                var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                list.Add(element);

                // delete previous element
                if (list.Count > 1 && i % 2 == 0)
                {
                  ptr = list[list.Count - 2].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(list.Count - 2);
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}, Pile objects {3}, Pile segments {4} Pile Bytes {5}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count, pile.ObjectCount, pile.SegmentCount, pile.AllocatedMemoryBytes));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as byte[];
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
              Console.WriteLine("Thread {0} is doing final read of {1} elements, ObjectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as byte[];
                Assert.AreEqual(element.FirstByte, buf[0]);
                Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, true)]
    [TestCase(false, 30, true)]
    public static void Chessboard_TRow(bool speed, int durationSec, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var val = PersonRow.MakeFake(new GDID(0, (ulong)i));

                var ptr = pile.Put(val);

                var element = new CheckTRow(ptr, val.ID, val.Address1);
                list.Add(element);

                // delete previous element
                if (list.Count > 1 && i % 2 == 0)
                {
                  ptr = list[list.Count - 2].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(list.Count - 2);
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}, Pile objects {3}, Pile segments {4} Pile Bytes {5}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count, pile.ObjectCount, pile.SegmentCount, pile.AllocatedMemoryBytes));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Assert.IsTrue(element.Id.Equals(buf.ID));
                    Assert.IsTrue(element.Address.Equals(buf.Address1));
                  }
                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckTRow>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, objectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as PersonRow;
                Assert.IsTrue(element.Id.Equals(buf.ID));
                Assert.IsTrue(element.Address.Equals(buf.Address1));
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 100, 200, 4, 2, 1000, true)]
    [TestCase(false, 30, 100, 200, 4, 2, 1000, true)]
    public static void DeleteSeveral_ByteArray(bool speed, int durationSec, int putMin, int putMax, int delFactor, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
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

                  var ptr = pile.Put(val);

                  list.Add(new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]));
                }

                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                  var ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    var element = list[NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as byte[];
                    Assert.AreEqual(element.FirstByte, buf[0]);
                    Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
                  }
                }

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckByteArray>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, objectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as byte[];
                Assert.AreEqual(element.FirstByte, buf[0]);
                Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
              return;
            }));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 100, 200, 4, true)]
    [TestCase(false, 30, 100, 200, 4, true)]
    public static void DeleteSeveral_TRow(bool speed, int durationSec, int putMin, int putMax, int delFactor, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var putCount = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putMin, putMax);
                for (int i = 0; i < putCount; i++)
                {
                  var val = PersonRow.MakeFake(new GDID());
                  var ptr = pile.Put(val);
                  list.Add(new CheckTRow(ptr, val.ID, val.Address1));
                }

                // delete several random elements
                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  var idx = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1);
                  var ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    var element = list[NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Assert.IsTrue(element.Id.Equals(buf.ID));
                    Assert.IsTrue(element.Address.Equals(buf.Address1));
                  }
                }
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, objectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as PersonRow;
                Assert.IsTrue(element.Id.Equals(buf.ID));
                Assert.IsTrue(element.Address.Equals(buf.Address1));
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [TestCase(true,  30, 2, 1000, 1000, 200000)]
    [TestCase(false, 30, 2, 1000, 1000, 200000)]
    [TestCase(true,  30, 2, 1000, 100000, 200000)]
    [TestCase(false, 30, 2, 1000, 100000, 200000)]
    public static void NoGrowth_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int countMin, int countMax)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (System.Environment.ProcessorCount - 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              bool put = true;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

                if (put)
                {
                  var cnt = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(countMin, countMax);
                  for (int j = 0; j < cnt; j++)
                  {
                    var payloadSize = NFX.ExternalRandomGenerator
                                        .Instance.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                    var val = new byte[payloadSize];
                    val[0] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;
                    val[payloadSize - 1] = (byte)NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

                    var ptr = pile.Put(val);

                    var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                    list.Add(element);
                  }
                  Console.WriteLine("Thread {0} put {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  put = false;
                }
                else
                {
                  Console.WriteLine("Thread {0} deleted {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  for(var j=0; j < list.Count; j++)
                  {
                    var element = list[j];
                    var buf = pile.Get(element.Ptr) as byte[];
                    Assert.AreEqual(element.FirstByte, buf[0]);
                    Assert.AreEqual(element.LastByte, buf[element.IdxLast]);
                    pile.Delete(element.Ptr);
                    
                  }
                  list.Clear();
                  put = true;
                }
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
      Console.WriteLine("Test finished.");
    }

    [TestCase(true,  30, 100000, 200000)]
    [TestCase(false, 30, 100000, 200000)]
    public static void NoGrowth_TRow(bool speed, int durationSec, int countMin, int countMax)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (System.Environment.ProcessorCount - 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              bool put = true;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

                if (put)
                {
                  var cnt = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(countMin, countMax);
                  for (int j = 0; j < cnt; j++)
                  {
                    var val = PersonRow.MakeFake(new GDID());

                    var ptr = pile.Put(val);

                    var element = new CheckTRow(ptr, val.ID, val.Address1);
                    list.Add(element);
                  }
                  Console.WriteLine("Thread {0} put {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  put = false;
                }
                else
                {
                  Console.WriteLine("Thread {0} deleted {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  for (var j = 0; j < list.Count; j++)
                  {
                    var element = list[j];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Assert.AreEqual(element.Id, buf.ID);
                    Assert.AreEqual(element.Address, buf.Address1);
                    pile.Delete(element.Ptr);
                  }
                  list.Clear();
                  put = true;
                }
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
      Console.WriteLine("Test finished.");
    }

    public struct CheckByteArray
    {
      public CheckByteArray(PilePointer pp, int il, byte fb, byte lb)
      {
        Ptr = pp;
        IdxLast = il;
        FirstByte = fb;
        LastByte = lb;
      }
      public readonly PilePointer Ptr;
      public readonly int IdxLast;
      public readonly byte FirstByte;
      public readonly byte LastByte;
    }

    public struct CheckTRow
    {
      public CheckTRow(PilePointer ptr, GDID id, string address)
      {
        Ptr = ptr;
        Id = id;
        Address = address;
      }
      public readonly PilePointer Ptr;
      public readonly GDID Id;
      public readonly string Address;
    }
  }
}
