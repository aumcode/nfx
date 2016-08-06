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

using NFX.Glue.Protocol;
using NFX.Serialization;
using NFX.Serialization.Slim;
using NFX.ServiceModel;


namespace NFX.Glue.Native
{
    /// <summary>
    /// Provides server-side functionality for synchronous communication pattern based on TCP blocking sockets and Slim serializer
    /// for maximum serialization performance and lowest latency.
    /// The SyncBinding is usable for interconnection between NFX-native components on LANs (i.e. server clusters) in scenarios when
    ///  low latency is more important than total call invocation throughput.
    /// SyncServerTransport uses dedicated thread for request processing and is not scalable beyond a few hundred connections by design,
    ///  however it provides low latency benefit
    /// </summary>
    public class SyncServerTransport : ServerTransport<SyncBinding>
    {
        #region CONSTS

            public const string THREAD_NAME = "SyncServerTransport.Listener";

         #endregion


        #region .ctor

           /// <summary>
           /// Allocates a listener transport
           /// </summary>
           public SyncServerTransport(SyncBinding binding, ServerEndPoint serverEndpoint, IPAddress localAddr, int port) : base(binding, serverEndpoint)
           {
             Node = serverEndpoint.Node;
             m_IPAddress = localAddr;
             m_Port = port;
           }

        #endregion

        #region Fields/Props

            private byte[] m_SizeBuf = new byte[4];

            private TcpListener m_TcpListener;
            private Thread m_Thread;

            private IPAddress m_IPAddress;
            private int m_Port;

            private int m_OpenChannels;

        #endregion


        #region Protected

            protected override bool DoSendResponse(ResponseMsg response)
            {
              throw new InvalidGlueOperationException(StringConsts.OPERATION_NOT_SUPPORTED_ERROR + GetType().FullName+".SendResponse()");
            }



            protected override void DoStart()
            {
                base.DoStart();

                if (IsListener)
                {
                  m_TcpListener = new TcpListener(m_IPAddress, m_Port);
                  m_TcpListener.Start();
                  m_Thread = new Thread(listenerThreadSpin);
                  m_Thread.Name = THREAD_NAME;
                  m_Thread.Start();
                }
            }

            protected override void DoSignalStop()
            {
                base.DoSignalStop();
            }

            protected override void DoWaitForCompleteStop()
            {
                base.DoWaitForCompleteStop();

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

            protected override void DoDumpInstrumentationData()
            {
              base.DoDumpInstrumentationData();

              Instrumentation.ServerTransportChannelCount.Record(Node, m_OpenChannels);
            }

        #endregion


        #region .pvt

           private void listenerThreadSpin()
           {
               try
               {
                    while (Running)
                    {
                       var client = m_TcpListener.AcceptTcpClient();  //the blocking call is broken by close - as expected

                       Task.Factory.StartNew(clientThreadSpin, client, TaskCreationOptions.LongRunning);
                    }
               }
               catch(Exception error)
               {
                   if (this.Status != ControlStatus.Stopping)//then this is a bad error
                   {
                     Binding.WriteLog(LogSrc.Server,
                                      Log.MessageType.CatastrophicError,
                                      StringConsts.GLUE_LISTENER_EXCEPTION_ERROR + error.Message,
                                      from: "SyncServerTransport.listenerThreadSpin",
                                      exception: error);

                     Instrumentation.ServerListenerErrorEvent.Happened(Node);
                   }
               }
           }

           //20140206 DKh
           private void clientThreadSpin(object c)//executes in its own thread
           {
              var client = (TcpClient)c;
              var remoteIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();//DO NOT NEED POrt as it creates 1000s instrumentation messages

              if (Binding.InstrumentServerTransportStat)
                Instrumentation.ClientConnectedEvent.Happened(remoteIP);

              try
              {
                Interlocked.Increment(ref m_OpenChannels);
                try
                {
                  var serializer = new SlimSerializer(SyncBinding.KNOWN_SERIALIZER_TYPES);
                  serializer.TypeMode = TypeRegistryMode.Batch;
                  clientThreadSpinBody(client, serializer);
                }
                finally
                {
                  Interlocked.Decrement(ref m_OpenChannels);
                }
              }
              catch(Exception error)
              {
                 Binding.WriteLog(
                                LogSrc.Server,
                                Log.MessageType.Error,
                                StringConsts.GLUE_CLIENT_THREAD_ERROR + error.ToMessageWithType(),
                                from: "SyncServerTransport.clientThreadSpin",
                                exception: error);
              }

              if (Binding.InstrumentServerTransportStat)
                Instrumentation.ClientDisconnectedEvent.Happened(remoteIP);
           }

           private void clientThreadSpinBody(TcpClient client, SlimSerializer serializer)//executes in its own thread
           {
            const int POLL_WAIT_MS = 5000;
            const int POLL_WAIT_MCRS = POLL_WAIT_MS * 1000;//microsecond

              using(var ms = new MemoryStream(Consts.DEFAULT_SERIALIZER_STREAM_CAPACITY))
                 using(client)
                 {
                   ms.SetLength(Consts.DEFAULT_SERIALIZER_STREAM_CAPACITY);

                   client.LingerState.Enabled = false;
                   client.NoDelay = true;

                   client.ReceiveBufferSize =  Binding.ServerReceiveBufferSize;
                   client.SendBufferSize    =  Binding.ServerSendBufferSize;

                   client.ReceiveTimeout    =  Binding.ServerReceiveTimeout;
                   client.SendTimeout       =  Binding.ServerSendTimeout;

                   //20140206 DKh
                   byte[] check = new byte[1];
                   var remote = client.Client.RemoteEndPoint.ToString();
                   int idleSpins = 0;

                   while (Running)
                   {
                      RequestMsg request = null;

                      try
                      {
                        //20140206 DKh -------------
                        if (!client.Client.Poll(POLL_WAIT_MCRS, SelectMode.SelectRead))//microseconds
                        {
                          //break loop upon server listener idle timeout
                          var st = Binding.ServerTransportIdleTimeoutMs;
                          if (st>0)
                          {
                            idleSpins++;
                            if (idleSpins*POLL_WAIT_MS > st)
                            {
                              //timeout happened
                              Instrumentation.InactiveServerTransportClosedEvent.Happened(Node);
                              Binding.WriteLog( LogSrc.Server,
                                       Log.MessageType.DebugZ,
                                      "TCPClient timed out: " + remote,
                                      from: "TIMEOUT_CHECK");
                              break;
                            }
                          }
                          continue;
                        }
                        else
                        {
                          if (client.Client.Receive(check, SocketFlags.Peek)<=0)
                          {
                            Binding.WriteLog( LogSrc.Server,
                                         Log.MessageType.DebugZ,
                                        "TCPClient disconnected: " + remote,
                                        from: "RECEIVED<=0");
                            break;//disconnected
                          }

                          idleSpins = 0;//something arrived
                        }
                        //--------------------------

                        request = getRequest(client, ms, serializer);

                        request.__setServerTransport(this);
                        ResponseMsg response = Glue.ServerHandleRequest(request); //This does actual server work

                        if (!request.OneWay && response!=null)
                         putResponse(client, ms, response, serializer);
                      }
                      catch(Exception error)
                      {
                        var commError = error is SocketException ||
                                        error is System.IO.IOException ||
                                        (error is ProtocolException && ((ProtocolException)error).CloseChannel)||
                                        (error is SlimDeserializationException) ||
                                        (error is SlimSerializationException && serializer.BatchTypesAdded);

                        Binding.WriteLog( LogSrc.Server,
                                       Log.MessageType.Warning,
                                      (commError ? StringConsts.GLUE_CLIENT_THREAD_COMMUNICATION_ERROR :
                                                   StringConsts.GLUE_CLIENT_THREAD_ERROR ) + error.Message,
                                      from: "SyncServerTransport.clientThreadSpinBody",
                                      exception: error);

                        stat_Errors();

                        if (commError) break;//close the channel

                        if (request != null)
                          try
                          {
                            var response = Glue.ServerHandleRequestFailure(request.RequestID, request.OneWay, error, request.BindingSpecificContext);
                            if (!request.OneWay && response!=null)
                             putResponse(client, ms, response, serializer);
                          }
                          catch (Exception e)
                          {
                            Binding.WriteLog(
                                LogSrc.Server,
                                Log.MessageType.Warning,
                                StringConsts.GLUE_CLIENT_THREAD_ERROR + "couldn't deliver response to client's request: " + e.Message,
                                relatedTo: request.RequestID.ToGuid(),
                                from: "SyncServerTransport.clientThreadSpinBody",
                                exception: e);
                          }
                      }
                   }
                 }//using
           }


           private RequestMsg getRequest(TcpClient client, MemoryStream ms, SlimSerializer serializer)
           {
             var nets = client.GetStream();


             var msb = ms.GetBuffer();
             var frameBegin = Consts.PACKET_DELIMITER_LENGTH;
             SyncBinding.socketRead(nets, msb, 0, frameBegin);

             var size = msb.ReadBEInt32();

             if (size<1 || size>Binding.MaxMsgSize)
              {
                Instrumentation.ServerGotOverMaxMsgSizeErrorEvent.Happened(Node);
                // This is unrecoverable error - close the channel!
                throw new MessageSizeException(size, Binding.MaxMsgSize, "getRequest()", closeChannel: true);
              }


             ms.SetLength(Consts.PACKET_DELIMITER_LENGTH+size);  //this may invalidate msb
             SyncBinding.socketRead(nets, ms.GetBuffer(), Consts.PACKET_DELIMITER_LENGTH, size);

             var arrivalTime = Binding.StatTimeTicks;

             ms.Position = Consts.PACKET_DELIMITER_LENGTH;
             RequestMsg result = null;


              WireFrame frame;
              object received = null;
              try
              {
                  try
                  {
                      frame = new WireFrame(ms);
                      //todo Skip frames that are not GLUE type
                      received = serializer.Deserialize(ms);
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

              }
              finally
              {
                  Binding.DumpMsg(true, received as Msg, ms.GetBuffer(), 0, size+Consts.PACKET_DELIMITER_LENGTH);
              }

              result.__SetArrivalTimeStampTicks(arrivalTime);


             stat_MsgReceived();
             stat_BytesReceived(size);
             return result;
           }

           private void putResponse(TcpClient client, MemoryStream ms, ResponseMsg response, SlimSerializer serializer)
           {
             var frameBegin = Consts.PACKET_DELIMITER_LENGTH;
             ms.Position = frameBegin;

             var frame = new WireFrame(WireFrame.SLIM_FORMAT, true, response.RequestID);

             // Write the frame
             var frameSize = frame.Serialize(ms);
             // Write the message
             serializer.Serialize(ms, response);

             var size = (int)ms.Position - frameBegin;

             var buffer = ms.GetBuffer();//no stream expansion beyond this point
             buffer.WriteBEInt32(0, size);

             Binding.DumpMsg(true, response, buffer, 0, (int)ms.Position);

             if (size>Binding.MaxMsgSize)
             {
                 Instrumentation.ServerSerializedOverMaxMsgSizeErrorEvent.Happened(Node);
                 throw new MessageSizeException(size, Binding.MaxMsgSize, "putResponse()", serializer.BatchTypesAdded);
             }

             client.GetStream().Write(buffer, 0, (int)ms.Position);

             stat_MsgSent();
             stat_BytesSent(size);
           }


       #endregion
    }
}
