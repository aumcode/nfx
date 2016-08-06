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

using NFX.ApplicationModel;

namespace NFX
{
    /// <summary>
    /// Provides a shortcut access to app-global context NFX.ApplicationModel.ExecutionContext.Application.*
    /// </summary>
    public static class App
    {

       public static IApplication Instance
       {
          get { return ExecutionContext.Application; }
       }


     /// <summary>
     /// Returns current session, this is a shortuct to ExecutionContext.Session
     /// </summary>
     public static ISession Session
     {
        get { return ExecutionContext.Session; }
     }

     /// <summary>
     /// Returns unique identifier of this running instance
     /// </summary>
     public static Guid InstanceID
     {
        get { return Instance.InstanceID; }
     }

     /// <summary>
     /// Returns timestamp when application started as localized app time
     /// </summary>
     public static DateTime StartTime
     {
        get { return Instance.StartTime; }
     }


     /// <summary>
     /// Returns true when application container is active non-NOPApplication instance
     /// </summary>
     public static bool Available
     {
        get
        {
             var inst = Instance;
             return inst!=null &&
                    !(inst is NOPApplication) &&
                    inst.Active;
        }
     }


     /// <summary>
     /// Returns true when application instance is active and working. This property returns false as soon as application finalization starts on shutdown
     /// Use to exit long-running loops and such
     /// </summary>
     public static bool Active { get { return Instance.Active; } }


     /// <summary>
     /// References app log
     /// </summary>
     public static Log.ILog Log { get { return Instance.Log; } }

     /// <summary>
     /// References instrumentation for this application instance
     /// </summary>
     public static Instrumentation.IInstrumentation Instrumentation { get { return Instance.Instrumentation; } }

     /// <summary>
     /// References throttling for this application instance
     /// </summary>
     public static Throttling.IThrottling Throttling { get { return Instance.Throttling; } }

     /// <summary>
     /// References application configuration root
     /// </summary>
     public static Environment.ConfigSectionNode  ConfigRoot { get { return Instance.ConfigRoot; } }

     /// <summary>
     /// References application data store
     /// </summary>
     public static DataAccess.IDataStore DataStore { get { return Instance.DataStore; } }

     /// <summary>
     /// References object store that may be used to persist object graphs between volatile application shutdown cycles
     /// </summary>
     public static NFX.ApplicationModel.Volatile.IObjectStore ObjectStore { get { return Instance.ObjectStore; } }

     /// <summary>
     /// References glue implementation that may be used to "glue" remote instances/processes/contracts together
     /// </summary>
     public static Glue.IGlue Glue { get { return Instance.Glue; } }

     /// <summary>
     /// References security manager that performs user authentication based on passed credentials and other security-related global tasks
     /// </summary>
     public static Security.ISecurityManager SecurityManager { get { return Instance.SecurityManager; } }

     /// <summary>
     /// References time source - an entity that supplies local and UTC times. The concrete implementation
     ///  may elect to get accurate times from the network or other external precision time sources (i.e. NASA atomic clock)
     /// </summary>
     public static Time.ITimeSource TimeSource { get { return Instance.TimeSource;} }


     /// <summary>
     /// References event timer that maintains and runs scheduled Event instances
     /// </summary>
     public static Time.IEventTimer EventTimer { get { return Instance.EventTimer;} }


     /// <summary>
     /// Returns application name
     /// </summary>
     public static string Name { get { return Instance.Name;} }


     /// <summary>
     /// Returns the location
     /// </summary>
     public static Time.TimeLocation TimeLocation { get { return Instance.TimeLocation;} }


     /// <summary>
     /// Returns current time localized per TimeLocation
     /// </summary>
     public static DateTime LocalizedTime { get { return Instance.LocalizedTime; } }

     /// <summary>
     /// Converts universal time to local time as of TimeLocation property
     /// </summary>
     public static DateTime UniversalTimeToLocalizedTime(DateTime utc)
     {
       return Instance.UniversalTimeToLocalizedTime(utc);
     }

     /// <summary>
     /// Converts localized time to universal time as of TimeLocation property
     /// </summary>
     public static DateTime LocalizedTimeToUniversalTime(DateTime local)
     {
       return Instance.LocalizedTimeToUniversalTime(local);
     }


     /// <summary>
     /// Returns the current call context user
     /// </summary>
     public static Security.User CurrentCallUser
     {
       get {return NFX.ApplicationModel.ExecutionContext.Session.User; }
     }

    }
}
