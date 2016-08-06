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
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using NFX.ApplicationModel;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.Log;
using NFX.Glue.Protocol;

namespace NFX.Glue.Implementation
{
    /// <summary>
    /// Provides default implementation for IGlue. This is the root context for all other glue objects
    /// </summary>
    [ConfigMacroContext]
    public sealed class GlueService : ServiceWithInstrumentationBase<object>, IGlueImplementation
    {
        #region CONSTS
           public const string CONFIG_PROVIDERS_SECTION = "providers";
           public const string CONFIG_PROVIDER_SECTION = "provider";
           public const string CONFIG_PROVIDER_ATTR = "provider";

           public const string CONFIG_BINDINGS_SECTION = "bindings";
           public const string CONFIG_BINDING_SECTION = "binding";

           public const string CONFIG_SERVERS_SECTION = "servers";
           public const string CONFIG_SERVER_SECTION = "server";

           public const string CONFIG_TRANSPORT_SECTION = "transport";

           public const int DEFAULT_DISPATCH_TIMEOUT_MS = 100;
           public const int DEFAULT_TIMEOUT_MS = 20000;

           public const int DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS = 10*1000;
           public const int MINIMUM_SERVER_INSTANCE_LOCK_TIMEOUT_MS = 5;

           public const MessageType DEFAULT_CLIENT_LOG_LEVEL = MessageType.Error;
           public const MessageType DEFAULT_SERVER_LOG_LEVEL = MessageType.Error;

           private const string THREAD_NAME = "GlueService.Manager";
           private const int THREAD_GRANULARITY = 5000;


        #endregion

        #region .ctors

            public GlueService() : base()
            {

            }

            public GlueService(object director) : base(director)
            {

            }
        #endregion

        #region Fields

          private ServerHandler m_ServerHandler;

          private Calls m_Calls;

          private Thread m_Thread;
          private bool m_InstrumentationEnabled;

          internal Providers m_Providers = new Providers();
          internal Bindings  m_Bindings = new Bindings();
          internal Servers   m_Servers = new Servers();

          [Config("$default-dispatch-timeout-ms", DEFAULT_DISPATCH_TIMEOUT_MS)]
          private int m_DefaultDispatchTimeoutMs;

          [Config("$default-timeout-ms", DEFAULT_TIMEOUT_MS)]
          private int m_DefaultTimeoutMs;


          [Config("$client-log-level", DEFAULT_CLIENT_LOG_LEVEL)]
          private MessageType m_ClientLogLevel = DEFAULT_CLIENT_LOG_LEVEL;

          [Config("$server-log-level", DEFAULT_SERVER_LOG_LEVEL)]
          private MessageType m_ServerLogLevel = DEFAULT_SERVER_LOG_LEVEL;

          [Config("$server-instance-lock-timeout-ms", DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS)]
          private int m_ServerInstanceLockTimeoutMs = DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS;


          private OrderedRegistry<IClientMsgInspector> m_ClientMsgInspectors = new OrderedRegistry<IClientMsgInspector>();
          private OrderedRegistry<IServerMsgInspector> m_ServerMsgInspectors = new OrderedRegistry<IServerMsgInspector>();


        #endregion


                #region Properties

                    public override string ComponentCommonName { get { return "glue"; }}

                    /// <summary>
                    /// Implements IInstrumentable
                    /// </summary>
                    [Config(Default=false)]
                    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
                    public override bool InstrumentationEnabled
                    {
                      get { return m_InstrumentationEnabled;}
                      set { m_InstrumentationEnabled = value;}
                    }

                    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
                    public int DefaultDispatchTimeoutMs
                    {
                      get { return m_DefaultDispatchTimeoutMs; }
                      set { m_DefaultDispatchTimeoutMs = value>0 ? value : 0; }
                    }

                    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
                    public int DefaultTimeoutMs
                    {
                      get { return m_DefaultTimeoutMs; }
                      set { m_DefaultTimeoutMs = value>0 ? value : 0; }
                    }

                    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
                    public MessageType ClientLogLevel
                    {
                      get { return m_ClientLogLevel; }
                      set { m_ClientLogLevel = value; }
                    }

                    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
                    public MessageType ServerLogLevel
                    {
                      get { return m_ServerLogLevel; }
                      set { m_ServerLogLevel = value; }
                    }

                    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
                    public int ServerInstanceLockTimeoutMs
                    {
                      get { return m_ServerInstanceLockTimeoutMs; }
                      set { m_ServerInstanceLockTimeoutMs = value<MINIMUM_SERVER_INSTANCE_LOCK_TIMEOUT_MS ? MINIMUM_SERVER_INSTANCE_LOCK_TIMEOUT_MS : value; }
                    }

                    public IRegistry<Provider> Providers
                    {
                        get { return m_Providers; }
                    }

                    public IRegistry<Binding> Bindings
                    {
                        get { return m_Bindings; }
                    }

                    public IRegistry<ServerEndPoint> Servers
                    {
                        get { return m_Servers; }
                    }



                    public IConfigSectionNode GlueConfiguration
                    {
                      get { return App.ConfigRoot[CommonApplicationLogic.CONFIG_GLUE_SECTION];}
                    }

                    public IConfigSectionNode ProvidersConfigurationSection
                    {
                      get { return GlueConfiguration[CONFIG_PROVIDERS_SECTION]; }
                    }

                    public IEnumerable<IConfigSectionNode> ProviderConfigurations
                    {
                      get { return ProvidersConfigurationSection.Children.Where(n=> n.IsSameName(CONFIG_PROVIDER_SECTION)); }
                    }

                    public IConfigSectionNode BindingsConfigurationSection
                    {
                      get { return GlueConfiguration[CONFIG_BINDINGS_SECTION]; }
                    }

                    public IEnumerable<IConfigSectionNode> BindingConfigurations
                    {
                       get { return BindingsConfigurationSection.Children.Where(n=> n.IsSameName(CONFIG_BINDING_SECTION)); }
                    }

                    public IConfigSectionNode ServersConfigurationSection
                    {
                      get { return GlueConfiguration[CONFIG_SERVERS_SECTION]; }
                    }

                    public IEnumerable<IConfigSectionNode> ServerConfigurations
                    {
                      get { return ServersConfigurationSection.Children.Where(n=> n.IsSameName(CONFIG_SERVER_SECTION)); }
                    }

                    /// <summary>
                    /// Returns client message inspectors for this instance
                    /// </summary>
                    public OrderedRegistry<IClientMsgInspector> ClientMsgInspectors { get { return m_ClientMsgInspectors; } }

                    /// <summary>
                    /// Returns server message inspectors for this instance
                    /// </summary>
                    public OrderedRegistry<IServerMsgInspector> ServerMsgInspectors { get { return m_ServerMsgInspectors; } }


                #endregion



        #region Public



            public void RegisterProvider(Provider p)
            {
                if (!m_Providers.Register(p))
                    throw new GlueException(StringConsts.GLUE_DUPLICATE_NAMED_INSTANCE_ERROR +
                        "Provider = " + p.Name);
            }

            public void UnregisterProvider(Provider p)
            {
                m_Providers.Unregister(p);
            }

            public void RegisterBinding(Binding b)
            {
                if (!m_Bindings.Register(b))
                    throw new GlueException(StringConsts.GLUE_DUPLICATE_NAMED_INSTANCE_ERROR +
                        "Binding = " + b.Name);
            }

            public void UnregisterBinding(Binding b)
            {
                m_Bindings.Unregister(b);
            }

            public void RegisterServerEndpoint(ServerEndPoint ep)
            {
                if (!m_Servers.Register(ep))
                    throw new GlueException(StringConsts.GLUE_DUPLICATE_NAMED_INSTANCE_ERROR +
                        "ServerEndPoint = " + ep.Name);
            }


            public void UnregisterServerEndpoint(ServerEndPoint ep)
            {
                m_Servers.Unregister(ep);
            }




            public Binding GetNodeBinding(Node node)
            {
                var binding = Bindings[node.Binding];
                if (binding==null)
                  throw new GlueException(StringConsts.GLUE_NAMED_BINDING_NOT_FOUND_ERROR + node.Binding);

                return binding;
            }

            public Binding GetNodeBinding(string node)
            {
                var nd = new Node(node);
                var binding = Bindings[nd.Binding];
                if (binding==null)
                  throw new GlueException(StringConsts.GLUE_NAMED_BINDING_NOT_FOUND_ERROR + nd.Binding);

                return binding;
            }



            public RequestMsg ClientDispatchingRequest(ClientEndPoint client, RequestMsg request)
            {
              //Glue level inspectors
              foreach(var insp in ClientMsgInspectors.OrderedValues)
                    request = insp.ClientDispatchCall(client, request);

              return request;
            }

            public void ClientDispatchedRequest(ClientEndPoint client, RequestMsg request, CallSlot callSlot)
            {
               if (client.Binding.OperationFlow == OperationFlow.Asynchronous)
                 m_Calls.Put(callSlot);
            }


            public void ClientDeliverAsyncResponse(ResponseMsg response)
            {
               clientDeliverAsyncResponse(response, true);
            }

            private void clientDeliverAsyncResponse(ResponseMsg response, bool first)
            {
               var callSlot =  m_Calls.TryGetAndRemove(response.RequestID);

               if (callSlot!=null)
                   callSlot.DeliverResponse(response);
               else
               {
                  if (first)
                  // If execution paused right after Send and before ClientDispatchedRequest could register callslot
                  // we re-try asynchronously to find call slot again
                   Task.Delay(1000).ContinueWith( (t, objRes) => clientDeliverAsyncResponse(objRes as ResponseMsg, false), response);
                  else
                   if (m_InstrumentationEnabled) Instrumentation.CallSlotNotFoundErrorEvent.Happened();
               }
            }

            public void ServerDispatchRequest(RequestMsg request)
            {
                m_ServerHandler.HandleRequestAsynchronously(request);
            }

            public ResponseMsg ServerHandleRequest(RequestMsg request)
            {
                return m_ServerHandler.HandleRequestSynchronously(request);
            }

            public ResponseMsg ServerHandleRequestFailure(FID reqID, bool oneWay, Exception failure, object bindingSpecCtx)
            {
                return m_ServerHandler.HandleRequestFailure(reqID, oneWay, failure, bindingSpecCtx);
            }

        #endregion

        #region Protected





            protected override void DoStart()
            {
                const string FROM  = "GlueService.DoStart()";
                const string START = "starting";
                const string STOP  = "stopping";

                base.DoStart();

                try
                {
                  m_ServerHandler = new ServerHandler(this);

                  m_Calls = new Calls(0);

                  run(() => m_ServerHandler.Start(), START, "server handler", m_ServerHandler.Name, m_ServerHandler.GetType());

                  //todo  Add partial failure handling, i.e. what if 2nd binding fails? then Providers that have started already need to stop
                  foreach(var p in m_Providers) run(() => p.Start(), START, "provider", p.Name, p.GetType());
                  foreach(var b in m_Bindings)  run(() => b.Start(), START, "binding",  b.Name, b.GetType());
                  foreach(var s in m_Servers)   run(() => s.Open(),  START, "server",   s.Name, s.GetType());

                  m_Thread = new Thread(threadSpin);
                  m_Thread.Name = THREAD_NAME;
                  m_Thread.Start();

                  log(MessageType.Info, "Started OK", FROM);
                }
                catch(Exception error)
                {
                  AbortStart();

                  log(MessageType.CatastrophicError, "Exception: " + error.ToMessageWithType(), FROM);

                  if (m_Thread != null)
                  {
                      m_Thread.Join();
                      m_Thread = null;
                  }

                  foreach (var s in m_Servers)   run(() => s.Close(),               STOP, "server",   s.Name, s.GetType(), false);
                  foreach (var b in m_Bindings)  run(() => b.WaitForCompleteStop(), STOP, "binding",  b.Name, b.GetType(), false);
                  foreach (var p in m_Providers) run(() => p.WaitForCompleteStop(), STOP, "provider", p.Name, p.GetType(), false);

                  if (m_ServerHandler != null)
                  {
                      run(() => m_ServerHandler.Dispose(), STOP, "server handler", m_ServerHandler.Name, m_ServerHandler.GetType(), false);
                      m_ServerHandler = null;
                  }

                  throw error;
                }
            }

            protected override void DoSignalStop()
            {
                base.DoSignalStop();
                m_ServerHandler.SignalStop();

                foreach(var bind in m_Bindings) bind.SignalStop();
                foreach(var prov in m_Providers) prov.SignalStop();

                m_Thread.Interrupt();
            }

            protected override void DoWaitForCompleteStop()
            {
                const string FROM = "GlueService.DoWaitForCompleteStop()";
                const string STOP = "stopping";

                base.DoWaitForCompleteStop();

                try
                {
                    m_Thread.Join();
                    m_Thread = null;

                    foreach(var s in m_Servers)   run(() => s.Close(), STOP, "server", s.Name, s.GetType(), false);
                    foreach(var b in m_Bindings)  run(() => b.WaitForCompleteStop(), STOP, "binding", b.Name, b.GetType(), false);
                    foreach(var p in m_Providers) run(() => p.WaitForCompleteStop(), STOP, "provider", p.Name, p.GetType(), false);

                    run(() => m_ServerHandler.WaitForCompleteStop(), STOP, "server handler", m_ServerHandler.Name, m_ServerHandler.GetType(), false);

                    m_ServerHandler.Dispose();
                    m_ServerHandler = null;

                    m_Calls = null;
                }
                catch(Exception error)
                {
                    if (m_ServerHandler != null)
                    try
                    {
                        m_ServerHandler.Dispose();
                        m_ServerHandler = null;
                    }
                    catch
                    {
                    }

                    log(MessageType.CatastrophicError, "Exception: " + error.ToMessageWithType(), FROM);
                    throw error;
                }
            }


            protected override void DoConfigure(IConfigSectionNode node)
            {
                const string CONFIG = "configuring";

                base.DoConfigure(node);

                run(() => ConfigAttribute.Apply(this, node), CONFIG, "glue", Name, GetType());

                foreach (var pnode in node[CONFIG_PROVIDERS_SECTION].Children.Where(n =>  n.IsSameName(CONFIG_PROVIDER_SECTION)))
                {
                    var name = pnode.AttrByName(CONFIG_NAME_ATTR).ValueAsString();
                    run(() => FactoryUtils.MakeAndConfigure<Provider>(pnode, args: new object[] { this, name }), CONFIG, "provider", name);
                }

                foreach (var bnode in node[CONFIG_BINDINGS_SECTION].Children.Where(n =>  n.IsSameName(CONFIG_BINDING_SECTION)))
                {
                    var name = bnode.AttrByName(CONFIG_NAME_ATTR).ValueAsString();
                    run(() => FactoryUtils.MakeAndConfigure<Binding>(bnode, args: new object[] { this, name }), CONFIG, "binding", name);
                }

                foreach (var snode in node[CONFIG_SERVERS_SECTION].Children.Where(n =>  n.IsSameName(CONFIG_SERVER_SECTION)))
                {
                    var name = snode.AttrByName(CONFIG_NAME_ATTR).ValueAsString();
                    run(() => FactoryUtils.MakeAndConfigure<ServerEndPoint>(snode,
                                defaultType: typeof(ServerEndPoint), args: new object[] { this, name }), CONFIG, "server", name);
                }

                run(() => MsgInspectorConfigurator.ConfigureClientInspectors(m_ClientMsgInspectors, node), CONFIG, "ClientInspectors");
                run(() => MsgInspectorConfigurator.ConfigureServerInspectors(m_ServerMsgInspectors, node), CONFIG, "ServerInspectors");
            }


        #endregion



        #region .pvt. impl

                  private void threadSpin()
                  {
                      var cmpName = string.Empty;

                      while(Running)
                          try
                          {
                                var now = LocalizedTime;

                                cmpName = ".Providers";
                                foreach(var component in Providers)
                                {
                                    cmpName = component.Name;
                                    component.AcceptManagerVisit(this,  now);
                                }

                                cmpName = ".Bindings";
                                foreach(var component in Bindings)
                                {
                                    cmpName = component.Name;
                                    component.AcceptManagerVisit(this,  now);
                                }

                                cmpName = m_ServerHandler.Name;
                                m_ServerHandler.AcceptManagerVisit(this, now);

                                cmpName = "purgeTimedOutCallSlots";
                                purgeTimedOutCallSlots(now);

                                cmpName = string.Empty;

                                try { Thread.Sleep(THREAD_GRANULARITY); }
                                catch(ThreadInterruptedException) { }
                          }
                          catch(Exception error)
                          {
                               log(MessageType.CatastrophicError, error.Message, "GlueService.threadSpin(component: '{0}')".Args(cmpName), error);
                          }

                  }



            private void run(Action func, string topic, string what, string name = null, Type type = null, bool rethrow = true)
            {
                  try { func(); }
                  catch (Exception e)
                  {
                      log(MessageType.CatastrophicError, err: e, msg:
                          "Error {0} {1}{2}{3}".Args(
                                topic, what,
                                (string.IsNullOrEmpty(name) ? string.Empty : " '{0}'".Args(name)),
                                (type == null ? string.Empty : " ({0})".Args(type.Name))));
                      if (rethrow)
                          throw;
                  }
            }

            private void tryClose(Action func, string type, string name)
            {
                try { func(); }
                catch (Exception e)
                {
                    log(MessageType.Error, string.Format("Error stopping {0} [{1}]", name, type), err: e);
                }
            }

            private void log(MessageType type, string msg, string from = "GlueService", Exception err = null)
            {
              App.Log.Write(
                new Message
                {
                  Type = type,
                  From = from,
                  Topic = CoreConsts.GLUE_TOPIC,
                  Text = msg,
                  Exception = err
                }

              );
            }


            private DateTime lastPurgeTimedOutCallSlots = DateTime.MinValue;
            private void purgeTimedOutCallSlots(DateTime now)
            {
              const int PURGE_EVERY_SEC = 197;

              if ((now - lastPurgeTimedOutCallSlots).TotalSeconds < PURGE_EVERY_SEC) return;
              lastPurgeTimedOutCallSlots = now;

              Task.Factory.StartNew(
                 () =>
                 {
                   var calls = m_Calls;
                   if (calls!=null)
                   {
                     var removed = calls.PurgeTimedOutSlots();
                     if (removed>0)
                     {
                       if (m_InstrumentationEnabled)
                        Instrumentation.ClientTimedOutCallSlotsRemoved.Record(removed);

                       if (m_ClientLogLevel<=MessageType.Info)
                        log(MessageType.Info, "Purged {0} timed-out CallSlot objects".Args(removed));

                       NFX.ExternalRandomGenerator.Instance.FeedExternalEntropySample(removed << 7);
                     }
                   }
                 });
            }

        #endregion

    }
}
