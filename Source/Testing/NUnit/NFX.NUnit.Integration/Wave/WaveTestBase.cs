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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NFX.Security;
using NUnit.Framework;

namespace NFX.NUnit.Integration.Wave
{
  public class WaveTestBase
  {
    #region Consts

      private const string DEFAULT_PROCESS_FILENAME = "WaveTestSite.exe";
      private const string WAVE_HTTP_ADDR = "http://localhost:8080";
      public static Uri S_WAVE_URI = new Uri(WAVE_HTTP_ADDR);

      protected const string INTEGRATIONN_TESTER = "IntegrationTester";

      protected const string INTEGRATION_HTTP_ADDR = WAVE_HTTP_ADDR + "/mvc/" + INTEGRATIONN_TESTER + "/";
      protected const string PAGES_HTTP_ADDR = WAVE_HTTP_ADDR + "/pages/";

      private const string WAVE_COOKIE_NAME = "ZEKRET";
      private const string WAVE_COOKIE_VALUE = "Hello";
      public static Cookie S_WAVE_COOKIE = new Cookie(WAVE_COOKIE_NAME, WAVE_COOKIE_VALUE);

    #endregion

    #region Pvt Fields

      private Process m_ServerProcess = new Process();

    #endregion

    #region Init/TearDown

    [TestFixtureSetUp]
    public void SetUp()
    {
      try
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
      catch (Exception)
      {
        Console.WriteLine("The test must be executed with admin priviledges!");
        throw;
      }
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      m_ServerProcess.StandardInput.WriteLine(string.Empty);
    }

    #endregion

    #region Protected

    protected virtual string ProcessFileName { get { return DEFAULT_PROCESS_FILENAME; } }

    protected WebClientCookied CreateWebClient()
    {
      var wc = new WebClientCookied();
      wc.CookieContainer.Add(S_WAVE_URI, S_WAVE_COOKIE);
      return wc;
    }

    #endregion
  }
}
