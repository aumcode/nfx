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
using System.Diagnostics;
using NUnit.Framework;

using NFX.Throttling;
using NFX.Environment;
using NFX.ServiceModel;
using System.Threading;

namespace NFX.NUnit.Throttling
{
    [TestFixture]
    class ThrottlingTestFixtureTest
    {
        private readonly int HALF_SEC_TICKS = 5000000;

        [TestCase]
        public void ThrottleSlidingWindowTest()
        {
            SlidingWindowThrottle throttler = new SlidingWindowThrottle(
                name: "test", throttleLimit: 100, throttleInterval: 3, bucketsPerSecond: 2);

            const int count = 8;

            int i;
            for (i=0; i < count; i++)
                throttler.Try(new DateTime((long)(HALF_SEC_TICKS * (i + 1))), i + 1);

            Assert.AreEqual(33, throttler.Sum);

            DateTime time = new DateTime((long)(HALF_SEC_TICKS * (count+1)));

            time = time.AddSeconds(2);
            Assert.AreEqual(true, throttler.Try(time, ++i)); // i=9
            Assert.AreEqual(17, throttler.Sum);

            time = time.AddSeconds(3);
            Assert.AreEqual(true, throttler.Try(time, ++i)); // i=10
            Assert.AreEqual(10, throttler.Sum);

            time = time.AddSeconds(9);
            Assert.AreEqual(true, throttler.Try(time, ++i)); // i=11
            Assert.AreEqual(11, throttler.Sum);

            time = time.AddSeconds(2);
            Assert.AreEqual(true, throttler.Try(time, ++i)); // i=12
            Assert.AreEqual(23, throttler.Sum);

            time = time.AddSeconds(2);
            Assert.AreEqual(true, throttler.Try(time, ++i)); // i=13
            Assert.AreEqual(25, throttler.Sum);

            time = time.AddSeconds(1);
            Assert.AreEqual(true, throttler.Try(time, ++i)); // i=14
            Assert.AreEqual(27, throttler.Sum);

            time = time.AddSeconds(2);
            Assert.AreEqual(true, throttler.Try(time, ++i)); // i=15
            Assert.AreEqual(29, throttler.Sum);

            Assert.AreEqual(29.0 / 3, throttler.Avg);
        }

        [TestCase]
        public void ThrottleTimeSpacingTest()
        {
            TimeSpacingThrottle throttler =
                new TimeSpacingThrottle(name: "test", throttleLimit: 5, throttleInterval: 10);

            DateTime time = new DateTime((long)HALF_SEC_TICKS);

            Assert.AreEqual(true, throttler.Try(time, 1));
            Assert.AreEqual(false, throttler.Try(time, 1));

            time = time.AddSeconds(1);
            Assert.AreEqual(false, throttler.Try(time, 1));
            time = time.AddSeconds(1);
            Assert.AreEqual(true, throttler.Try(time, 2));

            time = time.AddSeconds(2);
            Assert.AreEqual(false, throttler.Try(time, 1));
            time = time.AddSeconds(2);
            Assert.AreEqual(true, throttler.Try(time, 1));
        }

        [TestCase]
        public void ThrottleConfigTest()
        {
            var xml = @"
                <my-config ns='NFX.Throttling' type='$(/$ns).ThrottlingService' name='MyTestService'>
                    <throttle type='$(/$ns).TimeSpacingThrottle'   name='t1' limit='10' interval='1'/>
                    <throttle type='$(/$ns).TimeSpacingThrottle'   name='t2' limit='5'  interval='2'/>
                    <throttle type='$(/$ns).SlidingWindowThrottle' name='t3' limit='10' interval='1' buckets-per-sec='4'/>
                </my-config>";

            var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

            var svc = FactoryUtils.MakeAndConfigure(conf.Root) as ThrottlingService;
            svc.Start();

            var t1 = svc.Get<TimeSpacingThrottle>("t1");
            Assert.AreEqual(10, t1.Limit);
            Assert.AreEqual(1, t1.Interval);

            var t2 = svc.Get<TimeSpacingThrottle>("t2");
            Assert.AreEqual(5, t2.Limit);
            Assert.AreEqual(2, t2.Interval);

            var t3 = svc.Get<SlidingWindowThrottle>("t3");
            Assert.AreEqual(10, t3.Limit);
            Assert.AreEqual(1, t3.Interval);
            Assert.AreEqual(4, t3.BucketsPerSec);

            new TimeSpacingThrottle("t4", 100, unit: "kaka").Register(svc);
            var t4 = svc.Get<TimeSpacingThrottle>("t4");
            Assert.AreEqual("kaka", t4.Unit);

            svc.WaitForCompleteStop();
        }
    }
}
