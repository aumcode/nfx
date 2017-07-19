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
using System.IO;
using System.Threading;
using System.Threading.Tasks;


using NFX.IO;
using NFX.ServiceModel;
using NFX.Serialization.Slim;
using NFX.Environment;


namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Provides base for default implementation of IPile which stores objects on a local machine in memory or in the MMF
  /// </summary>
  public abstract partial class DefaultPileBase : ServiceWithInstrumentationBase<object>, IPileImplementation
  {
      private static int CPU_COUNT = System.Environment.ProcessorCount;
      private static readonly TimeSpan INSTR_INTERVAL = TimeSpan.FromMilliseconds(3700);

      public const string CONFIG_PILE_SECTION = "pile";

      public const string CONFIG_FREE_CHUNK_SIZES_ATTR = "free-chunk-sizes";

      public const int SEG_SIZE_MIN  = 64 * 1024 * 1024; // 64 Mbyte
      public const int SEG_SIZE_DFLT = 256 * 1024 * 1024; // 256 Mbyte
      public const int SEG_SIZE_MAX  = CoreConsts.MAX_BYTE_BUFFER_SIZE;//2Gb


      public const int FREE_LST_COUNT = 16;
      public const int FREE_CHUNK_SIZE_MIN = 64;
      public const int FREE_LST_SIZE_MIN =  25*1024;//make sure that free list is in LOB, hence >85000 bytes. 25,600*4 = 102,400 bytes * 16 slots = 1.7 Mb per segment
      public const int FREE_LST_SIZE_MAX = 128*1024;//131,072*4 = 524,288 bytes * 16 slots = 8.5 Mb per segment

      /* CHUNK structure
       * ---------------
       *
       *  1. Chunk flag: 03AC0B = used | CBAB0D = free      [3 bytes]
       *
       *  2. Payload Size without header: int32             [4 bytes]
       *     payload size is ALWAYS extended to
       *     the last 3 bits=0, so it is always 8-aligned
       *
       *  3. Serializer version: byte 0..255                [1 bytes] <--- string, byte[], link, slim, or other...
       *
       *                                               --------------
       *                                                     8 bytes  <--- CHUNK_HDR_SZ
       *  4. ..... chunk data .....
       *     payload bytes as stated in (2)               [(2) bytes]
      */

      public const byte SVER_SLIM =  0;//Slim Serializer
      public const byte SVER_BUFF =  1;//byte[] - byte buffer
      public const byte SVER_UTF8 =  2;//string UTF8
      public const byte SVER_LINK =  3;//link to  another PilePointer


      private const int CHUNK_HDER_SZ = 8;

      private static readonly int[] DEFAULT_FREE_CHUNK_SIZES = new int[]{     64, //0
                                                                        128, //1  +64
                                                                        192, //2  +64
                                                                        256, //3  +64
                                                                        384, //4  +128
                                                                        512, //5  +128
                                                                        640, //6  +128
                                                                        896, //7  +256
                                                                       1408, //8  +512
                                                                       1920, //9  +512
                                                                       2944, //A  +1024
                                                                       4096, //B  4K
                                                                       8192, //C  8K
                                                                      16384, //D  16K
                                                                      32768, //E  32K
                                                                      65536, //F  64K
                                                                   };

    #region .ctor

      public DefaultPileBase(string name = null):base()
      {
        Name = name;
        ctor();
      }

      public DefaultPileBase(object director, string name = null):base(director)
      {
        Name = name;
        ctor();
      }

      protected override void Destructor()
      {
        DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
        base.Destructor();
      }

      private void ctor()
      {
         m_CurrentTypeRegistry = new TypeRegistry(
            TypeRegistry.BoxedCommonTypes,
            TypeRegistry.BoxedCommonNullableTypes,
            TypeRegistry.CommonCollectionTypes
         );
      }

    #endregion

    #region Fields

      private int m_Identity;
      private bool m_InstrumentationEnabled;
      private Time.Event m_InstrumentationEvent;

      private AllocationMode m_AllocMode;
      private long m_MaxMemoryLimit;
      private int m_MaxSegmentLimit;
      private int m_SegmentSize = SEG_SIZE_DFLT;
      private int m_FreeListSize = FREE_LST_SIZE_MIN;

      private object m_SegmentsLock = new object();
      private List<_segment> m_Segments;            internal void __addSegmentAtStart(_segment seg){ m_Segments.Add(seg); }//do not call this after service start - not thread safe
                                                    internal _segment[] __getSegmentsAtStart(){ return m_Segments.ToArray(); }//thread safe for snapshots
      private int[] m_FreeChunkSizes = DEFAULT_FREE_CHUNK_SIZES;


      private object m_CurrentTypeRegistryLock = new object();
      private volatile TypeRegistry m_CurrentTypeRegistry;

      private long m_stat_PutCount;
      private long m_stat_DeleteCount;
      private long m_stat_GetCount;

    #endregion

    #region Properties

        /// <summary>
        /// When set to value greater than zero, provides a process-unique identity of the pile instance.
        /// The property must be set to unique value in order to start the pile, or set to less or equal to zero to keep the instance private.
        /// Used by Piled via PileInstances retrived by Identity (if greater than zero)
        /// </summary>
        [Config]
        public int Identity
        {
          get{ return m_Identity;}
          set
          {
            CheckServiceInactive();
            m_Identity = value;
          }
        }


        /// <summary>
        /// Implements IInstrumentable
        /// </summary>
        [Config(Default=false)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_PILE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public override bool InstrumentationEnabled
        {
          get { return m_InstrumentationEnabled;}
          set
          {
             m_InstrumentationEnabled = value;

             if (m_InstrumentationEvent==null)
             {
               if (!value) return;
               m_stat_PutCount = 0;
               m_stat_DeleteCount = 0;
               m_stat_GetCount = 0;
               m_InstrumentationEvent = new Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTR_INTERVAL);
             }
             else
             {
               if (value) return;
               DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
             }
          }
        }


        /// <summary>
        /// Returns PileLocality.Local
        /// </summary>
        public virtual LocalityKind Locality { get { return LocalityKind.Local; }}

        /// <summary>
        /// Returns PilePersistence - where data is kept
        /// </summary>
        public abstract ObjectPersistence Persistence { get;}

        /// <summary>
        /// Returns false
        /// </summary>
        public bool SupportsObjectExpiration { get{ return false; }}

        /// <summary>
        /// Returns 1 as this pile is not distributed
        /// </summary>
        public int NodeCount { get{ return 1;} }

        /// <summary>
        /// Defines modes of allocation: space/time tradeoff
        /// </summary>
        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_PILE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public AllocationMode AllocMode
        {
          get{ return m_AllocMode;}
          set
          {
            m_AllocMode = value;
          }
        }

        /// <summary>
        /// Sets the sizes of free chunks that free lists group by.
        /// Must be an array of FREE_LST_COUNT(16) of consequitively increasing integers
        /// </summary>
        public int[] FreeChunkSizes
        {
          get { return m_FreeChunkSizes;}
          set
          {
            CheckServiceInactive();
            ensureFreeChunkSizes(value);
            m_FreeChunkSizes = value;
          }
        }

        /// <summary>
        /// Determines the size of free chunk list. Every segment has FREE_LST_COUNT=16 lists each of this size.
        /// This property may only be set on an inactive service instance
        /// </summary>
        [Config]
        public int FreeListSize
        {
          get { return m_FreeListSize;}
          set
          {
            CheckServiceInactive();
            m_FreeListSize = value < FREE_LST_SIZE_MIN ? FREE_LST_SIZE_MIN : value > FREE_LST_SIZE_MAX ? FREE_LST_SIZE_MAX : value;
          }
        }


        /// <summary>
        /// Gets the maximum count of segments tha this pile can have.
        /// The property is not thread-safe for set and can only be set if pile is inactive.
        /// The value of zero means no limit
        /// </summary>
        [Config]
        public int MaxSegmentLimit
        {
          get
          {
            return m_MaxSegmentLimit;
          }
          set
          {
            CheckServiceInactive();
            m_MaxSegmentLimit = value>0 ? value: 0;
          }
        }

        /// <summary>
        /// Gets the maximum segment size in bytes, up to (2^31)-1
        /// The property is not thread-safe for set and can only be set if pile is inactive
        /// </summary>
        [Config]
        public int SegmentSize
        {
          get
          {
            return m_SegmentSize;
          }
          set
          {
            CheckServiceInactive();
            value = IntMath.Align16(value);
            m_SegmentSize = value < SEG_SIZE_MIN ? SEG_SIZE_MIN : value > SEG_SIZE_MAX ? SEG_SIZE_MAX : value;
          }
        }

        /// <summary>
        /// Imposes a limit on maximum number of bytes that a pile can allocate of the system heap.
        /// The default value of 0 means no limit, meaning - the pile will keep allocating objects
        /// until the system allows. The value may not be less than minimum 1 seg size (64 mb).
        /// May set on an active instance, however no existing objects will be removed if the limit is exceeded
        /// </summary>
        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_PILE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public long MaxMemoryLimit
        {
          get { return m_MaxMemoryLimit;}
          set
          {
            if (value!=0 && value<SEG_SIZE_MIN) value = SEG_SIZE_MIN;
            m_MaxMemoryLimit = value;
          }
        }

        /// <summary>
        /// Returns the total number of objects allocated at this point in time
        /// </summary>
        public long ObjectCount
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return availSegs(segs).Sum( seg => (long)Thread.VolatileRead(ref seg.ObjectCount));
          }
        }

        /// <summary>
        /// Returns the total number of object links allocated at this point in time
        /// </summary>
        public long ObjectLinkCount
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return availSegs(segs).Sum( seg => (long)Thread.VolatileRead(ref seg.ObjectLinkCount));
          }
        }

        /// <summary>
        /// Returns the number of bytes allocated by this pile from the system memory heap.
        /// As pile pre-allocates memory in segments, it is absolutely normal to have this property
        /// return 100s of megabytes even when pile is almost empty.
        /// This property may return close to all physical memory available
        /// </summary>
        public long AllocatedMemoryBytes
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return availSegs(segs).Sum( seg => (long)seg.Data.Length);
          }
        }

        /// <summary>
        /// Returns the number of bytes currently occupied by object stored in this pile.
        /// This number is always less than AllocatedMemoryBytes
        /// </summary>
        public long UtilizedBytes
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return availSegs(segs).Sum( seg => (long)Thread.VolatileRead(ref seg.UsedBytes));
          }
        }

        /// <summary>
        /// Returns the number of extra bytes used by pile metadata currently occupied by object stored in this pile
        /// </summary>
        public long OverheadBytes
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return availSegs(segs).Sum( seg => seg.OverheadBytes);
          }
        }

        /// <summary>
        /// Returns the total number of segments allocated at this point in time
        /// </summary>
        public long SegmentCount
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return availSegs(segs).Count();
          }
        }

        /// <summary>
        /// Returns an approximate capacity of free memory that the system has left
        /// </summary>
        public long MemoryCapacityBytes
        {
          get
          {
            var mstat = NFX.OS.Computer.GetMemoryStatus();

            var avail = (long)((double)mstat.AvailablePhysicalBytes * 0.875d);

            if (m_MaxMemoryLimit>0)
            {
              var limited = m_MaxMemoryLimit - AllocatedMemoryBytes;
              if (limited<avail) avail = limited;
            }

            return avail;
          }
        }


        /// <summary>
        /// Returns the total number of total segments - allocated and empty at this point in time.
        /// This number is greater or equal to SegmentCount
        /// </summary>
        public int SegmentTotalCount
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return segs.Count();
          }
        }


        /// <summary>
        /// Returns false and does nothing on set in this implementation
        /// if they are set
        /// </summary>
        public bool SweepExpiredObjects{ get{ return false;} set{} }

    #endregion

    #region Public

      /// <summary>
      /// Analyzes all segments scanning for free spaces. Returns number of uncovered free bytes.
      /// This is a full-blocking long operation that may take around 8 seconds on a 64Gb heaps in non-parallel mode
      /// </summary>
      public SegmentCrawlStatus Crawl(bool parallel)
      {
        SegmentCrawlStatus total = new SegmentCrawlStatus();
        var segs = m_Segments.Where(s => s!=null);

        if (parallel)
        {
           var lck = new object();
           Parallel.ForEach(segs, (seg) =>
           {
              if (!getWriteLock(seg)) return;
              try
              {
                var status = seg.Crawl();
                lock(lck){ total = total.Sum(status);}
              }
              finally
              {
                releaseWriteLock(seg);
              }
           });
        }
        else
           foreach(var seg in segs)
           {
              if (!getWriteLock(seg)) return total;
              try
              {
                total = total.Sum(seg.Crawl());
              }
              finally
              {
                releaseWriteLock(seg);
              }
           }

        return total;
      }


      [ThreadStatic]
      private static int ts_PutStartSegIdx;

      /// <summary>
      /// Puts a CLR object into the pile and returns a newly-allocated pointer.
      /// Throws out-of-space if there is not enough space in the pile and limits are set.
      /// Optional lifeSpanSec is ignored by this implementation
      /// </summary>
      public PilePointer Put(object obj, uint lifeSpanSec = 0, int preallocateBlockSize = 0)
      {
        return put(null, obj, null, 0, 0, preallocateBlockSize, false);
      }
      private PilePointer put(_segment lockedSegment, object obj, byte[] buffer, int serializedSize, byte serializerVersion, int preallocateBlockSize, bool isLink)
      {
        if (!Running) return PilePointer.Invalid;

        if (obj==null) throw new PileException(StringConsts.ARGUMENT_ERROR+GetType().Name+".Put(obj==null)");

        Interlocked.Increment(ref m_stat_PutCount);

        //1 serialize to determine the size
        if (buffer==null)
        {
          buffer = serialize(obj, out serializedSize, out serializerVersion);
        }

        var payloadSize = IntMath.Align8(serializedSize);
        if (preallocateBlockSize>payloadSize) payloadSize =  IntMath.Align8(preallocateBlockSize);
        var chunkSize = CHUNK_HDER_SZ + payloadSize;


        if (chunkSize>m_SegmentSize)
         throw new PileOutOfSpaceException(StringConsts.PILE_OBJECT_LARGER_SEGMENT_ERROR.Args(payloadSize, m_SegmentSize));

        var isSpeed = m_AllocMode == AllocationMode.FavorSpeed;

        while(true)
        {
            if (!Running) return PilePointer.Invalid;

            var segs = m_Segments;

            int si = 0;
            if (segs.Count > 2 * CPU_COUNT)//20170325 DKh
            {
              si = ts_PutStartSegIdx++;
              if (si>=segs.Count-CPU_COUNT)
              {
                si = 0;
                ts_PutStartSegIdx = 0;
              }
            }

            var oldSegWindow = m_AllocMode==AllocationMode.ReuseSpace ? 8 : 2;
            for(int cnt=0, idxSegment=si; idxSegment<segs.Count; idxSegment++, cnt++)
            {
              if (cnt==oldSegWindow && idxSegment < segs.Count-CPU_COUNT-1)//20170325 DKh
              {
                idxSegment = segs.Count - CPU_COUNT - 1;
                continue;
              }

              var seg = segs[idxSegment];
              if (seg==null) continue;
              if (Thread.VolatileRead(ref seg.DELETED)!=0) continue;
              if (!seg.LOADED_AND_CRAWLED) continue;

              if ((isSpeed && seg.FreeCapacity > seg.Data.Length / 8) || (seg.FreeCapacity > chunkSize))
              {
                 //20170406 MUST use tryGetWriteLock() so deadlock does not happen when called from put(ptr, object)
                 //from within another lock
                 var needsLock = seg!=lockedSegment;
                 if (needsLock && !tryGetWriteLock(seg)) continue;//20170325 DKh if (!getWriteLock(seg)) return PilePointer.Invalid;
                 try
                 {
                   if (Thread.VolatileRead(ref seg.DELETED)==0 && seg.FreeCapacity > chunkSize)
                   {
                     var adr = seg.Allocate(buffer, serializedSize, payloadSize, serializerVersion, isLink);
                     if (adr>=0) return new PilePointer(idxSegment, adr);//allocated before crawl

                     var utcNow = DateTime.UtcNow;
                     var secSinceLastCrawl = (utcNow - seg.LastCrawl).TotalSeconds;
                     if (
                         (secSinceLastCrawl > IntMath.ChangeByRndPct(isSpeed ? 600 : 180, -0.25f)) ||
                         (seg.FreeBytesChangePctSinceLastCrawl > 0.1f && (secSinceLastCrawl > IntMath.ChangeByRndPct(isSpeed ? 60 : 10, -0.5f)))
                        )
                     {
                         //could not fit, try to reclaim
                         seg.Crawl();

                         //try again
                         adr = seg.Allocate(buffer, serializedSize, payloadSize, serializerVersion, isLink);
                         if (adr>=0) return new PilePointer(idxSegment, adr);//allocated after crawl
                     }
                     //if we are here - still could not allocate, will try next segment in iteration
                   }
                 }
                 finally
                 {
                   if (needsLock)
                     releaseWriteLock(seg);
                 }
              }
            }//for

            //if we are here, we still could not allocate space of existing segments
            if (m_AllocMode==AllocationMode.FavorSpeed) break;

            var nsegs = m_Segments;
            if (segs.Count >= nsegs.Count) break;//if segment list grew already, repeat the whole thing again as more space may have become available
        }//while

        //allocate segment
        lock(m_SegmentsLock)
        {
           if (
               (m_MaxMemoryLimit>0  && AllocatedMemoryBytes+m_SegmentSize     > m_MaxMemoryLimit  )||
               (m_MaxSegmentLimit>0 && (m_Segments.Count( s => s!=null ) + 1) > m_MaxSegmentLimit )
              )
           throw new PileOutOfSpaceException(StringConsts.PILE_OUT_OF_SPACE_ERROR.Args(m_MaxMemoryLimit, m_MaxSegmentLimit, m_SegmentSize));

           var newSegs = new List<_segment>(m_Segments);//thread safe copy

           var newSeg =  MakeSegment( m_Segments.Count );//segment number
           newSegs.Add( newSeg );

           var adr = newSeg.Allocate(buffer, serializedSize, payloadSize, serializerVersion, isLink);
           var pp = new PilePointer(newSegs.Count-1, adr);
           m_Segments = newSegs;
           return pp;
        }
      }



      /// <summary>
      /// Tries to put the new object over an existing one at the pre-define position.
      /// The pointer has to reference a valid allocated block.
      /// If object fits in the allocated block returns true, otherwise tries to create an internal link
      /// to the new pointer which is completely transparent to the caller. The linking may be explicitly disabled
      /// in which case the method returns false when the new object does not fit into the existing block
      /// </summary>
      public bool Put(PilePointer ptr, object obj, uint lifeSpanSec = 0, bool link = true)
      {
        var result = put(ptr, obj, lifeSpanSec, link);
        if (result) Interlocked.Increment(ref m_stat_PutCount);
        return result;
      }
      private bool put(PilePointer ptr, object obj, uint lifeSpanSec, bool link)
      {
        if (!Running) return false;
        if (obj==null) throw new PileException(StringConsts.ARGUMENT_ERROR+GetType().Name+".Put(obj==null)");

        //1 serialize to determine the size
        int serializedSize;
        byte serializerVersion;
        var buffer = serialize(obj, out serializedSize, out serializerVersion);

        var newPayloadSize = serializedSize;//do not Align as the existing block is already aligned

        //2 see if the slot is allocated (under write lock)
        var segs = m_Segments;
        if (ptr.Segment<0 || ptr.Segment>=segs.Count)
         throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

        var seg = segs[ptr.Segment];
        if (seg==null || Thread.VolatileRead(ref seg.DELETED)!=0)
          throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

        var linkPointer = PilePointer.Invalid;
        if (!getWriteLock(seg)) return false;//Service shutting down
        try
        {
          //2nd check under lock
          if (Thread.VolatileRead(ref seg.DELETED)!=0) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

          var data = seg.Data;
          var addr = ptr.Address;
          if (addr<0 || addr>=data.Length-CHUNK_HDER_SZ)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_EOF_ERROR + ptr.ToString());

          var cflag = data.ReadChunkFlag(addr); addr+=3;
          if (cflag!=ChunkFlag.Used)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR + ptr.ToString());

          var payloadSize = data.ReadInt32(addr); addr+=4;
          if (payloadSize>data.Length)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_PAYLOAD_SIZE_ERROR.Args(ptr, payloadSize));

          var existingSVer = data.ReadByte(addr);
          if (existingSVer==SVER_LINK)
            linkPointer = data.ReadPilePointer(addr+1);

          if (newPayloadSize<=payloadSize)//fit in existing block
          {
            data.WriteByte(addr, serializerVersion);
            addr++;
            if (serializerVersion==SVER_UTF8 || serializerVersion==SVER_BUFF)
            {
              data.WriteInt32(addr, serializedSize);
              addr+=4;
              data.WriteBuffer(addr, buffer, 0, newPayloadSize - 4);
            }
            else
             data.WriteBuffer(addr, buffer, 0, newPayloadSize);
          }
          else
          {
            if (!link || payloadSize<Memory.PTR_RAW_BYTE_SIZE) return false;//does not fit

            //3 Create link
            data.WriteByte(addr, SVER_LINK);
            addr++;
            var lptr = put(seg, obj, buffer, serializedSize, serializerVersion, 0, true);//this is NOT a deadlock because put(tryGetWriteLock) does not cause deadlock
            //write pointer to data....
            data.WritePilePointer(addr, lptr);
          }
        }
        finally
        {
          releaseWriteLock(seg);
        }

        if (linkPointer.Valid) delete(linkPointer, false, true);

        return true;
      }

      /// <summary>
      /// Returns a CLR object by its pointer or throws access violation if pointer is invalid
      /// </summary>
      public object Get(PilePointer ptr)
      {
        byte sver;
        return get(ptr, false, out sver);
      }

      /// <summary>
      /// Returns a raw byte[] occupied by the object payload, only payload is returned along with serializer flag
      /// which tells what kind of serializer was used.
      /// This method is rarely used, it is needed for debugging and special-case "direct" memory access on read
      /// to bypass the de-serialization process altogether
      /// </summary>
      public byte[] GetRawBuffer(PilePointer ptr, out byte serializerFlag)
      {
        return (byte[])get(ptr, true, out serializerFlag);
      }

      /// <summary>
      /// Returns a CLR object by its pointer or throws access violation if pointer is invalid
      /// </summary>
      private object get(PilePointer ptr, bool raw, out byte serVersion)
      {
        serVersion = 0;
        if (!Running) return null;

        var segs = m_Segments;
        if (ptr.Segment<0 || ptr.Segment>=segs.Count)
         throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());


        var seg = segs[ptr.Segment];
        if (seg==null || Thread.VolatileRead(ref seg.DELETED)!=0)
          throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

        Interlocked.Increment(ref m_stat_GetCount);

        if (!getReadLock(seg)) return null;//Service shutting down
        try
        {
          //2nd check under lock
          if (Thread.VolatileRead(ref seg.DELETED)!=0) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

          var data = seg.Data;
          var addr = ptr.Address;
          if (addr<0 || addr>=data.Length-CHUNK_HDER_SZ)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_EOF_ERROR + ptr.ToString());

          var cflag = data.ReadChunkFlag(addr); addr+=3;
          if (cflag!=ChunkFlag.Used)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR + ptr.ToString());

          var payloadSize = data.ReadInt32(addr); addr+=4;
          if (payloadSize>data.Length)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_PAYLOAD_SIZE_ERROR.Args(ptr, payloadSize));


          serVersion = data.ReadByte(addr);
          addr++;

          if (serVersion == SVER_LINK)
          {
           var linkPointer = data.ReadPilePointer(addr);
           return get(linkPointer, raw, out serVersion);//this does not deadlock because read locks do not deadlock but write locks do TrygetWriteLock under main WriteLock
          }

          return raw ? readRaw(data, addr, payloadSize) : deserialize(data, addr, payloadSize, serVersion);
        }
        finally
        {
          releaseReadLock(seg);
        }
      }

      /// <summary>
      /// Deletes object from pile by its pointer returning true if there is no access violation
      /// and pointer is pointing to the valid object, throws otherwise unless
      /// throwInvalid is set to false
      /// </summary>
      public bool Delete(PilePointer ptr, bool throwInvalid = true)
      {
        return delete(ptr, throwInvalid, false);
      }

      public bool delete(PilePointer ptr, bool throwInvalid, bool isLink)
      {
        if (!Running) return false;

        var segs = m_Segments;
        if (ptr.Segment<0 || ptr.Segment>=segs.Count)
        {
           if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());
           return false;
        }


        var seg = segs[ptr.Segment];
        if (seg==null || Thread.VolatileRead(ref seg.DELETED)!=0)
        {
          if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());
          return false;
        }

        Interlocked.Increment(ref m_stat_DeleteCount);

        var removeEmptySegment = false;

        var linkPointer = PilePointer.Invalid;
        if (!getWriteLock(seg)) return false;//Service shutting down
        try
        {
          //2nd check under lock
          if (Thread.VolatileRead(ref seg.DELETED)!=0)
          {
           if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());
           return false;
          }

          var data = seg.Data;
          var addr = ptr.Address;
          if (addr<0 || addr>=data.Length-CHUNK_HDER_SZ)
          {
            if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_EOF_ERROR + ptr.ToString());
            return false;
          }

          var cflag = data.ReadChunkFlag(addr); addr+=3;
          if (cflag!=ChunkFlag.Used)
          {
            if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR + ptr.ToString());
            return false;
          }

          addr+=4;//skip over payload size

          var serVer = data.ReadByte(addr);

          if (serVer==SVER_LINK)
          {
             addr++;
             linkPointer = data.ReadPilePointer(addr);
          }

          seg.Deallocate(ptr.Address, isLink);

          //release segment
          //if nothing left allocated and either reuseSpace or 50/50 chance
          if (seg.AllObjectCount==0 && (m_AllocMode==AllocationMode.ReuseSpace || (ptr.Address & 1) == 1))
          {
            Thread.VolatileWrite(ref seg.DELETED, 1);//Mark as deleted
            removeEmptySegment = true;
          }
        }
        finally
        {
          releaseWriteLock(seg);
        }

        if (removeEmptySegment) //it is ok that THIS is not under segment write lock BECAUSE
        {
          lock(m_SegmentsLock)  // the segment was marked as DELETED, and it can not be re-allocated as it was marked under the lock
          {
              seg.DeleteAndDispose();//this will delete MMFs

              var newSegs = new List<_segment>(m_Segments);
              if (newSegs.Count>0)//safeguard, always true
              {
                  if (newSegs.Count==1)
                  {
                    if (newSegs[0]==seg) newSegs.Clear();
                  }
                  else
                  if (seg==newSegs[newSegs.Count-1])
                  newSegs.Remove( seg );
                  else
                  {
                    for(var k=0; k<newSegs.Count; k++)
                    if (newSegs[k] == seg)
                    {
                      newSegs[k] = null;
                      break;
                    }
                  }
              }
              m_Segments = newSegs;//atomic
          }//lock barrier
        }

        //delete linked object
        if (linkPointer.Valid) delete(linkPointer, false, true);

        return true;
      }

      public bool Rejuvenate(PilePointer ptr)
      {
        return false;
      }

      public int SizeOf(PilePointer ptr)
      {
        if (!Running) return 0;

        var segs = m_Segments;
        if (ptr.Segment<0 || ptr.Segment>=segs.Count)
         throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());


        var seg = segs[ptr.Segment];
        if (seg==null || Thread.VolatileRead(ref seg.DELETED)!=0)
          throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

        if (!getReadLock(seg)) return 0;//Service shutting down
        try
        {
          //2nd check under lock
          if (Thread.VolatileRead(ref seg.DELETED)!=0) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

          var data = seg.Data;
          var addr = ptr.Address;
          if (addr<0 || addr>=data.Length-CHUNK_HDER_SZ)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_EOF_ERROR + ptr.ToString());

          var cflag = data.ReadChunkFlag(addr); addr+=3;
          if (cflag!=ChunkFlag.Used)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR + ptr.ToString());

          var payloadSize = data.ReadInt32(addr); addr+=4;

          var serVer = data.ReadByte(addr);
          if (serVer!=SVER_LINK)
            return payloadSize;

          addr++;
          var linkPointer = data.ReadPilePointer(addr);
          return SizeOf(linkPointer);
        }
        finally
        {
          releaseReadLock(seg);
        }
      }

      public void Purge()
      {
        if (!Running) return;

        lock(m_SegmentsLock)
        {
          foreach(var seg in m_Segments)
          {
           if (seg==null) continue;
            if (getWriteLock(seg))
              try
              {
                seg.DeleteAndDispose();
              }
              finally
              {
                releaseWriteLock(seg);
              }
          }
          m_Segments = new List<_segment>();
        }
      }


      public long Compact()
      {
        if (!Running) return 0;

        long total = 0;

        lock(m_SegmentsLock)
        {
            var newSegments = new List<_segment>();

            foreach(var seg in m_Segments)
            {
                   if (seg==null || seg.AllObjectCount!=0)
                   {
                     newSegments.Add(seg);
                     continue;
                   }

                   if (!getWriteLock(seg)) return total;
                   try
                   {
                     if (seg.AllObjectCount!=0)
                     {
                       newSegments.Add(seg);
                       continue;
                     }
                     total += seg.Data.Length;
                     seg.DeleteAndDispose();
                     newSegments.Add(null);
                   }
                   finally
                   {
                     releaseWriteLock(seg);
                   }
            }//foreach

            //trim last empty
            while(newSegments.Count>0 && newSegments[newSegments.Count-1]==null)
             newSegments.RemoveAt(newSegments.Count-1);

            m_Segments = newSegments;//atomic
        }//lock

        return total;
      }

      public IEnumerator<PileEntry> GetEnumerator()
      {
        return new _pileEnumerator(this);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

    #endregion


    #region Protected

      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node==null || !node.Exists)
        {
            node = App.ConfigRoot[CommonApplicationLogic.CONFIG_MEMORY_MANAGEMENT_SECTION]
                      .Children
                      .FirstOrDefault(s => s.IsSameName(CONFIG_PILE_SECTION) && s.IsSameNameAttr(Name));
            if (node==null)
            {
                node = App.ConfigRoot[CommonApplicationLogic.CONFIG_MEMORY_MANAGEMENT_SECTION]
                        .Children
                        .FirstOrDefault(s => s.IsSameName(CONFIG_PILE_SECTION) && !s.AttrByName(Configuration.CONFIG_NAME_ATTR).Exists);
                if (node==null) return;
            }
        }

        ConfigAttribute.Apply(this, node);


        var fcs = node.AttrByName(CONFIG_FREE_CHUNK_SIZES_ATTR).ValueAsString();
        if (fcs.IsNotNullOrWhiteSpace())
          try
          {
             int[] isz = new int[FREE_LST_COUNT];
             var segs = fcs.Split(',',';');
             var i =0;
             foreach(var seg in segs)
             {
               if (seg.IsNullOrWhiteSpace()) continue;
               if (i==isz.Length) throw new PileException("cnt > "+FREE_LST_COUNT.ToString());
               isz[i] = int.Parse(seg);
               i++;
             }
             ensureFreeChunkSizes(isz);
             m_FreeChunkSizes = isz;
          }
          catch(Exception error)
          {
             throw new PileException(StringConsts.PILE_CONFIG_PROPERTY_ERROR.Args(CONFIG_FREE_CHUNK_SIZES_ATTR, error.ToMessageWithType()), error);
          }
      }


      protected override void DoStart()
      {
        ensureFreeChunkSizes( m_FreeChunkSizes );

        if (m_Identity>0)
        if (!PileInstances._____Register(this))
         throw new PileException(StringConsts.PILE_DUPLICATE_ID_ERROR.Args(Name, m_Identity));

        m_Segments = new List<_segment>();
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();

        foreach(var seg in m_Segments)
          if (seg!=null)
           try
           {
             seg.Dispose();
           }
           catch
           {
            //todo log?  FINISH!!!!!
            throw;//for now
           }

        if (m_Identity>0)
          PileInstances._____Unregister(this);

        m_Segments = null;
      }

      protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
      {
        dumpStats();
      }

      /// <summary>
      /// Override to make segmnet backed by the appropriate memory technology
      /// </summary>
      internal abstract _segment MakeSegment(int segmentNumber);


    #endregion


    #region .pvt .impl

        private IEnumerable<_segment> availSegs(List<_segment> segs)
        {
          return segs.Where( s => s!=null && s.DELETED==0);
        }

        private void ensureFreeChunkSizes(int[] sizes)
        {
          if (m_FreeChunkSizes==null || m_FreeChunkSizes.Length!=FREE_LST_COUNT)
            throw new PileException(StringConsts.PILE_CHUNK_SZ_ERROR.Args(FREE_LST_COUNT, FREE_CHUNK_SIZE_MIN));
          var psize = 0;
          foreach(var size in m_FreeChunkSizes)
          {
            if (size < FREE_CHUNK_SIZE_MIN || size <= psize)
              throw new PileException(StringConsts.PILE_CHUNK_SZ_ERROR.Args(FREE_LST_COUNT, FREE_CHUNK_SIZE_MIN));
            psize = size;
          }
        }


        //the reader lock allows to have many readers but only 1 writer
        private bool getReadLock(_segment segment)
        {
          return segment.RWSynchronizer.GetReadLock((_) => !this.Running);
        }

        private void releaseReadLock(_segment segment)
        {
          segment.RWSynchronizer.ReleaseReadLock();
        }

        //the writer lock allows only 1 writer at a time that conflicts with a single reader
        //contrast with tryGetWriteLock: this returns false on shutdown
        internal bool getWriteLock(_segment segment)
        {
          return segment.RWSynchronizer.GetWriteLock((_) => !this.Running);
        }

        //the writer lock allows only 1 writer at a time that conflicts with a single reader
        //true if taken, false if someone else locked (contrast with getWriteLock which returns false on shutdown)
        internal bool tryGetWriteLock(_segment segment)
        {
          return segment.RWSynchronizer.TryGetWriteLock();
        }

        internal void releaseWriteLock(_segment segment)
        {
          segment.RWSynchronizer.ReleaseWriteLock();
        }


     //@->-->----

          [ThreadStatic] private static SlimSerializer ts_WriteSerializer;


          private const int MIN_TLS_WRITE_CAPACITY = 96 * 1024;//LOH
          private const int TRIM_TLS_WRITE_CAPACITY = 512 * 1024;

          [ThreadStatic] private static MemoryStream ts_WriteStream;


          private MemoryStream getTLWriteStream()
          {
            MemoryStream result = ts_WriteStream;
            if (result!=null && result.Capacity < TRIM_TLS_WRITE_CAPACITY)
              return result;

            result = new MemoryStream( MIN_TLS_WRITE_CAPACITY );
            ts_WriteStream = result;
            return result;
          }


          private byte[] serialize(object payload, out int payloadSize, out byte serVersion)
          {
            var spayload = payload as string;
            if (spayload != null) //Special handlig for strings
            {
              serVersion = SVER_UTF8;
              var len = spayload.Length;
              var encoding = NFX.IO.Streamer.UTF8Encoding;
              var bufStream = getTLWriteStream();
              var maxChars = (bufStream.Capacity / 3) - 16;//UTF8 3 bytes per char - headers BOM etc..
              if (len>maxChars)//This is much faster than Encoding.GetByteCount()
              {
                var buf = encoding.GetBytes(spayload);
                payloadSize = 4 + buf.Length;//preamble + content
                return buf;
              }
              //try to reuse pre-allocated buffer
              var strBuf = bufStream.GetBuffer();
              payloadSize = 4 + encoding.GetBytes(spayload, 0, len, strBuf, 0);//preamble + content
              return strBuf;
            }
            //------------------------------------------------------------------------------
            var bpayload = payload as byte[];
            if (bpayload!=null)
            {
              serVersion = SVER_BUFF;
              payloadSize = 4 + bpayload.Length;//preamble + content
              return bpayload;
            }
            //------------------------------------------------------------------------------


            var stream = getTLWriteStream();
            var serializer = ts_WriteSerializer;
            if (serializer==null || serializer.Owner != this || serializer.__globalTypeRegistry!=m_CurrentTypeRegistry)
            {
                serializer = new SlimSerializer(m_CurrentTypeRegistry, SlimFormat.Instance);
                serializer.Owner = this;
                serializer.TypeMode = TypeRegistryMode.Batch;
                ts_WriteSerializer = serializer;
            }

            while(Running)
            {
              stream.Position = 0;
              serializer.Serialize(stream, payload);
              if (!serializer.BatchTypesAdded) break;

              TypeRegistry newReg;
              lock(m_CurrentTypeRegistryLock)
              {
                newReg = new TypeRegistry( m_CurrentTypeRegistry );
                foreach(var t in serializer.BatchTypeRegistry)
                  newReg.Add( t );

                m_CurrentTypeRegistry = newReg;
              }
              //re-allocate serializer with the new type registry
              serializer = new SlimSerializer(newReg, SlimFormat.Instance);
              serializer.Owner = this;
              serializer.TypeMode = TypeRegistryMode.Batch;
              ts_WriteSerializer = serializer;
            }//while

            payloadSize = (int)stream.Position;
            serVersion =  SVER_SLIM;
            return stream.GetBuffer();
          }


          private object readRaw(Memory data, int addr, int payloadSize)
          {
            var result = new byte[payloadSize];
            data.ReadBuffer(addr, result, 0, payloadSize);
            return result;
          }


          [ThreadStatic] private static SlimSerializer ts_ReadSerializer;

          private object deserialize(Memory data, int addr, int payloadSize, byte serVersion)
          {
            if (serVersion==SVER_UTF8)
            {
              var sz = data.ReadInt32(addr) - 4;//less preamble size
              addr+=4;
              return data.ReadUTF8String(addr, sz);
            }

            if (serVersion==SVER_BUFF)
            {
              var sz = data.ReadInt32(addr) - 4;//less preamble size
              addr+=4;
              var buff = new byte[sz];
              data.ReadBuffer(addr, buff, 0, sz);
              return buff;
            }


            var stream = data.GetReadStream(addr, payloadSize);
            var serializer = ts_ReadSerializer;

            if (serializer==null || serializer.Owner!=this || serializer.__globalTypeRegistry!=m_CurrentTypeRegistry)
            {
               serializer = new SlimSerializer(m_CurrentTypeRegistry, SlimFormat.Instance);
               serializer.Owner = this;
               serializer.TypeMode = TypeRegistryMode.Batch;
               ts_ReadSerializer = serializer;
            }

            while(Running)
              try
              {
                return serializer.Deserialize(stream);
              }
              catch(SlimDeserializationException e)
              {
                if (!(e.InnerException is SlimInvalidTypeHandleException)) throw;

                serializer = new SlimSerializer(m_CurrentTypeRegistry, SlimFormat.Instance);
                serializer.Owner = this;
                serializer.TypeMode = TypeRegistryMode.Batch;
                ts_ReadSerializer = serializer;
              }

            return null;
          }




          private void dumpStats()
          {
            var src = this.Name;
            var instr = App.Instrumentation;
            if (!instr.Enabled) return;


            var count = this.ObjectCount;
            instr.Record( new Instrumentation.ObjectCount(src, count) );
            instr.Record( new Instrumentation.SegmentCount(src, this.SegmentCount) );

            instr.Record( new Instrumentation.AllocatedMemoryBytes(src, this.AllocatedMemoryBytes) );

            var ub = this.UtilizedBytes;
            instr.Record( new Instrumentation.UtilizedBytes(src, ub) );

            var mc = this.MemoryCapacityBytes;
            instr.Record( new Instrumentation.MemoryCapacityBytes(src, mc) );
            instr.Record( new Instrumentation.OverheadBytes(src, this.OverheadBytes) );

            instr.Record( new Instrumentation.AverageObjectSizeBytes(src, count>0 ? ub / count : 0) );

            instr.Record( new Instrumentation.PutCount(src, m_stat_PutCount) );
            instr.Record( new Instrumentation.DeleteCount(src, m_stat_DeleteCount) );
            instr.Record( new Instrumentation.GetCount(src, m_stat_GetCount) );

            var segs = m_Segments;
            var totalFreeCapacities
                      = segs.Where(seg => seg!=null)
                            .Aggregate( new int[FREE_LST_COUNT],
                                       (sum, seg)  => seg.SnapshotOfFreeChunkCapacities.Zip(sum, (i1, i2) => i1+i2).ToArray()
                                      );

            for(var i=0; i<totalFreeCapacities.Length; i++)
              instr.Record( new Instrumentation.FreeListCapacity(src+"::"+ m_FreeChunkSizes[i].ToString().PadLeft(10), totalFreeCapacities[i]) );


            if (count>1000)
            {
              if (mc%3==0) ExternalRandomGenerator.Instance.FeedExternalEntropySample((int)count);
              ExternalRandomGenerator.Instance.FeedExternalEntropySample((int)mc);
              ExternalRandomGenerator.Instance.FeedExternalEntropySample((int)ub);
              ExternalRandomGenerator.Instance.FeedExternalEntropySample((int)((m_stat_PutCount << 25) ^ (m_stat_GetCount << 14) ^ m_stat_DeleteCount));
            }


            m_stat_PutCount = 0;
            m_stat_DeleteCount = 0;
            m_stat_GetCount = 0;
          }

    #endregion
  }

}
