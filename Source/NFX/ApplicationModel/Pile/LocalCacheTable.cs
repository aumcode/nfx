/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
/*
 * Author: Dmitriy Khmaladze, Spring 2015  dmitriy@itadapter.com
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace NFX.ApplicationModel.Pile
{

  internal abstract class LocalCacheTable : ICacheTable
  {
    public const int BUCKETS = 256;

    protected static readonly object[] s_GetPutLocks;


    static LocalCacheTable()
    {
      s_GetPutLocks = new object[ IntMath.GetPrimeCapacityOfAtLeast( System.Environment.ProcessorCount * 2)];
      for(var i=0; i<s_GetPutLocks.Length; i++)
        s_GetPutLocks[i] = new object();
    }


    protected LocalCacheTable(LocalCache cache, string name, TableOptions options)
    {
       m_Name = name;
       m_Cache = cache;
       m_Options = options!=null ? options.Clone() : new TableOptions(name);
       m_Options.m_Name = name;
       m_CollisionMode = m_Options.CollisionMode;
    }

    private string m_Name;
    protected internal readonly LocalCache m_Cache;

    protected internal readonly TableOptions m_Options;

    protected internal readonly CollisionMode m_CollisionMode;

    protected internal int m_stat_Put;
    protected internal int m_stat_PutCollision;
    protected internal int m_stat_PutOverwrite;
    protected internal int m_stat_PutReplace;
    protected internal int m_stat_RemoveHit;
    protected internal int m_stat_RemoveMiss;
    protected internal long m_stat_Sweep;
    protected internal long m_stat_SweepDuration;
    protected internal int m_stat_RejuvenateHit;
    protected internal int m_stat_RejuvenateMiss;
    protected internal int m_stat_GetHit;
    protected internal int m_stat_GetMiss;

    protected internal int m_stat_Grew;
    protected internal int m_stat_Shrunk;




    public string Name { get{ return m_Name;} }

    public ICache Cache{ get { return m_Cache;} }

    public TableOptions Options{ get { return m_Options;} }

    public CollisionMode CollisionMode{ get{ return m_CollisionMode;} }

    public abstract long Count {  get; }
    public abstract long Capacity {  get; }
    public abstract double LoadFactor {  get; }

    protected internal abstract bool Sweep(Stopwatch timer, int maxTimeMs);


    internal DateTime? SweepWhenBecameEmpty;

    public void Purge()
    {
      if (!m_Cache.Running) return;
      DoPurge();
    }

    internal abstract void DoPurge();
  }



  internal sealed class LocalCacheTable<TKey> : LocalCacheTable, ICacheTable<TKey>
  {
    private static readonly int CPU_COUNT = System.Environment.ProcessorCount;
    private const int PROBE_COUNT = 5;


                   private struct _cacheentry : ICacheEntry<TKey>
                   {
                     public _cacheentry(_entry data, object value)
                     {
                        m_Data = data;
                        m_Value = value;
                     }

                     private _entry m_Data;
                     private  object m_Value;

                     TKey       ICacheEntry<TKey>.Key             { get{ return m_Data.Key;} }
                     int        ICacheEntry<TKey>.AgeSec          { get{ return m_Data.AgeSec;} }
                     int        ICacheEntry<TKey>.Priority        { get{ return m_Data.Priority;} }
                     int        ICacheEntry<TKey>.MaxAgeSec       { get{ return m_Data.MaxAgeSec;} }
                     DateTime?  ICacheEntry<TKey>.ExpirationUTC   { get{ return m_Data.ExpirationUTC.Ticks==0 ? (DateTime?)null : m_Data.ExpirationUTC;} }
                     object     ICacheEntry<TKey>.Value           { get{ return m_Value; } }
                   }


                   //Used for durable mode chaining
                   [Serializable]
                   private class _chain
                   {
                      public List<_entry> Links;
                   }

                   [Serializable]
                   private struct _entry
                   {
                     public TKey Key;
                     public int AgeSec;
                     public int Priority;
                     public int MaxAgeSec;
                     public DateTime ExpirationUTC;//if not set then DateTime(0)
                     public PilePointer DataPointer;//!Valid then slot is free

                     public _entry(PilePointer ppChain)
                     {
                       Key = default(TKey);
                       AgeSec = -1;//flag that this entry is a pointer to chain
                       Priority = 0;
                       MaxAgeSec= 0;
                       ExpirationUTC = new DateTime(0);
                       DataPointer = ppChain;
                     }

                     public bool IsChain{ get{ return AgeSec < 0;}}

                     public static readonly _entry Empty = new _entry{DataPointer = PilePointer.Invalid};
                   }

                   private class _bucket
                   {
                     public _bucket(LocalCacheTable<TKey> table)
                     {
                       Table = table;
                       var capacity = (int)(table.m_Options.InitialCapacity) / BUCKETS;
                       capacity = IntMath.GetPrimeCapacityOfAtLeast(capacity);
                       Entries = makeEntriesArray(capacity);
                     }

                     public readonly LocalCacheTable<TKey> Table;
                     public _entry[] Entries;

                     //be careful not to make this field readonly as interlocked(ref) just does not work in runtime
                     public OS.ManyReadersOneWriterSynchronizer RWSynchronizer;


                     public long COUNT;


                     public double LoadFactor
                     {
                        get{ return COUNT / (double)(Entries.Length);}
                     }

                     public long Capacity
                     {
                        get{ return Entries.Length;}
                     }


                       private DateTime m_LastGrownTime;

                     public bool growIfNeeded()//must be called under write lock
                     {
                       var loadf = LoadFactor;
                       var options = Table.m_Options;

                       if (loadf > options.LoadFactorHWM)
                       {
                         var newCapacity = IntMath.GetCapacityFactoredToPrime(Entries.Length, options.GrowthFactor);
                         if (options.MaximumCapacity>0)
                         {
                           var capacityPerBucket = (int)(options.MaximumCapacity / BUCKETS);
                           if (newCapacity>capacityPerBucket) newCapacity = IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(capacityPerBucket);
                         }
                         if (resize(newCapacity))
                         {
                           Interlocked.Increment(ref Table.m_stat_Grew);
                           m_LastGrownTime = DateTime.UtcNow;
                           return true;
                         }
                       }
                       return false;
                     }

                      private DateTime m_LastShrunkTime;

                     public bool shrinkIfNeeded()//must be called under write lock
                     {
                       const int SHRINK_WAIT_AFTER_LAST_GROW_SEC = 3 * 60;
                       const int SHRINK_WAIT_AFTER_LAST_SHRINK_SEC = 2 * 60;

                       var loadf = LoadFactor;
                       var options = Table.m_Options;

                       if (loadf < options.LoadFactorLWM)
                       {
                         var utcNow = DateTime.UtcNow;

                         if (
                             ((utcNow - m_LastGrownTime).TotalSeconds > SHRINK_WAIT_AFTER_LAST_GROW_SEC)&&
                             ((utcNow - m_LastShrunkTime).TotalSeconds > SHRINK_WAIT_AFTER_LAST_SHRINK_SEC)
                            )
                         {
                             var newCapacity = COUNT>0 ? IntMath.GetCapacityFactoredToPrime(Entries.Length, options.ShrinkFactor) : 0;
                             if (options.MinimumCapacity>0)
                             {
                               var capacityPerBucket = (int)(options.MinimumCapacity / BUCKETS);
                               if (newCapacity<capacityPerBucket) newCapacity = IntMath.GetPrimeCapacityOfAtLeast(capacityPerBucket);
                             }

                             var initialCapacityPerBucket = (int)(options.InitialCapacity / BUCKETS);
                             if (newCapacity<initialCapacityPerBucket) newCapacity = IntMath.GetPrimeCapacityOfAtLeast(initialCapacityPerBucket);

                             if (resize(newCapacity))
                             {
                               Interlocked.Increment(ref Table.m_stat_Shrunk);
                               m_LastShrunkTime = utcNow;
                               return true;
                             }
                         }
                       }
                       return false;
                     }



                     private bool resize(int newCapacity)
                     {
                       if (Entries.Length==newCapacity) return false;

                       var newEntries = makeEntriesArray(newCapacity);
                       var comparer = Table.KeyComparer;
                       var pile = Table.m_Cache.m_Pile;

                       this.COUNT = 0;

                       if (Table.m_CollisionMode==CollisionMode.Durable)
                       {
                         for(var i=0; i<Entries.Length; i++)
                         {
                           var entry = Entries[i];
                           if (!entry.DataPointer.Valid) continue;

                           int hcode;

                           if (entry.IsChain)
                           {
                             var chain = pile.Get( entry.DataPointer ) as _chain;
                             pile.Delete( entry.DataPointer);//delete old _chain
                             foreach(var clentry in chain.Links)
                             {
                               hcode = comparer.GetHashCode( clentry.Key );
                               Table.putEntry(newEntries,
                                          clentry.Key,
                                          hcode,
                                          null,
                                          clentry.DataPointer,
                                          clentry.AgeSec,
                                          clentry.MaxAgeSec,
                                          clentry.Priority,
                                          clentry.ExpirationUTC.Ticks!=0 ? (DateTime?)clentry.ExpirationUTC : (DateTime?)null,
                                          null);
                               this.COUNT++;
                             }

                             continue;
                           }//is chain

                           hcode = comparer.GetHashCode( entry.Key );

                           Table.putEntry(newEntries,
                                          entry.Key,
                                          hcode,
                                          null,
                                          entry.DataPointer,
                                          entry.AgeSec,
                                          entry.MaxAgeSec,
                                          entry.Priority,
                                          entry.ExpirationUTC.Ticks!=0 ? (DateTime?)entry.ExpirationUTC : (DateTime?)null,
                                          null);
                           //collision can not happen in durable mode
                           this.COUNT++;
                         }
                       }
                       else//Speculative
                       {
                         for(var i=0; i<Entries.Length; i++)
                         {
                           var entry = Entries[i];
                           if (!entry.DataPointer.Valid) continue;

                           var hcode = comparer.GetHashCode( entry.Key );

                           var putResult = Table.putEntry(newEntries,
                                          entry.Key,
                                          hcode,
                                          null,
                                          entry.DataPointer,
                                          entry.AgeSec,
                                          entry.MaxAgeSec,
                                          entry.Priority,
                                          entry.ExpirationUTC.Ticks!=0 ? (DateTime?)entry.ExpirationUTC : (DateTime?)null,
                                          null);

                           if (putResult==PutResult.Collision)//could not put the item in the new array, so it is lost, need to free pile memory
                             pile.Delete( entry.DataPointer, false);
                          else
                           if (putResult==PutResult.Inserted)
                             this.COUNT++;
                         }
                       }

                       this.Entries = newEntries;

                       return true;
                     }


                    // private DateTime? m_LastSweep;

                     private const int ONE_SWEEP = 128000;
                     private List<DateTime> m_LastSweeps = new List<DateTime>(32);
                     private int m_IdxSweep;

                     public void sweep()
                     {
                       var i = m_IdxSweep * ONE_SWEEP;

                       if (i>=Entries.Length)
                       {
                         while(m_LastSweeps.Count-1>m_IdxSweep) m_LastSweeps.RemoveAt(m_LastSweeps.Count-1);
                         m_IdxSweep = 0;
                         i = 0;
                       }

                       var pile = Table.m_Cache.m_Pile;
                       var now = App.TimeSource.UTCNow;

                       var elapsedSec = 0;
                       if (m_IdxSweep==m_LastSweeps.Count)
                        m_LastSweeps.Add(now);
                       else
                       {
                         elapsedSec = (int)(now - m_LastSweeps[m_IdxSweep]).TotalSeconds;
                         m_LastSweeps[m_IdxSweep] = now;
                       }

                       m_IdxSweep++;

                       var deleted = 0;
                       for(var cnt = 0; cnt<ONE_SWEEP && i<Entries.Length; i++, cnt++)
                       {
                         var entry = Entries[i];
                         var expired = false;
                         if (!entry.DataPointer.Valid) continue;//already blank

                         if (entry.IsChain)
                         {
                                var chain = pile.Get( entry.DataPointer ) as _chain;
                                var mutated = false;
                                for(var j=0; j<chain.Links.Count;)
                                {
                                  var clentry = chain.Links[j];
                                  if (clentry.MaxAgeSec>0)
                                  {
                                    clentry.AgeSec += elapsedSec;
                                    if (clentry.AgeSec > clentry.MaxAgeSec)
                                      expired = true;
                                    else
                                    {
                                      chain.Links[j] = clentry;
                                      mutated = true;
                                    }
                                  }

                                  if (expired || (clentry.ExpirationUTC.Ticks!=0 && now >= clentry.ExpirationUTC))
                                  {
                                    pile.Delete( clentry.DataPointer );
                                    chain.Links.RemoveAt(j);
                                    deleted++;
                                    mutated = true;
                                  }
                                  else j++;
                                }//for all chain links

                                if (mutated || chain.Links.Count==0)
                                {
                                  pile.Delete( entry.DataPointer );
                                  if (chain.Links.Count>0)
                                  {
                                    entry.DataPointer = Table.m_Cache.m_Pile.Put( chain );
                                    Entries[i] = entry;
                                  }
                                  else
                                  {
                                    Entries[i] = _entry.Empty;
                                  }
                                }
                            continue;
                         }//chain


                         //non-chain ----------------------------------
                         if (entry.MaxAgeSec>0)
                         {
                           entry.AgeSec += elapsedSec;
                           if (entry.AgeSec > entry.MaxAgeSec)
                             expired = true;
                           else
                             Entries[i].AgeSec = entry.AgeSec;
                         }

                         if (expired || (entry.ExpirationUTC.Ticks!=0 && now >= entry.ExpirationUTC))
                         {
                           pile.Delete( entry.DataPointer );
                           Entries[i] = _entry.Empty;
                           deleted++;
                         }
                       }//for i


                       this.COUNT-=deleted;
                       shrinkIfNeeded();

                       Interlocked.Add(ref Table.m_stat_Sweep, deleted);
                     }

                     private _entry[] makeEntriesArray(int capacity)
                     {
                       var array = new _entry[capacity];
                       for(var i=0; i<array.Length; i++)
                        array[i].DataPointer = PilePointer.Invalid;
                       return array;
                     }

                     public void purge()
                     {
                       var pile = Table.m_Cache.m_Pile;

                       for(var i=0; i<Entries.Length; i++)
                       {
                         var entry = Entries[i];
                         if (entry.DataPointer.Valid)
                         {
                           if (entry.IsChain)
                           {
                             var chain = pile.Get( entry.DataPointer ) as _chain;
                             foreach(var clentry in chain.Links)
                               pile.Delete( clentry.DataPointer );
                           }
                           pile.Delete( entry.DataPointer );
                           Entries[i] = _entry.Empty;
                         }
                       }

                       this.COUNT = 0;
                     }
                   }//bucket


                   private class _enumerable : IEnumerable<ICacheEntry<TKey>>
                   {
                     public _enumerable(LocalCacheTable<TKey> table, bool withValues)
                     {
                       Table = table;
                       WithValues = withValues;
                     }
                     public  readonly LocalCacheTable<TKey> Table;
                     public  readonly bool                  WithValues;

                     System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){ return this.GetEnumerator(); }
                     public IEnumerator<ICacheEntry<TKey>> GetEnumerator()
                     {
                       return new _enumerator(this);
                     }
                   }//_enumerable

                   private class _enumerator : IEnumerator<ICacheEntry<TKey>>
                   {
                     public _enumerator(_enumerable ctx){ Ctx = ctx; Reset();}

                     public readonly _enumerable Ctx;

                     public void Dispose(){ }

                     private int m_IdxBucket;
                     private int m_IdxEntry;
                     private ICacheEntry<TKey> m_Current;

                     private List<_entry> m_ChainLinks;
                     private int m_IdxChainLinks;

                     object System.Collections.IEnumerator.Current { get{ return this.Current;}}
                     public ICacheEntry<TKey> Current{ get{ return m_Current;}}

                     public bool MoveNext()
                     {
                       if (m_ChainLinks!=null)
                       {
                         while(m_IdxChainLinks<m_ChainLinks.Count)
                         {
                           var entry = m_ChainLinks[m_IdxChainLinks];
                           m_IdxChainLinks++;
                           if (entry.DataPointer.Valid)
                           {
                             object value = null;
                             if (Ctx.WithValues)
                                value = Ctx.Table.m_Cache.m_Pile.Get(entry.DataPointer);

                             m_Current = new _cacheentry(entry, value);
                             return true;
                           }
                         }
                         m_ChainLinks = null;
                       }


                       var buckets = Ctx.Table.m_Buckets;

                       while(m_IdxBucket<buckets.Length)
                       {
                         var bucket = buckets[m_IdxBucket];
                         if (!Ctx.Table.getReadLock(bucket)) return false;
                         try
                         {
                            while(m_IdxEntry<bucket.Entries.Length)
                            {
                              var entry = bucket.Entries[m_IdxEntry];
                              m_IdxEntry++;
                              if (entry.DataPointer.Valid)
                              {
                                  if (entry.IsChain)
                                  {
                                    var chain = Ctx.Table.m_Cache.m_Pile.Get(entry.DataPointer) as _chain;
                                    if (chain.Links.Count==0) continue;
                                    m_ChainLinks = chain.Links;
                                    m_IdxChainLinks = 0;
                                    return MoveNext();
                                  }
                                  else
                                  {
                                    object value = null;
                                    if (Ctx.WithValues)
                                      value = Ctx.Table.m_Cache.m_Pile.Get(entry.DataPointer);

                                    m_Current = new _cacheentry(entry, value);
                                    return true;
                                  }
                              }
                            }
                            m_IdxBucket++;
                            m_IdxEntry = 0;
                         }
                         finally
                         {
                           Ctx.Table.releaseReadLock(bucket);
                         }
                       }
                       return false;
                     }

                     public void Reset()
                     {
                       m_IdxBucket = 0;
                       m_IdxEntry = 0;
                       m_Current = null;
                       m_ChainLinks = null;
                       m_IdxChainLinks = 0;
                     }
                   }//_enumerator



    public LocalCacheTable(LocalCache cache, string name, IEqualityComparer<TKey> comparer, TableOptions options) : base(cache, name, options)
    {
        m_Comparer = comparer ?? EqualityComparer<TKey>.Default;
        m_Buckets = new _bucket[BUCKETS];
        for(var i=0; i<BUCKETS; i++)
         m_Buckets[i] = new _bucket(this);
    }

    IEqualityComparer<TKey> m_Comparer;
    private _bucket[] m_Buckets;


    public IEqualityComparer<TKey> KeyComparer
    {
      get { return m_Comparer; }
    }


    public IEnumerable<ICacheEntry<TKey>> AsEnumerable(bool withValues)
    {
      return new _enumerable(this, withValues);
    }

    /// <summary>
    /// How many records in the instance
    /// </summary>
    public override long Count {  get { return m_Buckets.Sum( b => b.COUNT); } }

    /// <summary>
    /// How many entries are allocated for data
    /// </summary>
    public override long Capacity {  get { return m_Buckets.Sum( b => b.Capacity); } }

    /// <summary>
    /// Load factor for the table
    /// </summary>
    public override double LoadFactor {  get { return m_Buckets.Sum( b => b.LoadFactor) / (double)BUCKETS; } }


    public bool ContainsKey(TKey key, int ageSec = 0)
    {
      if (!m_Cache.Running) return false;
      int hashCode;
      var bucket = getBucket(key, out hashCode);

      if (!getReadLock(bucket)) return false;//Service shutting down
      try
      {
        var entry = fetchExistingEntry(bucket, key, hashCode);
        var ptr = entry.DataPointer;
        if (ptr.Valid)
        {
           if (ageSec==0 || entry.AgeSec < ageSec)
           {
             Interlocked.Increment(ref m_stat_GetHit);
             return true;
           }
        }
      }
      finally
      {
        releaseReadLock(bucket);
      }

      return false;
    }

    public long SizeOfValue(TKey key, int ageSec = 0)
    {
      if (!m_Cache.Running) return 0;
      int hashCode;
      var bucket = getBucket(key, out hashCode);

      if (!getReadLock(bucket)) return 0;//Service shutting down
      try
      {
        var entry = fetchExistingEntry(bucket, key, hashCode);
        var ptr = entry.DataPointer;
        if (ptr.Valid)
        {
           if (ageSec==0 || entry.AgeSec < ageSec)
           {
             Interlocked.Increment(ref m_stat_GetHit);
             return m_Cache.m_Pile.SizeOf(ptr);
           }
        }
      }
      finally
      {
        releaseReadLock(bucket);
      }

      return 0;
    }



    public object Get(TKey key, int ageSec = 0)
    {
      if (!m_Cache.Running) return null;
      int hashCode;
      var bucket = getBucket(key, out hashCode);

      if (!getReadLock(bucket)) return null;//Service shutting down
      try
      {
        var entry = fetchExistingEntry(bucket, key, hashCode);
        var ptr = entry.DataPointer;
        if (ptr.Valid)
        {
           if (ageSec==0 || entry.AgeSec < ageSec)
           {
             Interlocked.Increment(ref m_stat_GetHit);
             return m_Cache.m_Pile.Get(ptr);
           }
        }
      }
      finally
      {
        releaseReadLock(bucket);
      }

      Interlocked.Increment(ref m_stat_GetMiss);
      return null;
    }

    public PutResult Put(TKey key, object obj, int? maxAgeSec = null, int priority = 0, DateTime? absoluteExpirationUTC = null)
    {
      if (obj==null) throw new PileException(StringConsts.ARGUMENT_ERROR+GetType().Name+".Put(obj==null)");
      if (!m_Cache.Running) return PutResult.Collision;

      int hashCode;
      var bucket = getBucket(key, out hashCode);

      var result = PutResult.Collision;
      if (!getWriteLock(bucket)) return PutResult.Collision;//Service shutting down
      try
      {
        var age = maxAgeSec ?? m_Options.DefaultMaxAgeSec;
        result = putEntry(bucket, key, hashCode, obj, age, priority, absoluteExpirationUTC);
        if (result!=PutResult.Collision)
        {
          if (result==PutResult.Inserted) bucket.COUNT++;
          bucket.growIfNeeded(); //the grow is done on calling thread, remove is deferred to sweep
        }
      }
      finally
      {
        releaseWriteLock(bucket);
      }

      switch(result)
      {
        case PutResult.Inserted:    {Interlocked.Increment(ref m_stat_Put); break;}
        case PutResult.Overwritten: {Interlocked.Increment(ref m_stat_PutOverwrite); break;}
        case PutResult.Replaced:    {Interlocked.Increment(ref m_stat_PutReplace); break;}
        default:
         Interlocked.Increment(ref m_stat_PutCollision);
         break;
      }
      return result;
    }


    public bool Remove(TKey key)
    {
      if (!m_Cache.Running) return false;
      int hashCode;
      var bucket = getBucket(key, out hashCode);

      var result = false;
      if (!getWriteLock(bucket)) return false;//Service shutting down
      try
      {
        result = removeEntry(bucket, key, hashCode);
      }
      finally
      {
        releaseWriteLock(bucket);
      }

      if (result)
       Interlocked.Increment(ref m_stat_RemoveHit);
      else
       Interlocked.Increment(ref m_stat_RemoveMiss);

      return result;
    }

    public bool Rejuvenate(TKey key)
    {
      if (!m_Cache.Running) return false;
      int hashCode;
      var bucket = getBucket(key, out hashCode);

      var result = false;
      if (!getWriteLock(bucket)) return false;//Service shutting down
      try
      {
        result = rejuvenateEntry(bucket, key, hashCode);
      }
      finally
      {
        releaseWriteLock(bucket);
      }

      if (result)
       Interlocked.Increment(ref m_stat_RejuvenateHit);
      else
       Interlocked.Increment(ref m_stat_RejuvenateMiss);

      return result;
    }



    public object GetOrPut(TKey key, Func<ICacheTable<TKey>, TKey, object, object> valueFactory, object factoryContext, out PutResult? putNewResult, int ageSec = 0, int putMaxAgeSec = 0, int putPriority = 0, DateTime? putAbsoluteExpirationUTC = null)
    {
      if (valueFactory==null)
       throw new PileCacheException(StringConsts.ARGUMENT_ERROR+GetType().Name+".GetOrPut(valueFactory==null)");

      putNewResult = null;
      if (!m_Cache.Running) return null;
      var result = this.Get(key, ageSec);
      if (result!=null) return result;

      var idx = (key.GetHashCode() & CoreConsts.ABS_HASH_MASK) % s_GetPutLocks.Length;
      lock(s_GetPutLocks[idx])
      {
        result = this.Get(key, ageSec);
        if (result!=null) return result;

        result = valueFactory(this, key, factoryContext);

        putNewResult = this.Put(key, result, putMaxAgeSec, putPriority, putAbsoluteExpirationUTC);
      }

      return result;
    }


    #region .pvt/.intr

      private int m_SweepIdx;
      private long m_SweepDurationMs;

    protected internal override bool Sweep(Stopwatch timer, int maxTimeMs)
    {
      timer.Restart();


      for(; m_SweepIdx<m_Buckets.Length; m_SweepIdx++)
      {
          if (!m_Cache.Running) return false;

          var bucket = m_Buckets[m_SweepIdx];

          if (!getWriteLock(bucket)) return false;
          try
          {
            bucket.sweep();
          }
          finally
          {
            releaseWriteLock(bucket);
          }

          var elapsed = timer.ElapsedMilliseconds;
          m_SweepDurationMs += elapsed;
          if (elapsed > maxTimeMs) break;
      }//for

      if (m_SweepIdx==m_Buckets.Length)
      {
        m_SweepIdx = 0;
        m_stat_SweepDuration = m_SweepDurationMs;
        m_SweepDurationMs = 0;
        return true; //whole table scanned
      }

      return false;
    }


    internal override void DoPurge()
    {
      foreach( var bucket in m_Buckets)
      {
          getWriteLock(bucket, true);
          try
          {
            bucket.purge();
          }
          finally
          {
            releaseWriteLock(bucket);
          }
      }
    }



      private _bucket getBucket(TKey key, out int hashCode)
      {
        hashCode = m_Comparer.GetHashCode( key );
        var ibucket = hashCode & 0xff;//Buckets must be 256, otherwise replace & with  Abs(hashCode % BUCKETS)
        return m_Buckets[ibucket];
      }

      private _entry fetchExistingEntry(_bucket bucket, TKey key, int hashCode)
      {
        var entries = bucket.Entries;
        var entryIdx = (hashCode & CoreConsts.ABS_HASH_MASK) % entries.Length;
        for(var probe=0; probe<PROBE_COUNT; probe++)
        {
          var entry = entries[entryIdx];

          if (entry.DataPointer.Valid)
          {
            if (entry.IsChain)
            {
              var chain = m_Cache.m_Pile.Get( entry.DataPointer) as _chain;
              foreach(var clentry in chain.Links)
                if (m_Comparer.Equals(clentry.Key, key)) return clentry;
            }
            else
            {
              if (m_Comparer.Equals(entry.Key, key)) return entry;
            }
          }
          entryIdx++;
          if (entryIdx==entries.Length) entryIdx = 0;//wrap
        }
        return _entry.Empty;
      }


      private PutResult putEntry(_bucket bucket, TKey key, int hashCode, object data, int maxAgeSec,int priority, DateTime? absoluteExpirationUTC)
      {
        return putEntry(bucket.Entries, key, hashCode, data, PilePointer.Invalid,  0, maxAgeSec, priority, absoluteExpirationUTC, (o) => m_Cache.m_Pile.Put(o));
      }

      private PutResult putEntry(_entry[] entries, TKey key, int hashCode, object data, PilePointer ptrData, int existingAgeSec, int  maxAgeSec,int priority, DateTime? absoluteExpirationUTC, Func<object, PilePointer> fPilePut)
      {
        return m_CollisionMode==Pile.CollisionMode.Durable ?
           putEntryDurable    (entries, key, hashCode, data, ptrData, existingAgeSec, maxAgeSec, absoluteExpirationUTC, fPilePut):
           putEntrySpeculative(entries, key, hashCode, data, ptrData, existingAgeSec, maxAgeSec, priority, absoluteExpirationUTC, fPilePut);
      }

      private PutResult putEntryDurable(_entry[] entries, TKey key, int hashCode, object data, PilePointer ptrData, int existingAgeSec, int  maxAgeSec, DateTime? absoluteExpirationUTC, Func<object, PilePointer> fPilePut)
      {
        var entryLocationIdx = (hashCode & CoreConsts.ABS_HASH_MASK) % entries.Length;
        var entryIdx = entryLocationIdx;


        if (!ptrData.Valid) ptrData = fPilePut(data);//may throw out-of-memory
                                                     //we do not want to corrupt existing item if the new one fails

        for(var probe=0; probe<PROBE_COUNT; probe++)
        {
          var entry = entries[entryIdx];

          if (!entry.DataPointer.Valid)//slot is free
          {
            entry.Key = key;
            entry.DataPointer = ptrData;
            entry.MaxAgeSec = maxAgeSec;
            entry.AgeSec = existingAgeSec;
            entry.Priority = 0;
            entry.ExpirationUTC = absoluteExpirationUTC ?? new DateTime(0);
            entries[entryIdx] = entry;//swap value type
            return PutResult.Inserted;
          }

          if (entry.IsChain)
          {
            var chain = m_Cache.m_Pile.Get( entry.DataPointer ) as _chain;

            var found = false;
            _entry clentry;
            for(var i=0; i<chain.Links.Count; i++)
            {
              clentry = chain.Links[i];
              if (m_Comparer.Equals(clentry.Key, key))
              {
                m_Cache.m_Pile.Delete(clentry.DataPointer);
                clentry.DataPointer = ptrData;
                chain.Links[i] = clentry;
                found = true;
                break;
              }
            }
            if (!found)
            {
              clentry = new _entry();
              clentry.Key = key;
              clentry.DataPointer = ptrData;
              clentry.MaxAgeSec = maxAgeSec;
              clentry.AgeSec = existingAgeSec;
              clentry.Priority = 0;
              clentry.ExpirationUTC = absoluteExpirationUTC ?? new DateTime(0);
              chain.Links.Add( clentry );
            }

            var ptr = m_Cache.m_Pile.Put( chain );//put new
            m_Cache.m_Pile.Delete( entry.DataPointer, false );//delete old, lastly we dont want to corrupt existing
            entry.DataPointer = ptr;
            entries[entryIdx] = entry;
            return found ? PutResult.Replaced : PutResult.Inserted;
          }

          if (m_Comparer.Equals(entry.Key, key))//replace with the same key
          {
            m_Cache.m_Pile.Delete( entry.DataPointer );
            entry.DataPointer = ptrData;
            entry.MaxAgeSec = maxAgeSec;
            entry.AgeSec = existingAgeSec;
            entry.Priority = 0;
            entry.ExpirationUTC = absoluteExpirationUTC ?? new DateTime(0);
            entries[entryIdx] = entry;
            return PutResult.Replaced;
          }


          if (probe==PROBE_COUNT-1)//last element - turn into chain
          {
            var links = new List<_entry>(2);
            links.Add( entry );

            var clentry = new _entry();
            clentry.Key = key;
            clentry.DataPointer = ptrData;
            clentry.MaxAgeSec = maxAgeSec;
            clentry.AgeSec = existingAgeSec;
            clentry.Priority = 0;
            clentry.ExpirationUTC = absoluteExpirationUTC ?? new DateTime(0);
            links.Add( clentry );

            var ptr = m_Cache.m_Pile.Put( new _chain{ Links = links} );

            var chain = new _entry(ptr);
            entries[entryIdx] = chain;
          }
        }//for probe
        return PutResult.Inserted;
      }

      private PutResult putEntrySpeculative(_entry[] entries, TKey key, int hashCode, object data, PilePointer ptrData, int existingAgeSec, int  maxAgeSec, int priority, DateTime? absoluteExpirationUTC, Func<object, PilePointer> fPilePut)
      {
        var entryLocationIdx = (hashCode & CoreConsts.ABS_HASH_MASK) % entries.Length;

        var secondPass = false;
        while(true)
        {
              var entryIdx = entryLocationIdx;
              for(var probe=0; probe<PROBE_COUNT; probe++)
              {
                var entry = entries[entryIdx];

                var sameKey = !secondPass && entry.DataPointer.Valid && m_Comparer.Equals(entry.Key, key) ;

                if (
                      (!secondPass &&
                         ( !entry.DataPointer.Valid ) ||       //slot is free
                         ( sameKey )//same key - replace
                      ) ||
                      (secondPass && entry.Priority <= priority)
                   )
                {
                  if (!ptrData.Valid) ptrData = fPilePut(data);//may throw out-of-memory
                                                               //we do not want to corrupt existing item if the new one fails

                  entry.Key = key;
                  if (entry.DataPointer.Valid)
                    m_Cache.m_Pile.Delete( entry.DataPointer, false );//delete previous pointed-to data

                  entry.DataPointer = ptrData;
                  entry.MaxAgeSec = maxAgeSec;
                  entry.AgeSec = existingAgeSec;
                  entry.Priority = priority;
                  entry.ExpirationUTC = absoluteExpirationUTC ?? new DateTime(0);
                  entries[entryIdx] = entry;//swap value type
                  if (secondPass)
                  {
                    Interlocked.Increment(ref m_stat_PutOverwrite);
                    return PutResult.Overwritten;
                  }
                  return sameKey ? PutResult.Replaced : PutResult.Inserted;
                }
                entryIdx++;
                if (entryIdx==entries.Length) entryIdx = 0;//wrap
              }

          if (secondPass) break;//could not find after 2nd pass

          //could not find at first pass, try second time
          secondPass = true;
        }
        return PutResult.Collision;
      }



      private bool removeEntry(_bucket bucket, TKey key, int hashCode)
      {
        var entries = bucket.Entries;
        var pile = m_Cache.m_Pile;
        var entryIdx = (hashCode & CoreConsts.ABS_HASH_MASK) % entries.Length;
        for(var probe=0; probe<PROBE_COUNT; probe++)
        {
          var entry = entries[entryIdx];

          if (entry.DataPointer.Valid)
          {
            if (entry.IsChain)
            {
              var chain = pile.Get( entry.DataPointer) as _chain;
              for(var i=0; i < chain.Links.Count; i++)
              {
                var clentry = chain.Links[i];
                if (m_Comparer.Equals(clentry.Key, key))
                {
                  pile.Delete( clentry.DataPointer );
                  bucket.COUNT--;
                  chain.Links.RemoveAt(i);
                  pile.Delete( entry.DataPointer );
                  if (chain.Links.Count>0)
                  {
                    var ptr = pile.Put(chain);
                    entry = new _entry( ptr );
                    bucket.Entries[entryIdx] = entry;
                  }
                  else
                    bucket.Entries[entryIdx] = _entry.Empty;
                  return true;
                }
              }
            }
            else
            {
              if (m_Comparer.Equals(entry.Key, key))
              {
                pile.Delete(entry.DataPointer);
                bucket.COUNT--;
                bucket.Entries[entryIdx] = _entry.Empty;
                return true;
              }
            }
          }
          entryIdx++;
          if (entryIdx==entries.Length) entryIdx = 0;//wrap
        }
        return false;
      }

      private bool rejuvenateEntry(_bucket bucket, TKey key, int hashCode)
      {
        var entries = bucket.Entries;
        var pile = m_Cache.m_Pile;
        var entryIdx = (hashCode & CoreConsts.ABS_HASH_MASK) % entries.Length;

        for(var probe=0; probe<PROBE_COUNT; probe++)
        {
          var entry = entries[entryIdx];

          if (entry.DataPointer.Valid)
          {
            if (entry.IsChain)
            {
              var chain = pile.Get( entry.DataPointer) as _chain;
              for(var i=0; i < chain.Links.Count; i++)
              {
                var clentry = chain.Links[i];
                if (m_Comparer.Equals(clentry.Key, key))
                {
                   clentry.AgeSec = 0;
                   chain.Links[i] = clentry;
                   pile.Delete( entry.DataPointer );
                   entry.DataPointer = pile.Put(chain);
                   bucket.Entries[entryIdx] = entry;
                  return true;
                }
              }
            }
            else
            {
              if (m_Comparer.Equals(entry.Key, key))
              {
                bucket.Entries[entryIdx].AgeSec = 0;
                return true;
              }
            }
          }
          entryIdx++;
          if (entryIdx==entries.Length) entryIdx = 0;//wrap
        }
        return false;
      }


      // --------------------------------------------------------------------------------------


      //the reader lock allows to have many readers but only 1 writer
      private bool getReadLock(_bucket bucket)
      {
        return bucket.RWSynchronizer.GetReadLock((_) => !m_Cache.Running);
      }

      private void releaseReadLock(_bucket bucket)
      {
          bucket.RWSynchronizer.ReleaseReadLock();
      }

      //the writer lock allows only 1 writer at a time that conflicts with a single reader
      private bool getWriteLock(_bucket bucket, bool disregardRunning = false)
      {
        return bucket.RWSynchronizer.GetWriteLock((_) => !disregardRunning && !m_Cache.Running);
      }

      private void releaseWriteLock(_bucket bucket)
      {
        bucket.RWSynchronizer.ReleaseWriteLock();
      }

    #endregion

  }


}
