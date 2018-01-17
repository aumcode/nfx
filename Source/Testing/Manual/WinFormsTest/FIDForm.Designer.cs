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
namespace WinFormsTest
{
  partial class FIDForm
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
      this.btnGUID = new System.Windows.Forms.Button();
      this.tbCount = new System.Windows.Forms.TextBox();
      this.btnFID = new System.Windows.Forms.Button();
      this.chkParallel = new System.Windows.Forms.CheckBox();
      this.tbDump = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnGUID
      // 
      this.btnGUID.Location = new System.Drawing.Point(59, 93);
      this.btnGUID.Name = "btnGUID";
      this.btnGUID.Size = new System.Drawing.Size(75, 41);
      this.btnGUID.TabIndex = 0;
      this.btnGUID.Text = "GUID";
      this.btnGUID.UseVisualStyleBackColor = true;
      this.btnGUID.Click += new System.EventHandler(this.btnGUID_Click);
      // 
      // tbCount
      // 
      this.tbCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbCount.Location = new System.Drawing.Point(59, 13);
      this.tbCount.Name = "tbCount";
      this.tbCount.Size = new System.Drawing.Size(186, 29);
      this.tbCount.TabIndex = 1;
      this.tbCount.Text = "100000000";
      // 
      // btnFID
      // 
      this.btnFID.Location = new System.Drawing.Point(170, 93);
      this.btnFID.Name = "btnFID";
      this.btnFID.Size = new System.Drawing.Size(75, 41);
      this.btnFID.TabIndex = 2;
      this.btnFID.Text = "FID";
      this.btnFID.UseVisualStyleBackColor = true;
      this.btnFID.Click += new System.EventHandler(this.btnFID_Click);
      // 
      // chkParallel
      // 
      this.chkParallel.AutoSize = true;
      this.chkParallel.Location = new System.Drawing.Point(59, 48);
      this.chkParallel.Name = "chkParallel";
      this.chkParallel.Size = new System.Drawing.Size(84, 17);
      this.chkParallel.TabIndex = 3;
      this.chkParallel.Text = "Parallel Test";
      this.chkParallel.UseVisualStyleBackColor = true;
      // 
      // tbDump
      // 
      this.tbDump.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
      this.tbDump.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbDump.ForeColor = System.Drawing.Color.Yellow;
      this.tbDump.Location = new System.Drawing.Point(310, 12);
      this.tbDump.Multiline = true;
      this.tbDump.Name = "tbDump";
      this.tbDump.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbDump.Size = new System.Drawing.Size(596, 682);
      this.tbDump.TabIndex = 4;
      this.tbDump.Text = "100000000";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(912, 24);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 41);
      this.button1.TabIndex = 5;
      this.button1.Text = "FID";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // FIDForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1010, 729);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.tbDump);
      this.Controls.Add(this.chkParallel);
      this.Controls.Add(this.btnFID);
      this.Controls.Add(this.tbCount);
      this.Controls.Add(this.btnGUID);
      this.Name = "FIDForm";
      this.Text = "TWIDForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnGUID;
    private System.Windows.Forms.TextBox tbCount;
    private System.Windows.Forms.Button btnFID;
    private System.Windows.Forms.CheckBox chkParallel;
    private System.Windows.Forms.TextBox tbDump;
    private System.Windows.Forms.Button button1;
  }
}