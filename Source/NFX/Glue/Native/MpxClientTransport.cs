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
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using NFX;
using NFX.Glue;
using NFX.Glue.Protocol;


namespace NFX.Glue.Native
{
  /// <summary>
  /// Implements asynchronous MPX client transport for MpxBinding
  /// </summary>
  public sealed class MpxClientTransport : ClientTransport<MpxBinding>, IMpxTransport
  {
      #region .ctor

           public MpxClientTransport(MpxBinding binding, Node node) : base(binding)
           {
               Node = node;
           }
      #endregion

      #region Fields/Props


           private MpxClientSocket m_Client;
           private int m_PriorDispatchTimeoutMs;
           private int m_PriorTimeoutMs;

      #endregion

      #region Protected
        protected override CallSlot DoSendRequest(ClientEndPoint endpoint, RequestMsg request, CallOptions options)
        {
          try
          {
             ensureClient();
             return sendRequest(endpoint, request, options);
          }
          catch(Exception error)
          {
            var commError = error is SocketException ||
                            error is System.IO.IOException ||
                            (typeof(ProtocolException).IsAssignableFrom(error.GetType()) && ((ProtocolException)error).CloseChannel);

            Binding.WriteLog( LogSrc.Client,
                              Log.MessageType.Error,
                              StringConsts.GLUE_CLIENT_CALL_ERROR + (commError ? "Socket error." : string.Empty)  + error.Message,
                              from: "MpxClientTransport.SendRequest",
                              exception: error);

            stat_Errors();
            if (commError) finClient();
            throw error;
          }
        }

        protected override void DoStart()
        {
          base.DoStart();
          ensureClient();
        }

        protected override void DoWaitForCompleteStop()
        {
          base.DoWaitForCompleteStop();
          finClient();
        }

      #endregion


      #region .pvt
           private void ensureClient()
           {
             if (m_Client!=null && m_Client.Active) return;

             finClient();

             var ep = MpxBinding.ToIPEndPoint(Node);

             m_Client = Binding.SocketFactory.MakeClientSocket(this, ep, new ClientSite(""), receiveAction);
           }

           private void finClient()
           {
             if (m_Client==null) return;
             m_Client.Dispose();
             m_Client = null;
           }


           private CallSlot sendRequest(ClientEndPoint endpoint, RequestMsg request, CallOptions options)
           {
              if (m_PriorDispatchTimeoutMs!=options.DispatchTimeoutMs)
              {
                m_Client.Socket.SendTimeout = options.DispatchTimeoutMs;
                m_PriorDispatchTimeoutMs = options.DispatchTimeoutMs;
              }

              if (m_PriorTimeoutMs!=options.TimeoutMs)
              {
                m_Client.Socket.ReceiveTimeout = options.TimeoutMs;
                m_PriorTimeoutMs = options.TimeoutMs;
              }


              var chunk = m_Client.GetSendChunk();
              try
              {
                var frame = new WireFrame(WireFrame.SLIM_FORMAT, request.OneWay, request.RequestID);
                var size = serialize(chunk, frame, request);
                var wm = new WireMsg(chunk);


                Binding.DumpMsg(false, request, chunk.GetBuffer(), 0, (int)chunk.Position );

                if (size>Binding.MaxMsgSize)
                {
                  Instrumentation.ClientSerializedOverMaxMsgSizeErrorEvent.Happened(Node);
                  throw new MessageSizeException(size, Binding.MaxMsgSize, "sendRequest("+request.RequestID+")");
                }

                m_Client.Send(wm);

                stat_MsgSent();
                stat_BytesSent(wm.BufferUsedSize);
              }
              catch
              {
                stat_Errors();
                throw;
              }
              finally
              {
                m_Client.ReleaseSendChunk();
              }


              //regardless of (request.OneWay) we return callslot anyway
              return new CallSlot(endpoint, this, request, CallStatus.Dispatched, options.TimeoutMs);
           }


           //called asynchronously to deliver data from server
           private void receiveAction(MpxSocket<MpxClientTransport> socket, WireMsg wm)
           {
             if (!Running) return;
             var response = deserialize(ref wm);

             Glue.ClientDeliverAsyncResponse(response);
           }

           private int serialize(MemChunk chunk, WireFrame frame, Msg msg)
           {
             chunk.Position = sizeof(int);
             frame.Serialize( chunk );
             Binding.Serializer.Serialize(chunk, msg);
             var size = (int)chunk.Position;//includes 4 byte len prefix

             var buff = chunk.GetBuffer();//no stream expansion beyond this point
             buff.WriteBEInt32(0, size);
             return size;
           }

           private ResponseMsg deserialize(ref WireMsg wmsg)
           {
             var chunk = wmsg.Data;
             chunk.Position = sizeof(int);

             WireFrame frame;
             ResponseMsg result = null;
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
                  Instrumentation.ClientDeserializationErrorEvent.Happened(Node);
                  throw;
                }


                if (received==null)
                  throw new ProtocolException(StringConsts.GLUE_UNEXPECTED_MSG_ERROR + "ResponseMsg. Got <null>");

                result = received as ResponseMsg;

                if (result==null)
                  throw new ProtocolException(StringConsts.GLUE_UNEXPECTED_MSG_ERROR + "ResponseMsg");

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
              Binding.DumpMsg(false, received as Msg, chunk.GetBuffer(), 0, (int)chunk.Position );
             }

              result.__SetArrivalTimeStampTicks(arrivalTime);
              return result;
           }

      #endregion

  }
}
