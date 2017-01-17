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
using NFX.Serialization.JSON;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class JSONtoRow
    {
        #region Arrays

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Strings()
        {
            var str = @"{name: ""Orlov"", StringArray: [""a"", null, ""b""]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual("Orlov", row.Name);

            Assert.IsNotNull(row.StringArray);
            Assert.AreEqual(3, row.StringArray.Length);

            Assert.AreEqual("a", row.StringArray[0]);
            Assert.AreEqual(null, row.StringArray[1]);
            Assert.AreEqual("b", row.StringArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Bytes()
        {
            var str = @"{UInt8: 255, UInt8Array: [1, 0, 255], UInt8NArray: [null, 0, 124]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(255, row.UInt8);

            Assert.IsNotNull(row.UInt8Array);
            Assert.AreEqual(3, row.UInt8Array.Length);
            Assert.AreEqual(1, row.UInt8Array[0]);
            Assert.AreEqual(0, row.UInt8Array[1]);
            Assert.AreEqual(255, row.UInt8Array[2]);

            Assert.IsNotNull(row.UInt8NArray);
            Assert.AreEqual(3, row.UInt8NArray.Length);
            Assert.AreEqual(null, row.UInt8NArray[0]);
            Assert.AreEqual(0, row.UInt8NArray[1]);
            Assert.AreEqual(124, row.UInt8NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_SBytes()
        {
            var str = @"{Int8: -56, Int8Array: [-1, 0, 127], Int8NArray: [null, 0, 127]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(-56, row.Int8);

            Assert.IsNotNull(row.Int8Array);
            Assert.AreEqual(3, row.Int8Array.Length);
            Assert.AreEqual(-1, row.Int8Array[0]);
            Assert.AreEqual(0, row.Int8Array[1]);
            Assert.AreEqual(127, row.Int8Array[2]);

            Assert.IsNotNull(row.Int8NArray);
            Assert.AreEqual(3, row.Int8NArray.Length);
            Assert.AreEqual(null, row.Int8NArray[0]);
            Assert.AreEqual(0, row.Int8NArray[1]);
            Assert.AreEqual(127, row.Int8NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Shorts()
        {
            var str = @"{Int16: 12345, Int16Array: [32767, 0, -32768], Int16NArray: [null, 0, 32767]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(12345, row.Int16);

            Assert.IsNotNull(row.Int16Array);
            Assert.AreEqual(3, row.Int16Array.Length);
            Assert.AreEqual(short.MaxValue, row.Int16Array[0]);
            Assert.AreEqual(0, row.Int16Array[1]);
            Assert.AreEqual(short.MinValue, row.Int16Array[2]);

            Assert.IsNotNull(row.Int16NArray);
            Assert.AreEqual(3, row.Int16NArray.Length);
            Assert.AreEqual(null, row.Int16NArray[0]);
            Assert.AreEqual(0, row.Int16NArray[1]);
            Assert.AreEqual(short.MaxValue, row.Int16NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_UShorts()
        {
            var str = @"{UInt16: 123, UInt16Array: [65535, 0, 12345], UInt16NArray: [null, 0, 65535]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.UInt16);

            Assert.IsNotNull(row.UInt16Array);
            Assert.AreEqual(3, row.UInt16Array.Length);
            Assert.AreEqual(ushort.MaxValue, row.UInt16Array[0]);
            Assert.AreEqual(0, row.UInt16Array[1]);
            Assert.AreEqual(12345, row.UInt16Array[2]);

            Assert.IsNotNull(row.UInt16NArray);
            Assert.AreEqual(3, row.UInt16NArray.Length);
            Assert.AreEqual(null, row.UInt16NArray[0]);
            Assert.AreEqual(0, row.UInt16NArray[1]);
            Assert.AreEqual(ushort.MaxValue, row.UInt16NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Int()
        {
            var str = @"{Int32: 123, Int32Array: [2147483647, 0, -2147483648], Int32NArray: [null, 0, 2147483647]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.Int32);

            Assert.IsNotNull(row.Int32Array);
            Assert.AreEqual(3, row.Int32Array.Length);
            Assert.AreEqual(int.MaxValue, row.Int32Array[0]);
            Assert.AreEqual(0, row.Int32Array[1]);
            Assert.AreEqual(int.MinValue, row.Int32Array[2]);

            Assert.IsNotNull(row.Int32NArray);
            Assert.AreEqual(3, row.Int32NArray.Length);
            Assert.AreEqual(null, row.Int32NArray[0]);
            Assert.AreEqual(0, row.Int32NArray[1]);
            Assert.AreEqual(int.MaxValue, row.Int32NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_UInt()
        {
            var str = @"{UInt32: 123, UInt32Array: [4294967295, 0, 124], UInt32NArray: [null, 0, 4294967295]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.UInt32);

            Assert.IsNotNull(row.UInt32Array);
            Assert.AreEqual(3, row.UInt32Array.Length);
            Assert.AreEqual(uint.MaxValue, row.UInt32Array[0]);
            Assert.AreEqual(0, row.UInt32Array[1]);
            Assert.AreEqual(124, row.UInt32Array[2]);

            Assert.IsNotNull(row.UInt32NArray);
            Assert.AreEqual(3, row.UInt32NArray.Length);
            Assert.AreEqual(null, row.UInt32NArray[0]);
            Assert.AreEqual(0, row.UInt32NArray[1]);
            Assert.AreEqual(uint.MaxValue, row.UInt32NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Long()
        {
            var str = @"{Int64: 123, Int64Array: [9223372036854775807, 0, -9223372036854775808], Int64NArray: [null, 0, 9223372036854775807]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.Int64);

            Assert.IsNotNull(row.Int64Array);
            Assert.AreEqual(3, row.Int64Array.Length);
            Assert.AreEqual(long.MaxValue, row.Int64Array[0]);
            Assert.AreEqual(0, row.Int64Array[1]);
            Assert.AreEqual(long.MinValue, row.Int64Array[2]);

            Assert.IsNotNull(row.Int64NArray);
            Assert.AreEqual(3, row.Int64NArray.Length);
            Assert.AreEqual(null, row.Int64NArray[0]);
            Assert.AreEqual(0, row.Int64NArray[1]);
            Assert.AreEqual(long.MaxValue, row.Int64NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_ULong()
        {
            var str = @"{UInt64: 123, UInt64Array: [18446744073709551615, 0, 124], UInt64NArray: [null, 0, 18446744073709551615]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.UInt64);

            Assert.IsNotNull(row.UInt64Array);
            Assert.AreEqual(3, row.UInt64Array.Length);
            Assert.AreEqual(ulong.MaxValue, row.UInt64Array[0]);
            Assert.AreEqual(0, row.UInt64Array[1]);
            Assert.AreEqual(124, row.UInt64Array[2]);

            Assert.IsNotNull(row.UInt64NArray);
            Assert.AreEqual(3, row.UInt64NArray.Length);
            Assert.AreEqual(null, row.UInt64NArray[0]);
            Assert.AreEqual(0, row.UInt64NArray[1]);
            Assert.AreEqual(ulong.MaxValue, row.UInt64NArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Single()
        {
            var str = @"{Single: 123.456, SingleArray: [3.4028E+38, 0, -3.402E+38], SingleNArray: [null, 0, 3.4028]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123.456F, row.Single);

            Assert.IsNotNull(row.SingleArray);
            Assert.AreEqual(3, row.SingleArray.Length);
            Assert.AreEqual(3.4028E+38F, row.SingleArray[0]);
            Assert.AreEqual(0, row.SingleArray[1]);
            Assert.AreEqual(-3.402E+38F, row.SingleArray[2]);

            Assert.IsNotNull(row.SingleNArray);
            Assert.AreEqual(3, row.SingleNArray.Length);
            Assert.AreEqual(null, row.SingleNArray[0]);
            Assert.AreEqual(0, row.SingleNArray[1]);
            Assert.AreEqual(3.4028F, row.SingleNArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Double()
        {
            var str = @"{Double: 123.456, DoubleArray: [1.79769E+308, 0, -1.7976931E+308], DoubleNArray: [null, 0, 3.482347E+38]}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123.456D, row.Double);

            Assert.IsNotNull(row.DoubleArray);
            Assert.AreEqual(3, row.DoubleArray.Length);
            Assert.AreEqual(1.79769E+308, row.DoubleArray[0]);
            Assert.AreEqual(0, row.DoubleArray[1]);
            Assert.AreEqual(-1.7976931E+308, row.DoubleArray[2]);

            Assert.IsNotNull(row.DoubleNArray);
            Assert.AreEqual(3, row.DoubleNArray.Length);
            Assert.AreEqual(null, row.DoubleNArray[0]);
            Assert.AreEqual(0, row.DoubleNArray[1]);
            Assert.AreEqual(3.482347E+38, row.DoubleNArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Guid()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var str = "{" + @"Guid: ""{0}"", GuidArray: [""{0}"", ""{1}"", ""{2}""], GuidNArray: [""{0}"", null, ""{2}""]".Args(guid1, guid2, guid3) + "}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(guid1, row.Guid);

            Assert.IsNotNull(row.GuidArray);
            Assert.AreEqual(3, row.GuidArray.Length);
            Assert.AreEqual(guid1, row.GuidArray[0]);
            Assert.AreEqual(guid2, row.GuidArray[1]);
            Assert.AreEqual(guid3, row.GuidArray[2]);

            Assert.IsNotNull(row.GuidNArray);
            Assert.AreEqual(3, row.GuidNArray.Length);
            Assert.AreEqual(guid1, row.GuidNArray[0]);
            Assert.AreEqual(null, row.GuidNArray[1]);
            Assert.AreEqual(guid3, row.GuidNArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_GDID()
        {
            var gdid1 = new GDID(4000000000, 8000000000);
            var gdid2 = new GDID(100002, 1, 8000000000);
            var gdid3 = new GDID(123, 123456789);
            var str = @"{" + @"GDID: ""{0}"", GDIDArray: [""{0}"", ""{1}"", ""{2}""], GDIDNArray: [""{0}"", null, ""{2}""]".Args(gdid1, gdid2, gdid3) + "}";

            var row = new RowWithArrays();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(gdid1, row.GDID);

            Assert.IsNotNull(row.GDIDArray);
            Assert.AreEqual(3, row.GDIDArray.Length);
            Assert.AreEqual(gdid1, row.GDIDArray[0]);
            Assert.AreEqual(gdid2, row.GDIDArray[1]);
            Assert.AreEqual(gdid3, row.GDIDArray[2]);

            Assert.IsNotNull(row.GDIDNArray);
            Assert.AreEqual(3, row.GDIDNArray.Length);
            Assert.AreEqual(gdid1, row.GDIDNArray[0]);
            Assert.AreEqual(null, row.GDIDNArray[1]);
            Assert.AreEqual(gdid3, row.GDIDNArray[2]);
        }

        [TestCase]
        public void ToTypedRow_Arrays_FromString_Row()
        {
            var str =
            @"{ 
                Row: { Name: ""Ivan"", Int32Array: [1, 0, -12345] }, 
                RowArray: 
                    [
                        { Name: ""John"", Int8: 123 },
                        { Name: ""Anna"", Single: 123.567 }
                    ]
            }";

            var row = new RowWithArrays();
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.IsNotNull(row.Row);
            Assert.AreEqual("Ivan", row.Row.Name);
            Assert.IsNotNull(row.Row.Int32Array);
            Assert.AreEqual(3, row.Row.Int32Array.Length);
            Assert.AreEqual(1, row.Row.Int32Array[0]);
            Assert.AreEqual(0, row.Row.Int32Array[1]);
            Assert.AreEqual(-12345, row.Row.Int32Array[2]);

            Assert.IsNotNull(row.RowArray);
            Assert.AreEqual(2, row.RowArray.Length);
            Assert.IsNotNull(row.RowArray[0]);
            Assert.AreEqual("John", row.RowArray[0].Name);
            Assert.AreEqual(123, row.RowArray[0].Int8);
            Assert.IsNotNull(row.RowArray[1]);
            Assert.AreEqual("Anna", row.RowArray[1].Name);
            Assert.AreEqual(123.567F, row.RowArray[1].Single);
        }

        #endregion Arrays

        #region Lists

        [TestCase]
        public void ToTypedRow_Lists_FromString_Strings()
        {
            var str = @"{name: ""Orlov"", StringList: [""a"", null, ""b""]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual("Orlov", row.Name);

            Assert.IsNotNull(row.StringList);
            Assert.AreEqual(3, row.StringList.Count);

            Assert.AreEqual("a", row.StringList[0]);
            Assert.AreEqual(null, row.StringList[1]);
            Assert.AreEqual("b", row.StringList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Bytes()
        {
            var str = @"{UInt8: 255, UInt8List: [1, 0, 255], UInt8NList: [null, 0, 124]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(255, row.UInt8);

            Assert.IsNotNull(row.UInt8List);
            Assert.AreEqual(3, row.UInt8List.Count);
            Assert.AreEqual(1, row.UInt8List[0]);
            Assert.AreEqual(0, row.UInt8List[1]);
            Assert.AreEqual(255, row.UInt8List[2]);

            Assert.IsNotNull(row.UInt8NList);
            Assert.AreEqual(3, row.UInt8NList.Count);
            Assert.AreEqual(null, row.UInt8NList[0]);
            Assert.AreEqual(0, row.UInt8NList[1]);
            Assert.AreEqual(124, row.UInt8NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_SBytes()
        {
            var str = @"{Int8: -56, Int8List: [-1, 0, 127], Int8NList: [null, 0, 127]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(-56, row.Int8);

            Assert.IsNotNull(row.Int8List);
            Assert.AreEqual(3, row.Int8List.Count);
            Assert.AreEqual(-1, row.Int8List[0]);
            Assert.AreEqual(0, row.Int8List[1]);
            Assert.AreEqual(127, row.Int8List[2]);

            Assert.IsNotNull(row.Int8NList);
            Assert.AreEqual(3, row.Int8NList.Count);
            Assert.AreEqual(null, row.Int8NList[0]);
            Assert.AreEqual(0, row.Int8NList[1]);
            Assert.AreEqual(127, row.Int8NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Shorts()
        {
            var str = @"{Int16: 12345, Int16List: [32767, 0, -32768], Int16NList: [null, 0, 32767]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(12345, row.Int16);

            Assert.IsNotNull(row.Int16List);
            Assert.AreEqual(3, row.Int16List.Count);
            Assert.AreEqual(short.MaxValue, row.Int16List[0]);
            Assert.AreEqual(0, row.Int16List[1]);
            Assert.AreEqual(short.MinValue, row.Int16List[2]);

            Assert.IsNotNull(row.Int16NList);
            Assert.AreEqual(3, row.Int16NList.Count);
            Assert.AreEqual(null, row.Int16NList[0]);
            Assert.AreEqual(0, row.Int16NList[1]);
            Assert.AreEqual(short.MaxValue, row.Int16NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_UShorts()
        {
            var str = @"{UInt16: 123, UInt16List: [65535, 0, 12345], UInt16NList: [null, 0, 65535]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.UInt16);

            Assert.IsNotNull(row.UInt16List);
            Assert.AreEqual(3, row.UInt16List.Count);
            Assert.AreEqual(ushort.MaxValue, row.UInt16List[0]);
            Assert.AreEqual(0, row.UInt16List[1]);
            Assert.AreEqual(12345, row.UInt16List[2]);

            Assert.IsNotNull(row.UInt16NList);
            Assert.AreEqual(3, row.UInt16NList.Count);
            Assert.AreEqual(null, row.UInt16NList[0]);
            Assert.AreEqual(0, row.UInt16NList[1]);
            Assert.AreEqual(ushort.MaxValue, row.UInt16NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Int()
        {
            var str = @"{Int32: 123, Int32List: [2147483647, 0, -2147483648], Int32NList: [null, 0, 2147483647]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.Int32);

            Assert.IsNotNull(row.Int32List);
            Assert.AreEqual(3, row.Int32List.Count);
            Assert.AreEqual(int.MaxValue, row.Int32List[0]);
            Assert.AreEqual(0, row.Int32List[1]);
            Assert.AreEqual(int.MinValue, row.Int32List[2]);

            Assert.IsNotNull(row.Int32NList);
            Assert.AreEqual(3, row.Int32NList.Count);
            Assert.AreEqual(null, row.Int32NList[0]);
            Assert.AreEqual(0, row.Int32NList[1]);
            Assert.AreEqual(int.MaxValue, row.Int32NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_UInt()
        {
            var str = @"{UInt32: 123, UInt32List: [4294967295, 0, 124], UInt32NList: [null, 0, 4294967295]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.UInt32);

            Assert.IsNotNull(row.UInt32List);
            Assert.AreEqual(3, row.UInt32List.Count);
            Assert.AreEqual(uint.MaxValue, row.UInt32List[0]);
            Assert.AreEqual(0, row.UInt32List[1]);
            Assert.AreEqual(124, row.UInt32List[2]);

            Assert.IsNotNull(row.UInt32NList);
            Assert.AreEqual(3, row.UInt32NList.Count);
            Assert.AreEqual(null, row.UInt32NList[0]);
            Assert.AreEqual(0, row.UInt32NList[1]);
            Assert.AreEqual(uint.MaxValue, row.UInt32NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Long()
        {
            var str = @"{Int64: 123, Int64List: [9223372036854775807, 0, -9223372036854775808], Int64NList: [null, 0, 9223372036854775807]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.Int64);

            Assert.IsNotNull(row.Int64List);
            Assert.AreEqual(3, row.Int64List.Count);
            Assert.AreEqual(long.MaxValue, row.Int64List[0]);
            Assert.AreEqual(0, row.Int64List[1]);
            Assert.AreEqual(long.MinValue, row.Int64List[2]);

            Assert.IsNotNull(row.Int64NList);
            Assert.AreEqual(3, row.Int64NList.Count);
            Assert.AreEqual(null, row.Int64NList[0]);
            Assert.AreEqual(0, row.Int64NList[1]);
            Assert.AreEqual(long.MaxValue, row.Int64NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_ULong()
        {
            var str = @"{UInt64: 123, UInt64List: [18446744073709551615, 0, 124], UInt64NList: [null, 0, 18446744073709551615]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.UInt64);

            Assert.IsNotNull(row.UInt64List);
            Assert.AreEqual(3, row.UInt64List.Count);
            Assert.AreEqual(ulong.MaxValue, row.UInt64List[0]);
            Assert.AreEqual(0, row.UInt64List[1]);
            Assert.AreEqual(124, row.UInt64List[2]);

            Assert.IsNotNull(row.UInt64NList);
            Assert.AreEqual(3, row.UInt64NList.Count);
            Assert.AreEqual(null, row.UInt64NList[0]);
            Assert.AreEqual(0, row.UInt64NList[1]);
            Assert.AreEqual(ulong.MaxValue, row.UInt64NList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Single()
        {
            var str = @"{Single: 123.456, SingleList: [3.4028E+38, 0, -3.402E+38], SingleNList: [null, 0, 3.4028]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123.456F, row.Single);

            Assert.IsNotNull(row.SingleList);
            Assert.AreEqual(3, row.SingleList.Count);
            Assert.AreEqual(3.4028E+38F, row.SingleList[0]);
            Assert.AreEqual(0, row.SingleList[1]);
            Assert.AreEqual(-3.402E+38F, row.SingleList[2]);

            Assert.IsNotNull(row.SingleNList);
            Assert.AreEqual(3, row.SingleNList.Count);
            Assert.AreEqual(null, row.SingleNList[0]);
            Assert.AreEqual(0, row.SingleNList[1]);
            Assert.AreEqual(3.4028F, row.SingleNList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Double()
        {
            var str = @"{Double: 123.456, DoubleList: [1.79769E+308, 0, -1.7976931E+308], DoubleNList: [null, 0, 3.482347E+38]}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123.456D, row.Double);

            Assert.IsNotNull(row.DoubleList);
            Assert.AreEqual(3, row.DoubleList.Count);
            Assert.AreEqual(1.79769E+308, row.DoubleList[0]);
            Assert.AreEqual(0, row.DoubleList[1]);
            Assert.AreEqual(-1.7976931E+308, row.DoubleList[2]);

            Assert.IsNotNull(row.DoubleNList);
            Assert.AreEqual(3, row.DoubleNList.Count);
            Assert.AreEqual(null, row.DoubleNList[0]);
            Assert.AreEqual(0, row.DoubleNList[1]);
            Assert.AreEqual(3.482347E+38, row.DoubleNList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Guid()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var str = "{" + @"Guid: ""{0}"", GuidList: [""{0}"", ""{1}"", ""{2}""], GuidNList: [""{0}"", null, ""{2}""]".Args(guid1, guid2, guid3) + "}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(guid1, row.Guid);

            Assert.IsNotNull(row.GuidList);
            Assert.AreEqual(3, row.GuidList.Count);
            Assert.AreEqual(guid1, row.GuidList[0]);
            Assert.AreEqual(guid2, row.GuidList[1]);
            Assert.AreEqual(guid3, row.GuidList[2]);

            Assert.IsNotNull(row.GuidNList);
            Assert.AreEqual(3, row.GuidNList.Count);
            Assert.AreEqual(guid1, row.GuidNList[0]);
            Assert.AreEqual(null, row.GuidNList[1]);
            Assert.AreEqual(guid3, row.GuidNList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_GDID()
        {
            var gdid1 = new GDID(4000000000, 8000000000);
            var gdid2 = new GDID(100002, 1, 8000000000);
            var gdid3 = new GDID(123, 123456789);
            var str = @"{" + @"GDID: ""{0}"", GDIDList: [""{0}"", ""{1}"", ""{2}""], GDIDNList: [""{0}"", null, ""{2}""]".Args(gdid1, gdid2, gdid3) + "}";

            var row = new RowWithLists();

            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(gdid1, row.GDID);

            Assert.IsNotNull(row.GDIDList);
            Assert.AreEqual(3, row.GDIDList.Count);
            Assert.AreEqual(gdid1, row.GDIDList[0]);
            Assert.AreEqual(gdid2, row.GDIDList[1]);
            Assert.AreEqual(gdid3, row.GDIDList[2]);

            Assert.IsNotNull(row.GDIDNList);
            Assert.AreEqual(3, row.GDIDNList.Count);
            Assert.AreEqual(gdid1, row.GDIDNList[0]);
            Assert.AreEqual(null, row.GDIDNList[1]);
            Assert.AreEqual(gdid3, row.GDIDNList[2]);
        }

        [TestCase]
        public void ToTypedRow_Lists_FromString_Row()
        {
            var str =
            @"{ 
                Row: { Name: ""Ivan"", Int32List: [1, 0, -12345] }, 
                RowList: 
                    [
                        { Name: ""John"", Int8: 123 },
                        { Name: ""Anna"", Single: 123.567 }
                    ]
            }";

            var row = new RowWithLists();
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.IsNotNull(row.Row);
            Assert.AreEqual("Ivan", row.Row.Name);
            Assert.IsNotNull(row.Row.Int32List);
            Assert.AreEqual(3, row.Row.Int32List.Count);
            Assert.AreEqual(1, row.Row.Int32List[0]);
            Assert.AreEqual(0, row.Row.Int32List[1]);
            Assert.AreEqual(-12345, row.Row.Int32List[2]);

            Assert.IsNotNull(row.RowList);
            Assert.AreEqual(2, row.RowList.Count);
            Assert.IsNotNull(row.RowList[0]);
            Assert.AreEqual("John", row.RowList[0].Name);
            Assert.AreEqual(123, row.RowList[0].Int8);
            Assert.IsNotNull(row.RowList[1]);
            Assert.AreEqual("Anna", row.RowList[1].Name);
            Assert.AreEqual(123.567F, row.RowList[1].Single);
        }

        #endregion Lists

        #region Amorphous

        [TestCase]
        public void ToAmorphousRow_FromString_Strings()
        {
            var str = @"{ Name_1: ""Orlov"", StringArray_1: [""a"", null, ""b""]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual("Orlov", row.AmorphousData["Name_1"]);

            var array = row.AmorphousData["StringArray_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual("a", array[0]);
            Assert.AreEqual(null, array[1]);
            Assert.AreEqual("b", array[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Bytes()
        {
            var str = @"{UInt8_1: 255, UInt8Array_1: [1, 0, 255], UInt8NArray_1: [null, 0, 124]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(255, row.AmorphousData["UInt8_1"]);

            var array = row.AmorphousData["UInt8Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(255, array[2]);

            var narray = row.AmorphousData["UInt8NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(124, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_SBytes()
        {
            var str = @"{Int8_1: -56, Int8Array_1: [-1, 0, 127], Int8NArray_1: [null, 0, 127]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(-56, row.AmorphousData["Int8_1"]);

            var array = row.AmorphousData["Int8Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(-1, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(127, array[2]);

            var narray = row.AmorphousData["Int8NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(127, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Shorts()
        {
            var str = @"{Int16_1: 12345, Int16Array_1: [32767, 0, -32768], Int16NArray_1: [null, 0, 32767]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(12345, row.AmorphousData["Int16_1"]);

            var array = row.AmorphousData["Int16Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(short.MaxValue, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(short.MinValue, array[2]);

            var narray = row.AmorphousData["Int16NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(short.MaxValue, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_UShorts()
        {
            var str = @"{UInt16_1: 123, UInt16Array_1: [65535, 0, 12345], UInt16NArray_1: [null, 0, 65535]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.AmorphousData["UInt16_1"]);

            var array = row.AmorphousData["UInt16Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(ushort.MaxValue, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(12345, array[2]);

            var narray = row.AmorphousData["UInt16NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(ushort.MaxValue, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Int()
        {
            var str = @"{Int32_1: 123, Int32Array_1: [2147483647, 0, -2147483648], Int32NArray_1: [null, 0, 2147483647]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.AmorphousData["Int32_1"]);

            var array = row.AmorphousData["Int32Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(int.MaxValue, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(int.MinValue, array[2]);

            var narray = row.AmorphousData["Int32NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(int.MaxValue, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_UInt()
        {
            var str = @"{UInt32_1: 123, UInt32Array_1: [4294967295, 0, 124], UInt32NArray_1: [null, 0, 4294967295]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.AmorphousData["UInt32_1"]);

            var array = row.AmorphousData["UInt32Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(uint.MaxValue, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(124, array[2]);

            var narray = row.AmorphousData["UInt32NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(uint.MaxValue, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Long()
        {
            var str = @"{Int64_1: 123, Int64Array_1: [9223372036854775807, 0, -9223372036854775808], Int64NArray_1: [null, 0, 9223372036854775807]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.AmorphousData["Int64_1"]);

            var array = row.AmorphousData["Int64Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(long.MaxValue, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(long.MinValue, array[2]);

            var narray = row.AmorphousData["Int64NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(long.MaxValue, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_ULong()
        {
            var str = @"{UInt64_1: 123, UInt64Array_1: [18446744073709551615, 0, 124], UInt64NArray_1: [null, 0, 18446744073709551615]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123, row.AmorphousData["UInt64_1"]);

            var array = row.AmorphousData["UInt64Array_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(ulong.MaxValue, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(124, array[2]);

            var narray = row.AmorphousData["UInt64NArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(ulong.MaxValue, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Single()
        {
            var str = @"{Single_1: 123.456, SingleArray_1: [3.4028E+38, 0, -3.402E+38], SingleNArray_1: [null, 0, 3.4028]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123.456F, row.AmorphousData["Single_1"].AsFloat());

            var array = row.AmorphousData["SingleArray_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(3.4028E+38F, array[0].AsFloat());
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(-3.402E+38F, array[2].AsFloat());

            var narray = row.AmorphousData["SingleNArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1].AsFloat());
            Assert.AreEqual(3.4028F, narray[2].AsFloat());
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Double()
        {
            var str = @"{Double_1: 123.456, DoubleArray_1: [1.79769E+308, 0, -1.7976931E+308], DoubleNArray_1: [null, 0, 3.482347E+38]}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(123.456D, row.AmorphousData["Double_1"]);

            var array = row.AmorphousData["DoubleArray_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(1.79769E+308, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(-1.7976931E+308, array[2]);

            var narray = row.AmorphousData["DoubleNArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(null, narray[0]);
            Assert.AreEqual(0, narray[1]);
            Assert.AreEqual(3.482347E+38, narray[2]);
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Guid()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var str = "{" + @"Guid_1: ""{0}"", GuidArray_1: [""{0}"", ""{1}"", ""{2}""], GuidNArray_1: [""{0}"", null, ""{2}""]".Args(guid1, guid2, guid3) + "}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(guid1, row.AmorphousData["Guid_1"].AsGUID(Guid.Empty));

            var array = row.AmorphousData["GuidArray_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(guid1, array[0].AsGUID(Guid.Empty));
            Assert.AreEqual(guid2, array[1].AsGUID(Guid.Empty));
            Assert.AreEqual(guid3, array[2].AsGUID(Guid.Empty));

            var narray = row.AmorphousData["GuidNArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(guid1, narray[0].AsGUID(Guid.Empty));
            Assert.AreEqual(null, narray[1].AsNullableGUID());
            Assert.AreEqual(guid3, narray[2].AsGUID(Guid.Empty));
        }

        [TestCase]
        public void ToAmorphousRow_FromString_GDID()
        {
            var gdid1 = new GDID(4000000000, 8000000000);
            var gdid2 = new GDID(100002, 1, 8000000000);
            var gdid3 = new GDID(123, 123456789);
            var str = @"{" + @"GDID_1: ""{0}"", GDIDArray_1: [""{0}"", ""{1}"", ""{2}""], GDIDNArray_1: [""{0}"", null, ""{2}""]".Args(gdid1, gdid2, gdid3) + "}";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            Assert.AreEqual(gdid1, row.AmorphousData["GDID_1"].AsGDID());

            var array = row.AmorphousData["GDIDArray_1"] as JSONDataArray;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.AreEqual(gdid1, array[0].AsGDID());
            Assert.AreEqual(gdid2, array[1].AsGDID());
            Assert.AreEqual(gdid3, array[2].AsGDID());

            var narray = row.AmorphousData["GDIDNArray_1"] as JSONDataArray;
            Assert.IsNotNull(narray);
            Assert.AreEqual(3, narray.Count);
            Assert.AreEqual(gdid1, narray[0].AsGDID());
            Assert.AreEqual(null, narray[1]);
            Assert.AreEqual(gdid3, narray[2].AsGDID());
        }

        [TestCase]
        public void ToAmorphousRow_FromString_Row()
        {
            var str =
            @"{ 
                Row_1: { Name: ""Ivan"", Int32Array: [1, 0, -12345] }, 
                RowArray_1: 
                    [
                        { Name: ""John"", Int8: 123 },
                        { Name: ""Anna"", Single: 123.567 }
                    ]
            }";

            var row = new AmorphousRow(Schema.GetForTypedRow(typeof(RowWithArrays)));
            JSONReader.ToRow(row, str.JSONToDataObject() as JSONDataMap);

            var innerRow = row.AmorphousData["Row_1"] as JSONDataMap;
            Assert.IsNotNull(innerRow);
            Assert.AreEqual("Ivan", innerRow["Name"]);

            var innerRowArray = innerRow["Int32Array"] as JSONDataArray;
            Assert.IsNotNull(innerRowArray);
            Assert.AreEqual(3, innerRowArray.Count);
            Assert.AreEqual(1, innerRowArray[0]);
            Assert.AreEqual(0, innerRowArray[1]);
            Assert.AreEqual(-12345, innerRowArray[2]);

            var innerArray = row.AmorphousData["RowArray_1"] as JSONDataArray;
            Assert.IsNotNull(innerArray);
            Assert.AreEqual(2, innerArray.Count);

            var innerArrayRow0 = innerArray[0] as JSONDataMap;
            Assert.IsNotNull(innerArrayRow0);
            Assert.AreEqual("John", innerArrayRow0["Name"]);
            Assert.AreEqual(123, innerArrayRow0["Int8"]);

            var innerArrayRow1 = innerArray[1] as JSONDataMap;
            Assert.IsNotNull(innerArrayRow1);
            Assert.AreEqual("Anna", innerArrayRow1["Name"]);
            Assert.AreEqual(123.567F, innerArrayRow1["Single"].AsFloat());
        }

        #endregion Amorphous

        #region Types

        public class RowWithArrays : TypedRow
        {
            [Field]
            public string Name { get; set; }
            [Field]
            public string[] StringArray { get; set; }

            [Field]
            public sbyte Int8 { get; set; }
            [Field]
            public sbyte[] Int8Array { get; set; }
            [Field]
            public sbyte?[] Int8NArray { get; set; }

            [Field]
            public byte UInt8 { get; set; }
            [Field]
            public byte[] UInt8Array { get; set; }
            [Field]
            public byte?[] UInt8NArray { get; set; }

            [Field]
            public short Int16 { get; set; }
            [Field]
            public short[] Int16Array { get; set; }
            [Field]
            public short?[] Int16NArray { get; set; }

            [Field]
            public ushort UInt16 { get; set; }
            [Field]
            public ushort[] UInt16Array { get; set; }
            [Field]
            public ushort?[] UInt16NArray { get; set; }

            [Field]
            public int Int32 { get; set; }
            [Field]
            public int[] Int32Array { get; set; }
            [Field]
            public int?[] Int32NArray { get; set; }

            [Field]
            public uint UInt32 { get; set; }
            [Field]
            public uint[] UInt32Array { get; set; }
            [Field]
            public uint?[] UInt32NArray { get; set; }

            [Field]
            public long Int64 { get; set; }
            [Field]
            public long[] Int64Array { get; set; }
            [Field]
            public long?[] Int64NArray { get; set; }

            [Field]
            public ulong UInt64 { get; set; }
            [Field]
            public ulong[] UInt64Array { get; set; }
            [Field]
            public ulong?[] UInt64NArray { get; set; }

            [Field]
            public float Single { get; set; }
            [Field]
            public float[] SingleArray { get; set; }
            [Field]
            public float?[] SingleNArray { get; set; }

            [Field]
            public double Double { get; set; }
            [Field]
            public double[] DoubleArray { get; set; }
            [Field]
            public double?[] DoubleNArray { get; set; }

            [Field]
            public Guid Guid { get; set; }
            [Field]
            public Guid[] GuidArray { get; set; }
            [Field]
            public Guid?[] GuidNArray { get; set; }

            [Field]
            public GDID GDID { get; set; }
            [Field]
            public GDID[] GDIDArray { get; set; }
            [Field]
            public GDID?[] GDIDNArray { get; set; }

            [Field]
            public RowWithArrays Row { get; set; }
            [Field]
            public RowWithArrays[] RowArray { get; set; }
        }

        public class RowWithLists : TypedRow
        {
            [Field]
            public string Name { get; set; }
            [Field]
            public List<string> StringList { get; set; }

            [Field]
            public sbyte Int8 { get; set; }
            [Field]
            public List<sbyte> Int8List { get; set; }
            [Field]
            public List<sbyte?> Int8NList { get; set; }

            [Field]
            public byte UInt8 { get; set; }
            [Field]
            public List<byte> UInt8List { get; set; }
            [Field]
            public List<byte?> UInt8NList { get; set; }

            [Field]
            public short Int16 { get; set; }
            [Field]
            public List<short> Int16List { get; set; }
            [Field]
            public List<short?> Int16NList { get; set; }

            [Field]
            public ushort UInt16 { get; set; }
            [Field]
            public List<ushort> UInt16List { get; set; }
            [Field]
            public List<ushort?> UInt16NList { get; set; }

            [Field]
            public int Int32 { get; set; }
            [Field]
            public List<int> Int32List { get; set; }
            [Field]
            public List<int?> Int32NList { get; set; }

            [Field]
            public uint UInt32 { get; set; }
            [Field]
            public List<uint> UInt32List { get; set; }
            [Field]
            public List<uint?> UInt32NList { get; set; }

            [Field]
            public long Int64 { get; set; }
            [Field]
            public List<long> Int64List { get; set; }
            [Field]
            public List<long?> Int64NList { get; set; }

            [Field]
            public ulong UInt64 { get; set; }
            [Field]
            public List<ulong> UInt64List { get; set; }
            [Field]
            public List<ulong?> UInt64NList { get; set; }

            [Field]
            public float Single { get; set; }
            [Field]
            public List<float> SingleList { get; set; }
            [Field]
            public List<float?> SingleNList { get; set; }

            [Field]
            public double Double { get; set; }
            [Field]
            public List<double> DoubleList { get; set; }
            [Field]
            public List<double?> DoubleNList { get; set; }

            [Field]
            public Guid Guid { get; set; }
            [Field]
            public List<Guid> GuidList { get; set; }
            [Field]
            public List<Guid?> GuidNList { get; set; }

            [Field]
            public GDID GDID { get; set; }
            [Field]
            public List<GDID> GDIDList { get; set; }
            [Field]
            public List<GDID?> GDIDNList { get; set; }

            [Field]
            public RowWithLists Row { get; set; }
            [Field]
            public List<RowWithLists> RowList { get; set; }
        }

        public class AmorphousRow : AmorphousDynamicRow
        {
            public AmorphousRow(Schema schema) : base(schema)
            {
            }
        }

        #endregion Types
    }
}
