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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NFX;
using System.Diagnostics;

namespace WinFormsTest
{
  public partial class FIDForm : Form
  {
    public FIDForm()
    {
      InitializeComponent();
    }

    private void btnGUID_Click(object sender, EventArgs e)
    {
      var cnt = tbCount.Text.AsInt();

      var w = Stopwatch.StartNew();
      if (chkParallel.Checked)
       Parallel.For(0,cnt,(i)=>
       {
          Guid.NewGuid();
       });
      else
       for(var i=0; i<cnt;i++)
        Guid.NewGuid();


      w.Stop();


      Text = "Guid {0:n2} gened in {1:n2} msec at {2:n2} ops/sec".Args(cnt, w.ElapsedMilliseconds, cnt / (w.ElapsedMilliseconds / 1000d));

    }

    private void btnFID_Click(object sender, EventArgs e)
    {
      var cnt = tbCount.Text.AsInt();

      var w = Stopwatch.StartNew();
      if (chkParallel.Checked)
       Parallel.For(0,cnt,(i)=>
       {
          FID.Generate();
       });
      else
       for(var i=0; i<cnt;i++)
          FID.Generate();

      w.Stop();


      Text = "FID {0:n2} gened in {1:n2} msec at {2:n2} ops/sec".Args(cnt, w.ElapsedMilliseconds, cnt / (w.ElapsedMilliseconds / 1000d));
    }



    private void button1_Click(object sender, EventArgs e)
    {
      var cnt = tbCount.Text.AsInt();
      
      var bag = new ConcurrentBag<FID>();
      
      if (chkParallel.Checked)
       Parallel.For(0,cnt,(i)=>
       {
          bag.Add( FID.Generate() );
       });
      else
       for(var i=0; i<cnt;i++)
          bag.Add( FID.Generate() );
      
      
      var sb = new StringBuilder();
      var c=0;
      foreach(var id in bag)
      {
        sb.AppendLine( "{0}:    {1}  ->  {2}".Args(c, id.ID, id) );
        c++;
        if (c>10000)
        {
          sb.AppendLine("......more......");
          break;
        }
      }

      //Uncomment to cause duplicates
      //var v =bag.FirstOrDefault();
      //bag.Add(v);
      //bag.Add(v);//duplicate


      if (bag.Count==bag.Distinct().Count()) 
        sb.Insert(0, "No Duplicates in the set of {0:n2}\r\n".Args(bag.Count));
      else  
        sb.Insert(0, "DUPLICATES!!!!!!!!!!!!! in the set of {0:n2}\r\n\r\n\r\n".Args(bag.Count));

      tbDump.Text = sb.ToString();

    }
  }
}
