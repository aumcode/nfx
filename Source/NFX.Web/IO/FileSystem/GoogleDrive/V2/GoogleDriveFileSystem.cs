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
using System.Linq;
using System.Collections.Generic;

using NFX.Environment;

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
  /// <summary>
  /// Implements FileSystem for Google Drive
  /// </summary>
  public class GoogleDriveFileSystem : FileSystem
  {
    #region .ctor

      public GoogleDriveFileSystem(string name, IConfigSectionNode node = null)
        : base(name, node)
      {

      }

    #endregion

    #region Public

      public override IFileSystemCapabilities GeneralCapabilities
      {
        get { return GoogleDriveCapabilities.Instance; }
      }

      public override IFileSystemCapabilities InstanceCapabilities
      {
        get { return GoogleDriveCapabilities.Instance; }
      }

      public GoogleDriveSession StartSession(GoogleDriveParameters cParams = null)
      {
        var gdParams = cParams ?? (DefaultSessionConnectParams as GoogleDriveParameters);

        if (gdParams == null)
        {
          throw new NFXException(NFX.Web.StringConsts.FS_SESSION_BAD_PARAMS_ERROR + this.GetType() + ".StartSession");
        }

        return new GoogleDriveSession(this, null, gdParams);
      }

      public override FileSystemSession StartSession(FileSystemSessionConnectParams cParams = null)
      {
        return this.StartSession(cParams as GoogleDriveParameters);
      }

    #endregion

    #region Protected

      protected internal override IEnumerable<string> DoGetSubDirectoryNames(FileSystemDirectory directory, bool recursive)
      {
        var session = (GoogleDriveSession)directory.Session;
        return session.Client.GetDirectories(directory.Path, recursive);
      }

      protected internal override IEnumerable<string> DoGetFileNames(FileSystemDirectory directory, bool recursive)
      {
        var session = (GoogleDriveSession)directory.Session;
        var handle = (GoogleDriveHandle)directory.Handle;
        return session.Client.GetFiles(handle.Id, recursive).Select(f => f.Name);
      }

      protected internal override FileSystemSessionItem DoNavigate(FileSystemSession session, string path)
      {
        if (path.IsNullOrEmpty())
        {
          return null;
        }

        var gds = (GoogleDriveSession)session;

        var client = gds.Client;

        var handle = client.GetHandle(path);

        if (handle == null)
        {
          return null;
        }

        var parentPath = GoogleDrivePath.GetParentPath(path);

        if (!handle.IsFolder)
        {
          return new FileSystemFile(gds, parentPath, handle.Name, handle);
        }

        return new FileSystemDirectory(gds, parentPath, handle.Name, handle);
      }

      protected internal override bool DoRenameItem(FileSystemSessionItem item, string newName)
      {
        var session = GetSession(item);
        var handle = GetHandle(item);

        session.Client.Rename(handle.Id, newName);

        return true;
      }

      protected internal override void DoDeleteItem(FileSystemSessionItem item)
      {
        var session = GetSession(item);
        var handle = GetHandle(item);

        session.Client.Delete(handle.Id);
      }

      protected internal override FileSystemFile DoCreateFile(FileSystemDirectory dir, string name, int size)
      {
        var parent = (GoogleDriveHandle)dir.Handle;
        var session = (GoogleDriveSession)dir.Session;

        var bytes = new byte[size];
        var stream = new MemoryStream(bytes);

        var handle = session.Client.CreateFile(parent.Id, name, stream);

        return new FileSystemFile(session, dir.Path, name, handle);
      }

      protected internal override FileSystemFile DoCreateFile(FileSystemDirectory dir, string name, string localFile, bool readOnly)
      {
        var session = (GoogleDriveSession)dir.Session;
        var parent = (GoogleDriveHandle)dir.Handle;
        var client = session.Client;

        using (var stream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
        {
          var handle = client.CreateFile(parent.Id, name, stream);
          return new FileSystemFile(dir.Session, dir.Path, name, handle);
        }
      }

      protected internal override FileSystemDirectory DoCreateDirectory(FileSystemDirectory dir, string name)
      {
        var parent = (GoogleDriveHandle)dir.Handle;
        var session = (GoogleDriveSession)dir.Session;

        var handle = session.Client.CreateDirectory(parent.Id, name);

        return new FileSystemDirectory(dir.Session, dir.Path, name, handle);
      }

      protected internal override ulong DoGetItemSize(FileSystemSessionItem item)
      {
        var session = (GoogleDriveSession)item.Session;
        var handle = (GoogleDriveHandle)item.Handle;
        var client = session.Client;

        if (!handle.IsFolder)
        {
          return handle.Size;
        }
        else if (item is FileSystemDirectory)
        {
          var files = client.GetFiles(handle.Id, recursive: true);

          var size = 0ul;

          foreach (var file in files)
          {
            size += file.Size;
          }

          return size;
        }

        throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".DoGetItemSize(item is FileSystemFile or FileSystemDirectory)");
      }

      protected internal override FileSystemStream DoGetPermissionsStream(FileSystemSessionItem item, Action<FileSystemStream> disposeAction)
      {
        return null;
      }

      protected internal override FileSystemStream DoGetMetadataStream(FileSystemSessionItem item, Action<FileSystemStream> disposeAction)
      {
        return null;
      }

      protected internal override FileSystemStream DoGetFileStream(FileSystemFile file, Action<FileSystemStream> disposeAction)
      {
        return new GoogleDriveStream(file, disposeAction);
      }

      protected internal override DateTime? DoGetCreationTimestamp(FileSystemSessionItem item)
      {
        return GetHandle(item).CreatedDate;
      }

      protected internal override DateTime? DoGetModificationTimestamp(FileSystemSessionItem item)
      {
        return GetHandle(item).ModifiedDate;
      }

      protected internal override DateTime? DoGetLastAccessTimestamp(FileSystemSessionItem item)
      {
        throw new NotImplementedException();
      }

      protected internal override void DoSetCreationTimestamp(FileSystemSessionItem item, DateTime timestamp)
      {
        throw new NotImplementedException();
      }

      protected internal override void DoSetModificationTimestamp(FileSystemSessionItem item, DateTime timestamp)
      {
        var session = GetSession(item);
        var handle = GetHandle(item);

        session.Client.SetModifiedDate(handle.Id, timestamp);

        handle.ModifiedDate = timestamp;
      }

      protected internal override void DoSetLastAccessTimestamp(FileSystemSessionItem item, DateTime timestamp)
      {
        throw new NotImplementedException();
      }

      protected internal override bool DoGetReadOnly(FileSystemSessionItem item)
      {
        return GetHandle(item).IsReadOnly;
      }

      protected internal override void DoSetReadOnly(FileSystemSessionItem item, bool readOnly)
      {
        throw new NotImplementedException();
      }

    #endregion

    #region Private

      private static GoogleDriveHandle GetHandle(FileSystemSessionItem item)
      {
          return (GoogleDriveHandle)item.Handle;
      }

      private static GoogleDriveSession GetSession(FileSystemSessionItem item)
      {
          return (GoogleDriveSession)item.Session;
      }

    #endregion
  }
}
