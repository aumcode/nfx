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
using System.Diagnostics;

using NUnit.Framework;

using NFX.IO;
using NFX.Serialization.JSON;




namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class JSON
    { 
        [TestCase]
        public void ReadSimple()
        {
            var obj = "{a: 2}".JSONToDynamic();

            Assert.AreEqual(2, obj.a);
         
        }

        [TestCase]
        public void ReadSimple2()
        {
            var obj = "{a: -2, b: true, c: false, d: 'hello'}".JSONToDynamic();

            Assert.AreEqual(-2, obj.a);
            Assert.AreEqual(true, obj.b);
            Assert.AreEqual(false, obj.c);
            Assert.AreEqual("hello", obj.d);
         
        }

        [TestCase]
        public void ReadSimpleNameWithSpace()
        {
            var obj = @"{a: -2, 'b or \'': 'yes, ok', c: false, d: 'hello' }".JSONToDynamic();

            Assert.AreEqual(-2, obj.a);
            Assert.AreEqual("yes, ok", obj["b or '"]);
            Assert.AreEqual(false, obj.c);
            Assert.AreEqual("hello", obj.d);
            //Assert.AreEqual(2147483650, obj.e);
        }


        [TestCase]
        public void JSONDataMapFromURLEncoded_1()
        {
            var map = JSONDataMap.FromURLEncodedString("name=Alex&title=Professor");

            Assert.AreEqual("Alex", map["name"]);
            Assert.AreEqual("Professor", map["title"]);
        }

        [TestCase]
        public void JSONDataMapFromURLEncoded_2()
        {
            var map = JSONDataMap.FromURLEncodedString("one=a%2Bb+%3E+123&two=Hello+%26+Welcome.");

            Assert.AreEqual("a+b > 123", map["one"]);
            Assert.AreEqual("Hello & Welcome.", map["two"]);
        }

        [TestCase]
        public void JSONDataMapFromURLEncoded_3()
        {
            var map = JSONDataMap.FromURLEncodedString("one=a%2Bb+%3E+123+%3D+true&two=Hello+%26+Welcome.%E4%B9%85%E6%9C%89%E5%BD%92%E5%A4%A9%E6%84%BF");

            Assert.AreEqual("a+b > 123 = true", map["one"]);
            Assert.AreEqual("Hello & Welcome.久有归天愿", map["two"]);
        }



        [TestCase]
        public void ReadRootComplexObject()
        {
            var obj = @"
 {FirstName: ""Oleg"",  //comments dont hurt
  'LastName': ""Ogurtsov"",
  ""Middle Name"": 'V.',
  ""Crazy\nName"": 'Shamanov',
  LuckyNumbers: [4,5,6,7,8,9], 
  /* comments
  do not break stuff */
  |* in this JSON superset *|
  History: 
  [
    #HOT_TOPIC    //ability to use directive pragmas
    {Date: '05/14/1905', What: 'Tsushima'},
    #MODERN_TOPIC
    {Date: '09/01/1939', What: 'WW2 Started', Who: ['Germany','USSR', 'USA', 'Japan', 'Italy', 'Others']}
  ] ,
  Note:
$'This note text
can span many lines
and
this \r\n is not escape'
 }
".JSONToDynamic();

            Assert.AreEqual("Oleg", obj.FirstName);
            Assert.AreEqual("Ogurtsov", obj.LastName);
            Assert.AreEqual("V.", obj["Middle Name"]);
            Assert.AreEqual("Shamanov", obj["Crazy\nName"]);
            Assert.AreEqual(6, obj.LuckyNumbers.Count);
            Assert.AreEqual(6, obj.LuckyNumbers.List.Count);
            Assert.AreEqual(7, obj.LuckyNumbers[3]);
            Assert.AreEqual("USSR", obj.History[1].Who[1]);
         
        }

         [TestCase]
        public void ParallelDeserializationManyComplex()
        {
            const int TOTAL = 1000;
            var src = @"
 {FirstName: ""Oleg"",  //comments dont hurt
  'LastName': ""Ogurtsov"",
  ""Middle Name"": 'V.',
  ""Crazy\nName"": 'Shamanov',
  LuckyNumbers: [4,5,6,7,8,9], 
  /* comments
  do not break stuff */
  |* in this JSON superset *|
  History: 
  [
    #HOT_TOPIC    //ability to use directive pragmas
    {Date: '05/14/1905', What: 'Tsushima'},
    #MODERN_TOPIC
    {Date: '09/01/1939', What: 'WW2 Started', Who: ['Germany','USSR', 'USA', 'Japan', 'Italy', 'Others']}
  ] ,
  Note:
$'This note text
can span many lines
and
this \r\n is not escape'
 }
";
            var watch = Stopwatch.StartNew();

            System.Threading.Tasks.Parallel.For
            (0, TOTAL,
                (i)=>
                {
                    var obj = src.JSONToDynamic();
                    Assert.AreEqual("Oleg", obj.FirstName);
                    Assert.AreEqual("Ogurtsov", obj.LastName);
                    Assert.AreEqual("V.", obj["Middle Name"]);
                    Assert.AreEqual("Shamanov", obj["Crazy\nName"]);
                    Assert.AreEqual(6, obj.LuckyNumbers.Count);
                    Assert.AreEqual(6, obj.LuckyNumbers.List.Count);
                    Assert.AreEqual(7, obj.LuckyNumbers[3]);
                    Assert.AreEqual("USSR", obj.History[1].Who[1]);
                }
            );

            var time = watch.ElapsedMilliseconds;
            Console.WriteLine("Long JSON->dynamic deserialization test took {0}ms for {1} objects @ {2}op/sec"
                              .Args(time, TOTAL, TOTAL / (time / 1000d))
                              );
        }

        [TestCase]
        public void JSONDoubleTest()
        {
            var map = JSONReader.DeserializeDataObject( "{ pi: 3.14159265359, exp1: 123e4, exp2: 2e-5 }" ) as JSONDataMap;
            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(3.14159265359D, map["pi"]);
            Assert.AreEqual(123e4D, map["exp1"]);
            Assert.AreEqual(2e-5D, map["exp2"]);
        }


    }
}
