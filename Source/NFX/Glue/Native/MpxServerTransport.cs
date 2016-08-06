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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using NFX;
using NFX.Glue;
using NFX.Glue.Protocol;

namespace NFX.Glue.Native
{
  /// <summary>
  /// Implements asynchronous MPX server transport for MpxBinding
  /// </summary>
  public sealed class MpxServerTransport : ServerTransport<MpxBinding>, IMpxTransport
  {
     #region CONSTS

        private const int GENERATIONS = 3;

     #endregion


     #region .ctor

        /// <summary>
        /// Allocates a listener transport
        /// </summary>
        public MpxServerTransport(MpxBinding binding, ServerEndPoint serverEndpoint, IPAddress localAddr, int port) : base(binding, serverEndpoint)
        {
          Node = serverEndpoint.Node;
          m_IPAddress = localAddr;
          m_Port = port;
        }



    #endregion

    #region Inner Classes

                     private class clientSiteState : INamed
                     {
                        public clientSiteState(MpxServerTransport transport, ClientSite site)
                        {
                          this.Transport = transport;
                          this.Site = site;
                          m_Sockets = new List<MpxServerSocket>();
                          OutQueues = new ConcurrentQueue<ResponseMsg>[GENERATIONS];
                          for(var i=0; i<GENERATIONS; i++) OutQueues[i] = new ConcurrentQueue<ResponseMsg>();
                        }

                        public MpxServerTransport Transport;
                        public readonly ClientSite Site;

                        public int LOCK_TOKEN;
                        private List<MpxServerSocket> m_Sockets;

                        public readonly ConcurrentQueue<ResponseMsg>[] OutQueues;

                        public string Name { get{return Site.Name;}}

                        public void AddSocket(MpxServerSocket socket)
                        {
                          if (Transport.Binding.InstrumentServerTransportStat)
                            Instrumentation.ClientConnectedEvent.Happened(socket.ClientSite.Name);

                          Interlocked.Increment(ref Transport.m_OpenChannels);
                          lock(m_Sockets) m_Sockets.Add(socket);
                        }

                        public bool RemoveSocket(MpxServerSocket socket)
                        {
                          if (Transport.Binding.InstrumentServerTransportStat)
                            Instrumentation.ClientDisconnectedEvent.Happened(socket.ClientSite.Name);

                          Interlocked.Decrement(ref Transport.m_OpenChannels);
                          lock(m_Sockets) return m_Sockets.Remove(socket);
                        }

                        public MpxServerSocket TryFindAnyConnectedSocketOrNull()
                        {
                          lock(m_Sockets) return m_Sockets.FirstOrDefault(s => s.Active);
                        }

                        public void AcceptManageVisit(DateTime now)
                        {
                          List<MpxServerSocket> closing = null;

                          lock(m_Sockets)
                          {
                           foreach(var socket in m_Sockets)
                           {
                             socket.AcceptManagerVisit(now);
                             if (!socket.Active && !socket.Disposed)
                             {
                                if (closing==null) closing = new List<MpxServerSocket>();
                                closing.Add(socket);
                             }
                           }
                          }
                          //---after lock so Dispose() does not interlock
                          //closing contains a list of sockets marked to be disposed, but not physically disposed
                          if (closing!=null)
                           foreach(var socket in closing) socket.Dispose();
                        }
                     }
    #endregion

    #region Fields/Props


        private MpxListener m_Listener;

        private IPAddress m_IPAddress;
        private int m_Port;


        internal int m_OpenChannels;

        private Registry<clientSiteState> m_ClientSites;

    #endregion

    #region Protected

        /// <summary>
        /// Notifies the transport that site connection has arrived so the transport may allocate some site/socket-specific state.
        /// Called from listener
        /// </summary>
        internal void ClientSiteConnected(MpxServerSocket socket)
        {
          var sites = m_ClientSites;
          if (sites==null) return;

          var existing = sites.GetOrRegister(socket.ClientSite.Name,  (_) => new clientSiteState(this, socket.ClientSite), true);

          existing.AddSocket(socket);
        }

        /// <summary>
        /// Notifies the transport that site connection has closed/broke so the transport may de-allocate some site/socket-specific state.
        /// Called from listener
        /// </summary>
        internal void ClientSiteDisconnected(MpxServerSocket socket)
        {
          var sites = m_ClientSites;
          if (sites==null) return;

          var existing = sites[socket.ClientSite.Name];

          if (existing==null) return;

          existing.RemoveSocket(socket);
        }


        //this may be invoked by many threads at the same time
        protected override bool DoSendResponse(ResponseMsg response)
        {
           if (!Running) return false;

           var socket = response.BindingSpecificContext as MpxServerSocket;
           if (socket==null) return false;

           var siteName = socket.ClientSite.Name;

           var state = m_ClientSites[siteName];
           if (state==null) return false;//this can not happen because since socket came in request then site is registered

           state.OutQueues[0].Enqueue( response );
           sendOutQueue(state, false);
           return true;
        }

        protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
        {
          if (!Running) return;


          foreach(var state in m_ClientSites)
          {

            //flush remaining out queue if there is at least one
            ResponseMsg msg;
            if (state.OutQueues.Any(q => q.TryPeek(out msg)))
               Task.Factory.StartNew(() => sendOutQueue(state, true) );

            //Manage states - i.e. close inactive server sockets
            try
            {
              state.AcceptManageVisit(managerNow);//this should not block for long
            }
            catch(Exception error)
            {
              Binding.WriteLog(LogSrc.Server, Log.MessageType.Error,
                               "state.AcceptManageVisit(managerNow) leaked: "+error.ToMessageWithType(),
                               GetType().Name+".DoAcceptManagerVisit", error);
            }
          }
        }




        protected override void DoStart()
        {
           base.DoStart();

           m_ClientSites = new Registry<clientSiteState>();

           m_Listener = Binding.SocketFactory.MakeListener(this, new IPEndPoint(m_IPAddress, m_Port), socketReceiveAction);
        }


        protected override void DoWaitForCompleteStop()
        {
          if (m_Listener!=null)
          {
            m_Listener.Dispose();
            m_Listener = null;
          }
          m_ClientSites = null;
        }

        protected override void DoDumpInstrumentationData()
        {
          base.DoDumpInstrumentationData();

          Instrumentation.ServerTransportChannelCount.Record(Node, m_OpenChannels);
        }

    #endregion


    #region .pvt

       //called asynchronously to deliver data from client socket
        private void socketReceiveAction(MpxSocket<MpxServerTransport> socket, WireMsg wm)
        {
          if (!Running) return;

          var request = deserialize(ref wm);

          request.__setServerTransport(this);
          request.__SetBindingSpecificContext(socket);

          Glue.ServerDispatchRequest(request);
        }


         private RequestMsg deserialize(ref WireMsg wmsg)
           {
             var chunk = wmsg.Data;
             chunk.Position = sizeof(int);

             WireFrame frame;
             RequestMsg result = null;
             var arrivalTime = Binding.StatTimeTicks;

             object received = null;
             try
             {
                try
                {
                  frame = new WireFrame(chunk);
                  wmsg.Frame = frame;
                  received = Binding.Serializer.Deserialize(chunk);
                }
                catch
                {
                  Instrumentation.ServerDeserializationErrorEvent.Happened(Node);
                  throw;
                }

                if (received==null)
                  throw new ProtocolException(StringConsts.GLUE_UNEXPECTED_MSG_ERROR + "RequestMsg. Got <null>");

                result = received as RequestMsg;

                if (result==null)
                  throw new ProtocolException(StringConsts.GLUE_UNEXPECTED_MSG_ERROR + "RequestMsg");

                stat_MsgReceived();
                stat_BytesReceived(chunk.Position);
             }
             catch
             {
                stat_Errors();
                throw;
             }
             finally
             {
                Binding.DumpMsg(true, received as Msg, chunk.GetBuffer(), 0, (int)chunk.Position );
             }


              result.__SetArrivalTimeStampTicks(arrivalTime);


              return result;
           }


        private void sendOutQueue(clientSiteState state, bool isManager)
        {
           try
           {
              sendOutQueueCore(state, isManager);
           }
           catch(Exception error)
           {
              Binding.WriteLog(LogSrc.Server, Log.MessageType.Error, "sendOutQueueCore leaked: " + error.ToMessageWithType(), "MpxServerTransport.sendOutQueue()", error);
           }
        }

        private void sendOutQueueCore(clientSiteState state, bool isManager)
        {
           const int MAX_SPINS = 50;
           int spins = 0;

           while (Interlocked.CompareExchange(ref state.LOCK_TOKEN, 1, 0)!=0)
           {
             if (!isManager) return;
             if (spins>MAX_SPINS) return;
             spins++;
             Thread.Sleep(10 + ExternalRandomGenerator.Instance.NextScaledRandomInteger(1, 10));
           }

           try
           { //may block on this thread until send succeeds
             for(var i=0; (!isManager && i==0) || (isManager && i<GENERATIONS); i++)
             {
                   var queue = state.OutQueues[i];

                   ResponseMsg response;
                   while(Running && queue.TryDequeue(out response))
                   {
                     if (!sendResponse(response))
                     {
                        if (i<GENERATIONS-1)
                          state.OutQueues[i+1].Enqueue(response);//put back in queue what did not go in older gen queue
                     }
                   }//while
             }//for
           }
           finally
           {
              Interlocked.Exchange(ref state.LOCK_TOKEN, 0);
           }
        }


        private bool sendResponse(ResponseMsg response)
        {
          try
          {
            MpxServerSocket socket = getSocket(response);
            if (socket==null || !socket.Active) return false;

            try
            {
              sendSocketResponse(socket, response);
            }
            catch(Exception err1)
            {
              var commError = err1 is SocketException ||
                              err1 is System.IO.IOException ||
                              (err1 is ProtocolException && ((ProtocolException)err1).CloseChannel);
              stat_Errors();
              if (commError) throw;//do nothing - just close the channel

              if (response != null)
                  try
                  {
                    var response2 = Glue.ServerHandleRequestFailure(response.RequestID, false, err1, response.BindingSpecificContext);
                    sendSocketResponse(socket, response2);
                  }
                  catch (Exception e)
                  {
                    Binding.WriteLog(
                        LogSrc.Server,
                        Log.MessageType.Warning,
                        StringConsts.GLUE_CLIENT_THREAD_ERROR + "couldn't deliver response to client's request: " + e.Message,
                        relatedTo: response.RequestID.ToGuid(),
                        from: "MpxServerTransport.sendResponse",
                        exception: e);
                  }
            }
            return true;
          }
          catch(Exception error)
          {
            Binding.WriteLog(LogSrc.Server, Log.MessageType.Error, error.ToMessageWithType(), "MpxServerTransport.sendResponse()", error);
            if (error is MessageSizeException) return true;
            return false;
          }
        }

        private MpxServerSocket getSocket(ResponseMsg response)
        {
          var socket = response.BindingSpecificContext as MpxServerSocket;
          if (socket==null) return null;
          if (socket.Active) return socket;

          var sites = m_ClientSites;
          if (sites==null) return null;
          var cs = socket.ClientSite;
          var site = m_ClientSites[cs.Name];
          if (site==null) return null;

          socket = site.TryFindAnyConnectedSocketOrNull();
          return socket;
        }


        private void sendSocketResponse(MpxServerSocket socket, ResponseMsg response)
        {
           var chunk = socket.GetSendChunk();
            try
            {
              var frame = new WireFrame(WireFrame.SLIM_FORMAT, true, response.RequestID);
              var size = serialize(chunk, frame, response);
              var wm = new WireMsg(chunk);


              Binding.DumpMsg(true, response, chunk.GetBuffer(), 0, (int)chunk.Position  );

              if (size>Binding.MaxMsgSize)
              {
                Instrumentation.ServerSerializedOverMaxMsgSizeErrorEvent.Happened(Node);
                throw new MessageSizeException(size, Binding.MaxMsgSize, "sendResponse("+response.RequestID+")");
              }

              socket.Send(wm);

              stat_MsgSent();
              stat_BytesSent(size);
            }
            catch
            {
              stat_Errors();
              throw;
            }
            finally
            {
              socket.ReleaseSendChunk();
            }
        }

        private int serialize(MemChunk chunk, WireFrame frame, Msg msg)
        {
          chunk.Position = sizeof(int);
          frame.Serialize( chunk );
          Binding.Serializer.Serialize(chunk, msg);
          var size = (int)chunk.Position;//with 4 byte len

          var buff = chunk.GetBuffer();//no stream expansion beyond this point
          buff.WriteBEInt32(0, size);
          return size;
        }



    #endregion

  }
}
