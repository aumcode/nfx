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
using System.Text;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using NFX.CodeAnalysis;
using NFX.CodeAnalysis.Source;
using NFX.CodeAnalysis.JSON;
using NFX.Serialization.JSON;
using JL=NFX.CodeAnalysis.JSON.JSONLexer;
using JP=NFX.CodeAnalysis.JSON.JSONParser;

namespace NFX.NUnit.CodeAnalysis
{
    [TestFixture]   
    public class JSONParser
    {

        [TestCase]
        public void RootLiteral_String()
        {
          var src = @"'abc'";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual("abc", parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_Int()
        {
          var src = @"12";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(12, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_NegativeDecimalInt()
        {
          var src = @"-16";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(-16, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_PositiveDecimalInt()
        {
          var src = @"+16";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(16, parser.ResultContext.ResultObject);
        }


        [TestCase]
        public void RootLiteral_NegativeHexInt()
        {
          var src = @"-0xf";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(-15, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_PositiveHexInt()
        {
          var src = @"+0xf";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(15, parser.ResultContext.ResultObject);
        }


        [TestCase]
        public void RootLiteral_Double()
        {
          var src = @"12.7";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(12.7, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_NegativeDouble()
        {
          var src = @"-12.7";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(-12.7, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_PositiveDouble()
        {
          var src = @"+12.7";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(12.7, parser.ResultContext.ResultObject);
        }



        [TestCase]
        public void RootLiteral_ScientificDouble()
        {
          var src = @"12e2";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(12e2d, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_NegativeScientificDouble()
        {
          var src = @"-12e2";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(-12e2d, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_PositiveScientificDouble()
        {
          var src = @"+12e2";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(12e2d, parser.ResultContext.ResultObject);
        }




        [TestCase]
        public void RootLiteral_True()
        {
          var src = @"true";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(true, parser.ResultContext.ResultObject);
        }


        [TestCase]
        public void RootLiteral_False()
        {
          var src = @"false";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(false, parser.ResultContext.ResultObject);
        }

        [TestCase]
        public void RootLiteral_Null()
        {
          var src = @"null";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          
          Assert.AreEqual(null, parser.ResultContext.ResultObject);
        }


        [TestCase]
        public void RootArray()
        {
          var src = @"[1,2,3]";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataArray), parser.ResultContext.ResultObject);
          var arr = (JSONDataArray)parser.ResultContext.ResultObject;

          Assert.AreEqual(3, arr.Count);
          Assert.AreEqual(1, arr[0]);
          Assert.AreEqual(2, arr[1]);
          Assert.AreEqual(3, arr[2]);
        }

        [TestCase]
        public void RootEmptyArray()
        {
          var src = @"[]";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataArray), parser.ResultContext.ResultObject);
          var arr = (JSONDataArray)parser.ResultContext.ResultObject;

          Assert.AreEqual(0, arr.Count);
        }

        [TestCase]
        public void RootObject()
        {
          var src = @"{a: 1, b: true, c: null}";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataMap), parser.ResultContext.ResultObject);
          var obj = (JSONDataMap)parser.ResultContext.ResultObject;

          Assert.AreEqual(3, obj.Count);
          Assert.AreEqual(1, obj["a"]);
          Assert.AreEqual(true, obj["b"]);
          Assert.AreEqual(null, obj["c"]);
        }

        [TestCase]
        public void RootEmptyObject()
        {
          var src = @"{}";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataMap), parser.ResultContext.ResultObject);
          var obj = (JSONDataMap)parser.ResultContext.ResultObject;

          Assert.AreEqual(0, obj.Count);
        }

        [TestCase]
        public void RootObjectWithArray()
        {
          var src = @"{age: 12, numbers: [4,5,6,7,8,9], name: ""Vasya""}";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataMap), parser.ResultContext.ResultObject);
          var obj = (JSONDataMap)parser.ResultContext.ResultObject;

          Assert.AreEqual(3, obj.Count);
          Assert.AreEqual(12, obj["age"]);
          Assert.AreEqual("Vasya", obj["name"]);
          Assert.AreEqual(6, ((JSONDataArray)obj["numbers"]).Count);
          Assert.AreEqual(7, ((JSONDataArray)obj["numbers"])[3]);
        }

        [TestCase]
        public void RootObjectWithSubObjects()
        {
          var src = @"{age: 120, numbers: {positive: true, bad: 12.7}, name: ""Vasya""}";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataMap), parser.ResultContext.ResultObject);
          var obj = (JSONDataMap)parser.ResultContext.ResultObject;

          Assert.AreEqual(3, obj.Count);
          Assert.AreEqual(120, obj["age"]);
          Assert.AreEqual("Vasya", obj["name"]);
          Assert.AreEqual(true, ((JSONDataMap)obj["numbers"])["positive"]);
          Assert.AreEqual(12.7, ((JSONDataMap)obj["numbers"])["bad"]);
        }

        [TestCase]
        public void ParseError1()
        {
          var src = @"{age 120}";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.AreEqual(1, parser.Messages.Count);
          Assert.AreEqual((int)JSONMsgCode.eColonOperatorExpected,  parser.Messages[0].Code);
        }


        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eColonOperatorExpected", MatchType=MessageMatch.Contains)]
        public void ParseError2()
        {
          var src = @"{age 120}";

          var parser = new JP(  new JL( new StringSource(src) ), throwErrors: true  );

          parser.Parse();

        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="ePrematureEOF", MatchType=MessageMatch.Contains)]
        public void ParseError3()
        {
          var src = @"{age: 120";

          var parser = new JP(  new JL( new StringSource(src) ), throwErrors: true  );

          parser.Parse();

        }


        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedObject", MatchType=MessageMatch.Contains)]
        public void ParseError4()
        {
          var src = @"{age: 120 d";

          var parser = new JP(  new JL( new StringSource(src) ), throwErrors: true  );

          parser.Parse();

        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eUnterminatedArray", MatchType=MessageMatch.Contains)]
        public void ParseError5()
        {
          var src = @"['age', 120 d";

          var parser = new JP(  new JL( new StringSource(src) ), throwErrors: true  );

          parser.Parse();

        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eSyntaxError", MatchType=MessageMatch.Contains)]
        public void ParseError6()
        {
          var src = @"[age: 120 d";

          var parser = new JP(  new JL( new StringSource(src) ), throwErrors: true  );

          parser.Parse();

        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eObjectKeyExpected", MatchType=MessageMatch.Contains)]
        public void ParseError7()
        {
          var src = @"{ true: 120}";

          var parser = new JP(  new JL( new StringSource(src) ), throwErrors: true  );

          parser.Parse();

        }

        [TestCase]
        [ExpectedException(typeof(CodeProcessorException), ExpectedMessage="eDuplicateObjectKey", MatchType=MessageMatch.Contains)]
        public void ParseError8()
        {
          var src = @"{ a: 120, b: 140, a: 12}";

          var parser = new JP(  new JL( new StringSource(src) ), throwErrors: true  );

          parser.Parse();
        }



        [TestCase]
        public void RootComplexObject()
        {
          var src = 
@"
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
    #HOT_TOPIC
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

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataMap), parser.ResultContext.ResultObject);
          var obj = (JSONDataMap)parser.ResultContext.ResultObject;

          Assert.AreEqual(7, obj.Count);
          Assert.AreEqual("Oleg", obj["FirstName"]);
          Assert.AreEqual("Ogurtsov", obj["LastName"]);
          Assert.AreEqual("V.", obj["Middle Name"]);
          Assert.AreEqual("Shamanov", obj["Crazy\nName"]);
         
          var lucky = obj["LuckyNumbers"] as JSONDataArray;
          Assert.IsNotNull(lucky);
          Assert.AreEqual(6, lucky.Count);
          Assert.AreEqual(4, lucky[0]);
          Assert.AreEqual(9, lucky[5]);

          var history = obj["History"] as JSONDataArray;
          Assert.IsNotNull(history);
          Assert.AreEqual(2, history.Count);
          
          var ww2 = history[1] as JSONDataMap;
          Assert.IsNotNull(ww2);
          Assert.AreEqual(3, ww2.Count);

          var who = ww2["Who"] as JSONDataArray;
          Assert.IsNotNull(who);
          Assert.AreEqual(6, who.Count);
          Assert.AreEqual("USA", who[2]);
        }


         [TestCase]
        public void RootComplexArray()
        {
          var src = 
@"[
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
    #HOT_TOPIC
    {Date: '05/14/1905', What: 'Tsushima'},
    #MODERN_TOPIC
    {Date: '09/01/1939', What: 'WW2 Started', Who: ['Germany','USSR', 'USA', 'Japan', 'Italy', 'Others']}
  ] ,
  Note:
$'This note text
can span many lines
and
this \r\n is not escape'
 }, 
 123
]";

          var parser = new JP(  new JL( new StringSource(src) )  );

          parser.Parse();

          Assert.IsInstanceOf(typeof(JSONDataArray), parser.ResultContext.ResultObject);
          var arr = (JSONDataArray)parser.ResultContext.ResultObject;
          Assert.AreEqual(2, arr.Count);
          Assert.AreEqual(123, arr[1]);

          var obj = (JSONDataMap)arr[0];

          Assert.AreEqual(7, obj.Count);
          Assert.AreEqual("Oleg", obj["FirstName"]);
          Assert.AreEqual("Ogurtsov", obj["LastName"]);
          Assert.AreEqual("V.", obj["Middle Name"]);
          Assert.AreEqual("Shamanov", obj["Crazy\nName"]);
         
          var lucky = obj["LuckyNumbers"] as JSONDataArray;
          Assert.IsNotNull(lucky);
          Assert.AreEqual(6, lucky.Count);
          Assert.AreEqual(4, lucky[0]);
          Assert.AreEqual(9, lucky[5]);

          var history = obj["History"] as JSONDataArray;
          Assert.IsNotNull(history);
          Assert.AreEqual(2, history.Count);
          
          var ww2 = history[1] as JSONDataMap;
          Assert.IsNotNull(ww2);
          Assert.AreEqual(3, ww2.Count);

          var who = ww2["Who"] as JSONDataArray;
          Assert.IsNotNull(who);
          Assert.AreEqual(6, who.Count);
          Assert.AreEqual("USA", who[2]);
        }


    }


}
