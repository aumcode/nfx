/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using NFX.ApplicationModel;
using NFX.Instrumentation;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// The central facade for working with MongoDB. The technology was tested against Mongo DB 3.0.6.
  /// The NFX MongoDB connector is purposely created for specific needs. It does not support: Mongo security, sharding and replication
  /// </summary>
  public sealed class MongoClient : ApplicationComponent, INamed, IConfigurable, IInstrumentable, IApplicationFinishNotifiable
  {
      #region CONSTS

          public const string CONFIG_MONGO_CLIENT_SECTION = "mongo-db-client";

          public const string CONFIG_CS_ROOT_SECTION = "mongo";
          public const string CONFIG_CS_SERVER_ATTR = "server";
          public const string CONFIG_CS_DB_ATTR = "db";

          private static readonly TimeSpan MANAGEMENT_INTERVAL = TimeSpan.FromMilliseconds(4795);
      #endregion

      /// <summary>
      /// Return a Database instance per supplied connection string in the following form:
      /// <code>
      /// mongo{server="mongo://localhost:27017" db="myDB"}
      /// </code>
      /// </summary>
      public static Database DatabaseFromConnectString(string cString)
      {
        if (cString.IsNullOrWhiteSpace())
          throw new MongoDBConnectorException(StringConsts.CONNECTION_STRING_NULL_OR_EMPTY_ERROR);

        var root = cString.AsLaconicConfig();
        if (root == null || !root.IsSameName(CONFIG_CS_ROOT_SECTION))
          throw new MongoDBConnectorException(StringConsts.CONNECTION_STRING_INVALID_ERROR.Args(cString, "Unparsable or not '{0}' root".Args(CONFIG_CS_ROOT_SECTION)));

        var cs = root.AttrByName(CONFIG_CS_SERVER_ATTR).Value;
        var dbn = root.AttrByName(CONFIG_CS_DB_ATTR).Value;
        if (cs.IsNullOrWhiteSpace() || dbn.IsNullOrWhiteSpace())
          throw new MongoDBConnectorException(StringConsts.CONNECTION_STRING_INVALID_ERROR.Args(cString, "Missing attr '{0}' or '{1}'".Args(CONFIG_CS_SERVER_ATTR, CONFIG_CS_DB_ATTR)));

        var server = Instance[ new NFX.Glue.Node(cs)];
        var database = server[dbn];
        return database;
      }

      #region .ctor/Lifecycle
          private static object s_InstanceLock = new object();
          private static volatile MongoClient s_Instance;

          /// <summary>
          /// Retrurns the default instance of the Client, lazily crating it on the first access
          /// </summary>
          public static MongoClient Instance
          {
            get
            {
                if (s_Instance!=null) return s_Instance;
                lock(s_InstanceLock)
                {
                  if (s_Instance==null)
                  {
                    s_Instance = new MongoClient("Default");
                    App.Instance.RegisterAppFinishNotifiable( s_Instance );
                  }
                  return s_Instance;
                }

            }
          }


          /// <summary>
          /// Creates a new instance of client.
          /// For most applications it is sufficient to use the default singleton instance Client.Instance
          /// </summary>
          public MongoClient(string name) : base(null)
          {
            if (name.IsNullOrWhiteSpace())
              name = Guid.NewGuid().ToString();

            m_Name = name;
            m_ManagementEvent = new Time.Event(App.EventTimer,
                                               "MongoClient('{0}'::{1})".Args(m_Name, Guid.NewGuid().ToString()),
                                               e => managementEventBody(),
                                               MANAGEMENT_INTERVAL);
          }

          protected override void Destructor()
          {
            DisposableObject.DisposeAndNull(ref m_ManagementEvent);

            foreach(var server in m_Servers)
              server.Dispose();

            base.Destructor();
            lock(s_InstanceLock)
            {
              if (this==s_Instance)
               s_Instance = null;
            }
          }

          void IApplicationFinishNotifiable.ApplicationFinishBeforeCleanup(IApplication application){   }
          void IApplicationFinishNotifiable.ApplicationFinishAfterCleanup(IApplication application)
          {
            try
            {
             this.Dispose();
            }
            catch(Exception error)
            {
              throw new MongoDBConnectorException("{0}.ApplicationFinishAfterCleanup leaked: {1}".Args(GetType().FullName, error.ToMessageWithType()), error);
            }
          }

      #endregion

      #region Fields
        private IConfigSectionNode m_ConfigRoot;

        private string m_Name;
        private bool m_InstrumentationEnabled;

        private Time.Event m_ManagementEvent;
        internal Registry<ServerNode> m_Servers = new Registry<ServerNode>();

      #endregion


      #region Properties

        public string Name{ get{ return m_Name;}}

        [Config]
        public bool InstrumentationEnabled { get{ return m_InstrumentationEnabled; } set { m_InstrumentationEnabled = value;} }

        /// <summary>
        /// Returns the config root of the client that was set by the last call to Configure()
        /// or App.CONFIG_MONGO_CLIENT_SECTION (which may be non-existent)
        /// </summary>
        public IConfigSectionNode ConfigRoot
        {
          get { return m_ConfigRoot ?? App.ConfigRoot[CONFIG_MONGO_CLIENT_SECTION];}
        }

        /// <summary>
        /// Returns all connected servers
        /// </summary>
        public IRegistry<ServerNode> Servers { get{ return m_Servers;} }

        /// <summary>
        /// Returns an existing server node or creates a new one
        /// </summary>
        public ServerNode this[Glue.Node node]
        {
          get
          {
             EnsureObjectNotDisposed();
             return m_Servers.GetOrRegister(node.Name, (n) => new ServerNode(this, n), node);
          }
        }

        /// <summary>
        /// Returns ServerNode for local server on a default port
        /// </summary>
        public ServerNode DefaultLocalServer
        {
          get { return this[Connection.DEFAUL_LOCAL_NODE];}
        }

      #endregion

      #region Public


        /// <summary>
        /// Sets config root. If this method is never called then configuration is done of the App.CONFIG_MONGO_CLIENT_SECTION section
        /// </summary>
        public void Configure(IConfigSectionNode node)
        {
          EnsureObjectNotDisposed();

          m_ConfigRoot = node;
          ConfigAttribute.Apply(this, ConfigRoot);
        }

        public override string ToString()
        {
          return "Client('{0}')".Args(m_Name);
        }

      #endregion

          #region IInstrumentable
            public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

            public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
            {
              return ExternalParameterAttribute.GetParameters(this, groups);
            }

            public bool ExternalGetParameter(string name, out object value, params string[] groups)
            {
               return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
            }

            public bool ExternalSetParameter(string name, object value, params string[] groups)
            {
              return ExternalParameterAttribute.SetParameter(this, name, value, groups);
            }
          #endregion

      #region .pvt

        private void managementEventBody()
        {
          foreach(var server in m_Servers)
           server.ManagerVisit();

          //todo Dump statistics
        }

      #endregion
  }
}
