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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NFX.Instrumentation.Analytics
{
    /// <summary>
    /// Time measurement 1D histogram aggregates measurements in fractions of a second
    /// </summary>
    public class TimeHistogram : ITimeHistogram
    {
        private class timeFrame : IDisposable
        {
            public timeFrame(TimeHistogram hist)
            {
                m_Hist = hist;
                m_Hist.Start();
            }

            public void Dispose()
            {
                m_Hist.Stop();
            }

            TimeHistogram m_Hist;
        }

        public TimeHistogram(
            string title,
            string dim1Name,
            PartitionFunc<double> dim1PartitionFunc = null,
            PartitionNameFunc dim1PartitionNameFunc = null)
        {
            m_Hist = new Histogram<double>(title,
                new TimeDimension(
                    dim1Name, TimeDimension.DEFAULT_PART_COUNT,
                    dim1PartitionFunc, dim1PartitionNameFunc));
        }

        public TimeHistogram(
            string title,
            string dim1Name,
            int    dim1PartCount,
            PartitionFunc<double>   dim1PartitionFunc = null,
            PartitionNameFunc       dim1PartitionNameFunc = null)
        {
            m_Hist = new Histogram<double>(title,
                new TimeDimension(
                    dim1Name, dim1PartCount, dim1PartitionFunc, dim1PartitionNameFunc));
        }

        #region Public/Props

            /// <summary>
            /// Returns a disposable instance suitable for time measurement in a "using" block
            /// </summary>
            public IDisposable TimeFrame { get { return new timeFrame(this as TimeHistogram); } }

            /// <summary>
            /// Start sample time measurement
            /// </summary>
            public void Start() { m_Timer.Start(); }

            /// <summary>
            /// Stop sample time measurement started by Start() call
            /// </summary>
            public void Stop()
            {
                double elapsed = m_Timer.Stop();
                m_Hist.DoSample(m_Hist.Keys(elapsed));
            }

            /// <summary>
            /// Records time measurement in seconds
            /// </summary>
            public void Sample(double secInterval)
            {
                m_Hist.Sample(secInterval);
            }

            /// <summary>
            /// Reset internal histogram state
            /// </summary>
            public void Clear()
            {
                m_Hist.Clear();
                m_Timer.Clear();
            }

            /// <summary>
            /// Histogram title used for displaying purposes
            /// </summary>
            public string Title
            {
                get
                {
                    return m_Timer.Title(m_Hist.Title, m_Hist.TotalSamples);
                }
            }

            /// <summary>
            /// Minimum time of all sample measurements
            /// </summary>
            public double MinTime { get { return m_Timer.MinTime; } }

            /// <summary>
            /// Maximum time of all sample measurements
            /// </summary>
            public double MaxTime { get { return m_Timer.MaxTime; } }

            /// <summary>
            /// Average time of all sample measurements
            /// </summary>
            public double AvgTime { get { return m_Timer.AvgTime(m_Hist.TotalSamples); } }

            /// <summary>
            /// Total number of samples in the histogram
            /// </summary>
            public long TotalSamples { get { return m_Hist.TotalSamples; } }

            /// <summary>
            /// Returns the dimension instances of this histogram
            /// </summary>
            public IEnumerable<Dimension> Dimensions { get { return m_Hist.Dimensions; } }

            /// <summary>
            /// Number of dimensions in this histogram
            /// </summary>
            public int DimensionCount { get { return m_Hist.DimensionCount; } }

            /// <summary>
            /// Return a dimension of a histogram identified by index
            /// </summary>
            public Dimension GetDimention(int dimension) { return m_Hist.GetDimention(dimension); }

            /// <summary>
            /// Returns the name of a partition in a given histogram dimension
            /// </summary>
            /// <param name="dimension">Dimension index</param>
            /// <param name="partition">Partition index</param>
            public string GetPartitionName(int dimension, int partition)
            {
                return m_Hist.GetPartitionName(dimension, partition);
            }

            /// <summary>
            /// Return the count of samples for the given histogram keys
            /// </summary>
            public int this[HistogramKeys keys] { get { return m_Hist[keys]; } }

            /// <summary>
            /// Try to get the count of samples for the given histogram keys
            /// </summary>
            public bool TryGet(HistogramKeys keys, out int count)
            {
                return m_Hist.TryGet(keys, out count);
            }

            public IEnumerator<HistogramEntry> GetEnumerator()
            {
                return m_Hist.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_Hist.GetEnumerator();
            }

        #endregion

        #region Private

            private Histogram<double>   m_Hist;
            private TimeHistogramHelper m_Timer;

        #endregion
    }
}
