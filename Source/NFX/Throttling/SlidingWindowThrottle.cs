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
// Source code ported from:
// https://github.com/saleyn/utxx/blob/master/include/utxx/rate_throttler.hpp

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;

namespace NFX.Throttling
{
    /// <summary>
    /// Efficiently calculates the throttling rate over a number of seconds.
    /// The algorithm implements a variation of token bucket algorithm that
    /// doesn't require to add tokens to the bucket on a timer but rather it
    /// maintains a cirtular buffer of tokens with resolution of 1/BucketsPerSec.
    /// The Add() function is used to add items to a bucket
    /// associated with the timestamp passed as the first argument to the
    /// function. The Sum() returns the total number of
    /// items over the given interval of seconds. The items automatically expire
    /// when the time moves on the successive invocations of the Add() method.
    /// </summary>
    public class SlidingWindowThrottle : Throttle
    {
        #region .ctor

            public SlidingWindowThrottle() : base()
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name">Name of this instance</param>
            /// <param name="throttleLimit">Throttling limit per throttleInterval</param>
            /// <param name="throttleInterval">Throttling interval in number of seconds</param>
            /// <param name="bucketsPerSecond">Number of buckets per second to calculate internal running sum.
            /// The greater the value the more precise the avg calculation is and the longer it takes to
            /// calculate</param>
            /// <param name="unit">Unit of measurement</param>
            public SlidingWindowThrottle(string name, int throttleLimit, int throttleInterval,
                int bucketsPerSecond = 5, string unit = null)
                : base(name, unit)
            {
                m_Interval      = throttleInterval;
                m_Limit         = throttleLimit;
                m_BucketsPerSec = bucketsPerSecond;

                Reset();
            }

            protected override void Destructor()
            {
                base.Destructor();
            }

        #endregion

        #region Fields

            private int         m_Interval;
            private double      m_Limit;
            private int         m_BucketsPerSec;

            private double[]    m_Buckets;
            private int         m_LastTimeBucket;
            private double      m_Sum;

            private int         m_BucketsInterval;
            private int         m_LogBucketsSec;
            private int         m_BucketCount;
            private int         m_BucketMask;

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

            /// <summary>
            /// Number of measurement buckets per second. The greater the number
            /// the more accurate throttle rate is calculated and the slower the
            /// calculation gets
            /// </summary>
            [Config("$buckets-per-sec", 8)]
            public int BucketsPerSec
            {
                get { return m_BucketsPerSec; }
                internal set { m_BucketsPerSec = (int)IntMath.UpperPow(value, 2); }
            }

            /// <summary>
            /// Return current running sum over the throttling interval
            /// </summary>
            public double Sum { get { return m_Sum; } }

            /// <summary>
            /// Return current running average over the throttling interval
            /// </summary>
            public double Avg { get { return m_Sum / m_Interval; } }

        #endregion

        #region Public

            /// <summary>
            /// Reset the internal state of the throttling strategy
            /// </summary>
            public override void Reset()
            {
                int maxSeconds = (int)IntMath.UpperPow(m_BucketsPerSec * m_Interval, 2);
                m_LogBucketsSec = IntMath.Log(m_BucketsPerSec, 2);
                m_BucketCount = maxSeconds * m_BucketsPerSec;
                m_BucketMask = m_BucketCount - 1;
                m_BucketsInterval = m_Interval << m_LogBucketsSec;

                // must be power of two
                Debug.Assert((m_BucketCount & (m_BucketCount - 1)) == 0);

                m_Buckets = new double[m_BucketCount];

                Array.Clear(m_Buckets, 0, m_Buckets.Length);
                m_LastTimeBucket = 0;
                m_Sum = 0;
            }

            /// <summary>
            /// Dump the internal state to string
            /// </summary>
            /// <param name="time">Time for which to dump internal state to string</param>
            public string ToString(DateTime time)
            {
                long now = (long)(((double)time.Ticks / 10000) * m_BucketsPerSec);
                long bucket = now & m_BucketMask;

                StringBuilder buf = new StringBuilder(256);
                buf.AppendFormat("last_time={0}, last_bucket={1}, sum={2} (interval={3})\n",
                        m_LastTimeBucket, bucket, m_Sum, m_BucketsInterval);
                long n = (bucket - m_BucketsInterval) & m_BucketMask;
                for (long j = 0; j < m_BucketCount; j++)
                {
                    buf.AppendFormat("{0:00000}{1}", j, (j == bucket || j == n) ? '|' : ' ');
                    buf.Append((j < m_BucketCount - 1) ? "" : "\n");
                }
                for (long j = 0; j < m_BucketCount; j++)
                {
                    buf.AppendFormat("{0:000.0}{1}", m_Buckets[j], (j == bucket || j == n) ? '|' : ' ');
                    buf.Append((j < m_BucketCount - 1) ? "" : "\n");
                }
                return buf.ToString();
            }

        #endregion

        #region Protected

            /// <summary>
            /// Add value to the bucket associated with time
            /// </summary>
            /// <param name="time">Monotonically increasing time value</param>
            /// <param name="value">The value to add to the bucket associated with time</param>
            /// <returns>Current running sum</returns>
            internal override bool Try(DateTime time, double value)
            {
                int nowTimeBucket = (int)(((double)time.Ticks / 10000000) * m_BucketsPerSec);
                if (m_LastTimeBucket == 0)
                    m_LastTimeBucket = nowTimeBucket;
                long bucket = nowTimeBucket & m_BucketMask;
                int  timeDiff = nowTimeBucket - m_LastTimeBucket;
                double origValue = value;

                if (nowTimeBucket < m_LastTimeBucket)
                {
                    // (unlikely) Clock was adjusted
                    if (value > m_Limit)
                        return false;

                    m_Buckets[bucket] = value;
                    m_Sum = value;
                }
                else if (timeDiff == 0)
                {
                    // Most frequent case
                    double newSum = m_Sum + value;
                    if (newSum > m_Limit)
                        return false;
                    m_Sum = newSum;
                    m_Buckets[bucket] += value;
                }
                else if (timeDiff >= m_BucketsInterval)
                {
                    // Second most frequent case
                    int start = (nowTimeBucket - m_BucketsInterval + 1) & m_BucketMask;
                    int end = nowTimeBucket & m_BucketMask;
                    reset(start, end);
                    if (value > m_Limit)
                        return false;
                    m_Buckets[bucket] = value;
                    m_Sum = value;
                }
                else
                {
                    // Calculate sum of buckets from start to end
                    int start, end, validBuckets = m_BucketsInterval - timeDiff;
                    if (validBuckets <= (m_BucketsInterval >> 1))
                    {
                        start = (nowTimeBucket - m_BucketsInterval + 1) & m_BucketMask;
                        end = (m_LastTimeBucket + 1) & m_BucketMask;
                        m_Sum = 0;

                        Debug.Write(string.Format("Summing {0} through {1}", start, end));

                        for (long i = start; i != end; i = (i + 1) & m_BucketMask)
                            m_Sum += m_Buckets[i];
                        start = end;
                        end = nowTimeBucket & m_BucketMask;
                    }
                    else
                    {
                        start = (m_LastTimeBucket - m_BucketsInterval + 1) & m_BucketMask;
                        end = (nowTimeBucket - m_BucketsInterval + 1) & m_BucketMask;

                        Debug.Write(string.Format("Subtracting/resetting {0} through {1}", start, end));

                        for (long i = start; i != end; i = (i + 1) & m_BucketMask)
                        {
                            m_Sum -= m_Buckets[i];
                            m_Buckets[i] = 0;
                        }

                        if (m_Sum < 0) m_Sum = 0;
                        start = (m_LastTimeBucket + 1) & m_BucketMask;
                        end = nowTimeBucket & m_BucketMask;
                    }

                    Debug.Write(string.Format("Resetting {0} through {1}", start, end));

                    // Reset values in intermediate buckets since there was no activity there
                    reset(start, end);

                    double newSum = m_Sum + value;

                    if (newSum > m_Limit)
                        return false;

                    m_Sum = newSum;
                    m_Buckets[bucket] = value;
                }
                m_LastTimeBucket = nowTimeBucket;

                Debug.Write(ToString(time));

                return true;
            }

            protected override void DoConfigure(IConfigSectionNode node)
            {
                base.DoConfigure(node);
                Reset();
            }

        #endregion

        #region .pvt

            private void reset(int start, int end)
            {
                for (long i = start; i != end; i = (i + 1) & m_BucketMask)
                    m_Buckets[i] = 0;
            }

        #endregion
    }
}