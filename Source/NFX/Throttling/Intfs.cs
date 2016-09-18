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
using NFX.Environment;

using NFX.ApplicationModel;

namespace NFX.Throttling
{
    public interface IThrottle : IApplicationComponent
    {
        /// <summary>
        /// Returns the unique name of this instance
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns the name of unit of measurement for this instance
        /// </summary>
        string Unit { get; }

        /// <summary>
        /// Checks whether the current throttle rate limit has not been reached
        /// per one unit of measurement.  This method is usually used to throttle based
        /// on pass count
        /// </summary>
        bool Try();

        /// <summary>
        /// Checks whether the current throttle rate limit has not been reached
        /// for the provided value.  This method is usually used to throttle based
        /// on values like financial amounts, data volumes, user counts, etc.
        /// </summary>
        bool Try(double value);

        /// <summary>
        /// Reset the internal state of the throttle
        /// </summary>
        void Reset();

        /// <summary>
        /// Register this instance with throttling container
        /// </summary>
        void Register(IThrottling throttling);

        /// <summary>
        /// Unregister this instance from throttling container
        /// </summary>
        bool Unregister();
    }



    /// <summary>
    /// Defines throttling interface to be implemented by the throttling service
    /// </summary>
    public interface IThrottling : IApplicationComponent
    {
        /// <summary>
        /// Turns throttling functionality on/off
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Get a throttle by name
        /// </summary>
        IThrottle this[string name] { get; }

        /// <summary>
        /// Register a throttle with this throttling container
        /// </summary>
        void RegisterThrottle(Throttle throttle);

        /// <summary>
        /// Unregister a throttle from this throttling container
        /// </summary>
        /// <returns>True if throttle was successfully unregistered</returns>
        bool UnregisterThrottle(Throttle throttle);
    }

    /// <summary>
    /// Interface implementing configurable and disposable throttling functionaliry
    /// </summary>
    public interface IThrottlingImplementation : IThrottling, IDisposable, IConfigurable
    {
    }
}
