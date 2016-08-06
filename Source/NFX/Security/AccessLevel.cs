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
 * Revision: NFX 1.3  2013.07.01
 */
using System;
using System.Collections.Generic;
using System.Text;
using NFX.Environment;

namespace NFX.Security
{
      /// <summary>
      /// A level of access granted to user for certain permission, i.e. if (level.Denied).....
      /// </summary>
      public sealed class AccessLevel
      {
        #region CONSTS

            public const int DENIED = 0;
            public const int VIEW = 1;
            public const int VIEW_CHANGE = 2;
            public const int VIEW_CHANGE_DELETE = 3;

            public const string CONFIG_LEVEL_ATTR = "level";

            public static readonly IConfigSectionNode DENIED_CONF = "p{level=0}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

        #endregion

        #region .ctor

            public static AccessLevel DeniedFor(User user, Permission permission)
            {
              return new AccessLevel(user, permission, DENIED_CONF);
            }

            public AccessLevel(User user, Permission permission, IConfigSectionNode data)
            {
                m_User = user;
                m_Permission = permission;
                m_Data = data;
            }

        #endregion


        #region Fields

            private User m_User;
            private Permission m_Permission;
            private IConfigSectionNode m_Data;


        #endregion


        #region Properties

            /// <summary>
            /// Returns user that this access level is for
            /// </summary>
            public User User
            {
               get { return m_User;}
            }


            /// <summary>
            /// Returns permission that this access level is for
            /// </summary>
            public Permission Permission
            {
               get { return m_Permission;}
            }

            /// <summary>
            /// Returns security data for this level
            /// </summary>
            public IConfigSectionNode Data
            {
               get { return m_Data ?? Rights.None.Root;}
            }


            /// <summary>
            /// Returns security level attribute from Data
            /// </summary>
            public int Level
            {
               get { return m_Data.AttrByName(CONFIG_LEVEL_ATTR).ValueAsInt(DENIED);}
            }

            /// <summary>
            /// Indicates whether access is denied
            /// </summary>
            public bool Denied
            {
              get { return Level == DENIED; }
            }

        #endregion

  }//aceess level

}
