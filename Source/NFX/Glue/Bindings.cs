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
using System.IO;



using NFX.ApplicationModel;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.Serialization;
using NFX.Glue.Protocol;
using System.Diagnostics;
using NFX.Glue.Native;

namespace NFX.Glue
{
    /// <summary>
    /// Represents a particular named binding.
    /// Binding type defines a protocol by implementing a transport instance management strategy
    /// that support particular technology, such as blocking TCP or async ZeroMQ.
    /// Binding instance retains state/config information about all transports
    /// and has logic for Node's host:service resolution.
    /// Bindings are services, meaning - they can have state/threads that
    /// manage transport channels that operate under binding
    /// </summary>
    public abstract class Binding : GlueComponentService
    {
        #region CONSTS

            public const string CONFIG_SERVER_TRANSPORT_SECTION = "server-transport";
            public const string CONFIG_CLIENT_TRANSPORT_SECTION = "client-transport";

            public const string ATTR_PATH = "$";
            public const string ATTR_SLASH_PATH = "/$";



            public const string CONFIG_CLIENT_DUMP_ATTR = "client-dump";
            public const string CONFIG_SERVER_DUMP_ATTR = "server-dump";
            public const string CONFIG_DUMP_PATH_ATTR   = "dump-path";
            public const string CONFIG_DUMP_FORMAT_ATTR = "dump-format";

            public const string CONFIG_INSTRUMENT_TRANSPORT_STAT_ATTR = "instrument-transport-stat";

            private const char SERVER_PREFIX = 'S';
            private const char CLIENT_PREFIX = 'C';


            public const string CONFIG_STAT_TIMES_EMA_FACTOR_ATTR = "stat-times-ema-factor";

            /// <summary>
            /// Defines how much smoothing the timing statistics filter does - the lower the number the more smoothing is done.
            /// Smoothing makes stat times insensitive to some seldom delays that may happen every now and then
            /// </summary>
            public const double DEFAULT_STAT_TIMES_EMA_FACTOR = 0.04d;

            public const double MIN_STAT_TIMES_EMA_FACTOR = 0.0001d;
            public const double MAX_STAT_TIMES_EMA_FACTOR = 0.999d;

            public const string CONFIG_TRANSPORT_IDLE_TIMEOUT_MS_ATTR                            = "idle-timeout-ms";
            public const string CONFIG_CLIENT_TRANSPORT_EXISTING_ACQUISITION_TIMEOUT_MS_ATTR     = "existing-acquisition-timeout-ms";
            public const string CONFIG_CLIENT_TRANSPORT_COUNT_WAIT_THRESHOLD_ATTR                = "count-wait-threshold";
            public const string CONFIG_CLIENT_TRANSPORT_MAX_COUNT_ATTR                           = "max-count";
            public const string CONFIG_CLIENT_TRANSPORT_MAX_EXISTING_ACQUISITION_TIMEOUT_MS_ATTR = "max-existing-acquisition-timeout-ms";

            public const int CLIENT_TRANSPORT_IDLE_TIMEOUT_MS_DEFAULT = 2 * 60 * 1000;
            public const int SERVER_TRANSPORT_IDLE_TIMEOUT_MS_DEFAULT = 10 * 60 * 1000;
            public const int CLIENT_TRANSPORT_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT = 100;
            public const int CLIENT_TRANSPORT_COUNT_WAIT_THRESHOLD_DEFAULT = 8;
            public const int CLIENT_TRANSPORT_MAX_COUNT_DEFAULT = 0;
            public const int CLIENT_TRANSPORT_MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT = 15 * 1000;

        #endregion

        #region .ctor

            private void __ctor()
            {
                if (string.IsNullOrWhiteSpace(Name))
                    throw new GlueException(StringConsts.CONFIGURATION_ENTITY_NAME_ERROR + this.GetType().FullName);
                Glue.RegisterBinding(this);

                for(var i=0; i< m_ClientTransportAllocatorLocks.Length; i++)
                  m_ClientTransportAllocatorLocks[i] = new object();
            }

            protected Binding(string name)
                : this((IGlueImplementation)ExecutionContext.Application.Glue, name)
            {
            }

            protected Binding(IGlueImplementation glue, string name = null, Provider provider = null)
                : base(glue, name)
            {
                m_Provider = provider;
                __ctor();
            }

            protected override void Destructor()
            {
                base.Destructor();
                Glue.UnregisterBinding(this);
            }

        #endregion

        #region Fields

            protected Provider m_Provider;

            private object m_ListSync = new object();
            internal volatile List<Transport> m_Transports = new List<Transport>();

            private OrderedRegistry<IClientMsgInspector> m_ClientMsgInspectors = new OrderedRegistry<IClientMsgInspector>();
            private OrderedRegistry<IServerMsgInspector> m_ServerMsgInspectors = new OrderedRegistry<IServerMsgInspector>();

            private DumpDetail  m_ClientDump;
            private DumpDetail  m_ServerDump;

            private string      m_DumpPath;
            private DumpFormat  m_DumpMsgFormat;

            private bool m_InstrumentClientTransportStat;
            private bool m_InstrumentServerTransportStat;

            private bool m_MeasureStatTimes;

            private int m_ClientTransportIdleTimeoutMs = CLIENT_TRANSPORT_IDLE_TIMEOUT_MS_DEFAULT;
            private int m_ServerTransportIdleTimeoutMs = SERVER_TRANSPORT_IDLE_TIMEOUT_MS_DEFAULT;


            private double m_StatTimesEMAFactor = DEFAULT_STAT_TIMES_EMA_FACTOR;

            private int m_ClientTransportExistingAcquisitionTimeoutMs    = CLIENT_TRANSPORT_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT;
            private int m_ClientTransportCountWaitThreshold              = CLIENT_TRANSPORT_COUNT_WAIT_THRESHOLD_DEFAULT;
            private int m_ClientTransportMaxCount                        = CLIENT_TRANSPORT_MAX_COUNT_DEFAULT;
            private int m_ClientTransportMaxExistingAcquisitionTimeoutMs = CLIENT_TRANSPORT_MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT;

        #endregion


        #region Properties
            /// <summary>
            /// Returns sync/async flow that this binding provides
            /// </summary>
            public abstract OperationFlow OperationFlow
            {
              get;
            }

            /// <summary>
            /// Returns name of msg format encoding
            /// </summary>
            public abstract string EncodingFormat {  get; }


            /// <summary>
            /// Returns all transports in the binding stack
            /// </summary>
            public IEnumerable<Transport> Transports
            {
              get
              {
               var list = m_Transports;
               return list;
              }
            }

            /// <summary>
            /// Returns all client transports in the binding stack
            /// </summary>
            public IEnumerable<ClientTransport> ClientTransports { get{ return Transports.Where( t => t is ClientTransport).Cast<ClientTransport>();} }


            /// <summary>
            /// Returns all server transports in the binding stack
            /// </summary>
            public IEnumerable<ServerTransport> ServerTransports { get{ return Transports.Where( t => t is ServerTransport).Cast<ServerTransport>();} }



            public Provider Provider { get { return m_Provider; } }


            /// <summary>
            /// Returns configuration node for this named instance
            /// </summary>
            public IConfigSectionNode ConfigNode
            {
              get
              {
                return  ComponentDirector
                        .BindingConfigurations
                        .FirstOrDefault(n => Name.EqualsIgnoreCase(n.AttrByName(Service.CONFIG_NAME_ATTR).Value)
                                       ) ?? App.ConfigRoot.Configuration.EmptySection;
              }
            }


            /// <summary>
            /// Returns client message inspectors for this instance
            /// </summary>
            public OrderedRegistry<IClientMsgInspector> ClientMsgInspectors { get { return m_ClientMsgInspectors; } }

            /// <summary>
            /// Returns server message inspectors for this instance
            /// </summary>
            public OrderedRegistry<IServerMsgInspector> ServerMsgInspectors { get { return m_ServerMsgInspectors; } }

            /// <summary>
            /// Turns on/off client message dumping to disk. Turning on dumping has negative effect on performance and consumes resources
            /// </summary>
            [Config(ATTR_PATH + CONFIG_CLIENT_DUMP_ATTR, DumpDetail.None)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public DumpDetail ClientDump
            {
              get { return m_ClientDump;}
              set { m_ClientDump = value;  }
            }

            /// <summary>
            /// Turns on/off server message dumping to disk. Turning on dumping has negative effect on performance and consumes resources
            /// </summary>
            [Config(ATTR_PATH + CONFIG_SERVER_DUMP_ATTR, DumpDetail.None)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public DumpDetail ServerDump
            {
              get { return m_ServerDump;}
              set { m_ServerDump = value;  }
            }

            /// <summary>
            /// Set the path for message dumping. Must be an existing navigable path
            /// </summary>
            [Config(ATTR_PATH + CONFIG_DUMP_PATH_ATTR)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public string DumpPath
            {
              get { return m_DumpPath ?? string.Empty; }
              set { m_DumpPath = value; }
            }

            /// <summary>
            /// Set the format for message dumping
            /// </summary>
            [Config(ATTR_PATH + CONFIG_DUMP_FORMAT_ATTR, DumpFormat.Binary)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public DumpFormat DumpMsgFormat
            {
              get { return m_DumpMsgFormat; }
              set { m_DumpMsgFormat = value; }
            }


            /// <summary>
            /// Defines whether client transport statistics is periodically dumped into instrumentation
            /// </summary>
            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_INSTRUMENT_TRANSPORT_STAT_ATTR)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public bool InstrumentClientTransportStat
            {
              get { return m_InstrumentClientTransportStat; }
              set { m_InstrumentClientTransportStat = value; }
            }

            /// <summary>
            /// Defines whether server transport statistics is periodically dumped into instrumentation
            /// </summary>
            [Config(CONFIG_SERVER_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_INSTRUMENT_TRANSPORT_STAT_ATTR)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public bool InstrumentServerTransportStat
            {
              get { return m_InstrumentServerTransportStat; }
              set { m_InstrumentServerTransportStat = value; }
            }

            /// <summary>
            /// Defines whether message processing latency should be measured - i.e. messages time-stamped on arrival
            /// </summary>
            [Config]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public bool MeasureStatTimes
            {
              get { return m_MeasureStatTimes; }
              set { m_MeasureStatTimes = value;}
            }

            /// <summary>
            /// Returns current time in system frequency-dependent ticks.
            /// This property is used to measure accurate times and depends on MeasureStatTimes set to true, otherwise 0 is returned
            /// </summary>
            public long StatTimeTicks
            {
              get
              {
                if (!m_MeasureStatTimes)  return 0;

                return Stopwatch.GetTimestamp();
              }
            }

            /// <summary>
            /// Defines how much smoothing the timing statistics filter does - the lower the number the more smoothing is done.
            /// Smoothing makes stat times insensitive to some seldom delays that may happen every now and then
            /// </summary>
            [Config(CONFIG_STAT_TIMES_EMA_FACTOR_ATTR, DEFAULT_STAT_TIMES_EMA_FACTOR)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public double StatTimesEMAFactor
            {
              get { return m_StatTimesEMAFactor; }
              set
              {

                m_StatTimesEMAFactor = (value<MIN_STAT_TIMES_EMA_FACTOR)? MIN_STAT_TIMES_EMA_FACTOR : (value > MAX_STAT_TIMES_EMA_FACTOR)? MAX_STAT_TIMES_EMA_FACTOR : value;
              }

            }


            /// <summary>
            /// Specifies when client transports get auto-closed. Interval is measured in ms. Zero means indefinite/never closed transport.
            /// For more info, see 'NFX.Glue: Client Call Concurrency' topic in manual/blog
            /// </summary>
            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_TRANSPORT_IDLE_TIMEOUT_MS_ATTR, CLIENT_TRANSPORT_IDLE_TIMEOUT_MS_DEFAULT)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
            public int ClientTransportIdleTimeoutMs
            {
              get { return m_ClientTransportIdleTimeoutMs; }
              set { m_ClientTransportIdleTimeoutMs = value<0 ? 0 : value;}
            }

            /// <summary>
            /// Specifies when server transports get auto-closed. Interval is measured in ms. Zero means indefinite/never closed transport.
            /// For more info, see 'NFX.Glue: Client Call Concurrency' topic in manual/blog
            /// </summary>
            [Config(CONFIG_SERVER_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_TRANSPORT_IDLE_TIMEOUT_MS_ATTR, SERVER_TRANSPORT_IDLE_TIMEOUT_MS_DEFAULT)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
            public int ServerTransportIdleTimeoutMs
            {
              get { return m_ServerTransportIdleTimeoutMs; }
              set { m_ServerTransportIdleTimeoutMs = value<0 ? 0 : value;}
            }


            /// <summary>
            /// Sets the length of interval for the binding trying to acquire existing client transport instance to make a call.
            /// When this interval is exhausted then binding tries to allocate a new client transport per remote address, unless
            ///  other limits prohibit (max transport count). The value has to be greater or equal to zero.
            /// NOTE: this property works in conjunction with ClientTransportCoutWaitThreshold, if the number of active client transports
            ///  is below ClientTransportCoutWaitThreshold, then binding does not wait and allocates a new client transport right away until
            ///   ClientTransportCoutWaitThreshold limit is reached, then binding will try to acquire existing transport for ClientTransportExistingAcquisitionTimeoutMs milliseconds.
            /// For more info, see 'NFX.Glue: Client Call Concurrency' topic in manual/blog
            /// </summary>
            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_CLIENT_TRANSPORT_EXISTING_ACQUISITION_TIMEOUT_MS_ATTR, CLIENT_TRANSPORT_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
            public int ClientTransportExistingAcquisitionTimeoutMs
            {
                get { return m_ClientTransportExistingAcquisitionTimeoutMs; }
                set
                {
                  if (value<0) value = 0;
                  m_ClientTransportExistingAcquisitionTimeoutMs = value;
                }

            }

            /// <summary>
            /// Sets the threshold, expressed as the number of active client transports per remote address, below which binding will always allocate a new instance
            ///  of client transport without trying/waiting to acquire an existing one. When this number is exceeded then binding will try to acquire an existing
            ///  client transport instance for up to ClientTransportExistingAcquisitionTimeoutMs milliseconds.
            ///  For more info, see 'NFX.Glue: Client Call Concurrency' topic in manual/blog
            /// </summary>
            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_CLIENT_TRANSPORT_COUNT_WAIT_THRESHOLD_ATTR, CLIENT_TRANSPORT_COUNT_WAIT_THRESHOLD_DEFAULT)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
            public int ClientTransportCountWaitThreshold
            {
                get { return m_ClientTransportCountWaitThreshold; }
                set
                {
                    if (value<0) value = 0;
                    m_ClientTransportCountWaitThreshold = value;
                }
            }

            /// <summary>
            /// Imposes a limit on number of active client transports per remote address. Once this limit is reached the binding will block until it can acquire
            /// an existing transport instance. Set value to zero to remove the limit.
            ///  For more info, see 'NFX.Glue: Client Call Concurrency' topic in manual/blog
            /// </summary>
            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_CLIENT_TRANSPORT_MAX_COUNT_ATTR, CLIENT_TRANSPORT_MAX_COUNT_DEFAULT)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
            public int ClientTransportMaxCount
            {
                get { return m_ClientTransportMaxCount; }
                set
                {
                    if (value<0) value = 0;
                    m_ClientTransportMaxCount = value;
                }
            }

            /// <summary>
            /// Imposes a timeout for binding trying to get an existing transport instance per remote address.
            /// Binding throws ClientCallException when this timeout is exceeded. A value of zero removes the limit
            /// </summary>
            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + CONFIG_CLIENT_TRANSPORT_MAX_EXISTING_ACQUISITION_TIMEOUT_MS_ATTR, CLIENT_TRANSPORT_MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
            public int ClientTransportMaxExistingAcquisitionTimeoutMs
            {
                get { return m_ClientTransportMaxExistingAcquisitionTimeoutMs; }
                set
                {
                    if (value<0) value = 0;
                    m_ClientTransportMaxExistingAcquisitionTimeoutMs = value;
                }
            }


        #endregion

        #region Public

            /// <summary>
            /// Extracts necessary information from client:request pair that characterizes the particular call
            ///  for the purpose of call timing
            /// </summary>
            public virtual string GetClientCallStatTimeKey(ClientEndPoint client, RequestMsg request)
            {
              //todo In future may add property StatCallTimesLevel {Transport, Contract, Method}
              return client.Node.ToString() + " -> " +request.Contract.FullName + '.' + request.MethodName + "()";
            }


            /// <summary>
            /// Returns true when two nodes represent the same binding, host and service identities.
            /// The implementation depends on binding, for example some binding may recognize an optional parameter as part of host segment, however
            ///  that parameter does not change the identity of the node instance
            /// </summary>
            public abstract bool AreNodesIdentical(Node left, Node right);


            /// <summary>
            /// Dispatches a call allocating new or re-using existing transport if needed. The strategy depends on particular Binding implementation.
            /// This call is thread-safe
            /// </summary>
            public CallSlot DispatchCall(ClientEndPoint client, RequestMsg request)
            {
              CheckRunningState();

              //Binding level inspectors
              foreach(var insp in m_ClientMsgInspectors.OrderedValues)
                    request = insp.ClientDispatchCall(client, request);

              //Glue level inspectors
              request = Glue.ClientDispatchingRequest(client, request);

              //==============================================================

              CallOptions  options;
              options.DispatchTimeoutMs = client.DispatchTimeoutMs;
              options.TimeoutMs = client.TimeoutMs;

              var reserve = client.ReserveTransport;

              var transport = client.m_ReservedTransport ?? AcquireClientTransportForCall(client, request);

              CallSlot slot = null;

              try
              {
                if (reserve)
                 client.m_ReservedTransport = transport;

                slot = transport.SendRequest(client, request, options);

                // If execution pauses here and server sends response BEFORE call
                //then Glue.Deliver response will retry to fimnd the missing call slot for some fixed interval
                if (slot != null)
                  Glue.ClientDispatchedRequest(client, request, slot);
              }
              finally
              {
                if (!reserve)
                  ReleaseClientTransportAfterCall(transport);
              }

              return slot;
            }


            /// <summary>
            /// Ensures that application and binding runtime are running or throws otherwise
            /// </summary>
            public void CheckRunningState(bool client = true)
            {
                if (!App.Active || !Running)
                    throw client ? (GlueException)new ClientCallException(CallStatus.DispatchError, StringConsts.GLUE_SYSTEM_NOT_RUNNING_ERROR) :
                                   (GlueException)new ServerNotRunningException(StringConsts.GLUE_SYSTEM_NOT_RUNNING_ERROR);
            }

        #endregion

        #region Protected/Internal

            /// <summary>
            /// Conditionally dumps message to disk
            /// </summary>
            /// <param name="server">True for server-side message</param>
            /// <param name="msg">Message that was deserialized/serialized. If null then error happened while deserializing</param>
            /// <param name="buffer">Message body</param>
            /// <param name="offset">Start index in buffer</param>
            /// <param name="count">Byte size of message body</param>
            protected internal void DumpMsg(bool server, Msg msg, byte[] buffer, int offset, int count)
            {
              if (!shouldDumpMsg(server)) return;
              internalDumpMsg(server, msg, EncodingFormat, new ArraySegment<byte>(buffer, offset, count));
            }

            /// <summary>
            /// Conditionally dumps message to disk where the data is lazily obtained by calling dataFun
            /// </summary>
            protected internal void DumpMsg(bool server, Msg msg, Func<ArraySegment<byte>> dataFun)
            {
              if (!shouldDumpMsg(server)) return;
              ArraySegment<byte> data = dataFun();
              internalDumpMsg(server, msg, EncodingFormat, data);
            }

            /// <summary>
            /// Conditionally dumps message to disk
            /// </summary>
            /// <param name="server">True for server-side message</param>
            /// <param name="msg">Message that was deserialized/serialized. If null then error happened while deserializing</param>
            /// <param name="data">Data</param>
            protected internal void DumpMsg(bool server, Msg msg, ArraySegment<byte> data)
            {
              if (!shouldDumpMsg(server)) return;
              internalDumpMsg(server, msg, EncodingFormat, data);
            }



            protected internal void _Register(Transport transport)
            {
              lock(m_ListSync)
              {
               var lst = new List<Transport>(m_Transports);

               if (!lst.Contains(transport, ReferenceEqualityComparer<Transport>.Instance))
                  lst.Add(transport);

               m_Transports = lst; //atomic
              }
            }

            protected internal void _Unregister(Transport transport)
            {
              lock(m_ListSync)
              {
               var lst = new List<Transport>(m_Transports);
               lst.Remove(transport);
               m_Transports = lst; //atomic
              }
            }

            /// <summary>
            /// Hash table of locks used during new transport allocation, must be prime size
            /// </summary>
            protected object[] m_ClientTransportAllocatorLocks = new object[997];

            /// <summary>
            /// Returns an instance of transport suitable for a call. The implementation may return existing transport or allocate a new instance.
            /// The call is thread-safe
            /// </summary>
            protected virtual ClientTransport AcquireClientTransportForCall(ClientEndPoint client , RequestMsg request)
            {
               var transport = TryGetExistingAcquiredTransportPerRemoteNode( client.Node );
               if (transport!=null) return transport;

               //otherwise we need to create a new transport
               if (m_ClientTransportMaxCount>0)
               {
                 var alock = m_ClientTransportAllocatorLocks[(client.Node.GetHashCode() & CoreConsts.ABS_HASH_MASK) % m_ClientTransportAllocatorLocks.Length];
                 lock(alock)
                 {
                   transport = TryGetExistingAcquiredTransportPerRemoteNode( client.Node );
                   if (transport!=null) return transport;
                   return makeAndLaunchClientTransportForCall(client, request);
                 }
               }

               return makeAndLaunchClientTransportForCall(client, request);
            }

            private ClientTransport makeAndLaunchClientTransportForCall(ClientEndPoint client , RequestMsg request)
            {
              var transport = MakeNewClientTransport(client);
              try
              {
                ConfigureAndStartNewClientTransport(transport);
              }
              catch
              {
                transport.Dispose();
                throw;
              }
              return transport;
            }

            /// <summary>
            /// Factory method - Override to make an instance of a new client transport suitable for particular binding type
            /// </summary>
            protected abstract ClientTransport MakeNewClientTransport(ClientEndPoint client);

            /// <summary>
            /// Override to perform custom transport preparation and launch
            /// </summary>
            protected virtual void ConfigureAndStartNewClientTransport(ClientTransport transport)
            {
                var cfg = ConfigNode.NavigateSection(CONFIG_CLIENT_TRANSPORT_SECTION);
                if (!cfg.Exists) cfg = ConfigNode;
                transport.Configure(cfg);
                transport.Start(); //this will fail if runtime is shutting down
            }

            /// <summary>
            /// Releases a transport instance that was acquired for call.
            /// The implementation may return this instance back to the pool of available transports or deallocate it.
            /// The default implementation releases the instance back to the pool
            /// </summary>
            protected internal virtual void ReleaseClientTransportAfterCall(ClientTransport transport)
            {
              transport.Release();
            }

            protected internal abstract ServerTransport OpenServerEndpoint(ServerEndPoint epoint);

            protected internal abstract void CloseServerEndpoint(ServerEndPoint epoint);


            protected override void DoConfigure(IConfigSectionNode node)
            {
                base.DoConfigure(node);
                ConfigAttribute.Apply(this, node);
                MsgInspectorConfigurator.ConfigureClientInspectors(m_ClientMsgInspectors, node);
                MsgInspectorConfigurator.ConfigureServerInspectors(m_ServerMsgInspectors, node);

            }

            protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
            {
                base.DoAcceptManagerVisit(manager, managerNow);

                closeIdleClientTransports(managerNow);
                dumpStatistics(managerNow);
            }


            protected override void DoSignalStop()
            {
                base.DoSignalStop();

                var transports = m_Transports; //atomic
                foreach (var t in transports)
                    t.SignalStop();
            }

            protected override void DoWaitForCompleteStop()
            {
                base.DoWaitForCompleteStop();

                List<Transport> transports;

                lock(m_ListSync)
                {
                    transports = m_Transports;
                    m_Transports = new List<Transport>();
                }

                foreach (var t in transports)
                    t.Dispose();
            }


            /// <summary>
            /// Tries to acquire an available client transport to make a call.
            /// This method respects binding/transport settings that impose a limit on the number of
            ///  open concurrent transports and timeouts for acqusition waiting
            /// </summary>
            /// <param name="remoteNode">remote node to connect to</param>
            /// <returns>Available acquired transport or null</returns>
            protected virtual ClientTransport TryGetExistingAcquiredTransportPerRemoteNode(Node remoteNode)
            {
                int GRANULARITY_MS = 5 + ((System.Threading.Thread.CurrentThread.GetHashCode() & CoreConsts.ABS_HASH_MASK) % 15);

                var elapsed = 0;
                do
                {
                    var lst = m_Transports;//atomic
                    var count = 0;//count transports PER DESTINATION
                    for(var i=0; i<lst.Count; i++)//the loop is faster than intermediary enumerables produced by LINQ
                    {
                        var tr = lst[i] as ClientTransport;
                        if (tr==null) continue;
                        if (!AreNodesIdentical(tr.Node, remoteNode)) continue;
                        count++;//Per destination
                        if (tr.TryAcquire())
                           return tr;
                    }

                    if (m_ClientTransportMaxCount<=0 || count<m_ClientTransportMaxCount)
                    {
                      if (count<= m_ClientTransportCountWaitThreshold) return null;//dont wait - create new one
                      if (elapsed>=m_ClientTransportExistingAcquisitionTimeoutMs) return null;
                    }

                    System.Threading.Thread.Sleep(GRANULARITY_MS);
                    elapsed+=GRANULARITY_MS;

                    if (m_ClientTransportMaxExistingAcquisitionTimeoutMs>0 && elapsed>m_ClientTransportMaxExistingAcquisitionTimeoutMs)
                     throw new ClientCallException(CallStatus.Timeout,
                                                   StringConsts.GLUE_CLIENT_CALL_TRANSPORT_ACQUISITION_TIMEOUT_ERROR
                                                   .Args(this.Name, elapsed)
                                                   );

                }while(App.Active && Running);

                return null;
            }

        #endregion

        #region .pvt

            private bool shouldDumpMsg(bool server)
            {
                 return ((server ? m_ServerDump : m_ClientDump) & DumpDetail.Message) == DumpDetail.Message;
            }

            private void internalDumpMsg(bool server, Msg msg, string encoding, ArraySegment<byte> data)
            {
              try
              {
                  if (!Directory.Exists(DumpPath))
                  {
                    WriteLog(LogSrc.Any, Log.MessageType.Error,
                             StringConsts.GLUE_MSG_DUMP_PATH_INVALID_ERROR + DumpPath,
                             from: "DumpMsg()", pars: "path="+DumpPath);
                    return;
                  }

                  string fname;
                  char sc = server ? SERVER_PREFIX : CLIENT_PREFIX;
                  string extension = m_DumpMsgFormat == DumpFormat.Binary ? encoding : "log";

                  if (msg==null)
                      fname = string.Format("{0}-{1}.{2:N}.err.{3}", sc, "null", Guid.NewGuid(), extension);
                  else
                      fname = string.Format("{0}-{1}.{2:N}.{3:N}.ok.{4}", sc, msg.GetType().Name, msg.ID, msg.RequestID, extension);

                  var fp = Path.Combine(DumpPath, fname);

                  using(var fs = new FileStream(fp, FileMode.CreateNew))
                      dumpData(fs, m_DumpMsgFormat, String.Empty, data);
              }
              catch(Exception error)
              {
                WriteLog(LogSrc.Any, Log.MessageType.Error,
                         StringConsts.GLUE_MSG_DUMP_FAILURE_ERROR + error.Message,
                         from: "DumpMsg()", exception: error);
              }
            }

            void dumpData(FileStream fs, DumpFormat fmt, string prefix, ArraySegment<byte> data)
            {
                if (fmt == DumpFormat.Binary)
                    fs.Write(data.Array, data.Offset, data.Count);
                else
                {
                    string s = (string.IsNullOrEmpty(prefix) ? string.Empty : prefix)
                             + data.Array.ToDumpString(fmt, data.Offset, data.Count, true);
                    var b = Encoding.ASCII.GetBytes(s);
                    fs.Write(b, 0, b.Length);
                }
            }


            private void closeIdleClientTransports(DateTime managerNow)
            {
                if (m_ClientTransportIdleTimeoutMs==0) return;//nothing to close

                var lst = m_Transports;
                foreach(var cltran in lst.Where(t => t is ClientTransport).Cast<ClientTransport>())
                {
                    var locked = cltran.TryAcquire();
                    if (!locked) continue;
                    try
                    {
                        var exp = cltran.ExpirationStart;
                        if (exp.HasValue)
                        {
                            if ((managerNow - exp.Value).TotalMilliseconds > m_ClientTransportIdleTimeoutMs)
                            {
                                cltran.Dispose();
                                if (Glue.InstrumentationEnabled && this.InstrumentationEnabled)
                                 Instrumentation.InactiveClientTransportClosedEvent.Happened(cltran.Node);
                            }
                        }
                        else
                            cltran.ExpirationStart = managerNow;
                    }
                    finally
                    {
                        cltran.Release();
                    }
                }

                //20140228 DKh so Server transports too get ExpirationStart and IdleAgeMs properly stamped
                foreach(var stran in lst.Where(t => t is ServerTransport).Cast<ServerTransport>())
                {
                  if (!stran.ExpirationStart.HasValue)
                    stran.ExpirationStart = managerNow;//Expiration gets reset by statistics update on traffic via transport
                }

            }

            private void dumpStatistics(DateTime managerNow)
            {
                if (!Glue.InstrumentationEnabled || !this.InstrumentationEnabled) return;

                if (!m_InstrumentClientTransportStat && !m_InstrumentServerTransportStat) return;

                var ctr = new Dictionary<Node, int>();
                var str = new Dictionary<Node, int>();
                foreach(var tr in Transports)
                {
                  if (tr is ClientTransport)
                  {
                    if (ctr.ContainsKey(tr.Node)) ctr[tr.Node] += 1; else ctr[tr.Node] = 1;
                  }
                  else
                  {
                    if (str.ContainsKey(tr.Node)) str[tr.Node] += 1; else str[tr.Node] = 1;
                  }

                  tr.DumpInstrumentationData();
                }

                if (m_InstrumentClientTransportStat)
                 foreach(var kvp in ctr)
                  Instrumentation.ClientTransportCount.Record(kvp.Key, kvp.Value);

                if (m_InstrumentServerTransportStat)
                 foreach(var kvp in str)
                  Instrumentation.ServerTransportCount.Record(kvp.Key, kvp.Value);
            }

            private DateTime dumpNowTime()
            {
                return LocalizedTime;
            }

        #endregion

    }

    /// <summary>
    /// A registry of Binding-derived instances
    /// </summary>
    public class Bindings : Registry<Binding>
    {

    }

}
