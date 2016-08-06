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
using NFX.Environment;
using System.Threading;

using NFX.Time;
using NFX.ApplicationModel;
using NFX.Instrumentation;
using NFX.Glue.Protocol;


namespace NFX.Glue
{
    /// <summary>
    /// Represents a contract for Glue - a technology that provides asynchronous distributed component interconnection
    /// </summary>
    public interface IGlue : IApplicationComponent, ILocalizedTimeProvider
    {

        /// <summary>
        /// Retrieves a binding for node and throws if such binding is not known
        /// </summary>
        Binding GetNodeBinding(Node node);

        /// <summary>
        /// Retrieves a binding for node and throws if such binding is not known
        /// </summary>
        Binding GetNodeBinding(string node);

        /// <summary>
        /// Performs provider lookup by name
        /// </summary>
        IRegistry<Provider> Providers { get; }

        /// <summary>
        /// Performs binding lookup by name
        /// </summary>
        IRegistry<Binding> Bindings { get; }

        /// <summary>
        /// Performs ServerEndPoint lookup by name
        /// </summary>
        IRegistry<ServerEndPoint> Servers { get; }


        /// <summary>
        /// Registry of inspectors that deal with client-side messages
        /// </summary>
        OrderedRegistry<IClientMsgInspector> ClientMsgInspectors { get; }

        /// <summary>
        /// Registry of inspectors that deal with server-side messages
        /// </summary>
        OrderedRegistry<IServerMsgInspector> ServerMsgInspectors { get; }



        /// <summary>
        /// Specifies default ms timout for call dispatch only
        /// </summary>
        int DefaultDispatchTimeoutMs { get; set;}

        /// <summary>
        /// Specified default ms timeout for the calls
        /// </summary>
        int DefaultTimeoutMs { get; set;}


        /// <summary>
        /// Determines how much information should be logged about client-side operations
        /// </summary>
        NFX.Log.MessageType ClientLogLevel { get;  set;}

        /// <summary>
        /// Determines how much information should be logged about server-side operations
        /// </summary>
        NFX.Log.MessageType ServerLogLevel { get; set; }

        /// <summary>
        /// Specifies ms timout for non-threadsafe server instance lock
        /// </summary>
        int ServerInstanceLockTimeoutMs { get; set;}

    }

    public interface IGlueImplementation : IGlue, IDisposable, IConfigurable, IInstrumentable
    {
        void RegisterProvider(Provider p);
        void UnregisterProvider(Provider p);

        void RegisterBinding(Binding b);
        void UnregisterBinding(Binding b);

        void RegisterServerEndpoint(ServerEndPoint ep);
        void UnregisterServerEndpoint(ServerEndPoint ep);


        RequestMsg ClientDispatchingRequest(ClientEndPoint client, RequestMsg request);
        void ClientDispatchedRequest(ClientEndPoint client, RequestMsg request, CallSlot callSlot);

        void ClientDeliverAsyncResponse(ResponseMsg response);

        /// <summary>
        /// Asynchronously dispatch client request
        /// </summary>
        void ServerDispatchRequest(RequestMsg request);

        /// <summary>
        /// Handle client request synchronously
        /// </summary>
        ResponseMsg ServerHandleRequest(RequestMsg request);

        /// <summary>
        /// Handle failure of client request synchronously
        /// </summary>
        ResponseMsg ServerHandleRequestFailure(FID reqID, bool oneWay, Exception failure, object bindingSpecCtx);


        IConfigSectionNode GlueConfiguration { get; }
        IConfigSectionNode ProvidersConfigurationSection { get; }
        IEnumerable<IConfigSectionNode> ProviderConfigurations { get; }
        IConfigSectionNode BindingsConfigurationSection { get; }
        IEnumerable<IConfigSectionNode> BindingConfigurations { get; }
        IConfigSectionNode ServersConfigurationSection { get; }
        IEnumerable<IConfigSectionNode> ServerConfigurations { get; }
    }
}
