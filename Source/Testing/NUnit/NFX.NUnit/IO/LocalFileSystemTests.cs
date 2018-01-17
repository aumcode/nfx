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
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

using NFX.IO.FileSystem;
using NFX.IO.FileSystem.Local;
using NFX.Security;

namespace NFX.NUnit.IO
{
    [TestFixture]  
    public class LocalFileSystemTests
    {
        public const string LOCAL_ROOT = @"c:\NFX";

        public const string FN1 = @"fstest1";
        public const string FN2 = @"fstest2.tezt";
        public const string FN3 = @"fstest3.txt";
        public const string FN4 = @"fstest4.txt";

        public const string FN_PARALLEL_MASK = @"parallel_fstest_{0}";


        public const int PARALLEL_FROM = 0;
        public const int PARALLEL_TO = 1000;

        public const string DIR_1 = "dir1";

        [TestFixtureSetUp]
        public void Setup()
        {
          cleanup();
        }

        [TestFixtureTearDownAttribute]
        public void TearDown()
        {
          cleanup();
        }

        private void cleanup()
        {
          for(var i=PARALLEL_FROM; i<PARALLEL_TO; i++)
           try
           {
             deleteFile( Path.Combine(LOCAL_ROOT, FN_PARALLEL_MASK.Args(i)) );  
           }
           catch{}

          deleteFile( Path.Combine(LOCAL_ROOT, FN1) );
          deleteFile( Path.Combine(LOCAL_ROOT, FN2) );
          deleteFile( Path.Combine(LOCAL_ROOT, FN3) );
          deleteFile( Path.Combine(LOCAL_ROOT, FN4) );
          try{Directory.Delete( Path.Combine(LOCAL_ROOT, DIR_1), true );} catch{}
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


        [TestCase]
        public void CombinePaths()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              Assert.AreEqual(@"c:\a.txt", fs.CombinePaths(@"c:\",@"a.txt"));
              Assert.AreEqual(@"c:\a.txt", fs.CombinePaths(@"c:\",@"\a.txt"));
              
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books",@"fiction","saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books",@"\fiction","saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books",@"\fiction",@"saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books",@"\fiction",@"\saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books",@"fiction", @"\saga.pdf"));
              
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books\",@"fiction","saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books\",@"\fiction","saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books\",@"\fiction",@"saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books\",@"\fiction",@"\saga.pdf"));
              Assert.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books\",@"fiction", @"\saga.pdf"));

              Assert.AreEqual(@"\books\fiction\saga.pdf", fs.CombinePaths("",@"books",@"fiction", @"\saga.pdf"));
            }   
        }


        [TestCase]
        public void Connect_NavigateRootDir()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              Assert.AreEqual(@"c:\", dir.ParentPath);
              Assert.AreEqual(@"c:\NFX", dir.Path);
            }   
        }

        [TestCase]
        public void CreateFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
            
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              var file = dir.CreateFile(FN1, 1500);

              Assert.AreEqual(FN1, file.Name);
            
              Assert.IsTrue( File.Exists(file.Path));
              Assert.AreEqual(1500, new FileInfo(file.Path).Length);
              Assert.AreEqual(1500, file.FileStream.Length);
            }   
        }

        [TestCase]
        public void CreateDeleteFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {

            
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              var file = dir.CreateFile(FN1, 1500);

            

              Assert.AreEqual(FN1, file.Name);
            
              Assert.IsTrue( File.Exists(file.Path));
              Assert.AreEqual(1500, new FileInfo(file.Path).Length);
              Assert.AreEqual(1500, file.FileStream.Length);   

              Assert.AreEqual(1500, file.Size);

              var file2 = session[fs.CombinePaths(LOCAL_ROOT, FN1)];
              Assert.IsNotNull( file2);
              Assert.IsTrue(file2 is FileSystemFile);
              Assert.AreEqual(FN1, ((FileSystemFile)file2).Name);

              file.Dispose();

              file2.Delete();
              Assert.IsFalse( File.Exists(file2.Path));
            }
        }


        [TestCase]
        public void CreateWriteReadFile_1()
        {
            using(var fs = new LocalFileSystem("L1"))
            {

            
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              using (var file = dir.CreateFile(FN2))
              {
                var str = file.FileStream;

                var wri = new BinaryWriter(str);

                wri.Write( "Hello!" );
                wri.Write( true );
                wri.Write( 27.4d );
                wri.Close();
              }

              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN2)] as FileSystemFile)
              {
                var str = file.FileStream;

                var rdr = new BinaryReader(str);

                Assert.AreEqual( "Hello!", rdr.ReadString()); 
                Assert.AreEqual( true,     rdr.ReadBoolean()); 
                Assert.AreEqual( 27.4d,    rdr.ReadDouble()); 
              }
            }
        }


        [TestCase]
        public void CreateWriteReadFile_2()
        {
            using(var fs = new LocalFileSystem("L1"))
            {

            
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              using (var file = dir.CreateFile(FN4))
              {
                file.WriteAllText("This is what it takes!");
              }

              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN4)] as FileSystemFile)
              {
                Assert.AreEqual( "This is what it takes!", file.ReadAllText()); 
              }
            }
        }

        [TestCase]
        public void CreateWriteReadFile_3()
        {
            using(var fs = new LocalFileSystem("L1"))
            {

            
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              using (var file = dir.CreateFile(FN4))
              {
                file.WriteAllText("Hello,");
                file.WriteAllText("this will overwrite");
              }

              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN4)] as FileSystemFile)
              {
                Assert.AreEqual( "this will overwrite", file.ReadAllText()); 
              }
            }
        }



        [TestCase]
        public void CreateDeleteDir()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
            
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              var dir2 = dir[DIR_1] as FileSystemDirectory;
              Assert.IsNull( dir2 );
                  
              dir2 = dir.CreateDirectory(DIR_1);
              
              Assert.AreEqual(DIR_1, ((FileSystemDirectory)dir[DIR_1]).Name);

              Assert.AreEqual(DIR_1, ((FileSystemDirectory)dir2[""]).Name);

              Assert.IsTrue( Directory.Exists(dir2.Path));

              Assert.AreEqual(DIR_1, dir2.Name);

              dir2.Delete();
              Assert.IsFalse( Directory.Exists(dir2.Path));

            }   
        }

        [TestCase]
        public void CreateDirCreateFileDeleteDir()
        {
            using(var fs = new LocalFileSystem("L1"))      
            {
            
              var session = fs.StartSession();    

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            
 Assert.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor                                                           
              var dir2 = dir[DIR_1] as FileSystemDirectory; 
              Assert.IsNull( dir2 );
 Assert.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor                                                                            
              dir2 = dir.CreateDirectory(DIR_1);
 Assert.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor             
              Assert.IsTrue( Directory.Exists(dir2.Path));

              Assert.AreEqual(DIR_1, dir2.Name);

              var f = dir2.CreateFile(FN1, 237);
Assert.AreEqual(3, session.Items.Count());//checking item registation via .ctor/.dctor
              Assert.IsTrue( File.Exists(f.Path));

              Assert.AreEqual(237, dir2.Size);

              Assert.IsTrue( dir.SubDirectoryNames.Contains(DIR_1) );
  
              Assert.IsTrue( dir2.FileNames.Contains(FN1) );
  

              dir2.Delete();
Assert.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
              Assert.IsFalse( Directory.Exists(dir2.Path));
              Assert.IsFalse( File.Exists(f.Path));
Assert.AreEqual(1, fs.Sessions.Count());//checking item registation via .ctor/.dctor
              session.Dispose();
Assert.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor
Assert.AreEqual(0, session.Items.Count());//checking item registation via .ctor/.dctor
            }   
        }



        [TestCase]
        public void CreateWriteCopyReadFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

              using (var file = dir.CreateFile(FN2))
              {
                var str = file.FileStream;

                var wri = new BinaryWriter(str);

                wri.Write( "Hello!" );
                wri.Write( true );
                wri.Write( 27.4d );
                wri.Close();
              }

              //FN3 copied from FN2 and made readonly
              Assert.IsNotNull( dir.CreateFile(FN3, Path.Combine(LOCAL_ROOT, FN2), true) );
                                   

              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN3)] as FileSystemFile)
              {
                Assert.IsTrue( file.ReadOnly);
                Assert.IsTrue( file.IsReadOnly);

                var str = file.FileStream;

                var rdr = new BinaryReader(str);

                Assert.AreEqual( "Hello!", rdr.ReadString()); 
                Assert.AreEqual( true,     rdr.ReadBoolean()); 
                Assert.AreEqual( 27.4d,    rdr.ReadDouble()); 

                file.ReadOnly = false;

                Assert.IsFalse( file.ReadOnly);
                Assert.IsFalse( file.IsReadOnly);

              }
            }
        }


        [TestCase]
        public void Parallel_CreateWriteReadFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
               (i)=>
               {
                    var fn = FN_PARALLEL_MASK.Args(i); 
                    var session = fs.StartSession();

                    var dir = session[LOCAL_ROOT] as FileSystemDirectory;            

                    using (var file = dir.CreateFile(fn))
                    {
                      file.WriteAllText("Hello, {0}".Args(i));
                    }

                    using (var file = session[fs.CombinePaths(LOCAL_ROOT, fn)] as FileSystemFile)
                    {
                      Assert.AreEqual( "Hello, {0}".Args(i), file.ReadAllText()); 
                      file.Delete(); 
                    }
                    Assert.IsNull( session[fs.CombinePaths(LOCAL_ROOT, fn)] );
                    
               });//Parallel.For

            }
        }





    }
}
