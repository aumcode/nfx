/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using NFX.Environment;
using NUnit.Framework;
using CONN_PARAMS = NFX.IO.FileSystem.S3.V4.S3V4FileSystemSessionConnectParams;

namespace NFX.NUnit.IO.FileSystem.S3.V4.S3V4Sign
{
  public class Base
  {
    public static string ACCESSKEY;
    public static string SECRETKEY;

    public const string HOST = "http://s3.amazonaws.com";
    public static string REGION;
    public static string BUCKET;
    public const string ITEM_RELATIVE_PATH = "Folder01/Test01.txt";

    public const string CONTENT = "Hello, World!";

    protected const string NFX_S3 = "NFX_S3";

    protected static void initCONSTS()
    {
      try
      {
        string envVarsStr = System.Environment.GetEnvironmentVariable(NFX_S3);

        var cfg = Configuration.ProviderLoadFromString(envVarsStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

        BUCKET = cfg.AttrByName(CONN_PARAMS.CONFIG_BUCKET_ATTR).Value;
        REGION = cfg.AttrByName(CONN_PARAMS.CONFIG_REGION_ATTR).Value;
        ACCESSKEY = cfg.AttrByName(CONN_PARAMS.CONFIG_ACCESSKEY_ATTR).Value;
        SECRETKEY = cfg.AttrByName(CONN_PARAMS.CONFIG_SECRETKEY_ATTR).Value;
      }
      catch (Exception ex)
      {
        throw new Exception( string.Format(
            "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added", 
              NFX_S3, 
              "s3{ bucket='bucket01' region='us-west-2' access-key='XXXXXXXXXXXXXXXXXXXX' secret-key='XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX'}"), 
          ex);
      }
    }
  }
}
