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
using System.Windows.Forms;

using NFX.WinForms;
using NFX.DataAccess;
using NFX.RecordModel;
using NFX.RecordModel.DataAccess;

using BusinessLogic;
using NFX;

namespace WinFormsTest
{
    public partial class MongoDBForm : Form
    {
        public MongoDBForm()
        {
            InitializeComponent();
            pnl.AttachModel(record);
            record.StateChanged += (s,a) => setButtons();
            setButtons();
        }

        PatientRecord record = Record.Make<PatientRecord>();


        private void setButtons()
        {
          if (pnl.Record==null) return;

          btnLoad.Enabled = btnCreate.Enabled = record.State == DataState.Viewing || record.State == DataState.Uninitialized;
          btnSave.Enabled = btnEdit.Enabled = record.State == DataState.Viewing;

          btnRevert.Enabled = record.fldID.IsMutable;
          btnPost.Enabled = record.fldID.IsMutable;
          btnCancel.Enabled = record.fldID.IsMutable;
        }



        private void btnLoad_Click(object sender, EventArgs e)
        {
          if (string.IsNullOrEmpty(edtID.Text)) return;
          
          try
          {
            record.Load( new NameValueDataStoreKey(record.fldID.FieldName, edtID.Text) );
          }
          catch(Exception error)
          {
            record.CancelLoad();
            MessageBox.Show("Could not load data for supplied id. Driver said: \n\n" + error.Message);
            return;
          }

        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
           record.Create();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
           record.Edit();
        }

        
        private void btnRevert_Click(object sender, EventArgs e)
        {
          record.Revert();
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            try
            {
              record.Post();
            }
            catch
            {
              MessageBox.Show("Could not post. See red error markers in fields");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
           record.Cancel();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            record.Save();
        }

        private void btnBatch_Click(object sender, EventArgs e)
        {
           pnl.DetachModel(); //if we dont do it, the panel will flicker with every record change
           

            var clock = System.Diagnostics.Stopwatch.StartNew();
           
                for(var i=0; i<10000; i++)
                {
                   record.Create();
                         record.fldID.Value = i.ToString() + "-a";
                         record.fldFirstName.Value = "Vsevolod_" + i.ToString();
                         record.fldLastName.Value = "Serdukovich_" + i.ToString();
                         record.fldDOB.Value = App.LocalizedTime.AddSeconds(-i);
                         record.fldContactPhone.Value = "(555) 123-" + i.ToString();
                         record.fldClassification.Value = "BUD";
                   record.Post();
                   
                   record.Save();
                }

            clock.Stop();

            MessageBox.Show("Finished batch in "+clock.ElapsedMilliseconds.ToString() + " ms");

          pnl.AttachModel(record);  //re-attach model so panel shows fields connected to record
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            record.Delete();
        }
    }
}
