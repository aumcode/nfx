using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using NFX.Serialization.BSON;
using NFX.DataAccess.CRUD;
using NFX.Serialization.JSON;
using NFX.DataAccess.Distributed;
using NFX.Financial;

namespace NFX.NUnit.Serialization
{
  [TestFixture]
  public class RowConverterTest
  {
    #region Tests

      [Test]
      public void T_00_Enum_Equals()
      {
        var row1 = new EnumRow
        {
          ETest1 = ETest.One,
          EFlags1 = EFlags.FirstSecond
        };

        var rc = new RowConverter();

        var docOriginal = rc.RowToBSONDocument(row1, "A");
        var doc = fullCopy(docOriginal);

        Console.WriteLine(doc.ToString());

        var row2 = new EnumRow();
        rc.BSONDocumentToRow(doc, row2, "A");

        Assert.AreEqual(row1, row2);

        Assert.AreEqual(ETest.One, row2.ETest1);
        Assert.AreEqual(EFlags.FirstSecond, row2.EFlags1);
      }

      [Test]
      public void T_01_Equals()
      {
        var BIN = new byte[] { 0x00, 0x79, 0x14 };

        var row = new RowA
        {
          String1 = "Mudaker",
          String2 = null,
          Date1 = new DateTime(1980, 07, 12, 10, 13, 27, DateTimeKind.Utc),
          Date2 = null,
          Bool1 = true,
          Bool2 = null,
          Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"),
          Guid2 = null,
          Gdid1 = new GDID(0, 12345),
          Gdid2 = null,
          Float1 = 127.0123f,
          Float2 = null,
          Double1 = 122345.012d,
          Double2 = null,
          Decimal1 = 1234567.098M,
          Decimal2 = null,
          Amount1 = new Amount("din", 123.11M),
          Amount2 = null,
          Bytes1 = BIN,
          Bytes2 = null,

          Byte1 = 23,
          SByte1 = -3,
          Short1 = -32761,
          UShort1 = 65535,
          Int1 = 4324,
          Uint1 = 42345,
          Long1 = 993,
          ULong1 = 8829383762,
          ETest1 = ETest.Two,
          EFlags1 = EFlags.First | EFlags.Third,

          Byte2 = null,
          SByte2 = null,
          Short2 = null,
          UShort2 = null,
          Int2 = null,
          Uint2 = null,
          Long2 = null,
          ULong2 = null,
          ETest2 = null,
          EFlags2 = null
        };

        var rc = new RowConverter();

        var docOriginal = rc.RowToBSONDocument(row, "A");
        var doc = fullCopy(docOriginal);

        Console.WriteLine(doc.ToString());

        Assert.IsTrue(BIN.SequenceEqual( ((BSONBinaryElement)doc["Bytes1"]).Value.Data ));
        //Assert.IsTrue(doc["Bytes2"] is global::MongoDB.Bson.BsonNull);

        var row2 = new RowA();
        rc.BSONDocumentToRow(doc, row2, "A");

        Assert.IsTrue(row.Equals(row2));
        Assert.IsTrue(BIN.SequenceEqual(row2.Bytes1));
        Assert.IsNull(row2.Bytes2);
        Assert.IsFalse(object.ReferenceEquals(BIN, row2.Bytes1));
      }

      [Test]
      public void T_02_Manual()
      {
        var BYTES1 = new byte[] {0x00, 0x79, 0x14};

        var row = new RowA
        {
          String1 = "Mudaker", String2 = null,
          Date1 = new DateTime(1980, 07, 12, 10, 10, 10, DateTimeKind.Utc), Date2 = null,
          Bool1 = true, Bool2 = null,
          Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"), Guid2 = null,
          Gdid1 = new GDID(0, 12345), Gdid2 = null,
          Float1 = 127.0123f, Float2 = null,
          Double1 = 122345.012d, Double2 = null,
          Decimal1 = 1234567.098M, Decimal2 = null,
          Amount1 = new Amount("din", 123.11M), Amount2 = null,
          Bytes1 = BYTES1, Bytes2 = null,
          ETest1 = 0, EFlags1 = 0, ETest2 = null, EFlags2 = null 
        };
        
        var rc = new RowConverter();
      
        var docOriginal = rc.RowToBSONDocument( row, "A" );
      
        var doc = fullCopy( docOriginal );

        Console.WriteLine(doc.ToString());

        var row2 = new RowA();
        rc.BSONDocumentToRow(doc, row2, "A");

        Assert.AreEqual("Mudaker", row2.String1);
        Assert.IsNull( row2.String2);
        Assert.IsTrue( row2.Bool1 );
        Assert.IsNull( row2.Bool2);
        Assert.AreEqual(new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"), row2.Guid1);
        Assert.IsNull( row2.Guid2);
        Assert.AreEqual(new GDID(0, 12345), row2.Gdid1);
        Assert.IsNull( row2.Gdid2);
        Assert.AreEqual(127.0123f, row2.Float1);
        Assert.IsNull( row2.Float2);
        Assert.AreEqual(122345.012d, row2.Double1);
        Assert.IsNull( row2.Double2);
        Assert.AreEqual(1234567.098M, row2.Decimal1);
        Assert.IsNull( row2.Decimal2);
        Assert.AreEqual(new Amount("din", 123.11M), row2.Amount1);
        Assert.IsNull( row2.Amount2);
        Assert.NotNull( row2.Bytes1);
        Assert.IsTrue( BYTES1.SequenceEqual( row2.Bytes1) );
        Assert.IsNull( row2.Bytes2);

        Assert.AreEqual(0, (int)row2.ETest1);
        Assert.AreEqual(0, (int)row2.EFlags1);
        Assert.IsNull(row2.ETest2);
        Assert.IsNull(row2.EFlags2);
      }

      [Test]
      public void T_03_Manual_wo_NULLs()
      {
        var BYTES1 = new byte[] {};
        var BYTES2 = new byte[] {0x00, 0x79, 0x14};

        var row = new RowA
        {
          String1 = "Mudaker", String2 = "Kapernik",
          Date1 = new DateTime(1980, 07, 12, 1, 2, 3, DateTimeKind.Utc), Date2 = new DateTime(1680, 12, 11, 5, 6, 7, DateTimeKind.Utc),
          Bool1 = false, Bool2 = true,
          Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"), Guid2 = new Guid("{BABACACA-FE21-4BB2-B006-2496F4E24D14}"),
          Gdid1 = new GDID(3, 12345), Gdid2 = new GDID(4, 1212345),
          Float1 = 127.0123f, Float2 = -0.123f,
          Double1 = 122345.012d, Double2 = -12345.11f,
          Decimal1 = 1234567.098M, Decimal2 = 22m,
          Amount1 = new Amount("din", 123.11M), Amount2 = new Amount("din", 8901234567890.012M),
          Bytes1 = BYTES1, Bytes2 = BYTES2,
          ETest1 = ETest.One, EFlags1 = EFlags.First,
          ETest2 = ETest.Two, EFlags2 = EFlags.Second | EFlags.Third
        };

        var rc = new RowConverter();
      
        var doc = rc.RowToBSONDocument( row, "A" );

        Console.WriteLine(doc.ToString());

        var row2 = new RowA();
        rc.BSONDocumentToRow(doc, row2, "A");

        Assert.AreEqual("Mudaker", row2.String1);
        Assert.AreEqual("Kapernik", row2.String2);
        Assert.IsFalse( row2.Bool1 );
        Assert.IsTrue( row2.Bool2.Value );
        Assert.AreEqual(new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"), row2.Guid1);
        Assert.AreEqual(new Guid("{BABACACA-FE21-4BB2-B006-2496F4E24D14}"), row2.Guid2);
        Assert.AreEqual(new GDID(3, 12345), row2.Gdid1);
        Assert.AreEqual(new GDID(4, 1212345), row2.Gdid2);
        Assert.AreEqual(127.0123f, row2.Float1);
        Assert.AreEqual(-0.123f, row2.Float2);
        Assert.AreEqual(122345.012d, row2.Double1);
        Assert.AreEqual(-12345.11f, row2.Double2);
        Assert.AreEqual(1234567.098M, row2.Decimal1);
        Assert.AreEqual(22m, row2.Decimal2);
        Assert.AreEqual(new Amount("din", 123.11M), row2.Amount1);
        Assert.AreEqual(new Amount("din", 8901234567890.012M), row2.Amount2);
        Assert.IsTrue(BYTES1.SequenceEqual(row2.Bytes1));
        Assert.IsTrue(BYTES2.SequenceEqual(row2.Bytes2));

        Assert.AreEqual(ETest.One, row2.ETest1);
        Assert.AreEqual(EFlags.First, row2.EFlags1);
        Assert.AreEqual(ETest.Two, row2.ETest2);
        Assert.AreEqual(EFlags.Second | EFlags.Third, row2.EFlags2);
      }

      [Test]
      public void T_04_Targeting()
      {
        var row = new RowA
        {
          String1 = "Mudaker", String2 = "Someone",
        };
        
        var rc = new RowConverter();
      
        var doc = rc.RowToBSONDocument( row, "A" );
        Console.WriteLine(doc.ToString());
        Assert.AreEqual( "Someone", doc["s2"].ObjectValue.ToString());

        doc = rc.RowToBSONDocument( row, "B" );
        Console.WriteLine(doc.ToString());
        Assert.AreEqual( "Someone", doc["STRING-2"].ObjectValue.ToString());


        doc = rc.RowToBSONDocument( row, "NonExistent" );
        Console.WriteLine(doc.ToString());
        Assert.AreEqual( "Someone", doc["String2"].ObjectValue.ToString());
      }

      [Test]
      public void T_05_WithInnerRows()
      {
        var BYTES = new byte[] {0x00, 0x79, 0x14};

        var row = new RowB
        {
          Row1 = new RowA
          {
            String1 = "Mudaker", String2 = null,
            Date1 = new DateTime(1980, 07, 12), Date2 = null,
            Bool1 = true, Bool2 = null,
            Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"), Guid2 = null,
            Gdid1 = new GDID(0, 12345), Gdid2 = null,
            Float1 = 127.0123f, Float2 = null,
            Double1 = 122345.012d, Double2 = null,
            Decimal1 = 1234567.098M, Decimal2 = null,
            Amount1 = new Amount("din", 123.11M), Amount2 = null,
            Bytes1 = BYTES, Bytes2 = null
          },
          Row2= new RowA
          {
            String1 = "Abraham ILyach Lincoln", String2 = "I know that I know nothing",
            Date1 = new DateTime(1877, 01, 02), Date2 = new DateTime(1977, 03, 15),
            Bool1 = false, Bool2 = true,
            Guid1 = new Guid("{AAAAAAAA-FE21-4BB2-B006-2496F4E24D14}"), Guid2 = null,
            Gdid1 = new GDID(0, 12323423423), Gdid2 =  new GDID(0, 187760098292476423),
            Float1 = 127.0123f, Float2 = 123.2f,
            Double1 = 122345.012d, Double2 = -18293f,
            Decimal1 = 1234567.098M, Decimal2 = -2312m,
            Amount1 = new Amount("usd", 89123M), Amount2 = new Amount("usd", 12398933.123m),
            Bytes1 = null, Bytes2 = BYTES
          }
        };
        
        var rc = new RowConverter();
      
        var doc = rc.RowToBSONDocument( row, "A" );

        Console.WriteLine(doc.ToString());

        var row2 = new RowB();
        rc.BSONDocumentToRow(doc, row2, "A");

        Assert.IsTrue( row.Equals( row2 ) );
      }

      [Test]
      public void T_06_TargetingInnerRows()
      {
        var row = new RowB{
         Row1 = new RowA  { String1 = "Mudaker", String2 = "Someone"},
         Row2 = new RowA  { String1 = "Zar", String2 = "Boris"}
        };
        
        var rc = new RowConverter();
      
        var doc = rc.RowToBSONDocument( row, "A" );
        Console.WriteLine(doc.ToString());
        Assert.AreEqual( "Someone", ((BSONDocumentElement)doc["Row1"]).Value["s2"].ObjectValue.ToString());
        Assert.AreEqual( "Boris", ((BSONDocumentElement)doc["Row2"]).Value["s2"].ObjectValue.ToString());

        doc = rc.RowToBSONDocument( row, "B" );
        Console.WriteLine(doc.ToString());
        Assert.AreEqual( "Someone", ((BSONDocumentElement)doc["Row1"]).Value["STRING-2"].ObjectValue.ToString());
        Assert.AreEqual( "Boris", ((BSONDocumentElement)doc["Row2"]).Value["STRING-2"].ObjectValue.ToString());

        doc = rc.RowToBSONDocument( row, "NonExistent" );
        Console.WriteLine(doc.ToString());
        Assert.AreEqual( "Someone", ((BSONDocumentElement)doc["Row1"]).Value["String2"].ObjectValue.ToString());
        Assert.AreEqual( "Boris", ((BSONDocumentElement)doc["Row2"]).Value["String2"].ObjectValue.ToString());
      }

      [Test]
      public void T_07_ArraysListsAndMaps()
      {
        var row = new RowC
        {
          Map = new JSONDataMap{{"Name","Xerson"},{"Age",123}},
          List = new List<object>{ 1,true, "YEZ!", -123.01m},
          ObjectArray = new object[]{123, -12, 789d, new GDID(0, 1223), null, new object[] { 54.67d, "alpIna"}},
          MapArray = new JSONDataMap[]{ new JSONDataMap{{"a",1},{"b",true}},  new JSONDataMap{{"kosmos",234.12},{"b",null}} },
          MapList = new List<JSONDataMap>{ new JSONDataMap{{"abc",0},{"buba", new GDID(2, 1223)}},  new JSONDataMap{{"nothing",null}} }
        };

        var rc = new RowConverter();

        var doc = rc.RowToBSONDocument(row, "A");
        Console.WriteLine( doc.ToString() );

        var row2 = new RowC();
        rc.BSONDocumentToRow(doc, row2, "A");

        Assert.AreEqual(2, row2.Map.Count);
        Assert.AreEqual("Xerson", row2.Map["Name"]);
        Assert.AreEqual(123, row2.Map["Age"]);

        Assert.AreEqual(4, row2.List.Count);
        Assert.AreEqual(1, row2.List[0]);
        Assert.AreEqual(true, row2.List[1]);
        Assert.AreEqual("YEZ!", row2.List[2]);
        Assert.AreEqual(-123010m, row2.List[3]); //notice that "decimalness" is lost, because reading back into list<object>
      
        Assert.AreEqual(6, row2.ObjectArray.Length);
        Assert.AreEqual(123, row2.ObjectArray[0]);
        Assert.AreEqual(-12, row2.ObjectArray[1]);
        Assert.AreEqual(789, row2.ObjectArray[2]);
        Assert.IsTrue((new byte[]{0,0,0,0,0,0,0,0,0,0,0x04,0xc7}).SequenceEqual( (byte[])(row2.ObjectArray[3]) ));//notice that GDID is lost, it got turned into int because reading back in object[], so no type conversion happens
        Assert.AreEqual(null, row2.ObjectArray[4]);
        var arr = (object[])row2.ObjectArray[5];
        Assert.IsNotNull(arr);
        Assert.AreEqual(54.67d, arr[0]);
        Assert.AreEqual("alpIna", arr[1]);

        Assert.AreEqual(2, row2.MapArray.Length);
        Assert.AreEqual(1, row2.MapArray[0]["a"]);
        Assert.AreEqual(true, row2.MapArray[0]["b"]);
        Assert.AreEqual(234.12, row2.MapArray[1]["kosmos"]);
        Assert.AreEqual(null, row2.MapArray[1]["b"]);

        Assert.AreEqual(2, row2.MapList.Count);
        Assert.AreEqual(2, row2.MapList[0].Count);
        Assert.AreEqual(0, row2.MapList[0]["abc"]);
        Assert.IsTrue((new byte[]{0,0,0,2,0,0,0,0,0,0,0x04,0xc7}).SequenceEqual( (byte[])(row2.MapList[0]["buba"]) ) );//"GDIDness" is lost
        Assert.AreEqual(1, row2.MapList[1].Count);
        Assert.AreEqual(null, row2.MapList[1]["nothing"]);
      }

      [Test]
      public void T_08_VersionChange()
      {
        var rowA = new RowVersionA
        {
           FirstName = "Vladimir",
           LastName = "Lenin",
           Age =  DateTime.Now.Year - 1870
        };

        var rc = new RowConverter();

        var doc = rc.RowToBSONDocument(rowA, "A");

        Console.WriteLine( doc.ToString() );

        var rowB = new RowVersionB();

        rc.BSONDocumentToRow(doc, rowB, "MyLegacySystem");

        Assert.AreEqual("Vladimir", rowB.FirstName);
        Assert.AreEqual("Lenin", rowB.LastName);
        Assert.AreEqual(1870, rowB.DOB.Year);
      }

      [Test]
      public void T_09_DynamicRow()
      {
          var BYTES = new byte[] {0x00, 0x79, 0x14};

          var schema = new Schema("Dynamic Schema", 
                new Schema.FieldDef("ID", typeof(int), new List<FieldAttribute>{ new FieldAttribute(required: true, key: true)}),
                new Schema.FieldDef("Description", typeof(string), new List<FieldAttribute>{ new FieldAttribute(required: true)}),
                new Schema.FieldDef("Bytes", typeof(byte[]), new List<FieldAttribute>{ new FieldAttribute(required: true)})
          );

          var row = new DynamicRow(schema);
            
          row["ID"] = 123;
          row["Description"] = "T-90 Tank";
          row["Bytes"] = BYTES;
     
          var rc = new RowConverter();
      
          var doc = rc.RowToBSONDocument( row, "A" );

          Console.WriteLine(doc.ToString());

          var row2 = new DynamicRow(schema);
          rc.BSONDocumentToRow(doc, row2, "A");

          Assert.AreEqual(123, row2["ID"]);
          Assert.AreEqual("T-90 Tank", row2["Description"]);
          Assert.IsTrue(BYTES.SequenceEqual((byte[])row2["Bytes"]));
      }

      [Test]
      public void T_10_RowCycle_NoCycle()
      {
          var root = new RowCycle();

          root.SomeInt = 1234;
          root.InnerRow = new RowCycle();
          root.InnerRow.SomeInt = 567;
          root.InnerRow.InnerRow = null; //NO CYCLE!!!!
            
          var rc = new RowConverter();
      
          var doc = rc.RowToBSONDocument( root, "A" );

          Console.WriteLine(doc.ToString());

          var root2 = new RowCycle();
          rc.BSONDocumentToRow(doc, root2, "A");

          Assert.AreEqual(1234, root2.SomeInt);
          Assert.IsNotNull( root2.InnerRow );
          Assert.AreEqual(567, root2.InnerRow.SomeInt);
      }

      [Test]
      [ExpectedException(typeof(BSONException), ExpectedMessage="reference cycle", MatchType=MessageMatch.Contains)]
      public void T_11_RowCycle_DirectCycle()
      {
          var root = new RowCycle();

          root.SomeInt = 1234;
          root.InnerRow = root; //Direct cycle
            
          var rc = new RowConverter();
      
          var doc = rc.RowToBSONDocument( root, "A" );  //exception
      }

      [Test]
      [ExpectedException(typeof(BSONException), ExpectedMessage="reference cycle", MatchType=MessageMatch.Contains)]
      public void T_12_RowCycle_TransitiveCycle_1()
      {
          var root = new RowCycle();

          root.SomeInt = 1234;
          root.InnerRow = new RowCycle();
          root.InnerRow.SomeInt = 567;
          root.InnerRow.InnerRow = root; //TRANSITIVE(via another instance) CYCLE!!!!
            
          var rc = new RowConverter();
      
          var doc = rc.RowToBSONDocument( root, "A" );  //exception
      }

      [Test]
      [ExpectedException(typeof(BSONException), ExpectedMessage="reference cycle", MatchType=MessageMatch.Contains)]
      public void T_13_RowCycle_TransitiveCycle_2()
      {
          var root = new RowCycle();

          root.SomeInt = 1234;
          root.InnerRow = new RowCycle();
          root.InnerRow.SomeInt = 567;
          root.InnerRow.InnerRow = new RowCycle();
          root.InnerRow.InnerRow.SomeInt = 890;
          root.InnerRow.InnerRow.InnerRow = root.InnerRow;  //TRANSITIVE(via another instance) CYCLE!!!!
            
          var rc = new RowConverter();
      
          var doc = rc.RowToBSONDocument( root, "A" );  //exception
      }

      [Test]
      [ExpectedException(typeof(BSONException), ExpectedMessage="reference cycle", MatchType=MessageMatch.Contains)]
      public void T_14_RowCycle_TransitiveCycle_3()
      {
          var root = new JSONDataMap();

          root["a"] = 1;
          root["b"] = true;
          root["array"] = new JSONDataArray(){1,2,3,true,true,root};  //TRANSITIVE(via another instance) CYCLE!!!!
            
          var rc = new RowConverter(); 
      
          var doc = rc.ConvertCLRtoBSON(null, root, "A");//exception
      }

      [Test]
      public void T_15_BSONtoJSONDataMap()
      {
        var rowA = new RowVersionA
        {
           FirstName = "Vladimir",
           LastName = "Lenin",
           Age =  DateTime.Now.Year - 1870
        };

        var rc = new RowConverter();

        var doc = rc.RowToBSONDocument(rowA, "A");

        Console.WriteLine( doc.ToString() );

        var map = rc.BSONDocumentToJSONMap(doc);

        Assert.AreEqual(rowA.FirstName, map["FirstName"]); 
        Assert.AreEqual(rowA.LastName, map["LastName"]); 
        Assert.AreEqual(rowA.Age, map["Age"]); 
      }

      [Test]
      public void T_15_BSONtoJSONDataMapFilter()
      {
        var rowA = new RowVersionA
        {
           FirstName = "Vladimir",
           LastName = "Lenin",
           Age =  DateTime.Now.Year - 1870
        };

        var rc = new RowConverter();

        var doc = rc.RowToBSONDocument(rowA, "A");

        Console.WriteLine( doc.ToString() );

        var map = rc.BSONDocumentToJSONMap(doc, (d, e) => e.Name!="LastName");

        Assert.AreEqual(rowA.FirstName, map["FirstName"]); 
        Assert.IsNull( map["LastName"] ); //filter skipped
        Assert.AreEqual(rowA.Age, map["Age"]); 
      }

      [Test]
      public void T_16_VersionChange_AmorphousDisabled()
      {
        var rowA = new RowVersionA
        {
           FirstName = "Vladimir",
           LastName = "Lenin",
           Age =  DateTime.Now.Year - 1870
        };

        var rc = new RowConverter();

        var doc = rc.RowToBSONDocument(rowA, "A", useAmorphousData: false);

        Console.WriteLine( doc.ToString() );

        var rowB = new RowVersionB();

        rc.BSONDocumentToRow(doc, rowB, "MyLegacySystem", useAmorphousData: false);

        Assert.AreEqual("Vladimir", rowB.FirstName);
        Assert.AreEqual("Lenin", rowB.LastName);
        Assert.AreEqual(new DateTime(), rowB.DOB);
      }

      [Test]
      public void T_16_VersionChange_AmorphousDisabled_WithFilter()
      {
        var rowA = new RowVersionA
        {
           FirstName = "Vladimir",
           LastName = "Lenin",
           Age =  DateTime.Now.Year - 1870
        };

        var rc = new RowConverter();

        var doc = rc.RowToBSONDocument(rowA, "A", useAmorphousData: false);

        Console.WriteLine( doc.ToString() );

        var rowB = new RowVersionB();

        rc.BSONDocumentToRow(doc, rowB, "MyLegacySystem", useAmorphousData: false, filter: (d,e) => e.Name!="LastName");

        Assert.AreEqual("Vladimir", rowB.FirstName);
        Assert.IsNull( rowB.LastName );
        Assert.AreEqual(new DateTime(), rowB.DOB);
      }

      [Test]
      public void T_17_VersionChange_AmorphousExtra()
      {
        var BYTES = new byte[] {0x00, 0x79, 0x14};

        var rowA = new RowVersionA
        {
           FirstName = "Vladimir",
           LastName = "Lenin",
           Age =  DateTime.Now.Year - 1870
        };

        rowA.AmorphousData["AABB"] = "extra data";
        rowA.AmorphousData["Bytes"] = BYTES;

        var rc = new RowConverter();

        var doc = rc.RowToBSONDocument(rowA, "A", useAmorphousData: true);

        Console.WriteLine( doc.ToString() );

        var rowB = new RowVersionB();

        rc.BSONDocumentToRow(doc, rowB, "MyLegacySystem", useAmorphousData: true);

        Assert.AreEqual("Vladimir", rowB.FirstName);
        Assert.AreEqual("Lenin", rowB.LastName);
        Assert.AreEqual(1870, rowB.DOB.Year);
        Assert.AreEqual("extra data", rowB.AmorphousData["AABB"]);
        Assert.IsTrue( BYTES.SequenceEqual((byte[])rowB.AmorphousData["Bytes"]));
      }

    #endregion

    #region pvt.

      private BSONDocument fullCopy(BSONDocument original)
      {
        //note: doc.DeepClone does not work as expected (copies references for byte[])
        using (var ms = new MemoryStream())
        {
          #pragma warning disable 0618
            original.WriteAsBSON(ms);
            ms.Position = 0;
            return new BSONDocument(ms);
          #pragma warning restore 0618
        }
      } 

    #endregion

    #region Mocks

         public enum ETest { Zero = 0x77, One, Two }

         [Flags]
         public enum EFlags { First = 0x01, Second = 0x02, Third = 0x04, Fifth = 0x0f, FirstSecond = First | Second }

         public class EnumRow: TypedRow
         {
           [Field] public ETest ETest1 {get; set;}
           [Field] public EFlags EFlags1 {get; set;}

           public override bool Equals(Row other)
           {
             var or = other as RowA;
             if (or==null) return false;

             foreach(var f in this.Schema)
             {
               var v1 = this.GetFieldValue(f);
               var v2 = or.GetFieldValue(f);

               if (v1==null)
               {
                if (v2==null) continue;
                else return false;
               }
               else if (v2 == null)
                return false;

               if (!v1.Equals( v2 )) return false;
             }
             
             return true;
           }
         }

         public class RowA : TypedRow
         {
           [Field] public string String1{get; set;}
           
           [Field(targetName: "A", backendName: "s2")] 
           [Field(targetName: "B", backendName: "STRING-2")] 
           public string String2{get; set;}
           
           [Field] public byte Byte1{get; set;}
           [Field] public sbyte SByte1{get; set;}
           [Field] public short Short1{get; set;}
           [Field] public ushort UShort1{get; set;}
           [Field] public int Int1{get; set;}
           [Field] public uint Uint1{get; set;}
           [Field] public long Long1{get; set;}
           [Field] public ulong ULong1{get; set;}

           [Field] public byte?   Byte2{get; set;}
           [Field] public sbyte?  SByte2{get; set;}
           [Field] public short?  Short2{get; set;}
           [Field] public ushort? UShort2{get; set;}
           [Field] public int?    Int2{get; set;}
           [Field] public uint?   Uint2{get; set;}
           [Field] public long?   Long2{get; set;}
           [Field] public ulong?  ULong2{get; set;}



           [Field] public DateTime Date1{get; set;}
           [Field] public DateTime? Date2{get; set;}
           [Field] public bool Bool1{get; set;}
           [Field] public bool? Bool2{get; set;}
           [Field] public Guid Guid1{get; set;}
           [Field] public Guid? Guid2{get; set;}
           [Field] public GDID Gdid1{get; set;}
           [Field] public GDID? Gdid2{get; set;}

           [Field] public float Float1{get; set;}
           [Field] public float? Float2{get; set;}
           [Field] public double Double1{get; set;}
           [Field] public double? Double2{get; set;}
           [Field] public decimal Decimal1{get; set;}
           [Field] public decimal? Decimal2{get; set;}
           [Field] public Amount Amount1{get; set;}
           [Field] public Amount? Amount2{get; set;}
           [Field] public byte[] Bytes1{get; set;}
           [Field] public byte[] Bytes2{get; set;}

           [Field] public ETest ETest1 {get; set;}
           [Field] public ETest? ETest2 {get; set;}

           [Field] public EFlags EFlags1 {get; set;}
           [Field] public EFlags? EFlags2 {get; set;}

           public override bool Equals(Row other)
           {
             var or = other as RowA;
             if (or==null) return false;

             foreach(var f in this.Schema)
             {
               var v1 = this.GetFieldValue(f);
               var v2 = or.GetFieldValue(f);

               if (v1==null)
               {
                if (v2==null) continue;
                else return false;
               }
               else if (v2 == null)
                return false;

               if (v1 is byte[])
               {
                 return ((byte[])v1).SequenceEqual((byte[])v2);
               }

               if (!v1.Equals( v2 )) return false;
             }
             
             return true;
           }
         }


         public class RowB : TypedRow
         {
           [Field] public RowA Row1{get; set;}
           [Field] public RowA Row2{get; set;}
         }

         public class RowC : TypedRow
         {
           [Field] public JSONDataMap  Map{get; set;}
           [Field] public object[]  ObjectArray{get; set;}
           [Field] public JSONDataMap[]  MapArray{get; set;}
           [Field] public List<object> List{get; set;}
           [Field] public List<JSONDataMap> MapList{get; set;}
         }


         public class RowVersionA : AmorphousTypedRow
         {
           [Field] public string FirstName{get;set;}
           [Field] public string LastName{get;set;}
           [Field] public int Age{get;set;} //suppose we designed it a few years back and made a mistake - keeping an age as an INT (instread of a date)
         }

         public class RowVersionB : AmorphousTypedRow
         {
           [Field] public string FirstName{get;set;}
           [Field] public string LastName{get;set;}
           [Field] public DateTime DOB{get;set;}//today we made a new version of row with proper design - DOB date field
                                                // AFterLoad() allows to preserve existing data (as much as it can be done) 

           public int Age{ get { return (int)((DateTime.Now-DOB).TotalDays / 365d);}}//Age is now a calculated property, so existing code does not break
           
           public override void AfterLoad(string targetName)
           {
             if (targetName=="MyLegacySystem")//if data came from THIS system
             {
               if (AmorphousData.ContainsKey("Age"))//we take older format that was now placed in an amorphous dictionary
               {
                  var age = AmorphousData["Age"].AsInt();//convert it to int
                  this.DOB = DateTime.Now.AddYears(-age);//and init new stored property DOB, thus preserving meaningful data at least partially without data loss
               }
             }
           }
         }


         public class RowCycle : TypedRow
         {
           [Field] public int SomeInt{get; set;}
           [Field] public RowCycle InnerRow{get; set;}
         }

    #endregion
     
  }
}
