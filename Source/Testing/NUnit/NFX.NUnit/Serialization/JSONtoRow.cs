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
using NFX.Serialization.JSON;



namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class JSONtoRow
    { 
       
        [TestCase]
        public void ToAmorphousDynamicRow_FromString()
        {
          var str = @"{name: ""Orlov"", StringArray: [""a"", null, ""b""]}";

          var row = new RowWithArrays();

          JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

          Assert.AreEqual("Orlov", row.Name);
          
          Assert.IsNotNull( row.StringArray );
          Assert.AreEqual(3, row.StringArray.Length );

          Assert.AreEqual("a",  row.StringArray[0] );
          Assert.AreEqual(null, row.StringArray[1] );
          Assert.AreEqual("b",  row.StringArray[2] );
        }

        //tests





        // TYPES------------------------------------------------------------------------

        public class RowWithArrays : TypedRow
        {
          [Field] public string   Name{ get; set;}
          [Field] public string[] StringArray{ get; set;}


          [Field] public sbyte[] Int8Array{ get; set;}
          [Field] public sbyte?[] Int8NArray{ get; set;}

          [Field] public byte[] UInt8Array{ get; set;}
          [Field] public byte?[] UInt8NArray{ get; set;}

          [Field] public short[] Int16Array{ get; set;}
          [Field] public short?[] Int16NArray{ get; set;}

          [Field] public ushort[] UInt16Array{ get; set;}
          [Field] public ushort?[] UInt16NArray{ get; set;}

          [Field] public int[] Int32Array{ get; set;}
          [Field] public int?[] Int32NArray{ get; set;}

          [Field] public uint[] UInt32Array{ get; set;}
          [Field] public uint?[] UInt32NArray{ get; set;}

          [Field] public long[] Int64Array{ get; set;}
          [Field] public long?[] Int64NArray{ get; set;}

          [Field] public ulong[] UInt64Array{ get; set;}
          [Field] public ulong?[] UInt64NArray{ get; set;}

          [Field] public byte[] ByteArray{ get; set;}
          
          //todo: GDID, Guid,  their nullable variants
          //  TypedRow[]
        }

        public class RowWithList : TypedRow
        {
          //todo repeat all tests from array but use List<T>
        }



    }
}
