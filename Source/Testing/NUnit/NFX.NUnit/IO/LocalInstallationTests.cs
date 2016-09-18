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

using NUnit.Framework;

using NFX.Environment;
using NFX.IO.FileSystem;
using NFX.IO.FileSystem.Local;
using NFX.IO.FileSystem.Packaging;
using NFX.Security;

namespace NFX.NUnit.IO
{
    [TestFixture]  
    public class LocalInstallationTests
    {

        [TestCase]
        public void Install_1()
        {
            var source = ManifestUtilsTests.Get_TEZT_PATH();
            var target = source+"_INSTALLED";

            using(var fs = new LocalFileSystem("L1"))
            {
              var session = fs.StartSession();
              var sourceDir = session[source] as FileSystemDirectory; 
            
              var manifest = ManifestUtils.GeneratePackagingManifest(sourceDir, packageName: "Package1");
              var fn = Path.Combine(source, ManifestUtils.MANIFEST_FILE_NAME);
              try
              {
                  manifest.Configuration.ToLaconicFile(fn);


                  var set = new List<LocalInstallation.PackageInfo>
                  {
                      new LocalInstallation.PackageInfo("Package1", sourceDir, null)//no relative path
                  };
              

                  using(var install = new LocalInstallation(target))
                  {
                    Console.WriteLine("Installed anew: "+ install.CheckLocalAndInstallIfNeeded(set));
                  }
              }
              finally
              {
                IOMiscUtils.EnsureFileEventuallyDeleted(fn);
              }
              
            }   
        }



    }
}
