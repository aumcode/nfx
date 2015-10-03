using System;
using System.Collections.Generic;

using NFX;
using NFX.Environment;
using NFX.IO.FileSystem;
using NFX.IO.FileSystem.GoogleDrive.V2;
using NFX.IO.FileSystem.GoogleDrive;
using NFX.IO.FileSystem.GoogleDrive.Auth;

namespace ConsoleTest.GoogleDrive
{
    public class GoogleDriveFileSystemTests
    {
        private const string NFX_GOOGLE_DRIVE = "NFX_GOOGLE_DRIVE";
        
        private const string TEST_DIR = "nfx-test";
        private const string LOCAL_FILE = "ConsoleTest.exe.config";

        private static GoogleDriveParameters CONN_PARAMS;
        
        static GoogleDriveFileSystemTests()
        {
            try
            {
                var env = System.Environment.GetEnvironmentVariable(NFX_GOOGLE_DRIVE);

                var cfg = Configuration.ProviderLoadFromString(env, Configuration.CONFIG_LACONIC_FORMAT).Root;
                CONN_PARAMS = FileSystemSessionConnectParams.Make<GoogleDriveParameters>(env);
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format(
                    "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added",
                        NFX_GOOGLE_DRIVE,
                        "google-drive{ client-id='<value>' client-secret='<value>' access-token='<value>' refresh-token='<value>' }"
                    ),
                    ex
                );
            }
        }      

        public static void Run()
        {
            using(var fs = new GoogleDriveFileSystem("GoogleDrive"))
            {
              Desc("Starting session");
              var session = fs.StartSession(CONN_PARAMS);
              Ok();

              Desc("Getting root directory");
              var root = session["/"] as FileSystemDirectory;
              Ok();

              Desc("Creating directory {0}", TEST_DIR);
              var testDir = root.CreateDirectory(TEST_DIR);
              Ok();

              Desc("file1.xml from {0}", LOCAL_FILE);
              var file1 = testDir.CreateFile("file1.xml", LOCAL_FILE);
              Ok();

              Desc("Creating an empty file2.txt");
              var file2 = testDir.CreateFile("file2.txt", 100);
              Ok();

              Desc("Creating folder1");
              var folder1 = testDir.CreateDirectory("folder1");
              Ok();

              Desc("Creating folder1/folder1-file1");
              folder1.CreateFile("folder1-file1");
              Ok();

              Desc("Creating folder1/folder1-file2");
              folder1.CreateFile("folder1-file2");
              Ok();

              Desc("Creating folder2");
              var folder2 = testDir.CreateDirectory("folder2");
              Ok();

              Desc("Creating folder2/folder1-file1");
              folder2.CreateFile("folder2-file1");
              Ok();

              Desc("Creating folder2/folder1-file2");
              folder2.CreateFile("folder2-file2");
              Ok();

              Desc("Renaming file2.txt");
              file2.Rename("renamed");
              Ok();
              
              Desc("Deleting file1.xml");
              file1.Delete();
              Ok();

              PrintHeader("Directory nfx-test info".Args(testDir.Name));
              PrintItemInfo(testDir);

              PrintHeader("File {0} info".Args(file2.Name));
              PrintItemInfo(file2);

              Console.WriteLine("Folders:");
              Print(testDir.SubDirectoryNames);

              Console.WriteLine("Files:");
              Print(testDir.FileNames);

              PrintHeader("Recursive");

              Console.WriteLine("Folders:");
              Print(testDir.RecursiveSubDirectoryNames);

              Console.WriteLine("Files:");
              Print(testDir.RecursiveFileNames);

              testDir.Delete();
            }
        }

        private static void PrintItemInfo(FileSystemSessionItem item)
        {
            Console.WriteLine("Name:\t\t\t{0}", item.Name);
            Console.WriteLine("Size:\t\t\t{0}", item.Size);
            Console.WriteLine("Path:\t\t\t{0}", item.Path);
            Console.WriteLine("Parent:\t\t\t{0}", item.ParentPath);

            Console.WriteLine("IsReadOnly:\t\t{0}", item.IsReadOnly);
            Console.WriteLine("CreationUser:\t\t{0}", item.CreationUser);
            Console.WriteLine("CreationTimestamp:\t{0}", item.CreationTimestamp);

            Console.WriteLine("LastAccessUser:\t\t{0}: ", item.LastAccessUser);

            Console.WriteLine("Modified: {0}\t\t", item.Modified);
            Console.WriteLine("ModificationTimestamp:\t{0}", item.ModificationTimestamp);
            Console.WriteLine("ModificationUser:\t{0}", item.ModificationUser);
        }

        private static void PrintHeader(string name)
        {
          Console.WriteLine();
          Console.WriteLine("============================================");
          Console.WriteLine("  " + name);
          Console.WriteLine("============================================");
          Console.WriteLine();
        }

        private static void Print(IEnumerable<string> items)
        {
          foreach (var item in items)
          {
            Console.WriteLine(" - " + item);
          }
        }

        private static void Desc(string text, params object[] args)
        {
          text = text.Args(args).PadRight(40);
          Console.Write(text);
        }

        private static void Ok()
        {
          var foreground = Console.ForegroundColor;

          Console.ForegroundColor = ConsoleColor.Green;

          Console.Write("[OK]\n");

          Console.ForegroundColor = foreground;
        }
    }
}
