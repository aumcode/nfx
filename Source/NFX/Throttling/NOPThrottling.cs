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

namespace NFX.Throttling
{
    /// <summary>
    /// Represents throttling implementation that does nothing and
    /// always returns NOPThrottle
    /// </summary>
    public class NOPThrottling : ApplicationComponent, IThrottlingImplementation
    {
        private NOPThrottling() : base()
        {
        }

        public static NOPThrottling Instance
        {
            get { return s_Instance; }
        }

        private static NOPThrottling s_Instance = new NOPThrottling();

        #region IThrottling Members

            public bool Enabled
            {
                get { return false; }
            }

            public IThrottle this[string name]
            {
                get { return NOPThrottle.Instance; }
            }

            public void RegisterThrottle(Throttle throttle)
            {
            }

            public bool UnregisterThrottle(Throttle throttle)
            {
                return false;
            }

        #endregion

        #region IConfigurable Members

            public void Configure(Environment.IConfigSectionNode node)
            {
            }

        #endregion
    }
}
