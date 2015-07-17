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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


using NFX.DataAccess.CRUD;
using NFX.Serialization.Slim;
using NFX.Serialization.JSON;




namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class Serialization
    {
       
        [TestCase]
        public void Slim_SerializeTable_TypedRows()
        {
            var tbl = new Table(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);
            Console.WriteLine("{0} rows took {1} bytes".Args(tbl.Count, ms.Position));
                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Assert.IsNotNull( tbl2 );
                Assert.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Assert.AreEqual( 1000, tbl2.Count);

                Assert.IsTrue( tbl.SequenceEqual( tbl2 ) );
            }
        }

       
        [TestCase]
        public void Slim_SerializeTable_DynamicRows()
        {
            var tbl = new Table(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
            {
                var row = new DynamicRow( tbl.Schema );

                row["ID"] = "POP{0}".Args(i);
                row["FirstName"] = "Oleg";
                row["LastName"] = "Popov-{0}".Args(i);
                row["DOB"] = new DateTime(1953, 12, 10);
                row["YearsInSpace"] = 12;
            
                tbl.Insert( row );
            }

            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);
            Console.WriteLine("{0} rows took {1} bytes".Args(tbl.Count, ms.Position));
                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Assert.IsNotNull( tbl2 );
                Assert.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Assert.AreEqual( 1000, tbl2.Count);
                Assert.IsTrue( tbl.SequenceEqual( tbl2 ) );
            }
        }

        [TestCase]
        public void Slim_SerializeTable_ComplexTypedRows()
        {
            var tbl = new Table(Schema.GetForTypedRow(typeof(PersonWithNesting)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new PersonWithNesting{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2] 
                                   });
            
            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);

               
         Console.WriteLine("{0} rows took {1} bytes".Args(tbl.Count, ms.Position));
               
                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Assert.IsNotNull( tbl2 );
                Assert.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Assert.AreEqual( 1000, tbl2.Count);

                Assert.IsTrue( tbl.SequenceEqual( tbl2 ) );
            }
        }

        [TestCase]
        public void Slim_SerializeRow_ComplexTypedRow()
        {
            var row1 =  new PersonWithNesting{
                                    ID = "A1",
                                    FirstName = "Joseph",
                                    LastName = "Mc'Cloud",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2] 
                                   };
            
            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, row1);

               
         Console.WriteLine("1 row took {0} bytes".Args(ms.Position));
               
                ms.Position = 0;

                var row2 = ser.Deserialize(ms) as PersonWithNesting;

                Assert.IsNotNull( row2 );
                Assert.IsFalse( object.ReferenceEquals(row1 , row2) );

                Assert.AreEqual("A1", row2.ID);
                Assert.AreEqual("Joseph",  row2.FirstName);
                Assert.AreEqual("Mc'Cloud",row2.LastName);
                Assert.AreEqual("111", row2.LatestHistory.ID);
                Assert.AreEqual(2, row2.History1.Count);
                Assert.AreEqual("234234", row2.History1[1].ID);
            }
        }



        [TestCase] 
        public void JSON_SerializeRowset_TypedRows()
        {
            var rowset = new Rowset(Schema.GetForTypedRow(typeof(Person)));
                                        
            for(var i=0; i<10; i++)
             rowset.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var json = rowset.ToJSON( NFX.Serialization.JSON.JSONWritingOptions.PrettyPrint);// );
           
            Console.WriteLine( json);

            var rowset2 = json.JSONToDynamic();

            Assert.AreEqual("Popov-1", rowset2.Rows[1][2]);

        }

        [TestCase] 
        public void JSON_SerializeRowset_ComplexTypedRows_Array()
        {
            var rowset = new Rowset(Schema.GetForTypedRow(typeof(PersonWithNesting)));
                                        
            for(var i=0; i<10; i++)
             rowset.Insert( new PersonWithNesting{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2] 
                                   });
            
            var json = rowset.ToJSON( NFX.Serialization.JSON.JSONWritingOptions.PrettyPrint);// );
           
            Console.WriteLine( json);

            var rowset2 = json.JSONToDynamic();

            Assert.AreEqual("Popov-1", rowset2.Rows[1][2]);

        }


        [TestCase] 
        public void JSON_SerializeRowset_ComplexTypedRows_Map()
        {
            var rowset = new Rowset(Schema.GetForTypedRow(typeof(PersonWithNesting)));
                                        
            for(var i=0; i<10; i++)
             rowset.Insert( new PersonWithNesting{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2] 
                                   });
            
            var json = rowset.ToJSON( NFX.Serialization.JSON.JSONWritingOptions.PrettyPrintRowsAsMap);// );
           
            Console.WriteLine( json);

            var rowset2 = json.JSONToDynamic();

            Assert.AreEqual("Popov-1", rowset2.Rows[1].LastName);
            Assert.AreEqual("789211", rowset2.Rows[1].History1[0].ID);
        }


        [TestCase] 
        public void JSON_SerializeRow_ComplexTypedRow_Map()
        {
            var row1 =  new PersonWithNesting{
                                    ID = "A1",
                                    FirstName = "Joseph",
                                    LastName = "Mc'Cloud",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2] 
                                   };
            
            var json = row1.ToJSON( NFX.Serialization.JSON.JSONWritingOptions.PrettyPrintRowsAsMap);//AS MAP!!!!
           
            Console.WriteLine(json);

            var row2 = json.JSONToDynamic();

            Assert.AreEqual("A1",      row2.ID);
            Assert.AreEqual("Joseph",  row2.FirstName);
            Assert.AreEqual("Mc'Cloud",row2.LastName);
            Assert.AreEqual("111",     row2.LatestHistory.ID);
            Assert.AreEqual(2,         row2.History1.Count);
            Assert.AreEqual("234234",  row2.History1[1].ID);
        }

        [TestCase] 
        public void JSON_SerializeRow_ComplexTypedRow_Array()
        {
            var row1 =  new PersonWithNesting{
                                    ID = "A1",
                                    FirstName = "Joseph",
                                    LastName = "Mc'Cloud",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2] 
                                   };
            
            var json = row1.ToJSON( NFX.Serialization.JSON.JSONWritingOptions.PrettyPrint);//AS ARRAY
           
            Console.WriteLine(json);

            var row2 = json.JSONToDynamic();

            Assert.AreEqual("A1",      row2[row1.Schema["ID"].Order]);
            Assert.AreEqual("Joseph",  row2[row1.Schema["FirstName"].Order]);
            Assert.AreEqual("Mc'Cloud",row2[row1.Schema["LastName"].Order]);
            Assert.AreEqual("111",     row2[row1.Schema["LatestHistory"].Order][0]);
            Assert.AreEqual(2,         row2[row1.Schema["History1"].Order].Count);
            Assert.AreEqual("234234",  row2[row1.Schema["History1"].Order][1][0]);
        }



    }
}
