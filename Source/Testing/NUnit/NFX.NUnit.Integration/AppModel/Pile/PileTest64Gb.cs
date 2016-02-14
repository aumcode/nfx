using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using NUnit.Framework;

using NFX.NUnit.AppModel.Pile;
using NFX.ApplicationModel.Pile;

namespace NFX.NUnit.Integration.AppModel.Pile
{
    public class PileTest64Gb : HighMemoryLoadTest64RAM
    {
        [TestCase(32, 3200, 1, 50, 8)]
        [TestCase(32, 12800, 1, 100, 16)]
        public void PutGetDelete_Parallel(int fromSize, int toSize, int fromObjCount, int toObjCount, int taskCount)
        {
            PileCacheTestCore.PutGetDelete_Parallel(fromSize, toSize, fromObjCount, toObjCount, taskCount);
        }

        [TestCase(32, 64000, 300, 1000)]
        public void PutGetDelete_Sequential(int fromSize, int toSize, int fromObjCount, int toObjCount)
        {
            PileCacheTestCore.PutGetDelete_Sequential(fromSize, toSize, fromObjCount, toObjCount);
        }

        [Test]
        public void Parallel_PutGetDelete_Random()
        {
            const int PUTTER_CNT = 2, PUTTER_OP_CNT = 2 * 10000;
            const int GETTER_CNT = 6, GETTER_OP_CNT = 2 * 30000;
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
                    var task = new Task(() =>
                    {

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
                    var task = new Task(() =>
                    {

                        for (int i = 0; i < GETTER_OP_CNT; i++)
                        {
                            if (data.Count == 0)
                            {
                                System.Threading.Thread.Yield();
                                continue;
                            }
                            var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, data.Count - 1);
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
                    var task = new Task(() =>
                    {

                        for (int i = 0; i < DELETER_OP_CNT; i++)
                        {
                            if (data.Count == 0)
                            {
                                System.Threading.Thread.Yield();
                                continue;
                            }
                            var idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, data.Count - 1);
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
        
        [TestCase(true, 50000, 0, 150000, true)]
        [TestCase(true, 50000, 0, 150000, false)]
        public void VarSizes_Checkboard(bool isParallel, int cnt, int minSz, int maxSz, bool speed)
        {
            PileCacheTestCore.VarSizes_Checkboard(isParallel, cnt, minSz, maxSz, speed);
        }

        [TestCase(true, 150000, 0, 24000, false, false)]
        [TestCase(true, 1000000, 0, 256, true, false)]
        [TestCase(true, 150000, 0, 24000, true, false)]
        [TestCase(true, 12000, 65000, 129000, true, false)]
        public void VarSizes_Increasing_Random(bool isParallel, int cnt, int minSz, int maxSz, bool speed, bool rnd)
        {
            PileCacheTestCore.VarSizes_Increasing_Random(isParallel, cnt, minSz, maxSz, speed, rnd);
        }
    }
}
