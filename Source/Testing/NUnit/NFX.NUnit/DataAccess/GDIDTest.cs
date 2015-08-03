using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.Serialization.JSON;
using NFX.DataAccess.Distributed;

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
        Assert.AreEqual("GDID[1293:3458764513820540985(3,57)]", s);
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
        Assert.AreEqual("\"2:3458764513820540985\"", s);
      }

      [TestCase]
      public void GDID_JSON_2()
      {
        var obj = new{ id = new GDID(22, 3, 57), Name = "Tezter"};
        var s = obj.ToJSON();
        Console.WriteLine(s);
        Assert.AreEqual("{\"id\":\"22:3458764513820540985\",\"Name\":\"Tezter\"}", s);
      }
  }
}
