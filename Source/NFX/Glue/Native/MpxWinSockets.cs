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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using NFX.Environment;

namespace NFX.Glue.Native
{

    /// <summary>
    /// Factory that makes MpxWin sockets based on Windows-IO completion ports
    /// </summary>
    public sealed class MpxWinSocketFactory : MpxSocketFactory
    {


      public override MpxClientSocket MakeClientSocket(MpxClientTransport transport,
                                                       IPEndPoint remoteServer,
                                                       ClientSite clientSite,
                                                       MpxSocketReceiveAction<MpxClientTransport> receiveAction)
      {
        return new MpxWinClientSocket(transport, remoteServer, clientSite, receiveAction);
      }

      public override MpxListener MakeListener(MpxServerTransport transport,
                                               IPEndPoint listenEndPoint,
                                               MpxSocketReceiveAction<MpxServerTransport> socketReceiveAction)
      {
        return new MpxWinListener(transport, listenEndPoint, socketReceiveAction);
      }

      public override void Configure(IConfigSectionNode node) { }
    }//factory




                      internal sealed class SocketState : SocketAsyncEventArgs
                      {
                        public SocketState(Socket socket, byte[] buffer) : base()
                        {
                          this.AcceptSocket = socket;
                          SetBuffer(buffer, 0, (int)buffer.Length);
                          Reset();
                        }
                        public void Reset()
                        {
                          WireMsgSize = -1;
                          SetBuffer(0, sizeof(int));
                        }
                        public int WireMsgSize;
                        public bool HasMsgSize{ get {return WireMsgSize>=0;}}
                      }


    /// <summary>
    /// Implements a MPX client socket using Windows IO completion ports for receive
    /// </summary>
    public sealed class MpxWinClientSocket : MpxClientSocket
    {
      internal MpxWinClientSocket(MpxClientTransport transport,
                                 IPEndPoint remoteServer,
                                 ClientSite clientSite,
                                 MpxSocketReceiveAction<MpxClientTransport> receiveAction) : base(transport, remoteServer, clientSite, receiveAction)
      {
        m_Client = new TcpClient();
        m_Client.Connect(m_EndPoint);
        m_Socket = m_Client.Client;

        m_Client.NoDelay = true;
        m_Client.LingerState.Enabled = true;
        m_Client.LingerState.LingerTime = m_Transport.Binding.SocketLingerSec;


        m_Client.ReceiveBufferSize =  m_Transport.Binding.ClientReceiveBufferSize;
        m_Client.SendBufferSize    =  m_Transport.Binding.ClientSendBufferSize;

        m_Client.ReceiveTimeout    =  m_Transport.Binding.ClientReceiveTimeout;
        m_Client.SendTimeout       =  m_Transport.Binding.ClientSendTimeout;


        //Send ClientSite right away
        m_SendChunk.Position = 0;
        var sz = m_ClientSite.Serialize( m_SendChunk );
        m_Client.GetStream().Write(m_SendChunk.GetBuffer(), 0, sz);
        //------------------


        m_RArgs = new SocketState(m_Client.Client, m_ReceiveChunk.GetBuffer());
        m_RArgs.Completed += socketCallback;

        initiateReceive();
      }

      protected override void Destructor()
      {
        m_Disposing = true;

        //1 close socket before calling base
        if (m_Client!=null)
        {
          try
          {
            m_Client.Close();
          }
          catch(Exception error)
          {
            Transport.Binding.WriteLog(LogSrc.Client, Log.MessageType.Error, error.ToMessageWithType(), "{0}.Destructor()".Args(GetType().Name));
          }
          m_Client = null;
        }

        //2 dealloc buffers will be done on ASYNC completion
        base.Destructor();
      }


      private TcpClient m_Client;
      private SocketState m_RArgs;


      protected override void DoSend(WireMsg data)
      {
         if (!Active)
           throw new ProtocolException(StringConsts.GLUE_MPX_SOCKET_SEND_CLOSED_ERROR, closeChannel: true);

         try
         {
            data.UpdateBufferStats();
            m_Client.GetStream().Write(data.Buffer, 0, data.BufferUsedSize);
         }
         catch(Exception error)
         {
            Dispose();
            throw new ProtocolException(error.Message, error, closeChannel: true);
         }
      }


      private void initiateReceive()
      {
        if (!Active) return;

        m_ReceiveChunk.Position = 0;
        m_RArgs.Reset();
        var pending =  m_Client.Client.ReceiveAsync(m_RArgs);
        if (!pending)
        {
          processReceive(m_RArgs);
        }
      }

      private void continueReceive()
      {
        if (!Active) return;

        var pending =  m_Client.Client.ReceiveAsync(m_RArgs);
        if (!pending)
        {
          processReceive(m_RArgs);
        }
      }

      private void socketCallback(object sender, SocketAsyncEventArgs args)
      {
        if (!Active)
        {
          ReleaseMemChunksAfterAsyncOperations();
          return;
        }

        if (args.LastOperation == SocketAsyncOperation.Receive)
        {
           processReceive((SocketState)args);
           return;
        }
        Dispose();//close the socket otherwise
        ReleaseMemChunksAfterAsyncOperations();
      }


      private void processReceive(SocketState state)
      {
        try
        {
          processReceiveCore(state);
        }
        catch(Exception error)
        {
          Transport.Binding.WriteLog(LogSrc.Client,
                                   Log.MessageType.Error,
                                   "processReceiveCore() leaked: " + error.ToMessageWithType(),
                                    "{0}.processRecive()".Args(GetType().Name), error);
          Dispose();//if process receive aborted then channel is irreperable
        }
      }

      private void processReceiveCore(SocketState state)
      {
        var readNow = state.BytesTransferred;

        if (state.SocketError != SocketError.Success || readNow <=0)
        {
          Dispose();
          ReleaseMemChunksAfterAsyncOperations();//socket closed/client has disconnected
          return;
        }

        m_ReceiveChunk.Position+=readNow;
        var hasSize = state.HasMsgSize;
        if (!hasSize && m_ReceiveChunk.Position>=sizeof(int))
        {
            state.WireMsgSize = m_ReceiveChunk.GetBuffer().ReadBEInt32();
            hasSize = true;

            //check max msg size
            if (state.WireMsgSize < 1 || state.WireMsgSize>Transport.Binding.MaxMsgSize)
            {
              Instrumentation.ClientGotOverMaxMsgSizeErrorEvent.Happened(Transport.Node);
              Transport.Binding.WriteLog(LogSrc.Client,
                                          Log.MessageType.Error,
                                          StringConsts.GLUE_MAX_MSG_SIZE_EXCEEDED_ERROR.Args(state.WireMsgSize, Transport.Binding.MaxMsgSize, "processReceive()"),
                                          "{0}.processRecive()".Args(GetType().Name));
              // This is unrecoverable error - close the channel!
              Dispose();
              ReleaseMemChunksAfterAsyncOperations();//socket closed
              return;
            }

            if (state.WireMsgSize>m_ReceiveChunk.Length)  //grow chunk if it is not large enough
                m_ReceiveChunk.SetLength(state.WireMsgSize);
        }

        var got = (int)m_ReceiveChunk.Position;

        if (hasSize && got >= state.WireMsgSize) //got all
        {
          WireMsg msg = new WireMsg(m_ReceiveChunk);
          msg.UpdateBufferStats();
          this.InvokeReceive(msg);
          initiateReceive();
          return;
        }

        state.SetBuffer(m_ReceiveChunk.GetBuffer(), got, (hasSize ? state.WireMsgSize : sizeof(int)) - got);
        continueReceive();
      }


    }//MpxWinClientSocket



    /// <summary>
    /// Implements a MPX server socket using Windows IO completion ports for receive
    /// </summary>
    public sealed class MpxWinServerSocket : MpxServerSocket
    {
      internal MpxWinServerSocket(MpxListener listener,
                                TcpClient client,
                                ClientSite clientSite,
                                MpxSocketReceiveAction<MpxServerTransport> receiveAction
                                ) : base(listener, (IPEndPoint)client.Client.RemoteEndPoint, clientSite, receiveAction)
      {
        m_Client = client;
        m_Socket = m_Client.Client;

        m_RArgs = new SocketState(m_Client.Client, m_ReceiveChunk.GetBuffer());
        m_RArgs.Completed += socketCallback;

        initiateReceive();
      }

      protected override void Destructor()
      {
        m_Disposing = true;

        //1 close socket before calling base
        if (m_Client!=null)
        {
          try
          {
            m_Client.Close();
          }
          catch(Exception error)
          {
            Transport.Binding.WriteLog(LogSrc.Server, Log.MessageType.Error, error.ToMessageWithType(), "{0}.Destructor()".Args(GetType().Name));
          }
          m_Client = null;
        }

        //2 dealloc buffers will be done on ASYNC completion
        base.Destructor();
      }


      private TcpClient m_Client;
      private SocketState m_RArgs;

      protected override void DoSend(WireMsg data)
      {
        if (!Active)
           throw new ProtocolException(StringConsts.GLUE_MPX_SOCKET_SEND_CLOSED_ERROR, closeChannel: true);

        try
        {
            data.UpdateBufferStats();
            m_Client.GetStream().Write(data.Buffer, 0, data.BufferUsedSize);
        }
        catch(Exception error)
        {
            Dispose();
            throw new ProtocolException(error.Message, error, closeChannel: true);
        }
      }

      private void initiateReceive()
      {
        if (!Active) return;

        m_ReceiveChunk.Position = 0;
        m_RArgs.Reset();
        var pending =  m_Client.Client.ReceiveAsync(m_RArgs);
        if (!pending)
        {
          processReceive(m_RArgs);
        }
      }

      private void continueReceive()
      {
        if (!Active) return;

        var pending =  m_Client.Client.ReceiveAsync(m_RArgs);
        if (!pending)
        {
          processReceive(m_RArgs);
        }
      }

      private void socketCallback(object sender, SocketAsyncEventArgs args)
      {
        if (!Active)
        {
          ReleaseMemChunksAfterAsyncOperations();
          return;
        }

        if (args.LastOperation == SocketAsyncOperation.Receive)
        {
           processReceive((SocketState)args);
           return;
        }
        Dispose();//close the socket otherwise
        ReleaseMemChunksAfterAsyncOperations();
      }

      private void processReceive(SocketState state)
      {
        try
        {
          processReceiveCore(state);
        }
        catch(Exception error)
        {
          Transport.Binding.WriteLog(LogSrc.Server,
                                   Log.MessageType.Error,
                                   "processReceiveCore() leaked: " + error.ToMessageWithType(),
                                    "{0}.processRecive()".Args(GetType().Name), error);
          Dispose();//if process receive aborted then channel is irreperable
        }
      }

      private void processReceiveCore(SocketState state)
      {
        var readNow = state.BytesTransferred;

        if (state.SocketError != SocketError.Success || readNow <=0)
        {
          Dispose();
          ReleaseMemChunksAfterAsyncOperations();//socket closed/client has disconnected
          return;
        }

        m_ReceiveChunk.Position+=readNow;
        var hasSize = state.HasMsgSize;
        if (!hasSize && m_ReceiveChunk.Position>=sizeof(int))
        {
          state.WireMsgSize = m_ReceiveChunk.GetBuffer().ReadBEInt32();
          hasSize = true;

          //check max msg size
          if (state.WireMsgSize < 1 || state.WireMsgSize>Transport.Binding.MaxMsgSize)
          {
            Instrumentation.ServerGotOverMaxMsgSizeErrorEvent.Happened(Transport.Node);
            Transport.Binding.WriteLog(LogSrc.Server,
                                       Log.MessageType.Error,
                                       StringConsts.GLUE_MAX_MSG_SIZE_EXCEEDED_ERROR.Args(state.WireMsgSize, Transport.Binding.MaxMsgSize, "processReceive()"),
                                       "{0}.processRecive()".Args(GetType().Name));
            // This is unrecoverable error - close the channel!
            Dispose();
            ReleaseMemChunksAfterAsyncOperations();//socket closed
            return;
          }

          if (state.WireMsgSize>m_ReceiveChunk.Length)  //grow chunk if it is not large enough
            m_ReceiveChunk.SetLength(state.WireMsgSize);
        }

        var got = (int)m_ReceiveChunk.Position;

        if (hasSize && got >= state.WireMsgSize) //got all
        {
          WireMsg msg = new WireMsg(m_ReceiveChunk);
          msg.UpdateBufferStats();
          this.InvokeReceive(msg);
          initiateReceive();
          return;
        }

        state.SetBuffer(m_ReceiveChunk.GetBuffer(), got, (hasSize ? state.WireMsgSize : sizeof(int)) - got);
        continueReceive();
      }

    }//MpxWinServerSocket




    /// <summary>
    /// Implements a MPX socket listener that accepts connections into MpxWinServerSocket
    /// </summary>
    public sealed class MpxWinListener : MpxListener
    {

       public const string THREAD_NAME = "MpxServerTransport.Listener";

      internal MpxWinListener(MpxServerTransport transport,
                              IPEndPoint listenEndPoint,
                              MpxSocketReceiveAction<MpxServerTransport> socketReceiveAction) : base(transport, listenEndPoint, socketReceiveAction)
      {
          m_Active = true;
          m_TcpListener = new TcpListener(listenEndPoint);
          m_TcpListener.ExclusiveAddressUse = true;
          m_TcpListener.Start();
          m_Thread = new Thread(listenerThreadSpin);
          m_Thread.Name = THREAD_NAME;
          m_Thread.Start();
      }

      protected override void Destructor()
      {
          m_Active = false;

          if (m_TcpListener!=null)
          {
            m_TcpListener.Stop();//this breaks the blocking call of AcceptTcpClient() in another thread
            m_TcpListener = null;
          }

          if (m_Thread!=null)
          {
            m_Thread.Join();
            m_Thread = null;
          }
      }

      private bool m_Active;
      private TcpListener m_TcpListener;
      private Thread m_Thread;

           private void listenerThreadSpin()
           {
               try
               {
                    while (m_Active)
                    {
                       var client = m_TcpListener.AcceptTcpClient();  //the blocking call is broken by close - as expected

                       //The task synchronously gets ClientSite and then keeps reading asynchronously via completion ports
                       Task.Factory.StartNew(clientThreadSpin, client);
                    }
               }
               catch(Exception error)
               {
                   if (m_Active)//then this is a bad error
                   {
                     Transport.Binding.WriteLog(LogSrc.Server,
                                      Log.MessageType.CatastrophicError,
                                      StringConsts.GLUE_LISTENER_EXCEPTION_ERROR + error.Message,
                                      from: "MpxWinSocket.listenerThreadSpin",
                                      exception: error);

                     Instrumentation.ServerListenerErrorEvent.Happened(Transport.Node);
                   }
               }
           }


           private void clientThreadSpin(object cli)//executes in its own thread/Task
           {
              TcpClient client = null;
              try
              {
                client = (TcpClient)cli;
                client.NoDelay = true;
                client.LingerState.Enabled = true;
                client.LingerState.LingerTime = Transport.Binding.SocketLingerSec;

                client.ReceiveBufferSize =  Transport.Binding.ServerReceiveBufferSize;
                client.SendBufferSize    =  Transport.Binding.ServerSendBufferSize;

                client.ReceiveTimeout    =  Transport.Binding.ServerReceiveTimeout;
                client.SendTimeout       =  Transport.Binding.ServerSendTimeout;



                var nets = client.GetStream();

                var ms = new MemoryStream();
                ms.SetLength( ClientSite.MAX_STREAM_BYTE_LEN );

                socketRead(nets, ms.GetBuffer(), 0, sizeof(short));

                var fsz = ms.ReadBEShort();
                if (fsz>ms.Length) throw new ProtocolException(StringConsts.GLUE_BAD_PROTOCOL_FRAME_ERROR +
                                                                "ClientSite sz={0} bytes over limit{1}".Args(fsz, ms.Length), closeChannel: true);

                socketRead(nets, ms.GetBuffer(), sizeof(short), fsz);
                ms.Position = sizeof(short);
                var cs = new ClientSite(ms);

                //this will register the site with transport and start the async receive
                var ss = new MpxWinServerSocket(this, client, cs, this.m_SocketReceiveAction);

              }
              catch(Exception error)
              {
                if (client!=null)
                 try{ client.Close(); } catch {}

                Transport.Binding.WriteLog(
                                LogSrc.Server,
                                Log.MessageType.Error,
                                StringConsts.GLUE_CLIENT_THREAD_ERROR + error.ToMessageWithType(),
                                from: "MpxWinListener.clientThreadSpin",
                                exception: error);
              }
           }


            private static void socketRead(NetworkStream nets, byte[] buffer, int offset, int total)
            {
              int cnt = 0;
              while(cnt<total)
              {
                var got = nets.Read(buffer, offset + cnt, total - cnt);
                if (got<=0) throw new SocketException();
                cnt += got;
              }
            }


    }



}
