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
    public class TypedRows
    {
        [TestCase]
        public void RowFieldValueEnumerator()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   };     
            
            var cnt = person.Count();

            Assert.AreEqual( cnt, person.Schema.FieldCount);
            Assert.AreEqual( "Popov", person.ElementAt(2));
        }

        
        
        
        [TestCase]
        public void Validate_NoError()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   };     
            var error = person.Validate();
            Assert.IsNull( error );
        }
        

        [TestCase]
        public void SetAndGetByName()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   };    
                                   
            person["dEscRipTION"] = "moon"; //field names are case-insensitive                       
           
            Assert.AreEqual(person.ID, person["ID"]);
            Assert.AreEqual(person.FirstName, person["FirstName"]);
            Assert.AreEqual(person.LastName, person["LastName"]);
            Assert.AreEqual(person.DOB, person["DOB"]);
            Assert.AreEqual(person.YearsInSpace, person["YearsInSpace"]);
            Assert.AreEqual(person.Description, person["Description"]);
        }

        [TestCase]
        public void SetAndGetByIndex()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12 
                                   };     
           
            Assert.AreEqual(person.ID,               person[person.Schema["ID"].Order]);
            Assert.AreEqual(person.FirstName,        person[person.Schema["FirstName"].Order]);
            Assert.AreEqual(person.LastName,         person[person.Schema["LastName"].Order]);
            Assert.AreEqual(person.DOB,              person[person.Schema["DOB"].Order]);
            Assert.AreEqual(person.YearsWithCompany, person[person.Schema["YearsWithCompany"].Order]);
            Assert.AreEqual(person.YearsInSpace,     person[person.Schema["YearsInSpace"].Order]);
            
        }


        [TestCase]
        public void Validate_Error_StringRequired()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = null,
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12  
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("LastName", ((CRUDFieldValidationException)error).FieldName);
        }

        [TestCase]
        public void Validate_Error_NullableIntRequired()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10) 
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("YearsInSpace", ((CRUDFieldValidationException)error).FieldName);
        }

        [TestCase]
        public void Validate_Error_DecimalMinMax_1()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = -100
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("is below") );
        }

        [TestCase]
        public void Validate_Error_DecimalMinMax_2()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 2000000
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("is above") );
        }


        [TestCase]
        public void Validate_Error_DateTimeMinMax_1()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1899, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("DOB", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("is below") );
        }

        [TestCase]
        public void Validate_Error_DateTimeMinMax_DifferentTarget()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1899, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100
                                   };     
            var error = person.Validate("sparta_system");
            Assert.IsNull (error);
        }

        [TestCase]
        public void Validate_Error_MaxLength()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1981, 2, 12),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    Description="0123456789012345678901234567890"
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Description", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("exceeds max length") );
        }


        [TestCase]
        public void Validate_Error_ValueList()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1981, 2, 12),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    Description="012345",
                                    Classification = "INVALID"
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Classification", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue( error.Message.Contains("not in list of permitted values") );

            person.Classification = "good";
            Assert.IsNull( person.Validate() );
            person.Classification = "bad";
            Assert.IsNull( person.Validate() );
            person.Classification = "ugly";
            Assert.IsNull( person.Validate() );

        }


        



        [TestCase]
        public void Equality()
        {
            var person1 = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1981, 2, 12),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    Description="Wanted to go to the moon"
                                   };     
            var person2 = new Person{
                                    ID = "POP1",
                                    FirstName = "Egor",
                                    LastName = "Pedorov",
                                    DOB = new DateTime(1982, 5, 2),
                                    YearsInSpace = 4,
                                    Amount = 1000000
                                   };
            Assert.IsTrue( person1.Equals(person2) );

            person2.ID = "POP2";

            Assert.IsFalse(person1.Equals(person2) );

        }



        [TestCase]
        public void Validate_PersonWithNesting_Error_ComplexFieldRequired()
        {
            var person = new PersonWithNesting{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    History1  = new List<HistoryItem>(),
                                   // History2  = new HistoryItem[2]
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("History2", ((CRUDFieldValidationException)error).FieldName);
        }

        [TestCase]
        public void Validate_PersonWithNesting_Error_ComplexFieldCustomValidation()
        {
            var person = new PersonWithNesting{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Someone is an idiot!" },
                                    History1  = new List<HistoryItem>(),
                                    History2  = new HistoryItem[2]
                                   };     
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("Description", ((CRUDFieldValidationException)error).FieldName);
            Assert.IsTrue(error.Message.Contains("Chaplin"));
        }

        [TestCase]
        public void Validate_PersonWithNesting_Error_ComplexFieldSubFieldRequired()
        {
            var person = new PersonWithNesting{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    LatestHistory = new HistoryItem{ ID = null, StartDate = DateTime.Now, Description="Chaplin is here" },
                                    History1  = new List<HistoryItem>(),
                                    History2  = new HistoryItem[2]
                                   };        
            var error = person.Validate();
            Console.WriteLine( error );
            Assert.IsInstanceOf(typeof(CRUDFieldValidationException), error);
            Assert.AreEqual("ID", ((CRUDFieldValidationException)error).FieldName);
        }




    }
}
