
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2/16/2014 3:13:09 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.IO;

namespace NFX.IO.FileSystem.SVN
{
  public class SVNFileSystemStream : FileSystemStream
  {
    #region .ctor

      public SVNFileSystemStream(FileSystemFile file, Action<FileSystemStream> disposeAction) : base(file, disposeAction)
      {
        m_wdFile = ((SVNFileSystem.SVNFSH)file.Handle).Item as WebDAV.File;
        SVNFileSystem fs = file.FileSystem as SVNFileSystem;
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private readonly WebDAV.File m_wdFile;

      private MemoryStream m_Stream;

      private MemoryStream BufferStream
      {
        get
        {
          if (m_Stream == null)
          {
            m_Stream = new MemoryStream();
            m_wdFile.GetContent(m_Stream);
            m_Stream.Position = 0;
          }

          return m_Stream;
        }
      }

    #endregion

    #region Protected

      protected override void Dispose(bool disposing)
      {
        if (m_Stream != null)
        {
          m_Stream.Dispose();
        }

        base.Dispose(disposing);
      }

      protected override long DoGetLength()
      {
        return BufferStream.Length;
      }

      protected override long DoGetPosition()
      {
        return BufferStream.Position;
      }

      protected override void DoSetPosition(long position)
      {
        BufferStream.Position = position;
      }

      protected override int DoRead(byte[] buffer, int offset, int count)
      {
        return BufferStream.Read(buffer, offset, count);
      }

      protected override long DoSeek(long offset, System.IO.SeekOrigin origin)
      {
        return BufferStream.Seek(offset, origin);
      }

      protected override void DoFlush()
      {
        throw new NotImplementedException();
      }

      protected override void DoSetLength(long value)
      {
        throw new NotImplementedException();
      }

      protected override void DoWrite(byte[] buffer, int offset, int count)
      {
        throw new NotImplementedException();
      }

    #endregion

  } //SVNFileSystemStream

}
