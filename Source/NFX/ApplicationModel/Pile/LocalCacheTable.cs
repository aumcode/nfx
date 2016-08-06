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
    }

    private string m_Name;
    protected internal readonly LocalCache m_Cache;

    protected internal readonly TableOptions m_Options;

    protected internal int m_stat_Put;
    protected internal int m_stat_PutCollision;
    protected internal int m_stat_PutOverwrite;
    protected internal int m_stat_PutReplace;
    protected internal int m_stat_RemoveHit;
    protected internal int m_stat_RemoveMiss;
    protected internal int m_stat_Sweep;
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


                   private struct _entry
                   {
                     public TKey Key;
                     public int AgeSec;
                     public int Priority;
                     public int MaxAgeSec;
                     public DateTime ExpirationUTC;//if not set then DateTime(0)
                     public PilePointer DataPointer;//!Valid then slot is free

                     public static _entry Empty
                     {
                       get{ return new _entry{DataPointer = PilePointer.Invalid};}
                     }
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

                       this.COUNT = 0;

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
                           Table.m_Cache.m_Pile.Delete( entry.DataPointer, false);
                        else
                         if (putResult==PutResult.Inserted)
                           this.COUNT++;
                       }

                       this.Entries = newEntries;

                       return true;
                     }


                     private DateTime? m_LastSweep;

                     public void sweep()
                     {

                       var now = App.TimeSource.UTCNow;
                       var elapsedSec = m_LastSweep.HasValue ? (int)((now - m_LastSweep.Value).TotalSeconds) : 0;
                       m_LastSweep = now;

                       var deleted = 0;

                       for(var i=0; i<Entries.Length; i++)
                       {
                         var entry = Entries[i];
                         var expired = false;
                         if (!entry.DataPointer.Valid) continue;//already blank

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
                           Table.m_Cache.m_Pile.Delete( entry.DataPointer, false);
                           Entries[i].DataPointer = PilePointer.Invalid;
                           Entries[i].Key = default(TKey);
                           deleted++;
                         }
                       }//for

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
                       for(var i=0; i<Entries.Length; i++)
                       {
                         var ptr = Entries[i].DataPointer;
                         if (ptr.Valid)
                         {
                           Table.m_Cache.Pile.Delete( ptr );
                           Entries[i].DataPointer = PilePointer.Invalid;
                           Entries[i].Key = default(TKey);
                         }
                       }

                       this.COUNT = 0;
                     }
                   }



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
        int idx;
        var entry = fetchExistingEntry(bucket, key, hashCode, out idx);
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
        int idx;
        var entry = fetchExistingEntry(bucket, key, hashCode, out idx);
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
        int idx;
        var entry = fetchExistingEntry(bucket, key, hashCode, out idx);
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

      if (!getWriteLock(bucket)) return false;//Service shutting down
      try
      {
        int idx;
        var entry = fetchExistingEntry(bucket, key, hashCode, out idx);
        var ptr = entry.DataPointer;
        if (ptr.Valid)
        {
           m_Cache.m_Pile.Delete( ptr, false);//delete pointed-to data
           entry.DataPointer = PilePointer.Invalid;//mark entry as "deleted"
           entry.Key = default(TKey);//in case of object, free the reference
           bucket.Entries[idx] = entry;
           bucket.COUNT--;
           Interlocked.Increment(ref m_stat_RemoveHit);
       //////    bucket.shrinkIfNeeded(); shrinking is done by sweep thread
           return true;
        }
      }
      finally
      {
        releaseWriteLock(bucket);
      }

      Interlocked.Increment(ref m_stat_RemoveMiss);
      return false;
    }

    public bool Rejuvenate(TKey key)
    {
      if (!m_Cache.Running) return false;
      int hashCode;
      var bucket = getBucket(key, out hashCode);

      if (!getWriteLock(bucket)) return false;//Service shutting down
      try
      {
        int idx;
        var entry = fetchExistingEntry(bucket, key, hashCode, out idx);
        var ptr = entry.DataPointer;
        if (ptr.Valid)
        {
           bucket.Entries[idx].AgeSec = 0;//reset age to 0
           Interlocked.Increment(ref m_stat_RejuvenateHit);
           return true;
        }
      }
      finally
      {
        releaseWriteLock(bucket);
      }

      Interlocked.Increment(ref m_stat_RejuvenateMiss);
      return false;
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

      private _entry fetchExistingEntry(_bucket bucket, TKey key, int hashCode, out int entryIdx)
      {
        var entries = bucket.Entries;
        entryIdx = (hashCode & CoreConsts.ABS_HASH_MASK) % entries.Length;
        for(var probe=0; probe<PROBE_COUNT; probe++)
        {
          var entry = entries[entryIdx];
          if (entry.DataPointer.Valid && m_Comparer.Equals(entry.Key, key)) return entry;
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
