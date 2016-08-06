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
    /// One-dimensional histogram for storing number of samples for a given
    /// dimension key
    /// </summary>
    public class Histogram<TData1> : Histogram
    {
        /// <summary>
        /// Constructs a histogram from a given array of dimensions
        /// </summary>
        /// <param name="title">Histogram title used for displaying result</param>
        /// <param name="dimension1">Dimension of the histogram dimension</param>
        public Histogram(string title, Dimension<TData1> dimension1)
            : base(title, 1, dimension1.PartitionCount)
        {
            m_Dimension1 = dimension1;
            m_Dimension1.SetIndex(0);
        }

        #region Public

            /// <summary>
            /// Number of dimensions in this histogram
            /// </summary>
            public override int DimensionCount { get { return 1; } }

            /// <summary>
            /// Return the sample count associated with given histogram keys
            /// </summary>
            public new int this[int key] { get { return this[new HistogramKeys(key)]; } }

            /// <summary>
            /// Try to get the sample count associated with the given histogram key.
            /// If the key is not present in the histogram dictionary return false
            /// </summary>
            public bool TryGet(int key, out int count) { return TryGet(new HistogramKeys(key), out count); }

            /// <summary>
            /// Increment histogram statistics for a given dimension value
            /// </summary>
            public virtual void Sample(TData1 value)
            {
                DoSample(Keys(value));
            }

            /// <summary>
            /// Convert a value to HistogramKeys struct
            /// </summary>
            public HistogramKeys Keys(TData1 value)
            {
                return new HistogramKeys(m_Dimension1[value]);
            }

            /// <summary>
            /// Returns number of samples collected for a given key.
            /// The key is obtained by mapping the given value into the dimension's partition.
            /// Return value of 0 indicates that key is not present in the histogram
            /// </summary>
            public int Value(TData1 value)
            {
                var keys = Keys(value);
                int count;
                return TryGet(keys, out count) ? count : 0;
            }

            public override IEnumerable<Dimension> Dimensions
            {
                get { yield return m_Dimension1; }
            }

        #endregion

        #region Fields

            protected Dimension<TData1> m_Dimension1;

        #endregion

    }
}
