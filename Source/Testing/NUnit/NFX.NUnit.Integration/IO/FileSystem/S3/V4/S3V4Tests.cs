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
using NFX.IO.FileSystem.S3.V4;
using NUnit.Framework;

namespace NFX.NUnit.Integration.IO.FileSystem.S3.V4
{
  [TestFixture]
  public class S3V4Tests: ExternalCfg
  {
    [Test]
    public void PutFolder()
    {
      using(new NFX.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        string fullFolderName = S3_DXW_ROOT;
        S3V4URI folderUri = S3V4URI.CreateFolder(fullFolderName);

        if (S3V4.FolderExists(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0))
          S3V4.RemoveFolder(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0);

        S3V4.PutFolder(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0);

        PutFile();

        S3V4.RemoveFolder(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0); 
      }
    }

    public void PutFile()
    {
      string fullFileName = S3_DXW_ROOT + "/" + S3_FN1;
      S3V4URI fileUri = new S3V4URI(fullFileName);

      if (S3V4.FileExists(fileUri, S3_ACCESSKEY, S3_SECRETKEY, 0))
        S3V4.RemoveFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, 0);

      S3V4.PutFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, S3_CONTENTSTREAM1, 0);

      using (MemoryStream ms = new MemoryStream())
      {
        S3V4.GetFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, ms, 0);

        byte[] s3FileContentBytes = ms.GetBuffer();

        string s3FileContentStr = Encoding.UTF8.GetString(s3FileContentBytes, 0, (int)ms.Length);

        Assert.AreEqual(S3_CONTENTSTR1, s3FileContentStr);
      }

      S3V4.RemoveFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, 0); 
    }
  }
}
