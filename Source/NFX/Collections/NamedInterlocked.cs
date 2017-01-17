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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using NFX.Financial;

namespace NFX.Collections
{
  /// <summary>
  /// Provides functionality similar to the Interlocked class executed over a named slot.
  /// All operations are THREAD-SAFE for calling concurrently.
  /// The name comparison is that of Registry's which is OrdinalIgnoreCase.
  /// This class was designed to better organize named counters incremented from different threads,
  /// i.e. this is needed to keep a count of calls to remote host identified by their names.
  /// This class is NOT designed for frequent additions/deletions of named slots, nor was it designed
  /// to keep millions of slots. Use it in cases when there are thousands at most slots and new slots
  /// appear infrequently. You must delete un-needed slots
  /// </summary>
  public class NamedInterlocked
  {
          private class slot : INamed
          {
            public slot(string name) { m_Name = name; }

            private string m_Name;

            public string Name { get { return m_Name; } }
            public long Long;
          }

      private Registry<slot> m_Data = new Registry<slot>();

      /// <summary>
      /// Returns the current number of named slots in the instance
      /// </summary>
      public int Count
      {
        get { return m_Data.Count;}
      }


      /// <summary>
      /// Enumerates all slot names. This operation is thread-safe, and returns a snapshot of the instance taken at the time of the first call
      /// </summary>
      public IEnumerable<string> AllNames
      {
        get { return m_Data.Names; }
      }

      /// <summary>
      /// Enumerates all named integers. This operation is thread-safe, and returns a snapshot of the instance taken at the time of the first call
      /// </summary>
      public IEnumerable<KeyValuePair<string, long>> AllLongs
      {
        get { return m_Data.Select(s => new KeyValuePair<string, long>(s.Name, s.Long)); }
      }

      /// <summary>
      /// Enumerates all named integers. This operation is thread-safe, and returns a snapshot of the instance taken at the time of the first call
      /// </summary>
      public IEnumerable<KeyValuePair<string, Amount>> AllAmounts
      {
        get { return m_Data.Select(s => new KeyValuePair<string, Amount>(s.Name, new Amount(s.Name, convertLongToDecimal(s.Long)))); }
      }

      /// <summary>
      /// Deletes all state for all slots
      /// </summary>
      public void Clear()
      {
        m_Data.Clear();
      }

      /// <summary>
      /// Deletes all state for the named slot returning true if the slot was found and removed
      /// </summary>
      public bool Clear(string name)
      {
        if (name==null) throw new NFXException(StringConsts.INVALID_ARGUMENT_ERROR+GetType().FullName+".Clear(name==null)");
        return m_Data.Unregister( name );
      }

      /// <summary>
      /// Returns true when the instance contains the named slot
      /// </summary>
      public bool Exists(string name)
      {
        if (name==null) throw new NFXException(StringConsts.INVALID_ARGUMENT_ERROR+GetType().FullName+".Exists(name==null)");
        return m_Data.ContainsName( name );
      }

      public long IncrementLong(string name)
      {
        var slot = getSlot(name);
        return Interlocked.Increment(ref slot.Long);
      }

      public long DecrementLong(string name)
      {
        var slot = getSlot(name);
        return Interlocked.Decrement(ref slot.Long);
      }

      public long AddLong(string name, long arg)
      {
        var slot = getSlot(name);
        return Interlocked.Add(ref slot.Long, arg);
      }

      public Amount Add(Amount arg)
      {
        var slot = getSlot(arg.CurrencyISO);
        return new Amount(arg.CurrencyISO, Interlocked.Add(ref slot.Long, convertDecimalToLong(arg.Value)));
      }

      public void Add(NamedInterlocked source, bool clearSource = false)
      {
        if (source == null) return;
        var longs = source.AllLongs;
        if (clearSource) source.Clear();
        foreach (var item in source.AllLongs) AddLong(item.Key, item.Value);
      }

      /// <summary>
      ///  Returns a 64-bit value, loaded as an atomic operation even on a 32bit platform
      /// </summary>
      public long ReadAtomicLong(string name)
      {
        var slot = getSlot(name);
        return Interlocked.Read(ref slot.Long);
      }

      /// <summary>
      /// Captures the current value of a named long value.
      /// If slot does not exist, creates it and captures the value (which may be non-zero even if the slot was just created)
      /// </summary>
      public long VolatileReadLong(string name)
      {
        var slot = getSlot(name);
        return Thread.VolatileRead( ref slot.Long );
      }

      /// <summary>
      /// Captures the current value of a named long value and atomically sets it to the passed value
      /// </summary>
      public long ExchangeLong(string name, long value)
      {
        var slot = getSlot(name);
        return Interlocked.Exchange(ref slot.Long, value);
      }

                          private slot getSlot(string name, [System.Runtime.CompilerServices.CallerMemberName]string opName = "")
                          {
                            if (name==null) throw new NFXException(StringConsts.INVALID_ARGUMENT_ERROR + GetType().FullName + opName);
                            return m_Data.GetOrRegister(name, (_) => new slot(name) , this);
                          }

                          private static long convertDecimalToLong(decimal val)
                          {
                            return (long)(val * 100);
                          }

                          private static decimal convertLongToDecimal(long val)
                          {
                            return val / 100M;
                          }

  }

  /// <summary>
  /// Adds context to NamedInterlocked
  /// </summary>
  public class NamedInterlocked<TContext> : NamedInterlocked
  {
    public TContext Context { get; set; }
  }
}
