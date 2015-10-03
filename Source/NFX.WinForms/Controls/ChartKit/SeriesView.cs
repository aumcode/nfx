using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.WinForms.Controls.ChartKit
{
  /// <summary>
  /// Denotes a view that visualizes series data in a chart
  /// </summary>
  public abstract class SeriesView : INamed, IOrdered
  {
    
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


    
     /// <summary>
    /// Shows/hides all views
    /// </summary>
    public bool Visible
    {
      get{ return m_Visible;}
      set{ m_Visible = value;} 
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
    /// Override to build elements that render the data by adding drawable elements to the chart 
    /// </summary>
    public abstract void BuildElements(Chart chart, PlotPane pane, Series series);
  }
}
