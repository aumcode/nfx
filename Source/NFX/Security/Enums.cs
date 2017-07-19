/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System;

namespace NFX.Security
{
  /// <summary>
  /// Denotes types of identities: Users, Groups etc.
  /// </summary>
  public enum IdentityType
  {
    Other= 0,
    /// <summary>Identity of particular system User</summary>
    User,
    /// <summary>Identity of group of users</summary>
    Group,
    /// <summary>Identity of system component such as Process</summary>
    Process,
    /// <summary>Identity of business entity such as vendor, bank etc.</summary>
    Business
  }

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


  /// <summary>
  /// Defines what actions should be logged by the system
  /// </summary>
  [Flags]
  public enum SecurityLogMask
  {
    Off = 0,

    Custom               = 1 << 0,
    Authentication       = 1 << 1,

    Authorization        = 1 << 2,

    Gate                 = 1 << 3,
    Login                = 1 << 4,
    Logout               = 1 << 5,
    LoginChange          = 1 << 6,

    UserCreate           = 1 << 7,
    UserDestroy          = 1 << 8,
    UserSuspend          = 1 << 9,
    UserResume           = 1 << 10,

    All = -1
  }

  /// <summary>
  /// Denotes security actions
  /// </summary>
  public enum SecurityLogAction
  {
    Custom = 0,
    Authentication,
    Authorization,
    Gate,
    Login,
    Logout,
    LoginChange,
    UserCreate,
    UserDestroy,
    UserSuspend,
    UserResume
  }

  public static class EnumUtils
  {
    public static SecurityLogMask ToMask(this SecurityLogAction action)
    {
      switch (action)
      {
        case SecurityLogAction.Custom:         return SecurityLogMask.Custom;
        case SecurityLogAction.Authentication: return SecurityLogMask.Authentication;
        case SecurityLogAction.Authorization:  return SecurityLogMask.Authorization;
        case SecurityLogAction.Gate:           return SecurityLogMask.Gate;
        case SecurityLogAction.Login:          return SecurityLogMask.Login;
        case SecurityLogAction.Logout:         return SecurityLogMask.Logout;
        case SecurityLogAction.LoginChange:    return SecurityLogMask.LoginChange;
        case SecurityLogAction.UserCreate:     return SecurityLogMask.UserCreate;
        case SecurityLogAction.UserDestroy:    return SecurityLogMask.UserDestroy;
        case SecurityLogAction.UserSuspend:    return SecurityLogMask.UserSuspend;
        case SecurityLogAction.UserResume:     return SecurityLogMask.UserResume;
        default: return SecurityLogMask.Off;
      }
    }
  }
}