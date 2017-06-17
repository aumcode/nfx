namespace WinFormsTest
{
  partial class ImagesForm
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
      this.m_ImgInitial = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.m_ImgNormalized = new System.Windows.Forms.PictureBox();
      this.label2 = new System.Windows.Forms.Label();
      this.m_Color1 = new System.Windows.Forms.PictureBox();
      this.m_Color2 = new System.Windows.Forms.PictureBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.m_Color3 = new System.Windows.Forms.PictureBox();
      this.label5 = new System.Windows.Forms.Label();
      this.m_Color4 = new System.Windows.Forms.PictureBox();
      this.button1 = new System.Windows.Forms.Button();
      this.m_TxtElapsed = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.m_ImgInitial)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ImgNormalized)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color3)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color4)).BeginInit();
      this.SuspendLayout();
      // 
      // m_ImgInitial
      // 
      this.m_ImgInitial.Location = new System.Drawing.Point(12, 41);
      this.m_ImgInitial.Name = "m_ImgInitial";
      this.m_ImgInitial.Size = new System.Drawing.Size(448, 447);
      this.m_ImgInitial.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.m_ImgInitial.TabIndex = 0;
      this.m_ImgInitial.TabStop = false;
      this.m_ImgInitial.WaitOnLoad = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(158, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(147, 17);
      this.label1.TabIndex = 1;
      this.label1.Text = "Drop or Upload Image";
      // 
      // m_ImgNormalized
      // 
      this.m_ImgNormalized.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.m_ImgNormalized.Location = new System.Drawing.Point(533, 41);
      this.m_ImgNormalized.Name = "m_ImgNormalized";
      this.m_ImgNormalized.Size = new System.Drawing.Size(142, 133);
      this.m_ImgNormalized.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.m_ImgNormalized.TabIndex = 2;
      this.m_ImgNormalized.TabStop = false;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(543, 220);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(61, 17);
      this.label2.TabIndex = 3;
      this.label2.Text = "Color #1";
      // 
      // m_Color1
      // 
      this.m_Color1.Location = new System.Drawing.Point(610, 205);
      this.m_Color1.Name = "m_Color1";
      this.m_Color1.Size = new System.Drawing.Size(47, 44);
      this.m_Color1.TabIndex = 4;
      this.m_Color1.TabStop = false;
      // 
      // m_Color2
      // 
      this.m_Color2.Location = new System.Drawing.Point(610, 253);
      this.m_Color2.Name = "m_Color2";
      this.m_Color2.Size = new System.Drawing.Size(47, 44);
      this.m_Color2.TabIndex = 6;
      this.m_Color2.TabStop = false;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(543, 268);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(61, 17);
      this.label3.TabIndex = 5;
      this.label3.Text = "Color #2";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(543, 316);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(61, 17);
      this.label4.TabIndex = 5;
      this.label4.Text = "Color #3";
      // 
      // m_Color3
      // 
      this.m_Color3.Location = new System.Drawing.Point(610, 301);
      this.m_Color3.Name = "m_Color3";
      this.m_Color3.Size = new System.Drawing.Size(47, 44);
      this.m_Color3.TabIndex = 6;
      this.m_Color3.TabStop = false;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(543, 366);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(62, 17);
      this.label5.TabIndex = 5;
      this.label5.Text = "Color #B";
      // 
      // m_Color4
      // 
      this.m_Color4.Location = new System.Drawing.Point(610, 351);
      this.m_Color4.Name = "m_Color4";
      this.m_Color4.Size = new System.Drawing.Size(47, 44);
      this.m_Color4.TabIndex = 6;
      this.m_Color4.TabStop = false;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(194, 494);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 7;
      this.button1.Text = "Upload";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // m_TxtElapsed
      // 
      this.m_TxtElapsed.AutoSize = true;
      this.m_TxtElapsed.Location = new System.Drawing.Point(573, 437);
      this.m_TxtElapsed.Name = "m_TxtElapsed";
      this.m_TxtElapsed.Size = new System.Drawing.Size(0, 17);
      this.m_TxtElapsed.TabIndex = 8;
      // 
      // ImagesForm
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(723, 584);
      this.Controls.Add(this.m_TxtElapsed);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.m_Color4);
      this.Controls.Add(this.m_Color3);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.m_Color2);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.m_Color1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.m_ImgNormalized);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.m_ImgInitial);
      this.Name = "ImagesForm";
      this.Text = "ImagesForm";
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.onImageDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ImagesForm_DragEnter);
      ((System.ComponentModel.ISupportInitialize)(this.m_ImgInitial)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_ImgNormalized)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color3)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_Color4)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox m_ImgInitial;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.PictureBox m_ImgNormalized;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.PictureBox m_Color1;
    private System.Windows.Forms.PictureBox m_Color2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.PictureBox m_Color3;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.PictureBox m_Color4;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label m_TxtElapsed;
  }
}