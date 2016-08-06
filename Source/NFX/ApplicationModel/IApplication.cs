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
 * Originated: 2011.01.31
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Time;

namespace NFX.ApplicationModel
{

  /// <summary>
  /// Describes general application model - usually a dependency injection container
  ///  that governs application initialization, state management, logging etc...
  /// An applications is usually implemented with a singleton class that has static
  ///  conduits to instance properties. Applications may be passed by reference (hence this interface)
  ///   to simplify mocking that otherwise (in case of static-only class) would have been impossible
  /// </summary>
  public interface IApplication : INamed, ILocalizedTimeProvider
  {
     /// <summary>
     /// True if app is launched as a unit test as set by the app config "unit-test=true"
     /// The general use of this flag is discouraged as code cnstructs should not form special cases just for unit testing,
     /// however in some cases this flag is usefull. It is not exposed via App. static accessors
     /// </summary>
     bool IsUnitTest{ get;}

     /// <summary>
     /// Returns unique identifier of this running instance
     /// </summary>
     Guid InstanceID { get; }

     /// <summary>
     /// Returns timestamp when application started as localized app time
     /// </summary>
     DateTime StartTime { get; }


     /// <summary>
     /// Returns true when application instance is active and working. This property returns false as soon as application finalization starts on shutdown or Stop() was called
     /// Use to exit long-running loops and such
     /// </summary>
     bool Active { get; }

     /// <summary>
     /// Returns true after Stop() was called
     /// </summary>
     bool Stopping { get;}

     /// <summary>
     /// Returns true after Dispose() was called to indicate that application is shutting down
     /// </summary>
     bool ShutdownStarted { get;}

     /// <summary>
     /// Initiates the stop of the application by setting its Stopping to true and Active to false so dependent services may start to terminate
     /// </summary>
     void Stop();


     /// <summary>
     /// References app log
     /// </summary>
     Log.ILog Log { get; }

     /// <summary>
     /// References instrumentation for this application instance
     /// </summary>
     Instrumentation.IInstrumentation Instrumentation { get; }

     /// <summary>
     /// References throttling for this application instance
     /// </summary>
     Throttling.IThrottling Throttling { get; }

     /// <summary>
     /// References application configuration root
     /// </summary>
     Environment.ConfigSectionNode  ConfigRoot { get; }

     /// <summary>
     /// References application data store
     /// </summary>
     DataAccess.IDataStore DataStore { get; }

     /// <summary>
     /// References object store that may be used to persist object graphs between volatile application shutdown cycles
     /// </summary>
     Volatile.IObjectStore ObjectStore { get; }

     /// <summary>
     /// References glue implementation that may be used to "glue" remote instances/processes/contracts together
     /// </summary>
     Glue.IGlue Glue { get; }

     /// <summary>
     /// References security manager that performs user authentication based on passed credentials and other security-related global tasks
     /// </summary>
     Security.ISecurityManager SecurityManager { get; }

     /// <summary>
     /// References time source - an entity that supplies local and UTC times. The concrete implementation
     ///  may elect to get accurate times from the network or other external precision time sources (i.e. NASA atomic clock)
     /// </summary>
     Time.ITimeSource TimeSource { get; }

     /// <summary>
     /// References event timer - an entity that maintains and runs scheduled instances of Event
     /// </summary>
     Time.IEventTimer EventTimer { get; }


     /// <summary>
     /// Factory method that creates new session object suitable for partcular application type
     /// </summary>
     /// <param name="sessionID">Session identifier</param>
     /// <param name="user">Optional user object that the session is for</param>
     /// <returns>New session object</returns>
     ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null);


     /// <summary>
     /// Registers an instance of IConfigSettings with application container to receive a call when
     ///  underlying app configuration changes
     /// </summary>
     /// <returns>True if settings instance was not found and was added</returns>
     bool RegisterConfigSettings(Environment.IConfigSettings settings);

     /// <summary>
     /// Removes the registration of IConfigSettings from application container
     /// </summary>
     /// <returns>True if settings instance was found and removed</returns>
     bool UnregisterConfigSettings(Environment.IConfigSettings settings);

     /// <summary>
     /// Forces notification of all registered IConfigSettings-implementers about configuration change
     /// </summary>
     void NotifyAllConfigSettingsAboutChange();

     /// <summary>
     /// Registers an instance of IApplicationFinishNotifiable with application container to receive a call when
     ///  underlying application instance will finish its lifecycle.
     /// </summary>
     /// <returns>True if notifiable instance was not found and was added</returns>
     bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable);

     /// <summary>
     /// Removes the registration of IApplicationFinishNotifiable from application container
     /// </summary>
     /// <returns>True if notifiable instance was found and removed</returns>
     bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable);
  }


}
