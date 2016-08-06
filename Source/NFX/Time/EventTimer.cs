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

using NFX.Log;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.Instrumentation;

namespace NFX.Time
{
  /// <summary>
  /// Provides default implementation for IEventTimer
  /// </summary>
  public sealed class EventTimer : ServiceWithInstrumentationBase<object>, IEventTimerImplementation
  {
    #region CONSTS
      public const int DEFAULT_RESOLUTION_MS = 500;
      public const int MIN_RESOLUTION_MS = 100;
      public const int MAX_RESOLUTION_MS = 5000;

      private const string THREAD_NAME = "EventTimer";
    #endregion

    #region .ctor

    #endregion

    #region Fields

      private int m_ResolutionMs = DEFAULT_RESOLUTION_MS;
      private bool m_InstrumentationEnabled;
      private Thread m_Thread;

      private Registry<Event> m_Events = new Registry<Event>();

    #endregion

    #region Properties
     /// <summary>
     /// Timer resolution in milliseconds
     /// </summary>
     [Config]
     [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
     public int ResolutionMs
     {
       get { return m_ResolutionMs; }
       set { m_ResolutionMs = value < MIN_RESOLUTION_MS ? MIN_RESOLUTION_MS : value > MAX_RESOLUTION_MS ? MAX_RESOLUTION_MS : value;  }
     }

     [Config]
     [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
     public override bool InstrumentationEnabled
     {
       get { return m_InstrumentationEnabled;}
       set { m_InstrumentationEnabled = value;}
     }

     /// <summary>
     /// Lists all events in the instance
     /// </summary>
     public IRegistry<Event> Events { get { return m_Events;}}

    #endregion

    #region Protected

      void IEventTimerImplementation.__InternalRegisterEvent(Event evt)
      {
        if (evt==null || evt.Timer!=this)
          throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+"__InternalRegisterEvent(evt==null|does not belog to timer)");

        Event existing;
        m_Events.RegisterOrReplace(evt, out existing);
        if ( existing!=null )
        {
          existing.Dispose();
        }
      }

      void IEventTimerImplementation.__InternalUnRegisterEvent(Event evt)
      {
        if (evt==null || evt.Timer!=this)
          throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+"__InternalUnRegisterEvent(evt==null|does not belog to timer)");

        m_Events.Unregister(evt);
      }

      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);
        foreach (var enode in node.Children.Where(n => n.IsSameName(Event.CONFIG_EVENT_SECTION)))
        {
           FactoryUtils.Make<Event>(enode, typeof(Event), new object[]{this, enode});
        }
      }

      protected override void DoStart()
      {
        base.DoStart();
        m_Thread = new Thread(threadSpin);
        m_Thread.Name = THREAD_NAME;
        m_Thread.IsBackground = true;
        m_Thread.Start();
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();
        m_Thread.Join();
        m_Thread = null;
      }

    #endregion

    #region .pvt


      private DateTime m_LastStatDump = DateTime.MinValue;

      private void threadSpin()
      {
        const int INSTR_DUMP_MS = 7397;

        while(App.Active && Running)
        {
          var utcNow = App.TimeSource.UTCNow;
          visitAll(utcNow);

          if (m_InstrumentationEnabled && (utcNow-m_LastStatDump).TotalMilliseconds>INSTR_DUMP_MS)
          {
            dumpStats();
            m_LastStatDump = utcNow;
          }

          Thread.Sleep(m_ResolutionMs);
        }
      }


      private int m_stat_EventCount;
      private int m_stat_EventsFired;

      private void dumpStats()
      {
        Instrumentation.EventCount.Record( m_stat_EventCount );
        Instrumentation.FiredEventCount.Record( m_stat_EventsFired );
        m_stat_EventsFired = 0;
      }


      private void visitAll(DateTime utcNow)
      {
        try
        {
           foreach(var evt in m_Events)
           {
             try
             {
               var fired = evt.VisitAndCheck(utcNow);
               if (fired)
                 m_stat_EventsFired++;
             }
             catch(Exception evtError)
             {
               log(MessageType.Error, "Event: "+evt.Name, "Event processing leaked: "+evtError.ToMessageWithType(), evtError);
             }
           }

           m_stat_EventCount = m_Events.Count;

        }
        catch(Exception error)
        {
          log(MessageType.CatastrophicError, "visitAll()", "Exception leaked: "+error.ToMessageWithType(), error);
        }
      }


      private void log(MessageType tp, string from ,string text, Exception error)
      {
        App.Log.Write(new Message
        {
          Type = tp,
          Topic = CoreConsts.TIME_TOPIC,
          From = "{0}.{1}".Args(GetType().Name, from),
          Text = text,
          Exception = error
        });
      }


    #endregion


  }
}
