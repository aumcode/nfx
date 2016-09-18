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


namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class Rowsets
    {
        [TestCase]
        public void Filtered_TypedRows()
        {
            var data = makeTypedRows(1000);
            
            var view =  new Rowset( data, row => (int)row["YearsWithCompany"] > 500 );
            
                                        
            Assert.AreEqual(499, view.Count);
        }


        [TestCase]
        public void SimpleFiltered_TypedRows()
        {
            var data = makeTypedRows(1000);
            
            var view =  new Rowset( data, row => row.SimpleFilterPredicate("*ov-22") );
            
                                        
            Assert.AreEqual(1, view.Count);
            Assert.AreEqual(22, view[0]["YearsWithCompany"]);
        }


        [TestCase]
        public void Sorted1columnDESC_TypedRows()
        {
            var data = makeTypedRows(1000);
            
            var view =  new Rowset( data, row => (int)row["YearsWithCompany"] > 500 );
            
                                        
            Assert.AreEqual(499, view.Count);

            view.SortDefinition = "-YearsWithCompany";

            Assert.AreEqual(999, view[0]["YearsWithCompany"]);
        }




                private Rowset makeTypedRows(int count)
                {
                  var rset = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
                  for(var i=0; i<count; i++)
                     rset.Insert( new Person{
                                            ID = "POP{0}".Args(i),
                                            FirstName = "Oleg",
                                            LastName = "Popov-{0}".Args(i),
                                            DOB = new DateTime(1953, 12, 10),
                                            YearsInSpace = 1000-i,
                                            YearsWithCompany = i 
                                           });
                  return rset;
                }



         [TestCase]
        public void PopulateAndFindKey_TypedRows()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
                                   
                                        
            Assert.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("POP35");
            Assert.IsNotNull( match1 );
            Assert.AreEqual("Popov-35", match1["LastName"]); //example of dynamic row access

            var match2 = tbl.FindByKey("POP36") as Person;
            Assert.IsNotNull( match2 );
            Assert.AreEqual("Popov-36", match2.LastName);//example of typed row access

            var match3 = tbl.FindByKey("DoesNotExist");
            Assert.IsNull( match3 );
        }

        [TestCase]
        public void PopulateAndCloneThenFindKey_TypedRows()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
                                   
            
            tbl = new Rowset(tbl);//make copy
                                        
            Assert.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("POP35");
            Assert.IsNotNull( match1 );
            Assert.AreEqual("Popov-35", match1["LastName"]); //example of dynamic row access

            var match2 = tbl.FindByKey("POP36") as Person;
            Assert.IsNotNull( match2 );
            Assert.AreEqual("Popov-36", match2.LastName);//example of typed row access

            var match3 = tbl.FindByKey("DoesNotExist");
            Assert.IsNull( match3 );
        }


        [TestCase]
        public void PopulateAndFindKey_MixedRows()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
            {
                 var row =  new DynamicRow(tbl.Schema);

                 row["ID"] = "DYN{0}".Args(i);
                 row["FirstName"] = "Oleg";
                 row["LastName"] = "DynamicPopov-{0}".Args(i);
                 row["DOB"] = new DateTime(1953, 12, 10);
                 row["YearsInSpace"] = 12;
                 
                 tbl.Insert( row );

                 tbl.Insert( new Person{
                                    ID = "TYPED{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "TypedPopov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            }                       
                                        
            Assert.AreEqual(2000, tbl.Count);

            var match1 = tbl.FindByKey("DYN35");
            Assert.IsNotNull( match1 );
            Assert.IsTrue( match1 is DynamicRow );
            Assert.AreEqual("DynamicPopov-35", match1["LastName"]); 

            var match2 = tbl.FindByKey("TYPED36") as Person;
            Assert.IsNotNull( match2 );
            Assert.AreEqual("TypedPopov-36", match2["LastName"]);

            var match3 = tbl.FindByKey("DoesNotExist");
            Assert.IsNull( match3 );
        }


        [TestCase]
        public void PopulateAndFindKey_DynamicRows()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
            {
                 var row =  new DynamicRow(tbl.Schema);

                 row["ID"] = "POP{0}".Args(i);
                 row["FirstName"] = "Oleg";
                 row["LastName"] = "Popov-{0}".Args(i);
                 row["DOB"] = new DateTime(1953, 12, 10);
                 row["YearsInSpace"] = 12;
                 
                 tbl.Insert( row );
            }                       
                                        
            Assert.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("POP35");
            Assert.IsNotNull( match1 );
            Assert.AreEqual("Popov-35", match1["LastName"]); 

            var match2 = tbl.FindByKey("POP36") as DynamicRow;
            Assert.IsNotNull( match2 );
            Assert.AreEqual("Popov-36", match2["LastName"]);

            var match3 = tbl.FindByKey("DoesNotExist");
            Assert.IsNull( match3 );
        }

        [TestCase]
        public void PopulateAndFindCompositeKey_TypedRows()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(WithCompositeKey)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new WithCompositeKey{
                                    ID = "ID{0}".Args(i),
                                    StartDate = new DateTime(1953, 12, 10),
                                    Description = "Descr{0}".Args(i) 
                                   });
                                   
                                        
            Assert.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("ID35", new DateTime(1953, 12, 10));
            Assert.IsNotNull( match1 );
            Assert.AreEqual("Descr35", match1["Description"]); 

            var match2 = tbl.FindByKey("ID35", new DateTime(1953, 07, 10));
            Assert.IsNull( match2 );
        }

        [TestCase]
        public void BuildUsingAdHockSchema()
        {
            var schema = new Schema("TEZT", 
                           new Schema.FieldDef("ID", typeof(int), new List<FieldAttribute>{ new FieldAttribute(required: true, key: true)}),
                           new Schema.FieldDef("Description", typeof(string), new List<FieldAttribute>{ new FieldAttribute(required: true)})
            );
            
            var tbl = new Rowset(schema);

            for(var i=0; i<1000; i++)
            {
                 var row =  new DynamicRow(tbl.Schema);

                 row["ID"] = i;
                 row["Description"] = "Item-{0}".Args(i);

                 tbl.Insert( row );
            }                       
                                        
            Assert.AreEqual(1000, tbl.Count);
            
            var match = tbl.FindByKey(178);
            Assert.IsNotNull( match );
            Assert.AreEqual("Item-178", match["Description"]);
        }


        [TestCase]
        public void PopulateAndUpdateExisting()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var update =  new Person{
                                    ID = "POP17",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14 
                                   };
            
            var idx = tbl.Update(update);//<-------------!!!!!!                       
           
            Assert.IsTrue( idx>=0 );

            var match = tbl.FindByKey("POP17") as Person;
            Assert.IsNotNull( match );
            Assert.AreEqual("Yaroslav", match.FirstName);
            Assert.AreEqual("Suzkever", match.LastName);

        }


        [TestCase]
        public void PopulateAndUpdateNonExisting()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var update =  new Person{
                                    ID = "NONE17",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14 
                                   };
            
            var idx = tbl.Update(update);//<-------------!!!!!!                       
           
            Assert.IsTrue( idx==-1 );

            var match = tbl.FindByKey("NONE17") as Person;
            Assert.IsNull( match );

        }

        [TestCase]
        public void PopulateAndUpsertExisting()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var update =  new Person{
                                    ID = "POP17",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14 
                                   };
            
            var existed = tbl.Upsert(update);//<-------------!!!!!!  
            
            Assert.IsTrue( existed );                     
           
            var match = tbl.FindByKey("POP17") as Person;
            Assert.IsNotNull( match );
            Assert.AreEqual("Yaroslav", match.FirstName);
            Assert.AreEqual("Suzkever", match.LastName);

        }

        [TestCase]
        public void PopulateAndUpsertNonExisting()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var update =  new Person{
                                    ID = "GOODMAN17",
                                    FirstName = "John",
                                    LastName = "Jeffer",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14 
                                   };
            
            var existed = tbl.Upsert(update);//<-------------!!!!!!     
            
            Assert.IsFalse( existed );

            Assert.AreEqual(1001, tbl.Count);                  
           
            var match = tbl.FindByKey("POP17") as Person;
            Assert.IsNotNull( match );
            Assert.AreEqual("Oleg", match.FirstName);
            Assert.AreEqual("Popov-17", match.LastName);

            match = tbl.FindByKey("GOODMAN17") as Person;
            Assert.IsNotNull( match );
            Assert.AreEqual("John", match.FirstName);
            Assert.AreEqual("Jeffer", match.LastName);

        }

        [TestCase]
        public void PopulateAndDeleteExisting()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var delete =  new Person{
                                    ID = "POP17"
                                   };
            
            var idx = tbl.Delete( delete );//<-------------!!!!!!                       
           
            Assert.IsTrue( idx>=0 );
            Assert.AreEqual(999, tbl.Count);

            var match = tbl.FindByKey("POP17") as Person;
            Assert.IsNull( match );
        }

        [TestCase]
        public void PopulateAndDeleteNonExisting()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var delete =  new Person{
                                    ID = "NONE17"
                                   };
            
            var idx = tbl.Delete( delete );//<-------------!!!!!!                       
           
            Assert.IsTrue( idx==-1 );
            Assert.AreEqual(1000, tbl.Count);

            var match = tbl.FindByKey("POP17") as Person;
            Assert.IsNotNull( match );
        }

        [TestCase]
        public void PopulateAndDeleteExisting_UsingValues()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
            var idx = tbl.Delete( "POP17" );//<-------------!!!!!!                       
           
            Assert.IsTrue( idx>=0 );
            Assert.AreEqual(999, tbl.Count);

            var match = tbl.FindByKey("POP17") as Person;
            Assert.IsNull( match );
        }


        [TestCase]
        public void LogChanges_Insert()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            tbl.LogChanges = true;
            
            tbl.Insert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            
           
            Assert.AreEqual(1, tbl.ChangeCount);

            Assert.AreEqual(RowChangeType.Insert, tbl.GetChangeAt(0).Value.ChangeType);
        }

        [TestCase]
        public void LogChanges_Update()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            
            tbl.Insert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            tbl.LogChanges = true;

            tbl.Update( tbl[0] );

           
            Assert.AreEqual(1, tbl.ChangeCount);

            Assert.AreEqual(RowChangeType.Update, tbl.GetChangeAt(0).Value.ChangeType);
        }

        [TestCase]
        public void LogChanges_Upsert()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            tbl.LogChanges = true;

            tbl.Upsert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            

           
            Assert.AreEqual(1, tbl.ChangeCount);

            Assert.AreEqual(RowChangeType.Upsert, tbl.GetChangeAt(0).Value.ChangeType);
        }

         [TestCase]
        public void LogChanges_Delete()
        {
            var tbl = new Rowset(Schema.GetForTypedRow(typeof(Person)));
            
            
            tbl.Insert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   });
            tbl.LogChanges = true;

            tbl.Delete( tbl[0] );

           
            Assert.AreEqual(1, tbl.ChangeCount);
            Assert.AreEqual(0, tbl.Count);

            Assert.AreEqual(RowChangeType.Delete, tbl.GetChangeAt(0).Value.ChangeType);
        }



    }
}
