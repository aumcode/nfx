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
using NFX.Environment;
using NFX.IO.FileSystem;

namespace NFX.IO.FileSystem.GoogleDrive
{
  public class GoogleDriveParameters : FileSystemSessionConnectParams
  {
    #region CONST

      public const string CONFIG_EMAIL_ATTR = "email";
      public const string CONFIG_CERT_PATH_ATTR = "cert-path";

    #endregion

    #region .ctor

      public GoogleDriveParameters(): base() {}
      public GoogleDriveParameters(IConfigSectionNode node): base(node) {}
      public GoogleDriveParameters(string connectStr, string format = Configuration.CONFIG_LACONIC_FORMAT) : base(connectStr, format) { }

    #endregion

    #region Properties

      [Config]
      public string CertPath { get; set; }

      [Config]
      public int TimeoutMs { get; set; }

      [Config]
      public int Attempts { get; set; }

    #endregion

    #region Public

      public override void Configure(IConfigSectionNode node)
      {
        base.Configure(node);

        var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;

        var credentials = new GoogleDriveCredentials(email);

        var authToken = new AuthenticationToken();

        User = new User(credentials, authToken, UserStatus.User, name:null, descr:null, rights:Rights.None);
      }

    #endregion
  }
}
