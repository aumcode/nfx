
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
 * Revision: NFX 1.0  2/16/2014 3:12:42 PM
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

namespace NFX.IO.FileSystem.S3.V4
{
  public class S3V4FileSystemCapabilities : IFileSystemCapabilities
  {
    #region Static /.ctor

      private static readonly char[] PATH_SEPARATORS = new char[]{'/'};

      private static S3V4FileSystemCapabilities s_Instance = new S3V4FileSystemCapabilities();

      public static S3V4FileSystemCapabilities Instance { get { return s_Instance;} }

    #endregion

    #region ctor

      private S3V4FileSystemCapabilities() {}

    #endregion

    #region IFileSystemCapabilities

      public bool SupportsVersioning
      {
        get { return false; }
      }

      public bool SupportsTransactions
      {
        get { return false; }
      }

      public int MaxFilePathLength
      {
        get { return 2048; }
      }

      public int MaxFileNameLength
      {
        get { return 1024; }
      }

      public int MaxDirectoryNameLength
      {
        get { return 256; }
      }

      public ulong MaxFileSize
      {
        get { return 104857600; }
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
        get { return true; }
      }

      public bool SupportsCustomMetadata
      {
        get { return true; }
      }

      public bool SupportsDirectoryRenaming
      {
        get { return false; }
      }

      public bool SupportsFileRenaming
      {
        get { return false; }
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
        get { return false; }
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
  } //S3FileSystemCapabilities

}
