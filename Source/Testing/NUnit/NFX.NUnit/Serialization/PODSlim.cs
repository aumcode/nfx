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
using System.IO;

using NUnit.Framework;

using NFX.Serialization.Slim;

namespace NFX.NUnit.Serialization
{
    //public class Suka
    //{
    //  public int Age;
    //  public string Name;
    //  public DateTime DOB;
    //}


    [TestFixture]
    public class PODSlim
    {
      //[TestCase]
      //public void TempTest()  //DKh: dont refactor - this is very temporary test for me locally
      //{
      //  using (var ms = new FileStream(@"c:\nfx\TEMP_1.POD", FileMode.Create))
      //  {
      //    var s = new SlimSerializer();


      //    var data = new Suka() { Age=15, Name="Pidaras", DOB = DateTime.Now};
      //    s.Serialize(ms, data);
      //  }
      //}

      

        [TestCase]
        public void ComplexObjectWithArrays_1() 
        {
          using(var ms = new MemoryStream())
          {           
            var s = new PODSlimSerializer();
            
            var data = new DataObject();
            data.Populate();
            s.Serialize(ms, data);

            ms.Seek(0, SeekOrigin.Begin);

            var result = s.Deserialize(ms);

            Assert.IsTrue( data.Equals( result));

          }
        }

        [TestCase]
        public void RootCompositeWriteRead_BusinessFamily()  
        {
            using(var ms = new MemoryStream())//new FileStream(@"c:\nfx\TEMP.POD", FileMode.Create))// new MemoryStream())
            {           
                var s = new PODSlimSerializer();  

                var originalData = 
                            new TestBusinessFamily{
                                Husband = new TestPerson{ Name = "Kolyan Zver'", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.5489},
                                Wife = new TestPerson{ Name = "Feiga Pozman", DOB = DateTime.Now, Assets=578, IsRegistered=false, Luck=250.489},
                                Kid = new TestPerson{ Name = "Mykola Zver'", DOB = DateTime.Now, Assets=12, IsRegistered=true, Luck=350.189},
                                Assets = 9000000000,
                                IsCertified = true
                            };

                    s.Serialize(ms, originalData);

                    ms.Seek(0, SeekOrigin.Begin);

                    var convertedData = s.Deserialize(ms);
                                
                Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

                Assert.IsTrue (originalData.Equals( convertedData ) );
            }
        }


        [TestCase]
        public void RootCompositeWriteRead_PersonList()  
        {
            using(var ms = new MemoryStream())
            {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new List<TestPerson>
                                    {
                                        new TestPerson{ Name = "Kolyan", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.89},
                                        new TestPerson{ Name = "Zmeyan", DOB = DateTime.Now.AddYears(-25), Assets=50000000, IsRegistered=false, Luck=283.4},

                                    };
                s.Serialize(ms, originalData);
                                
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = (List<TestPerson>)s.Deserialize(ms);
                                
                Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );   

                Assert.AreEqual(originalData.Count, convertedData.Count);

                Assert.IsTrue (originalData[0].Equals( convertedData[0] ) );

                Assert.IsTrue (originalData[1].Equals( convertedData[1] ) );
            }
        }


        [TestCase]
        public void RootCompositeWriteRead_tuple()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                    var originalData = new Tuple<int, string>(5,"yes");

                    s.Serialize(ms, originalData);
                                
                    ms.Seek(0, SeekOrigin.Begin);
                                
                    var convertedData = (Tuple<int, string>)s.Deserialize(ms);
                    
                    Assert.IsFalse( object.ReferenceEquals(originalData, convertedData) );
                                
                    Assert.AreEqual(5, convertedData.Item1);
                    Assert.AreEqual("yes", convertedData.Item2);
           }
        }


        [TestCase]
        public void RootSimpleWriteRead_string()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = "hello Dolly!";

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }


        [TestCase]
        public void RootSimpleWriteRead_bool()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = true;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootSimpleWriteRead_decimal()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = 125000m;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootSimpleWriteRead_nullabledecimal_1()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                decimal? originalData = 125000m;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootSimpleWriteRead_nullabledecimal_2()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                decimal? originalData = null;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootSimpleWriteRead_nullabledatetime_1()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                DateTime? originalData = DateTime.Now;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootSimpleWriteRead_nullabledatetime_2()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                DateTime? originalData = null;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }


        [TestCase]
        public void RootSimpleWriteRead_nullabletimespan_1()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                TimeSpan? originalData = TimeSpan.FromHours(12.57);

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootSimpleWriteRead_nullabletimespan_2()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                TimeSpan? originalData = null;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);
                Assert.AreEqual(originalData, convertedData);
           }
        }



        [TestCase]
        public void RootArrayWriteRead_1D()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new int[100];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);  
                Assert.IsFalse(  object.ReferenceEquals(originalData, convertedData) );
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootArrayWriteRead_2D()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new int[100,15];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);  
                Assert.IsFalse(  object.ReferenceEquals(originalData, convertedData) );
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootArrayWriteRead_3D()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new int[22,15,4];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);  
                Assert.IsFalse(  object.ReferenceEquals(originalData, convertedData) );
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootArrayWriteRead_4D()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new int[3,5,2,8];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);  
                Assert.IsFalse(  object.ReferenceEquals(originalData, convertedData) );
                Assert.AreEqual(originalData, convertedData);
           }
        }



        [TestCase]
        public void RootArrayWriteRead_5D()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new int[10,4,6,2,2];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);  
                Assert.IsFalse(  object.ReferenceEquals(originalData, convertedData) );
                Assert.AreEqual(originalData, convertedData);
           }
        }


        [TestCase]
        public void RootArrayWriteRead_1D_datetime()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new DateTime[100];
                var sd = DateTime.UtcNow;
                for(var i=0; i<originalData.Length; i++)
                {
                  originalData[i] = sd;
                  sd = sd.AddHours(i+(i*0.01));
                }

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);  
                Assert.IsFalse(  object.ReferenceEquals(originalData, convertedData) );
                Assert.AreEqual(originalData, convertedData);
           }
        }

        [TestCase]
        public void RootArrayWriteRead_1D_nullabledatetime()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer();  
                    
                var originalData = new DateTime?[100];
                var sd = DateTime.UtcNow;
                for(var i=0; i<originalData.Length; i++)
                    if ((i%2)==0)
                    {
                        originalData[i] = sd;
                        sd = sd.AddHours(i+(i*0.01));
                    }

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);
                                
                var convertedData = s.Deserialize(ms);  
                Assert.IsFalse(  object.ReferenceEquals(originalData, convertedData) );
                Assert.AreEqual(originalData, convertedData);
           }
        }


        [TestCase]
        public void DeserializationTransform1()  
        {
           using(var ms = new MemoryStream())
           {           
                var s = new PODSlimSerializer(); 
          
                var originalData = new PODTest_Ver1
                {
                    Name = "Xerson Person", 
                    Description = "Some description",
                    Age = 25  
                };
            
   
                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms, new PODTestVersionUpgradeStrategy());
            
                Assert.IsTrue( convertedData is PODTest_Ver2);
   
                var ver2 = convertedData as PODTest_Ver2;

                Assert.AreEqual( originalData.Name, ver2.Name);
                Assert.AreEqual( originalData.Description, ver2.Description);
                Assert.AreEqual( originalData.Age, ver2.AgeAsOfToday);
                Assert.AreEqual( DateTime.Now.AddYears(-originalData.Age).Year, ver2.DOB.Year);
           }
        }


    }
}
