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

    public const string JS_USE_CTX_VAR = "ljs_useCtx_{0}";
    public const string JS_ROOT_VAR = "ljs_r_{0}";
    public const string JS_CTX_VAR = "ljs_ctx_{0}";
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
    private string m_LineEnding;
    private int m_Indent;
    private int m_IndexInSource;
    private string m_JsUseCtxVar;
    private string m_JsRootVar;
    private string m_JsCtxVar;
    private bool m_Pretty;
    #endregion

    #region Properties
    public string LineEnding
    {
      get { return m_LineEnding ?? (m_LineEnding = m_Pretty ? "\n" : ""); }
    }
    #endregion

    #region Public

    public void Configure(IConfigSectionNode node)
    {
      if (node == null || !node.Exists) return;

      var attr = node.AttrByName(CONFIG_PRETTIFY_ATTR);
      m_Pretty = attr != null && attr.Exists && attr.ValueAsBool(false);
    }

    public string Generate(IConfigSectionNode node, int indent, ref int indexInSource)
    {
      m_IndexInSource = indexInSource;
      m_Indent = indent;
      m_JsUseCtxVar = JS_USE_CTX_VAR.Args(m_IndexInSource);
      m_JsCtxVar = JS_CTX_VAR.Args(m_IndexInSource);
      m_JsRootVar = JS_ROOT_VAR.Args(m_IndexInSource);
      int counter = 0;
      return createElement(node, null, ref counter);
    }

    #endregion

    private string createElement(IConfigSectionNode node, string root, ref int counter)
    {
      var isRootNode = root.IsNullOrWhiteSpace();//this means that this is a root node
      var sb = new StringBuilder();
      var elemId = "ljs_{0}_{1}".Args(m_IndexInSource, ++counter);

      if (isRootNode) sb.AppendFormat("var {0} = WAVE.isObject(arguments[1]);{1}", m_JsUseCtxVar, LineEnding, getIndent());
      sb.AppendFormat("{3}var {0} = document.createElement('{1}');{2}", elemId, MiscUtils.EscapeJSLiteral(node.Name), LineEnding, getIndent());


      if (node.Value.IsNotNullOrWhiteSpace())
      {
        var v = evalValue(node.Value, ++counter, ref sb);
        sb.AppendFormat("{3}{0}.innerText = {1};{2}", elemId, v, LineEnding, getIndent());
      }

      foreach(var attr in node.Attributes)
      {
        var value = attr.Value;
        if (value.IsNullOrWhiteSpace())
          continue;

        var name = MiscUtils.EscapeJSLiteral(attr.Name);
        if (name.StartsWith(CONFIG_EVENT_PREFIX_ATTR, StringComparison.Ordinal))
          sb.AppendFormat("{4}{0}.addEventListener('{1}', {2}, false);{3}", elemId, name.Replace(CONFIG_EVENT_PREFIX_ATTR, ""), value, LineEnding, getIndent());
        else {
          var v = evalValue(value, ++counter, ref sb);
          sb.AppendFormat("{4}{0}.setAttribute('{1}', {2});{3}", elemId, name, v, LineEnding, getIndent());
        }
      }
      foreach(var child in node.Children)
      {
        sb.Append(createElement(child, elemId, ref counter));
      }
      if (isRootNode)
      {
        sb.AppendFormat("{2}var {0} = arguments[0];{1}", m_JsRootVar, LineEnding, getIndent());
        sb.AppendFormat("{2}if (typeof({0}) !== 'undefined' && {0} !== null) {{{1}", m_JsRootVar, LineEnding, getIndent());
        sb.AppendFormat("{2}if (WAVE.isString({0})){1}", m_JsRootVar, LineEnding, getIndent(2));
        sb.AppendFormat("{2}{0} = WAVE.id({0});{1}", m_JsRootVar, LineEnding, getIndent(4));
        sb.AppendFormat("{2}if (WAVE.isObject({0})){1}", m_JsRootVar, LineEnding, getIndent(2));
        sb.AppendFormat("{3}{0}.appendChild({1});{2}", m_JsRootVar, elemId, LineEnding, getIndent(4));
        sb.AppendFormat("{0}}}", getIndent());
      }
      else
        sb.AppendFormat("{3}{0}.appendChild({1});{2}{2}", root, elemId, LineEnding, getIndent());

      return sb.ToString();
    }

    private string evalValue(string value, int seed, ref StringBuilder result)
    {
      if (value.Length > 15)
      {
        var valueId = "ljsv_{0}_{1}".Args(m_IndexInSource, seed);
        result.AppendFormat("{3}var {0} = \"{1}\";{2}", valueId, MiscUtils.EscapeJSLiteral(value), LineEnding, getIndent());
        return "{0} ? WAVE.strHTMLTemplate({1}, ctx) : {1}".Args(m_JsUseCtxVar, valueId);
      }

      return "{0} ? WAVE.strHTMLTemplate(\"{1}\", ctx) : \"{1}\"".Args(m_JsUseCtxVar, MiscUtils.EscapeJSLiteral(value));
    }

    public string getIndent(int extraSpaces = 0)
    {
      return m_Pretty ? new String(' ', m_Indent + extraSpaces) : string.Empty;
    }
  }
}
