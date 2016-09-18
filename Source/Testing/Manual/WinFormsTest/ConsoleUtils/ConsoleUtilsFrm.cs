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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using NFX;
using NFX.IO;

namespace WinFormsTest.ConsoleUtils
{
  public partial class ConsoleUtilsFrm : Form
  {
    public ConsoleUtilsFrm()
    {
      InitializeComponent();
    }

    private void m_btnGenerate_Click(object sender, EventArgs e)
    {
      WriteHTMLFromContentPath("Help.txt", m_cmbBase.SelectedItem.ToString());
    }

    private void button1_Click(object sender, EventArgs e)
    {
      WriteHTMLFromContentPath("Welcome.txt", m_cmbBase.SelectedItem.ToString());
    }

    private static void WriteHTMLFromContentPath(string contentPath, string baseName)
    {
      var content = typeof(ConsoleUtilsFrm).GetText(contentPath);
      WriteHTML(content, baseName);
    }

    private static void WriteHTML(string content, string baseName)
    {
      var baseHtm = typeof(ConsoleUtilsFrm).GetText(baseName);

      using (var output = new MemoryStream())
      {
        NFX.IO.ConsoleUtils.WriteMarkupContentAsHTML(output, content);

        var outStr = Encoding.UTF8.GetString(output.GetBuffer(), 0, (int)output.Position);

        var processedHtm = baseHtm.Replace("[BODY]", outStr);

        using (var fs = new FileStream("ConsoleHtml.htm", FileMode.Create, FileAccess.Write))
        {
          using (var fwri = new StreamWriter(fs, Encoding.UTF8))
          {
            fwri.Write(processedHtm);
          }
        }
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      var d = new OpenFileDialog() { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*" };

      if (d.ShowDialog() == DialogResult.OK)
        m_txtFile.Text = d.FileName;
    }

    private void m_btnGenerateOpenedFile_Click(object sender, EventArgs e)
    {
      try
      {
        var contet = File.ReadAllText(m_txtFile.Text);
        WriteHTML(contet, m_cmbBase.SelectedItem.ToString());
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

  }
}
