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
namespace TelemetryViewer
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartForm));
            this.tStrip = new System.Windows.Forms.ToolStrip();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btnLogarithmicScale = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cboStyle = new System.Windows.Forms.ToolStripComboBox();
            this.tStrip.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
            this.SuspendLayout();
            // 
            // tStrip
            // 
            this.tStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnLogarithmicScale,
            this.toolStripSeparator1,
            this.cboStyle});
            this.tStrip.Location = new System.Drawing.Point(0, 0);
            this.tStrip.Name = "tStrip";
            this.tStrip.Size = new System.Drawing.Size(749, 25);
            this.tStrip.TabIndex = 0;
            this.tStrip.Text = "toolStrip1";
            // 
            // pnlBody
            // 
            this.pnlBody.Controls.Add(this.chart);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 25);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(749, 411);
            this.pnlBody.TabIndex = 1;
            // 
            // chart
            // 
            chartArea1.InnerPlotPosition.Auto = false;
            chartArea1.InnerPlotPosition.Height = 91F;
            chartArea1.InnerPlotPosition.Width = 95F;
            chartArea1.InnerPlotPosition.X = 4F;
            chartArea1.InnerPlotPosition.Y = 2F;
            chartArea1.Name = "chartArea1";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 100F;
            chartArea1.Position.Width = 100F;
            this.chart.ChartAreas.Add(chartArea1);
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart.Legends.Add(legend1);
            this.chart.Location = new System.Drawing.Point(0, 0);
            this.chart.Name = "chart";
            this.chart.Size = new System.Drawing.Size(749, 411);
            this.chart.TabIndex = 0;
            this.chart.Text = "chart1";
            // 
            // btnLogarithmicScale
            // 
            this.btnLogarithmicScale.CheckOnClick = true;
            this.btnLogarithmicScale.Image = global::TelemetryViewer.Properties.Resources.logscale;
            this.btnLogarithmicScale.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLogarithmicScale.Name = "btnLogarithmicScale";
            this.btnLogarithmicScale.Size = new System.Drawing.Size(121, 22);
            this.btnLogarithmicScale.Text = "Logarithmic Scale";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // cboStyle
            // 
            this.cboStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStyle.Items.AddRange(new object[] {
            "Line",
            "Spline",
            "Area",
            "SplineArea",
            "Radar"});
            this.cboStyle.Name = "cboStyle";
            this.cboStyle.Size = new System.Drawing.Size(121, 25);
            // 
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 436);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.tStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChartForm";
            this.Text = "NFX Telemetry Viewer Chart";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChartForm_FormClosed);
            this.Load += new System.EventHandler(this.ChartForm_Load);
            this.tStrip.ResumeLayout(false);
            this.tStrip.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tStrip;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;
        private System.Windows.Forms.ToolStripButton btnLogarithmicScale;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox cboStyle;
    }
}