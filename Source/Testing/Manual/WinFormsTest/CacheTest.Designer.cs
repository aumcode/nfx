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
    partial class CacheTest
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
      this.components = new System.ComponentModel.Container();
      this.btnPopulate = new System.Windows.Forms.Button();
      this.tbID = new System.Windows.Forms.TextBox();
      this.btnFind = new System.Windows.Forms.Button();
      this.tbCount = new System.Windows.Forms.TextBox();
      this.btnGC = new System.Windows.Forms.Button();
      this.tmr = new System.Windows.Forms.Timer(this.components);
      this.cbTimer = new System.Windows.Forms.CheckBox();
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.tbExpiration = new System.Windows.Forms.TextBox();
      this.btnAllocHuge = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnPopulate
      // 
      this.btnPopulate.Location = new System.Drawing.Point(12, 35);
      this.btnPopulate.Name = "btnPopulate";
      this.btnPopulate.Size = new System.Drawing.Size(230, 23);
      this.btnPopulate.TabIndex = 0;
      this.btnPopulate.Text = "Populate";
      this.btnPopulate.UseVisualStyleBackColor = true;
      this.btnPopulate.Click += new System.EventHandler(this.btnPopulate_Click);
      // 
      // tbID
      // 
      this.tbID.Location = new System.Drawing.Point(342, 112);
      this.tbID.Name = "tbID";
      this.tbID.Size = new System.Drawing.Size(100, 20);
      this.tbID.TabIndex = 1;
      // 
      // btnFind
      // 
      this.btnFind.Location = new System.Drawing.Point(448, 109);
      this.btnFind.Name = "btnFind";
      this.btnFind.Size = new System.Drawing.Size(75, 23);
      this.btnFind.TabIndex = 2;
      this.btnFind.Text = "Find";
      this.btnFind.UseVisualStyleBackColor = true;
      this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
      // 
      // tbCount
      // 
      this.tbCount.Location = new System.Drawing.Point(14, 9);
      this.tbCount.Name = "tbCount";
      this.tbCount.Size = new System.Drawing.Size(100, 20);
      this.tbCount.TabIndex = 3;
      this.tbCount.Text = "1000";
      // 
      // btnGC
      // 
      this.btnGC.Location = new System.Drawing.Point(181, 180);
      this.btnGC.Name = "btnGC";
      this.btnGC.Size = new System.Drawing.Size(75, 23);
      this.btnGC.TabIndex = 4;
      this.btnGC.Text = "GC";
      this.btnGC.UseVisualStyleBackColor = true;
      this.btnGC.Click += new System.EventHandler(this.btnGC_Click);
      // 
      // tmr
      // 
      this.tmr.Enabled = true;
      this.tmr.Interval = 1000;
      this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
      // 
      // cbTimer
      // 
      this.cbTimer.AutoSize = true;
      this.cbTimer.Location = new System.Drawing.Point(67, 109);
      this.cbTimer.Name = "cbTimer";
      this.cbTimer.Size = new System.Drawing.Size(52, 17);
      this.cbTimer.TabIndex = 5;
      this.cbTimer.Text = "Timer";
      this.cbTimer.UseVisualStyleBackColor = true;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(273, 35);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(230, 23);
      this.button1.TabIndex = 6;
      this.button1.Text = "Populate";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(529, 109);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 7;
      this.button2.Text = "Find";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // tbExpiration
      // 
      this.tbExpiration.Location = new System.Drawing.Point(19, 183);
      this.tbExpiration.Name = "tbExpiration";
      this.tbExpiration.Size = new System.Drawing.Size(122, 20);
      this.tbExpiration.TabIndex = 8;
      this.tbExpiration.Text = "2/2/2014 8:00 PM";
      // 
      // btnAllocHuge
      // 
      this.btnAllocHuge.Location = new System.Drawing.Point(367, 155);
      this.btnAllocHuge.Name = "btnAllocHuge";
      this.btnAllocHuge.Size = new System.Drawing.Size(124, 68);
      this.btnAllocHuge.TabIndex = 9;
      this.btnAllocHuge.Text = "Alloc Huge";
      this.btnAllocHuge.UseVisualStyleBackColor = true;
      this.btnAllocHuge.Click += new System.EventHandler(this.btnAllocHuge_Click);
      // 
      // CacheTest
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(931, 246);
      this.Controls.Add(this.btnAllocHuge);
      this.Controls.Add(this.tbExpiration);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.cbTimer);
      this.Controls.Add(this.btnGC);
      this.Controls.Add(this.tbCount);
      this.Controls.Add(this.btnFind);
      this.Controls.Add(this.tbID);
      this.Controls.Add(this.btnPopulate);
      this.Name = "CacheTest";
      this.Text = "CacheTest";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CacheTest_FormClosed);
      this.Load += new System.EventHandler(this.CacheTest_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPopulate;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.TextBox tbCount;
        private System.Windows.Forms.Button btnGC;
        private System.Windows.Forms.Timer tmr;
        private System.Windows.Forms.CheckBox cbTimer;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tbExpiration;
        private System.Windows.Forms.Button btnAllocHuge;
    }
}