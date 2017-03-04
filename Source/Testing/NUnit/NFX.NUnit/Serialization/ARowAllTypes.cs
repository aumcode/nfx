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
using System.IO;
using System.Diagnostics;
using System.Reflection;

using NUnit.Framework;

using NFX.IO;
using NFX.Serialization.Arow;
using NFX.Serialization.JSON;
using NFX.ApplicationModel;
using NFX.Financial;
using NFX.DataAccess.Distributed;
using NFX.ApplicationModel.Pile;

namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class ARowAllTypes
    {
      [TestFixtureSetUp]
      public void Setup()
      {
        ArowSerializer.RegisterTypeSerializationCores( Assembly.GetExecutingAssembly() );
      }

      private static readonly AllArowTypesRow[] CASES = new []
      {
        new AllArowTypesRow(),
        new AllArowTypesRow(){Bool1=true, Bool2=null},
        new AllArowTypesRow(){Bool1=false, Bool2=true},
        new AllArowTypesRow(){Bool1=false, Bool2=true, Bool3=new bool[]{true, false, true}},
        new AllArowTypesRow(){Bool1=false, Bool2=true, Bool4=new List<bool>{false, true, false, true}},

        new AllArowTypesRow(){Char1='a', Char2='x'},
        new AllArowTypesRow(){Char1='a', Char2=null, Char3=new char[]{'x','y','z'}},
        new AllArowTypesRow(){Char1='a', Char2=null, Char4=new List<char>{'x','y','z'}},

        new AllArowTypesRow(){String1="abba"},
        new AllArowTypesRow(){String1="dad", String3=new string[]{"werwer","werwerwerwer","bnmfg"}},
        new AllArowTypesRow(){String1="dad", String3=new string[]{null,null,"bnmfg"}},
        new AllArowTypesRow(){String1="bug", String4=new List<string>{"asaaa", null,"wweer"}},

        new AllArowTypesRow(){Float1=12.3f, Float2=null},
        new AllArowTypesRow(){Float1=3423.455f, Float2=56456.989f},
        new AllArowTypesRow(){Float1=324.234f, Float2=23423f, Float3=new float[]{0,0,234.23f}},
        new AllArowTypesRow(){Float1=123.12f, Float2=33.34f, Float4=new List<float>{1f,3f,90.34f,23.2f}},

        new AllArowTypesRow(){Double1=12.3d, Double2=null},
        new AllArowTypesRow(){Double1=3423.455d, Double2=56456.989d},
        new AllArowTypesRow(){Double1=324.234d, Double2=23423d, Double3=new double[]{0,0,234.23d}},
        new AllArowTypesRow(){Double1=123.12d, Double2=33.34d, Double4=new List<double>{1d,3d,90.34d,23.2d}},


        new AllArowTypesRow(){Decimal1=12.3m, Decimal2=null},
        new AllArowTypesRow(){Decimal1=3423.455m, Decimal2=56456.989m},
        new AllArowTypesRow(){Decimal1=324.234m, Decimal2=23423m, Decimal3=new decimal[]{0,0,234.23m,}},
        new AllArowTypesRow(){Decimal1=123.12m, Decimal2=33.34m, Decimal4=new List<decimal>{1m,3m,90.34m,23.2m}},

        new AllArowTypesRow(){Amount1=new Amount("aaa", 12.3m), Amount2=null},
        new AllArowTypesRow(){Amount1=new Amount("aaa", 12.3m), Amount2=new Amount("bbb", 12.3m)},
        new AllArowTypesRow(){Amount1=new Amount("awa", 212.3m), Amount2=new Amount("zxc", 212.3m), Amount3=new Amount[]{new Amount("aaa", 212.3m),new Amount("bbb", 312.3m)}},
        new AllArowTypesRow(){Amount1=new Amount("aqa", 4412.3m), Amount2=new Amount("zxc", 212.3m), Amount4=new List<Amount>{new Amount("aaa", 212.3m),new Amount("bbb", 312.3m)}},

        new AllArowTypesRow(){Byte1=123, Byte2=null},
        new AllArowTypesRow(){Byte1=2, Byte2=23},
        new AllArowTypesRow(){Byte1=34, Byte2=23, Byte3=new byte[]{0,0,1,2,3,4,5,6,7,8,9,198}},
        new AllArowTypesRow(){Byte1=76, Byte2=32, Byte4=new List<byte>{9,9,23,43,3,189,245}},

        new AllArowTypesRow(){SByte1=123, SByte2=null},
        new AllArowTypesRow(){SByte1=2,   SByte2=23},
        new AllArowTypesRow(){SByte1=34,  SByte2=23, SByte3=new sbyte[]{0,0,-1,2,3,4,5,6,-7,8,9,23}},
        new AllArowTypesRow(){SByte1=76,  SByte2=32, SByte4=new List<sbyte>{9,9,23,43,3,-123,-11}},

        new AllArowTypesRow(){Short1=123,Short2=null},
        new AllArowTypesRow(){Short1=2,  Short2=23},
        new AllArowTypesRow(){Short1=34, Short2=23, Short3=new short[]{0,30,1,2,3,4,532,6,7,8,9,198}},
        new AllArowTypesRow(){Short1=76, Short2=32, Short4=new List<short>{-39,-32767,23,43,3,189,245}},

        new AllArowTypesRow(){UShort1=123, UShort2=null},
        new AllArowTypesRow(){UShort1=2,   UShort2=23},
        new AllArowTypesRow(){UShort1=34,  UShort2=23, UShort3=new ushort[]{0,0,1,2,3,4,5,6,7,8,9,23}},
        new AllArowTypesRow(){UShort1=76,  UShort2=32, UShort4=new List<ushort>{9,9,23,43,3,323,65535}},


        new AllArowTypesRow(){Int1=123,Int2=null},
        new AllArowTypesRow(){Int1=2,  Int2=23},
        new AllArowTypesRow(){Int1=34, Int2=23, Int3=new int[]{0,0,-1,2,3,4,5,6,7,8,9,23}},
        new AllArowTypesRow(){Int1=76, Int2=32, Int4=new List<int>{9,9,23,43,3,-323,-2365535}},

        new AllArowTypesRow(){UInt1=123,UInt2=null},
        new AllArowTypesRow(){UInt1=2,  UInt2=23},
        new AllArowTypesRow(){UInt1=34, UInt2=23, UInt3=new uint[]{0,0,1,2,3,4,5,6,7,8,9,23}},
        new AllArowTypesRow(){UInt1=76, UInt2=32, UInt4=new List<uint>{9,9,23,43,3,323,65535}},


        new AllArowTypesRow(){Long1=123,Long2=null},
        new AllArowTypesRow(){Long1=2,  Long2=23},
        new AllArowTypesRow(){Long1=34, Long2=23, Long3=new long[]{0,0,-14,2,3,4,5,6,7,8,9,23}},
        new AllArowTypesRow(){Long1=76, Long2=32, Long4=new List<long>{9,9,23,43,3,-323,-2365243535}},

        new AllArowTypesRow(){ULong1=123,ULong2=null},
        new AllArowTypesRow(){ULong1=2,  ULong2=23},
        new AllArowTypesRow(){ULong1=34, ULong2=23, ULong3=new ulong[]{0,0,14,2,3,4,5,6,7,8,9,23}},
        new AllArowTypesRow(){ULong1=76, ULong2=32, ULong4=new List<ulong>{9,9,23,43,3,323,2365243535}},

        new AllArowTypesRow(){DateTime1= DateTime.Now, DateTime2=null},
        new AllArowTypesRow(){DateTime1= DateTime.Now, DateTime2=DateTime.Now},
        new AllArowTypesRow(){DateTime1=DateTime.Now, DateTime2=DateTime.Now, DateTime3=new DateTime[]{DateTime.Now,DateTime.Now}},
        new AllArowTypesRow(){DateTime1=DateTime.Now, DateTime2=DateTime.Now, DateTime4=new List<DateTime>{DateTime.Now,DateTime.Now,DateTime.Now}},

        new AllArowTypesRow(){TimeSpan1= new TimeSpan(23333),  TimeSpan2=null},
        new AllArowTypesRow(){TimeSpan1= new TimeSpan(23333),  TimeSpan2=new TimeSpan(4332)},
        new AllArowTypesRow(){TimeSpan1= new TimeSpan(23333),  TimeSpan2=new TimeSpan(4332), TimeSpan3=new TimeSpan[]{new TimeSpan(2323), new TimeSpan(32432)}},
        new AllArowTypesRow(){TimeSpan1= new TimeSpan(23333),  TimeSpan2=new TimeSpan(4332), TimeSpan4=new List<TimeSpan>{new TimeSpan(1234), new TimeSpan(23423), new TimeSpan(23423)}},

        new AllArowTypesRow(){Guid1= Guid.NewGuid(),  Guid2=null},
        new AllArowTypesRow(){Guid1= Guid.NewGuid(),  Guid2=Guid.NewGuid()},
        new AllArowTypesRow(){Guid1= Guid.NewGuid(),  Guid2=Guid.NewGuid(), Guid3=new Guid[]{Guid.NewGuid(),Guid.NewGuid()}},
        new AllArowTypesRow(){Guid1= Guid.NewGuid(),  Guid2=Guid.NewGuid(), Guid4=new List<Guid>{Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid()}},


        new AllArowTypesRow(){GDID1= new GDID(32,2335532),  GDID2=null},
        new AllArowTypesRow(){GDID1= new GDID(32,2323432),  GDID2= new GDID(32,2323432)},
        new AllArowTypesRow(){GDID1= new GDID(32,2233432),  GDID2= new GDID(32,2233432), GDID3=new GDID[]{new GDID(32,232), new GDID(2, 876)}},
        new AllArowTypesRow(){GDID1= new GDID(32,2323352),  GDID2= new GDID(32,2323352), GDID4=new List<GDID>{new GDID(3652,232),new GDID(432,232),new GDID(312,232)}},


        new AllArowTypesRow(){Fid1= FID.Generate(),  Fid2=null},
        new AllArowTypesRow(){Fid1= FID.Generate(),  Fid2=FID.Generate()},
        new AllArowTypesRow(){Fid1= FID.Generate(),  Fid2=FID.Generate(), Fid3=new FID[]{FID.Generate(),FID.Generate()}},
        new AllArowTypesRow(){Fid1= FID.Generate(),  Fid2=FID.Generate(), Fid4=new List<FID>{FID.Generate(),FID.Generate(),FID.Generate()}},


        new AllArowTypesRow(){PilePointer1= new PilePointer(32,26335532),  PilePointer2=null},
        new AllArowTypesRow(){PilePointer1= new PilePointer(32,278323432),  PilePointer2= new PilePointer(32,23223432)},
        new AllArowTypesRow(){PilePointer1= new PilePointer(32,22633432),  PilePointer2= new PilePointer(32,22336432), PilePointer3=new PilePointer[]{new PilePointer(32,23382), new PilePointer(2, 8746)}},
        new AllArowTypesRow(){PilePointer1= new PilePointer(32,233323352),  PilePointer2= new PilePointer(32,23263352), PilePointer4=new List<PilePointer>{new PilePointer(3652,2232),new PilePointer(432,2432),new PilePointer(3132,23532)}},

        new AllArowTypesRow(){NLS1= new NLSMap("{eng:{n: 1, d: 1}}"),  NLS2=null},
        new AllArowTypesRow(){NLS1= new NLSMap("{eng:{n: 1, d: 3}}"),  NLS2= new NLSMap("{eng:{n: 1, d: 1}}")},
        new AllArowTypesRow(){NLS1= new NLSMap("{eng:{n: 1, d: 4}}"),  NLS2= new NLSMap("{eng:{n: 2, d: 2}}"), NLS3=new NLSMap[]{new NLSMap("{eng:{n: 12, d: 13}}"), new NLSMap("{eng:{n: 231, d: 21}}")}},
        new AllArowTypesRow(){NLS1= new NLSMap("{eng:{n: 1, d: 5}}"),  NLS2= new NLSMap("{eng:{n: 3, d: 3}}"), NLS4=new List<NLSMap>{new NLSMap("{eng:{n: 14, d: 13}}"),new NLSMap("{eng:{n: 21, d: 21}}"),new NLSMap("{eng:{n: 21, d: 3331}}")}},

        new AllArowTypesRow{ RowArray = new []{ new AllArowTypesRow{ Bool1=true, Double2=21331.23d}, new AllArowTypesRow{ Bool2=false, Double1=21331.23d} }},

        new AllArowTypesRow{ RowList = new List<AllArowTypesRow>{ new AllArowTypesRow{ Bool1=true, Double2=21331.23d}, new AllArowTypesRow{ Bool2=false, Double1=21331.23d} }},

        new AllArowTypesRow
        {
          Bool1 = true,
          Bool2 = null,
          Bool3 = new bool[]{true, false, true, true},
          Bool4=new List<bool>{false, true, false, true},
          Char1='a', Char2='x', Char3=new char[]{'x','y','z'}, Char4=new List<char>{'x','y','z','e','y'},
          String1="wjeirjwejrwjqnjcpwiojernpiqfojpowijrfoijwneoifjrqjer",
          String3=null,
          String4=new List<string>{"sss", "rftttt"},
          Float1=123.12f, Float2=33.34f, Float4=new List<float>{1f,3f,90.34f,23.2f},
          Double1=123.12d, Double2=33.34d, Double4=new List<double>{1d,3d,90.34d,23.2d},
          Decimal1=123.12m, Decimal2=33.34m, Decimal4=new List<decimal>{1m,3m,90.34m,23.2m},
          Amount1=new Amount("aqa", 4412.3m), Amount2=new Amount("zxc", 212.3m), Amount4=new List<Amount>{new Amount("aaa", 212.3m),new Amount("bbb", 312.3m)},
          Byte1=76, Byte2=32, Byte4=new List<byte>{9,9,23,43,3,189,245},
          SByte1=76,  SByte2=32, SByte4=new List<sbyte>{9,9,23,43,3,-123,-11},
          Short1=76, Short2=32, Short4=new List<short>{-39,-32767,23,43,3,189,245},
          UShort1=76,  UShort2=32, UShort4=new List<ushort>{9,9,23,43,3,323,65535},
          Int1=76, Int2=32, Int4=new List<int>{9,9,23,43,3,-323,-2365535},
          UInt1=76, UInt2=32, UInt4=new List<uint>{9,9,23,43,3,323,65535},
          Long1=76, Long2=32, Long4=new List<long>{9,9,23,43,3,-323,-2365243535},
          ULong1=76, ULong2=32, ULong4=new List<ulong>{9,9,23,43,3,323,2365243535},
          DateTime1=DateTime.Now, DateTime2=DateTime.Now, DateTime4=new List<DateTime>{DateTime.Now,DateTime.Now,DateTime.Now},
          TimeSpan1= new TimeSpan(23333),  TimeSpan2=new TimeSpan(4332), TimeSpan4=new List<TimeSpan>{new TimeSpan(1234), new TimeSpan(23423), new TimeSpan(23423)},
          Guid1= Guid.NewGuid(),  Guid2=Guid.NewGuid(), Guid4=new List<Guid>{Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid()},
          GDID1= new GDID(32,2323352),  GDID2= new GDID(32,2323352), GDID4=new List<GDID>{new GDID(3652,232),new GDID(432,232),new GDID(312,232)},
          Fid1= FID.Generate(),  Fid2=FID.Generate(), Fid4=new List<FID>{FID.Generate(),FID.Generate(),FID.Generate()},
          PilePointer1= new PilePointer(32,233323352),  PilePointer2= new PilePointer(32,23263352), PilePointer4=new List<PilePointer>{new PilePointer(3652,2232),new PilePointer(432,2432),new PilePointer(3132,23532)},
          NLS1= new NLSMap("{eng:{n: 1, d: 5}}"),  NLS2= new NLSMap("{eng:{n: 3, d: 3}}"), NLS4=new List<NLSMap>{new NLSMap("{eng:{n: 14, d: 13}}"),new NLSMap("{eng:{n: 21, d: 21}}"),new NLSMap("{eng:{n: 21, d: 3331}}")},
          RowArray = new []{ new AllArowTypesRow{ Bool1=true, Double2=21331.23d}, new AllArowTypesRow{ Bool2=false, Double1=21331.23d} },
          RowList = new List<AllArowTypesRow>{ new AllArowTypesRow{ Bool1=true, Double2=21331.23d}, new AllArowTypesRow{ Bool2=false, Double1=21331.23d} }
        },

      };


      [Test]
      public void Test_AllFieldsEqual()
      {
        var r1 = new AllArowTypesRow{ Bool3=new bool[]{true, false, true}};
        var r2 = new AllArowTypesRow{ Bool3=new bool[]{true, false}};
        Aver.IsFalse( r1.AllFieldsEqual(r2) );

        r1 = new AllArowTypesRow{ Bool3=new bool[]{true, false}};
        r2 = new AllArowTypesRow{ Bool3=new bool[]{true, false, true}};
        Aver.IsFalse( r1.AllFieldsEqual(r2) );

        r1 = new AllArowTypesRow{ Bool4=new List<bool>{true, false, true}};
        r2 = new AllArowTypesRow{ Bool4=new List<bool>{false}};
        Aver.IsFalse( r1.AllFieldsEqual(r2) );

        r1 = new AllArowTypesRow{ Bool4=new List<bool>{true}};
        r2 = new AllArowTypesRow{ Bool4=new List<bool>{false}};
        Aver.IsFalse( r1.AllFieldsEqual(r2) );

        r1 = new AllArowTypesRow{ Bool4=new List<bool>{true}};
        r2 = new AllArowTypesRow{ Bool4=new List<bool>{true}};
        Aver.IsTrue( r1.AllFieldsEqual(r2) );
      }


      [Test]
      public void OneByOne()
      {
        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        var fail = "";
        using(var ms = new MemoryStream())
        {
          for(var i=0; i<CASES.Length; i++)
          {
            var row1 = CASES[i];

            Console.WriteLine("Test #{0}".Args(i));
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine(row1.ToJSON());


            ms.Position = 0;

            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new AllArowTypesRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            if (!row1.AllFieldsEqual( row2 ))
            {
              Console.WriteLine(row2.ToJSON());
              Console.WriteLine("FAIL");
              fail =  "The test case #{0} has failed.\nJSON:\n{1}".Args(i, row1.ToJSON());
            }
            else
              Console.WriteLine("PASS");

            Console.WriteLine();
          }
          if (fail.IsNotNullOrWhiteSpace())
            Aver.Fail(fail);
        }
      }


    }
}
