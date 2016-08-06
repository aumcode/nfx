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
namespace NFX.IO.FileSystem.GoogleDrive.V2
{
  /// <summary>
  /// Google Drive metadata fields
  /// </summary>
  static class Metadata
  {
    public static readonly string ID            = "id";
    public static readonly string TITLE         = "title";
    public static readonly string ITEMS         = "items";
    public static readonly string PARENTS       = "parents";
    public static readonly string EDITABLE      = "editable";
    public static readonly string MIME_TYPE     = "mimeType";
    public static readonly string FILE_SIZE     = "fileSize";
    public static readonly string CREATED_DATE  = "createdDate";
    public static readonly string MODIFIED_DATE = "modifiedDate";

    public static readonly string ASSERTION     = "assertion";
    public static readonly string GRANT_TYPE    = "grant_type";
    public static readonly string ACCESS_TOKEN  = "access_token";
  }
}
