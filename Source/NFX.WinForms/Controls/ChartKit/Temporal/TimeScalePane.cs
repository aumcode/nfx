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

using NFX.Financial.Market;
using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{

  /// <summary>
  /// Provides a viewport for horizntal scale
  /// </summary>
  public class TimeScalePane : ElementHostControl
  {
      private const int TICK_SPACE = 52;

            public class Tick
            {
               public Tick(ITimeSeriesSample sample, int x, bool warp, bool dayChange){ Sample = sample; X = x; Warp = warp; DayChange = dayChange; }
               public readonly ITimeSeriesSample Sample;
               public readonly int X;
               public readonly bool Warp;
               public readonly bool DayChange;

            }


         #region Time Cursor Element
            /// <summary>
            /// Represents an element for candle mid line
            /// </summary>
            public class TimeCursorElement : Element
            {
              protected internal TimeCursorElement(TimeScalePane host) : base(host)
              {
                this.Region = new Rectangle(0, 0, TICK_SPACE, host.Height);
              }

                 private string m_Text;

                 public void SetText(string text, int x)
                 {
                   m_Text = text; 
                   m_Host.Invalidate(this.Region); 
                   this.Region = new Rectangle(x - (Width / 2), 0, TICK_SPACE, Host.Height);
                 } 

                 protected internal override void Paint(Graphics gr)
                 {           
                     using(var font = new Font("Verdana", 6f))
                      using(var sf = new StringFormat())
                      {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center; 
                        
                        gr.FillRectangle(Brushes.Orange, this.Region); 
                        gr.DrawString(m_Text, font, Brushes.White, this.Region, sf);
                      }
                     
                 }
            }
        #endregion



     internal TimeScalePane(TimeSeriesChart chart) : base()
     {              
       m_Chart = chart;
       m_TimeCursor = new TimeCursorElement(this);
     }

     private TimeSeriesChart m_Chart;

     private List<Tick> m_Ticks; 
     private int m_MouseCursorX;
     private TimeCursorElement m_TimeCursor;

     public TimeSeriesChart Chart{ get{ return m_Chart;}}

     public IEnumerable<Tick> Ticks { get{ return m_Ticks;}}

     public int MouseCursorX
     {
       get{ return m_MouseCursorX;}
     }

     public void SetMouseCursorX(int x, bool force = false)
     {
         if (!force && m_MouseCursorX==x) return;

         foreach(var pane in m_Chart.Panes)
          pane.Invalidate( new Rectangle(m_MouseCursorX-1, 0, 2, pane.Height) );

         m_MouseCursorX = x;

         foreach(var pane in m_Chart.Panes)
          pane.Invalidate( new Rectangle(m_MouseCursorX-1, 0, 2, pane.Height) );

         var sample = m_Chart.MapXToSample(x);
         if (sample!=null)
         {
           var ts = sample.TimeStamp;
           m_TimeCursor.SetText( "{0:D2}/{1:D2}\n{2:D2}:{3:D2}:{4:D2}".Args(ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second), x);
           m_TimeCursor.Visible = true;
         }
         else
          m_TimeCursor.Visible = false;
     }


     internal void ComputeTicks(IEnumerable<ITimeSeriesSample> data, int maxSampleWidth)
     {
      

       m_Ticks = new List<Tick>();
      
       //x is in non-scalable coords
       float x = m_Chart.VRulerPosition == VRulerPosition.Left ? m_Chart.VRulerWidth + 1 : 1;

       x += (maxSampleWidth / 2);

       float xcutof = m_Chart.VRulerPosition == VRulerPosition.Right ? Width - 1 - m_Chart.VRulerWidth : Width;
       

       var scaledMaxSampleWidth = maxSampleWidth * m_Chart.Zoom;

       ITimeSeriesSample priorSample = null;
       Tick priorTick = null;
       int msDist = 0;
       foreach(var sample in data)
       {
         var warp = false;
         //WARP detection
         if (priorSample!=null)
         {
           var dist = (int)((sample.TimeStamp - priorSample.TimeStamp).TotalMilliseconds);//positive because data is pre-ordered
           if (msDist>0)
           {
             var delta = 2;
             if (dist<2000) delta = 3;//more than 5x change
             if( dist/msDist > delta) 
             {
              //WARP!
               priorTick = new Tick(sample, (int)x, true, priorSample!=null ? priorSample.TimeStamp.DayOfYear != sample.TimeStamp.DayOfYear : false);
               m_Ticks.Add( priorTick );
               warp = true;
             }
             msDist = (dist + msDist) / 2;
           }
           else
            msDist = dist;
         }

         if (!warp)//try to create regular scale notch
         {
           if (
               (priorTick==null && x >= TICK_SPACE) ||
               (priorTick!=null && (((x - priorTick.X) >= TICK_SPACE) || (priorSample.TimeStamp.DayOfYear != sample.TimeStamp.DayOfYear))  )
              )
           {
             priorTick = new Tick(sample, (int)x, false, priorSample!=null ? priorSample.TimeStamp.DayOfYear != sample.TimeStamp.DayOfYear : false);
             m_Ticks.Add(priorTick);
           }
         }

         priorSample = sample;
         x += scaledMaxSampleWidth;
         x += m_Chart.Zoom;
         if (x>xcutof) break;
       }

     }
    


     protected override void OnPaint(PaintEventArgs e)
     {
          if (m_Ticks==null) return;

          
          var gr = e.Graphics;

         // gr.Clear(Color.Black);

          using(var font = new Font("Verdana", 6f))
          using(var sf = new StringFormat())
          {
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            var hh = Height / 2;
            Tick prior = null;
            foreach( var tick in m_Ticks)
            {
            

              var dt = tick.Sample.TimeStamp;
              string txt;
                   
              if (prior==null || prior.Sample.TimeStamp.Day!=dt.Day)
               txt = "{0}/{1}\n{2:D2}:{3:D2}:{4:D2}".Args(dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
              else
               if (prior.Sample.TimeStamp.Hour!=dt.Hour)
                 txt = "{0:D2}:{1:D2}:{2:D2}".Args(dt.Hour, dt.Minute, dt.Second);
               else
                  txt = "{0:D2}:{1:D2}".Args(dt.Minute, dt.Second);
                      

              var rect = new RectangleF(tick.X, 0, TICK_SPACE, Height);

              if (tick.Warp)
              {
               gr.FillRectangle(Brushes.Gray, rect);
               gr.DrawLine(Pens.White, tick.X, 0, tick.X, Height);
              }
              else
               gr.DrawLine(Pens.DimGray, tick.X, 0, tick.X, hh);

              
              gr.DrawString(txt, font, Brushes.White, rect, sf);

              prior = tick;
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

  }

}
