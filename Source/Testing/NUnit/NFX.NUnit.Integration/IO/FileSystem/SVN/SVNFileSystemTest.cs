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
using System.Diagnostics;
using System.Linq;
using System.Text;

using NFX.IO.FileSystem;
using NFX.IO.FileSystem.SVN;
using NFX.Security;
using FS = NFX.IO.FileSystem.FileSystem;

using NUnit.Framework;

namespace NFX.NUnit.Integration.IO.FileSystem.SVN
{
  [TestFixture]
  public class SVNFileSystemTest: ExternalCfg
  {
    private static SVNFileSystemSessionConnectParams CONN_PARAMS, CONN_PARAMS_TIMEOUT;

    [TestFixtureSetUp]
    public void SetUp()
    {
      CONN_PARAMS = FileSystemSessionConnectParams.Make<SVNFileSystemSessionConnectParams>(
        "svn{{ name='aaa' server-url='{0}' user-name='{1}' user-password='{2}' }}".Args(SVN_ROOT, SVN_UNAME, SVN_UPSW));

      CONN_PARAMS_TIMEOUT = FileSystemSessionConnectParams.Make<SVNFileSystemSessionConnectParams>(
        "svn{{ name='aaa' server-url='{0}' user-name='{1}' user-password='{2}' timeout-ms=1 }}".Args(SVN_ROOT, SVN_UNAME, SVN_UPSW));
    }

    [TestCase]
    public void NavigateRootDir()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        using(var fs = new SVNFileSystem("NFX-SVN"))
        {
          var session = fs.StartSession(CONN_PARAMS);

          var dir = session[string.Empty] as FileSystemDirectory;

          Assert.IsNotNull(dir);
          Assert.AreEqual("/", dir.Path);
        }
      }
    }

    [TestCase]
    public void Size()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["NFX-SVN"];

        using(var session = fs.StartSession())
        {
          var dir = session["trunk/Source/Testing/NUnit/NFX.NUnit.Integration/IO/FileSystem/SVN/Esc Folder+"] as FileSystemDirectory;

          var file1 = session["trunk/Source/Testing/NUnit/NFX.NUnit.Integration/IO/FileSystem/SVN/Esc Folder+/Escape.txt"] as FileSystemFile;
          var file2 = session["trunk/Source/Testing/NUnit/NFX.NUnit.Integration/IO/FileSystem/SVN/Esc Folder+/NestedFolder/Escape.txt"] as FileSystemFile;

          Assert.AreEqual(19, file1.Size);
          Assert.AreEqual(11, file2.Size);

          Assert.AreEqual(30, dir.Size);
        }
      }
    }

    [TestCase]
    public void NavigateChildDir()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["NFX-SVN"];

        using(var session = fs.StartSession())
        {
          {
            var dir = session["trunk"] as FileSystemDirectory;

            Assert.IsNotNull(dir);
            Assert.AreEqual("/trunk", dir.Path);
            Assert.AreEqual("/", dir.ParentPath);
          }

          {
            var dir = session["trunk/Source"] as FileSystemDirectory;

            Assert.IsNotNull(dir);
            Assert.AreEqual("/trunk/Source", dir.Path);
            Assert.AreEqual("/trunk", dir.ParentPath);
          }
        } 
      }
    }

    [TestCase]
    public void NavigateChildFile()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["NFX-SVN"];
        using (var session = fs.StartSession())
        {
          var file = session["/trunk/Source/NFX/LICENSE.txt"] as FileSystemFile;

          Assert.IsNotNull(file);
          Assert.AreEqual("/trunk/Source/NFX/LICENSE.txt", file.Path);
        }  
      }
    }

    [TestCase]
    public void GetFileContent()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["NFX-SVN"];
        using (var session = fs.StartSession())
        {
          var file = session["/trunk/Source/NFX/LICENSE.txt"] as FileSystemFile;

          Assert.IsTrue(file.ReadAllText().IsNotNullOrEmpty());
        }  
      }
    }

    [TestCase]
    public void GetVersions()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["NFX-SVN"];
        using (var session = fs.StartSession())
        {
          var currentVersion = session.LatestVersion;

          var preVersions = session.GetVersions(currentVersion, 5);

          Assert.AreEqual(5, preVersions.Count());
          Assert.AreEqual(preVersions.Last().Name.AsInt() + 1, currentVersion.Name.AsInt());
        }  
      }
    }

    [TestCase]
    public void GetVersionedFiles()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        IList<WebDAV.Version> versions = WebDAV.GetVersions(SVN_ROOT, SVN_UNAME, SVN_UPSW).ToList();

        WebDAV.Version v1530 = versions.First(v => v.Name == "1530");
        WebDAV.Version v1531 = versions.First(v => v.Name == "1531");

        var fs = FS.Instances["NFX-SVN"];
        using (var session = fs.StartSession())
        {
          session.Version = v1530;
          var file1530 = session["trunk/Source/NFX.Web/IO/FileSystem/SVN/WebDAV.cs"] as FileSystemFile;
          string content1530 = file1530.ReadAllText();

          session.Version = v1531;
          var file1531 = session["trunk/Source/NFX.Web/IO/FileSystem/SVN/WebDAV.cs"] as FileSystemFile;
          string content1531 = file1531.ReadAllText();

          Assert.AreNotEqual(content1530, content1531);
        }  
      }
    }

    [TestCase]
    [ExpectedException(typeof(System.Net.WebException), ExpectedMessage = "timed out", MatchType = MessageMatch.Contains)]
    public void FailedFastTimeout()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["NFX-SVN"];
        using (var session = fs.StartSession(CONN_PARAMS_TIMEOUT))
        {
          var dir = session[string.Empty] as FileSystemDirectory;
        } 
      }
    }
  }
}
