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
using NFX.IO.FileSystem.Packaging;
using NFX.Security;

namespace NFX.NUnit.IO
{
    [TestFixture]  
    public class DeepCopyTests
    {

        [TestCase]
        public void DeepCopy_1()
        {
            var p1 = ManifestUtilsTests.Get_TEZT_PATH();
            var p2 = p1+"_2";

            IOMiscUtils.EnsureDirectoryDeleted(p2);
            Directory.CreateDirectory(p2);

            using(var fs = new LocalFileSystem("L1"))
            {
              using(var session = fs.StartSession())
              {
                var fromDir = session[p1] as FileSystemDirectory; 
                var manifest1 = ManifestUtils.GeneratePackagingManifest(fromDir);
              
                var toDir = session[p2] as FileSystemDirectory; 

                fromDir.DeepCopyTo(toDir, FileSystemDirectory.DirCopyFlags.Directories | FileSystemDirectory.DirCopyFlags.Files);
                var manifest2 = ManifestUtils.GeneratePackagingManifest(toDir);


                Console.WriteLine(manifest1.Configuration.ContentView);

                Assert.IsTrue( ManifestUtils.HasTheSameContent(manifest1, manifest2) );
              }
            } 
        }


    }
}
