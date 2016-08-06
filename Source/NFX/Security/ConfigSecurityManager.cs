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

using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Security
{

    /// <summary>
    /// Provides security manager implementation that authenticates and authorizes users from configuration
    /// </summary>
    public class ConfigSecurityManager : ApplicationComponent, ISecurityManagerImplementation
    {
        #region CONSTS

           public const string CONFIG_USERS_SECTION = "users";
           public const string CONFIG_USER_SECTION = "user";

           public const string CONFIG_RIGHTS_SECTION = Rights.CONFIG_ROOT_SECTION;
           public const string CONFIG_PERMISSION_SECTION = "permission";

           public const string CONFIG_NAME_ATTR = "name";
           public const string CONFIG_DESCRIPTION_ATTR = "description";
           public const string CONFIG_STATUS_ATTR = "status";
           public const string CONFIG_ID_ATTR = "id";
           public const string CONFIG_PASSWORD_ATTR = "password";


        #endregion

        #region .ctor

             /// <summary>
             /// Constructs security manager that authenticates users listed in application configuration
             /// </summary>
             public ConfigSecurityManager():base()
             {

             }

             /// <summary>
             /// Constructs security manager that authenticates users listed in the supplied configuration section
             /// </summary>
             public ConfigSecurityManager(IConfigSectionNode config):base()
             {
               m_Config = config;
             }




        #endregion

        #region Fields

            private IConfigSectionNode m_Config;

        #endregion

        #region Properties

            public override string ComponentCommonName { get { return "secman"; }}

            /// <summary>
            /// Returns config node that this instance is configured from.
            /// If null is returned then manager performs authentication from application configuration
            /// </summary>
            public IConfigSectionNode Config
            {
              get { return m_Config; }
            }

        #endregion

        #region Public

            public User Authenticate(Credentials credentials)
            {
              var sect = m_Config ?? App.ConfigRoot[CommonApplicationLogic.CONFIG_SECURITY_SECTION];
              if (sect.Exists && credentials is IDPasswordCredentials)
              {
                  var idpass = (IDPasswordCredentials)credentials;

                  var usern = findUserNode(sect, idpass);

                  if (usern.Exists)
                  {
                      var name = usern.AttrByName(CONFIG_NAME_ATTR).ValueAsString(string.Empty);
                      var descr = usern.AttrByName(CONFIG_DESCRIPTION_ATTR).ValueAsString(string.Empty);
                      var status = usern.AttrByName(CONFIG_STATUS_ATTR).ValueAsEnum<UserStatus>(UserStatus.Invalid);

                      var rights = Rights.None;

                      var rightsn = usern[CONFIG_RIGHTS_SECTION];

                      if (rightsn.Exists)
                      {
                        var data = new MemoryConfiguration();
                        data.CreateFromNode(rightsn);
                        rights = new Rights(data);
                      }

                      return new User(credentials,
                                      credToAuthToken(idpass),
                                      status,
                                      name,
                                      descr,
                                      rights);
                  }
              }

              return new User(credentials,
                              new AuthenticationToken(),
                              UserStatus.Invalid,
                              StringConsts.SECURITY_NON_AUTHENTICATED,
                              StringConsts.SECURITY_NON_AUTHENTICATED,
                              Rights.None);
            }


            public User Authenticate(AuthenticationToken token)
            {
                var idpass = authTokenToCred(token);
                return Authenticate(idpass);
            }


            public void Authenticate(User user)
            {
                if (user==null) return;
                var token = user.AuthToken;
                var reuser = Authenticate(token);

                user.___update_status(reuser.Status, reuser.Name, reuser.Description, reuser.Rights);
            }


            public AccessLevel Authorize(User user, Permission permission)
            {
                if (user==null || permission==null)
                 throw new SecurityException(StringConsts.ARGUMENT_ERROR+GetType().Name+".Authorize(user==null|permission==null)");

                var node = user.Rights.Root.NavigateSection(permission.FullPath);

                return new AccessLevel(user, permission, node);
            }


            public void Configure(IConfigSectionNode node)
            {
                m_Config = node;
            }

        #endregion

        #region .pvt

            private IConfigSectionNode findUserNode(IConfigSectionNode securityRootNode, IDPasswordCredentials cred)
            {
                var users = securityRootNode[CONFIG_USERS_SECTION];

                return users.Children.FirstOrDefault( cn => cn.IsSameName(CONFIG_USER_SECTION) &&
                                                     string.Equals(cn.AttrByName(CONFIG_ID_ATTR).Value, cred.ID, StringComparison.InvariantCulture) &&
                                                     string.Equals(cn.AttrByName(CONFIG_PASSWORD_ATTR).Value, cred.Password.ToMD5String(), StringComparison.InvariantCultureIgnoreCase)
                                                    ) ?? users.Configuration.EmptySection;
            }

            private AuthenticationToken credToAuthToken(IDPasswordCredentials cred)
            {
                return new AuthenticationToken(this.GetType().FullName, "{0}\n{1}".Args(cred.ID, cred.Password));
            }

            private IDPasswordCredentials authTokenToCred(AuthenticationToken token)
            {
                if (token.Data==null)
                    return new IDPasswordCredentials(string.Empty, string.Empty);

                var seg = token.Data.ToString().Split('\n');

                if (seg.Length<2)
                    return new IDPasswordCredentials(string.Empty, string.Empty);

                return new IDPasswordCredentials(seg[0], seg[1]);
            }



        #endregion
    }
}
