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
using NUnit.Framework;


using NFX.DataAccess.CRUD;


namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class DynamicRows
    {
       
        [TestCase]
        public void BuildUsingTypedSchema()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = "Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12; 

            Assert.AreEqual( "POP1",                       person["ID"] );
            Assert.AreEqual( "Oleg",                       person["FirstName"] );
            Assert.AreEqual( "Popov",                      person["LastName"] );
            Assert.AreEqual( new DateTime(1953, 12, 10),   person["DOB"] );
            Assert.AreEqual( 12,                           person["YearsInSpace"] );
        }

        
        [TestCase]
        public void BuildUsingAdHockSchema()
        {
            var schema = new Schema("TEZT", 
                           new Schema.FieldDef("ID", typeof(int), new List<FieldAttribute>{ new FieldAttribute(required: true, key: true)}),
                           new Schema.FieldDef("Description", typeof(string), new List<FieldAttribute>{ new FieldAttribute(required: true)})
            );

            var person = new DynamicRow(schema);
            
            person["ID"] = 123;
            person["Description"] = "Tank";

            Assert.AreEqual( 123,                       person["ID"] );
            Assert.AreEqual( "Tank",                    person["Description"] );
        }
        
        
        [TestCase]
        public void SetValuesAsDifferentTypes()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = "Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12; 

            Assert.AreEqual( "POP1",                       person["ID"] );
            Assert.AreEqual( "Oleg",                       person["FirstName"] );
            Assert.AreEqual( "Popov",                      person["LastName"] );
            Assert.AreEqual( new DateTime(1953, 12, 10),   person["DOB"] );
            Assert.AreEqual( 12,                           person["YearsInSpace"] );

            person["DOB"] = "05/15/2009 18:00";
            Assert.AreEqual( new DateTime(2009, 5, 15, 18, 0, 0),   person["DOB"] );

            person["DOB"] = null;
            Assert.IsNull( person["DOB"] );

            person["YearsInSpace"] = "-190";
            Assert.AreEqual( -190,   person["YearsInSpace"] );

            person["Description"] = 2+2;
            Assert.AreEqual( "4",   person["Description"] );

            person["Amount"] = "180.12";
            Assert.AreEqual( 180.12m,   person["Amount"] );

            person["GoodPerson"] = "true";
            Assert.AreEqual( true,   person["GoodPerson"] );

            person["LuckRatio"] = 123;
            Assert.AreEqual( 123d,   person["LuckRatio"] );

        }

        
        [TestCase]
        public void SetGetAndValidate_NoError()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = "Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12; 

            var error = person.Validate();
            Assert.IsNull( error );

            Assert.AreEqual( "POP1",                       person["ID"] );
            Assert.AreEqual( "Oleg",                       person["FirstName"] );
            Assert.AreEqual( "Popov",                      person["LastName"] );
            Assert.AreEqual( new DateTime(1953, 12, 10),   person["DOB"] );
            Assert.AreEqual( 12,                           person["YearsInSpace"] );
        }

        [TestCase]
        public void Validate_Error_StringRequired()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = null;
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12;
                
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("LastName", ((CRUDFieldValidationException)error).FieldName);
        }

        [TestCase]
        public void Validate_Error_NullableIntRequired()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
           // person["YearsInSpace"] = 12;  NOT SET!!!
            
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("YearsInSpace", ((CRUDFieldValidationException)error).FieldName);
        }

        [TestCase]
        public void Validate_Error_DecimalMinMax_1()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 45;  
            person["Amount"] = -100;
            
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("is below") );
        }

        [TestCase]
        public void Validate_Error_DecimalMinMax_2()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 45;  
            person["Amount"] = 2000000;

            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("is above") );
        }


        [TestCase]
        public void Validate_Error_DateTimeMinMax_1()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1899, 12, 10);
            person["YearsInSpace"] = 45;  
            person["Amount"] = 100;

            
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("DOB", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("is below") );
        }

        [TestCase]
        public void Validate_Error_DateTimeMinMax_DifferentTarget()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1899, 12, 10);
            person["YearsInSpace"] = 45;  
            person["Amount"] = 100;
                
            var error = person.Validate("sparta_system");
            Assert.IsNull (error);
        }

        [TestCase]
        public void Validate_Error_MaxLength()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1981, 2, 12);
            person["YearsInSpace"] = 45;  
            person["Amount"] = 100;
            person["Description"] = "0123456789012345678901234567890";
            
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Description", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("exceeds max length") );
        }


        [TestCase]
        public void Validate_Error_ValueList()
        {
             var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1981, 2, 12);
            person["YearsInSpace"] = 45;  
            person["Amount"] = 100;
            person["Description"] = "0123";
            person["Classification"] = "INVALID";
                 
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Classification", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("not in list of permitted values") );

            person["Classification"] = "good";
            Assert.IsNull( person.Validate() );
            person["Classification"] = "bad";
            Assert.IsNull( person.Validate() );
            person["Classification"] = "ugly";
            Assert.IsNull( person.Validate() );

        }



        [TestCase]
        public void Equality()
        {
            var person1 = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person1["ID"] = "POP1";
            person1["FirstName"] = "Oleg";
            person1["LastName"] = "Popov";
            person1["DOB"] = new DateTime(1981, 2, 12);
            person1["YearsInSpace"] = 45;
            person1["Amount"] = 100;
            person1["Description"]="Wanted to go to the moon";

            var person2 = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person2["ID"] = "POP1";
            person2["FirstName"] = "Egor";
            person2["LastName"] = "Pedorov";
            person2["DOB"] = new DateTime(1982, 5, 2);
            person2["YearsInSpace"] = 4;
            person2["Amount"] = 1000000;
                                   
            Assert.IsTrue( person1.Equals(person2) );

            person2["ID"] = "POP2";

            Assert.IsFalse(person1.Equals(person2) );

        }



    }
}
