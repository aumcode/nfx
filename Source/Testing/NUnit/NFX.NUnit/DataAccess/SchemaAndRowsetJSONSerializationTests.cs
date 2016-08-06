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

using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.Serialization.JSON;


namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class SchemaAndRowsetSerializationTests
    {
      [TestCase(true)]
      [TestCase(false)]
      public void Schema_FromJSON(bool readOnly)
      {
        var src = new TeztRow().Schema;
        var json = src.ToJSON();

        var trg = Schema.FromJSON(json, readOnly);

        Assert.AreEqual(trg.ReadOnly, readOnly);
        schemaAssertions(trg, src);
      }

      [Test]
      public void Rowset_FromJSON_ShemaOnly()
      {
        var src = new Rowset(new TeztRow().Schema);
        var options = new NFX.Serialization.JSON.JSONWritingOptions
                                  {
                                    RowsetMetadata = true,
                                    SpaceSymbols = true,
                                    IndentWidth = 2,
                                    MemberLineBreak = true,
                                    ObjectLineBreak = true
                                  };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json, true);

        schemaAssertions(trg.Schema, src.Schema);
        Assert.AreEqual(trg.Count, 0);
      }

      [Test]
      public void Table_FromJSON_ShemaOnly()
      {
        var src = new Table(new TeztRow().Schema);
        var options = new NFX.Serialization.JSON.JSONWritingOptions
                                  {
                                    RowsetMetadata = true,
                                    SpaceSymbols = true,
                                    IndentWidth = 2,
                                    MemberLineBreak = true,
                                    ObjectLineBreak = true
                                  };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json, true);

        schemaAssertions(trg.Schema, src.Schema);
        Assert.AreEqual(trg.Count, 0);
      }

      [TestCase(true)]
      [TestCase(false)]
      public void Rowset_FromJSON(bool rowsAsMap)
      {
        var row = new TeztRow();
        var src = new Rowset(row.Schema);

        row.BoolField = true;
        row.CharField = 'a';
        row.StringField = "aaa";
        row.DateTimeField = new DateTime(2016, 1, 2);
        row.GDIDField = new GDID(1, 2, 3);

        row.ByteField = 100;
        row.ShortField = -100;
        row.IntField = -999;

        row.UIntField = 254869;
        row.LongField = -267392;

        row.FloatField = 32768.32768F;
        row.DoubleField = -1048576.1048576D;

        row.DecimalField = 1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {-1, 0, 1};
        row.ListString = new List<string> {"one", "two", "three"};
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "John", Age = 20 };

        src.Add(row);

        row.BoolField = false;
        row.CharField = 'b';
        row.StringField = "bbb";
        row.DateTimeField = new DateTime(2016, 2, 1);
        row.GDIDField = new GDID(4, 5, 6);

        row.ByteField = 101;
        row.ShortField = 100;
        row.IntField = 999;

        row.UIntField = 109876;
        row.LongField = 267392;

        row.FloatField = -32768.32768F;
        row.DoubleField = -048576.1048576D;

        row.DecimalField = -1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {1, 0, -1};
        row.ListString = new List<string> { "three","two", "one" };
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {0, "zero"},
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "Ann", Age = 19 };

        src.Add(row);

        var options = new NFX.Serialization.JSON.JSONWritingOptions
                          {
                            RowsetMetadata = true,
                            SpaceSymbols = true,
                            IndentWidth = 2,
                            MemberLineBreak = true,
                            ObjectLineBreak = true,
                            RowsAsMap = rowsAsMap 
                          };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json);

        schemaAssertions(trg.Schema, src.Schema);
        rowsAssertions(src, trg, rowsAsMap);
      }

      [TestCase(true)]
      [TestCase(false)]
      public void Table_FromJSON(bool rowsAsMap)
      {
        var row = new TeztRow();
        var src = new Table(row.Schema);

        row.BoolField = true;
        row.CharField = 'a';
        row.StringField = "aaa";
        row.DateTimeField = new DateTime(2016, 1, 2);
        row.GDIDField = new GDID(1, 2, 3);

        row.ByteField = 100;
        row.ShortField = -100;
        row.IntField = -999;

        row.UIntField = 254869;
        row.LongField = -267392;

        row.FloatField = 32768.32768F;
        row.DoubleField = -1048576.1048576D;

        row.DecimalField = 1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {-1, 0, 1};
        row.ListString = new List<string> {"one", "two", "three"};
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "John", Age = 20 };

        src.Add(row);

        row.BoolField = false;
        row.CharField = 'b';
        row.StringField = "bbb";
        row.DateTimeField = new DateTime(2016, 2, 1);
        row.GDIDField = new GDID(4, 5, 6);

        row.ByteField = 101;
        row.ShortField = 100;
        row.IntField = 999;

        row.UIntField = 109876;
        row.LongField = 267392;

        row.FloatField = -32768.32768F;
        row.DoubleField = -048576.1048576D;

        row.DecimalField = -1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {1, 0, -1};
        row.ListString = new List<string> { "three","two", "one" };
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {0, "zero"},
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "Ann", Age = 19 };

        src.Add(row);

        var options = new NFX.Serialization.JSON.JSONWritingOptions
                                  {
                                    RowsetMetadata = true,
                                    SpaceSymbols = true,
                                    IndentWidth = 2,
                                    MemberLineBreak = true,
                                    ObjectLineBreak = true,
                                    RowsAsMap = rowsAsMap 
                                  };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json);

        schemaAssertions(trg.Schema, src.Schema);
        rowsAssertions(src, trg, rowsAsMap);
      }

      [TestCase(true)]
      [TestCase(false)]
      public void Rowset_FromJSON_FieldMissed(bool rowsAsMap)
      {
        var row = new Person { Name = "Henry", Age = 43 };
        var rowSet = new Rowset(row.Schema);
        rowSet.Add(row);
        var options = new NFX.Serialization.JSON.JSONWritingOptions
                          {
                            RowsetMetadata = true,
                            RowsAsMap = rowsAsMap 
                          };
        var json = rowSet.ToJSON(options);
        var map = JSONReader.DeserializeDataObject( json ) as JSONDataMap;
        var rows = (map["Rows"] as IList<object>);
        if (rowsAsMap)
        {
          var pers = rows[0] as IDictionary<string, object>;
          pers.Remove("Age");
        }
        else
        {
          var pers = rows[0] as IList<object>;
          pers.RemoveAt(1);
        }

        bool allMatched;
        var trg = RowsetBase.FromJSON(map, out allMatched);

        Assert.IsFalse(allMatched);
        var trgRow = trg[0];
        Assert.AreEqual(trgRow.Schema.FieldCount, 2);
        Assert.AreEqual(trgRow["Name"], "Henry");
        Assert.IsNull(trgRow["Age"]);
      }

      [TestCase(true)]
      [TestCase(false)]
      public void Rowset_FromJSON_DefMissed(bool rowsAsMap)
      {
        var row = new Person { Name = "Henry", Age = 43 };
        var rowSet = new Rowset(row.Schema);
        rowSet.Add(row);
        var options = new NFX.Serialization.JSON.JSONWritingOptions
                          {
                            RowsetMetadata = true,
                            RowsAsMap = rowsAsMap 
                          };
        var json = rowSet.ToJSON(options);
        var map = JSONReader.DeserializeDataObject( json ) as JSONDataMap;
        var schema = (map["Schema"] as IDictionary<string, object>);
        var defs = schema["FieldDefs"] as IList<object>;
        defs.RemoveAt(1);

        bool allMatched;
        var trg = RowsetBase.FromJSON(map, out allMatched);

        Assert.IsFalse(allMatched);
        var trgRow = trg[0];
        Assert.AreEqual(trgRow.Schema.FieldCount, 1);
        Assert.AreEqual(trgRow["Name"], "Henry");
      }

      private void rowsAssertions(RowsetBase src, RowsetBase trg, bool rowsAsMap)
      {
        Assert.AreEqual(trg.Count, src.Count);
        for (var j = 0; j < src.Count; j++)
        {
          var trgRow = trg[j];
          var srcRow = src[j] as TeztRow;
          Assert.AreEqual(trgRow["BoolField"].AsBool(), srcRow.BoolField);
          Assert.AreEqual(trgRow["CharField"].AsString(), srcRow.CharField.ToString());
          Assert.AreEqual(trgRow["StringField"].AsString(), srcRow.StringField);
          Assert.AreEqual(trgRow["DateTimeField"].AsDateTime(), srcRow.DateTimeField);
          Assert.AreEqual(trgRow["GDIDField"].AsGDID(), srcRow.GDIDField);

          Assert.AreEqual(trgRow["ByteField"].AsByte(), srcRow.ByteField);
          Assert.AreEqual(trgRow["ShortField"].AsShort(), srcRow.ShortField);
          Assert.AreEqual(trgRow["IntField"].AsInt(), srcRow.IntField);

          Assert.AreEqual(trgRow["UIntField"].AsUInt(), srcRow.UIntField);
          Assert.AreEqual(trgRow["LongField"].AsLong(), srcRow.LongField);

          Assert.AreEqual(trgRow["FloatField"].AsFloat(), srcRow.FloatField);
          Assert.AreEqual(trgRow["DoubleField"].AsDouble(), srcRow.DoubleField);
          Assert.AreEqual(trgRow["DecimalField"].AsDecimal(), srcRow.DecimalField);

          Assert.AreEqual(trgRow["NullableField"].AsNullableInt(), srcRow.NullableField);

          var array = trgRow["ArrayInt"] as IList<object>;
          Assert.IsNotNull(array);
          Assert.AreEqual(array.Count, srcRow.ArrayInt.Length);
          for (var i = 0; i < array.Count; i++)
            Assert.AreEqual(array[i].AsInt(), srcRow.ArrayInt[i]);

          var list = trgRow["ListString"] as IList<object>;
          Assert.IsNotNull(list);
          Assert.IsTrue(list.SequenceEqual(srcRow.ListString));

          var dict = trgRow["DictionaryIntStr"] as IDictionary<string, object>;
          Assert.IsNotNull(dict);
          Assert.AreEqual(dict.Count, srcRow.DictionaryIntStr.Count);
          foreach (var kvp in srcRow.DictionaryIntStr)
            Assert.AreEqual(dict[kvp.Key.ToString()].ToString(), kvp.Value);

          if (rowsAsMap)
          {
            var pers = trgRow["RowField"] as IDictionary<string, object>;
            Assert.IsNotNull(pers);
            Assert.AreEqual(pers.Count, 2);
            Assert.AreEqual(pers["Name"].AsString(), srcRow.RowField.Name); 
            Assert.AreEqual(pers["Age"].AsInt(), srcRow.RowField.Age); 
          }
          else
          {
            var pers = trgRow["RowField"] as IList<object>;
            Assert.IsNotNull(pers);
            Assert.AreEqual(pers.Count, 2);
            Assert.AreEqual(pers[0].AsString(), srcRow.RowField.Name); 
            Assert.AreEqual(pers[1].AsInt(), srcRow.RowField.Age); 
          }
        }
      }

      private void schemaAssertions(Schema trg, Schema src)
      {
        Assert.AreEqual(trg.FieldCount, src.FieldCount);

        Assert.AreEqual(trg["BoolField"].Type, typeof(bool));
        Assert.AreEqual(trg["CharField"].Type, typeof(string));
        Assert.AreEqual(trg["StringField"].Type, typeof(string));
        Assert.AreEqual(trg["DateTimeField"].Type, typeof(DateTime));
        Assert.AreEqual(trg["GDIDField"].Type, typeof(object));

        Assert.AreEqual(trg["ByteField"].Type, typeof(uint));
        Assert.AreEqual(trg["UShortField"].Type, typeof(uint));
        Assert.AreEqual(trg["UInt16Field"].Type, typeof(uint));
        Assert.AreEqual(trg["UIntField"].Type, typeof(uint));
        Assert.AreEqual(trg["UInt32Field"].Type, typeof(uint));

        Assert.AreEqual(trg["SByteField"].Type, typeof(int));
        Assert.AreEqual(trg["ShortField"].Type, typeof(int));
        Assert.AreEqual(trg["Int16Field"].Type, typeof(int));
        Assert.AreEqual(trg["IntField"].Type, typeof(int));
        Assert.AreEqual(trg["Int32Field"].Type, typeof(int));

        Assert.AreEqual(trg["ULongField"].Type, typeof(ulong));
        Assert.AreEqual(trg["UInt64Field"].Type, typeof(ulong));

        Assert.AreEqual(trg["LongField"].Type, typeof(long));
        Assert.AreEqual(trg["Int64Field"].Type, typeof(long));

        Assert.AreEqual(trg["FloatField"].Type, typeof(double));
        Assert.AreEqual(trg["SingleField"].Type, typeof(double));
        Assert.AreEqual(trg["DoubleField"].Type, typeof(double));

        Assert.AreEqual(trg["DecimalField"].Type, typeof(decimal));

        Assert.AreEqual(trg["NullableField"].Type, typeof(int?));

        Assert.AreEqual(trg["ArrayInt"].Type, typeof(List<object>));
        Assert.AreEqual(trg["ListString"].Type, typeof(List<object>));
        Assert.AreEqual(trg["DictionaryIntStr"].Type, typeof(Dictionary<string, object>));

        Assert.AreEqual(trg["RowField"].Type, typeof(object));

        Assert.AreEqual(trg["FieldWithAttrs1"].Attrs, src["FieldWithAttrs1"].Attrs);
        Assert.AreEqual(trg["FieldWithAttrs2"].Attrs, src["FieldWithAttrs2"].Attrs);
      }

      public class Person : TypedRow
      {
        [Field] public string Name { get; set; }
        [Field] public int Age { get; set; }
      }

      private class TeztRow : TypedRow
      {
        [Field] public bool BoolField { get; set;}
        [Field] public char CharField { get; set;}
        [Field] public string StringField { get; set;}
        [Field] public DateTime DateTimeField { get; set;}
        [Field] public GDID GDIDField { get; set;}

        [Field] public byte ByteField { get; set;}
        [Field] public sbyte SByteField { get; set; }
        [Field] public short ShortField { get; set; }
        [Field] public Int16 Int16Field { get; set; }
        [Field] public ushort UShortField { get; set; }
        [Field] public UInt16 UInt16Field { get; set; }
        [Field] public int IntField { get; set; }
        [Field] public Int32 Int32Field { get; set; }

        [Field] public uint UIntField { get; set; }
        [Field] public UInt32 UInt32Field { get; set; }
        [Field] public long LongField { get; set; }
        [Field] public Int64 Int64Field { get; set; }

        [Field] public ulong ULongField { get; set; }
        [Field] public UInt64 UInt64Field { get; set; }

        [Field] public float FloatField { get; set; }
        [Field] public Single SingleField { get; set; }
        [Field] public double DoubleField { get; set; }

        [Field] public decimal DecimalField { get; set; }

        [Field] public int? NullableField { get; set; }

        [Field] public int[] ArrayInt { get; set; }
        [Field] public List<string> ListString { get; set; }
        [Field] public Dictionary<int, string> DictionaryIntStr { get; set; }

        [Field] public Person RowField { get; set; }

        [Field(required:true, visible: true, key: true, description: "FieldWithAttrs1", valueList: "-1;0;1", kind: DataKind.Number)]
        public int FieldWithAttrs1 { get; set; }
        
        [Field(required:false, visible: false, key: false, minLength: 3, maxLength: 20, charCase: CharCase.Upper)]
        public string FieldWithAttrs2 { get; set; }
      }
    }
}
