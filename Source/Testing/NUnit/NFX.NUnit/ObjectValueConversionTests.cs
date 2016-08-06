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
    public enum TestEnum{ A=0,B=123,C=234 }


    [TestFixture]
    public class ObjectValueConversionTests
    {
        [TestCase]
        public void AsLaconicConfig()
        {
          Assert.AreEqual(23, "value=23".AsLaconicConfig().AttrByName("value").ValueAsInt());
          Assert.AreEqual(223, "nfx{value=223}".AsLaconicConfig().AttrByName("value").ValueAsInt());
          Assert.IsNull("nfx{value 223}".AsLaconicConfig());

          try
          {
            "nfx".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
            Assert.Fail("No exception");
          }
          catch
          {
            Assert.Pass("Got exception as expected");
          }
        }

        [TestCase]
        public void AsJSONConfig()
        {
          Assert.AreEqual(23, "{value: 23}".AsJSONConfig().AttrByName("value").ValueAsInt());
          Assert.AreEqual(223, "{nfx: {value: 223}}".AsJSONConfig().AttrByName("value").ValueAsInt());
          Assert.IsNull("{ bad }".AsJSONConfig());

          try
          {
            "bad".AsJSONConfig(handling: ConvertErrorHandling.Throw);
            Assert.Fail("No exception");
          }
          catch
          {
            Assert.Pass("Got exception as expected");
          }
        }

        [TestCase]
        public void FromInt()
        {
            object obj = 123;

            Assert.AreEqual(123, obj.AsShort());
            Assert.AreEqual(123, obj.AsNullableShort());

            Assert.AreEqual(123, obj.AsUShort());
            Assert.AreEqual(123, obj.AsNullableUShort());

            Assert.AreEqual(123, obj.AsInt());
            Assert.AreEqual(123, obj.AsNullableInt());

            Assert.AreEqual(123, obj.AsUInt());
            Assert.AreEqual(123, obj.AsNullableUInt());

            Assert.AreEqual(123L, obj.AsLong());
            Assert.AreEqual(123L, obj.AsNullableLong());

            Assert.AreEqual(123UL, obj.AsULong());
            Assert.AreEqual(123UL, obj.AsNullableULong());

            Assert.AreEqual(123d, obj.AsDouble());
            Assert.AreEqual(123d, obj.AsNullableDouble());

            Assert.AreEqual(123f, obj.AsFloat());
            Assert.AreEqual(123f, obj.AsNullableFloat());

            Assert.AreEqual(123m, obj.AsDecimal());
            Assert.AreEqual(123m, obj.AsNullableDecimal());

            Assert.AreEqual(true, obj.AsBool());
            Assert.AreEqual(true, obj.AsNullableBool());

            Assert.AreEqual("123", obj.AsString());
            Assert.AreEqual(123, obj.AsDateTime().Ticks);
            Assert.AreEqual(123, obj.AsTimeSpan().Ticks);

            Assert.AreEqual(TestEnum.B, obj.AsEnum(TestEnum.A));

        }


        [TestCase]
        public void FromLong()
        {
            object obj = 123L;

            Assert.AreEqual(123, obj.AsShort());
            Assert.AreEqual(123, obj.AsNullableShort());

            Assert.AreEqual(123, obj.AsUShort());
            Assert.AreEqual(123, obj.AsNullableUShort());

            Assert.AreEqual(123, obj.AsInt());
            Assert.AreEqual(123, obj.AsNullableInt());

            Assert.AreEqual(123, obj.AsUInt());
            Assert.AreEqual(123, obj.AsNullableUInt());

            Assert.AreEqual(123L, obj.AsLong());
            Assert.AreEqual(123L, obj.AsNullableLong());

            Assert.AreEqual(123UL, obj.AsULong());
            Assert.AreEqual(123UL, obj.AsNullableULong());

            Assert.AreEqual(123d, obj.AsDouble());
            Assert.AreEqual(123d, obj.AsNullableDouble());

            Assert.AreEqual(123f, obj.AsFloat());
            Assert.AreEqual(123f, obj.AsNullableFloat());

            Assert.AreEqual(123m, obj.AsDecimal());
            Assert.AreEqual(123m, obj.AsNullableDecimal());

            Assert.AreEqual(true, obj.AsBool());
            Assert.AreEqual(true, obj.AsNullableBool());

            Assert.AreEqual("123", obj.AsString());
            Assert.AreEqual(123, obj.AsDateTime().Ticks);
            Assert.AreEqual(123, obj.AsTimeSpan().Ticks);

            Assert.AreEqual(TestEnum.B, obj.AsEnum(TestEnum.A));

        }

        [TestCase]
        public void FromShort()
        {
            short s = 123;
            object obj = s;

            Assert.AreEqual(123, obj.AsShort());
            Assert.AreEqual(123, obj.AsNullableShort());

            Assert.AreEqual(123, obj.AsUShort());
            Assert.AreEqual(123, obj.AsNullableUShort());

            Assert.AreEqual(123, obj.AsInt());
            Assert.AreEqual(123, obj.AsNullableInt());

            Assert.AreEqual(123, obj.AsUInt());
            Assert.AreEqual(123, obj.AsNullableUInt());

            Assert.AreEqual(123L, obj.AsLong());
            Assert.AreEqual(123L, obj.AsNullableLong());

            Assert.AreEqual(123UL, obj.AsULong());
            Assert.AreEqual(123UL, obj.AsNullableULong());

            Assert.AreEqual(123d, obj.AsDouble());
            Assert.AreEqual(123d, obj.AsNullableDouble());

            Assert.AreEqual(123f, obj.AsFloat());
            Assert.AreEqual(123f, obj.AsNullableFloat());

            Assert.AreEqual(123m, obj.AsDecimal());
            Assert.AreEqual(123m, obj.AsNullableDecimal());

            Assert.AreEqual(true, obj.AsBool());
            Assert.AreEqual(true, obj.AsNullableBool());

            Assert.AreEqual("123", obj.AsString());

            Assert.AreEqual(TestEnum.B, obj.AsEnum(TestEnum.A));

        }


        [TestCase]
        public void FromDouble()
        {
            object obj = 123d;

            Assert.AreEqual(123, obj.AsShort());
            Assert.AreEqual(123, obj.AsNullableShort());

            Assert.AreEqual(123, obj.AsUShort());
            Assert.AreEqual(123, obj.AsNullableUShort());

            Assert.AreEqual(123, obj.AsInt());
            Assert.AreEqual(123, obj.AsNullableInt());

            Assert.AreEqual(123, obj.AsUInt());
            Assert.AreEqual(123, obj.AsNullableUInt());

            Assert.AreEqual(123L, obj.AsLong());
            Assert.AreEqual(123L, obj.AsNullableLong());

            Assert.AreEqual(123UL, obj.AsULong());
            Assert.AreEqual(123UL, obj.AsNullableULong());

            Assert.AreEqual(123d, obj.AsDouble());
            Assert.AreEqual(123d, obj.AsNullableDouble());

            Assert.AreEqual(123f, obj.AsFloat());
            Assert.AreEqual(123f, obj.AsNullableFloat());

            Assert.AreEqual(123m, obj.AsDecimal());
            Assert.AreEqual(123m, obj.AsNullableDecimal());

            Assert.AreEqual(true, obj.AsBool());
            Assert.AreEqual(true, obj.AsNullableBool());

            Assert.AreEqual("123", obj.AsString());
            Assert.AreEqual(123, obj.AsDateTime().Ticks);
            Assert.AreEqual(123, obj.AsTimeSpan().Ticks);

            Assert.AreEqual(TestEnum.B, obj.AsEnum(TestEnum.A));

        }

        [TestCase]
        public void FromDecimal()
        {
            object obj = 123m;

            Assert.AreEqual(123, obj.AsShort());
            Assert.AreEqual(123, obj.AsNullableShort());

            Assert.AreEqual(123, obj.AsUShort());
            Assert.AreEqual(123, obj.AsNullableUShort());

            Assert.AreEqual(123, obj.AsInt());
            Assert.AreEqual(123, obj.AsNullableInt());

            Assert.AreEqual(123, obj.AsUInt());
            Assert.AreEqual(123, obj.AsNullableUInt());

            Assert.AreEqual(123L, obj.AsLong());
            Assert.AreEqual(123L, obj.AsNullableLong());

            Assert.AreEqual(123UL, obj.AsULong());
            Assert.AreEqual(123UL, obj.AsNullableULong());

            Assert.AreEqual(123d, obj.AsDouble());
            Assert.AreEqual(123d, obj.AsNullableDouble());

            Assert.AreEqual(123f, obj.AsFloat());
            Assert.AreEqual(123f, obj.AsNullableFloat());

            Assert.AreEqual(123m, obj.AsDecimal());
            Assert.AreEqual(123m, obj.AsNullableDecimal());

            Assert.AreEqual(true, obj.AsBool());
            Assert.AreEqual(true, obj.AsNullableBool());

            Assert.AreEqual("123", obj.AsString());
            Assert.AreEqual(123, obj.AsDateTime().Ticks);
            Assert.AreEqual(123, obj.AsTimeSpan().Ticks);

            Assert.AreEqual(TestEnum.B, obj.AsEnum(TestEnum.A));

        }

        [TestCase]
        public void FromString()
        {
            object obj = "123";

            Assert.AreEqual(123, obj.AsShort());
            Assert.AreEqual(123, obj.AsNullableShort());

            Assert.AreEqual(123, obj.AsUShort());
            Assert.AreEqual(123, obj.AsNullableUShort());

            Assert.AreEqual(123, obj.AsInt());
            Assert.AreEqual(123, obj.AsNullableInt());

            Assert.AreEqual(123, obj.AsInt());
            Assert.AreEqual(123, obj.AsNullableUInt());

            Assert.AreEqual(123L, obj.AsLong());
            Assert.AreEqual(123L, obj.AsNullableLong());

            Assert.AreEqual(123UL, obj.AsULong());
            Assert.AreEqual(123UL, obj.AsNullableULong());

            Assert.AreEqual(123d, obj.AsDouble());
            Assert.AreEqual(123d, obj.AsNullableDouble());

            Assert.AreEqual(123f, obj.AsFloat());
            Assert.AreEqual(123f, obj.AsNullableFloat());

            Assert.AreEqual(123m, obj.AsDecimal());
            Assert.AreEqual(123m, obj.AsNullableDecimal());

            Assert.AreEqual(true, obj.AsBool());
            Assert.AreEqual(true, obj.AsNullableBool());

            Assert.AreEqual("123", obj.AsString());
            Assert.AreEqual(123, obj.AsDateTime().Ticks);
            Assert.AreEqual(123, obj.AsTimeSpan().Ticks);

            Assert.AreEqual(TestEnum.B, obj.AsEnum(TestEnum.A));

        }


         [TestCase]
        public void FromBool()
        {
            object obj = true;

            Assert.AreEqual(1, obj.AsShort());
            Assert.AreEqual(1, obj.AsNullableShort());

            Assert.AreEqual(1, obj.AsInt());
            Assert.AreEqual(1, obj.AsNullableInt());

            Assert.AreEqual(1d, obj.AsDouble());
            Assert.AreEqual(1d, obj.AsNullableDouble());

            Assert.AreEqual(1f, obj.AsFloat());
            Assert.AreEqual(1f, obj.AsNullableFloat());

            Assert.AreEqual(1m, obj.AsDecimal());
            Assert.AreEqual(1m, obj.AsNullableDecimal());

            Assert.AreEqual(true, obj.AsBool());
            Assert.AreEqual(true, obj.AsNullableBool());

            Assert.AreEqual("True", obj.AsString());
            Assert.AreEqual(new DateTime(2001,1,1), obj.AsDateTime(new DateTime(2001,1,1)));
            Assert.AreEqual(1, obj.AsTimeSpan().Ticks);


        }


        [TestCase]
        public void FromNull()
        {
            object obj = null;

            Assert.AreEqual(0, obj.AsShort());
            Assert.AreEqual(null, obj.AsNullableShort());

            Assert.AreEqual(0, obj.AsUShort());
            Assert.AreEqual(null, obj.AsNullableUShort());

            Assert.AreEqual(0, obj.AsInt());
            Assert.AreEqual(null, obj.AsNullableInt());

            Assert.AreEqual(0, obj.AsUInt());
            Assert.AreEqual(null, obj.AsNullableUInt());

            Assert.AreEqual(0L, obj.AsLong());
            Assert.AreEqual(null, obj.AsNullableLong());

            Assert.AreEqual(0UL, obj.AsULong());
            Assert.AreEqual(null, obj.AsNullableULong());

            Assert.AreEqual(0d, obj.AsDouble());
            Assert.AreEqual(null, obj.AsNullableDouble());

            Assert.AreEqual(0f, obj.AsFloat());
            Assert.AreEqual(null, obj.AsNullableFloat());

            Assert.AreEqual(0m, obj.AsDecimal());
            Assert.AreEqual(null, obj.AsNullableDecimal());

            Assert.AreEqual(false, obj.AsBool());
            Assert.AreEqual(null, obj.AsNullableBool());

            Assert.AreEqual(null, obj.AsString());
            Assert.AreEqual(null, obj.AsNullableDateTime());
            Assert.AreEqual(null, obj.AsNullableTimeSpan());

            Assert.AreEqual(null, obj.AsNullableEnum<TestEnum>());

        }

        [TestCase]
        public void FromNullWithDifferentDefaults()
        {
            object obj = null;

            Assert.AreEqual(5, obj.AsShort(5));
            Assert.AreEqual(null, obj.AsNullableShort(5));

            Assert.AreEqual(5, obj.AsUShort(5));
            Assert.AreEqual(null, obj.AsNullableUShort(5));

            Assert.AreEqual(6, obj.AsInt(6));
            Assert.AreEqual(null, obj.AsNullableInt(6));

            Assert.AreEqual(6, obj.AsUInt(6));
            Assert.AreEqual(null, obj.AsNullableUInt(6));

            Assert.AreEqual(6L, obj.AsLong(6L));
            Assert.AreEqual(null, obj.AsNullableLong(6L));

            Assert.AreEqual(6, obj.AsULong(6));
            Assert.AreEqual(null, obj.AsNullableULong(6));

            Assert.AreEqual(7d, obj.AsDouble(7));
            Assert.AreEqual(null, obj.AsNullableDouble(7));

            Assert.AreEqual(8f, obj.AsFloat(8));
            Assert.AreEqual(null, obj.AsNullableFloat(8));

            Assert.AreEqual(9m, obj.AsDecimal(9));
            Assert.AreEqual(null, obj.AsNullableDecimal(9));

            Assert.AreEqual(true, obj.AsBool(true));
            Assert.AreEqual(null, obj.AsNullableBool(true));

            Assert.AreEqual("yez!", obj.AsString("yez!"));
            Assert.AreEqual(new DateTime(1921, 04,07), obj.AsDateTime(new DateTime(1921, 04,07)));
            Assert.AreEqual(null, obj.AsNullableDateTime(new DateTime(1921, 04,07)));
            Assert.AreEqual(TimeSpan.FromHours(12.5), obj.AsTimeSpan( TimeSpan.FromHours(12.5)));
            Assert.AreEqual(null, obj.AsNullableTimeSpan( TimeSpan.FromHours(12.5)));

            Assert.AreEqual(TestEnum.C, obj.AsEnum<TestEnum>(TestEnum.C));
            Assert.AreEqual(null, obj.AsNullableEnum<TestEnum>(TestEnum.C));

        }


        [TestCase]
        public void Unsigned()
        {
            object obj = "127";

            Assert.AreEqual(127, obj.AsByte());
            Assert.AreEqual(127, obj.AsUShort());
            Assert.AreEqual(127, obj.AsUInt());
            Assert.AreEqual(127, obj.AsULong());
        }

        [TestCase]
        public void GUID_1()
        {
            object obj = "{CF04F818-6194-48C3-B618-8965ACA4D229}";

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }

        [TestCase]
        public void GUID_2()
        {
            object obj = "CF04F818-6194-48C3-B618-8965ACA4D229";

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }

        [TestCase]
        public void GUID_3()
        {
            object obj = "sfsadfsd fsdafsd CF04F818-6194-48C3-B618-8965ACA4D229";

            Assert.AreEqual(Guid.Empty, obj.AsGUID(Guid.Empty));
        }

        [TestCase]
        public void GUID_4()
        {
            object obj = "CF04F818619448C3B6188965ACA4D229";

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }


        [TestCase]
        public void GUID_5()
        {
            object obj = new Guid("CF04F818-6194-48C3-B618-8965ACA4D229");

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }


        [TestCase]
        public void NullableGUID_1()
        {
            object obj = "{CF04F818-6194-48C3-B618-8965ACA4D229}";

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }

        [TestCase]
        public void NullableGUID_2()
        {
            object obj = "CF04F818-6194-48C3-B618-8965ACA4D229";

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }

        [TestCase]
        public void NullableGUID_3()
        {
            object obj = "sfsadfsd fsdafsd CF04F818-6194-48C3-B618-8965ACA4D229";

            Assert.IsNull(obj.AsNullableGUID());
        }

        [TestCase]
        public void NullableGUID_4()
        {
            object obj = "sfsadfsd fsdafsd CF04F818-6194-48C3-B618-8965ACA4D229";

            Assert.AreEqual(Guid.Empty, obj.AsNullableGUID(Guid.Empty));
        }

        [TestCase]
        public void NullableGUID_5()
        {
            object obj = "CF04F818619448C3B6188965ACA4D229";

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }

        [TestCase]
        public void NullableGUID_6()
        {
            object obj = new Guid("CF04F818-6194-48C3-B618-8965ACA4D229");

            Assert.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }


        [TestCase]
        public void GDID()
        {
            object obj = new GDID(3,4,5);

            Assert.AreEqual(obj, obj.AsGDID());
            Assert.AreEqual(obj, obj.AsGDID(new GDID(2,3,4)));


            Assert.AreEqual(obj, "3:4:5".AsGDID(new GDID(2,3,4)));
            Assert.AreEqual(new GDID(2,3,4), "3rewtfef:4:5".AsGDID(new GDID(2,3,4)));

            try
            {
              "3rewtfef:4:5".AsGDID(new GDID(2,3,4), handling: ConvertErrorHandling.Throw);
              Assert.Fail("No execpetion");
            }
            catch
            {
              Assert.Pass();
            }
        }

        [TestCase]
        public void NullableGDID()
        {
            object obj = new GDID(3,4,5);

            Assert.AreEqual(obj, obj.AsNullableGDID());
            Assert.AreEqual(obj, obj.AsNullableGDID(new GDID(2,3,4)));


            Assert.AreEqual(obj, "3:4:5".AsNullableGDID(new GDID(2,3,4)));
            object on = null;
            Assert.IsNull( on.AsNullableGDID() );

            Assert.IsNull( on.AsNullableGDID(new GDID(3,4,5)));

            Assert.AreEqual(obj, "fdwsfsdfds".AsNullableGDID(new GDID(3,4,5)));
        }


        [TestCase]
        public void GDIDSymbol()
        {
            object obj = new GDIDSymbol(new GDID(3,4,5), "ABC");

            Assert.AreEqual(obj, obj.AsGDIDSymbol());
            Assert.AreEqual(obj, obj.AsGDIDSymbol(new GDIDSymbol(new GDID(23,14,15), "ABC")));


            var link = new ELink(new GDID(4,12,8721));


            Assert.AreEqual(link.AsGDIDSymbol, link.Link.AsGDIDSymbol());
            Assert.AreEqual(link.AsGDIDSymbol, "3rewtfef:4:5".AsGDIDSymbol(link.AsGDIDSymbol()));

            try
            {
              "3rewtfef:4:5".AsGDIDSymbol(link.AsGDIDSymbol, handling: ConvertErrorHandling.Throw);
              Assert.Fail("No excepetion");
            }
            catch
            {
              Assert.Pass();
            }
        }


        [TestCase]
        public void NullableGDIDSymbol()
        {
            var obj = new GDIDSymbol(new GDID(3,4,5), "3:4:5");

            Assert.AreEqual(obj, obj.AsNullableGDIDSymbol());
            Assert.AreEqual(obj, obj.AsNullableGDIDSymbol(new GDIDSymbol(new GDID(13,14,15), "ABC")));


            Assert.AreEqual(obj, "3:4:5".AsNullableGDIDSymbol(new GDIDSymbol(new GDID(13,14,15), "ABC")));
            object on = null;
            Assert.IsNull( on.AsNullableGDIDSymbol() );

            Assert.IsNull( on.AsNullableGDIDSymbol(obj));

            Assert.AreEqual(obj, "fdwsfsdfds".AsNullableGDIDSymbol(obj));
        }

        [TestCase]
        public void Uri()
        {
          Assert.AreEqual(null, new { _ = "" }.AsUri());

          Assert.Throws<NFXException>(() => NFX.DataAccess.Distributed.GDID.Zero.AsUri(handling: ConvertErrorHandling.Throw), "GDID.AsUri");

          object obj = "https://example.com";

          Assert.AreEqual(new Uri("https://example.com"), obj.AsUri());
        }
    }
}
