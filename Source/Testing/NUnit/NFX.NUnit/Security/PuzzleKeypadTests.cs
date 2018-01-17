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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX;
using NFX.Security.CAPTCHA;

namespace NFX.NUnit.Security
{
    [TestFixture]
    public class PuzzleKeypadTests
    {
        [TestCase]
        public void ParallelRendering_PNG()
        {
            const int CNT = 10000;
            
            long totalBytes = 0;

            var sw = Stopwatch.StartNew();
            Parallel.For(0, CNT, 
               (i) =>
               {
                    var kp = new PuzzleKeypad( (new ELink((ulong)ExternalRandomGenerator.Instance.NextRandomInteger, null)).Link);
                    var img = kp.DefaultRender();
                    var ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    Interlocked.Add(ref totalBytes, ms.Length);

               });
            var elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine("Generated {0} in {1} ms at {2} ops./sec. Bytes: {3}".Args(CNT, elapsed, CNT / (elapsed / 1000d), totalBytes));

        }

        [TestCase]
        public void ParallelRendering_JPEG()
        {
            const int CNT = 10000;
            
            long totalBytes = 0;

            var sw = Stopwatch.StartNew();
            Parallel.For(0, CNT, 
               (i) =>
               {
                    var kp = new PuzzleKeypad( (new ELink((ulong)ExternalRandomGenerator.Instance.NextRandomInteger, null)).Link);
                    var img = kp.DefaultRender();
                    var ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Interlocked.Add(ref totalBytes, ms.Length);

               });
            var elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine("Generated {0} in {1} ms at {2} ops./sec. Bytes: {3}".Args(CNT, elapsed, CNT / (elapsed / 1000d), totalBytes));

        }

    }

}
