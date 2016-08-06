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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Instrumentation.Analytics
{
    /// <summary>
    /// Represents histogram dimension key vector.
    /// This class is introduced for performance enhancement vs int[] approach
    /// that would have allocated extra object on heap per call
    /// </summary>
    public struct HistogramKeys : IComparable<HistogramKeys>, IComparable
    {
        public readonly int Key1;
        public readonly int Key2;
        public readonly int Key3;

        public HistogramKeys(int k1)
        {
            Key1 = k1; Key2 = 0; Key3 = 0;
        }

        public HistogramKeys(int k1, int k2)
        {
            Key1 = k1; Key2 = k2; Key3 = 0;
        }

        public HistogramKeys(int k1, int k2, int k3)
        {
            Key1 = k1; Key2 = k2; Key3 = k3;
        }

        public override int GetHashCode()
        {
            return Key1 + Key2 + Key3;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = (HistogramKeys)obj;
            return Key1 == other.Key1
                && Key2 == other.Key2
                && Key3 == other.Key3;
        }

        public override string ToString()
        {
            return "{" + Key1 + "," + Key2 + "," + Key3 + "}";
        }

        public int this[int idx]
        {
            get
            {
                switch (idx)
                {
                    case 0: return Key1;
                    case 1: return Key2;
                    case 2: return Key3;
                    default: throw new NFXException(StringConsts.ARGUMENT_ERROR + "idx[" + idx + "]");
                }
            }
        }

        #region IComparable Members

            public int CompareTo(object obj)
            {
                return CompareTo((HistogramKeys)obj);
            }

            public int CompareTo(HistogramKeys rhs)
            {
                if (Key1 == rhs.Key1)
                    if (Key2 == rhs.Key2)
                        return (Key3 == rhs.Key3) ? 0 : (Key3 < rhs.Key3 ? -1 : 1);
                    else
                        return (Key2 < rhs.Key2 ? -1 : 1);
                else
                    return (Key1 < rhs.Key1 ? -1 : 1);
            }

        #endregion
    }

    /// <summary>
    /// The placeholder of histogram data that represents a sample count
    /// for a given set of histogram keys.
    /// </summary>
    public class HistogramEntry
    {
        public HistogramEntry(HistogramKeys keys)
        {
            m_BucketKeys = keys;
            m_Count = 1;
        }

        public HistogramKeys BucketKeys { get { return m_BucketKeys; } }
        public int           Count      { get { return m_Count; } }

        internal HistogramKeys  m_BucketKeys;
        internal int            m_Count;
    }

    /// <summary>
    /// Helper type representing a dictionary for storing historam data
    /// </summary>
    public class HistData : Dictionary<HistogramKeys, HistogramEntry>
    {
        public HistData(int capacity) : base(capacity) {}
    }

    /// <summary>
    /// Basic histogram interface to be implemented by all types of
    /// instrumentation histograms
    /// </summary>
    public interface IHistogram : IEnumerable<HistogramEntry>
    {
        /// <summary>
        /// Histogram title used for displaying purposes
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Total number of samples in the histogram
        /// </summary>
        long TotalSamples { get; }

        /// <summary>
        /// Number of dimensions in this histogram
        /// </summary>
        int DimensionCount { get; }

        /// <summary>
        /// Return a dimension of a histogram identified by index
        /// </summary>
        Dimension GetDimention(int dimension);

        /// <summary>
        /// Returns the dimension instances of this histogram
        /// </summary>
        IEnumerable<Dimension> Dimensions { get; }

        /// <summary>
        /// Returns the name of a partition in a given histogram dimension
        /// </summary>
        /// <param name="dimension">Dimension index</param>
        /// <param name="partition">Partition index</param>
        string GetPartitionName(int dimension, int partition);

        /// <summary>
        /// Return the count of samples for the given histogram keys
        /// </summary>
        int this[HistogramKeys keys] { get; }

        /// <summary>
        /// Try to get the count of samples for the given histogram keys
        /// </summary>
        bool TryGet(HistogramKeys keys, out int count);

        /// <summary>
        /// Reset histogram state
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Base histogram to be used by typed histogram generic derived classes.
    /// The histogram holds a dictionary of integer counts for each measurement
    /// keys (with number of keys correspondent to the number of dimensions
    /// in the histogram).
    /// </summary>
    public abstract class Histogram : IHistogram
    {
        protected Histogram(string title, int dimCount, int approxCapacity)
        {
            m_TotalSamples  = 0;
            m_Title         = title;
            m_Data          = new HistData(approxCapacity);
        }

        #region Fields

            protected HistData m_Data;
            protected long     m_TotalSamples;
            protected string   m_Title;

        #endregion

        #region Public/Props

            /// <summary>
            /// Number of dimensions in this histogram
            /// </summary>
            public abstract int DimensionCount { get; }

            /// <summary>
            /// Return a dimension of a histogram identified by index
            /// </summary>
            public Dimension GetDimention(int dimension)
            {
                var dim = Dimensions.ElementAtOrDefault(dimension);

                if (dim == null)
                    throw new NFXException(StringConsts.ARGUMENT_ERROR + "dimension");

                return dim;
            }

            /// <summary>
            /// Histogram title used for displaying purposes
            /// </summary>
            public virtual string Title { get { return m_Title ?? GetType().FullName; } }

            /// <summary>
            /// Total number of samples in the histogram
            /// </summary>
            public long TotalSamples { get { return m_TotalSamples; } }

            /// <summary>
            /// Returns the dimension instances of this histogram
            /// </summary>
            public abstract IEnumerable<Dimension> Dimensions { get; }

            /// <summary>
            /// Returns the name of a partition in a given histogram dimension
            /// </summary>
            /// <param name="dimension">Dimension index</param>
            /// <param name="partition">Partition index</param>
            public string GetPartitionName(int dimension, int partition)
            {
                var dim = GetDimention(dimension);
                return dim.GetPartitionName(partition);
            }

            /// <summary>
            /// Return the count of samples for the given histogram keys
            /// </summary>
            public int this[HistogramKeys keys] { get { return m_Data[keys].Count; } }

            /// <summary>
            /// Try to get the count of samples for the given histogram keys
            /// </summary>
            public bool TryGet(HistogramKeys keys, out int count)
            {
                HistogramEntry entry = new HistogramEntry(keys);
                bool result = m_Data.TryGetValue(keys, out entry);
                count = result ? entry.Count : 0;
                return result;
            }

            /// <summary>
            /// Reset histogram state
            /// </summary>
            public virtual void Clear()
            {
                m_TotalSamples = 0;
                m_Data.Clear();
            }

            public override string ToString()
            {
                return Title ?? GetType().FullName;
            }

            public override int GetHashCode()
            {
                return m_Data.GetHashCode() * DimensionCount;
            }

            public IEnumerator<HistogramEntry> GetEnumerator()
            {
                return m_Data.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_Data.Values.GetEnumerator();
            }

        #endregion

        #region Protected

            protected Dimension this[int dimIndex] { get { return GetDimention(dimIndex); } }

            internal void DoSample(HistogramKeys keys)
            {
                HistogramEntry entry;
                if (m_Data.TryGetValue(keys, out entry))
                    entry.m_Count++;
                else
                    m_Data.Add(keys, new HistogramEntry(keys));

                m_TotalSamples++;
            }

        #endregion
    }
}
