using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.IO;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Represents local memory accessed via byte[]
  /// </summary>
  [NFX.Serialization.Slim.SlimSerializationProhibited]
  internal sealed class LocalMemory : Memory
  {
    public LocalMemory(int size)
    {
      m_Data = new byte[size];
    }

    public override void DeleteAndDispose()
    {
      m_Data = null;
      Dispose();
    }

    private byte[] m_Data;

    public override int Length { get{ return m_Data.Length;} }

    public override ChunkFlag ReadChunkFlag(int addr)
    {
      var first = m_Data[addr];

      if (first          == CHUNK_FREE_FLAG1 &&
          m_Data[addr+1] == CHUNK_FREE_FLAG2 &&
          m_Data[addr+2] == CHUNK_FREE_FLAG3) return ChunkFlag.Free;

      if (first          == CHUNK_USED_FLAG1 &&
          m_Data[addr+1] == CHUNK_USED_FLAG2 &&
          m_Data[addr+2] == CHUNK_USED_FLAG3) return ChunkFlag.Used;

      return ChunkFlag.Wrong;
    }

    public override void WriteFreeChunkFlag(int addr)
    {
       m_Data[addr]   = CHUNK_FREE_FLAG1;
       m_Data[addr+1] = CHUNK_FREE_FLAG2;
       m_Data[addr+2] = CHUNK_FREE_FLAG3;
    }

    public override void WriteUsedChunkFlag(int addr)
    {
       m_Data[addr]   = CHUNK_USED_FLAG1;
       m_Data[addr+1] = CHUNK_USED_FLAG2;
       m_Data[addr+2] = CHUNK_USED_FLAG3;
    }

    public override byte ReadByte(int addr)
    {
      return m_Data[addr];
    }

    public override void WriteByte(int addr, byte b)
    {
      m_Data[addr] = b;
    }

    public override int ReadInt32(int addr)
    {
      var a = addr;
      return m_Data.ReadBEInt32(ref a);
    }

    public override void WriteInt32(int addr, int i)
    {
      m_Data.WriteBEInt32(addr, i);
    }

    public override void ReadBuffer(int addr, byte[] buffer, int offset, int count)
    {
      Array.Copy(m_Data, addr, buffer, 0, count);
    }

    public override void WriteBuffer(int addr, byte[] buffer, int offset, int count)
    {
      Array.Copy(buffer, offset, m_Data, addr, count);
    }

    public override PilePointer ReadPilePointer(int addr)
    {
      var n = m_Data.ReadBEInt32(ref addr);
      var s = m_Data.ReadBEInt32(ref addr);
      var a = m_Data.ReadBEInt32(ref addr);
      return new PilePointer(n, s, a);
    }

    public override void WritePilePointer(int addr, PilePointer ptr)
    {
      m_Data.WriteBEInt32(addr, ptr.NodeID); addr+=4;
      m_Data.WriteBEInt32(addr, ptr.Segment); addr+=4;
      m_Data.WriteBEInt32(addr, ptr.Address);
    }

    public override string ReadUTF8String(int addr, int size)
    {
      return NFX.IO.Streamer.UTF8Encoding.GetString(m_Data, addr, size);
    }

    [ThreadStatic] private static BufferSegmentReadingStream ts_ReadStream;

    public override BufferSegmentReadingStream GetReadStream(int addr, int count)
    {
      BufferSegmentReadingStream result = ts_ReadStream;
      if (result==null)
      {
        result = new BufferSegmentReadingStream();
        ts_ReadStream = result;
      }
      result.UnsafeBindBuffer(m_Data, addr, count);
      return result;
    }
  }
}
