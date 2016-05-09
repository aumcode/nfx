namespace WinFormsTest
{
  partial class ChartFormDemo
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
            this.chkRealtime = new System.Windows.Forms.CheckBox();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.chkMidLines = new System.Windows.Forms.CheckBox();
            this.chkVolumes = new System.Windows.Forms.CheckBox();
            this.chkAutoScroll = new System.Windows.Forms.CheckBox();
            this.btnResetZoom = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.chkMidLevels = new System.Windows.Forms.CheckBox();
            this.cboVolumeKind = new NFX.WinForms.Controls.ComboBoxEx();
            this.cboResolution = new NFX.WinForms.Controls.ComboBoxEx();
            this.cboFile = new NFX.WinForms.Controls.ComboBoxEx();
            this.chart = new NFX.WinForms.Controls.ChartKit.Temporal.TimeSeriesChart();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkRealtime
            // 
            this.chkRealtime.AutoSize = true;
            this.chkRealtime.Checked = true;
            this.chkRealtime.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRealtime.Location = new System.Drawing.Point(204, 7);
            this.chkRealtime.Name = "chkRealtime";
            this.chkRealtime.Size = new System.Drawing.Size(67, 17);
            this.chkRealtime.TabIndex = 2;
            this.chkRealtime.Text = "Realtime";
            this.chkRealtime.UseVisualStyleBackColor = true;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 250;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // chkMidLines
            // 
            this.chkMidLines.AutoSize = true;
            this.chkMidLines.Checked = true;
            this.chkMidLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMidLines.Location = new System.Drawing.Point(455, 7);
            this.chkMidLines.Name = "chkMidLines";
            this.chkMidLines.Size = new System.Drawing.Size(71, 17);
            this.chkMidLines.TabIndex = 4;
            this.chkMidLines.Text = "Mid Lines";
            this.chkMidLines.UseVisualStyleBackColor = true;
            this.chkMidLines.CheckedChanged += new System.EventHandler(this.chkMidLines_CheckedChanged);
            // 
            // chkVolumes
            // 
            this.chkVolumes.AutoSize = true;
            this.chkVolumes.Checked = true;
            this.chkVolumes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVolumes.Location = new System.Drawing.Point(532, 7);
            this.chkVolumes.Name = "chkVolumes";
            this.chkVolumes.Size = new System.Drawing.Size(66, 17);
            this.chkVolumes.TabIndex = 5;
            this.chkVolumes.Text = "Volumes";
            this.chkVolumes.UseVisualStyleBackColor = true;
            this.chkVolumes.CheckedChanged += new System.EventHandler(this.chkVolumes_CheckedChanged);
            // 
            // chkAutoScroll
            // 
            this.chkAutoScroll.AutoSize = true;
            this.chkAutoScroll.Checked = true;
            this.chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoScroll.Location = new System.Drawing.Point(277, 7);
            this.chkAutoScroll.Name = "chkAutoScroll";
            this.chkAutoScroll.Size = new System.Drawing.Size(77, 17);
            this.chkAutoScroll.TabIndex = 6;
            this.chkAutoScroll.Text = "Auto Scroll";
            this.chkAutoScroll.UseVisualStyleBackColor = true;
            this.chkAutoScroll.CheckedChanged += new System.EventHandler(this.chkAutoScroll_CheckedChanged);
            // 
            // btnResetZoom
            // 
            this.btnResetZoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetZoom.ForeColor = System.Drawing.Color.Teal;
            this.btnResetZoom.Location = new System.Drawing.Point(698, 3);
            this.btnResetZoom.Name = "btnResetZoom";
            this.btnResetZoom.Size = new System.Drawing.Size(79, 23);
            this.btnResetZoom.TabIndex = 8;
            this.btnResetZoom.Text = "Reset Zoom";
            this.btnResetZoom.UseVisualStyleBackColor = true;
            this.btnResetZoom.Click += new System.EventHandler(this.btnResetZoom_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.White;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 557);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1216, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(39, 17);
            this.lblStatus.Text = "Status";
            // 
            // chkMidLevels
            // 
            this.chkMidLevels.AutoSize = true;
            this.chkMidLevels.Location = new System.Drawing.Point(783, 7);
            this.chkMidLevels.Name = "chkMidLevels";
            this.chkMidLevels.Size = new System.Drawing.Size(77, 17);
            this.chkMidLevels.TabIndex = 10;
            this.chkMidLevels.Text = "Mid Levels";
            this.chkMidLevels.UseVisualStyleBackColor = true;
            this.chkMidLevels.CheckedChanged += new System.EventHandler(this.chkMidLevels_CheckedChanged);
            // 
            // cboVolumeKind
            // 
            this.cboVolumeKind.BackColor = System.Drawing.Color.White;
            this.cboVolumeKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVolumeKind.FormattingEnabled = true;
            this.cboVolumeKind.HighlightColor = System.Drawing.Color.Gray;
            this.cboVolumeKind.Items.AddRange(new object[] {
            "SideBySide",
            "Centered",
            "Stacked"});
            this.cboVolumeKind.Location = new System.Drawing.Point(604, 3);
            this.cboVolumeKind.Name = "cboVolumeKind";
            this.cboVolumeKind.Size = new System.Drawing.Size(88, 21);
            this.cboVolumeKind.TabIndex = 7;
            this.cboVolumeKind.TextChanged += new System.EventHandler(this.cboVolumeKind_TextChanged);
            // 
            // cboResolution
            // 
            this.cboResolution.BackColor = System.Drawing.Color.White;
            this.cboResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboResolution.FormattingEnabled = true;
            this.cboResolution.HighlightColor = System.Drawing.Color.Gray;
            this.cboResolution.Items.AddRange(new object[] {
            "1 sec",
            "5 sec",
            "15 sec",
            "30 sec",
            "1 min",
            "5 min",
            "15 min",
            "30 min",
            "1 hr"});
            this.cboResolution.Location = new System.Drawing.Point(360, 3);
            this.cboResolution.Name = "cboResolution";
            this.cboResolution.Size = new System.Drawing.Size(75, 21);
            this.cboResolution.TabIndex = 3;
            this.cboResolution.TextChanged += new System.EventHandler(this.cboResolution_TextChanged);
            // 
            // cboFile
            // 
            this.cboFile.BackColor = System.Drawing.Color.White;
            this.cboFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFile.FormattingEnabled = true;
            this.cboFile.HighlightColor = System.Drawing.Color.White;
            this.cboFile.Location = new System.Drawing.Point(10, 3);
            this.cboFile.Name = "cboFile";
            this.cboFile.Size = new System.Drawing.Size(185, 21);
            this.cboFile.TabIndex = 1;
            this.cboFile.SelectedValueChanged += new System.EventHandler(this.cboFile_SelectedValueChanged);
            // 
            // chart
            // 
            this.chart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chart.AutoScroll = true;
            this.chart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(225)))), ((int)(((byte)(203)))));
            this.chart.HScrollPosition = 0;
            this.chart.ID = "";
            this.chart.Location = new System.Drawing.Point(0, 30);
            this.chart.MouseCursorMode = NFX.WinForms.Controls.ChartKit.Temporal.MouseCursorMode.Click;
            this.chart.Name = "chart";
            this.chart.ReadOnly = false;
            this.chart.Series = null;
            this.chart.Size = new System.Drawing.Size(1216, 524);
            this.chart.TabIndex = 0;
            this.chart.Text = "timeSeriesChart1";
            this.chart.VRulerPosition = NFX.WinForms.Controls.ChartKit.VRulerPosition.Right;
            this.chart.VRulerFixedWidth = 48;
            this.chart.Zoom = 1F;
            this.chart.ChartPaneMouseEvent += new NFX.WinForms.Controls.ChartKit.Temporal.ChartPaneMouseEventHandler(this.chart_ChartPaneMouseEvent);
            // 
            // ChartFormDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1216, 579);
            this.Controls.Add(this.chkMidLevels);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnResetZoom);
            this.Controls.Add(this.cboVolumeKind);
            this.Controls.Add(this.chkAutoScroll);
            this.Controls.Add(this.chkVolumes);
            this.Controls.Add(this.chkMidLines);
            this.Controls.Add(this.cboResolution);
            this.Controls.Add(this.chkRealtime);
            this.Controls.Add(this.cboFile);
            this.Controls.Add(this.chart);
            this.Name = "ChartFormDemo";
            this.Text = "Market View";
            this.Load += new System.EventHandler(this.ChartFormDemo_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private NFX.WinForms.Controls.ChartKit.Temporal.TimeSeriesChart chart;
    private System.Windows.Forms.CheckBox chkRealtime;
    private System.Windows.Forms.Timer tmrUpdate;
    private System.Windows.Forms.CheckBox chkMidLines;
    private System.Windows.Forms.CheckBox chkVolumes;
    private System.Windows.Forms.CheckBox chkAutoScroll;
    private System.Windows.Forms.Button btnResetZoom;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    private System.Windows.Forms.CheckBox chkMidLevels;
        private NFX.WinForms.Controls.ComboBoxEx cboFile;
        private NFX.WinForms.Controls.ComboBoxEx cboResolution;
        private NFX.WinForms.Controls.ComboBoxEx cboVolumeKind;
    }
}