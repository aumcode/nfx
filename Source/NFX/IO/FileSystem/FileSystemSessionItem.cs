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
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Security;

namespace NFX.IO.FileSystem
{
    /// <summary>
    ///  Represents an abstraction for items kept in a file system - i.e. directories and files.This class is NOT thread-safe
    /// </summary>
    public abstract class FileSystemSessionItem : DisposableObject, INamed
    {
      #region .ctor
        protected FileSystemSessionItem(FileSystemSession session,
                                        string parentPath,
                                        string name,
                                        IFileSystemHandle handle)
        {
            m_FileSystem = session.FileSystem;
            m_Session = session;
            m_ParentPath = parentPath;
            m_Handle = handle;
            m_Name = name;

            m_Session.m_Items.Add( this );
        }

        protected override void Destructor()
        {
            m_Session.m_Items.Remove( this );

            if (m_MetadataStream!=null)
            {
              m_MetadataStream.Dispose();
              m_MetadataStream = null;
            }

            if (m_PermissionsStream!=null)
            {
              m_PermissionsStream.Dispose();
              m_PermissionsStream = null;
            }

            base.Destructor();
        }
      #endregion

      #region Fields
        protected readonly FileSystem m_FileSystem;
        protected readonly FileSystemSession m_Session;
        private string m_ParentPath;

        private FileSystemStream m_PermissionsStream;
        private FileSystemStream m_MetadataStream;

        protected readonly IFileSystemHandle m_Handle;
        private string m_Name;
        protected internal bool m_Modified;

      #endregion

      #region Properties
        /// <summary>
        /// Returns file system handle for this entity
        /// </summary>
        public IFileSystemHandle Handle { get{ return m_Handle;} }

        public string Name
        {
          get { return m_Name;}
          set
          {
            if (m_Name==value) return;
            Rename( value );
          }
        }


        /// <summary>
        /// Indicates whether anything has changed since last transaction has started
        /// </summary>
        public bool Modified { get { return m_Modified;} }

        /// <summary>
        /// Returns an absolute path for this item
        /// </summary>
        public string Path
        {
          get
          {
            var result = ParentPath;

            result = m_FileSystem.CombinePaths(result, Name);

            return result;
          }
        }

         /// <summary>
        /// Returns path to directory that contains this item
        /// </summary>
        public string ParentPath { get { return m_ParentPath.IsNullOrWhiteSpace() ? m_FileSystem.InstanceCapabilities.PathSeparatorCharacters[0].ToString() : m_ParentPath;} }



        /// <summary>
        /// Returns filesystem - this is a shortcut to Session.FileSystem
        /// </summary>
        public FileSystem FileSystem { get { return m_FileSystem;} }


        /// <summary>
        /// Returns session through which this object was obtained
        /// </summary>
        public FileSystemSession Session { get { return m_Session;} }



        /// <summary>
        /// Returns item permissions stream
        /// </summary>
        public FileSystemStream PermissionsStream
        {
            get { return m_PermissionsStream ?? (m_PermissionsStream = FileSystem.DoGetPermissionsStream(this, (s) => m_PermissionsStream = null));}
        }

        /// <summary>
        /// Returns item metadata stream
        /// </summary>
        public FileSystemStream MetadataStream
        {
            get { return m_MetadataStream ?? (m_MetadataStream = FileSystem.DoGetMetadataStream(this, (s) => m_MetadataStream = null));}
        }

        /// <summary>
        /// Returns the byte size of the item, depending on implementation this property may return approximate sizes for files and directories
        /// (i.e in distributed systems)
        /// </summary>
        public ulong Size
        {
            get { return FileSystem.DoGetItemSize( this ); }
        }


                /// <summary>
                /// Async version of <see cref="P:Size"/>
                /// </summary>
                public Task<ulong> GetSizeAsync()
                {
                    return FileSystem.DoGetItemSizeAsync( this );
                }


        /// <summary>
        /// Gets/sets UTC creation timestamp
        /// </summary>
        public DateTime? CreationTimestamp
        {
            get { return FileSystem.DoGetCreationTimestamp(this); }
            set
            {
              if (!value.HasValue) throw new NFXIOException(StringConsts.ARGUMENT_ERROR+"CreationTimestamp=null");
              CheckCanChange();
              FileSystem.DoSetCreationTimestamp(this, value.Value);
              m_Modified = true;
            }
        }

                /// <summary>
                /// Async version of <see cref="P:CreationTimestamp"/>
                /// </summary>
                public Task SetCreationTimestampAsync(DateTime timestamp)
                {
                    return FileSystem.DoSetCreationTimestampAsync( this, timestamp );
                }

        /// <summary>
        /// Gets/sets UTC modification timestamp
        /// </summary>
        public DateTime? ModificationTimestamp
        {
            get { return FileSystem.DoGetModificationTimestamp(this); }
            set
            {
              if (!value.HasValue) throw new NFXIOException(StringConsts.ARGUMENT_ERROR+"ModificationTimestamp=null");
              CheckCanChange();
              FileSystem.DoSetModificationTimestamp(this, value.Value);
              m_Modified = true;
            }
        }

                /// <summary>
                /// Async version of <see cref="P:ModificationTimestamp"/>
                /// </summary>
                public Task SetModificationTimestampAsync(DateTime timestamp)
                {
                    return FileSystem.DoSetModificationTimestampAsync( this, timestamp );
                }

        /// <summary>
        /// Gets/sets UTC last access timestamp
        /// </summary>
        public DateTime? LastAccessTimestamp
        {
            get { return FileSystem.DoGetLastAccessTimestamp(this); }
            set
            {
              if (!value.HasValue) throw new NFXIOException(StringConsts.ARGUMENT_ERROR+"LastAccessTimestamp=null");
              CheckCanChange();
              FileSystem.DoSetLastAccessTimestamp(this, value.Value);
              m_Modified = true;
            }
        }

                /// <summary>
                /// Async version of <see cref="P:LastAccessTimestamp"/>
                /// </summary>
                public Task SetLastAccessTimestampAsync(DateTime timestamp)
                {
                    return FileSystem.DoSetLastAccessTimestampAsync( this, timestamp );
                }


        /// <summary>
        /// Gets the user who created this item
        /// </summary>
        public User CreationUser     { get { return FileSystem.DoGetCreationUser(this); }}

                /// <summary>
                /// Async version of <see cref="P:CreationUser"/>
                /// </summary>
                public Task<User> GetCreationUserAsync()     { return FileSystem.DoGetCreationUserAsync(this); }

        /// <summary>
        /// Gets the user who modified this item
        /// </summary>
        public User ModificationUser { get { return FileSystem.DoGetModificationUser(this); }}

                /// <summary>
                /// Async version of <see cref="P:ModificationUser"/>
                /// </summary>
                public Task<User> GetModificationUserAsync()     { return FileSystem.DoGetModificationUserAsync(this); }

        /// <summary>
        /// Gets the last user who accessed the item
        /// </summary>
        public User LastAccessUser    { get { return FileSystem.DoGetLastAccessUser(this); }}

                /// <summary>
                /// Async version of <see cref="P:LastAccessUser"/>
                /// </summary>
                public Task<User> GetLastAccessUserAsync()     { return FileSystem.DoGetLastAccessUserAsync(this); }


        /// <summary>
        /// Gets/sets readonly attribute
        /// </summary>
        public bool ReadOnly
        {
          get{ return FileSystem.DoGetReadOnly(this);}
          set
          {
            //Must be able to change flag even if file is read-only //CheckCanChange();
            FileSystem.DoSetReadOnly(this, value);
            m_Modified = true;
          }
        }

                /// <summary>
                /// Async version of <see cref="P:ReadOnly"/>
                /// </summary>
                public Task SetReadOnlyAsync(bool readOnly)
                {
                    return FileSystem.DoSetReadOnlyAsync( this, readOnly );
                }

        /// <summary>
        /// Indicates whether this item can change and file system supports modifications
        /// </summary>
        public bool IsReadOnly
        {
          get{ return m_FileSystem.InstanceCapabilities.IsReadonly || ReadOnly;}
        }

      #endregion


      #region Public Sync Methods

        /// <summary>
        /// Throws when item can not change
        /// </summary>
        public void CheckCanChange()
        {
          FileSystem.DoCheckCanChange( this );
          if (IsReadOnly)
            throw new NFXIOException(StringConsts.IO_FS_ITEM_IS_READONLY_ERROR.Args(this.m_FileSystem, this));
        }

                /// <summary>
                /// Async version of <see cref="CheckCanChange()"/>
                /// </summary>
                public Task CheckCanChangeAsync()
                {
                  return FileSystem.DoCheckCanChangeAsync(this);
                }

        /// <summary>
        /// Renames an item. Check file system capabilities to see if renaming is supported
        /// </summary>
        public void Rename( string newName)
        {
          CheckCanChange();
          EnsureObjectNotDisposed();
          if (m_Session.FileSystem.DoRenameItem(this, newName))
            m_Modified = true;
        }

                /// <summary>
                /// Async version of <see cref="Rename(string)"/>
                /// </summary>
                public Task RenameAsync( string newName)
                {
                  return CheckCanChangeAsync().ContinueWith(t => {
                    EnsureObjectNotDisposed();
                    m_Session.FileSystem.DoRenameItemAsync(this, newName).ContinueWith(t1 => {
                      if (t1.Result) m_Modified = true;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                  }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }


        /// <summary>
        /// Deletes this item from file system
        /// </summary>
        public void Delete()
        {
          CheckCanChange();
          EnsureObjectNotDisposed();
          m_FileSystem.DoDeleteItem(this);
          Dispose();
        }

                /// <summary>
                /// Async version of <see cref="Delete()"/>
                /// </summary>
                public Task DeleteAsync()
                {
                  return CheckCanChangeAsync().ContinueWith(t => {
                    EnsureObjectNotDisposed();
                    m_Session.FileSystem.DoDeleteItemAsync(this).ContinueWith(t1 => {
                      Dispose();
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                  }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }

        /// <summary>
        /// Refreshes the state represented by this item, i.e. this may re-read attributes from remote file system
        /// </summary>
        public void Refresh()
        {
          m_FileSystem.DoRefresh( this );
        }

                /// <summary>
                /// Async version of <see cref="Refresh()"/>
                /// </summary>
                public Task RefreshAsync()
                {
                  return m_FileSystem.DoRefreshAsync(this);
                }

        public override string ToString()
        {
          return "{0}({1})".Args(GetType().Name, Path);
        }

      #endregion
    }
}
