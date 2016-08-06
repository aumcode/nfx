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

namespace NFX.DataAccess.Cache
{
    /// <summary>
    /// Represents a table that stores cached items identified by keys
    /// </summary>
    public sealed class Table : INamed
    {
        #region CONSTS

            public const int MAX_AGE_SEC_MINIMUM = 5;

            public const int MAX_AGE_SEC_DEFAULT = 5 *//min
                                                   60;//sec


            public const int BUCKET_COUNT_DEFAULT = 25111; //must be prime
            public const int REC_PER_PAGE_DEFAULT = 7;   //must be prime
        #endregion


        #region .ctor


            internal Table(CacheStore store, string name, TableOptions opt)
            {
                var bucketCount = opt!=null ? opt.BucketCount : 0;
                var recPerPage  = opt!=null ? opt.RecPerPage  : 0;
                var lockCount   = opt!=null ? opt.LockCount   : 0;

                if (opt!=null)
                {
                  MaxAgeSec = opt.MaxAgeSec;
                  ParallelSweep = opt.ParallelSweep;
                }


                Debug.Assert(store!=null);
                Debug.Assert(name!=null);

                if (bucketCount<=0) bucketCount = BUCKET_COUNT_DEFAULT;
                if (recPerPage<=0) recPerPage = REC_PER_PAGE_DEFAULT;

                if (bucketCount==recPerPage)
                {
                    recPerPage += 7;//prime
                    store.log(Log.MessageType.Warning,
                               StringConsts.CACHE_TABLE_CTOR_SIZES_WARNING,
                               parameters: "Table: {0} BucketCount: {1} RecPerPage: {2}".Args(name, bucketCount, recPerPage) );
                }

                m_Store = store;
                m_Name = name;
                m_BucketCount = bucketCount;
                m_RecPerPage = recPerPage;
                m_Buckets = new Bucketed[m_BucketCount];

                m_LockCount = lockCount>0 ? lockCount : System.Environment.ProcessorCount * 16;
                m_Locks = new object[m_LockCount];
                for(var i=0; i<m_LockCount; i++)
                    m_Locks[i] = new object();
            }
        #endregion

        #region Private
            private CacheStore m_Store;
            private string m_Name;

            private int m_LockCount;
            private int m_BucketCount;
            private int m_RecPerPage;

            private object[] m_Locks;

            private Bucketed[] m_Buckets;

            private int m_MaxAgeSec = MAX_AGE_SEC_DEFAULT;

            private int m_Count;
            private int m_PageCount;

            private bool m_ParallelSweep;

            //statistics/instrumentation
            internal int stat_LastTime;

            /// <summary>
            /// how many hits - get was called
            /// </summary>
            internal int stat_HitCount;

            /// <summary>
            /// how many complex key hits - get was called
            /// </summary>
            internal int stat_ComplexHitCount;

            /// <summary>
            /// how many misses - get was called
            /// </summary>
            internal int stat_MissCount;

            /// <summary>
            /// how many complex key misses - get was called
            /// </summary>
            internal int stat_ComplexMissCount;

            /// <summary>
            /// how many times factory called from GetOrPut
            /// </summary>
            internal int stat_ValueFactoryCount;

            /// <summary>
            /// how many times swept
            /// </summary>
            internal int stat_SweepTableCount;

            /// <summary>
            /// how many pages swept
            /// </summary>
            internal int stat_SweepPageCount;

            /// <summary>
            /// how many records removed by sweep
            /// </summary>
            internal int stat_SweepRemoveCount;



            /// <summary>
            /// how many times put was called
            /// </summary>
            internal int stat_PutCount;


            /// <summary>
            /// how many times new value successfully inserted without collision
            /// </summary>
            internal int stat_PutInsertCount;


            /// <summary>
            /// how many times new value successfully replaced existing one (by the same key) without collision
            /// </summary>
            internal int stat_PutReplaceCount;

            /// <summary>
            /// how many times bucket collision occured that resulted in page creation
            /// </summary>
            internal int stat_PutPageCreateCount;


            /// <summary>
            /// how many times new value overrode existing because of collision
            /// </summary>
            internal int stat_PutCollisionCount;

            /// <summary>
            /// how many times priority prevented collision
            /// </summary>
            internal int stat_PutPriorityPreventedCollisionCount;

            /// <summary>
            /// how many pages have been deleted, a page gets deleted when there are no records stored in it
            /// </summary>
            internal int stat_RemovePageCount;

            /// <summary>
            /// how many records have been found and removed
            /// </summary>
            internal int stat_RemoveHitCount;

            /// <summary>
            /// how many records have been sought to be removed but were not found
            /// </summary>
            internal int stat_RemoveMissCount;

        #endregion


        #region Properties
            /// <summary>
            /// Returns the store instance that this table is a part of
            /// </summary>
            public CacheStore Store { get { return m_Store;} }

            /// <summary>
            /// Returns table name which is a unique string within the cache store
            /// </summary>
            public string Name { get { return m_Name;} }

            /// <summary>
            /// Returns item count in the table
            /// </summary>
            public int Count { get{ return m_Count;} }

            /// <summary>
            /// Returns page count in the table
            /// </summary>
            public int PageCount { get{ return m_PageCount;} }

            /// <summary>
            /// Returns hit count stats
            /// </summary>
            public int StatHitCount { get{ return stat_HitCount;} }

            /// <summary>
            /// Returns hit count stats for using complex keys
            /// </summary>
            public int StatComplexHitCount { get{ return stat_ComplexHitCount;} }

            /// <summary>
            /// Returns miss count stats
            /// </summary>
            public int StatMissCount { get{ return stat_MissCount;} }

            /// <summary>
            /// Returns miss count stats for using complex keys
            /// </summary>
            public int StatComplexMissCount { get{ return stat_ComplexMissCount;} }


            /// <summary>
            /// Returns the ratio of how many buckets are loaded with pages vs. bucket count
            /// </summary>
            public double BucketPageLoadFactor { get{ return m_PageCount / (double)m_BucketCount;} }


            /// <summary>
            /// Returns the maximum number of items that this table can hold at any given time given that
            ///  no items will have any key hash collisions
            /// </summary>
            public int Capacity { get{ return m_BucketCount * m_RecPerPage;}}


            /// <summary>
            /// Returns how many locks can be used for thread coordination during table changes
            /// </summary>
            public int LockCount { get{ return m_LockCount;} }


            /// <summary>
            /// Returns how many slots/buckets are pre-allocated per table, the higher the number the more memory will be
            /// reserved at table construction time, every slot is a reference (4 bytes on 32bit systems, 8 bytes on 64bit).
            /// For optimal performance this number should be around 75% of total record count stored in the table (table load factor).
            /// </summary>
            public int BucketCount { get{ return m_BucketCount;} }

            /// <summary>
            /// Returns how many slots are pre-allocated per table's bucket(page) when more than one item produces hash collision.
            /// The higher the number, the more primary hash collisions can be accomodated by re-hashing on pages (secondary hash table within primary buckets)
            /// </summary>
            public int RecPerPage { get { return m_RecPerPage;}}

            /// <summary>
            /// Gets/sets maximum age of items in the table expressed in seconds. After this age is exceeded, the system will delete entries.
            /// The system does not guarantee that items will expire right on time, however it does guarantee that items will be available for at least
            ///  this long.
            /// </summary>
            public int MaxAgeSec
            {
                get { return m_MaxAgeSec; }
                set
                {
                    if (value==0)
                      value = MAX_AGE_SEC_DEFAULT;
                    else
                      if (value<MAX_AGE_SEC_MINIMUM) value = MAX_AGE_SEC_MINIMUM;

                    m_MaxAgeSec = value;
                }
            }

            /// <summary>
            /// When enabled, uses parallel execution while sweeping table buckets, otherwise sweeps sequentially (default behavior)
            /// </summary>
            public bool ParallelSweep
            {
                get { return m_ParallelSweep;}
                set { m_ParallelSweep = value;}
            }

        #endregion

        #region Public


            /// <summary>
            /// Puts a key-identified item into this table.
            /// If item with such key is already in this table then replaces it and returns false, returns true otherwise
            /// </summary>
            /// <param name="key">Item's unique key</param>
            /// <param name="value">Item</param>
            /// <param name="maxAgeSec">For how long will the item exist in cache before it gets swept out. Pass 0 to use table-level setting (default)</param>
            /// <param name="priority">Items priority relative to others in the table used during collision resolution, 0 = default</param>
            /// <param name="absoluteExpirationUTC">Sets absolute UTC time stamp when item should be swept out of cache, null is default</param>
            public bool Put(ulong key, object value, int maxAgeSec = 0, int priority = 0, DateTime? absoluteExpirationUTC = null)
            {
                CacheRec rec;
                return Put(key, value, out rec, maxAgeSec, priority, absoluteExpirationUTC);
            }

            /// <summary>
            /// Puts a key-identified item into this table.
            /// If item with such key is already in this table then replaces it and returns false, returns true otherwise
            /// </summary>
            /// <param name="key">Item's unique key</param>
            /// <param name="value">Item</param>
            /// <param name="rec">Returns new or existing CacheRec</param>
            /// <param name="maxAgeSec">For how long will the item exist in cache before it gets swept out. Pass 0 to use table-level setting (default)</param>
            /// <param name="priority">Items priority relative to others in the table used during collision resolution, 0 = default</param>
            /// <param name="absoluteExpirationUTC">Sets absolute UTC time stamp when item should be swept out of cache, null is default</param>
            public bool Put(ulong key, object value, out CacheRec rec, int maxAgeSec = 0, int priority = 0, DateTime? absoluteExpirationUTC = null)
            {
                Interlocked.Increment(ref stat_PutCount);
                return _Put(key, value, out rec, maxAgeSec, priority, absoluteExpirationUTC);
            }

            private bool _Put(ulong key, object value, out CacheRec rec, int maxAgeSec, int priority, DateTime? absoluteExpirationUTC)
            {
                if (value==null)
                    throw new NFXException(StringConsts.ARGUMENT_ERROR + "Cache.TablePut(item=null)");

                var idx = getBucketIndex( key );

                var lck = m_Locks[ getLockIndex(idx) ];
                lock(lck)
                {
                    var bucketed = m_Buckets[idx];

                    if (bucketed==null)
                    {
                        rec = new CacheRec(key, value, maxAgeSec, priority, absoluteExpirationUTC);
                        m_Buckets[idx] = rec;
                        Interlocked.Increment(ref m_Count);
                        Interlocked.Increment(ref stat_PutInsertCount);
                        return true;
                    }
                    else
                    {
                        if (bucketed is Page)
                        {
                             var page = (Page)bucketed;

                             lock(page)
                             {
                                 var pidx = getPageIndex( key );

                                 var existing = page.m_Records[pidx];
                                 if (existing!=null)
                                 {
                                    if (existing.Key==key) //reuse the CacheRec instance
                                    {
                                        existing.ReuseCTOR(value, maxAgeSec, priority, absoluteExpirationUTC);
                                        Interlocked.Increment(ref stat_PutReplaceCount);
                                        rec = existing;
                                        return false;
                                    }

                                    if (existing.m_Priority<=priority)
                                    {
                                        rec = new CacheRec(key, value, maxAgeSec, priority, absoluteExpirationUTC);
                                        page.m_Records[pidx] = rec;
                                        Interlocked.Increment(ref stat_PutCollisionCount);
                                        return false;
                                    }
                                    rec = existing;
                                    Interlocked.Increment(ref stat_PutPriorityPreventedCollisionCount);
                                    return false;
                                 }
                                 rec = new CacheRec(key, value, maxAgeSec, priority, absoluteExpirationUTC);
                                 page.m_Records[pidx] = rec;
                                 Interlocked.Increment(ref stat_PutInsertCount);
                             }
                             Interlocked.Increment(ref m_Count);
                             return true;
                        }
                        else
                        {
                            var existing = (CacheRec)bucketed;
                            if (existing.Key==key)
                            {
                                existing.ReuseCTOR(value, maxAgeSec, priority, absoluteExpirationUTC);
                                rec = existing;
                                Interlocked.Increment(ref stat_PutReplaceCount);
                                return false;
                            }
                            else
                            {//1st collision
                                var pidx = getPageIndex( existing.Key );
                                var npidx = getPageIndex( key );

                                if (npidx==pidx)//collision
                                {
                                    if (existing.m_Priority<=priority)
                                    {
                                        rec = new CacheRec(key, value, maxAgeSec, priority, absoluteExpirationUTC);
                                        m_Buckets[idx] = rec;
                                        Interlocked.Increment(ref stat_PutCollisionCount);
                                        return false;
                                    }

                                    rec = existing;
                                    Interlocked.Increment(ref stat_PutPriorityPreventedCollisionCount);
                                    return false;
                                }

                                // turn CacheRec into a Page
                                var page = new Page(m_RecPerPage);
                                Interlocked.Increment(ref stat_PutPageCreateCount);
                                Interlocked.Increment(ref m_PageCount);
                                page.m_Records[pidx] = existing;

                                rec =  new CacheRec(key, value, maxAgeSec, priority, absoluteExpirationUTC);
                                page.m_Records[npidx] = rec;
                                Thread.MemoryBarrier();
                                m_Buckets[idx] = page;//assign page to backet AFTER page init above

                                Interlocked.Increment(ref m_Count);
                                Interlocked.Increment(ref stat_PutInsertCount);
                                return true;
                            }
                        }
                    }
                }//lock
            }

            /// <summary>
            /// Removes a key-identified item from the named table returning true when item was deleted
            ///  or false when item was not found
            /// </summary>
            public bool Remove(ulong key)
            {
                var result = _Remove(key);
                if (result)
                    Interlocked.Increment(ref stat_RemoveHitCount);
                else
                    Interlocked.Increment(ref stat_RemoveMissCount);

                return result;
            }

            private bool _Remove(ulong key)
            {
                var idx = getBucketIndex( key );

                var lck = m_Locks[ getLockIndex(idx) ];
                lock(lck)
                {
                    var bucketed = m_Buckets[idx];
                    if (bucketed==null) return false;

                    if (bucketed is Page)
                    {
                       var page = (Page)bucketed;
                       lock(page)
                       {
                           var pidx = getPageIndex( key );
                           var existing = page.m_Records[pidx];
                           if (existing==null) return false;

                           if (existing.Key!=key) return false;//keys dont match

                           existing.Dispose();
                           page.m_Records[pidx] = null;

                           if (page.m_Records.All(r=>r==null))
                           {
                             m_Buckets[idx] = null;
                             Interlocked.Increment(ref stat_RemovePageCount);
                             Interlocked.Decrement(ref m_PageCount);
                           }
                       }
                       Interlocked.Decrement(ref m_Count);
                       return true;
                    }
                    else
                    {
                       var existing = (CacheRec)bucketed;
                       if (existing.Key!=key) return false;//keys dont match

                       bucketed.Dispose();
                       m_Buckets[idx] = null;
                       Interlocked.Decrement(ref m_Count);
                       return true;
                    }
                }//lock
            }

            /// <summary>
            /// Retrieves an item from this table by key where item age is less or equal to requested, or
            ///  calls the itemFactory function and inserts the value in the store
            /// </summary>
            /// <param name="key">Item key</param>
            /// <param name="valueFactory">A function that returns new value for the specified tableName, key, and context</param>
            /// <param name="factoryContext">An object to pass into the factory function if it gets invoked</param>
            /// <param name="ageSec">Age of item in seconds, or 0 for any age</param>
            /// <param name="putMaxAgeSec">MaxAge for item if Put is called</param>
            /// <param name="putPriority">Priority for item if Put is called</param>
            /// <param name="putAbsoluteExpirationUTC">Absolute expiration UTC timestamp for item if Put is called</param>
            /// <typeparam name="TContext">A type of item factory context</typeparam>
            public CacheRec GetOrPut<TContext>(ulong key,
                                               Func<string, ulong, TContext, object> valueFactory,
                                               TContext factoryContext = default(TContext),
                                               int ageSec = 0,
                                               int putMaxAgeSec = 0,
                                               int putPriority = 0,
                                               DateTime? putAbsoluteExpirationUTC = null)
            {
                var result = Get(key, ageSec);//1st check is lock-free
                if (result != null) return result;

                var idx = getBucketIndex( key );

                var lck = m_Locks[ getLockIndex(idx) ];
                lock(lck)
                {
                    result = Get(key, ageSec); //dbl-check under lock
                    if (result!=null) return result;

                    object value = null;
                    try
                    {
                        value = valueFactory(Name, key, factoryContext);
                        Interlocked.Increment(ref stat_ValueFactoryCount);
                    }
                    catch(Exception error)
                    {
                        throw new NFXException(StringConsts.CACHE_VALUE_FACTORY_ERROR.Args("Cache.Table.GetOrPut()", error.ToMessageWithType()), error);
                    }

                    CacheRec rec;
                    this.Put(key, value, out rec, putMaxAgeSec, putPriority, putAbsoluteExpirationUTC);
                    return rec;
                }//lock
            }

            /// <summary>
            /// Retrieves an item from this table by key where item age is less or equal to requested, or null if it does not exist
            /// </summary>
            /// <param name="key">Item key</param>
            /// <param name="ageSec">Age of item in seconds, or 0 for any age</param>
            public CacheRec Get(ulong key, int ageSec = 0)
            {
                var result = _Get(key, ageSec);
                if (result!=null)
                {
                    Interlocked.Increment(ref stat_HitCount);
                   #pragma warning disable 420
                    Interlocked.Increment(ref result.m_HitCount);
                   #pragma warning restore 420
                }
                else
                    Interlocked.Increment(ref stat_MissCount);
                return result;
            }

            private CacheRec _Get(ulong key, int ageSec)
            {
                //note: This implementation is not thread-safe in terms of data accuracy, but it does not have to be
                //as this whole cache solution provides/finds "any data per key not older than X" as soon as possible, so
                //the fact that some other thread may have mutated the CacheRec object does not matter

                CacheRec result;

                var idx = getBucketIndex( key );
                var bucketed = m_Buckets[idx]; //atomic

                if (bucketed==null) return null;
                if (bucketed is CacheRec)
                {
                    result = (CacheRec)bucketed;

                    if (result.Key!=key) return null;//keys dont match

                    if (ageSec>0 && result.m_AgeSec > ageSec) return null;//expired

                    return result;
                }
                else
                {
                    var page = (Page)bucketed;
                    var pidx = getPageIndex( key );
                    result = page.m_Records[pidx];//atomic
                    if (result==null) return null;

                    if (result.Key!=key) return null;//keys dont match

                    if (ageSec>0 && result.m_AgeSec > ageSec) return null;//expired

                    return result;
                }
            }




        #endregion


        #region .pvt/.internal

            private int getLockIndex(int bucketIndex)
            {
                var r = (bucketIndex % m_LockCount);
                return (r >= 0) ? r : -r;
            }

            private int getBucketIndex(ulong key)
            {
                var hc = (int)(key>>32) ^ (int)key;
                var r = (hc % m_BucketCount);
                return (r >= 0) ? r : -r;
            }

            private int getPageIndex(ulong key)
            {
                var hc = (int)(key>>32) ^ (int)key;
                var r = (hc % m_RecPerPage);
                return (r >= 0) ? r : -r;
            }


            //called from another thread
            internal void Sweep()
            {
                var now = App.TimeSource.UTCNow;

                if (m_ParallelSweep)
                    Parallel.For(0, m_BucketCount, (bidx, loop) =>
                                                   {
                                                     sweepBucket(now, m_Buckets[bidx]);
                                                     if (!m_Store.isRunning) loop.Stop();
                                                   });
                else
                    for(var i=0; m_Store.isRunning && i< m_BucketCount; i++)
                    {
                      sweepBucket(now, m_Buckets[i]);
                    }

                Interlocked.Increment(ref stat_SweepTableCount);
            }

            private void sweepBucket(DateTime now,  Bucketed bucketed)
            {
                if (bucketed==null) return;
                if (bucketed is CacheRec)
                {
                   sweepCacheRec(now, (CacheRec)bucketed);
                   return;
                }

                var page = (Page)bucketed;
                for(int i=0; i<m_RecPerPage; i++)
                {
                    var rec = page.m_Records[i];
                    if (rec!=null)
                        sweepCacheRec(now, rec);
                }
                Interlocked.Increment(ref stat_SweepPageCount);
            }

            private void sweepCacheRec(DateTime now,  CacheRec rec)
            {
                var age = 0;
                var cd = rec.m_CreateDate;
                if (cd.Ticks!=0)
                {
                    age = (int)(now - cd).TotalSeconds;
                    rec.m_AgeSec = age; // the Age field is needed for efficency not to compute dates on Table.Get()
                }
                else
                {
                    rec.m_CreateDate = now;
                    return;
                }

                var rma = rec.m_MaxAgeSec;
                var axp = rec.m_AbsoluteExpirationUTC;
                if (
                    (age > ( rma>0 ? rma : this.m_MaxAgeSec)) ||
                    (axp.HasValue && now > axp.Value)
                   )
                {
                   this.Remove(rec.Key);//delete expired
                   Interlocked.Increment(ref stat_SweepRemoveCount);
                }

            }


        #endregion
    }
}
