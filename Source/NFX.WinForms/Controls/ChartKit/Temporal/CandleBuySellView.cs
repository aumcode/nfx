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
  /// View of candles buy/sell bars line
  /// </summary>
  public class CandleBuySellView : CandleViewBase
  {

        public enum ViewKind{ SideBySide = 0, Centered, Stacked }


        #region Candle Mid-Line Element
            /// <summary>
            /// Represents an element for candle buy sell
            /// </summary>
            public class CandleBuySellElement : Element
            {
              protected internal CandleBuySellElement(CandleBuySellView view, PlotPane host, CandleSample sample, int x, float minScale, float maxScale) : base(host)
              {
                m_View = view;
                m_Sample = sample;
                m_ScaleMin = minScale;
                m_ScaleMax = maxScale;

                computeCoords();
                var h = Math.Max(m_Lay_BuyHeight, m_Lay_SellHeight);
                this.Region = new Rectangle(x, (int)(host.Height / host.Zoom) - h, CandleView.BAR_WIDTH, h);
              }

              private CandleBuySellView m_View;
              private CandleSample m_Sample;
              private float m_ScaleMin;
              private float m_ScaleMax;


              private int m_Lay_BuyHeight;
              private int m_Lay_SellHeight;

                 private void computeCoords()
                 {
                   var pxTotalHeight = (int)(m_Host.Height / m_Host.Zoom);
                   var pxPrice = (float)pxTotalHeight / (m_ScaleMax - m_ScaleMin);

                   if (m_View.Kind== ViewKind.Centered)
                   {
                     m_Lay_BuyHeight = (int)( m_Sample.BuyVolume * pxPrice );
                     m_Lay_SellHeight = (int)( m_Sample.SellVolume * pxPrice );
                   }
                   else
                   {
                     m_Lay_BuyHeight = (int)( (m_Sample.BuyVolume - m_ScaleMin) * pxPrice );
                     m_Lay_SellHeight = (int)( (m_Sample.SellVolume - m_ScaleMin) * pxPrice );
                   }
                 }

                 protected internal override void Paint(Graphics gr)
                 {
                   var hf = CandleView.BAR_WIDTH / 2;

                   if (m_View.Kind== ViewKind.SideBySide)
                   {
                       var hh = Host.Height / Host.Zoom;

                       gr.FillRectangle(Brushes.Green, this.Left, hh - m_Lay_BuyHeight, hf, m_Lay_BuyHeight);
                       gr.FillRectangle(Brushes.Red, this.Left+hf, hh - m_Lay_SellHeight, hf, m_Lay_SellHeight);
                   }
                   else if (m_View.Kind== ViewKind.Stacked)
                   {
                      var hh = Host.Height / Host.Zoom;

                      gr.FillRectangle(Brushes.Green, this.Left, hh - m_Lay_BuyHeight, CandleView.BAR_WIDTH, m_Lay_BuyHeight);
                      gr.FillRectangle(Brushes.Red, this.Left, hh - m_Lay_BuyHeight-m_Lay_SellHeight, CandleView.BAR_WIDTH, m_Lay_SellHeight);
                   }
                   else//centered
                   {
                      var mid = (Host.Height / 2) / Host.Zoom;

                      gr.FillRectangle(Brushes.Green, this.Left, mid - m_Lay_BuyHeight, CandleView.BAR_WIDTH, m_Lay_BuyHeight);
                      gr.FillRectangle(Brushes.Red, this.Left, mid, CandleView.BAR_WIDTH, m_Lay_SellHeight);
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



      public CandleBuySellView(string name, int order, string paneName = null) : base(name, order, paneName)
      {
      }


      public ViewKind Kind{ get; set;}


      public override string DefaultPaneName
      {
        get
        {
          return "Volume";
        }
      }


      protected override VScaleZoomAlign VScaleZoomAlignment
      {
        get
        {
          switch(this.Kind)
          {
            case CandleBuySellView.ViewKind.Centered: return VScaleZoomAlign.Mid;
            case CandleBuySellView.ViewKind.Stacked: return VScaleZoomAlign.Bottom;
            default: return VScaleZoomAlign.Bottom;
          }
        }
      }


      protected override Elements.Element MakeSampleElement(TimeSeriesChart chart, PlotPane pane, CandleSample sample, int x, float minScale, float maxScale)
      {
        return new CandleBuySellElement(this, pane, sample, x, minScale, maxScale);
      }

      protected override void DoCalcMinMaxScale(IEnumerable<CandleSample> data, out float minScale, out float maxScale)
      {
        if (this.Kind==ViewKind.Centered)
        {
          minScale = 0;
          maxScale = 0;
          foreach(var sample in data)
          {

            if (sample.BuyVolume > maxScale) maxScale = sample.BuyVolume;
            if (sample.SellVolume > maxScale) maxScale = sample.SellVolume;
          }
          minScale = -maxScale;
        }
        else if (this.Kind==ViewKind.Stacked)
        {
          minScale = 0;
          maxScale = 0;
          foreach(var sample in data)
          {
            var sum = sample.BuyVolume + sample.SellVolume;
            if (sum > maxScale) maxScale = sum;
          }
        }
        else
        {
          minScale = 0;
          maxScale = 0;
          foreach(var sample in data)
          {
            var max = Math.Max(sample.BuyVolume, sample.SellVolume);
            if (max > maxScale) maxScale = max;
          }
        }

        var margin = 2f * //percent
                     ((maxScale - minScale) / 100f);

        minScale -= margin;
        maxScale += margin;
      }
  }

}
