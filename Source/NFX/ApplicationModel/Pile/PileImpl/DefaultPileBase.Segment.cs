using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NFX.ApplicationModel.Pile{ public abstract partial class DefaultPileBase{


  internal class freeChunks
  {
      public int[] Addresses;//the free addresses. From 0..CurrentIndex up to
      public int CurrentIndex;
  }


  internal class _segment : DisposableObject
  {

      public _segment(DefaultPileBase pile, Memory data, bool brandNew)
      {
        Pile = pile;
        Data = data;//segment memory

        FreeChunks = new freeChunks[FREE_LST_COUNT];
        for(var i=0; i<FreeChunks.Length; i++)
        {
          var chunk = new freeChunks();
          chunk.Addresses = new int[pile.m_FreeListSize];
          chunk.CurrentIndex = -1;
          FreeChunks[i] = chunk;
        }

        if (brandNew)
        {
          //init 1 chunk for the whole segment
          var adr = 0;
          data.WriteFreeChunkFlag(0); adr+=3;//flag
          data.WriteInt32(adr, Data.Length - CHUNK_HDER_SZ);
          FreeChunks[FreeChunks.Length-1].CurrentIndex = 0;//mark the largest chunk as free
          LOADED_AND_CRAWLED = true;
        }
        else
        {
          //Segment will be Crawled first
          LOADED_AND_CRAWLED = false;
        }
      }

      protected override void Destructor()
      {
        Thread.VolatileWrite(ref DELETED, 1);
        DisposeAndNull(ref Data);
      }

      public void DeleteAndDispose()
      {
        Thread.VolatileWrite(ref DELETED, 1);
        var data = Data;
        if (data!=null)
          data.DeleteAndDispose();
        Dispose();
      }


      private readonly DefaultPileBase Pile;


      //be careful not to make this field readonly as interlocked(ref) just does not work in runtime
      public OS.ManyReadersOneWriterSynchronizer RWSynchronizer;

      public bool LOADED_AND_CRAWLED;
      public int DELETED;

      public int ObjectCount; //allocated objects
      public int ObjectLinkCount; //allocated object links
      public int UsedBytes;//object-allocated bytes WITHOUT headers

      public freeChunks[] FreeChunks;//index is size # from CHUNK_SIZES

      public Memory Data;//raw memory
      public DateTime LastCrawl = DateTime.UtcNow;
      public int UsedBytesAtLastCrawl;


      public int AllObjectCount{ get{ return ObjectCount + ObjectLinkCount;}}

      public long OverheadBytes
      {
        get
        {
          var chunkOverhead = (long)AllObjectCount * CHUNK_HDER_SZ;
          var listsOverhead = FreeChunks.LongLength * Pile.m_FreeListSize * sizeof(int);
          return chunkOverhead + listsOverhead;
        }
      }

      public int FreeCapacity
      {
        get
        {
          var freePayload = Data.Length - UsedBytes;
          var chunkOverhead = (AllObjectCount * CHUNK_HDER_SZ) + CHUNK_HDER_SZ;//+1 extra chunk for 1 free space
          return freePayload - chunkOverhead;
        }
      }

      /// <summary>
      /// How much %-wise more space became available since last crawl
      /// </summary>
      public float FreeBytesChangePctSinceLastCrawl
      {
        get
        {
          var diff = UsedBytesAtLastCrawl - UsedBytes;
          return diff / (float)Data.Length;
        }
      }

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
      public int Allocate(byte[] payloadBuffer, int serializedSize, int allocPayloadSize, byte serVer, bool isLink)
      {
        allocPayloadSize = IntMath.Align8(allocPayloadSize);
        var allocSize = allocPayloadSize + CHUNK_HDER_SZ;

        var fcs = Pile.FreeChunkSizes;

        //Larger than the largest tracked free chunk
        if (allocSize>fcs[fcs.Length-1])
        {
          var address = scanForFreeLarge(allocPayloadSize);
          if (address<0) return -1;//no space in this segment to fit large payload
          allocChunk(address, payloadBuffer, serializedSize, allocPayloadSize, serVer, isLink);
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
              allocChunk(address, payloadBuffer, serializedSize, allocPayloadSize, serVer, isLink);
              return address;
            }//slot found
          }//<fcs
        }
        return -1;
      }


      private void allocChunk(int address, byte[] payloadBuffer, int serializedSize, int allocPayloadSize, byte serVer, bool isLink)
      {
        var allocSize = allocPayloadSize + CHUNK_HDER_SZ;

        var adr = address;
        adr+=3;//skip the flag
        var pointedToPayloadSize = Data.ReadInt32(adr); adr+=4;
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
        Data.WriteUsedChunkFlag(adr); adr+=3;//flag
        Data.WriteInt32(adr, allocPayloadSize);  adr+=4;
        Data.WriteByte(adr, serVer);
        adr++;

        var bsz = allocPayloadSize;
        if (serVer==SVER_UTF8 || serVer==SVER_BUFF)
        {
          //Write serializedSize preamble
          Data.WriteInt32(adr, serializedSize);
          adr+=4;
          bsz-=4;
        }

        Data.WriteBuffer(adr, payloadBuffer, 0, bsz > payloadBuffer.Length ? payloadBuffer.Length : bsz);
        adr+=bsz;

        //see if the chunk is too big and may be re-split
        if (leftoverSize >0)//then split
        {
          //2nd (free)
          var splitPayloadSize = leftoverSize - CHUNK_HDER_SZ;
          addFreeChunk(adr, splitPayloadSize);
        }//split

        UsedBytes += allocPayloadSize;

        if (isLink)
          ObjectLinkCount++;
        else
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
          var payloadSize = Data.ReadInt32(adr); adr+=4;
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

      public void Deallocate(int address, bool isLink)
      {
        var flag = Data.ReadChunkFlag(address);
        if (flag==ChunkFlag.Used)
        {
          var adr = address;
          //free chunk writes the flag anyway
          //writeFreeFlag(Data, adr);
          adr+=3;
          var freedPayloadSize = Data.ReadInt32(adr); adr+=4;

          if (LOADED_AND_CRAWLED)
            addFreeChunk(address, freedPayloadSize);

          UsedBytes -= freedPayloadSize;

          if (isLink)
            ObjectLinkCount--;
          else
            ObjectCount--;
        }
      }

      //must be called under lock
      //walks all segment chunks from first to last byte trying to consolidate the free chunks
      // and adds free chunks into free list
      public SegmentCrawlStatus Crawl()
      {
        if (Thread.VolatileRead(ref this.DELETED)!=0) return new SegmentCrawlStatus();


        var statObjectCount = 0;
        var statObjectLinkCount = 0;

        var statCrawledChunks = 0;
        var statOriginalFreeChunks = 0;
        var statResultFreeChunks = 0;
        var statFreePayloadSize = 0;
        var statUsedPayloadSize = 0;

        var addr = 0;
        var contFreeStartAdr = -1;
        var contFreeSize = 0;

        clearAllFreeLists();

        while(addr<Data.Length-CHUNK_HDER_SZ)
        {
          var chunkStartAdr = addr;
          var flag = Data.ReadChunkFlag(addr);
          if (flag==ChunkFlag.Wrong) //todo In future corruption recovery attempt may take place if we scan form 8-aligned block
            throw new PileException(StringConsts.PILE_CRAWL_INTERNAL_SEGMENT_CORRUPTION_ERROR.Args(addr));

          statCrawledChunks++;

          addr += 3;

          var payloadSize = Data.ReadInt32(addr); addr+=4;

          var sver = Data.ReadByte(addr); addr++; //read serializer version

          addr += payloadSize;//skip the body of payload

          if (flag==ChunkFlag.Free)
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
          {//chunk is USED
            statUsedPayloadSize+=payloadSize;

            if (sver==DefaultPileBase.SVER_LINK)
              statObjectLinkCount++;
            else
              statObjectCount++;
          }


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

        LastCrawl = DateTime.UtcNow;
        UsedBytesAtLastCrawl = statUsedPayloadSize;

        return new SegmentCrawlStatus(statObjectCount,
                                      statObjectLinkCount,

                                      statCrawledChunks,
                                      statOriginalFreeChunks,
                                      statResultFreeChunks,
                                      statFreePayloadSize,
                                      statUsedPayloadSize);
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
        Data.WriteFreeChunkFlag(adr);
        adr += 3;
        Data.WriteInt32(adr, payloadSize);
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






}}
