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
    partial class ELinkForm
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
      this.tbELink = new System.Windows.Forms.TextBox();
      this.sb = new System.Windows.Forms.HScrollBar();
      this.tbID = new System.Windows.Forms.TextBox();
      this.btnCalculate = new System.Windows.Forms.Button();
      this.btnBigID = new System.Windows.Forms.Button();
      this.tbResult = new System.Windows.Forms.TextBox();
      this.btnDecode = new System.Windows.Forms.Button();
      this.tbPassword = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.sbh = new System.Windows.Forms.HScrollBar();
      this.sbv = new System.Windows.Forms.VScrollBar();
      this.cb2 = new System.Windows.Forms.CheckBox();
      this.pb3 = new System.Windows.Forms.PictureBox();
      this.pb2 = new System.Windows.Forms.PictureBox();
      this.pb1 = new System.Windows.Forms.PictureBox();
      this.sbEra = new System.Windows.Forms.HScrollBar();
      this.button2 = new System.Windows.Forms.Button();
      this.btnPuzzle = new System.Windows.Forms.Button();
      this.pic = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pb3)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
      this.SuspendLayout();
      // 
      // tbELink
      // 
      this.tbELink.Font = new System.Drawing.Font("Courier New", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbELink.Location = new System.Drawing.Point(12, 12);
      this.tbELink.Name = "tbELink";
      this.tbELink.Size = new System.Drawing.Size(536, 31);
      this.tbELink.TabIndex = 0;
      // 
      // sb
      // 
      this.sb.Location = new System.Drawing.Point(12, 150);
      this.sb.Maximum = 2000000000;
      this.sb.Name = "sb";
      this.sb.Size = new System.Drawing.Size(802, 23);
      this.sb.TabIndex = 1;
      this.sb.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sb_Scroll);
      // 
      // tbID
      // 
      this.tbID.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbID.Location = new System.Drawing.Point(12, 58);
      this.tbID.Name = "tbID";
      this.tbID.Size = new System.Drawing.Size(174, 29);
      this.tbID.TabIndex = 2;
      // 
      // btnCalculate
      // 
      this.btnCalculate.Location = new System.Drawing.Point(205, 59);
      this.btnCalculate.Name = "btnCalculate";
      this.btnCalculate.Size = new System.Drawing.Size(130, 28);
      this.btnCalculate.TabIndex = 3;
      this.btnCalculate.Text = "button1";
      this.btnCalculate.UseVisualStyleBackColor = true;
      this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
      // 
      // btnBigID
      // 
      this.btnBigID.Location = new System.Drawing.Point(445, 61);
      this.btnBigID.Name = "btnBigID";
      this.btnBigID.Size = new System.Drawing.Size(130, 28);
      this.btnBigID.TabIndex = 4;
      this.btnBigID.Text = "Buig ID";
      this.btnBigID.UseVisualStyleBackColor = true;
      this.btnBigID.Click += new System.EventHandler(this.btnBigID_Click);
      // 
      // tbResult
      // 
      this.tbResult.Font = new System.Drawing.Font("Courier New", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbResult.Location = new System.Drawing.Point(702, 12);
      this.tbResult.Name = "tbResult";
      this.tbResult.Size = new System.Drawing.Size(650, 31);
      this.tbResult.TabIndex = 5;
      // 
      // btnDecode
      // 
      this.btnDecode.Location = new System.Drawing.Point(566, 15);
      this.btnDecode.Name = "btnDecode";
      this.btnDecode.Size = new System.Drawing.Size(130, 28);
      this.btnDecode.TabIndex = 6;
      this.btnDecode.Text = ">>> Decode >>>";
      this.btnDecode.UseVisualStyleBackColor = true;
      this.btnDecode.Click += new System.EventHandler(this.btnDecode_Click);
      // 
      // tbPassword
      // 
      this.tbPassword.Font = new System.Drawing.Font("Courier New", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbPassword.Location = new System.Drawing.Point(25, 231);
      this.tbPassword.Name = "tbPassword";
      this.tbPassword.Size = new System.Drawing.Size(536, 31);
      this.tbPassword.TabIndex = 7;
      this.tbPassword.TextChanged += new System.EventHandler(this.tbPassword_TextChanged);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(205, 119);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(130, 28);
      this.button1.TabIndex = 8;
      this.button1.Text = "GDID";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // sbh
      // 
      this.sbh.Location = new System.Drawing.Point(851, 150);
      this.sbh.Maximum = 800;
      this.sbh.Minimum = 10;
      this.sbh.Name = "sbh";
      this.sbh.Size = new System.Drawing.Size(334, 23);
      this.sbh.TabIndex = 11;
      this.sbh.Value = 50;
      this.sbh.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sbv_Scroll);
      // 
      // sbv
      // 
      this.sbv.Location = new System.Drawing.Point(830, 178);
      this.sbv.Maximum = 800;
      this.sbv.Minimum = 10;
      this.sbv.Name = "sbv";
      this.sbv.Size = new System.Drawing.Size(20, 237);
      this.sbv.TabIndex = 12;
      this.sbv.Value = 50;
      this.sbv.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sbv_Scroll);
      // 
      // cb2
      // 
      this.cb2.AutoSize = true;
      this.cb2.Location = new System.Drawing.Point(777, 440);
      this.cb2.Name = "cb2";
      this.cb2.Size = new System.Drawing.Size(80, 17);
      this.cb2.TabIndex = 14;
      this.cb2.Text = "checkBox1";
      this.cb2.UseVisualStyleBackColor = true;
      // 
      // pb3
      // 
      this.pb3.Image = global::WinFormsTest.Properties.Resources._20140601_204233___2;
      this.pb3.Location = new System.Drawing.Point(616, 356);
      this.pb3.Name = "pb3";
      this.pb3.Size = new System.Drawing.Size(130, 167);
      this.pb3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pb3.TabIndex = 13;
      this.pb3.TabStop = false;
      // 
      // pb2
      // 
      this.pb2.Location = new System.Drawing.Point(862, 198);
      this.pb2.Name = "pb2";
      this.pb2.Size = new System.Drawing.Size(246, 128);
      this.pb2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pb2.TabIndex = 10;
      this.pb2.TabStop = false;
      // 
      // pb1
      // 
      this.pb1.Image = global::WinFormsTest.Properties.Resources._20140601_204233;
      this.pb1.Location = new System.Drawing.Point(579, 183);
      this.pb1.Name = "pb1";
      this.pb1.Size = new System.Drawing.Size(235, 167);
      this.pb1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pb1.TabIndex = 9;
      this.pb1.TabStop = false;
      // 
      // sbEra
      // 
      this.sbEra.Location = new System.Drawing.Point(12, 198);
      this.sbEra.Maximum = 2000000000;
      this.sbEra.Name = "sbEra";
      this.sbEra.Size = new System.Drawing.Size(506, 23);
      this.sbEra.TabIndex = 15;
      this.sbEra.Value = 7;
      this.sbEra.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sbEra_Scroll);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(341, 61);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(57, 28);
      this.button2.TabIndex = 16;
      this.button2.Text = "button1";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // btnPuzzle
      // 
      this.btnPuzzle.Location = new System.Drawing.Point(221, 268);
      this.btnPuzzle.Name = "btnPuzzle";
      this.btnPuzzle.Size = new System.Drawing.Size(130, 28);
      this.btnPuzzle.TabIndex = 17;
      this.btnPuzzle.Text = "Puzzle";
      this.btnPuzzle.UseVisualStyleBackColor = true;
      this.btnPuzzle.Click += new System.EventHandler(this.btnPuzzle_Click);
      // 
      // pic
      // 
      this.pic.Location = new System.Drawing.Point(25, 314);
      this.pic.Name = "pic";
      this.pic.Size = new System.Drawing.Size(100, 50);
      this.pic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pic.TabIndex = 18;
      this.pic.TabStop = false;
      // 
      // ELinkForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1383, 535);
      this.Controls.Add(this.pic);
      this.Controls.Add(this.btnPuzzle);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.sbEra);
      this.Controls.Add(this.cb2);
      this.Controls.Add(this.pb3);
      this.Controls.Add(this.sbv);
      this.Controls.Add(this.sbh);
      this.Controls.Add(this.pb2);
      this.Controls.Add(this.pb1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.tbPassword);
      this.Controls.Add(this.btnDecode);
      this.Controls.Add(this.tbResult);
      this.Controls.Add(this.btnBigID);
      this.Controls.Add(this.btnCalculate);
      this.Controls.Add(this.tbID);
      this.Controls.Add(this.sb);
      this.Controls.Add(this.tbELink);
      this.Name = "ELinkForm";
      this.Text = "ELinkForm";
      this.Load += new System.EventHandler(this.ELinkForm_Load);
      ((System.ComponentModel.ISupportInitialize)(this.pb3)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbELink;
        private System.Windows.Forms.HScrollBar sb;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Button btnBigID;
        private System.Windows.Forms.TextBox tbResult;
        private System.Windows.Forms.Button btnDecode;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pb1;
        private System.Windows.Forms.PictureBox pb2;
        private System.Windows.Forms.HScrollBar sbh;
        private System.Windows.Forms.VScrollBar sbv;
        private System.Windows.Forms.PictureBox pb3;
        private System.Windows.Forms.CheckBox cb2;
        private System.Windows.Forms.HScrollBar sbEra;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnPuzzle;
        private System.Windows.Forms.PictureBox pic;
    }
}