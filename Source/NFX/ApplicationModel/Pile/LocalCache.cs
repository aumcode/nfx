using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX.Log;
using NFX.Environment;
using NFX.ServiceModel;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Provides default implementation of a cache that stores the mapping locally.
  /// The mapped-to objects may reside in local or distributed pile as configured
  /// </summary>
  public sealed class LocalCache : ServiceWithInstrumentationBase<object>, ICacheImplementation
  {
    #region CONSTS
      public const string DEFAULT_TABLE_OPTIONS_SECTION = "default-table-options";


      private const string THREAD_NAME = "Pile.LocalCache.Thread";
      private const int THREAD_MIN_GRANULARITY_MS = 1793;
      private const int THREAD_GRANULARITY_VARIANCE_MS = 3597;
      private const int THREAD_MAX_TIME_FOR_SWEEP_ALL_MS = 5000;

      private const string TBL_PARAM_PREFIX = "Table: ";

      private const int EMPTY_TABLE_LIFE_SEC = 60;

    #endregion

    #region .ctor
      public LocalCache():base()
      {
      }

      public LocalCache(string name, object director):base(director)
      {
        Name = name;
      }

      public LocalCache(IPileImplementation pile, object director, string name):base(director)
      {
        m_Pile = pile;
        Name = name;
      }

      protected override void Destructor()
      {
        base.Destructor();
      }

    #endregion


    #region Fields

       private bool m_InstrumentationEnabled;

       private object m_TablesLock = new object();
       private Registry<LocalCacheTable> m_Tables = new Registry<LocalCacheTable>();
       internal IPileImplementation m_Pile;

       private long m_PileMaxMemoryLimit;
       private AllocationMode m_PileAllocMode;

       private Registry<TableOptions> m_TableOptions = new Registry<TableOptions>();
       private TableOptions m_DefaultTableOptions;

       private Thread m_Thread;
       private AutoResetEvent m_Trigger;

    #endregion


    #region Properties


        /// <summary>
        /// Implements IInstrumentable
        /// </summary>
        [Config(Default=false)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE, CoreConsts.EXT_PARAM_GROUP_PILE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public override bool InstrumentationEnabled
        {
          get { return m_InstrumentationEnabled;}
          set
          {
             m_InstrumentationEnabled = value;
            var pile = m_Pile;
            if (pile!=null)
             pile.InstrumentationEnabled = value;
          }
        }

        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE, CoreConsts.EXT_PARAM_GROUP_PILE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public long PileMaxMemoryLimit
        {
          get
          {
            return m_PileMaxMemoryLimit;
          }
          set
          {
            m_PileMaxMemoryLimit = value;
            var pile = m_Pile;
            if (pile!=null)
             pile.MaxMemoryLimit = value;
          }
        }

        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE, CoreConsts.EXT_PARAM_GROUP_PILE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public AllocationMode PileAllocMode
        {
          get
          {
            return m_PileAllocMode;
          }
          set
          {
            m_PileAllocMode = value;
            var pile = m_Pile;
            if (pile!=null)
             pile.AllocMode = value;
          }
        }

        public LocalityKind Locality { get { return LocalityKind.Local;}}

        public ObjectPersistence Persistence { get{ return ObjectPersistence.Memory;}}

        public IPileStatus PileStatus{ get{ return m_Pile;}}

        /// <summary>
        /// Gets/sets a pile instance that this cache is using.
        /// Can set on an inactive only.
        /// If the target pile is directed by this service then it will start/stop the pile,
        /// otherwise the pile has to be managed externally
        /// </summary>
        public IPileImplementation Pile
        {
          get{ return m_Pile;}
          set
          {
            CheckServiceInactive();
            m_Pile = value;
          }
        }

        /// <summary>
        /// Tables that this cache contains
        /// </summary>
        public IRegistry<ICacheTable> Tables { get{ return m_Tables;}}

        /// <summary>
        /// Returns table options - used for table creation
        /// </summary>
        public Registry<TableOptions> TableOptions { get{ return m_TableOptions;} }


        /// <summary>
        /// Sets default options for a table which is not found in TableOptions collection.
        /// If this property is null then every table assumes the set of constant values defined in Table class
        /// </summary>
        public TableOptions DefaultTableOptions
        {
          get { return m_DefaultTableOptions;}
          set { m_DefaultTableOptions = value;}
        }


        /// <summary>
        /// Handy admin property that sets detailed instrumentation flag for all tables at once
        /// </summary>
        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE, CoreConsts.EXT_PARAM_GROUP_PILE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public bool? DetailedInstrumentationForAllTables
        {
          get
          {
            return null; //the fake get is needed so that the external param binder works correctly (it needs the ability to get existing value)
          }
          set
          {
            ensureRunning("DetailedInstrumentationForAllTables.set");
            if (!value.HasValue) return;
            foreach(var tbl in m_Tables) tbl.Options.DetailedInstrumentation = value.Value;
          }
        }


        /// <summary>
        /// Returns total number of records in cache
        /// </summary>
        public long Count
        {
          get { return m_Tables.Sum(t => t.Count);}
        }


    #endregion

    #region Public

        public void PurgeAll()
        {
          if (!Running) return;
          purgeAll();
        }

        public ICacheTable<TKey> GetOrCreateTable<TKey>(string tableName, IEqualityComparer<TKey> keyComparer = null)
        {
          bool created;
          return this.GetOrCreateTable<TKey>(tableName, out created, keyComparer);
        }

        public ICacheTable<TKey> GetOrCreateTable<TKey>(string tableName, out bool createdNew, IEqualityComparer<TKey> keyComparer = null)
        {
           if (tableName.IsNullOrWhiteSpace())
                throw new PileCacheException(StringConsts.ARGUMENT_ERROR + "GetTable(tableName=null|empty)");

           ensureRunning("GetOrCreateTable");

           LocalCacheTable<TKey> result;

           var tbl = m_Tables[tableName];
           if (tbl!=null)
           {
             result = tbl as LocalCacheTable<TKey>;
             if (result==null)
               throw new PileCacheException(StringConsts.PILE_CACHE_TBL_KEYTYPE_MISMATCH_ERROR.Args(tableName,
                                                                    tbl.GetType().DisplayNameWithExpandedGenericArgs(),
                                                                    typeof(LocalCacheTable<TKey>).DisplayNameWithExpandedGenericArgs() ));
             if (keyComparer!=null && result.KeyComparer!=keyComparer)
               throw new PileCacheException(StringConsts.PILE_CACHE_TBL_KEYCOMPARER_MISMATCH_ERROR.Args(tableName,
                                                                    result.KeyComparer!=null ? result.KeyComparer.GetType().FullName : StringConsts.NULL_STRING,
                                                                    keyComparer!=null ? keyComparer.GetType().FullName : StringConsts.NULL_STRING ));
             createdNew = false;
             return result;
           }

           lock(m_TablesLock)
           {
             tbl = m_Tables[tableName];
             if (tbl!=null)
             {
               result = tbl as LocalCacheTable<TKey>;
               if (result==null)
                 throw new PileCacheException(StringConsts.PILE_CACHE_TBL_KEYTYPE_MISMATCH_ERROR.Args(tableName,
                                                                    tbl.GetType().DisplayNameWithExpandedGenericArgs(),
                                                                    typeof(LocalCacheTable<TKey>).DisplayNameWithExpandedGenericArgs() ));
               if (keyComparer!=null && result.KeyComparer!=keyComparer)
                 throw new PileCacheException(StringConsts.PILE_CACHE_TBL_KEYCOMPARER_MISMATCH_ERROR.Args(tableName,
                                                                    result.KeyComparer!=null ? result.KeyComparer.GetType().FullName : StringConsts.NULL_STRING,
                                                                    keyComparer!=null ? keyComparer.GetType().FullName : StringConsts.NULL_STRING ));
               createdNew = false;
               return result;
             }

             result = new LocalCacheTable<TKey>(this, tableName, keyComparer, m_TableOptions[tableName] ?? m_DefaultTableOptions);
             m_Tables.Register(result);
             createdNew = true;
             return result;
           }

        }

        public ICacheTable<TKey> GetTable<TKey>(string tableName)
        {
           if (tableName.IsNullOrWhiteSpace())
                throw new PileCacheException(StringConsts.ARGUMENT_ERROR + "GetTable(tableName=null|empty)");

           ensureRunning("GetTable");

           ICacheTable<TKey> result;

           var tbl = m_Tables[tableName];
           if (tbl!=null)
           {
             result = tbl as ICacheTable<TKey>;
             if (result==null) throw new PileCacheException(StringConsts.PILE_CACHE_TBL_KEYTYPE_MISMATCH_ERROR.Args(tableName,
                                                                    tbl.GetType().DisplayNameWithExpandedGenericArgs(),
                                                                    typeof(LocalCacheTable<TKey>).DisplayNameWithExpandedGenericArgs() ));
             return result;
           }

          throw new PileCacheException(StringConsts.PILE_CACHE_TBL_DOES_NOT_EXIST_ERROR.Args(tableName));
        }



          /// <summary>
          /// Returns named parameters that can be used to control this component
          /// </summary>
          public override IEnumerable<KeyValuePair<string, Type>> ExternalParameters
          {
            get
            {
              return ExternalParameterAttribute.GetParameters(this).Concat( getTableOptionsAsExternalParameters());
            }
          }

          /// <summary>
          /// Returns named parameters that can be used to control this component
          /// </summary>
          public override IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
          {
            return ExternalParameterAttribute.GetParameters(this, groups).Concat( getTableOptionsAsExternalParameters());
          }

          /// <summary>
          /// Gets external parameter value returning true if parameter was found
          /// </summary>
          public override bool ExternalGetParameter(string name, out object value, params string[] groups)
          {
            if (name.StartsWith(TBL_PARAM_PREFIX) && name.Length > TBL_PARAM_PREFIX.Length)
            {
              var tname = name.Substring(TBL_PARAM_PREFIX.Length);
              var tbl = m_Tables[tname];
              if (tbl==null)
              {
                value = null;
                return false;
              }
              value = tbl.Options.AsExternalParameter;
              return true;
            }
            else
             return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
          }

          /// <summary>
          /// Sets external parameter value returning true if parameter was found and set
          /// </summary>
          public override bool ExternalSetParameter(string name, object value, params string[] groups)
          {
            if (name.StartsWith(TBL_PARAM_PREFIX) && name.Length > TBL_PARAM_PREFIX.Length)
            {
              var tname = name.Substring(TBL_PARAM_PREFIX.Length);
              var tbl = m_Tables[tname];
              if (tbl==null)
                return false;
              tbl.Options.AsExternalParameter = value;
              return true;
            }
            else
              return ExternalParameterAttribute.SetParameter(this, name, value, groups);
          }

    #endregion

    #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
          if (node==null || !node.Exists)
                {
                    node = App.ConfigRoot[DataAccess.Cache.CacheStore.CONFIG_CACHE_SECTION]
                              .Children
                              .FirstOrDefault(s => s.IsSameName(DataAccess.Cache.CacheStore.CONFIG_STORE_SECTION) && s.IsSameNameAttr(Name) );
                    if (node==null)
                    {

                        node = App.ConfigRoot[DataAccess.Cache.CacheStore.CONFIG_CACHE_SECTION]
                               .Children
                               .FirstOrDefault(s => s.IsSameName(DataAccess.Cache.CacheStore.CONFIG_STORE_SECTION) && !s.AttrByName(Configuration.CONFIG_NAME_ATTR).Exists);
                        if (node==null) return;
                    }
                }

                ConfigAttribute.Apply(this, node);

                m_TableOptions = new Registry<TableOptions>();
                foreach(var tn in node.Children.Where(cn => cn.IsSameName(DataAccess.Cache.CacheStore.CONFIG_TABLE_SECTION)) )
                {
                    var tbl = new TableOptions(tn);
                    m_TableOptions.Register( tbl );
                }

                var dton = node[DEFAULT_TABLE_OPTIONS_SECTION];
                if (dton.Exists)
                 m_DefaultTableOptions = new TableOptions(dton, false);

                if (m_Pile.ComponentDirector==this)
                  m_Pile.Configure(node[DefaultPile.CONFIG_PILE_SECTION]);
        }


        protected override void DoStart()
        {
          log(MessageType.Info, "DoStart()", "Entering...");

          try
          {
              if (m_Pile==null) throw new PileCacheException(StringConsts.PILE_CACHE_SCV_START_PILE_NULL_ERROR);

              if (m_Pile.ComponentDirector==this)
               m_Pile.Start();
              else
               if (!m_Pile.Running) throw new PileCacheException(StringConsts.PILE_CACHE_SCV_START_PILE_NOT_STARTED_ERROR);


              m_Tables.Clear();
              m_Pile.InstrumentationEnabled = m_InstrumentationEnabled;
              m_Pile.MaxMemoryLimit = m_PileMaxMemoryLimit;
              m_Pile.AllocMode = m_PileAllocMode;


              m_Thread = new Thread(threadSpin);
              m_Thread.Name = THREAD_NAME;
              m_Thread.IsBackground = false;
              m_Trigger = new AutoResetEvent(false);
              m_Thread.Start();
          }
          catch(Exception error)
          {
             AbortStart();

             if (m_Pile!=null && m_Pile.ComponentDirector==this && m_Pile.Running)
              try {m_Pile.WaitForCompleteStop();}
              catch(Exception pe)
              {
                log(MessageType.Error, "DoStart().pileAbort", "Exception: " + pe.ToMessageWithType(), pe);
              }

             if (m_Thread != null)
             {
                try
                {
                  m_Thread.Join();
                  if (m_Trigger!=null) m_Trigger.Dispose();
                } catch{}
                m_Thread = null;
                m_Trigger = null;
             }

             log(MessageType.CatastrophicError, "DoStart()", "Exception: " + error.ToMessageWithType(), error);

             throw error;
          }
          log(MessageType.Info, "DoStart()", "...Exiting");
        }

        protected override void DoSignalStop()
        {
           log(MessageType.Info, "DoSignalStop()", "Entering...");

           try
           {
              if (m_Pile.ComponentDirector==this)
                m_Pile.SignalStop();
           }
           catch (Exception error)
           {
             log(MessageType.CatastrophicError, "DoSignalStop()", "Exception: " + error.ToMessageWithType(), error);
             throw;
           }

           log(MessageType.Info, "DoSignalStop()", "...Exiting");
        }

        protected override void DoWaitForCompleteStop()
        {
          log(MessageType.Info, "DoWaitForCompleteStop()", "Entering...");

          try
          {
              if (m_Thread!=null)
              {
               m_Trigger.Set();
               m_Thread.Join();
               m_Thread = null;
               m_Trigger.Dispose();
               m_Trigger = null;
              }

              if (m_Pile.ComponentDirector==this)
                m_Pile.WaitForCompleteStop();
              else
                purgeAll();//if pile not owned by us then we need to remove all records stored in a pile by us (may take time)

              m_Tables.Clear();
          }
          catch (Exception error)
          {
            log(MessageType.CatastrophicError, "DoWaitForCompleteStop()", "Exception leaked: " + error.ToMessageWithType(), error);
            throw error;
          }

          log(MessageType.Info, "DoWaitForCompleteStop()", "...Exiting");
        }

    #endregion

    #region .pvt

        private void ensureRunning(string operation)
        {
          if (!Running) throw new PileCacheException(StringConsts.SERVICE_INVALID_STATE+"{0}.{1} needs running".Args(Name, operation));
        }

        private void log(MessageType type, string from, string message, Exception error = null)
        {
          App.Log.Write(
                  new Log.Message
                  {
                    Text = message ?? string.Empty,
                    Type = type,
                    From = "{0}('{1}').{2}".Args(GetType().Name, Name, from ?? "*"),
                    Topic = CoreConsts.CACHE_TOPIC,
                    Exception = error
                  }
                );
        }


        private void purgeAll()
        {
          foreach(var tbl in m_Tables)
           tbl.DoPurge();
        }


        private void threadSpin()
        {
              var lastPileCompact = DateTime.UtcNow;

              log(MessageType.Info, "threadSpin()", "Entering...");
              try
              {
                  var wasActive = App.Active;//remember whether app was active during start
                                             //this is needed so that CacheStore works without app container (using NOPApplication) in which case
                                             //service must be .Disposed() to stop this thread
                  var timer = Stopwatch.StartNew();
                  while ((App.Active | !wasActive) && Running)
                  {
                    var utcNow = DateTime.UtcNow;

                    var tc = m_Tables.Count;
                    var maxTimePerTableMs = THREAD_MAX_TIME_FOR_SWEEP_ALL_MS / (tc!=0 ? tc : 1);
                    if (maxTimePerTableMs<2) maxTimePerTableMs = 2;

                    foreach(var table in m_Tables)
                    {
                       if (!Running) break;
                       try
                       {
                         var sweptToEnd = table.Sweep(timer, maxTimePerTableMs);
                         if (m_InstrumentationEnabled)
                         {
                           Instrumentation.CacheTableSwept.Happened(Name); //for cache
                           Instrumentation.CacheTableSwept.Happened(Name+"."+table.Name); //for table
                         }

                         if (sweptToEnd)
                         {
                           if (table.Count==0)
                           {
                               if (!table.SweepWhenBecameEmpty.HasValue)
                                 table.SweepWhenBecameEmpty = utcNow;
                               else
                                if ((utcNow - table.SweepWhenBecameEmpty.Value).TotalSeconds > EMPTY_TABLE_LIFE_SEC)
                                {
                                  m_Tables.Unregister(table);//can mutate m_Tables because foreach does a snapshot via GetEnumerator
                                }
                           }
                           else
                            table.SweepWhenBecameEmpty = null;
                         }
                       }
                       catch(Exception error)
                       {
                         log(MessageType.Critical,
                             "threadSpin().foreach.Sweep",
                             "Leaked exception while sweeping table '{0}': {1}'"
                               .Args(table.Name, error.ToMessageWithType()), error );
                       }
                    }


                    try
                    {
                      dumpInstruments();
                    }
                    catch(Exception error)
                    {
                         log(MessageType.Critical,
                             "threadSpin().dumpInstruments",
                             "Leaked exception dumping instrumentation: {0}'"
                               .Args(error.ToMessageWithType()), error );
                    }

                    try
                    {

                      if ((utcNow-lastPileCompact).TotalSeconds > 5 * 60)
                      {
                         var freed = m_Pile.Compact();
                         lastPileCompact = utcNow;
                         if (freed>0)
                          log(MessageType.Info, "threadSpin().pile.compact()", "Freed {0:n0} bytes".Args(freed));
                      }
                    }
                    catch(Exception error)
                    {
                         log(MessageType.Critical,
                             "threadSpin().pile.compact()",
                             "Leaked exception compacting pile: {0}'"
                               .Args(error.ToMessageWithType()), error );
                    }

                    m_Trigger.WaitOne(THREAD_MIN_GRANULARITY_MS + ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, THREAD_GRANULARITY_VARIANCE_MS));
                  }//while

              }
              catch(Exception e)
              {
                  log(MessageType.Emergency, "threadSpin()", "Leaked exception, the tables are not swept anymore"+e.ToMessageWithType(), e);
              }

              log(MessageType.Info, "threadSpin()", "...Exiting");
        }


            private void dumpInstruments()
            {
                if (!m_InstrumentationEnabled || !App.Instrumentation.Enabled) return;

                var instr = App.Instrumentation;

                var total_Count = 0L;
                var total_Capacity = 0L;
                var total_LoadFactor = 0d;
                var total_Put = 0L;
                var total_PutCollision = 0L;
                var total_PutOverwrite = 0L;
                var total_PutReplace = 0L;
                var total_RemoveHit = 0L;
                var total_RemoveMiss = 0L;
                var total_Sweep = 0L;
                var total_SweepDuration = 0L;
                var total_RejuvenateHit = 0L;
                var total_RejuvenateMiss = 0L;
                var total_GetHit = 0L;
                var total_GetMiss = 0L;
                var total_Grew = 0L;
                var total_Shrunk = 0L;

                var cnt = 0;

                foreach(var tbl in m_Tables)
                {
                    var di = tbl.Options.DetailedInstrumentation;
                    var ts = "{0}.{1}".Args(Name, tbl.Name);
                    long lv;
                    double dv;

                    lv = tbl.Count;       if (di) instr.Record( new Instrumentation.CacheCount(ts, lv) );       total_Count  += lv;
                    lv = tbl.Capacity;    if (di) instr.Record( new Instrumentation.CacheCapacity(ts, lv) );    total_Capacity  += lv;
                    dv = tbl.LoadFactor * 100d;  if (di) instr.Record( new Instrumentation.CacheLoadFactor(ts, dv) );  total_LoadFactor  += dv;

                    lv = Interlocked.Exchange( ref tbl.m_stat_Put, 0);           if (di) instr.Record( new Instrumentation.CachePut(ts, lv) );          total_Put  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_PutCollision, 0);  if (di) instr.Record( new Instrumentation.CachePutCollision(ts, lv) ); total_PutCollision  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_PutOverwrite, 0);  if (di) instr.Record( new Instrumentation.CachePutOverwrite(ts, lv) ); total_PutOverwrite  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_PutReplace, 0);    if (di) instr.Record( new Instrumentation.CachePutReplace(ts, lv) );   total_PutReplace  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_RemoveHit, 0);     if (di) instr.Record( new Instrumentation.CacheRemoveHit(ts, lv) );    total_RemoveHit  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_RemoveMiss, 0);    if (di) instr.Record( new Instrumentation.CacheRemoveMiss(ts, lv) );   total_RemoveMiss  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_Sweep, 0);         if (di) instr.Record( new Instrumentation.CacheSweep(ts, lv) );        total_Sweep  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_SweepDuration, 0); if (di) instr.Record( new Instrumentation.CacheSweepDuration(ts, lv) );  total_SweepDuration  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_RejuvenateHit, 0); if (di) instr.Record( new Instrumentation.CacheRejuvenateHit(ts, lv) );  total_RejuvenateHit  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_RejuvenateMiss, 0);if (di) instr.Record( new Instrumentation.CacheRejuvenateMiss(ts, lv) ); total_RejuvenateMiss  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_GetHit, 0);        if (di) instr.Record( new Instrumentation.CacheGetHit(ts, lv) );  total_GetHit  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_GetMiss, 0);       if (di) instr.Record( new Instrumentation.CacheGetMiss(ts, lv) ); total_GetMiss  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_Grew, 0);          if (di) instr.Record( new Instrumentation.CacheGrew(ts, lv) );    total_Grew  += lv;
                    lv = Interlocked.Exchange( ref tbl.m_stat_Shrunk, 0);        if (di) instr.Record( new Instrumentation.CacheShrunk(ts, lv) );  total_Shrunk  += lv;

                    cnt++;
                }

                var src = Name;
                instr.Record( new Instrumentation.CacheTableCount(src, m_Tables.Count) );
                instr.Record( new Instrumentation.CacheCount(src, total_Count) );
                instr.Record( new Instrumentation.CacheCapacity(src, total_Capacity) );
                instr.Record( new Instrumentation.CacheLoadFactor(src, cnt>0 ?  total_LoadFactor / (double)cnt : 0d) );
                instr.Record( new Instrumentation.CachePut(src, total_Put) );
                instr.Record( new Instrumentation.CachePutCollision(src, total_PutCollision) );
                instr.Record( new Instrumentation.CachePutOverwrite(src, total_PutOverwrite) );
                instr.Record( new Instrumentation.CachePutReplace(src, total_PutReplace) );
                instr.Record( new Instrumentation.CacheRemoveHit(src, total_RemoveHit) );
                instr.Record( new Instrumentation.CacheRemoveMiss(src, total_RemoveMiss) );
                instr.Record( new Instrumentation.CacheSweep(src, total_Sweep) );
                instr.Record( new Instrumentation.CacheSweepDuration(src, total_SweepDuration) );
                instr.Record( new Instrumentation.CacheRejuvenateHit(src, total_RejuvenateHit) );
                instr.Record( new Instrumentation.CacheRejuvenateMiss(src, total_RejuvenateMiss) );
                instr.Record( new Instrumentation.CacheGetHit(src, total_GetHit) );
                instr.Record( new Instrumentation.CacheGetMiss(src, total_GetMiss) );
                instr.Record( new Instrumentation.CacheGrew(src, total_Grew) );
                instr.Record( new Instrumentation.CacheShrunk(src, total_Shrunk) );


                if (total_Count > 0)
                {
                  var entropySample =
                           ((total_Put & 0xff) << 56)        |
                           ((total_GetHit & 0xff) << 48)     |
                           ((total_RemoveHit & 0xff) << 40)  |
                           ((total_Sweep & 0xff) << 32)      |
                           ((total_Capacity & 0xffff) << 16) |
                           (total_Count & 0xffff);

                  NFX.ExternalRandomGenerator.Instance.FeedExternalEntropySample( (int)((entropySample >> 32) ^ (entropySample & 0xffffffff)) );
                }
            }


       private IEnumerable<KeyValuePair<string, Type>> getTableOptionsAsExternalParameters()
       {
         return m_Tables.Select( tbl => new KeyValuePair<string, Type>(TBL_PARAM_PREFIX+tbl.Name, typeof(string)) );
       }


    #endregion
  }
}
