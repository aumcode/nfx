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

using NFX.Environment;
using NFX.Security;


namespace NFX.IO.FileSystem.Local
{
  /// <summary>
  /// Implements NFX.IO.FileSystem support around local machine file system. This is needed for
  ///  components that may need to work with various file systems i.e. Apache HDFS or Aum Cluster File System (ACFS).
  /// This particular implementation uses traditional System.IO.* and does not support transactions, versioning, metadata and NFX security
  /// </summary>
  public sealed class LocalFileSystem : FileSystem
  {
                #region Inner classes

                      internal class FSH : IFileSystemHandle
                      {
                        public FileSystemInfo m_Info;
                      }

                #endregion

    #region .ctor
       public LocalFileSystem() : base(null)
       {
       }

       public LocalFileSystem(string name, IConfigSectionNode node = null) : base(name, node)
       {
       }

    #endregion

    #region Properties

      public override IFileSystemCapabilities GeneralCapabilities { get { return LocalFileSystemCapabilities.Instance; }}

      public override IFileSystemCapabilities InstanceCapabilities  { get { return LocalFileSystemCapabilities.Instance; }}

    #endregion

    #region Public

      public override FileSystemSession StartSession(FileSystemSessionConnectParams cParams = null)
      {
        if (cParams==null) cParams = new FileSystemSessionConnectParams();
        return new FileSystemSession(this, null, cParams);
      }

    #endregion

    #region Properties
      public override string ComponentCommonName { get { return "fslocal"; }}
    #endregion

    #region Protected

      protected internal override void DoRefresh(FileSystemSessionItem item)
      {
        var fsh = item.Handle as FSH;
        if (fsh==null) return;
        var fsi = fsh.m_Info;
        fsi.Refresh();
      }

      protected internal override IEnumerable<string> DoGetSubDirectoryNames(FileSystemDirectory directory, bool recursive)
      {
        var di = new DirectoryInfo(directory.Path);

        return  di.GetDirectories("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                  .Select( sdi => sdi.Name );
      }

      protected internal override IEnumerable<string> DoGetFileNames(FileSystemDirectory directory, bool recursive)
      {
        var dirPath = directory.Path;
        var di = new DirectoryInfo(dirPath);

        return  di.GetFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                  .Select( fi => Path.GetFileName( fi.FullName ) );
      }

      protected internal override FileSystemSessionItem DoNavigate(FileSystemSession session, string path)
      {
        if (File.Exists(path))
        {
          var fi = new FileInfo(path);
          return new FileSystemFile(session, fi.DirectoryName, fi.Name, new FSH{m_Info = fi});
        }

        if (Directory.Exists(path))
        {
          var di = new DirectoryInfo(path);
          return new FileSystemDirectory(session, di.Parent.FullName, di.Name, new FSH{m_Info=di});
        }
        return null;
      }

      protected internal override bool DoRenameItem(FileSystemSessionItem item, string newName)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        if (fsi is FileInfo)
        {
          File.Move(fsi.FullName, Path.Combine(((FileInfo)fsi).DirectoryName, newName) );
          return true;
        }
        if (fsi is DirectoryInfo)
        {
          Directory.Move(fsi.FullName, Path.Combine(((DirectoryInfo)fsi).Parent.FullName, newName) );
          return true;
        }

        return false;
      }

      protected internal override void DoDeleteItem(FileSystemSessionItem item)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        if (fsi is DirectoryInfo)
         ((DirectoryInfo)fsi).Delete(true);
        else
         fsi.Delete();
      }

      protected internal override FileSystemFile DoCreateFile(FileSystemDirectory dir, string name, int size)
      {
        var fn = Path.Combine(dir.Path, name);
        using(var fs = new FileStream(fn, FileMode.Create, FileAccess.Write))
        {
          if (size>0)
          {
           fs.Seek(size-1, SeekOrigin.Begin);
           fs.WriteByte(0);
          }
        }

        var fi = new FileInfo(fn);
        return new FileSystemFile(dir.Session, fi.DirectoryName, fi.Name, new FSH{m_Info = fi});
      }

      protected internal override FileSystemFile DoCreateFile(FileSystemDirectory dir, string name, string localFile, bool readOnly)
      {
        var fn = Path.Combine(dir.Path, name);
        File.Copy(localFile, fn, true);

        var fi = new FileInfo(fn);
        fi.IsReadOnly = readOnly;
        return new FileSystemFile(dir.Session, fi.DirectoryName, fi.Name, new FSH{m_Info = fi});
      }

      protected internal override FileSystemDirectory DoCreateDirectory(FileSystemDirectory dir, string name)
      {
        var dn = Path.Combine(dir.Path, name);

        var di = new DirectoryInfo(dn);
        di.Create();
        di.Refresh();
        return new FileSystemDirectory(dir.Session, di.Parent.FullName, name, new FSH{m_Info = di});
      }

      protected internal override ulong DoGetItemSize(FileSystemSessionItem item)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        if (fsi is DirectoryInfo)
        {
          var result = 0ul;
          var di = fsi as DirectoryInfo;

          var files = di.GetFiles("*.*", SearchOption.AllDirectories);

	        foreach(var file in files)
            result += (ulong)file.Length;

          return result;
        }
        else
         return (ulong)((FileInfo)fsi).Length;
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
        return new LocalFileSystemStream(file, disposeAction);
      }


      protected internal override DateTime? DoGetCreationTimestamp(FileSystemSessionItem item)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        return fsi.CreationTimeUtc;
      }

      protected internal override DateTime? DoGetModificationTimestamp(FileSystemSessionItem item)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        return fsi.LastWriteTimeUtc;
      }

      protected internal override DateTime? DoGetLastAccessTimestamp(FileSystemSessionItem item)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        return fsi.LastAccessTimeUtc;
      }

      protected internal override void DoSetCreationTimestamp(FileSystemSessionItem item, DateTime timestamp)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        fsi.CreationTimeUtc = timestamp;
      }

      protected internal override void DoSetModificationTimestamp(FileSystemSessionItem item, DateTime timestamp)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        fsi.LastWriteTimeUtc = timestamp;
      }

      protected internal override void DoSetLastAccessTimestamp(FileSystemSessionItem item, DateTime timestamp)
      {
        var fsi = ((FSH)item.Handle).m_Info;
        fsi.LastAccessTimeUtc = timestamp;
      }

      protected internal override bool DoGetReadOnly(FileSystemSessionItem item) //Windows does not support ReadOnly directories
      {
        var fi = ((FSH)item.Handle).m_Info as FileInfo;
        return fi!=null ? fi.IsReadOnly : false;
      }

      protected internal override void DoSetReadOnly(FileSystemSessionItem item, bool readOnly) //Windows does not support ReadOnly directories
      {
        var fi = ((FSH)item.Handle).m_Info as FileInfo;
        if (fi!=null)
          fi.IsReadOnly = readOnly;
      }


    #endregion

  }
}
