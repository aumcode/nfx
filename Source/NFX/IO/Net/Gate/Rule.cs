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
using NFX.Parsing;

namespace NFX.IO.Net.Gate
{
  /// <summary>
  /// Represents the named rule for NetGate
  /// </summary>
  public class Rule : INamed, IOrdered
  {
    public static string VAR_EXPRESSION_PREFIX = "$";
    public static readonly char[] LIST_DELIMITERS = new char[]{',',';','|'};

    public Rule(string name, int order, GateAction action)
    {
      m_Name = name.IsNullOrWhiteSpace()? Guid.NewGuid().ToString() : name;
      m_Order = order;
      m_Action = action;
    }

    public Rule(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      if (m_Name.IsNullOrWhiteSpace()) m_Name = Guid.NewGuid().ToString();
    }

    [Config]
    private string m_Name;

    [Config]
    private int m_Order;

    [Config(Default=GateAction.Deny)]
    private GateAction m_Action = GateAction.Deny;



    private string[] m_FromAddrs;
    private string[] m_ToAddrs;
    private string[] m_Methods;
    private string[] m_Services;
    private string[] m_URLFragments;
    private string[] m_FromGroups;
    private string[] m_ToGroups;
    private Evaluator m_FromExpression;
    private Evaluator m_ToExpression;


    public string Name    { get{return m_Name;}}
    public int    Order   { get{return m_Order;}}
    public GateAction Action  { get{return m_Action;}}

      [Config]
      public string FromAddrs
      {
        get { return m_FromAddrs==null ? null : string.Join(",", m_FromAddrs); }
        set { m_FromAddrs = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string ToAddrs
      {
        get { return m_ToAddrs==null ? null : string.Join(",", m_ToAddrs); }
        set { m_ToAddrs = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string Methods
      {
        get { return m_Methods==null ? null : string.Join(",", m_Methods); }
        set { m_Methods = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string Services
      {
        get { return m_Services==null ? null : string.Join(",", m_Services); }
        set { m_Services = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string URLFragments
      {
        get { return m_URLFragments==null ? null : string.Join(",", m_URLFragments); }
        set { m_URLFragments = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string FromGroups
      {
        get { return m_FromGroups==null ? null : string.Join(",", m_FromGroups); }
        set { m_FromGroups = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string ToGroups
      {
        get { return m_ToGroups==null ? null : string.Join(",", m_ToGroups); }
        set { m_ToGroups = value.IsNullOrWhiteSpace() ? null : value.Split(LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries); }
      }

      [Config]
      public string FromExpression
      {
        get { return m_FromExpression==null ? null : m_FromExpression.Expression; }
        set { m_FromExpression = value.IsNullOrWhiteSpace() ? null :  new Evaluator(value); }
      }

      [Config]
      public string ToExpression
      {
        get { return m_ToExpression==null ? null : m_ToExpression.Expression; }
        set { m_ToExpression = value.IsNullOrWhiteSpace() ? null :  new Evaluator(value); }
      }

    /// <summary>
    /// Cheks whether the rule is satisfied - all listed conditions are met. May lazily resolve addresses to groups
    /// </summary>
    public virtual bool Check(NetGate.State state, ITraffic traffic, ref Group fromGroup, ref Group toGroup)
    {
      if (
           !Check_FromAddrs(traffic.FromAddress) ||
           !Check_ToAddrs(traffic.ToAddress)  ||
           !Check_Methods(traffic.Method) ||
           !Check_Services(traffic.Service) ||
           !Check_URLFragments(traffic.RequestURL)
         ) return false;

      if (m_FromGroups!=null)
      {
        if (fromGroup==null)
          fromGroup = state.FindGroupForAddress(traffic.FromAddress);
        if (fromGroup==null) return false;
        if (!Check_FromGroups(fromGroup.Name)) return false;
      }

      if (m_ToGroups!=null)
      {
        if (toGroup==null)
          toGroup = state.FindGroupForAddress(traffic.ToAddress);
        if (toGroup==null) return false;
        if (!Check_ToGroups(toGroup.Name)) return false;
      }

      if (m_FromExpression!=null)
      {
        var netState = state.FindNetSiteStateForAddress(traffic.FromAddress, ref fromGroup);
        if (!invokeEvaluator(netState, m_FromExpression)) return false;
      }

      if (m_ToExpression!=null)
      {
        var netState = state.FindNetSiteStateForAddress(traffic.ToAddress, ref toGroup);
        if (!invokeEvaluator(netState, m_ToExpression)) return false;
      }


      return true;
    }


    protected virtual bool Check_FromAddrs(string address)
    {
      if (m_FromAddrs==null || address.IsNullOrWhiteSpace()) return true;
        return m_FromAddrs.Any(s=> NFX.Parsing.Utils.MatchPattern(address, s));
    }

    protected virtual bool Check_ToAddrs(string address)
    {
      if (m_ToAddrs==null || address.IsNullOrWhiteSpace()) return true;
        return m_ToAddrs.Any(s=>NFX.Parsing.Utils.MatchPattern(address, s));
    }

    protected virtual bool Check_Methods(string method)
    {
      if (m_Methods==null || method.IsNullOrWhiteSpace()) return true;
        return m_Methods.Any(m=>string.Equals(m, method, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual bool Check_Services(string service)
    {
      if (m_Services==null || service.IsNullOrWhiteSpace()) return true;
        return m_Services.Any(s=>string.Equals(s, service, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual bool Check_URLFragments(string url)
    {
      if (m_URLFragments==null || url.IsNullOrWhiteSpace()) return true;
        return m_URLFragments.Any(uf => NFX.Parsing.Utils.MatchPattern(url, uf));
    }

    protected virtual bool Check_FromGroups(string fromGroup)
    {
      if (m_FromGroups==null || fromGroup.IsNullOrWhiteSpace()) return true;
        return m_FromGroups.Any(grp => string.Equals(grp, fromGroup, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual bool Check_ToGroups(string toGroup)
    {
      if (m_ToGroups==null || toGroup.IsNullOrWhiteSpace()) return true;
        return m_ToGroups.Any(grp => string.Equals(grp, toGroup, StringComparison.OrdinalIgnoreCase));
    }


    private bool invokeEvaluator(NetSiteState netState, Evaluator evaluator)
    {
        if (netState==null) return false;

        string evaluated = string.Empty;

        lock(netState.m_Variables)
        {
          evaluated = evaluator.Evaluate(
                         (varName)=>
                         {
                            if (varName.StartsWith(VAR_EXPRESSION_PREFIX)&&varName.Length>VAR_EXPRESSION_PREFIX.Length)
                            {
                               NetSiteState._value lookedUp;
                               varName = varName.Substring(VAR_EXPRESSION_PREFIX.Length);
                               if (netState.m_Variables.TryGetValue(varName, out lookedUp)) return lookedUp.Value.ToString();
                               //is it important to return "0" so all expressions keep working even if the variable does not exist
                               return "0";
                            }
                            return varName;
                         });
        }
        if (evaluated=="1" ||  //this check for speed as AsBool() does extra conv and parses as int
            evaluated.AsBool()) return true;

        return false;
    }


  }
}
