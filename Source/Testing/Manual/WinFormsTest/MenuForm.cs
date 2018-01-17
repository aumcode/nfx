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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using NFX;

namespace WinFormsTest
{
  public partial class MenuForm : Form
  {
    public MenuForm()
    {
      InitializeComponent();
    }

    private void btnGlue_Click(object sender, EventArgs e)
    {
      new GlueForm().Show();
    }

    private void btnSlim_Click(object sender, EventArgs e)
    {
      new SerializerForm2().Show();
    }

    private void btnImages_Click(object sender, EventArgs e)
    {
      new ImagesForm().Show();
    }

    private void btnWave_Click(object sender, EventArgs e)
    {
      new WaveForm().Show();
    }

    private void btnCache_Click(object sender, EventArgs e)
    {
      new CacheTest().Show();
    }

    private void btnGlueServer_Click(object sender, EventArgs e)
    {
      startProcess("TestServer.exe");
    }

    private void btnWaveServer_Click(object sender, EventArgs e)
    {
      startProcess("WaveTestSite.exe");
    }

    private void startProcess(string processName)
    {
      try
      {
        var si = new ProcessStartInfo
        {
          FileName = processName,
          UseShellExecute = false
        };

        var process = new Process { StartInfo = si };

        process.Start();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnPile_Click(object sender, EventArgs e)
    {
      new PileForm().Show();
    }


    private void btnPDF_Click(object sender, EventArgs e)
    {
     new PdfTestForm().Show();
    }

    private byte[] getGarbageBuffer(int bufferLength)
    {
      var buf = new byte[bufferLength];
      for(var i=0; i<buf.Length; i++)
      {
           buf[i] = (byte)(i & (byte)0xff);
      }
      return buf;
    }

     private volatile byte[] optimizerconfuser;

    private void btnSystemSpeed_Click(object sender, EventArgs e)
    {
      MessageBox.Show("Will test the system speed now. Do not move mouse and switch screens. Test takes around 30 sec");

      const int CNT = 256;
      const int BLEN = 32*1024*1024;

      optimizerconfuser = null;

      var sw = Stopwatch.StartNew();

      for(var i=0; i<CNT; i++)
      {
         var buf = getGarbageBuffer(BLEN);

         for(var k=0; k<buf.Length-3; k++)
         {
           if (k==129 || k==773) continue;
           buf[k] = (byte)((buf[k+1] + buf[k+2] + buf[k+3]) / 3);
         }

         if (optimizerconfuser==null) optimizerconfuser = buf; //confuse the optimizer
      }

      optimizerconfuser = null;

      var time1 = sw.ElapsedMilliseconds;

      sw.Restart();
      Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = System.Environment.ProcessorCount}, (i) =>
      {
         var buf = getGarbageBuffer(BLEN);
         for(var k=0; k<buf.Length-3; k++)
         {
           if (k==129 || k==773) continue;
           buf[k] = (byte)((buf[k+1] + buf[k+2] + buf[k+3]) / 3);
         }
         if (optimizerconfuser==null) optimizerconfuser = buf;//confuse the optimizer
      });

      var time2 = sw.ElapsedMilliseconds;

      long ops = CNT * (long)BLEN;

      MessageBox.Show(
@"
System
------
 (01) Machine: {0}
 (02) OSVersion: {1}
 (03) CPU Count: {2}
 (04) 64 Bit process: {3}
 (05) 64 Bit OS: {4}
 (06) OS Family: {5}
 (07) Network Signature: {6}

Performance
-----------

 (01) Single thread: {7:n0} in {8:n0} ms @ {9:n0} ops/sec
 (02) Multi thread:  {10:n0} in {11:n0} ms @ {12:n0} ops/sec
                                 -----------------------------
                                  {13:n2} faster
"
           .Args(
             System.Environment.MachineName,
             System.Environment.OSVersion,
             System.Environment.ProcessorCount,
             System.Environment.Is64BitProcess,
             System.Environment.Is64BitOperatingSystem,
             NFX.OS.Computer.OSFamily,
             NFX.OS.Computer.UniqueNetworkSignature,

             ops, time1, ops / (time1/ 1000d),
             ops, time2, ops / (time2/ 1000d),
             time1 / (double)time2
           ));
    }

    private void btnBSON_Click(object sender, EventArgs e)
    {
      new BSONTestForm().Show();
    }

    private void btnMONGOCONNECTOR_Click(object sender, EventArgs e)
    {
      new MongoConnectorForm().Show();
    }

    private void btnChart_Click(object sender, EventArgs e)
    {
      new ChartForm().Show();
    }

    private void btnChartDemo_Click(object sender, EventArgs e)
    {
      new ChartFormDemo().Show();
    }

        private void btnMailSink_Click(object sender, EventArgs e)
        {
            new MailForm().Show();
        }

        private void btnMMFPile_Click(object sender, EventArgs e)
        {
            new PileForm(true).Show();
        }

        private void btnGlueStress_Click(object sender, EventArgs e)
        {
            new GlueStressForm().Show();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
          new LogForm().Show();
        }
    }
}
