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
using NFX;
using NFX.Glue;
using NFX.Glue.Protocol;

namespace BusinessLogic
{
  public class TextInfoReporter : IClientMsgInspector
  {
    public RequestMsg ClientDispatchCall(ClientEndPoint endpoint, RequestMsg request)
    {
      request.Headers.Add(new TextInfoHeader { Text = "Moscow time is " + App.LocalizedTime.ToString(), Info = @"/\EH|/|H  }|{|/|B!" });
      return request;
    }

    public ResponseMsg ClientDeliverResponse(CallSlot callSlot, ResponseMsg response)
    {
      if (response.ReturnValue is string)
        return new ResponseMsg(response, (string)(response.ReturnValue) + " Added by Client Inspector");
      else
        return response;
    }

    public string Name { get { return "Marazm"; } }

    public int Order { get { return 0; } }

    public void Configure(NFX.Environment.IConfigSectionNode node) { }
  }

  public class ServerInspector : IServerMsgInspector
  {
    public RequestMsg ServerDispatchRequest(ServerEndPoint endpoint, RequestMsg request)
    {
      NFX.ApplicationModel.ExecutionContext.Application.Log.Write(new NFX.Log.Message { Type = NFX.Log.MessageType.TraceA, From = "ServeInspector", Text = "Received " + request.ServerTransport.StatBytesReceived.ToString() + " bytes" });
      return request;
    }

    public ResponseMsg ServerReturnResponse(ServerEndPoint endpoint, RequestMsg request, ResponseMsg response)
    {
      response.Headers.Add(new TextInfoHeader { Text = "Response generated at " + App.LocalizedTime.ToString(), Info = "Serve Node: " + endpoint.Node });
      return response;
    }

    public string Name { get { return "MarazmOnServer"; } }

    public int Order { get { return 0; } }

    public void Configure(NFX.Environment.IConfigSectionNode node) { }
  }
}
