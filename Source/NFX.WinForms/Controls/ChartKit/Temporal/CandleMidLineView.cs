using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using NFX.Financial.Market;

using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{
  public enum MidLineType { HighLow = 0, OpenClose }


  /// <summary>
  /// View of candles middle line
  /// </summary>
  public class CandleMidLineView : CandleViewBase
  {

        #region Candle Mid-Line Element
            /// <summary>
            /// Represents an element for candle mid line
            /// </summary>
            public class CandleMidLineElement : Element
            {
              protected internal CandleMidLineElement(CandleMidLineView view, PlotPane host, Point[] points) : base(host)
              {
                m_View = view;
                m_Points = points;

                this.Region = new Rectangle(points[0].X, 0, points[points.Length-1].X, host.Height);
              }

              private CandleMidLineView m_View;
              private Point[] m_Points;


                 protected internal override void Paint(Graphics gr)
                 {
                   //for now, no styles
                     gr.SmoothingMode = SmoothingMode.AntiAlias;
                     using(var pen = getContourPen(m_View.LineStyle))
                     {
                       gr.DrawLines(pen, m_Points);
                     }
                 }

                 private Pen getContourPen(LineStyle style)
                 {
                      var result = new Pen(style.Color);
                      result.Width = style.Width;
                      result.DashStyle = style.DashStyle;
                      return result;
                 }
            }
        #endregion



      public CandleMidLineView(string name, int order, string paneName = null) : base(name, order, paneName)
      {
      }



      private static readonly LineStyle s_DefaultLineStyle = new LineStyle{ Color = Color.FromArgb(200, 50, 150, 255),
                                                                  Width = 2,
                                                                  DashStyle = System.Drawing.Drawing2D.DashStyle.Solid};

      private LineStyle m_LineStyle = s_DefaultLineStyle;

      public MidLineType MidLineType
      {
        get; set;
      }

      public LineStyle LineStyle
      {
        get { return m_LineStyle;}
        set { m_LineStyle = value;}
      }



      protected override Elements.Element MakeSampleElement(TimeSeriesChart chart, PlotPane pane, CandleSample sample, int x, float minScale, float maxScale)
      {
        return null;
      }

      protected override Element MakeSeriesElement(TimeSeriesChart chart,
                                                   PlotPane pane,
                                                   IEnumerable<CandleSample> data,
                                                   int xStart,
                                                   float minScale,
                                                   float maxScale,
                                                   int maxSampleWidth,
                                                   out int fitSamplesCount)
      {
        var points = new List<Point>();


        var pxTotalHeight = (int)(pane.Height / pane.Zoom);
        var pxPrice = (float)pxTotalHeight / (maxScale - minScale);

        fitSamplesCount = 0;
        var x = xStart+(maxSampleWidth / 2);
        var xcutof = chart.VRulerPosition == VRulerPosition.Right ? pane.Width - 1 - chart.VRulerWidth : pane.Width;
        foreach(var candle in data)
        {
          var midPrice =  MidLineType==Temporal.MidLineType.HighLow ?
                       (candle.LowPrice + ((candle.HighPrice - candle.LowPrice) / 2f)) :
                       (Math.Min(candle.OpenPrice, candle.ClosePrice) + (Math.Abs(candle.OpenPrice - candle.ClosePrice) / 2f));


          var y = pxTotalHeight - (int)((midPrice-minScale) * pxPrice);
          points.Add( new Point(x, y));


          x += maxSampleWidth;
          x++;
          if ((x*pane.Zoom)>=xcutof) break;
          fitSamplesCount++;
        }

        return new CandleMidLineElement(this, pane, points.ToArray());
      }




  }


}
