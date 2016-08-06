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
using System.Net;
using System.Net.Sockets;
using System.IO;

using NFX.Environment;

namespace NFX.Glue.Native
{

  /// <summary>
  /// Delivers wire msg from another side. DO NOT use WireMsg beyound the implementation of this delegate,
  ///  if needed make a copy of msg.Data as the memory will be freed after this call returns
  /// </summary>
  public delegate void MpxSocketReceiveAction<TTransport>(MpxSocket<TTransport> sender, WireMsg msg) where TTransport : Transport, IMpxTransport;



  /// <summary>
  /// Base for MpxSockets implementation. An MpxSocket represents an asynchronous bi-directional channel of communication
  /// that delivers binary/wire messages of flexible size.
  /// The socket is NOT THREAD SAFE for calling Send() from parallel threads. Send() is done synchronously on the
  /// calling thread, i.e. the sender waits until the data is written into the OS socket buff.
  ///  Receive is invoked by internal/system/io thread asynchronously and should not block for long (seconds)
  /// </summary>
  public abstract class MpxSocket<TTransport> : DisposableObject where TTransport : Transport, IMpxTransport
  {
      protected MpxSocket(TTransport transport,
                          IPEndPoint endPoint,
                          ClientSite clientSite,
                          MpxSocketReceiveAction<TTransport> receiveAction)
      {
        m_Transport = transport;
        m_EndPoint = endPoint;
        m_ClientSite = clientSite;
        m_ReceiveAction = receiveAction;


        m_SendChunk = ((IMpxTransport)transport).Binding.MakeOrReuseChunk();
        m_ReceiveChunk = ((IMpxTransport)transport).Binding.MakeOrReuseChunk();
      }

             private object ____disposeLock = new object();

      public override void Dispose() //override needed because multiple threads may want to Dispose() in parallel
      {
        lock(____disposeLock) base.Dispose();
      }

      protected override void Destructor()
      {
        m_Disposing = true;
      }

      /// <summary>
      /// Releases socket memory chunks. This method MUST BE called AFTER all pending async IO is done,
      /// otherwise the chunks may get released to the pool and get corrupted by the prior operation
      /// </summary>
      protected void ReleaseMemChunksAfterAsyncOperations()
      {
        lock(m_ChunkReleasedLock)
        {
          if (m_ChunksReleased) return;
          m_ChunksReleased = true;
          DoReleaseMemChunksAfterAsyncOperations();
        }
      }

      /// <summary>
      /// Do not call this method, override only when freeing more than 2 standard chunks (send/recv)
      /// </summary>
      protected virtual void DoReleaseMemChunksAfterAsyncOperations()
      {
        ((IMpxTransport)m_Transport).Binding.ReleaseChunk(m_ReceiveChunk);
        ((IMpxTransport)m_Transport).Binding.ReleaseChunk(m_SendChunk);
      }

      protected volatile bool m_Disposing;

      private object m_ChunkReleasedLock = new object();
      private bool m_ChunksReleased;

      private bool m_SendChunkGotten;
      protected MemChunk m_SendChunk;
      protected MemChunk m_ReceiveChunk;
      protected readonly TTransport m_Transport;
      protected readonly IPEndPoint m_EndPoint;
      protected readonly ClientSite m_ClientSite;
      private MpxSocketReceiveAction<TTransport> m_ReceiveAction;
      protected DateTime? m_LastIdleManagerVisit;


      protected Socket m_Socket;

      /// <summary>
      /// Returns transport that this socket is under
      /// </summary>
      public TTransport Transport { get{return m_Transport;}}


      /// <summary>
      /// Returns IP End point that this socket services, remote server for client socket and remote client for server sockets
      /// </summary>
      public IPEndPoint EndPoint { get{ return m_EndPoint;}}


      /// <summary>
      /// Returns the underlying socket
      /// </summary>
      public Socket Socket{get{ return m_Socket;}}


      /// <summary>
      /// True when socket is not disposing and runtime is not shutting down
      /// </summary>
      public bool Active{ get {return !m_Disposing && !Disposed && Transport.Running;} }


      /// <summary>
      /// Returns client site for the computer that initiated the call.
      /// Server socket returns the transmitted value from client. Client socket returns the one that was
      ///  sent to server upon handshake
      /// </summary>
      public ClientSite ClientSite { get {return m_ClientSite;}}



      /// <summary>
      /// Returns timestamp of last manager visit since then no traffic came through. Null indicates that traffic came though
      ///  and socket has not been idle since last visit. Manager is an extrenal visitor that closes idle sockets
      /// </summary>
      public DateTime? LastIdleManagerVisit{ get {return m_LastIdleManagerVisit; }}

      /// <summary>
      /// Adds the specified socket msg to the outgoing stack.
      /// This is a synchronous blocking call that executes until the data is written
      /// into socket stack (but not necessarily delivered). If send error occurs then socket is
      /// marked as !Active (Disposed)
      /// </summary>
      public void Send(WireMsg data)
      {
        m_LastIdleManagerVisit = null;//taffic came through, reset idle timestamp
        DoSend(data);
      }

      protected abstract void DoSend(WireMsg data);


      /// <summary>
      /// Reserves a chunk of memory of the suggested size for network send operation.
      /// The buffer must be released by a call to ReleaseSendBuffer().
      /// Keep in mind that this method is NOT RE-ENTRANT by this or any other thread until corresponding ReleaseSendChunk() is called.
      /// MpxSocket is not thread safe for sends in general.
      /// </summary>
      public virtual MemChunk GetSendChunk(int desiredSize = 0)
      {
        if (m_SendChunkGotten)
         throw new ProtocolException(StringConsts.GLUE_MPX_SOCKET_SEND_CHUNK_ALREADY_GOTTEN_ERROR);

        m_SendChunkGotten = true;

        return m_SendChunk;
      }

      /// <summary>
      /// Releases previously reserved buffer, i.e. may trim excess allocation after a large message
      /// </summary>
      public virtual void ReleaseSendChunk()
      {
        m_SendChunkGotten = false;
        //nothing to physically release here
      }

      /// <summary>
      /// Override to perform maintenance i.e. trim excessive m_SendChunk size etc..
      /// This method is called by Glue/Binding/Transport runtime.
      /// Also resets last last idle manager visit timestamp which is used for socket closing
      /// </summary>
      public virtual void AcceptManagerVisit(DateTime when)
      {
        var copy = m_LastIdleManagerVisit;
        if (!copy.HasValue) m_LastIdleManagerVisit = when;
      }


      /// <summary>
      /// Calls ReceiveAction callback guarding for possible unhandled receive action errors
      /// </summary>
      protected void InvokeReceive(WireMsg msg)
      {
        try
        {
          m_LastIdleManagerVisit = null;//taffic came through, reset idle timestamp
          m_ReceiveAction(this, msg);
        }
        catch(Exception error)
        {
          Transport.Binding.WriteLog(this is MpxServerSocket ?  LogSrc.Server : LogSrc.Client,
                                     Log.MessageType.Error,
                                     StringConsts.GLUE_MPX_SOCKET_RECEIVE_ACTION_ERROR + error.ToMessageWithType(),
                                       "{0}.processRecive()".Args(GetType().Name), error);
        }

      }

  }


  /// <summary>
  /// Represents client-side asynchronous socket. This socket initiates a connection to MpxServerSocket
  /// </summary>
  public abstract class MpxClientSocket : MpxSocket<MpxClientTransport>
  {
      protected internal MpxClientSocket(MpxClientTransport transport,
                                IPEndPoint remoteServer,
                                ClientSite clientSite,
                                MpxSocketReceiveAction<MpxClientTransport> receiveAction)
                                : base(transport, remoteServer, clientSite, receiveAction)
      {
      }
  }





  /// <summary>
  /// Represents server-side asynchronous socket that clients connect to
  /// </summary>
  public abstract class MpxServerSocket : MpxSocket<MpxServerTransport>
  {

      protected internal MpxServerSocket(MpxListener listener,
                                IPEndPoint clientEndPoint,
                                ClientSite clientSite,
                                MpxSocketReceiveAction<MpxServerTransport> receiveAction
                                ) : base(listener.Transport, clientEndPoint,  clientSite, receiveAction)
      {
        m_Listener = listener;
        listener.Transport.ClientSiteConnected(this);
      }

      protected override void Destructor()
      {
        m_Listener.Transport.ClientSiteDisconnected(this);
        base.Destructor();
      }


      private MpxListener m_Listener;


      /// <summary>
      /// Returns listener socket that initiated/opened this server socket
      /// </summary>
      public MpxListener Listener{ get {return m_Listener;}}


      public override void AcceptManagerVisit(DateTime when)
      {
        //Note: This code below is not 100% thread-safe because when determination is made
        // that socket has been idle the socket may right then strat to receive data in another thread, which is
        // OK because such case is pretty rare (if socket has been idle for 5 minutes) but locking every socket send/receive is very slow
        // and is not worth it. What will happen is socket.Active will become false and any ongong send/receives will fail
        var lastIdleVisit = m_LastIdleManagerVisit;//thread-safe copy
        if (lastIdleVisit.HasValue)
        {
          var tMs = Transport.Binding.ServerTransportIdleTimeoutMs;
          if (tMs>0)
          {
            if ((when - lastIdleVisit.Value).TotalMilliseconds > tMs)
            {
              Instrumentation.InactiveServerTransportClosedEvent.Happened(Transport.Node);
              Transport.Binding.WriteLog(LogSrc.Server, Log.MessageType.TraceGlue,
                                        "Server transport on '{0}' closed idle client socket from '{1}'".Args(Transport.Node, ClientSite.Name),
                                        GetType().Name+".AcceptManagerVisit(when)");
              m_Disposing = true;//just mark for removal/Dispose
            }
          }
        }
        base.AcceptManagerVisit(when);
      }

  }



  /// <summary>
  /// Represents a server-side listener socket that accepts the calls
  /// </summary>
  public abstract class MpxListener : DisposableObject
  {
      protected internal MpxListener(MpxServerTransport transport,
                                     IPEndPoint listenEndPoint,
                                     MpxSocketReceiveAction<MpxServerTransport> socketReceiveAction)
      {
        m_Transport = transport;
        m_EndPoint = listenEndPoint;
        m_SocketReceiveAction = socketReceiveAction;
      }


      private MpxServerTransport m_Transport;
      protected readonly IPEndPoint m_EndPoint;
      protected readonly MpxSocketReceiveAction<MpxServerTransport> m_SocketReceiveAction;

      /// <summary>
      /// Returns transpoirt that this listener listens under
      /// </summary>
      public MpxServerTransport Transport{ get {return m_Transport;}}


      /// <summary>
      /// Returns IP End point that this listener listens on
      /// </summary>
      public IPEndPoint EndPoint { get{ return m_EndPoint;}}
  }


  /// <summary>
  /// Defines factory abstraction that creates socket per particular technology
  /// </summary>
  public abstract class MpxSocketFactory : IConfigurable
  {

    public abstract MpxClientSocket
           MakeClientSocket(MpxClientTransport transport,
                            IPEndPoint remoteServer,
                            ClientSite clientSite,
                            MpxSocketReceiveAction<MpxClientTransport> receiveAction);

    public abstract MpxListener
           MakeListener(MpxServerTransport transport,
                        IPEndPoint listenEndPoint,
                        MpxSocketReceiveAction<MpxServerTransport> socketReceiveAction);


    public abstract void Configure(IConfigSectionNode node);
  }



}
