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

namespace NFX.ApplicationModel
{
    /// <summary>
    /// Represents an entity that performs work on application start.
    /// This entity must be either invoked directly or declared in config file under "starters" section
    /// </summary>
    public interface IApplicationStarter : IConfigurable, INamed
    {
        /// <summary>
        /// Indicates whether an exception that leaks from starter method invocation should break the application start,
        ///  or just get logged
        /// </summary>
        bool ApplicationStartBreakOnException { get; }

        void ApplicationStartBeforeInit(IApplication application);

        void ApplicationStartAfterInit(IApplication application);
    }


    /// <summary>
    /// Represents an entity that can get notified about application finish
    /// </summary>
    public interface IApplicationFinishNotifiable : INamed
    {
        void ApplicationFinishBeforeCleanup(IApplication application);
        void ApplicationFinishAfterCleanup(IApplication application);
    }

}
