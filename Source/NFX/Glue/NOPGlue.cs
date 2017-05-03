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

using NFX.ApplicationModel;
using NFX.Instrumentation;
using NFX.Glue.Implementation;
using NFX.Glue.Protocol;

namespace NFX.Glue
{
    public class NOPGlue : ApplicationComponent, IGlueImplementation
    {
        private static NOPGlue s_Instance = new NOPGlue();

        private NOPGlue():base() {}

        public static NOPGlue Instance { get { return s_Instance;} }


         public bool InstrumentationEnabled{ get{return false;} set{}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get{ return null;}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups){ return null;}
         public bool ExternalGetParameter(string name, out object value, params string[] groups)
         {
           value = null;
           return false;
         }
         public bool ExternalSetParameter(string name, object value, params string[] groups)
         {
           return false;
         }


        public void RegisterProvider(Provider p)
        {
        }

        public void UnregisterProvider(Provider p)
        {
        }

        public void RegisterBinding(Binding b)
        {
        }

        public void UnregisterBinding(Binding b)
        {
        }

        public void RegisterServerEndpoint(ServerEndPoint ep)
        {

        }

        public void UnregisterServerEndpoint(ServerEndPoint ep)
        {

        }

        public Binding GetNodeBinding(Node node)
        {
            return null;
        }

        public Binding GetNodeBinding(string node)
        {
            return null;
        }

        public IRegistry<Provider> Providers
        {
            get { return new Registry<Provider>(); }
        }

        public IRegistry<Binding> Bindings
        {
            get { return new Registry<Binding>(); }
        }

        public IRegistry<ServerEndPoint> Servers
        {
            get { return new Registry<ServerEndPoint>(); }
        }


        public OrderedRegistry<IClientMsgInspector> ClientMsgInspectors { get { return new OrderedRegistry<IClientMsgInspector>(); } }

        public OrderedRegistry<IServerMsgInspector> ServerMsgInspectors { get { return new OrderedRegistry<IServerMsgInspector>(); } }


        public int DefaultDispatchTimeoutMs
        {
            get { return GlueService.DEFAULT_DISPATCH_TIMEOUT_MS; }
            set {}
        }

        public int DefaultTimeoutMs
        {
            get { return GlueService.DEFAULT_TIMEOUT_MS; }
            set {}
        }

         public int ServerInstanceLockTimeoutMs
        {
            get { return GlueService.DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS; }
            set {}
        }

        public void Configure(Environment.IConfigSectionNode node)
        {

        }


        public RequestMsg ClientDispatchingRequest(ClientEndPoint client, RequestMsg request)
        {
          return request;
        }

        public void ClientDispatchedRequest(ClientEndPoint client, RequestMsg request, CallSlot callSlot)
        {

        }

        public void ClientDeliverAsyncResponse(ResponseMsg response)
        {

        }

        public void ServerDispatchRequest(RequestMsg request)
        {
        }

        public ResponseMsg ServerHandleRequest(RequestMsg request)
        {
            return null;
        }

        public ResponseMsg ServerHandleRequestFailure(FID reqID, bool oneWay, Exception failure, object bindingSpecCtx)
        {
            return null;
        }

        public ApplicationModel.IApplication Application
        {
            get { return ExecutionContext.Application; }
        }

        public Environment.IConfigSectionNode GlueConfiguration
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public Environment.IConfigSectionNode ProvidersConfigurationSection
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public IEnumerable<Environment.IConfigSectionNode> ProviderConfigurations
        {
            get { return Enumerable.Empty<Environment.IConfigSectionNode>(); }
        }

        public Environment.IConfigSectionNode BindingsConfigurationSection
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public IEnumerable<Environment.IConfigSectionNode> BindingConfigurations
        {
            get { return Enumerable.Empty<Environment.IConfigSectionNode>(); }
        }

        public Environment.IConfigSectionNode ServersConfigurationSection
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public IEnumerable<Environment.IConfigSectionNode> ServerConfigurations
        {
            get { return Enumerable.Empty<Environment.IConfigSectionNode>(); }
        }


        public Log.MessageType ClientLogLevel
        {
            get { return Log.MessageType.CatastrophicError; }
            set{}
        }

        public Log.MessageType ServerLogLevel
        {
            get { return Log.MessageType.CatastrophicError; }
            set{}
        }

        public Time.TimeLocation TimeLocation
        {
            get { return App.TimeLocation; }
        }

        public DateTime LocalizedTime
        {
            get { return App.LocalizedTime; }
        }

        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            return App.UniversalTimeToLocalizedTime(utc);
        }

        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            return App.LocalizedTimeToUniversalTime(local);
        }
    }
}
