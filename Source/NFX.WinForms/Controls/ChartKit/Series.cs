using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.WinForms.Controls.ChartKit
{

  /// <summary>
  /// Base for series
  /// </summary>
  public abstract class Series : DisposableObject, INamed, IOrdered
  {
    protected Series(string name, int order, Series parent = null)
    {
      if (name.IsNullOrWhiteSpace())
        throw new WFormsException(StringConsts.ARGUMENT_ERROR+"Series.ctor(name==null)");

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
    private Series m_Parent;
    private bool m_Visible = true;
    private OrderedRegistry<Series> m_Children = new OrderedRegistry<Series>();
    private OrderedRegistry<SeriesView> m_Views = new OrderedRegistry<SeriesView>();


    
    /// <summary>
    /// Rtuens parent of this series or null
    /// </summary>
    public Series Parent{ get{ return m_Parent;}}
    
    /// <summary>
    /// Series name
    /// </summary>
    public string Name{ get{ return m_Name;}}

    /// <summary>
    /// Series order in the list of orders
    /// </summary>
    public int Order{ get{ return m_Order;}}


    /// <summary>
    /// Returns the children of this series
    /// </summary>
    public IOrderedRegistry<Series> Children
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
    /// Override to sample count in this series, excluding children
    /// </summary>
    protected abstract int SampleCount { get;}


    /// <summary>
    /// Returns the maximum width of one sample extracted from all views
    /// </summary>
    public int GetMaxSampleWidth(bool includeChildren = true) 
    { 
      if (!this.m_Visible) return 0;

      var result = m_Views.Where(v => v.Visible).Max( v => v.SampleWidth );

      if (!includeChildren) return result;

      return Math.Max(result, m_Children.Max( cv =>  cv.GetMaxSampleWidth(true)));
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

    internal void BuildViews(Chart chart)
    {
      foreach(var view in m_Views.OrderedValues.Where( v => v.Visible))
      {
        var pane = chart.Panes[view.PaneName];
        if (pane==null)
         throw new WFormsException("Pane '{0}' not found in chart".Args(view.PaneName));

        view.BuildElements(chart, pane, this);
      }
    }

  }


}
