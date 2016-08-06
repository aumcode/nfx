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
  /// Provides default implementation of IPile which stores objects on a local machine
  /// </summary>
  public sealed class DefaultPile : ServiceWithInstrumentationBase<object>, IPileImplementation
  {
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
       *  3. Serializer version: byte 0..255                [1 bytes] <--- not used for now
       *
       *                                               --------------
       *                                                     8 bytes  <--- CHUNK_HDR_SZ
       *  4. ..... chunk data .....
       *     payload bytes as stated in (2)               [(2) bytes]
      */


      private const int CHUNK_HDER_SZ = 8;

      //note: for better dump readability and protection bytes should not intersect with each other
      private const int CHUNK_USED_FLAG1 = 0x03;//0x03AC0B
      private const int CHUNK_USED_FLAG2 = 0xAC;//0x03AC0B
      private const int CHUNK_USED_FLAG3 = 0x0B;//0x03AC0B

      private const int CHUNK_FREE_FLAG1 = 0xCB;//0xCBAB0D
      private const int CHUNK_FREE_FLAG2 = 0xAB;//0xCBAB0D
      private const int CHUNK_FREE_FLAG3 = 0x0D;//0xCBAB0D


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


            private class freeChunks
            {
               public int[] Addresses;//the free addresses. From 0..CurrentIndex up to
               public int CurrentIndex;
            }

            /// <summary>
            /// Holds information obtained after a segment crawl
            /// </summary>
            public struct SegmentCrawlStatus
            {
               internal SegmentCrawlStatus(long crawledChunks, long originalFreeChunks, long resultFreeChunks, long freePayloadSize, long usedPayloadSize)
               {
                CrawledChunks      = crawledChunks;
                OriginalFreeChunks = originalFreeChunks;
                ResultFreeChunks   = resultFreeChunks;
                FreePayloadSize    = freePayloadSize;
                UsedPayloadSize    = usedPayloadSize;
               }
               public readonly long CrawledChunks;
               public readonly long OriginalFreeChunks;
               public readonly long ResultFreeChunks;
               public readonly long FreePayloadSize;
               public readonly long UsedPayloadSize;

               public SegmentCrawlStatus Sum(SegmentCrawlStatus other)
               {
                 return new SegmentCrawlStatus(
                  CrawledChunks      + other.CrawledChunks       ,
                  OriginalFreeChunks + other.OriginalFreeChunks  ,
                  ResultFreeChunks   + other.ResultFreeChunks    ,
                  FreePayloadSize    + other.FreePayloadSize     ,
                  UsedPayloadSize    + other.UsedPayloadSize
                 );
               }

               public override string ToString()
               {
                 return "Chunks crawled: {0:n0}, Free chunks orig: {1:n0}, Free chunks: {2:n0}, Free sz: {3:n0}, Used sz: {4:n0}"
                        .Args(CrawledChunks, OriginalFreeChunks, ResultFreeChunks, FreePayloadSize,  UsedPayloadSize);
               }

            }


            private class _segment
            {

               public _segment(DefaultPile pile)
               {
                 Pile = pile;
                 Data = new byte[pile.m_SegmentSize];//segment memory

                 //init 1 chunk for the whole segment
                 var adr = 0;
                 writeFreeFlag(Data, 0); adr+=3;//flag
                 Data.WriteBEInt32(adr, Data.Length - CHUNK_HDER_SZ);

                 FreeChunks = new freeChunks[FREE_LST_COUNT];
                 for(var i=0; i<FreeChunks.Length; i++)
                 {
                   var chunk = new freeChunks();
                   chunk.Addresses = new int[pile.m_FreeListSize];
                   chunk.CurrentIndex = -1;
                   FreeChunks[i] = chunk;
                 }
                 FreeChunks[FreeChunks.Length-1].CurrentIndex = 0;//mark the largest chunk as free
               }

               private readonly DefaultPile Pile;


               //be careful not to make this field readonly as interlocked(ref) just does not work in runtime
               public OS.ManyReadersOneWriterSynchronizer RWSynchronizer;


               public int DELETED;

               public int ObjectCount; //allocated objects
               public int UsedBytes;//object-allocated bytes WITHOUT headers

               public long OverheadBytes
               {
                 get
                 {
                   var chunkOverhead = (long)ObjectCount * CHUNK_HDER_SZ;
                   var listsOverhead = FreeChunks.LongLength * Pile.m_FreeListSize * sizeof(int);
                   return chunkOverhead + listsOverhead;
                 }
               }

               public int FreeCapacity
               {
                 get
                 {
                   var freePayload = Data.Length - UsedBytes;
                   var chunkOverhead = (ObjectCount * CHUNK_HDER_SZ) + CHUNK_HDER_SZ;//+1 extra chunk for 1 free space
                   return freePayload - chunkOverhead;
                 }
               }

               public freeChunks[] FreeChunks;//index is size # from CHUNK_SIZES

               public byte[] Data;//raw buffer
               public DateTime LastCrawl = DateTime.UtcNow;


               /// <summary>
               /// takes a snapshot of free chunk capacities for instrumentation
               /// </summary>
               public int[] SnapshotOfFreeChunkCapacities
               {
                 get { return FreeChunks.Select( fc => fc.CurrentIndex + 1).ToArray(); } //index 0 = 1 free element
               }

               //must be under lock
               //tries to allocate the payload size in this segment ant put payload bytes in.
               // returns -1 when could not find spot. does not do crawl
               public int Allocate(byte[] payloadBuffer, int allocPayloadSize, byte serVer)
               {
                  allocPayloadSize = IntMath.Align8(allocPayloadSize);
                  var allocSize = allocPayloadSize + CHUNK_HDER_SZ;

                  var fcs = Pile.FreeChunkSizes;

                  //Larger than the largest tracked free chunk
                  if (allocSize>fcs[fcs.Length-1])
                  {
                    var address = scanForFreeLarge(allocPayloadSize);
                    if (address<0) return -1;//no space in this segment to fit large payload
                    allocChunk(address, payloadBuffer, allocPayloadSize, serVer);
                    return address;
                  }

                  //else try to get a chunk from size-bucketed list
                  for(var i=0; i<fcs.Length; i++)
                  {
                    if (allocSize<=fcs[i])
                    {
                      var freeList = FreeChunks[i];
                      if (freeList.CurrentIndex>=0)//slot found
                      {
                        var address = freeList.Addresses[freeList.CurrentIndex];
                        freeList.CurrentIndex--;
                        allocChunk(address, payloadBuffer, allocPayloadSize, serVer);
                        return address;
                      }//slot found
                    }//<fcs
                  }
                  return -1;
               }


               private void allocChunk(int address, byte[] payloadBuffer, int allocPayloadSize, byte serVer)
               {
                  var allocSize = allocPayloadSize + CHUNK_HDER_SZ;

                  var adr = address;
                  adr+=3;//skip the flag
                  var pointedToPayloadSize = Data.ReadBEInt32(ref adr);
                  var pointedToSize = pointedToPayloadSize + CHUNK_HDER_SZ;

                  //see if the chunk is too big and may be re-split
                  var leftoverSize = pointedToSize - allocSize;
                  if ( leftoverSize < Pile.FreeChunkSizes[0])//then include tail in 1st as it it smaller than min chunk size
                  {                                          //and can not be split in the second chunk
                    allocPayloadSize+=leftoverSize;
                    allocSize+=leftoverSize;
                    leftoverSize=0;
                  }

                  adr = address;
                  //1st (used)
                  writeUsedFlag(Data, adr); adr+=3;//flag
                  Data.WriteBEInt32(adr, allocPayloadSize);
                  adr+=4;
                  Data[adr] = serVer;
                  adr++;
                  Array.Copy(payloadBuffer, 0, Data, adr, allocPayloadSize > payloadBuffer.Length ? payloadBuffer.Length : allocPayloadSize);

                  adr+=allocPayloadSize;

                  //see if the chunk is too big and may be re-split
                  if (leftoverSize >0)//then split
                  {
                    //2nd (free)
                    var splitPayloadSize = leftoverSize - CHUNK_HDER_SZ;
                    addFreeChunk(adr, splitPayloadSize);
                  }//split

                  UsedBytes += allocPayloadSize;
                  ObjectCount++;
               }

               private int scanForFreeLarge(int allocPayloadSize)
               {
                 var chunks = FreeChunks[FreeChunks.Length-1];//get the largest(last) list
                 for(var i=chunks.CurrentIndex; i>=0; i--)
                 {
                   var address = chunks.Addresses[i];
                   var adr = address;
                   adr+=3;//skip header
                   var payloadSize = Data.ReadBEInt32(ref adr);
                   if (allocPayloadSize<=payloadSize)
                   {
                     //shift array to delete [i] element
                     if (i<chunks.CurrentIndex)
                       Array.Copy(chunks.Addresses, i+1,
                                  chunks.Addresses, i, chunks.CurrentIndex - i);

                     chunks.CurrentIndex--;
                     return address;
                   }

                 }
                 return -1;//nothing found
               }

               public void Deallocate(int address)
               {
                 var flag = readChunkFlag(Data, address);
                 if (flag==chunkFlag.Used)
                 {
                   var adr = address;
                   //free chunk writes the flag anyway
                   //writeFreeFlag(Data, adr);
                   adr+=3;
                   var freedPayloadSize = Data.ReadBEInt32(ref adr);
                   addFreeChunk(address, freedPayloadSize);
                   UsedBytes -= freedPayloadSize;
                   ObjectCount--;
                 }
               }

               //must be called under lock
               //walks all segment chunks from first to last byte trying to consolidate the free chunks
               // and adds free chunks into free list
               public SegmentCrawlStatus Crawl()
               {
                  if (Thread.VolatileRead(ref this.DELETED)!=0) return new SegmentCrawlStatus();

                  var statCrawledChunks = 0;
                  var statOriginalFreeChunks = 0;
                  var statResultFreeChunks = 0;
                  var statFreePayloadSize = 0;
                  var statUsedPayloadSize = 0;

                  var adr = 0;
                  var contFreeStartAdr = -1;
                  var contFreeSize = 0;

                  var minSize = Pile.FreeChunkSizes[0];

                  clearAllFreeLists();

                  while(adr<Data.Length-minSize)
                  {
                    var chunkStartAdr = adr;
                    var flag = readChunkFlag(Data, adr);
                    if (flag==chunkFlag.Wrong)
                      throw new PileException(StringConsts.PILE_CRAWL_INTERNAL_SEGMENT_CORRUPTION_ERROR.Args(adr));

                    statCrawledChunks++;

                    adr += 3;

                    var payloadSize = Data.ReadBEInt32(ref adr);

                    adr += 1 + payloadSize;//1 for version, then skip the body of payload

                    if (flag==chunkFlag.Free)
                    {
                      statOriginalFreeChunks++;

                      if (contFreeStartAdr>=0)
                      { //if the prior chunk was free, then add this space to prior accumulated free space
                        contFreeSize+=payloadSize+CHUNK_HDER_SZ;
                      }
                      else
                      {
                        contFreeStartAdr = chunkStartAdr;
                        contFreeSize = payloadSize;
                        statResultFreeChunks++;
                      }
                      continue;
                    }
                    else
                      statUsedPayloadSize+=payloadSize;


                    //came to non-free block
                    if (contFreeStartAdr>=0)
                    {
                      //add the block to list
                      addFreeChunk(contFreeStartAdr, contFreeSize);
                      statFreePayloadSize += contFreeSize;
                      contFreeStartAdr = -1;
                    }

                  }//while

                  //flush remaining block
                  if (contFreeStartAdr>=0)
                  {
                    addFreeChunk(contFreeStartAdr, contFreeSize);
                    statFreePayloadSize += contFreeSize;
                  }

                  return new SegmentCrawlStatus(statCrawledChunks, statOriginalFreeChunks, statResultFreeChunks, statFreePayloadSize, statUsedPayloadSize);
               }


               private void clearAllFreeLists()
               {
                 for(var i=0; i<FreeChunks.Length; i++)
                   FreeChunks[i].CurrentIndex = -1;
               }


               private void addFreeChunk(int adr, int payloadSize)
               {
                  var chunkStartAdr = adr;
                  var allocSize = payloadSize + CHUNK_HDER_SZ;
                  writeFreeFlag(Data, adr);
                  adr += 3;
                  Data.WriteBEInt32(adr, payloadSize);
                  adr += 5;//4 + 1 ser version

                  //now try to add add addr ot free slot
                  var fcs = Pile.FreeChunkSizes;
                  for(var k=fcs.Length-1; k>=0;k--)
                  {
                    if (allocSize>=fcs[k])
                    {
                      var freeList = FreeChunks[k];
                      if (freeList.CurrentIndex<freeList.Addresses.Length-1)
                      {
                        freeList.CurrentIndex++;
                        freeList.Addresses[freeList.CurrentIndex] = chunkStartAdr;
                      }
                      break;
                    }
                  }
               }

            }




    #region .ctor

      public DefaultPile(string name = null):base()
      {
        Name = name;
        ctor();
      }

      public DefaultPile(object director, string name = null):base(director)
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

      private bool m_InstrumentationEnabled;
      private Time.Event m_InstrumentationEvent;

      private AllocationMode m_AllocMode;
      private long m_MaxMemoryLimit;
      private int m_MaxSegmentLimit;
      private int m_SegmentSize = SEG_SIZE_DFLT;
      private int m_FreeListSize = FREE_LST_SIZE_MIN;

      private object m_SegmentsLock = new object();
      private List<_segment> m_Segments;
      private int[] m_FreeChunkSizes = DEFAULT_FREE_CHUNK_SIZES;


      private object m_CurrentTypeRegistryLock = new object();
      private volatile TypeRegistry m_CurrentTypeRegistry;

      private long m_stat_PutCount;
      private long m_stat_DeleteCount;
      private long m_stat_GetCount;

    #endregion

    #region Properties

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
        public LocalityKind Locality { get { return LocalityKind.Local; }}

        /// <summary>
        /// Returns PilePersistence.Memory
        /// </summary>
        public ObjectPersistence Persistence { get{ return ObjectPersistence.Memory; }}

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
             return segs.Where(s=>s!=null).Sum( seg => (long)Thread.VolatileRead(ref seg.ObjectCount));
          }
        }

        /// <summary>
        /// Returns the number of bytes allocated by this pile from the system memory heap.
        /// As pile pre-allocates memory in segments, it is absolutely normal to have this property return 100s of megabytes even when pile is almost empty.
        /// This property may return close to all physical memory available
        /// </summary>
        public long AllocatedMemoryBytes
        {
          get
          {
             if (!Running) return 0;
             var segs = m_Segments;
             return segs.Where(s=>s!=null).Sum( seg => seg.Data.LongLength);
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
             return segs.Where(s=>s!=null).Sum( seg => (long)Thread.VolatileRead(ref seg.UsedBytes));
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
             return segs.Where(s=>s!=null).Sum( seg => seg.OverheadBytes);
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
             return segs.Count( s => s!=null);
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

      /// <summary>
      /// Puts a CLR object into the pile and returns a newly-allocated pointer.
      /// Throws out-of-space if there is not enough space in the pile and limits are set.
      /// Optional lifeSpanSec is ignored by this implementation
      /// </summary>
      public PilePointer Put(object obj, uint lifeSpanSec = 0)
      {
        if (!Running) return PilePointer.Invalid;

        if (obj==null) throw new PileException(StringConsts.ARGUMENT_ERROR+GetType().Name+".Put(obj==null)");

        Interlocked.Increment(ref m_stat_PutCount);

        //1 serialize to determine the size
        int serializedSize;
        byte serializerVersion;
        var buffer = serialize(obj, out serializedSize, out serializerVersion);

        var payloadSize = IntMath.Align8(serializedSize);
        var chunkSize = CHUNK_HDER_SZ + payloadSize;


        if (chunkSize>m_SegmentSize)
         throw new PileOutOfSpaceException(StringConsts.PILE_OBJECT_LARGER_SEGMENT_ERROR.Args(payloadSize));

        while(true)
        {
            if (!Running) return PilePointer.Invalid;

            var segs = m_Segments;

            for(var idxSegment=0; idxSegment<segs.Count; idxSegment++)
            {
              var seg = segs[idxSegment];
              if (seg==null) continue;
              if (seg.DELETED!=0) continue;

              var sused = seg.UsedBytes;
              if (seg.FreeCapacity > chunkSize)
              {
                 if (!getWriteLock(seg)) return PilePointer.Invalid;
                 try
                 {
                   if (Thread.VolatileRead(ref seg.DELETED)==0 && seg.FreeCapacity > chunkSize)
                   {
                     var adr = seg.Allocate(buffer, payloadSize, serializerVersion);
                     if (adr>=0) return new PilePointer(idxSegment, adr);//allocated before crawl

                     var utcNow = DateTime.UtcNow;
                     if ((utcNow - seg.LastCrawl).TotalSeconds > (m_AllocMode==AllocationMode.FavorSpeed ? 30 : 5))
                     {
                         //could not fit, try to reclaim
                         seg.Crawl();
                         seg.LastCrawl = utcNow;

                         //try again
                         adr = seg.Allocate(buffer, payloadSize, serializerVersion);
                         if (adr>=0) return new PilePointer(idxSegment, adr);//allocated after crawl
                     }
                     //if we are here - still could not allocate, will try next segment in iteration
                   }
                 }
                 finally
                 {
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

           var newSeg = new _segment(this);
           var newSegs = new List<_segment>(m_Segments);
           newSegs.Add( newSeg );
           var adr = newSeg.Allocate(buffer, payloadSize, serializerVersion);
           var pp = new PilePointer(newSegs.Count-1, adr);
           m_Segments = newSegs;
           return pp;
        }
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
        if (seg==null || seg.DELETED!=0)
          throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

        Interlocked.Increment(ref m_stat_GetCount);

        if (!getReadLock(seg)) return null;//Service shutting down
        try
        {
          //2nd check under lock
          if (seg.DELETED!=0) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

          var data = seg.Data;
          var addr = ptr.Address;
          if (addr<0 || addr>=data.LongLength-CHUNK_HDER_SZ)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_EOF_ERROR + ptr.ToString());

          var cflag = readChunkFlag(data, addr); addr+=3;
          if (cflag!=chunkFlag.Used)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR + ptr.ToString());

          var payloadSize = data.ReadBEInt32(ref addr);
          if (payloadSize>data.Length)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_PAYLOAD_SIZE_ERROR.Args(ptr, payloadSize));

          if (raw)
          {
             serVersion = data[addr];
          }
          //otherwise not used for now
          addr++;

          return raw ? readRaw(data, addr, payloadSize) : deserialize(data, addr, payloadSize);
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
        if (!Running) return false;

        var segs = m_Segments;
        if (ptr.Segment<0 || ptr.Segment>=segs.Count)
        {
           if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());
           return false;
        }


        var seg = segs[ptr.Segment];
        if (seg==null || seg.DELETED!=0)
        {
          if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());
          return false;
        }

        Interlocked.Increment(ref m_stat_DeleteCount);

        var removeEmptySegment = false;

        if (!getWriteLock(seg)) return false;//Service shutting down
        try
        {
          //2nd check under lock
          if (seg.DELETED!=0) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

          var data = seg.Data;
          var addr = ptr.Address;
          if (addr<0 || addr>=data.LongLength-CHUNK_HDER_SZ)
          {
            if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_EOF_ERROR + ptr.ToString());
            return false;
          }

          var cflag = readChunkFlag(data, addr); addr+=3;
          if (cflag!=chunkFlag.Used)
          {
            if (throwInvalid) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR + ptr.ToString());
            return false;
          }

          seg.Deallocate(ptr.Address);


          //release segment
          //if nothing left allocated and either reuseSpace or 50/50 chance
          if (seg.ObjectCount==0 && (m_AllocMode==AllocationMode.ReuseSpace || (ptr.Address & 1) == 1))
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
          lock(m_SegmentsLock)  // the segment was marked as DELETED, and it can not be re-allocated as it was marked under the lock
          {

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
        if (seg==null || seg.DELETED!=0)
          throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

        if (!getReadLock(seg)) return 0;//Service shutting down
        try
        {
          //2nd check under lock
          if (seg.DELETED!=0) throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_SEGMENT_ERROR + ptr.ToString());

          var data = seg.Data;
          var addr = ptr.Address;
          if (addr<0 || addr>=data.LongLength-CHUNK_HDER_SZ)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_EOF_ERROR + ptr.ToString());

          var cflag = readChunkFlag(data, addr); addr+=3;
          if (cflag!=chunkFlag.Used)
           throw new PileAccessViolationException(StringConsts.PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR + ptr.ToString());

          var payloadSize = data.ReadBEInt32(ref addr);
          return payloadSize;
        }
        finally
        {
          releaseReadLock(seg);
        }
      }

      public void Purge()
      {
        lock(m_SegmentsLock)
        {
          foreach(var seg in m_Segments)
          {
           if (seg!=null) Thread.VolatileWrite(ref seg.DELETED, 1);
          }
          m_Segments = new List<_segment>();
        }
      }


      public long Compact()
      {
        long total = 0;

        lock(m_SegmentsLock)
        {
            var newSegments = new List<_segment>();

            foreach(var seg in m_Segments)
            {
                   if (seg==null || Thread.VolatileRead(ref seg.ObjectCount)!=0)
                   {
                     newSegments.Add(seg);
                     continue;
                   }

                   if (!getWriteLock(seg)) return total;
                   try
                   {
                     if (Thread.VolatileRead(ref seg.ObjectCount)!=0)
                     {
                       newSegments.Add(seg);
                       continue;
                     }
                     Thread.VolatileWrite(ref seg.DELETED, 1);
                     total += seg.Data.LongLength;
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

        m_Segments = new List<_segment>();
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();

        m_Segments = null;
      }

      protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
      {
        dumpStats();
      }


    #endregion


    #region .pvt .impl

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
        private bool getWriteLock(_segment segment)
        {
          return segment.RWSynchronizer.GetWriteLock((_) => !this.Running);
        }

        private void releaseWriteLock(_segment segment)
        {
          segment.RWSynchronizer.ReleaseWriteLock();
        }


          private static void writeUsedFlag(byte[] data, int adr)
          {
            data[adr] = CHUNK_USED_FLAG1;
            data[adr+1] = CHUNK_USED_FLAG2;
            data[adr+2] = CHUNK_USED_FLAG3;
          }

          private static void writeFreeFlag(byte[] data, int adr)
          {
            data[adr] = CHUNK_FREE_FLAG1;
            data[adr+1] = CHUNK_FREE_FLAG2;
            data[adr+2] = CHUNK_FREE_FLAG3;
          }


          private enum chunkFlag{ Wrong, Free, Used }

          private static chunkFlag readChunkFlag(byte[] data, int adr)
          {
            if (data[adr]   == CHUNK_FREE_FLAG1 &&
                data[adr+1] == CHUNK_FREE_FLAG2 &&
                data[adr+2] == CHUNK_FREE_FLAG3) return chunkFlag.Free;

            if (data[adr]   == CHUNK_USED_FLAG1 &&
                data[adr+1] == CHUNK_USED_FLAG2 &&
                data[adr+2] == CHUNK_USED_FLAG3) return chunkFlag.Used;

            return chunkFlag.Wrong;
          }



          [ThreadStatic] private static SlimSerializer ts_WriteSerializer;

          private byte[] serialize(object payload, out int payloadSize, out byte serVersion)
          {
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
            serVersion = 0;//not used for now
            return stream.GetBuffer();
          }


          private object readRaw(byte[] data, int addr, int payloadSize)
          {
            var result = new byte[payloadSize];
            Array.Copy(data, addr, result, 0, payloadSize);
            return result;
          }


          [ThreadStatic] private static SlimSerializer ts_ReadSerializer;

          private object deserialize(byte[] data, int addr, int payloadSize)
          {
            var stream = getTLReadStream(data, addr, payloadSize);
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


          [ThreadStatic] private static BufferSegmentReadingStream ts_ReadStream;

          private BufferSegmentReadingStream getTLReadStream(byte[] buf, long idxStart, long count)
          {
            BufferSegmentReadingStream result = ts_ReadStream;
            if (result==null)
            {
             result = new BufferSegmentReadingStream();
             ts_ReadStream = result;
            }
            result.BindBuffer(buf, idxStart, count);
            return result;
          }



          [ThreadStatic] private static MemoryStream ts_WriteStream;


          private MemoryStream getTLWriteStream()
          {
            const int DEFAULT_TLS_WRITE_CAPACITY = 64 * 1024;
            MemoryStream result = ts_WriteStream;
            if (result!=null) return result;
            result = new MemoryStream( DEFAULT_TLS_WRITE_CAPACITY );
            ts_WriteStream = result;
            return result;
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
