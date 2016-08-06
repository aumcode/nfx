using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.Serialization.JSON;


namespace NFX.NUnit.DataAccess
{
  [TestFixture]
  public class GDIDTest
  {
      [TestCase]
      public void GDID_1()
      {
        var gdid = new GDID(2, 5, 89078);
        Assert.AreEqual(2,     gdid.Era);
        Assert.AreEqual(5,     gdid.Authority);
        Assert.AreEqual(89078, gdid.Counter);
      }

      [TestCase]
      [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="GDID can not be created from the supplied", MatchType=MessageMatch.Contains) ]
      public void GDID_2()
      {
        var gdid = new GDID(0, 16, 89078);
      }

      [TestCase]
      [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="GDID can not be created from the supplied", MatchType=MessageMatch.Contains) ]
      public void GDID_3()
      {
        var gdid = new GDID(0, 12, GDID.COUNTER_MAX+1);
      }

      [TestCase]
      public void GDID_4()
      {
        var gdid = new GDID(0, 15, GDID.COUNTER_MAX);
        Assert.AreEqual(15,               gdid.Authority);
        Assert.AreEqual(GDID.COUNTER_MAX, gdid.Counter);
      }

      [TestCase]
      public void GDID_5()
      {
        var gdid = new GDID(0, 0, GDID.COUNTER_MAX);
        Assert.AreEqual(0,                gdid.Authority);
        Assert.AreEqual(GDID.COUNTER_MAX, gdid.Counter);
      }

      [TestCase]
      public void GDID_6()
      {
        var gdid = new GDID(0, 0, 0);
        Assert.AreEqual(0, gdid.Authority);
        Assert.AreEqual(0, gdid.Counter);
      }


      [TestCase]
      public void GDID_7()
      {
        var gdid1 = new GDID(0, 0, 12321);
        var gdid2 = new GDID(0, 1, 0);
        Assert.AreEqual(-1, gdid1.CompareTo(gdid2));
        Assert.IsFalse( gdid1.Equals(gdid2));
      }

      [TestCase]
      public void GDID_8()
      {
        var gdid1 = new GDID(0, 1, 12321);
        var gdid2 = new GDID(0, 1, 0);
        Assert.AreEqual(1, gdid1.CompareTo(gdid2));
        Assert.IsFalse( gdid1.Equals(gdid2));
      }

      [TestCase]
      public void GDID_9()
      {
        var gdid1 = new GDID(0, 3, 57);
        var gdid2 = new GDID(0, 3, 57);
        Assert.AreEqual(0, gdid1.CompareTo(gdid2));
        Assert.IsTrue( gdid1.Equals(gdid2));

        var gdid3 = new GDID(1, 3, 57);
        var gdid4 = new GDID(2, 3, 57);
        Assert.AreEqual(-1, gdid3.CompareTo(gdid4));
        Assert.IsFalse( gdid3.Equals(gdid4));
      }

      [TestCase]
      public void GDID_10()
      {
        var gdid = new GDID(1293, 3, 57);
        var s = gdid.ToString();
        Console.WriteLine(s);
        Assert.AreEqual("1293:3:57", s);
      }

      [TestCase]
      public void GDID_11()
      {
        var gdid = new GDID(0x01020304, 0xfacaca00aa55aa55);
          
        Assert.IsTrue( "01,02,03,04,fa,ca,ca,00,aa,55,aa,55".AsByteArray().SequenceEqual( gdid.Bytes ) );
      }


      [TestCase]
      public void GDID_JSON_1()
      {
        var gdid = new GDID(2, 3, 57);
        var s = gdid.ToJSON();
        Console.WriteLine(s);
        Assert.AreEqual("\"2:3:57\"", s);
      }

      [TestCase]
      public void GDID_JSON_2()
      {
        var obj = new{ id = new GDID(22, 3, 57), Name = "Tezter"};
        var s = obj.ToJSON();
        Console.WriteLine(s);
        Assert.AreEqual("{\"id\":\"22:3:57\",\"Name\":\"Tezter\"}", s);
      }

      [TestCase()]
      public void GDID_TryParse()
      {
        GDID parsed;
        Assert.IsTrue( GDID.TryParse("1:2:3", out parsed) );
        Assert.AreEqual(1, parsed.Era);
        Assert.AreEqual(2, parsed.Authority);
        Assert.AreEqual(3, parsed.Counter);

        Assert.IsTrue( GDID.TryParse("231:2:3123", out parsed) );
        Assert.AreEqual(231, parsed.Era);
        Assert.AreEqual(2, parsed.Authority);
        Assert.AreEqual(3123, parsed.Counter);

        Assert.IsTrue( GDID.TryParse("   231:2:3123   ", out parsed) );
        Assert.AreEqual(231, parsed.Era);
        Assert.AreEqual(2, parsed.Authority);
        Assert.AreEqual(3123, parsed.Counter);

        Assert.IsTrue( GDID.TryParse("31 : 2:  3123", out parsed) );
        Assert.AreEqual(31, parsed.Era);
        Assert.AreEqual(2, parsed.Authority);
        Assert.AreEqual(3123, parsed.Counter);

        Assert.IsFalse( GDID.TryParse("-1:2:3123", out parsed) );
        Assert.IsFalse( GDID.TryParse("1:18:3123", out parsed) );
        Assert.IsFalse( GDID.TryParse(":18:3123", out parsed) );
        Assert.IsFalse( GDID.TryParse("::3123", out parsed) );
        Assert.IsFalse( GDID.TryParse("1::3", out parsed) );
        Assert.IsFalse( GDID.TryParse("1::", out parsed) );
        Assert.IsFalse( GDID.TryParse("1:-:-", out parsed) );
        Assert.IsFalse( GDID.TryParse("1: : ", out parsed) );

        Assert.IsFalse( GDID.TryParse("FTGEJK-IR", out parsed) );


        //0x 00 00 00 00 10 00 00 00 00 00 00 4B
        //   ----era---- ------ulong -----------
        Assert.IsTrue( GDID.TryParse("0x00000000100000000000004B", out parsed) );
        Assert.AreEqual( new GDID(0,1,0x4b), parsed);

        GDID? nullable;
        Assert.IsTrue( GDID.TryParse("0x00000000100000000000004B", out nullable) );
        Assert.IsTrue( nullable.HasValue);
        Assert.AreEqual( new GDID(0,1,0x4b), nullable.Value);

        Assert.IsFalse( GDID.TryParse("0x0001000000000000", out nullable) );//too short
        Assert.IsFalse( nullable.HasValue);

        Assert.IsFalse( GDID.TryParse("0x00000303030303003031000000000000", out nullable) );//too long
        Assert.IsFalse( nullable.HasValue);
      }

      [TestCase()]
      public void GDID_BinBuffer()
      {
        var gdid = new GDID(0,1,0x4b);
        var buf = gdid.Bytes;
        Console.WriteLine(buf.ToDumpString(DumpFormat.Hex));
        var gdid2 = new GDID(buf);
        Assert.AreEqual(gdid, gdid2);
      }

      [TestCase()]
      public void GDID_BinBufferAndTryParseBin()
      {
        var gdid = new GDID(347827,15,0xaedb3434b);
        var buf = gdid.Bytes;
        var hex = "0x"+buf.ToDumpString(DumpFormat.Hex).Replace(" ","");
        
        Console.WriteLine(hex);
        
        GDID gdid2;
        Assert.IsTrue(GDID.TryParse(hex, out gdid2));
        Assert.AreEqual(gdid, gdid2);
      }


      [TestCase()]
      public void GDID_RangeComparer()
      {
        Assert.AreEqual(-1, GDIDRangeComparer.Instance.Compare( new GDID(0, 1, 2), new GDID(1,1,2)));
        Assert.AreEqual(+1, GDIDRangeComparer.Instance.Compare( new GDID(1, 1, 2), new GDID(0,1,2)));

        Assert.AreEqual(-1, GDIDRangeComparer.Instance.Compare( new GDID(0, 1, 2), new GDID(0,1,3)));
        Assert.AreEqual(+1, GDIDRangeComparer.Instance.Compare( new GDID(1, 1, 2), new GDID(1,1,0)));

        Assert.AreEqual(0, GDIDRangeComparer.Instance.Compare( new GDID(1, 1, 2), new GDID(1,1,2)));

        //notice: Authority is ignored
        Assert.AreEqual(0, GDIDRangeComparer.Instance.Compare( new GDID(1, 13, 2), new GDID(1,8,2)));

      }

      [TestCase()]
      public void GDID_Zero()
      {
        var zero = GDID.Zero;
        Assert.IsTrue( zero.IsZero );

        zero = new GDID(0,1,0);
        Assert.IsFalse( zero.IsZero );
      }

      [TestCase()]
      public void GDID_RequiredRow_1()
      {
        var row = new GDIDRow();
        var err = row.Validate();

        Assert.IsNotNull(err);
        Assert.IsInstanceOf<CRUDFieldValidationException>(err);
        Assert.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);

        row.GDID = new GDID(1,1,1);
        err = row.Validate();

        Assert.IsNull(err);
        row.GDID = GDID.Zero;
        err = row.Validate();

        Assert.IsNotNull(err);
        Assert.IsInstanceOf<CRUDFieldValidationException>(err);
        Assert.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);
      }

      [TestCase()]
      public void GDID_RequiredRow_2()
      {
        var row = new NullableGDIDRow();
        var err = row.Validate();

        Assert.IsNotNull(err);
        Assert.IsInstanceOf<CRUDFieldValidationException>(err);
        Assert.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);

        row.GDID = new GDID(1,1,1);
        err = row.Validate();

        Assert.IsNull(err);

        row.GDID = GDID.Zero;
        err = row.Validate();

        Assert.IsNotNull(err);
        Assert.IsInstanceOf<CRUDFieldValidationException>(err);
        Assert.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);

        row.GDID = null;//Nullable
        err = row.Validate();

        Assert.IsNotNull(err);
        Assert.IsInstanceOf<CRUDFieldValidationException>(err);
        Assert.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);
      }

      public class GDIDRow: TypedRow
      {
        [Field(required: true)] public GDID GDID {get; set;}
      }

      public class NullableGDIDRow: TypedRow
      {
        [Field(required: true)] public GDID? GDID {get; set;}
      }


      [TestCase()]
      public void GDIDSymbol()
      {
        var link = new ELink(new GDID(1280, 2, 8902382));

        var gs = link.AsGDIDSymbol();

        Assert.AreEqual(link.GDID, gs.GDID);
        Assert.AreEqual(link.Link, gs.Symbol);

        var gs2 = new GDIDSymbol(gs.GDID, gs.Symbol);
        Assert.AreEqual(gs, gs2);
        Assert.IsTrue( gs.Equals(gs2));
        Assert.IsFalse( gs.Equals(this));
        Assert.IsFalse( gs.Equals(null));

        Assert.AreEqual(gs.GetHashCode(), gs2.GetHashCode());

        Console.WriteLine( gs.ToString());
      }

  }
}
