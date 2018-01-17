/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NUnit.Framework;

using NFX.IO;

namespace NFX.NUnit.IO
{
    [TestFixture]  
    public class LEB128Tests
    {
        [TestCase]
        public void Basic_ULEBEncoding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteULEB128(0, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(0, buf[0]);
          Assert.AreEqual(0, buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(1, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(1, buf[0]);
          Assert.AreEqual(1, buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0x7f, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(0x7f, buf[0]);
          Assert.AreEqual(0x7f, buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0x80, out cnt);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0x80, buf[0]);
          Assert.AreEqual(0x01, buf[1]);
          Assert.AreEqual(0x80, buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0x81, out cnt);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0x81, buf[0]);
          Assert.AreEqual(0x01, buf[1]);
          Assert.AreEqual(0x81, buf.ReadULEB128());

          buf = new byte[16];
          buf.WriteULEB128(0xff, out cnt);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0xff, buf[0]);
          Assert.AreEqual(0x01, buf[1]);
          Assert.AreEqual(0xff, buf.ReadULEB128());

          
          buf = new byte[16];
          buf.WriteULEB128(0x101, out cnt);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0x81, buf[0]);
          Assert.AreEqual(0x02, buf[1]);
          Assert.AreEqual(0x101, buf.ReadULEB128());
          
        }

        [TestCase]
        public void Basic_ULEBEncoding_Padding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteULEB128(0, out cnt, padding: 1);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0x80, buf[0]);
          Assert.AreEqual(0x00, buf[1]);
          Assert.AreEqual(0, buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0, out cnt, padding: 2);
          Assert.AreEqual(3, cnt);
          Assert.AreEqual(0x80, buf[0]);
          Assert.AreEqual(0x80, buf[1]);
          Assert.AreEqual(0x00, buf[2]);
          Assert.AreEqual(0, buf.ReadULEB128());

          buf = new byte[16];
          buf.WriteULEB128(0x80, out cnt, padding: 2);
          Assert.AreEqual(4, cnt);
          Assert.AreEqual(0x80, buf[0]);
          Assert.AreEqual(0x81, buf[1]);
          Assert.AreEqual(0x80, buf[2]);
          Assert.AreEqual(0x00, buf[3]);
          Assert.AreEqual(0x80, buf.ReadULEB128());
        }



        [TestCase]
        public void Basic_SLEBEncoding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteSLEB128(0, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(0, buf[0]);
          Assert.AreEqual(0, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(1, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(1, buf[0]);
          Assert.AreEqual(1, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(-1, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(0x7f, buf[0]);
          Assert.AreEqual(-1, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(+63, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(0x3f, buf[0]);
          Assert.AreEqual(63, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(-63, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(0x41, buf[0]);
          Assert.AreEqual(-63, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-64, out cnt);
          Assert.AreEqual(1, cnt);
          Assert.AreEqual(0x40, buf[0]);
          Assert.AreEqual(-64, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-65, out cnt);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0xbf, buf[0]);
          Assert.AreEqual(0x7f, buf[1]);
          Assert.AreEqual(-65, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(64, out cnt);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0xc0, buf[0]);
          Assert.AreEqual(0x00, buf[1]);
          Assert.AreEqual(64, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-12345L, out cnt);
          Assert.AreEqual(3, cnt);
          Assert.AreEqual(0xc7, buf[0]);
          Assert.AreEqual(0x9f, buf[1]);
          Assert.AreEqual(0x7f, buf[2]);
          Assert.AreEqual(-12345L, buf.ReadSLEB128());
        }



        [TestCase]
        public void Basic_SLEBEncoding_Padding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteSLEB128(-1, out cnt, padding: 1);
          Assert.AreEqual(2, cnt);
          Assert.AreEqual(0x7f, buf[0]);
          Assert.AreEqual(0x00, buf[1]);
          Assert.AreEqual(-1, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(-129, out cnt, padding: 1);
          Assert.AreEqual(3, cnt);
          Assert.AreEqual(0xff, buf[0]);
          Assert.AreEqual(0x7e, buf[1]);
          Assert.AreEqual(0x00, buf[2]);
          Assert.AreEqual(-129, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-12345L, out cnt, padding: 2);
          Assert.AreEqual(5, cnt);
          Assert.AreEqual(0xc7, buf[0]);
          Assert.AreEqual(0x9f, buf[1]);
          Assert.AreEqual(0x7f, buf[2]);
          Assert.AreEqual(0x80, buf[3]);
          Assert.AreEqual(0x00, buf[4]);
          Assert.AreEqual(-12345L, buf.ReadSLEB128());
        }




        [TestCase]
        public void Basic_ULEBUsingStream()
        {
          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(1);
             ms.Position = 0;
             Assert.AreEqual(1, ms.ReadULEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(65);
             ms.Position = 0;
             Assert.AreEqual(65, ms.ReadULEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(0x45789878L);
             ms.Position = 0;
             Assert.AreEqual(0x45789878L, ms.ReadULEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(0xFA76f812638796f1UL);
             ms.Position = 0;
             Assert.AreEqual(0xFA76f812638796f1L, ms.ReadULEB128());
          }
        }


        [TestCase]
        public void Basic_SLEBUsingStream()
        {
          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(-1);
             ms.Position = 0;
             Assert.AreEqual(-1, ms.ReadSLEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(-65);
             ms.Position = 0;
             Assert.AreEqual(-65, ms.ReadSLEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(0x45789878L);
             ms.Position = 0;
             Assert.AreEqual(0x45789878L, ms.ReadSLEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(0x7A76f812638796f1L);
             ms.Position = 0;
             Assert.AreEqual(0x7A76f812638796f1L, ms.ReadSLEB128());
          }
        }



    }
}
