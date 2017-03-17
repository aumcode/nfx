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
    public const string CONFIG_EVENT_PREFIX_ATTR = "on-";
    public const string CONFIG_PRETTIFY_ATTR = "pretty";

    public const string JS_USE_CTX_VARIABLE = "ljs_useCtx";
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
    private string m_LineEnding = "";
    #endregion

    #region Public

    public void Configure(IConfigSectionNode node)
    {
      if (node == null || !node.Exists) return;

      var attr = node.AttrByName(CONFIG_PRETTIFY_ATTR);
      if (attr != null && attr.Exists && attr.ValueAsBool(false))
      {
        m_LineEnding = System.Environment.NewLine;
      }
    }

    public string Generate(IConfigSectionNode node)
    {
      int counter = 0;
      return createElement(node, null, ref counter);
    }

    #endregion

    private string createElement(IConfigSectionNode node, string root, ref int counter)
    {
      var isRootNode = root.IsNullOrWhiteSpace();//this means that this is a root node
      var sb = new StringBuilder();
      var elemId = "ljs_{0}".Args(++counter);

      if (isRootNode) sb.AppendFormat("var {0} = WAVE.isObject(ctx);{1}", JS_USE_CTX_VARIABLE, m_LineEnding);
      sb.AppendFormat("var {0} = document.createElement('{1}');{2}", elemId, MiscUtils.EscapeJSLiteral(node.Name), m_LineEnding);


      if (node.Value.IsNotNullOrWhiteSpace()) 
      {
        var v = makeValueVariable(node.Value, ++counter, ref sb);
        sb.AppendFormat("{0}.innerText = {1};{2}", elemId, v, m_LineEnding);
      }

      foreach(var attr in node.Attributes)
      {
        var value = attr.Value;
        if (value.IsNullOrWhiteSpace())
          continue;

        var name = MiscUtils.EscapeJSLiteral(attr.Name);
        if (name.StartsWith(CONFIG_EVENT_PREFIX_ATTR, StringComparison.Ordinal))
          sb.AppendFormat("{0}.addEventListener('{1}', {2}, false);{3}", elemId, name.Replace(CONFIG_EVENT_PREFIX_ATTR, ""), value, m_LineEnding);
        else {
          var v = makeValueVariable(value, ++counter, ref sb);
          sb.AppendFormat("{0}.setAttribute('{1}', {2});{3}", elemId, name, v, m_LineEnding);
        }
      }
      foreach(var child in node.Children)
      {
        sb.Append(createElement(child, elemId, ref counter));
      }
      if (isRootNode)
      {
        sb.AppendFormat("if (typeof(root) !== 'undefined' && root !== null) {{{0}", m_LineEnding);
        sb.AppendFormat("if (WAVE.isString(root)){0}", m_LineEnding);
        sb.AppendFormat("root = WAVE.id(root);{0}", m_LineEnding);
        sb.AppendFormat("if (WAVE.isObject(root)){0}", m_LineEnding);
        sb.AppendFormat("root.appendChild({0});{1}", elemId, m_LineEnding);
        sb.AppendFormat("}}");
      }
      else
        sb.AppendFormat("{0}.appendChild({1});{2}", root, elemId, m_LineEnding);

      return sb.ToString();
    }

    private string makeValueVariable(string value, int seed, ref StringBuilder result)
    {
      var valueId = "ljs_{0}".Args(seed);
      result.AppendFormat("var {0} = \"{1}\";{2}", valueId, MiscUtils.EscapeJSLiteral(value), m_LineEnding);
      result.AppendFormat("{1} = {0} ? WAVE.strHTMLTemplate({1}, ctx) : {1};{2}".Args(JS_USE_CTX_VARIABLE, valueId, m_LineEnding));
      return valueId;
    }
  }
}
