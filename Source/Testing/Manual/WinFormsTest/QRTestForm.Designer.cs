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
namespace WinFormsTest
{
  partial class QRTestForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QRTestForm));
      this.m_pnlParams = new System.Windows.Forms.Panel();
      this.m_cmbScale = new System.Windows.Forms.ComboBox();
      this.m_lblScale = new System.Windows.Forms.Label();
      this.m_cmbCorrection = new System.Windows.Forms.ComboBox();
      this.m_lblCorrection = new System.Windows.Forms.Label();
      this.m_lblContent = new System.Windows.Forms.Label();
      this.m_txtContent = new System.Windows.Forms.TextBox();
      this.m_btnSave = new System.Windows.Forms.Button();
      this.m_btnGenerate = new System.Windows.Forms.Button();
      this.m_tbllayoutLeft = new System.Windows.Forms.TableLayoutPanel();
      this.m_pnlImage = new System.Windows.Forms.Panel();
      this.m_txtTrace = new System.Windows.Forms.TextBox();
      this.m_pnlParams.SuspendLayout();
      this.m_tbllayoutLeft.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_pnlParams
      // 
      this.m_pnlParams.Controls.Add(this.m_cmbScale);
      this.m_pnlParams.Controls.Add(this.m_lblScale);
      this.m_pnlParams.Controls.Add(this.m_cmbCorrection);
      this.m_pnlParams.Controls.Add(this.m_lblCorrection);
      this.m_pnlParams.Controls.Add(this.m_lblContent);
      this.m_pnlParams.Controls.Add(this.m_txtContent);
      this.m_pnlParams.Controls.Add(this.m_btnSave);
      this.m_pnlParams.Controls.Add(this.m_btnGenerate);
      this.m_pnlParams.Dock = System.Windows.Forms.DockStyle.Right;
      this.m_pnlParams.Location = new System.Drawing.Point(401, 0);
      this.m_pnlParams.Name = "m_pnlParams";
      this.m_pnlParams.Size = new System.Drawing.Size(365, 545);
      this.m_pnlParams.TabIndex = 0;
      // 
      // m_cmbScale
      // 
      this.m_cmbScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_cmbScale.FormattingEnabled = true;
      this.m_cmbScale.Location = new System.Drawing.Point(73, 76);
      this.m_cmbScale.Name = "m_cmbScale";
      this.m_cmbScale.Size = new System.Drawing.Size(67, 21);
      this.m_cmbScale.TabIndex = 3;
      // 
      // m_lblScale
      // 
      this.m_lblScale.AutoSize = true;
      this.m_lblScale.Location = new System.Drawing.Point(31, 81);
      this.m_lblScale.Name = "m_lblScale";
      this.m_lblScale.Size = new System.Drawing.Size(34, 13);
      this.m_lblScale.TabIndex = 2;
      this.m_lblScale.Text = "Scale";
      // 
      // m_cmbCorrection
      // 
      this.m_cmbCorrection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_cmbCorrection.FormattingEnabled = true;
      this.m_cmbCorrection.Location = new System.Drawing.Point(73, 42);
      this.m_cmbCorrection.Name = "m_cmbCorrection";
      this.m_cmbCorrection.Size = new System.Drawing.Size(47, 21);
      this.m_cmbCorrection.TabIndex = 3;
      // 
      // m_lblCorrection
      // 
      this.m_lblCorrection.AutoSize = true;
      this.m_lblCorrection.Location = new System.Drawing.Point(10, 44);
      this.m_lblCorrection.Name = "m_lblCorrection";
      this.m_lblCorrection.Size = new System.Drawing.Size(55, 13);
      this.m_lblCorrection.TabIndex = 2;
      this.m_lblCorrection.Text = "Correction";
      // 
      // m_lblContent
      // 
      this.m_lblContent.AutoSize = true;
      this.m_lblContent.Location = new System.Drawing.Point(21, 13);
      this.m_lblContent.Name = "m_lblContent";
      this.m_lblContent.Size = new System.Drawing.Size(44, 13);
      this.m_lblContent.TabIndex = 2;
      this.m_lblContent.Text = "Content";
      // 
      // m_txtContent
      // 
      this.m_txtContent.Location = new System.Drawing.Point(73, 9);
      this.m_txtContent.Name = "m_txtContent";
      this.m_txtContent.Size = new System.Drawing.Size(280, 20);
      this.m_txtContent.TabIndex = 1;
      this.m_txtContent.Text = "ABCDEF";
      // 
      // m_btnSave
      // 
      this.m_btnSave.Location = new System.Drawing.Point(169, 121);
      this.m_btnSave.Name = "m_btnSave";
      this.m_btnSave.Size = new System.Drawing.Size(75, 23);
      this.m_btnSave.TabIndex = 0;
      this.m_btnSave.Text = "Save";
      this.m_btnSave.UseVisualStyleBackColor = true;
      this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
      // 
      // m_btnGenerate
      // 
      this.m_btnGenerate.Location = new System.Drawing.Point(88, 121);
      this.m_btnGenerate.Name = "m_btnGenerate";
      this.m_btnGenerate.Size = new System.Drawing.Size(75, 23);
      this.m_btnGenerate.TabIndex = 0;
      this.m_btnGenerate.Text = "Generate";
      this.m_btnGenerate.UseVisualStyleBackColor = true;
      this.m_btnGenerate.Click += new System.EventHandler(this.m_btnGenerate_Click);
      // 
      // m_tbllayoutLeft
      // 
      this.m_tbllayoutLeft.ColumnCount = 1;
      this.m_tbllayoutLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_tbllayoutLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.m_tbllayoutLeft.Controls.Add(this.m_pnlImage, 0, 0);
      this.m_tbllayoutLeft.Controls.Add(this.m_txtTrace, 0, 1);
      this.m_tbllayoutLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_tbllayoutLeft.Location = new System.Drawing.Point(0, 0);
      this.m_tbllayoutLeft.Name = "m_tbllayoutLeft";
      this.m_tbllayoutLeft.RowCount = 2;
      this.m_tbllayoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.m_tbllayoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.m_tbllayoutLeft.Size = new System.Drawing.Size(401, 545);
      this.m_tbllayoutLeft.TabIndex = 0;
      // 
      // m_pnlImage
      // 
      this.m_pnlImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.m_pnlImage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_pnlImage.Location = new System.Drawing.Point(3, 3);
      this.m_pnlImage.Name = "m_pnlImage";
      this.m_pnlImage.Size = new System.Drawing.Size(395, 266);
      this.m_pnlImage.TabIndex = 0;
      this.m_pnlImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_pnlImage_MouseDown);
      // 
      // m_txtTrace
      // 
      this.m_txtTrace.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_txtTrace.Location = new System.Drawing.Point(3, 275);
      this.m_txtTrace.Multiline = true;
      this.m_txtTrace.Name = "m_txtTrace";
      this.m_txtTrace.ReadOnly = true;
      this.m_txtTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.m_txtTrace.Size = new System.Drawing.Size(395, 267);
      this.m_txtTrace.TabIndex = 1;
      this.m_txtTrace.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_txtTrace_MouseDown);
      // 
      // QRTestForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(766, 545);
      this.Controls.Add(this.m_tbllayoutLeft);
      this.Controls.Add(this.m_pnlParams);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(676, 452);
      this.Name = "QRTestForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "QR Test";
      this.m_pnlParams.ResumeLayout(false);
      this.m_pnlParams.PerformLayout();
      this.m_tbllayoutLeft.ResumeLayout(false);
      this.m_tbllayoutLeft.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel m_pnlParams;
    private System.Windows.Forms.Button m_btnGenerate;
    private System.Windows.Forms.ComboBox m_cmbScale;
    private System.Windows.Forms.Label m_lblScale;
    private System.Windows.Forms.ComboBox m_cmbCorrection;
    private System.Windows.Forms.Label m_lblCorrection;
    private System.Windows.Forms.Label m_lblContent;
    private System.Windows.Forms.TextBox m_txtContent;
    private System.Windows.Forms.TableLayoutPanel m_tbllayoutLeft;
    private System.Windows.Forms.Panel m_pnlImage;
    private System.Windows.Forms.TextBox m_txtTrace;
    private System.Windows.Forms.Button m_btnSave;
  }
}