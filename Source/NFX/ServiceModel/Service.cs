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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;


using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Time;

namespace NFX.ServiceModel
{
    /// <summary>
    /// Represents a lightweight service that can be controlled by Start/SignalStop-like commands.
    /// This class serves a a base for various implementations (i.e. LogService) including their composites.
    /// This class is thread-safe
    /// </summary>
    public abstract class Service : ApplicationComponent, IService, ILocalizedTimeProvider
    {
        #region CONSTS
              public const string CONFIG_NAME_ATTR = "name";

        #endregion



        #region .ctor


                protected Service() : base()
                {
                }

                protected Service(object director) : base(director)
                {
                }

                protected override void Destructor()
                {
                    if (m_Disposing) return;

                    m_Disposing = true;
                    try
                    {
                        SignalStop();
                        WaitForCompleteStop();

                        base.Destructor();
                    }
                    finally
                    {
                      m_Disposing = false;
                    }
                }


        #endregion

        #region Private Fields

            private volatile bool m_Disposing;

            private object m_StatusLock = new object();

            private volatile ControlStatus m_Status;

            [Config]
            private string m_Name;

            [Config(TimeLocation.CONFIG_TIMELOCATION_SECTION)]
            private TimeLocation m_TimeLocation = new TimeLocation();

        #endregion


        #region Properties

            /// <summary>
            /// Checks whether the class is decorated with ApplicationDontAutoStartServiceAttribute
            /// </summary>
            public bool ApplicationDontAutoStartService
            {
              get{ return Attribute.IsDefined(GetType(), typeof(ApplicationDontAutoStartServiceAttribute));}
            }

            /// <summary>
            /// Current service status
            /// </summary>
            public ControlStatus Status
            {
                get { return m_Status; }
            }

            /// <summary>
            /// Returns true when service is active or about to become active.
            /// Check in service implementation loops/threads/tasks
            /// </summary>
            public bool Running
            {
                get { return m_Status == ControlStatus.Active || m_Status == ControlStatus.Starting; }
            }

            /// <summary>
            /// Provides textual name for the service
            /// </summary>
            public string Name
            {
                get { return m_Name ?? this.GetType().Name; }
                protected set { m_Name = value; }
            }

            /// <summary>
            /// Returns time location of this LocalizedTimeProvider implementation
            /// </summary>
            public TimeLocation TimeLocation
            {
                get { return m_TimeLocation ?? TimeLocation.Parent;}
                set { m_TimeLocation = value; }
            }

            /// <summary>
            /// Returns current time localized per TimeLocation
            /// </summary>
            public DateTime LocalizedTime
            {
                get { return UniversalTimeToLocalizedTime(App.TimeSource.UTCNow); }
            }


        #endregion

        #region Public

            /// <summary>
            /// Configures service from configuration node (and possibly it's sub-nodes)
            /// </summary>
            public void Configure(IConfigSectionNode fromNode)
            {
                EnsureObjectNotDisposed();
                lock (m_StatusLock)
                {
                    CheckServiceInactive();
                    ConfigAttribute.Apply(this, fromNode);
                    DoConfigure(fromNode);
                }
            }

            /// <summary>
            /// Blocking call that starts the service instance
            /// </summary>
            internal bool StartByApplication()
            {
                if (ApplicationDontAutoStartService) return false;
                Start();
                return true;
            }

            /// <summary>
            /// Blocking call that starts the service instance
            /// </summary>
            public void Start()
            {
                EnsureObjectNotDisposed();
                lock (m_StatusLock)
                    if (m_Status == ControlStatus.Inactive)
                    {
                        m_Status = ControlStatus.Starting;
                        try
                        {
                          Behavior.ApplyBehaviorAttributes(this);
                          DoStart();
                          m_Status = ControlStatus.Active;
                        }
                        catch
                        {
                          m_Status = ControlStatus.Inactive;
                          throw;
                        }
                    }
            }

            /// <summary>
            /// Non-blocking call that initiates the stopping of the service
            /// </summary>
            public void SignalStop()
            {
                lock (m_StatusLock)
                    if (m_Status == ControlStatus.Active)
                    {
                        m_Status = ControlStatus.Stopping;
                        DoSignalStop();
                    }
            }


            /// <summary>
            /// Non-blocking call that returns true when the service instance has completely stopped after SignalStop()
            /// </summary>
            public bool CheckForCompleteStop()
            {
                lock (m_StatusLock)
                {
                    if (m_Status == ControlStatus.Inactive) return true;

                    if (m_Status == ControlStatus.Stopping)
                        return DoCheckForCompleteStop();
                    else
                        return false;
                }
            }

            /// <summary>
            /// Blocks execution of current thread until this service has completely stopped
            /// </summary>
            public void WaitForCompleteStop()
            {
                lock (m_StatusLock)
                {
                    if (m_Status == ControlStatus.Inactive) return;

                    if (m_Status != ControlStatus.Stopping) SignalStop();

                    DoWaitForCompleteStop();

                    m_Status = ControlStatus.Inactive;
                }
            }


            /// <summary>
            /// Accepts a visit of a manager entity - this call is useful for periodic updates of service status,
            /// i.e.  when service does not have a thread of its own it can be periodically managed by some other service through this method.
            /// The default implementation of DoAcceptManagerVisit(object, DateTime) does nothing
            /// </summary>
            public void AcceptManagerVisit(object manager, DateTime managerNow)
            {
              if (!Running) return;
              //call rooted at non-virt method for future extensibility with state checks here
              EnsureObjectNotDisposed();
              DoAcceptManagerVisit(manager, managerNow);
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
                   return DateTime.SpecifyKind(utc + loc.UTCOffset, DateTimeKind.Local);
                }
                else
                {
                  if (ComponentDirector is ILocalizedTimeProvider)
                    return ((ILocalizedTimeProvider)ComponentDirector).UniversalTimeToLocalizedTime(utc);

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
                   return DateTime.SpecifyKind(local - loc.UTCOffset, DateTimeKind.Utc);
                }
                else
                {
                   if (ComponentDirector is ILocalizedTimeProvider)
                    return ((ILocalizedTimeProvider)ComponentDirector).LocalizedTimeToUniversalTime(local);

                  return App.LocalizedTimeToUniversalTime(local);
                }
            }



        #endregion


        #region Protected

            /// <summary>
            /// Allows to abort unsuccessful DoStart() overridden implementation.
            /// This method must be called from within DoStart()
            /// </summary>
            protected void AbortStart()
            {
                var trace = new StackTrace(1, false);

                if (!trace.GetFrames().Any(f => f.GetMethod().Name.Equals("DoStart", StringComparison.Ordinal)))
                    Debugging.Fail(
                        text:   "Service.AbortStart() must be called from within DoStart()",
                        action: DebugAction.ThrowAndLog);

                m_Status = ControlStatus.AbortingStart;
            }

            /// <summary>
            /// Provides implementation that starts the service
            /// </summary>
            protected virtual void DoStart()
            {
            }

            /// <summary>
            /// Provides implementation that signals service to stop. This is expected not to block
            /// </summary>
            protected virtual void DoSignalStop()
            {
            }

            /// <summary>
            /// Provides implementation for checking whether the service has completely stopped
            /// </summary>
            protected virtual bool DoCheckForCompleteStop()
            {
                return m_Status == ControlStatus.Inactive;
            }

            /// <summary>
            /// Provides implementation for a blocking call that returns only after a complete service stop
            /// </summary>
            protected virtual void DoWaitForCompleteStop()
            {
            }


            /// <summary>
            /// Provides implementation that configures service from configuration node (and possibly it's sub-nodes)
            /// </summary>
            protected virtual void DoConfigure(IConfigSectionNode node)
            {

            }

            /// <summary>
            /// Checks for service inactivity and throws exception if service is running (started, starting or stopping)
            /// </summary>
            protected void CheckServiceInactive()
            {
                if (Status!=ControlStatus.Inactive)
                throw new NFXException(StringConsts.SERVICE_INVALID_STATE + Name);
            }

            /// <summary>
            /// Checks for service activity and throws exception if service is not in ControlStatus.Active state
            /// </summary>
            protected void CheckServiceActive()
            {
                if (m_Status!=ControlStatus.Active)
                throw new NFXException(StringConsts.SERVICE_INVALID_STATE + Name);
            }

            /// <summary>
            /// Checks for service activity and throws exception if service is not in ControlStatus.Active state
            /// </summary>
            protected void CheckServiceActiveOrStarting()
            {
                if (m_Status!=ControlStatus.Active && m_Status!=ControlStatus.Starting)
                throw new NFXException(StringConsts.SERVICE_INVALID_STATE + Name);
            }


            /// <summary>
            /// Accepts a visit from external manager. Base implementation does nothing.
            ///  Override in services that need external management calls
            ///   to update their state periodically, i.e. when they don't have a thread on their own
            /// </summary>
            protected virtual void DoAcceptManagerVisit(object manager, DateTime managerNow)
            {

            }

        #endregion


        #region Private Utils


        #endregion


    }



    /// <summary>
    /// Represents service with typed ComponentDirector property
    /// </summary>
    public class Service<TDirector> : Service where TDirector : class
    {

        protected Service() : base()
        {
        }

        protected Service(TDirector director) : base(director)
        {
        }

        public new TDirector ComponentDirector
        {
            get { return (TDirector)base.ComponentDirector; }
        }

    }


}
