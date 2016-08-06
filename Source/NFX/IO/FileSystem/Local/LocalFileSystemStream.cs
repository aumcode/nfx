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

namespace NFX.IO.FileSystem.Local
{
  internal class LocalFileSystemStream : FileSystemStream
  {

    public LocalFileSystemStream(FileSystemFile file, Action<FileSystemStream> disposeAction) : base(file, disposeAction)
    {
      var fa = FileAccess.ReadWrite;

      var hndl = file.Handle as LocalFileSystem.FSH;
      if (hndl!=null)
      {
        if (((FileInfo)hndl.m_Info).IsReadOnly)
          fa = FileAccess.Read;
      }
      m_FS = new FileStream(file.Path, FileMode.OpenOrCreate, fa, FileShare.ReadWrite);
    }

    protected override void Dispose(bool disposing)
    {
      m_FS.Dispose();
      base.Dispose(disposing);
    }

    private FileStream m_FS;


    protected override void DoFlush()
    {
      m_FS.Flush();
    }

    protected override long DoGetLength()
    {
      return m_FS.Length;
    }

    protected override long DoGetPosition()
    {
      return m_FS.Position;
    }

    protected override void DoSetPosition(long position)
    {
      m_FS.Position = position;
    }

    protected override int DoRead(byte[] buffer, int offset, int count)
    {
      return m_FS.Read(buffer, offset, count);
    }

    protected override long DoSeek(long offset, SeekOrigin origin)
    {
      return m_FS.Seek(offset, origin);
    }

    protected override void DoSetLength(long value)
    {
      m_FS.SetLength(value);
    }

    protected override void DoWrite(byte[] buffer, int offset, int count)
    {
      m_FS.Write(buffer, offset, count);
    }
  }
}
