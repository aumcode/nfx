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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NFX
{
  /// <summary>
  /// Provides Monitor thread synchronization functionality over lock objects addressable by key(name).
  /// This class is thread-safe. The internal implementation is based on a fixed-size array of Dictionary objects
  /// to minimize inter-locking. Do not allocate/deallocate this class often, instead allocate
  /// once per service that needs to synchronize by keys and call methods on the instance.
  /// </summary>
  public sealed class KeyedMonitor<TKey>
  {
          private class _slot
          {
            public _slot() { RefCount = 1; }
            public int RefCount;
          }


    public KeyedMonitor(IEqualityComparer<TKey> comparer = null)
    {
      m_Buckets = new Dictionary<TKey, _slot>[0xff + 1];
      for(var i=0; i< m_Buckets.Length; i++)
        m_Buckets[i] =  comparer !=null ? new Dictionary<TKey, _slot>(comparer) : new Dictionary<TKey, _slot>();
    }

    private Dictionary<TKey, _slot>[] m_Buckets;

    /// <summary>
    /// Executes an action under a lock() taken on TKey value
    /// </summary>
    public void Synchronized(TKey key, Action action)
    {
      Enter(key);
      try
      {
        action();
      }
      finally
      {
        Exit(key);
      }
    }

    /// <summary>
    /// Executes a function under a lock() taken on TKey value
    /// </summary>
    public TResult Synchronized<TResult>(TKey key, Func<TResult> action)
    {
      Enter(key);
      try
      {
        return action();
      }
      finally
      {
        Exit(key);
      }
    }

    /// <summary>
    /// Performs Monitor.Enter() on TKey value.
    /// Unlike TryEnter() this method does block
    /// </summary>
    public void Enter(TKey key)
    {
      var bucket = getBucket(key);
      _slot _lock;
      lock(bucket)
      {
        if (!bucket.TryGetValue(key, out _lock))
        {
          _lock = new _slot();
          bucket.Add(key, _lock);
        }
        else
          _lock.RefCount++;
      }

      Monitor.Enter(_lock);
    }

    /// <summary>
    /// Tries to perform Monitor.TryEnter() on TKey value.
    /// Returns true when lock was taken. Unlike Enter() this method does not block
    /// </summary>
    public bool TryEnter(TKey key)
    {
      var bucket = getBucket(key);
      _slot _lock;
      lock (bucket)
      {
        if (!bucket.TryGetValue(key, out _lock))
        {
          _lock = new _slot();
          Monitor.Enter(_lock);
          bucket.Add(key, _lock);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Performs Monitor.Exit() on TKey value. Returns false in cases when lock was not taken which indicates an error
    /// in the calling control flow
    /// </summary>
    public bool Exit(TKey key)
    {
      var bucket = getBucket(key);
      _slot _lock;
      lock (bucket)
      {
        if (!bucket.TryGetValue(key, out _lock)) return false;
        Monitor.Exit(_lock);
        _lock.RefCount--;
        if (_lock.RefCount==0)
          bucket.Remove(key);
      }
      return true;
    }


    private Dictionary<TKey, _slot> getBucket(TKey key)
    {
      var hc = key.GetHashCode();
      return m_Buckets[hc & 0xff];
    }
  }
}

