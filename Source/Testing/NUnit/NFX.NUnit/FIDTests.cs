/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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

namespace NFX.NUnit
{
    [TestFixture]
    public class FIDTests
    {
        [TestCase]
        public void FID1()
        {
            var f = new FID(1);
            var s = f.ToString();
            Console.WriteLine(s);
            Assert.AreEqual("0-0-1", s);
        }

        [TestCase]
        public void FID2()
        {
            var f = new FID(4353425342532);
            var s = f.ToString();
            Console.WriteLine(s);
            Assert.AreEqual("3-16096351-68", s);
        }

        [TestCase]
        public void FID3()
        {
            var CNT = 500000;

            for(var c=0; c<100; c++)
            {
                var set1 = new FID[CNT];
                var set2 = new FID[CNT];
                for(var i=0; i<CNT; i++)
                {
                  set1[i] = FID.Generate();
                  set2[i] = FID.Generate();
                }

                Assert.IsFalse( set1.Intersect(set2).Any() );
                Console.WriteLine("{0}: Set of {1} FIDs no intersection", c, CNT);
            }
        }

        [TestCase]
        public void FID4()
        {
            var CNT = 100000;

            var tasks = new List<Task>();
            var sets= new List<FID>(); 

            for(var c=0; c<100; c++)
            {
                tasks.Add( Task.Factory.StartNew(()=>
                {
                  var set = new FID[CNT];
                  for(var i=0; i<CNT; i++)
                  {
                    set[i] = FID.Generate();
                  }
                  
                  lock(sets)
                   sets.AddRange(set);
                }));
            }

            Task.WaitAll( tasks.ToArray() );
            Console.WriteLine("Analyzing {0:n} FIDs", sets.Count);
            Assert.IsTrue( sets.Distinct().Count() == sets.Count() );
        }
        
    }
}
