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
using System.Text;
using NFX.Serialization.JSON;

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
  /// <summary>
  /// Google Drive file handle
  /// </summary>
  public class GoogleDriveHandle : IFileSystemHandle
  {
    #region Properties

      public string Id { get; set; }
      public string Name { get; set; }
      public bool IsFolder { get; set; }
      public bool IsReadOnly { get; set; }
      public ulong Size { get; set; }
      public DateTime CreatedDate { get; set; }
      public DateTime ModifiedDate { get; set; }

    #endregion

    #region .ctor

      public GoogleDriveHandle(JSONDataMap info)
      {
        Id            = info[Metadata.ID].AsString();
        Name          = info[Metadata.TITLE].AsString();
        IsFolder      = info[Metadata.MIME_TYPE].Equals(GoogleDriveMimeType.FOLDER);
        Size          = info[Metadata.FILE_SIZE].AsULong();
        CreatedDate   = info[Metadata.CREATED_DATE].AsDateTime();
        ModifiedDate  = info[Metadata.MODIFIED_DATE].AsDateTime();
        IsReadOnly    = !info[Metadata.EDITABLE].AsBool();
      }

    #endregion
  }
}
