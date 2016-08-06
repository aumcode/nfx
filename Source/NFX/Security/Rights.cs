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
using System;
using System.Collections.Generic;
using System.Text;

using NFX.Environment;

namespace NFX.Security
{
      /// <summary>
      /// User rights contains data about access levels to permissions in the system.
      /// Use Configuration internally to keep the data organized in hierarchical navigable structure.
      /// Configuration also allows to cross-link permission levels using vars and make acess level
      ///  dependent on settings on a particular machine using environmental vars
      /// </summary>
      public sealed class Rights
      {
        public const string CONFIG_ROOT_SECTION = "rights";

        private static Rights m_NoneInstance = new Rights( Configuration.NewEmptyRoot(CONFIG_ROOT_SECTION).Configuration );

        /// <summary>
        /// An instance that signifies an absence of any rights at all - complete access denied
        /// </summary>
        public static Rights None
        {
          get
          {
            return m_NoneInstance;
          }
        }

        public Rights(Configuration data)
        {
          m_Data = data;
        }

        private Configuration m_Data;

        public IConfigSectionNode Root
        {
          get { return m_Data.Root; }
        }
  }
}
