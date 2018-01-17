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
  partial class BlankForm
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
      this.pic = new System.Windows.Forms.PictureBox();
      this.button4 = new System.Windows.Forms.Button();
      this.tbCode = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
      this.SuspendLayout();
      // 
      // pic
      // 
      this.pic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pic.Location = new System.Drawing.Point(12, 70);
      this.pic.Name = "pic";
      this.pic.Size = new System.Drawing.Size(903, 415);
      this.pic.TabIndex = 4;
      this.pic.TabStop = false;
      this.pic.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pic_MouseClick);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(12, 12);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(128, 34);
      this.button4.TabIndex = 5;
      this.button4.Text = "Show Puzzle";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // tbCode
      // 
      this.tbCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbCode.Location = new System.Drawing.Point(162, 15);
      this.tbCode.Name = "tbCode";
      this.tbCode.Size = new System.Drawing.Size(275, 31);
      this.tbCode.TabIndex = 6;
      this.tbCode.Text = "1234567890";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(508, 16);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(128, 34);
      this.button1.TabIndex = 7;
      this.button1.Text = "Show Puzzle";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click_1);
      // 
      // BlankForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(927, 497);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.tbCode);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.pic);
      this.Name = "BlankForm";
      this.Text = "BlankForm";
      ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox pic;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.TextBox tbCode;
    private System.Windows.Forms.Button button1;
  }
}