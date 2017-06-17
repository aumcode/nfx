using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;

using NFX.IO;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Represents memory backed by Memory Mapped Files (MMF)
  /// </summary>
  [NFX.Serialization.Slim.SlimSerializationProhibited]
  internal sealed class MMFMemory : Memory
  {
    public const string MMF_EXT = "mmf";
    public const string MMF_PREFIX = "pile-";

    /// <summary>
    /// Returns file names in the order of segment idex
    /// </summary>
    public static KeyValuePair<string, int>[] GetSegmentFileNames(string path)
    {
      var all = path.AllFileNamesThatMatch("{0}*.{1}".Args(MMF_PREFIX, MMF_EXT), false)
                    .Select( f => Path.GetFileName(f) );

      var result = new List<KeyValuePair<string, int>>();

      foreach(var fn in all)
      {
        var name = fn;
        if (!name.StartsWith(MMF_PREFIX, StringComparison.InvariantCultureIgnoreCase)) continue;//ignore non-pile files
        name = name.Substring(MMF_PREFIX.Length);//cut "pile-"
        var ie = name.LastIndexOf('.');
        if (ie<0) continue;//wrong file name
        name = name.Substring(0, ie);

        int idx;
        if (!int.TryParse(name, out idx)) continue;// wrong file name

        result.Add( new KeyValuePair<string, int>(fn, idx) );
      }

      result.Sort( (l, r) => l.Value.CompareTo(r.Value) );

      return result.ToArray();
    }


    public static string MakeFileName(int segment)
    {
      return "{0}{1:000000}.{2}".Args(MMF_PREFIX, segment, MMF_EXT);
    }

    /// <summary>
    /// Mounts existing segment file
    /// </summary>
    public MMFMemory(string dataPath, int segment)
    {
      var fn = MakeFileName( segment );

      m_FullFilePath = Path.Combine(dataPath, fn);

      m_File = MemoryMappedFile.CreateFromFile(
                       m_FullFilePath,
                       FileMode.Open,
                       fn, //Map name
                       0,
                       MemoryMappedFileAccess.ReadWrite);

      m_View = m_File.CreateViewAccessor();
    }

    /// <summary>
    /// Mounts new segment file
    /// </summary>
    public MMFMemory(string dataPath, int segment,  int length)
    {
      var fn = MakeFileName( segment );
      m_FullFilePath = Path.Combine(dataPath, fn);

      m_File = MemoryMappedFile.CreateFromFile(
                       m_FullFilePath,
                       FileMode.Create,
                       fn, //Map name
                       length,
                       MemoryMappedFileAccess.ReadWrite);

      m_View= m_File.CreateViewAccessor();
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_View);
      DisposeAndNull(ref m_File);
    }

    public override void DeleteAndDispose()
    {
      Dispose();
      NFX.IOMiscUtils.EnsureFileEventuallyDeleted(m_FullFilePath);
    }


    private string m_FullFilePath;
    private MemoryMappedFile m_File;
    private MemoryMappedViewAccessor m_View;


    public override int Length
    {
      get { return (int)m_View.Capacity; }
    }

    public unsafe override ChunkFlag ReadChunkFlag(int addr)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        var first = *ptr++;

        if (first  == CHUNK_FREE_FLAG1 &&
            *ptr++ == CHUNK_FREE_FLAG2 &&
            *ptr++ == CHUNK_FREE_FLAG3) return ChunkFlag.Free;

        if (first  == CHUNK_USED_FLAG1 &&
            *ptr++ == CHUNK_USED_FLAG2 &&
            *ptr++ == CHUNK_USED_FLAG3) return ChunkFlag.Used;

        return ChunkFlag.Wrong;
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }


    public unsafe override void WriteFreeChunkFlag(int addr)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        *ptr++ = CHUNK_FREE_FLAG1;
        *ptr++ = CHUNK_FREE_FLAG2;
        *ptr   = CHUNK_FREE_FLAG3;
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override void WriteUsedChunkFlag(int addr)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        *ptr++ = CHUNK_USED_FLAG1;
        *ptr++ = CHUNK_USED_FLAG2;
        *ptr   = CHUNK_USED_FLAG3;
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override byte ReadByte(int addr)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        return *ptr;
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override void WriteByte(int addr, byte b)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        *ptr = b;
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override int ReadInt32(int addr)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        return ((int)*ptr++ << 24) +
               ((int)*ptr++ << 16) +
               ((int)*ptr++ << 8)  +
                (int)*ptr;
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override void WriteInt32(int addr, int i)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        *ptr++ = (byte)(i >> 24);
        *ptr++ = (byte)(i >> 16);
        *ptr++ = (byte)(i >> 8);
        *ptr   = (byte)(i);
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override void ReadBuffer(int addr, byte[] buffer, int offset, int count)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), buffer, 0, count);
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override void WriteBuffer(int addr, byte[] buffer, int offset, int count)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        System.Runtime.InteropServices.Marshal.Copy(buffer, offset, new IntPtr(ptr), count);
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override PilePointer ReadPilePointer(int addr)
    {
      byte* ptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
      try
      {
        ptr += addr;
        var n = ((int)*ptr++ << 24) +
                ((int)*ptr++ << 16) +
                ((int)*ptr++ << 8)  +
                 (int)*ptr++;

        var s = ((int)*ptr++ << 24) +
                ((int)*ptr++ << 16) +
                ((int)*ptr++ << 8)  +
                 (int)*ptr++;


        var a = ((int)*ptr++ << 24) +
                ((int)*ptr++ << 16) +
                ((int)*ptr++ << 8)  +
                 (int)*ptr;

        return new PilePointer(n, s, a);
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public unsafe override void WritePilePointer(int addr, PilePointer ptr)
    {
      byte* bptr = null;
      m_View.SafeMemoryMappedViewHandle.AcquirePointer(ref bptr);
      try
      {
        bptr += addr;
        *bptr++ = (byte)(ptr.NodeID >> 24);
        *bptr++ = (byte)(ptr.NodeID >> 16);
        *bptr++ = (byte)(ptr.NodeID >> 8);
        *bptr++ = (byte)(ptr.NodeID);

        *bptr++ = (byte)(ptr.Segment >> 24);
        *bptr++ = (byte)(ptr.Segment >> 16);
        *bptr++ = (byte)(ptr.Segment >> 8);
        *bptr++ = (byte)(ptr.Segment);

        *bptr++ = (byte)(ptr.Address >> 24);
        *bptr++ = (byte)(ptr.Address >> 16);
        *bptr++ = (byte)(ptr.Address >> 8);
        *bptr   = (byte)(ptr.Address);
      }
      finally
      {
        m_View.SafeMemoryMappedViewHandle.ReleasePointer();
      }
    }

    public override string ReadUTF8String(int addr, int size)
    {
      var buf = new byte[size];
      ReadBuffer(addr, buf, 0, size);
      return NFX.IO.Streamer.UTF8Encoding.GetString(buf, 0, size);
    }

    [ThreadStatic] private static BufferSegmentReadingStream ts_ReadStream;

    public override BufferSegmentReadingStream GetReadStream(int addr, int count)
    {
      var buf = new byte[count];
      ReadBuffer(addr, buf, 0, count);//todo Add semantic for unsafe pointer to not make copies

      BufferSegmentReadingStream result = ts_ReadStream;
      if (result==null)
      {
        result = new BufferSegmentReadingStream();
        ts_ReadStream = result;
      }
      result.UnsafeBindBuffer(buf, 0, count); //todo Bind low-level memory accessor not to make any copies
      return result;
    }
  }
}
