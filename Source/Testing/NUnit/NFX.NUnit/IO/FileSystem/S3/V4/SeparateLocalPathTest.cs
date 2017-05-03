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
using NFX.IO.FileSystem.S3.V4;
using NUnit.Framework;

namespace NFX.NUnit.IO.FileSystem.S3.V4
{
  [TestFixture]
  public class SeparateLocalPathTest
  {
    [Test]
    public void TestEmpty()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath(null, out parent, out name);
      Assert.IsNull(parent);
      Assert.IsEmpty(name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("", out parent, out name);
      Assert.IsNull(parent);
      Assert.IsEmpty(name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/", out parent, out name);
      Assert.IsNull(parent);
      Assert.IsEmpty(name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath(" /  ", out parent, out name);
      Assert.IsNull(parent);
      Assert.IsEmpty(name);
    }

    [Test]
    public void TestRootOnly()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root", out parent, out name);
      Assert.IsEmpty(parent);
      Assert.AreEqual("root", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("root/", out parent, out name);
      Assert.IsEmpty(parent);
      Assert.AreEqual("root", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/", out parent, out name);
      Assert.IsEmpty(parent);
      Assert.AreEqual("root", name);
    }

    [Test]
    public void TestRootNChild()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child", out parent, out name);
      Assert.AreEqual("root", parent);
      Assert.AreEqual("child", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("root/child/", out parent, out name);
      Assert.AreEqual("root", parent);
      Assert.AreEqual("child", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child/", out parent, out name);
      Assert.AreEqual("root", parent);
      Assert.AreEqual("child", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/ root//child/ ", out parent, out name);
      Assert.AreEqual("root", parent);
      Assert.AreEqual("child", name);
    }

    [Test]
    public void TestParentNChild()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child/baby", out parent, out name);
      Assert.AreEqual("root/child", parent);
      Assert.AreEqual("baby", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("root/child/baby/", out parent, out name);
      Assert.AreEqual("root/child", parent);
      Assert.AreEqual("baby", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child/baby/", out parent, out name);
      Assert.AreEqual("root/child", parent);
      Assert.AreEqual("baby", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath(" /root/child//baby/", out parent, out name);
      Assert.AreEqual("root/child", parent);
      Assert.AreEqual("baby", name);
    }
  }
}
