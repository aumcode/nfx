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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

using NFX.DataAccess.CRUD;


namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class SchemaRegExpAndDisplayFormat
    {
        [TestCase]
        public void ValidateRegexp()
        {
            var row = new MyCar
            {
               Code = "adsd"
            };

            var ve = row.Validate();
            Assert.IsNotNull(ve);
            Assert.IsTrue( ve.Message.Contains("Allowed characters: A-Z,0-9,-"));
            Console.WriteLine( ve.ToMessageWithType());

            row.Code = "AZ-90";
            ve = row.Validate();
            Assert.IsNull(ve);
        }


        [TestCase]
        public void DisplayFormat()
        {
            var row = new MyCar
            {
               Code = "ABZ-01", 
               Milage = 150000
            };

            Assert.AreEqual("150000", row["Milage"].ToString());
            Assert.AreEqual("Milage: 150,000 miles", row.GetDisplayFieldValue("Milage"));
        }

        [TestCase]
        public void FieldValueDescription()
        {
            var row = new MyCar();

            row.Sex = "F";
            Assert.AreEqual("Female", row.GetFieldValueDescription("Sex"));
            
            row.Sex = "M";
            Assert.AreEqual("Male", row.GetFieldValueDescription("Sex"));
            
            row.Sex = "U";
            Assert.AreEqual("Unknown", row.GetFieldValueDescription("Sex"));
        }

        [TestCase]
        public void SchemaEquivalence()
        {
            Assert.IsTrue( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar2)), false ));
            Assert.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCarDiffOrder)), false ));

            Assert.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar3)), false ));
            Assert.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar4)), false ));
            Assert.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar5)), false ));
        }
    }


    public class MyCar : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}

      [Field(valueList:"M: Male, F: Female, U: Unknown")]
      public string Sex{ get; set;}
    }

    public class MyCar2 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}

      [Field(valueList:"M: Male, F: Female, U: Unknown")]
      public string Sex{ get; set;}
    }

    public class MyCarDiffOrder : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}

      [Field(valueList:"M: Male, U: Unknown, F: Female")]
      public string Sex{ get; set;}
    }

    public class MyCar3 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-8\-]+$",  //difference in regexp
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}
    }

    public class MyCar4 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed DIFFERENT characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}
    }


    public class MyCar5 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} kilometers")]
      public int Milage{ get; set;}
    }


}
