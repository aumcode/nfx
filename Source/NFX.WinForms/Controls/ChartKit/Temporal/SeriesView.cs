using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{
  /// <summary>
  /// Denotes a view that visualizes series data in a chart
  /// </summary>
  public abstract class SeriesView : INamed, IOrdered
  {

     #region Levels
        /// <summary>
        /// Represents an element for a single candle
        /// </summary>
        public class YLevelElement : Element
        {
          protected internal YLevelElement(PlotPane host, TimeSeries.YLevel level, float minScale, float maxScale) : base(host)
          {
            m_Level = level;

          //   m_Style = new Style(null, getBaseStyle());
            m_ScaleMin = minScale;
            m_ScaleMax = maxScale;

            computeCoords();
            this.Region = new Rectangle(0, m_Y-2, host.Width, m_Y+2);
          }

          public PlotPane Pane { get { return (PlotPane)Host; } }

          private TimeSeries.YLevel m_Level;

          private int m_Y;

          private float m_ScaleMin;
          private float m_ScaleMax;

          private void computeCoords()
          {
            var pxTotalHeight = (int)(m_Host.Height / m_Host.Zoom);
            var pxValue = (float)pxTotalHeight / (m_ScaleMax - m_ScaleMin);

            m_Y = pxTotalHeight - (int)( ((m_Level.Value - m_ScaleMin) * pxValue ));

          }

          protected internal override void Paint(Graphics gr)
          {
            //for now, no styles

            using(var pen = getContourPen(m_Level.HLineStyle))
            {
              gr.DrawLine(pen, 0, m_Y, Host.Width / Host.Zoom, m_Y); //horizontal line

              if (Host.Zoom==1.0f)
              {
                var   chart = ((PlotPane)Host).Chart;
                Color cbg   = m_Level.Style == null ? Color.Black : m_Level.Style.BGColor;
                Color ctxt  = m_Level.Style == null ? Color.White : m_Level.Style.ForeColor;

                var rw = chart.VRulerWidth;

                var x = chart.VRulerPosition== VRulerPosition.Left ? 0 : Width - rw;

                var th = Pane.Chart.RulerStyle.Font.Height; // Host.CurrentFontHeight;
                var txtRect = new RectangleF(x, m_Y-(th/2)-1, rw, th+2);

                //var txtRect1 = new RectangleF(x, m_Y-(th/2)-1, rw, (th/2)+1);
                //var txtRect2 = new RectangleF(x, m_Y, rw, (th/2)+1);


                const int ARROW_SZ = 8;
                const int PEN_ARROW_SZ = 6;

                //using(var gb1 = new LinearGradientBrush(txtRect, pen.Color, cbg, 90))
                //using(var gb2 = new LinearGradientBrush(txtRect, pen.Color, cbg, 270))
                using(var gb  = new SolidBrush(pen.Color))
                using(var ap  = new Pen(pen.Color, PEN_ARROW_SZ))
                using(var tbr = new SolidBrush(ctxt))
                {
                  gr.SmoothingMode = SmoothingMode.AntiAlias;

                  if (chart.VRulerPosition==VRulerPosition.Right)
                  {
                    ap.StartCap = LineCap.ArrowAnchor;
                    gr.DrawLine(ap, x-ARROW_SZ, m_Y, x, m_Y);
                  }
                  else
                  {
                    ap.EndCap = LineCap.ArrowAnchor;
                    gr.DrawLine(ap, x+rw, m_Y, x+rw+ARROW_SZ, m_Y);
                  }

                  gr.FillRectangle(gb, txtRect);
                  //gr.FillRectangle(gb2, txtRect2);

                  gr.DrawString(m_Level.DisplayValue, Pane.Chart.RulerStyle.Font, tbr,
                    txtRect.X + (TimeSeriesChart.VRULLER_HPADDING/2), txtRect.Y+1);
                }
              }
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


    protected SeriesView(string name, int order, string paneName = null)
    {
      if (name.IsNullOrWhiteSpace())
        throw new WFormsException(StringConsts.ARGUMENT_ERROR+"SeriesView.ctor(name==null)");

      m_Name = name;
      m_Order = order;
      m_PaneName = paneName;
    }

    private string m_Name;
    private int m_Order;
    private string m_PaneName;
    private bool m_Visible = true;

    private bool m_ShowYLevels;

    protected float m_MinScale;
    protected float m_MaxScale;


    /// <summary>
    /// Series name
    /// </summary>
    public string Name{ get{ return m_Name;}}

    /// <summary>
    /// Series order in the list of orders
    /// </summary>
    public int Order{ get{ return m_Order;}}


    /// <summary>
    /// Returns the width (horizontal size) of one sample, including any padding
    /// </summary>
    public abstract int SampleWidth{ get;}


    public float MinScale{ get{ return m_MinScale;}}
    public float MaxScale{ get{ return m_MaxScale;}}


    /// <summary>
    /// Shows/hides all views
    /// </summary>
    public bool Visible
    {
      get{ return m_Visible;}
      set{ m_Visible = value;}
    }

    /// <summary>
    /// Shows/hides all views
    /// </summary>
    public bool ShowYLevels
    {
      get{ return m_ShowYLevels;}
      set{ m_ShowYLevels = value;}
    }


    /// <summary>
    /// Returns the scale/pane name which is either assigned in .ctor or taken from DefaultPaneName. Series redering is directed in the named panes
    /// Every pane has its own Y scale
    /// </summary>
    public string PaneName
    {
      get
      {
        return m_PaneName.IsNotNullOrWhiteSpace() ? m_PaneName : DefaultPaneName;
      }
    }


    /// <summary>
    /// Returns the default scale/pane name which is used if PaneName is not assigned in .ctor Series redering is directed in the named panes
    /// Every pane has its own Y scale
    /// </summary>
    public abstract string DefaultPaneName { get;}


    /// <summary>
    /// Override to build elements that render the data by adding drawable elements to the chart.
    /// Returns how many samples could fit
    /// </summary>
    public abstract int BuildElements(TimeSeriesChart chart, PlotPane pane, TimeSeries series, int maxSampleWidth);
  }


  public enum VScaleZoomAlign{Mid=0, Top, Bottom}

  /// <summary>
  /// Denotes a view that visualizes series data in a chart
  /// </summary>
  public abstract class SeriesView<TSeries, TSample> : SeriesView where TSeries : TimeSeries
                                                                  where TSample : class
  {
    protected SeriesView(string name, int order, string paneName = null) : base(name, order, paneName)
    {
    }

    protected virtual VScaleZoomAlign VScaleZoomAlignment { get{ return VScaleZoomAlign.Mid;} }

    /// <summary>
    /// Builds elements that render the data by adding drawable elements to the chart.
    /// Returns how many samples could fit
    /// </summary>
    public sealed override int BuildElements(TimeSeriesChart chart, PlotPane pane, TimeSeries series, int maxSampleWidth)
    {
        var cdata = series as TSeries;
        if (cdata==null) throw new WFormsException("{0} requires {1} source".Args(GetType().FullName, typeof(TSeries).FullName));

        var drawData = GetDataPerScroll(chart, cdata);

        if (!drawData.Any()) return 0;//nothing to draw


        var x = chart.VRulerPosition == VRulerPosition.Left ? chart.VRulerWidth + 1 : 1;

        if (pane.Zoom != 1.0f)
          x = (int)(x/pane.Zoom);

        var xcutof = chart.VRulerPosition == VRulerPosition.Right ? pane.Width - 1 - chart.VRulerWidth : pane.Width;
        var fitSamples = 0;

        var canFitSamples = (int)(pane.Width / ((maxSampleWidth+1) * pane.Zoom)) + 2;//safe margin

        float minScale;
        float maxScale;
        CalcMinMaxScale(series, drawData.Take(canFitSamples), out minScale, out maxScale);

        //adjust scale for zoom
        if (pane.Zoom!=1.0f)
        {
          switch (this.VScaleZoomAlignment)
          {
             case VScaleZoomAlign.Bottom:
              {
                maxScale = maxScale / pane.Zoom;
                break;
              }

             case VScaleZoomAlign.Top:
              {
                minScale = minScale / pane.Zoom;
                break;
              }

             default: //case VScaleZoomAlign.Mid:
              {
                var range = maxScale - minScale;
                var originalMid = minScale + (range  / 2f);

                range = range / pane.Zoom;

                minScale = originalMid - (range / 2f);
                maxScale = originalMid + (range / 2f);
                break;
              }
          }//switch
        }

        m_MinScale = minScale;
        m_MaxScale = maxScale;

        pane.SetMinMaxScale(minScale, maxScale, false);

        //1. Try to build whole view from 1 Element
        var elm = MakeSeriesElement(chart, pane, drawData, x, minScale, maxScale, maxSampleWidth, out fitSamples);

        if (elm!=null)
        {
          elm.Visible = true;
        }
        else
        {
          //2. Make elements per sample
          foreach(var candle in drawData)
          {
            elm = MakeSampleElement(chart, pane, candle, x, minScale, maxScale);
            var edx = elm.DisplayRegion.Left;
            var edw = elm.DisplayRegion.Width;
            if (edx + edw + 1 >= xcutof)
            {
              elm.Dispose();
              break;
            }
            elm.Visible = true;
            x += maxSampleWidth;
            x++;
            fitSamples++;
          }
        }

        //Build level elements
        if (ShowYLevels)
        {
          foreach(var level in series.YLevels.OrderedValues)
          {
             if (!level.Visible) continue;
             elm = MakeYLevelElement(chart, pane, level, minScale, maxScale);
             elm.Visible = true;
          }
        }

        return fitSamples;
      }

      protected abstract Elements.Element MakeSampleElement(TimeSeriesChart chart,
                                                            PlotPane pane,
                                                            TSample sample,
                                                            int x,
                                                            float minScale,
                                                            float maxScale);

      protected virtual Elements.Element MakeSeriesElement( TimeSeriesChart chart,
                                                            PlotPane pane,
                                                            IEnumerable<TSample> data,
                                                            int xStart,
                                                            float minScale,
                                                            float maxScale,
                                                            int maxSampleWidth,
                                                            out int fitSamplesCount)
      {
         fitSamplesCount = 0;
         return null;
      }


      protected virtual Elements.Element MakeYLevelElement( TimeSeriesChart chart,
                                                            PlotPane pane,
                                                            TimeSeries.YLevel level,
                                                            float minScale,
                                                            float maxScale)
      {
         return new YLevelElement(pane, level, minScale, maxScale);
      }


      protected IEnumerable<TSample> GetDataPerScroll(TimeSeriesChart chart, TSeries cdata)
      {
        return cdata.Data.Cast<TSample>().Skip(chart.HScrollPosition);
      }

      protected void CalcMinMaxScale(TimeSeries series, IEnumerable<TSample> data, out float minScale, out float maxScale)
      {
        DoCalcMinMaxScale(data, out minScale, out maxScale);

        if (series.YLevels.Count>0)
        {
          var minlvl = minScale;
          var maxlvl = maxScale;

          var any = false;
          series.YLevels.Where( lvl => lvl.Visible && lvl.AffectsScale ).ForEach( lvl =>
          {
             var v = lvl.Value;
             if (v<minlvl) minlvl = v;
             if (v>maxlvl) maxlvl = v;
             any = true;
          });

          if (any)
          {
            if (minlvl<minScale) minScale = minlvl;
            if (maxlvl>maxScale) maxScale = maxlvl;
          }
        }

      }

      protected abstract void DoCalcMinMaxScale(IEnumerable<TSample> data, out float minScale, out float maxScale);


  }

}
