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
