/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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

using NFX.ApplicationModel;

namespace NFX.Security
{
    /// <summary>
    /// Provides security manager implementation that does nothing and always returns fake user instance
    /// </summary>
    public sealed class NOPSecurityManager : ApplicationComponent, ISecurityManagerImplementation
    {
         private NOPSecurityManager():base() {}

         private static NOPSecurityManager s_Instance = new NOPSecurityManager();

         public static NOPSecurityManager Instance
         {
            get { return s_Instance;}
         }

         public User Authenticate(Credentials credentials)
         {
             return User.Fake;
         }

         public void Configure(Environment.IConfigSectionNode node)
         {

         }


         public User Authenticate(AuthenticationToken token)
         {
             return User.Fake;
         }

         public void Authenticate(User user)
         {
         }

         public AccessLevel Authorize(User user, Permission permission)
         {
             return new AccessLevel(user, permission, Rights.None.Root);
         }
    }
}
