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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;


using NFX.Wave;
using NFX.Web.GeoLookup;


namespace WinFormsTest
{
  public partial class WaveServerForm : Form
  {
    public WaveServerForm()
    {
      InitializeComponent();
    }


    private WaveServer m_Server;

    private void button1_Click(object sender, EventArgs e)
    {
      if (m_Server!=null) return;
      

     // System.Threading.ThreadPool.SetMaxThreads(1024, 1024);
      
      m_Server = new WaveServer();
      m_Server.Configure(null);
      try
      {
       // m_Server.Prefixes.Add(tbPrefix.Text);
       // m_Server.KernelHttpQueueLimit = 1;//0000;
       // m_Server.ParallelAccepts = 2;
        m_Server.Start();
      }
      catch
      {
        m_Server.Dispose();
        m_Server = null;
        throw;
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
       if (m_Server==null) return;
       m_Server.Dispose();
       m_Server = null;
    }

    private void WaveServerForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      button2_Click(null, null);
    }

    private void button3_Click(object sender, EventArgs e)
    {
      var pat = new URIPattern(tbPattern.Text);

      var uri = new Uri("http://localhost:8080/cities/cleveland.htm?port=true&law=%2e%2d%56");

    //  MessageBox.Show(pat._____Chunks);

      var nu = pat.MakeURI(new Dictionary<string, object>
                  {
                     {"city", "Hamburg"},
                     {"state", "Ohio"}
                  }, new Uri("http://test.com"));

      MessageBox.Show(nu.ToString());

     // Text = pat.MatchURI( uri )["city"].ToString();
    }

    private GeoLookupService m_GeoService;

    private void button4_Click(object sender, EventArgs e)
    {
      if (m_GeoService==null)
      {
        var svc = new GeoLookupService();
        svc.DataPath = @"d:\geodata";
        svc.Resolution = LookupResolution.City;
        svc.Start();
        m_GeoService = svc;
      }

      var result = m_GeoService.Lookup( IPAddress.Parse( tbIP.Text));
      if (result!=null)
      {
        MessageBox.Show( result.ToString() );
      }

    }




  }
}
