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

namespace NFX.RelationalModel.DataTypes
{
    /// <summary>
    /// Represents a domain - a named type
    /// </summary>
    public abstract class Domain : INamed, IConfigurable
    {

        #region Properties

            /// <summary>
            /// Returns the name of this domain, i.e. 'THumanAge', 'TSalary'
            /// </summary>
            public virtual string Name
            {
                get { return this.GetType().Name; }
            }

        #endregion



        public virtual void Configure(IConfigSectionNode node)
        {
            ConfigAttribute.Apply(this, node);
        }
    }
}
