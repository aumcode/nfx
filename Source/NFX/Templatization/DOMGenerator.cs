using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NFX.Environment;

namespace NFX.Templatization
{
  public class DOMGenerator : IConfigurable
  {
    #region CONSTS
    public const string EVT_PREFIX_ATTR = "on-";
    public const string CONFIG_PRETTIFY_ATTR = "pretty";
    #endregion

    #region Static
    private static DOMGenerator s_Default;

    public static DOMGenerator Default
    {
      get
      {
        if (s_Default == null) s_Default = new DOMGenerator();
        return s_Default;
      }
    }
    #endregion

    #region private
    private string m_LineEnd = System.Environment.NewLine;
    #endregion

    #region Public

    public void Configure(IConfigSectionNode node)
    {
      if (node == null || !node.Exists) return;

      var attr = node.AttrByName(CONFIG_PRETTIFY_ATTR);
      if (attr != null && attr.Exists && attr.ValueAsBool(true))
      {
        m_LineEnd = System.Environment.NewLine;
      }
    }

    public string Generate(IConfigSectionNode node)
    {
      string elemId;
      int counter = 0;
      return createElement(node, null, ref counter, out elemId);
    }

    #endregion

    private string createElement(IConfigSectionNode node, string parent, ref int counter, out string elemId)
    {
      var sb = new StringBuilder();
      elemId = "ljs_{0}".Args(++counter);
      sb.AppendFormat("var {0} = document.createElement('{1}');{2}", elemId, node.Name, m_LineEnd);
      foreach(var attr in node.Attributes)
      {
        var value = attr.Value;
        if (value.IsNullOrWhiteSpace())
          continue;

        var name = attr.Name;
        if (name.StartsWith(EVT_PREFIX_ATTR, StringComparison.Ordinal))
          sb.AppendFormat("{0}.addEventListener('{1}', {2}, false);{3}", elemId, name.Replace(EVT_PREFIX_ATTR, "").ToLower(), value, m_LineEnd);
        else
          sb.AppendFormat("{0}.setAttribute('{1}', '{2}');{3}", elemId, name, value, m_LineEnd);
      }
      foreach(var child in node.Children)
      {
        string cel;
        sb.Append(createElement(child, elemId, ref counter, out cel));
      }
      //if (parent.IsNullOrWhiteSpace())
      //  throw new TemplatizationException("Parent node does not specified: {0}".Args(node.ToLaconicString()));

      sb.AppendFormat("{0}.appendChild({1});{2}", parent, elemId, m_LineEnd);
      return sb.ToString();
    }
  }
}
