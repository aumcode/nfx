using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NFX.ApplicationModel.Pile{ public abstract partial class DefaultPileBase{

  //Enumerates the segment, the parallel segment mutations are ok, the snapshot stability of the view IS NOT guaranteed
  //the class is thread-safe
  private class _pileEnumerator : DisposableObject, IEnumerator<PileEntry>
  {
    private const int BUFFER_SIZE = 1024;

    public _pileEnumerator(DefaultPileBase pile) { m_Pile = pile; Reset();}

    protected override void Destructor()
    {
      m_Buffer = null;
    }


    private DefaultPileBase m_Pile;

    private int m_BufferIdx;
    private List<PileEntry> m_Buffer;
    private int m_SegmentIdx;
    private int m_Address;


    object System.Collections.IEnumerator.Current{ get{ return this.Current; } }

    public PileEntry Current
    {
      get { return !Disposed && m_BufferIdx < m_Buffer.Count ? m_Buffer[m_BufferIdx] : PileEntry.Invalid; }
    }

    public bool MoveNext()
    {
      if (Disposed) return false;
      m_BufferIdx++;
      if (m_BufferIdx<m_Buffer.Count) return true;

      if (!advanceBuffer()) return false;

      m_BufferIdx++;
      if (m_BufferIdx<m_Buffer.Count) return true;
      return false;
    }

    public void Reset()
    {
      if (Disposed) return;
      m_BufferIdx = -1;

      if (m_Buffer==null)
        m_Buffer = new List<PileEntry>(BUFFER_SIZE);
      else
        m_Buffer.Clear();

      m_SegmentIdx = 0;
      m_Address = 0;
    }

    private bool advanceBuffer()
    {
      var segs = m_Pile.m_Segments;
      if (segs.Count==0) return false;

      m_BufferIdx=-1;
      m_Buffer.Clear();

      while(true)
      {
        _segment seg = null;
        while(m_SegmentIdx < segs.Count)
        {
          seg = segs[m_SegmentIdx];
          if (seg==null || Thread.VolatileRead(ref seg.DELETED)!=0)
          {
            m_SegmentIdx++;
            m_Address = 0;
          }
          else
            break;
        }
        if (seg==null) return m_Buffer.Count>0;

        if (!m_Pile.getReadLock(seg)) return false;
        try
        {
          var eof = crawlSegment(seg);
          if (eof)
          {
            m_SegmentIdx++;
            m_Address = 0;
            continue;
          }
          return true;
        }
        finally
        {
          m_Pile.releaseReadLock(seg);
        }
      }
    }

    private bool crawlSegment(_segment seg)//must be called under segment read lock
    {
      const int BUFFER_SIZE = 1024;

      var addr = 0;//start crawling from very first byte to avoid segment corruption as previous address may have been deleted

      while(addr<seg.Data.Length-CHUNK_HDER_SZ)
      {
        var add = addr >= m_Address;

        var chunkStartAdr = addr;
        var flag = seg.Data.ReadChunkFlag(addr);
        if (flag==ChunkFlag.Wrong) //todo In future corruption recovery attempt may take place if we scan form 8-aligned block
            throw new PileException(StringConsts.PILE_CRAWL_INTERNAL_SEGMENT_CORRUPTION_ERROR.Args(addr));
        addr += 3;

        var payloadSize = seg.Data.ReadInt32(addr); addr+=4;

        var sver = seg.Data.ReadByte(addr); addr++; //read serializer version

        addr += payloadSize;//skip the body of payload

        if (!add) continue;

        m_Address = addr;//next chunk

        if (flag==ChunkFlag.Used)
        {
          var ptr = new PilePointer(m_SegmentIdx, chunkStartAdr);
          var dt = sver==SVER_UTF8 ? PileEntry.DataType.String :
                   sver==SVER_BUFF ? PileEntry.DataType.Buffer :
                   sver==SVER_LINK ? PileEntry.DataType.Link   : PileEntry.DataType.Object;
          var entry = new PileEntry(ptr, dt, payloadSize);
          m_Buffer.Add(entry);
          if (m_Buffer.Count==BUFFER_SIZE) return m_Address >= seg.Data.Length-CHUNK_HDER_SZ;
        }

      }//while

      return true;
    }

  }

}}
