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
using System.Diagnostics;
using System.Linq;
using System.Text;
using NFX.Security;
using NUnit.Framework;

namespace NFX.NUnit.Integration.Glue
{
  public class JokeTestBase
  {
    public const string DEFAULT_PROCESS_FILENAME = "TestServer.exe";

    public const string DEFAULT_TEST_SERVER_HOST = "127.0.0.1";
    public const int DEFAULT_TEST_SERVER_SYNC_PORT = 8000;
    public const int DEFAULT_TEST_SERVER_MPX_PORT = 5701;

    public readonly string DEFAULT_TEST_SERVER_SYNC_NODE = "sync://" + DEFAULT_TEST_SERVER_HOST + ":" + DEFAULT_TEST_SERVER_SYNC_PORT;

    public readonly string DEFAULT_TEST_SERVER_MPX_NODE = "mpx://" + DEFAULT_TEST_SERVER_HOST + ":" + DEFAULT_TEST_SERVER_MPX_PORT;

    public readonly Credentials DEFAULT_TEST_CREDENTIALS = new IDPasswordCredentials("dima", "thejake");

    #region Pvt Fields

    private Process m_ServerProcess = new Process();

    #endregion

    #region Init/TearDown

    [TestFixtureSetUp]
    public void SetUp()
    {
      ProcessStartInfo start = new ProcessStartInfo()
      {
        FileName = ProcessFileName,
        RedirectStandardInput = true,
        UseShellExecute = false
      };

      m_ServerProcess = new Process() { StartInfo = start };

      m_ServerProcess.Start();

      System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      m_ServerProcess.StandardInput.WriteLine(string.Empty);
    }

    #endregion

    #region Protected

    protected virtual string ProcessFileName { get { return DEFAULT_PROCESS_FILENAME; } }

    protected virtual string TestServerHost { get { return DEFAULT_TEST_SERVER_HOST; } }
    protected virtual int TestServerSyncPort { get { return DEFAULT_TEST_SERVER_SYNC_PORT; } }
    protected virtual string TestServerSyncNode { get { return DEFAULT_TEST_SERVER_SYNC_NODE; } }
    protected virtual string TestServerMpxNode { get { return DEFAULT_TEST_SERVER_MPX_NODE; } }
    protected virtual Credentials TestCredentials { get { return DEFAULT_TEST_CREDENTIALS; } }

    #endregion
  }
}
