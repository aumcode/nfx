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
using NFX.Glue.Native;
using System.IO;
using NFX.Glue;
using NFX.IO;

namespace NFX.NUnit.Glue
{
    [TestFixture]
    public class WireFrameTests
    {
        [TestCase]
        public void Glue_SerializeDeserialize()
        {
            var frm1 = new WireFrame(123, false, FID.Generate());

            Assert.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();
            
            Assert.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);
            
            Assert.AreEqual( frm1.Type, frm2.Type );
            Assert.AreEqual( frm1.RequestID, frm2.RequestID );
            Assert.AreEqual( frm1.OneWay, frm2.OneWay );

            Assert.AreEqual( frm1.Length, frm2.Length );
            Assert.AreEqual( frm1.Format, frm2.Format );
            Assert.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Assert.IsFalse( frm2.OneWay );

        }

        [TestCase]
        public void Glue_SerializeDeserialize_WithHeadersWithLatinText()
        {
            var hdr = "<a><remote name='zzz'/></a>";//Latin only chars
            
            var frm1 = new WireFrame(123, false, FID.Generate(), hdr);


            var utfLen = WireFrame.HEADERS_ENCODING.GetByteCount( hdr );
            
            Assert.IsTrue( utfLen == hdr.Length);

            Assert.AreEqual( WireFrame.FRAME_LENGTH + hdr.Length, frm1.Length );

            var ms = new MemoryStream();
            
            Assert.AreEqual(WireFrame.FRAME_LENGTH + hdr.Length, frm1.Serialize(ms));
            
            ms.Position = 0;

            var frm2 = new WireFrame(ms);
            
            Assert.AreEqual( frm1.Type, frm2.Type );
            Assert.AreEqual( frm1.RequestID, frm2.RequestID );
            Assert.AreEqual( frm1.OneWay, frm2.OneWay );

            Assert.AreEqual( frm1.Length, frm2.Length );
            Assert.AreEqual( frm1.Format, frm2.Format );
            Assert.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Assert.IsFalse( frm2.OneWay );
            
            Assert.AreEqual( "zzz", frm2.Headers["remote"].AttrByName("name").Value);
        }

        [TestCase]
        public void Glue_SerializeDeserialize_WithHeadersWithChineseText()
        {
            var hdr = "<a><remote name='久有归天愿'/></a>";
            
            var frm1 = new WireFrame(123, false, FID.Generate(), hdr);

            
            var utfLen = WireFrame.HEADERS_ENCODING.GetByteCount( hdr );
            
            Assert.IsTrue( utfLen > hdr.Length);
            Console.WriteLine("{0} has {1} byte len and {2} char len".Args(hdr, utfLen, hdr.Length) );

            Assert.AreEqual( WireFrame.FRAME_LENGTH + utfLen, frm1.Length );

            var ms = new MemoryStream();

            Assert.AreEqual(WireFrame.FRAME_LENGTH + utfLen, frm1.Serialize(ms));
            
            ms.Position = 0;

            var frm2 = new WireFrame(ms);
            
            Assert.AreEqual( frm1.Type, frm2.Type );
            Assert.AreEqual( frm1.RequestID, frm2.RequestID );
            Assert.AreEqual( frm1.OneWay, frm2.OneWay );

            Assert.AreEqual( frm1.Length, frm2.Length );
            Assert.AreEqual( frm1.Format, frm2.Format );
            Assert.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Assert.IsFalse( frm2.OneWay );
            
            Assert.AreEqual( "久有归天愿", frm2.Headers["remote"].AttrByName("name").Value);
        }



        [TestCase]
        public void Echo_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.Echo, 123, true, FID.Generate());
            Assert.IsFalse( frm1.OneWay );
            Assert.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();
            
            Assert.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);
            
            Assert.AreEqual( frm1.Type, frm2.Type );
            Assert.AreEqual( frm1.RequestID, frm2.RequestID );
            Assert.AreEqual( frm1.OneWay, frm2.OneWay );

            Assert.AreEqual( frm1.Length, frm2.Length );
            Assert.AreEqual( frm1.Format, frm2.Format );
            Assert.AreEqual( frm1.HeadersContent, frm2.HeadersContent );
            
            Assert.IsFalse( frm2.OneWay );
        }

        [TestCase]
        public void Dummy_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.Dummy, 123, false, FID.Generate());
            Assert.IsTrue( frm1.OneWay );
            Assert.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();
            
            Assert.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);
            
            Assert.AreEqual( frm1.Type, frm2.Type );
            Assert.AreEqual( frm1.RequestID, frm2.RequestID );
            Assert.AreEqual( frm1.OneWay, frm2.OneWay );

            Assert.AreEqual( frm1.Length, frm2.Length );
            Assert.AreEqual( frm1.Format, frm2.Format );
            Assert.AreEqual( frm1.HeadersContent, frm2.HeadersContent );
            
            Assert.IsTrue( frm2.OneWay );
        }

        [TestCase]
        public void EchoResponse_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.EchoResponse, 123, false, FID.Generate());
            Assert.IsTrue( frm1.OneWay );
            Assert.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();
            
            Assert.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);
            
            Assert.AreEqual( frm1.Type, frm2.Type );
            Assert.AreEqual( frm1.RequestID, frm2.RequestID );
            Assert.AreEqual( frm1.OneWay, frm2.OneWay );

            Assert.AreEqual( frm1.Length, frm2.Length );
            Assert.AreEqual( frm1.Format, frm2.Format );
            Assert.AreEqual( frm1.HeadersContent, frm2.HeadersContent );
            
            Assert.IsTrue( frm2.OneWay );
        }

        [TestCase]
        public void HeartBeat_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.Heartbeat, 123, false, FID.Generate());
            Assert.IsTrue( frm1.OneWay );
            Assert.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();
            
            Assert.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);
            
            Assert.AreEqual( frm1.Type, frm2.Type );
            Assert.AreEqual( frm1.RequestID, frm2.RequestID );
            Assert.AreEqual( frm1.OneWay, frm2.OneWay );

            Assert.AreEqual( frm1.Length, frm2.Length );
            Assert.AreEqual( frm1.Format, frm2.Format );
            Assert.AreEqual( frm1.HeadersContent, frm2.HeadersContent );
            
            Assert.IsTrue( frm2.OneWay );
        }


    }
}
