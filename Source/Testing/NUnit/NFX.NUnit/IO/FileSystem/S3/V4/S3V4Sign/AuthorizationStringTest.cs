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
using System.IO;
using System.Text;
using NFX.IO.FileSystem.S3.V4.S3V4Sign;
using NUnit.Framework;

namespace NFX.NUnit.IO.FileSystem.S3.V4.S3V4Sign
{
  [TestFixture]
  public class AuthorizationStringTest: Base
  {
    [TestFixtureSetUp]
    public void SetUp()
    {
      initCONSTS();
    }

    [Test]
    public void AutorizationGetQueryParameters()
    {
      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      IDictionary<string, string> queryParameters = new Dictionary<string, string>() { 
        {"marker", "1"},
        {"delimiter", "/"}
      };

      S3V4Signer s3v4 = new S3V4Signer() {
        AccessKey = ACCESSKEY,
        SecretKey = SECRETKEY,
        Bucket = BUCKET,
        Region = REGION,
        Method = "GET", 
        RequestDateTime = dateTime,
        QueryParams = queryParameters
      };

      
      string expected = "AWS4-HMAC-SHA256 Credential={0}/20140217/us-west-2/s3/aws4_request, SignedHeaders=content-type;host;x-amz-content-sha256;x-amz-date, ".Args(ACCESSKEY) +
        "Signature=9cda8f26243f1874d921938037a3fc6839f3067092bc305dc4b41dc810a98edb";

      Assert.AreEqual(5, s3v4.Headers.Count);
      Assert.AreEqual(expected, s3v4.Headers["Authorization"]);
    }

    [Test]
    public void AutorizationDeleteFile()
    {
      DateTime dateTime = new DateTime(2014, 02, 17, 11, 11, 11, DateTimeKind.Utc);

      S3V4Signer s3v4 = new S3V4Signer()
      {
        AccessKey = ACCESSKEY,
        SecretKey = SECRETKEY,
        Bucket = BUCKET,
        Region = REGION,
        ItemLocalPath = ITEM_RELATIVE_PATH,
        Method = "DELETE",
        RequestDateTime = dateTime
      };

      string expected = "AWS4-HMAC-SHA256 Credential={0}/20140217/us-west-2/s3/aws4_request, SignedHeaders=content-type;host;x-amz-content-sha256;x-amz-date, ".Args(ACCESSKEY) +
        "Signature=b9d74d6372c55ee54b89c9f44c866edd2577aca878d05a8eccf0cec4a921e07a";

      Assert.AreEqual(5, s3v4.Headers.Count);
      Assert.AreEqual(expected, s3v4.Headers["Authorization"]);
    }

    [Test]
    public void AutorizationPutFile()
    {
      DateTime dateTime = new DateTime(2015, 02, 17, 11, 11, 11, DateTimeKind.Utc);

      MemoryStream contentStream = new MemoryStream(Encoding.UTF8.GetBytes(CONTENT));

      S3V4Signer s3v4 = new S3V4Signer()
      {
        AccessKey = ACCESSKEY,
        SecretKey = SECRETKEY,
        Bucket = BUCKET,
        Region = REGION,
        ItemLocalPath = ITEM_RELATIVE_PATH,
        Method = "PUT",
        RequestDateTime = dateTime,
        ContentStream = contentStream
      };

      string expected = "AWS4-HMAC-SHA256 Credential={0}/20150217/us-west-2/s3/aws4_request, SignedHeaders=content-length;content-type;host;x-amz-content-sha256;x-amz-date, ".Args(ACCESSKEY) +
        "Signature=b0226cf0a900e16489d7f1feaace3f1c6d9e9f516fac2ebd61177b1f3d9b98ea";

      Assert.AreEqual(6, s3v4.Headers.Count);
      Assert.AreEqual(expected, s3v4.Headers["Authorization"]);
    }
  }
}
