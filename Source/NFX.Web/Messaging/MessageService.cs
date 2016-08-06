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
using System.Threading;



using NFX;
using NFX.Log;
using NFX.Environment;
using NFX.ApplicationModel;
using NFX.ServiceModel;
using NFX.Instrumentation;


namespace NFX.Web.Messaging
{

  /// <summary>
  /// Provides implementation for IMessenger service
  /// </summary>
  public sealed class MessageService : Service, IMessengerImplementation, IInstrumentable
  {
    #region CONSTS

      public const string CONFIG_MESSAGING_SECTION = "messaging";
      public const string CONFIG_MAILER_SECTION = "mailer";
      public const string CONFIG_SINK_SECTION = "sink";

      private const string THREAD_NAME = "MailerService Thread";
      private const int INSTRUMENTATION_GRANULARITY_MS = 10000;
    #endregion

    #region .ctor and static/lifecycle
      private static object s_Lock = new object();
      private static IMessengerImplementation s_Instance;

      /// <summary>
      /// Returns a singleton instance of the default mailer
      /// </summary>
      public static IMessenger Instance
      {
        get
        {
          var instance = s_Instance;
          if (instance!=null) return instance;
          lock(s_Lock)
          {
            instance = s_Instance;
            if (instance!=null) return instance;

            instance = FactoryUtils.MakeAndConfigure<IMessengerImplementation>(App.ConfigRoot[CONFIG_MESSAGING_SECTION][CONFIG_MAILER_SECTION], typeof(MessageService));
            instance.Start();
            App.Instance.RegisterAppFinishNotifiable(instance);
            s_Instance = instance;
            return s_Instance;
          }
        }
      }

      /// <summary>
      /// Constructs the service. For most-typical cases use MailerService.Instance instead
      /// </summary>
      public MessageService() : base(null) {}

      protected override void Destructor()
      {
         base.Destructor();
         if (s_Instance==this)  s_Instance = null;
      }

      public void ApplicationFinishBeforeCleanup(IApplication application)
      {
        Dispose();
      }

      public void ApplicationFinishAfterCleanup(IApplication application) {}
    #endregion

    #region Private Fields

     private Thread m_Thread;
     private ConcurrentQueue<Message>[] m_Queues;
     private MessageSink m_Sink;
     private AutoResetEvent m_Trigger;
     private long m_stat_MessagesCount, m_stat_MessagesErrorCount;

    #endregion

    #region Properties

     /// <summary>
     /// Gets/sets sink that performs sending
     /// </summary>
     public IMessageSink Sink
     {
        get { return m_Sink; }
        set
        {
          CheckServiceInactive();

          if (value!=null && value.ComponentDirector!=this)
            throw new WebException(StringConsts.MAILER_SINK_IS_NOT_OWNED_ERROR);
          m_Sink = value as MessageSink;
        }
     }


    #endregion

    #region Public

      public void SendMsg(Message msg)
      {
        if (!Running || msg==null) return;
        var queues = m_Queues;
        if (queues==null) return;

        var idx = (int)msg.Priority;
        if (idx>queues.Length) idx=queues.Length-1;


        var queue = queues[idx];
        queue.Enqueue(msg);
        var trigger = m_Trigger;
        if (trigger!=null) trigger.Set();
      }

    #endregion

    #region Protected

      protected override void DoConfigure(Environment.IConfigSectionNode node)
      {
        base.DoConfigure(node);
        m_Sink = FactoryUtils.MakeAndConfigure<MessageSink>(node[CONFIG_SINK_SECTION], typeof(SMTPMessageSink), args: new object[]{this});
      }

      protected override void DoStart()
      {
        log(MessageType.Info, "Entering DoStart()", null);

        try
        {
          if (m_Sink==null)
           throw new WebException(StringConsts.MAILER_SINK_IS_NOT_SET_ERROR);

          m_Trigger = new AutoResetEvent(false);

          m_Queues = new ConcurrentQueue<Message>[(int)MsgPriority.Slowest+1];
          for(var i=0; i<m_Queues.Length; i++)
           m_Queues[i] = new ConcurrentQueue<Message>();

          m_Sink.Start();

           m_Thread = new Thread(threadSpin);
           m_Thread.Name = THREAD_NAME;
           m_Thread.IsBackground = false;

           m_Thread.Start();
        }
        catch (Exception error)
        {
          AbortStart();

          if (m_Thread != null)
          {
            m_Thread.Join();
            m_Thread = null;
          }

          log(MessageType.CatastrophicError, "DoStart() exception: " + error.Message, null);
          throw error;
        }

        log(MessageType.Info, "Exiting DoStart()", null);
      }

      protected override void DoSignalStop()
      {
        log(MessageType.Info, "Entering DoSignalStop()", null);
        try
        {
            m_Sink.SignalStop();
            m_Trigger.Set();
        }
        catch (Exception error)
        {
          log(MessageType.CatastrophicError, "DoSignalStop() exception: " + error.Message, null);
          throw error;
        }

        log(MessageType.Info, "Exiting DoSignalStop()", null);

      }

      protected override void DoWaitForCompleteStop()
      {
        log(MessageType.Info, "Entering DoWaitForCompleteStop()", null);

        try
        {
          base.DoWaitForCompleteStop();

          m_Thread.Join();
          m_Thread = null;

          m_Sink.WaitForCompleteStop();
          m_Trigger.Dispose();
          m_Trigger = null;
        }
        catch (Exception error)
        {
          log(MessageType.CatastrophicError, "DoWaitForCompleteStop() exception: " + error.Message, null);
          throw error;
        }

        log(MessageType.Info, "Exiting DoWaitForCompleteStop()", null);
      }

    #endregion

    #region IInstrumentation

    /// <summary>
    /// Turns instrumentation on/off
    /// </summary>
    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_WEB)]
    public bool InstrumentationEnabled { get; set; }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters { get { return ExternalParameterAttribute.GetParameters(this); } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }

    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
    }

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(this, name, value, groups);
    }
    #endregion

    #region .pvt. impl.
                private void log(MessageType type, string message, string parameters)
                {
                  App.Log.Write(
                          new Log.Message
                          {
                            Text = message ?? string.Empty,
                            Type = type,
                            From = this.Name,
                            Topic = StringConsts.MAILER_LOG_TOPIC,
                            Parameters = parameters ?? string.Empty
                          }
                        );
                }

                private void threadSpin()
                {
                     try
                     {
                          var lastInstr = DateTime.Now;
                          while (Running)
                          {
                            var count = 50;
                            for(var i=0; i<m_Queues.Length && Running; i++)
                            {
                              write(m_Queues[i], count<1 ? 1 : count);
                              count /= 2;
                            }

                            m_Trigger.WaitOne(1000);

                            var now = DateTime.Now;

                            if (InstrumentationEnabled && (now - lastInstr).TotalMilliseconds > INSTRUMENTATION_GRANULARITY_MS)
                            {
                              lastInstr = now;
                              dumpStats();
                            }
                          }//while

                          for(var i=0; i<m_Queues.Length; i++)
                           write(m_Queues[i], -1);
                          dumpStats();
                     }
                     catch(Exception e)
                     {
                         log(MessageType.Emergency, " threadSpin() leaked exception", e.Message);
                     }

                     log(MessageType.Info, "Exiting threadSpin()", null);
                }

                private void write(ConcurrentQueue<Message> queue, int count)  //-1 ==all
                {
                   const int ABORT_TIMEOUT_MS = 10000;

                   var processed = 0;
                   Message msg;
                   var started = DateTime.Now;
                   while( (count<0 || processed<count) && queue.TryDequeue(out msg))
                   {
                      if (!Running && (DateTime.Now-started).TotalMilliseconds>ABORT_TIMEOUT_MS)
                      {
                         log(MessageType.Error, "{0}.Write(msg) aborted on svc shutdown: timed-out after {1} ms.".Args( m_Sink.GetType().FullName, ABORT_TIMEOUT_MS), null);
                         break;
                      }

                      try
                      {
                        statSend();
                        m_Sink.SendMsg(msg);
                      }
                      catch(Exception error)
                      {
                        statSendError();
                        var et = error.ToMessageWithType();
                        log(MessageType.Error, "{0}.Write(msg) leaked {1}".Args(m_Sink.GetType().FullName, et), et);
                      }
                      processed++;
                   }
                }

                private void dumpStats()
                {
                  var src = this.Name;
                  Instrumentation.MessagingSinkCount.Record(src, m_stat_MessagesCount);
                  m_stat_MessagesCount = 0;

                  Instrumentation.MessagingSinkErrorCount.Record(src, m_stat_MessagesErrorCount);
                  m_stat_MessagesErrorCount = 0;
                }

                private void resetStats()
                {
                  m_stat_MessagesCount = 0;
                  m_stat_MessagesErrorCount = 0;
                }
                private void statSendError()
                {
                  Interlocked.Increment(ref m_stat_MessagesErrorCount);
                }

                private void statSend()
                {
                  Interlocked.Increment(ref m_stat_MessagesCount);
                }
    #endregion

  }//service

}
