/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.Serialization.JSON;
using NFX.Security;

namespace NFX.Wave
{
  /// <summary>
  /// Decides whether the specifies WorkContext matches the requirements specified in the instance.
  /// The match may consider Request and Items properties of work context for match determination.
  /// Work matches do not belong to particular handler or filter, so they ARE STATELESS and their instances
  ///  can be used by multiple different processors (i.e. handlers and filters).
  /// </summary>
  public class WorkMatch : INamed, IOrdered
  {
      public const string CONFIG_MATCH_SECTION = "match";
      public const string CONFIG_VAR_SECTION = "var";
      public const string CONFIG_PATH_ATTR = "path";
      public const string CONFIG_NOT_PATH_ATTR = "not-path";

      public static readonly char[] LIST_DELIMITERS = new char[]{',',';','|'};


              /// <summary>
              /// Represents name/value tuple
              /// </summary>
              public struct NameValuePair
              {
                public NameValuePair(string name, string value)
                {
                  Name = name ?? string.Empty;
                  Value = value ?? string.Empty;
                }
                public readonly string Name;
                public readonly string Value;

                public override string ToString()
                {
                  return "['{0}' = '{1}']".Args(Name, Value);
                }

                /// <summary>
                /// Converts a string with a list of separated pairs into a list i.e. "a=1;b=2"
                /// </summary>
                public static List<NameValuePair> ParseList(string list)
                {
                  var result = new List<NameValuePair>();
                  var segs = list.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
                  foreach(var seg in segs)
                  {
                    var ieq = seg.IndexOf('=');
                    if (ieq>=1 && ieq<seg.Length-1)
                     result.Add(new NameValuePair(Uri.UnescapeDataString(seg.Substring(0, ieq)), Uri.UnescapeDataString(seg.Substring(ieq+1))));
                    else
                     result.Add(new NameValuePair(seg, null));
                  }
                  return result;
                }

                /// <summary>
                /// Joins NameValuePair sequence into a string list i.e. "a=1;b=2"
                /// </summary>
                public static string ToStringList(IEnumerable<NameValuePair> list)
                {
                  var result = new StringBuilder();
                  var first = true;
                  foreach(var pair in list)
                  {
                    result.Append("{0}={1}".Args(Uri.EscapeDataString(pair.Name), Uri.EscapeDataString(pair.Value)));
                    if (!first) result.Append(";");
                    first = false;
                  }
                  return result.ToString();
                }
              }



              /// <summary>
              /// Represents capture variable
              /// </summary>
              public class Variable : INamed
              {
                public const string QUERY_NAME_WC = "*";

                public Variable(string name)
                {
                  m_Name = name;
                  if (m_Name.IsNullOrWhiteSpace()) m_Name = Guid.NewGuid().ToString();
                }

                public Variable(IConfigSectionNode confNode)
                {
                  ConfigAttribute.Apply(this, confNode);
                  if (m_Name.IsNullOrWhiteSpace()) m_Name = Guid.NewGuid().ToString();
                }


                [Config]private string m_Name;
                [Config]private string m_QueryName;
                [Config]private string m_Default;
                [Config]private string m_MatchEquals;
                [Config]private string m_MatchContains;
                [Config]private bool   m_MatchCaseSensitive;

                //Todo in future add reg expressions

                /// <summary>Name of variable</summary>
                public string Name { get {return m_Name;} }

                /// <summary>Name of URI query variable to fetch into this variable</summary>
                public string QueryName { get {return m_QueryName;} set{m_QueryName = value;}}

                /// <summary>Default value if no URI match could be made</summary>
                public string Default { get {return m_Default;} set{m_Default = value;}}

                /// <summary>Optional value that must exactly equal the captured value for the whole match to succeed</summary>
                public string MatchEquals { get {return m_MatchEquals;} set{m_MatchEquals = value;}}

                /// <summary>Optional value that must be contained in the captured value for the whole match to succeed</summary>
                public string MatchContains { get {return m_MatchContains;} set{m_MatchContains = value;}}

                /// <summary>Optional value comparison case sensitivity for the whole match to succeed</summary>
                public bool   MatchCaseSensitive { get {return m_MatchCaseSensitive;} set{m_MatchCaseSensitive = value;}}
              }




      public WorkMatch(string name, int order)
      {
        if (name.IsNullOrWhiteSpace())
          name = "{0}({1})".Args(GetType().FullName, Guid.NewGuid());

        m_Name = name;
        m_Order = order;
      }

      public WorkMatch(IConfigSectionNode confNode)
      {
        if (confNode==null)
          throw new WaveException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(node==null)");

        m_Name = confNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        m_Order = confNode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0);

        if (m_Name.IsNullOrWhiteSpace())
          m_Name = "{0}({1})".Args(GetType().FullName, Guid.NewGuid());

        var ppattern = confNode.AttrByName(CONFIG_PATH_ATTR).Value;
        if (ppattern.IsNotNullOrWhiteSpace())
          m_PathPattern = new URIPattern( ppattern );

        var nppattern = confNode.AttrByName(CONFIG_NOT_PATH_ATTR).Value;
        if (nppattern.IsNotNullOrWhiteSpace())
          m_NotPathPattern = new URIPattern( nppattern );

        //Variables
        foreach(var vnode in confNode.Children.Where(c=>c.IsSameName(CONFIG_VAR_SECTION)))
          m_Variables.Register(new Variable(vnode) );

        ConfigAttribute.Apply(this, confNode);

        var permsNode = confNode[Permission.CONFIG_PERMISSIONS_SECTION];
        if (permsNode.Exists)
          m_Permissions = Permission.MultipleFromConf(permsNode);
      }

      private string m_Name;
      private int m_Order;
      private URIPattern m_PathPattern;
      private URIPattern m_NotPathPattern;

      private string m_TypeNsPrefix;

      private string[] m_Schemes;
      private string[] m_AcceptTypes;
      private bool     m_AcceptJson;
      private string[] m_ContentTypes;
      private string[] m_Hosts;
      private string[] m_Ports;
      private string[] m_UserAgents;
      private string[] m_UserHosts;
      private string[] m_Methods;
      private NameValuePair[] m_Cookies;
      private NameValuePair[] m_AbsentCookies;
      private NameValuePair[] m_Headers;
      private bool? m_IsLocal;
      private bool? m_IsSocialNetBot;
      private int?  m_ApiMinVer;
      private int?  m_ApiMaxVer;
      private IEnumerable<Permission> m_Permissions;

      private Registry<Variable> m_Variables = new Registry<Variable>();


      /// <summary>
      /// Returns the match instance name
      /// </summary>
      public string Name { get{ return m_Name;}}

      /// <summary>
      /// Returns the match order in handler registry. Order is used for URI pattern matching.
      /// Although Order property can change the match needs to be Unregistered/Registered again with the handler
      /// to change pattern matching order
      /// </summary>
      public int Order
      {
        get{ return m_Order;}
        set {m_Order = value;}
      }

      /// <summary>
      /// Returns the URIPattern matcher for URI path segment matching
      /// </summary>
      public URIPattern PathPattern
      {
        get { return m_PathPattern; }
        set { m_PathPattern = value;}
      }

      /// <summary>
      /// Returns the URIPattern matcher for reverse URI path segment matching
      /// </summary>
      public URIPattern NotPathPattern
      {
        get { return m_NotPathPattern; }
        set { m_NotPathPattern = value;}
      }

      /// <summary>
      /// Namespace prefix used for type lookups. The prefix uses '/' or '\' path separation char not '.'
      /// </summary>
      [Config]
      public string TypeNsPrefix
      {
        get { return m_TypeNsPrefix;}
        set { m_TypeNsPrefix = value;}
      }

      [Config]
      public string Schemes
      {
        get { return m_Schemes==null ? null : string.Join(",", m_Schemes); }
        set { m_Schemes = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string AcceptTypes
      {
        get { return m_AcceptTypes==null ? null : string.Join(",", m_AcceptTypes); }
        set { m_AcceptTypes = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      /// <summary>
      /// Shortcut to AcceptTypes contaioning application/json
      /// </summary>
      [Config]
      public bool AcceptJson
      {
        get{ return m_AcceptJson;}
        set{ m_AcceptJson = value;}
      }

      [Config]
      public string ContentTypes
      {
        get { return m_ContentTypes==null ? null : string.Join(",", m_ContentTypes); }
        set { m_ContentTypes = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string Hosts
      {
        get { return m_Hosts==null ? null : string.Join(",", m_Hosts); }
        set { m_Hosts = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string Ports
      {
        get { return m_Ports==null ? null : string.Join(",", m_Ports); }
        set { m_Ports = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string UserAgents
      {
        get { return m_UserAgents==null ? null : string.Join(",", m_UserAgents); }
        set { m_UserAgents = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string UserHosts
      {
        get { return m_UserHosts==null ? null : string.Join(",", m_UserHosts); }
        set { m_UserHosts = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string Methods
      {
        get { return m_Methods==null ? null : string.Join(",", m_Methods); }
        set { m_Methods = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string Cookies
      {
        get { return m_Cookies==null ? null : NameValuePair.ToStringList(m_Cookies); }
        set { m_Cookies = value.IsNullOrWhiteSpace() ? null : NameValuePair.ParseList(value).ToArray(); }
      }

      [Config]
      public string AbsentCookies
      {
        get { return m_AbsentCookies==null ? null : NameValuePair.ToStringList(m_AbsentCookies); }
        set { m_AbsentCookies = value.IsNullOrWhiteSpace() ? null : NameValuePair.ParseList(value).ToArray(); }
      }

      [Config]
      public string Headers
      {
        get { return m_Headers==null ? null : NameValuePair.ToStringList(m_Headers); }
        set { m_Headers = value.IsNullOrWhiteSpace() ? null : NameValuePair.ParseList(value).ToArray(); }
      }

      [Config]
      public bool? IsLocal
      {
        get { return m_IsLocal; }
        set { m_IsLocal = value; }
      }

      [Config]
      public bool? IsSocialNetBot
      {
        get { return m_IsSocialNetBot; }
        set { m_IsSocialNetBot = value;}
      }

      [Config]
      public int? ApiMinVer
      {
        get { return m_ApiMinVer; }
        set { m_ApiMinVer = value; }
      }

      [Config]
      public int? ApiMaxVer
      {
        get { return m_ApiMaxVer; }
        set { m_ApiMaxVer = value; }
      }

      public IEnumerable<Permission> Permissions
      {
        get { return m_Permissions;}
        set { m_Permissions = value;}
      }

      /// <summary>
      /// Returns registry of variables. May register/unregister variables at runtime
      /// </summary>
      public Registry<Variable> Variables { get { return m_Variables;}}


      /// <summary>
      /// Decides whether the specified WorkContext makes the match per this instance and if it does, returns the match variables.
      /// Returns null if match was not made
      /// </summary>
      public virtual JSONDataMap Make(WorkContext work)
      {
        if (
            !Check_Methods(work) ||
            !Check_AcceptTypes(work) ||
            !Check_ContentTypes(work) ||
            !Check_IsLocal(work) ||
            !Check_Hosts(work) ||
            !Check_Ports(work) ||
            !Check_Schemes(work) ||
            !Check_UserAgents(work)||
            !Check_UserHosts(work) ||
            !Check_Permissions(work) ||
            !Check_Cookies(work) ||
            !Check_AbsentCookies(work) ||
            !Check_IsSocialNetBot(work) ||
            !Check_Headers(work) ||
            !Check_ApiVersions(work)
           ) return null;

        JSONDataMap result = null;

        if (m_PathPattern!=null)
        {
          result = m_PathPattern.MatchURIPath(work.Request.Url);
          if (result==null) return null;
        }

        if (m_NotPathPattern!=null)
        {
          if (m_NotPathPattern.MatchURIPath(work.Request.Url)!=null) return null;
        }

        if (!Check_VariablesAndGetValues(work, ref result)) return null;

        return result ?? new JSONDataMap(false);
      }

      protected virtual bool Check_Schemes(WorkContext work)
      {
        if (m_Schemes==null) return true;
        return m_Schemes.Any(s=>s.Equals(work.Request.Url.Scheme, StringComparison.OrdinalIgnoreCase));
      }

      protected virtual bool Check_AcceptTypes(WorkContext work)
      {
        if (m_AcceptJson)
        {
          if (!work.RequestedJSON) return false;
        }

        if (m_AcceptTypes==null) return true;
        var atps = work.Request.AcceptTypes;
        if (atps==null || atps.Length==0) return false;

        var found = false;

        foreach(var at in m_AcceptTypes)
            if (atps.Any(t=>at.Equals(t, StringComparison.OrdinalIgnoreCase)))
            {
              found = true;
              break;
            }
        return found;
      }

      protected virtual bool Check_ContentTypes(WorkContext work)
      {
        if (m_ContentTypes==null) return true;
        var ct = work.Request.ContentType;
        if (ct.IsNullOrWhiteSpace()) return false;

        return m_ContentTypes.Any(t=>ct.Equals(t, StringComparison.OrdinalIgnoreCase));
      }


      protected virtual bool Check_Hosts(WorkContext work)
      {
        if (m_Hosts==null) return true;
        return m_Hosts.Any(h=>h.Equals(work.Request.Url.Host, StringComparison.OrdinalIgnoreCase));
      }

      protected virtual bool Check_Ports(WorkContext work)
      {
        if (m_Ports==null) return true;
        return m_Ports.Any(p=>p.Equals(work.Request.Url.Port.ToString(), StringComparison.Ordinal));
      }

      protected virtual bool Check_UserAgents(WorkContext work)
      {
        if (m_UserAgents==null) return true;
        return m_UserAgents.Any(ua=> work.Request.UserAgent.IndexOf(ua, StringComparison.InvariantCultureIgnoreCase)>=0);
      }

      protected virtual bool Check_UserHosts(WorkContext work)
      {
        if (m_UserHosts==null) return true;
        return m_UserHosts.Any(uh=> work.Request.UserHostName.IndexOf(uh, StringComparison.InvariantCultureIgnoreCase)>=0);
      }

      protected virtual bool Check_Methods(WorkContext work)
      {
        if (m_Methods==null) return true;
        return m_Methods.Any(m => m.Equals(work.Request.HttpMethod, StringComparison.OrdinalIgnoreCase));
      }

      protected virtual bool Check_IsLocal(WorkContext work)
      {
        if (!m_IsLocal.HasValue) return true;
        return work.Request.IsLocal == m_IsLocal;
      }

      protected virtual bool Check_IsSocialNetBot(WorkContext work)
      {
        if (!m_IsSocialNetBot.HasValue) return true;
        var isBot = Web.Social.SocialNetwork.IsAnySocialNetBotUserAgent(work.Request.UserAgent);
        return m_IsSocialNetBot == isBot;
      }

      protected virtual bool Check_VariablesAndGetValues(WorkContext work, ref JSONDataMap result)
      {
        if (m_Variables.Count==0) return true;

        result = result ?? new JSONDataMap(false);
        foreach(var cvar in m_Variables)
        {
          if (cvar.QueryName==Variable.QUERY_NAME_WC)
          {
            foreach(var qk in work.Request.QueryString.AllKeys)
              if (qk.IsNotNullOrWhiteSpace())//20150528 DKh  fixed: ?a=1&b
               result[qk] = work.Request.QueryString[qk];
            continue;
          }


          var qv = cvar.QueryName.IsNotNullOrWhiteSpace() ? work.Request.QueryString[cvar.QueryName] : string.Empty;
          if (qv.IsNullOrWhiteSpace()) qv = cvar.Default ?? string.Empty;
          result[cvar.Name] = qv;

          if (cvar.MatchEquals.IsNotNullOrWhiteSpace())
            if (!cvar.MatchEquals.Equals(qv, cvar.MatchCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)) return false;

          if (cvar.MatchContains.IsNotNullOrWhiteSpace())
            if (qv.IndexOf(cvar.MatchContains, cvar.MatchCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)<0) return false;
        }

        return true;
      }


      protected virtual bool Check_Permissions(WorkContext work)
      {
        if (m_Permissions==null) return true;
        var failed = m_Permissions.FirstOrDefault(prm => !prm.Check(work.Session));
        return failed == null;
      }

      protected virtual bool Check_Cookies(WorkContext work)
      {
        if (m_Cookies==null) return true;
        foreach(var pair in m_Cookies)
        {
          var cookie = work.Request.Cookies[pair.Name];
          if (cookie==null) continue;
          if (string.Equals( cookie.Value,
                             pair.Value,
                             StringComparison.InvariantCultureIgnoreCase)) return true;
        }
        return false;
      }

      protected virtual bool Check_AbsentCookies(WorkContext work)
      {
        if (m_AbsentCookies==null) return true;
        foreach(var pair in m_AbsentCookies)
        {
          var cookie = work.Request.Cookies[pair.Name];
          if (cookie==null) return true;
          if (!string.Equals( cookie.Value,
                              pair.Value,
                              StringComparison.InvariantCultureIgnoreCase)) return true;
        }
        return false;
      }

      protected virtual bool Check_Headers(WorkContext work)
      {
        if (m_Headers==null) return true;
        foreach(var pair in m_Headers)
          if (string.Equals( work.Request.Headers[pair.Name],
                             pair.Value,
                             StringComparison.InvariantCultureIgnoreCase)) return true;
        return false;
      }

      protected virtual bool Check_ApiVersions(WorkContext work)
      {
        if (!m_ApiMinVer.HasValue && !m_ApiMaxVer.HasValue) return true;

        var hdr = work.Request.Headers[SysConsts.HEADER_API_VERSION];

        if (hdr.IsNullOrWhiteSpace()) return false;

        int v;
        if (!Int32.TryParse(hdr, out v)) return false;

        if (m_ApiMinVer.HasValue && m_ApiMinVer.Value>v) return false;
        if (m_ApiMaxVer.HasValue && m_ApiMaxVer.Value<v) return false;

        return true;
      }

  }
}
