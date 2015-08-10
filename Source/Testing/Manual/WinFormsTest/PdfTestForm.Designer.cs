namespace WinFormsTest
{
    partial class PdfTestForm
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
      this.fontDialog1 = new System.Windows.Forms.FontDialog();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.button5 = new System.Windows.Forms.Button();
      this.button6 = new System.Windows.Forms.Button();
      this.button7 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(12, 12);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(161, 35);
      this.button1.TabIndex = 0;
      this.button1.Text = "Primitives test";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(12, 53);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(161, 35);
      this.button2.TabIndex = 0;
      this.button2.Text = "Image test";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(179, 58);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(324, 26);
      this.textBox1.TabIndex = 1;
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(509, 55);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(39, 29);
      this.button3.TabIndex = 2;
      this.button3.Text = "...";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(13, 95);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(160, 36);
      this.button4.TabIndex = 3;
      this.button4.Text = "Page sizes test";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(13, 138);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(160, 34);
      this.button5.TabIndex = 4;
      this.button5.Text = "User units test";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // button6
      // 
      this.button6.Location = new System.Drawing.Point(13, 179);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(160, 33);
      this.button6.TabIndex = 5;
      this.button6.Text = "Print test";
      this.button6.UseVisualStyleBackColor = true;
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // button7
      // 
      this.button7.Location = new System.Drawing.Point(13, 219);
      this.button7.Name = "button7";
      this.button7.Size = new System.Drawing.Size(160, 32);
      this.button7.TabIndex = 6;
      this.button7.Text = "Fonts test";
      this.button7.UseVisualStyleBackColor = true;
      this.button7.Click += new System.EventHandler(this.button7_Click);
      // 
      // PdfTestForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(589, 322);
      this.Controls.Add(this.button7);
      this.Controls.Add(this.button6);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Name = "PdfTestForm";
      this.Text = "PdfTestForm";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.FontDialog fontDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
    private System.Windows.Forms.Button button6;
    private System.Windows.Forms.Button button7;
  }
}