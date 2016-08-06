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
// Created: 2013-05-03
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFX.Environment;

namespace NFX.Throttling
{
    /// <summary>
    /// Throttle based on space reservation in time. It allows at most Limit
    /// calls of Try() function per Interval. The calls are assumed to be
    /// equally spaced within the Interval
    /// </summary>
    public class TimeSpacingThrottle : Throttle
    {
        #region .ctor

            public TimeSpacingThrottle() : base()
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name">Name of this instance</param>
            /// <param name="throttleLimit">Throttling limit per throttleInterval</param>
            /// <param name="throttleInterval">Throttling interval in number of seconds</param>
            /// <param name="unit">Unit of measurement</param>
            public TimeSpacingThrottle(string name, int throttleLimit,
                int throttleInterval = 1, string unit = null)
                : base(name, unit)
            {
                Limit = throttleLimit;
                Interval = throttleInterval;
                Reset();
            }

            protected override void Destructor()
            {
                base.Destructor();
            }

        #endregion

        #region Fields

            private int    m_Interval;
            private double m_Limit;

            private DateTime m_NextTime;
            private double m_MinTimeSlice;

        #endregion

        #region Props

            /// <summary>
            /// Return throttle limit over Interval
            /// </summary>
            [Config("$limit")]
            public double Limit
            {
                get { return m_Limit; }
                internal set
                {
                    if (value < 0)
                        throw new NFXException(StringConsts.ARGUMENT_ERROR + "Limit = " + value);
                    m_Limit = value;
                }
            }

            /// <summary>
            /// Return throttling interval in seconds
            /// </summary>
            [Config("$interval")]
            public int Interval { get { return m_Interval; } internal set { m_Interval = value; } }

        #endregion

        #region Public

            /// <summary>
            /// Reset the internal state of the throttling strategy
            /// </summary>
            public override void Reset()
            {
                m_MinTimeSlice  = m_Limit == 0.0 ? 0 : (double)m_Interval / m_Limit;
                m_NextTime      = new DateTime(Convert.ToInt64(m_MinTimeSlice));
            }

        #endregion

        #region Protected

            /// <summary>
            /// Try to reserve a value of units assuming given time.
            /// </summary>
            /// <param name="time">Monotonically increasing time value</param>
            /// <param name="value">The value of timeslots needed to reserve</param>
            /// <returns>True if reservation is successful</returns>
            internal override bool Try(DateTime time, double value)
            {
                if (time < m_NextTime)
                    return false;
                m_NextTime = time.AddSeconds(m_MinTimeSlice * value);
                return true;
            }

            protected override void DoConfigure(IConfigSectionNode node)
            {
                base.DoConfigure(node);
                Reset();
            }

        #endregion
    }
}
