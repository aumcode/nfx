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
  /// <summary>
  /// Base class for View of candles/price based
  /// </summary>
  public abstract class CandleViewBase : SeriesView<CandleTimeSeries, CandleSample>
  {
     public const string PRICE_PANE = "Price";
     public const int BAR_WIDTH = 7;

      public CandleViewBase(string name, int order, string paneName = null) : base(name, order, paneName)
      {
      }

      public override string DefaultPaneName
      {
        get { return PRICE_PANE;}
      }

      public override int SampleWidth
      {
        get { return  BAR_WIDTH + 1;}
      }

      protected override void DoCalcMinMaxScale(IEnumerable<CandleSample> data, out float minScale, out float maxScale)
      {
        minScale = float.MaxValue;
        maxScale = float.MinValue;
        foreach(var sample in data)
        {
          var min = Math.Min(sample.LowPrice, Math.Min(sample.OpenPrice, sample.ClosePrice));// - 1.0f;
          var max = Math.Max(sample.HighPrice, Math.Max(sample.OpenPrice, sample.ClosePrice));// + 1.0f;

          if (min < minScale) minScale = min;
          if (max > maxScale) maxScale = max;
        }

        var margin = 2f * //percent
                     ((maxScale - minScale) / 100f);

        minScale -= margin;
        maxScale += margin;
      }

  }


  /// <summary>
  /// View of candles
  /// </summary>
  public class CandleView : CandleViewBase
  {

        #region Candle Element
            /// <summary>
            /// Represents an element for a single candle
            /// </summary>
            public class CandleElement : Element
            {
              protected internal CandleElement(PlotPane host, CandleSample candle, int x, float minScale, float maxScale, bool bw) : base(host)
              {
                Candle = candle;

              //   m_Style = new Style(null, getBaseStyle());
                m_ScaleMin = minScale;
                m_ScaleMax = maxScale;
                m_Lay_X = x;
                m_Lay_W = CandleView.BAR_WIDTH-1;

                computeCoords();
                this.Region = new Rectangle(m_Lay_X, m_Lay_Y, m_Lay_W, m_Lay_H);
                m_BW = bw;
              }

              public readonly CandleSample Candle;

              private float m_ScaleMin;
              private float m_ScaleMax;
              private int m_Lay_X;
              private int m_Lay_Y;
              private int m_Lay_W;
              private int m_Lay_H;

              private int m_Lay_HighTickY;
              private int m_Lay_LowTickY;
              private Rectangle m_Lay_Body;
              private bool m_Lay_Inc;
              private bool m_BW;



                 private void computeCoords()
                 {
                   var pxTotalHeight = (int)(m_Host.Height / m_Host.Zoom);
                   var pxPrice = (float)pxTotalHeight / (m_ScaleMax - m_ScaleMin);

                   m_Lay_HighTickY = pxTotalHeight - (int)( (Candle.HighPrice - m_ScaleMin) * pxPrice );
                   m_Lay_LowTickY = pxTotalHeight - (int)( (Candle.LowPrice - m_ScaleMin) * pxPrice );

                   m_Lay_Y = m_Lay_HighTickY;
                   m_Lay_H = m_Lay_LowTickY - m_Lay_HighTickY;

                   if (Candle.OpenPrice < Candle.ClosePrice)
                   {
                     m_Lay_Inc = true; //green, the close is > open
                     var top =  pxTotalHeight - (int)( (Candle.ClosePrice - m_ScaleMin) * pxPrice );
                     var bot =  pxTotalHeight - (int)( (Candle.OpenPrice - m_ScaleMin) * pxPrice );
                     m_Lay_Body = new Rectangle(m_Lay_X, top, m_Lay_W, Math.Max(bot-top, 1));
                   }
                   else
                   {
                     m_Lay_Inc = false;//red, the open is > than close
                     var top =  pxTotalHeight - (int)( (Candle.OpenPrice - m_ScaleMin) * pxPrice );
                     var bot =  pxTotalHeight - (int)( (Candle.ClosePrice - m_ScaleMin) * pxPrice );
                     m_Lay_Body = new Rectangle(m_Lay_X, top, m_Lay_W, Math.Max(bot-top, 1));
                   }
                 }

                 protected internal override void Paint(Graphics gr)
                 {
                   //for now, no styles
                     using(var pen = getContourPen(new LineStyle{ Color = Color.Black,
                                                                  Width = 1,
                                                                  DashStyle = System.Drawing.Drawing2D.DashStyle.Solid}))
                     {
                  //     gr.DrawLine(pen, m_Lay_X+1, m_Lay_HighTickY, m_Lay_X+m_Lay_W-2, m_Lay_HighTickY); //High tick
                  //     gr.DrawLine(pen, m_Lay_X+1, m_Lay_LowTickY, m_Lay_X+m_Lay_W-2, m_Lay_LowTickY); //Low tick

                       var x = m_Lay_X + (m_Lay_W/2);
                       gr.DrawLine(pen,x, m_Lay_LowTickY, x, m_Lay_HighTickY); //Vertical



                    // using(var brush = getBrush(m_Style))//body
                     {
                       var brush = m_BW ? Brushes.Silver : ( m_Lay_Inc ? Brushes.LimeGreen : Brushes.Red );
                       gr.FillRectangle(brush, m_Lay_Body);
                     }

                       gr.DrawRectangle(pen, m_Lay_Body);
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


      public CandleView(string name, int order, string paneName = null) : base(name, order, paneName)
      {
      }


      public bool BlackWhite { get; set;}

      public bool ShowBalloons { get; set;}

      protected override Elements.Element MakeSampleElement(TimeSeriesChart chart, PlotPane pane, CandleSample sample, int x, float minScale, float maxScale)
      {
        var elm = new CandleElement(pane, sample, x, minScale, maxScale, BlackWhite);

        if (ShowBalloons)
          elm.MouseClick += elm_MouseClick;

        return elm;
      }

      //protected override Element MakeSeriesElement(TimeSeriesChart chart, PlotPane pane, IEnumerable<CandleSample> data, int xStart, float minScale, float maxScale, int maxSampleWidth, out int fitSamplesCount)
      //{
      //  if (m_Balloon!=null)
      //  {
      //    m_Balloon.FadeOut();
      //    m_Balloon = null;
      //  }
      //  return base.MakeSeriesElement(chart, pane, data, xStart, minScale, maxScale, maxSampleWidth, out fitSamplesCount);
      //}


      private Balloon m_Balloon;



      void elm_MouseClick(object sender, EventArgs e)
      {
        if (m_Balloon!=null)
        {
          m_Balloon.FadeOut();
          m_Balloon = null;
        }

        var elm = sender as CandleElement;
        if (elm==null) return;

        var target = new Point( elm.DisplayRegion.Left + BAR_WIDTH / 2, elm.DisplayRegion.Top - 1);

        target = elm.Host.PointToScreen(target);

        var c = elm.Candle;

        var body = new Rectangle(target.X-100, target.Y-128, 200, 100);

        m_Balloon = new Balloon(body, target, Color.FromArgb(200, 255, 255, 50));

        m_Balloon.Deactivate += (s, _)=> { DisposableObject.DisposeAndNull(ref m_Balloon); };

        m_Balloon.Text =
@"{0} {1} sec
 Open: {2}
 Close: {3}
 Hi: {4}
 Low: {5}
 Buy: {6}
 Sell: {7}
".Args(c.TimeStamp, c.TimeSpanMs / 1000, c.OpenPrice, c.ClosePrice, c.LowPrice, c.HighPrice, c.BuyVolume, c.SellVolume);

        m_Balloon.DisposeOnFadeOut = true;
        m_Balloon.FadeIn();
      }


  }


}
