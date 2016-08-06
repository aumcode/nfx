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
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NFX.IO;

namespace NFX.NUnit
{

    [TestFixture]
    public class MiscUtilsTest
    {
         private class Tezt
         {
            public int A;
            public string B;
            public double C;
         }

        [TestCase]
        public void FirstMin()
        {
          var data = new []{ new Tezt{A=0, B="Car", C=12.1d},
                             new Tezt{A=2, B="Tzar", C=-21.7d},
                             new Tezt{A=-3, B="Zuma", C=10000d},
                           };

          var min = data.FirstMin(d => d.A);
          Assert.AreEqual(-3, min.A);
          Assert.AreEqual("Zuma", min.B);
          Assert.AreEqual(10000d, min.C);

          min = data.FirstMin(d => d.B);
          Assert.AreEqual(0, min.A);
          Assert.AreEqual("Car", min.B);
          Assert.AreEqual(12.1d, min.C);

          min = data.FirstMin(d => d.C);
          Assert.AreEqual(2, min.A);
          Assert.AreEqual("Tzar", min.B);
          Assert.AreEqual(-21.7d, min.C);

          double c;
          min = data.FirstMin(d => d.C, out c);
          Assert.AreEqual(2, min.A);
          Assert.AreEqual("Tzar", min.B);
          Assert.AreEqual(-21.7d, min.C);
          Assert.AreEqual(-21.7d, c);

        }

        [TestCase]
        public void FirstMax()
        {
          var data = new []{ new Tezt{A=0, B="Car", C=12.1d},
                             new Tezt{A=2, B="Tzar", C=-21.7d},
                             new Tezt{A=-3, B="Zuma", C=10000d},
                           };

          var max = data.FirstMax(d => d.A);
          Assert.AreEqual(2, max.A);
          Assert.AreEqual("Tzar", max.B);
          Assert.AreEqual(-21.7d, max.C);

          max = data.FirstMax(d => d.B);
          Assert.AreEqual(-3, max.A);
          Assert.AreEqual("Zuma", max.B);
          Assert.AreEqual(10000d, max.C);

          max = data.FirstMax(d => d.C);
          Assert.AreEqual(-3, max.A);
          Assert.AreEqual("Zuma", max.B);
          Assert.AreEqual(10000d, max.C);

          double c;
          max = data.FirstMax(d => d.C, out c);
          Assert.AreEqual(-3, max.A);
          Assert.AreEqual("Zuma", max.B);
          Assert.AreEqual(10000d, max.C);
          Assert.AreEqual(10000d, c);
        }

        [TestCase]
        public void FirstOrAnyOrDefault()
        {
          var data = new []{ new Tezt{A=0, B="Car", C=12.1d},
                             new Tezt{A=2, B="Tzar", C=-21.7d},
                             new Tezt{A=-3, B="Zuma", C=10000d},
                           };

          var m = data.FirstOrAnyOrDefault(elm => elm.B=="Tzar");
          Assert.AreEqual(2, m.A);

          m = data.FirstOrAnyOrDefault(elm => elm.B=="Sidor");
          Assert.AreEqual(0, m.A);

          data = null;
          m = data.FirstOrAnyOrDefault(elm => elm.B=="Sidor");
          Assert.IsNull(m);

          data = new Tezt[0];
          m = data.FirstOrAnyOrDefault(elm => elm.B=="Sidor");
          Assert.IsNull(m);
        }




        [TestCase]
        public void SkipLastTest()
        {
            var Seq = Enumerable.Range(1, 5);
            Assert.AreEqual("1,2,3,4", string.Join(",", Seq.SkipLast().ToArray()));
            Assert.AreEqual("1,2,3,4", string.Join(",", Seq.SkipLast(1).ToArray()));
            Assert.AreEqual("1,2,3",   string.Join(",", Seq.SkipLast(2).ToArray()));
            Assert.AreEqual("",        string.Join(",", Seq.SkipLast(5).ToArray()));
        }

        [TestCase]
        public void ReadWriteBEShortTestStream()
        {
            var ms = new MemoryStream();
            ms.WriteBEShort(789);
            Assert.AreEqual(2, ms.Position);
            Assert.AreEqual(new byte[] { 0x03, 0x15 }, ms.ToArray());

            ms.Position = 0;
            Assert.AreEqual(789, ms.ReadBEShort());
        }

        [TestCase]
        public void ReadWriteBEShortTestArray()
        {
            var buf = new byte[2];
            buf.WriteBEShort(0, 789);
            Assert.AreEqual(new byte[] { 0x03, 0x15 }, buf);

            var idx = 0;
            Assert.AreEqual(789, buf.ReadBEShort(ref idx));
            Assert.AreEqual(2, idx);
        }


        [TestCase]
        public void ReadWriteBEInt32TestStream()
        {
            var ms = new MemoryStream();
            ms.WriteBEInt32(16909060);
            Assert.AreEqual(4, ms.Position);
            Assert.AreEqual(new byte[] { 1, 2, 3, 4 }, ms.ToArray());

            ms.Position = 0;
            Assert.AreEqual(16909060, ms.ReadBEInt32());
        }

        [TestCase]
        public void ReadWriteBEInt32TestArray()
        {
            var buf = new byte[4];
            buf.WriteBEInt32(16909060);
            
            Assert.AreEqual(new byte[] { 1, 2, 3, 4 }, buf);

            Assert.AreEqual(16909060, buf.ReadBEInt32());

            var idx = 0;
            Assert.AreEqual(16909060, buf.ReadBEInt32(ref idx));
            Assert.AreEqual(4,idx);
        }

        [TestCase]
        public void ReadWriteBEInt64TestStream()
        {
            var ms = new MemoryStream();
            ms.WriteBEUInt64(0xFACACA07EBEDDAFE);
            Assert.AreEqual(8, ms.Position);
            Assert.AreEqual(new byte[] { 0xFA, 0xCA, 0xCA, 0x07, 0xEB, 0xED, 0xDA, 0xFE }, ms.ToArray());

            ms.Position = 0;
            Assert.AreEqual(0xFACACA07EBEDDAFE, ms.ReadBEUInt64());
        }

        [TestCase]
        public void ReadWriteBEInt64TestArray()
        {
            var buf = new byte[8];
            buf.WriteBEUInt64(0xFACACA07EBEDDAFE);
            
            Assert.AreEqual(new byte[] { 0xFA, 0xCA, 0xCA, 0x07, 0xEB, 0xED, 0xDA, 0xFE }, buf);
            Assert.AreEqual(0xFACACA07EBEDDAFE, buf.ReadBEUInt64());
            var idx = 0;
            Assert.AreEqual(0xFACACA07EBEDDAFE, buf.ReadBEUInt64(ref idx));
            Assert.AreEqual(8,idx);
        }


        [TestCase]
        public void StringLines()
        {
            var txt = 
@"A,b,
c,d,e
f
";
            Assert.AreEqual("A,b,", txt.ReadLine());

            var lines = txt.SplitLines();

            Assert.AreEqual(4, lines.Length);
            Assert.AreEqual("A,b,", lines[0]);
            Assert.AreEqual("c,d,e", lines[1]);
            Assert.AreEqual("f", lines[2]);
            Assert.AreEqual("", lines[3]);
        }


        [TestCase]
        public void Type_FullNameWithExpandedGenericArgs1()
        {
            var t = typeof(List<string>);
            
            Assert.AreEqual("System.Collections.Generic.List<System.String>", t.FullNameWithExpandedGenericArgs(false));
            Assert.AreEqual("@System.@Collections.@Generic.@List<@System.@String>", t.FullNameWithExpandedGenericArgs(true));
            Assert.AreEqual("@System.@Collections.@Generic.@List<@System.@String>", t.FullNameWithExpandedGenericArgs());
        }

        [TestCase]
        public void Type_FullNameWithExpandedGenericArgs2()
        {
            var t = typeof(int?);

            Assert.AreEqual("System.Nullable<System.Int32>", t.FullNameWithExpandedGenericArgs(false));
            Assert.AreEqual("@System.@Nullable<@System.@Int32>", t.FullNameWithExpandedGenericArgs(true));
            Assert.AreEqual("@System.@Nullable<@System.@Int32>", t.FullNameWithExpandedGenericArgs());
        }

        [TestCase]
        public void Type_FullNameWithExpandedGenericArgs3()
        {
            var t = typeof(Dictionary<DateTime?,List<bool?>>);
            Assert.AreEqual("System.Collections.Generic.Dictionary<System.Nullable<System.DateTime>, System.Collections.Generic.List<System.Nullable<System.Boolean>>>", t.FullNameWithExpandedGenericArgs(false));
            Assert.AreEqual("@System.@Collections.@Generic.@Dictionary<@System.@Nullable<@System.@DateTime>, @System.@Collections.@Generic.@List<@System.@Nullable<@System.@Boolean>>>", t.FullNameWithExpandedGenericArgs(true));
            Assert.AreEqual("@System.@Collections.@Generic.@Dictionary<@System.@Nullable<@System.@DateTime>, @System.@Collections.@Generic.@List<@System.@Nullable<@System.@Boolean>>>", t.FullNameWithExpandedGenericArgs());
        }

        [TestCase]
        public void Type_FullNameWithExpandedGenericArgs4()
        {
            var t = typeof(DateTime);
            Assert.AreEqual("System.DateTime", t.FullNameWithExpandedGenericArgs(false));
            Assert.AreEqual("@System.@DateTime", t.FullNameWithExpandedGenericArgs(true));
            Assert.AreEqual("@System.@DateTime", t.FullNameWithExpandedGenericArgs());
        }


        [TestCase]
        public void Type_DisplayNameWithExpandedGenericArgs1()
        {
            var t = typeof(List<string>);
            
            Assert.AreEqual("List<String>", t.DisplayNameWithExpandedGenericArgs());
        }

        [TestCase]
        public void Type_DisplayNameWithExpandedGenericArgs2()
        {
            var t = typeof(List<Dictionary<string, List<DateTime?>>>);
            
            Assert.AreEqual("List<Dictionary<String, List<Nullable<DateTime>>>>", t.DisplayNameWithExpandedGenericArgs());
        }

        [TestCase]
        public void Burmatographize1()
        {
            Assert.AreEqual("aDmi", "Dima".Burmatographize());
            Assert.AreEqual("aDmi", "Dima".Burmatographize(false));
            Assert.AreEqual("Daim", "Dima".Burmatographize(true));
        }

        [TestCase]
        public void Burmatographize2()
        {
            Assert.AreEqual("mDi", "Dim".Burmatographize());
            Assert.AreEqual("mDi", "Dim".Burmatographize(false));
            Assert.AreEqual("Dmi", "Dim".Burmatographize(true));

        }

         [TestCase]
        public void Burmatographize3()
        {
            Assert.AreEqual("iD", "Di".Burmatographize());
            Assert.AreEqual("iD", "Di".Burmatographize(false));
            Assert.AreEqual("Di", "Di".Burmatographize(true));

        }


         [TestCase]
        public void Burmatographize4()
        {
            Assert.AreEqual("D", "D".Burmatographize());
            Assert.AreEqual("D", "D".Burmatographize(false));
            Assert.AreEqual("D", "D".Burmatographize(true));

        }

         [TestCase]
        public void Burmatographize5()
        {
           var b = "Some.Assembly.Namespace1234.MyClassName".Burmatographize();
           Console.WriteLine(b);
           Assert.AreEqual("eSmoamNes.sAaslsCeymMb.l4y3.2N1aemceasp", b);
        }


        [TestCase]
        public void ArgsTpl1()
        {
           var s = "My first name is {@FirstName@} and last name is {@LastName@}"
                    .ArgsTpl(new{FirstName="Alex", LastName="Borisov"});
           Console.WriteLine(s);
           Assert.AreEqual("My first name is Alex and last name is Borisov", s);
        }

        [TestCase]
        public void ArgsTpl2()
        {
           var s = "My name is {@Name@} and i make {@Salary@,10} an hour"
                    .ArgsTpl(new{Name="Someone", Salary=125});
           Console.WriteLine(s);
           Assert.AreEqual("My name is Someone and i make        125 an hour", s);
        }


        [TestCase]
        public void MemBufferEquals_1()
        {
          var b1 = new byte[]{0,1,2,3,4,5,6,7,8,9,0,1};
          var b2 = new byte[]{0,1,2,3,4,5,6,7,8,9,0,1};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));
        }

        [TestCase]
        public void MemBufferEquals_2()
        {
          var b1 = new byte[]{0,1,2,3,4,5,6,7,8,9,0,1};
          var b2 = new byte[]{0,1,2,3,4,5,6,7,8,9,2,1};

          Assert.IsFalse( b1.MemBufferEquals( b2 ));
        }

        [TestCase]
        public void MemBufferEquals_3()
        {
          var b1 = new byte[]{0,1,2,3,4,5,6,7,8,9,0,1};
          var b2 = new byte[]{0,1,2,3,4,5,6,7,8,9,0,1,1};

          Assert.IsFalse( b1.MemBufferEquals( b2 ));
        }

        [TestCase]
        public void MemBufferEquals_4()
        {
          var b1 = new byte[]{};
          var b2 = new byte[]{};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));
        }

        [TestCase]
        public void MemBufferEquals_5()
        {
          var b1 = new byte[]{};
          byte[] b2 = null;

          Assert.IsFalse( b1.MemBufferEquals( b2 ));
        }

        [TestCase]
        public void MemBufferEquals_6()
        {
          var b1 = new byte[]{0};
          var b2 = new byte[]{0};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1};
          b2 = new byte[]{0,1};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88};
          b2 = new byte[]{0,1,88};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99};
          b2 = new byte[]{0,1,88,99};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99,22};
          b2 = new byte[]{0,1,88,99,22};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99,22,3};
          b2 = new byte[]{0,1,88,99,22,3};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99,22,3,0};
          b2 = new byte[]{0,1,88,99,22,3,0};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99,22,3,0,44};
          b2 = new byte[]{0,1,88,99,22,3,0,44};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99,22,3,0,44,122};
          b2 = new byte[]{0,1,88,99,22,3,0,44,122};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99,22,3,0,44,122,121};
          b2 = new byte[]{0,1,88,99,22,3,0,44,122,121};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));

          b1 = new byte[]{0,1,88,99,22,3,0,44,122,121,7};
          b2 = new byte[]{0,1,88,99,22,3,0,44,122,121,7};

          Assert.IsTrue( b1.MemBufferEquals( b2 ));
        }


        [TestCase]
        public void MemBufferEquals_Benchmark()
        {
          const int CNT = 10000000;

          var b1 = new byte[]{0,1,2,3,4,5,6,7,8,9,0,1,123,2,3,45,89,3,23,143,124,44,1,7,89,32,44,33,112};
          var b2 = new byte[]{0,1,2,3,4,5,6,7,8,9,0,1,123,2,3,45,89,3,23,143,124,44,1,7,89,32,44,33,112};


          var sw = System.Diagnostics.Stopwatch.StartNew();
          for(var i = 0; i<CNT; i++)
            Assert.IsTrue( b1.MemBufferEquals(b2) );
          
          sw.Stop();
          
          Console.WriteLine("Fast Compared {0} in {1}ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));

          sw = System.Diagnostics.Stopwatch.StartNew();
          for(var i = 0; i<CNT; i++)
           Assert.IsTrue( compareSlow(b1, b2) );
          sw.Stop();
          
          Console.WriteLine("Slow Compared {0} in {1}ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));

        }


             private bool compareSlow(byte[] b1, byte[] b2)
             {
               if (b1.Length != b2.Length) return false;
               for(var i=0; i<b1.Length; i++)
                if (b1[i] != b2[i]) return false;

               return true;
             }



        [TestCase]
        public void URI_Join()
        {
          Assert.AreEqual("static/site/content",  URIUtils.JoinPathSegs("static","site","content"));
          Assert.AreEqual("static/site/content",  URIUtils.JoinPathSegs(" static","  site  "," content"));
          Assert.AreEqual("static/site/content",  URIUtils.JoinPathSegs(" static"," \\ site  "," // content"));
          Assert.AreEqual("static/site/content",  URIUtils.JoinPathSegs(" static/","//site  "," // content"));
          Assert.AreEqual("static/site/content",  URIUtils.JoinPathSegs(" static/","/","/site","// content"));
          Assert.AreEqual("/static/site/content", URIUtils.JoinPathSegs("/static/","/","/site","// content"));
          Assert.AreEqual("/static/site/content", URIUtils.JoinPathSegs("      /static/","site","\\content"));
          Assert.AreEqual("/static/site/content", URIUtils.JoinPathSegs(" ", null, "      /static/","site","\\content"));
          Assert.AreEqual("static/site/content",  URIUtils.JoinPathSegs("static", null, "site","", "", "\\content"));
        }
    
        [Test]
        public void ComposeURLQueryString_Empty()
        {
          Dictionary<string, object> pars = null;
          var result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual(string.Empty, result);
        
          pars = new Dictionary<string, object>();
          result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual(string.Empty, result);
        }
        
        [Test]
        public void ComposeURLQueryString_NullOrEmptyQueryParts()
        {
          var pars = new Dictionary<string, object>
          {
            { "name", null }
          };
          var result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("name", result);
          
          pars = new Dictionary<string, object>
          {
            { "name", string.Empty },
          };
          result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("name=", result);
          
          pars = new Dictionary<string, object>
          {
            { string.Empty, "ABBA" }
          };
          result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual(string.Empty, result);
        
          pars = new Dictionary<string, object>
          {
            { "name1", null },
            { "name2", string.Empty },
            { string.Empty, "ABBA" }
          };
          result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("name1&name2=", result);
        
          pars = new Dictionary<string, object>
          {
            { "name1", string.Empty },
            { "name2", null },
            { string.Empty, "ABBA" },
            { "name3", "John" }
          };
          result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("name1=&name2&name3=John", result);
        }
        
        [Test]
        public void ComposeURLQueryString_SpecSymbols()
        {
          var pars = new Dictionary<string, object> { { "name", "Petrov" }, { "age", 19 }, { "spec", @" -y~!@#$%^&*()_?><|';:\/=+" } };
        
          var result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("name=Petrov&age=19&spec=%20-y%7E%21%40%23%24%25%5E%26%2A%28%29_%3F%3E%3C%7C%27%3B%3A%5C%2F%3D%2B", result);
        }
        
        [Test]
        public void ComposeURLQueryString_Types()
        {
          var pars = new Dictionary<string, object>
          {
            { "int", -257 },
            { "bool", true },
            { "double", 1.9D },
            { "string", "data&data" },
            { "dec", 23.45M },
            { "float", -12.34F }
          };
        
          var result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("int=-257&bool=True&double=1.9&string=data%26data&dec=23.45&float=-12.34", result);
        }
        
        [Test]
        public void ComposeURLQueryString_UTF8()
        {
          var pars = new Dictionary<string, object>
          {
            { "eng", "Hello!" },
            { "jap", "こんにちは" },
            { "chi", "久有归天愿"},
            { "chi2", "你好" },
            { "fra", "Allô" },
            { "привет", "rus" },
            { "नमस्कार", "hind" }
          };
        
          var result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("eng=Hello%21&jap=%E3%81%93%E3%82%93%E3%81%AB%E3%81%A1%E3%81%AF&chi=%E4%B9%85%E6%9C%89%E5%BD%92%E5%A4%A9%E6%84%BF&chi2=%E4%BD%A0%E5%A5%BD&fra=All%C3%B4&%D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82=rus&%E0%A4%A8%E0%A4%AE%E0%A4%B8%E0%A5%8D%E0%A4%95%E0%A4%BE%E0%A4%B0=hind", result);
        }
        
        [Test]
        public void ComposeURLQueryString_Mixed()
        {
          var pars = new Dictionary<string, object>
          {
            { "eng", "Hello!" },
            { "jap", null },
            { "chi", "久有归天愿"},
            { "chi2", 12 },
            { "", -123456 },
            { "привет", string.Empty },
            { "नमस्कार", null }
          };
        
          var result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("eng=Hello%21&jap&chi=%E4%B9%85%E6%9C%89%E5%BD%92%E5%A4%A9%E6%84%BF&chi2=12&%D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82=&%E0%A4%A8%E0%A4%AE%E0%A4%B8%E0%A5%8D%E0%A4%95%E0%A4%BE%E0%A4%B0", result);
        }
        
        [Test]
        public void ComposeURLQueryString_PlusAndSpaces()
        {
          var pars = new Dictionary<string, object>
          {
            { "eng", "Hello Lenin!" },
            { "rus", "Привет Ленин!" }
          };
        
          var result = URIUtils.ComposeURLQueryString(pars);
          Assert.AreEqual("eng=Hello%20Lenin%21&rus=%D0%9F%D1%80%D0%B8%D0%B2%D0%B5%D1%82%20%D0%9B%D0%B5%D0%BD%D0%B8%D0%BD%21", result);
        }

    }
}
