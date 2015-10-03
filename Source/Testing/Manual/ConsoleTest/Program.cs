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
using System.Linq;
using System.Text;

using NFX;
using NFX.ApplicationModel;
using NFX.Log;

//using ConsoleTest.S3;
using ConsoleTest.WebDAVTest;
using ConsoleTest.S3;

namespace ConsoleTest
{

  class Program
  {

    static void Main(string[] args)
    {
      //S3Test.Run();
      //S3V4FilesystemTests.ListBucket();

      //WebDAVClientTest.List();

      //ConsoleTest.SettingsTest.WebSettingsTest.Test();

      //(new ConsoleTest.Pay.EPayTest()).PayStarterTest();// StripeTest();

      //(new FileSystem.FileSystemTest()).TestFSCreation();

      //Social.SocialTest.Test01();

      //CacheTest.Run(16, 20, 150000, 40);

      GoogleDrive.GoogleDriveFileSystemTests.Run();

      Console.WriteLine("done"); Console.ReadKey();

        //Console.WriteLine("Server starting with arguments: " + string.Join(" ", args));

        //using(new ServiceBaseApplication(args, null))
        //{
        //      //Throttling.Run();
        //      //   Console.WriteLine("test string".ToGUID());
        //      //Markup.Run();
        //      //Config.Run();
        //      //ConsoleLog.Run();
        //      //CSTemplate.Run();

        //     // ServiceBaseApplication.WriteToLog("Fame-mouse Japanese singer Atomuli Ya Dala");



        //      GlueServer.Run();

        //      Console.WriteLine("Bitte shtreken sie ein knopken <ENTER>");
        //      Console.ReadLine();

        //}
    }


  }
  
}
