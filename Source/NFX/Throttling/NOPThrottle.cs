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


// Author: Serge Aleynikov
// Created: 2013-05-02
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Throttling
{
    /// <summary>
    /// No operation throttle
    /// </summary>
    public sealed class NOPThrottle : Throttle
    {
        #region .ctor

            private NOPThrottle() {}

        #endregion

        #region Private

            private static NOPThrottle s_Instance = new NOPThrottle();

        #endregion

        #region Public

            /// <summary>
            /// Returns a singlelton instance of the NOPThrottle
            /// </summary>
            public static NOPThrottle Instance { get { return s_Instance; } }

            /// <summary>
            /// Checks whether the current throttle rate limit has not been reached
            /// per one unit of measurement.  This method is usually used to throttle based
            /// on pass count
            /// </summary>
            public override bool Try() { return true; }

            /// <summary>
            /// Checks whether the current throttle rate limit has not been reached
            /// for the provided value.  This method is usually used to throttle based
            /// on values like financial amounts, data volumes, user counts, etc.
            /// </summary>
            public override bool Try(double value) { return true; }

            /// <summary>
            /// Reset the internal state of the throttle
            /// </summary>
            public override void Reset() {}


        #endregion

        #region Protected

            /// <summary>
            /// Update current throttle timestamp
            /// </summary>
            internal override bool Try(DateTime time, double value) { return true; }

        #endregion
    }
}
