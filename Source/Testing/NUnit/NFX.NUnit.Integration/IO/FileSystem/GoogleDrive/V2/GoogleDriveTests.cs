using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;
using NFX.IO.FileSystem;
using NFX.IO.FileSystem.GoogleDrive;
using NFX.IO.FileSystem.GoogleDrive.V2;

namespace NFX.NUnit.Integration.IO.FileSystem.GoogleDrive.V2
{
  using FS = NFX.IO.FileSystem;
  using System.Collections.Generic;
  using System;

  [TestFixture]
  class GoogleDriveTests : ExternalCfg
  {
    private const string ROOT = "/nfx-root";
    private const string ROOT_ID = "root";
    private const string LOCAL_ROOT = @"c:\NFX";
    private static string FILE2_FULLPATH = Path.Combine(LOCAL_ROOT, FILE2);

    private const string DIR_1 = "dir1";
    private const string FILE1 = "nfxtest01.txt";
    private const string FILE2 = "nfxtest02.txt";
    private const string FILE3 = "nfxtest03.txt";
    private const string FILE4 = "nfxtest04.txt";

    private const int PARALLEL_FROM = 0;
    private const int PARALLEL_TO = 10;
    private const string FN_PARALLEL_MASK = @"parallel_fstest_{0}";

    private static GoogleDriveParameters CONN_PARAMS, CONN_PARAMS_TIMEOUT;

    [TestFixtureSetUp]
    public void SetUp()
    {
      var csDefault = "google-drive{{ email='{0}' cert-path=$'{1}' }}"
        .Args(GOOGLE_DRIVE_EMAIL, GOOGLE_DRIVE_CERT_PATH);

      CONN_PARAMS = FileSystemSessionConnectParams.Make<GoogleDriveParameters>(csDefault);

      var csTimeout = "google-drive{{ email='{0}' cert-path=$'{1}' timeout-ms=1 }}"
        .Args(GOOGLE_DRIVE_EMAIL, GOOGLE_DRIVE_CERT_PATH);

      CONN_PARAMS_TIMEOUT = FileSystemSessionConnectParams.Make<GoogleDriveParameters>(csTimeout);

      cleanUp();
      initialize();
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      cleanUp();
    }

    [TestCase]
    public void GoogleDrive_NavigateToRoot()
    {
      using(CreateApplication())
      {
        using (var fs = new GoogleDriveFileSystem("GoogleDrive"))
        {
          using(var session = StartSession(fs))
          {
            var dir = session[ROOT] as FileSystemDirectory;

            Assert.AreEqual("/", dir.ParentPath);
            Assert.AreEqual(ROOT, dir.Path);
          }
        }  
      }
    }

    [TestCase]
    public void GoogleDrive_CreateFile()
    {
      using(CreateApplication())
      {        
        using(var session = StartSession())
        {
          var dir = session[ROOT] as FileSystemDirectory;

          var file = dir.CreateFile(FILE1, 1500);

          Assert.AreEqual(FILE1, file.Name);

          Assert.IsTrue(session.Client.FileExists(file.Path));
          Assert.AreEqual(1500, file.Size);
          Assert.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateFileAsync()
    {
      using(CreateApplication())
      {
        using(var session = StartSession())
        {
          var task = session.GetItemAsync(ROOT);

          var dir = task.Result as FileSystemDirectory;

          var task1 = dir.CreateFileAsync(FILE1, 1500);

          var file = task1.Result;

          Assert.AreEqual(FILE1, file.Name);

          var client = GetClient(session);

          Assert.IsTrue(client.FileExists(file.Path));
          Assert.AreEqual(1500, file.Size);
          Assert.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateDeleteFile()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;
          var file = dir.CreateFile(FILE1, 1500);

          Assert.AreEqual(FILE1, file.Name);

          var fileId = (file.Handle as GoogleDriveHandle).Id;
                   
          var item = session.Client.GetItemInfoById(fileId);
          Assert.AreEqual(1500, item.Size);
          Assert.AreEqual(1500, file.FileStream.Length);

          var file2 = session[fs.CombinePaths(ROOT, FILE1)];
          Assert.IsNotNull(file2);
          Assert.IsTrue(file2 is FileSystemFile);
          Assert.AreEqual(FILE1, file2.Name);

          file.Dispose();

          file2.Delete();
          Assert.IsFalse(session.Client.FileExists(file2.Path));
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateWriteCopyReadFile()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var stream = new FileStream(FILE2_FULLPATH, FileMode.Create, FileAccess.Write))
          {
            var writer = new BinaryWriter(stream);

            writer.Write("Hello!");
            writer.Write(true);
            writer.Write(27.4d);
            writer.Close();
          }

          // FILE3 copied from FILE2 and made readonly.
          Assert.IsNotNull(dir.CreateFile(FILE3, Path.Combine(LOCAL_ROOT, FILE2), true));

          using (var file = session[fs.CombinePaths(ROOT, FILE3)] as FileSystemFile)
          {
            var str = file.FileStream;

            var reader = new BinaryReader(str);

            Assert.AreEqual("Hello!", reader.ReadString());
            Assert.AreEqual(true, reader.ReadBoolean());
            Assert.AreEqual(27.4d, reader.ReadDouble());
          }
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateWriteReadFile_1()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var file = dir.CreateFile(FILE2))
          {
            var str = file.FileStream;

            var writer = new BinaryWriter(str);

            writer.Write("Hello!");
            writer.Write(true);
            writer.Write(27.4d);
            writer.Close();
          }

          using (var file = session[fs.CombinePaths(ROOT, FILE2)] as FileSystemFile)
          {
            var str = file.FileStream;

            var reader = new BinaryReader(str);

            Assert.AreEqual("Hello!", reader.ReadString());
            Assert.AreEqual(true, reader.ReadBoolean());
            Assert.AreEqual(27.4d, reader.ReadDouble());
          }
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateWriteReadFile_2()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var file = dir.CreateFile(FILE4))
          {
            file.WriteAllText("This is what it takes!");
          }

          using (var file = session[fs.CombinePaths(ROOT, FILE4)] as FileSystemFile)
          {
            Assert.AreEqual("This is what it takes!", file.ReadAllText());
          }
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateWriteReadFile_3()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var file = dir.CreateFile(FILE4))
          {
            file.WriteAllText("Hello,");
            file.WriteAllText("this will overwrite");
          }

          using (var file = session[fs.CombinePaths(ROOT, FILE4)] as FileSystemFile)
          {
            Assert.AreEqual("this will overwrite", file.ReadAllText());
          }
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateWriteReadFile_3_Async()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dirTask = session.GetItemAsync(ROOT);
          dirTask.ContinueWith(t => {

            var dir = t.Result as FileSystemDirectory;

            var createFileTask = dir.CreateFileAsync(FILE4);
            createFileTask.ContinueWith(t1 => {

              using (var file = t1.Result)
              {
                var writeTask1 = file.WriteAllTextAsync("Hello,");
                writeTask1.Wait();
                var writeTask2 = file.WriteAllTextAsync("this will overwrite");
                writeTask2.ContinueWith(t2 => {

                  var readTask = GetFileText(session, fs.CombinePaths(ROOT, FILE4));

                  Assert.AreEqual("this will overwrite", readTask.Result);

                });
              }
             
            });

          });
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateDeleteDir()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Assert.IsNull(dir2);

          dir2 = dir.CreateDirectory(DIR_1);

          Assert.AreEqual(DIR_1, ((FileSystemDirectory)dir[DIR_1]).Name);

          Assert.AreEqual(DIR_1, ((FileSystemDirectory)dir2[""]).Name);

          Assert.IsTrue(session.Client.FolderExists(dir2.Path));

          Assert.AreEqual(DIR_1, dir2.Name);

          dir2.Delete();
          Assert.IsFalse(session.Client.FolderExists(dir2.Path));
        }
      }
    }

    [TestCase]
    public void GoogleDrive_CreateDirCreateFileDeleteDir()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;
          Assert.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor                                                           
          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Assert.IsNull(dir2);
          Assert.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor                                                                            
          dir2 = dir.CreateDirectory(DIR_1);
          Assert.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor             
          Assert.IsTrue(session.Client.FolderExists(dir2.Path));

          Assert.AreEqual(DIR_1, dir2.Name);

          var f = dir2.CreateFile(FILE1, 237);
          Assert.AreEqual(3, session.Items.Count());//checking item registation via .ctor/.dctor
          Assert.IsTrue(session.Client.FileExists(f.Path));

          Assert.AreEqual(237, dir2.Size);

          Assert.IsTrue(dir.SubDirectoryNames.Contains(DIR_1));

          Assert.IsTrue(dir2.FileNames.Contains(FILE1));

          dir2.Delete();
          Assert.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
          Assert.IsFalse(session.Client.FolderExists(dir2.Path));
          Assert.IsFalse(session.Client.FileExists(f.Path));
          Assert.AreEqual(1, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          session.Dispose();
          Assert.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          Assert.AreEqual(0, session.Items.Count());//checking item registation via .ctor/.dctor
        }  
      }
    }

    [TestCase]
    public void GoogleDrive_CreateRefreshDeleteDirAsync()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          var task = dir.CreateDirectoryAsync(DIR_1).ContinueWith(t => {
            var dir1 = t.Result;
            dir1.RefreshAsync().Wait();
            dir1.DeleteAsync().Wait();
          });
          
          task.Wait();
        }  
      }
    }

    [TestCase]
    public void GoogleDrive_Parallel_CreateWriteReadFile()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);
            using(var session = StartSession(fs))
            {
              var dir = session[ROOT] as FileSystemDirectory;

              using (var file = dir.CreateFile(fn))
              {
                file.WriteAllText("Hello, {0}".Args(i));
              }

              using (var file = session[fs.CombinePaths(ROOT, fn)] as FileSystemFile)
              {
                Assert.AreEqual("Hello, {0}".Args(i), file.ReadAllText());
                file.Delete();
              }
              Assert.IsNull(session[fs.CombinePaths(ROOT, fn)]);
            }

          });//Parallel.For
      }
    }

    [TestCase]
    public void GoogleDrive_Parallel_CreateWriteReadFile_Async()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        var tasks = new List<Task>();

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);

            var session = StartSession(fs);

            FileSystemDirectory dir = null;
            FileSystemFile file = null;

            var t = session.GetItemAsync(ROOT)
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
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(ROOT, fn)) )
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
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(ROOT, fn)) )
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
    [ExpectedException(typeof(FileSystemException), MatchType = MessageMatch.Contains)]
    public void GoogleDrive_FailedFastTimeout()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs, CONN_PARAMS_TIMEOUT))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          Assert.AreEqual("/", dir.ParentPath);
          Assert.AreEqual(ROOT, dir.Path);
        }
      }
    }

    private async Task<string> GetFileText(FileSystemSession session, string path)
    {
      using (var file = await session.GetItemAsync(path) as FileSystemFile)
      {
        return await file.ReadAllTextAsync();
      }  
    }

    private NFX.ApplicationModel.ServiceBaseApplication CreateApplication()
    {
      return new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig());
    }

    private GoogleDriveFileSystem GetFileSystem()
    {
      return FS.FileSystem.Instances[NFX_GOOGLE_DRIVE] as GoogleDriveFileSystem;
    }

    private GoogleDriveSession StartSession(GoogleDriveFileSystem fs = null, GoogleDriveParameters connParams = null)
    {
      if (fs == null)
      {
        fs = GetFileSystem();
      }

      return fs.StartSession(connParams ?? CONN_PARAMS) as GoogleDriveSession;
    }

    private void cleanUp()
    {
      using(CreateApplication())
      {
        var fs = FS.FileSystem.Instances[NFX_GOOGLE_DRIVE];

        using(var session = fs.StartSession(CONN_PARAMS))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          if (dir != null)
          {
            dir.Delete();
          }
        }
      }
    }

    private void initialize()
    {
      var client = new GoogleDriveClient(GOOGLE_DRIVE_EMAIL, GOOGLE_DRIVE_CERT_PATH);

      client.CreateDirectory(ROOT_ID, ROOT.Trim('/'));      
    }

    private GoogleDriveClient GetClient(FileSystemSession session)
    {
      return (session as GoogleDriveSession).Client;
    }
  }
}
