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
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{
  /// <summary>
  /// Provides a viewport for chart plot area for the named pane
  /// </summary>
  public class PlotPane : ElementHostControl, INamed, IOrdered
  {

     internal PlotPane(TimeSeriesChart chart, string paneName, int paneOrder ) : base()
     {              
       m_Chart = chart;
       base.Name = paneName;
       this.m_Order = paneOrder;


       //for(var x=0; x<1600; x+= 48)
       //  for(var y=0; y<1800; y+= 16)
       //  {
       //  var elm =  new CheckBoxElement(this);// new TextLabelElement(this);
       //  elm.Region = new Rectangle(x,y, 46, 14);
       // // elm.Text = "Cell " + x+"x"+y;
       //  elm.Visible = true;
       //  }
     }

     protected override void Dispose(bool disposing)
     {
       DisposableObject.DisposeAndNull(ref m_Splitter);
     }


     private TimeSeriesChart m_Chart;
     private int m_Order;

     //associated splitter
     internal Splitter m_Splitter;


     private float m_VRulerMinScale;
     private float m_VRulerMaxScale;

     public new string Name { get{ return base.Name;} }
     int IOrdered.Order  {  get { return m_Order;} }
     public TimeSeriesChart Chart{ get{ return m_Chart;}}


      public float VRulerMinScale
      {
        get{ return m_VRulerMinScale;}
      }

      public float VRulerMaxScale
      {
        get{ return m_VRulerMaxScale;}
      }
    
      public void SetMinMaxScale(float min, float max, bool update)
      {
        if (min>max)
        {
          var t = max;
          max = min;
          min = t;
        }

        var invalidate = false;
      
        if ((!update && m_VRulerMinScale!=min) || (update && min<m_VRulerMinScale))
        {
          m_VRulerMinScale = min;
          invalidate = true;
        }

        if ((!update && m_VRulerMaxScale!=max) || (update && max>m_VRulerMaxScale))
        {
          m_VRulerMaxScale = max;
          invalidate = true;
        }

        if (invalidate)
          Invalidate();
      }


      private int? m_MouseGrabX;
      private int m_MouseGrabScroll;

      protected override void OnMouseDown(MouseEventArgs e)
      {
        base.OnMouseDown(e);
        
        var alt = (Control.ModifierKeys&Keys.Alt) >0;
        var ctrl = (Control.ModifierKeys&Keys.Control) >0;
        var shift = (Control.ModifierKeys&Keys.Shift) >0;
        
        this.m_MouseGrabX = ctrl ? e.X : (int?)null;
        this.m_MouseGrabScroll = m_Chart.HScrollPosition;
      }


    

      protected override void OnMouseMove(MouseEventArgs e)
      {
        const int AVG_SAMPLE_WIDTH = 16;

        base.OnMouseMove(e);

        if (m_MouseGrabX.HasValue)
        {
          var swidth = AVG_SAMPLE_WIDTH * m_Chart.Zoom;
          m_Chart.HScrollPosition = m_MouseGrabScroll + (int)((m_MouseGrabX.Value-e.X) / swidth);
        }

        if (m_Chart.MouseCursorMode==MouseCursorMode.Move)
          m_Chart.m_TimeScalePane.SetMouseCursorX( e.X ) ;

        m_Chart.OnChartPaneMouseEvent(ChartPaneMouseEventArgs.MouseEventType.Move, this, e);
      }

      protected override void OnMouseUp(MouseEventArgs e)
      {
        m_MouseGrabX = null;
        base.OnMouseUp(e);

        if (m_Chart.MouseCursorMode==MouseCursorMode.Click)
          m_Chart.m_TimeScalePane.SetMouseCursorX( e.X ) ;
      }



     protected override void OnPaint(PaintEventArgs e)
     {
          const int TXT_H_MARGIN = 8;
          const int TXT_V_MARGIN = 4;
          
          var gr = e.Graphics;

          using(var font = new Font("Verdana", 6f))
          {
           var s1 = scaleValueToString(m_VRulerMaxScale);
           var s2 = scaleValueToString(m_VRulerMinScale);


           var fSize = gr.MeasureString(s1.Length>s2.Length ? s1 : s2, font);//margin
           var lineWidth = m_Chart.VRulerWidth;// + TXT_H_MARGIN;// (int)(fSize.Width + TXT_H_MARGIN);//H margin
           var lineHeight = (int)(fSize.Height + TXT_V_MARGIN);//V margin
           var halfLineHeight = lineHeight / 2;

           var rulerX = m_Chart.VRulerPosition==VRulerPosition.Right ? this.Width - lineWidth : 0;


           //Ruler bar
           using( var brush = new SolidBrush(Color.FromArgb(150, 200, 200, 180)))
              gr.FillRectangle(brush, new Rectangle(rulerX, 0, lineWidth, Height));

         //for now, no styles
           using(var pen = getGridPen(new LineStyle{ Color = Color.Silver, Width = 1, DashStyle = System.Drawing.Drawing2D.DashStyle.Dot}))
           {

             var lineCount = this.Height / lineHeight; 
             var linePrice = (m_VRulerMaxScale - m_VRulerMinScale) / lineCount;

             var price = m_VRulerMinScale;
             var y = this.Height;
             while(y >= 0)
             {
               if (m_Chart.VRulerPosition==VRulerPosition.Right)
                 gr.DrawLine(pen, 0, y, this.Width - lineWidth, y);
               else
                 gr.DrawLine(pen, lineWidth, y, this.Right, y);

               gr.DrawString(scaleValueToString(price), font, Brushes.DarkRed, rulerX + (TXT_H_MARGIN / 2), y - halfLineHeight + (TXT_V_MARGIN / 2));
               y-=lineHeight;
               price += linePrice;
             }

             using(var warpPen = getGridPen(new LineStyle{ Color = Color.FromArgb(100,100, 70), Width = 1, DashStyle = System.Drawing.Drawing2D.DashStyle.Solid})) 
               foreach(var tick in m_Chart.m_TimeScalePane.Ticks)
               {
                 if (tick.Warp)
                 {
                   gr.DrawLine(warpPen, tick.X-1, 0, tick.X-1, Height);
                   gr.DrawLine(warpPen, tick.X+1, 0, tick.X+1, Height);
                 }

                 if (tick.DayChange)
                  gr.DrawLine(Pens.Yellow, tick.X, 0, tick.X, Height);
                 else
                  gr.DrawLine(pen, tick.X, 0, tick.X, Height);
               } 

             
              //Vertical cursor line
              using (var vp = getMouseVCursorPen())
               gr.DrawLine(vp, m_Chart.m_TimeScalePane.MouseCursorX, 0, m_Chart.m_TimeScalePane.MouseCursorX, Height);
             

           }
         }

       base.OnPaint(e);
     }

       private Pen getGridPen(LineStyle style)
       {
         var result = new Pen(style.Color);
         result.Width = style.Width;
         result.DashStyle = style.DashStyle;
         return result;
       }

       private Pen getMouseVCursorPen()
       {
         var result = new Pen(Color.FromArgb(200, 0,0,0));
         result.Width = 1;
         result.DashStyle =  System.Drawing.Drawing2D.DashStyle.Dot;
         return result;
       }

       private string scaleValueToString(float price)
       {
         return price.ToString("n2");
       }

  }

}
