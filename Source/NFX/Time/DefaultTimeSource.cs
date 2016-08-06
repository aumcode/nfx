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

namespace NFX.Time
{
    /// <summary>
    /// Provides default time source implementation which is build on DateTime local class
    /// </summary>
    public class DefaultTimeSource : ApplicationComponent, ITimeSourceImplementation
    {
        private static DefaultTimeSource s_Instance = new DefaultTimeSource();


        /// <summary>
        /// Returns a singleton DefaultTimeSource instance
        /// </summary>
        public static DefaultTimeSource Instance
        {
          get { return s_Instance; }
        }

        private DefaultTimeSource() : base()
        {
        }


        public TimeLocation TimeLocation
        {
          get { return new TimeLocation(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now), "Local Computer", true); }
        }


        public DateTime Now { get { return DateTime.Now; } }

        /// <summary>
        /// Returns current time localized per TimeLocation
        /// </summary>
        public DateTime LocalizedTime { get { return Now;} }

        public DateTime UTCNow { get { return DateTime.UtcNow; } }


        /// <summary>
        /// Converts universal time to local time as of TimeLocation property
        /// </summary>
        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            if (utc.Kind!=DateTimeKind.Utc)
              throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

            var loc = TimeLocation;
            if (!loc.UseParentSetting)
            {
                return DateTime.SpecifyKind(utc + loc.UTCOffset, DateTimeKind.Local);
            }
            else
            {
                return utc.ToLocalTime();
            }
        }

        /// <summary>
        /// Converts localized time to UTC time as of TimeLocation property
        /// </summary>
        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            if (local.Kind!=DateTimeKind.Local)
              throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

            var loc = TimeLocation;
            if (!loc.UseParentSetting)
            {
                return DateTime.SpecifyKind(local - loc.UTCOffset, DateTimeKind.Utc);
            }
            else
            {
                return local.ToUniversalTime();
            }
        }



        public void Configure(Environment.IConfigSectionNode node)
        {

        }

    }
}
