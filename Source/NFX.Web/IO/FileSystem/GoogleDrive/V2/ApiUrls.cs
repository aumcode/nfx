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

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
  /// <summary>
  /// Google Drive REST API URLs
  /// </summary>
  static class ApiUrls
  {
    #region Public

      public static Uri GoogleApi()
      {
        return new Uri("https://www.googleapis.com");
      }

      public static Uri Token()
      {
        return new Uri("https://accounts.google.com/o/oauth2/token");
      }

      public static Uri Drive()
      {
        return "/drive/v2".FormatUri();
      }

      public static Uri Files()
      {
        return "/drive/v2/files".FormatUri();
      }

      public static Uri Upload()
      {
        return "upload/drive/v2/files?uploadType=resumable".FormatUri();
      }

      public static Uri Download(string id)
      {
        return "/drive/v2/files/{0}?alt=media".FormatUri(id);
      }

      public static Uri Update(string id)
      {
        return "upload/drive/v2/files/{0}".FormatUri(id);
      }

      public static Uri Files(string parentId)
      {
        return "/drive/v2/files?q=trashed = false and '{0}' in parents and mimeType != 'application/vnd.google-apps.folder'".FormatUri(parentId);
      }

      public static Uri Subfolders(string parentId)
      {
        return "/drive/v2/files?q=trashed = false and '{0}' in parents and mimeType = 'application/vnd.google-apps.folder'".FormatUri(parentId);
      }

      public static Uri FileById(string id)
      {
        return "/drive/v2/files/{0}".FormatUri(id);
      }

      public static Uri FileByName(string parentId, string name)
      {
        return "/drive/v2/files?q=trashed = false and title='{0}' and '{1}' in parents".FormatUri(name, parentId);
      }

      public static Uri SetModifiedDate(string id)
      {
        return "/drive/v2/files/{0}?setModifiedDate=true".FormatUri(id);
      }

      public static Uri Rename(string id)
      {
        return FileById(id);
      }

    #endregion
  }
}
