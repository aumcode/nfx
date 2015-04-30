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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using NFX;
using NFX.Glue.Protocol;
using NFX.Security;
using System.IO;

namespace WinFormsTest.Glue
{
  public partial class JokeCalculatorClientForm : Form
  {
    #region Consts

      public const string DEFAULT_TEST_SERVER_HOST = "192.168.1.102";//"127.0.0.1";

      public const int DEFAULT_TEST_SERVER_SYNC_PORT = 8000;
      public const int DEFAULT_TEST_SERVER_ASYNC_PORT = 8002;

      public readonly string DEFAULT_TEST_SERVER_SYNC_NODE = "sync://" + DEFAULT_TEST_SERVER_HOST + ":" + DEFAULT_TEST_SERVER_SYNC_PORT;
      public readonly string DEFAULT_TEST_SERVER_ASYNC_NODE = "async://" + DEFAULT_TEST_SERVER_HOST + ":" + DEFAULT_TEST_SERVER_ASYNC_PORT;

      public readonly Credentials DEFAULT_TEST_CREDENTIALS = new IDPasswordCredentials("dima", "dima");

    #endregion

    #region ctor

      public JokeCalculatorClientForm()
      {
        InitializeComponent();
      } 

    #endregion

    private void m_btnRun_Click(object sender, EventArgs e)
    {
      try
      {
        //using (JokeHelper.MakeApp())
        {
          var cl = new JokeContractClient(DEFAULT_TEST_SERVER_SYNC_NODE);
          cl.Headers.Add(new AuthenticationHeader(DEFAULT_TEST_CREDENTIALS));

          var result = cl.Echo("Gello A!");

          m_txtLog.AppendText(result);
          m_txtLog.AppendText(Environment.NewLine);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void m_btnCallAsync_Click(object sender, EventArgs e)
    {
      try
      {
        using (var cl = new JokeContractClient(DEFAULT_TEST_SERVER_ASYNC_NODE))
        {
          cl.Headers.Add(new AuthenticationHeader(DEFAULT_TEST_CREDENTIALS));

          var result = cl.Echo("Gello A!");


          m_txtLog.AppendText(result);
          m_txtLog.AppendText(Environment.NewLine);
        }

        var glue = App.Glue as NFX.Glue.Implementation.GlueService;
        var binding = glue.Bindings["async"];
        var active = binding.ClientTransports.ToList();
        foreach (var ct in active)
          ct.Dispose();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private CancellationTokenSource m_CancellationTokenSource;

    private void m_btnStartASync_Click(object sender, EventArgs e)
    {
      CallLongRunningMethod();
      m_lblASyncTotal.Text = "Started";
    }

    private async void CallLongRunningMethod()
    {
      m_CancellationTokenSource = new CancellationTokenSource();
      try
      {
        string res = await LongRunningMethodAsync("Done", m_CancellationTokenSource.Token);
        m_lblASyncTotal.Text = res;
      }
      catch (OperationCanceledException)
      {
        throw;
      }
    }

    private Task<string> LongRunningMethodAsync(string message, CancellationToken cancellationToken)
    {
      return Task.Run<string>(() => LongRunningMethod(message));
    }

    private string LongRunningMethod(string message)
    {
      int errQty = 0;
      for (int i = 0; i < 10000000; i++)
      {
        try  
        {
          using (var cl = new JokeContractClient(DEFAULT_TEST_SERVER_ASYNC_NODE))
          {
            cl.Headers.Add(new AuthenticationHeader(DEFAULT_TEST_CREDENTIALS));

            var result = cl.Echo("Gello A!");

            //LogLine(result);
            if (i % 10 == 0) LogTotal(i, errQty);
          }

          // close all transports
          var glue = App.Glue as NFX.Glue.Implementation.GlueService;
          var binding = glue.Bindings["async"];
          var active = binding.ClientTransports.ToList();
          foreach (var ct in active)
            ct.Dispose();

        }
        catch (Exception ex)
        {
          errQty++;
          LogLine(ex.Message);
          //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        
      }
      
      return message;
    }

    private void m_btnStopASync_Click(object sender, EventArgs e)
    {

    }

    private void LogTotal(int opQty, int errQty)
    {
      if (m_lblASyncTotal.InvokeRequired)
      {
        var cb = new Action<int, int>(LogTotal);
        this.Invoke(cb, new object[] {opQty, errQty});
      }
      else
      {
        m_lblASyncTotal.Text = "{0:#,#} ops, {1:#,#} err".Args(opQty, errQty);
      }
    }

    private void LogLine(string line)
    {
      if (this.m_txtLog.InvokeRequired)
      {
        var cb = new Action<string>(LogLine);
        this.Invoke(cb, new object[] { line});
      }
      else
      {
        m_txtLog.AppendText(line);
        m_txtLog.AppendText(Environment.NewLine);
      }
    }

    private void m_btnStream_Click(object sender, EventArgs e)
    {
      var buf = new byte[317];
      using (var ms = new MemoryStream(buf))
      {
        ms.WriteByte(14);
        ms.SetLength(40);
        ms.SetLength(310);
      }

      MessageBox.Show(buf[0].ToString(), "");
    }
  }
}
