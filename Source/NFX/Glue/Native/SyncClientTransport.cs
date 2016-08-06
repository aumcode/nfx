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
using System.Net.Sockets;

using NFX.Glue.Protocol;
using NFX.Serialization;
using NFX.Serialization.Slim;

namespace NFX.Glue.Native
{
    /// <summary>
    /// Provides client-side functionality for synchronous communication pattern based on TCP blocking sockets and Slim serializer
    /// for maximum serialization performance and lowest latency.
    /// The SyncBinding is usable for interconnection between NFX-native components on LANs (i.e. server clusters) in scenarios when
    ///  low latency is more important than total call invocation throughput
    /// </summary>
    public class SyncClientTransport : ClientTransport<SyncBinding>
    {
        #region CONSTS


        #endregion


        #region .ctor

           public SyncClientTransport(SyncBinding binding, Node node) : base(binding)
           {
               Node = node;
           }


        #endregion


        #region Fields/Props

           private MemoryStream m_MemStream;
           private byte[] m_SizeBuf = new byte[4];

           private TcpClient m_Client;

           private int m_PriorDispatchTimeoutMs;
           private int m_PriorTimeoutMs;

           private SlimSerializer m_Serializer;

        #endregion


        #region Public

                protected override CallSlot DoSendRequest(ClientEndPoint endpoint, RequestMsg request, CallOptions options)
                {
                  try
                  {
                    ensureClient();
                    return doRequest(endpoint, request, options);
                  }
                  catch(Exception error)
                  {
                    var commError = error is SocketException ||
                                    error is System.IO.IOException ||
                                    (typeof(ProtocolException).IsAssignableFrom(error.GetType()) && ((ProtocolException)error).CloseChannel)||
                                    (error is SlimSerializationException && m_Serializer.BatchTypesAdded) ||
                                    (error is SlimDeserializationException);

                    Binding.WriteLog( LogSrc.Client,
                                      Log.MessageType.Error,
                                      StringConsts.GLUE_CLIENT_CALL_ERROR + (commError ? "Socket error." : string.Empty)  + error.Message,
                                      from: "SyncClientTransport.SendRequest",
                                      exception: error);

                    stat_Errors();
                    if (commError) finClient();
                    throw error;
                  }
                }



            protected override void DoStart()
            {
                base.DoStart();
                m_MemStream = new MemoryStream(Consts.DEFAULT_SERIALIZER_STREAM_CAPACITY);
                m_MemStream.SetLength(Consts.DEFAULT_SERIALIZER_STREAM_CAPACITY);
                m_Serializer = new SlimSerializer(SyncBinding.KNOWN_SERIALIZER_TYPES);
                m_Serializer.TypeMode = TypeRegistryMode.Batch;
                ensureClient();
            }

            protected override void DoSignalStop()
            {
                base.DoSignalStop();
            }

            protected override void DoWaitForCompleteStop()
            {
                base.DoWaitForCompleteStop();
                finClient();
                m_MemStream.Dispose();
                m_MemStream = null;
            }

        #endregion


        #region .pvt

           private void ensureClient()
           {
             if (m_Client!=null) return;

             var ep = SyncBinding.ToIPEndPoint(Node);

             m_Client = new TcpClient();
             m_Client.Connect(ep);

             m_Client.NoDelay = true;
             m_Client.LingerState.Enabled = false;

             m_Client.ReceiveBufferSize =  Binding.ClientReceiveBufferSize;
             m_Client.SendBufferSize    =  Binding.ClientSendBufferSize;

             m_Client.ReceiveTimeout    =  Binding.ClientReceiveTimeout;
             m_Client.SendTimeout       =  Binding.ClientSendTimeout;

             m_Serializer.ResetCallBatch();
           }

           private void finClient()
           {
             if (m_Client==null) return;
             m_Client.Close();
             m_Client = null;
           }


        private CallSlot doRequest(ClientEndPoint endpoint, RequestMsg request, CallOptions options)
        {
          long actualStartTimeTicks = Binding.StatTimeTicks;
          DateTime actualStartTimeUtc = DateTime.UtcNow;

          if (m_PriorDispatchTimeoutMs!=options.DispatchTimeoutMs)
          {
            m_Client.SendTimeout = options.DispatchTimeoutMs;
            m_PriorDispatchTimeoutMs = options.DispatchTimeoutMs;
          }

          if (m_PriorTimeoutMs!=options.TimeoutMs)
          {
            m_Client.ReceiveTimeout = options.TimeoutMs;
            m_PriorTimeoutMs = options.TimeoutMs;
          }

          putRequest(request);
          if (request.OneWay) return new CallSlot(endpoint, this, request, CallStatus.Dispatched, options.TimeoutMs);

          var response = getResponse();

          return  new CallSlot(endpoint,
                                    this,
                                    actualStartTimeTicks,
                                    actualStartTimeUtc,
                                    request,
                                    response,
                                    options.TimeoutMs);
       }


            private void putRequest(RequestMsg request)
            {
               var ms = m_MemStream;
               var dataBegin = Consts.PACKET_DELIMITER_LENGTH;

               ms.Position = dataBegin;

               var frame = new WireFrame(WireFrame.SLIM_FORMAT, request.OneWay, request.RequestID);

               // Write the frame
               var frameSize = frame.Serialize(ms);
               // Write the message
               m_Serializer.Serialize(ms, request);

               var size = (int)ms.Position - dataBegin;

               var buffer = ms.GetBuffer();//no stream expansion beyond this point
               buffer.WriteBEInt32(0, size);

               Binding.DumpMsg(false, request, buffer, 0, (int)ms.Position);

               if (size>Binding.MaxMsgSize)
               {
                  Instrumentation.ClientSerializedOverMaxMsgSizeErrorEvent.Happened(Node);
                  //do not tear the socket, however we may have added extra types to Serializer typereg that server never received
                  //so in thatcase we do close the channel
                  throw new MessageSizeException(size, Binding.MaxMsgSize, "putRequest("+request.RequestID+")", m_Serializer.BatchTypesAdded);
               }

               m_Client.GetStream().Write(buffer, 0, (int)ms.Position);

               stat_MsgSent();
               stat_BytesSent(size);
            }

            private ResponseMsg getResponse()
            {
              var ms = m_MemStream;

              var nets = m_Client.GetStream();

              var msb = ms.GetBuffer();
              var frameBegin = Consts.PACKET_DELIMITER_LENGTH;
              SyncBinding.socketRead(nets, msb, 0, frameBegin);


              var size = msb.ReadBEInt32();
              if (size<1 || size>Binding.MaxMsgSize)
              {
                Instrumentation.ClientGotOverMaxMsgSizeErrorEvent.Happened(Node);
                // There is no recovery here - close the channel!
                throw new MessageSizeException(size, Binding.MaxMsgSize, "getResponse()", closeChannel: true);
              }

              ms.SetLength(frameBegin+size);  //this may invalidate msb
              SyncBinding.socketRead(nets, ms.GetBuffer(), frameBegin, size);

              var arrivalTime = Binding.StatTimeTicks;

              ms.Position = Consts.PACKET_DELIMITER_LENGTH;

              ResponseMsg result = null;
              WireFrame frame;

              object received = null;
              try
              {
                      try
                      {
                        frame = new WireFrame(ms);
                        received = m_Serializer.Deserialize(ms);
                      }
                      catch
                      {
                        Instrumentation.ClientDeserializationErrorEvent.Happened(Node);
                        throw;
                      }

                      if (received==null)
                        throw new ProtocolException(StringConsts.GLUE_UNEXPECTED_MSG_ERROR + "ResponseMsg. Got <null>");

                      result = received as ResponseMsg;

                      if (result==null)
                        throw new ProtocolException(StringConsts.GLUE_UNEXPECTED_MSG_ERROR + "ResponseMsg");

              }
              finally
              {
                 Binding.DumpMsg(false, received as Msg, ms.GetBuffer(), 0, size+Consts.PACKET_DELIMITER_LENGTH);
              }


              result.__SetArrivalTimeStampTicks(arrivalTime);

              stat_MsgReceived();
              stat_BytesReceived(size);
              return result;
            }


        #endregion

    }
}
