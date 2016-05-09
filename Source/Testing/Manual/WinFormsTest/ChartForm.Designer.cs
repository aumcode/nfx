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
      this.chkLeftRuller = new System.Windows.Forms.CheckBox();
      this.chkAutoScroll = new System.Windows.Forms.CheckBox();
      this.tbInterval = new System.Windows.Forms.TextBox();
      this.btnResetZoom = new System.Windows.Forms.Button();
      this.btnLoadSecDB = new System.Windows.Forms.Button();
      this.tbSecDBFile = new System.Windows.Forms.TextBox();
      this.btnResample = new System.Windows.Forms.Button();
      this.btnQuotes = new System.Windows.Forms.Button();
      this.tbHowMany = new System.Windows.Forms.TextBox();
      this.btnSynthCandles = new System.Windows.Forms.Button();
      this.tbSecPeriod = new System.Windows.Forms.TextBox();
      this.chart = new NFX.WinForms.Controls.ChartKit.Temporal.TimeSeriesChart();
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
      this.btnGC.Location = new System.Drawing.Point(1224, 14);
      this.btnGC.Name = "btnGC";
      this.btnGC.Size = new System.Drawing.Size(75, 23);
      this.btnGC.TabIndex = 6;
      this.btnGC.Text = "GC";
      this.btnGC.UseVisualStyleBackColor = true;
      this.btnGC.Click += new System.EventHandler(this.btnGC_Click);
      // 
      // chkLeftRuller
      // 
      this.chkLeftRuller.AutoSize = true;
      this.chkLeftRuller.Location = new System.Drawing.Point(267, 16);
      this.chkLeftRuller.Name = "chkLeftRuller";
      this.chkLeftRuller.Size = new System.Drawing.Size(74, 17);
      this.chkLeftRuller.TabIndex = 7;
      this.chkLeftRuller.Text = "Left Ruller";
      this.chkLeftRuller.UseVisualStyleBackColor = true;
      this.chkLeftRuller.CheckStateChanged += new System.EventHandler(this.chkLeftRuller_CheckStateChanged);
      // 
      // chkAutoScroll
      // 
      this.chkAutoScroll.AutoSize = true;
      this.chkAutoScroll.Checked = true;
      this.chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAutoScroll.Location = new System.Drawing.Point(347, 16);
      this.chkAutoScroll.Name = "chkAutoScroll";
      this.chkAutoScroll.Size = new System.Drawing.Size(77, 17);
      this.chkAutoScroll.TabIndex = 8;
      this.chkAutoScroll.Text = "Auto Scroll";
      this.chkAutoScroll.UseVisualStyleBackColor = true;
      this.chkAutoScroll.CheckStateChanged += new System.EventHandler(this.chkAutoScroll_CheckStateChanged);
      // 
      // tbInterval
      // 
      this.tbInterval.Location = new System.Drawing.Point(431, 16);
      this.tbInterval.Name = "tbInterval";
      this.tbInterval.Size = new System.Drawing.Size(62, 20);
      this.tbInterval.TabIndex = 9;
      this.tbInterval.Text = "1";
      // 
      // btnResetZoom
      // 
      this.btnResetZoom.Location = new System.Drawing.Point(499, 12);
      this.btnResetZoom.Name = "btnResetZoom";
      this.btnResetZoom.Size = new System.Drawing.Size(75, 23);
      this.btnResetZoom.TabIndex = 10;
      this.btnResetZoom.Text = "Reset Zoom";
      this.btnResetZoom.UseVisualStyleBackColor = true;
      this.btnResetZoom.Click += new System.EventHandler(this.btnResetZoom_Click);
      // 
      // btnLoadSecDB
      // 
      this.btnLoadSecDB.Location = new System.Drawing.Point(594, 2);
      this.btnLoadSecDB.Name = "btnLoadSecDB";
      this.btnLoadSecDB.Size = new System.Drawing.Size(116, 23);
      this.btnLoadSecDB.TabIndex = 11;
      this.btnLoadSecDB.Text = "Load SecDB";
      this.btnLoadSecDB.UseVisualStyleBackColor = true;
      this.btnLoadSecDB.Click += new System.EventHandler(this.btnLoadSecDB_Click);
      // 
      // tbSecDBFile
      // 
      this.tbSecDBFile.Location = new System.Drawing.Point(726, 2);
      this.tbSecDBFile.Name = "tbSecDBFile";
      this.tbSecDBFile.Size = new System.Drawing.Size(236, 20);
      this.tbSecDBFile.TabIndex = 12;
      this.tbSecDBFile.Text = "c:\\nfx\\1.sdb";
      // 
      // btnResample
      // 
      this.btnResample.Location = new System.Drawing.Point(968, -1);
      this.btnResample.Name = "btnResample";
      this.btnResample.Size = new System.Drawing.Size(75, 23);
      this.btnResample.TabIndex = 13;
      this.btnResample.Text = "Resample";
      this.btnResample.UseVisualStyleBackColor = true;
      this.btnResample.Click += new System.EventHandler(this.btnResample_Click);
      // 
      // btnQuotes
      // 
      this.btnQuotes.Location = new System.Drawing.Point(968, 28);
      this.btnQuotes.Name = "btnQuotes";
      this.btnQuotes.Size = new System.Drawing.Size(75, 23);
      this.btnQuotes.TabIndex = 14;
      this.btnQuotes.Text = "Quotes";
      this.btnQuotes.UseVisualStyleBackColor = true;
      this.btnQuotes.Click += new System.EventHandler(this.btnQuotes_Click);
      // 
      // tbHowMany
      // 
      this.tbHowMany.Location = new System.Drawing.Point(888, 28);
      this.tbHowMany.Name = "tbHowMany";
      this.tbHowMany.Size = new System.Drawing.Size(74, 20);
      this.tbHowMany.TabIndex = 15;
      this.tbHowMany.Text = "10";
      // 
      // btnSynthCandles
      // 
      this.btnSynthCandles.Location = new System.Drawing.Point(594, 28);
      this.btnSynthCandles.Name = "btnSynthCandles";
      this.btnSynthCandles.Size = new System.Drawing.Size(116, 23);
      this.btnSynthCandles.TabIndex = 16;
      this.btnSynthCandles.Text = "Synth Candle";
      this.btnSynthCandles.UseVisualStyleBackColor = true;
      this.btnSynthCandles.Click += new System.EventHandler(this.btnSynthCandles_Click);
      // 
      // tbSecPeriod
      // 
      this.tbSecPeriod.Location = new System.Drawing.Point(716, 31);
      this.tbSecPeriod.Name = "tbSecPeriod";
      this.tbSecPeriod.Size = new System.Drawing.Size(32, 20);
      this.tbSecPeriod.TabIndex = 17;
      this.tbSecPeriod.Text = "10";
      // 
      // chart
      // 
      this.chart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.chart.AutoScroll = true;
      this.chart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(221)))), ((int)(((byte)(201)))));
      this.chart.HScrollPosition = 0;
      this.chart.ID = "";
      this.chart.Location = new System.Drawing.Point(12, 56);
      this.chart.MouseCursorMode = NFX.WinForms.Controls.ChartKit.Temporal.MouseCursorMode.Click;
      this.chart.Name = "chart";
      this.chart.ReadOnly = false;
      this.chart.Series = null;
      this.chart.Size = new System.Drawing.Size(1391, 621);
      this.chart.TabIndex = 3;
      this.chart.Text = "chart1";
      this.chart.VRulerPosition = NFX.WinForms.Controls.ChartKit.VRulerPosition.Right;
      this.chart.VRulerFixedWidth = 48;
      this.chart.Zoom = 1F;
      this.chart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chart_MouseDown);
      // 
      // ChartForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.ClientSize = new System.Drawing.Size(1415, 689);
      this.Controls.Add(this.tbSecPeriod);
      this.Controls.Add(this.btnSynthCandles);
      this.Controls.Add(this.tbHowMany);
      this.Controls.Add(this.btnQuotes);
      this.Controls.Add(this.btnResample);
      this.Controls.Add(this.tbSecDBFile);
      this.Controls.Add(this.btnLoadSecDB);
      this.Controls.Add(this.btnResetZoom);
      this.Controls.Add(this.tbInterval);
      this.Controls.Add(this.chkAutoScroll);
      this.Controls.Add(this.chkLeftRuller);
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
    private NFX.WinForms.Controls.ChartKit.Temporal.TimeSeriesChart chart;
    private System.Windows.Forms.Button btnAnimate;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.CheckBox chkSlide;
    private System.Windows.Forms.Button btnGC;
    private System.Windows.Forms.CheckBox chkLeftRuller;
    private System.Windows.Forms.CheckBox chkAutoScroll;
    private System.Windows.Forms.TextBox tbInterval;
    private System.Windows.Forms.Button btnResetZoom;
    private System.Windows.Forms.Button btnLoadSecDB;
    private System.Windows.Forms.TextBox tbSecDBFile;
    private System.Windows.Forms.Button btnResample;
    private System.Windows.Forms.Button btnQuotes;
    private System.Windows.Forms.TextBox tbHowMany;
    private System.Windows.Forms.Button btnSynthCandles;
    private System.Windows.Forms.TextBox tbSecPeriod;
  }
}