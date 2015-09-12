namespace WinFormsTest
{
  partial class BSONTestForm
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
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.button5 = new System.Windows.Forms.Button();
      this.button6 = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(20, 114);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(139, 35);
      this.button1.TabIndex = 0;
      this.button1.Text = "Empty";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(20, 155);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(139, 35);
      this.button2.TabIndex = 1;
      this.button2.Text = "String";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(20, 196);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(139, 35);
      this.button3.TabIndex = 2;
      this.button3.Text = "Double";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(20, 237);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(139, 35);
      this.button4.TabIndex = 3;
      this.button4.Text = "Gagarin";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 18);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(356, 20);
      this.label1.TabIndex = 4;
      this.label1.Text = "Click on button to get output BSON file: test.bson";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 42);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(414, 20);
      this.label2.TabIndex = 5;
      this.label2.Text = "The use bsondump.exe to convert it to readable json. See";
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(16, 66);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(432, 20);
      this.linkLabel1.TabIndex = 6;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "http://docs.mongodb.org/v2.6/reference/program/bsondump/";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(20, 278);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(139, 135);
      this.button5.TabIndex = 7;
      this.button5.Text = "Export All";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // button6
      // 
      this.button6.Location = new System.Drawing.Point(233, 114);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(244, 51);
      this.button6.TabIndex = 8;
      this.button6.Text = "Write byte array or many bytes to stream?";
      this.button6.UseVisualStyleBackColor = true;
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(229, 170);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(51, 20);
      this.label3.TabIndex = 9;
      this.label3.Text = "label3";
      // 
      // BSONTestForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(507, 425);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.button6);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.linkLabel1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Name = "BSONTestForm";
      this.Text = "BSONTestForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.Button button5;
    private System.Windows.Forms.Button button6;
    private System.Windows.Forms.Label label3;
  }
}