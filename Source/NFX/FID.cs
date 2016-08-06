using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace NFX
{
  /* Timestamp: 24 bits
   * ------------------
   *
   *   100 msec interval = 10/sec
   *   10 * 60/sec * 60/min * 24/hr = 864,000 intervals/day
   *   2 ^ 24 = 16,777,216 combinations / 864,000 per day = 19.41 days ~> 2 weeks (almost 3, but we need 2 + reserved margin)
   *
   * ThreadSeed: 24 bits
   * -------------------
   *
   *   2^24 = 16,777,216 combinations
   *
   *   To create a duplicate ID within the same 100msec interval one would need to:
   *    a. allocate over 2^24 threads (this is not technically possible)
   *     or
   *    b. generate more than 2^24 * 2^16 IDs (1,099,511,627,776) from one thread (this is not technically possible)
   *     or
   *    c. allocate many threads and generate many IDs, thus wrapping around 2^24 in number of threads/blocks (this is not likely at all on any hardware)
   *
   *   If a process generates 1,000,000,000 IDs/sec (one billion IDs a second), then it generates 100,000,000 IDs/100 msec interval
   *     100,000,000 / 2^16 = 1,525 blocks (out of 2^24). Let's assume that there are 256 threads involved:
   *     100,000,000 / 256 threads = 390,625 IDs/thread  / 2^16 = 5.96 = 6 slots/thread * 256 threads = 1,536 blocks (out of 2^24)
   *
   * Counter: 16 bits
   * -----------------
   *
   *  2^16 = 65,536 combinations
   *
   *   When these are exhausted, a new thread seed block is generated and counter reset to 0
   *
   */


  /// <summary>
  /// Represents an ultra-efficient 64 bit in-process-wide unique identifier "Fast Id".
  /// The ID is going to wrap-around after at least 2 weeks (19 days).
  /// The ID consists of 3 segments: [timestamp: 24bit][threadseed: 24bit][counter: 16 bit].
  /// This is needed because:
  ///  a). FID stays unique after process restarts
  ///  b). This design does not use interlock on global seed, but uses thread-static vars which is 10-20 times faster
  ///
  /// The timestamp is the number of 100ms intervals elapsed since Jan 1 2015 expressed as a 24 bit unsigned int, which gives 2^24 = 16,777,216 combinations
  /// which covers 19 days (around 2 weeks) at 100 msec resolution, consequently the ID will generate duplicates after this period.
  /// This struct is useful for creating unique IDs for protocol/traffic messages that live for a limited time (no more than 2 weeks).
  /// Caution: This ID does not identify the machine or process, only items within the process, however when a hosting process restarts(i.e. crash or reboot)
  /// the new IDs will not collide with IDs generated right before the crash for at least 14 days (14 day sliding window).
  /// In a parallel test on 6 Core i7 3.2 GHz this class generates 405 million IDs/sec, which is 57 times faster than Guid that only generates 7 million IDs/sec
  /// </summary>
  [Serializable]
  public struct FID : IEquatable<FID>, DataAccess.Distributed.IDistributedStableHashProvider
  {
    private const int MASK_16_BIT = 0x0000ffff;
    private const int MASK_24_BIT = 0x00ffffff;
    private static readonly DateTime START = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static int s_Seed;

    [ThreadStatic] private static ulong ts_Prefix;// timestamp + threadseed
    [ThreadStatic] private static int   ts_Counter;


    private static void genSeed()
    {
      var seed = (ulong)(Interlocked.Increment(ref s_Seed) & MASK_24_BIT) << 16;

      var ts = ((ulong)((DateTime.UtcNow - START).TotalMilliseconds / 100d) & 0x0000000000fffffful) << 40;//64-24

      ts_Prefix = ts | seed;//prefix is:  24 bit timestamp + 24 bit thread seed

      ts_Counter = 0;
    }

    /// <summary>
    /// Creates instance from ULONG value. Use FID.Generate() to create new IDs instead
    /// </summary>
    public FID(ulong id) { ID = id; }

    /// <summary>
    /// Returns 64 bit process-wide unique ID which is unique for up to 2 weeks
    /// </summary>
    public readonly ulong ID;

    /// <summary>
    /// Generates new process-wide unique ID which is unique for up to 2 weeks
    /// </summary>
    public static FID Generate()
    {
      if (ts_Prefix == 0 || ts_Counter == MASK_16_BIT) genSeed();

      ts_Counter++;

      var id = ts_Prefix | (ulong)(uint)ts_Counter;

      return new FID(id);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is FID)) return false;
      return this.Equals((FID)obj);
    }

    public bool Equals(FID other)
    {
      return this.ID == other.ID;
    }

    public override int GetHashCode()
    {
      return ID.GetHashCode();
    }

    public override string ToString()
    {
      return "{0}-{1}-{2}".Args(ID >> 40, (ID >> 16) & MASK_24_BIT, ID & MASK_16_BIT);
    }

    public ulong GetDistributedStableHash()
    {
      return ID;
    }

    /// <summary>
    /// Converts to Guid by copying ID twice
    /// </summary>
    public Guid ToGuid()
    {
      var b = new byte[16];
      b.WriteBEUInt64(0, ID);
      b.WriteBEUInt64(4, ID);
      return new Guid( b );
    }
  }
}
