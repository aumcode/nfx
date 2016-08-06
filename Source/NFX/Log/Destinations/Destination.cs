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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.3  2009.10.12
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Time;
using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Log.Destinations
{
  /// <summary>
  /// Delegate for message filtering
  /// </summary>
  public delegate bool MessageFilterHandler(Destination destination, Message msg);

  /// <summary>
  /// Represents logging message destination - an abstract entity that messages are written to by LogService.
  /// Destinations must be efficient as they block logger thread. They provide failover mechanism when
  ///  processing can not be completed. Once failed, the processing can try to be resumed after configurable interval.
  /// Destinations also provide optional SLA on the time it takes to perform actual message write - once exceeded destination is considered to have failed.
  /// Basic efficient filtering is provided for times, dates and levels. Complex C# expression-based filtering is also supported
  /// </summary>
  public abstract class Destination : ApplicationComponent, IConfigurable, IExternallyParameterized
  {
    #region Local Classes

        public class LevelsList : List<Tuple<MessageType,MessageType>> {}

    #endregion

    #region CONSTS
        public const string CONFIG_NAME_ATTR = "name";
        public const string CONFIG_FAILOVER_ATTR = "failover";
        public const string CONFIG_GENERATE_FAILOVER_MSG_ATTR = "generate-failover-msg";
        public const string CONFIG_ONLY_FAILURES_ATTR = "only-failures";
        public const string CONFIG_MIN_LEVEL_ATTR = "min-level";
        public const string CONFIG_MAX_LEVEL_ATTR = "max-level";
        public const string CONFIG_LEVELS_ATTR = "levels";
        public const string CONFIG_DAYS_OF_WEEK_ATTR = "days-of-week";
        public const string CONFIG_START_DATE_ATTR = "start-date";
        public const string CONFIG_END_DATE_ATTR = "end-date";
        public const string CONFIG_START_TIME_ATTR = "start-time";
        public const string CONFIG_END_TIME_ATTR = "end-time";
        public const string CONFIG_FILTER_ATTR = "filter";
        public const string CONFIG_TEST_ON_START_ATTR = "test-on-start";
        public const string CONFIG_NAME_DEFAULT = "Un-named log destination";

        public const string CONFIG_MAX_PROCESSING_TIME_MS_ATTR = "max-processing-time-ms";
        public const int CONFIG_MAX_PROCESSING_TIME_MS_MIN_VALUE = 25;

        public const string CONFIG_RESTART_PROCESSING_AFTER_MS_ATTR = "restart-processing-after-ms";
        public const int CONFIG_RESTART_PROCESSING_AFTER_MS_DEFAULT = 60000;

        /// <summary>
        /// Defines how much smoothing the processing time filter does - the lower the number the more smoothing is done.
        /// Smoothing makes MaxProcessingTimeMs detection insensitive to some seldom delays that may happen every now and then
        /// while destination performs actual write into its sink
        /// </summary>
        public const float PROCESSING_TIME_EMA_FILTER = 0.0007f;

    #endregion

    #region .ctor

        public Destination() : this(null) {}

        public Destination(string name)
        {
          m_Name = name;
          m_Levels = new LevelsList();
        }

        protected override void Destructor()
        {
          Close();
          base.Destructor();
        }
    #endregion

    #region Pvt/Protected Fields

        protected internal CompositeDestination m_Owner;



        private Exception m_LastError;
        protected string m_Name;

        private DateTime? m_LastErrorTimestamp;

        private MessageFilterExpression m_Filter;

        private System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();

        private MessageType? m_MinLevel;
        private MessageType? m_MaxLevel;
        private LevelsList   m_Levels;
        private DaysOfWeek? m_DaysOfWeek;
        private DateTime? m_StartDate;
        private DateTime? m_EndDate;
        private TimeSpan? m_StartTime;
        private TimeSpan? m_EndTime;


        private bool m_GenerateFailoverMessages;
        private bool m_OnlyFailures;
        private string m_Failover;
        private bool m_TestOnStart;

        private int? m_MaxProcessingTimeMs;
        private float m_AverageProcessingTimeMs;
        private int m_RestartProcessingAfterMs = CONFIG_RESTART_PROCESSING_AFTER_MS_DEFAULT;


    #endregion


    #region Properties

        public LogServiceBase DirectorLog { get { return this.ComponentDirector as LogServiceBase; } }
        internal void __setLogSvc(LogServiceBase svc) { __setComponentDirector(svc); }

        /// <summary>
        /// References a log service that this destination services
        /// </summary>
        public LogServiceBase Service
        {
          get { return DirectorLog ?? (m_Owner != null ? m_Owner.Service : null); }
        }


        /// <summary>
        /// Returns a composite destination that ownes this destination or null
        /// </summary>
        public CompositeDestination Owner
        {
          get { return m_Owner;}
        }


        /// <summary>
        /// Provides mnemonic destination name
        /// </summary>
        [Config("$" + CONFIG_NAME_ATTR, CONFIG_NAME_DEFAULT)]
        public string Name
        {
          get { return m_Name ?? string.Empty; }
          set { m_Name = value; }
        }


        /// <summary>
        /// Gets/sets filter expression for this destination.
        /// Filter expressions get dynamically compiled into filter assembly,
        /// consequently it is not a good practice to create too many different filters.
        /// Filters are heavyweight, and it is advisable to use them ONLY WHEN regular destination filtering (using Min/Max levels, dates and times) can not be used
        ///  to achieve the desired result
        /// </summary>
        public MessageFilterExpression Filter
        {
          get { return m_Filter; }
          set { m_Filter = value; }
        }

        /// <summary>
        /// References message filtering method or null
        /// </summary>
        public MessageFilterHandler FilterMethod
        {
          get;
          set;
        }

        /// <summary>
        /// Returns last error that this destination has encountered
        /// </summary>
        public Exception LastError
        {
          get { return m_LastError; }
        }

        /// <summary>
        /// Returns last error timestamp (if any)
        /// </summary>
        private DateTime? LastErrorTimestamp
        {
          get { return m_LastErrorTimestamp; }
        }

        /// <summary>
        /// Imposes a minimum log level constraint
        /// </summary>
        [Config("$" + CONFIG_MIN_LEVEL_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public MessageType? MinLevel
        {
          get { return m_MinLevel; }
          set { m_MinLevel = value;}
        }

        /// <summary>
        /// Imposes a maximum log level constraint
        /// </summary>
        [Config("$" + CONFIG_MAX_LEVEL_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public MessageType? MaxLevel
        {
          get { return m_MaxLevel; }
          set { m_MaxLevel = value;}
        }

        public LevelsList Levels
        {
          get { return m_Levels; }
          set { m_Levels = m_Levels ?? new LevelsList(); }
        }

        /// <summary>
        /// Imposes a filter on days when this destination handles messages
        /// </summary>
        [Config("$" + CONFIG_DAYS_OF_WEEK_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public DaysOfWeek? DaysOfWeek
        {
          get { return m_DaysOfWeek; }
          set { m_DaysOfWeek = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the starting date and time
        /// after which this destination will start processing log messages
        /// </summary>
        [Config("$" + CONFIG_START_DATE_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public DateTime? StartDate
        {
          get { return m_StartDate; }
          set { m_StartDate = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the ending date and time
        /// before which this destination will be processing log messages
        /// </summary>
        [Config("$" + CONFIG_END_DATE_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public DateTime? EndDate
        {
          get { return m_EndDate; }
          set { m_EndDate = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the starting time of the day
        /// after which this destination will start processing log messages
        /// </summary>
        [Config("$" + CONFIG_START_TIME_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public TimeSpan? StartTime
        {
          get { return m_StartTime; }
          set { m_StartTime = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the ending time of the day
        /// before which this destination will be processing log messages
        /// </summary>
        [Config("$" + CONFIG_END_TIME_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public TimeSpan? EndTime
        {
          get { return m_EndTime; }
          set { m_EndTime = value;}
        }

        /// <summary>
        /// Indicates whether this destination should only process failures - messages that crashed other destinations.
        /// When set to true regular messages (dispatched by Send(msg)) are ignored
        /// </summary>
        [Config("$" + CONFIG_ONLY_FAILURES_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public bool OnlyFailures
        {
          get { return m_OnlyFailures; }
          set { m_OnlyFailures = value; }
        }


        /// <summary>
        /// Determines whether additional co-related error message should be generated when this destination fails or when it is
        ///  used as failover by some other destination. When this property is true an additional error message gets written into failover destination that
        ///   describes what message caused failure (error is co-related to original) at what destination. False by default.
        /// </summary>
        [Config("$" + CONFIG_GENERATE_FAILOVER_MSG_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public bool GenerateFailoverMessages
        {
          get { return m_GenerateFailoverMessages; }
          set { m_GenerateFailoverMessages = value;}
        }



        /// <summary>
        /// Sets destination name used for failover of this one
        /// </summary>
        [Config("$" + CONFIG_FAILOVER_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public string Failover
        {
          get { return m_Failover ?? string.Empty; }
          set { m_Failover = value; }
        }


        /// <summary>
        /// Indicates whether this destination should try to test the underlying sink on startup.
        /// For example DB-based destinations will try to connect to server upon log service launch when this property is true
        /// </summary>
        [Config("$" + CONFIG_TEST_ON_START_ATTR)]
        public bool TestOnStart
        {
          get { return m_TestOnStart; }
          set { m_TestOnStart = value;}
        }


        /// <summary>
        /// Imposes a time limit on internal message processing (writing into actual sink) by this destination.
        /// If this limit is exceeded, this destination fails and processing is re-tried to be resumed after RestartProcessingAfterMs interval.
        /// The minimum value for this property is 25 ms as lower values compromise timer accuracy
        /// </summary>
        [Config("$" + CONFIG_MAX_PROCESSING_TIME_MS_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public int? MaxProcessingTimeMs
        {
          get { return m_MaxProcessingTimeMs; }
          set
          {
           if (value.HasValue)
           {
            m_MaxProcessingTimeMs = value > CONFIG_MAX_PROCESSING_TIME_MS_MIN_VALUE ? value : CONFIG_MAX_PROCESSING_TIME_MS_MIN_VALUE;
            m_AverageProcessingTimeMs = 0f;
           }
           else
            m_MaxProcessingTimeMs = null;
          }
        }

        /// <summary>
        /// Returns average time it takes destination implementation to write the log message to actual sink.
        /// This property is only computed when MaxProcessingTimeMs limit is imposed, otherwise it returns 0f
        /// </summary>
        public float AverageProcessingTimeMs
        {
          get { return m_AverageProcessingTimeMs; }
        }


        /// <summary>
        /// Specifies how much time must pass before processing will be tried to resume after failure.
        /// The default value is 60000 ms
        /// </summary>
        [Config("$" + CONFIG_RESTART_PROCESSING_AFTER_MS_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public int RestartProcessingAfterMs
        {
          get { return m_RestartProcessingAfterMs; }
          set { m_RestartProcessingAfterMs = value; }
        }


            /// <summary>
            /// Returns named parameters that can be used to control this component
            /// </summary>
            public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

            /// <summary>
            /// Returns named parameters that can be used to control this component
            /// </summary>
            public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
            {
              return ExternalParameterAttribute.GetParameters(this, groups);
            }


    #endregion


    #region Public

        /// <summary>
        /// Configures specified destination
        /// </summary>
        public void Configure(IConfigSectionNode fromNode)
        {
          DoConfigure(fromNode);
          ConfigAttribute.Apply(this, fromNode);
        }


        /// <summary>
        /// Activates destination by preparing it to start operation
        /// </summary>
        public virtual void Open()
        {

        }

        /// <summary>
        /// Deactivates destination
        /// </summary>
        public virtual void Close()
        {

        }

        /// <summary>
        /// Allows to insert a destination right before this one
        /// </summary>
        public volatile Destination Before;

        /// <summary>
        /// Allows to insert a destination right after this one
        /// </summary>
        public volatile Destination After;


        /// <summary>
        /// Sends the message into destination doing filter checks first.
        /// </summary>
        public void Send(Message msg)
        {
          if (m_OnlyFailures) return;

          SendRegularAndFailures(msg);
        }

        internal void SendRegularAndFailures(Message msg)
        {
          //When there was failure and it was not long enough
          if (m_LastErrorTimestamp.HasValue)
           if ((DirectorLog.Now - m_LastErrorTimestamp.Value).TotalMilliseconds < m_RestartProcessingAfterMs)
           {
             //this is afster than throwing exception
             var error = new NFXException(string.Format(StringConsts.LOGSVC_DESTINATION_IS_OFFLINE_ERROR, Name));
             SetError(error, msg);
             return;
           }

          try
          {

            if (!satisfyFilter(msg)) return;

            if (m_Levels.Count > 0)
            {
                bool found = false;
                foreach (var r in m_Levels)
                    if (r.Item1 <= msg.Type && msg.Type <= r.Item2)
                    {
                        found = true;
                        break;
                    }
                 if (!found)
                     return;
            }

            //to avoid possible thread collisions
            var before = Before;
            var after = After;

            if (
                (!m_MinLevel.HasValue   || msg.Type >= m_MinLevel.Value) &&
                (!m_MaxLevel.HasValue   || msg.Type <= m_MaxLevel.Value) &&
                (!m_DaysOfWeek.HasValue || m_DaysOfWeek.Value.Contains(msg.TimeStamp.DayOfWeek)) &&
                (!m_StartDate.HasValue  || msg.TimeStamp >= m_StartDate.Value) &&
                (!m_EndDate.HasValue    || msg.TimeStamp <= m_EndDate.Value) &&
                (!m_StartTime.HasValue  || msg.TimeStamp.TimeOfDay >= m_StartTime.Value) &&
                (!m_EndTime.HasValue    || msg.TimeStamp.TimeOfDay <= m_EndTime.Value)
               )
            {
                if (before != null) before.Send(msg);

                if (!m_MaxProcessingTimeMs.HasValue)
                 DoSend(msg);
                else
                {
                   m_StopWatch.Restart();
                    DoSend(msg);
                   m_StopWatch.Stop();

                   if (m_LastError != null) m_AverageProcessingTimeMs = 0f;//reset average time to 0 after 1st successfull execution after prior failure

                   //EMA filter
                   m_AverageProcessingTimeMs = ( PROCESSING_TIME_EMA_FILTER * m_StopWatch.ElapsedMilliseconds) +
                                               ( (1.0f - PROCESSING_TIME_EMA_FILTER) * m_AverageProcessingTimeMs );

                   if (m_AverageProcessingTimeMs > m_MaxProcessingTimeMs.Value)
                    throw new NFXException(string.Format(StringConsts.LOGSVC_DESTINATION_EXCEEDS_MAX_PROCESSING_TIME_ERROR,
                                                         Name,
                                                         m_MaxProcessingTimeMs,
                                                         m_StopWatch.ElapsedMilliseconds));
                }

                if (after != null) after.Send(msg);

                if (m_LastError != null) SetError(null, msg);//clear-out error
            }

          }
          catch (Exception error)
          {
            //WARNING!!!
            //under no condition MAY any exception escape from here
            SetError(error, msg);
          }
        }

        /// <summary>
        /// Provides periodic notification of destinations from central Log thread even if there are no messages to write.
        /// Override DoPulse to commit internal batching buffers provided by particular destinations
        /// </summary>
        public void Pulse()
        {
           try
           {
              DoPulse();
           }
           catch (Exception error)
           {
            //WARNING!!!
            //under no condition MAY any exception escape from here
            SetError(error, null);
           }
        }



        /// <summary>
        /// Notifies destination about pivotal time change (i.e. every 12 hrs).
        /// Implementors may choose to create a different log file or catalog etc.
        /// </summary>
        public virtual void TimeChanged()
        {

        }

        /// <summary>
        /// Notifies destination about fundamental settings change that may need to create a differently named file,
        ///  database or other storage medium
        /// </summary>
        public virtual void SettingsChanged()
        {

        }

        /// <summary>
        /// Parses levels into a tuple list of level ranges
        /// </summary>
        /// <param name="levels">String representation of levels using ',' or ';' or '|'
        /// as range group delimiters, and '-' as range indicators.  If first/second bound of the range
        /// is empty, the min/max value of that bound is assumed.
        /// Examples: "Debug-DebugZ | Error", "-DebugZ | Info | Warning", "Info-", "DebugB-DebugC, Error"</param>
        public static LevelsList ParseLevels(string levels)
        {
            var result = new LevelsList();

            if (!string.IsNullOrWhiteSpace(levels))
                foreach (var p in levels.Split(',', ';', '|'))
                {
                    var minmax = p.Split(new char[] { '-' }, 2).Select(s => s.Trim()).ToArray();

                    if (minmax.Length == 0)
                        throw new NFXException(StringConsts.INVALID_ARGUMENT_ERROR + "levels: " + p);

                    MessageType min, max;

                    if (string.IsNullOrWhiteSpace(minmax[0]))
                        min = MessageType.Debug;
                    else if (!Enum.TryParse(minmax[0], true, out min))
                        throw new NFXException(StringConsts.INVALID_ARGUMENT_ERROR +
                            "levels: {0} (error parsing: {1})".Args(p, minmax[0]));

                    if (minmax.Length < 2)
                        max = min;
                    else if (string.IsNullOrWhiteSpace(minmax[1]))
                        max = MessageType.CatastrophicError;
                    else if (!Enum.TryParse(minmax[1], true, out max))
                        throw new NFXException(StringConsts.INVALID_ARGUMENT_ERROR +
                            "levels: {0} (error parsing: {1})".Args(p, minmax[1]));

                    result.Add(new Tuple<MessageType, MessageType>(min, max));
                }

            return result;
        }


            /// <summary>
            /// Gets external parameter value returning true if parameter was found
            /// </summary>
            public virtual bool ExternalGetParameter(string name, out object value, params string[] groups)
            {
                return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
            }

            /// <summary>
            /// Sets external parameter value returning true if parameter was found and set
            /// </summary>
            public virtual bool ExternalSetParameter(string name, object value, params string[] groups)
            {
              return ExternalParameterAttribute.SetParameter(this, name, value, groups);
            }

        #endregion


        #region Protected

        /// <summary>
        /// Override to perform derivative-specific configuration
        /// </summary>
        protected virtual void DoConfigure(IConfigSectionNode node)
        {
          var expr = node.AttrByName(CONFIG_FILTER_ATTR).Value;
          if (!string.IsNullOrWhiteSpace(expr))
           m_Filter = new MessageFilterExpression(expr);

          m_Levels = ParseLevels(node.AttrByName(CONFIG_LEVELS_ATTR).Value);
        }

        /// <summary>
        /// Notifies log service of exception that surfaced during processing of a particular message
        /// </summary>
        protected void SetError(Exception error, Message msg)
        {
          if (error != null)
          {
            DirectorLog.FailoverDestination(this, error, msg);
            m_LastError = error;
            m_LastErrorTimestamp = DirectorLog.Now;
          }
          else
          {
            m_LastError = null;
            DirectorLog.FailoverDestination(this, null, null);
            m_LastErrorTimestamp = null;
          }
        }

        /// <summary>
        /// Performs physical send, i.e. storage in file for FileDestinations
        /// </summary>
        protected internal abstract void DoSend(Message entry);

        /// <summary>
        /// Provides periodic notification of destinations from central Log thread even if there are no messages to write.
        /// Override to commit internal batching buffers provided by particular destinations
        /// </summary>
        protected internal virtual void DoPulse()
        {

        }

    #endregion


    #region Private Utils

        private bool satisfyFilter(Message msg)
        {
          //to avoid possible thread collisions
          var mf = FilterMethod;
          var fe = m_Filter;

          if (mf != null)
             if (!mf(this, msg)) return false;

          if (fe!=null)
           return fe.Evaluate(this, msg);

          return true;
        }

    #endregion
  }

}
