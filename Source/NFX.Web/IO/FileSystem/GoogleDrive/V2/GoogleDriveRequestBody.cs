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
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
  /// <summary>
  /// Represents a request to Google Drive REST API
  /// </summary>
  class GoogleDriveRequestBody : Dictionary<string, object>
  {
    #region CONST

      private const string RFC3339 = "yyyy-MM-dd'T'HH:mm:ss.fffffffK";

    #endregion

    #region Public

      public void SetTitle(string title)
      {
        Set(Metadata.TITLE, title);
      }

      public void SetParent(string parentId)
      {
        var parent = new Dictionary<string, string>();
        parent[Metadata.ID] = parentId;

        var parents = new Dictionary<string, string>[] { parent };

        Set(Metadata.PARENTS, parents);
      }

      public void SetModifiedDate(DateTime date)
      {
        Set(Metadata.MODIFIED_DATE, date.ToString(RFC3339));
      }

      public void SetMimeType(string mimeType)
      {
        Set(Metadata.MIME_TYPE, mimeType);
      }

    #endregion

    #region Private

      private void Set(string key, object value)
      {
        this[key] = value;
      }

    #endregion
  }
}
