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
using System.Windows.Forms;
using BusinessLogic;
using NFX;
using NFX.WinForms;

namespace WinFormsTest
{
    
    public partial class ObjectStoreForm : Form
    {
        public ObjectStoreForm()
        {
            InitializeComponent();
        }


        List<Thread> m_List;
        List<Guid> m_Guids;
        Random m_Rnd = new Random();

        bool STARTED;

        long COUNT;

        private void btnStart_Click(object sender, EventArgs e)
        {
           var cnt = int.Parse(this.edtCount.Text);

           m_Guids = new List<Guid>();
           for(int i=0; i<1000 * 128; i++)
            m_Guids.Add(Guid.NewGuid());
           
           STARTED = true;

           m_List = new List<Thread>();
           for(var i=0; i<cnt; i++)
            m_List.Add(new Thread(spin));//(i%3==0)?  new Thread( spin)   :  new Thread(spin2) );

           foreach(var t in m_List)
            t.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
          STARTED = false;
        }


        private void spin()
        {
          while(STARTED)
          {
             var guid = m_Guids[m_Rnd.Next(m_Guids.Count-1)];
             var rec = BaseApplication.Objects.CheckOut(guid) as PatientRecord;

             if (rec==null)
             {
                rec = PatientRecord.Make<PatientRecord>();
                   rec.Create();
                         rec.fldID.Value = m_Rnd.Next(10000).ToString() + "-a";
                         rec.fldFirstName.Value = "Vsevolod_" +  m_Rnd.Next(10000).ToString();
                         rec.fldLastName.Value = "Serdukovich_" +  m_Rnd.Next(10000).ToString();
                         rec.fldDOB.Value = App.LocalizedTime.AddSeconds(-m_Rnd.Next(1000));
                         rec.fldContactPhone.Value = "(555) 123-121" +  m_Rnd.Next(9).ToString();
                         rec.fldClassification.Value = "BUD";
                   rec.Post();
               BaseApplication.Objects.CheckIn(guid, rec);
               continue;
             }

             lock(rec)
             {
                rec.Edit();
                 rec.fldLastName.Value = "Serdukovich_" +  m_Rnd.Next(10000).ToString();
                rec.Post();
             }
             BaseApplication.Objects.CheckIn(guid, rec);
             
             Interlocked.Increment(ref COUNT);

          //   Thread.Sleep(m_Rnd.Next(5));
          }
        }

        private void spin2()
        {
           Random r = new Random();

List<Guid> m_Guids = new List<Guid>();
m_Guids = new List<Guid>();
for(int i=0; i<1000; i++)
m_Guids.Add(Guid.NewGuid());


          while(STARTED)
          {
             var guid = m_Guids[r.Next(m_Guids.Count-1)];
             var lst = BaseApplication.Objects.CheckOut(guid) as List<int>;

             if (lst==null)
             {
               var n = 0xff;
               lst = new List<int>(n);
               for(int i=0; i<n;i++) lst.Add(i);
             }

             lock(lst)
             {
                lst[r.Next(lst.Count-1)] = r.Next(100000);
             }

             BaseApplication.Objects.CheckIn(guid, lst);
             
             Interlocked.Increment(ref COUNT);
             
          //   Thread.Sleep(m_Rnd.Next(5));
          }
        }

        long pcount;
        DateTime ptime = App.LocalizedTime;

        private void tmStatus_Tick(object sender, EventArgs e)
        {
            var now = App.LocalizedTime;
            var cnt = COUNT;
            var rps = (cnt - pcount) / (now - ptime).TotalSeconds;
            pcount = cnt;
            ptime = now;
            Text = string.Format("{0} rps \n", rps);
            tbStatus.Text += Text;
        }
               


    }
}
