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
namespace WinFormsTest.ConsoleUtils
{
  partial class ConsoleUtilsFrm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_btnGenerate = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.m_btnGenerateOpenedFile = new System.Windows.Forms.Button();
      this.m_txtFile = new System.Windows.Forms.TextBox();
      this.button2 = new System.Windows.Forms.Button();
      this.m_cmbBase = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // m_btnGenerate
      // 
      this.m_btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_btnGenerate.Location = new System.Drawing.Point(432, 12);
      this.m_btnGenerate.Name = "m_btnGenerate";
      this.m_btnGenerate.Size = new System.Drawing.Size(75, 23);
      this.m_btnGenerate.TabIndex = 0;
      this.m_btnGenerate.Text = "Help";
      this.m_btnGenerate.UseVisualStyleBackColor = true;
      this.m_btnGenerate.Click += new System.EventHandler(this.m_btnGenerate_Click);
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(432, 51);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Welcome";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // m_btnGenerateOpenedFile
      // 
      this.m_btnGenerateOpenedFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_btnGenerateOpenedFile.Location = new System.Drawing.Point(432, 89);
      this.m_btnGenerateOpenedFile.Name = "m_btnGenerateOpenedFile";
      this.m_btnGenerateOpenedFile.Size = new System.Drawing.Size(75, 23);
      this.m_btnGenerateOpenedFile.TabIndex = 2;
      this.m_btnGenerateOpenedFile.Text = "Generate";
      this.m_btnGenerateOpenedFile.UseVisualStyleBackColor = true;
      this.m_btnGenerateOpenedFile.Click += new System.EventHandler(this.m_btnGenerateOpenedFile_Click);
      // 
      // m_txtFile
      // 
      this.m_txtFile.Location = new System.Drawing.Point(12, 91);
      this.m_txtFile.Name = "m_txtFile";
      this.m_txtFile.Size = new System.Drawing.Size(326, 20);
      this.m_txtFile.TabIndex = 3;
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(348, 89);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 4;
      this.button2.Text = "Open";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // m_cmbBase
      // 
      this.m_cmbBase.FormattingEnabled = true;
      this.m_cmbBase.Items.AddRange(new object[] {
            "ConsoleUtils.Default.htm",
            "ConsoleUtils.Modern.htm"});
      this.m_cmbBase.Location = new System.Drawing.Point(12, 14);
      this.m_cmbBase.Name = "m_cmbBase";
      this.m_cmbBase.Size = new System.Drawing.Size(146, 21);
      this.m_cmbBase.TabIndex = 5;
      // 
      // ConsoleUtilsFrm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(519, 164);
      this.Controls.Add(this.m_cmbBase);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.m_txtFile);
      this.Controls.Add(this.m_btnGenerateOpenedFile);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.m_btnGenerate);
      this.Name = "ConsoleUtilsFrm";
      this.Text = "ConsoleUtilsFrm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button m_btnGenerate;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button m_btnGenerateOpenedFile;
    private System.Windows.Forms.TextBox m_txtFile;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.ComboBox m_cmbBase;
  }
}