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
  partial class WaveServerForm
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
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.tbPrefix = new System.Windows.Forms.TextBox();
      this.button3 = new System.Windows.Forms.Button();
      this.tbPattern = new System.Windows.Forms.TextBox();
      this.button4 = new System.Windows.Forms.Button();
      this.tbIP = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(26, 41);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(122, 41);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 1;
      this.button2.Text = "button2";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // tbPrefix
      // 
      this.tbPrefix.Location = new System.Drawing.Point(36, 12);
      this.tbPrefix.Name = "tbPrefix";
      this.tbPrefix.Size = new System.Drawing.Size(306, 20);
      this.tbPrefix.TabIndex = 2;
      this.tbPrefix.Text = "http://+:8080/";
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(901, 204);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(75, 23);
      this.button3.TabIndex = 3;
      this.button3.Text = "button3";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // tbPattern
      // 
      this.tbPattern.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbPattern.Location = new System.Drawing.Point(12, 172);
      this.tbPattern.Name = "tbPattern";
      this.tbPattern.Size = new System.Drawing.Size(964, 26);
      this.tbPattern.TabIndex = 4;
      this.tbPattern.Text = "sales/{controller}/{action=send}.htm?h=\'%2e%3d\'&{*query}";
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(799, 50);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(75, 23);
      this.button4.TabIndex = 5;
      this.button4.Text = "button4";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // tbIP
      // 
      this.tbIP.Location = new System.Drawing.Point(645, 24);
      this.tbIP.Name = "tbIP";
      this.tbIP.Size = new System.Drawing.Size(306, 20);
      this.tbIP.TabIndex = 6;
      this.tbIP.TabStop = false;
      this.tbIP.Text = "69.175.12.90";
      // 
      // WaveServerForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(988, 245);
      this.Controls.Add(this.tbIP);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.tbPattern);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.tbPrefix);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Name = "WaveServerForm";
      this.Text = "WaveServerForm";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WaveServerForm_FormClosed);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.TextBox tbPrefix;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.TextBox tbPattern;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.TextBox tbIP;
  }
}