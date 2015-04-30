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
using BusinessLogic;
using NFX;

namespace WinFormsTest
{
  public partial class GridForm : Form
  {
    public GridForm()
    {
      InitializeComponent();
    }

    List<PatientRecord> m_List = new List<PatientRecord>();

    private void GridForm_Load(object sender, EventArgs e)
    {
     // populateRecordModel();
     populateDataTable();
    }

    private void populateRecordModel()
    {
      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      
      for(var i=0; i<1000; i++)
      {
        var rec = Record.Make<PatientRecord>();
        
       
        rec.Create();
        rec.fldID.Value = i.ToString();
        rec.fldFirstName.Value = "Alex_" + i.ToString();
        rec.fldLastName.Value = "Zukerman_" + i.ToString();
        rec.fldMonthlyCost.Value = i * 1000;
        rec.fldDOB.Value = App.LocalizedTime.AddYears(-i % 1000);

         // rec.Post();

        m_List.Add(rec); //add records in Creating mode
      } 
      
      sw.Stop();
      Text = sw.ElapsedMilliseconds.ToString();
      
      grid.DataSource = m_List;          
    }
    
    
    private void populateDataTable()
    {
      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      
      var table = new DataTable();
      table.Columns.Add(new DataColumn("ID", typeof(Int64)){ AllowDBNull = false});
      table.Columns.Add(new DataColumn("First_Name", typeof(string)){ AllowDBNull = false, MaxLength = 100});
      table.Columns.Add(new DataColumn("Last_Name", typeof(string)){ AllowDBNull = false, MaxLength = 100});
      table.Columns.Add(new DataColumn("Monthly_Cost", typeof(decimal)){ AllowDBNull = true});
      table.Columns.Add(new DataColumn("DOB", typeof(DateTime)){ AllowDBNull = true});
      
      
      for(var i=0; i<1000; i++)
      {
        var row = table.NewRow();
       
        row[0] = i.ToString();
        row[1] = "Alex_" + i.ToString();
        row[2] = "Zukerman_" + i.ToString();
        row[3] = i * 1000;
        row[4] = App.LocalizedTime.AddYears(-i % 1000); 
             
        table.Rows.Add(row);
      } 
      
      sw.Stop();
      Text = sw.ElapsedMilliseconds.ToString();
      
      grid.DataSource = table;
    }
    
    

    private void grid_SelectionChanged(object sender, EventArgs e)
    {
      if (grid.SelectedRows!=null)
       if (grid.SelectedRows.Count>0)
         props.SelectedObject = grid.SelectedRows[0].DataBoundItem;
    }
  }
}
