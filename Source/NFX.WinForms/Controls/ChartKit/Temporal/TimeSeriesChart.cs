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
using NFX.Financial.Market;
using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{
  /// <summary>
  /// Implements a Chart control base. 
  /// </summary>
  public class TimeSeriesChart : Control, IConfigurable, IConfigurationPersistent
  {
     #region CONSTS
       public const int DEFAULT_VRULER_WIDTH = 48;
       public const int SPLITTER_HEIGHT = 5;


       public const string CONFIG_CHART_SECTION = "chart";
       public const string CONFIG_PANE_SECTION = "pane";
       public const string CONFIG_ID_ATTR = "id";
       
       public const int MIN_PANE_HEIGHT = 8;
       public const float DEFAULT_PANE_VPROPORTION= 0.25f;
       
       public const int REBUILD_TIMEOUT_MS = 100;
     #endregion
  
  
     #region .ctor

       public TimeSeriesChart() : base ()
       {
         //SetStyle(ControlStyles.OptimizedDoubleBuffer, true); //the PlotPane is already dbl buffered
         SetStyle(ControlStyles.UserPaint, true);
         SetStyle(ControlStyles.AllPaintingInWmPaint, true); //no ERASE BACKGROUND
         
         SetStyle(ControlStyles.UserMouse, true);

         
         UpdateStyles();
         

         m_TimeScalePane = new TimeScalePane(this);
         m_TimeScalePane.Parent = this;
         m_TimeScalePane.Height = 25;
         m_TimeScalePane.Dock = DockStyle.Bottom;
         m_TimeScalePane.BackColor = Color.Black;

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
                                                        rebuildChart();
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
      
      private float m_Zoom = 1.0f;
      private OrderedRegistry<PlotPane> m_Panes = new OrderedRegistry<PlotPane>();
      
      
      private bool m_ReadOnly;
      
      private Style m_Style;
      private Style m_RulerStyle;
      private Style m_PlotStyle;
      
      private TimeSeries m_Series;
      
      private VRulerPosition m_VRulerPosition;
      private int m_VRulerWidth = DEFAULT_VRULER_WIDTH;

      internal TimeScalePane m_TimeScalePane;

      private Timer m_ControllerNotificationTimer;
      private bool m_NeedRebuildPanes;
  
      private bool m_AutoScroll = true;
      private int m_HScrollPosition;

      private MouseCursorMode m_MouseCursorMode;

      protected Dictionary<string, float> m_PaneVProportions = new Dictionary<string,float>(StringComparer.InvariantCultureIgnoreCase);
  
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
      public Style PlotStyle
      {
        get { return m_PlotStyle; }
      }
      


      public VRulerPosition VRulerPosition
      {
        get { return m_VRulerPosition;}
        set { m_VRulerPosition = value;}
      }

      public int VRulerWidth
      {
        get { return m_VRulerWidth;}
        set { m_VRulerWidth = value < 0 ? 0 : value;}
      }

      /// <summary>
      /// Series that this chart shows 
      /// </summary>
      [Browsable(false)]
      public TimeSeries Series
      {
        get { return m_Series; }
        set
        {
          m_Series = value;
          m_HScrollPosition = 0;
          NotifySeriesChange(true);
        }
      }


      /// <summary>
      /// Horizontal scroll position in samples
      /// </summary>
      public int HScrollPosition
      {
        get { return m_HScrollPosition;}
        set
        {
          if (value<0) value = 0;
          if (m_HScrollPosition==value) return;

          m_HScrollPosition = value;
          NotifySeriesChange();
        }
      }


      /// <summary>
      /// When true, auto scrolls to the end of time series
      /// </summary>
      public bool AutoScroll
      {
        get { return m_AutoScroll; }
        set { m_AutoScroll = value;}
      }


      public float Zoom
      {
        get{ return m_Zoom;}
        set
        {
          if (value<=ElementHostControl.MIN_ZOOM) value = ElementHostControl.MIN_ZOOM;
          if (value==m_Zoom) return;
          
          m_Zoom = value;
          this.rebuildChart();
        }
      }


      public MouseCursorMode MouseCursorMode
      {
        get { return m_MouseCursorMode;}
        set { m_MouseCursorMode = value;}
      }

      /// <summary>
      /// Returns the number of samples that can fit across the cahrt
      /// </summary>
      public int SampleFitAcrossCount
      {
        get
        {
          if (m_Series==null) return 0;
          var maxSampleWidth = m_Series.GetMaxSampleWidth();
          var width = Width - VRulerWidth;
          var canFit = (int)(width / ((maxSampleWidth+1) * Zoom));
          return canFit;
        }
      }


      public IOrderedRegistry<PlotPane> Panes{ get{ return m_Panes;}}
    #endregion 
    
    
    #region Events
    
     
     /// <summary>
     /// Called when user moves the mouse over chart pane
     /// </summary>
     public event ChartPaneMouseEventHandler ChartPaneMouseEvent;

    #endregion
    
    
    #region Public Methods
    
        
      /// <summary>
      /// Notifies chart series that data/metadata has changed in series model/view properties 
      /// This method is NOT THREAD SAFE and runs in GUI thread.
      /// This method does call aggregation, that is - when this method is called many times within timespan it will rebuild chart only once at the end.
      /// </summary>
      public bool NotifySeriesChange(bool rebuildPanes = false)
      {
          m_NeedRebuildPanes = rebuildPanes;
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
      /// Maps mouse Y coordinate per pane's vertical scale value
      /// </summary>
      public float MapPaneYToValue(PlotPane pane, int y)
      {
        if (pane==null) throw new WFormsException(StringConsts.ARGUMENT_ERROR+"MapPaneYToValue(pane==null)");

        if (pane.Chart!=this) throw new WFormsException(StringConsts.ARGUMENT_ERROR+"MapPaneYToValue(pane.Chart!=this)");

        var pp = (pane.VRulerMaxScale - pane.VRulerMinScale) / (float)(pane.Height==0? 1 : pane.Height);
        
        return pane.VRulerMaxScale - (y * pp);
      }
      
      /// <summary>
      /// Maps X coordinate to primary series sample
      /// </summary>
      public ITimeSeriesSample MapXToSample(int xcoord)
      {
        if (m_Series==null) return null;
        var maxSampleWidth = m_Series.GetMaxSampleWidth();
        var data = m_Series.Data.Skip(m_HScrollPosition);


        var x = m_VRulerPosition == VRulerPosition.Left ? m_VRulerWidth + 1 : 1;

        if (m_Zoom!=1.0f)
        {
          x = (int)(x  / m_Zoom);
          xcoord = (int)(xcoord  / m_Zoom);
        }

        var xcutof = m_VRulerPosition == VRulerPosition.Right ? Width - 1 - VRulerWidth : Width;
        ITimeSeriesSample prior = null;
        foreach(var sample in data)
        {
          if (xcoord<x)
          {
            return prior;
          }
          x += maxSampleWidth;
          x++;//margin
          prior = sample;
        }
        return prior;
      }


       public float GetPaneVProportion(string name)
       {
         float result;
         if (m_PaneVProportions.TryGetValue(name, out result)) return result;

         return DEFAULT_PANE_VPROPORTION;
       }

       public void SetPaneVProportion(string name, float proportion)
       {
         m_PaneVProportions[name] = proportion;
       }

    #endregion
    
    
    #region Protected

       /// <summary>
       /// Performs layout of sub controls, such as element host, scroll bars etc.. 
       /// </summary>
       protected virtual void LayoutSubControls(int oldLeft, int oldTop, int oldWidth, int oldHeight)
       {
         

         oldHeight = oldHeight - ((m_Panes.Count-1) * SPLITTER_HEIGHT);
         if (oldHeight<MIN_PANE_HEIGHT) oldHeight = MIN_PANE_HEIGHT;

         var newHeight = Height - ((m_Panes.Count-1) * SPLITTER_HEIGHT);
         if (newHeight<MIN_PANE_HEIGHT) newHeight = MIN_PANE_HEIGHT;

         this.SuspendLayout();
         try
         {
           foreach(var pane in m_Panes)
           {
             var nph = (int)(GetPaneVProportion(pane.Name) * newHeight);
             if (nph<MIN_PANE_HEIGHT) nph = MIN_PANE_HEIGHT;
             pane.Height = nph;
           }
         }
         finally
         {
           this.ResumeLayout(true);
         }

         rebuildChart();
       }
       
       protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
       {
         var oldLeft = Left;
         var oldTop = Top;
         var oldWidth = Width;
         var oldHeight = Height;
         base.SetBoundsCore(x, y, width, height, specified);
         LayoutSubControls(oldLeft, oldTop, oldWidth, oldHeight);
       }
       
       protected override void OnPaddingChanged(EventArgs e)
       {
         base.OnPaddingChanged(e);
         LayoutSubControls(Left, Top, Width, Height);
       }
  
  
       
       protected internal virtual void OnChartPaneMouseEvent(ChartPaneMouseEventArgs.MouseEventType et, PlotPane senderPane, MouseEventArgs mouse)
       {
         if (this.ChartPaneMouseEvent!=null)
         {
           var sample = MapXToSample(mouse.X);
           var value = MapPaneYToValue(senderPane, mouse.Y);
           var args = new ChartPaneMouseEventArgs(et, this, senderPane, mouse, sample, value);
           ChartPaneMouseEvent(this, args);
         }
       }

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
           
              


       protected override void OnMouseWheel(MouseEventArgs e)
       {
         var alt = (Control.ModifierKeys&Keys.Alt) >0;
         var ctrl = (Control.ModifierKeys&Keys.Control) >0;
         var shift = (Control.ModifierKeys&Keys.Shift) >0;
        
        
         if (ctrl)//zoom
         {
           Zoom += Math.Sign(e.Delta)* 0.1f;
         }

         if (!ctrl && !shift && !alt)
          HScrollPosition -= Math.Sign(e.Delta) * 4;

         base.OnMouseWheel(e);
       }


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
                      
       protected override bool ProcessDialogKey(Keys keyData)
       {
           if (keyData==Keys.Left)
           {
             HScrollPosition--;
             return true;
           }

           if (keyData==Keys.Right)
           {
             HScrollPosition++;
             return true;
           }


           if (keyData==Keys.PageDown)
           {
             HScrollPosition += SampleFitAcrossCount;
             return true; 
           }

           if (keyData==Keys.PageUp)
           {
             HScrollPosition -= SampleFitAcrossCount;
             return true; 
           }

           if (keyData==Keys.Home)
           {
             HScrollPosition = 0;
             return true; 
           }

           if (keyData==Keys.End)
           {
             if (m_Series!=null)
               HScrollPosition = m_Series.SampleCount;
             return true; 
           }
         

       //  if (keyData==(Keys.C | Keys.Control))
       //  {
       //   CopyToClipboard();
       //   return true;
       //  }
         return base.ProcessDialogKey(keyData);
       }
       

       

    #endregion
    
    #region .pvt  .impl
       

       private void rebuildChart()
       {
          if (m_Disposed) return;

          buildPanes();

          if (m_Series==null) return;
          if (m_Panes.Count==0) return;
          var zoom = m_Panes[0].Zoom;

          //the maximum width of 1 sample - taken from all views
          var maxSampleWidth = m_Series.GetMaxSampleWidth();

          
          //Adjust scroll
          var width = Width - VRulerWidth;
          var canFit = (int)(width / ((maxSampleWidth+1) * zoom));

          var rightMostScroll = m_Series.SampleCount - canFit;

          if (m_AutoScroll || m_HScrollPosition>rightMostScroll)
          m_HScrollPosition = m_Series.SampleCount - canFit;
          if (m_HScrollPosition<0) m_HScrollPosition = 0;

          

          var timeScaleData = m_Series.Data.Skip(m_HScrollPosition);
          m_TimeScalePane.ComputeTicks(timeScaleData,
                                        maxSampleWidth); 
          m_TimeScalePane.Refresh();

          var fit = m_Series.BuildViews(this, maxSampleWidth);

          m_TimeScalePane.SetMouseCursorX(m_TimeScalePane.MouseCursorX, true);
         
          fireChartUpdateMouseEvent();
       }


       private void fireChartUpdateMouseEvent()
       {
          var screenMousePos = Cursor.Position;
          var chartMousePos = this.PointToClient(screenMousePos);

          var pane = this.GetChildAtPoint(chartMousePos, GetChildAtPointSkip.None) as PlotPane;

          if (pane==null) return;//mouse is not over plot pane now

          var paneMousePos = pane.PointToClient(screenMousePos);

          var args = new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, paneMousePos.X, paneMousePos.Y, 0);

          this.OnChartPaneMouseEvent(ChartPaneMouseEventArgs.MouseEventType.ChartUpdate, pane, args);
       }
       
      
       private void buildPanes()
       {
         this.SuspendLayout();
         try
         {
            buildPanesCore();
         }
         finally
         {
           this.ResumeLayout(true);
           m_NeedRebuildPanes = false;
         }
       }

       private void buildPanesCore()
       {
         if (m_Series==null || m_NeedRebuildPanes)
         {
           foreach(var ctl in m_Panes)
           {
             this.Controls.Remove(ctl);
             ctl.Dispose();
           }
          
           m_Panes.Clear();
          
           //todo ubit shkalu X vnizy
           if (m_Series==null) return;
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
             this.Controls.Remove(ctl);
             ctl.Dispose();
           }
         }

         //2. Add panes
         foreach(var pn in paneNames.Reverse())
         {
           var ctl = m_Panes[pn];
           if (ctl!=null)
           {
             ctl.UpdateZoom(m_Zoom);
             ctl.DeleteAllElements();//Clear the pane
             continue;//pane already exists
           }
           //Add pane
           ctl = new PlotPane(this, pn, m_Panes.Count+1);
           ctl.TabStop = false;
           ctl.Cursor = Cursors.Cross;
           ctl.Height = (int)(this.Height * GetPaneVProportion(pn));
           SetPaneVProportion(pn, ctl.Height / (float)(this.Height>0 ? this.Height : 1));
           
           ctl.Parent = this;
           ctl.Dock = DockStyle.Top;
           ctl.UpdateZoom(m_Zoom);
        
           //ctl.BackColor = Color.HotPink;
           m_Panes.Register(ctl);
         }


         //3. Build splitters
         var ilast = m_Panes.Count()-1;
         for(var i=0; i<=ilast; i++)
         {
           var pane = m_Panes[i];
           
           {
          //   pane.Dock = DockStyle.Top;
             if (pane.m_Splitter==null)
             { 
               pane.m_Splitter = new Splitter();
               pane.m_Splitter.Parent = this;
               this.Controls.SetChildIndex( pane.m_Splitter, this.Controls.GetChildIndex(pane)); 
               pane.m_Splitter.Dock = DockStyle.Top;
               pane.m_Splitter.Height = SPLITTER_HEIGHT;
               pane.m_Splitter.BackColor = Color.Silver;
               pane.m_Splitter.BorderStyle = BorderStyle.FixedSingle;
               //todo Splitter moving - suspend update?
               pane.m_Splitter.SplitterMoved += delegate(object sender, SplitterEventArgs e)
               {
                 this.NotifySeriesChange(false);
                 foreach(var p in m_Panes)
                    SetPaneVProportion(p.Name, p.Height / (float)(this.Height>0 ? this.Height : 1));
               };
             }
           }
         }
       }

    #endregion
  }
  
  
  
 
  
  
}
