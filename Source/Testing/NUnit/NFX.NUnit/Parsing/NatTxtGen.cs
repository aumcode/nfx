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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NUnit.Framework;

using NFX.Parsing;


namespace NFX.NUnit.Parsing
{
    [TestFixture]   
    public class NatTxtGen
    {
        [TestCase(25)]
        [TestCase(50)]
        [TestCase(155)]
        public void GenerateUpToSize(int sz)
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.Generate(sz);
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length > 0);
            Assert.IsTrue(txt.Length <= sz);
          }
        }

        [Test]
        public void GenerateDefault()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.Generate();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length > 4);
            
          }
        }

        [Test]
        public void GenerateRandomSizes()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.Generate(0);
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length > 4);
            
          }
        }

        
        [TestCase(6, 10)]
        [TestCase(8, 20)]
        public void GenerateWordSizes(int min, int max)
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateWord(min, max);
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= min);
            Assert.IsTrue(txt.Length <= max);
          }
        }

        [Test]
        public void GenerateDefaultWordSizes()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateWord();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 4);
            Assert.IsTrue(txt.Length <= 20);
          }
        }

        [Test]
        public void GenerateLastNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateLastName();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 3);
            Assert.IsTrue(txt.Length <= 20);
          }
        }

        [Test]
        public void GenerateFirstNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateFirstName();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 3);
            Assert.IsTrue(txt.Length <= 20);
          }
        }

        [Test]
        public void GenerateFulltNames_woMiddle()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateFullName();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 6);
            Assert.IsTrue(txt.Length <= 40);
          }
        }

         [Test]
        public void GenerateFulltNames_withMiddle()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateFullName(true);
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 6);
            Assert.IsTrue(txt.Length <= 40);
          }
        }

        [Test]
        public void GenerateCityNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateCityName();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 3);
            Assert.IsTrue(txt.Length <= 40);
          }
        }

        [TestCase(    1000)]
        [TestCase(   10000)]
        [TestCase(  100000)]
        [TestCase( 1000000)]
        [TestCase( 5000000)]
        public void AnalyzeUniqueness(int CNT)
        {
          var fnames = new List<string>();
          var lnames = new List<string>();
          var flnames = new List<string>();
          var cities = new List<string>();

          for (var i=0; i<CNT; i++)
          {
            var fn = NaturalTextGenerator.GenerateFirstName();
            fnames.Add( fn );
            var ln = NaturalTextGenerator.GenerateLastName();
            lnames.Add( ln );
            flnames.Add(fn+ " " + ln);
            cities.Add( NaturalTextGenerator.GenerateCityName() );
          }
          Console.WriteLine("Generated {0:n0} times", CNT);
          Console.WriteLine("----------------------------");
          var dfn = fnames.Distinct().Count();
          var pfn = 100d * (dfn / (double)CNT);
          Console.WriteLine(" First names {0:n0} unique {1:n3}%", dfn, pfn);
          
          var dln = lnames.Distinct().Count();
          var pln = 100d * (dln / (double)CNT);
          Console.WriteLine(" Last names {0:n0} unique {1:n3}%", dln, pln);

          var dfln = flnames.Distinct().Count();
          var pfln = 100d * (dfln / (double)CNT);
          Console.WriteLine(" First+Last names {0:n0} unique {1:n3}%", dfln, pfln);
          Assert.IsTrue( pfln > 85d);//85% uniqueness

          var dct = cities.Distinct().Count();
          var pct = 100d * (dct / (double)CNT);
          Console.WriteLine(" Cities {0:n0} unique {1:n3}%", dct, pct);
          Console.WriteLine();
        }


        [Test]
        public void GenerateUSCityStateZipNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateUSCityStateZip();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 3);
            Assert.IsTrue(txt.Length <= 50);
          }
        }

        [Test]
        public void GenerateAddressLines()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateAddressLine();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 3);
            Assert.IsTrue(txt.Length <= 100);
          }
        }

        [Test]
        public void GenerateEMails()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateEMail();
            Console.WriteLine( txt );
            Assert.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(txt.Length >= 3);
            Assert.IsTrue(txt.Length <= 100);
          }
        }

        [Test]
        public void GenerateFullInfo()
        {
          for (var i=0; i<100; i++)
          {
            var name = NaturalTextGenerator.GenerateFirstName()+" "+NaturalTextGenerator.GenerateLastName();
            var addr = NaturalTextGenerator.GenerateAddressLine();
            var csz = NaturalTextGenerator.GenerateUSCityStateZip();
            Console.WriteLine( name );
            Console.WriteLine( addr );
            Console.WriteLine( csz );
            Console.WriteLine("-------------------------------------------" );
            Assert.IsTrue( name.IsNotNullOrWhiteSpace() );
            Assert.IsTrue( addr.IsNotNullOrWhiteSpace() );
            Assert.IsTrue( csz.IsNotNullOrWhiteSpace() );
            Assert.IsTrue(name.Length >= 3);
            Assert.IsTrue(name.Length <= 100);
            Assert.IsTrue(addr.Length >= 3);
            Assert.IsTrue(addr.Length <= 100);
            Assert.IsTrue(csz.Length >= 3);
            Assert.IsTrue(csz.Length <= 100);
          }
        }


        [Test]
        public void PerfFullInfo()
        {
          const int CNT = 1000000;
          var sw = Stopwatch.StartNew();
          for (var i=0; i<CNT; i++)
          {
            var name = NaturalTextGenerator.GenerateFirstName()+" "+NaturalTextGenerator.GenerateLastName();
            var addr = NaturalTextGenerator.GenerateAddressLine();
            var csz = NaturalTextGenerator.GenerateUSCityStateZip();
          }

          var elapsed = sw.ElapsedMilliseconds;
          var ops = CNT / (elapsed / 1000d);

          Assert.IsTrue( ops > 250000);//250,000 ops/sec

          Console.WriteLine("Genereated {0} full infos, in {1:n0} ms at {2:n0} ops/sec", CNT, elapsed, ops);
        }


        [Test]
        public void PerfFullInfo_Parallel()
        {
          const int CNT = 3000000;
          var sw = Stopwatch.StartNew();
          System.Threading.Tasks.Parallel.For(0, CNT,
          (i)=>
          {
            var name = NaturalTextGenerator.GenerateFirstName()+" "+NaturalTextGenerator.GenerateLastName();
            var addr = NaturalTextGenerator.GenerateAddressLine();
            var csz = NaturalTextGenerator.GenerateUSCityStateZip();
          });

          var elapsed = sw.ElapsedMilliseconds;
          var ops = CNT / (elapsed / 1000d);

          Assert.IsTrue( ops > 900000);//900,000 ops/sec

          Console.WriteLine("Genereated {0} full infos, in {1:n0} ms at {2:n0} ops/sec", CNT, elapsed, ops);
        }



    }
}


