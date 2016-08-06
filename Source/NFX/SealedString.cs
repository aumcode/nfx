using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NFX
{
  /// <summary>
  /// Represents an immutable string data that is stored in an efficient way that relieves the GC pressure.
  /// The string can not be changed or deleted. Once created it stays as-is until the process is terminated.
  /// This structure is used to store much dictionary data (100s of millions of strings) in the process without causing GC overload.
  /// Note: The default .ctor DOES NOT check whether the same string is already present in memory. Use SealedString.Scope to
  ///  store unique strings only (do not store the repetitions).
  /// This struct is THREAD SAFE and NOT SERIALIZABLE.
  /// </summary>
  [NFX.Serialization.Slim.SlimSerializationProhibited]
  public struct SealedString : IEquatable<SealedString>
  {
            /// <summary>
            /// Represents a scope of a SealedString creation that tracks the existing strings.
            /// Use Seal(string) to get an existing string or create a new one.
            /// Do not persist the instance of Scope for a long time as it accumulates references (string keys) that pressure the GC.
            /// This class is used in batched that create many sealed strings. Once those strings rae created the scope can be released.
            /// </summary>
            public class Scope
            {
              /// <summary>
              /// Create a Scope with InvariantCultureIgnoreCase comparer
              /// </summary>
              public Scope() : this(null){}

              /// <summary>
              ///  Create a Scope with the specified comparer or InvariantCultureIgnoreCase comparer if the supplied one is null
              /// </summary>
              public Scope(StringComparer comparer)
              {
                if (comparer==null) comparer = StringComparer.InvariantCultureIgnoreCase;
                m_Data = new Dictionary<string, SealedString>(comparer);
              }

              private Dictionary<string, SealedString> m_Data;


              /// <summary>
              /// Returns the number of unique strings in scope
              /// </summary>
              public int Count{ get{ lock(m_Data) return m_Data.Count;}}


              /// <summary>
              /// Returns true if a unique string is in the scope
              /// </summary>
              public bool Contains(string value)
              {
                if (value==null) return false;
                lock(m_Data) return m_Data.ContainsKey(value);
              }


              /// <summary>
              /// Returns a SealedString for an existing key in scope or SealedString.Unassigned for a non-exisiting string
              /// </summary>
              public SealedString this[string value]
              {
                get
                {
                  if (value!=null)
                  {
                    SealedString result;
                    lock(m_Data)
                     if (m_Data.TryGetValue(value, out result)) return result;
                  }
                  return SealedString.Unassigned;
                }
              }

              /// <summary>
              /// Returns an existing SealedString instance for existing data or allocates a new one.
              /// This method is thread-safe
              /// </summary>
              public SealedString Seal(string value)
              {
                bool dummy;
                return this.Seal(value, out dummy);
              }

              /// <summary>
              /// Returns an existing SealedString instance for existing data or allocates a new one.
              /// This method is thread-safe
              /// </summary>
              public SealedString Seal(string value, out bool existed)
              {
                if (value==null) throw new NFXException(StringConsts.ARGUMENT_ERROR+"SealedString.Scope.Seal(value==null)");

                SealedString result;

                lock(m_Data)
                {
                  if (!m_Data.TryGetValue(value, out result))
                  {
                    existed = false;
                    result = new SealedString(value);
                    m_Data.Add(value, result);
                  }
                  else existed = true;
                }

                return result;
              }
            }


    private const int SEG_START_SIZE = 4 * 1024 * 1024;

    private const int SEG_INC_FROM_COUNT = 250;

    private const int SEG_MAX_SIZE = CoreConsts.MAX_BYTE_BUFFER_SIZE;

    private const int STR_BUF_SZ = 32 * 1024;

    private const int MAX_BUF_STR_LEN = STR_BUF_SZ / 2; //2 bytes per UTF16 character
                                                        //this is done on purpose NOT to call
                                                        //Encoding.GetByteCount()
    [ThreadStatic]
    private static byte[] ts_StrBuff;

    private static Encoding s_Encoding = new UTF8Encoding(false, false);
    private static OS.ManyReadersOneWriterSynchronizer s_Sync = new OS.ManyReadersOneWriterSynchronizer();

    private static byte[][] s_Data = new byte[ushort.MaxValue][];
    private static int s_CurrentSegment = -1;
    private static int s_CurrentAddress;

    private static long s_TotalCount;
    private static long s_TotalBytesUsed;


    /// <summary>
    /// Returns the total number of bytes used
    /// </summary>
    public static long TotalBytesUsed{ get{ return s_TotalBytesUsed;}}

    /// <summary>
    /// Returns the total count of strings allocated
    /// </summary>
    public static long TotalCount{ get{ return s_TotalCount; }}


    /// <summary>
    /// Returns the total number of bytes allocated
    /// </summary>
    public static long TotalBytesAllocated
    {
      get{ return s_Data.Sum( (a) => a==null ? 0 : a.LongLength ); }
    }

    /// <summary>
    /// Total number of segments
    /// </summary>
    public static int TotalSegmentCount { get{ return s_CurrentSegment+1; } }


    /// <summary>
    /// Returns a SealedString that is IsAssigned==false - a special invalid value
    /// </summary>
    public static SealedString Unassigned{ get{ return new SealedString();}}

    /// <summary>
    /// Creates a sealed copy of string data. This .ctor DOES NOT check whether this string was already sealed
    /// </summary>
    public SealedString(string value)
    {
      if (value==null) throw new NFXException(StringConsts.ARGUMENT_ERROR+"SealedString.ctor(value==null)");


      byte[] encoded;
      int size = 0;

      var len = value.Length;
      if (len>MAX_BUF_STR_LEN)//This is much faster than Encoding.GetByteCount()
      {
        encoded = s_Encoding.GetBytes(value);
        size = encoded.Length;
      }
      else
      {
        if (ts_StrBuff==null) ts_StrBuff = new byte[STR_BUF_SZ];
        encoded = ts_StrBuff;
        size = s_Encoding.GetBytes(value, 0, len, encoded, 0);
      }



      var totalSize = sizeof(int) + size;

      s_Sync.GetWriteLock();
      try
      {
        var array = s_CurrentSegment < 0 ? null : s_Data[s_CurrentSegment];
        if (array==null || (array.Length - s_CurrentAddress < totalSize))
        {
          if (s_CurrentSegment==ushort.MaxValue)
            throw new NFXException(StringConsts.SEALED_STRING_OUT_OF_SPACE_ERROR.Args(ushort.MaxValue));



          var newSize = getSegmentSize(totalSize);

//Console.WriteLine("NEW SIZE IS: {0:n0}", newSize);
          array = new byte[newSize];
          s_CurrentSegment++;

          s_CurrentAddress = s_CurrentSegment==0 ? 1 : 0;
          s_Data[s_CurrentSegment] = array;
        }

        array.WriteBEInt32(s_CurrentAddress, size);
        Array.Copy(encoded, 0, array, s_CurrentAddress + sizeof(int), size);

        Segment = (ushort)s_CurrentSegment;
        Address = s_CurrentAddress;

        s_CurrentAddress += totalSize;
      }
      finally
      {
        s_Sync.ReleaseWriteLock();
      }

      Interlocked.Increment(ref s_TotalCount);
      Interlocked.Add(ref s_TotalBytesUsed, totalSize);
    }

    public readonly ushort Segment;
    public readonly int Address;


    /// <summary>
    /// Returns true if this instance represents an assigned valid string (is not equal to Unassigned)
    /// </summary>
    public bool IsAssigned
    {
      get{ return Segment!=0 || Address!=0;}
    }

    /// <summary>
    /// Returns the original string copy
    /// </summary>
    public string Value
    {
      get
      {
        if (Segment==0 && Address==0) return null;

        //there is no need to lock anything on read as the structure is read-only
        var array = s_Data[Segment];
        var addr = Address;
        var count = array.ReadBEInt32(ref addr);
        return s_Encoding.GetString(array, addr, count);
      }
    }


    public override bool Equals(object obj)
    {
      if (obj is SealedString)
       return this.Equals((SealedString)obj);

      return false;
    }

    public bool Equals(SealedString other)
    {
      return (this.Segment == other.Segment) && (this.Address == other.Address);
    }

    public override int GetHashCode()
    {
      return Address;
    }

    public override string ToString()
    {
      return "SealedString<"+ (IsAssigned ? Segment.ToString("X4")+":"+Address.ToString("X4") : "unassigned")+">";
    }

    public static bool operator ==(SealedString l, SealedString r)
    {
      return l.Equals(r);
    }

    public static bool operator !=(SealedString l, SealedString r)
    {
      return !l.Equals(r);
    }


    private static int getSegmentSize(int atLeast)
    {
      long result = SEG_START_SIZE;

      if (s_CurrentSegment>SEG_INC_FROM_COUNT)
      {
        result = 2L * s_Data[s_CurrentSegment].LongLength;
      }

      if (result < atLeast) result = atLeast;

      if (result > SEG_MAX_SIZE)
      {
        result = SEG_MAX_SIZE;
        if (result < atLeast) throw new NFXException(StringConsts.SEALED_STRING_TOO_BIG_ERROR.Args(atLeast));
      }

      return (int)result;
    }

  }
}
