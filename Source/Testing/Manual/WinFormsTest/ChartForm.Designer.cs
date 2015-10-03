namespace WinFormsTest
{
  partial class ChartForm
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
      this.btnConnect = new System.Windows.Forms.Button();
      this.btnAnimate = new System.Windows.Forms.Button();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.chkSlide = new System.Windows.Forms.CheckBox();
      this.btnGC = new System.Windows.Forms.Button();
      this.chart = new NFX.WinForms.Controls.ChartKit.Chart();
      this.SuspendLayout();
      // 
      // btnConnect
      // 
      this.btnConnect.Location = new System.Drawing.Point(12, 12);
      this.btnConnect.Name = "btnConnect";
      this.btnConnect.Size = new System.Drawing.Size(75, 23);
      this.btnConnect.TabIndex = 1;
      this.btnConnect.Text = "Connect";
      this.btnConnect.UseVisualStyleBackColor = true;
      this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
      // 
      // btnAnimate
      // 
      this.btnAnimate.Location = new System.Drawing.Point(125, 12);
      this.btnAnimate.Name = "btnAnimate";
      this.btnAnimate.Size = new System.Drawing.Size(75, 23);
      this.btnAnimate.TabIndex = 4;
      this.btnAnimate.Text = "Animate";
      this.btnAnimate.UseVisualStyleBackColor = true;
      this.btnAnimate.Click += new System.EventHandler(this.btnAnimate_Click);
      // 
      // timer
      // 
      this.timer.Interval = 250;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // chkSlide
      // 
      this.chkSlide.AutoSize = true;
      this.chkSlide.Location = new System.Drawing.Point(212, 16);
      this.chkSlide.Name = "chkSlide";
      this.chkSlide.Size = new System.Drawing.Size(49, 17);
      this.chkSlide.TabIndex = 5;
      this.chkSlide.Text = "Slide";
      this.chkSlide.UseVisualStyleBackColor = true;
      // 
      // btnGC
      // 
      this.btnGC.Location = new System.Drawing.Point(1131, 16);
      this.btnGC.Name = "btnGC";
      this.btnGC.Size = new System.Drawing.Size(75, 23);
      this.btnGC.TabIndex = 6;
      this.btnGC.Text = "GC";
      this.btnGC.UseVisualStyleBackColor = true;
      this.btnGC.Click += new System.EventHandler(this.btnGC_Click);
      // 
      // chart
      // 
      this.chart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.chart.BackColor = System.Drawing.SystemColors.Window;
      this.chart.ID = "";
      this.chart.Location = new System.Drawing.Point(12, 56);
      this.chart.Name = "chart";
      this.chart.ReadOnly = false;
      this.chart.Series = null;
      this.chart.Size = new System.Drawing.Size(1391, 621);
      this.chart.TabIndex = 3;
      this.chart.Text = "chart1";
      this.chart.VRulerPosition = NFX.WinForms.Controls.ChartKit.VRulerPosition.Right;
      // 
      // ChartForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1415, 689);
      this.Controls.Add(this.btnGC);
      this.Controls.Add(this.chkSlide);
      this.Controls.Add(this.btnAnimate);
      this.Controls.Add(this.chart);
      this.Controls.Add(this.btnConnect);
      this.Name = "ChartForm";
      this.Text = "ChartForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnConnect;
    private NFX.WinForms.Controls.ChartKit.Chart chart;
    private System.Windows.Forms.Button btnAnimate;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.CheckBox chkSlide;
    private System.Windows.Forms.Button btnGC;
  }
}