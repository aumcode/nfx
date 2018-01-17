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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NFX;
using NFX.Log;
using NFX.WinForms;

namespace WinFormsTest
{
    public partial class LogForm : Form
    {
        const int CNT = 100;

        public LogForm()
        {
            InitializeComponent();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            write(MessageType.Info);
        }



        private void write(MessageType t)
        {
          for(var i=0; i<CNT; i++)
             App.Log.Write( new NFX.Log.Message{Type = t, From = tbFrom.Text, Text = tbText.Text + i.ToString()});
        }

        private void btnTrace_Click(object sender, EventArgs e)
        {
            write(MessageType.Trace);
        }

        private void btnWarning_Click(object sender, EventArgs e)
        {
              write(MessageType.Warning);
        }

        private void btnError_Click(object sender, EventArgs e)
        {
              write(MessageType.Error);
        }

        private void btnCatastrophy_Click(object sender, EventArgs e)
        {
              write(MessageType.CatastrophicError);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
              write(MessageType.Debug);
        }

        private void btnThrow_Click(object sender, EventArgs e)
        {
          try
          {
            srow1();
          }
          catch(Exception error)
          {
            App.Log.Write( new NFX.Log.Message{
                     Type =  MessageType.Error,
                     Topic = "Tezt Errors",
                     Source = 89,
                     From = tbFrom.Text,
                     Text = error.ToMessageWithType(),
                     Exception = error
                   });
          }
        }

        private void srow1()
        {
          try
          {
            srow2();
          }
          catch(Exception error)
          {
            throw new NFXException("This is a test caught: "+error.ToMessageWithType(), error);
          }
        }

        private void srow2()
        {
          throw new ArgumentException("Zis is a test argument exception", "teztArg");
        }
    }
}
