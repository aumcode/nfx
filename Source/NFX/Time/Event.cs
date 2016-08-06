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
using System.Threading.Tasks;

using NFX.Log;
using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Time
{

  /// <summary>
  /// Denotes statuses that events get transitioned through
  /// </summary>
  public enum EventStatus {NotStarted = 0, Started, Expired, Invalid}



  /// <summary>
  /// Defines event body execution asynchrony model
  /// </summary>
  public enum EventBodyAsyncModel
  {
    /// <summary>
    /// The body should be called as a short-lived (less than 1 sec) task
    /// </summary>
    AsyncTask = 0,

    /// <summary>
    /// The body should be called as a long-runnig task. The system may dedicate it a thread.
    /// Use this ONLY for events that fire infrequently (i.e. once every X minutes+) and take long time to execute (seconds+)
    /// </summary>
    LongRunningAsyncTask,

    /// <summary>
    /// ADVANCED FEATURE. Run task synchronously on the timer thread.
    /// Use this option ONLY if the task body is very short (less than 10 ms).
    /// In most cases do not use this option as event body blocks the whole global application timer thread
    /// </summary>
    Sync
  }

  /// <summary>
  /// Represents an entity that can handle events.
  /// This type is used to implement event handlers that get injected via config
  /// </summary>
  public interface IEventHandler
  {
    void EventHandlerBody(Event sender);
    void EventStatusChange(Event sender, EventStatus priorStatus);
    void EventDefinitionChange(Event sender, string parameterName);
  }


  /// <summary>
  /// Delegate type for events that get called when timer fires
  /// </summary>
  public delegate void TimerEvent(Event sender);

  /// <summary>
  /// Delegate type for events that get called when timer event status changes
  /// </summary>
  public delegate void TimerEventStatusChange(Event sender, EventStatus priorStatus);

  /// <summary>
  /// Delegate type for events that get called when one of timer event definition parameters changes
  /// </summary>
  public delegate void TimerEventDefinitionChange(Event sender, string parameterName);

  /// <summary>
  /// Represents an event manageable by IEventTimer
  /// </summary>
  public class Event : ApplicationComponent, INamed, IExternallyParameterized, IConfigurable, ILocalizedTimeProvider
  {
    public const string CONFIG_EVENT_SECTION = "event";
    public const string CONFIG_HANDLER_SECTION = "handler";

    #region .ctor

      public Event(IEventTimer timer, IConfigSectionNode config) : this(timer, null, null, null, config)
      {

      }

      public Event(IEventTimer timer, string name = null, TimerEvent body = null, TimeSpan? interval = null, IConfigSectionNode config = null) : base(timer)
      {
         if ((timer as IEventTimerImplementation) == null)
          throw new TimeException(StringConsts.ARGUMENT_ERROR + "Event.ctor(timer=null | timer!=IEventTimerImplementation)");

         m_Timer = timer;

         if (body!=null)
          Body += body;

         if (interval.HasValue)
          m_Interval = interval.Value;

         if (config!=null)
         {
           Configure(config);
           m_Name = config.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
         }

         if (name.IsNotNullOrWhiteSpace())
           m_Name = name;

         if (m_Name.IsNullOrWhiteSpace()) m_Name = Guid.NewGuid().ToString();

         ((IEventTimerImplementation)timer).__InternalRegisterEvent(this);
      }

      protected override void Destructor()
      {
        if (!this.Disposed)
          lock(m_Lock)
          {
            ((IEventTimerImplementation)m_Timer).__InternalUnRegisterEvent(this);

            m_Context = null;
            Body = null;
            StatusChange = null;
            DefinitionChange = null;

            base.Destructor();
          }
      }

    #endregion

    #region Fields

      protected readonly object m_Lock = new object();

      private IEventTimer m_Timer;
      private string m_Name;
      private TimeLocation m_TimeLocation;
      private EventStatus m_Status = EventStatus.NotStarted;

      private bool m_Enabled = true;

      private DateTime?   m_StartDate;
      private DateTime?   m_EndDate;
      private TimeSpan    m_Interval;
      private int         m_MaxCount;

      private object m_Context;
      private int m_CallCount;
      private DateTime m_LastCall;
      private Exception m_LastError;

    #endregion

    #region Properties


      public IEventTimer Timer  { get { return m_Timer; }}
      public string      Name   { get { return m_Name;  }}
      public EventStatus Status { get { return m_Status;}}

      /// <summary>
      /// Returns time location that this Event instance operates under.
      /// </summary>
      public TimeLocation TimeLocation
      {
         get { return m_TimeLocation ?? TimeLocation.Parent;}
         set
         {
           lock(m_Lock)
           {
            if (m_TimeLocation==value) return;
            m_TimeLocation = value;
            definitionChange("TimeLocation");
           }
         }
      }

      /// <summary>
      /// Returns current time localized per TimeLocation
      /// </summary>
      public DateTime LocalizedTime
      {
          get { return UniversalTimeToLocalizedTime(App.TimeSource.UTCNow); }
      }


      /// <summary>
      /// References event handler that handles events. If it is null then only delegate events are called
      /// </summary>
      public IEventHandler EventHandler;

      /// <summary>
      /// Event body that gets called when the event is due. The body is always invoked ASYNCHRONOUSLY
      /// unless Fire(syncronous=true) is called in which case it gets called on a thread that called Fire(true)
      /// </summary>
      public event TimerEvent Body;

      /// <summary>
      /// Invoked when timer event status changes. Always called synchronously by the timer thread.
      /// Subscribers should not block for long
      /// </summary>
      public event TimerEventStatusChange StatusChange;

      /// <summary>
      /// Invoked when one of timer event definition parameters changes.
      /// Always called synchronously on the same thread that made a change.
      /// Subscribers should not block for long
      /// </summary>
      public event TimerEventDefinitionChange DefinitionChange;

      /// <summary>
      /// Specifies whether this event will fire/participate in timer loop
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
      public bool Enabled
      {
         get { return m_Enabled; }
         set
         {
           lock(m_Lock)
           {
            if (m_Enabled==value) return;
            m_Enabled = value;
            definitionChange("Enabled");
           }
         }
      }


      /// <summary>
      /// Defines how event body should be invoked
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
      public virtual EventBodyAsyncModel BodyAsyncModel
      {
         get;
         set;
      }

      /// <summary>
      /// Specifies when this event will start firing in the local event time
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
      public DateTime? StartDate
      {
         get { return m_StartDate; }
         set
         {
           lock(m_Lock)
           {
            value = AdjustDate(value);
            if (m_StartDate==value) return;
            m_StartDate = value;
            definitionChange("StartDate");
           }
         }
      }


      /// <summary>
      /// Specifies when this event will stop firing in the local event time
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
      public DateTime? EndDate
      {
         get { return m_EndDate; }
         set
         {
           lock(m_Lock)
           {
            value = AdjustDate(value);
            if (m_EndDate==value) return;
            m_EndDate = value;
            definitionChange("EndDate");
           }
         }
      }


      /// <summary>
      /// Specifies how often event fires
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
      public TimeSpan Interval
      {
         get { return m_Interval; }
         set
         {
           lock(m_Lock)
           {
            if (m_Interval==value) return;
            m_Interval = value;
            definitionChange("Interval");
           }
         }
      }

      /// <summary>
      /// Specifies how many times this event can be called. If less or equal than zero then no limit is set
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
      public int MaxCount
      {
         get { return m_MaxCount; }
         set
         {
           lock(m_Lock)
           {
            if (m_MaxCount==value) return;
            m_MaxCount = value;
            definitionChange("MaxCount");
           }
         }
      }


      /// <summary>
      /// Adds arbitrary context object to the event
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
      public object Context
      {
         get { return m_Context; }
         set
         {
           lock(m_Lock)
           {
            if (m_Context==value) return;
            m_Context = value;
            definitionChange("Context");
           }
         }
      }

      /// <summary>
      /// Returns how many times this event was called
      /// </summary>
      public int CallCount{ get { return m_CallCount; } }


      /// <summary>
      /// Returns when was this event called for the last time in the local event time
      /// </summary>
      public DateTime LastCall{ get { return m_LastCall; } }

      /// <summary>
      /// Returns the last exception thrown from event handler or nul if no error happened
      /// </summary>
      public Exception LastError{ get { return m_LastError;}}



      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }


    #endregion

    #region Public

      /// <summary>
      /// Resets call count counter. If this event has expired it will eventually transition to Started state.
      /// Keep in mind that it may expire again if EndDate is set and past due
      /// </summary>
      public void ResetCallCount()
      {
         m_CallCount = 0;
      }


      /// <summary>
      /// Calls event regardless of any constraints.
      /// Invokes a handler right away if syncInvoke is true or BodyAsyncModel is Sync,
      /// otherwise queues the task on a thread pool either as a regular or long-running task depending on BodyAsyncModel
      /// </summary>
      public void Fire(bool syncInvoke = false)
      {
        if (Disposed) return;

        if (syncInvoke || BodyAsyncModel==EventBodyAsyncModel.Sync)
         fireBody();
        else
        {
          if (BodyAsyncModel==EventBodyAsyncModel.LongRunningAsyncTask)
            Task.Factory.StartNew( fireBody, TaskCreationOptions.LongRunning );
          else
            Task.Factory.StartNew( fireBody );
        }
      }



      public virtual void Configure(IConfigSectionNode config)
      {
        if (Disposed) return;

        if (config==null) return;
        ConfigAttribute.Apply(this, config);

        var loc = config[TimeLocation.CONFIG_TIMELOCATION_SECTION];
        if (loc.Exists)
          m_TimeLocation = FactoryUtils.MakeAndConfigure<TimeLocation>(loc, typeof(TimeLocation));

        var ehnode = config[CONFIG_HANDLER_SECTION];
        if (ehnode.Exists)
          EventHandler = FactoryUtils.MakeUsingCtor<IEventHandler>(ehnode);
      }

            /// <summary>
            /// Converts universal time to local time as of TimeLocation property
            /// </summary>
            public DateTime UniversalTimeToLocalizedTime(DateTime utc)
            {
                if (utc.Kind!=DateTimeKind.Utc)
                 throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

                var loc = TimeLocation;
                if (!loc.UseParentSetting)
                {
                   return DateTime.SpecifyKind(utc + TimeLocation.UTCOffset, DateTimeKind.Local);
                }
                else
                {
                   return App.UniversalTimeToLocalizedTime(utc);
                }
            }

            /// <summary>
            /// Converts localized time to UTC time as of TimeLocation property
            /// </summary>
            public DateTime LocalizedTimeToUniversalTime(DateTime local)
            {
                if (local.Kind!=DateTimeKind.Local)
                 throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

                var loc = TimeLocation;
                if (!loc.UseParentSetting)
                {
                   return DateTime.SpecifyKind(local - TimeLocation.UTCOffset, DateTimeKind.Utc);
                }
                else
                {
                   return App.LocalizedTimeToUniversalTime(local);
                }
            }

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

    #region Protected

      /// <summary>
      /// Adjusts date time Localized time
      /// </summary>
      protected DateTime? AdjustDate(DateTime? date)
      {
        if (!date.HasValue) return null;

        if (date.Value.Kind==DateTimeKind.Utc)
         return this.UniversalTimeToLocalizedTime(date.Value);

        return date;
      }

      /// <summary>
      /// Invoked by timer, checks all conditions and fires/expires event depending on the status.
      /// Returns true if event was fired, false otherwise
      /// </summary>
      protected internal bool VisitAndCheck(DateTime utcNow)
      {
        if (Disposed) return false;

        bool fire = false;
        lock(m_Lock)
        {
          if (!this.m_Enabled) return false;
          var localNow = this.UniversalTimeToLocalizedTime(utcNow);
          fire = DoVisit(localNow);
          if (fire) Fire();
        }
        return fire;
      }

      /// <summary>
      /// Override to perform extra status checks
      /// </summary>
      protected virtual bool DoVisit(DateTime localNow)
      {
        //check validity
        if (
             (m_StartDate.HasValue &&
              m_EndDate.HasValue &&
              m_StartDate.Value>=m_EndDate.Value) ||
             (m_MaxCount<0)||
             (m_Interval==TimeSpan.Zero)
            )
        {
          setStatus(EventStatus.Invalid);
          return false;
        }

        if (m_EndDate.HasValue && localNow>=m_EndDate.Value)
        {
          setStatus(EventStatus.Expired);
          return false;
        }

        if (m_MaxCount>0 && m_CallCount>=m_MaxCount)
        {
          setStatus(EventStatus.Expired);
          return false;
        }

        if (m_StartDate.HasValue && localNow<m_StartDate.Value)
        {
          setStatus(EventStatus.NotStarted);
          return false;
        }

        setStatus(EventStatus.Started);

        return (localNow-m_LastCall)>=m_Interval;
      }



      protected virtual void DoFire()
      {
        var body = Body;//thread-safe ref swap
        if (body!=null) body(this);
        var ih = EventHandler;//thread-safe ref swap
        if (ih!=null) ih.EventHandlerBody(this);
      }

      protected virtual void DoHandleError(Exception error)
      {

      }

    #endregion


    #region .pvt .impl

      private void setStatus(EventStatus status)
      {
        EventStatus priorStatus;
        lock(m_Lock)
        {
          priorStatus = m_Status;
          if (priorStatus==status) return;
          m_Status = status;

          var schange = StatusChange;

          if (schange!=null)
          {
             schange(this, priorStatus);
             var ih = EventHandler;
             if (ih!=null) ih.EventStatusChange(this, priorStatus);
          }
        }
      }

      private void definitionChange(string changedParameterName)
      {
        var dchange = DefinitionChange;

        if (dchange!=null)
         dchange(this, changedParameterName);

        var ih = EventHandler;
        if (ih!=null) ih.EventDefinitionChange(this, changedParameterName);
      }


      private void fireBody()
      {
        try
        {
          DoFire();
          m_LastError = null;
        }
        catch(Exception error)
        {
          //set task error exception
          m_LastError = error;
          try
          {
            DoHandleError(error);
          }
          catch(Exception another)
          {
            App.Log.Write(new Message
            {
              Type = MessageType.CatastrophicError,
              Topic = CoreConsts.TIME_TOPIC,
              From = "{0}.{1}".Args(GetType().Name, "fireBody.DoHandleError()"),
              Text = "{0} caused DoHandleError() which leaked {1}".Args(error.ToMessageWithType(),
                                                                        another.ToMessageWithType()),
              Exception = another
            });
          }
        }
        finally
        {
          m_CallCount++;
          m_LastCall = LocalizedTime;
        }
      }

    #endregion

  }
}
