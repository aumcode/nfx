
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2/16/2014 3:13:09 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using System.IO;

namespace NFX.IO.FileSystem.S3.V4
{
  public class S3V4FileSystemStream : FileSystemStream
  {
    #region .ctor

      public S3V4FileSystemStream(FileSystemFile file, Action<FileSystemStream> disposeAction, int timeoutMs) : base(file, disposeAction)
      {
        m_Handle = (S3V4FileSystem.S3V4FSH)file.Handle;

        m_timeoutMs = timeoutMs;

        m_Session = file.Session as S3V4FileSystemSession;

        m_IsNewFile = !S3V4.FileExists(m_Handle.Path, m_Session.AccessKey, m_Session.SecretKey, m_Session.Bucket, m_Session.Region,
          timeoutMs);
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private S3V4FileSystem.S3V4FSH m_Handle;

      private S3V4FileSystemSession m_Session;

      private MemoryStream m_Stream;

      private int m_timeoutMs;

      private bool m_IsNewFile; // if this file already exists or not
      private bool m_IsChanged; // has unsaved changes

      private MemoryStream BufferStream
      {
        get
        {
          if (m_Stream == null)
          {
            m_Stream = new MemoryStream();
            if (!m_IsNewFile)
            {
              S3V4.GetFile(m_Handle.Path, m_Session.AccessKey, m_Session.SecretKey, m_Session.Bucket, m_Session.Region, m_Stream, m_timeoutMs);
              m_Stream.Position = 0;
            }

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
          //S3V4.PutFile(m_Uri, m_Session.AccessKey, m_Session.SecretKey, BufferStream);
          S3V4.PutFile(m_Handle.Path, m_Session.AccessKey, m_Session.SecretKey, m_Session.Bucket, m_Session.Region, BufferStream, m_timeoutMs);
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

      protected override long DoSeek(long offset, System.IO.SeekOrigin origin)
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

  } //S3FileSystemStream

}
