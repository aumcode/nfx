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
 * Author: Andrey Kolbasov <andrey@kolbasov.com>
 */
using System;
using System.IO;

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
  public class GoogleDriveStream : FileSystemStream
  {
    #region .ctor

      public GoogleDriveStream(FileSystemFile file, Action<FileSystemStream> disposeAction)
        : base(file, disposeAction)
      {
        m_Handle = (GoogleDriveHandle)file.Handle;
        m_Session = file.Session as GoogleDriveSession;
      }

    #endregion

    #region Private Fields / Properties

      private bool m_IsChanged;
      private MemoryStream m_Stream;
      private GoogleDriveHandle m_Handle;
      private GoogleDriveSession m_Session;

      private MemoryStream BufferStream
      {
          get
          {
              if (m_Stream == null)
              {
                  m_Stream = new MemoryStream();
                  m_Session.Client.GetFile(m_Handle.Id, m_Stream);
                  m_Stream.Position = 0;
                  m_IsChanged = false;
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
          DoFlush();
          m_Stream.Dispose();
        }

        base.Dispose(disposing);
      }

      protected override void DoFlush()
      {
        if (m_IsChanged)
        {
          m_Session.Client.UpdateFile(m_Handle.Id, BufferStream);
          m_IsChanged = false;
        }
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

      protected override long DoSeek(long offset, SeekOrigin origin)
      {
        return BufferStream.Seek(offset, origin);
      }

      protected override void DoSetLength(long value)
      {
        if (BufferStream.Length != value)
        {
          BufferStream.SetLength(value);
          m_IsChanged = true;
        }
      }

      protected override void DoWrite(byte[] buffer, int offset, int count)
      {
        BufferStream.Write(buffer, offset, count);
        m_IsChanged = true;
      }

    #endregion
  }
}
