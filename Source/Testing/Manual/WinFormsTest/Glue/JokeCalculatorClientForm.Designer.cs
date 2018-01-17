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
namespace WinFormsTest.Glue
{
  partial class JokeCalculatorClientForm
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
      this.m_spltMain = new System.Windows.Forms.SplitContainer();
      this.m_txtLog = new System.Windows.Forms.TextBox();
      this.m_lblASyncTotal = new System.Windows.Forms.Label();
      this.m_btnStopASync = new System.Windows.Forms.Button();
      this.m_btnStartASync = new System.Windows.Forms.Button();
      this.m_btnCallAsync = new System.Windows.Forms.Button();
      this.m_btnRun = new System.Windows.Forms.Button();
      this.m_btnStream = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.m_spltMain)).BeginInit();
      this.m_spltMain.Panel1.SuspendLayout();
      this.m_spltMain.Panel2.SuspendLayout();
      this.m_spltMain.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_spltMain
      // 
      this.m_spltMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_spltMain.Location = new System.Drawing.Point(0, 0);
      this.m_spltMain.Name = "m_spltMain";
      // 
      // m_spltMain.Panel1
      // 
      this.m_spltMain.Panel1.Controls.Add(this.m_txtLog);
      // 
      // m_spltMain.Panel2
      // 
      this.m_spltMain.Panel2.Controls.Add(this.m_btnStream);
      this.m_spltMain.Panel2.Controls.Add(this.m_lblASyncTotal);
      this.m_spltMain.Panel2.Controls.Add(this.m_btnStopASync);
      this.m_spltMain.Panel2.Controls.Add(this.m_btnStartASync);
      this.m_spltMain.Panel2.Controls.Add(this.m_btnCallAsync);
      this.m_spltMain.Panel2.Controls.Add(this.m_btnRun);
      this.m_spltMain.Size = new System.Drawing.Size(569, 375);
      this.m_spltMain.SplitterDistance = 343;
      this.m_spltMain.TabIndex = 0;
      // 
      // m_txtLog
      // 
      this.m_txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_txtLog.Location = new System.Drawing.Point(0, 0);
      this.m_txtLog.Multiline = true;
      this.m_txtLog.Name = "m_txtLog";
      this.m_txtLog.Size = new System.Drawing.Size(343, 375);
      this.m_txtLog.TabIndex = 0;
      // 
      // m_lblASyncTotal
      // 
      this.m_lblASyncTotal.AutoSize = true;
      this.m_lblASyncTotal.Location = new System.Drawing.Point(43, 111);
      this.m_lblASyncTotal.Name = "m_lblASyncTotal";
      this.m_lblASyncTotal.Size = new System.Drawing.Size(58, 13);
      this.m_lblASyncTotal.TabIndex = 1;
      this.m_lblASyncTotal.Text = "async total";
      // 
      // m_btnStopASync
      // 
      this.m_btnStopASync.Location = new System.Drawing.Point(135, 76);
      this.m_btnStopASync.Name = "m_btnStopASync";
      this.m_btnStopASync.Size = new System.Drawing.Size(75, 23);
      this.m_btnStopASync.TabIndex = 0;
      this.m_btnStopASync.Text = "Stop ASync";
      this.m_btnStopASync.UseVisualStyleBackColor = true;
      this.m_btnStopASync.Click += new System.EventHandler(this.m_btnStopASync_Click);
      // 
      // m_btnStartASync
      // 
      this.m_btnStartASync.Location = new System.Drawing.Point(46, 76);
      this.m_btnStartASync.Name = "m_btnStartASync";
      this.m_btnStartASync.Size = new System.Drawing.Size(75, 23);
      this.m_btnStartASync.TabIndex = 0;
      this.m_btnStartASync.Text = "Start ASync";
      this.m_btnStartASync.UseVisualStyleBackColor = true;
      this.m_btnStartASync.Click += new System.EventHandler(this.m_btnStartASync_Click);
      // 
      // m_btnCallAsync
      // 
      this.m_btnCallAsync.Location = new System.Drawing.Point(135, 41);
      this.m_btnCallAsync.Name = "m_btnCallAsync";
      this.m_btnCallAsync.Size = new System.Drawing.Size(75, 23);
      this.m_btnCallAsync.TabIndex = 0;
      this.m_btnCallAsync.Text = "Call ASync";
      this.m_btnCallAsync.UseVisualStyleBackColor = true;
      this.m_btnCallAsync.Click += new System.EventHandler(this.m_btnCallAsync_Click);
      // 
      // m_btnRun
      // 
      this.m_btnRun.Location = new System.Drawing.Point(135, 12);
      this.m_btnRun.Name = "m_btnRun";
      this.m_btnRun.Size = new System.Drawing.Size(75, 23);
      this.m_btnRun.TabIndex = 0;
      this.m_btnRun.Text = "Call Sync";
      this.m_btnRun.UseVisualStyleBackColor = true;
      this.m_btnRun.Click += new System.EventHandler(this.m_btnRun_Click);
      // 
      // m_btnStream
      // 
      this.m_btnStream.Location = new System.Drawing.Point(74, 176);
      this.m_btnStream.Name = "m_btnStream";
      this.m_btnStream.Size = new System.Drawing.Size(75, 23);
      this.m_btnStream.TabIndex = 2;
      this.m_btnStream.Text = "Stream";
      this.m_btnStream.UseVisualStyleBackColor = true;
      this.m_btnStream.Click += new System.EventHandler(this.m_btnStream_Click);
      // 
      // JokeCalculatorClientForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(569, 375);
      this.Controls.Add(this.m_spltMain);
      this.Name = "JokeCalculatorClientForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "JokeCalculatorClientForm";
      this.m_spltMain.Panel1.ResumeLayout(false);
      this.m_spltMain.Panel1.PerformLayout();
      this.m_spltMain.Panel2.ResumeLayout(false);
      this.m_spltMain.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_spltMain)).EndInit();
      this.m_spltMain.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer m_spltMain;
    private System.Windows.Forms.TextBox m_txtLog;
    private System.Windows.Forms.Button m_btnRun;
    private System.Windows.Forms.Button m_btnCallAsync;
    private System.Windows.Forms.Label m_lblASyncTotal;
    private System.Windows.Forms.Button m_btnStopASync;
    private System.Windows.Forms.Button m_btnStartASync;
    private System.Windows.Forms.Button m_btnStream;
  }
}