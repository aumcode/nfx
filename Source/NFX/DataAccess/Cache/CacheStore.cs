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
using System.Threading.Tasks;

using NFX.Log;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.ApplicationModel;
using NFX.Instrumentation;


namespace NFX.DataAccess.Cache
{
    /// <summary>
    /// Represents an efficient in-memory cache of expiring optionally-prioritized objects.
    /// This class is higly optimized for caching of business objects in data store implementations and does not guarantee that
    /// all hash collisions are handled, that is - some data may be overridden. The implementation relies on 2 stage hashing, where the second collision replaces the
    ///  existing item with the colliding one if items are equal in their priorities. The degree of collisions is controlled by 'bucketSize' and 'recPerPage' parameters that
    ///  are passed to the store per table, so basically the tables are capped at a certain size and can not change (bucketSize*recPerPage).
    /// The lookup implementation is 100% lock-free, whereas the degree of mutability-related locking is controlled by 'lockCount' per table.
    /// This class is thread safe for reading and writing cache items, however it does not guarantee instant read/write consistency between threads.
    /// </summary>
    /// <remarks>
    /// Perfomance testing of this class vs. System.Runtime.Caching.MemoryCache storing a typical database record identified by a long key:
    ///  NFX is 2.5-5 times faster for concurrent reads and takes 20% less ram.
    ///  NFX is 1.3-2.5 times faster for writes
    /// </remarks>
    public sealed class CacheStore : ApplicationComponent, INamed, IConfigurable, IInstrumentable
    {
        #region CONSTS

            public const string CONFIG_CACHE_SECTION = "cache";
            public const string CONFIG_STORE_SECTION = "store";
            public const string CONFIG_TABLE_SECTION = "table";

        #endregion


        #region .ctor

            public CacheStore() : this(null)
            {
            }

            public CacheStore(string name)
            {
                if (name.IsNullOrWhiteSpace())
                    m_Name =  Guid.NewGuid().ToString();
                else
                    m_Name = name;


                m_Tables = new Registry<Table>();
                m_TableOptions = new Registry<TableOptions>();

                m_Thread = new Thread(threadSpin);
                m_Thread.Name = "CacheStore Thread";
                m_Thread.IsBackground = false;
                m_Trigger = new AutoResetEvent(false);
                m_Running = true;
                m_Thread.Start();
            }

            protected override void Destructor()
            {
                if (m_Thread!=null)//safeguard
                {
                    m_Running = false;
                    m_Trigger.Set();

                    m_Thread.Join();
                    m_Thread = null;
                    m_Trigger.Dispose();
                }
                base.Destructor();
            }

        #endregion

        #region Fields

            private string m_Name;
            private bool m_Running;             internal bool isRunning{ get{return m_Running;} }
            private Registry<Table> m_Tables;
            private Registry<TableOptions> m_TableOptions;
            private TableOptions m_DefaultTableOptions;
            private Thread m_Thread;
            private AutoResetEvent m_Trigger;

            private bool m_ParallelSweep;
            private bool m_InstrumentationEnabled;
        #endregion

        #region Properties

            /// <summary>
            /// Returns store name which can be used to identify stores in registries and instrumentation/telemetry outputs
            /// </summary>
            public string Name { get{ return m_Name;} }


            /// <summary>
            /// Returns all tables that this store currently contains
            /// </summary>
            public IRegistry<Table> Tables { get{ return m_Tables; }}

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
            /// Returns a table by its name creating its' instance if such table is not in the set. Table names are case-insensitive
            /// </summary>
            public Table this[string table]
            {
                get
                {
                    if (table==null)
                      throw new NFXException(StringConsts.ARGUMENT_ERROR + "CacheStore[table=null]");

                    var tbl = m_Tables[table];
                    if (tbl!=null) return tbl;

                    tbl = new Table(this, table, m_TableOptions[table] ?? m_DefaultTableOptions);
                    if (m_Tables.Register(tbl)) return tbl;

                    return m_Tables[table];
                }
            }


            /// <summary>
            /// Returns a cached record from named table identified by the key or null if this item was not found
            /// </summary>
            public CacheRec this[string table, ulong key]
            {
                get
                {
                  return this[table].Get(key);
                }
            }

            /// <summary>
            /// When enabled, uses parallel execution while sweeping tables, otherwise sweeps sequentially (default behavior)
            /// </summary>
            [Config]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE)]
            public bool ParallelSweep
            {
                get { return m_ParallelSweep;}
                set { m_ParallelSweep = value;}
            }


            /// <summary>
            /// Returns total number of records in all tables in the store
            /// </summary>
            public int Count
            {
                get { return m_Tables.Sum( (t) => t.Count );}
            }

            /// <summary>
            /// When true, emits instrumentation messages
            /// </summary>
            [Config]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public bool InstrumentationEnabled
            {
                get { return m_InstrumentationEnabled;}
                set { m_InstrumentationEnabled = value;}
            }

             /// <summary>
            /// Returns named parameters that can be used to control this component
            /// </summary>
            public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

            /// <summary>
            /// Returns named parameters that can be used to control this component
            /// </summary>
            public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
            {
              return ExternalParameterAttribute.GetParameters(this, groups);
            }
        #endregion


        #region Public

            /// <summary>
            /// Configures store from node, if node==null then store will be configured by named node of 'app/cache/store[name=X]' path, if such path
            /// is not found the store tries to find 'app/cache/store[!name]' (node without name)
            /// </summary>
            public void Configure(IConfigSectionNode node)
            {
                if (node==null)
                {
                    node = App.ConfigRoot[CONFIG_CACHE_SECTION]
                              .Children
                              .FirstOrDefault(s => s.IsSameName(CONFIG_STORE_SECTION) && s.IsSameNameAttr(Name) );
                    if (node==null)
                    {

                        node = App.ConfigRoot[CONFIG_CACHE_SECTION]
                               .Children
                               .FirstOrDefault(s => s.IsSameName(CONFIG_STORE_SECTION) && !s.AttrByName(Configuration.CONFIG_NAME_ATTR).Exists);
                        if (node==null) return;
                    }
                }

                ConfigAttribute.Apply(this, node);

                m_TableOptions = new Registry<TableOptions>();
                foreach(var tn in node.Children.Where(cn => cn.IsSameName(CONFIG_TABLE_SECTION)) )
                {
                    var tbl = new TableOptions();
                    ((IConfigurable)tbl).Configure(tn);
                    m_TableOptions.Register( tbl );
                }
            }


          /// <summary>
          /// Gets external parameter value returning true if parameter was found
          /// </summary>
          public bool ExternalGetParameter(string name, out object value, params string[] groups)
          {
              return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
          }

          /// <summary>
          /// Sets external parameter value returning true if parameter was found and set
          /// </summary>
          public bool ExternalSetParameter(string name, object value, params string[] groups)
          {
            return ExternalParameterAttribute.SetParameter(this, name, value, groups);
          }

          /// <summary>
          /// Drops table by name returning true if it was found and removed
          /// </summary>
          public bool DropTable(string name)
          {
            if (name==null) return false;
            return m_Tables.Unregister( name );
          }


        #endregion


        #region .pvt

            internal void log(MessageType type, string message, Exception error = null, string parameters = null)
            {
                App.Log.Write(
                        new Message
                        {
                            Text = message ?? string.Empty,
                            Type = type,
                            From = "Cache.MemStore({0})".Args(Name),
                            Topic = CoreConsts.CACHE_TOPIC,
                            Parameters = parameters ?? string.Empty,
                            Exception = error
                        }
                    );
            }


            private void threadSpin()
            {
                 try
                 {
                      var rnd = new Random();
                      var wasActive = App.Active;//remember whether app was active during start
                                                 //this is needed so that CacheStore works without app container (using NOPApplication) in which case
                                                 //service must be .Disposed() to stop this thread
                      while ((App.Active | !wasActive) && m_Running)
                      {
                        try
                        {
                          if (m_ParallelSweep)
                              Parallel.ForEach(m_Tables, (tbl) => tbl.Sweep() );
                          else
                              m_Tables.ForEach( (tbl) => tbl.Sweep() );

                          updateInstruments();
                        }
                        catch(Exception innerE)
                        {
                          log(MessageType.Emergency, " threadSpin().while body leaked exception: "+ innerE.ToMessageWithType(), innerE);
                        }

                        m_Trigger.WaitOne(2000 + rnd.Next(2000));
                      }//while
                 }
                 catch(Exception e)
                 {
                     log(MessageType.CatastrophicError, " threadSpin() leaked exception. Cache is not swept anymore. Error: "+e.ToMessageWithType(), e);
                 }
            }


            private void updateInstruments()
            {
                if (!m_InstrumentationEnabled || !App.Instrumentation.Enabled) return;

                var instr = App.Instrumentation;

                var store_Count = 0;
                var store_PageCount = 0;
                var store_BucketPageLoadFactor = 0d;
                var store_HitCount = 0;
                var store_MissCount = 0;
                var store_ValueFactoryCount = 0;
                var store_SweepTableCount = 0;
                var store_SweepPageCount = 0;
                var store_SweepRemoveCount = 0;
                var store_PutCount = 0;
                var store_PutInsertCount = 0;
                var store_PutReplaceCount = 0;
                var store_PutPageCreateCount = 0;
                var store_PutCollisionCount = 0;
                var store_PutPriorityPreventedCollisionCount = 0;
                var store_RemovePageCount = 0;
                var store_RemoveHitCount = 0;
                var store_RemoveMissCount = 0;
                var cnt = 0;

                foreach(var tbl in m_Tables)
                {
                    var ts = "{0}.{1}".Args(Name, tbl.Name);
                    instr.Record( new Instrumentation.RecordCount(ts, tbl.Count) );            store_Count  += tbl.Count;
                    instr.Record( new Instrumentation.PageCount  (ts, tbl.PageCount) );        store_PageCount  += tbl.PageCount;
                    instr.Record( new Instrumentation.BucketPageLoadFactor  (ts, tbl.BucketPageLoadFactor) ); store_BucketPageLoadFactor  += tbl.BucketPageLoadFactor;
                    instr.Record( new Instrumentation.HitCount   (ts, tbl.stat_HitCount)  );   store_HitCount  += Interlocked.Exchange( ref tbl.stat_HitCount, 0);
                    instr.Record( new Instrumentation.MissCount  (ts, tbl.stat_MissCount) );   store_MissCount += Interlocked.Exchange( ref tbl.stat_MissCount, 0);

                    instr.Record( new Instrumentation.ValueFactoryCount(ts, tbl.stat_ValueFactoryCount) ); store_ValueFactoryCount += Interlocked.Exchange( ref tbl.stat_ValueFactoryCount, 0);

                    instr.Record( new Instrumentation.SweepTableCount (ts, tbl.stat_SweepTableCount) );  store_SweepTableCount  += Interlocked.Exchange( ref tbl.stat_SweepTableCount, 0);
                    instr.Record( new Instrumentation.SweepPageCount  (ts, tbl.stat_SweepPageCount) );   store_SweepPageCount   += Interlocked.Exchange( ref tbl.stat_SweepPageCount, 0);
                    instr.Record( new Instrumentation.SweepRemoveCount(ts, tbl.stat_SweepRemoveCount) ); store_SweepRemoveCount += Interlocked.Exchange( ref tbl.stat_SweepRemoveCount, 0);

                    instr.Record( new Instrumentation.PutCount          (ts, tbl.stat_PutCount) );           store_PutCount           += Interlocked.Exchange( ref tbl.stat_PutCount, 0);
                    instr.Record( new Instrumentation.PutInsertCount    (ts, tbl.stat_PutInsertCount) );     store_PutInsertCount     += Interlocked.Exchange( ref tbl.stat_PutInsertCount, 0);
                    instr.Record( new Instrumentation.PutReplaceCount   (ts, tbl.stat_PutReplaceCount) );    store_PutReplaceCount    += Interlocked.Exchange( ref tbl.stat_PutReplaceCount, 0);
                    instr.Record( new Instrumentation.PutPageCreateCount(ts, tbl.stat_PutPageCreateCount) ); store_PutPageCreateCount += Interlocked.Exchange( ref tbl.stat_PutPageCreateCount, 0);
                    instr.Record( new Instrumentation.PutCollisionCount (ts, tbl.stat_PutCollisionCount) );  store_PutCollisionCount  += Interlocked.Exchange( ref tbl.stat_PutCollisionCount, 0);
                    instr.Record( new Instrumentation.PutPriorityPreventedCollisionCount
                                                                        (ts, tbl.stat_PutPriorityPreventedCollisionCount) );
                                                                                                             store_PutPriorityPreventedCollisionCount
                                                                                                                                      += Interlocked.Exchange( ref tbl.stat_PutPriorityPreventedCollisionCount, 0);

                    instr.Record( new Instrumentation.RemovePageCount(ts, tbl.stat_RemovePageCount) );  store_RemovePageCount += Interlocked.Exchange( ref tbl.stat_RemovePageCount, 0);
                    instr.Record( new Instrumentation.RemoveHitCount (ts, tbl.stat_RemoveHitCount) );   store_RemoveHitCount  += Interlocked.Exchange( ref tbl.stat_RemoveHitCount, 0);
                    instr.Record( new Instrumentation.RemoveMissCount(ts, tbl.stat_RemoveMissCount) );  store_RemoveMissCount += Interlocked.Exchange( ref tbl.stat_RemoveMissCount, 0);

                    cnt++;
                }

                var src = Name;
                instr.Record( new Instrumentation.RecordCount(src, store_Count) );
                instr.Record( new Instrumentation.PageCount  (src, store_PageCount) );
                if (cnt>0d) instr.Record( new Instrumentation.BucketPageLoadFactor(src, store_BucketPageLoadFactor / (double)cnt) );
                instr.Record( new Instrumentation.HitCount   (src, store_HitCount) );
                instr.Record( new Instrumentation.MissCount  (src, store_MissCount) );

                instr.Record( new Instrumentation.ValueFactoryCount(src, store_ValueFactoryCount) );

                instr.Record( new Instrumentation.SweepTableCount (src, store_SweepTableCount) );
                instr.Record( new Instrumentation.SweepPageCount  (src, store_SweepPageCount) );
                instr.Record( new Instrumentation.SweepRemoveCount(src, store_SweepRemoveCount) );

                instr.Record( new Instrumentation.PutCount          (src, store_PutCount) );
                instr.Record( new Instrumentation.PutInsertCount    (src, store_PutInsertCount) );
                instr.Record( new Instrumentation.PutReplaceCount   (src, store_PutReplaceCount) );
                instr.Record( new Instrumentation.PutPageCreateCount(src, store_PutPageCreateCount) );
                instr.Record( new Instrumentation.PutCollisionCount (src, store_PutCollisionCount) );
                instr.Record( new Instrumentation.PutPriorityPreventedCollisionCount (src, store_PutPriorityPreventedCollisionCount) );

                instr.Record( new Instrumentation.RemovePageCount(src, store_RemovePageCount) );
                instr.Record( new Instrumentation.RemoveHitCount (src, store_RemoveHitCount) );
                instr.Record( new Instrumentation.RemoveMissCount(src, store_RemoveMissCount) );


                if (store_Count>0)
                {
                  var entropySample = (cnt << (19 + (store_Count % 7))) ^
                                      (store_PageCount << (store_PutInsertCount % 17)) ^
                                      (store_HitCount << (store_MissCount % 13)) ^
                                       store_Count;

                  NFX.ExternalRandomGenerator.Instance.FeedExternalEntropySample( entropySample );
               }
            }


        #endregion


    }
}
