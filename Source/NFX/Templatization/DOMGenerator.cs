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
    public const string CONFIG_ESCAPE_ATTR = "__";
    public const string CONFIG_PRETTIFY_ATTR = "pretty";
    public const string CONFIG_JS_PREFIX_ATTR = "?";
    public const string CONFIG_LJS_ID_ATTR = "ljsid";
    private readonly string[] CONFIG_BOOL_ATTRS = new string[] {"readonly", "disabled", "checked", "required", "selected"};

    public const string JS_ROOT_VAR = "Ør";
    public const string JS_CURRENT_ELEMENT = "?this";
    private readonly string[] JS_CODE_BLOCK_STARTS_WTIH = new string[] {"?for(", "?while(", "?do{", "?if("};
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
    private string m_JsRootVar;
    private bool m_Pretty;
    private Dictionary<string, string> m_MapLjsIdsToJsIds;
    #endregion

    #region Properties
    public string LineEnding
    {
      get { return m_LineEnding ?? (m_LineEnding = m_Pretty ? "\n" : string.Empty); }
    }

    public string Space
    {
      get { return m_Pretty ? " " : string.Empty; }
    }
    #endregion

    #region Public

    public void Configure(IConfigSectionNode node)
    {
      if (node == null || !node.Exists) return;

      var attr = node.AttrByName(CONFIG_PRETTIFY_ATTR);
      m_Pretty = attr != null && attr.Exists && attr.ValueAsBool(false);
    }

    public string Generate(IConfigSectionNode node, int indent, ref int indexInSource, ref Dictionary<string, string> dictIds)
    {
      m_IndexInSource = indexInSource;
      m_Indent = indent;
      m_JsRootVar = JS_ROOT_VAR.Args(m_IndexInSource);
      m_MapLjsIdsToJsIds = dictIds;

      var counter = 0;
      string id;
      return createElement(node, true, null, ref counter, 0, out id);
    }

    public string GenerateAnonymousFunction(IConfigSectionNode node, int indent, ref int indexInSource, ref Dictionary<string, string> dictIds)
    {
      m_IndexInSource = indexInSource;
      m_Indent = indent;
      m_JsRootVar = JS_ROOT_VAR.Args(m_IndexInSource);
      m_MapLjsIdsToJsIds = dictIds;

      m_Pretty = false;
      m_LineEnding = string.Empty;

      var counter = 0;
      string id;
      return "(function(){{{0}}})()".Args(createElement(node, true, null, ref counter, 0, out id));
    }

    #endregion

    private string createElement(IConfigSectionNode node, bool isRootNode, string container, ref int counter, int extraSpaces, out string elId)
    {
      var sb = new StringBuilder();
      var nName = node.Name;
      var hasChildren = node.HasChildren;
      var isJsNode = nName.StartsWith(CONFIG_JS_PREFIX_ATTR, StringComparison.Ordinal);
      var elemId = isJsNode ? container : "Ø{0}".Args(++counter);

      if (isRootNode)
      {
        sb.AppendFormat("var {0}{2}={2}arguments[0];{1}", m_JsRootVar, LineEnding, Space);
        sb.AppendFormat("{2}if{3}(WAVE.isString({0})){1}", m_JsRootVar, LineEnding, getIndent(extraSpaces), Space);
        sb.AppendFormat("{2}{0}{3}={3}WAVE.id({0});{1}", m_JsRootVar, LineEnding, getIndent(2 + extraSpaces), Space);
      }

      if (isJsNode)
        sb.AppendFormat("{0}{1}{4}{2}{3}", getIndent(extraSpaces), jsWrap(node.Name, elemId), hasChildren ? "{" : string.Empty, LineEnding, Space);
      else
      {
        sb.AppendFormat("{0}var {1}{4}={4}WAVE.ce('{2}');{3}", getIndent(extraSpaces), elemId, MiscUtils.EscapeJSLiteral(nName), LineEnding, Space);
        if (node.Value.IsNotNullOrWhiteSpace())
          sb.AppendFormat("{3}{0}.innerText{4}={4}{1};{2}", elemId, wrapValue(node.Value, elemId), LineEnding, getIndent(extraSpaces), Space);

        foreach(var attr in node.Attributes)
        {
          var value = attr.Value;
          value = value ?? string.Empty;

          var name = MiscUtils.EscapeJSLiteral(attr.Name);
          if (name.StartsWith(CONFIG_EVENT_PREFIX_ATTR, StringComparison.Ordinal))
            sb.AppendFormat("{4}{0}.addEventListener('{1}',{5}{2},{5}false);{3}", elemId, name.Replace(CONFIG_EVENT_PREFIX_ATTR, ""), value, LineEnding, getIndent(extraSpaces), Space);

          else if (name.EqualsOrdSenseCase(CONFIG_LJS_ID_ATTR))
          {
            if (m_MapLjsIdsToJsIds.ContainsKey(name))
              throw new TemplatizationException(StringConsts.TEMPLATE_JS_COMPILER_DUPLICATION_ID.Args(name));

            m_MapLjsIdsToJsIds.Add(value, elemId);
          }
          else
          {
            if (CONFIG_BOOL_ATTRS.Any(s => s.EqualsSenseCase(name)) || name.StartsWith(CONFIG_ESCAPE_ATTR))
              sb.AppendFormat("{0}{1}.{2}{5}={5}{3};{4}".Args(getIndent(extraSpaces), elemId, name, wrapValue(value, elemId), LineEnding, Space));
            else
              sb.AppendFormat("{4}{0}.setAttribute('{1}',{5}{2});{3}", elemId, name, wrapValue(value, elemId), LineEnding, getIndent(extraSpaces), Space);
          }
        }
      }

      var rId = string.Empty;
      foreach(var child in node.Children)
      {
        var pName = nName.Replace(" ", string.Empty).Replace("\n\r", string.Empty).Replace("\n", string.Empty);
        sb.Append(createElement(child, false, elemId, ref counter, isJsNode && JS_CODE_BLOCK_STARTS_WTIH.Any(s => pName.StartsWith(s, StringComparison.Ordinal)) ? extraSpaces + 2 : 0, out rId));
      }
      elId = isJsNode ? rId : elemId;

      if (!isJsNode)
      {
        if (container.IsNullOrWhiteSpace())
          sb.AppendFormat("{0}if{4}(WAVE.isObject({1})){4}{1}.appendChild({2});{3}", getIndent(extraSpaces), m_JsRootVar, elemId, LineEnding, Space);
        else
          sb.AppendFormat("{0}{1}.appendChild({2});{3}", getIndent(extraSpaces), container, elemId, LineEnding);
      }
      if (isJsNode && hasChildren)
        sb.AppendFormat("{0}}}{1}", getIndent(extraSpaces), LineEnding);

      if (isRootNode)
        sb.AppendFormat("{0}return {1};", getIndent(extraSpaces), elId);

      return sb.ToString();
    }

    private string wrapValue(string value, string currentElemId)
    {
      return value.StartsWith(CONFIG_JS_PREFIX_ATTR) ? jsWrap(value, currentElemId) : "'{0}'".Args(MiscUtils.EscapeJSLiteral(value));
    }

    private string jsWrap(string value, string currentElemId)
    {
      return value.Remove(0, 1).Replace(JS_CURRENT_ELEMENT, currentElemId);
    }

    public string getIndent(int extraSpaces = 0)
    {
      return m_Pretty ? new String(' ', m_Indent + extraSpaces) : string.Empty;
    }
  }
}
