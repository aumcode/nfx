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
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.IO;
using NFX.CodeAnalysis;
using NFX.CodeAnalysis.Source;
using NFX.CodeAnalysis.JSON;
using NFX.Serialization.JSON;
using JL=NFX.CodeAnalysis.JSON.JSONLexer;
using JP=NFX.CodeAnalysis.JSON.JSONParser;
using JW=NFX.Serialization.JSON.JSONWriter;
using JDO=NFX.Serialization.JSON.JSONDynamicObject;



namespace NFX.NUnit.Integration.Serialization
{


    [TestFixture]
    public class JSON
    { 
        private const int BENCHMARK_ITERATIONS = 10000;//250000;
   
        private const string SOURCE1 = @"
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

private const string SOURCE2 = @"
 {FirstName: ""Oleg"", 
  ""LastName"": ""Ogurtsov"",
  ""Middle Name"": ""V."",
  ""Crazy\nName"": ""Shamanov"",
  LuckyNumbers: [4,5,6,7,8,9], 
  History: 
  [
    {Date: ""05/14/1905"", What: ""Tsushima""},
    {Date: ""09/01/1939"", What: ""WW2 Started"", Who: [""Germany"",""USSR"", ""USA"", ""Japan"", ""Italy"", ""Others""]}
  ] ,
  Note: ""This note text can span many lines""
 }
";




        [TestCase]
        public void ParallelDeserializationOfManyComplexObjects_JSONPlus()
        {
          parallelDeserializationOfManyComplexObjects(SOURCE1, 12);
        }

         [TestCase]
        public void ParallelDeserializationOfManyComplexObjects_JSONRegular()
        {
          parallelDeserializationOfManyComplexObjects(SOURCE2, 12);
        }


        private void parallelDeserializationOfManyComplexObjects(string src, int threadCount)
        {
            const int TOTAL = 1000000;//100;//500000;
         
            var watch = Stopwatch.StartNew();
            
            System.Threading.Tasks.Parallel.For
            (0, TOTAL,
                new ParallelOptions(){ MaxDegreeOfParallelism = threadCount},
                (i)=>
                {
                    var obj = src.JSONToDynamic();
              //      var obj = System.Web.Helpers.Json.Decode(src);
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
        public void Benchmark_Serialize_PersonClass()
        {
          var data = new APerson{ FirstName = "Dima", LastName="Sokolov", Age = 16, DOB = DateTime.Now};
          serializeBenchmark("PersonClass", data);
        }

        [TestCase]
        public void Benchmark_Serialize_DataObjectClass()
        {
          var data = new DataObject{ fBool = true, fBoolNullable = null, fByteArray = new byte[128], fDateTime = DateTime.Now, fDouble = 122.03,
                                     fFloat = 12f, fString = "my message", fStringArray = new string[128], fDecimal = 121.12m, fULong= 2312 };
          serializeBenchmark("DataObjectClass", data);
        }

        [TestCase]
        public void Benchmark_Serialize_ListPrimitive()
        {
          var data = new List<object>{ 1, true, 12.34, "yes!", DateTime.Now, 'a'};
          serializeBenchmark("ListPrimitive", data);
        }

        [TestCase]
        public void Benchmark_Serialize_DictionaryPrimitive()
        {
          var data = new Dictionary<string, object>{ {"a",1} , {"b",true}, {"c", 12.34}, {"Message","yes!"}, {"when",DateTime.Now}, {"status",'a'}};
          serializeBenchmark("DictionaryPrimitive", data);
        }
        

        [TestCase]
        public void Benchmark_Serialize_ListObjects()
        {
          var p1 = new APerson{ FirstName = "Dima", LastName="Sokolov", Age = 16, DOB = DateTime.Now};
          var p2 = new APerson{ FirstName = "Fima", LastName="Orloff", Age = 23, DOB = DateTime.Now};
          var p3 = new APerson{ FirstName = "Dodik", LastName="Stevenson", Age = 99, DOB = DateTime.Now};
          var data = new List<object>{ 1, true, p1, 12.34, p2, "yes!", p3, DateTime.Now, 'a'};
          serializeBenchmark("ListObjects", data);
        }














            private void serializeBenchmark(string name, object data)
            {
                  var sw = Stopwatch.StartNew();

                  for (var i=0; i<BENCHMARK_ITERATIONS;i++)
                  {
                    //Console.WriteLine( JW.Write(data) );
                    JW.Write(data);
                  }

                  var nfxTime =  sw.ElapsedMilliseconds;

                  //------ JavaScriptSerialier

                  var ser = new JavaScriptSerializer();
                  sw = Stopwatch.StartNew();
              
                  for (var i=0; i<BENCHMARK_ITERATIONS;i++)
                  {
                    //Console.WriteLine( ser.Serialize(data) );
                    ser.Serialize(data);
                  }

                  var jssTime =  sw.ElapsedMilliseconds;

                  //---DataContractJsonSerializer
                  var dcs = new DataContractJsonSerializer( data.GetType());

                  sw = Stopwatch.StartNew();
                  var ms = new MemoryStream();
                  var dcsTime =  long.MaxValue;
                  try
                  {
                      for (var i=0; i<BENCHMARK_ITERATIONS;i++)
                      {
                         dcs.WriteObject( ms, data);
                    //     Console.WriteLine( Encoding.Default.GetString(ms.ToArray()));
                    //     throw new Exception();

                         ms.Position = 0;
                      }
                      dcsTime =  sw.ElapsedMilliseconds;
                  }
                  catch(Exception error)
                  {
                    Console.WriteLine("DataContractJSONSerializer does not support this test case: " + error.ToMessageWithType()); 
                  }


              var _nfx = BENCHMARK_ITERATIONS / (nfxTime /1000d);
              var _jss =  BENCHMARK_ITERATIONS / (jssTime /1000d);
              var _dcs =  BENCHMARK_ITERATIONS / (dcsTime /1000d);
              Console.WriteLine(
@"Serialize.{0}
    NFX: {1} op/sec 
    MS JSser: {2} op/sec
    MS DataContractSer: {3} op/sec
    Ratio NFX/JS: {4}
    Ratio NFX/DC: {5} 
     ".Args(name, _nfx, _jss, _dcs,   _nfx / _jss, _nfx / _dcs));

            }


    }
}
