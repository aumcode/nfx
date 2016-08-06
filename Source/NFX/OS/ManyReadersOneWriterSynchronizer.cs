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
using System.Threading;

namespace NFX.OS
{
  /// <summary>
  /// This struct is for ADVANCED SYSTEM use-cases. DEVELOPERS: do not use this in a business application code.
  /// Represents an efficient SpinWait/Yield-based lock that allows many readers to operate concurrently
  ///  and only one single concurrent writer. This struct must be used ONLY with a very fast/tight operations that
  ///  do not block on IO and other locks, and are expected to complete under a tenth fraction of a second.
  /// Warning: this lock CAN NOT be held for longer than 5 hrs by design. DO NOT use this lock with IO-dependent operations.
  /// This is not really a limitation as this class is used to guard operations which are very fast (fractions of a second) and
  /// should fail faster otherwise.
  /// This lock IS NOT REENTRANT!
  /// </summary>
  public struct ManyReadersOneWriterSynchronizer
  {
      private static readonly int CPU_COUNT = System.Environment.ProcessorCount;
      private static readonly bool MULTI_CPU = CPU_COUNT > 2;

      private const long READ_TIGHT_THRESHOLD = -4000000000L;
      private const int PAUSE_COUNT_PER_BATCH = 1000;//must be at least 500 to make sense
      private const int MIN_SLEEP_MS = 5;//affects the READ_TIGHT_THRESHOLD value

      private long m_Readers;
      private int m_WaitingWriters;


      /// <summary>
      /// Obtains a read lock returning true on success.
      /// Many threads may obtain a read lock at the same time, however a single write lock excludes any read locks,
      /// corollary any single read lock held excludes anyone obtaining a write lock.
      /// False is returned if cancel func was supplied and returned true to cancel-out the waiting
      /// </summary>
      public bool GetReadLock(Func<object, bool> cancel = null, object cancelContext = null)
      {
          int pauseCount = 0;
          while(true)
          {
            long current = Interlocked.Increment(ref m_Readers);

            if (current>0) return true;//lock taken

            if (current==0)
             throw new NFXException(StringConsts.INVALID_OPERATION_ERROR+GetType().FullName+".GetReadLock() overflow failure");


            //Suppose there are 1000 real/physical threads all executing at the same time (which is not physically possible but is used as the worst case)
            //There are 200 5-ms intervals in a second (the worst case below MIN_SLEEP_MS=5), = 1000 threads * 200 intervals * 60 sec = 12,000,000 increments a minute
            //4B / 12M = 333 minutes = 5.5+ hrs
            if (current < READ_TIGHT_THRESHOLD)
            {
                if (pauseCount<PAUSE_COUNT_PER_BATCH)
                {
                     var tightWait = MULTI_CPU && Thread.VolatileRead(ref m_WaitingWriters) < CPU_COUNT;

                     if (tightWait)
                       tightOnePause(pauseCount);
                     else
                       Thread.Sleep(1);

                     pauseCount++;
                     continue;
                }

                pauseCount = 0;
            }

            //severe contention
            if (cancel!=null && cancel(cancelContext)==true) return false;//lock failed
            Thread.Sleep(MIN_SLEEP_MS + (Thread.CurrentThread.GetHashCode() & 0xf));
          }//while
      }

      /// <summary>
      /// Releases the read lock that was previously obtained by a call to GetReadLock()
      /// </summary>
      public void ReleaseReadLock()
      {
        var result = Interlocked.Decrement(ref m_Readers);
        if (result<0)
         throw new NFXException(StringConsts.INVALID_OPERATION_ERROR+GetType().FullName+".ReleaseReadLock() call count mismatched with get lock");
      }


      /// <summary>
      /// Obtains a write lock returning true on success. Only one thread may hold a write lock at a time,
      /// and noone else can obtain a read lock until the write lock is released.
      /// False is returned if cancel func was supplied and returned true to cancel-out the waiting
      /// </summary>
      public bool GetWriteLock(Func<object, bool> cancel = null, object cancelContext = null)
      {
          int pauseCount = 0;

          Interlocked.Increment(ref m_WaitingWriters);

          while(true)
          {
            long current = Interlocked.CompareExchange(ref m_Readers, long.MinValue, 0);

            if (current==0)
            {
              Interlocked.Decrement(ref m_WaitingWriters);
              return true;//lock taken
            }

            if (pauseCount<PAUSE_COUNT_PER_BATCH)
            {
                  var tightWait = MULTI_CPU && Thread.VolatileRead(ref m_WaitingWriters) < CPU_COUNT;

                  if (tightWait)
                    tightOnePause(pauseCount);
                  else
                    Thread.Sleep(1);

                  pauseCount++;
                  continue;
            }

            pauseCount = 0;

            //severe contention
            if (cancel!=null)
            {
               var canceled = false;
                try
                {
                   canceled = cancel(cancelContext);
                }
                catch
                {
                   Interlocked.Decrement(ref m_WaitingWriters);
                   throw;
                }

                if (canceled)
                {
                  Interlocked.Decrement(ref m_WaitingWriters);
                  return false;//lock failed
                }
            }


            Thread.Sleep(MIN_SLEEP_MS + (Thread.CurrentThread.GetHashCode() & 0xf));

          }
      }


      /// <summary>
      /// Releases the write lock that was previously obtained by a call to GetWriteLock()
      /// </summary>
      public void ReleaseWriteLock()
      {
        Thread.VolatileWrite(ref m_Readers, 0L);
      }


      private void tightOnePause(int pauseCount)
      {
        if (pauseCount>PAUSE_COUNT_PER_BATCH-2) { Thread.Sleep(1); }
        else
          if (pauseCount>PAUSE_COUNT_PER_BATCH-5) { Thread.Sleep(0); }
          else
           if (pauseCount>PAUSE_COUNT_PER_BATCH-25) { if (!Thread.Yield()) Thread.SpinWait(1000); }
           else
             if (pauseCount>(PAUSE_COUNT_PER_BATCH / 2)) { Thread.SpinWait(1000); }
             else
               if (pauseCount>(PAUSE_COUNT_PER_BATCH / 4)) { Thread.SpinWait(500); }
               else
                 Thread.SpinWait(250);
      }

  }
}
