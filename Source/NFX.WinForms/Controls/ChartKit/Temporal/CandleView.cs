using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;


using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{
   /// <summary>
  /// View of candles
  /// </summary>
  public class CandleView : SeriesView
  {
     public const string PRICE_PANE = "Price";
     public const int BAR_WIDTH = 7;

        #region Candle Element
            /// <summary>
            /// Represents an element for a single candle
            /// </summary>
            public class CandleElement : Element
            {
              protected internal CandleElement(PlotPane host, CandleSample candle, int x, float minScale, float maxScale) : base(host)
              {
                m_Candle = candle;
         
              //   m_Style = new Style(null, getBaseStyle());
                m_ScaleMin = minScale;
                m_ScaleMax = maxScale;
                m_Lay_X = x;
                m_Lay_W = CandleView.BAR_WIDTH;

                computeCoords();
                this.Region = new Rectangle(m_Lay_X, m_Lay_Y, m_Lay_W, m_Lay_H);
              }

              private CandleSample m_Candle;

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
    


                 private void computeCoords()
                 {
                   var pxTotalHeight = m_Host.Height;
                   var pxPrice = (float)pxTotalHeight / (m_ScaleMax - m_ScaleMin);
         
                   m_Lay_HighTickY = pxTotalHeight - (int)( (m_Candle.HighPrice - m_ScaleMin) * pxPrice );
                   m_Lay_LowTickY = pxTotalHeight - (int)( (m_Candle.LowPrice - m_ScaleMin) * pxPrice );

                   m_Lay_Y = m_Lay_HighTickY;
                   m_Lay_H = m_Lay_LowTickY - m_Lay_HighTickY;

                   if (m_Candle.OpenPrice < m_Candle.ClosePrice)
                   {
                     m_Lay_Inc = true; //green, the close is > open
                     var top =  pxTotalHeight - (int)( (m_Candle.ClosePrice - m_ScaleMin) * pxPrice );
                     var bot =  pxTotalHeight - (int)( (m_Candle.OpenPrice - m_ScaleMin) * pxPrice );
                     m_Lay_Body = new Rectangle(m_Lay_X, top, m_Lay_W, bot-top);
                   }
                   else
                   {
                     m_Lay_Inc = false;//red, the open is > than close
                     var top =  pxTotalHeight - (int)( (m_Candle.OpenPrice - m_ScaleMin) * pxPrice );
                     var bot =  pxTotalHeight - (int)( (m_Candle.ClosePrice - m_ScaleMin) * pxPrice );
                     m_Lay_Body = new Rectangle(m_Lay_X, top, m_Lay_W, bot-top);
                   }
                 }

                 protected internal override void Paint(Graphics gr)
                 {           
                   //for now, no styles
                     using(var pen = getContourPen(new LineStyle{ Color = Color.Black, 
                                                                  Width = 1, 
                                                                  DashStyle = System.Drawing.Drawing2D.DashStyle.Solid}))
                     {
                       gr.DrawLine(pen, m_Lay_X+1, m_Lay_HighTickY, m_Lay_X+m_Lay_W-2, m_Lay_HighTickY); //High tick             
                       gr.DrawLine(pen, m_Lay_X+1, m_Lay_LowTickY, m_Lay_X+m_Lay_W-2, m_Lay_LowTickY); //Low tick             

                       var x = m_Lay_X + (m_Lay_W/2);
                       gr.DrawLine(pen,x, m_Lay_LowTickY, x, m_Lay_HighTickY); //Vertical


             
                    // using(var brush = getBrush(m_Style))//body
                     {
                       var brush = m_Lay_Inc ? Brushes.LimeGreen : Brushes.Red;
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


      public override string DefaultPaneName
      {
        get { return PRICE_PANE;}
      }

      public override int SampleWidth
      {
        get { return  BAR_WIDTH;}
      }


      private float m_MinScale;
      private float m_MaxScale;


      public override void BuildElements(Chart chart, PlotPane pane, Series series)
      {
        var cdata = series as CandleTimeSeries;
        if (cdata==null) throw new WFormsException("CandleView requires CandleTimeSeries source");

      
        var drawData = getDataPerScroll(cdata);

        if (!drawData.Any()) return;//nothing to draw


        float minScale;
        float maxScale;
        calcMinMaxScale(drawData, out minScale, out maxScale);

        m_MinScale = minScale;
        m_MaxScale = maxScale;

        pane.SetMinMaxScale(minScale, maxScale, true);


        var x = 0;
        foreach(var candle in cdata.Data)
        {
          if (x > pane.Width) break;
          var elm = new CandleElement(pane, candle, x, minScale, maxScale);
          elm.Visible = true;
          x += elm.Width;
          x++;//margin
        }
      }

      private IEnumerable<CandleSample> getDataPerScroll(CandleTimeSeries cdata)
      {
        return cdata.Data;//todo SKIP depending on scroll bar position
      }

      private void calcMinMaxScale(IEnumerable<CandleSample> data, out float minScale, out float maxScale)
      {
        minScale = float.MaxValue;
        maxScale = float.MinValue;
        foreach(var sample in data)
        {
          var min = Math.Min(sample.LowPrice, Math.Min(sample.OpenPrice, sample.ClosePrice)) - 1.0f;
          var max = Math.Max(sample.HighPrice, Math.Max(sample.OpenPrice, sample.ClosePrice)) + 1.0f;

          if (min < minScale) minScale = min;
          if (max > maxScale) maxScale = max;
        }
      }

  }

  
}
