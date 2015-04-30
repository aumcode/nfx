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
using System.Linq;
using System.Text;

using NUnit.Framework;


using NFX.ApplicationModel;
using NFX.Log;

namespace NFX.NUnit.Config
{
    [TestFixture]
    public class ValueAccessors
    {

    static string conf = @"
 test
 {
   vInt1=123
   vInt2=-123
   vDouble=-123.8002341
   vDecimal=123000456.1233
   vHex=0xABAB
   vBin=0b1010101001010101 //AA55
   vBool=true
   vStr=$'My
   name 
   spanning many lines'
   vDate=12/10/2014
   vGuid1='{3A7C4641-B24E-453D-9D28-93D96071B575}'
   vGuid2='3A7C4641-B24E-453D-9D28-93D96071B575'
   vGuid3='3A7C4641B24E453D9D2893D96071B575'
   vBuffer=fa,CA,dA,Ba
 }
";

        [TestCase]
        public void Ints()
        {
            var root = conf.AsLaconicConfig(handling: ConvertErrorHandling.Throw);//throw needed so we can see error (if any) while processing config
                                                                                  // instead of just getting null back
            
            Assert.AreEqual(123, root.AttrByName("vInt1").ValueAsInt());
            Assert.AreEqual(-123, root.AttrByName("vInt2").ValueAsInt());

        }

        [TestCase]
        public void Doubles()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(-123.8002341d, root.AttrByName("vDouble").ValueAsDouble());
        }

        [TestCase]
        public void Decimals()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(123000456.1233M, root.AttrByName("vDecimal").ValueAsDecimal());
        }

        [TestCase]
        public void HexIntegers()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(0xabab, root.AttrByName("vHex").ValueAsUShort());
            Assert.AreEqual(0xabab, root.AttrByName("vHex").ValueAsUInt());
            Assert.AreEqual(0xabab, root.AttrByName("vHex").ValueAsULong());
        }

        [TestCase]
        public void BinIntegers()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(0xaa55, root.AttrByName("vBin").ValueAsUShort());
            Assert.AreEqual(0xaa55, root.AttrByName("vBin").ValueAsUInt());
            Assert.AreEqual(0xaa55, root.AttrByName("vBin").ValueAsULong());
        }

        [TestCase]
        public void Bools()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(true, root.AttrByName("vBool").ValueAsBool());
        }

        [TestCase]
        public void Strs()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(@"My
   name 
   spanning many lines", root.AttrByName("vStr").ValueAsString());
        }

        [TestCase]
        public void Dates()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(2014, root.AttrByName("vDate").ValueAsDateTime(DateTime.Now).Year);
            Assert.AreEqual(12, root.AttrByName("vDate").ValueAsDateTime(DateTime.Now).Month);
        }

        [TestCase]
        public void Guids()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.AreEqual(new Guid("3A7C4641B24E453D9D2893D96071B575"), root.AttrByName("vGUID1").ValueAsGUID(Guid.Empty));
            Assert.AreEqual(new Guid("3A7C4641B24E453D9D2893D96071B575"), root.AttrByName("vGUID2").ValueAsGUID(Guid.Empty));
            Assert.AreEqual(new Guid("3A7C4641B24E453D9D2893D96071B575"), root.AttrByName("vGUID3").ValueAsGUID(Guid.Empty));
        }

        [TestCase]
        public void ByteArray()
        {
            var root = conf.AsLaconicConfig();
            
            Assert.IsTrue(new byte[]{0xFA, 0xCA, 0xDA, 0xBA}.SequenceEqual(  root.AttrByName("vBuffer").ValueAsByteArray() ) );
        }

   }//class

}
