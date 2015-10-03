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
using NFX.IO.FileSystem;

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
  public class GoogleDriveCapabilities : IFileSystemCapabilities
  {
    #region Static

      private static readonly char[] PATH_SEPARATORS = new char[] { '/' };

      private static GoogleDriveCapabilities s_Instance = new GoogleDriveCapabilities();

      public static GoogleDriveCapabilities Instance { get { return s_Instance; } }

    #endregion

    #region IFileSystemCapabilities

      public bool SupportsVersioning
      {
          get { return true; }
      }

      public bool SupportsTransactions
      {
          get { return false; }
      }

      public int MaxFilePathLength
      {
          get { return 260; }
      }

      public int MaxFileNameLength
      {
          get { return 255; }
      }

      public int MaxDirectoryNameLength
      {
          get { return 255; }
      }

      public ulong MaxFileSize
      {
          // 5120 GB
          get { return 5629499534213120; }
      }

      public char[] PathSeparatorCharacters
      {
          get { return PATH_SEPARATORS; }
      }

      public bool IsReadonly
      {
          get { return false; }
      }

      public bool SupportsSecurity
      {
          get { return false; }
      }

      public bool SupportsCustomMetadata
      {
          get { return false; }
      }

      public bool SupportsDirectoryRenaming
      {
          get { return true; }
      }

      public bool SupportsFileRenaming
      {
          get { return true; }
      }

      public bool SupportsStreamSeek
      {
          get { return false; }
      }

      public bool SupportsFileModification
      {
          get { return true; }
      }

      public bool SupportsCreationTimestamps
      {
          get { return false; }
      }

      public bool SupportsModificationTimestamps
      {
          get { return true; }
      }

      public bool SupportsLastAccessTimestamps
      {
          get { return false; }
      }

      public bool SupportsReadonlyDirectories
      {
          get { return false; }
      }

      public bool SupportsReadonlyFiles
      {
          get { return false; }
      }

      public bool SupportsCreationUserNames
      {
          get { return true; }
      }

      public bool SupportsModificationUserNames
      {
          get { return true; }
      }

      public bool SupportsLastAccessUserNames
      {
          get { return false; }
      }

      public bool SupportsFileSizes
      {
          get { return true; }
      }

      public bool SupportsDirectorySizes
      {
          get { return false; }
      }

      public bool SupportsAsyncronousAPI
      {
          get { return false; }
      }

    #endregion
  }
}
