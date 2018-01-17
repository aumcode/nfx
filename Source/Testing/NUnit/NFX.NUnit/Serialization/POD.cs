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
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

using NFX.Serialization.POD;
using System.Collections.Concurrent;

namespace NFX.NUnit.Serialization
{  

 #pragma warning disable 659

    [TestFixture]
    public class POD
    {
        [TestCase]
        public void RootPrimitive1_int()  
        {
            var data = 5;

            var doc = new PortableObjectDocument(data);
                                
            Assert.AreEqual(typeof(MetaPrimitiveType), doc.RootMetaType.GetType());
            Assert.AreEqual(5, doc.Root);
        }

        [TestCase]
        public void RootPrimitive2_bool()  
        {
            var data = true;

            var doc = new PortableObjectDocument(data);
                                
            Assert.AreEqual(typeof(MetaPrimitiveType), doc.RootMetaType.GetType());
            Assert.AreEqual(true, doc.Root);
        }

        [TestCase]
        public void RootPrimitive3_string()  
        {
            var data = "test string";

            var doc = new PortableObjectDocument(data);
                                
            Assert.AreEqual(typeof(MetaPrimitiveType), doc.RootMetaType.GetType());
            Assert.AreEqual("test string", doc.Root);
        }

        [TestCase]
        public void RootComposite1()  
        {
            var data = new Tuple<int, string>(5,"yes");

            var doc = new PortableObjectDocument(data);
                                
            Assert.AreEqual(typeof(MetaComplexType), doc.RootMetaType.GetType());
            Assert.AreEqual(typeof(CompositeReflectedData), doc.Root.GetType());
            
            var crd = (CompositeReflectedData)doc.Root;

            Assert.AreEqual(2, crd.FieldData.Length);
            Assert.AreEqual(5, crd.FieldData[0]);
            Assert.AreEqual("yes", crd.FieldData[1]);
        }


        [TestCase]
        public void RootPrimitiveWriteRead_int()  
        {
            var originalData = 5;

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }

        [TestCase]
        public void RootPrimitiveWriteRead_int_nullable1()  
        {
            int? originalData = 5;

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }

        [TestCase]
        public void RootPrimitiveWriteRead_int_nullable2()  
        {
            int? originalData = null;

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }

        [TestCase]
        public void RootPrimitiveWriteRead_bool()  
        {
            var originalData = true;

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }


        [TestCase]
        public void RootPrimitiveWriteRead_string()  
        {
            var originalData = "hello testing";

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }

        [TestCase]
        public void RootPrimitiveWriteRead_string_null()  
        {
            string originalData = null;

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }

        [TestCase]
        public void RootPrimitiveWriteRead_double()  
        {
            var originalData = 10 / 3.01d;

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }

        [TestCase]
        public void RootPrimitiveWriteRead_datetime()  
        {
            var originalData = DateTime.Now;

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }

        [TestCase]
        public void RootPrimitiveWriteRead_timespan()  
        {
            var originalData = TimeSpan.FromDays(12.4321);

            var doc = new PortableObjectDocument(originalData);
            
            var convertedData = doc.ToOriginalObject();
                                
            Assert.AreEqual(originalData, convertedData);
        }


                [TestCase]
                public void RootCompositeWriteRead_tuple()  
                {
                    var originalData = new Tuple<int, string>(5,"yes");

                    var doc = new PortableObjectDocument(originalData);
                                
                    var convertedData = doc.ToOriginalObject() as Tuple<int, string>;
                    
                    Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );
                                
                    Assert.AreEqual(5, convertedData.Item1);
                    Assert.AreEqual("yes", convertedData.Item2);
                }

                [TestCase]
                public void RootCompositeWriteRead_Person()  
                {
                    var originalData = new TestPerson{ Name = "Kolyan", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.89};

                    var doc = new PortableObjectDocument(originalData);
                                
                    var convertedData = doc.ToOriginalObject() as TestPerson;
                                
                    Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

                    Assert.IsTrue (originalData.Equals( convertedData ) );
                }


                [TestCase]
                public void RootCompositeWriteRead_Family()  
                {
                    var originalData = 
                             new TestFamily{
                                  Husband = new TestPerson{ Name = "Kolyan", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.5489},
                                  Wife = new TestPerson{ Name = "Feiga", DOB = DateTime.Now, Assets=578, IsRegistered=false, Luck=250.489},
                                  Kid = new TestPerson{ Name = "Yasha", DOB = DateTime.Now, Assets=12, IsRegistered=true, Luck=350.189},
                             };


                    var doc = new PortableObjectDocument(originalData);
                                
                    var convertedData = doc.ToOriginalObject() as TestFamily;
                                
                    Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

                    Assert.IsTrue (originalData.Equals( convertedData ) );
                }


                [TestCase]
                public void RootCompositeWriteRead_BusinessFamily()  
                {
                    var originalData = 
                             new TestBusinessFamily{
                                  Husband = new TestPerson{ Name = "Kolyan Zver'", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.5489},
                                  Wife = new TestPerson{ Name = "Feiga Pozman", DOB = DateTime.Now, Assets=578, IsRegistered=false, Luck=250.489},
                                  Kid = new TestPerson{ Name = "Mykola Zver'", DOB = DateTime.Now, Assets=12, IsRegistered=true, Luck=350.189},
                                  Assets = 9000000000,
                                  IsCertified = true
                             };


                    var doc = new PortableObjectDocument(originalData);
                                
                    var convertedData = doc.ToOriginalObject() as TestFamily;
                                
                    Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

                    Assert.IsTrue (originalData.Equals( convertedData ) );
                }


                [TestCase]
                public void RootCompositeWriteRead_PersonList()  
                {
                    var originalData = new List<TestPerson>
                                        {
                                            new TestPerson{ Name = "Kolyan", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.89},
                                            new TestPerson{ Name = "Zmeyan", DOB = DateTime.Now.AddYears(-25), Assets=50000000, IsRegistered=false, Luck=283.4},

                                        };
                    var doc = new PortableObjectDocument(originalData);
                                
                    var convertedData = doc.ToOriginalObject() as List<TestPerson>;
                                
                    Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

                    Assert.AreEqual(originalData.Count, convertedData.Count);

                    Assert.IsTrue (originalData[0].Equals( convertedData[0] ) );

                    Assert.IsTrue (originalData[1].Equals( convertedData[1] ) );
                }


        [TestCase]
        public void RootDictionary()  
        {
            var originalData = new Dictionary<string, int>
                                {
                                    {"x",10},
                                    {"y",-20}
                                };
            var doc = new PortableObjectDocument(originalData);
   
            var convertedData = doc.ToOriginalObject() as Dictionary<string, int>;
   
            Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

            Assert.AreEqual(originalData.Count, convertedData.Count);
            Assert.AreEqual(10, convertedData["x"]);
            Assert.AreEqual(-20, convertedData["y"]);
            
        }

        [TestCase]
        public void RootConcurrentDictionary()  
        {
            var originalData = new ConcurrentDictionary<string, int>();

            originalData["x"] = 10;
            originalData["y"] = -20;
            
            var doc = new PortableObjectDocument(originalData);
   
            var convertedData = doc.ToOriginalObject() as ConcurrentDictionary<string, int>;
   
            Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

            Assert.AreEqual(originalData.Count, convertedData.Count);
            Assert.AreEqual(10, convertedData["x"]);
            Assert.AreEqual(-20, convertedData["y"]);
            
        }


        [TestCase]
        public void DeserializationTransform1()  
        {
            var originalData = new PODTest_Ver1
            {
                Name = "Xerson Person", 
                Description = "Some description",
                Age = 25  
            };
            
            var doc = new PortableObjectDocument(originalData);
   
            var convertedData = doc.ToOriginalObject(new PODTestVersionUpgradeStrategy());
            
            Assert.IsTrue( convertedData is PODTest_Ver2);
   
            var ver2 = convertedData as PODTest_Ver2;

            Assert.AreEqual( originalData.Name, ver2.Name);
            Assert.AreEqual( originalData.Description, ver2.Description);
            Assert.AreEqual( originalData.Age, ver2.AgeAsOfToday);
            Assert.AreEqual( DateTime.Now.AddYears(-originalData.Age).Year, ver2.DOB.Year);
        }



    }

}
