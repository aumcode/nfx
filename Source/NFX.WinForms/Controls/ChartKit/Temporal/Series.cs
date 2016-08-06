using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

using NFX.Financial.Market;

namespace NFX.WinForms.Controls.ChartKit.Temporal
{

  /// <summary>
  /// Base for time series. This class is NOT thread-safe
  /// </summary>
  public abstract class TimeSeries : DisposableObject, INamed, IOrdered
  {
    public const int MAX_SAMPLES_DEFAULT = 32 * 1024;
    public const int MAX_SAMPLES_MIN = 16;

    #region Levels

        /// <summary>
        /// Represents a named level that gets represented on a vertical scale, usually as the horizontal line
        /// that goes across the chart
        /// </summary>
        public class YLevel : INamed, IOrdered
        {
          public YLevel(string name, int order, Style style = null)
          {
            if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();

            m_Name = name;
            m_Order = order;
            Visible = true;
            AffectsScale = true;
            HLineStyle = new LineStyle{ Color = style == null ? Color.FromArgb(240, 255, 100, 0) : style.BGColor,
                                        Width = 1,
                                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dot};
            Style = new Style(null, null);
            ValueFormat = "n2";
          }

          private string m_Name;
          private int    m_Order;

          public string  Name { get{ return m_Name; }}
          public int     Order{ get{ return m_Order;}}

          /// <summary>
          /// The value is in primary series unit
          /// </summary>
          public float   Value { get; set;}

          public string  ValueFormat{ get; set;}


          public string  DisplayValue
          {
            get
            {
              return ValueFormat.IsNotNullOrWhiteSpace() ?
                           Value.ToString(ValueFormat)
                           : Value.ToString();
            }
          }

          public string Text { get; set;}
          public object AssociatedData { get; set;}

          public bool Visible { get; set;}

          //When true may adjust pane vertical scale size,
          // i.e. if the value is outside the display range and this value is false,
          //the horizontal level is not going to be visible at a normal zoom
          public bool AffectsScale { get; set;}

          public LineStyle HLineStyle  { get; set;}

          [Description("Style of the horizontal line marker")]
          public Style     Style { get; private set; }
        }

    #endregion

    protected TimeSeries(string name, int order, TimeSeries parent = null)
    {
      if (name.IsNullOrWhiteSpace())
        throw new WFormsException(StringConsts.ARGUMENT_ERROR+"TimeSeries.ctor(name==null)");

      m_Name = name;
      m_Order = order;
      if (parent!=null)
      {
        m_Parent.m_Children.RegisterOrReplace( this );
        m_Parent = parent;
      }
    }

    protected override void Destructor()
    {
      if (m_Parent!=null)
      {
        m_Parent.m_Children.Unregister( this );
        m_Parent = null;
      }
    }

    private string m_Name;
    private int m_Order;
    private TimeSeries m_Parent;
    private bool m_Visible = true;
    private OrderedRegistry<TimeSeries> m_Children = new OrderedRegistry<TimeSeries>();
    private OrderedRegistry<SeriesView> m_Views = new OrderedRegistry<SeriesView>();

    private LinkedList<ITimeSeriesSample> m_Data = new LinkedList<ITimeSeriesSample>();
    private int m_MaxSamples = MAX_SAMPLES_DEFAULT;

    private OrderedRegistry<YLevel> m_YLevels = new OrderedRegistry<YLevel>();

    /// <summary>
    /// Returns parent of this series or null
    /// </summary>
    public TimeSeries Parent{ get{ return m_Parent;}}

    /// <summary>
    /// Series name
    /// </summary>
    public string Name{ get{ return m_Name;}}

    /// <summary>
    /// Series order in the list of orders
    /// </summary>
    public int Order{ get{ return m_Order;}}


    public int MaxSamples
    {
      get{ return m_MaxSamples;}
      set
      {
        const int MIN_CNT = 32;

        m_MaxSamples = value < MIN_CNT ? MIN_CNT : value;
      }
    }

    /// <summary>
    /// Returns the registry of horizontal levels on Y access
    /// </summary>
    public OrderedRegistry<YLevel> YLevels
    {
      get{ return m_YLevels;}
    }

    /// <summary>
    /// Returns the children of this series
    /// </summary>
    public IOrderedRegistry<TimeSeries> Children
    {
      get { return m_Children; }
    }

    /// <summary>
    /// Views that visualize the series data (may be more than one),
    /// for example a market data may have 3 views that show open.close and volume as 3 views based on the same source
    /// </summary>
    public OrderedRegistry<SeriesView> Views
    {
      get{ return m_Views;}
    }


    /// <summary>
    /// Shows/hides all views
    /// </summary>
    public bool Visible
    {
      get{ return m_Visible;}
      set{ m_Visible = value;}
    }

    /// <summary>
    /// Sample count in this series, excluding children
    /// </summary>
    public int SampleCount { get {return m_Data.Count;}}


    /// <summary>
    /// Returns data in the series in natural time order
    /// </summary>
    public IEnumerable<ITimeSeriesSample> Data { get{ return m_Data;}}

    /// <summary>
    /// Returns data in the series in the reveresed time order
    /// </summary>
    public IEnumerable<ITimeSeriesSample> DataReveresed
    {
      get
      {
          var current = m_Data.Last;

          while(current!=null)
          {
            yield return current.Value;
            current = current.Previous;
          }
      }
    }

    /// <summary>
    /// Returns last X data samples in the series in natural order. It may return less samples than X
    /// </summary>
    public IEnumerable<ITimeSeriesSample> LastXSamplesData(int x)
    {
      var current = m_Data.Last;
      if (current==null) yield break;
      for(var i=0; i<x; i++)
      {
        var prior = current.Previous;
        if (prior==null) break;
        current = prior;
      }

      while(current!=null)
      {
        yield return current.Value;
        current = current.Next;
      }
    }



    /// <summary>
    /// Returns the maximum width of one sample extracted from all views
    /// </summary>
    public int GetMaxSampleWidth(bool includeChildren = true)
    {
      if (!this.m_Visible) return 0;

      var result = m_Views.Where(v => v.Visible).DefaultIfEmpty().Max( v => v!=null? v.SampleWidth : 0 );

      if (!includeChildren) return result;

      return Math.Max(result, m_Children.DefaultIfEmpty().Max( cv =>  cv!=null? cv.GetMaxSampleWidth(true) : 0));
    }


    /// <summary>
    /// Gets a list of distinct pane names ordered by occurence in this and all child series if includeChildren is true
    /// </summary>
    public IEnumerable<string> GetPaneNames(bool includeChildren = true)
    {
      if (!this.m_Visible) return Enumerable.Empty<string>();

      var paneNames = m_Views.OrderedValues.Where(v => v.Visible).Select( v => v.PaneName).Distinct();

      if (!includeChildren) return paneNames;

      foreach(var child in m_Children.OrderedValues)
       paneNames = paneNames.Union(child.GetPaneNames(true));

      return paneNames;
    }

    /// <summary>
    /// Returns how many samples fit
    /// </summary>
    internal int BuildViews(TimeSeriesChart chart, int maxSampleWidth)
    {
      var fitSamples = 0;
      foreach(var view in m_Views.OrderedValues.Where( v => v.Visible))
      {
        var pane = chart.Panes[view.PaneName];
        if (pane==null)
         throw new WFormsException("Pane '{0}' not found in chart".Args(view.PaneName));

        var fit = view.BuildElements(chart, pane, this, maxSampleWidth);
        if (fit>fitSamples)
         fitSamples = fit;
      }

      foreach(var child in m_Children.OrderedValues.Where( c => c.Visible))
       child.BuildViews(chart, maxSampleWidth);

      return fitSamples;
    }

    /// <summary>
    /// Adds sample to the series at the appropriate position.
    /// This method respects MaxSamples and first deletes older samples making room for new additions
    /// </summary>
    protected void Add(ITimeSeriesSample sample)
    {
      if (sample==null) throw new WFormsException(StringConsts.ARGUMENT_ERROR+"TimeSeries.Add(sample==null)");

      //remove data over max samples
      while(m_Data.Count >= m_MaxSamples)
        m_Data.RemoveFirst();


      var dt = sample.TimeStamp;
      var head = m_Data.First;
      if (head==null || head.Value.TimeStamp >= dt)
      {
          m_Data.AddFirst(sample);
          return;
      }

      var last = m_Data.Last;
      if (last.Value.TimeStamp <= dt)
      {
        m_Data.AddLast(sample);
        return;
      }


      var d1 = dt - head.Value.TimeStamp;
      var d2 = last.Value.TimeStamp - dt;

      if (d1<d2)
      {
          var node = head;
          while(node!=null)
          {
            if (node.Value.TimeStamp>=dt)
            {
              m_Data.AddBefore(node, sample);
              return;
            }
            node = node.Next;
          }
          m_Data.AddLast(sample);
      }
      else
      {
          var node = last;
          while(node!=null)
          {
            if (node.Value.TimeStamp<=dt)
            {
              m_Data.AddAfter(node, sample);
              return;
            }
            node = node.Previous;
          }
          m_Data.AddFirst(sample);
      }
    }

    /// <summary>
    /// Replace last data sample.
    /// This function requires that the new sample has the same timestamp
    /// as the last sample in the time series data.
    /// </summary>
    public void ReplaceLast(ITimeSeriesSample sample)
    {
      var last = m_Data.Last;
      if (last == null)
        throw new WFormsException(StringConsts.ARGUMENT_ERROR + "last sample not assigned!");

      if (sample.TimeStamp < last.Value.TimeStamp)
        throw new WFormsException(StringConsts.ARGUMENT_ERROR +
          "inconsistent time stamp of the new sample (expected: {0}, got: {1}!".Args(
            last.Value.TimeStamp, sample.TimeStamp));

      m_Data.Last.Value = sample;
    }

    /// <summary>
    /// Deletes sample from the set. This method is not efficient as it does linear list scan
    /// </summary>
    public bool Delete(ITimeSeriesSample sample)
    {
      return m_Data.Remove(sample);
    }

    public void Clear()
    {
      m_Data.Clear();
    }

    public void ReplaceSamples(IEnumerable<ITimeSeriesSample> data)
    {
      m_Data.Clear();
      foreach(var sample in data)
        this.Add(sample);
    }


  }//TimeSeries


  /// <summary>
  /// TimeSeries with types TSample
  /// </summary>
  public abstract class TimeSeries<TSample> : TimeSeries where TSample : ITimeSeriesSample
  {

    protected TimeSeries(string name, int order, TimeSeries parent = null) : base(name, order, parent)
    {
    }

    public void Add(TSample sample)
    {
      base.Add(sample);
    }

    public void ReplaceLast(TSample sample)
    {
      base.ReplaceLast(sample);
    }

    public new IEnumerable<TSample> Data { get{ return base.Data.Cast<TSample>(); } }

    public new IEnumerable<TSample> DataReveresed { get{ return base.DataReveresed.Cast<TSample>(); } }

    public new IEnumerable<TSample> LastXSamplesData(int x)
    {
       return base.LastXSamplesData(x).Cast<TSample>();
    }
  }





}
