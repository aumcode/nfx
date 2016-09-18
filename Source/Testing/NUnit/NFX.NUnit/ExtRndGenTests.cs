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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.Instrumentation.Analytics;

namespace NFX.NUnit
{
    [TestFixture]
    public class ExtRndTests
    {
        [TestCase]
        public void Loop1()
        {
            var rnd = new Random();
            for(var i=0; i<1000;i++)
            {
    if (i%3==0)
    ExternalRandomGenerator.Instance.FeedExternalEntropySample(NFX.OS.Computer.CurrentProcessorUsagePct);      

               var n1 = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,100);
               var n2 = rnd.Next(100); 
               Console.WriteLine( "{0} {1}".Args(n1, n2) );

    //           System.Threading.Thread.Sleep(n2);
            }
        }

        [TestCase]
        public void LoopParallel()
        {
            Parallel.For(0, 1000, (i)=>
            {
  
    if (i%3==0)
      ExternalRandomGenerator.Instance.FeedExternalEntropySample(NFX.OS.Computer.CurrentProcessorUsagePct);      
       

               var n1 = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,1000);
               Console.WriteLine( n1 );
               System.Threading.Thread.SpinWait(ExternalRandomGenerator.Instance.NextScaledRandomInteger(10,250));
            });
        }

        [TestCase]
        public void WebSafeStrings()
        {
          for(var i=0; i<25; i++)
          {
            var s = ExternalRandomGenerator.Instance.NextRandomWebSafeString();
            Console.WriteLine(s);
            Assert.True( s.Length >= 16 && s.Length <= 32);
          } 
        }

        [TestCase]
        public void NextRandom16Bytes()
        {
          for(var i=0; i<25; i++)
          {
            var a = ExternalRandomGenerator.Instance.NextRandom16Bytes;
            Console.WriteLine(a.ToDumpString(DumpFormat.Hex));
          } 
        }


        [TestCase]
        public void Hystogram_Dump_1()
        {
          const int CNT = 100000;
          const int MAX_RND = 100;

          var hist = new Histogram<int>("Random Histogram",
                new Dimension<int>(
                    "ValBucket",
                    partCount: MAX_RND,
                    partitionFunc: (dim, v) => {
                        return v;// % 100;
                    },
                    partitionNameFunc: (i) => i.ToString()
                )
            );

         // var rnd = new Random();
          for(var i=0; i<CNT; i++)
          {
         //   var r = rnd.Next(100);// ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,100);
            var r = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, MAX_RND);
            hist.Sample( r );
         //   ExternalRandomGenerator.Instance.FeedExternalEntropySample( (int)NFX.OS.Computer.GetMemoryStatus().AvailablePhysicalBytes);
          }               

          string output = hist.ToStringReport();
          Console.WriteLine( output );

          var countPerRandomSeed = CNT / (double)MAX_RND;
          var tolerance = countPerRandomSeed * 0.15d;//Guarantees uniform random distribution. The lower the number, the more uniform gets
          foreach(var he in hist)
           Assert.IsTrue( he.Count >countPerRandomSeed-tolerance && he.Count < countPerRandomSeed+tolerance);
        }

    }
}
