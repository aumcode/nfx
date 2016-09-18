/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using NFX.IO.FileSystem.SVN;
using NUnit.Framework;
using NFX.Web;

namespace NFX.NUnit.Integration.IO.FileSystem.SVN
{
  [TestFixture]
  public class WebDAVTest: ExternalCfg
  {
    [Test]
    public void ItemProperties()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Directory root = client.Root;

        Assert.Greater(root.Version.AsInt(), 0);
        Assert.Greater(root.CreationDate, DateTime.MinValue);
        Assert.Greater(root.LastModificationDate, DateTime.MinValue);

        var maxVersionChild = root.Children.OrderByDescending(c => c.Version).First();

        Console.WriteLine("First Child: " + maxVersionChild);

        Assert.Greater(maxVersionChild.Version.AsInt(), 0);
        Assert.Greater(maxVersionChild.CreationDate, DateTime.MinValue);
        Assert.Greater(maxVersionChild.LastModificationDate, DateTime.MinValue);

        Assert.AreEqual(root.Version, maxVersionChild.Version);
      }
    }

    [Test]
    public void DirectoryChildren()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {  
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Directory root = client.Root;

        var children = root.Children;

        Assert.IsNotNull(children);
        Assert.AreEqual(3, children.Count());
        Assert.IsTrue(children.Any(c => c.Name == "trunk"));

        var firstChild = children.First();
        Assert.AreEqual(root, firstChild.Parent);
      }
    }

    [Test]
    public void NavigatePathFolder()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Directory root = client.Root;

        WebDAV.Directory nested = root.NavigatePath("/trunk/Source/NFX") as WebDAV.Directory;

        Assert.IsNotNull(nested);
        Assert.AreEqual("NFX", nested.Name);
        Assert.AreEqual("/trunk/Source/NFX", nested.Path);

        Assert.AreEqual("Source", nested.Parent.Name);
        Assert.AreEqual("/trunk/Source", nested.Parent.Path);

        Assert.AreEqual("trunk", nested.Parent.Parent.Name);
        Assert.AreEqual("/trunk", nested.Parent.Parent.Path);
      }
    }

    [Test]
    public void NavigatePathFile()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Directory root = client.Root;

        WebDAV.File nested = root.NavigatePath("/trunk/Source/NFX/LICENSE.txt") as WebDAV.File;

        Assert.IsNotNull(nested);
        Assert.AreEqual("LICENSE.txt", nested.Name);

        using (MemoryStream s = new MemoryStream())
        {
          nested.GetContent(s);
          Assert.Greater(s.Length, 0);
        }
      }
    }

    [Test]
    public void ContentType()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Directory root = client.Root;

        var file1 = root.NavigatePath("/trunk/Source/NFX/LICENSE.txt");
        var file2 = root.NavigatePath("/trunk/Source/NFX.Wave/Templatization/StockContent/Embedded/flags/ad.png");

        Assert.AreEqual(0, string.Compare("text/xml; charset=\"utf-8\"", file1.ContentType, true));
        Assert.AreEqual(0, string.Compare("application/octet-stream", file2.ContentType, true));
      }
    }

    [Test]
    public void FileContent()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Directory root = client.Root;

        WebDAV.File file = root.Children.First(c => c is WebDAV.File) as WebDAV.File;

        using (MemoryStream ms = new MemoryStream())
        {
          file.GetContent(ms);
          Assert.Greater(ms.Length, 0);
        }
      }
    }

    [Test]
    public void GetVersions()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        IList<WebDAV.Version> versions = WebDAV.GetVersions(SVN_ROOT, SVN_UNAME, SVN_UPSW).ToList();

        Assert.IsNotNull(versions);
        Assert.Greater(versions.Count(), 0);
      
        WebDAV.Version v1 = versions.First();

        Assert.IsNotNull(v1);
        Assert.IsNotNullOrEmpty(v1.Creator);
        Assert.IsNotNullOrEmpty(v1.Comment);
        Assert.AreEqual("1", v1.Name);
        Assert.Greater(v1.Date, DateTime.MinValue);
      }
    }

    [Test]
    public void DifferentDirectoryVersions()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        IList<WebDAV.Version> versions = WebDAV.GetVersions(SVN_ROOT, SVN_UNAME, SVN_UPSW).ToList();

        WebDAV.Version v1513 = versions.First(v => v.Name == "1513");
        WebDAV.Version v1523 = versions.First(v => v.Name == "1523");

        var client1513 = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW, version: v1513);

        var client1523 = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW, version: v1523);

        WebDAV.Directory root1513 = client1513.Root;
        WebDAV.Directory root1523 = client1523.Root;

        WebDAV.Directory nested1513 = root1513.NavigatePath("trunk/Source/NFX.Web/IO/FileSystem") as WebDAV.Directory;
        WebDAV.Directory nested1523 = root1523.NavigatePath("trunk/Source/NFX.Web/IO/FileSystem") as WebDAV.Directory;

        Assert.IsNull(nested1513["SVN"]);
        Assert.IsNotNull(nested1523["SVN"]);
      }
    }

    [Test]
    public void DifferentFileVersions()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        IList<WebDAV.Version> versions = WebDAV.GetVersions(SVN_ROOT, SVN_UNAME, SVN_UPSW).ToList();

        WebDAV.Version v1530 = versions.First(v => v.Name == "1530");
        WebDAV.Version v1531 = versions.First(v => v.Name == "1531");

        var client1530 = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW, version: v1530);

        var client1531 = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW, version: v1531);

        WebDAV.Directory root1530 = client1530.Root;
        WebDAV.Directory root1531 = client1531.Root;

        WebDAV.File file1530 = root1530.NavigatePath("trunk/Source/NFX.Web/IO/FileSystem/SVN/WebDAV.cs") as WebDAV.File;
        WebDAV.File file1531 = root1531.NavigatePath("trunk/Source/NFX.Web/IO/FileSystem/SVN/WebDAV.cs") as WebDAV.File;

        using (MemoryStream ms1530 = new MemoryStream())
        {
          using (MemoryStream ms1531 = new MemoryStream())
          {
            file1530.GetContent(ms1530);
            file1531.GetContent(ms1531);

            Assert.AreNotEqual(ms1530.Length, ms1531.Length);
          }
        }
      }
    }

    [Test]
    public void NonExistingItem()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Directory root = client.Root;

        var children = root.Children;

        var nonexistingChild = root[children.OrderBy(c => c.Name.Length).First().Name + "_"];

        Assert.IsNull(nonexistingChild);
      }
    }
    
    [Test]
    public void GetHeadRootVersion()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        WebDAV.Version lastVersion = client.GetHeadRootVersion();
        Assert.IsNotNull(lastVersion);
      }
    }

    [Test]
    public void EscapeDir()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        {
          var d = client.Root.NavigatePath("trunk/Source/Testing/NUnit/NFX.NUnit.Integration/IO/FileSystem/SVN") as WebDAV.Directory;
          Assert.IsNotNull(d.Children.FirstOrDefault(c => c.Name == "Esc Folder+"));
        }

        {
          var d = client.Root.NavigatePath("trunk/Source/Testing/NUnit/NFX.NUnit.Integration/IO/FileSystem/SVN/Esc Folder+") as WebDAV.Directory;
          Assert.IsNotNull(d);
          Assert.AreEqual("Esc Folder+", d.Name);
        }
      }
    }

    [Test]
    public void EscapeFile()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        var f = client.Root.NavigatePath("trunk/Source/Testing/NUnit/NFX.NUnit.Integration/IO/FileSystem/SVN/Esc Folder+/Escape.txt") as WebDAV.File;
        var ms = new MemoryStream();
        f.GetContent(ms);
        var s = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        Console.WriteLine(s);
        Assert.AreEqual("Escape file+content", s);
      }
    }

    [Test]
    public void GetManyFiles()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var client = new WebDAV(SVN_ROOT, 0, SVN_UNAME, SVN_UPSW);

        var d = client.Root.NavigatePath("trunk/Source/NFX.Wave/Templatization/StockContent/Embedded/flags") as WebDAV.Directory;

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        foreach (WebDAV.File f in d.Children)
        {
          MemoryStream ms = new MemoryStream();
          f.GetContent(ms);

          Console.WriteLine("{0} done".Args(f.Name));
        }
        stopwatch.Stop();

        Console.WriteLine(stopwatch.Elapsed);
      }
    }

    [Test]
    [ExpectedException(typeof(System.Net.WebException), MatchType=MessageMatch.Contains)]
    public void FailedFastTimeout()
    {
      var conf = LACONF.AsLaconicConfig();

      using (var app = new NFX.ApplicationModel.ServiceBaseApplication(new string[] {}, conf))
      {
        try
        {
          var client = new WebDAV(SVN_ROOT, 1, SVN_UNAME, SVN_UPSW);

          WebDAV.Directory root = client.Root;
          var children = root.Children;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToMessageWithType());
          throw;
        }
      }
    }
  }
}
