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
using System.IO;
using NUnit.Framework;

using NFX.DataAccess;
using NFX.DataAccess.CRUD;


namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class FieldAttrPrototyping
    {
        [TestCase]
        public void FieldProperties()
        {
           var schema = Schema.GetForTypedRow(typeof(RowB));
           Assert.AreEqual(7, schema["FirstName"]["A"].MinLength);
           Assert.AreEqual(10, schema["FirstName"]["A"].MaxLength);
           Assert.AreEqual(true, schema["FirstName"]["A"].Required);
           Assert.AreEqual(true, schema["FirstName"]["A"].Key);

           Assert.AreEqual(8, schema["FirstName"]["B"].MinLength);
           Assert.AreEqual(15, schema["FirstName"]["B"].MaxLength);
           Assert.AreEqual(true, schema["FirstName"]["B"].Required);
           Assert.AreEqual(false, schema["FirstName"]["B"].Key);
        }

        [TestCase]
        public void MetadataProperties()
        {
           var schema = Schema.GetForTypedRow(typeof(RowB));
           Assert.IsTrue( schema["FirstName"]["A"].Metadata.AttrByName("ABC").ValueAsBool() );
           Assert.IsTrue( schema["FirstName"]["A"].Metadata.AttrByName("DEF").ValueAsBool() );
           Assert.IsTrue( schema["FirstName"]["A"].Metadata["Another"].Exists );
           Assert.IsTrue( schema["FirstName"]["B"].Metadata.AttrByName("ABC").ValueAsBool() );
           Assert.IsFalse( schema["FirstName"]["B"].Metadata.AttrByName("DEF").ValueAsBool() );

           Assert.IsTrue( schema["LastName"]["A"].Metadata.AttrByName("flag").ValueAsBool() );
           Assert.IsFalse( schema["LastName"]["B"].Metadata.AttrByName("flag").ValueAsBool() );
        }


        [TestCase]
        [ExpectedException(typeof(CRUDException), ExpectedMessage="recursive field definition", MatchType=MessageMatch.Contains)]
        public void Recursive()
        {
           var schema = Schema.GetForTypedRow(typeof(RowC));
        }



    }


              public class RowA : TypedRow
              {
                [Field(targetName: "A", storeFlag: StoreFlag.LoadAndStore, key: true, minLength: 5, maxLength: 10, required: true, metadata: "abc=true")]
                [Field(targetName: "B", storeFlag: StoreFlag.OnlyLoad, key: true, minLength: 5, maxLength: 15, required: true, metadata: "abc=false")]
                public string FirstName{get; set;}
                
                [Field(targetName: "A", storeFlag: StoreFlag.OnlyStore, minLength: 15, maxLength: 25, required: true, metadata: "flag=true")]
                [Field(targetName: "B", storeFlag: StoreFlag.LoadAndStore, minLength: 15, maxLength: 20, required: false, metadata: "flag=false")]
                public string LastName{get; set;}
              }

              public class RowB : TypedRow
              {
                [Field(protoType: typeof(RowA), protoFieldName: "FirstName", targetName: "A", minLength: 7, metadata: "def=true another{}")]
                [Field(protoType: typeof(RowA), protoFieldName: "FirstName", targetName: "B", key: false, minLength: 8, metadata: "abc=true def=false")]
                public string FirstName{get; set;}
                
                [Field(protoType: typeof(RowA), protoFieldName: "LastName", targetName: "A")]
                [Field(protoType: typeof(RowA), protoFieldName: "LastName", targetName: "B")]
                public string LastName{get; set;}
              }

              public class RowC : TypedRow
              {
                [Field(protoType: typeof(RowE), protoFieldName: "FirstName")]
                public string FirstName{get; set;}
              }

              public class RowD : TypedRow
              {
                [Field(protoType: typeof(RowC), protoFieldName: "FirstName")]
                public string FirstName{get; set;}
              }

              public class RowE : TypedRow
              {
                [Field(protoType: typeof(RowD), protoFieldName: "FirstName")]
                public string FirstName{get; set;}
              }





}
