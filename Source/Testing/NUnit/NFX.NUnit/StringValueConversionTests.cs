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
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using NFX;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit
{

    [TestFixture]
    public class StringValueConversionTests
    {
        [TestCase]
        public void GDID()
        {
           var link1 = new ELink(new GDID(5,1,2));
           var link2 = new ELink(new GDID(0,1,2));


           Assert.AreEqual( new GDID(5, 1, 2),  "5:1:2".AsGDID());
           Assert.AreEqual( new GDID(5, 1, 2),  "5:1:2".AsType(typeof(GDID)));
           Assert.AreEqual( new GDID(5, 1, 2),  link1.Link.AsType(typeof(GDID), false));


           Assert.AreEqual( new GDID(0, 1, 2),  "0:1:2".AsGDID());
           Assert.AreEqual( new GDID(0, 1, 2),  "0:1:2".AsType(typeof(GDID)));
           Assert.AreEqual( new GDID(0, 1, 2),  link2.Link.AsType(typeof(GDID), false));

           string ns = null;
           Assert.IsNull( ns.AsNullableGDID());
           Assert.IsNull( ns.AsNullableGDID(new GDID(7, 8, 9)));
           Assert.AreEqual( new GDID(7,8,9), "dewsdfwefwerf".AsNullableGDID(new GDID(7, 8, 9)));
        }

        [TestCase]
        public void GDIDSymbol()
        {
           var link1 = new ELink(new GDID(5,1,2));
           var link2 = new ELink(new GDID(0,1,2));

           Assert.AreEqual( new GDIDSymbol(new GDID(5, 1, 2), "5:1:2"),  "5:1:2".AsGDIDSymbol());
           Assert.AreEqual( new GDIDSymbol(new GDID(5, 1, 2), "5:1:2"),  "5:1:2".AsType(typeof(GDIDSymbol)));
           Assert.AreEqual( link1.AsGDIDSymbol, link1.Link.AsType(typeof(GDIDSymbol), false));


           Assert.AreEqual( new GDIDSymbol(new GDID(0, 1, 2), "0:1:2"),  "0:1:2".AsGDIDSymbol());
           Assert.AreEqual( new GDIDSymbol(new GDID(0, 1, 2), "0:1:2"),  "0:1:2".AsType(typeof(GDIDSymbol)));
           Assert.AreEqual( link2.AsGDIDSymbol, link2.Link.AsType(typeof(GDIDSymbol), false));

           string ns = null;
           Assert.IsNull( ns.AsNullableGDIDSymbol());
           Assert.IsNull( ns.AsNullableGDIDSymbol(new GDIDSymbol(new GDID(7, 8, 9), "AAA")));
           Assert.AreEqual( new GDIDSymbol(new GDID(7,8,9), "AAA"), "wdef8we9f9u8".AsNullableGDIDSymbol(new GDIDSymbol(new GDID(7, 8, 9), "AAA")));
        }

        [TestCase]
        public void GDIDSymbol_from_string()
        {
           //1280:2:8902382::'AUCKIR-ABASVITILT'
           var slink = "AUCKIR-ABASVITILT";
           var gs = slink.AsGDIDSymbol();
           Assert.AreEqual(new GDID(1280, 2, 8902382), gs.GDID);
           Assert.AreEqual(slink, gs.Symbol);

           var gs2 = slink.AsType(typeof(GDIDSymbol?), false);
           Assert.AreEqual(new GDID(1280, 2, 8902382), ((GDIDSymbol)gs2).GDID);
           Assert.AreEqual(slink, ((GDIDSymbol)gs2).Symbol);
        }

         [TestCase]
        public void GUID()
        {
           Assert.AreEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "CF04F818-6194-48C3-B618-8965ACA4D229".AsGUID(Guid.Empty));
           Assert.AreEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "{CF04F818-6194-48C3-B618-8965ACA4D229}".AsGUID(Guid.Empty));
           Assert.AreEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "CF04F818619448C3B6188965ACA4D229".AsGUID(Guid.Empty));
           Assert.AreEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "CF04F818-6194-48C3-B618-8965ACA4D229".AsType(typeof(Guid)));
        }

        [TestCase]
        public void ByteArray()
        {
           Assert.IsTrue( new byte[]{0x2f, 0x3d, 0xea, 0x22}.SequenceEqual("2f,3d,ea,22".AsByteArray()));
           Assert.IsTrue( new byte[]{1,2,3,4,5,6,7,8,9,0}.SequenceEqual("1,2,3,4, 5,    6,7,8,9, 0".AsByteArray()));
        }


        [TestCase]
        public void Int()
        {
           Assert.AreEqual( 10,  "10".AsShort());
           Assert.AreEqual( 10,  "10".AsUShort());
           Assert.AreEqual( 10,  "10".AsInt());
           Assert.AreEqual( 10,  "10".AsUInt());
           Assert.AreEqual( 10,  "10".AsLong());
           Assert.AreEqual( 10,  "10".AsULong());
           Assert.AreEqual( 10f, "10".AsFloat());
           Assert.AreEqual( 10d, "10".AsDouble());
           Assert.AreEqual( 10m, "10".AsDecimal());

           Assert.AreEqual( 10,  "10".AsType(typeof(short)));
           Assert.AreEqual( 10,  "10".AsType(typeof(ushort)));
           Assert.AreEqual( 10,  "10".AsType(typeof(int)));
           Assert.AreEqual( 10,  "10".AsType(typeof(uint)));
           Assert.AreEqual( 10,  "10".AsType(typeof(long)));
           Assert.AreEqual( 10,  "10".AsType(typeof(ulong)));
           Assert.AreEqual( 10f, "10".AsType(typeof(float)));
           Assert.AreEqual( 10d, "10".AsType(typeof(double)));
           Assert.AreEqual( 10m, "10".AsType(typeof(decimal)));
        }

        [TestCase]
        public void Int_Negative()
        {
           Assert.AreEqual( -10,  "-10".AsShort());
           Assert.AreEqual( 0,  "-10".AsUShort());
           Assert.AreEqual( -10,  "-10".AsInt());
           Assert.AreEqual( 0,  "-10".AsUInt());
           Assert.AreEqual( -10,  "-10".AsLong());
           Assert.AreEqual( 0,  "-10".AsULong());
           Assert.AreEqual( -10f, "-10".AsFloat());
           Assert.AreEqual( -10d, "-10".AsDouble());
           Assert.AreEqual( -10m, "-10".AsDecimal());

           Assert.AreEqual( -10,  "-10".AsType(typeof(short)));
           Assert.AreEqual( -10,  "-10".AsType(typeof(int)));
           Assert.AreEqual( -10f, "-10".AsType(typeof(float)));
           Assert.AreEqual( -10d, "-10".AsType(typeof(double)));
           Assert.AreEqual( -10m, "-10".AsType(typeof(decimal)));
        }

        [TestCase]
        public void Bool()
        {
           Assert.AreEqual( true, "1".AsBool());
           Assert.AreEqual( true, "yes".AsBool());
           Assert.AreEqual( true, "YES".AsBool());
           Assert.AreEqual( true, "true".AsBool());
           Assert.AreEqual( true, "True".AsBool());
           Assert.AreEqual( true, "TRUE".AsBool());
           Assert.AreEqual( true, "on".AsBool());
           Assert.AreEqual( true, "ON".AsBool());
        }

        [TestCase]
        public void Double()
        {
           Assert.AreEqual( 10f, "10.0".AsFloat());
           Assert.AreEqual( 10d, "10.0".AsDouble());
           Assert.AreEqual( 10m, "10.0".AsDecimal());

           Assert.AreEqual( 10f, "10.0".AsType(typeof(float)));
           Assert.AreEqual( 10d, "10.0".AsType(typeof(double)));
           Assert.AreEqual( 10m, "10.0".AsType(typeof(decimal)));
        }

        [TestCase]
        public void Double_Negative()
        {
           Assert.AreEqual( -10f, "-10.0".AsFloat());
           Assert.AreEqual( -10d, "-10.0".AsDouble());
           Assert.AreEqual( -10m, "-10.0".AsDecimal());

           Assert.AreEqual( -10f, "-10.0".AsType(typeof(float)));
           Assert.AreEqual( -10d, "-10.0".AsType(typeof(double)));
           Assert.AreEqual( -10m, "-10.0".AsType(typeof(decimal)));
        }


        [TestCase]
        public void DateTime()
        {
           Assert.AreEqual( new DateTime(1953, 12, 10, 14, 0, 0, DateTimeKind.Local), "12/10/1953 14:00:00".AsDateTime());
           Assert.AreEqual( new DateTime(1953, 12, 10, 14, 0, 0, DateTimeKind.Local), "Dec 10 1953 14:00:00".AsDateTime());
        }

        [TestCase]
        public void ISODateTime()
        {
           Assert.AreEqual( new DateTime(1953, 12, 10, 1,  0, 0, DateTimeKind.Utc), "1953-12-10T03:00:00+02:00".AsDateTime().ToUniversalTime());
        }


        [TestCase]
        public void Defaults_BadData()
        {
           string data = "dsifj f80q9w8";

           Assert.AreEqual( 0,  data.AsShort());
           Assert.AreEqual( 0,  data.AsInt());
           Assert.AreEqual( 0f, data.AsFloat());
           Assert.AreEqual( 0d, data.AsDouble());
           Assert.AreEqual( 0m, data.AsDecimal());

           Assert.AreEqual( 0,  data.AsType(typeof(short),   false));
           Assert.AreEqual( 0,  data.AsType(typeof(int),     false));
           Assert.AreEqual( 0f, data.AsType(typeof(float),   false));
           Assert.AreEqual( 0d, data.AsType(typeof(double),  false));
           Assert.AreEqual( 0m, data.AsType(typeof(decimal), false));

           Assert.AreEqual( 0,  data.AsNullableShort());
           Assert.AreEqual( 0,  data.AsNullableInt());
           Assert.AreEqual( 0f, data.AsNullableFloat());
           Assert.AreEqual( 0d, data.AsNullableDouble());
           Assert.AreEqual( 0m, data.AsNullableDecimal());

           Assert.AreEqual( 0,  data.AsType(typeof(short?),   false));
           Assert.AreEqual( 0,  data.AsType(typeof(int?),     false));
           Assert.AreEqual( 0f, data.AsType(typeof(float?),   false));
           Assert.AreEqual( 0d, data.AsType(typeof(double?),  false));
           Assert.AreEqual( 0m, data.AsType(typeof(decimal?), false));
        }

        [TestCase]
        public void Defaults_StringNull()
        {
           string data = null;
           Assert.AreEqual( 0,  data.AsShort());
           Assert.AreEqual( 0,  data.AsInt());
           Assert.AreEqual( 0f, data.AsFloat());
           Assert.AreEqual( 0d, data.AsDouble());
           Assert.AreEqual( 0m, data.AsDecimal());

           Assert.AreEqual( 0,  data.AsType(typeof(short),   false));
           Assert.AreEqual( 0,  data.AsType(typeof(int),     false));
           Assert.AreEqual( 0f, data.AsType(typeof(float),   false));
           Assert.AreEqual( 0d, data.AsType(typeof(double),  false));
           Assert.AreEqual( 0m, data.AsType(typeof(decimal), false));

           Assert.AreEqual( null, data.AsNullableShort());
           Assert.AreEqual( null, data.AsNullableInt());
           Assert.AreEqual( null, data.AsNullableFloat());
           Assert.AreEqual( null, data.AsNullableDouble());
           Assert.AreEqual( null, data.AsNullableDecimal());

           Assert.AreEqual( null, data.AsType(typeof(short?),   false));
           Assert.AreEqual( null, data.AsType(typeof(int?),     false));
           Assert.AreEqual( null, data.AsType(typeof(float?),   false));
           Assert.AreEqual( null, data.AsType(typeof(double?),  false));
           Assert.AreEqual( null, data.AsType(typeof(decimal?), false));

        }


        [TestCase]
        public void AsTypeSpeed()
        {
           var CNT = 1000000;

           var sw = System.Diagnostics.Stopwatch.StartNew();

           for(var i=0; i<CNT; i++)
           {
              "123".AsType(typeof(int), false);
              "123".AsType(typeof(int), true);
              "123".AsType(typeof(double), false);
              "123".AsType(typeof(double), true);
              "123".AsType(typeof(string), false);
              "123".AsType(typeof(bool), false);
              "123".AsType(typeof(decimal), false);
              "123".AsType(typeof(decimal), true);

              "123".AsType(typeof(int?), false);
              "123".AsType(typeof(int?), true);
              "123".AsType(typeof(double?), false);
              "123".AsType(typeof(double?), true);
              "123".AsType(typeof(bool?), false);
              "123".AsType(typeof(decimal?), false);
              "123".AsType(typeof(decimal?), true);
           }


           sw.Stop();
           Console.WriteLine("Did {0} in {1}ms at {2}/sec".Args(CNT, sw.ElapsedMilliseconds, (int)(CNT / (sw.ElapsedMilliseconds/1000d))));

        }


        [TestCase]
        public void HexNumbers()
        {
            Assert.AreEqual(0xed, "0xed".AsByte());
            Assert.AreEqual(0xed, "0xEd".AsByte());
            Assert.AreEqual(0xed, "0xED".AsByte());

            Assert.AreEqual(0x10ba, "0x10BA".AsShort());
            Assert.AreEqual(0x10ba, "0x10BA".AsUShort());

            Assert.AreEqual(0xdd10BA, "0xdd10BA".AsInt());
            Assert.AreEqual(0xdd10BA, "0xdd10BA".AsUInt());

            Assert.AreEqual(0x40001020f0a0f0ba, "0x40001020f0a0f0ba".AsLong());
            Assert.AreEqual(0x40001020f0a0f0ba, "0x40001020f0a0f0ba".AsULong());

            Assert.AreEqual(0xA0001020f0a0f0ba, "0xA0001020f0a0f0ba".AsULong());
        }

        [TestCase]
        public void BinNumbers()
        {
            Assert.AreEqual(0xA2, "0b10100010".AsByte());

            Assert.AreEqual(0xA2A2, "0b1010001010100010".AsUShort());

            Assert.AreEqual(0xaa55aa55, "0b10101010010101011010101001010101".AsUInt());

            Assert.AreEqual(0xaa55aa55aa55aa55, "0b1010101001010101101010100101010110101010010101011010101001010101".AsULong());

        }


        [TestCase]
        public void Uri()
        {
          Assert.AreEqual(new Uri("https://example.org/absolute/URI/resource.txt"), "https://example.org/absolute/URI/resource.txt".AsUri());
          Assert.AreEqual(new Uri("ftp://example.org/resource.txt"), "ftp://example.org/resource.txt".AsUri());
          Assert.AreEqual(new Uri("urn:ISSN:1535–3613"), "urn:ISSN:1535–3613".AsType(typeof(Uri)));
          Assert.AreEqual(null, "   ".AsType(typeof(Uri)));

          Assert.Throws<UriFormatException>(() => "resource.txt".AsUri(handling: ConvertErrorHandling.Throw));

          var uri = "schema://username:password@example.com:123/path/data?key=value#fragid".AsUri();
          Assert.AreEqual(uri.Scheme, "schema");
          Assert.AreEqual(uri.Authority, "example.com:123");
          Assert.AreEqual(uri.UserInfo, "username:password");
          Assert.AreEqual(uri.Host, "example.com");
          Assert.AreEqual(uri.Port, 123);
          Assert.AreEqual(uri.PathAndQuery, "/path/data?key=value");
          Assert.AreEqual(uri.AbsolutePath, "/path/data");
          Assert.AreEqual(uri.Query, "?key=value");
          Assert.AreEqual(uri.Fragment, "#fragid");
        }
    }
}
