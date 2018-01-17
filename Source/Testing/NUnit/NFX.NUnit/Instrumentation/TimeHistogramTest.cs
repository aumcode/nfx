/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

using NFX.Instrumentation.Analytics;

namespace NFX.NUnit.Instrumentation.Analytics
{
    [TestFixture]
    public class TestTimeHistogram
    {
        [TestCase]
        public void TimeHistogram1D()
        {
            var hist = new TimeHistogram(title: "1D Time Histogram",
                    dim1Name: "ValBucket",
                    dim1PartCount: 8,
                    dim1PartitionFunc: (x, v) =>
                    {
                        if (v <  0.000010) return 0;
                        if (v <= 0.000025) return Convert.ToInt32(System.Math.Round((v - 0.000010) * 1000000.0) /  5) + 0;
                        if (v <= 0.000100) return Convert.ToInt32(System.Math.Round((v - 0.000025) * 1000000.0) / 25) + 3;
                        return 7;
                    },
                    dim1PartitionNameFunc: (i) =>
                    {
                        switch (i)
                        {
                            case 0: return "<10us";
                            case 1: return "<15us";
                            case 2: return "<20us";
                            case 3: return "<25us";
                            case 4: return "<50us";
                            case 5: return "<75us";
                            case 6: return "<100us";
                            case 7: return ">100us";
                            default: throw new ArgumentException(); // impossible
                        }
                    }
            );

            for (int n = 100; n < 100000; n += 1000)
                using (hist.TimeFrame)
                    for (int i = 0; i < n; i++) ;

            string output = hist.ToStringReport();

            Debug.Write(output);
        }

        [TestCase]
        public void TimeHistogram2D()
        {
            var hist = new TimeHistogram<bool>("2D Time Histogram",
                dim1Name:   "Latency",
                dimension2: new Dimension<bool>(
                    name: "Sex",
                    partCount: 2,
                    partitionFunc: (x, v)  => v ? 0 : 1,
                    partitionNameFunc: (i) => i == 0 ? "Male" : "Female"
                )
            );

            for (int n = 0; n < 100000; n += 1000)
                using (hist.TimeFrame((n / 1000) % 2 == 0 /* Bool value of the 1st dimension */))
                    for (int i = 0; i < n + 100; i++) ;

            string output = hist.ToStringReport();

            Debug.Write(output);
        }

    }
}
