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

using NFX.Log;
using NFX.Instrumentation;
using NFX.Environment;
using NFX.DataAccess;
using NFX.ApplicationModel.Volatile;
using NFX.Throttling;
using NFX.Glue;
using NFX.Security;
using NFX.Time;

namespace NFX.ApplicationModel
{
  /// <summary>
  /// Represents an application that consists of pure-nop providers, consequently
  ///  this application does not log, does not store data and does not do anything else
  /// still satisfying its contract
  /// </summary>
  public class NOPApplication : IApplication
  {

     private static NOPApplication s_Instance = new NOPApplication();

     protected NOPApplication()
     {
        ApplicationModel.ExecutionContext.__SetApplicationLevelContext(this, null, null, NOPSession.Instance);

        m_Configuration = new MemoryConfiguration();
        m_Configuration.Create();

        m_StartTime = DateTime.Now;
     }



     /// <summary>
     /// Returns a singlelton instance of the NOPApplication
     /// </summary>
     public static NOPApplication Instance
     {
       get { return s_Instance; }
     }


    private Guid m_InstanceID = Guid.NewGuid();
    private DateTime m_StartTime;
    protected MemoryConfiguration m_Configuration;



    #region IApplication Members


        public bool IsUnitTest{ get{ return false;}}

        public Guid InstanceID
        {
            get { return m_InstanceID; }
        }

        public DateTime StartTime
        {
            get { return m_StartTime; }
        }

        public bool Active
        {
          get { return false;}//20140128 DKh was true before
        }

        public bool Stopping
        {
          get { return false;}
        }

        public bool ShutdownStarted
        {
          get { return false;}
        }

        public string Name
        {
          get { return GetType().FullName; }
        }

        public ILog Log
        {
          get { return NOPLog.Instance; }
        }

        public IInstrumentation Instrumentation
        {
          get { return NOPInstrumentation.Instance; }
        }

        public IThrottling Throttling
        {
            get { return NOPThrottling.Instance; }
        }

        public ConfigSectionNode ConfigRoot
        {
          get { return m_Configuration.Root; }
        }

        public IDataStore DataStore
        {
          get { return NOPDataStore.Instance; }
        }

        public IObjectStore ObjectStore
        {
          get { return NOPObjectStore.Instance; }
        }

        public IGlue Glue
        {
          get { return NOPGlue.Instance; }
        }

        public ISecurityManager SecurityManager
        {
          get { return NOPSecurityManager.Instance; }
        }


        public ITimeSource TimeSource
        {
            get { return DefaultTimeSource.Instance; }
        }


        public IEventTimer EventTimer
        {
            get { return NOPEventTimer.Instance; }
        }


        public TimeLocation TimeLocation
        {
            get { return TimeLocation.Parent; }
        }

        public DateTime LocalizedTime
        {
            get { return TimeSource.Now; }
        }

        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            return TimeSource.UniversalTimeToLocalizedTime(utc);
        }

        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            return TimeSource.LocalizedTimeToUniversalTime(local);
        }

        public ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
        {
            return NOPSession.Instance;
        }

        public bool RegisterConfigSettings(IConfigSettings settings)
        {
            return false;
        }

        public bool UnregisterConfigSettings(IConfigSettings settings)
        {
            return false;
        }

        public void NotifyAllConfigSettingsAboutChange()
        {

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

    #endregion









  }
}
