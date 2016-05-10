///*<FILE_LICENSE>
//* NFX (.NET Framework Extension) Unistack Library
//* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
//*
//* Licensed under the Apache License, Version 2.0 (the "License");
//* you may not use this file except in compliance with the License.
//* You may obtain a copy of the License at
//*
//* http://www.apache.org/licenses/LICENSE-2.0
//*
//* Unless required by applicable law or agreed to in writing, software
//* distributed under the License is distributed on an "AS IS" BASIS,
//* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//* See the License for the specific language governing permissions and
//* limitations under the License.
//</FILE_LICENSE>*/
//using System;
//using System.IO;
//using System.Text;
//using System.Linq;
//using NUnit.Framework;
//using NFX;
//using NFX.Web;
//using NFX.NUnit.Web.MultiPart;
//using System.Reflection;

//namespace NFX.NUnit.Web.MultiPart
//{
//  [TestFixture]
//  public class MultiPartContentTest
//  {
//    #region CONSTS

//      private static byte[] upload_img_bmp = getEmbeddedFileBytes("upload_img.bmp.dat");
//      private static byte[] upload_stream_2_bmp_files = getEmbeddedFileBytes("upload_stream_2_bmp_files.dat");
//      private static byte[] upload_stream_2_text_files_lf_txt = getEmbeddedFileBytes("upload_stream_2_text_files_lf.txt.dat");

//      private static byte[] upload_stream_bmp_and_text = getEmbeddedFileBytes("upload_stream_bmp_and_text.dat");
//      private const string upload_stream_bmp_and_text_content_type = "---------------------------7de31e2f5200d8";

//      private static byte[] upload_stream_inputs_and_bmp_lf_txt = getEmbeddedFileBytes("upload_stream_inputs_and_bmp_lf.txt.dat");

//      private static byte[] upload_stream_inputs_and_file = getEmbeddedFileBytes("upload_stream_inputs_and_file.dat");
//      private const string upload_stream_inputs_and_file_content_type = "---------------------------7de23d3b1d07fe";

//      private static byte[] upload_stream_inputs_and_test_chinese_txt = getEmbeddedFileBytes("upload_stream_inputs_and_test_chinese.txt.dat");
//      private static byte[] upload_stream_inputs_and_test_russian_txt = getEmbeddedFileBytes("upload_stream_inputs_and_test_russian.txt.dat");

//      private static byte[] upload_stream_2_text_files_txt = getEmbeddedFileBytes("upload_stream_2_text_files.txt.dat");
//      private const string upload_stream_2_text_files_txt_content_type = "---------------------------7de23d55002c0";
    
//      private static byte[] upload = getEmbeddedFileBytes("upload.txt");

//	  #endregion

//    #region Init

//      private static byte[] getEmbeddedFileBytes(string resourseName)
//      {
//        var resourceFullName = "Resources." + resourseName;
//        using (var stream = NFX.EmbeddedResource.GetBinaryStream(typeof(MultiPartContentTest), resourceFullName))
//        {
//          var buf = new byte[stream.Length];
//          stream.Read( buf, 0, buf.Length);
//          return buf;
//        }
//      }
      
//    #endregion

//    #region Public

//      #region Decode
//        [Test]
//        public void ParseTwoText()
//        {
//          using (var stream = new MemoryStream(upload_stream_2_text_files_txt))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(2, mp.Parts.Count);

//            Assert.AreEqual("content1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[0].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[0].Parameters.ContentType);

//            Assert.AreEqual("content2", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[1].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[1].Parameters.ContentType);
//          }
//        }

//        [Test]
//        public void ParseInputsAndText_New()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_file))
//          {
//            string boundary;
//            Encoding encoding = Encoding.UTF8;
//            var mp = Multipart.ReadFromStream(stream, out boundary, encoding);

//            Assert.AreEqual(3, mp.Parts.Count);

//            Assert.AreEqual("abba", encoding.GetString(mp.Parts["text1"].Content));
//            Assert.AreEqual("on", encoding.GetString(mp.Parts["check1"].Content));

//            Assert.AreEqual("upload.txt", mp.Parts["content2"].FileName);
//            Assert.AreEqual("text/plain", mp.Parts["content2"].ContentType);
//            Assert.IsTrue(upload.SequenceEqual(mp.Parts["content2"].Content));
//          }
//        }

//        [Test]
//        public void ParseBmpAndText()
//        {
//          using (var stream = new MemoryStream(upload_stream_bmp_and_text))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(2, mp.Parts.Count);

//            Assert.AreEqual("content1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("upload_img.bmp", mp.Parts[0].Parameters.FileName);
//            Assert.AreEqual("image/bmp", mp.Parts[0].Parameters.ContentType);
//            Assert.IsTrue(upload_img_bmp.SequenceEqual(mp.Parts[0].Content));

//            Assert.AreEqual("content2", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[1].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[1].Parameters.ContentType);
//            Assert.IsTrue(upload.SequenceEqual(mp.Parts[1].Content));
//          }
//        }

//        [Test]
//        public void ParseBmpAndBmp()
//        {
//          using (var stream = new MemoryStream(upload_stream_2_bmp_files))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(2, mp.Parts.Count);

//            Assert.AreEqual("content1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("upload_img.bmp", mp.Parts[0].Parameters.FileName);
//            Assert.AreEqual("image/bmp", mp.Parts[0].Parameters.ContentType);
//            Assert.IsTrue(upload_img_bmp.SequenceEqual(mp.Parts[0].Content));

//            Assert.AreEqual("content2", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("upload_img.bmp", mp.Parts[1].Parameters.FileName);
//            Assert.AreEqual("image/bmp", mp.Parts[1].Parameters.ContentType);
//            Assert.IsTrue(upload_img_bmp.SequenceEqual(mp.Parts[1].Content));
//          }
//        }

//        [Test]
//        public void ParseInputsAndText()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_file))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(3, mp.Parts.Count);

//            Assert.AreEqual("text1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("abba", mp.Parts[0].ContentAsString);

//            Assert.AreEqual("check1", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("on", mp.Parts[1].ContentAsString);

//            Assert.AreEqual("content2", mp.Parts[2].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[2].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[2].Parameters.ContentType);
//            Assert.IsTrue(upload.SequenceEqual(mp.Parts[2].Content));
//          }
//        }

//        [Test]
//        public void ParseTwoTextUnixEOL()
//        {
//          using (var stream = new MemoryStream(upload_stream_2_text_files_lf_txt))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(2, mp.Parts.Count);

//            Assert.AreEqual("content1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[0].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[0].Parameters.ContentType);

//            Assert.AreEqual("content2", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[1].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[1].Parameters.ContentType);
//          }
//        }

//        [Test]
//        public void ParseInputsAndTextUnixEOL()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_bmp_lf_txt))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(3, mp.Parts.Count);

//            Assert.AreEqual("text1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("abba", mp.Parts[0].ContentAsString);

//            Assert.AreEqual("check1", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("on", mp.Parts[1].ContentAsString);

//            Assert.AreEqual("content2", mp.Parts[2].Parameters.Name);
//            Assert.AreEqual("upload_img.bmp", mp.Parts[2].Parameters.FileName);
//            Assert.AreEqual("image/bmp", mp.Parts[2].Parameters.ContentType);
//            Assert.IsTrue(upload_img_bmp.SequenceEqual(mp.Parts[2].Content));
//          }
//        }

//        [Test]
//        public void ParseInputsAndTextChinese()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_test_chinese_txt))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(3, mp.Parts.Count);

//            Assert.AreEqual("text1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("我的书法情结", mp.Parts[0].ContentAsString);

//            Assert.AreEqual("check1", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("on", mp.Parts[1].ContentAsString);

//            Assert.AreEqual("content2", mp.Parts[2].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[2].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[2].Parameters.ContentType);
//            Assert.IsTrue(upload.SequenceEqual(mp.Parts[2].Content));
//          }
//        }

//        [Test]
//        public void ParseInputsAndTextRussian()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_test_russian_txt))
//          {
//            var mp = MultiPartContent.Decode(stream);

//            Assert.AreEqual(3, mp.Parts.Count);

//            Assert.AreEqual("text1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("фрайштадт", mp.Parts[0].ContentAsString);

//            Assert.AreEqual("check1", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("on", mp.Parts[1].ContentAsString);

//            Assert.AreEqual("content2", mp.Parts[2].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[2].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[2].Parameters.ContentType);
//            Assert.IsTrue(upload.SequenceEqual(mp.Parts[2].Content));
//          }
//        }

//        [Test]
//        public void ParseInputsAndTextEnsureBoundary()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_file))
//          {
//            var mp = MultiPartContent.Decode(stream);
//            Assert.AreEqual("---------------------------7de23d3b1d07fe", mp.Boundary);
//          }
//        }

//        [Test]
//        public void ParseInputsAndTextExplicitBoundary()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_file))
//          {
//            var mp = MultiPartContent.Decode(stream, "---------------------------7de23d3b1d07fe");
//            Assert.AreEqual("---------------------------7de23d3b1d07fe", mp.Boundary);

//            Assert.AreEqual(3, mp.Parts.Count);

//            Assert.AreEqual("text1", mp.Parts[0].Parameters.Name);
//            Assert.AreEqual("abba", mp.Parts[0].ContentAsString);

//            Assert.AreEqual("check1", mp.Parts[1].Parameters.Name);
//            Assert.AreEqual("on", mp.Parts[1].ContentAsString);

//            Assert.AreEqual("content2", mp.Parts[2].Parameters.Name);
//            Assert.AreEqual("upload.txt", mp.Parts[2].Parameters.FileName);
//            Assert.AreEqual("text/plain", mp.Parts[2].Parameters.ContentType);
//            Assert.IsTrue(upload.SequenceEqual(mp.Parts[2].Content));
//          }
//        }

//        [Test]
//        [ExpectedException(typeof(NFXException))]
//        public void ParseInputsAndTextExplicitBoundaryNotMatch()
//        {
//          using (var stream = new MemoryStream(upload_stream_inputs_and_file))
//          {
//            var mp = MultiPartContent.Decode(stream, "---------------------------7de23d3b1d07fd");
//          }
//        } 

//      #endregion

//      #region Encode

//        [Test]
//        public void EncodeDecode_FieldSingleTest()
//        {
//          byte[] eol = new byte[] { 0x0D, 0x0A};

//          var part0 = NFX.Web.MultiPartContent.MultiPart.CreateField("text1", "abba", eol);
//          var parts = new NFX.Web.MultiPartContent.MultiPart[] { part0};

//          var mpc0 = MultiPartContent.Encode(parts);

//          var mpc1 = MultiPartContent.Decode(mpc0.Stream);

//          Assert.AreEqual(1, mpc1.Parts.Count);
//          Assert.AreEqual("text1", mpc1.Parts[0].Parameters.Name);
//          Assert.AreEqual("abba", mpc1.Parts[0].ContentAsString);
//        }

//        [Test]
//        public void EncodeDecode_FieldsAndFileTest()
//        {
//          byte[] eol = new byte[] { 0x0D, 0x0A};
//          byte[] contentBytes = upload;

//          var part0 = NFX.Web.MultiPartContent.MultiPart.CreateField("text1", "abba", eol);
//          var part1 = NFX.Web.MultiPartContent.MultiPart.CreateField("check1", "on", eol);
//          var part2 = NFX.Web.MultiPartContent.MultiPart.CreateFile("content2", "upload.txt", "text/plain", contentBytes, eol);
//          var parts = new NFX.Web.MultiPartContent.MultiPart[] { part0, part1, part2};

//          var mpc0 = MultiPartContent.Encode(parts);

//          var mpc1 = MultiPartContent.Decode(mpc0.Stream);

//          Assert.AreEqual(3, mpc1.Parts.Count);

//          Assert.AreEqual("text1", mpc1.Parts[0].Parameters.Name);
//          Assert.AreEqual("abba", mpc1.Parts[0].ContentAsString);

//          Assert.AreEqual("check1", mpc1.Parts[1].Parameters.Name);
//          Assert.AreEqual("on", mpc1.Parts[1].ContentAsString);

//          Assert.AreEqual("content2", mpc1.Parts[2].Parameters.Name);
//          Assert.AreEqual("upload.txt", mpc1.Parts[2].Parameters.FileName);
//          Assert.AreEqual("text/plain", mpc1.Parts[2].Parameters.ContentType);
//          Assert.IsTrue(mpc1.Parts[2].Content.SequenceEqual(contentBytes));
//        }

//        #region JSONDataMap

//          [Test]
//          public void ToJSONDataMap_BmpAndTextFiles()
//          {
//            using (var stream = new MemoryStream(upload_stream_bmp_and_text))
//            {
//              var contentType = "multipart/form-data; boundary={0}".Args(upload_stream_bmp_and_text_content_type);
//              var data = MultiPartContent.ToJSONDataMap(stream, contentType);

//              Assert.IsNotNull(data);
//              Assert.AreEqual(6, data.Count());

//              Assert.IsTrue(upload_img_bmp.SequenceEqual((byte[])data["content1"]));
//              Assert.AreEqual("upload_img.bmp", data["content1_filename"]);
//              Assert.AreEqual("image/bmp", data["content1_contenttype"]);

//              Assert.IsTrue(upload.SequenceEqual((byte[])data["content2"]));
//              Assert.AreEqual("upload.txt", data["content2_filename"]);
//              Assert.AreEqual("text/plain", data["content2_contenttype"]);
//            }
//          }

//          [Test]
//          public void ToJSONDataMap_FieldsAndFile()
//          {
//            using (var stream = new MemoryStream(upload_stream_inputs_and_file))
//            {
//              var contentType = "multipart/form-data; boundary={0}".Args(upload_stream_inputs_and_file_content_type);
//              var data = MultiPartContent.ToJSONDataMap(stream, contentType);

//              Assert.IsNotNull(data);
//              Assert.AreEqual(5, data.Count());

//              Assert.IsTrue(data.ContainsKey("text1"));
//              Assert.AreEqual("abba", data["text1"]);

//              Assert.IsTrue(data.ContainsKey("check1"));
//              Assert.AreEqual("on", data["check1"]);

//              Assert.IsTrue(data.ContainsKey("content2"));
//              Assert.IsTrue(upload.SequenceEqual((byte[])data["content2"]));
//            }
//          }
    
//        #endregion

//      #endregion

//    #endregion

//  }
//}
