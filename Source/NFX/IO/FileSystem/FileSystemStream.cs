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
using System.Threading;
using System.Threading.Tasks;


namespace NFX.IO.FileSystem
{
    /// <summary>
    /// Represents a stream that can work with contents of FileSystem. This class is NOT thread-safe
    /// </summary>
    public abstract class FileSystemStream : Stream
    {
        #region .ctor

          protected FileSystemStream(FileSystemSessionItem item, Action<FileSystemStream> disposeAction)
          {
            Item = item;
            m_DisposeAction = disposeAction;
          }

          protected override void Dispose(bool disposing)
          {
            if (m_DisposeAction!=null) m_DisposeAction(this);
            base.Dispose(disposing);
          }

        #endregion

        #region Fields
          private Action<FileSystemStream> m_DisposeAction;

          /// <summary>
          /// Item that this stream is for
          /// </summary>
          public readonly FileSystemSessionItem Item;

        #endregion

        #region Public
          public override bool CanRead
          {
            get { return true; }
          }

          public override bool CanSeek
          {
            get { return Item.FileSystem.InstanceCapabilities.SupportsStreamSeek; }
          }

          public override bool CanWrite
          {
            get { return !Item.IsReadOnly; }
          }


          public sealed override void Flush()
          {
            Item.CheckCanChange();
            DoFlush();
          }

                  public sealed override Task FlushAsync(CancellationToken ct)
                  {
                    return Item.FileSystem.DoFlushAsync(this, ct);
                  }

          public sealed override long Length
          {
            get { return DoGetLength(); }
          }

          public sealed override long Position
          {
            get
            {
              return DoGetPosition();
            }
            set
            {
              if (DoGetPosition()==value) return;
              Item.CheckCanChange();
              Item.m_Modified = true;
              DoSetPosition(value);
            }
          }

          public sealed override int Read(byte[] buffer, int offset, int count)
          {
            return DoRead(buffer, offset, count);
          }

                  public override sealed Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                  {
                    return Item.FileSystem.DoReadAsync(this, buffer, offset, count, cancellationToken);
                  }

          public sealed override long Seek(long offset, SeekOrigin origin)
          {
            var willChange = false;
            switch(origin)
            {
              case SeekOrigin.Begin: { if (offset > Length)       willChange = true; break; }
              case SeekOrigin.End:   { if (Length+offset > Length)willChange = true; break; } //offset may be negative
              default: //current
              {
                if (Position+offset > Length) willChange = true;
                break;
              }
            }

            if (willChange)
            {
              Item.CheckCanChange();
              Item.m_Modified = true;
            }
            return DoSeek(offset, origin);
          }


          public sealed override void SetLength(long value)
          {
            if (DoGetLength()==value) return;

            Item.CheckCanChange();
            Item.m_Modified = true;
            DoSetLength(value);
          }

          public sealed override void Write(byte[] buffer, int offset, int count)
          {
            Item.CheckCanChange();
            Item.m_Modified = true;
            DoWrite(buffer, offset, count);
          }

                  public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                  {
                    return Item.FileSystem.DoWriteAsync(this, buffer, offset, count, cancellationToken);
                  }

       #endregion

       #region Protected
          protected abstract void DoFlush();
          protected abstract long DoGetLength();
          protected abstract long DoGetPosition();
          protected abstract void DoSetPosition(long position);
          protected abstract int  DoRead(byte[] buffer, int offset, int count);
          protected abstract long DoSeek(long offset, SeekOrigin origin);
          protected abstract void DoSetLength(long value);
          protected abstract void DoWrite(byte[] buffer, int offset, int count);
       #endregion
    }
}
