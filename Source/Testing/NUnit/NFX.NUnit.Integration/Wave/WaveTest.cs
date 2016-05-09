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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

using NUnit.Framework;

using NFX;
using NFX.Serialization.JSON;
using NFX.DataAccess.CRUD;
using TestRow = WaveTestSite.Controllers.IntegrationTester.TestRow;
using TestStatus = WaveTestSite.Controllers.IntegrationTester.TestStatus;
using TestComplexRow = WaveTestSite.Controllers.IntegrationTester.TestComplexRow;

namespace NFX.NUnit.Integration.Wave
{
  [TestFixture]
  public class WaveTest: WaveTestBase
  {
    #region Consts

      private const string EXPECTED_400 = "400 expected but wasn't thrown";
      private const string EXPECTED_404 = "404 expected but wasn't thrown";

      private const string USER_ID = "dxw";
      private const string USER_PWD = "thejake";
      private const string USER_NAME = "Denis";
      private const string USER_STATUS = "User";

    #endregion

    #region Tests

      #region Action

        [Test]
        public void Action_Empty()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.DownloadString(INTEGRATION_HTTP_ADDR + "Empty");
            Assert.AreEqual(string.Empty, res);
          }
        }

        [Test]
        public void Action_Action1Name()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.DownloadString(INTEGRATION_HTTP_ADDR + "ActionName1");
            Assert.AreEqual("ActionName1", res);
          }
        }

        [Test]
        public void Action_Action0Name()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              wc.DownloadString(INTEGRATION_HTTP_ADDR + "ActionName0");
              throw new Exception(EXPECTED_404);
            }
            catch (WebException ex)
            {
              Assert.IsTrue( Is404(ex));
            }
          }
        }

        [Test]
        public void Action_ActionPost_Found()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + "ActionPost", string.Empty);
            Assert.AreEqual("ActionPost", res);
          }
        }

        [Test]
        public void Action_ActionPost_NotFound()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              wc.DownloadString(INTEGRATION_HTTP_ADDR + "ActionPost");
              throw new Exception(EXPECTED_404);
            }
            catch (WebException ex)
            {
              Assert.IsTrue( Is404(ex));
            }
          }
        }

        [Test]
        public void Action_IsLocalAction()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.DownloadString(INTEGRATION_HTTP_ADDR + "IsLocalAction");
            Assert.AreEqual("IsLocalAction", res);
          }
        }

        [Test]
        public void Action_IsNotLocalAction()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              wc.DownloadString(INTEGRATION_HTTP_ADDR + "IsNotLocalAction");
              throw new Exception(EXPECTED_404);
            }
            catch (WebException ex)
            {
              Assert.IsTrue( Is404(ex));
            }
          }
        }

        [Test]
        public void Action_GetSetTimeSpan()
        {
          var initTs = TimeSpan.FromDays(10);

          using (var wc = CreateWebClient())
          {
            var values = new NameValueCollection();
            values.Add("ts", initTs.AsString());

            byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "GetSetTimeSpan", values);
            string str = GetUTF8StringWOBOM(bytes);

            var gotTs = TimeSpan.Parse(str);

            Assert.AreEqual(initTs.Add(TimeSpan.FromDays(1)), gotTs);
          }
        }

        [Test]
        public void Action_Add_BothArgs()
        {
          int a = 3, b = 14;

          using (var wc = CreateWebClient())
          {
            var values = new NameValueCollection();
            values.Add("a", a.AsString());
            values.Add("b", b.AsString());

            byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
            string str = Encoding.ASCII.GetString(bytes);
            int sum = str.AsInt();
            Assert.AreEqual(a + b, sum);
          }
        }

        [Test]
        public void Action_Add_InsufficientArgs()
        {
          int a = 3, b = 14;

          using (var wc = CreateWebClient())
          {
            {
              var values = new NameValueCollection();
              values.Add("a", a.AsString());

              byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
              string str = Encoding.ASCII.GetString(bytes);
              int sum = str.AsInt();
              Assert.AreEqual(a, sum);
            }

            {
              var values = new NameValueCollection();
              values.Add("b", b.AsString());

              byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
              string str = Encoding.ASCII.GetString(bytes);
              int sum = str.AsInt();
              Assert.AreEqual(b, sum);
            }

            {
              var values = new NameValueCollection();

              byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
              string str = Encoding.ASCII.GetString(bytes);
              int sum = str.AsInt();
              Assert.AreEqual(0, sum);
            }
          }
        }

        [Test]
        public void Action_Add_DefaultArgs()
        {
          int a = 5, b = 7;

          using (var wc = CreateWebClient())
          {
            var values = new NameValueCollection();

            byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "AddDefault", values);
            string str = Encoding.ASCII.GetString(bytes);
            int sum = str.AsInt();
            Assert.AreEqual(a + b, sum);
          }
        }

        [Test]
        public void Action_GetList()
        {
          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetList");

            Assert.AreEqual("application/json", wc.ResponseHeaders[HttpResponseHeader.ContentType]);

            var obj = JSONReader.DeserializeDynamic(str);

            Assert.AreEqual(3, obj.Data.Count);
            Assert.AreEqual(1UL, obj.Data[0]);
            Assert.AreEqual(2UL, obj.Data[1]);
            Assert.AreEqual(3UL, obj.Data[2]);
          }
        }

        [Test]
        public void Action_GetArray()
        {
          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetArray");

            Assert.AreEqual("application/json", wc.ResponseHeaders[HttpResponseHeader.ContentType]);

            var obj = JSONReader.DeserializeDynamic(str);

            Assert.AreEqual(3, obj.Data.Count);
            Assert.AreEqual(1UL, obj.Data[0]);
            Assert.AreEqual(2UL, obj.Data[1]);
            Assert.AreEqual(3UL, obj.Data[2]);
          }
        }

        [Test]
        [ExpectedException(typeof(System.Net.WebException), ExpectedMessage="(403)", MatchType=MessageMatch.Contains)]
        public void Action_GetWithNoPermission()
        {
          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetWithPermission");
           // Assert.AreEqual("text/html", wc.ResponseHeaders[HttpResponseHeader.ContentType]);
           // Assert.IsTrue( Regex.IsMatch(str, "Authorization to .+/TestPath/TestPermission.+ failed"));
          }
        }

        [Test]
        public void Action_InboundJSONMapEcho()
        {
          var inboundJSONStr = "ID=777&Name=TestName";

          using (var wc = CreateWebClient())
          {
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string str = wc.UploadString(INTEGRATION_HTTP_ADDR + "InboundJSONMapEcho", inboundJSONStr);

            Assert.AreEqual("application/json", wc.ResponseHeaders[HttpResponseHeader.ContentType]);

            var obj = JSONReader.DeserializeDynamic(str);

            Assert.AreEqual(4, obj.Data.Count);
            Assert.AreEqual("777", obj.Data["ID"]);
            Assert.AreEqual("TestName", obj.Data["Name"]);
            Assert.AreEqual(INTEGRATIONN_TESTER, obj.Data["type"]);
            Assert.AreEqual("InboundJSONMapEcho", obj.Data["mvc-action"]);
          }
        }

        [Test]
        public void Action_RowGet_JSONDataMap()
        {
          DateTime start = DateTime.Now;

          System.Threading.Thread.Sleep(3000);

          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RowGet");

            Assert.AreEqual("application/json", wc.ResponseHeaders[HttpResponseHeader.ContentType]);

            var obj = JSONReader.DeserializeDynamic(str);

            Assert.AreEqual(16, obj.Data.Count);
            Assert.AreEqual(777UL, obj.Data["ID"]);
            Assert.AreEqual("Test Name", obj.Data["Name"]);

            var date = DateTime.Parse(obj.Data["Date"]);
            Assert.IsTrue( (DateTime.Now - start).TotalSeconds >= 2.0d ); 
            Assert.AreEqual("Ok", obj.Data["Status"]);

            var gotTs = TimeSpan.FromTicks((long)(ulong)(obj.Data["Span"]));
            Assert.AreEqual(TimeSpan.FromSeconds(1), gotTs);
          }
        }

        [Test]
        public void TypeRowConversion()
        {
          var r = new WaveTestSite.Controllers.IntegrationTester.SpanRow() { Span = TimeSpan.FromTicks(1) };

          var str = r.ToJSON(JSONWritingOptions.CompactRowsAsMap);

          var map = JSONReader.DeserializeDataObject(str) as JSONDataMap;
          var gotRow = JSONReader.ToRow<WaveTestSite.Controllers.IntegrationTester.SpanRow>(map);
        }

        [Test]
        public void Action_RowGet_TypeRow()
        {
          DateTime start = DateTime.Now;

          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RowGet");

            
            Assert.AreEqual("application/json", wc.ResponseHeaders[HttpResponseHeader.ContentType]);

            var map = JSONReader.DeserializeDataObject(str) as JSONDataMap;
            var gotRow = JSONReader.ToRow<TestRow>(map);
          }
        }

        [Test]
        public void Action_ComplexRow()
        {
          var initalRow = new TestComplexRow();

          initalRow.ID = 777;

          initalRow.Row1 = new TestRow(){ID = 101, Name = "Test Row 1", Date = DateTime.Now};
          initalRow.Row2 = new TestRow(){ID = 102, Name = "Test Row 2", Date = DateTime.Now};

          initalRow.ErrorRows = new TestRow[] { 
            new TestRow() {ID = 201, Name = "Err Row 1", Date = DateTime.Now},
            new TestRow() {ID = 202, Name = "Err Row 2", Date = DateTime.Now},
            new TestRow() {ID = 203, Name = "Err Row 3", Date = DateTime.Now}
          };

          var str = initalRow.ToJSON(JSONWritingOptions.CompactRowsAsMap);
 
 Console.WriteLine(str);

          using (var wc = CreateWebClient())
          {
            wc.Headers[HttpRequestHeader.ContentType] = NFX.Web.ContentType.JSON;
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + "ComplexRowSet", str);

            var map = JSONReader.DeserializeDataObject(res) as JSONDataMap;
            var gotRow = JSONReader.ToRow<TestComplexRow>(map);

            Assert.AreEqual(initalRow.ID + 1, gotRow.ID);
            Assert.AreEqual(initalRow.Row1.ID + 2, gotRow.Row1.ID);
            Assert.IsTrue(gotRow.ErrorRows[2].Date - initalRow.ErrorRows[2].Date.AddDays(-2) < TimeSpan.FromMilliseconds(1) ); // dlat 20140617: date string format preservs 3 signs after decimal second instead of 7 digits preserved by .NET DateTime type
          }
        }

        [Test]
        public void Action_RowAndPrimitive_RowFirst()
        {
          rowAndPrimitive("RowAndPrimitive_RowFirst");
        }

        [Test]
        public void Action_RowAndPrimitive_RowLast()
        {
          rowAndPrimitive("RowAndPrimitive_RowLast");
        }

        [Test]
        public void Action_RowAndPrimitive_RowMiddle()
        {
          rowAndPrimitive("RowAndPrimitive_RowMiddle");
        }

        private void rowAndPrimitive(string actionName)
        {
          var initalRow = new TestRow() { ID = 0, Name = "Name" };
          var str = initalRow.ToJSON(JSONWritingOptions.CompactRowsAsMap);

          var values = new NameValueCollection();
          values.Add("n", "777");
          values.Add("s", "sss");

          using (var wc = CreateWebClient())
          {
            wc.QueryString = values;
            wc.Headers[HttpRequestHeader.ContentType] = NFX.Web.ContentType.JSON;
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + actionName, str);

            var map = JSONReader.DeserializeDataObject(res) as JSONDataMap;
            var gotRow = JSONReader.ToRow<TestRow>(map);

            Assert.AreEqual(gotRow.ID, 777);
            Assert.AreEqual(gotRow.Name, "sss");
          }
        }

        [Test]
        public void Action_JSONMapAndPrimitive_JSONFirst()
        {
          jsonMapAndPrimitives("JSONMapAndPrimitive_JSONFirst");
        }

        [Test]
        public void Action_JSONMapAndPrimitive_JSONLast()
        {
          jsonMapAndPrimitives("JSONMapAndPrimitive_JSONLast");
        }

        [Test]
        public void Action_JSONMapAndPrimitive_JSONMiddle()
        {
          jsonMapAndPrimitives("JSONMapAndPrimitive_JSONMiddle");
        }

        private void jsonMapAndPrimitives(string actionName)
        {
          var initialMap = new JSONDataMap();
          initialMap["ID"] = 100;
          initialMap["Name"] = "Initial Name";
          var str = initialMap.ToJSON(JSONWritingOptions.CompactRowsAsMap);

          var values = new NameValueCollection();
          values.Add("n", "777");
          values.Add("s", "sss");

          using (var wc = CreateWebClient())
          {
            wc.QueryString = values;
            wc.Headers[HttpRequestHeader.ContentType] = NFX.Web.ContentType.JSON;
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + actionName, str);

            var gotMap = JSONReader.DeserializeDataObject(res) as JSONDataMap;

            Assert.AreEqual(gotMap["ID"], 777);
            Assert.AreEqual(gotMap["Name"], "sss");
          }
        }

        //[Test]
        //public void Action_RowDifferentFieldTypes()
        //{
        //  var initalRow = new TestRow() { 
        //    ID = 0, Name = "Name", Date { get; set; }

        //    Status Status { get; set; }
        //    Status? StatusNullable { get; set; }

        //    Is { get; set; }
        //    IsNullable { get; set; }

        //    Money { get; set; }
        //    MoneyNullable { get; set; }

        //    Float { get; set; }
        //    FloatNullable { get; set; }

        //    Double { get; set; }
        //    DoubleNullable { get; set; }
        //  };
        //  var str = initalRow.ToJSON(JSONWritingOptions.CompactRowsAsMap);

        //  using (var wc = CreateWebClient())
        //  {
        //    wc.Headers[HttpRequestHeader.ContentType] = NFX.Wave.ContentTypeUtils.JSON;
        //    var res = wc.UploadString(INTEGRATION_HTTP_ADDR + "RowDifferentFieldTypes", str);

        //    var gotRow = JSONReader.ToRow<TestRow>(JSONReader.DeserializeDataObject(res) as JSONDataMap);

        //    Assert.AreEqual(gotRow.ID, 777);
        //    Assert.AreEqual(gotRow.Name, "sss");
        //  }
        //}

        [Test]
        public void Action_GetAnonymousObject()
        {
          using (var wc = CreateWebClient())
          {
            var str = wc.UploadString(INTEGRATION_HTTP_ADDR + "GetAnonymousObject", string.Empty);

            Console.WriteLine(str);

            var obj = JSONReader.DeserializeDynamic(str);
            Assert.AreEqual(55UL, obj.Data["ID"]);
            Assert.AreEqual("test", obj.Data["Name"]);
          }
        }

        [Test]
        public void Action_Login()
        {
          NameValueCollection values = new NameValueCollection();
          values.Add("id", USER_ID);
          values.Add("pwd", USER_PWD);

          using (var wc = CreateWebClient())
          {
            var bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "LoginUser", values);
            var str = GetUTF8StringWOBOM(bytes);

            Console.WriteLine(str);

            Assert.AreEqual("application/json", wc.ResponseHeaders[HttpResponseHeader.ContentType]);

            var obj = JSONReader.DeserializeDynamic(str);
            Assert.AreEqual(USER_STATUS, obj["Status"]);
            Assert.AreEqual(USER_NAME, obj["Name"]);

            str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetWithPermission");

            Assert.IsFalse( Regex.IsMatch(str, "Authorization to .+/TestPath/TestPermission.+ failed"));
          }
        }

        [Test]
        public void Action_NullableDateNoParameter()
        {
          using (var wc = CreateWebClient())
          {
            var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RelaxedDateTime");
            Assert.IsTrue(response.IsNullOrEmpty());
          }
        }

        [Test]
        public void Action_NullableDateTickFormat()
        {
          using (var wc = CreateWebClient())
          {
            var dt = new DateTime(2014, 12, 5);
            var dtStr = dt.ToString();

            var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RelaxedDateTime?dt=" + dt.Ticks);
            Assert.IsTrue(response.IsNotNullOrEmpty());
            var responseDt = DateTime.Parse(response.Trim('"'));
            Assert.AreEqual(dt, responseDt);
          }
        }

        [Test]
        public void Action_NullableDateWrongFormat_Relaxed()
        {
          using (var wc = CreateWebClient())
          {
            var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RelaxedDateTime?dt=fgtYY");
            Assert.IsTrue(response.IsNullOrEmpty());
          }
        }

        [Test]
        public void Action_NullableDateWrongFormat_Strict()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime?dt=fgtYY");
              throw new Exception(EXPECTED_400);
            }
            catch (WebException wex)
            {
              Assert.IsTrue(Is400(wex));
            }
          }
        }


        [Test]
        public void MultipartByteArray()
        {
          MultipartTest("MultipartByteArray");
        }

        [Test]
        public void MultipartMap()
        {
          MultipartTest("MultipartMap");
        }

        [Test]
        public void MultipartRow()
        {
          MultipartTest("MultipartRow");
        }

        [Test]
        public void MultipartStream()
        {
          MultipartTest("MultipartStream");
        }

        [Test]
        public void MultipartEncoding()
        {
          var encoding = Encoding.GetEncoding(1251);
          var part = new NFX.Web.Multipart.Part("field");
          part.Content = "Значение";

          var mp = new NFX.Web.Multipart(new NFX.Web.Multipart.Part[] { part });
          var enc = mp.Encode(encoding);

          var req = HttpWebRequest.CreateHttp(INTEGRATION_HTTP_ADDR + "MultipartEncoding");
          req.Method = "POST";
          req.ContentType = NFX.Web.ContentType.FORM_MULTIPART_ENCODED + "; charset=windows-1251";
          req.ContentLength = enc.Length;
          req.CookieContainer = new CookieContainer();
          req.CookieContainer.Add(S_WAVE_URI, S_WAVE_COOKIE);

          using (var reqs = req.GetRequestStream())
          {
            reqs.Write(enc.Buffer, 0, (int)enc.Length);
            var resp = req.GetResponse();

            var ms = new MemoryStream();
            resp.GetResponseStream().CopyTo(ms);

            Assert.AreEqual(part.Content.AsString(), encoding.GetString(ms.ToArray()));
          }
        }

    //[Test]
    //public void Action_NullableDateTimeWrongFormat()
    //{
    //  using (var wc = CreateWebClient())
    //  {
    //    var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime");
    //    Assert.IsTrue(response.IsNullOrEmpty());

    //    var dt = new DateTime(2014, 12, 5);
    //    var dtStr = dt.ToString();

    //    response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime?dt=" + dtStr);
    //    Assert.IsTrue(response.IsNotNullOrEmpty());
    //    var responseDt = DateTime.Parse(response.Trim('"'));
    //    Assert.AreEqual(dt, responseDt);

    //    response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime?dt=" + dt.Ticks);
    //    Assert.IsTrue(response.IsNotNullOrEmpty());
    //    responseDt = DateTime.Parse(response.Trim('"'));
    //    Assert.AreEqual(dt, responseDt);
    //  }
    //}

    #endregion

    #region Helpers

    private bool Is400(WebException ex)
        {
          return ex.Message.IndexOf("(400)") >= 0;
        }

        private bool Is404(WebException ex)
        {
          return ex.Message.IndexOf("(404)") >= 0;
        }

        private static string GetUTF8StringWOBOM(byte[] buf)
        {
          if (buf.Length > 3 && (buf[0] == 0xEF && buf[1] == 0xBB && buf[2] == 0xBF))
              return Encoding.UTF8.GetString(buf, 3, buf.Length-3);

          return Encoding.UTF8.GetString(buf);
        }

        private void MultipartTest(string type)
        {
          var partField = new NFX.Web.Multipart.Part("field");
          partField.Content = "value";

          var partTxtFile = new NFX.Web.Multipart.Part("text");
          partTxtFile.Content = "Text with\r\nnewline";
          partTxtFile.FileName = "TxtFile";
          partTxtFile.ContentType = "Content-type: text/plain";

          var partBinFile = new NFX.Web.Multipart.Part("bin");
          partBinFile.Content = new byte[] { 0xff, 0xaa, 0x89, 0xde, 0x23, 0x20, 0xff, 0xfe, 0x02 };
          partBinFile.FileName = "BinFile";
          partBinFile.ContentType = "application/octet-stream";

          var mp = new NFX.Web.Multipart(new NFX.Web.Multipart.Part[] { partField, partTxtFile, partBinFile });

          var enc = mp.Encode();

          var req = HttpWebRequest.CreateHttp(INTEGRATION_HTTP_ADDR + type);
          req.Method = "POST";
          req.ContentType = NFX.Web.ContentType.FORM_MULTIPART_ENCODED;
          req.ContentLength = enc.Length;
          req.CookieContainer = new CookieContainer();
          req.CookieContainer.Add(S_WAVE_URI, S_WAVE_COOKIE);

          using (var reqs = req.GetRequestStream())
          {
            reqs.Write(enc.Buffer, 0, (int)enc.Length);
            var resp = req.GetResponse();

            var ms = new MemoryStream();
            resp.GetResponseStream().CopyTo(ms);
            var returned = ms.ToArray();

            var fieldSize = Encoding.UTF8.GetBytes(partField.Content.AsString()).Length;
            var txtFileSize = Encoding.UTF8.GetBytes(partTxtFile.Content.AsString()).Length;
            Assert.AreEqual(partField.Content.AsString(), Encoding.UTF8.GetString(returned.Take(fieldSize).ToArray()));
            Assert.AreEqual(partTxtFile.Content.AsString(), Encoding.UTF8.GetString(returned.Skip(fieldSize).Take(txtFileSize).ToArray()));
            Assert.IsTrue(NFX.IOMiscUtils.MemBufferEquals(partBinFile.Content as byte[], returned.Skip(fieldSize + txtFileSize).ToArray()));
          }
        }

    #endregion

    #endregion
  }
}
