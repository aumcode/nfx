using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

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
  public sealed class NamedInterlocked
  {
          private class slot : INamed
          {
             public slot(string name) { m_Name = name;}

             private string m_Name;

             public string Name { get { return m_Name;}}
             public int Integer;
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
      public IEnumerable<KeyValuePair<string, int>> AllInts
      {
         get{ return m_Data.Select(s => new KeyValuePair<string, int>(s.Name, s.Integer)); }
      }

      /// <summary>
      /// Enumerates all named integers. This operation is thread-safe, and returns a snapshot of the instance taken at the time of the first call
      /// </summary>
      public IEnumerable<KeyValuePair<string, long>> AllLongs
      {
         get{ return m_Data.Select(s => new KeyValuePair<string, long>(s.Name, s.Long)); }
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


      public int IncrementInt(string name)
      {
        var slot = getSlot(name);
        return Interlocked.Increment(ref slot.Integer);
      }

      public long IncrementLong(string name)
      {
        var slot = getSlot(name);
        return Interlocked.Increment(ref slot.Long);
      }

      public int DecrementInt(string name)
      {
        var slot = getSlot(name);
        return Interlocked.Decrement(ref slot.Integer);
      }

      public long DecrementLong(string name)
      {
        var slot = getSlot(name);
        return Interlocked.Decrement(ref slot.Long);
      }

      public int AddInt(string name, int arg)
      {
        var slot = getSlot(name);
        return Interlocked.Add(ref slot.Integer, arg);
      }

      public long AddLong(string name, long arg)
      {
        var slot = getSlot(name);
        return Interlocked.Add(ref slot.Long, arg);
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
      /// Captures the current value of a named int value.
      /// If slot does not exist, creates it and captures the value (which may be non-zero even if the slot was just created)
      /// </summary>
      public int VolatileReadInt(string name)
      {
        var slot = getSlot(name);
        return Thread.VolatileRead( ref slot.Integer );
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
      /// Captures the current value of a named int value and atomically sets it to the passed value
      /// </summary>
      public int ExchangeInt(string name, int value)
      {
        var slot = getSlot(name);
        return Interlocked.Exchange(ref slot.Integer, value);
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

  }

}
