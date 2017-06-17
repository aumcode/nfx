using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.IO;

namespace NFX.ApplicationModel.Pile
{

  /// <summary>
  /// Chunk flags: Free/Used
  /// </summary>
  internal enum ChunkFlag{ Wrong=0, Free, Used }


  /// <summary>
  /// INTERNAL implementation detail. This class is critical for performance.
  /// The  methods that read integers and pile pointer in a single call are more efficient that reading byte-by-byte.
  /// Provides abstraction for working with raw memory chunk.
  /// The chunk may be a maximum of 2 Gb in size.
  /// </summary>
  internal abstract class Memory : DisposableObject
  {
    //note: for better dump readability and protection bytes should not intersect with each other
    public const int CHUNK_USED_FLAG1 = 0x03;//0x03AC0B
    public const int CHUNK_USED_FLAG2 = 0xAC;//0x03AC0B
    public const int CHUNK_USED_FLAG3 = 0x0B;//0x03AC0B

    public const int CHUNK_FREE_FLAG1 = 0xCB;//0xCBAB0D
    public const int CHUNK_FREE_FLAG2 = 0xAB;//0xCBAB0D
    public const int CHUNK_FREE_FLAG3 = 0x0D;//0xCBAB0D


    //For efficiency PilePointer MUST be of fixed size: PilePointer.RAW_BYTE_SIZE
    public const int PTR_RAW_BYTE_SIZE = sizeof(int) + sizeof(int) + sizeof(int);//node+seg+addr


    public abstract void DeleteAndDispose();

    public abstract int Length{ get;}

    public abstract ChunkFlag ReadChunkFlag(int addr);
    public abstract void WriteFreeChunkFlag(int addr);
    public abstract void WriteUsedChunkFlag(int addr);

    public abstract byte ReadByte(int addr);
    public abstract void WriteByte(int addr, byte b);

    public abstract int ReadInt32(int addr);
    public abstract void WriteInt32(int addr, int i);

    public abstract void ReadBuffer(int addr, byte[] buffer, int offset, int count);
    public abstract void WriteBuffer(int addr, byte[] buffer, int offset, int count);

    //WARNING - all implementations must create the same footprint in memory.
    //For efficiency PilePointer MUST be of fixed size: PilePointer.PTR_RAW_BYTE_SIZE
    public abstract PilePointer ReadPilePointer(int addr);
    public abstract void WritePilePointer(int addr, PilePointer ptr);


    public abstract string ReadUTF8String(int addr, int size);

    public abstract BufferSegmentReadingStream GetReadStream(int addr, int count);
  }

}
