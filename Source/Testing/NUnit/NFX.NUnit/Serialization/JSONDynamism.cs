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


using NUnit.Framework;

using NFX.DataAccess.CRUD;
using NFX.Serialization.JSON;



namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class JSONDynamism
    { 
        [TestCase]
        public void Map_GetSet_ByMember()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 7;
            Assert.AreEqual(7, obj.A);
        }

        [TestCase]
        public void Map_GetSet_ByIndexer()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Map);

            obj["A"] = 7;
            Assert.AreEqual(7, obj["A"]);
        }

        [TestCase]
        public void Map_GetSet_IntAdd()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 3;
            obj.B = 5;
            obj.C = obj.A + obj.B;

            Assert.AreEqual(8, obj.C);
        }


        [TestCase]
        public void Map_GetSet_DoubleAddInt()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 3.4d;
            obj.B = 5;
            obj.C = obj.A + obj.B;

            Assert.AreEqual(8.4d, obj.C);
        }

        [TestCase]
        public void Map_GetSet_DateTimeAddDays()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Map);

            obj.StartDate = new DateTime(1980, 12, 05);
            obj.Interval = 5;
            obj.EndDate = obj.StartDate.AddDays(obj.Interval);

            Assert.AreEqual(new DateTime(1980, 12, 10), obj.EndDate);
        }


        [TestCase]
        public void Map_GetMemberNames()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 3;
            obj.B = 5;
            obj.C = obj.A + obj.B;

            IEnumerable<string> nms = obj.GetDynamicMemberNames();
            var names = nms.ToList();
            Assert.AreEqual(3, names.Count);
            Assert.AreEqual("A", names[0]);
            Assert.AreEqual("B", names[1]);
            Assert.AreEqual("C", names[2]);
        }


        [TestCase]
        public void Array_Autogrow_1()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Array);

            Assert.AreEqual(0, obj.Count);
            Assert.AreEqual(0, obj.Length);

            obj[0] = 100;
            obj[1] = 120;

            Assert.AreEqual(2, obj.Count);
            Assert.AreEqual(100, obj[0]);
            Assert.AreEqual(120, obj[1]);
         
        }

        [TestCase]
        public void Array_Autogrow_2()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Array);

            Assert.AreEqual(0, obj.Count);
            Assert.AreEqual(0, obj.Length);

            obj[150] = 100;

            Assert.AreEqual(151, obj.Count);
            Assert.AreEqual(100, obj[150]);
         
        }


        [TestCase]
        public void Array_GetBeyondRange()
        {
            dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Array);

            Assert.AreEqual(0, obj.Count);
            Assert.AreEqual(0, obj.Length);

            obj[0] = 100;
            obj[1] = 120;

            Assert.AreEqual(2, obj.Count);
            Assert.AreEqual(100, obj[0]);
            Assert.AreEqual(120, obj[1]);

            Assert.IsNull(obj[10001]);
         
        }

        [TestCase]
        public void WithSubDocumentsAndConversionAccessors()
        {
          // http://stackoverflow.com/questions/2954531/lots-of-first-chance-microsoft-csharp-runtimebinderexceptions-thrown-when-dealin
          dynamic obj = new JSONDynamicObject(JSONDynamicObjectKind.Map);
          obj.type = "abc";
          obj.startDate = "5/15/2001 6:00pm";
          obj.target = new JSONDynamicObject(JSONDynamicObjectKind.Map);
          obj.target.id = "A678";
          obj.target.image = "hello";
          obj.target.type = "good";
          obj.target.description = "Thank You";
          obj.target.Age = 123;
          obj.target.Salary = 125000m;


          string s1 = obj.ToJSON();

            var ro1 = s1.JSONToDynamic();
            Assert.AreEqual("abc", ro1.type);
            Assert.AreEqual(new DateTime(2001,5,15,18,00,00), ((string)ro1.startDate).AsDateTime());
            Assert.AreEqual("A678", ro1.target.id);
            Assert.AreEqual("hello", ro1.target.image);
            Assert.AreEqual("good", ro1.target.type);
            Assert.AreEqual("Thank You", ro1.target.description);

            Assert.AreEqual(123, ro1.target.Age);

            Assert.AreEqual(125000, ro1.target.Salary);


          string s2 = ((object)obj).ToJSON();

            var ro2 = s2.JSONToDynamic();
            Assert.AreEqual("abc", ro2.type);
            Assert.AreEqual(new DateTime(2001,5,15,18,00,00), ((string)ro2.startDate).AsDateTime());
            Assert.AreEqual("A678", ro2.target.id);
            Assert.AreEqual("hello", ro2.target.image);
            Assert.AreEqual("good", ro2.target.type);
            Assert.AreEqual("Thank You", ro2.target.description);
         
        }

        [TestCase]
        public void ToTypedRow_FromString()
        {
            var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000}";

            MySimpleData row = str.JSONToDynamic();

            Assert.AreEqual("Orlov", row.Name);
            Assert.AreEqual(new DateTime(2007, 2,12,18,45,0), row.DOB);
            Assert.AreEqual(true, row.Certified);
            Assert.AreEqual(12, row.ServiceYears);
            Assert.AreEqual(145000m, row.Salary);
        }

        [TestCase]
        public void ToAmorphousTypedRow_FromString()
        {
            var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000, extra: -1, yes: true}";

            MySimpleAmorphousData row = str.JSONToDynamic();

            Assert.AreEqual("Orlov", row.Name);
            Assert.AreEqual(new DateTime(2007, 2,12,18,45,0), row.DOB);
            Assert.AreEqual(true, row.Certified);
            Assert.AreEqual(12, row.ServiceYears);
            Assert.AreEqual(145000m, row.Salary);
            Assert.AreEqual(2, row.AmorphousData.Count);
            Assert.AreEqual(-1, row.AmorphousData["extra"]);
            Assert.AreEqual(true, row.AmorphousData["yes"]);
        }

        [TestCase]
        public void ToDynamicRow_FromString()
        {
          var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000}";

          var row = new DynamicRow(Schema.GetForTypedRow(typeof(MySimpleData)));

          JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

          Assert.AreEqual("Orlov", row["Name"]);
          Assert.AreEqual(new DateTime(2007, 2, 12, 18, 45, 0), row["DOB"]);
          Assert.AreEqual(true, row["Certified"]);
          Assert.AreEqual(12, row["ServiceYears"]);
          Assert.AreEqual(145000m, row["Salary"]);
        }

        [TestCase]
        public void ToAmorphousDynamicRow_FromString()
        {
          var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000, extra: -1, yes: true}";

          var row = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(MySimpleData)));

          JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

          Assert.AreEqual("Orlov", row["Name"]);
          Assert.AreEqual(new DateTime(2007, 2, 12, 18, 45, 0), row["DOB"]);
          Assert.AreEqual(true, row["Certified"]);
          Assert.AreEqual(12, row["ServiceYears"]);
          Assert.AreEqual(145000m, row["Salary"]);
          Assert.AreEqual(2, row.AmorphousData.Count);
          Assert.AreEqual(-1, row.AmorphousData["extra"]);
          Assert.AreEqual(true, row.AmorphousData["yes"]);
        }

                    [TestCase]
                    public void ToAmorphousTypedRow_FromString_PerfParallel()
                    {
                        const int CNT = 500000;

                        var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000, extra: -1, yes: true}";

                        var tmr = System.Diagnostics.Stopwatch.StartNew();
                        System.Threading.Tasks.Parallel.For(0, CNT,
                          (i) =>
                          {
                             MySimpleAmorphousData row = str.JSONToDynamic();

                             Assert.AreEqual("Orlov", row.Name);
                             Assert.AreEqual(new DateTime(2007, 2,12,18,45,0), row.DOB);
                             Assert.AreEqual(true, row.Certified);
                             Assert.AreEqual(12, row.ServiceYears);
                             Assert.AreEqual(145000m, row.Salary);
                             Assert.AreEqual(2, row.AmorphousData.Count);
                             Assert.AreEqual(-1, row.AmorphousData["extra"]);
                             Assert.AreEqual(true, row.AmorphousData["yes"]);
                         }
                       );

                       var elp = tmr.ElapsedMilliseconds;
                       Console.WriteLine("Total: {0} in {1} ms = {2} ops/sec".Args(CNT, elp, CNT/(elp/1000d)));


                    }


        [TestCase]
        public void ToTypedRowWithGDID_FromString()
        {
            var str = @"{ id: ""12:4:5"", name: ""Orlov""}";

            DataWithGDID row = str.JSONToDynamic();

            Assert.AreEqual(new NFX.DataAccess.Distributed.GDID(12, 4, 5), row.ID);
            Assert.AreEqual("Orlov", row.Name);
        }

        [TestCase]
        public void ToTypedRowWithGDID_FromOtherRow()
        {
            var row1 = new DataWithGDID
            {
              ID = new NFX.DataAccess.Distributed.GDID(1,189),
              Name = "Graf Orlov"
            };

            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            DataWithGDID row2 = str.JSONToDynamic();

            Assert.AreEqual(new NFX.DataAccess.Distributed.GDID(1, 189), row2.ID);
            Assert.AreEqual("Graf Orlov", row2.Name);
        }

        [TestCase]
        public void ToTypedRow_FromOtherRow()
        {
            var row1 = new MySimpleData
            {
              Name = "Graf Orlov", 
              DOB = new DateTime(1980,12,11,19,23,11),
              Certified = true,
              ServiceYears = 37,
              Salary = 123455.8712m
            };
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MySimpleData row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.Equals(row2));
        }

        [TestCase]
        public void ToAmorphousTypedRow_FromOtherRow()
        {
            var row1 = new MySimpleAmorphousData
            {
              Name = "Graf Orlov", 
              DOB = new DateTime(1980,12,11,19,23,11),
              Certified = true,
              ServiceYears = 37,
              Salary = 123455.8712m
            };
            row1.AmorphousData["frage"] = "Was machst du mit dem schwert?";
            row1.AmorphousData["antwort"] = "Ich kämpfe damit";
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MySimpleAmorphousData row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.Equals(row2));

            Assert.AreEqual("Was machst du mit dem schwert?", row2.AmorphousData["frage"]);
            Assert.AreEqual("Ich kämpfe damit", row2.AmorphousData["antwort"]);
        }

        [TestCase]
        public void ToTypedRow_MyComplexData()
        {
            var row1 = new MyComplexData
            { ID = 12345,
              D1 = new MySimpleData
              {
                Name = "Graf Orlov", 
                DOB = new DateTime(1980,12,11,19,23,11),
                Certified = true,
                ServiceYears = 37,
                Salary = 123455.8712m
              },
              D2 = new MySimpleData
              {
                Name = "Oleg Popov", 
                DOB = new DateTime(1981,11,01,14,08,19),
                Certified = true,
                ServiceYears = 37,
                Salary = 123455.8712m
              }
            };
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexData row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.Equals(row2));
        }

        [TestCase]
        public void ToTypedRow_MyComplexData_Null()
        {
            var row1 = new MyComplexData
            { ID = 12345,
              D1 = new MySimpleData
              {
                Name = "Graf Orlov", 
                DOB = new DateTime(1980,12,11,19,23,11),
                Certified = true,
                ServiceYears = 37,
                Salary = 123455.8712m
              },
              D2 = null
            };
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexData row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.D1.Equals(row2.D1));
            Assert.IsNull(row2.D2);
        }


        [TestCase]
        public void ToTypedRow_MyComplexDataWithArray()
        {
            var row1 = new MyComplexDataWithArray
            { ID = 12345,
              Data = new MySimpleData[]{
                new MySimpleData
                {
                  Name = "Graf Orlov", 
                  DOB = new DateTime(1980,12,11,19,23,11),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                },
                new MySimpleData
                {
                  Name = "Oleg Popov", 
                  DOB = new DateTime(1981,11,01,14,08,19),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                }
              }
            };
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithArray row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.Equals(row2));
        }

        [TestCase]
        public void ToTypedRow_MyComplexDataWithList()
        {
            var row1 = new MyComplexDataWithList
            { ID = 12345,
              Data = new List<MySimpleData>{
                new MySimpleData
                {
                  Name = "Graf Orlov", 
                  DOB = new DateTime(1980,12,11,19,23,11),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                },
                new MySimpleData
                {
                  Name = "Oleg Popov", 
                  DOB = new DateTime(1981,11,01,14,08,19),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                }
              }
            };
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithList row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.Equals(row2));
        }




        [TestCase]
        public void ToTypedRow_MyComplexDataWithPrimitiveArray()
        {
            var row1 = new MyComplexDataWithPrimitiveArray
            { ID = 12345,
              Data = new int[]{ 1,7,12,3,8,9,0,2134,43,6,2,5}
            };
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithPrimitiveArray row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.Equals(row2));
        }


        [TestCase]
        public void ToTypedRow_MyComplexDataWithPrimitiveList()
        {
            var row1 = new MyComplexDataWithPrimitiveList
            { ID = 12345,
              Data = new List<int>{ 1,7,12,3,8,9,0,2134,43,6,2,5}
            };
            
            var str = row1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithPrimitiveList row2 = str.JSONToDynamic();

            Assert.IsTrue(row1.Equals(row2));
        }


        [TestCase]
        public void ToTypedRow_RowWithSUbDocuments_1()
        {
            var json ="{data: null, map: null}";

            RowWithSubDocuments row = json.JSONToDynamic();
            Assert.IsNull(row.Data);
            Assert.IsNull(row.Map);
        }

         [TestCase]
        public void ToTypedRow_RowWithSUbDocuments_2()
        {
            var json ="{data: {a: 1, b: 2}, map: null}";

            RowWithSubDocuments row = json.JSONToDynamic();

            Assert.AreEqual(1, ((JSONDataMap)row.Data)["a"]);
            Assert.AreEqual(2, ((JSONDataMap)row.Data)["b"]);
            Assert.IsNull(row.Map);
        }

         [TestCase]
        public void ToTypedRow_RowWithSUbDocuments_3()
        {
            var json ="{data: '{a: 1, b: 2}', map: null}";

            RowWithSubDocuments row = json.JSONToDynamic();

            Assert.AreEqual(1, ((JSONDataMap)row.Data)["a"]);
            Assert.AreEqual(2, ((JSONDataMap)row.Data)["b"]);
            Assert.IsNull(row.Map);
        }

         [TestCase]
        public void ToTypedRow_RowWithSUbDocuments_4()
        {
            var json ="{data: '{a: 1, b: 2}', map: {c: 3, d: 4}}";

            RowWithSubDocuments row = json.JSONToDynamic();

            Assert.AreEqual(3, row.Map["c"]);
            Assert.AreEqual(4, row.Map["d"]);
        }

         [TestCase]
        public void ToTypedRow_RowWithSUbDocuments_5()
        {
            var json ="{data: '{a: 1, b: 2}', map: '{c: 3, d: 4}'}";

            RowWithSubDocuments row = json.JSONToDynamic();

            Assert.AreEqual(3, row.Map["c"]);
            Assert.AreEqual(4, row.Map["d"]);
        }


                     private class DataWithGDID : TypedRow
                     {
                       [Field] public NFX.DataAccess.Distributed.GDID ID { get; set;}
                       [Field] public string Name { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as DataWithGDID;
                         if (b==null) return false;
                         return this.Name==b.Name &&
                                this.ID.Equals(b.ID);
                       }
                     }


                     private class MySimpleData : TypedRow
                     {
                       [Field] public string Name { get; set;}
                       [Field] public DateTime DOB { get; set;}
                       [Field] public bool Certified { get; set;}
                       [Field] public int ServiceYears { get; set;}
                       [Field] public decimal Salary { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as MySimpleData;
                         if (b==null) return false;
                         return this.Name==b.Name &&
                                this.DOB==b.DOB&&
                                this.Certified==b.Certified&&
                                this.ServiceYears==b.ServiceYears&&
                                this.Salary==b.Salary;
                       }
                     }

                     private class MySimpleAmorphousData : AmorphousTypedRow
                     {
                       [Field] public string Name { get; set;}
                       [Field] public DateTime DOB { get; set;}
                       [Field] public bool Certified { get; set;}
                       [Field] public int ServiceYears { get; set;}
                       [Field] public decimal Salary { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as MySimpleAmorphousData;
                         if (b==null) return false;
                         return this.Name==b.Name &&
                                this.DOB==b.DOB&&
                                this.Certified==b.Certified&&
                                this.ServiceYears==b.ServiceYears&&
                                this.Salary==b.Salary;
                       }
                     }



                     private class MyComplexData : TypedRow
                     {
                       [Field] public long ID { get; set;}
                       [Field] public MySimpleData D1 { get; set;}
                       [Field] public MySimpleData D2 { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as MyComplexData;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.D1.Equals(b.D1)&&
                                this.D2.Equals(b.D2);
                       }
                     }

                     private class MyComplexDataWithArray : TypedRow
                     {
                       [Field] public long ID { get; set;}
                       [Field] public MySimpleData[] Data { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as MyComplexDataWithArray;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }

                     private class MyComplexDataWithList : TypedRow
                     {
                       [Field] public long ID { get; set;}
                       [Field] public List<MySimpleData> Data { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as MyComplexDataWithList;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }

                     private class MyComplexDataWithPrimitiveArray : TypedRow
                     {
                       [Field] public long ID { get; set;}
                       [Field] public int[] Data { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as MyComplexDataWithPrimitiveArray;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }

                     private class MyComplexDataWithPrimitiveList : TypedRow
                     {
                       [Field] public long ID { get; set;}
                       [Field] public List<int> Data { get; set;}

                       public override bool Equals(Row other)
                       {
                         var b = other as MyComplexDataWithPrimitiveList;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }


                     private class RowWithSubDocuments : TypedRow
                     {
                       [Field] public IJSONDataObject Data { get; set;}
                       [Field] public JSONDataMap Map { get; set;}
                     }


                     private class RowWithBinaryData : TypedRow
                     {
                       [Field] public string FileName { get; set;}
                       [Field] public string ContentType { get; set;}
                       [Field] public byte[] Content { get; set;}
                     }

        [TestCase]
        public void ToFromRowWithBinaryData()
        {
            var r1 = new RowWithBinaryData
            {
              FileName = "SaloInstructions.txt",
              ContentType = "text/plain",
              Content = new byte[]{1,2,3,120}
            };

            var json =  r1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(json);

            RowWithBinaryData r2 = json.JSONToDynamic();

            Assert.AreEqual(r1.FileName, r2.FileName);
            Assert.AreEqual(r1.ContentType, r2.ContentType);
            Assert.IsTrue( r1.Content.SequenceEqual(r2.Content) );
        }


                     

    }
}
