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

using NFX.Glue;
using NFX.Security;

namespace NFX.Instrumentation.Telemetry
{
    /// <summary>
    /// Represents a contract for working with remote receiver of telemetry information
    /// </summary>
    [Glued]
    [LifeCycle(Mode = ServerInstanceMode.Singleton)]
    public interface ITelemetryReceiver
    {
        /// <summary>
        /// Sends data to remote telemetry receiver
        /// </summary>
        /// <param name="siteName">the name/identifier of the reporting site</param>
        /// <param name="data">Telemetry data</param>
        [OneWay] void Send(string siteName, Datum data);
    }
}
