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


using NFX.RecordModel;
using NFX.WinForms;

using BusinessLogic;

namespace WinFormsTest
{
  public partial class PatientForm : Form
  {
    public PatientForm()
    {
      InitializeComponent();
    }
    
    Guid key = new Guid("AB6FEF13-C575-402e-8E8C-D62C0CB8A50F");

    PatientRecord record;

    private void PatientForm_Load(object sender, EventArgs e)
    {
    //  record = BaseApplication.Objects.CheckOut(key) as PatientRecord;

   //for now   if (record==null)
        record = Record.Make<BusinessLogic.PatientRecord>();
      
      pnlRecord.AttachModel(record);


      record.StateChanged += (s,a) => setButtons();
      
      setButtons();
    }
    
    
    private void PatientForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      BaseApplication.Objects.CheckIn(key, record);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      record.Cancel();         
    }

    private void btnPost_Click(object sender, EventArgs e)
    {
      try
      {
        record.Post();
      }
      catch(Exception error)
      {
        MessageBox.Show(error.Message);
      }  
    }

    private void btnRevert_Click(object sender, EventArgs e)
    {
      record.Revert();    
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
      record.Edit();
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
      record.Create();
    }
    
    private void setButtons()
    {
      btnCreate.Enabled = record.State == DataState.Viewing || record.State == DataState.Uninitialized;
      btnEdit.Enabled = record.State == DataState.Viewing;

      btnRevert.Enabled = record.fldID.IsMutable;
      btnPost.Enabled = record.fldID.IsMutable;
      btnCancel.Enabled = record.fldID.IsMutable;
    }

    
  }
}
