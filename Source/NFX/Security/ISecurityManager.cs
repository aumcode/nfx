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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Security
{
    /// <summary>
    /// Represents an entity that performs user authentication based on passed credentials and other security-related global tasks
    /// </summary>
    public interface ISecurityManager : IApplicationComponent
    {

       /// <summary>
       /// Authenticates user by checking the supplied credentials against the
       /// authentication store that this manager represents.
       /// If credential are invalid then UserKind.Invalid is returned.
       /// This method may populate user access rights completely or partially - depending on store implementation.
       /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
       ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
       /// </summary>
       /// <param name="credentials">User credentials.
       /// Particular manager implementation may elect to support multiple credential types, i.e.
       /// IdPassword, Twitter, Facebook, OAuth, LegacySystemA/B/C etc.
       /// </param>
       /// <returns>
       /// User object. Check User.Status for UserStatus.Invalid flag to see if authentication succeeded
       /// </returns>
       User Authenticate(Credentials credentials);


       /// <summary>
       /// Authenticates user by checking the supplied token against the
       /// authentication store that this manager represents.
       /// If token is invalid then UserKind.Invalid is returned.
       /// This method may populate user access rights completely or partially - depending on store implementation.
       /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
       ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
       /// </summary>
       /// <param name="token">User authentication token </param>
       /// <returns>
       /// User object. Check User.Status for UserStatus.Invalid flag to see if authentication succeeded
       /// </returns>
       User Authenticate(AuthenticationToken token);


       /// <summary>
       /// Authenticates user by checking the supplied user's token against the
       /// authentication store that this manager represents.
       /// This method is called by the framework after User object was deserialized and it's Rights need to be re-fetched.
       /// If token is invalid then UserStatus.Invalid is set.
       /// This method may populate user access rights completely or partially - depending on store implementation.
       /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
       ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
       /// </summary>
       /// <param name="user">User object which is checked and updated</param>
       void Authenticate(User user);



       /// <summary>
       /// Authorizes user by finding appropriate access level to permission by supplied path.
       /// Depending on particular implementation, rights may be fully or partially cached in memory.
       /// Note: this authorization call returns AccessLevel object that may contain a complex data structure.
       /// The final assertion of user's ability to perform a certain action is encapsulated in Permission.Check() method.
       /// Call Permission.AuthorizeAndGuardAction(MemberInfo, ISession) to guard classes and methods from unauthorized access
       /// </summary>
       /// <param name="user">A user to perform authorization for</param>
       /// <param name="permission">An instance of permission to get</param>
       /// <returns>AccessLevel granted to specified permission</returns>
       AccessLevel Authorize(User user, Permission permission);
    }


    /// <summary>
    /// Represents an implementation of an entity that performs user authentication based on passed credentials and other security-related global tasks
    /// </summary>
    public interface ISecurityManagerImplementation : ISecurityManager, IDisposable, IConfigurable
    {

    }
}
