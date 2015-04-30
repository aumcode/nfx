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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NFX.Instrumentation.Analytics
{
    /// <summary>
    /// Helper class used by TimerHistogram
    /// </summary>
    internal struct TimeHistogramHelper
    {
        #region Public

            /// <summary>
            /// Histogram title used for displaying purposes
            /// </summary>
            public string Title(string prefix, long totalSamples)
            {
                return string.Format(
                    "{0}\n" +
                    "  MinTime: {1:0.000000}\n" +
                    "  MaxTime: {2:0.000000}\n" +
                    "  AvgTime: {3:0.000000}",
                    prefix, m_MinTime, m_MaxTime, AvgTime(totalSamples));
            }

            /// <summary>
            /// Reset internal histogram state
            /// </summary>
            public void Clear()
            {
                m_Start   = 0;
                m_MinTime = 0;
                m_MaxTime = 0;
                m_SumTime = 0;
            }

            /// <summary>
            /// Minimum time of all sample measurements
            /// </summary>
            public double MinTime { get { return m_MinTime; } }

            /// <summary>
            /// Maximum time of all sample measurements
            /// </summary>
            public double MaxTime { get { return m_MaxTime; } }

            public double AvgTime(long totalSamples) { return totalSamples == 0 ? 0 : m_SumTime / totalSamples; }

            /// <summary>
            /// Start sample time measurement
            /// </summary>
            public void Start()
            {
                NFX.Time.WinApi.QueryPerformanceCounter(out m_Start);
            }

            /// <summary>
            /// Stop sample time measurement started by Start() call
            /// </summary>
            public double Stop()
            {
                long stop, freq;
                NFX.Time.WinApi.QueryPerformanceCounter(out stop);
                NFX.Time.WinApi.QueryPerformanceFrequency(out freq);
                double dfreq = freq;
                double elapsed = stop < m_Start ? 0d : (double)(stop - m_Start) / dfreq;

                if (m_MinTime > elapsed || m_MinTime == 0) m_MinTime = elapsed;
                if (m_MaxTime < elapsed) m_MaxTime = elapsed;

                m_SumTime += elapsed;

                return elapsed;
            }

        #endregion

        #region Private

            private long   m_Start;
            private double m_MinTime;
            private double m_MaxTime;
            private double m_SumTime;

        #endregion
    }
}
