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
using System.Reflection;
using System.IO;
using System.Threading;

using NFX.ApplicationModel;
using NFX.ApplicationModel.Volatile;

using NFX.Log;
using NFX.Instrumentation;
using NFX.Environment;
using NFX.DataAccess;
using NFX.ServiceModel;
using NFX.Throttling;
using NFX.Glue;
using NFX.Security;
using NFX.Time;

namespace NFX.ApplicationModel
{
  /// <summary>
  /// Provides base implementation of IApplication for various application kinds
  /// </summary>
  [ConfigMacroContext]
  public abstract class CommonApplicationLogic : DisposableObject,  IApplication
  {
    #region CONSTS
      public const string CONFIG_SWITCH = "config";
      
      public const string CONFIG_APP_NAME_ATTR = "application-name";
    
      public const string CONFIG_MEMORY_MANAGEMENT_SECTION = "memory-management";

      public const string CONFIG_STARTERS_SECTION = "starters";
      public const string CONFIG_STARTER_SECTION = "starter";
      public const string CONFIG_TIMESOURCE_SECTION = "time-source";
      public const string CONFIG_EVENT_TIMER_SECTION = "event-timer";
      public const string CONFIG_LOG_SECTION = "log";
      public const string CONFIG_INSTRUMENTATION_SECTION = "instrumentation";
      public const string CONFIG_THROTTLING_SECTION = "throttling";
      public const string CONFIG_DATA_STORE_SECTION = "data-store";
      public const string CONFIG_OBJECT_STORE_SECTION = "object-store";
      public const string CONFIG_GLUE_SECTION = "glue";
      public const string CONFIG_SECURITY_SECTION = "security";


      public const string CONFIG_PRELOAD_ASSEMBLIES_SECTION = "preload-assemblies";
      public const string CONFIG_ASSEMBLY_SECTION = "assembly";
      public const string CONFIG_PATH_ATTR = "path";
      public const string CONFIG_ENABLED_ATTR = "enabled";
    
    #endregion

    #region .ctor/.dctor

      protected override void Destructor()
      {
          m_ShutdownStarted = true;
          base.Destructor();
      }

    #endregion


    #region Fields

      private Guid m_InstanceID = Guid.NewGuid();

      private DateTime m_StartTime;

      private string m_Name; 
      
      private bool m_ShutdownStarted;
      private bool m_Stopping;

      protected List<IConfigSettings> m_ConfigSettings = new List<IConfigSettings>();
      protected List<IApplicationFinishNotifiable> m_FinishNotifiables = new List<IApplicationFinishNotifiable>();

      [Config(TimeLocation.CONFIG_TIMELOCATION_SECTION)]
      private TimeLocation m_TimeLocation = new TimeLocation();
      
      
      protected ConfigSectionNode m_ConfigRoot;

      protected ILogImplementation m_Log;

      protected IInstrumentationImplementation m_Instrumentation;

      protected IThrottlingImplementation m_Throttling;

      protected IDataStoreImplementation m_DataStore;

      protected IObjectStoreImplementation m_ObjectStore;

      protected IGlueImplementation m_Glue;

      protected ISecurityManagerImplementation m_SecurityManager;

      protected ITimeSourceImplementation m_TimeSource;

      protected IEventTimerImplementation m_EventTimer;
  
    #endregion
    
  
    #region Properties


      #region IApplication Members

        /// <summary>
        /// Returns unique identifier of this running instance 
        /// </summary>
        public Guid InstanceID
        {
            get { return m_InstanceID; }
        }


        /// <summary>
        /// Returns timestamp when application started as localized app time 
        /// </summary>
        public DateTime StartTime
        {
            get { return m_StartTime; }
        }
        
        
        
        /// <summary>
        /// Returns the name of this application
        /// </summary>
        public string Name
        {
          get { return m_Name ?? GetType().FullName; }
        }
        
        
        /// <summary>
        /// Returns true when application instance is active and working. This property returns false as soon as application finalization starts on shutdown
        /// Use to exit long-running loops and such
        /// </summary>
        public bool Active
        { 
          get { return !m_ShutdownStarted && !m_Stopping; } 
        }

        /// <summary>
        /// Returns true to indicate that Stop() was called
        /// </summary>
        public bool Stopping
        {
          get { return m_Stopping; }
        }

        /// <summary>
        /// Returns true to indicate that Dispose() has been called and shutdown has started
        /// </summary>
        public bool ShutdownStarted
        {
          get { return m_ShutdownStarted; }
        }

        /// <summary>
        /// Initiates the stop of the application by setting its Stopping to true and Active to false so dependent services may start to terminate 
        /// </summary>
        public void Stop()
        {
          m_Stopping = true;
        }

        /// <summary>
        /// References application logger
        /// </summary>
        public ILog Log
        {
          get { return (ILog)m_Log ?? NOPLog.Instance; }
        }
        
        
        /// <summary>
        /// References application instrumentation
        /// </summary>
        public IInstrumentation Instrumentation
        {
          get { return m_Instrumentation ?? NOPInstrumentation.Instance; }
        }

        /// <summary>
        /// References application throttling
        /// </summary>
        public IThrottling Throttling
        {
            get { return m_Throttling ?? NOPThrottling.Instance; }
        }
        
        /// <summary>
        /// Provides access to configuration root for the whole application
        /// </summary>
        public ConfigSectionNode ConfigRoot
        {
          get { return m_ConfigRoot;}
        }


        /// <summary>
        /// References application data store
        /// </summary>
        public IDataStore DataStore
        {
          get { return m_DataStore ?? NOPDataStore.Instance; }
        }


        /// <summary>
        /// References application object store. Objects will survive application termination 
        /// </summary>
        public IObjectStore ObjectStore
        {
          get { return (IObjectStore)m_ObjectStore ?? NOPObjectStore.Instance; }
        }


        /// <summary>
        /// References glue that can be used to interconnect remote instances
        /// </summary>
        public IGlue Glue
        {
          get { return m_Glue ?? NOPGlue.Instance; }
        }


        /// <summary>
        /// References security manager that performs user authentication based on passed credentials and other security-related global tasks
        /// </summary>
        public ISecurityManager SecurityManager
        {
          get { return m_SecurityManager ?? NOPSecurityManager.Instance; }
        }

        /// <summary>
        /// References time source - an entity that supplies local and UTC times. The concrete implementation
        ///  may elect to get accurate times from the network or other external precision time sources (i.e. NASA atomic clock)
        /// </summary>
        public ITimeSource TimeSource
        {
          get { return m_TimeSource ?? DefaultTimeSource.Instance; }
        }

        /// <summary>
        /// References event timer which maintains and runs scheduled Event instances
        /// </summary>
        public IEventTimer EventTimer
        {
          get { return m_EventTimer ?? NOPEventTimer.Instance; }
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
            get { return UniversalTimeToLocalizedTime(TimeSource.UTCNow); }
        }


    #endregion
                              
      
    #endregion
    
    
    #region Public

            public  void WriteLog(MessageType type, string from, string msgText)
            {
                if (m_Log==null) return;
        
                m_Log.Write(new NFX.Log.Message()
                                    {
                                      Topic = LogTopic,
                                      Type = type,
                                      From = from,
                                      Text = msgText
                                    }
                                   );
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
                   return TimeSource.UniversalTimeToLocalizedTime(utc);
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
                   return TimeSource.LocalizedTimeToUniversalTime(local);
                }
            }


            /// <summary>
            /// Makes BaseSession instance
            /// </summary>
            public virtual ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
            {
                var result = new BaseSession(sessionID);
                result.User = user;

                return result;
            }


            /// <summary>
            /// Registers an instance of IConfigSettings with application container to receive a call when
            ///  underlying app configuration changes
            /// </summary>
            /// <returns>True if settings instance was not found and was added</returns>
            public bool RegisterConfigSettings(IConfigSettings settings)
            {
                if (m_ShutdownStarted || settings==null) return false;
                lock(m_ConfigSettings)
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
            public bool UnregisterConfigSettings(IConfigSettings settings)
            {
                if (m_ShutdownStarted || settings==null) return false;
                lock(m_ConfigSettings)
                  return m_ConfigSettings.Remove(settings);
            }

            /// <summary>
            /// Forces notification of all registered IConfigSettings-implementers about configuration change
            /// </summary>
            public void NotifyAllConfigSettingsAboutChange()
            {
                NotifyAllConfigSettingsAboutChange(m_ConfigRoot);
            }


            /// <summary>
            /// Registers an instance of IApplicationFinishNotifiable with application container to receive a call when
            ///  underlying application instance will finish its lifecycle
            /// </summary>
            /// <returns>True if notifiable instance was not found and was added</returns>
            public bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
            {
                if (m_ShutdownStarted || notifiable==null) return false;

                lock(m_FinishNotifiables)
                  if (!m_FinishNotifiables.Contains(notifiable, ReferenceEqualityComparer<IApplicationFinishNotifiable>.Instance))
                  {
                     m_FinishNotifiables.Add(notifiable);
                     return true;
                  }
                return false;
            }

            /// <summary>
            /// Removes the registration of IConfigSettings from application container
            /// </summary>
            /// <returns>True if notifiable instance was found and removed</returns>
            public bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
            {
                if (m_ShutdownStarted || notifiable==null) return false;

                lock(m_FinishNotifiables)
                  return m_FinishNotifiables.Remove(notifiable);
            }


            /// <summary>
            /// RESERVED for future use. Loads assemblies specified in 'preload-assemblies' section from disk optionally checking the 'enabled' flag.
            /// This method is called on application startup by the framework
            /// </summary>
            public bool PreloadAssemblies(bool checkFlag = true)
            {
               var nPreload = m_ConfigRoot[CONFIG_PRELOAD_ASSEMBLIES_SECTION];

               if (checkFlag && !nPreload.AttrByName(CONFIG_ENABLED_ATTR).ValueAsBool(true)) return false;

               string apath = CoreConsts.UNKNOWN;
               try
               { 
                foreach(var anode in nPreload.Children.Where(c=>c.IsSameName(CONFIG_ASSEMBLY_SECTION)))
                {
                  apath = anode.AttrByName(CONFIG_PATH_ATTR).Value;
                  if (apath.IsNullOrWhiteSpace()) continue;
                  if (!File.Exists(apath)) continue;
                  Assembly.LoadFrom(apath);
                }
               }
               catch(Exception error)
               {
                  throw new NFXException(StringConsts.APP_ASSEMBLY_PRELOAD_ERROR.Args(apath, error.ToMessageWithType()), error);
               }

               return true;
            }


    #endregion


    #region Protected
    
      /// <summary>
      /// Provides log topic name
      /// </summary>
      protected abstract string LogTopic
      {
       get;
      } 


      protected IEnumerable<IApplicationStarter> GetStarters()
      {
        var snodes = m_ConfigRoot[CONFIG_STARTERS_SECTION].Children.Where(n=>n.IsSameName(CONFIG_STARTER_SECTION)); 
        foreach(var snode in snodes)
        {
            var starter = FactoryUtils.MakeAndConfigure<IApplicationStarter>(snode);
            yield return starter;
        }
      }

      protected virtual void InitApplication()
      {
        PreloadAssemblies();

        var exceptions = new List<Exception>();

        var starters = GetStarters().ToList();     
        
            string name = CoreConsts.UNKNOWN;
            bool breakOnError = true;
            foreach (var starter in starters)
                try
                {
                    breakOnError = starter.ApplicationStartBreakOnException;
                    name = starter.Name ?? starter.GetType().FullName;
                    starter.ApplicationStartBeforeInit(this);
                }
                catch (Exception error)
                {
                    error = new NFXException(StringConsts.APP_STARTER_BEFORE_ERROR.Args(name, error.ToMessageWithType()), error);
                    if (breakOnError) throw error;
                    exceptions.Add(error);
                    //log not available at this point
                }
      
        
        DoInitApplication(); //<----------------------------------------------

            

            name = CoreConsts.UNKNOWN;
            breakOnError = true;
            foreach(var starter in starters)
            try
            {
              breakOnError = starter.ApplicationStartBreakOnException;
              name = starter.Name ?? starter.GetType().FullName;
              starter.ApplicationStartAfterInit(this);
            }
            catch(Exception error)
            {
              error = new NFXException(StringConsts.APP_STARTER_AFTER_ERROR.Args(name, error.ToMessageWithType()), error);
              if (breakOnError) throw error;
              WriteLog(MessageType.Error, "InitApplication().After", error.Message);
            }

        if (exceptions.Count>0)
            foreach(var exception in exceptions)
                WriteLog(MessageType.Error, "InitApplication().Before", exception.ToMessageWithType());

      }

      protected virtual void CleanupApplication()
      {
            var exceptions = new List<Exception>();
        
            lock(m_FinishNotifiables)
            {
                string name = CoreConsts.UNKNOWN;
                foreach(var notifiable in m_FinishNotifiables)
                try
                {
                    name = notifiable.Name ?? notifiable.GetType().FullName;
                    notifiable.ApplicationFinishBeforeCleanup(this);
                }
                catch(Exception error)
                {
                   error = new NFXException(StringConsts.APP_FINISH_NOTIFIABLE_BEFORE_ERROR.Args(name, error.ToMessageWithType()), error);
                   exceptions.Add(error); 
                   WriteLog(MessageType.Error, "CleanupApplication()", error.Message); 
                }
            }

        DoCleanupApplication(); //<----------------------------------------------

            lock(m_FinishNotifiables)
            {
                string name = CoreConsts.UNKNOWN;
                foreach(var notifiable in m_FinishNotifiables)
                try
                {
                    name = notifiable.Name ?? notifiable.GetType().FullName;
                    notifiable.ApplicationFinishAfterCleanup(this);
                }
                catch(Exception error)
                {
                  error = new NFXException(StringConsts.APP_FINISH_NOTIFIABLE_AFTER_ERROR.Args(name, error.ToMessageWithType()), error);
                  exceptions.Add(error);
                  //log not available at this point
                }
            }

        if (exceptions.Count>0)
        {
            var text= new StringBuilder();

            text.AppendLine(StringConsts.APP_FINISH_NOTIFIABLES_ERROR);

            foreach(var exception in exceptions)
                text.AppendLine( exception.ToMessageWithType());
            
            throw new NFXException(text.ToString());
        }

      }


          

      /// <summary>
      /// Tries to find a configuration file name looping through various supported estensions
      /// </summary>
      /// <returns>File name that exists or empty string</returns>
      protected string GetDefaultConfigFileName()
      {
          var exeName = System.Reflection.Assembly.GetEntryAssembly().Location;
          var exeNameWoExt = Path.Combine(Path.GetDirectoryName(exeName), Path.GetFileNameWithoutExtension(exeName));
//Console.WriteLine("EXENAME:" +exeName);
//Console.WriteLine("EXENAME wo extension:" +exeNameWoExt);
          var extensions = Configuration.AllSupportedFormats.Select(fmt => '.'+fmt); 
          foreach(var ext in extensions)
          {
             var configFile = exeName + ext;
//Console.WriteLine("Probing:" +configFile);
             if (File.Exists(configFile)) return configFile;
             configFile = exeNameWoExt + ext;
//Console.WriteLine("Probing:" +configFile);
             if (File.Exists(configFile)) return configFile;
             
          }
          return string.Empty;
      }


      /// <summary>
      /// Forces notification of all registered IConfigSettings-implementers about configuration change
      /// </summary>
      protected void NotifyAllConfigSettingsAboutChange(IConfigSectionNode node)
      {
          node = node ?? m_ConfigRoot;

          lock(m_ConfigSettings)
           foreach(var s in m_ConfigSettings) s.ConfigChanged(node);
      }
      
      /// <summary>
      /// Override to prep log implementation i.e. inject log destinations programmaticaly 
      /// </summary>
      protected virtual void BeforeLogStart(ILogImplementation logImplementation)
      {
      
      }

      /// <summary>
      /// Override to prep instr implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeInstrumentationStart(IInstrumentationImplementation instrumentationImplementation)
      {
      
      }

      /// <summary>
      /// Override to prep throttling implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeThrottlingStart(IThrottlingImplementation throttlingImplementation)
      {

      }

      /// <summary>
      /// Override to prep data store implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeDataStoreStart(IDataStoreImplementation datastoreImplementation)
      {
      
      }

      /// <summary>
      /// Override to prep object store implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeObjectStoreStart(IObjectStoreImplementation objectstoreImplementation)
      {
      
      }


      /// <summary>
      /// Override to prep glue implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeGlueStart(IGlueImplementation glueImplementation)
      {
      
      }


      /// <summary>
      /// Override to prep security manager implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeSecurityManagerStart(ISecurityManagerImplementation securitymanagerImplementation)
      {
      
      }


      /// <summary>
      /// Override to prep time source implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeTimeSourceStart(ITimeSourceImplementation timesourceImplementation)
      {
      
      }

      /// <summary>
      /// Override to prep event timer implementation i.e. inject something programmaticaly 
      /// </summary>
      protected virtual void BeforeEventTimerStart(IEventTimerImplementation eventTimerImplementation)
      {
      
      }
    
    
   
      protected virtual void DoInitApplication()
      {
        ExecutionContext.__SetApplicationLevelContext(this, null, null, NOPSession.Instance);

        const string FROM = "app.init";
        
        ConfigAttribute.Apply(this, m_ConfigRoot);

        m_Name = m_ConfigRoot.AttrByName(CONFIG_APP_NAME_ATTR).ValueAsString(GetType().FullName);

        Debugging.DefaultDebugAction = Debugging.ReadDefaultDebugActionFromConfig();
        Debugging.TraceDisabled      = Debugging.ReadTraceDisableFromConfig();

        var node = m_ConfigRoot[CONFIG_LOG_SECTION];
        if (node.Exists)
          try
          {
            m_Log = FactoryUtils.MakeAndConfigure(node, typeof(LogService)) as ILogImplementation;
            
            if (m_Log==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                    node
                                                   .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                   .ValueAsString(CoreConsts.UNKNOWN));
            
            WriteLog(MessageType.Info, FROM, "Log made");

            BeforeLogStart(m_Log);
            
            if (m_Log is Service)
            {
              ((Service)m_Log).Start();
              WriteLog(MessageType.Info, FROM, "Log started, msg times are localized of machine-local time until time source starts");
            }
          }
          catch(Exception error)
          {
            throw new NFXException(StringConsts.APP_LOG_INIT_ERROR + error);
          }


        node = m_ConfigRoot[CONFIG_TIMESOURCE_SECTION];
        if (node.Exists)
        {
          try
          {
            m_TimeSource = FactoryUtils.MakeAndConfigure(node, null) as ITimeSourceImplementation;
            
            if (m_TimeSource==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                    node
                                                   .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                   .ValueAsString(CoreConsts.UNKNOWN));
            
            WriteLog(MessageType.Info, FROM, "TimeSource made");

            BeforeTimeSourceStart(m_TimeSource);
            
            if (m_TimeSource is Service)
            {
              ((Service)m_TimeSource).Start();
              WriteLog(MessageType.Info, FROM, "TimeSource started");
            }

            WriteLog(MessageType.Info, FROM, "Log msg time is time source-supplied now");
          }
          catch(Exception error)
          {
            throw new NFXException(StringConsts.APP_TIMESOURCE_INIT_ERROR + error);
          }
        }

        try
        {
          m_StartTime = LocalizedTime;
          WriteLog(MessageType.Info, FROM, "App start time is {0}".Args(m_StartTime));
        }
        catch(Exception error)
        {
            throw new NFXException(StringConsts.APP_TIMESOURCE_INIT_ERROR + error);
        }


        node = m_ConfigRoot[CONFIG_EVENT_TIMER_SECTION];
        //20150827 DKh event timer must allocate even if it is absent in config
        //// if (node.Exists)
        {
          try
          {
            m_EventTimer = FactoryUtils.MakeAndConfigure(node, typeof(EventTimer)) as IEventTimerImplementation;
            
            if (m_EventTimer==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                    node
                                                   .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                   .ValueAsString(CoreConsts.UNKNOWN));
            
            WriteLog(MessageType.Info, FROM, "EventTimer made");

            BeforeEventTimerStart(m_EventTimer);
            
            if (m_EventTimer is Service)
            {
              ((Service)m_EventTimer).Start();
              WriteLog(MessageType.Info, FROM, "EventTimer started");
            }
          }
          catch(Exception error)
          {
            throw new NFXException(StringConsts.APP_EVENT_TIMER_INIT_ERROR + error);
          }
        }




        node = m_ConfigRoot[CONFIG_SECURITY_SECTION];
        if (node.Exists)
          try
          {
            m_SecurityManager = FactoryUtils.MakeAndConfigure(node, typeof(ConfigSecurityManager)) as ISecurityManagerImplementation;
            
            if (m_SecurityManager==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                            node
                                                           .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                           .ValueAsString(CoreConsts.UNKNOWN));

            WriteLog(MessageType.Info, FROM, "Security Manager made");
           
            BeforeSecurityManagerStart(m_SecurityManager);
           
            if (m_SecurityManager is Service)
            {
              ((Service)m_SecurityManager).Start();
              WriteLog(MessageType.Info, FROM, "Security Manager started");
            }
          }
          catch (Exception error)
          {
            var msg = StringConsts.APP_SECURITY_MANAGER_INIT_ERROR + error;
            WriteLog(MessageType.Error, FROM, msg);
            throw new NFXException(msg);
          }

        try
        {
           Behavior.ApplyConfiguredBehaviors(this, m_ConfigRoot);
        }
        catch(Exception error)
        {
           var msg = StringConsts.APP_APPLY_BEHAVIORS_ERROR + error;
           WriteLog(MessageType.Error, FROM, msg);
           throw new NFXException(msg, error);
        }


        node = m_ConfigRoot[CONFIG_INSTRUMENTATION_SECTION];
        if (node.Exists)
          try
          {
            m_Instrumentation = FactoryUtils.MakeAndConfigure(node, typeof(InstrumentationService)) as IInstrumentationImplementation;
            
            if (m_Instrumentation==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                    node
                                                   .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                   .ValueAsString(CoreConsts.UNKNOWN));
            
            WriteLog(MessageType.Info, FROM, "Instrumentation made");

            BeforeInstrumentationStart(m_Instrumentation);

            if (m_Instrumentation is Service)
            {
              ((Service)m_Instrumentation).Start();
              WriteLog(MessageType.Info, FROM, "Instrumentation started");
            }

          }
          catch (Exception error)
          {
            var msg = StringConsts.APP_INSTRUMENTATION_INIT_ERROR + error;
            WriteLog(MessageType.Error, FROM, msg);
            throw new NFXException(msg);
          }


        node = m_ConfigRoot[CONFIG_THROTTLING_SECTION];
        if (node.Exists)
            try
            {
                m_Throttling = FactoryUtils.MakeAndConfigure(node, typeof(ThrottlingService)) as IThrottlingImplementation;

                if (m_Throttling == null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR +
                                                          node
                                                         .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                         .ValueAsString(CoreConsts.UNKNOWN));

                WriteLog(MessageType.Info, FROM, "Throttling made");

                BeforeThrottlingStart(m_Throttling);

                if (m_Throttling is Service)
                {
                    ((Service)m_Throttling).Start();
                    WriteLog(MessageType.Info, FROM, "Throttling started");
                }

            }
            catch (Exception error)
            {
                var msg = StringConsts.APP_THROTTLING_INIT_ERROR + error;
                WriteLog(MessageType.Error, FROM, msg);
                throw new NFXException(msg);
            }



        node = m_ConfigRoot[CONFIG_DATA_STORE_SECTION];
        if (node.Exists)
          try
          {
            m_DataStore = FactoryUtils.MakeAndConfigure(node) as IDataStoreImplementation;
            
            if (m_DataStore==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                    node
                                                   .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                   .ValueAsString(CoreConsts.UNKNOWN));

            WriteLog(MessageType.Info, FROM, "DataStore made");

            
            BeforeDataStoreStart(m_DataStore);

            if (m_DataStore is Service)
            {
              ((Service)m_DataStore).Start();
              WriteLog(MessageType.Info, FROM, "DataStore started");
            }
          }
          catch (Exception error)
          {
            var msg = StringConsts.APP_DATA_STORE_INIT_ERROR + error;
            WriteLog(MessageType.Error, FROM, msg);
            throw new NFXException(msg);
          }



        node = m_ConfigRoot[CONFIG_OBJECT_STORE_SECTION];
        if (node.Exists)
          try
          {
            m_ObjectStore = FactoryUtils.MakeAndConfigure(node, typeof(ObjectStoreService)) as IObjectStoreImplementation;
            
            if (m_ObjectStore==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                            node
                                                           .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                           .ValueAsString(CoreConsts.UNKNOWN));

            WriteLog(MessageType.Info, FROM, "ObjectStore made");
           
            BeforeObjectStoreStart(m_ObjectStore);
           
            if (m_ObjectStore is Service)
            {
              ((Service)m_ObjectStore).Start();
              WriteLog(MessageType.Info, FROM, "ObjectStore started");
            }
          }
          catch (Exception error)
          {
            var msg = StringConsts.APP_OBJECT_STORE_INIT_ERROR + error;
            WriteLog(MessageType.Error, FROM, msg);
            throw new NFXException(msg);
          }

        node = m_ConfigRoot[CONFIG_GLUE_SECTION];
        if (node.Exists)
          try
          {
            m_Glue = FactoryUtils.MakeAndConfigure(node, typeof(NFX.Glue.Implementation.GlueService)) as IGlueImplementation;
            
            if (m_Glue==null) throw new NFXException(StringConsts.APP_INJECTION_TYPE_MISMATCH_ERROR  +
                                                            node
                                                           .AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                                           .ValueAsString(CoreConsts.UNKNOWN));

            WriteLog(MessageType.Info, FROM, "Glue made");
           
            BeforeGlueStart(m_Glue);
           
            if (m_Glue is Service)
            {
              ((Service)m_Glue).Start();
              WriteLog(MessageType.Info, FROM, "Glue started");
            }
          }
          catch (Exception error)
          {
            var msg = StringConsts.APP_GLUE_INIT_ERROR + error;
            WriteLog(MessageType.Error, FROM, msg);
            throw new NFXException(msg);
          }

        


        WriteLog(MessageType.Info, FROM, "Common application initialized");
        WriteLog(MessageType.Info, FROM, "Time localization information follows");
        WriteLog(MessageType.Info, FROM, "    Application: " + this.TimeLocation.ToString());
        WriteLog(MessageType.Info, FROM, "    Log: " + this.Log.TimeLocation.ToString());
        WriteLog(MessageType.Info, FROM, "    Glue: " + this.Glue.TimeLocation.ToString());
        WriteLog(MessageType.Info, FROM, "    Instrumentation: " + this.Instrumentation.TimeLocation.ToString());
        WriteLog(MessageType.Info, FROM, "    ObjStore: " + this.ObjectStore.TimeLocation.ToString());

      }                                           



      protected virtual void DoCleanupApplication()
      {
         const string FROM = "app.cleanup";

         


         if (m_Glue!=null)
         {
           WriteLog(MessageType.Info, FROM, "Finalizing Glue");
           try
           {
             if (m_Glue is Service)
             {
                 ((Service)m_Glue).SignalStop();
                 ((Service)m_Glue).WaitForCompleteStop();
                 WriteLog(MessageType.Info, FROM, "Glue stopped");
             }

             m_Glue.Dispose();
             WriteLog(MessageType.Info, FROM, "Glue disposed");
           }
           catch(Exception error)
           {
             WriteLog(MessageType.Error, FROM, "Error finalizing Glue: " + error.Message);
           }
         }
         
         
         if (m_ObjectStore!=null)
         {
           WriteLog(MessageType.Info, FROM, "Finalizing ObjectStore");
           try
           {
             if (m_ObjectStore is Service)
             {
                 ((Service)m_ObjectStore).SignalStop();
                 ((Service)m_ObjectStore).WaitForCompleteStop();
                 WriteLog(MessageType.Info, FROM, "ObjectStore stopped");
             }

             m_ObjectStore.Dispose();
             WriteLog(MessageType.Info, FROM, "ObjectStore disposed");
           }
           catch(Exception error)
           {
             WriteLog(MessageType.Error, FROM, "Error finalizing ObjectStore: " + error.Message);
           }
         }


         if (m_DataStore!=null)
         {
           WriteLog(MessageType.Info, FROM, "Finalizing DataStore");
           try
           {
             if (m_DataStore is Service)
             {
                 ((Service)m_DataStore).SignalStop();
                 ((Service)m_DataStore).WaitForCompleteStop();
                 WriteLog(MessageType.Info, FROM, "DataStore stopped");
             }
             m_DataStore.Dispose();
             WriteLog(MessageType.Info, FROM, "DataStore disposed");
           }
           catch(Exception error)
           {
             WriteLog(MessageType.Error, FROM, "Error finalizing DataStore: " + error.Message);
           }
         }


         if (m_Throttling != null)
         {
             WriteLog(MessageType.Info, FROM, "Finalizing Throttling");
             try
             {
                 if (m_Throttling is Service)
                 {
                     ((Service)m_Throttling).SignalStop();
                     ((Service)m_Throttling).WaitForCompleteStop();
                     WriteLog(MessageType.Info, FROM, "Throttling stopped");
                 }

                 m_Throttling.Dispose();
                 WriteLog(MessageType.Info, FROM, "Throttling disposed");
             }
             catch (Exception error)
             {
                 WriteLog(MessageType.Error, FROM, "Error finalizing Throttling: " + error.Message);
             }
         }

         
         if (m_Instrumentation != null)
         {
           WriteLog(MessageType.Info, FROM, "Finalizing Instrumentation");
           try
           {
             if (m_Instrumentation is Service)
             {
                 ((Service)m_Instrumentation).SignalStop();
                 ((Service)m_Instrumentation).WaitForCompleteStop();
                 WriteLog(MessageType.Info, FROM, "Instrumentation stopped");
             }
             
             m_Instrumentation.Dispose();
             WriteLog(MessageType.Info, FROM, "Instrumentation disposed");
           }
           catch(Exception error)
           {
             WriteLog(MessageType.Error, FROM, "Error finalizing Instrumentation: " + error.Message);
           }
         }


         if (m_SecurityManager!=null)
         {
           WriteLog(MessageType.Info, FROM, "Finalizing Security Manager");
           try
           {
             if (m_SecurityManager is Service)
             {
                 ((Service)m_SecurityManager).SignalStop();
                 ((Service)m_SecurityManager).WaitForCompleteStop();
                 WriteLog(MessageType.Info, FROM, "Security Manager stopped");
             }

             m_SecurityManager.Dispose();
             WriteLog(MessageType.Info, FROM, "Security Manager disposed");
           }
           catch(Exception error)
           {
             WriteLog(MessageType.Error, FROM, "Error finalizing Security Manager: " + error.Message);
           }
         }


         if (m_EventTimer != null)
         {
           WriteLog(MessageType.Info, FROM, "Finalizing EventTimer");
           try
           {
             if (m_EventTimer is Service)
             {
                 ((Service)m_EventTimer).SignalStop();
                 ((Service)m_EventTimer).WaitForCompleteStop();
                 WriteLog(MessageType.Info, FROM, "EventTimer stopped");
             }
             
             m_EventTimer.Dispose();
             WriteLog(MessageType.Info, FROM, "EventTimer disposed");
           }
           catch(Exception error)
           {
             WriteLog(MessageType.Error, FROM, "Error finalizing EventTimer: " + error.Message);
           }
         }


         if (m_TimeSource != null)
         {
           WriteLog(MessageType.Info, FROM, "Finalizing TimeSource");
           try
           {
             if (m_TimeSource is Service)
             {
                 ((Service)m_TimeSource).SignalStop();
                 ((Service)m_TimeSource).WaitForCompleteStop();
                 WriteLog(MessageType.Info, FROM, "TimeSource stopped");
             }
             
             m_TimeSource.Dispose();
             WriteLog(MessageType.Info, FROM, "TimeSource disposed");
             WriteLog(MessageType.Info, FROM, "Log msg times are machine-local now");
           }
           catch(Exception error)
           {
             WriteLog(MessageType.Error, FROM, "Error finalizing TimeSource: " + error.Message);
           }
         }
         

         if (m_Log!=null)
         {
             WriteLog(MessageType.Info, FROM, "Stopping logger. This is the last message");
             if (m_Log is Service)
             {
                 ((Service)m_Log).SignalStop();
                 ((Service)m_Log).WaitForCompleteStop();
             }
             m_Log.Dispose();
         } 


         ExecutionContext.__SetApplicationLevelContext(null, null, null, NOPSession.Instance);
      }

      


    #endregion
  }
  
}
