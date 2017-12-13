using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NFX.Time;

namespace NFX.Collections
{
  /// <summary>
  /// Implements a set of T with the optional size limit and optional item lifespan limit.
  /// This class is thread-safe and must be disposed in a deterministic way
  /// </summary>
  public sealed class CappedSet<T> : DisposableObject, IEnumerable<KeyValuePair<T, DateTime>>
  {
    public const int BUCKET_COUNT = 251;
    public const int THREAD_GRANULARITY_MS = 5000;

    private class bucket : Dictionary<T, DateTime>
    {
       public bucket(IEqualityComparer<T> comparer) : base(comparer){}
       public DateTime m_LastLock;
       public int m_ApproximateCount;
    }


    public CappedSet(IEqualityComparer<T> comparer = null)
    {
      m_Comparer = comparer ?? EqualityComparer<T>.Default;
      m_Data = new bucket[BUCKET_COUNT];

      for(var i=0; i<m_Data.Length; i++)
        m_Data[i] = new bucket(m_Comparer);

      Task.Delay(THREAD_GRANULARITY_MS).ContinueWith( _ => visit() );
    }


    private IEqualityComparer<T> m_Comparer;
    private int m_SizeLimit;
    private int m_TimeLimitSec;
    private bucket[] m_Data;


    /// <summary>
    /// If set > 0, imposes a limit on the maximum number of entries.
    /// When Count exceeds the limit the set asynchronously capps the capacity by
    /// removing older entries. The limit does not guarantee the instant or exact consistency with the Count property
    /// </summary>
    public int SizeLimit
    {
       get { return m_SizeLimit;}
       set { m_SizeLimit = value > 0 ? value : 0;}
    }

    /// <summary>
    /// If set >0 then evicts the entries older than the specified value.
    /// Does not guarantee the exact consistency of eviction time
    /// </summary>
    public int TimeLimitSec
    {
       get { return m_TimeLimitSec;}
       set { m_TimeLimitSec = value > 0 ? value : 0;}
    }

    /// <summary>
    /// Returns the number of members in the set
    /// </summary>
    public int Count
    {
      get{ return m_Data.Sum( (b) => { lock(b) return b.Count;} ); }
    }

    /// <summary>
    /// Atomic operation which tries to include a member in the set. Returns true if member was included.
    /// </summary>
    public bool Put(T item)
    {
      var data = getBucket(item);
      lock(data)
      {
        if (data.ContainsKey(item)) return false;
        data.Add(item, App.TimeSource.UTCNow);
        return true;
      }
    }


    /// <summary>
    /// Atomic operation which tries to get a member data returning true if it was found
    /// </summary>
    public bool Get(T item, out DateTime createDate)
    {
      var data = getBucket(item);
      lock(data)
        return data.TryGetValue(item, out createDate);
    }

    /// <summary>
    /// Atomic operation which tries to get a member data returning true if it was found
    /// </summary>
    public bool Contains(T item)
    {
      var data = getBucket(item);
      lock(data)
        return data.ContainsKey(item);
    }

    /// <summary>
    /// Atomic operation which tries to remove and existing memeber returning true
    /// </summary>
    public bool Remove(T item)
    {
      var data = getBucket(item);
      lock(data)
        return data.Remove(item);
    }

    /// <summary>
    /// Removes all items
    /// </summary>
    public void Clear()
    {
      foreach( var d in m_Data)
        lock(d) d.Clear();
    }


    public IEnumerator<KeyValuePair<T, DateTime>> GetEnumerator()
    {
      return all.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return all.GetEnumerator();
    }



    #region .pvt

    private IEnumerable<KeyValuePair<T, DateTime>> all
    {
      get
      {
        for(var i=0; i<m_Data.Length; i++)
        {
          var data = m_Data[i];
          KeyValuePair<T, DateTime>[] copy;
          lock(data)
           copy = data.ToArray();

          foreach(var e in copy)
            yield return e;
        }
      }
    }


    private bucket getBucket(T item)
    {
      var hc = m_Comparer.GetHashCode(item);
      var i = (hc & CoreConsts.ABS_HASH_MASK) % m_Data.Length;
      return m_Data[i];
    }

    private void visit()
    {
      if (Disposed) return;
      try
      {
        visitCore();
      }
      catch(Exception error)
      {
        App.Log.Write( new Log.Message
        {
          Type = Log.MessageType.Error,
          Topic = CoreConsts.APPLICATION_TOPIC,
          From = "{0}.visit()".Args(GetType().Name),
          Text = error.ToMessageWithType(),
          Exception = error
        });
      }
      finally
      {
        if (!DisposeStarted)
          Task.Delay(IntMath.ChangeByRndPct(THREAD_GRANULARITY_MS, 0.25f))
              .ContinueWith( _ => visit() );
      }
    }


    private void visitCore()
    {
      var now = App.TimeSource.UTCNow;
      var toDelete = new HashSet<T>(m_Comparer);


      var ttlSec =  TimeLimitSec;
      var maxCount = SizeLimit;

      var overage = 0;
      if (maxCount>0)
        overage = m_Data.Sum( d => d.m_ApproximateCount ) - maxCount;

      var overagePerBucket = overage / m_Data.Length; //equal distribution is assumed per getBucket()

      for(var i=0; i<m_Data.Length; i++)
      {
        var dict = m_Data[i];
        var mustLock = (now - dict.m_LastLock).TotalMinutes > 5;

        if (!mustLock)
        {
          if (!Monitor.TryEnter(dict)) continue;
        }
        else Monitor.Enter(dict);
        try
        {
          dict.m_LastLock = now;//lock taken now

          toDelete.Clear();

          if (ttlSec>0) //impose time limit
            dict.Where(kvp => ((now - kvp.Value).TotalSeconds > ttlSec))
                .ForEach( kvp => toDelete.Add(kvp.Key));

          //impose count limit
          if (overagePerBucket > toDelete.Count)
          {
            var timeSorted = dict.OrderBy(kvp => kvp.Value);//ascending = older first
            foreach(var item in timeSorted)
             if (overagePerBucket>toDelete.Count)
              toDelete.Add( item.Key );
             else
              break;
          }

          toDelete.ForEach( k => dict.Remove(k) );
          dict.m_ApproximateCount = dict.Count;
        }
        finally
        {
          Monitor.Exit(dict);
        }
      }


    }

    #endregion
  }
}
