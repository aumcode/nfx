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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using NFX.Time;
using NFX.ApplicationModel.Volatile;
using NFX.DataAccess;
using NFX.Environment;
using NFX.Glue;
using NFX.Instrumentation;
using NFX.Log;
using NFX.Security;
using NFX.Throttling;

namespace NFX.ApplicationModel
{
    /// <summary>
    /// Application designated for use in various unit test cases.
    /// This class is not intended for use in production systems!
    /// </summary>
    public class TestApplication : DisposableObject, IApplication
    {
        protected Guid m_InstanceID = Guid.NewGuid();
        protected List<IConfigSettings> m_ConfigSettings = new List<IConfigSettings>();
        protected ConfigSectionNode m_ConfigRoot;

        public TestApplication(ConfigSectionNode cfgRoot = null)
        {
            this.ConfigRoot = cfgRoot;

            Active = true;
            StartTime = DateTime.Now;
            Log = NOPLog.Instance;
            Instrumentation = NOPInstrumentation.Instance;
            Throttling = NOPThrottling.Instance;
            DataStore = NOPDataStore.Instance;
            ObjectStore = NOPObjectStore.Instance;
            Glue = NOPGlue.Instance;
            SecurityManager = NOPSecurityManager.Instance;
            TimeSource = NFX.Time.DefaultTimeSource.Instance;
            TimeLocation = new Time.TimeLocation();
            EventTimer = NFX.Time.NOPEventTimer.Instance;

            ApplicationModel.ExecutionContext.__SetApplicationLevelContext(this, null, null, NOPSession.Instance);
        }

        public virtual bool IsUnitTest { get; set; }

        public virtual Guid InstanceID { get { return m_InstanceID;}}

        public virtual DateTime StartTime { get; set;}

        public virtual bool Active { get; set; }

        public virtual bool Stopping { get; set; }

        public virtual bool ShutdownStarted { get; set; }

        public virtual Log.ILog Log { get; set; }

        public virtual Instrumentation.IInstrumentation Instrumentation { get; set; }

        public virtual Throttling.IThrottling Throttling { get; set; }

        public virtual Environment.ConfigSectionNode ConfigRoot
        {
           get{return m_ConfigRoot;}
           set
           {
             if (value==null)
             {
                var conf = new MemoryConfiguration();
                conf.Create();
                value = conf.Root;
             }
             m_ConfigRoot = value;
           }
         }

        public virtual DataAccess.IDataStore DataStore { get; set; }

        public virtual Volatile.IObjectStore ObjectStore { get; set; }

        public virtual Glue.IGlue Glue { get; set; }

        public virtual Security.ISecurityManager SecurityManager { get; set; }

        public virtual Time.ITimeSource TimeSource { get; set; }

        public virtual Time.IEventTimer EventTimer { get; set; }

        public virtual ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
        {
            return NOPSession.Instance;
        }

        /// <summary>
        /// Registers an instance of IConfigSettings with application container to receive a call when
        ///  underlying app configuration changes
        /// </summary>
        public virtual bool RegisterConfigSettings(IConfigSettings settings)
        {
            lock (m_ConfigSettings)
                if (!m_ConfigSettings.Contains(settings, ReferenceEqualityComparer<IConfigSettings>.Instance))
                {
                    m_ConfigSettings.Add(settings);
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Removes the registration of IConfigSettings from application container
        /// </summary>
        /// <returns>True if settings instance was found and removed</returns>
        public virtual bool UnregisterConfigSettings(IConfigSettings settings)
        {
            lock (m_ConfigSettings)
                return m_ConfigSettings.Remove(settings);
        }

        /// <summary>
        /// Forces notification of all registered IConfigSettings-implementers about configuration change
        /// </summary>
        public virtual void NotifyAllConfigSettingsAboutChange()
        {
            NotifyAllConfigSettingsAboutChange(m_ConfigRoot);
        }


        public virtual string Name { get; set; }

        public virtual Time.TimeLocation TimeLocation { get; set; }

        public virtual DateTime LocalizedTime
        {
            get { return UniversalTimeToLocalizedTime(TimeSource.UTCNow); }
            set { }
        }

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
                   return TimeSource.UniversalTimeToLocalizedTime(utc);
                }
            }

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
                   return TimeSource.LocalizedTimeToUniversalTime(local);
                }
            }

        /// <summary>
        /// Forces notification of all registered IConfigSettings-implementers about configuration change
        /// </summary>
        protected void NotifyAllConfigSettingsAboutChange(IConfigSectionNode node)
        {
            node = node ?? m_ConfigRoot;

            lock (m_ConfigSettings)
                foreach (var s in m_ConfigSettings) s.ConfigChanged(node);
        }


        public bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
        {
            return false;
        }

        public bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
        {
            return false;
        }

        public void Stop()
        {

        }
    }
}
