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

namespace NFX.IO.FileSystem.Local
{
  /// <summary>
  /// Describes local file system capabilities
  /// </summary>
  public sealed class LocalFileSystemCapabilities : IFileSystemCapabilities
  {
    #region Static /.ctor

      private static readonly char[] PATH_SEPARATORS = new char[]{System.IO.Path.DirectorySeparatorChar, '/'};

      private static LocalFileSystemCapabilities s_Instance = new LocalFileSystemCapabilities();

      private LocalFileSystemCapabilities() {}

      public static LocalFileSystemCapabilities Instance { get { return s_Instance;} }

    #endregion

    #region IFileSystemCapabilities

      public bool SupportsVersioning { get{ return false;} }

      public bool SupportsTransactions { get{ return false;} }

      public int MaxFilePathLength { get{ return 260;} }

      public int MaxFileNameLength { get{ return 0xff;} }

      public int MaxDirectoryNameLength { get{ return 0xff;} }

      public ulong MaxFileSize { get{ return 17592185978880;}}

      public char[] PathSeparatorCharacters  { get { return PATH_SEPARATORS; }}

      public bool IsReadonly { get{ return false;} }

      public bool SupportsSecurity { get{ return false;} }

      public bool SupportsCustomMetadata { get{ return false;} }

      public bool SupportsDirectoryRenaming { get{ return true;} }

      public bool SupportsFileRenaming { get{ return true;} }

      public bool SupportsStreamSeek { get{ return true;} }

      public bool SupportsFileModification { get{ return true;} }

      public bool SupportsCreationTimestamps     { get { return true;  }}
      public bool SupportsModificationTimestamps { get { return true;  }}
      public bool SupportsLastAccessTimestamps   { get { return true;  }}
      public bool SupportsReadonlyDirectories    { get { return false; }}
      public bool SupportsReadonlyFiles          { get { return true;  }}

      public bool SupportsCreationUserNames      { get { return false;  }}
      public bool SupportsModificationUserNames  { get { return false;  }}
      public bool SupportsLastAccessUserNames    { get { return false;  }}

      public bool SupportsFileSizes        { get { return true;  }}
      public bool SupportsDirectorySizes   { get { return true;  }}

      public bool SupportsAsyncronousAPI { get { return false; }}
    #endregion
  }
}
