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
 * Revision: NFX 0.3  2009.10.12
 */
namespace NFX.Security
{

  /// <summary>
  /// User status enumeration -  super-permission levels
  /// </summary>
  public enum UserStatus
  {

        /// <summary>
        /// Invalid user, not authenticated and not authorized
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The lowest level of a user, bound by permissions inside their domain and domain section (such as facility)
        /// </summary>
        User = 1,
        Usr = User,

        /// <summary>
        /// Administrators may run administration console, but always bound by their domain
        /// </summary>
        Administrator = 1000,

        Admin = Administrator,
        Adm = Administrator,

        /// <summary>
        /// Cross domain user, all restrictions are lifted
        /// </summary>
        System = 1000000,
        Sys = System,
  }
}