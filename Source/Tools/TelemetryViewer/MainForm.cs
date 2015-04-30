/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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

using NFX;
using NFX.Instrumentation;

namespace TelemetryViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        internal ChartForm frmChart;
        internal static MainForm Instance;

        private void MainForm_Load(object sender, EventArgs e)
        {
            Instance = this;
            buildTree();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            buildTree();
        }

        private void tmrGraph_Tick(object sender, EventArgs e)
        {
           if (frmChart!=null)
             frmChart.ShowData(tv.Nodes);
        }

        public const int MAX_SITE_TRAFFIC_PAUSE_SEC = 10;

        public const int I_WORLD = 0;
        public const int I_WORLD_ERR = 1;
        public const int I_SITE = 3;
        public const int I_SITE_ERR = 4;
        public const int I_FOLDER = 2;
        public const int I_DATUM = 5;
        public const int I_DATUM_ERR = 7;
        public const int I_VALUE = 8;

          class NSComparer : IEqualityComparer<Type>
          {
              private static NSComparer s_Instance = new NSComparer();

              public static NSComparer Instance { get{ return s_Instance;}}

              private NSComparer(){}

              public bool Equals(Type x, Type y)
              {
                  return x.Namespace == y.Namespace;
              }

              public int GetHashCode(Type obj)
              {
                  return obj.Namespace.GetHashCode();
              }
          }


        public class selection
        {
            public SiteData m_Site;
            public Type m_Type;
            public string m_Source;
            public List<Datum> m_Data;
        }



        private void buildTree()
        {
            var now = App.TimeSource.Now;  
            
            var root =  tv.Nodes.Count>0 ? tv.Nodes[0] : tv.Nodes.Add("Telemetry Sites");
            root.ImageIndex = I_WORLD;
            root.SelectedImageIndex = I_WORLD;            
                                                       
                 
            foreach(var site in Receiver.Sites.OrderBy(s => s.Name))//SITES
            {
                var nsite = root.Nodes[site.Name];
                if (nsite==null) nsite = putNode(root.Nodes, site.Name, site.Name);
               
                var siteError = (now-site.LastTraffic).TotalSeconds > MAX_SITE_TRAFFIC_PAUSE_SEC;

                if (siteError)
                {
                    nsite.ImageIndex = I_SITE_ERR;
                    nsite.SelectedImageIndex = I_SITE_ERR;

                    root.ImageIndex = I_WORLD_ERR;
                    root.SelectedImageIndex = I_WORLD_ERR;
                }
                else
                {
                    nsite.ImageIndex = I_SITE;
                    nsite.SelectedImageIndex = I_SITE;
                }
                var types = site.DataTypes;

                foreach(var ns in types.Distinct(NSComparer.Instance).OrderBy(ns => ns.Namespace))//NAMESPACES
                {
                    var nns =  nsite.Nodes[ns.Namespace];
                    if (nns==null) nns = putNode(nsite.Nodes,  ns.Namespace, ns.Namespace);    
                    nns.ImageIndex = I_FOLDER;
                    nns.SelectedImageIndex = I_FOLDER;
                    foreach(var tp in types.Where(tp=>tp.Namespace==ns.Namespace).OrderBy(tp=> tp.Name))//TYPES 
                    {
                      var ntp = nns.Nodes[tp.Name];
                      if (ntp==null) ntp = putNode(nns.Nodes, tp.Name, tp.Name);    
                      ntp.ImageIndex = siteError? I_DATUM_ERR : I_DATUM;
                      ntp.SelectedImageIndex = siteError? I_DATUM_ERR : I_DATUM;
                      
                      var first = true;
                      foreach(var src in site[tp]) //SOURCES
                      {
                              var nsrc = ntp.Nodes[src];
                              if (nsrc==null) nsrc = putNode(ntp.Nodes, src, src);
                              nsrc.ImageIndex = I_VALUE;
                              nsrc.SelectedImageIndex = I_VALUE;
                              
                              var data = site[tp, src];
                              nsrc.Tag = new selection
                               {
                                m_Site = site, m_Type = tp, m_Source = src, m_Data = data
                               };
                              var lastDatum = data.LastOrDefault(d => d!=null);
                              if (lastDatum!=null)
                              {                         
                                var txt = "{0} = {1} {2} {3}".Args(src, lastDatum.ValueAsObject, lastDatum.ValueUnitName, lastDatum.Count);
                                if (lastDatum.Rate.IsNotNullOrWhiteSpace()) txt += " @ {0}".Args(lastDatum.Rate);
                                nsrc.Text = txt;
                                if (first)
                                {
                                    first = false;
                                    ntp.Text = "{0} - {1}".Args(tp.Name, lastDatum.Description);
                                }
                              }
                      }
                    }
                }
            }
           root.Expand();
        }

       


        private TreeNode putNode(TreeNodeCollection col, string key, string text)
        {
            for(int i=0; i<col.Count; i++)
            {
                var cmp = string.Compare(col[i].Text, text);
                if (cmp<0) continue;
                return col.Insert(i, key, text);
            }
            return col.Add(key, text);
        }


        private void btnViewChart_Click(object sender, EventArgs e)
        {
           if (frmChart==null)
             frmChart = new ChartForm(); 
           
           frmChart.Show();

           if (frmChart.WindowState== FormWindowState.Minimized)
            frmChart.WindowState = FormWindowState.Normal;
        }

        

        


    }
}
