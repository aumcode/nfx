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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.IO;
using NFX.Serialization.Slim;
using NFX.Environment;


namespace NFX.NUnit.AppModel.Pile
{
  [TestFixture]
  public class KeyStructTest
  {

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void GDIDWithISOKey_BadCtor_1()
      {
        var k = new GDIDWithISOKey(new GDID(10, 20), null);
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void GDIDWithISOKey_BadCtor_2()
      {
        var k = new GDIDWithISOKey(new GDID(10, 20), "");
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void GDIDWithISOKey_BadCtor_3()
      {
        var k = new GDIDWithISOKey(new GDID(10, 20), "ertewrtewrte");
      }

      [Test]
      public void GDIDWithISOKey_CreateEquate_1()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "eng");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "eNG");
        Assert.AreEqual("ENG", k1.ISOCode);
        Assert.AreEqual("ENG", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void GDIDWithISOKey_CreateEquate_2()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "ua");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "UA");
        Assert.AreEqual("UA", k1.ISOCode);
        Assert.AreEqual("UA", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void GDIDWithISOKey_CreateNotEquate_1()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "eng");
        var k2 = new GDIDWithISOKey(new GDID(10, 21), "eNG");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void GDIDWithISOKey_CreateNotEquate_2()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "eng");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "fra");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void GDIDWithISOKey_CreateNotEquate_3()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "en");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "fr");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void GDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<GDIDWithISOKey, string>();

        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "eng"), "123eng");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "deu"), "123deu");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "eN"), "123en");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "dE"), "123de");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "ua"), "123ua");

        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "eng"), "345eng");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "deu"), "345deu");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "eN"), "345en");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "dE"), "345de");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "ua"), "345ua");

        Assert.AreEqual("123eng", dict[new GDIDWithISOKey(new GDID(1, 123), "eng")]);
        Assert.AreEqual("123deu", dict[new GDIDWithISOKey(new GDID(1, 123), "deu")]);
        Assert.AreEqual("123eng", dict[new GDIDWithISOKey(new GDID(1, 123), "ENG")]);
        Assert.AreEqual("123deu", dict[new GDIDWithISOKey(new GDID(1, 123), "DEU")]);

        Assert.AreEqual("123en", dict[new GDIDWithISOKey(new GDID(1, 123), "en")]);
        Assert.AreEqual("123de", dict[new GDIDWithISOKey(new GDID(1, 123), "de")]);
        Assert.AreEqual("123ua", dict[new GDIDWithISOKey(new GDID(1, 123), "ua")]);

        Assert.AreEqual("123en", dict[new GDIDWithISOKey(new GDID(1, 123), "EN")]);
        Assert.AreEqual("123de", dict[new GDIDWithISOKey(new GDID(1, 123), "DE")]);
        Assert.AreEqual("123ua", dict[new GDIDWithISOKey(new GDID(1, 123), "UA")]);


        Assert.AreEqual("345eng", dict[new GDIDWithISOKey(new GDID(1, 345), "eng")]);
        Assert.AreEqual("345deu", dict[new GDIDWithISOKey(new GDID(1, 345), "deu")]);
        Assert.AreEqual("345eng", dict[new GDIDWithISOKey(new GDID(1, 345), "ENG")]);
        Assert.AreEqual("345deu", dict[new GDIDWithISOKey(new GDID(1, 345), "DEU")]);

        Assert.AreEqual("345en", dict[new GDIDWithISOKey(new GDID(1, 345), "en")]);
        Assert.AreEqual("345de", dict[new GDIDWithISOKey(new GDID(1, 345), "de")]);
        Assert.AreEqual("345ua", dict[new GDIDWithISOKey(new GDID(1, 345), "ua")]);

        Assert.AreEqual("345en", dict[new GDIDWithISOKey(new GDID(1, 345), "EN")]);
        Assert.AreEqual("345de", dict[new GDIDWithISOKey(new GDID(1, 345), "DE")]);
        Assert.AreEqual("345ua", dict[new GDIDWithISOKey(new GDID(1, 345), "UA")]);


        Assert.IsTrue( dict.ContainsKey(new GDIDWithISOKey(new GDID(1, 123), "UA")));
        Assert.IsFalse( dict.ContainsKey(new GDIDWithISOKey(new GDID(1, 122), "UA")));
        Assert.IsFalse( dict.ContainsKey(new GDIDWithISOKey(new GDID(21, 123), "UA")));
      }







      [Test]
      [ExpectedException(typeof(NFXException))]
      public void DatedGDIDWithISOKey_BadCtor_1()
      {
        var k = new DatedGDIDWithISOKey(DateTime.Now, new GDID(10, 20), null);
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void DatedGDIDWithISOKey_BadCtor_2()
      {
        var k = new DatedGDIDWithISOKey(DateTime.Now, new GDID(10, 20), "");
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void DatedGDIDWithISOKey_BadCtor_3()
      {
        var k = new DatedGDIDWithISOKey(DateTime.Now, new GDID(10, 20), "ertewrtewrte");
      }

      [Test]
      public void DatedGDIDWithISOKey_CreateEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eng");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eNG");
        Assert.AreEqual("ENG", k1.ISOCode);
        Assert.AreEqual("ENG", k2.ISOCode);

        Assert.AreEqual(1980, k1.DateTime.Year);
        Assert.AreEqual(10,   k1.DateTime.Month);
        Assert.AreEqual(2,    k1.DateTime.Day);

        Assert.AreEqual(1980, k2.DateTime.Year);
        Assert.AreEqual(10,   k2.DateTime.Month);
        Assert.AreEqual(2,    k2.DateTime.Day);

        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void DatedGDIDWithISOKey_CreateEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "ua");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "UA");
        Assert.AreEqual("UA", k1.ISOCode);
        Assert.AreEqual("UA", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void DatedGDIDWithISOKey_CreateEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 14, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 2, 0, 18, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt1, new GDID(10, 20), "ua");
        var k2 = new DatedGDIDWithISOKey(dt2, new GDID(10, 20), "UA");
        Assert.AreEqual("UA", k1.ISOCode);
        Assert.AreEqual("UA", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void DatedGDIDWithISOKey_CreateNotEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eng");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 21), "eNG");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void DatedGDIDWithISOKey_CreateNotEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eng");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "fra");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void DatedGDIDWithISOKey_CreateNotEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt1, new GDID(10, 20), "en");
        var k2 = new DatedGDIDWithISOKey(dt2, new GDID(10, 20), "en");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void DatedGDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<DatedGDIDWithISOKey, string>();

        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);



        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "eng"), "123eng");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "deu"), "123deu");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "eN"), "123en");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "dE"), "123de");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "ua"), "123ua");

        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "eng"), "345eng");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "deu"), "345deu");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "eN"), "345en");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "dE"), "345de");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "ua"), "345ua");

        Assert.AreEqual("123eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "eng")]);
        Assert.AreEqual("123deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "deu")]);
        Assert.AreEqual("123eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "ENG")]);
        Assert.AreEqual("123deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "DEU")]);

        Assert.AreEqual("123en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "en")]);
        Assert.AreEqual("123de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "de")]);
        Assert.AreEqual("123ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "ua")]);

        Assert.AreEqual("123en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "EN")]);
        Assert.AreEqual("123de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "DE")]);
        Assert.AreEqual("123ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "UA")]);


        Assert.AreEqual("345eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "eng")]);
        Assert.AreEqual("345deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "deu")]);
        Assert.AreEqual("345eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "ENG")]);
        Assert.AreEqual("345deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "DEU")]);

        Assert.AreEqual("345en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "en")]);
        Assert.AreEqual("345de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "de")]);
        Assert.AreEqual("345ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "ua")]);

        Assert.AreEqual("345en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "EN")]);
        Assert.AreEqual("345de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "DE")]);
        Assert.AreEqual("345ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "UA")]);


        Assert.IsTrue ( dict.ContainsKey(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "UA")));
        Assert.IsFalse( dict.ContainsKey(new DatedGDIDWithISOKey(dt2, new GDID(1, 123), "UA")));
        Assert.IsFalse( dict.ContainsKey(new DatedGDIDWithISOKey(dt1, new GDID(1, 122), "UA")));
        Assert.IsFalse( dict.ContainsKey(new DatedGDIDWithISOKey(dt1, new GDID(21, 123), "UA")));
      }







      [Test]
      [ExpectedException(typeof(NFXException))]
      public void Dated2GDIDWithISOKey_BadCtor_1()
      {
        var k = new Dated2GDIDWithISOKey(DateTime.Now, new GDID(10, 20), new GDID(10, 30), null);
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void Dated2GDIDWithISOKey_BadCtor_2()
      {
        var k = new Dated2GDIDWithISOKey(DateTime.Now, new GDID(10, 20), new GDID(10,30), "");
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void Dated2GDIDWithISOKey_BadCtor_3()
      {
        var k = new Dated2GDIDWithISOKey(DateTime.Now, new GDID(10, 20), new GDID(10,30), "ertewrtewrte");
      }

      [Test]
      public void Dated2GDIDWithISOKey_CreateEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eNG");
        Assert.AreEqual("ENG", k1.ISOCode);
        Assert.AreEqual("ENG", k2.ISOCode);

        Assert.AreEqual(1980, k1.DateTime.Year);
        Assert.AreEqual(10,   k1.DateTime.Month);
        Assert.AreEqual(2,    k1.DateTime.Day);

        Assert.AreEqual(1980, k2.DateTime.Year);
        Assert.AreEqual(10,   k2.DateTime.Month);
        Assert.AreEqual(2,    k2.DateTime.Day);

        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void Dated2GDIDWithISOKey_CreateEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "UA");
        Assert.AreEqual("UA", k1.ISOCode);
        Assert.AreEqual("UA", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void Dated2GDIDWithISOKey_CreateEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 14, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 2, 0, 18, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt1, new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new Dated2GDIDWithISOKey(dt2, new GDID(10, 20), new GDID(10, 30), "UA");
        Assert.AreEqual("UA", k1.ISOCode);
        Assert.AreEqual("UA", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }


      [Test]
      public void Dated2GDIDWithISOKey_CreateNotEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 21), new GDID(10, 30), "eNG");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void Dated2GDIDWithISOKey_CreateNotEquate_1_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 31), "eNG");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void Dated2GDIDWithISOKey_CreateNotEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "fra");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void Dated2GDIDWithISOKey_CreateNotEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt1, new GDID(10, 20), new GDID(10, 30), "en");
        var k2 = new Dated2GDIDWithISOKey(dt2, new GDID(10, 20), new GDID(10, 30), "en");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }


      [Test]
      public void Dated2GDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<Dated2GDIDWithISOKey, string>();

        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);



        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "eng"), "123eng");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "deu"), "123deu");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "eN"), "123en");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "dE"), "123de");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "ua"), "123ua");

        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "eng"), "345eng");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "deu"), "345deu");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "eN"), "345en");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "dE"), "345de");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "ua"), "345ua");

        Assert.AreEqual("123eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "eng")]);
        Assert.AreEqual("123deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "deu")]);
        Assert.AreEqual("123eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "ENG")]);
        Assert.AreEqual("123deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "DEU")]);

        Assert.AreEqual("123en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "en")]);
        Assert.AreEqual("123de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "de")]);
        Assert.AreEqual("123ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "ua")]);

        Assert.AreEqual("123en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "EN")]);
        Assert.AreEqual("123de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "DE")]);
        Assert.AreEqual("123ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "UA")]);


        Assert.AreEqual("345eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "eng")]);
        Assert.AreEqual("345deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "deu")]);
        Assert.AreEqual("345eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "ENG")]);
        Assert.AreEqual("345deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "DEU")]);

        Assert.AreEqual("345en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "en")]);
        Assert.AreEqual("345de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "de")]);
        Assert.AreEqual("345ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "ua")]);

        Assert.AreEqual("345en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "EN")]);
        Assert.AreEqual("345de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "DE")]);
        Assert.AreEqual("345ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "UA")]);


        Assert.IsTrue ( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "UA")));
        Assert.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(20, 40), "UA")));
        Assert.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 321), new GDID(10, 30), "UA")));

        Assert.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt2, new GDID(1, 123), new GDID(10, 30), "UA")));
        Assert.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 122), new GDID(10, 30), "UA")));
        Assert.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(21, 123), new GDID(10, 30), "UA")));
      }













      [Test]
      [ExpectedException(typeof(NFXException))]
      public void TwoGDIDWithISOKey_BadCtor_1()
      {
        var k = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), null);
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void TwoGDIDWithISOKey_BadCtor_2()
      {
        var k = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10,30), "");
      }

      [Test]
      [ExpectedException(typeof(NFXException))]
      public void TwoGDIDWithISOKey_BadCtor_3()
      {
        var k = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10,30), "ertewrtewrte");
      }

      [Test]
      public void TwoGDIDWithISOKey_CreateEquate_1()
      {

        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eNG");
        Assert.AreEqual("ENG", k1.ISOCode);
        Assert.AreEqual("ENG", k2.ISOCode);

        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void TwoGDIDWithISOKey_CreateEquate_2()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "UA");
        Assert.AreEqual("UA", k1.ISOCode);
        Assert.AreEqual("UA", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void TwoGDIDWithISOKey_CreateEquate_3()
      {

        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "UA");
        Assert.AreEqual("UA", k1.ISOCode);
        Assert.AreEqual("UA", k2.ISOCode);
        Assert.AreEqual(k1, k2);

        Assert.IsTrue(k1.Equals(k2));
        var o = k2;
        Assert.IsTrue(k1.Equals(o));

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }


      [Test]
      public void TwoGDIDWithISOKey_CreateNotEquate_1()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 21), new GDID(10, 30), "eNG");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void TwoGDIDWithISOKey_CreateNotEquate_1_1()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 31), "eNG");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void TwoGDIDWithISOKey_CreateNotEquate_2()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "fra");
        Assert.AreNotEqual(k1, k2);

        Assert.IsFalse(k1.Equals(k2));
        var o = k2;
        Assert.IsFalse(k1.Equals(o));

        Assert.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Test]
      public void TwoGDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<TwoGDIDWithISOKey, string>();


        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "eng"), "123eng");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "deu"), "123deu");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "eN"), "123en");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "dE"), "123de");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "ua"), "123ua");

        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "eng"), "345eng");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "deu"), "345deu");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "eN"), "345en");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "dE"), "345de");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "ua"), "345ua");

        Assert.AreEqual("123eng", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "eng")]);
        Assert.AreEqual("123deu", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "deu")]);
        Assert.AreEqual("123eng", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "ENG")]);
        Assert.AreEqual("123deu", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "DEU")]);

        Assert.AreEqual("123en", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "en")]);
        Assert.AreEqual("123de", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "de")]);
        Assert.AreEqual("123ua", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "ua")]);

        Assert.AreEqual("123en", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "EN")]);
        Assert.AreEqual("123de", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "DE")]);
        Assert.AreEqual("123ua", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "UA")]);


        Assert.AreEqual("345eng", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "eng")]);
        Assert.AreEqual("345deu", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "deu")]);
        Assert.AreEqual("345eng", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "ENG")]);
        Assert.AreEqual("345deu", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "DEU")]);

        Assert.AreEqual("345en", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "en")]);
        Assert.AreEqual("345de", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "de")]);
        Assert.AreEqual("345ua", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "ua")]);

        Assert.AreEqual("345en", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "EN")]);
        Assert.AreEqual("345de", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "DE")]);
        Assert.AreEqual("345ua", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "UA")]);


        Assert.IsTrue ( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "UA")));
        Assert.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(20, 40), "UA")));
        Assert.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 321), new GDID(10, 30), "UA")));

        Assert.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 122), new GDID(10, 30), "UA")));
        Assert.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(21, 123), new GDID(10, 30), "UA")));
      }

      [Test]
      public void GDIDWithStrHash_Equals()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        var g2 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        Assert.AreEqual(g1, g2);
        Assert.AreEqual(g1.GetHashCode(), g2.GetHashCode());
        Assert.AreEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Test]
      public void GDIDWithStrHash_NotEquals()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        var g2 = new GDIDWithStrHash(new GDID(1, 123), "this iS my long line");
        Assert.AreNotEqual(g1, g2);
        Assert.AreNotEqual(g1.GetHashCode(), g2.GetHashCode());
        Assert.AreNotEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Test]
      public void GDIDWithStrHash_NotEquals2()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        var g2 = new GDIDWithStrHash(new GDID(1, 121), "this is my long line");
        Assert.AreNotEqual(g1, g2);
        Assert.AreNotEqual(g1.GetHashCode(), g2.GetHashCode());
        Assert.AreNotEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Test]
      public void GDIDWithStrHash_NotEquals3()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "lenen");
        var g2 = new GDIDWithStrHash(new GDID(1, 123), "lenin");
        Assert.AreNotEqual(g1, g2);
        Assert.AreNotEqual(g1.GetHashCode(), g2.GetHashCode());
        Assert.AreNotEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }


      [Test]
      public void GDIDWithInt_Equals()
      {
        var g1 = new GDIDWithInt(new GDID(1, 123), 100);
        var g2 = new GDIDWithInt(new GDID(1, 123), 100);
        Assert.AreEqual(g1, g2);
        Assert.AreEqual(g1.GetHashCode(), g2.GetHashCode());
        Assert.AreEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Test]
      public void GDIDWithInt_NotEquals()
      {
        var g1 = new GDIDWithInt(new GDID(1,123), 341);
        var g2 = new GDIDWithInt(new GDID(1,123), 143);
        Assert.AreNotEqual(g1,g2);
        Assert.AreNotEqual(g1.GetHashCode(),g2.GetHashCode());
        Assert.AreNotEqual(g1.GetDistributedStableHash(),g2.GetDistributedStableHash());
      }

      [Test]
      public void GDIDWithInt_NotEquals2()
      {
        var g1 = new GDIDWithInt(new GDID(1,123), 341);
        var g2 = new GDIDWithInt(new GDID(1,214), 341);
        Assert.AreNotEqual(g1,g2);
        Assert.AreNotEqual(g1.GetHashCode(),g2.GetHashCode());
        Assert.AreNotEqual(g1.GetDistributedStableHash(),g2.GetDistributedStableHash());
      }
  }
}

