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
    /// Three-dimensional histogram for storing number of samples for given
    /// dimension keys
    /// </summary>
    public class Histogram<TData1, TData2, TData3> : Histogram
    {
        /// <summary>
        /// Constructs a histogram from a given array of dimensions
        /// </summary>
        /// <param name="title">Histogram title used for displaying result</param>
        /// <param name="dimension1">Dimension of the 1st histogram dimension</param>
        /// <param name="dimension2">Dimension of the 2nd histogram dimension</param>
        /// <param name="dimension3">Dimension of the 3nd histogram dimension</param>
        public Histogram(string title,
            Dimension<TData1> dimension1,
            Dimension<TData2> dimension2,
            Dimension<TData3> dimension3)
            : base(title, 1, dimension1.PartitionCount +
                dimension2.PartitionCount + dimension3.PartitionCount)
        {
            m_Dimension1 = dimension1;
            m_Dimension2 = dimension2;
            m_Dimension3 = dimension3;
            m_Dimension1.SetIndex(0);
            m_Dimension2.SetIndex(1);
            m_Dimension3.SetIndex(2);
        }

        #region Public

            /// <summary>
            /// Number of dimensions in this histogram
            /// </summary>
            public override int DimensionCount { get { return 3; } }

            /// <summary>
            /// Return the sample count associated with given histogram keys
            /// </summary>
            public int this[int k1, int k2, int k3] { get { return this[new HistogramKeys(k1, k2, k3)]; } }

            /// <summary>
            /// Try to get the sample count associated with the given histogram keys.
            /// If the keys are not present in the histogram dictionary return false
            /// </summary>
            public bool TryGet(int k1, int k2, int k3, out int count)
            {
                return TryGet(new HistogramKeys(k1, k2, k3), out count);
            }

            /// <summary>
            /// Increment histogram statistics for a given triplet of dimension values
            /// </summary>
            public virtual void Sample(TData1 value1, TData2 value2, TData3 value3)
            {
                DoSample(Keys(value1, value2, value3));
            }

            /// <summary>
            /// Convert values to HistogramKeys struct
            /// </summary>
            public HistogramKeys Keys(TData1 value1, TData2 value2, TData3 value3)
            {
                return new HistogramKeys(m_Dimension1[value1], m_Dimension2[value2], m_Dimension3[value3]);
            }

            /// <summary>
            /// Returns number of samples collected for a given key.
            /// The key is obtained by mapping the given values into the dimensions' partitions.
            /// Return value of 0 indicates that key is not present in the histogram
            /// </summary>
            public int Value(TData1 value1, TData2 value2, TData3 value3)
            {
                var keys = Keys(value1, value2, value3);
                int count;
                return TryGet(keys, out count) ? count : 0;
            }

            public override IEnumerable<Dimension> Dimensions
            {
                get
                {
                    yield return m_Dimension1;
                    yield return m_Dimension2;
                    yield return m_Dimension3;
                }
            }

        #endregion

        #region Fields

            protected Dimension<TData1> m_Dimension1;
            protected Dimension<TData2> m_Dimension2;
            protected Dimension<TData3> m_Dimension3;

        #endregion

    }
}
