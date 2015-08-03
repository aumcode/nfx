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
using NFX.WinForms.Controls;

using NFX.RecordModel;

namespace WinFormsTest
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }


    Balloon balloon;

    private void Form1_Load(object sender, EventArgs e)
    {
      balloon = new Balloon(
@"Hello from Ballon 
  This is very cool, line can be looooooooooooooooong 
  Ciao!",
                      new Font("Arial", 10),
                      textBox1,
                      120,
                      NFX.Geometry.MapDirection.North,
                      Color.Yellow);
      
      
      var text = BaseApplication.Objects.CheckOut(new Guid("AB6FEF13-C575-402e-8E8C-D62C0CB8A50F")) as string;
      if (text!=null)
       memo.Text = text;
      
    }
    
    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
       BaseApplication.Objects.CheckIn(new Guid("AB6FEF13-C575-402e-8E8C-D62C0CB8A50F"), memo.Text);
      
    }
    

    private void button2_Click(object sender, EventArgs e)
    {
      balloon.Beating = !balloon.Beating;
    }

    private void button3_Click(object sender, EventArgs e)
    {
      balloon.FadeIn();
      
    }

    private void button4_Click(object sender, EventArgs e)
    {
      balloon.FadeOut();
    }

    private void button5_Click(object sender, EventArgs e)
    {
      balloon.Text = "Marazmatik!";
      
    }

    private void button6_Click(object sender, EventArgs e)
    {
      for(int i=0; i<1000; i++)
             BaseApplication.WriteToLog("Message number: "+ i.ToString());
             
             
      BaseApplication.Instance.Log.Write(new NFX.Log.Message(){}); 
             
    }

    private void button7_Click(object sender, EventArgs e)
    {
      var rec =  Record.Make<BusinessLogic.PatientRecord>();
      recordContextPanel1.AttachModel( rec );

      rec.Create();
    }

    private void button1_Click(object sender, EventArgs e)
    {

    }

    
  }
}
