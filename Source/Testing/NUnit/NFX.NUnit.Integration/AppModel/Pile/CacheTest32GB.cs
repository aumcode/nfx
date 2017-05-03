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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.NUnit.AppModel.Pile;
using NFX.ApplicationModel.Pile;

namespace NFX.NUnit.Integration.AppModel.Pile
{
    public class CacheTest32Gb : HighMemoryLoadTest32RAM
    {
        [Test]
        public void T070_DoesNotSeeAgedOrExpired()
        {
            using (var cache = PileCacheTestCore.MakeCache())
            {
                var tA = cache.GetOrCreateTable<string>("A");
                var tB = cache.GetOrCreateTable<string>("B");
                var tC = cache.GetOrCreateTable<string>("C");
                tC.Options.DefaultMaxAgeSec = 4;
                var tD = cache.GetOrCreateTable<string>("D");

                Assert.AreEqual(PutResult.Inserted, tA.Put("key1", "value1"));    //does not expire by itself
                Assert.AreEqual(PutResult.Inserted, tB.Put("key1", "value1", 7)); //will expire in 7 seconds
                Assert.AreEqual(PutResult.Inserted, tC.Put("key1", "value1"));    //will expire in Options.DefaultMaxAgeSec
                Assert.AreEqual(PutResult.Inserted, tD.Put("key1", "value1", absoluteExpirationUTC: DateTime.UtcNow.AddSeconds(4)));//will expire at specific time


                Assert.AreEqual("value1", tA.Get("key1"));
                Assert.AreEqual("value1", tA.Get("key1", 3));

                Assert.AreEqual("value1", tB.Get("key1"));

                Assert.AreEqual("value1", tC.Get("key1"));

                Assert.AreEqual("value1", tD.Get("key1"));


                Thread.Sleep(20000);// wait long enough to cover a few swep cycles (that may be 5+sec long)


                Assert.AreEqual("value1", tA.Get("key1"));
                Assert.AreEqual(null, tA.Get("key1", 3)); //did not expire, but aged over get limit
                Assert.AreEqual(null, tB.Get("key1")); //expired because of put with time limit
                Assert.AreEqual(null, tC.Get("key1"));//expired because of Options
                Assert.AreEqual(null, tD.Get("key1"));//expired because of absolute expiration on put
            }
        }

        [Test]
        public void T120_Rejuvenate()
        {
            using (var cache = PileCacheTestCore.MakeCache())
            {
                var tA = cache.GetOrCreateTable<string>("A");

                Assert.AreEqual(PutResult.Inserted, tA.Put("key1", "value1", 12));
                Assert.AreEqual(PutResult.Inserted, tA.Put("key2", "value2", 12));
                Assert.AreEqual(PutResult.Inserted, tA.Put("key3", "value3", 12));



                Assert.AreEqual("value1", tA.Get("key1"));
                Assert.AreEqual("value2", tA.Get("key2"));
                Assert.AreEqual("value3", tA.Get("key3"));

                for (var i = 0; i < 30; i++)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Second {0}   Load Factor {1}", i, tA.LoadFactor);
                    Assert.IsTrue(tA.Rejuvenate("key2"));
                }

                Assert.AreEqual(null, tA.Get("key1"));
                Assert.AreEqual("value2", tA.Get("key2"));//this is still here because it got rejuvenated
                Assert.AreEqual(null, tA.Get("key3"));

                Thread.Sleep(30000);
                Assert.AreEqual(null, tA.Get("key2"));//has died too
                Assert.AreEqual(0, tA.Count);
            }
        }

        [TestCase(1000000)]
        public void T130_KeyInt_ManyPutGet(int cnt)
        {
            PileCacheTestCore.KeyInt_ManyPutGet(cnt);
        }

        [TestCase(1000000)]
        public void T140_KeyGDID_ManyPutGet(int cnt)
        {
            PileCacheTestCore.KeyGDID_ManyPutGet(cnt);
        }

        [TestCase(1000000)]
        public void T150_KeyString_ManyPutGet(int cnt)
        {
            PileCacheTestCore.KeyString_ManyPutGet(cnt);
        }

        [TestCase(25, 100000, 4 * 1024)]
        [TestCase(10, 3000000, 16)]
        [TestCase(10, 1000, 1024 * 1024)]
        [TestCase(10, 200, 8 * 1024 * 1024)]
        public void T160_ResizeTable(int cnt, int rec, int payload)
        {
            PileCacheTestCore.ResizeTable(cnt, rec, payload);
        }

        [TestCase(1000000, 1)]
        [TestCase(1000000, 16)]
        [TestCase(1000000, 512)]
        public void T190_FID_PutGetCorrectness(int cnt, int tbls)
        {
            PileCacheTestCore.FID_PutGetCorrectness(cnt, tbls);
        }

        [TestCase(5, 7, 1000, 20)]
        public void T9000000_ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
        {
            PileCacheTestCore.ParalellGetPutRemove(workers, tables, putCount, durationSec);
        }
    }
}
