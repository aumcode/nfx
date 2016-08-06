using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NFX;
using NFX.Financial.Market;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{



  public delegate void ChartPaneMouseEventHandler(object sender, ChartPaneMouseEventArgs args);

  public class ChartPaneMouseEventArgs : EventArgs
  {
    public enum MouseEventType
    {
        /// <summary>
        /// Mouse moved
        /// </summary>
        Move,

        /// <summary>
        /// Mouse was clicked
        /// </summary>
        Click,

        /// <summary>
        /// Mouse did not change but chart content changed under the mouse as-if mouse moved
        /// </summary>
        ChartUpdate
    }


    internal ChartPaneMouseEventArgs( MouseEventType type,
                                      TimeSeriesChart chart,
                                      PlotPane pane,
                                      MouseEventArgs mouse,
                                      ITimeSeriesSample sample,
                                      float value)
    {
      this.EventType = type;
      this.Chart = chart;
      this.Pane = pane;
      this.MouseEvent = mouse;
      this.SampleAtX = sample;
      this.ValueAtY = value;
    }

    public readonly MouseEventType EventType;
    public readonly TimeSeriesChart Chart;
    public readonly PlotPane Pane;
    public readonly MouseEventArgs MouseEvent;
    public readonly ITimeSeriesSample SampleAtX;
    public readonly float ValueAtY;
  }

}
