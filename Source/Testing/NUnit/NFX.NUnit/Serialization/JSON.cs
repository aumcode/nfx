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
using NFX.Collections;




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
        public void JSONDataMapFromURLEncoded_PlusSign()
        {
            var map = JSONDataMap.FromURLEncodedString("a=I+Am John&b=He Is++Not");

            Assert.AreEqual("I Am John", map["a"]);
            Assert.AreEqual("He Is  Not", map["b"]);
        }

        [TestCase]
        public void JSONDataMapFromURLEncoded_PlusAnd20Mix()
        {
            var map = JSONDataMap.FromURLEncodedString("a=I+Am%20John&b=He%20Is++Not");

            Assert.AreEqual("I Am John", map["a"]);
            Assert.AreEqual("He Is  Not", map["b"]);
        }

        [Test]
        public void JSONDataMapFromURLEncoded_Empty()
        {
          Assert.AreEqual(0, JSONDataMap.FromURLEncodedString(null).Count);
          Assert.AreEqual(0, JSONDataMap.FromURLEncodedString(string.Empty).Count);
          Assert.AreEqual(0, JSONDataMap.FromURLEncodedString(" ").Count);
          Assert.AreEqual(0, JSONDataMap.FromURLEncodedString("\r \n").Count);
          Assert.AreEqual(0, JSONDataMap.FromURLEncodedString("\t \t ").Count);
        }
        
        [Test]
        public void JSONDataMapFromURLEncoded_WOAmKey()
        {
          var dict = JSONDataMap.FromURLEncodedString("a");
        
          Assert.AreEqual(1, dict.Count);
          Assert.AreEqual(null, dict["a"]);
        }
        
        [Test]
        public void JSONDataMapFromURLEncoded_WOAmpKeyVal()
        {
          var dict = JSONDataMap.FromURLEncodedString("a=1");
        
          Assert.AreEqual(1, dict.Count);
          Assert.AreEqual("1", dict["a"]);
        }
        
        [Test]
        public void JSONDataMapFromURLEncoded_WOAmpVal()
        {
          var dict = JSONDataMap.FromURLEncodedString("=1");
        
          Assert.AreEqual(0, dict.Count);
        }
        
        [Test]
        public void JSONDataMapFromURLEncoded_DoubleEq()
        {
          var dict = JSONDataMap.FromURLEncodedString("a==1");
        
          Assert.AreEqual(1, dict.Count);
          Assert.AreEqual("=1", dict["a"]);
        }
        
        [TestCase("a=1&b=rrt")]
        [TestCase("a=1&b=rrt&")]
        [TestCase("&a=1&b=rrt&")]
        [TestCase("&&a=1&&b=rrt&&&")]
        public void JSONDataMapFromURLEncoded_KeyVal(string query)
        {
          var dict = JSONDataMap.FromURLEncodedString(query);
        
          Assert.AreEqual(2, dict.Count);
          Assert.AreEqual("1", dict["a"]);
          Assert.AreEqual("rrt", dict["b"]);
        }
        
        [Test]
        public void JSONDataMapFromURLEncoded_KeyEmptyEqNormal()
        {
          var dict = JSONDataMap.FromURLEncodedString("a=&b&&=&=14&c=3459");
        
          Assert.AreEqual(3, dict.Count);
          Assert.AreEqual(string.Empty, dict["a"]);
          Assert.AreEqual(null, dict["b"]);
          Assert.AreEqual("3459", dict["c"]);
        }
        
        [Test]
        public void JSONDataMapFromURLEncoded_Esc()
        {
          string[] strs = { " ", "!", "=", "&", "\"zele/m\\h()an\"" };
        
          foreach (var str in strs)
          {
            var query = "a=" + Uri.EscapeDataString(str);
        
            var dict = JSONDataMap.FromURLEncodedString(query);
        
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(str, dict["a"]);
          }
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



        [TestCase]
        public void NLSMap_Basic1()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}}";

            var nls = new NLSMap(content);

            Assert.IsTrue (nls["eng"].IsAssigned);
            Assert.IsFalse(nls["rus"].IsAssigned);

            Assert.AreEqual("Cucumber", nls["eng"].Name);
            Assert.AreEqual(null, nls["rus"].Name);

            Assert.AreEqual("It is green", nls["eng"].Description);
            Assert.AreEqual(null, nls["rus"].Description);
        }


        [TestCase]
        public void NLSMap_Basic2()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            Assert.IsTrue (nls["eng"].IsAssigned);
            Assert.IsTrue (nls["deu"].IsAssigned);
            Assert.IsFalse(nls["rus"].IsAssigned);

            Assert.AreEqual("Cucumber", nls["eng"].Name);
            Assert.AreEqual("Gurke", nls["deu"].Name);

            Assert.AreEqual("It is green", nls["eng"].Description);
            Assert.AreEqual("Es ist grün", nls["deu"].Description);
        }

        [TestCase]
        public void NLSMap_Basic1_WithRoot()
        {
            var content=@"{""nls"": {""eng"": {""n"": ""Cucumber"",""d"": ""It is green""}}}";

            var nls = new NLSMap(content);

            Assert.IsTrue (nls["eng"].IsAssigned);
            Assert.IsFalse(nls["rus"].IsAssigned);

            Assert.AreEqual("Cucumber", nls["eng"].Name);
            Assert.AreEqual(null, nls["rus"].Name);

            Assert.AreEqual("It is green", nls["eng"].Description);
            Assert.AreEqual(null, nls["rus"].Description);
        }

        [TestCase]
        public void NLSMap_Basic2_WithRoot()
        {
            var content=@"{""nls"": {""eng"": {""n"": ""Cucumber"",""d"": ""It is green""}, ""deu"": {""n"": ""Gurke"", ""d"": ""Es ist grün""}}}";

            var nls = new NLSMap(content);

            Assert.IsTrue (nls["eng"].IsAssigned);
            Assert.IsTrue (nls["deu"].IsAssigned);
            Assert.IsFalse(nls["rus"].IsAssigned);

            Assert.AreEqual("Cucumber", nls["eng"].Name);
            Assert.AreEqual("Gurke", nls["deu"].Name);

            Assert.AreEqual("It is green", nls["eng"].Description);
            Assert.AreEqual("Es ist grün", nls["deu"].Description);
        }

        [TestCase]
        public void NLSMap_SerializeAll()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            var json = nls.ToJSON();
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Assert.IsNotNull(read);

            Assert.AreEqual("Cucumber", read.eng.n);
            Assert.AreEqual("Gurke", read.deu.n);
        }

        [TestCase]
        public void NLSMap_SerializeOnlyOneExisting()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);


            var options = new JSONWritingOptions{ NLSMapLanguageISO = "deu", Purpose = JSONSerializationPurpose.UIFeed};
            var json = nls.ToJSON(options);
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Assert.IsNotNull(read);

            Assert.AreEqual("Gurke", read.n);
            Assert.AreEqual("Es ist grün", read.d);
        }

        [TestCase]
        public void NLSMap_SerializeOnlyOneNoneExisting()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);


            var options = new JSONWritingOptions{ NLSMapLanguageISO = "rus", Purpose = JSONSerializationPurpose.UIFeed};
            var json = nls.ToJSON(options);
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Assert.IsNotNull(read);

            Assert.AreEqual("Cucumber", read.n);
            Assert.AreEqual("It is green", read.d);
        }

        [TestCase]
        public void NLSMap_Get_Name()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            var name = nls.Get(NLSMap.GetParts.Name);
            Assert.AreEqual("Cucumber", name);

            name = nls.Get(NLSMap.GetParts.Name, "deu");
            Assert.AreEqual("Gurke", name);

            name = nls.Get(NLSMap.GetParts.Name, "XXX");
            Assert.AreEqual("Cucumber", name);

            name = nls.Get(NLSMap.GetParts.Name, "XXX", dfltLangIso: "ZZZ");
            Assert.IsNull(name);
        }

        [TestCase]
        public void NLSMap_Get_Descr()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            var descr = nls.Get(NLSMap.GetParts.Description);
            Assert.AreEqual("It is green", descr);

            descr = nls.Get(NLSMap.GetParts.Description, "deu");
            Assert.AreEqual("Es ist grün", descr);

            descr = nls.Get(NLSMap.GetParts.Description, "XXX");
            Assert.AreEqual("It is green", descr);

            descr = nls.Get(NLSMap.GetParts.Description, "XXX", dfltLangIso: "ZZZ");
            Assert.IsNull(descr);
        }

        [TestCase]
        public void NLSMap_Get_NameOrDescr()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var nord = nls.Get(NLSMap.GetParts.NameOrDescription);
            Assert.AreEqual("Cucumber", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "deu");
            Assert.AreEqual("Gurke", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "XXX");
            Assert.AreEqual("Cucumber", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "rus");
            Assert.AreEqual("On Zeleniy", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "XXX", dfltLangIso: "ZZZ");
            Assert.IsNull(nord);
        }

        [TestCase]
        public void NLSMap_Get_DescrOrName()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var dorn = nls.Get(NLSMap.GetParts.DescriptionOrName);
            Assert.AreEqual("It is green", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "deu");
            Assert.AreEqual("Es ist grün", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "XXX");
            Assert.AreEqual("It is green", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "rus");
            Assert.AreEqual("On Zeleniy", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "XXX", dfltLangIso: "ZZZ");
            Assert.IsNull(dorn);
        }


        [TestCase]
        public void NLSMap_Get_NameAndDescr()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var nand = nls.Get(NLSMap.GetParts.NameAndDescription);
            Assert.AreEqual("Cucumber - It is green", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "deu");
            Assert.AreEqual("Gurke - Es ist grün", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "XXX");
            Assert.AreEqual("Cucumber - It is green", nand);

             nand = nls.Get(NLSMap.GetParts.NameAndDescription, "YYY", concat: "::");
            Assert.AreEqual("Cucumber::It is green", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "rus");
            Assert.AreEqual("On Zeleniy", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "XXX", dfltLangIso: "ZZZ");
            Assert.IsNull(nand);
        }

        [TestCase]
        public void NLSMap_Get_DescrAndName()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var dan = nls.Get(NLSMap.GetParts.DescriptionAndName);
            Assert.AreEqual("It is green - Cucumber", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "deu");
            Assert.AreEqual("Es ist grün - Gurke", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "XXX");
            Assert.AreEqual("It is green - Cucumber", dan);

             dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "YYY", concat: "::");
            Assert.AreEqual("It is green::Cucumber", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "rus");
            Assert.AreEqual("On Zeleniy", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "XXX", dfltLangIso: "ZZZ");
            Assert.IsNull(dan);
        }


        [TestCase]
        public void StringMap_Compact()
        {
            var map = new StringMap
            {
              {"p1", "v1"},
              {"p2", "v2"},
              {"Age", "149"}
            };

            var json = map.ToJSON(JSONWritingOptions.Compact);
            Console.WriteLine(json);

            Assert.AreEqual(@"{""p1"":""v1"",""p2"":""v2"",""Age"":""149""}",json);


            dynamic read = json.JSONToDynamic();
            Assert.IsNotNull(read);

            Assert.AreEqual("v1", read.p1);
            Assert.AreEqual("v2", read.p2);
            Assert.AreEqual("149", read.Age);
        }

        [TestCase]
        public void StringMap_Pretty()
        {
            var map = new StringMap
            {
              {"p1", "v1"},
              {"p2", "v2"},
              {"Age", "149"}
            };

            var json = map.ToJSON(JSONWritingOptions.PrettyPrint);
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Assert.IsNotNull(read);

            Assert.AreEqual("v1", read.p1);
            Assert.AreEqual("v2", read.p2);
            Assert.AreEqual("149", read.Age);
        }


    }
}
