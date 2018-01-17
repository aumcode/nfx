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

using NFX.Security.CAPTCHA;

namespace WinFormsTest
{
  public partial class BlankForm : Form
  {
    public BlankForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      GC.Collect();
    }

    private PuzzleKeypad m_Keypad;

    private void button4_Click(object sender, EventArgs e)
    {
      m_Keypad = new PuzzleKeypad(tbCode.Text, puzzleBoxWidth: 12);
      pic.Image = m_Keypad.DefaultRender();

    }

    private List<Point> m_Clicked = new List<Point>();

    private void pic_MouseClick(object sender, MouseEventArgs e)
    {
      m_Clicked.Add(new Point(e.X, e.Y));
    }

    private void button1_Click_1(object sender, EventArgs e)
    {
      if (m_Keypad==null) return;

      Text = "Deciphered: "+ m_Keypad.DecipherCoordinates(m_Clicked);
      m_Clicked.Clear();
    }
  }
}
