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
using NFX.IO.FileSystem.S3.V4.S3V4Sign;
using NUnit.Framework;

namespace NFX.NUnit.IO.FileSystem.S3.V4.S3V4Sign
{
  [TestFixture]
  public class S3V4URLHelpersTest
  {
    [Test]
    public void EmptyRegion()
    {
      string uri = S3V4URLHelpers.CreateURIString();
      Assert.AreEqual("https://s3.amazonaws.com/", uri);
    }

    [Test]
    public void BucketRoot()
    {
      string uri = S3V4URLHelpers.CreateURIString("us-west-2", "dxw");
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/", uri);
    }

    [Test]
    public void FileRoot()
    {
      string uri = S3V4URLHelpers.CreateURIString("us-west-2", "dxw", "MyFile.txt");
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/MyFile.txt", uri);
    }

    [Test]
    public void FolderRoot()
    {
      string uri = S3V4URLHelpers.CreateURIString("us-west-2", "dxw", "MyFolder/");
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/", uri);
    }

    [Test]
    public void FolderFile()
    {
      string uri = S3V4URLHelpers.CreateURIString("us-west-2", "dxw", "MyFolder/MyFile1.txt");
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/MyFile1.txt", uri);
    }

    [Test]
    public void FolderFileParameterEmpty()
    {
      var queryParams = new Dictionary<string, string>() { {"acl", ""}};
      string uri = S3V4URLHelpers.CreateURIString("us-west-2", "dxw", "MyFolder/MyFile1.txt", queryParams);
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/MyFile1.txt?acl=", uri);
    }

    [Test]
    public void FolderFileParameters()
    {
      var queryParams = new Dictionary<string, string>() { { "marker", "1" }, { "max-keys", "100" } };
      string uri = S3V4URLHelpers.CreateURIString("us-west-2", "dxw", "MyFolder/MyFile1.txt", queryParams);
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/MyFile1.txt?marker=1&max-keys=100", uri);
    }

    [Test]
    public void FolderFileParametersEncoding()
    {
      var queryParams = new Dictionary<string, string>() { { "delimiter", "/" } };
      string uri = S3V4URLHelpers.CreateURIString("us-west-2", "dxw", "MyFolder/", queryParams);
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/?delimiter=%2F", uri);
    }

    [Test]
    public void ParentEmpty()
    {
      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/");
        string parent = uri.GetParentURL();
        Assert.AreEqual(string.Empty, parent);
      }

      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com");
        string parent = uri.GetParentURL();
        Assert.AreEqual(string.Empty, parent);
      }
    }

    [Test]
    public void ParentFile()
    {
      Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFile1.txt");
      string parent = uri.GetParentURL();
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com", parent);
    }

    [Test]
    public void ParentFolder()
    {
      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFolder");
        string parent = uri.GetParentURL();
        Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com", parent);
      }

      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/");
        string parent = uri.GetParentURL();
        Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com", parent);
      }
    }

    [Test]
    public void ParentFolderFile()
    {
      Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/MyFile1.txt");
      string parent = uri.GetParentURL();
      Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/MyFolder", parent);
    }

    [Test]
    public void LocalNameEmpty()
    {
      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/");
        string parent = uri.GetLocalName();
        Assert.AreEqual(string.Empty, parent);
      }

      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com");
        string parent = uri.GetLocalName();
        Assert.AreEqual(string.Empty, parent);
      }
    }

    [Test]
    public void LocalNameFolder()
    {
      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/");
        string parent = uri.GetLocalName();
        Assert.AreEqual("MyFolder", parent);
      }

      {
        Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFolder");
        string parent = uri.GetLocalName();
        Assert.AreEqual("MyFolder", parent);
      }
    }

    [Test]
    public void LocalNameFile()
    {
      Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFile1.txt");
      string parent = uri.GetLocalName();
      Assert.AreEqual("MyFile1.txt", parent);
    }

    [Test]
    public void LocalNameFolderFile()
    {
      Uri uri = new Uri("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/MyFile1.txt");
      string parent = uri.GetLocalName();
      Assert.AreEqual("MyFile1.txt", parent);
    }

    [Test]
    public void ParseDefaultRegion()
    {
      string bucket;
      string region;
      string itemLocalPath;

      S3V4URLHelpers.Parse("https://dxw.s3.amazonaws.com/", out bucket, out region, out itemLocalPath);

      Assert.AreEqual("dxw", bucket);
      Assert.AreEqual(S3V4URLHelpers.US_EAST_1, region);
      Assert.AreEqual(string.Empty, itemLocalPath);
    }

    [Test]
    public void ParseRoot()
    {
      string bucket;
      string region;
      string itemLocalPath;

      S3V4URLHelpers.Parse("https://dxw.s3-us-west-2.amazonaws.com/", out bucket, out region, out itemLocalPath);

      Assert.AreEqual("dxw", bucket);
      Assert.AreEqual("us-west-2", region);
      Assert.AreEqual(string.Empty, itemLocalPath);
    }

    [Test]
    public void ParseFileRoot()
    {
      string bucket;
      string region;
      string itemLocalPath;

      S3V4URLHelpers.Parse("https://dxw.s3-us-west-2.amazonaws.com/MyFile.txt", out bucket, out region, out itemLocalPath);

      Assert.AreEqual("dxw", bucket);
      Assert.AreEqual("us-west-2", region);
      Assert.AreEqual("MyFile.txt", itemLocalPath);
    }

    [Test]
    public void ParseFolderRoot()
    {
      string bucket;
      string region;
      string itemLocalPath;

      S3V4URLHelpers.Parse("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/", out bucket, out region, out itemLocalPath);

      Assert.AreEqual("dxw", bucket);
      Assert.AreEqual("us-west-2", region);
      Assert.AreEqual("MyFolder/", itemLocalPath);
    }

    [Test]
    public void ParseFolderFile()
    {
      string bucket;
      string region;
      string itemLocalPath;

      S3V4URLHelpers.Parse("https://it-adapter.s3-ap-southeast-1.amazonaws.com/MyFolder/MyFile1.txt", out bucket, out region, out itemLocalPath);

      Assert.AreEqual("it-adapter", bucket);
      Assert.AreEqual("ap-southeast-1", region);
      Assert.AreEqual("MyFolder/MyFile1.txt", itemLocalPath);
    }

    [Test]
    public void ParseFolderFileParameters()
    {
      string bucket;
      string region;
      string itemLocalPath;
      IDictionary<string, string> queryParams;

      S3V4URLHelpers.Parse("https://dxw.s3-us-west-2.amazonaws.com/MyFolder/MyFile1.txt?marker=1&max-keys=100",
        out bucket, out region, out itemLocalPath, out queryParams);

      Assert.AreEqual("dxw", bucket);
      Assert.AreEqual("us-west-2", region);
      Assert.AreEqual("MyFolder/MyFile1.txt", itemLocalPath);

      Assert.IsNotNull(queryParams);
      Assert.AreEqual(2, queryParams.Count);
      Assert.IsTrue(queryParams.ContainsKey("marker"));
      Assert.IsTrue(queryParams.ContainsKey("max-keys"));
      Assert.AreEqual("1", queryParams["marker"]);
      Assert.AreEqual("100", queryParams["max-keys"]);
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void ParseExceptionInvalidUriEmpty()
    {
      parseException("");
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void ParseExceptionInvalidUri()
    {
      parseException("aaa");
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void ParseExceptionUriWOS3()
    {
      parseException("https://amazonaws.com");
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void ParseExceptionUriWS4()
    {
      parseException("https://s4.amazonaws.com");
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void ParseExceptionUriTooManyDomains()
    {
      parseException("https://a1.b2.s3.amazonaws.com");
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void ParseExceptionUriInvalidRegion()
    {
      parseException("https://s3_blah.amazonaws.com");
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void ParseExceptionUriNonHttp()
    {
      parseException("ftp://dxw.s3.amazonaws.com");
    }

    private void parseException(string uri)
    {
      string bucket;
      string region;
      string itemLocalPath;
      IDictionary<string, string> queryParams;

      S3V4URLHelpers.Parse(uri, out bucket, out region, out itemLocalPath, out queryParams);
    }
  }
}
