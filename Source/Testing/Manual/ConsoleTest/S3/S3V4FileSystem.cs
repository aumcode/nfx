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
using NFX.Environment;
using NFX.IO.FileSystem;
using NFX.IO.FileSystem.S3.V4;
using NFX.Security;
using CONN_PARAMS = NFX.IO.FileSystem.S3.V4.S3V4FileSystemSessionConnectParams;

namespace ConsoleTest.S3
{
  public class S3V4FilesystemTests
  {
    private static readonly string BUCKET;
    private static readonly string REGION;

    private static readonly string ACCESSKEY;
    private static readonly string SECRETKEY;

    protected const string DXW_ROOT = "MyFolder02";

    protected const string NFX_S3 = "NFX_S3";

    private static S3V4FileSystemSessionConnectParams CONN_PARAMS;

    static S3V4FilesystemTests()
    {
      try
      {
        string envVarsStr = System.Environment.GetEnvironmentVariable(NFX_S3);

        var cfg = Configuration.ProviderLoadFromString(envVarsStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

        BUCKET = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_BUCKET_ATTR).Value;
        REGION = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_REGION_ATTR).Value;
        ACCESSKEY = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_ACCESSKEY_ATTR).Value;
        SECRETKEY = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_SECRETKEY_ATTR).Value;
      }
      catch (Exception ex)
      {
        throw new Exception( string.Format(
            "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added", 
              NFX_S3, 
              "s3{ bucket='bucket01' region='us-west-2' access-key='XXXXXXXXXXXXXXXXXXXX' secret-key='XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX'}"), 
          ex);
      }

      CONN_PARAMS = FileSystemSessionConnectParams.Make<S3V4FileSystemSessionConnectParams>(
        "s3v3{{ name='s3v4' bucket='{0}' region='{1}' access-key='{2}' secret-key='{3}' }}"
          .Args(BUCKET, REGION, ACCESSKEY, SECRETKEY));
    }

    public static void ListBucket()
    {
      using(var fs = new S3V4FileSystem("S3-1"))
      {
        var session = fs.StartSession(CONN_PARAMS);

        FileSystemDirectory dir = session[DXW_ROOT] as FileSystemDirectory;

        Console.WriteLine("************* FOLDERS **********************");

        foreach(string subdir in dir.SubDirectoryNames)
        {
          Console.WriteLine(subdir);
        }

        Console.WriteLine("************* FILES **********************");

        foreach(string file in dir.FileNames)
        {
          Console.WriteLine(file);
        }
      } 
    }

    public static void Connect_NavigateRootDir()
    {
      using(var fs = new S3V4FileSystem("S3-1"))
      {
        var session = fs.StartSession(CONN_PARAMS);

        var dir = session[DXW_ROOT] as FileSystemDirectory;

        //Assert.AreEqual(@"c:\", dir.ParentPath);
        //Assert.AreEqual(@"c:\NFX", dir.Path);
      } 
    }
  }
}
