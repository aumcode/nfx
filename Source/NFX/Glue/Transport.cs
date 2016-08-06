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
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;



using NFX.ServiceModel;
using NFX.Glue.Protocol;
using NFX.Serialization;


namespace NFX.Glue
{
    /// <summary>
    /// Defines kinds of trasport
    /// </summary>
    public enum TransportKind
    {
        Server,
        Client
    }

    /// <summary>
    /// Transports are thread-safe and are intended for use by multiple threads from ClientCallReactor
    /// and ServerProcessor when bindings operate in OperationFlow.Asynchronous mode.
    /// Transports are acquired by the thread that creates them
    /// </summary>
    public abstract class Transport : Service<Binding>
    {
        #region CONSTS

            private const int NOT_ACQUIRED  = 0;
            private const int IS_ACQUIRED   = 1;

        #endregion

        #region .ctor

            protected Transport(Binding binding, TransportKind kind) : base(binding)
            {
                m_TransportKind = kind;
                m_Glue = binding.Glue;
                binding._Register(this);
            }

            protected override void Destructor()
            {
                Binding._Unregister(this);
                base.Destructor();
            }

        #endregion


        #region Fields

            protected TransportKind m_TransportKind;
            private int m_Acquired = IS_ACQUIRED;

            private DateTime? m_ExpirationStart;
            private IGlueImplementation m_Glue;

        #endregion


        #region Properties Pub and Protected

            /// <summary>
            /// Returns true when this transport instance has been acquired by some operation and is busy
            /// </summary>
            public bool Acquired { get { return m_Acquired != NOT_ACQUIRED; } }


            /// <summary>
            /// Returns a binding that this transport operates under
            /// </summary>
            public Binding Binding { get { return ComponentDirector as Binding; } }

            /// <summary>
            /// Returns glue implementation that this transport is under
            /// </summary>
            public IGlueImplementation Glue { get { return m_Glue; } }

            /// <summary>
            /// Reports if this is a server or client transport
            /// </summary>
            public TransportKind TransportKind { get { return m_TransportKind; } }

            /// <summary>
            /// Returns timestamp when manager touched the instance for last time and no traffic went through it since
            /// </summary>
            public DateTime? ExpirationStart
            {
               get{ return m_ExpirationStart; }
               internal set { m_ExpirationStart = value; }
            }

            /// <summary>
            /// Returns the duration of transport idle period in ms.
            /// </summary>
            public int IdleAgeMs
            {
              get
              {
                if (!m_ExpirationStart.HasValue) return 0;
                return (int)(Glue.LocalizedTime - m_ExpirationStart.Value).TotalMilliseconds;
              }
            }


            /// <summary>
            /// Returns node that this transport services
            /// </summary>
            public Node Node { get; protected set; }

        #endregion

        #region Statistics

            private long m_StatBytesReceived; protected internal void stat_BytesReceived(long val) { Interlocked.Add(ref m_StatBytesReceived, val); }
            private long m_StatBytesSent;     protected internal void stat_BytesSent(long val) { Interlocked.Add(ref m_StatBytesSent, val); }
            private long m_StatMsgReceived;   protected internal void stat_MsgReceived(long count = 1) { Interlocked.Add(ref m_StatMsgReceived, count); m_ExpirationStart = null; }
            private long m_StatMsgSent;       protected internal void stat_MsgSent() { Interlocked.Increment(ref m_StatMsgSent); m_ExpirationStart = null; }
            private long m_StatErrors;        protected internal void stat_Errors() { Interlocked.Increment(ref m_StatErrors); }

            private long m_Prev_StatBytesReceived;
            private long m_Prev_StatBytesSent;
            private long m_Prev_StatMsgReceived;
            private long m_Prev_StatMsgSent;
            private long m_Prev_StatErrors;


            private ConcurrentDictionary<string, double> m_StatTimes = new ConcurrentDictionary<string, double>();
                         /// <summary>
                         /// Processes named time measurement statistical sample by using EMA filter
                         /// </summary>
                         /// <param name="key">Name of time measurement key</param>
                         /// <param name="ticks">Duration in ticks</param>
                         protected internal void stat_Time(string key, long ticks)
                         {
                            if (!Binding.MeasureStatTimes) return;

                            var time = ticks / (double)Stopwatch.Frequency;
                            var F = Binding.StatTimesEMAFactor;

                            var ema = m_StatTimes.AddOrUpdate(key, time,  (k, old) => ((F * time) + (( 1d - F) * old))  );
                         }


            /// <summary>
            /// Resets all statistical counters
            /// </summary>
            public void ResetStats()
            {
                m_StatBytesReceived = 0;
                m_StatBytesSent = 0;
                m_StatMsgReceived = 0;
                m_StatMsgSent = 0;
                m_StatErrors = 0;

                m_Prev_StatBytesReceived = 0;
                m_Prev_StatBytesSent = 0;
                m_Prev_StatMsgReceived = 0;
                m_Prev_StatMsgSent = 0;
                m_Prev_StatErrors = 0;

                m_StatTimes.Clear();
            }


            /// <summary>
            /// Dumps instrumentation data
            /// </summary>
            public void DumpInstrumentationData()
            {
              if (!Glue.InstrumentationEnabled || !Binding.InstrumentationEnabled) return;

              if (
                  (!Binding.InstrumentClientTransportStat && this is ClientTransport) ||
                  (!Binding.InstrumentServerTransportStat && this is ServerTransport)
                 ) return;

              DoDumpInstrumentationData();
            }

            /// <summary>
            /// Override to dump instrumentation data, dont forget to call base to dump basic metrics
            /// </summary>
            protected virtual void DoDumpInstrumentationData()
            {
              var delta_StatBytesReceived = m_StatBytesReceived - m_Prev_StatBytesReceived; m_Prev_StatBytesReceived = m_StatBytesReceived;
              var delta_StatBytesSent     = m_StatBytesSent     - m_Prev_StatBytesSent;     m_Prev_StatBytesSent     = m_StatBytesSent;
              var delta_StatMsgReceived   = m_StatMsgReceived   - m_Prev_StatMsgReceived;   m_Prev_StatMsgReceived   = m_StatMsgReceived;
              var delta_StatMsgSent       = m_StatMsgSent       - m_Prev_StatMsgSent;       m_Prev_StatMsgSent       = m_StatMsgSent;
              var delta_StatErrors        = m_StatErrors        - m_Prev_StatErrors;        m_Prev_StatErrors        = m_StatErrors;

              var node = Node;

              //Introduce entropy into random generator
              var entropySample = (delta_StatBytesReceived << (int)(delta_StatMsgReceived%27)) ^
                                  (delta_StatBytesSent << (int)(delta_StatMsgSent%19)) ^
                                  (delta_StatMsgReceived << 3) ^
                                  (delta_StatMsgSent << 5) ^
                                  (delta_StatErrors);

              if (entropySample!=0)
                NFX.ExternalRandomGenerator.Instance.FeedExternalEntropySample((int)entropySample);

              if (this is ClientTransport)
              {
                 Instrumentation.ClientBytesReceived.Record(node, delta_StatBytesReceived);
                 Instrumentation.ClientBytesSent    .Record(node, delta_StatBytesSent);
                 Instrumentation.ClientMsgReceived  .Record(node, delta_StatMsgReceived);
                 Instrumentation.ClientMsgSent      .Record(node, delta_StatMsgSent);
                 Instrumentation.ClientErrors       .Record(node, delta_StatErrors);

                 Instrumentation.ClientTotalBytesReceived.Record(node, m_StatBytesReceived);
                 Instrumentation.ClientTotalBytesSent    .Record(node, m_StatBytesSent);
                 Instrumentation.ClientTotalMsgReceived  .Record(node, m_StatMsgReceived);
                 Instrumentation.ClientTotalMsgSent      .Record(node, m_StatMsgSent);
                 Instrumentation.ClientTotalErrors       .Record(node, m_StatErrors);

                 if (Binding.MeasureStatTimes)
                  foreach(var kvp in m_StatTimes)
                    Instrumentation.ClientCallRoundtripTime.Record(kvp.Key, kvp.Value * 1000);//Value is in sec, but gauge measures in milliseconds
              }
              else
              {
                 Instrumentation.ServerBytesReceived.Record(node, delta_StatBytesReceived);
                 Instrumentation.ServerBytesSent    .Record(node, delta_StatBytesSent);
                 Instrumentation.ServerMsgReceived  .Record(node, delta_StatMsgReceived);
                 Instrumentation.ServerMsgSent      .Record(node, delta_StatMsgSent);
                 Instrumentation.ServerErrors       .Record(node, delta_StatErrors);

                 Instrumentation.ServerTotalBytesReceived.Record(node, m_StatBytesReceived);
                 Instrumentation.ServerTotalBytesSent    .Record(node, m_StatBytesSent);
                 Instrumentation.ServerTotalMsgReceived  .Record(node, m_StatMsgReceived);
                 Instrumentation.ServerTotalMsgSent      .Record(node, m_StatMsgSent);
                 Instrumentation.ServerTotalErrors       .Record(node, m_StatErrors);
              }
            }


            /// <summary>
            /// Returns how many bytes were received since .ctor or last ResetStats() call
            /// </summary>
            public long StatBytesReceived
            {
                get { return m_StatBytesReceived; }
            }

            /// <summary>
            /// Returns how many bytes were sent since .ctor or last ResetStats() call
            /// </summary>
            public long StatBytesSent
            {
                get { return m_StatBytesSent; }
            }


            /// <summary>
            /// Returns how many messages were received since .ctor or last ResetStats() call
            /// </summary>
            public long StatMsgReceived
            {
                get { return m_StatMsgReceived; }
            }

            /// <summary>
            /// Returns how many messages were sent since .ctor or last ResetStats() call
            /// </summary>
            public long StatMsgSent
            {
                get { return m_StatMsgSent; }
            }

            /// <summary>
            /// Returns how many message processing errors happened since .ctor or last ResetStats() call
            /// </summary>
            public long StatErrors
            {
                get { return m_StatErrors; }
            }


            /// <summary>
            /// Returns enumerable of named times measured in double second fractions.
            /// The returned times are EMA-filtered from supplied individual measurement samples
            /// </summary>
            public IEnumerable<KeyValuePair<string, double>> StatTimes
            {
               get { return m_StatTimes; }
            }

        #endregion


        #region Public

            /// <summary>
            /// A thread-safe operation that tries to acquire(reserve) this instance for exclusive use.
            /// Returns true if acqusition succeded, false is this instance is reserved by someone else
            /// </summary>
            public bool TryAcquire()
            {
                return Interlocked.CompareExchange(ref m_Acquired, IS_ACQUIRED, NOT_ACQUIRED) == NOT_ACQUIRED;
            }

            /// <summary>
            /// Releases transport by setting Acquired flag to false so it can be used by other operations
            /// </summary>
            public void Release()
            {
               Interlocked.Exchange(ref m_Acquired, NOT_ACQUIRED);
            }


            /// <summary>
            /// Ensures that application and transport instance are running or throws otherwise
            /// </summary>
            public void CheckRunningState()
            {
                if (!App.Active || !Running)
                    throw m_TransportKind==TransportKind.Client ?
                            (GlueException)new ClientCallException(CallStatus.DispatchError, StringConsts.GLUE_SYSTEM_NOT_RUNNING_ERROR) :
                            (GlueException)new ServerNotRunningException(StringConsts.GLUE_SYSTEM_NOT_RUNNING_ERROR);
            }

        #endregion

        #region Protected

            protected override void DoStart()
            {
                Binding.CheckRunningState( m_TransportKind == NFX.Glue.TransportKind.Client );
                base.DoStart();
            }

        #endregion
    }


    public abstract class ClientTransport : Transport
    {
        protected ClientTransport(Binding binding) : base(binding, TransportKind.Client)
        {
        }

        /// <summary>
        /// Sends a client request into remote endpoint.
        /// This is a blocking call for bindings that are OperationFlow.Synchronous and
        /// result arrives immediately into CallSlot.
        /// </summary>
        public CallSlot SendRequest(ClientEndPoint endpoint, RequestMsg request, CallOptions options)
        {
            CheckRunningState();
            return DoSendRequest(endpoint, request, options);
        }

        /// <summary>
        /// Override to send a client request into remote endpoint.
        /// This is a blocking call for bindings that are OperationFlow.Synchronous and
        /// result arrives immediately into CallSlot.
        /// </summary>
        protected abstract CallSlot DoSendRequest(ClientEndPoint endpoint, RequestMsg request, CallOptions options);

    }


    public abstract class ServerTransport : Transport
    {
        /// <summary>
        /// Creates an instance of listener transport
        /// </summary>
        protected ServerTransport(Binding binding, ServerEndPoint serverEndpoint)
            : base(binding, TransportKind.Server)
        {
            m_ServerEndpoint = serverEndpoint;
        }


        /// <summary>
        /// Creates instance of a transport that resulted from connection accept by
        /// listener. Not all technologies support listener/child transports (TCP does)
        /// </summary>
        protected ServerTransport(Binding binding, ServerTransport listener)
            : base(binding, TransportKind.Server)
        {
            m_Listener = listener;
            m_ServerEndpoint = listener.m_ServerEndpoint;
        }


        private ServerTransport m_Listener;
        private ServerEndPoint m_ServerEndpoint;

        /// <summary>
        /// Returns server endpoint instance  that opened this transport
        /// </summary>
        public ServerEndPoint ServerEndpoint
        {
            get { return m_ServerEndpoint; }
        }

        /// <summary>
        /// Indicates whether this instance is the one that accepts connections.
        /// Depending on implementation listeners may allocate other transports after
        /// they accept incoming connect request (i.e. TCP)
        /// </summary>
        public bool IsListener
        {
            get { return m_Listener == null; }
        }

        /// <summary>
        /// Returns a listener transport that opened this one. If this is a listener
        /// transport then returns null. Not all technologies support listener/child
        /// transports (TCP does)
        /// </summary>
        public ServerTransport Listener
        {
            get { return m_Listener; }
        }

        /// <summary>
        /// Sends a response into remote client endpoint
        /// </summary>
        public bool SendResponse(ResponseMsg response)
        {
            CheckRunningState();
            return DoSendResponse(response);
        }

        /// <summary>
        /// Override to send a response into remote client endpoint
        /// </summary>
        protected abstract bool DoSendResponse(ResponseMsg response);

    }



    public abstract class ClientTransport<TBinding> : ClientTransport where TBinding : Binding
    {
        public ClientTransport(Binding binding) : base(binding)
        {
        }

        public new TBinding Binding { get { return base.Binding as TBinding; } }
    }



    public abstract class ServerTransport<TBinding> : ServerTransport where TBinding : Binding
    {
        /// <summary>
        /// Creates an instance of listener transport
        /// </summary>
        protected ServerTransport(Binding binding, ServerEndPoint serverEndpoint)
            : base(binding, serverEndpoint)
        {
        }


        /// <summary>
        /// Creates instance of a transport that resulted from connection accept by listener.
        /// Not all technologies support listener/child transports (TCP does)
        /// </summary>
        protected ServerTransport(Binding binding, ServerTransport listener)
            : base(binding, listener)
        {
        }

        public new TBinding Binding { get { return base.Binding as TBinding; } }
    }
}
