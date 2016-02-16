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
using System.Linq;       
using System.Collections.Generic;
using NUnit.Framework;
using NFX.DataAccess.CRUD;

namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class RowCopyFields
    {
        [TestCase]
        public void CopyFields_TypedRow()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                LastName = "Petrov",
                Amount = 10,
                Classification = "class1",
                Description = null,
                DOB = new DateTime(1990, 2, 16),
                GoodPerson = true,
                ID = "abc",
                LuckRatio = 12345.6789D,
                YearsInSpace = 20,
                YearsWithCompany = null
            };

            var to = new Person
            {
                Description = "descr",
                YearsWithCompany = 30
            };

            from.CopyFields(to);

            Assert.AreEqual(to.FirstName, from.FirstName);
            Assert.AreEqual(to.LastName, from.LastName);
            Assert.AreEqual(to.Amount, from.Amount);
            Assert.AreEqual(to.Classification, from.Classification);
            Assert.AreEqual(to.Description, from.Description);
            Assert.AreEqual(to.DOB, from.DOB);
            Assert.AreEqual(to.GoodPerson, from.GoodPerson);
            Assert.AreEqual(to.ID, from.ID);
            Assert.AreEqual(to.LuckRatio, from.LuckRatio);
            Assert.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Assert.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
        }   

        [TestCase]
        public void CopyFields_TypedRow_Filter()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                Amount = 10,
                DOB = new DateTime(1990, 2, 16),
                GoodPerson = true,
                LuckRatio = 12345.6789D,
                YearsInSpace = 20,
                YearsWithCompany = null
            };

            var to = new Person
            {
                Description = "descr",
                DOB = new DateTime(1980, 2, 16),
                GoodPerson = false,
                LuckRatio = 12345.6789D,
                YearsWithCompany = 30
            };

            from.CopyFields(to, false, false, (n, f) => f.Name != "DOB" && f.Name != "GoodPerson" );

            Assert.AreEqual(to.FirstName, from.FirstName);
            Assert.AreEqual(to.LastName, from.LastName);
            Assert.AreEqual(to.Amount, from.Amount);
            Assert.AreEqual(to.Classification, from.Classification);
            Assert.AreEqual(to.Description, from.Description);
            Assert.AreEqual(new DateTime(1980, 2, 16), to.DOB);
            Assert.AreEqual(false, to.GoodPerson);
            Assert.AreEqual(to.ID, from.ID);
            Assert.AreEqual(to.LuckRatio, from.LuckRatio);
            Assert.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Assert.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
        } 

        [TestCase]
        public void CopyFields_TypedRow_Complex()
        {
            var from = new ExtendedPerson
            {
                FirstName = "Ivan",
                Parent = new Person { FirstName = "John", Amount = 12, GoodPerson = true },
                Children = new List<Person> { new Person { FirstName = "John" }, new Person { LuckRatio = 12.3D } }
            };

            var to = new ExtendedPerson
            {
                FirstName = "Anna",
                Description = "descr",
                YearsWithCompany = 30,
                Parent = new Person { FirstName = "Maria" }
            };

            from.CopyFields(to);

            Assert.AreEqual(to.FirstName, from.FirstName);
            Assert.AreEqual(to.LastName, from.LastName);
            Assert.AreEqual(to.Amount, from.Amount);
            Assert.AreEqual(to.Classification, from.Classification);
            Assert.AreEqual(to.Description, from.Description);
            Assert.AreEqual(to.DOB, from.DOB);
            Assert.AreEqual(to.GoodPerson, from.GoodPerson);
            Assert.AreEqual(to.ID, from.ID);
            Assert.AreEqual(to.LuckRatio, from.LuckRatio);
            Assert.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Assert.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
            Assert.AreEqual(to.Info, from.Info);
            Assert.AreEqual(to.Parent, from.Parent);
            Assert.AreEqual(to.Children, from.Children);
        }

        [TestCase]
        public void CopyFields_TypedRow_To_Extended()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                Amount = 10,
                DOB = new DateTime(1990, 2, 16),
                YearsWithCompany = null
            };

            var to = new ExtendedPerson
            {
                FirstName = "John",
                Description = "descr",
                YearsWithCompany = 30,
                Info = "extended info",
                Count = long.MaxValue
            };

            from.CopyFields(to);

            Assert.AreEqual(to.FirstName, from.FirstName);
            Assert.AreEqual(to.LastName, from.LastName);
            Assert.AreEqual(to.Amount, from.Amount);
            Assert.AreEqual(to.Classification, from.Classification);
            Assert.AreEqual(to.Description, from.Description);
            Assert.AreEqual(to.DOB, from.DOB);
            Assert.AreEqual(to.GoodPerson, from.GoodPerson);
            Assert.AreEqual(to.ID, from.ID);
            Assert.AreEqual(to.LuckRatio, from.LuckRatio);
            Assert.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Assert.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
            Assert.AreEqual("extended info", to.Info);
            Assert.AreEqual(long.MaxValue, to.Count);
        }    

        [TestCase]
        public void CopyFields_Extended_To_TypedRow()
        {
            var from = new ExtendedPerson
            {
                FirstName = "John",
                Description = "descr",
                YearsWithCompany = 30,
                Info = "extended info",
                Count = long.MaxValue
            };

            var to = new Person
            {
                FirstName = "Ivan",
                Amount = 10,
                DOB = new DateTime(1990, 2, 16),
                YearsWithCompany = null
            };

            from.CopyFields(to);

            Assert.AreEqual(to.FirstName, from.FirstName);
            Assert.AreEqual(to.LastName, from.LastName);
            Assert.AreEqual(to.Amount, from.Amount);
            Assert.AreEqual(to.Classification, from.Classification);
            Assert.AreEqual(to.Description, from.Description);
            Assert.AreEqual(to.DOB, from.DOB);
            Assert.AreEqual(to.GoodPerson, from.GoodPerson);
            Assert.AreEqual(to.ID, from.ID);
            Assert.AreEqual(to.LuckRatio, from.LuckRatio);
            Assert.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Assert.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
        }
                                   
        [TestCase]
        public void CopyFields_TypedRow_To_Amorphous_NotIncludeAmorphous()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                LastName = "Petrov",
                Amount = 10,
                Classification = "class1",
                YearsWithCompany = null
            };

            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, false);

            Assert.AreEqual(0, to.Schema.FieldCount);
            Assert.AreEqual(2, to.AmorphousData.Count);
            Assert.AreEqual(123, to.AmorphousData["field1"]);
            Assert.AreEqual("John", to.AmorphousData["FirstName"]);        
        }
                                   
        [TestCase]
        public void CopyFields_TypedRow_To_Amorphous_IncludeAmorphous()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                LastName = "Petrov",
                Amount = 10,
                Classification = "class1",
                YearsWithCompany = null
            };

            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, true);

            Assert.AreEqual(0, to.Schema.FieldCount);
            Assert.AreEqual(12, to.AmorphousData.Count);
            Assert.AreEqual(123, to.AmorphousData["field1"]);
            Assert.AreEqual(from.FirstName, to.AmorphousData["FirstName"]);
            Assert.AreEqual(from.LastName, to.AmorphousData["LastName"]);
            Assert.AreEqual(from.Amount, to.AmorphousData["Amount"]);
            Assert.AreEqual(from.Classification, to.AmorphousData["Classification"]);
            Assert.AreEqual(from.YearsWithCompany, to.AmorphousData["YearsWithCompany"]);
        }  
                                   
        [TestCase]
        public void CopyFields_ExtendedTypedRow_To_Amorphous_IncludeAmorphous()
        {
            var from = new ExtendedPerson
            {
                FirstName = "Ivan",
                Amount = 10,
                YearsWithCompany = null,
                Count = 4567,
                Info = "extended info"
            };

            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Person)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, true);

            Assert.AreEqual(11, to.Schema.FieldCount);
            Assert.AreEqual(6, to.AmorphousData.Count);
            Assert.AreEqual(123, to.AmorphousData["field1"]);
            Assert.AreEqual("John", to.AmorphousData["FirstName"]);
            Assert.AreEqual(from.Count, to.AmorphousData["Count"]);
            Assert.AreEqual(from.Info, to.AmorphousData["Info"]);
            Assert.AreEqual(null, to.AmorphousData["Parent"]);
            Assert.AreEqual(null, to.AmorphousData["Children"]);
            Assert.AreEqual(from.FirstName, to["FirstName"]);     
            Assert.AreEqual(from.Amount, to["Amount"]);     
            Assert.AreEqual(from.YearsWithCompany, to["YearsWithCompany"]);     
        }

        [TestCase]
        public void CopyFields_DynamicRow()
        {
            var from = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LastName"] = "Petrov";
            from["Amount"] = 10;
            from["Classification"] = "class1";
            from["Description"] = null;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;
            from["ID"] = "abc";
            from["LuckRatio"] = 12345.6789D;
            from["YearsInSpace"] = 20;
            from["YearsWithCompany"] = null;

            var to = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            to["Description"] = "descr";
            to["YearsWithCompany"] = 30;

            from.CopyFields(to);

            Assert.AreEqual(to["FirstName"], from["FirstName"]);
            Assert.AreEqual(to["LastName"], from["LastName"]);
            Assert.AreEqual(to["Amount"], from["Amount"]);
            Assert.AreEqual(to["Classification"], from["Classification"]);
            Assert.AreEqual(to["Description"], from["Description"]);
            Assert.AreEqual(to["DOB"], from["DOB"]);
            Assert.AreEqual(to["GoodPerson"], from["GoodPerson"]);
            Assert.AreEqual(to["ID"], from["ID"]);
            Assert.AreEqual(to["LuckRatio"], from["LuckRatio"]);
            Assert.AreEqual(to["YearsInSpace"], from["YearsInSpace"]);
            Assert.AreEqual(to["YearsWithCompany"], from["YearsWithCompany"]);
        }

        [TestCase]
        public void CopyFields_DynamicRow_To_Extended()
        {
            var schema = Schema.GetForTypedRow(typeof(Person));
            var from = new DynamicRow(schema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;

            var fieldDefs = schema.FieldDefs.ToList();
            fieldDefs.Add(new Schema.FieldDef("Info", typeof(string), new QuerySource.ColumnDef("Info")));
            fieldDefs.Add(new Schema.FieldDef("Count", typeof(long), new QuerySource.ColumnDef("Info")));
            var extendedSchema = new Schema("sname", fieldDefs.ToArray());

            var to = new DynamicRow(extendedSchema);
            to["Info"] = "extended info";
            to["Count"] = long.MaxValue;

            from.CopyFields(to);

            Assert.AreEqual(to["FirstName"], from["FirstName"]);
            Assert.AreEqual(to["Amount"], from["Amount"]);
            Assert.AreEqual(to["DOB"], from["DOB"]);
            Assert.AreEqual(to["GoodPerson"], from["GoodPerson"]);
            Assert.AreEqual("extended info", to["Info"]);
            Assert.AreEqual(long.MaxValue, to["Count"]);
        }

        [TestCase]
        public void CopyFields_ExtendedDynamicRow_To_DynamicRow()
        {
            var schema = Schema.GetForTypedRow(typeof(Person));
            var fieldDefs = schema.FieldDefs.ToList();
            fieldDefs.Add(new Schema.FieldDef("Info", typeof(string), new QuerySource.ColumnDef("Info")));
            fieldDefs.Add(new Schema.FieldDef("Count", typeof(long), new QuerySource.ColumnDef("Info")));
            var extendedSchema = new Schema("sname", fieldDefs.ToArray());

            var from = new DynamicRow(extendedSchema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;
            from["Info"] = "extended info";
            from["Count"] = long.MaxValue;

            var to = new DynamicRow(schema);

            from.CopyFields(to);

            Assert.AreEqual(to["FirstName"], from["FirstName"]);
            Assert.AreEqual(to["Amount"], from["Amount"]);
            Assert.AreEqual(to["DOB"], from["DOB"]);
            Assert.AreEqual(to["GoodPerson"], from["GoodPerson"]);
        } 
                                   
        [TestCase]
        public void CopyFields_DynamicRow_To_Amorphous_NotIncludeAmorphous()
        {
            var schema = Schema.GetForTypedRow(typeof(Person));
            var from = new DynamicRow(schema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;

            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, false);

            Assert.AreEqual(0, to.Schema.FieldCount);
            Assert.AreEqual(2, to.AmorphousData.Count);
            Assert.AreEqual(123, to.AmorphousData["field1"]);
            Assert.AreEqual("John", to.AmorphousData["FirstName"]);        
        } 
                                   
        [TestCase]
        public void CopyFields_DynamicRow_To_Amorphous_IncludeAmorphous()
        {
            var schema = Schema.GetForTypedRow(typeof(Person));
            var from = new DynamicRow(schema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;

            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, true);

            Assert.AreEqual(0, to.Schema.FieldCount);
            Assert.AreEqual(12, to.AmorphousData.Count);
            Assert.AreEqual(123, to.AmorphousData["field1"]);
            Assert.AreEqual(from["FirstName"], to.AmorphousData["FirstName"]);        
            Assert.AreEqual(from["Amount"], to.AmorphousData["Amount"]);        
            Assert.AreEqual(from["DOB"], to.AmorphousData["DOB"]);        
            Assert.AreEqual(from["GoodPerson"], to.AmorphousData["GoodPerson"]);        
        }

        [TestCase]
        public void CopyFields_AmorphousDynamicRow_To_TypedRow()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LuckRatio"] = 12345.6789D;
            from.AmorphousData["field1"] = "some data";

            var to = new Person
            {
                FirstName = "Jack",
                Description = "descr",
                YearsWithCompany = 30
            };

            from.CopyFields(to);

            Assert.AreEqual(to["FirstName"], from["FirstName"]);
            Assert.AreEqual(to["LuckRatio"], from["LuckRatio"]);
            Assert.AreEqual(to["YearsWithCompany"], from["YearsWithCompany"]);
            Assert.AreEqual(null, to.Schema["field1"]);
        }

        [TestCase]
        public void CopyFields_AmorphousDynamicRow_To_DynamicRow()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LuckRatio"] = 12345.6789D;
            from.AmorphousData["field1"] = "some data";

            var to = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            to["Description"] = "descr";
            to["YearsWithCompany"] = 30;

            from.CopyFields(to);

            Assert.AreEqual(to["FirstName"], from["FirstName"]);
            Assert.AreEqual(to["LuckRatio"], from["LuckRatio"]);
            Assert.AreEqual(to["Description"], from["Description"]);
            Assert.AreEqual(to["YearsWithCompany"], from["YearsWithCompany"]);  
            Assert.AreEqual(null, to.Schema["field1"]);
        }

        [TestCase]
        public void CopyFields_AmorphousDynamicRow_NotIncludeAmorphous()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LuckRatio"] = 12345.6789D;
            from.AmorphousData["field1"] = "some data";

            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Person)));
            from["FirstName"] = "Jack";
            from["YearsInSpace"] = 20;

            from.CopyFields(to, false);

            Assert.AreEqual(to["FirstName"], from["FirstName"]);
            Assert.AreEqual(to["LuckRatio"], from["LuckRatio"]);
            Assert.AreEqual(to["YearsInSpace"], from["YearsInSpace"]); 
            Assert.AreEqual(null, to.Schema["field1"]);
            Assert.AreEqual(0, to.AmorphousData.Count);
        } 

        [TestCase]
        public void CopyFields_AmorphousDynamicRow_IncludeAmorphous()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            from.AmorphousData["field1"] = "some data";
            from.AmorphousData["field2"] = 123;
                                      
            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            to.AmorphousData["field2"] = "234";
            to.AmorphousData["field3"] = 1.2D;

            from.CopyFields(to, true);

            Assert.AreEqual(3, to.AmorphousData.Count);
            Assert.AreEqual(from.AmorphousData["field1"], to.AmorphousData["field1"]);
            Assert.AreEqual(from.AmorphousData["field2"], to.AmorphousData["field2"]);
            Assert.AreEqual(1.2D, to.AmorphousData["field3"]); 
        } 

        [TestCase]
        public void CopyFields_AmorphousDynamicRow_Filter()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            from.AmorphousData["field1"] = "some data";
            from.AmorphousData["field2"] = 123;
            from.AmorphousData["field3"] = "info";
                                      
            var to = new AmorphousDynamicRow(Schema.GetForTypedRow(typeof(Empty)));
            to.AmorphousData["field2"] = "234";
            to.AmorphousData["field3"] = 1.2D;
            to.AmorphousData["field4"] = 12345;

            from.CopyFields(to, true, false, null, (s, n) => n != "field2" );

            Assert.AreEqual(4, to.AmorphousData.Count);
            Assert.AreEqual(from.AmorphousData["field1"], to.AmorphousData["field1"]);
            Assert.AreEqual("234", to.AmorphousData["field2"]);
            Assert.AreEqual(to.AmorphousData["field3"], to.AmorphousData["field3"]); 
            Assert.AreEqual(12345, to.AmorphousData["field4"]); 
        }
    }
}
