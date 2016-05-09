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
using NUnit.Framework;


using NFX.DataAccess.CRUD;


namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class FieldAttrCloningTests
    {
        [TestCase]
        public void T1()
        {
            var schema1 = Schema.GetForTypedRow<Row1>();
            Assert.AreEqual(3, schema1.FieldCount);
            
            Assert.IsTrue( schema1["First_Name"][null].Required );
            Assert.AreEqual(45, schema1["First_Name"][null].MaxLength);

            Assert.IsTrue( schema1["Last_Name"][null].Required );
            Assert.AreEqual(75, schema1["Last_Name"][null].MaxLength);

            Assert.IsFalse( schema1["Age"][null].Required );
            Assert.AreEqual(150, schema1["Age"][null].Max);




            var schema2 = Schema.GetForTypedRow<Row2>();
            Assert.AreEqual(3, schema2.FieldCount);

            Assert.IsTrue( schema2["First_Name"][null].Required );
            Assert.AreEqual(47, schema2["First_Name"][null].MaxLength);

            Assert.IsTrue( schema2["Last_Name"][null].Required );
            Assert.AreEqual(75, schema2["Last_Name"][null].MaxLength);

            Assert.IsFalse( schema2["Age"][null].Required );
            Assert.AreEqual(150, schema2["Age"][null].Max);
        }

        [TestCase]
        [ExpectedException(typeof(CRUDException), ExpectedMessage="only a single", MatchType=MessageMatch.Contains)]
        public void T2()
        {
            var schema1 = Schema.GetForTypedRow<Row3>();
        }

        [TestCase]
        [ExpectedException(typeof(CRUDException), ExpectedMessage="there is no field", MatchType=MessageMatch.Contains)]
        public void T3()
        {
            var schema1 = Schema.GetForTypedRow<Row4>();
        }

        [TestCase]
        [ExpectedException(typeof(CRUDException), ExpectedMessage="recursive field", MatchType=MessageMatch.Contains)]
        public void T4()
        {
            var schema1 = Schema.GetForTypedRow<Row5>();
        }

        [TestCase]
        [ExpectedException(typeof(CRUDException), ExpectedMessage="recursive field", MatchType=MessageMatch.Contains)]
        public void T5()
        {
            var schema1 = Schema.GetForTypedRow<Row6>();
        }

        [TestCase]
        [ExpectedException(typeof(CRUDException), ExpectedMessage="recursive field", MatchType=MessageMatch.Contains)]
        public void T6()
        {
            var schema1 = Schema.GetForTypedRow<Row7>();
        }


        public class Row1 : TypedRow
        {
          [Field(required: true, maxLength: 45)]
          public string First_Name{ get; set;}

          [Field(required: true, maxLength: 75)]
          public string Last_Name{ get; set;}

          [Field(required: false, max: 150)]
          public int? Age{ get; set;}
        }

        public class Row2 : TypedRow
        {
          [Field(typeof(Row1), "First_Name", maxLength: 47)]
          public string First_Name{ get; set;}

          [Field(typeof(Row1))]
          public string Last_Name{ get; set;}

          [Field(typeof(Row1))]
          public int? Age{ get; set;}
        }

        public class Row3 : TypedRow
        {
          [Field(typeof(Row1))]
          [Field] //cant have mnore than one
          public string Last_Name{ get; set;}
        }

        public class Row4 : TypedRow
        {
          [Field(typeof(Row1))]
          public string DOESNOTEXIST{ get; set;}
        }

        public class Row5 : TypedRow
        {
          [Field(required: true, maxLength: 45)]
          public string First_Name{ get; set;}

          [Field(required: true, maxLength: 75)]
          public string Last_Name{ get; set;}

          [Field(typeof(Row7))]
          public int? Age{ get; set;}
        }

        public class Row6 : TypedRow
        {
          [Field(typeof(Row5), "First_Name", maxLength: 47)]
          public string First_Name{ get; set;}

          [Field(typeof(Row5))]
          public string Last_Name{ get; set;}

          [Field(typeof(Row5))]
          public int? Age{ get; set;}
        }

         public class Row7 : TypedRow
        {
          [Field(typeof(Row6))]
          public int? Age{ get; set;}
        }

    }
}
