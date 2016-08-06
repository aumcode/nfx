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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Glue
{

    /// <summary>
    /// Represents a server endpoint that accepts client requests. This is a sealed class
    /// </summary>
    public sealed class ServerEndPoint : EndPoint, IConfigurable, INamed
    {
        #region CONSTS

            public const string CONFIG_NAME_ATTR = "name";
            public const string CONFIG_NODE_ATTR = "node";
            public const string CONFIG_BINDING_ATTR = "binding";
            public const string CONFIG_CONTRACT_SERVERS_ATTR = "contract-servers";

        #endregion


        //used by configuration
        internal ServerEndPoint(IGlueImplementation glue, string name) : base(glue)
        {
            ctor(name, null);
        }

        public ServerEndPoint(string node, IEnumerable<Type> contractServers, Binding binding = null) : base(ExecutionContext.Application.Glue, new Node(node), binding) { ctor(null, contractServers); }
        public ServerEndPoint(Node node, IEnumerable<Type> contractServers, Binding binding = null)   : base(ExecutionContext.Application.Glue, node, binding) { ctor(null, contractServers); }
        public ServerEndPoint(string name, string node, IEnumerable<Type> contractServers, Binding binding = null) : base(ExecutionContext.Application.Glue, new Node(node), binding) { ctor(name, contractServers); }
        public ServerEndPoint(string name, Node node, IEnumerable<Type> contractServers, Binding binding = null)   : base(ExecutionContext.Application.Glue, node, binding) { ctor(name, contractServers); }

        public ServerEndPoint(IGlue glue, string node, IEnumerable<Type> contractServers, Binding binding = null) : base(glue, new Node(node), binding) { ctor(null, contractServers); }
        public ServerEndPoint(IGlue glue, Node node, IEnumerable<Type> contractServers, Binding binding = null)   : base(glue, node, binding) { ctor(null, contractServers); }
        public ServerEndPoint(IGlue glue, string name, string node, IEnumerable<Type> contractServers, Binding binding = null) : base(glue, new Node(node), binding) { ctor(name, contractServers); }
        public ServerEndPoint(IGlue glue, string name, Node node, IEnumerable<Type> contractServers, Binding binding = null)   : base(glue, node, binding) { ctor(name, contractServers); }


        private void ctor(string name, IEnumerable<Type> contractServers)
        {
            if (string.IsNullOrWhiteSpace(name))
               name = Guid.NewGuid().ToString();

            m_Name = name;
            m_Glue.RegisterServerEndpoint(this);

            if (contractServers!=null)
             m_ContractServers = contractServers.ToArray();
            else
             m_ContractServers = new Type[0];
        }

        protected override void Destructor()
        {
            base.Destructor();
            m_Glue.UnregisterServerEndpoint(this);
            Close();
        }

        private bool m_IsOpen;
        private ServerTransport m_Transport;
        private string m_Name;
        private Type[] m_ContractServers;//[C]ontract

        private OrderedRegistry<IServerMsgInspector> m_MsgInspectors = new OrderedRegistry<IServerMsgInspector>();


        /// <summary>
        /// INTERNAL! Maps ContractType -> ServerImplementing class. This is a cache used not to recompute everything on every call
        /// </summary>
        internal ConcurrentDictionary<Type, Implementation.ServerHandler.serverImplementer> m_ContractImplementers = new ConcurrentDictionary<Type, Implementation.ServerHandler.serverImplementer>();


        /// <summary>
        /// Returns name of the endpoint. Named endpoints must be unique in the context, if name was not supplied in .ctor then it is auto-generated
        /// </summary>
        public string Name { get { return m_Name ?? CoreConsts.UNKNOWN; } }


        /// <summary>
        /// Returns server transport that services this endpoint
        /// </summary>
        public ServerTransport Transport  { get { return m_Transport; } }


        /// <summary>
        /// Returns server message inspectors for this instance
        /// </summary>
        public OrderedRegistry<IServerMsgInspector> MsgInspectors { get { return m_MsgInspectors; } }

        /// <summary>
        /// Returns types that implement/serve contracts that this endpoint accepts
        /// </summary>
        public Type[] ContractServers
        {
          get { return m_ContractServers; }
        }

        /// <summary>
        /// Indicates whether endpoint is open and is accepting incoming requests
        /// </summary>
        public bool IsOpen
        {
          get { return m_IsOpen; }
        }

        /// <summary>
        /// Opens endpoint by allocating transports (if necessary).
        /// ServerEndpoint can be Close() or Dispose() after it was open
        /// </summary>
        public void Open()
        {
          EnsureObjectNotDisposed();
          if (m_IsOpen) return;
          m_Transport = m_Binding.OpenServerEndpoint(this);
          m_IsOpen = true;
        }

        public void Close()
        {
          if (!m_IsOpen) return;
          m_Binding.CloseServerEndpoint(this);
          m_Transport = null;
          m_IsOpen = false;
        }

        public void Configure(IConfigSectionNode node)
        {
            EnsureObjectNotDisposed();
            m_Name = node.AttrByName(CONFIG_NAME_ATTR).ValueAsString(Guid.NewGuid().ToString());
            m_Node = new Node(node.AttrByName(CONFIG_NODE_ATTR).ValueAsString());

            if (m_Binding==null)
             if (!string.IsNullOrWhiteSpace(m_Node.Binding))
               m_Binding = Glue.GetNodeBinding(m_Node);

            var bnode = node.AttrByName(CONFIG_BINDING_ATTR);
            if (bnode.Exists)
              m_Binding = Glue.Bindings[bnode.Value];


            var tNames = node.AttrByName(CONFIG_CONTRACT_SERVERS_ATTR).ValueAsString(CoreConsts.UNKNOWN).Split(';');

            var servers = new List<Type>();
            foreach(var t in tNames)
            {
                var tServer = Type.GetType(t, false);

               if (tServer == null)
                throw new GlueException(StringConsts.GLUE_SERVER_ENDPOINT_CONTRACT_SERVER_TYPE_ERROR + (t ?? CoreConsts.UNKNOWN));

               servers.Add(tServer);
            }

            m_ContractServers = servers.ToArray();

            MsgInspectorConfigurator.ConfigureServerInspectors(m_MsgInspectors, node);
        }


        public override string ToString()
        {
            var s = new StringBuilder();
            foreach(var c in this.ContractServers)
            {
              s.Append(c.FullName);
              s.Append(",");
            }

            if (s.Length>0) s.Remove(s.Length-1, 1);

            return string.Format("{0}@{1}({2}) -> {3}", this.Name, this.Node, this.Binding.Name, s);
        }
    }


    public sealed class Servers : Registry<ServerEndPoint>
    {
        public Servers() : base() { }
    }


}
