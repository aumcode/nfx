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
using System.Threading;

using NFX.Log.Destinations;
using NFX.Environment;

using NFX.ServiceModel;
using System.Collections.Concurrent;

namespace NFX.Log
{


  /// <summary>
  /// Provides logging services by buffering messages and dispatching them into destinations.
  /// This is an asynchronous service, meaning that Write(msg) never blocks for long.
  /// </summary>
  [ConfigMacroContext]
  public class LogService : LogServiceBase
  {
    #region CONSTS
        private const int MIN_INTERVAL_MSEC = 10;
        private const int DEFAULT_INTERVAL_MSEC = 250;
        private const int MAX_INTERVAL_MSEC = 10000;
        private const int THREAD_GRANULARITY_MSEC = 500;

        private const int INSTRUMENTATION_GRANULARITY_MSEC = 3971;

        private const string THREAD_NAME = "LogService Thread";

        public const string CONFIG_FILEEXTENSION_ATTR = "file-extension";
        public const string CONFIG_WRITEINTERVAL_ATTR = "write-interval-ms";

        public const string CONFIG_RELIABLE_ATTR = "reliable";
    #endregion


    #region .ctor

        /// <summary>
        /// Creates a new logging service instance
        /// </summary>
        public LogService() : base(null) {}


        /// <summary>
        /// Creates a new logging service instance
        /// </summary>
        public LogService(Service director = null) : base(director) {}

    #endregion


    #region Private Fields

        private ConcurrentQueue<Message> m_Queue = new ConcurrentQueue<Message>();
        private int m_QueuedCount = 0;
        private AutoResetEvent m_Wakeup;

        private int m_WriteInterval = DEFAULT_INTERVAL_MSEC;
        private Thread m_Thread;

        private string m_FileExtension;

        private bool m_Reliable = true;

    #endregion


    #region Properties

        /// <summary>
        /// Determines how often a log should be written to storage.
        /// The value of this property must be between 10 and 5000 (milliseconds)
        /// </summary>
        [Config("$" + CONFIG_WRITEINTERVAL_ATTR, DEFAULT_INTERVAL_MSEC)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public int WriteInterval
        {
          get { return m_WriteInterval; }
          set { m_WriteInterval = IntMath.MinMax(MIN_INTERVAL_MSEC, value, MAX_INTERVAL_MSEC); }
        }

        /// <summary>
        /// Extension for log files
        /// </summary>
        [Config("$" + CONFIG_FILEEXTENSION_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public string FileExtension
        {
          get
          {
            return m_FileExtension ?? string.Empty;
          }
          set
          {
            m_FileExtension = value;
          }
        }


        /// <summary>
        /// Determines whether this service blocks on stop longer until all buffered messages have been tried to be dispatched into all destinations.
        /// This property is true by default.
        /// Certain destinations may take considerable time to fail per message (i.e. database connection timeout), consequently buffered messages
        ///  processing may delay service stop significantly if this property is true
        /// </summary>
        [Config("$" + CONFIG_RELIABLE_ATTR, true)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public bool Reliable
        {
          get { return m_Reliable;}
          set { m_Reliable = value;}
        }

    #endregion

    #region Protected

        /// <summary>
        /// Writes log message into log
        /// </summary>
        protected override void DoWrite(Message msg, bool urgent)
        {
            m_Queue.Enqueue(msg);
            int n = Interlocked.Increment(ref m_QueuedCount);
            if (urgent && n == 1)
                m_Wakeup.Set();
        }

        protected override void DoStart()
        {
            try
            {
                base.DoStart();

                m_Wakeup = new AutoResetEvent(false);

                m_Thread = new Thread(threadSpin);
                m_Thread.Name = THREAD_NAME;
                m_Thread.Start();
            }
            catch
            {
                AbortStart();
                throw;
            }
        }

        protected override void DoWaitForCompleteStop()
        {
            m_Wakeup.Set();

            m_Thread.Join();
            m_Thread = null;

            m_Wakeup.Close();


            base.DoWaitForCompleteStop();
        }

    #endregion


    #region Write to destinations


        private Dictionary<MessageType, _stat> m_Stats = new Dictionary<MessageType,_stat>();
        private class _stat { public long V;}

        private void threadSpin()
        {
            DateTime lastInstr = this.Now;
            while (Running)
            {
                Interlocked.Exchange(ref m_QueuedCount, 0);
                write();
                Pulse();

                var now = this.Now;

                if (m_InstrumentationEnabled && (now-lastInstr).TotalMilliseconds > INSTRUMENTATION_GRANULARITY_MSEC)
                {
                 lastInstr = now;
                 dumpStats();
                }

                DateTime wakeupTime     = now.AddMilliseconds(m_WriteInterval);
                int      sleepInterval  = Math.Min(m_WriteInterval, THREAD_GRANULARITY_MSEC);

                do
                {
                    if (this.Status != ControlStatus.Active || !m_Queue.IsEmpty) break;
                    if (m_Wakeup.WaitOne(sleepInterval)) break;
                }
                while (this.Now < wakeupTime);
            }

            write(); //the rest
            Pulse();
        }


        private void write()
        {
            lock (m_Destinations) //the lock on destinations here is on purpose, so while write takes place no other thread can remove destinations
            {
                Message msg;
                while (m_Queue.TryDequeue(out msg))
                {
                    if (m_InstrumentationEnabled)
                    {
                      _stat s;
                      if (!m_Stats.TryGetValue(msg.Type, out s))
                      {
                       s = new _stat();
                       m_Stats[msg.Type] = s;
                      }
                      s.V++;
                    }

                    foreach (var destination in m_Destinations)
                    {
                        //20130318 DKh
                        if (!m_Reliable && !this.Running)
                            return;
                        destination.Send(msg);
                    }
                }
            }
        }

        private void dumpStats()
        {
           long total = 0;
           foreach(var kvp in m_Stats)
           {
             var count = kvp.Value.V;
             kvp.Value.V = 0;
             Instrumentation.LogMsgCount.Record(kvp.Key.ToString(), count);
             total += count;
           }

           Instrumentation.LogMsgCount.Record(Instrumentation.LogMsgCount.UNSPECIFIED_SOURCE, total);
        }

    #endregion
  }
}
