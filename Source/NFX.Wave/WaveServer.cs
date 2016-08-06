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
using System.Threading;

using NFX.Log;
using NFX.Collections;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.IO.Net.Gate;
using NFX.Instrumentation;
using NFX.Serialization.JSON;

using NFX.Wave.Filters;

namespace NFX.Wave
{
  /// <summary>
  /// Represents "(W)eb(A)pp(V)iew(E)nhanced" web server which provides DYNAMIC web site services.
  /// This server is not meant to be exposed directly to the public Internet, rather it should be used as an application server
  /// behind the reverse proxy, such as NGINX. This server is designed to serve dynamic data-driven requests/APIs and not meant to be used
  /// for serving static content files (although it can).
  /// The implementation is based on a lightweight HttpListener that processes incoming Http requests via an injectable WorkDispatcher
  /// which governs the threading and WorkContext lifecycle.
  /// The server processing pipeline is purposely designed to be synchronous-blocking (thread per call) which does not mean that the
  /// server is inefficient, to the contrary - this server design is specifically targeting short-called methods relying on a classical thread call stack.
  /// This approach obviates the need to create surrogate message loops/synchro contexts, tasks and other objects that introduce extra GC load.
  /// The server easily support "dangling"(left open indefinitely) WorkContext instances that can stream events (i.e. SSE/Server Push) and WebSockets from
  ///  specially-purposed asynchronous notification threads.
  /// </summary>
  /// <remarks>
  /// The common belief that asynchronous non-thread-based web servers always work faster (i.e. Node.js) is not true in the data-oriented systems of high scale because
  ///  eventually multiple web server machines still block on common data access resources, so it is much more important to design the database backend
  /// in an asynchronous fashion, as it is the real bottle neck of the system. Even if most of the available threads are not physically paused by IO,
  ///  they are paused logically as the logical units of work are waiting for IO and the fact that server can accept more socket requests does not mean that they
  ///  will not timeout.  The downsides of asynchronous web layers are:
  ///   (a) much increased implementation/maintenance complexity
  ///   (b) many additional task/thread context switches and extra objects that facilitate the event loops/messages/tasks etc...
  /// </remarks>
  public class WaveServer : ServiceWithInstrumentationBase<object>
  {
    #region CONSTS

      public const string CONFIG_SERVER_SECTION = "server";

      public const string CONFIG_PREFIX_SECTION = "prefix";

      public const string CONFIG_GATE_SECTION = "gate";

      public const string CONFIG_DISPATCHER_SECTION = "dispatcher";

      public const string CONFIG_DEFAULT_ERROR_HANDLER_SECTION = "default-error-handler";


      public const int DEFAULT_KERNEL_HTTP_QUEUE_LIMIT = 1000;
      public const int MIN_KERNEL_HTTP_QUEUE_LIMIT = 16;
      public const int MAX_KERNEL_HTTP_QUEUE_LIMIT = 512 * 1024;

      public const int DEFAULT_PARALLEL_ACCEPTS = 64;
      public const int MIN_PARALLEL_ACCEPTS = 1;
      public const int MAX_PARALLEL_ACCEPTS = 1024;

      public const int DEFAULT_PARALLEL_WORKS = 256;
      public const int MIN_PARALLEL_WORKS = 1;
      public const int MAX_PARALLEL_WORKS = 1024*1024;

      public const string DEFAULT_CLIENT_VARS_COOKIE_NAME = "WV.CV";

      public const int ACCEPT_THREAD_GRANULARITY_MS = 250;

      public const int INSTRUMENTATION_DUMP_PERIOD_MS = 3377;
    #endregion

    #region Static

      public static Registry<WaveServer> s_Servers = new Registry<WaveServer>();

      /// <summary>
      /// Returns the global registry of all server instances that are active in this process
      /// </summary>
      public static IRegistry<WaveServer> Servers
      {
        get{ return s_Servers; }
      }


    #endregion


    #region .ctor
      public WaveServer() : base()
      {
        m_Prefixes = new EventedList<string,WaveServer>(this, true);
        m_Prefixes.GetReadOnlyEvent = (l) => Status != ControlStatus.Inactive;
      }
    #endregion

    #region Fields

      private string m_EnvironmentName;

      private int m_KernelHttpQueueLimit = DEFAULT_KERNEL_HTTP_QUEUE_LIMIT;
      private int m_ParallelAccepts = DEFAULT_PARALLEL_ACCEPTS;
      private int m_ParallelWorks = DEFAULT_PARALLEL_WORKS;
      private HttpListener m_Listener;
      private bool m_IgnoreClientWriteErrors = true;
      private bool m_LogHandleExceptionErrors;
      private EventedList<string, WaveServer> m_Prefixes;

      private Thread m_AcceptThread;
      private Thread m_InstrumentationThread;
      private AutoResetEvent m_InstrumentationThreadWaiter;

      private Semaphore m_AcceptSemaphore;
      internal Semaphore m_WorkSemaphore;

      private INetGate m_Gate;
      private WorkDispatcher m_Dispatcher;

      private string m_ClientVarsCookieName;

      private OrderedRegistry<WorkMatch> m_ErrorShowDumpMatches = new OrderedRegistry<WorkMatch>();
      private OrderedRegistry<WorkMatch> m_ErrorLogMatches = new OrderedRegistry<WorkMatch>();


      //*Instrumentation Statistics*//
      internal bool m_InstrumentationEnabled;

          internal long m_Stat_ServerRequest;
          internal long m_Stat_ServerGateDenial;
          internal long m_Stat_ServerHandleException;
          internal long m_Stat_FilterHandleException;

          internal long m_Stat_ServerAcceptSemaphoreCount;
          internal long m_Stat_ServerWorkSemaphoreCount;

          internal long m_Stat_WorkContextWrittenResponse;
          internal long m_Stat_WorkContextBufferedResponse;
          internal long m_Stat_WorkContextBufferedResponseBytes;
          internal long m_Stat_WorkContextCtor;
          internal long m_Stat_WorkContextDctor;
          internal long m_Stat_WorkContextWorkSemaphoreRelease;
          internal long m_Stat_WorkContextAborted;
          internal long m_Stat_WorkContextHandled;
          internal long m_Stat_WorkContextNoDefaultClose;
          internal long m_Stat_WorkContextNeedsSession;

          internal long m_Stat_SessionNew;
          internal long m_Stat_SessionExisting;
          internal long m_Stat_SessionEnd;
          internal long m_Stat_SessionInvalidID;

          internal long m_Stat_GeoLookup;
          internal long m_Stat_GeoLookupHit;

          internal NamedInterlocked m_Stat_PortalRequest = new NamedInterlocked();

    #endregion




    #region Properties


       public override string ComponentCommonName { get { return "ws-"+Name; }}



      /// <summary>
      /// Provides the name of environment, i.e. DEV,PROD, TEST i.e. some handlers may depend on environment name to serve DEV vs PROD java script files etc.
      /// </summary>
      [Config]
      public string EnvironmentName
      {
        get { return m_EnvironmentName ?? string.Empty;}
        set
        {
          CheckServiceInactive();
          m_EnvironmentName = value;
        }
      }

      /// <summary>
      /// Provides the name of cookie where server keeps client vars
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB)]
      public string ClientVarsCookieName
      {
        get { return m_ClientVarsCookieName.IsNullOrWhiteSpace() ? DEFAULT_CLIENT_VARS_COOKIE_NAME : m_ClientVarsCookieName;}
        set { m_ClientVarsCookieName = value;}
      }

      /// <summary>
      /// When true, emits instrumentation messages
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public override bool InstrumentationEnabled
      {
          get { return m_InstrumentationEnabled;}
          set { m_InstrumentationEnabled = value;}
      }

      /// <summary>
      /// When true does not throw exceptions on client channel write
      /// </summary>
      [Config(Default=true)]
      public bool IgnoreClientWriteErrors
      {
        get { return m_IgnoreClientWriteErrors;}
        set
        {
          CheckServiceInactive();
          m_IgnoreClientWriteErrors = value;
        }
      }

      /// <summary>
      /// When true writes errors that get thrown in server cathc-all HandleException methods
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public bool LogHandleExceptionErrors
      {
        get { return m_LogHandleExceptionErrors;}
        set { m_LogHandleExceptionErrors = value;}
      }


      /// <summary>
      /// Establishes HTTP.sys kernel queue limit
      /// </summary>
      [Config]
      public int KernelHttpQueueLimit
      {
        get { return m_KernelHttpQueueLimit;}
        set
        {
          CheckServiceInactive();
          if (value < MIN_KERNEL_HTTP_QUEUE_LIMIT) value = MIN_KERNEL_HTTP_QUEUE_LIMIT;
           else
            if (value > MAX_KERNEL_HTTP_QUEUE_LIMIT) value = MAX_KERNEL_HTTP_QUEUE_LIMIT;
          m_KernelHttpQueueLimit = value;
        }
      }

      /// <summary>
      /// Specifies how many requests can get accepted from kernel queue in parallel
      /// </summary>
      [Config(Default=DEFAULT_PARALLEL_ACCEPTS)]
      public int ParallelAccepts
      {
        get { return m_ParallelAccepts;}
        set
        {
          CheckServiceInactive();
          if (value < MIN_PARALLEL_ACCEPTS) value = MIN_PARALLEL_ACCEPTS;
           else
            if (value > MAX_PARALLEL_ACCEPTS) value = MAX_PARALLEL_ACCEPTS;
          m_ParallelAccepts = value;
        }
      }


      /// <summary>
      /// Specifies how many instances of WorkContext(or derivatives) can be processed at the same time
      /// </summary>
      [Config(Default=DEFAULT_PARALLEL_WORKS)]
      public int ParallelWorks
      {
        get { return m_ParallelWorks;}
        set
        {
          CheckServiceInactive();
          if (value < MIN_PARALLEL_WORKS) value = MIN_PARALLEL_WORKS;
           else
            if (value > MAX_PARALLEL_WORKS) value = MAX_PARALLEL_WORKS;
          m_ParallelWorks = value;
        }
      }

      /// <summary>
      /// Returns HttpListener prefixes such as "http://+:8080/"
      /// </summary>
      public IList<string> Prefixes { get { return m_Prefixes;}}


      /// <summary>
      /// Gets/sets network gate
      /// </summary>
      public INetGate Gate
      {
        get { return m_Gate;}
        set
        {
          CheckServiceInactive();
          m_Gate = value;
        }
      }

      /// <summary>
      /// Gets/sets work dispatcher
      /// </summary>
      public WorkDispatcher Dispatcher
      {
        get { return m_Dispatcher;}
        set
        {
          CheckServiceInactive();
          if (value!=null && value.ComponentDirector!=this)
            throw new WaveException(StringConsts.DISPATCHER_NOT_THIS_SERVER_ERROR);
          m_Dispatcher = value;
        }
      }

      /// <summary>
      /// Returns matches used by the server's default error handler to determine whether exception details should be shown
      /// </summary>
      public OrderedRegistry<WorkMatch> ShowDumpMatches { get{ return m_ErrorShowDumpMatches;}}

      /// <summary>
      /// Returns matches used by the server's default error handler to determine whether exception details should be logged
      /// </summary>
      public OrderedRegistry<WorkMatch> LogMatches { get{ return m_ErrorLogMatches;}}

    #endregion

    #region Public
      /// <summary>
      /// Handles processing exception by calling ErrorFilter.HandleException(work, error).
      /// All parameters except ERROR can be null - which indicates error that happened during WorkContext dispose
      /// </summary>
      public virtual void HandleException(WorkContext work, WorkFilter filter, WorkHandler handler, Exception error)
      {
         try
         {
            if (m_InstrumentationEnabled) Interlocked.Increment(ref m_Stat_ServerHandleException);

            //work may be null (when WorkContext is already disposed)
            if (work!=null)
              ErrorFilter.HandleException(work, error, m_ErrorShowDumpMatches, m_ErrorLogMatches);
            else
             Log(MessageType.Error,
                 StringConsts.SERVER_DEFAULT_ERROR_WC_NULL_ERROR + error.ToMessageWithType(),
                 "HandleException()",
                 error);
         }
         catch(Exception error2)
         {
            if (m_LogHandleExceptionErrors)
              try
              {
                Log(MessageType.Error,
                     StringConsts.SERVER_DEFAULT_ERROR_HANDLER_ERROR + error2.ToMessageWithType(),
                     "HandleException.catch()",
                     error2,
                     pars: new
                      {
                        OriginalError = error.ToMessageWithType()
                      }.ToJSON()
                     );
              }
              catch{}
         }
      }

      /// <summary>
      /// Facilitates server logging
      /// </summary>
      public void Log(MessageType type, string text, string from = null, Exception error = null, string pars = null, Guid? related = null)
      {
        var msg = new Message
        {
          Type = type,
          Topic = SysConsts.WAVE_LOG_TOPIC,
          From = "Server('{0}.{1}').{2}".Args(GetType().FullName, Name, from),
          Text = text,
          Exception = error,
          Parameters = pars
        };

        if (related.HasValue)
          msg.RelatedTo = related.Value;

        App.Log.Write(msg);
      }

    #endregion


    #region Protected

      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node==null)
        {
          //0 get very root
          node = App.ConfigRoot[SysConsts.CONFIG_WAVE_SECTION];
          if (!node.Exists) return;

          //1 try to find the server with the same name as this instance
          var snode = node.Children.FirstOrDefault(cn=>cn.IsSameName(CONFIG_SERVER_SECTION) && cn.IsSameNameAttr(Name));

          //2 try to find a server without a name
          if (snode==null)
            snode = node.Children.FirstOrDefault(cn=>cn.IsSameName(CONFIG_SERVER_SECTION) && cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.IsNullOrWhiteSpace());

          if (snode==null) return;
          node = snode;
        }


        ConfigAttribute.Apply(this, node);

        m_Prefixes.Clear();
        foreach(var name in node.Children
                             .Where(c=>c.IsSameName(CONFIG_PREFIX_SECTION))
                             .Select(c=>c.AttrByName(Configuration.CONFIG_NAME_ATTR).Value)
                             .Where(n=>n.IsNotNullOrWhiteSpace()))
           m_Prefixes.Add(name);

        var nGate = node[CONFIG_GATE_SECTION];

        if (nGate.Exists)
          m_Gate = FactoryUtils.MakeAndConfigure<INetGateImplementation>(nGate, typeof(NetGate), args: new object[]{this});

        var nDispatcher = node[CONFIG_DISPATCHER_SECTION];

        if (nDispatcher.Exists)
          m_Dispatcher = FactoryUtils.MakeAndConfigure<WorkDispatcher>(nDispatcher, typeof(WorkDispatcher), args: new object[]{this});

        ErrorFilter.ConfigureMatches(node[CONFIG_DEFAULT_ERROR_HANDLER_SECTION], m_ErrorShowDumpMatches, m_ErrorLogMatches, GetType().FullName);
      }

      protected override void DoStart()
      {
        if (m_Prefixes.Count==0)
          throw new WaveException(StringConsts.SERVER_NO_PREFIXES_ERROR.Args(Name));

        if (!s_Servers.Register(this))
          throw new WaveException(StringConsts.SERVER_COULD_NOT_GET_REGISTERED_ERROR.Args(Name));

        try
        {
           if (m_Gate!=null)
             if (m_Gate is Service)
               ((Service)m_Gate).Start();


           if (m_Dispatcher==null)
              m_Dispatcher = new WorkDispatcher(this);

           m_Dispatcher.Start();

           m_AcceptSemaphore = new Semaphore(m_ParallelAccepts, m_ParallelAccepts);
           m_WorkSemaphore = new Semaphore(m_ParallelWorks, m_ParallelWorks);

           m_AcceptThread = new Thread(acceptThreadSpin);
           m_AcceptThread.Name = "{0}-AcceptThread".Args(Name);

           m_InstrumentationThread = new Thread(instrumentationThreadSpin);
           m_InstrumentationThread.Name = "{0}-InstrumentationThread".Args(Name);
           m_InstrumentationThreadWaiter = new AutoResetEvent(false);

           m_Listener = new HttpListener();

           foreach(var prefix in m_Prefixes)
             m_Listener.Prefixes.Add(prefix);

           BeforeListenerStart(m_Listener);

           m_Listener.Start();

           AfterListenerStart(m_Listener);


           m_Listener.IgnoreWriteExceptions = m_IgnoreClientWriteErrors;

           if (m_KernelHttpQueueLimit!=DEFAULT_KERNEL_HTTP_QUEUE_LIMIT)
              PlatformUtils.SetRequestQueueLimit(m_Listener, m_KernelHttpQueueLimit);
        }
        catch
        {
          closeListener();

          if (m_AcceptSemaphore!=null) { m_AcceptSemaphore.Dispose(); m_AcceptSemaphore = null;}
          if (m_WorkSemaphore!=null) { m_WorkSemaphore.Dispose(); m_WorkSemaphore = null;}
          if (m_AcceptThread!=null) { m_AcceptThread = null;}
          if (m_Dispatcher!=null) m_Dispatcher.WaitForCompleteStop();

          if (m_Gate!=null && m_Gate is Service)
            ((Service)m_Gate).WaitForCompleteStop();

          s_Servers.Unregister(this);

          throw;
        }

        m_InstrumentationThread.Start();
        m_AcceptThread.Start();
      }

      protected override void DoSignalStop()
      {
       // m_Listener.Stop();
        m_Listener.Abort();
        m_Dispatcher.SignalStop();

        if (m_InstrumentationThreadWaiter!=null)
              m_InstrumentationThreadWaiter.Set();

        if (m_Gate!=null)
          if (m_Gate is Service)
             ((Service)m_Gate).SignalStop();
      }

      protected override void DoWaitForCompleteStop()
      {
        s_Servers.Unregister(this);

        if (m_AcceptThread!=null)
        {
          m_AcceptThread.Join();
          m_AcceptThread = null;
        }

        if (m_InstrumentationThread!=null)
        {
          m_InstrumentationThread.Join();
          m_InstrumentationThread = null;
          m_InstrumentationThreadWaiter.Close();
        }

        closeListener();

        try
        {
           m_Dispatcher.WaitForCompleteStop();
           if (m_Gate!=null)
             if (m_Gate is Service)
                ((Service)m_Gate).WaitForCompleteStop();
        }
        finally
        {
          m_AcceptSemaphore.Dispose();
          m_AcceptSemaphore = null;

          m_WorkSemaphore.Dispose();
          m_WorkSemaphore = null;
        }
      }


      /// <summary>
      /// Factory method that makes new WorkContext instances. Override to make a WorkContext-derivative
      /// </summary>
      protected virtual WorkContext MakeContext(HttpListenerContext listenerContext)
      {
        return new WorkContext(this, listenerContext);
      }

      /// <summary>
      /// Override to set listener options such as TimeoutManager.MinSendBytesPerSecond before listener.Start()
      /// </summary>
      protected virtual void BeforeListenerStart(HttpListener listener)
      {
      }

      /// <summary>
      /// Override to set listener options such as TimeoutManager.MinSendBytesPerSecond after listener.Start()
      /// </summary>
      protected virtual void AfterListenerStart(HttpListener listener)
      {
          //this setting does not do anything on Windows 7
          //todo test on Windows 2008/20012
          m_Listener.TimeoutManager.MinSendBytesPerSecond = 16;

          //todo Need to add server-wide properties for all timeouts
      }


    #endregion

    #region .pvt

     private void acceptThreadSpin()
     {
        var semaphores = new Semaphore[]{m_AcceptSemaphore, m_WorkSemaphore};
        while(Running)
        {
          //Both semaphores get acquired here
          if (!WaitHandle.WaitAll(semaphores, ACCEPT_THREAD_GRANULARITY_MS)) continue;

          if (m_Listener.IsListening)
               m_Listener.BeginGetContext(callback, null);//the BeginGetContext/EndGetContext is called on a different thread (pool IO background)
                                                          // whereas GetContext() is called on the caller thread
        }
     }

     private void callback(IAsyncResult result)
     {
       if (m_Listener==null) return;//callback sometime happens when listener is null on shutdown
       if (!m_Listener.IsListening) return;

       //This is called on its own pool thread by HttpListener
       bool gateAccessDenied = false;
       HttpListenerContext listenerContext;
       try
       {
         listenerContext = m_Listener.EndGetContext(result);

         if (m_InstrumentationEnabled) Interlocked.Increment(ref m_Stat_ServerRequest);


         if (m_Gate!=null)
            try
            {
              var action = m_Gate.CheckTraffic(new HTTPIncomingTraffic(listenerContext.Request));
              if (action!=GateAction.Allow)
              {
                //access denied
                gateAccessDenied = true;
                listenerContext.Response.StatusCode = Web.WebConsts.STATUS_403;         //todo - need properties for this
                listenerContext.Response.StatusDescription = Web.WebConsts.STATUS_403_DESCRIPTION;
                listenerContext.Response.Close();

                if (m_InstrumentationEnabled) Interlocked.Increment(ref m_Stat_ServerGateDenial);
                return;
              }
            }
            catch(Exception denyError)
            {
              Log(MessageType.Error, denyError.ToMessageWithType(), "callback(deny request)", denyError);
            }
       }
       catch(Exception error)
       {
          if (error is HttpListenerException)
           if ((error as HttpListenerException).ErrorCode==995) return;//Aborted

          Log(MessageType.Error, error.ToMessageWithType(), "callback(endGetContext())", error);
          return;
       }
       finally
       {
          if (Running)
          {
             var acceptCount = m_AcceptSemaphore.Release();

             if (m_InstrumentationEnabled)
              Thread.VolatileWrite(ref m_Stat_ServerAcceptSemaphoreCount, acceptCount);

             if (gateAccessDenied)//if access was denied then no work will be done either
             {
                var workCount = m_WorkSemaphore.Release();
                if (m_InstrumentationEnabled)
                  Thread.VolatileWrite(ref m_Stat_ServerWorkSemaphoreCount, workCount);
             }
          }
       }

       //no need to call process() asynchronously because this whole method is on its own thread already
       if (Running)
       {
          var workContext = MakeContext(listenerContext);
          m_Dispatcher.Dispatch(workContext);
       }
     }

     private void closeListener()
     {
        if (m_Listener!=null)
        {
          try { m_Listener.Close(); }
          catch(Exception error)
          {
            Log(MessageType.Error, error.ToMessageWithType(), "closeListener()", error);
          }
          m_Listener = null;
        }
     }


     private void instrumentationThreadSpin()
     {
        var pe = m_InstrumentationEnabled;
        while(Running)
        {
          if (pe!=m_InstrumentationEnabled)
          {
            resetStats();
            pe = m_InstrumentationEnabled;
          }

          if (m_InstrumentationEnabled &&
              App.Instrumentation.Enabled)
          {
             dumpStats();
             resetStats();
          }

          m_InstrumentationThreadWaiter.WaitOne(INSTRUMENTATION_DUMP_PERIOD_MS);
        }
     }

     private void resetStats()
     {
        m_Stat_ServerRequest                        = 0;
        m_Stat_ServerGateDenial                     = 0;
        m_Stat_ServerHandleException                = 0;
        m_Stat_FilterHandleException                = 0;

        m_Stat_ServerAcceptSemaphoreCount           = 0;
        m_Stat_ServerWorkSemaphoreCount             = 0;

        m_Stat_WorkContextWrittenResponse           = 0;
        m_Stat_WorkContextBufferedResponse          = 0;
        m_Stat_WorkContextBufferedResponseBytes     = 0;
        m_Stat_WorkContextCtor                      = 0;
        m_Stat_WorkContextDctor                     = 0;
        m_Stat_WorkContextWorkSemaphoreRelease      = 0;
        m_Stat_WorkContextAborted                   = 0;
        m_Stat_WorkContextHandled                   = 0;
        m_Stat_WorkContextNoDefaultClose            = 0;
        m_Stat_WorkContextNeedsSession              = 0;

        m_Stat_SessionNew                           = 0;
        m_Stat_SessionExisting                      = 0;
        m_Stat_SessionEnd                           = 0;
        m_Stat_SessionInvalidID                     = 0;

        m_Stat_GeoLookup                            = 0;
        m_Stat_GeoLookupHit                         = 0;

        m_Stat_PortalRequest.Clear();
     }

     private void dumpStats()
     {
        var i = App.Instrumentation;

        i.Record( new Instrumentation.ServerRequest                      (Name, m_Stat_ServerRequest                      ));
        i.Record( new Instrumentation.ServerGateDenial                   (Name, m_Stat_ServerGateDenial                   ));
        i.Record( new Instrumentation.ServerHandleException              (Name, m_Stat_ServerHandleException              ));
        i.Record( new Instrumentation.FilterHandleException              (Name, m_Stat_FilterHandleException              ));

        i.Record( new Instrumentation.ServerAcceptSemaphoreCount         (Name, m_Stat_ServerAcceptSemaphoreCount         ));
        i.Record( new Instrumentation.ServerWorkSemaphoreCount           (Name, m_Stat_ServerWorkSemaphoreCount           ));

        i.Record( new Instrumentation.WorkContextWrittenResponse         (Name, m_Stat_WorkContextWrittenResponse         ));
        i.Record( new Instrumentation.WorkContextBufferedResponse        (Name, m_Stat_WorkContextBufferedResponse        ));
        i.Record( new Instrumentation.WorkContextBufferedResponseBytes   (Name, m_Stat_WorkContextBufferedResponseBytes   ));
        i.Record( new Instrumentation.WorkContextCtor                    (Name, m_Stat_WorkContextCtor                    ));
        i.Record( new Instrumentation.WorkContextDctor                   (Name, m_Stat_WorkContextDctor                   ));
        i.Record( new Instrumentation.WorkContextWorkSemaphoreRelease    (Name, m_Stat_WorkContextWorkSemaphoreRelease    ));
        i.Record( new Instrumentation.WorkContextAborted                 (Name, m_Stat_WorkContextAborted                 ));
        i.Record( new Instrumentation.WorkContextHandled                 (Name, m_Stat_WorkContextHandled                 ));
        i.Record( new Instrumentation.WorkContextNoDefaultClose          (Name, m_Stat_WorkContextNoDefaultClose          ));
        i.Record( new Instrumentation.WorkContextNeedsSession            (Name, m_Stat_WorkContextNeedsSession            ));

        i.Record( new Instrumentation.SessionNew                         (Name, m_Stat_SessionNew                         ));
        i.Record( new Instrumentation.SessionExisting                    (Name, m_Stat_SessionExisting                    ));
        i.Record( new Instrumentation.SessionEnd                         (Name, m_Stat_SessionEnd                         ));
        i.Record( new Instrumentation.SessionInvalidID                   (Name, m_Stat_SessionInvalidID                   ));

        i.Record( new Instrumentation.GeoLookup                          (Name, m_Stat_GeoLookup                          ));
        i.Record( new Instrumentation.GeoLookupHit                       (Name, m_Stat_GeoLookupHit                       ));

        foreach(var kvp in m_Stat_PortalRequest.AllLongs)
            i.Record( new Instrumentation.ServerPortalRequest(Name+"."+kvp.Key, kvp.Value) );

        var sample = (int)m_Stat_WorkContextBufferedResponseBytes;
        if (sample!=0) ExternalRandomGenerator.Instance.FeedExternalEntropySample(sample);
     }

    #endregion

  }

}
