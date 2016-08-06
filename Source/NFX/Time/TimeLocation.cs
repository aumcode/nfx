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

namespace NFX.Time
{

    /// <summary>
    /// Data about location where time is measured relative to UTC
    /// </summary>
    public sealed class TimeLocation : IConfigurable
    {
       public const string CONFIG_TIMELOCATION_SECTION = "time-location";



       private static TimeLocation s_Parent = new TimeLocation(true);
       private static TimeLocation s_UTC = new TimeLocation(TimeSpan.Zero, "UTC", true);


       /// <summary>
       /// Returns a singleton instance of timelocation which uses parent setting
       /// </summary>
       public static TimeLocation Parent
       {
          get { return s_Parent; }
       }

       /// <summary>
       /// Returns a singleton instance of UTC timelocation (zero offset from UTC)
       /// </summary>
       public static TimeLocation UTC
       {
          get { return s_UTC; }
       }



       /// <summary>
       /// Creates instance that uses entity's parent settings. May use TimeLocation.Parent static instance instead
       /// </summary>
       public TimeLocation(bool immutable = false)
       {
         m_Immutable = immutable;
       }

       public TimeLocation(TimeSpan utcOffset, string description, bool immutable = false)
       {
          m_UTCOffset = utcOffset;
          m_Description = description;
          m_Immutable = immutable;
       }

       private bool m_Immutable;

       [Config("$utc-offset")]
       private TimeSpan?  m_UTCOffset;

       [Config("$description")]
       private string m_Description;


       /// <summary>
       /// Returns UTC offset
       /// </summary>
       public TimeSpan UTCOffset
       {
          get { return m_UTCOffset.Value; }
          set { if (!m_Immutable) m_UTCOffset = value; }
       }

       /// <summary>
       /// Location description
       /// </summary>
       public string Description
       {
         get { return m_Description ?? string.Empty; }
         set { if (!m_Immutable) m_Description = value; }
       }

       /// <summary>
       /// Returns true when entity's parent settings should be used
       /// </summary>
       public bool UseParentSetting
       {
         get { return !m_UTCOffset.HasValue; }
       }

       /// <summary>
       /// Returns true to indicate that the instance can not be mutated with Configure() or property sets
       /// </summary>
       public bool Immutable { get{ return m_Immutable;}}

       public void Configure(IConfigSectionNode node)
       {
          if (!m_Immutable)
           ConfigAttribute.Apply(this, node);
       }

       public override string ToString()
       {
           if (UseParentSetting) return "Parent Time Localization";

           if (m_UTCOffset.Value.TotalSeconds<0)
             return string.Format(" UTC {0} '{1}'", m_UTCOffset.Value, Description);
           else
             return string.Format(" UTC +{0} '{1}'", m_UTCOffset.Value, Description);
       }
    }
}
