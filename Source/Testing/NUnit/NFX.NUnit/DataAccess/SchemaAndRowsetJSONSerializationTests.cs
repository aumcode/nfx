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

using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.Serialization.Slim;
using NFX.Serialization.JSON;

namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class SchemaAndRowsetSerializationTests
    {
       //Pokryt:
       //RowsetBase(string/map) 

      [TestCase]
      public void Schema_FromJSON()
      {
        var src = new TestRow().Schema;
        var json = src.ToJSON();
        var trg = Schema.FromJSON(json, true);

        Assert.IsTrue(trg.Name.StartsWith("JSON"));
        Assert.IsTrue(trg.ReadOnly);
        Assert.AreEqual(trg.FieldDefs.Count(), src.FieldDefs.Count());

        Assert.AreEqual(trg["BoolField"].Type, typeof(Boolean?));
        Assert.AreEqual(trg["CharField"].Type, typeof(Char?));
        Assert.AreEqual(trg["DateTimeField"].Type, typeof(DateTime?));
        Assert.AreEqual(trg["GDIDField"].Type, typeof(string));

        Assert.AreEqual(trg["ByteField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["SByteField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["ShortField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["Int16Field"].Type, typeof(Int32?));
        Assert.AreEqual(trg["UShortField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["UInt16Field"].Type, typeof(Int32?));
        Assert.AreEqual(trg["IntField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["Int32Field"].Type, typeof(Int32?));

        Assert.AreEqual(trg["UIntField"].Type, typeof(Int64?));
        Assert.AreEqual(trg["UInt32Field"].Type, typeof(Int64?));
        Assert.AreEqual(trg["LongField"].Type, typeof(Int64?));
        Assert.AreEqual(trg["Int64Field"].Type, typeof(Int64?));

        Assert.AreEqual(trg["ULongField"].Type, typeof(UInt64?));
        Assert.AreEqual(trg["UInt64Field"].Type, typeof(UInt64?));

        Assert.AreEqual(trg["FloatField"].Type, typeof(Double?));
        Assert.AreEqual(trg["SingleField"].Type, typeof(Double?));
        Assert.AreEqual(trg["DoubleField"].Type, typeof(Double?));

        Assert.AreEqual(trg["DecimalField"].Type, typeof(Decimal?));

        Assert.AreEqual(trg["FieldWithAttrs"].Attrs, src["FieldWithAttrs"].Attrs);
      }

      [TestCase]
      public void Schema_FromMap()
      {
        var src = new TestRow().Schema;
        var json = src.ToJSON();
        var trg = Schema.FromJSON(JSONReader.DeserializeDataObject(json) as JSONDataMap, true);

        Assert.IsTrue(trg.Name.StartsWith("JSON"));
        Assert.IsTrue(trg.ReadOnly);
        Assert.AreEqual(trg.FieldDefs.Count(), src.FieldDefs.Count());

        Assert.AreEqual(trg["BoolField"].Type, typeof(Boolean?));
        Assert.AreEqual(trg["CharField"].Type, typeof(Char?));
        Assert.AreEqual(trg["DateTimeField"].Type, typeof(DateTime?));
        Assert.AreEqual(trg["GDIDField"].Type, typeof(string));

        Assert.AreEqual(trg["ByteField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["SByteField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["ShortField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["Int16Field"].Type, typeof(Int32?));
        Assert.AreEqual(trg["UShortField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["UInt16Field"].Type, typeof(Int32?));
        Assert.AreEqual(trg["IntField"].Type, typeof(Int32?));
        Assert.AreEqual(trg["Int32Field"].Type, typeof(Int32?));

        Assert.AreEqual(trg["UIntField"].Type, typeof(Int64?));
        Assert.AreEqual(trg["UInt32Field"].Type, typeof(Int64?));
        Assert.AreEqual(trg["LongField"].Type, typeof(Int64?));
        Assert.AreEqual(trg["Int64Field"].Type, typeof(Int64?));

        Assert.AreEqual(trg["ULongField"].Type, typeof(UInt64?));
        Assert.AreEqual(trg["UInt64Field"].Type, typeof(UInt64?));

        Assert.AreEqual(trg["FloatField"].Type, typeof(Double?));
        Assert.AreEqual(trg["SingleField"].Type, typeof(Double?));
        Assert.AreEqual(trg["DoubleField"].Type, typeof(Double?));

        Assert.AreEqual(trg["DecimalField"].Type, typeof(Decimal?));

        Assert.AreEqual(trg["FieldWithAttrs"].Attrs, src["FieldWithAttrs"].Attrs);
      }

      private class TestRow : TypedRow
      {
        [Field] public bool BoolField { get; set;}
        [Field] public char CharField { get; set;}
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

        [Field(required:true, visible: true, key: true)] public bool FieldWithAttrs { get; set; }
      }
    }
}
