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
using System.IO;
using System.Linq;
using System.Text;
using NFX.Environment;
using NFX.IO.FileSystem.SVN;

using CONN_PARAMS = NFX.IO.FileSystem.SVN.SVNFileSystemSessionConnectParams;

namespace ConsoleTest.WebDAVTest
{
  class WebDAVClientTest
  {
    public const string NFX_SVN = "NFX_SVN";

    public static string ROOT;
    private static string UNAME;
    private static string UPSW;

    #region LACONF

      private const string LACONF = @"nfx 
{
  starters
  {
    starter
    {
      name='Payment Processing'
      type='NFX.Web.Pay.PaySystemStarter, NFX.Web'
      application-start-break-on-exception='true'
    }
  }  

  web-settings
  {
    log-type='Log.MessageType.TraceD'

    service-point-manager
    {
      default-connection-limit=4
      use-nagle-algorithm=true
    }

    payment-processing
    {
      pay-system
      {
        name='Stripe1'
        type='NFX.Web.Pay.StripeSystem, NFX.Web'
        auto-start='true'

        default-session-connection-params
        {
          type='NFX.Web.Pay.Stripe.StripeConnectionParameters, NFX.Web'
          secret-key='abcde-1234567890'
        }
      }
    }

    social
    {
      //provider {}
    }

    web-dav
    {
      log-type='Log.MessageType.TraceZ'
    }

    session
    {
      timeout-ms='30001'
      web-strategy
      {
        cookie-name='kalabashka'
      }
    }
  }

}
";

    #endregion

    public static void InitCONSTS()
    {
      try
      {
        string envVarsStr = System.Environment.GetEnvironmentVariable(NFX_SVN);

        var cfg = Configuration.ProviderLoadFromString(envVarsStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

        ROOT = cfg.AttrByName(CONN_PARAMS.CONFIG_SERVERURL_ATTR).Value;
        UNAME = cfg.AttrByName(CONN_PARAMS.CONFIG_UNAME_ATTR).Value;
        UPSW = cfg.AttrByName(CONN_PARAMS.CONFIG_UPWD_ATTR).Value;
      }
      catch (Exception ex)
      {
        throw new Exception( string.Format(
            "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added", 
              NFX_SVN, 
              "svn{ server-url='https://subversion.assembla.com/svn/XXX' user-name='XXXX' user-password='XXXXXXXXXXXX' }"), 
          ex);
      }
    }

    public static void List()
    {
      InitCONSTS();

      WebDAV client = new WebDAV(ROOT, 0, UNAME, UPSW);

      WebDAV.Directory root = client.Root;

      var children = root.Children;

      foreach (var item in children)
      {
        Console.WriteLine( item.Name);
      }
    }

    public static void FileContent()
    {
      InitCONSTS();

      WebDAV client = new WebDAV(ROOT, 0, UNAME, UPSW);

      WebDAV.Directory root = client.Root;

      WebDAV.File file = root.Children.First(c => c is WebDAV.File) as WebDAV.File;

      using(MemoryStream ms = new MemoryStream())
	    {
        file.GetContent(ms);
      }
    }

    public static void NavigatePath()
    {
      InitCONSTS();

      WebDAV client = new WebDAV(ROOT, 0, UNAME, UPSW);

      WebDAV.Directory root = client.Root;

      WebDAV.Directory nested = root.NavigatePath("/trunk/Source/NFX/") as WebDAV.Directory;
    }

    public static void GetVersions()
    {
      InitCONSTS();

      foreach (var v in WebDAV.GetVersions(ROOT, UNAME, UPSW))
      {
        Console.WriteLine(v);
      }
    }
    
  }
}
