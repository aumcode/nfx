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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S3V4FSH = NFX.IO.FileSystem.S3.V4.S3V4FileSystem.S3V4FSH;

using NFX;
using NFX.Environment;
using NFX.IO.FileSystem;
using NFX.IO.FileSystem.S3.V4;
using NFX.Security;
using FS = NFX.IO.FileSystem;

using NUnit.Framework;

namespace NFX.NUnit.Integration.IO.FileSystem.S3.V4
{
  [TestFixture]
  public class S3V4FileSystemTests: ExternalCfg
  {
    private const string S3_ROOT_FS = "/nfx-root";
    private const string LOCAL_ROOT_FS = @"c:\NFX";

    private const string DIR_1 = "dir1";

    private const string FN1_FS = "nfxtest01.txt";
    private const string FN2_FS = "nfxtest02.txt";
    private const string FN3_FS = "nfxtest03.txt";
    private const string FN4_FS = "nfxtest04.txt";

    private static string FN2_FS_FULLPATH = Path.Combine(LOCAL_ROOT_FS, FN2_FS);

    private const string CONTENT2 = "The entity tag is a hash of the object. The ETag only reflects changes to the contents of an object, not its metadata. The ETag is determined when an object is created. For objects created by the PUT Object operation and the POST Object operation, the ETag is a quoted, 32-digit hexadecimal string representing the MD5 digest of the object data. For other objects, the ETag may or may not be an MD5 digest of the object data. If the ETag is not an MD5 digest of the object data, it will contain one or more non-hexadecimal characters and/or will consist of less than 32 or more than 32 hexadecimal digits.";
    private static byte[] CONTENT2_BYTES = Encoding.UTF8.GetBytes(CONTENT2);

    private const int PARALLEL_FROM = 0;
    private const int PARALLEL_TO = 10;
    private const string FN_PARALLEL_MASK = @"parallel_fstest_{0}";

    private static S3V4FileSystemSessionConnectParams CONN_PARAMS, CONN_PARAMS_TIMEOUT;

    [TestFixtureSetUp]
    public void SetUp()
    {
      CONN_PARAMS = FileSystemSessionConnectParams.Make<S3V4FileSystemSessionConnectParams>(
        "s3 {{ name='s3v4' bucket='{0}' region='{1}' access-key='{2}' secret-key='{3}' }}"
          .Args(S3_BUCKET, S3_REGION, S3_ACCESSKEY, S3_SECRETKEY));

      CONN_PARAMS_TIMEOUT = FileSystemSessionConnectParams.Make<S3V4FileSystemSessionConnectParams>(
        "s3 {{ name='s3v4' bucket='{0}' region='{1}' access-key='{2}' secret-key='{3}' timeout-ms=1 }}"
          .Args(S3_BUCKET, S3_REGION, S3_ACCESSKEY, S3_SECRETKEY));

      cleanUp();
      initialize();
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      cleanUp();
    }

    [TestCase]
    public void CombinePaths()
    {
      using(var fs = new S3V4FileSystem("S3-1"))
      {
        Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/a.txt", fs.CombinePaths("https://dxw.s3-us-west-2.amazonaws.com/", "a.txt"));
        Assert.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/a.txt", fs.CombinePaths("https://dxw.s3-us-west-2.amazonaws.com/", "/a.txt"));

              
        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "books", "saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "/books", "saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx", "/books", "saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx", "/books", "/saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "books", "/saga.pdf"));


        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx/", "books", "saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx/", "/books", "saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx/", "/books", "saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx/", "/books", "/saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx/", "/books", "/saga.pdf"));

        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx/", "books", "/saga.pdf"));


        Assert.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf", 
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "books", "/saga.pdf"));
      }   
    }

    [TestCase]
    public void Connect_NavigateRootDir()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        using (var fs = new S3V4FileSystem("S3-1"))
        {
          using(var session = fs.StartSession(CONN_PARAMS))
          {
            var dir = session[S3_ROOT_FS] as FileSystemDirectory;

            Assert.AreEqual("/", dir.ParentPath);
            Assert.AreEqual(S3_ROOT_FS, dir.Path);
          }
        }  
      }
    }

    [TestCase]
    public void CreateFile()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          var file = dir.CreateFile(FN1_FS, 1500);

          Assert.AreEqual(FN1_FS, file.Name);

          Assert.IsTrue(S3V4.FileExists(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0));
          Assert.AreEqual(1500, S3V4.GetItemMetadata(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0)["Content-Length"].AsLong());
          Assert.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [TestCase]
    public void CreateFileAsync()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var task = session.GetItemAsync(S3_ROOT_FS);
          //task.Start();
          var dir = task.Result as FileSystemDirectory;

          var task1 = dir.CreateFileAsync(FN1_FS, 1500);
          //task1.Start();
          var file = task1.Result;

          Assert.AreEqual(FN1_FS, file.Name);

          Assert.IsTrue(S3V4.FileExists(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0));
          Assert.AreEqual(1500, S3V4.GetItemMetadata(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0)["Content-Length"].AsLong());
          Assert.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [TestCase]
    public void CreateDeleteFile()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;
          var file = dir.CreateFile(S3_FN1, 1500);

          Assert.AreEqual(S3_FN1, file.Name);

          IDictionary<string, string> headersFN1 = S3V4.GetItemMetadata(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0);
          Assert.AreEqual(1500, headersFN1["Content-Length"].AsInt());
          Assert.AreEqual(1500, file.FileStream.Length);

          Assert.AreEqual(1500, file.Size);

          var file2 = session[fs.CombinePaths(S3_ROOT_FS, S3_FN1)];
          Assert.IsNotNull(file2);
          Assert.IsTrue(file2 is FileSystemFile);
          Assert.AreEqual(S3_FN1, ((FileSystemFile)file2).Name);

          file.Dispose();

          file2.Delete();
          Assert.IsFalse(fileExists((FileSystemFile)file2));
        }
      }
    }

    [TestCase]
    public void CreateWriteCopyReadFile()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (Stream s = new FileStream(FN2_FS_FULLPATH, FileMode.Create, FileAccess.Write))
          {
            var wri = new BinaryWriter(s);

            wri.Write("Hello!");
            wri.Write(true);
            wri.Write(27.4d);
            wri.Close();
          }

          //FN3 copied from FN2 and made readonly

          Assert.IsNotNull(dir.CreateFile(FN3_FS, Path.Combine(LOCAL_ROOT_FS, FN2_FS), true));


          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN3_FS)] as FileSystemFile)
          {
            var str = file.FileStream;

            var rdr = new BinaryReader(str);

            Assert.AreEqual("Hello!", rdr.ReadString());
            Assert.AreEqual(true, rdr.ReadBoolean());
            Assert.AreEqual(27.4d, rdr.ReadDouble());
          }
        }
      }
    }

    [TestCase]
    public void CreateWriteReadFile_1()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (var file = dir.CreateFile(FN2_FS))
          {
            var str = file.FileStream;

            var wri = new BinaryWriter(str);

            wri.Write("Hello!");
            wri.Write(true);
            wri.Write(27.4d);
            wri.Close();
          }

          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN2_FS)] as FileSystemFile)
          {
            var str = file.FileStream;

            var rdr = new BinaryReader(str);

            Assert.AreEqual("Hello!", rdr.ReadString());
            Assert.AreEqual(true, rdr.ReadBoolean());
            Assert.AreEqual(27.4d, rdr.ReadDouble());
          }
        }
      }
    }

    [TestCase]
    public void CreateWriteReadFile_2()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (var file = dir.CreateFile(FN4_FS))
          {
            file.WriteAllText("This is what it takes!");
          }

          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN4_FS)] as FileSystemFile)
          {
            Assert.AreEqual("This is what it takes!", file.ReadAllText());
          }
        }
      }
    }

    [TestCase]
    public void CreateWriteReadFile_3()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (var file = dir.CreateFile(FN4_FS))
          {
            file.WriteAllText("Hello,");
            file.WriteAllText("this will overwrite");
          }

          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN4_FS)] as FileSystemFile)
          {
            Assert.AreEqual("this will overwrite", file.ReadAllText());
          }
        }
      }
    }

    [TestCase]
    public void CreateWriteReadFile_3_Async()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dirTask = session.GetItemAsync(S3_ROOT_FS);
          dirTask.ContinueWith(t => {

            var dir = t.Result as FileSystemDirectory;

            var createFileTask = dir.CreateFileAsync(FN4_FS);
            createFileTask.ContinueWith(t1 => {

              using (var file = t1.Result)
              {
                var writeTask1 = file.WriteAllTextAsync("Hello,");
                writeTask1.Wait();
                var writeTask2 = file.WriteAllTextAsync("this will overwrite");
                writeTask2.ContinueWith(t2 => {

                  var readTask = getFileText(session, fs.CombinePaths(S3_ROOT_FS, FN4_FS));

                  Assert.AreEqual("this will overwrite", readTask.Result);

                });
              }
             
            });

          });
        }
      }
    }

            private async Task<string> getFileText(FileSystemSession session, string path)
            {
              using (var file = await session.GetItemAsync(path) as FileSystemFile)
              {
                return await file.ReadAllTextAsync();
              }  
            }

    [TestCase]
    public void CreateDeleteDir()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Assert.IsNull(dir2);

          dir2 = dir.CreateDirectory(DIR_1);

          Assert.AreEqual(DIR_1, ((FileSystemDirectory)dir[DIR_1]).Name);

          Assert.AreEqual(DIR_1, ((FileSystemDirectory)dir2[""]).Name);

          Assert.IsTrue(folderExists(dir2));

          Assert.AreEqual(DIR_1, dir2.Name);

          dir2.Delete();
          Assert.IsFalse(folderExists(dir2));
        }
      }
    }

    [TestCase]
    public void CreateDirCreateFileDeleteDir()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;
          Assert.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor                                                           
          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Assert.IsNull(dir2);
          Assert.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor                                                                            
          dir2 = dir.CreateDirectory(DIR_1);
          Assert.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor             
          Assert.IsTrue(folderExists(dir2));

          Assert.AreEqual(DIR_1, dir2.Name);

          var f = dir2.CreateFile(S3_FN1, 237);
          Assert.AreEqual(3, session.Items.Count());//checking item registation via .ctor/.dctor
          Assert.IsTrue(fileExists(f));

          Assert.AreEqual(237, dir2.Size);

          Assert.IsTrue(dir.SubDirectoryNames.Contains(DIR_1));

          Assert.IsTrue(dir2.FileNames.Contains(S3_FN1));


          dir2.Delete();
          Assert.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
          Assert.IsFalse(folderExists(dir2));
          Assert.IsFalse(fileExists(f));
          Assert.AreEqual(1, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          session.Dispose();
          Assert.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          Assert.AreEqual(0, session.Items.Count());//checking item registation via .ctor/.dctor
        }  
      }
    }

    [TestCase]
    public void CreateRefreshDeleteDirAsync()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;
          dir.CreateDirectoryAsync(DIR_1).ContinueWith(t => {
            var dir1 = t.Result;
            dir1.RefreshAsync().Wait();
            dir1.DeleteAsync().Wait();
          });
        }  
      }
    }

    [TestCase]
    public void Parallel_CreateWriteReadFile()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);
            using(var session = fs.StartSession())
            {
              var dir = session[S3_ROOT_FS] as FileSystemDirectory;

              using (var file = dir.CreateFile(fn))
              {
                file.WriteAllText("Hello, {0}".Args(i));
              }

              using (var file = session[fs.CombinePaths(S3_ROOT_FS, fn)] as FileSystemFile)
              {
                Assert.AreEqual("Hello, {0}".Args(i), file.ReadAllText());
                file.Delete();
              }
              Assert.IsNull(session[fs.CombinePaths(S3_ROOT_FS, fn)]);
            }

          });//Parallel.For
      }
    }

    [TestCase]
    public void Parallel_CreateWriteReadFile_Async()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        var tasks = new List<Task>();

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);

            var session = fs.StartSession();

            FileSystemDirectory dir = null;
            FileSystemFile file = null;

            var t = session.GetItemAsync(S3_ROOT_FS)
            .OnOk(item => 
            {
              dir = item as FileSystemDirectory;
              return dir.CreateFileAsync(fn);
                 
            }).OnOk(f => {
              Console.WriteLine("file '{0}' created", f.Name);
              file = f;
              return file.WriteAllTextAsync("Hello, {0}".Args(i));
            })
            .OnOkOrError(_ => {
              Console.WriteLine("text written into '{0}'", file.Name);
              if (file != null && !file.Disposed) {
                  file.Dispose(); 
                  Console.WriteLine("file '{0}' disposed", file.Name);
              } 
            })
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(S3_ROOT_FS, fn)) )
            .OnOk(item => {
              file = item as FileSystemFile;
              Console.WriteLine("file {0} got", file.Name);
              return file.ReadAllTextAsync();
            })
            .OnOk(txt => {
              Console.WriteLine("file '{0}' red {1}", file.Name, txt);
              Assert.AreEqual("Hello, {0}".Args(i), txt);
              return file.DeleteAsync();
            })
            .OnOkOrError(_ => {
              Console.WriteLine("file '{0}' deleted", file.Name);
              if (file != null && !file.Disposed) {
                file.Dispose(); 
                Console.WriteLine("file '{0}' disposed", file.Name);
              }
             })
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(S3_ROOT_FS, fn)) )
            .OnOk(item => { Assert.IsNull(item); })
            .OnOkOrError(_ => { if (!session.Disposed) session.Dispose(); } );

            tasks.Add(t);
          });//Parallel.For

          Console.WriteLine("all tasks created");

          Task.WaitAll(tasks.ToArray());

          Assert.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor

          Console.WriteLine("done");
      }
    }

    [Test]
    [ExpectedException(typeof(System.Net.WebException), MatchType = MessageMatch.Contains)]
    public void FailedFastTimeout()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession(CONN_PARAMS_TIMEOUT))
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          Assert.AreEqual("/", dir.ParentPath);
          Assert.AreEqual(S3_ROOT_FS, dir.Path);
        }
      }
    }

    private void cleanUp()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        deleteFile(FN2_FS_FULLPATH);

        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          if (dir != null)
            dir.Delete();
        }
      }
    }

    private void initialize()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        using (Stream s = new FileStream(FN2_FS_FULLPATH, FileMode.Create, FileAccess.Write))
        {
          s.Write(CONTENT2_BYTES, 0, CONTENT2_BYTES.Length);
        }

        S3V4.PutFolder(S3_ROOT_FS, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0); 
      }
    }

    private void deleteFile(string name)
    {
      try
      {
        var fi = new FileInfo(name);
        if (fi.IsReadOnly) fi.IsReadOnly = false;

        fi.Delete();
      }
      catch{}
    }

    private bool fileExists(FileSystemFile file)
    {
      var handle = (S3V4FSH)file.Handle;
      return S3V4.FileExists(handle.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0); 
    }

    private bool folderExists(FileSystemDirectory folder)
    {
      var handle = (S3V4FSH)folder.Handle;
      return S3V4.FolderExists(handle.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0); 
    }

    private IConfigSectionNode getFSNode(string name)
    {
      return LACONF.AsLaconicConfig()[NFX.IO.FileSystem.FileSystem.CONFIG_FILESYSTEMS_SECTION].Children.First(c => c.IsSameNameAttr(name));
    }
  }
}
