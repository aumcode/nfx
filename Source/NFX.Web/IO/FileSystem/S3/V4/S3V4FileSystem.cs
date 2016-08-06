
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
 * Revision: NFX 1.0  2/16/2014 3:11:49 PM
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
using NFX.IO.FileSystem.S3.V4.S3V4Sign;
using System.Xml.Linq;
using System.IO;

namespace NFX.IO.FileSystem.S3.V4
{
  /// <summary>
  /// Implements FileSystem for Amazon S3
  /// </summary>
  public class S3V4FileSystem : FileSystem
  {
    #region CONSTS

      private const char PATH_SEPARATOR = '/';
      private static char[] PATH_SEPARATORS = new [] { PATH_SEPARATOR};

    #endregion

    #region Inner Types

      public class S3V4FSH : IFileSystemHandle
      {
        #region Static

          public static void SeparateLocalPath(string localPath, out string parentPath, out string name)
          {
            if (localPath.IsNullOrWhiteSpace())
            {
              parentPath = null; name = string.Empty;
              return;
            }

            localPath = localPath.Trim(PATH_SEPARATOR, ' ', '\t', '\r', '\n');

            var segs = localPath.Split(PATH_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);

            if (segs.Length == 0)
            {
              parentPath = null; name = string.Empty;
              return;
            }

            if (segs.Length == 1)
            {
              parentPath = string.Empty;
              name = segs[0].Trim(' ', '\t', '\r', '\n');
            }
            else
            {
              StringBuilder parentBuilder = new StringBuilder();
              for (int i = 0; i < segs.Length-1; i++)
              {
                if (i > 0) parentBuilder.Append(PATH_SEPARATOR);

                parentBuilder.Append(segs[i]);
              }

              parentPath = parentBuilder.ToString();

              name = segs[segs.Length-1].Trim(PATH_SEPARATOR, ' ', '\t', '\r', '\n');
            }
          }

        #endregion

        public S3V4FSH(string itemPath)
        {
          SeparateLocalPath( itemPath, out Parent, out Name);
          Path = Parent + PATH_SEPARATOR + Name;
        }

        //public S3V4FSH(string parent, string name)
        //{
        //  Parent = parent;
        //  Name = name;
        //  Path = Parent + PATH_SEPARATOR + Name;
        //}

        public readonly string Parent;
        public readonly string Name;
        public readonly string Path;

        public S3V4FSH(S3V4URI uri)
        {
          Uri = uri;
        }

        public readonly S3V4URI Uri;
      }

    #endregion

    #region .ctor

      public S3V4FileSystem(string name, IConfigSectionNode node = null) : base(name, node)
      {
        NFX.Web.WebSettings.RequireInitializedSettings();
      }

    #endregion

    #region Public

      public override string ComponentCommonName { get { return "fss3"; }}

      public override IFileSystemCapabilities GeneralCapabilities
      {
        get { return S3V4FileSystemCapabilities.Instance; }
      }

      public override IFileSystemCapabilities InstanceCapabilities
      {
        get { return S3V4FileSystemCapabilities.Instance; }
      }

      public S3V4FileSystemSession StartSession(S3V4FileSystemSessionConnectParams cParams)
      {
        S3V4FileSystemSessionConnectParams s3CParams = cParams ?? (DefaultSessionConnectParams as S3V4FileSystemSessionConnectParams);
        if (s3CParams == null)
          throw new NFXException(NFX.Web.StringConsts.FS_SESSION_BAD_PARAMS_ERROR + this.GetType() + ".StartSession");

        return new S3V4FileSystemSession(this, null, s3CParams);
      }

      public override FileSystemSession StartSession(FileSystemSessionConnectParams cParams = null)
      {
        return this.StartSession(cParams as S3V4FileSystemSessionConnectParams);
      }

    #endregion

    #region Protected

      protected internal override IEnumerable<string> DoGetSubDirectoryNames(FileSystemDirectory directory, bool recursive)
      {
        return getSubitemNames(directory, recursive).Where(i => i.IsFolder).Select(i => i.ItemName);
      }

      protected internal override IEnumerable<string> DoGetFileNames(FileSystemDirectory directory, bool recursive)
      {
        return getSubitemNames(directory, recursive).Where(i => !i.IsFolder).Select(i => i.ItemName);
      }

      protected internal override FileSystemSessionItem DoNavigate(FileSystemSession session, string path)
      {
        var s3session = (S3V4FileSystemSession)session;

        S3V4FSH handle = new S3V4FSH(path);

        if (path != "" && S3V4.FileExists(path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs))
        {
          return new FileSystemFile(s3session, handle.Parent, handle.Name, handle);
        }

        if (S3V4.FolderExists(path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs))
        {
          return new FileSystemDirectory(s3session, handle.Parent, handle.Name, handle);
        }

        return null;
      }

      protected internal override bool DoRenameItem(FileSystemSessionItem item, string newName)
      {
        throw new NotImplementedException();
      }

      protected internal override void DoDeleteItem(FileSystemSessionItem item)
      {
        var s3session = (S3V4FileSystemSession)item.Session;
        var handle = (S3V4FSH)item.Handle;

        FileSystemFile file = item as FileSystemFile;
        if (file != null)
        {
          S3V4.RemoveItem(handle.Path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs);
          return;
        }

        FileSystemDirectory dir = item as FileSystemDirectory;
        if (dir != null)
        {
          // reverse order to get children prior to parents
          foreach( S3V4ListBucketItem subItem in getSubitemNames( dir, true).Reverse())
          {
            string subItemPath = CombinePaths( handle.Path, subItem.Key);
            S3V4.RemoveItem(subItemPath, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs);
          }

          S3V4.RemoveFolder(handle.Path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs);
          return;
        }
      }

      protected internal override FileSystemFile DoCreateFile(FileSystemDirectory dir, string name, int size)
      {
        var s3session = (S3V4FileSystemSession)dir.Session;

        var filePath = this.CombinePaths(dir.Path, name);
        var handle = new S3V4FSH(filePath);

        byte[] bytes = new byte[size];
        MemoryStream contentStream = new MemoryStream(bytes);

        S3V4.PutFile(handle.Path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, contentStream, s3session.TimeoutMs);

        return new FileSystemFile(s3session, handle.Parent, handle.Name, handle);
      }

      protected internal override FileSystemFile DoCreateFile(FileSystemDirectory dir, string name, string localFile, bool readOnly)
      {
        var s3session = (S3V4FileSystemSession)dir.Session;
        var filePath = this.CombinePaths(dir.Path, name);
        var handle = new S3V4FSH(filePath);

        using(System.IO.FileStream r = new System.IO.FileStream(localFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
          S3V4.PutFile(filePath, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, r, s3session.TimeoutMs);
          return new FileSystemFile(dir.Session, handle.Parent, handle.Name, handle);
        }
      }

      protected internal override FileSystemDirectory DoCreateDirectory(FileSystemDirectory dir, string name)
      {
        var s3session = (S3V4FileSystemSession)dir.Session;
        var dirPath = this.CombinePaths(dir.Path, name);
        var handle = new S3V4FSH(dirPath);

        S3V4.PutFolder(handle.Path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs);

        return new FileSystemDirectory(s3session, handle.Parent, handle.Name, handle);
      }

      protected internal override ulong DoGetItemSize(FileSystemSessionItem item)
      {
        var s3session = (S3V4FileSystemSession)item.Session;
        var handle = item.Handle as S3V4FSH;

        FileSystemFile file = item as FileSystemFile;
        if (file != null)
        {
          IDictionary<string, string> metaHeaders = S3V4.GetItemMetadata(handle.Path,
            s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs);
          return (ulong)metaHeaders["Content-Length"].AsLong();
        }

        FileSystemDirectory dir = item as FileSystemDirectory;
        if (dir != null)
        {
          ulong size = 0;
          foreach( S3V4ListBucketItem subItem in getSubitemNames( dir, true).Where(si => !si.IsFolder))
          {
            size += subItem.Size;
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
        return new S3V4FileSystemStream(file, disposeAction, ((S3V4FileSystemSession)file.Session).TimeoutMs);
      }

      protected internal override DateTime? DoGetCreationTimestamp(FileSystemSessionItem item)
      {
        throw new NotImplementedException();
      }

      protected internal override DateTime? DoGetModificationTimestamp(FileSystemSessionItem item)
      {
        var s3session = (S3V4FileSystemSession)item.Session;
        var handle = item.Handle as S3V4FSH;

        var metaHeaders = S3V4.GetItemMetadata(handle.Path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region,
          s3session.TimeoutMs);
        return metaHeaders["Last-Modified"].AsDateTime();
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
        throw new NotImplementedException();
      }

      protected internal override void DoSetLastAccessTimestamp(FileSystemSessionItem item, DateTime timestamp)
      {
        throw new NotImplementedException();
      }

      protected internal override bool DoGetReadOnly(FileSystemSessionItem item)
      {
        return false;
      }

      protected internal override void DoSetReadOnly(FileSystemSessionItem item, bool readOnly)
      {
        throw new NotImplementedException();
      }

      protected override FileSystemSessionConnectParams MakeSessionConfigParams(IConfigSectionNode node)
      {
        return FileSystemSessionConnectParams.Make<S3V4FileSystemSessionConnectParams>(node);
      }

    #endregion

    #region .pvt. impl.

      private IEnumerable<S3V4ListBucketItem> getSubitemNames(FileSystemDirectory directory, bool recursive, int maxKeys = 1000)
      {
        var s3session = (S3V4FileSystemSession)directory.Session;
        var handle = directory.Handle as S3V4FSH;
        string prefix = handle.Path.ToDirectoryPath().TrimStart(PATH_SEPARATOR);

        string xml = S3V4.ListBucket(handle.Path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs,
          prefix, maxKeys: maxKeys);
        S3V4ListBucketResult list = S3V4ListBucketResult.FromXML(xml);

        while (list.IsTruncated)
        {
          xml = S3V4.ListBucket(handle.Path, s3session.AccessKey, s3session.SecretKey, s3session.Bucket, s3session.Region, s3session.TimeoutMs,
            prefix, marker: list.Items.Last().Key, maxKeys: maxKeys);
          list.AddXML(xml);
        }

        IEnumerable<S3V4ListBucketItem> q = list.Items;

        if (!recursive)
          q = q.Where(i => !i.IsNested);

        return q.ToList();
      }

    #endregion

  } //S3FileSystem

}
