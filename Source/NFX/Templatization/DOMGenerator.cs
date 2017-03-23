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
    private string m_LineEnding = "";
    private int m_IndexInSource;
    private string m_JsUseCtxVar;
    private string m_JsRootVar;
    private string m_JsCtxVar;
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

    public string Generate(IConfigSectionNode node, ref int indexInSource)
    {
      m_IndexInSource = indexInSource;
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

      if (isRootNode) sb.AppendFormat("var {0} = WAVE.isObject(arguments[1]);{1}", m_JsUseCtxVar, m_LineEnding);
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
        sb.AppendFormat("var {0} = arguments[0];{1}", m_JsRootVar, m_LineEnding);
        sb.AppendFormat("if (typeof({0}) !== 'undefined' && {0} !== null) {{{1}", m_JsRootVar, m_LineEnding);
        sb.AppendFormat("if (WAVE.isString({0})){1}", m_JsRootVar, m_LineEnding);
        sb.AppendFormat("{0} = WAVE.id({0});{1}", m_JsRootVar, m_LineEnding);
        sb.AppendFormat("if (WAVE.isObject({0})){1}", m_JsRootVar, m_LineEnding);
        sb.AppendFormat("{0}.appendChild({1});{2}", m_JsRootVar, elemId, m_LineEnding);
        sb.AppendFormat("}}");
      }
      else
        sb.AppendFormat("{0}.appendChild({1});{2}", root, elemId, m_LineEnding);

      return sb.ToString();
    }

    private string makeValueVariable(string value, int seed, ref StringBuilder result)
    {
      var valueId = "ljsv_{0}_{1}".Args(m_IndexInSource, seed);
      result.AppendFormat("var {0} = \"{1}\";{2}", valueId, MiscUtils.EscapeJSLiteral(value), m_LineEnding);
      result.AppendFormat("{1} = {0} ? WAVE.strHTMLTemplate({1}, ctx) : {1};{2}".Args(m_JsUseCtxVar, valueId, m_LineEnding));
      return valueId;
    }
  }
}
