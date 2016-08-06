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
using System.Threading.Tasks;
using System.IO;
using System.Net;

using NFX.Environment;
using NFX.Serialization;
using NFX.Serialization.Slim;

namespace NFX.Glue.Native
{

  public interface IMpxTransport
  {
    MpxBinding Binding { get;}
  }


  /// <summary>
  /// Provides asynchronous communicating pattern based on asynchronous virtual socket.
  /// The concrete socket may be based on OS-supported technology i.e. -  IO completion ports on Windows
  /// </summary>
  public sealed class MpxBinding : Binding
  {
       #region CONSTS

            public const int DEFAULT_RCV_TIMEOUT = 0;
            public const int DEFAULT_SND_TIMEOUT = 0;

            public const int DEFAULT_PORT = 8237;

            public const string CONFIG_SOCKET_FACTORY_SECT = "socket-factory";


            private const int CHUNK_DORMANT_PERIOD_SEC = 60;

            public const int DEFAULT_IDLE_CHUNK_LIFE_SEC = 10 * 60;
            public const int DEFAULT_MAX_CHUNK_POOL_COUNT = 1024;

            public const int DEFAULT_SOCKET_LINGER_SEC = 3;

        #endregion

        #region .ctor
            public MpxBinding(string name) : base (name)
            {

            }

            public MpxBinding(IGlueImplementation glue, string name) : base(glue, name)
            {

            }

        #endregion


         #region Fields

            private ISerializer m_Serializer;
            private MpxSocketFactory m_SocketFactory;
            private List<MemChunk> m_Chunks;

            private int m_MaxMsgSize = Consts.DEFAULT_MAX_MSG_SIZE;

            private int m_ServerReceiveBufferSize = Consts.DEFAULT_RCV_BUFFER_SIZE;
            private int m_ServerSendBufferSize    = Consts.DEFAULT_SND_BUFFER_SIZE;
            private int m_ClientReceiveBufferSize = Consts.DEFAULT_RCV_BUFFER_SIZE;
            private int m_ClientSendBufferSize    = Consts.DEFAULT_SND_BUFFER_SIZE;

            private int m_ServerReceiveTimeout    = DEFAULT_RCV_TIMEOUT;
            private int m_ServerSendTimeout       = DEFAULT_SND_TIMEOUT;
            private int m_ClientReceiveTimeout    = DEFAULT_RCV_TIMEOUT;
            private int m_ClientSendTimeout       = DEFAULT_SND_TIMEOUT;

            private int m_SocketLingerSec         = DEFAULT_SOCKET_LINGER_SEC;

            private int m_IdleChunkLifeSec  =  DEFAULT_IDLE_CHUNK_LIFE_SEC;
            private int m_MaxChunkPoolCount =  DEFAULT_MAX_CHUNK_POOL_COUNT;
        #endregion


        #region Properties

          /// <summary>
          /// Mpx binding is always async by definition
          /// </summary>
          public override OperationFlow OperationFlow
          {
            get { return OperationFlow.Asynchronous; }
          }

          public override string EncodingFormat
          {
              get { return Consts.SLIM_FORMAT; }
          }


          internal ISerializer Serializer { get { return m_Serializer; } }


          /// <summary>
          /// Returns factory used to make new socket instances per particular technology
          /// </summary>
          public MpxSocketFactory SocketFactory { get{ return m_SocketFactory; }}


          /// <summary>
            /// Imposes a limit on maximum message size in bytes
            /// </summary>
            [Config("$" + Consts.CONFIG_MAX_MSG_SIZE_ATTR, Consts.DEFAULT_MAX_MSG_SIZE)]
            public int MaxMsgSize
            {
               get { return m_MaxMsgSize; }
               set { m_MaxMsgSize = value < Consts.MAX_MSG_SIZE_LOW_BOUND ? Consts.MAX_MSG_SIZE_LOW_BOUND : value;}
            }


            [Config(CONFIG_SERVER_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_RCV_BUF_SIZE_ATTR, Consts.DEFAULT_RCV_BUFFER_SIZE)]
            public int ServerReceiveBufferSize
            {
               get { return m_ServerReceiveBufferSize; }
               set { m_ServerReceiveBufferSize = value <=0 ? Consts.DEFAULT_RCV_BUFFER_SIZE : value;}
            }

            [Config(CONFIG_SERVER_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_SND_BUF_SIZE_ATTR, Consts.DEFAULT_SND_BUFFER_SIZE)]
            public int ServerSendBufferSize
            {
               get { return m_ServerSendBufferSize; }
               set { m_ServerSendBufferSize = value <=0 ? Consts.DEFAULT_SND_BUFFER_SIZE : value;}
            }

            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_RCV_BUF_SIZE_ATTR, Consts.DEFAULT_RCV_BUFFER_SIZE)]
            public int ClientReceiveBufferSize
            {
               get { return m_ClientReceiveBufferSize; }
               set { m_ClientReceiveBufferSize = value <=0 ? Consts.DEFAULT_RCV_BUFFER_SIZE : value;}
            }

            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_SND_BUF_SIZE_ATTR, Consts.DEFAULT_SND_BUFFER_SIZE)]
            public int ClientSendBufferSize
            {
               get { return m_ClientSendBufferSize; }
               set { m_ClientSendBufferSize = value <=0 ? Consts.DEFAULT_SND_BUFFER_SIZE : value;}
            }

            [Config(CONFIG_SERVER_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_RCV_TIMEOUT_ATTR, DEFAULT_RCV_TIMEOUT)]
            public int ServerReceiveTimeout
            {
               get { return m_ServerReceiveTimeout; }
               set { m_ServerReceiveTimeout = value <0 ? DEFAULT_RCV_TIMEOUT : value;}
            }

            [Config(CONFIG_SERVER_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_SND_TIMEOUT_ATTR, DEFAULT_SND_TIMEOUT)]
            public int ServerSendTimeout
            {
               get { return m_ServerSendTimeout; }
               set { m_ServerSendTimeout = value <0 ? DEFAULT_SND_TIMEOUT : value;}
            }

            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_RCV_TIMEOUT_ATTR, DEFAULT_RCV_TIMEOUT)]
            public int ClientReceiveTimeout
            {
               get { return m_ClientReceiveTimeout; }
               set { m_ClientReceiveTimeout = value <0 ? DEFAULT_RCV_TIMEOUT : value;}
            }

            [Config(CONFIG_CLIENT_TRANSPORT_SECTION + ATTR_SLASH_PATH + Consts.CONFIG_SND_TIMEOUT_ATTR, DEFAULT_SND_TIMEOUT)]
            public int ClientSendTimeout
            {
               get { return m_ClientSendTimeout; }
               set { m_ClientSendTimeout = value <0 ? DEFAULT_SND_TIMEOUT : value;}
            }


            /// <summary>
            /// Specifies the maximum length of life of an unused memory chunk in the pool.
            /// The idle chunk will be evicted after this interval.
            /// This setting is common for both server and client as they share the same pool
            /// </summary>
            [Config(null, DEFAULT_IDLE_CHUNK_LIFE_SEC)]
            public int IdleChunkLifeSec
            {
               get { return m_IdleChunkLifeSec; }
               set { m_IdleChunkLifeSec = value <=0 ? DEFAULT_IDLE_CHUNK_LIFE_SEC : value;}
            }

            /// <summary>
            /// Imposes a limit on how many memory chunks can be stored in free chunk pool.
            /// The chunks can be resued for making new connections.
            /// This setting is common for both server and client as they share the same pool
            /// </summary>
            [Config(null, DEFAULT_MAX_CHUNK_POOL_COUNT)]
            public int MaxChunkPoolCount
            {
               get { return m_MaxChunkPoolCount; }
               set { m_MaxChunkPoolCount = value <=0 ? DEFAULT_MAX_CHUNK_POOL_COUNT : value;}
            }


            /// <summary>
            /// Sets socket linger options.
            /// This setting is common for both server and client as they have the same channel semantics
            /// </summary>
            [Config(null, DEFAULT_SOCKET_LINGER_SEC)]
            public int SocketLingerSec
            {
               get { return m_SocketLingerSec; }
               set { m_SocketLingerSec = value <0 ? DEFAULT_SOCKET_LINGER_SEC : value;}
            }

        #endregion



        #region Public

          public override bool AreNodesIdentical(Node left, Node right)
          {
             return  left.Assigned && right.Assigned && left.ConnectString.EqualsIgnoreCase( right.ConnectString);
          }

        #endregion


        #region Protected

          protected override void DoConfigure(IConfigSectionNode node)
          {
            base.DoConfigure(node);

            var nsf = node[CONFIG_SOCKET_FACTORY_SECT];
            if (nsf.Exists)
              m_SocketFactory = FactoryUtils.MakeAndConfigure<MpxSocketFactory>(nsf, typeof(MpxWinSocketFactory));
          }


          internal static IPEndPoint ToIPEndPoint(Node node)
          {
                return (node.Host + ':' + node.Service).ToIPEndPoint(DEFAULT_PORT);
          }


          protected override void DoStart()
          {
            base.DoStart();
            m_Serializer = new SlimSerializer(SyncBinding.KNOWN_SERIALIZER_TYPES);
            if (m_SocketFactory==null)
             m_SocketFactory = new MpxWinSocketFactory();

            m_Chunks = new List<MemChunk>();
          }

          protected override void DoWaitForCompleteStop()
          {
            base.DoWaitForCompleteStop();
            m_Serializer = null;
            m_Chunks = null;
          }


          protected override ClientTransport MakeNewClientTransport(ClientEndPoint client)
          {
             return new MpxClientTransport(this, client.Node);
          }

          protected internal override ServerTransport OpenServerEndpoint(ServerEndPoint epoint)
          {
              var cfg = ConfigNode.NavigateSection(CONFIG_SERVER_TRANSPORT_SECTION);
              if (!cfg.Exists) cfg = ConfigNode;

              var ipep = MpxBinding.ToIPEndPoint(epoint.Node);
              var transport = new MpxServerTransport(this, epoint, ipep.Address, ipep.Port);
              transport.Configure(cfg);
              transport.Start();

              return transport;
          }

          protected internal override void CloseServerEndpoint(ServerEndPoint epoint)
          {
            var t = epoint.Transport;
            if (t!=null) t.Dispose();
          }

          /// <summary>
          /// Fetches available existing chunk from cache or creates a new one.
          /// Called by new sockets by incipient connections
          /// </summary>
          internal MemChunk MakeOrReuseChunk()
          {
            var chunks = m_Chunks;

            MemChunk chunk = null;

            if (chunks!=null)
            {
              var now = App.TimeSource.UTCNow;

              lock(m_Chunks)//since this method is called on new socket creation, it is OK to lock for short time
              {
                 for(var i=0; i<m_Chunks.Count; i++)
                 {
                    chunk = m_Chunks[i];
                    if (chunk.Acquired) continue;//is already taken by some socket

                    if ((now - chunk._LastReleaseUtc).TotalSeconds >= CHUNK_DORMANT_PERIOD_SEC) //chunk must be in the pool for the specified minimum 'rest' time
                    {                                                                           // this is needed so all pending async socket operations timeout
                      chunk.Acquired = true;
                      return chunk;
                    }
                 }
              }
            }

            //otherwise create new chunk, no need to add it to pool here. The release will do it later (if ever called)
            chunk =  new MemChunk(Consts.DEFAULT_SERIALIZER_STREAM_CAPACITY);
            return chunk;
          }

          internal void ReleaseChunk(MemChunk chunk)
          {
            if (chunk==null || !Running) return;

            chunk.Acquired = false;
            chunk._LastReleaseUtc = App.TimeSource.UTCNow;

            WriteLog(LogSrc.Any,
                     Log.MessageType.TraceGlue,
                     "Chunk released back to pool; Used {0} capacity {1} ".Args(chunk.stat_MaxUsedPosition, chunk.stat_MaxLength));

            var chunks = m_Chunks;
            if (chunks==null) return;

            lock(chunks)
            {
              if (m_Chunks.Count < m_MaxChunkPoolCount)
              {
                for(var i=0; i<m_Chunks.Count; i++)
                  if (object.ReferenceEquals(m_Chunks[i], chunk))  return;//already in pool

                chunks.Add(chunk);
              }
            }
          }


          protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
          {
            var now = App.TimeSource.UTCNow;
            evictOldUnusedChunks(now);
            visitServerTransports(now);
            base.DoAcceptManagerVisit(manager, managerNow);
          }



      #endregion


      #region .pvt

                  private DateTime lastChunkStackPurge = DateTime.MinValue;

                  private void evictOldUnusedChunks(DateTime now)
                  {
                    const int PURGE_EVERY_SEC = 72;//every sec will re-examine the pool

                    var chunks = m_Chunks;
                    if (chunks==null) return;

                    if ((now - lastChunkStackPurge).TotalSeconds < PURGE_EVERY_SEC) return;
                    lastChunkStackPurge = now;

                    lock(chunks)
                    {
                      for(var i=0; i<chunks.Count;)
                      {
                        var chunk = chunks[i];
                        if (!chunk.Acquired && (now - chunk._LastReleaseUtc).TotalSeconds > m_IdleChunkLifeSec)
                        {
                          chunks.RemoveAt(i);//evict chunk
                          continue;
                        }
                        i++;
                      }
                    }//lock
                  }

                  private void visitServerTransports(DateTime now)
                  {
                      foreach(var transport in Transports.Where(t => t is MpxServerTransport).Cast<MpxServerTransport>())
                       transport.AcceptManagerVisit(this, now);
                  }


      #endregion


  }



}
