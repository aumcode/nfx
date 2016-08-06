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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NFX.IO
{
  /// <summary>
  /// Implements a read-only stream wrapper around a segment of byte[].
  /// Unlike MemoryStream, this class allows for use of long indexes and reuse the same stream instance with different byte[].
  /// </summary>
  public sealed class BufferSegmentReadingStream : Stream
  {
    public BufferSegmentReadingStream()
    {
    }

    protected override void Dispose(bool disposing)
    {

    }

    private byte[] m_Buffer;
    private long m_Offset;
    private long m_Count;
    private long m_Position;

    /// <summary>
    /// Target stream that this stream wraps
    /// </summary>
    public  byte[] Buffer { get{ return m_Buffer;} }

    /// <summary>
    /// Sets byte[] as stream source
    /// </summary>
    public void BindBuffer(byte[] buffer, long idxStart, long count)
    {
      if (buffer==null)
      throw new NFXIOException(StringConsts.ARGUMENT_ERROR+GetType().Name+".BindBuffer(buffer=null)");

      if (idxStart<0 || idxStart>=buffer.LongLength ||
          count<=0 ||
          idxStart+count>buffer.LongLength)
      throw new NFXIOException(StringConsts.ARGUMENT_ERROR+GetType().Name+".BindBuffer(idxStart={0}, count={1})".Args(idxStart, count));

      m_Buffer = buffer;
      m_Offset = idxStart;
      m_Count = count;
      m_Position = 0;
    }


    public override void Close()
    {

    }

    public override bool CanRead { get { return true;}}

    public override bool CanSeek { get { return true;}}

    public override bool CanWrite { get { return false;}}

    public override void Flush()
    {
      throw new NFXIOException(StringConsts.IO_STREAM_NOT_SUPPORTED_ERROR.Args(GetType().FullName, "Flush()"));
    }

    public override long Length { get { return m_Count;}}

    public override long Position
    {
      get
      {
        return m_Position;
      }
      set
      {
        if ( value < 0 || value>m_Count)
         throw new NFXIOException(StringConsts.IO_STREAM_POSITION_ERROR.Args(value, m_Count));
        m_Position = value;
      }
    }

    public override int ReadByte()
    {
      if (m_Position<m_Count)
      {
       var idx = m_Offset+m_Position;
       m_Position++;
       return m_Buffer[idx];
      }
      return -1;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (m_Position+count>m_Count) count = (int)(m_Count - m_Position);
      if (count<=0) return 0;//eof
      Array.Copy(m_Buffer, m_Offset+m_Position, buffer, (long)offset, (long)count);
      m_Position+=count;
      return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      long position;
      switch (origin)
      {
        case SeekOrigin.Begin:   { position = offset; break;}
        case SeekOrigin.Current: { position = m_Position + offset; break;}
        default: { position = m_Count + offset; break;}
      }
      Position = position;
      return position;
    }

    public override void SetLength(long value)
    {
      throw new NFXIOException(StringConsts.IO_STREAM_NOT_SUPPORTED_ERROR.Args(GetType().FullName, "SetLength()"));
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NFXIOException(StringConsts.IO_STREAM_NOT_SUPPORTED_ERROR.Args(GetType().FullName, "Write()"));
    }



  }
}
