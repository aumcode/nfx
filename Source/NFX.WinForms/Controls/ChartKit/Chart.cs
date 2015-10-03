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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;


using NFX.Environment;
using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.ChartKit
{
  /// <summary>
  /// Implements a Chart control base. 
  /// </summary>
  public class Chart : Control, IConfigurable, IConfigurationPersistent
  {
     #region CONSTS
       public const string CONFIG_CHART_SECTION = "chart";
       public const string CONFIG_PANE_SECTION = "pane";
       public const string CONFIG_ID_ATTR = "id";
       
       public const int MIN_ROW_HEIGHT = 1;
       public const int DEFAULT_ROW_HEIGHT = 22;
       
       public const int REBUILD_TIMEOUT_MS = 100;
     #endregion
  
  
     #region .ctor

       public Chart() : base ()
       {
         //SetStyle(ControlStyles.OptimizedDoubleBuffer, true); //the PlotPane is already dbl buffered
         SetStyle(ControlStyles.UserPaint, true);
         SetStyle(ControlStyles.AllPaintingInWmPaint, true); //no ERASE BACKGROUND
         
         SetStyle(ControlStyles.UserMouse, true);

         
         UpdateStyles();
         
         m_Style = new Style(this, null);
         m_RulerStyle = new Style(this, null);
         m_PlotStyle = new Style(this, null);
         
         BuildDefaultStyle(m_Style);
         BuildDefaultRulerStyle(m_RulerStyle);
         BuildDefaultPlotStyle(m_PlotStyle);
         
         
         m_ControllerNotificationTimer = new Timer();
         m_ControllerNotificationTimer.Interval = REBUILD_TIMEOUT_MS;
         m_ControllerNotificationTimer.Enabled = false;
         m_ControllerNotificationTimer.Tick += new EventHandler((o, e) =>
                                                       {
                          //todo ETO NUJNO DLYA DRAG/DROP   if (m_RepositioningColumn!=null || m_ResizingColumn!=null ) return;//while grid is being manipulated dont flush timer just yet
                                                       
                                                        m_ControllerNotificationTimer.Stop(); 
                                                        notifyAndRebuildChart();
                                                       });
       }

       protected override void Dispose(bool disposing)
       {
         base.Dispose(disposing);
         m_Disposed = true;
       }

       

    #endregion


    #region Private Fields
  
      private bool m_Disposed;
  
      private string m_ID;
      
      
      private OrderedRegistry<PlotPane> m_Panes = new OrderedRegistry<PlotPane>();
      
      
      private bool m_ReadOnly;
      
      private Style m_Style;
      private Style m_RulerStyle;
      private Style m_PlotStyle;
      
      private Series m_Series;
      
      private VRulerPosition m_VRulerPosition;

      private Timer m_ControllerNotificationTimer;
  
      private int m_BatchChangeCounter;
      private bool m_BatchChangeNeedsRebuild;
      
  
    #endregion


    #region Properties
    
    
      /// <summary>
      /// Specifies ID for this chart instance. IDs are needed for chart config persistence
      /// </summary>
      public string ID
      {
        get { return m_ID ?? string.Empty; }
        set { m_ID = value;}
      }
    
    
      /// <summary>
      /// Determines whether chart data can be modified (if chart supports it)
      /// </summary>
      public bool ReadOnly
      {
        get { return m_ReadOnly; }
        set { m_ReadOnly = value;}
      }
      
      
      /// <summary>
      /// Returns a default style object for all elements in the chart 
      /// </summary>
      [Browsable(false)]
      public Style Style
      {
        get { return m_Style; }
      }
      
      /// <summary>
      /// Returns the default style object for all rulers in the chart
      /// </summary>
      [Browsable(false)]
      public Style RulerStyle
      {
        get { return m_RulerStyle; }
      }
      
      
      /// <summary>
      /// Returns the style object for default plots 
      /// </summary>
      [Browsable(false)]
      public Style PlotStyleStyle
      {
        get { return m_PlotStyle; }
      }
      


      public VRulerPosition VRulerPosition
      {
        get { return m_VRulerPosition;}
        set { m_VRulerPosition = value;}
      }

      /// <summary>
      /// Series that this chart shows 
      /// </summary>
      [Browsable(false)]
      public Series Series
      {
        get { return m_Series; }
        set
        {
          m_Series = value;
          NotifySeriesChange();
        }
      }

      public IOrderedRegistry<PlotPane> Panes{ get{ return m_Panes;}}
    #endregion 
    
    
    #region Events
    
     ///// <summary>
     ///// Occurs after columns attributes changed such as visibility,width, titile etc..
     ///// </summary>
     //public event ColumnAttributesChangedEventHandler ColumnAttributesChanged;
     
     ///// <summary>
     ///// Occurs after columns sort changed 
     ///// </summary>
     //public event ColumnAttributesChangedEventHandler ColumnSortChanged;
     
     
     ///// <summary>
     ///// Occurs when external data source changes and notifies the grid
     ///// </summary>
     //public event NotifyDataSourceChangedEventHandler NotifyDataSourceChanged;
     
     ///// <summary>
     ///// Occurs when user selects the cell
     ///// </summary>
     //public event CellSelectionEventHandler CellSelection;
     
    #endregion
    
    
    #region Public Methods
    
        
      /// <summary>
      /// Notifies chart series that data/metadata has changed in series model/view properties 
      /// This method is NOT THREAD SAFE and runs in GUI thread.
      /// This method does call aggregation, that is - when this method is called many times within timespan it will rebuild chart only once at the end.
      /// </summary>
      public bool NotifySeriesChange()
      {
          m_ControllerNotificationTimer.Start();
          return true;
      }
      
    
      // IConfigurable Members
      /// <summary>
      /// Reads column configuration from config node that should contain [grid] sub elemnt/s. The id attribute should either match or be absent
      /// </summary>
      public void Configure(IConfigSectionNode node) //should point to node that has grids
      {
         //if (node==null) return;
         
         //node = findSubNodeForThisGrid(node);
         
         //if (node==null || !node.Exists) return;
         
         //BeginBatchChange();
         //try
         //{
         //    var idx = 0;
         //    foreach (ConfigSectionNode cnode in 
         //             node.Children.Where(n => string.Equals(n.Name, CONFIG_COLUMN_SECTION, StringComparison.OrdinalIgnoreCase)))
         //    {
         //       var col = TryFindColumnByID(cnode.AttrByName(CONFIG_ID_ATTR).Value);
         //       if (col!=null)
         //       {
         //         col.Configure(cnode);
         //         m_Columns.Remove(col);
         //         m_Columns.Insert(idx, col);
         //         idx++;
         //       }  
         //    }
             
         //   columnsChanged();
         //}
         //finally
         //{
         //  EndBatchChange();
         //}
      }

      // IConfigurationPersistent Members
      /// <summary>
      /// Persists column configuration to config node. [grid] subnode will be created under specified node pr reused if one already exists
      /// </summary>
      public void PersistConfiguration(ConfigSectionNode node)
      {
       //  if (node==null) return;
         
       //  ConfigSectionNode gnode = findSubNodeForThisGrid(node) as ConfigSectionNode; //see if node for this grid already exists

       //  if (gnode!=null)//delete with all column defs that are different now
       //   gnode.Delete();
         
         
       //  gnode = node.AddChildNode(CONFIG_GRID_SECTION);
         
       //  if (!string.IsNullOrEmpty(m_ID))
       //    gnode.AddAttributeNode(CONFIG_ID_ATTR, ID);
         
       //  foreach(var col in m_Columns)
       //   col.PersistConfiguration(gnode);
      }
    
    
      /// <summary>
      /// Builds default style for data cells in non-selected rows
      /// </summary>
      public virtual void BuildDefaultStyle(Style style)
      {
      }
      
      /// <summary>
      /// Builds default style for rulers
      /// </summary>
      public virtual void BuildDefaultRulerStyle(Style style)
      {
         style.HAlignment = HAlignment.Center;
         style.BorderLeft = new LineStyle(){ Color = Color.White };
         style.BorderTop = new LineStyle(){ Color = Color.White };
         style.BorderBottom = new LineStyle(){ Color = Color.FromArgb(100,100,100) };
         style.BGKind = BGKind.HorizontalGradient;
         style.BGColor = Color.White;
         style.BGColor2 = Color.Silver;
      }
      
      /// <summary>
      /// Builds default style for data plots
      /// </summary>
      public virtual void BuildDefaultPlotStyle(Style style)
      {
       //  style.HAlignment = HAlignment.Center;
         style.BorderTop = new LineStyle(){ Color = Color.Transparent };
         style.BorderBottom = new LineStyle(){ Color = Color.Silver};
         style.BGKind = BGKind.BackwardDiagonalGradient;//.VerticalGradient;
         style.BGColor = Color.FromArgb(199,199,255);
         style.BGColor2 = Color.FromArgb(250,250,255);
      }
      
      
      
      /// <summary>
      /// Marks the beginning of the series of changes that are going to be performed on the grid so
      ///  the grid does not have to be rebuild unnecessarily many times.
      /// The complementary method is EndBatchChange() that does rebuild the grid IF it is needed.
      /// These methods respect call nesting, so only the last call to EndBatchChange() would result in rebuild
      /// </summary>
      public void BeginBatchChange()
      {
        m_BatchChangeCounter++;
      }
      
      /// <summary>
      /// Complementary method for BeginBatchChange()
      /// </summary>
      public void EndBatchChange()
      {
        if (m_BatchChangeCounter>0) m_BatchChangeCounter--;
        
        if (m_BatchChangeCounter==0 && m_BatchChangeNeedsRebuild)
        {
          rebuildChart();
        }
      }

    #endregion
    
    
    #region Protected
    
       
       
       
       /// <summary>
       /// Performs layout of sub controls, such as element host, scroll bars etc.. 
       /// </summary>
       protected virtual void LayoutSubControls()
       {

       }
       
       protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
       {
         base.SetBoundsCore(x, y, width, height, specified);
         LayoutSubControls();
       }
       
       protected override void OnPaddingChanged(EventArgs e)
       {
         base.OnPaddingChanged(e);
         LayoutSubControls();
       }
  
  
       
       //protected virtual void OnSeriesChanged(EventArgs e)
       //{
       //  if (SeriesChanged!=null) SeriesChanged(this, e);
       //}
       
       //protected virtual void OnColumnAttributesChanged(Column column)
       //{
       //  if (ColumnAttributesChanged!=null) ColumnAttributesChanged(column);
       //}
       
       //protected virtual void OnColumnSortChanged(Column column)
       //{
       //  if (ColumnSortChanged!=null) ColumnSortChanged(column);
       //}

       //protected virtual void OnNotifyDataSourceChanged(NotifyDataSourceChangedEventArgs e)
       //{
       // if (NotifyDataSourceChanged!=null) NotifyDataSourceChanged(this, e);
       //}

           
       ///// <summary>
       ///// Invokes cell selection  event
       ///// </summary>
       //protected virtual void OnCellSelection(CellElement oldCell, CellElement newCell)
       //{
       //  if (CellSelection!=null) CellSelection(oldCell, newCell);
       //}    
           
              


       //protected override void OnMouseWheel(MouseEventArgs e)
       //{
       //  var alt = (Control.ModifierKeys&Keys.Alt) >0;
       //  var ctrl = (Control.ModifierKeys&Keys.Control) >0;
       //  var shift = (Control.ModifierKeys&Keys.Shift) >0;
        
        
       //  if (ctrl)//zoom
       //  {
       //    m_CellView.UpdateZoom( m_CellView.Zoom + Math.Sign(e.Delta)* 0.1f);
       //    rebuildAllCells();
       //  }
       //  else  if (shift)//stretch data rows
       //  {
       //    DataRowHeight -= Math.Sign(e.Delta);
       //    rebuildAllCells(); 
       //  }
       //   else //scroll
       //  {
       //    var v = m_VScrollBar.Value;
                                     
       //    v -= Math.Sign(e.Delta)* (alt ? 1 : m_RowMap.Count -1);
           
           
       //    if (v>m_VScrollBar.Maximum-2)
       //     v = m_VScrollBar.Maximum-2; 

       //    if (v<m_VScrollBar.Minimum)
       //     v = m_VScrollBar.Minimum;
             
       //    m_VScrollBar.Value = v;  
       //  }                                   
       //  base.OnMouseWheel(e);
       //}


       ////protected override bool ProcessKeyPreview(ref Message m)
       ////{
       ////  if (m.Msg == (int)NFX.WinApi.Messages.WM_KEYDOWN)
       ////  {
       ////   var e = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | ModifierKeys);
          
          
          
       ////   if (e.KeyCode==Keys.Up)
       ////   {
       ////     MessageBox.Show("Up pressed and eaten");
       ////     return true;
       ////   }else if (e.KeyCode==Keys.Down)
       ////   {
       ////     MessageBox.Show("Down pressed and eaten");
       ////     return true;
       ////   }          
          
       ////  }
       ////  return base.ProcessKeyPreview(ref m); 
       ////}
                      
       //protected override bool ProcessDialogKey(Keys keyData)
       //{
       //  if (keyData==(Keys.C | Keys.Control))
       //  {
       //   CopyToClipboard();
       //   return true;
       //  }
         
       //  if (keyData==Keys.Up)
       //  {
       //     if (m_VScrollBar.Value>0)
       //       m_VScrollBar.Value-=1;
       //     return true;
       //  }
         
       //  if (keyData==Keys.Down)
       //  {
       //     if (m_VScrollBar.Value<m_VScrollBar.Maximum)
       //      m_VScrollBar.Value+=1;
       //     return true;
       //  }
         
       //  if (keyData==Keys.PageUp)
       //  {
       //     var v = m_VScrollBar.Value - m_RowMap.Count;
       //     if (v<0) v = 0;
       //     m_VScrollBar.Value = v;
       //     return true;
       //  }
         
       //  if (keyData==Keys.PageDown)
       //  {
       //     if (m_VScrollBar.Maximum>2)
       //     {
       //       var v = m_VScrollBar.Value + m_RowMap.Count;
       //       if (v>m_VScrollBar.Maximum-1) v = m_VScrollBar.Maximum-1;
       //       m_VScrollBar.Value = v;
       //     }
       //     return true;
       //  }
         
       //  if (keyData==Keys.Left)
       //  {
       //     if (m_HScrollBar.Value>0)
       //       m_HScrollBar.Value-=1;
       //     return true;
       //  }
         
       //  if (keyData==Keys.Right)
       //  {
       //     if (m_HScrollBar.Value<m_HScrollBar.Maximum)
       //      m_HScrollBar.Value+=1;
       //     return true;
       //  }
         
       //  if (keyData==Keys.Home)
       //  {
       //     m_VScrollBar.Value=0;
       //     return true;
       //  }
         
       //  if (keyData==Keys.End)
       //  {
       //     if (m_VScrollBar.Maximum>2)
       //     {
       //       m_VScrollBar.Value = m_VScrollBar.Maximum - 2;
       //     } 
       //     return true;
       //  }
         
         
       //  return base.ProcessDialogKey(keyData);
       //}
       

       

    #endregion
    
    #region .pvt  .impl
       

        private void notifyAndRebuildChart()
       {
         BeginBatchChange();
         try
         {
         //  updateScrollBars();
           rebuildChart();
         //  OnNotifySeriesChanged( new NotifySeriesChangedEventArgs(row));
         }
         finally
         {
           EndBatchChange();
         }    
       }

       private void rebuildChart(bool force = false)//true to force rebuild inspite of Begin/End Change 
       {
         if (!force && m_BatchChangeCounter>0)
         {
           m_BatchChangeNeedsRebuild = true;
           return;
         }
         
         if (m_Disposed) return;

         try
         {
            if (m_Disposed) return;

            buildPanes();

            if (m_Series==null) return;

            //the maximum width of 1 sample - taken from all views
            var sampleWidth = m_Series.GetMaxSampleWidth();
            

            m_Series.BuildViews(this);
         }
         finally
         {
            if (!force)
             m_BatchChangeNeedsRebuild = false;
         }
       }
       
      
       private void buildPanes()
       {
         if (m_Series==null)
         {
           foreach(var ctl in m_Panes) ctl.Dispose();
           m_Panes.Clear();
           //todo ubit shkalu X vnizy
           return;
         }

         var paneNames = m_Series.GetPaneNames().ToArray();

         //1. Delete panes that are no longer in series
         var toDelete = m_Panes.Select(p => p.Name).Except(paneNames);
         foreach(var pn in toDelete) 
         {
           var ctl = m_Panes[pn];
           if (ctl!=null)
           {
             m_Panes.Unregister(ctl);
             ctl.Dispose();
           }
         }

         //2. Add panes
         foreach(var pn in paneNames)
         {
           var ctl = m_Panes[pn];
           if (ctl!=null)
           {
             ctl.DeleteAllElements();//Clear the pane
             continue;//pane already exists
           }
           //Add pane
           ctl = new PlotPane(this, pn, m_Panes.Count+1);
           ctl.TabStop = false;
           ctl.Height = 100;//todo CONFIG
           ctl.Parent = this;
           ctl.Dock = DockStyle.Top;
           //ctl.BackColor = Color.HotPink;
           m_Panes.Register(ctl);
         }


         //3. Build splitters
         var ilast = m_Panes.Count()-1;
         for(var i=0; i<=ilast; i++)
         {
           var pane = m_Panes[i];
           
     //      if (i==ilast)
     //      {
     //       if (pane.m_Splitter!=null)
     //       {
     //         pane.m_Splitter.Dispose();
     //         pane.m_Splitter = null;
     //       }
     //    //   this.Controls.SetChildIndex( pane, 1000); 
     ////       pane.Dock = DockStyle.Left;
     //      }
     //      else
           {
             pane.Dock = DockStyle.Top;
             if (pane.m_Splitter==null)
             { 
               pane.m_Splitter = new Splitter();
               pane.m_Splitter.Parent = this;
               this.Controls.SetChildIndex( pane.m_Splitter, this.Controls.GetChildIndex(pane)); 
               pane.m_Splitter.Dock = DockStyle.Top;
               pane.m_Splitter.Height = 6;
               pane.m_Splitter.BackColor = Color.Silver;
               pane.m_Splitter.BorderStyle = BorderStyle.FixedSingle;
               //todo Splitter moving - suspend update?
               pane.m_Splitter.SplitterMoved += delegate(object sender, SplitterEventArgs e)
               {
                 this.NotifySeriesChange();
               };
             }
           }
         }
       }

    
       
       
       
       private void buildElements()
       {   /*
         float zoom = m_CellView.Zoom;
         
         var y = 0;
         var rowHeight = GetRowHeight(null);
         
         m_RowMap.Clear();
         m_RowMap.Add( new RowMapEntry(){ Row = null, Height = rowHeight, Top = y} );        
         
         for(int i=m_HScrollBar.Value,x=0; i<m_Columns.Count; i++)
         {
             var col = m_Columns[i];
             if (!col.Visible) continue;
             var celm = col.MakeCellElementInstance(null); //header
             celm.Region = new Rectangle(x, y, col.Width, rowHeight);
             celm.Visible = true;
             x+=col.Width+1;
         }
         
         y += rowHeight+1; 
                  
         
         if (m_DataRowSource==null) return;
         var idx = m_VScrollBar.Value;

         while( idx < m_DataRowSource.Count && (int)(y*zoom)<m_CellView.Height)
         {
           var row = m_DataRowSource[idx];
           rowHeight = GetRowHeight(row);
           m_RowMap.Add( new RowMapEntry(){ Row = row, Height = rowHeight, Top = y} ); 
           
           for(int i=m_HScrollBar.Value,x=0; i<m_Columns.Count; i++)
           {
             var col = m_Columns[i];
             if (!col.Visible) continue;
             
             var celm = col.MakeCellElementInstance(row);
             celm.Region = new Rectangle(x, y, col.Width, rowHeight);
             celm.Visible = true;
             x+=col.Width+1;
             
             //var cbe = new CheckBoxElement(m_CellView);
             //cbe.Region = new Rectangle(x,y, 12,12);
             //cbe.Visible = true;
             //cbe.ZOrder = 10;
             //cbe.Checked = true;
           } 
           idx++;
           y+= rowHeight+1;
         }
             */
       }
    
    #endregion
  }
  
  
  
 
  
  
}
