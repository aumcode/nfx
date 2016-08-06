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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

using NFX.Log;
using NFX.Environment;
using NFX.ServiceModel;

namespace NFX.IO.Net.Gate
{
  /// <summary>
  /// Represents a network gate - a logical filter of incoming network traffic.
  /// Network gate is somewhat similar to a firewall - it allows/denies the int/out traffic based on the set of rules
  /// </summary>
  public class NetGate : Service, INetGateImplementation
  {
    #region CONSTS
      public const string CONFIG_INCOMING_SECTION = "incoming";
      public const string CONFIG_OUTGOING_SECTION = "outgoing";

      public const string CONFIG_RULE_SECTION  = "rule";
      public const string CONFIG_GROUP_SECTION = "group";
      public const string CONFIG_VARDEF_SECTION   = "var-def";

      public const string CONFIG_ADDRESS_SECTION   = "address";

      public const string CONFIG_DEFAULT_ACTION_ATTR = "default-action";

      public const string PATTERN_CAPTURE_WC = "*";

      private const int THREAD_GRANULARITY_MS = 1000;
    #endregion

          #region inner class

              public class State
              {
                internal State(NetGate gate)
                {
                  Gate = gate;
                  Rules     = new OrderedRegistry<Rule>();
                  Groups    = new OrderedRegistry<Group>();
                  VarDefs   = new Registry<VarDef>();
                  NetState  = new ConcurrentDictionary<string,NetSiteState>(System.Environment.ProcessorCount * 8, 1024);
                }

                [Config] public GateAction DefaultAction;
                public readonly NetGate                   Gate;
                public readonly OrderedRegistry<Rule>     Rules;
                public readonly OrderedRegistry<Group>    Groups;
                public readonly Registry<VarDef>          VarDefs;
                internal readonly ConcurrentDictionary<string, NetSiteState> NetState;


                /// <summary>
                /// Returns the first matching group for address, or null
                /// </summary>
                public Group FindGroupForAddress(string address)
                {
                  return Groups.FirstOrDefault(grp => grp.Match(address)!=null);
                }

                /// <summary>
                /// Returns existing NetSiteState object for specified address, first checking group membership or null
                /// </summary>
                public NetSiteState FindNetSiteStateForAddress(string address, ref Group group)
                {
                  NetSiteState result;
                  if (group==null) group = FindGroupForAddress(address);
                  if (group!=null)
                    if (NetState.TryGetValue(group.Key, out result)) return result;

                  if (NetState.TryGetValue(address, out result)) return result;

                  return null;
                }


                internal void configure(IConfigSectionNode node)
                {
                  if (node==null || !node.Exists) return;
                  Rules.Clear();
                  Groups.Clear();
                  VarDefs.Clear();
                  ConfigAttribute.Apply(this, node);

                  foreach(var cn in node.Children.Where(cn=>cn.IsSameName(CONFIG_RULE_SECTION)))
                    if(!Rules.Register( FactoryUtils.Make<Rule>(cn, typeof(Rule), args: new object[]{ cn })) )
                       throw new NetGateException(StringConsts.NETGATE_CONFIG_DUPLICATE_ENTITY_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, CONFIG_RULE_SECTION));

                  foreach(var cn in node.Children.Where(cn=>cn.IsSameName(CONFIG_GROUP_SECTION)))
                    if(!Groups.Register( FactoryUtils.Make<Group>(cn, typeof(Group), args: new object[]{ cn })) )
                       throw new NetGateException(StringConsts.NETGATE_CONFIG_DUPLICATE_ENTITY_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, CONFIG_GROUP_SECTION));

                  foreach(var cn in node.Children.Where(cn=>cn.IsSameName(CONFIG_VARDEF_SECTION)))
                    if(!VarDefs.Register( FactoryUtils.Make<VarDef>(cn, typeof(VarDef), args: new object[]{ cn })) )
                       throw new NetGateException(StringConsts.NETGATE_CONFIG_DUPLICATE_ENTITY_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, CONFIG_VARDEF_SECTION));
                }
              }

          #endregion


    #region .ctor
      public NetGate() : base()
      {

      }

      public NetGate(object director) : base (director)
      {
        m_IncomingState = new State(this);
        m_OutgoingState = new State(this);
      }
    #endregion

    #region Fields

      private bool m_Enabled = true;

      private State m_IncomingState;
      private State m_OutgoingState;

      private Thread m_Thread;

    #endregion

    #region Properties


      public override string ComponentCommonName { get { return "gate"; }}

      /// <summary>
      /// Returns gate state
      /// </summary>
      public State this[TrafficDirection direction] { get{ return direction==TrafficDirection.Incoming ? m_IncomingState : m_OutgoingState; } }

      /// <summary>
      /// Enables/diables the protection. When protection is disabled then all traffic is allowed
      /// </summary>
      [Config]
      public bool Enabled
      {
        get {return m_Enabled;}
        set {m_Enabled = value;}
      }


    #endregion

    #region Public


       /// <summary>
      /// Checks whether the specified traffic is allowed or denied
      /// </summary>
      public GateAction CheckTraffic(ITraffic traffic)
      {
        if (!m_Enabled) return GateAction.Allow;
        Rule rule;
        return this.CheckTraffic(traffic, out rule);
      }


      /// <summary>
      /// Checks whether the specified traffic is allowed or denied.
      /// Returns the rule that determined the allow/deny outcome or null when no rule matched
      /// </summary>
      public GateAction CheckTraffic(ITraffic traffic, out Rule rule)
      {
        rule = null;
        if (!m_Enabled) return GateAction.Allow;
        if (traffic==null) return GateAction.Deny;

        var state = this[traffic.Direction];
        var result = state.DefaultAction;

        Group fromGroup = null;
        Group toGroup = null;

        foreach(var rItem in state.Rules.OrderedValues)
        {
          if (rItem.Check(state, traffic, ref fromGroup, ref toGroup))
          {
            result = rItem.Action;
            rule = rItem;
            break;
          }
        }

        return result;
      }

      /// <summary>
      /// Increases the named variable in the network scope which this specified traffic falls under
      /// </summary>
      public virtual void IncreaseVariable(TrafficDirection direction, string address, string varName, int value)
      {
        if (!m_Enabled) return;
        setVariable(true, direction, address, varName, value);
      }

      /// <summary>
      /// Sets the named variable in the network scope which this specified traffic falls under
      /// </summary>
      public virtual void SetVariable(TrafficDirection direction, string address, string varName, int value)
      {
        if (!m_Enabled) return;
        setVariable(false, direction, address, varName, value);
      }

    #endregion

    #region Protected

      protected override void DoConfigure(Environment.IConfigSectionNode node)
      {
        if (node==null) return;
        m_IncomingState.configure(node[CONFIG_INCOMING_SECTION]);
        m_OutgoingState.configure(node[CONFIG_OUTGOING_SECTION]);
      }

      protected override void DoStart()
      {
        m_Thread = new Thread(threadSpin);
        m_Thread.Name = "Thread {0}('{1}')".Args(GetType().DisplayNameWithExpandedGenericArgs(), Name);
        m_Thread.Start();
      }

      protected override void DoWaitForCompleteStop()
      {
        m_Thread.Join();
        m_Thread = null;

      }

      /// <summary>
      /// Facilitates net gate logging
      /// </summary>
      protected void Log(MessageType type, string text, string from = null, Exception error = null, string pars = null, Guid? related = null)
      {
        var msg = new Message
        {
          Type = type,
          Topic = CoreConsts.LOG_NET_TOPIC,
          From = "{0}.{1}".Args(GetType().FullName, from),
          Text = text,
          Exception = error,
          Parameters = pars
        };

        if (related.HasValue)
          msg.RelatedTo = related.Value;

        App.Log.Write(msg);
      }

    #endregion

    #region .private


      private void threadSpin()
      {
        while(Running)
        {
          try
          {
            process(m_IncomingState);
            process(m_OutgoingState);
          }
          catch(Exception error)
          {
            Log(MessageType.CatastrophicError, error.ToMessageWithType(), "threadSpin()", error);
          }
          Thread.Sleep(THREAD_GRANULARITY_MS);
        }
      }

      private void process(State state)
      {
          var now = DateTime.UtcNow;

          List<NetSiteState> empty = new List<NetSiteState>();//this will collect NetSiteState instances without any vars left - need to be deleted
          foreach(var netState in state.NetState.Select(kvp=>kvp.Value))
          {
            if (!Running) return;
            lock(netState)
            {
              double elapsedSec =  (now - netState.m_LastTouch).TotalSeconds;
              Lazy<List<string>> deletedVars = new Lazy<List<string>>();
              foreach(var kvp in netState.m_Variables)
              {
                var varDef = state.VarDefs[kvp.Key];
                if (varDef==null) continue;//variable not found in definition. Keep as-is

                double reduction = (double)varDef.DecayBy * (elapsedSec / (double)varDef.IntervalSec);//Interval is never less than 1 sec
                int intReduction = (int)reduction;

                if (intReduction<1) continue;

                netState.m_LastTouch = now;

                var _v = kvp.Value;
                var vval = _v.Value;

                if(vval<0)
                {
                  vval+=intReduction;
                  if (vval>0) vval = 0;
                }
                else
                {
                  vval-=intReduction;
                  if (vval<0) vval = 0;
                }

                _v.Value = vval;

                if(vval==0)
                  deletedVars.Value.Add(kvp.Key);
              }//foreach

              if (deletedVars.IsValueCreated)
                foreach(var key in deletedVars.Value)
                  netState.m_Variables.Remove(key);

              if (netState.m_Variables.Count==0)
                empty.Add(netState);
            }//lock (netState)
          }//foreach

          for(var i=0; i<empty.Count; i++)
          {
            if (!Running) return;
            var netState = empty[i];
            lock(netState)
            {
                if (netState.m_Variables.Count==0)
                {
                 NetSiteState removed;
                 state.NetState.TryRemove(netState.Key, out removed);
                }
            }
          }//for
      }




      private void setVariable(bool inc, TrafficDirection direction, string address, string varName, int value)
      {
        if (!Running || address.IsNullOrWhiteSpace() || varName.IsNullOrWhiteSpace() || value==0) return;

        var state = this[direction];
        var grp = state.FindGroupForAddress(address);

        var key = grp==null ? address : grp.Key;

        var nstate = state.NetState.GetOrAdd(key, (k) => grp==null ? new NetSiteState(address) : new NetSiteState(grp));

        lock(nstate)
        {
          if (inc)
          {
           NetSiteState._value vval;
           if (!nstate.m_Variables.TryGetValue(varName, out vval))
           {
             nstate.m_Variables[varName] = vval = new NetSiteState._value();
           }
           vval.Value += value;
          }
          else
           nstate.m_Variables[varName] = new NetSiteState._value{Value = value};
        }
      }

    #endregion


  }
}
