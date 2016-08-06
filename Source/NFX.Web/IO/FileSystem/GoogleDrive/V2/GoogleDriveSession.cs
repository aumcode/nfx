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

using NFX.Security;
using NFX.IO.FileSystem;

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
    public class GoogleDriveSession : FileSystemSession
    {
      #region Properties

        public GoogleDriveClient Client { get; private set; }

      #endregion

      #region .ctor

        protected internal GoogleDriveSession(GoogleDriveFileSystem fs, IFileSystemHandle handle, GoogleDriveParameters cParams)
          : base(fs, handle, cParams)

        {
          var email  = (m_User.Credentials as GoogleDriveCredentials).Email;
          Client = new GoogleDriveClient(email, cParams.CertPath, cParams.TimeoutMs, cParams.Attempts);
        }

      #endregion
    }
}
