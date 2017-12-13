/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
    partial class MailForm
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
      this.lblFROMAddress = new System.Windows.Forms.Label();
      this.tbFROMAddress = new System.Windows.Forms.TextBox();
      this.tbTOAddress = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tbSubject = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.tbBody = new System.Windows.Forms.TextBox();
      this.btSend = new System.Windows.Forms.Button();
      this.tbHTML = new System.Windows.Forms.TextBox();
      this.includeAttachments = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblFROMAddress
      // 
      this.lblFROMAddress.AutoSize = true;
      this.lblFROMAddress.Location = new System.Drawing.Point(13, 12);
      this.lblFROMAddress.Name = "lblFROMAddress";
      this.lblFROMAddress.Size = new System.Drawing.Size(33, 13);
      this.lblFROMAddress.TabIndex = 1;
      this.lblFROMAddress.Text = "From:";
      // 
      // tbFROMAddress
      // 
      this.tbFROMAddress.Location = new System.Drawing.Point(64, 9);
      this.tbFROMAddress.Name = "tbFROMAddress";
      this.tbFROMAddress.Size = new System.Drawing.Size(441, 20);
      this.tbFROMAddress.TabIndex = 3;
      // 
      // tbTOAddress
      // 
      this.tbTOAddress.Location = new System.Drawing.Point(64, 38);
      this.tbTOAddress.Name = "tbTOAddress";
      this.tbTOAddress.Size = new System.Drawing.Size(441, 20);
      this.tbTOAddress.TabIndex = 7;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 41);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(23, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "To:";
      // 
      // tbSubject
      // 
      this.tbSubject.Location = new System.Drawing.Point(64, 67);
      this.tbSubject.Name = "tbSubject";
      this.tbSubject.Size = new System.Drawing.Size(215, 20);
      this.tbSubject.TabIndex = 9;
      this.tbSubject.Text = "Test";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(13, 70);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(46, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "Subject:";
      // 
      // tbBody
      // 
      this.tbBody.Location = new System.Drawing.Point(16, 133);
      this.tbBody.Multiline = true;
      this.tbBody.Name = "tbBody";
      this.tbBody.Size = new System.Drawing.Size(605, 148);
      this.tbBody.TabIndex = 10;
      this.tbBody.Text = "Test";
      // 
      // btSend
      // 
      this.btSend.Location = new System.Drawing.Point(521, 9);
      this.btSend.Name = "btSend";
      this.btSend.Size = new System.Drawing.Size(100, 49);
      this.btSend.TabIndex = 11;
      this.btSend.Text = "Send";
      this.btSend.UseVisualStyleBackColor = true;
      this.btSend.Click += new System.EventHandler(this.btSend_Click);
      // 
      // tbHTML
      // 
      this.tbHTML.Location = new System.Drawing.Point(16, 301);
      this.tbHTML.Multiline = true;
      this.tbHTML.Name = "tbHTML";
      this.tbHTML.Size = new System.Drawing.Size(605, 148);
      this.tbHTML.TabIndex = 12;
      this.tbHTML.Text = "<html>\r\n<body>\r\n<h1>Header</h1>\r\n<p>Test Message</p>\r\n</body>\r\n</html>";
      // 
      // includeAttachments
      // 
      this.includeAttachments.AutoSize = true;
      this.includeAttachments.Location = new System.Drawing.Point(382, 70);
      this.includeAttachments.Name = "includeAttachments";
      this.includeAttachments.Size = new System.Drawing.Size(123, 17);
      this.includeAttachments.TabIndex = 13;
      this.includeAttachments.Text = "Include Attachments";
      this.includeAttachments.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(13, 104);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(333, 13);
      this.label2.TabIndex = 14;
      this.label2.Text = "Note that From and To addresses should be entered in laconic format";
      // 
      // MailForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(640, 464);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.includeAttachments);
      this.Controls.Add(this.tbHTML);
      this.Controls.Add(this.btSend);
      this.Controls.Add(this.tbBody);
      this.Controls.Add(this.tbSubject);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.tbTOAddress);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.tbFROMAddress);
      this.Controls.Add(this.lblFROMAddress);
      this.Name = "MailForm";
      this.Text = "MailForm";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblFROMAddress;
        private System.Windows.Forms.TextBox tbFROMAddress;
        private System.Windows.Forms.TextBox tbTOAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbSubject;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbBody;
        private System.Windows.Forms.Button btSend;
        private System.Windows.Forms.TextBox tbHTML;
    private System.Windows.Forms.CheckBox includeAttachments;
    private System.Windows.Forms.Label label2;
  }
}