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
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Erlang
{
  public class ErlApp : IApplicationStarter
  {
    public static ErlTraceLevel DefaultTraceLevel = ErlTraceLevel.Off;
    public static bool UseExtendedPidsPorts = true;

    public static bool IgnoreLocalEpmdConnectErrors = false;

    private static ErlLocalNode s_Node;

    /// <summary>
    /// Global node from config
    /// </summary>
    public static ErlLocalNode Node { get { return s_Node; } internal set { s_Node = value; } }

    public static bool TraceEnabled(ErlTraceLevel level, ErlTraceLevel minLevel)
    {
      return level != ErlTraceLevel.Off && level >= minLevel;
    }

  #region Application Startup (Erlang node)

    private ConfigSectionNode m_AllNodes;

    //---------------------------------------------------------------------
    // Application model startup
    //---------------------------------------------------------------------
    public string Name { get { return "ErlApp"; } }
    public bool ApplicationStartBreakOnException { get { return true; } }

    public void ApplicationStartBeforeInit(IApplication application)
    {
      s_Node.Start();

      // Establish connections to all listed remote nodes
      m_AllNodes.Children
                .Where(n => n.Name.EqualsSenseCase("node")
                         && !n.AttrByName(ErlConsts.CONFIG_IS_LOCAL_ATTR).ValueAsBool()
                         &&  n.AttrByName(ErlConsts.ERLANG_CONNECT_ON_STARUP).ValueAsBool(true))
                .ForEach(n => s_Node.Connection(n.Value, n));

      //remember configs for remote nodes
      s_Node.AllNodeConfigs = m_AllNodes;

      // Ensure proper cleanup of local node's global state
      application.RegisterAppFinishNotifiable(s_Node);
    }

    public void ApplicationStartAfterInit(IApplication application)
    {}

    public void Configure(IConfigSectionNode node)
    {
      var appRoot = node.NavigateSection("/" + ErlConsts.ERLANG_CONFIG_SECTION);

      if (appRoot == null)
        throw new ErlException(
          StringConsts.CONFIGURATION_NAVIGATION_SECTION_REQUIRED_ERROR,
          ErlConsts.ERLANG_CONFIG_SECTION);

      // Configure global node variables

      ErlAbstractNode.s_DefaultCookie = new ErlAtom(
        appRoot.AttrByName(ErlConsts.ERLANG_COOKIE_ATTR)
            .ValueAsString(ErlAbstractNode.s_DefaultCookie.Value));

      ErlAbstractNode.s_UseShortNames =
        appRoot.AttrByName(ErlConsts.ERLANG_SHORT_NAME_ATTR)
            .ValueAsBool(ErlAbstractNode.s_UseShortNames);

      ErlAbstractConnection.ConnectTimeout =
        appRoot.AttrByName(ErlConsts.ERLANG_CONN_TIMEOUT_ATTR)
            .ValueAsInt(ErlAbstractConnection.ConnectTimeout);

      // Configure local node and remote connections

      var cfg = new MemoryConfiguration();
      cfg.CreateFromNode(appRoot);

      var root  = cfg.Root;
      var nodes = root.Children
                      .Where(n => n.Name.EqualsIgnoreCase(ErlConsts.ERLANG_NODE_SECTION));

      var localNodes = nodes.Where(n => n.AttrByName(ErlConsts.CONFIG_IS_LOCAL_ATTR).ValueAsBool()).ToArray();
      if (localNodes.Length != 1)
        throw new ErlException(StringConsts.ERL_CONFIG_SINGLE_NODE_ERROR, localNodes.Length);

      var localNode = localNodes[0];

      // Create and configure local node

      s_Node = new ErlLocalNode(localNode.Value, localNode);

      // Configure connections to all remote nodes

      //m_AllNodes = nodes.Where(n => !n.AttrByName(ErlConsts.CONFIG_IS_LOCAL_ATTR).ValueAsBool());
      m_AllNodes = root;
    }

  #endregion
  }


}
