namespace WinFormsTest
{
  partial class GlueStressForm
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
      this.cbo = new System.Windows.Forms.ComboBox();
      this.btnStart = new System.Windows.Forms.Button();
      this.btnStop = new System.Windows.Forms.Button();
      this.cboThreads = new System.Windows.Forms.ComboBox();
      this.btnReset = new System.Windows.Forms.Button();
      this.lblTotalCalls = new System.Windows.Forms.Label();
      this.lblDuration = new System.Windows.Forms.Label();
      this.lblThroughput = new System.Windows.Forms.Label();
      this.tmr = new System.Windows.Forms.Timer(this.components);
      this.chart = new NFX.WinForms.Controls.ChartKit.Temporal.TimeSeriesChart();
      this.SuspendLayout();
      // 
      // cbo
      // 
      this.cbo.FormattingEnabled = true;
      this.cbo.Items.AddRange(new object[] {
            "mpx://sextod:5701",
            "mpx://octod:5701",
            "mpx://cube:5701",
            "mpx://127.0.0.1:5701",
            "-------------------------",
            "sync://sextod:8000",
            "sync://octod:8000",
            "sync://cube:8000",
            "sync://127.0.0.1:8000",
            ""});
      this.cbo.Location = new System.Drawing.Point(12, 12);
      this.cbo.Name = "cbo";
      this.cbo.Size = new System.Drawing.Size(228, 21);
      this.cbo.TabIndex = 5;
      // 
      // btnStart
      // 
      this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.btnStart.Location = new System.Drawing.Point(256, 10);
      this.btnStart.Name = "btnStart";
      this.btnStart.Size = new System.Drawing.Size(75, 23);
      this.btnStart.TabIndex = 6;
      this.btnStart.Text = "Start";
      this.btnStart.UseVisualStyleBackColor = false;
      this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
      // 
      // btnStop
      // 
      this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
      this.btnStop.Location = new System.Drawing.Point(337, 10);
      this.btnStop.Name = "btnStop";
      this.btnStop.Size = new System.Drawing.Size(75, 23);
      this.btnStop.TabIndex = 7;
      this.btnStop.Text = "Stop";
      this.btnStop.UseVisualStyleBackColor = false;
      this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
      // 
      // cboThreads
      // 
      this.cboThreads.FormattingEnabled = true;
      this.cboThreads.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16",
            "24",
            "32",
            "48",
            "64",
            "96",
            "128"});
      this.cboThreads.Location = new System.Drawing.Point(437, 12);
      this.cboThreads.Name = "cboThreads";
      this.cboThreads.Size = new System.Drawing.Size(55, 21);
      this.cboThreads.TabIndex = 8;
      this.cboThreads.Text = "1";
      // 
      // btnReset
      // 
      this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.btnReset.Location = new System.Drawing.Point(12, 60);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new System.Drawing.Size(75, 23);
      this.btnReset.TabIndex = 9;
      this.btnReset.Text = "Reset";
      this.btnReset.UseVisualStyleBackColor = false;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // lblTotalCalls
      // 
      this.lblTotalCalls.AutoSize = true;
      this.lblTotalCalls.Location = new System.Drawing.Point(12, 100);
      this.lblTotalCalls.Name = "lblTotalCalls";
      this.lblTotalCalls.Size = new System.Drawing.Size(61, 13);
      this.lblTotalCalls.TabIndex = 10;
      this.lblTotalCalls.Text = "Total calls: ";
      // 
      // lblDuration
      // 
      this.lblDuration.AutoSize = true;
      this.lblDuration.Location = new System.Drawing.Point(12, 125);
      this.lblDuration.Name = "lblDuration";
      this.lblDuration.Size = new System.Drawing.Size(70, 13);
      this.lblDuration.TabIndex = 11;
      this.lblDuration.Text = "Duration sec:";
      // 
      // lblThroughput
      // 
      this.lblThroughput.AutoSize = true;
      this.lblThroughput.Location = new System.Drawing.Point(12, 152);
      this.lblThroughput.Name = "lblThroughput";
      this.lblThroughput.Size = new System.Drawing.Size(111, 13);
      this.lblThroughput.TabIndex = 12;
      this.lblThroughput.Text = "Throughput calls/sec:";
      // 
      // tmr
      // 
      this.tmr.Enabled = true;
      this.tmr.Interval = 500;
      this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
      // 
      // chart
      // 
      this.chart.AllowMultiLineTitle = false;
      this.chart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.chart.AutoScroll = true;
      this.chart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(235)))), ((int)(((byte)(209)))));
      this.chart.HScrollPosition = 0;
      this.chart.ID = "";
      this.chart.Location = new System.Drawing.Point(12, 189);
      this.chart.MouseCursorMode = NFX.WinForms.Controls.ChartKit.Temporal.MouseCursorMode.Click;
      this.chart.Name = "chart";
      this.chart.ReadOnly = false;
      this.chart.Series = null;
      this.chart.ShowTimeGaps = true;
      this.chart.Size = new System.Drawing.Size(942, 467);
      this.chart.TabIndex = 13;
      this.chart.Text = "timeSeriesChart1";
      this.chart.TimeLineTickSpace = 52;
      this.chart.UseLocalTime = false;
      this.chart.VRulerDefaultFormat = null;
      this.chart.VRulerFixedWidth = 0;
      this.chart.VRulerPosition = NFX.WinForms.Controls.ChartKit.VRulerPosition.Right;
      this.chart.Zoom = 1F;
      this.chart.ZoomStepPercent = 5;
      // 
      // GlueStressForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(966, 668);
      this.Controls.Add(this.chart);
      this.Controls.Add(this.lblThroughput);
      this.Controls.Add(this.lblDuration);
      this.Controls.Add(this.lblTotalCalls);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.cboThreads);
      this.Controls.Add(this.btnStop);
      this.Controls.Add(this.btnStart);
      this.Controls.Add(this.cbo);
      this.Name = "GlueStressForm";
      this.Text = "GlueStressForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox cbo;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.ComboBox cboThreads;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Label lblTotalCalls;
    private System.Windows.Forms.Label lblDuration;
    private System.Windows.Forms.Label lblThroughput;
    private NFX.WinForms.Controls.ChartKit.Temporal.TimeSeriesChart chart;
    private System.Windows.Forms.Timer tmr;
  }
}