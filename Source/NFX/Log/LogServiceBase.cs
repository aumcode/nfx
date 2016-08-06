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

using NFX.Log.Destinations;
using NFX.Environment;

using NFX.ServiceModel;
using System.Collections.Concurrent;

namespace NFX.Log
{


    /// <summary>
    /// Based class for implementing test and non-test logging services.
    /// Destinations may fail and the message will be failed-over into another destination in the same logger
    ///  as specified by 'failover' attribute of destination. This attribute is also present on service level.
    /// Cascading failover is not supported (failover of failovers). Another consideration is that messages
    ///  get sent into destinations synchronously by internal thread so specifying too many destinations may
    ///  limit overall LogService throughput. In complex scenarios consider using LogServiceDestination instead.
    /// </summary>
    public abstract class LogServiceBase : ServiceWithInstrumentationBase<object>, ILogImplementation
    {
        public class DestinationList : List<Destination> {}

        #region CONSTS
            internal const string CONFIG_DESTINATION_SECTION   = "destination";
            internal const string CONFIG_DEFAULT_FAILOVER_ATTR = "default-failover";
        #endregion


        #region .ctor

            /// <summary>
            /// Creates a new logging service instance
            /// </summary>
            protected LogServiceBase() : base(null) { ctor();  }


            /// <summary>
            /// Creates a new logging service instance
            /// </summary>
            protected LogServiceBase(Service director = null) : base(director) { ctor(); }

            private void ctor()
            {
              m_InstrBuffer = new MemoryBufferDestination();
              m_InstrBuffer.__setLogSvc(this);
            }

            protected override void Destructor()
            {
                base.Destructor();

                foreach (Destination dest in m_Destinations)
                    dest.Dispose();

                m_InstrBuffer.Dispose();
            }

        #endregion


        #region Private Fields

            protected DestinationList m_Destinations = new DestinationList();

            private int             MAX_NESTED_FAILURES = 8;
            private int             m_NestedFailureCount;
            private string          m_DefaultFailover;

            private Destination     m_FailoverErrorDestination;
            private Exception       m_FailoverError;

            private Message m_LastWarning;
            private Message m_LastError;
            private Message m_LastCatastrophy;

            protected bool m_InstrumentationEnabled;

            private MemoryBufferDestination m_InstrBuffer;

        #endregion


        #region Properties

            public override string ComponentCommonName { get { return "log"; }}

            /// <summary>
            /// Latches last problematic msg
            /// </summary>
            public Message LastWarning     { get {return m_LastWarning;}}

            /// <summary>
            /// Latches last problematic msg
            /// </summary>
            public Message LastError       { get {return m_LastError;}}

            /// <summary>
            /// Latches last problematic msg
            /// </summary>
            public Message LastCatastrophy { get {return m_LastCatastrophy;}}

            /// <summary>
            /// Returns registered destinations. This call is thread safe
            /// </summary>
            public IEnumerable<Destination> Destinations
            {
                get { lock (m_Destinations) return m_Destinations.ToList(); }
            }


            /// <summary>
            /// Implements IInstrumentable
            /// </summary>
            [Config(Default=false)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public override bool InstrumentationEnabled
            {
              get { return m_InstrumentationEnabled;}
              set { m_InstrumentationEnabled = value;}
            }

            [Config]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public int InstrumentationBufferSize
            {
              get { return m_InstrBuffer.BufferSize; }
              set { m_InstrBuffer.BufferSize = value; }
            }

            /// <summary>
            /// Sets destination name used for failover on the service-level
            /// if particular failing destination did not specify its specific failover
            /// </summary>
            [Config("$" + CONFIG_DEFAULT_FAILOVER_ATTR)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
            public string DefaultFailover
            {
                get { return m_DefaultFailover ?? string.Empty; }
                set { m_DefaultFailover = value; }
            }

            /// <summary>
            /// Returns a destination that threw last exception that happened durng failover. This kind of exceptions is never propagated and always handled
            /// </summary>
            public Destination FailoverErrorDestination { get { return m_FailoverErrorDestination; } }

            /// <summary>
            /// Returns last exception that happened during failover. This kind of exceptions is never propagated and always handled
            /// </summary>
            public Exception FailoverError { get { return m_FailoverError; } }

            /// <summary>
            /// Returns localized log time
            /// </summary>
            public DateTime Now { get { return this.LocalizedTime; } }


            /// <summary>
            /// Indicates whether the service can operate without any destinations registered, i.e. some test loggers may not need
            ///  any destinations to operate as they synchronously write to some buffer without any extra destinations
            /// </summary>
            public virtual bool DestinationsAreOptional
            {
              get{ return false;}
            }

        #endregion

        #region Public

            /// <summary>
            /// Writes log message into log
            /// </summary>
            public void Write(MessageType type, string text, string topic = null, string from = null)
            {
                Write(type, text, false, topic, from);
            }

            /// <summary>
            /// Writes log message into log
            /// </summary>
            public void Write(MessageType type, string text, bool urgent, string topic = null, string from = null)
            {
                Write(new Message
                {
                    Type = type,
                    Topic = topic,
                    From = from,
                    Text = text
                }, urgent);
            }

            /// <summary>
            /// Writes log message into log
            /// </summary>
            /// <param name="msg">Message to write</param>
            public void Write(Message msg)
            {
                Write(msg, false);
            }

            /// <summary>
            /// Writes log message into log
            /// </summary>
            /// <param name="msg">Message to write</param>
            /// <param name="urgent">Indicates that the logging service implementation must
            /// make an effort to write the message to its destinations urgently</param>
            public void Write(Message msg, bool urgent)
            {
                if (Status != ControlStatus.Active) return;


                if (msg==null) return;

                if (msg.Type>=MessageType.Emergency) m_LastCatastrophy = msg;
                else
                if (msg.Type>=MessageType.Error) m_LastError = msg;
                else
                if (msg.Type>=MessageType.Warning) m_LastWarning = msg;

                if (m_InstrumentationEnabled) m_InstrBuffer.Send( msg);

                DoWrite(msg, urgent);
            }

            /// <summary>
            /// Adds a destination to this service active destinations. Negative index to append
            /// </summary>
            public void RegisterDestination(Destination dest, int atIdx = -1)
            {
                if (dest == null) return;

                lock (m_Destinations)
                {
                    if (m_Destinations.Count==0 || atIdx<0 || atIdx > m_Destinations.Count)
                     m_Destinations.Add(dest);
                    else
                     m_Destinations.Insert(atIdx, dest);
                    dest.__setLogSvc(this);
                }
            }

            /// <summary>
            /// Removes a destiantion from this service active destinations, returns true if destination was found and removed
            /// </summary>
            public bool UnRegisterDestination(Destination dest)
            {
                if (dest == null) return false;

                lock (m_Destinations)
                {
                    bool r = m_Destinations.Remove(dest);
                    if (r) dest.__setLogSvc(null);
                    return r;
                }
            }

            /// <summary>
            /// Notifies log that time changed and log destinations should be notified.
            /// Usually this causes log file close/open under different name
            /// </summary>
            public void TimeChanged()
            {
                lock (m_Destinations)
                {
                    foreach (Destination d in m_Destinations)
                        d.TimeChanged();
                }
            }

            /// <summary>
            /// Returns instrumentation buffer if instrumentation enabled
            /// </summary>
            public IEnumerable<Message> GetInstrumentationBuffer(bool asc)
            {
              return asc ? m_InstrBuffer.BufferedTimeAscending : m_InstrBuffer.BufferedTimeDescending;
            }

        #endregion


        #region Protected

            /// <summary>
            /// Writes log message into log
            /// </summary>
            /// <param name="msg">Message to write</param>
            /// <param name="urgent">Indicates that the logging service implementation must
            /// make an effort to write the message to its destinations urgently</param>
            protected abstract void DoWrite(Message msg, bool urgent);

            protected override void DoConfigure(IConfigSectionNode node)
            {
                base.DoConfigure(node);

                foreach (ConfigSectionNode dnode in
                    node.Children.Where(n => n.IsSameName(CONFIG_DESTINATION_SECTION)))
                {
                    Destination dest = FactoryUtils.MakeAndConfigure<Destination>(dnode);
                    this.RegisterDestination(dest);
                }
            }

            protected override void DoStart()
            {
                base.DoStart();

                lock (m_Destinations)
                {
                    if (!DestinationsAreOptional && m_Destinations.Count == 0)
                        throw new NFXException(StringConsts.LOGSVC_NODESTINATIONS_ERROR);

                    foreach (Destination dest in m_Destinations)
                        try
                        {
                            dest.Open();
                        }
                        catch (Exception error)
                        {
                            throw new NFXException(
                                StringConsts.LOGSVC_DESTINATION_OPEN_ERROR.Args(Name, dest.Name, dest.TestOnStart, error.Message),
                                error);
                        }
                }
            }

            protected override void DoWaitForCompleteStop()
            {
                base.DoWaitForCompleteStop();

                foreach (Destination dest in m_Destinations)
                    try { dest.Close(); } catch {}  // Can't do much here in case of an error
            }

            protected void Pulse()
            {
                lock (m_Destinations)
                    foreach (Destination destination in m_Destinations)
                        destination.Pulse();
            }

            /// <summary>
            /// When error=null => error cleared. When msg==null => exceptions surfaced from DoPulse()
            /// </summary>
            internal void FailoverDestination(Destination destination, Exception error, Message msg)
            {
              if (m_NestedFailureCount>=MAX_NESTED_FAILURES) return;//stop cascade recursion

              m_NestedFailureCount++;
              try
              {
                      if (error==null)//error lifted
                      {
                        if (destination==m_FailoverErrorDestination)
                        {
                          m_FailoverErrorDestination = null;
                          m_FailoverError = null;
                        }
                        return;
                      }

                      if (msg==null) return; //i.e. OnPulse()

                      var failoverName = destination.Failover;
                      if (string.IsNullOrEmpty(failoverName))
                         failoverName = this.DefaultFailover;
                      if (string.IsNullOrEmpty(failoverName))  return;//nowhere to failover

                      Destination failover = null;
                      lock(m_Destinations)
                         failover = m_Destinations.FirstOrDefault(d => string.Equals(d.Name , failoverName, StringComparison.InvariantCultureIgnoreCase));


                      if (failover==null) return;

                      if (failover==destination) return;//circular reference, cant fail into destination that failed

                      try
                      {
                        failover.SendRegularAndFailures(msg);

                          if (destination.GenerateFailoverMessages || failover.GenerateFailoverMessages)
                          {
                            var emsg = new Message();
                            emsg.Type = MessageType.Error;
                            emsg.From = destination.Name;
                            emsg.Topic = CoreConsts.LOGSVC_FAILOVER_TOPIC;
                            emsg.Text = string.Format(
                                   StringConsts.LOGSVC_FAILOVER_MSG_TEXT,
                                   msg.Guid,
                                   destination.Name,
                                   failover.Name,
                                   destination.AverageProcessingTimeMs);
                            emsg.RelatedTo = msg.Guid;
                            emsg.Exception = error;


                            failover.SendRegularAndFailures(emsg);
                          }

                        failover.DoPulse();
                      }
                      catch(Exception failoverError)
                      {
                        m_FailoverErrorDestination = failover;
                        m_FailoverError = failoverError;
                      }
              }
              finally
              {
                m_NestedFailureCount--;
              }

            }

        #endregion
    }
}
