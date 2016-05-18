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

using NFX;
using NFX.WinForms.Controls;
using NFX.WinForms.Controls.GridKit;

namespace WinFormsTest
{
  public partial class Form2 : Form
  {
    public Form2()
    {
      InitializeComponent();
    }

    private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
    {
      
    }

    private void vScrollBar1_ValueChanged(object sender, EventArgs e)
    {

    }
    
    class Person
    {
      public string FirstName;
      public string LastName;
      public DateTime DOB; 
      public decimal Balance;
      public decimal Investment;
      public decimal Capital;
      public decimal NetGain;
           
    }
    

    private List<Person> m_List = new List<Person>();

    private void Form2_Load(object sender, EventArgs e)
    {
      var now = App.LocalizedTime;
      for(var i=0; i<1000; i++)
       m_List.Add(new Person(){ FirstName="Alex_"+i, LastName= "Zukerman_"+i, DOB = new DateTime(1900+i%1000, 1+ i%11, 1+i%20), Balance = i*i, Investment= -i, Capital = i % 2789273, NetGain = -1000+i%1000});
// MessageBox.Show("Made records");   

      //grid.Style.HAlignment = HAlignment.Right; 
     // grid.Style.BGKind = BGKind.VerticalGradient; 
      //grid.Style.BGColor = Color.Silver; 

      
    
      new Column<Person, string>(grid, "First Name"){
                                        Width = 150,
                                        GetValue = (col,row)=>row.FirstName,
                                        GetHasValue = (col, row)=>row.FirstName!=null,
                                        GetComment=(col, row)=>"BuKasgka",
                                        SortingAllowed = true }.MinWidth = 32; 
                                        
      new Column<Person, string>(grid, "Last Name"){
                                        Width = 250,
                                        GetValue = (col,row)=>row.LastName,
                                        GetHasValue = (col, row)=>row.LastName!=null,
                                        SortingAllowed = true }.MinWidth = 100; 
                                        
      new Column<Person, DateTime>(grid, "DOB"){ Title = "Date Of Birth",
                                        Width = 95,
                                        GetValue = (col,row)=>row.DOB,
                                        GetHasValue = (col, row)=>row.DOB.ToBinary()!=0,
                                        SortingAllowed = true };
                                        
      var c1 = new Column<Person, DateTime>(grid, "DOB2"){ Title = "Date Of Birth",
                                        Width = 95,
                                        GetValue = (col,row)=>row.DOB,
                                        GetHasValue = (col, row)=>row.DOB.ToBinary()!=0 };     
      
      c1.Style.HAlignment = HAlignment.Right;
      c1.Style.BGKind = BGKind.Hatched;// BGKind.Gradient;
      c1.Style.BGHatchStyle = System.Drawing.Drawing2D.HatchStyle.LightUpwardDiagonal;
      c1.Style.BGColor = Color.FromArgb(0xff, 0xff, 0x60);                            
      c1.Style.BGHatchColor = Color.Silver;       
                                                                     
                                        
       
      new Column<Person, string>(grid, "Last Name2"){ Title = "Last Name Again",
                                        Width = 150,
                                        GetValue = (col,row)=>row.LastName,
                                        GetHasValue = (col, row)=>row.LastName!=null };
                                        
      new Column<Person, decimal>(grid, "Balance"){ 
                                        Width = 80,
                                        GetValue = (col,row)=>row.Balance,
                                        GetHasValue = (col, row)=>row.Balance!=0,
                                        GetComment = (col, row)=>"Kakashka! potomu chto on ne znaet kto est lenina na utro el griby a lenin grib no ya znaju eto takje kak i ty" +( row!=null?(row.LastName+row.FirstName):" ETO XEDER!"),
                                        FormatString = "{0:C2}" };
      new Column<Person, decimal>(grid, "Investment"){ 
                                        Width = 80,
                                        GetValue = (col,row)=>row.Investment,
                                        GetHasValue = (col, row)=>row.Investment!=0,
                                        FormatString = "{0:C2}" };
      new Column<Person, decimal>(grid, "Capital"){ 
                                        Width = 80,
                                        GetValue = (col,row)=>row.Capital,
                                        GetHasValue = (col, row)=>true,
                                        FormatString = "{0:C2}" }.Style.HAlignment = HAlignment.Right;                                                                                                      
      new Column<Person, decimal>(grid, "NetGain"){ 
                                        Width = 80,
                                        GetValue = (col,row)=>row.NetGain,
                                        GetHasValue = (col, row)=>row.NetGain!=0,
                                        FormatString = "{0:C2}" };                                  
                                        
                                        
      var c2 =  new Column<Person, string>(grid, "Last Name3"){ Title = "Last Name even more",
                                        Width = 300,
                                        GetValue = (col,row)=>row.LastName,
                                        GetHasValue = (col, row)=> row.LastName!=null };                                   
       
      c2.Style.HAlignment = HAlignment.Right;
      c2.Style.BGKind = BGKind.HorizontalGradient;
      c2.Style.BGColor = Color.Violet; 
       
      grid.DataRowSource = m_List; 
      
         
      grid.MultiSelect = true;
      grid.MultiSelectWithCtrl = false;
     // grid.CellSelectionAllowed = true;
     // grid.SelectedStyle.Assign(grid.Style);
       
       grid.ColumnRepositionAllowed = true;
       grid.ColumnResizeAllowed = true;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      var r  = new Random();
      for(int i=0; i<100; i++)
      {
         m_List[i].LastName = "Student_"+r.Next(100000);
         m_List[i].Capital = i * 37828.12M * r.Next(1, 1000);
         grid.SendDataSourceChangedNotification(m_List[i]);
      }
     // grid.Columns[1].Style.HAlignment = HAlignment.Center;
     // grid.Columns[2].Width = 250;
    }

    private void grid_CellSelection(CellElement oldCell, CellElement newCell)
    {
      if (newCell==null) return;
      if (newCell.Row==null) return;
      
      Text = newCell.Column.RepresentValueAsString(newCell.Row, newCell.Column.GetValueFromRow(newCell.Row));
    }

    private void button2_Click(object sender, EventArgs e)
    {
      var r  = new Random();
      for(int i=0; i<1000 && m_List.Count>10; i++)
      {
         if (r.Next(100)>60) 
          m_List.RemoveAt(r.Next(m_List.Count-1));
         else
          m_List.Insert(r.Next(m_List.Count-1), m_List[r.Next(m_List.Count-1)]); 
      }
      grid.SendDataSourceChangedNotification(null);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      button2_Click(button2, null);
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      timer1.Enabled = checkBox1.Checked;
      Text = "Timer is " + timer1.Enabled;
    }

    private void button3_Click(object sender, EventArgs e)
    {
      grid.Columns[0].Width--;
      grid.Columns[1].Visible = !grid.Columns[1].Visible;
    }

    private void button4_Click(object sender, EventArgs e)
    {
      grid.Configure(App.ConfigRoot);
    }

    private void button5_Click(object sender, EventArgs e)
    {
      grid.PersistConfiguration(App.ConfigRoot);
      App.ConfigRoot.Configuration.Save();
      //MessageBox.Show(NFX.WinForms.BaseApplication.ConfRoot.Configuration.ToString());
    }

    private void grid_ColumnSortChanged(Column sender)
    {
      var str = "";
      foreach(var col in grid.Columns)
       if (col.SortingAllowed)
        str+= col.Title+" "+col.SortDirection.ToString()+",";
      Text = str;  
    }
  }
}
