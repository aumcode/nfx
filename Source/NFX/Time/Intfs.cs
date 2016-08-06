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
using NFX.ApplicationModel;

namespace NFX.Time
{

    /// <summary>
    /// Describes an entity that provides the localized time
    /// </summary>
    public interface ILocalizedTimeProvider
    {
        /// <summary>
        /// Returns the location
        /// </summary>
        TimeLocation TimeLocation { get; }


        /// <summary>
        /// Returns current time localized per TimeLocation
        /// </summary>
        DateTime LocalizedTime { get; }

        /// <summary>
        /// Converts universal time to local time as of TimeLocation property
        /// </summary>
        DateTime UniversalTimeToLocalizedTime(DateTime utc);

        /// <summary>
        /// Converts localized time to UTC time as of TimeLocation property
        /// </summary>
        DateTime LocalizedTimeToUniversalTime(DateTime local);
    }


    /// <summary>
    /// Denotes app-global time source - an entity that supplies time in this application instance
    /// </summary>
    public interface ITimeSource : IApplicationComponent, ILocalizedTimeProvider
    {
        /// <summary>
        /// Returns local time stamp, Alias to this.LocalizedTime
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Returns current UTC time stamp
        /// </summary>
        DateTime UTCNow { get; }
    }

    /// <summary>
    /// Denotes an implementation for an app-global time source - an entity that supplies time in this application instance
    /// </summary>
    public interface ITimeSourceImplementation : ITimeSource, IDisposable, IConfigurable
    {

    }



    /// <summary>
    /// Denotes a contract for an app-global event timer - an entity that fires requested events
    /// </summary>
    public interface IEventTimer : IApplicationComponent, NFX.Instrumentation.IInstrumentable
    {
       /// <summary>
       /// Gets the granularity of event firing resolution
       /// </summary>
       int ResolutionMs { get; }

       /// <summary>
       /// Lists all events in the instance
       /// </summary>
       IRegistry<Event> Events { get;}
    }


    /// <summary>
    /// Denotes an implementation for an app-global event timer - an entity that fires requested events
    /// </summary>
    public interface IEventTimerImplementation : IEventTimer, IDisposable, IConfigurable
    {
       new int ResolutionMs { get; set; }

       /// <summary>
       /// Internall call, developers - do not call
       /// </summary>
       void __InternalRegisterEvent(Event evt);

       /// <summary>
       /// Internall call, developers - do not call
       /// </summary>
       void __InternalUnRegisterEvent(Event evt);
    }



}
