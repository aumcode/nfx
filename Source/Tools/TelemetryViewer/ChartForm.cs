/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using NFX;

namespace TelemetryViewer
{
    public partial class ChartForm : Form
    {
        public ChartForm()
        {
            InitializeComponent();
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {
            cboStyle.SelectedIndex = 0;

             
            var area = chart.ChartAreas[0];
            
            area.AxisX.LabelStyle.Format = "HH:mm:ss";
            area.AxisX.LineColor = Color.DarkGray;
            area.AxisX.LineDashStyle = ChartDashStyle.Solid;
            area.AxisY.LineColor = Color.DarkGray;
            area.AxisY.LineDashStyle = ChartDashStyle.Solid;


            area.AxisX.MajorGrid.LineColor = Color.Silver;
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

            area.AxisY.MajorGrid.LineColor = Color.Silver;
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

            
          //  area.AxisX.Interval = 1;
          //  area.AxisX.IntervalType = DateTimeIntervalType.Auto;
          //  area.AxisX.IntervalOffset = 1;

        }

        private void ChartForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainForm.Instance.frmChart = null;
        }

        internal void ShowData(TreeNodeCollection nodes)
        {
            var lst = getSelection(nodes);
            chart.Series.Clear();

            var logarithmic = btnLogarithmicScale.Checked;
            
            var area = chart.ChartAreas[0];
            area.AxisY.IsLogarithmic = logarithmic;

            var chartType = cboStyle.Text.AsEnum<SeriesChartType>(SeriesChartType.Spline);


            foreach(var sel in lst)
            { 
                var series = chart.Series.Add( sel.m_Type.Name + "::" + sel.m_Source );
                series.ChartType = chartType;
                

                foreach(var d in sel.m_Data)
                {
                    var v = d.ValueAsObject;

                    if (logarithmic)
                    {
                        if (v is long) v = 1 +(long)v;
                        if (v is double) v = 1 +(double)v;
                    }
                    series.Points.AddXY(d.UTCTime, v );
                }
            }
        }





                    private List<MainForm.selection> getSelection(TreeNodeCollection nodes)
                    {
                        var result = new List<MainForm.selection>();
                        for(var i =0; i<nodes.Count; i++)
                        {
                            var node = nodes[i];
                            if (node.Checked && node.Tag!=null && node.Tag is MainForm.selection)
                            {
                                result.Add( (MainForm.selection)node.Tag);  
                            }
                            result.AddRange( getSelection( node.Nodes ));
                        }
                        return result;
                    }

                    

    }
}
