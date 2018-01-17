/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
    /// A dimension of data type double partitions a range of values
    /// by mapping the given value into [0 ... PartitionCount] partitions.
    /// This is a helper class for creating a dimension of TimeHistogram
    /// </summary>
    public class TimeDimension : Dimension<double>
    {
        internal const int DEFAULT_PART_COUNT = 7;

        /// <summary>
        /// Creates a histogram dimension. If partitionFunc is null then DoPartition must be
        /// overriden in the derived class
        /// </summary>
        /// <param name="desc">Description of dimension used in reporting</param>
        /// <param name="partCount">Total number of partitions</param>
        /// <param name="partitionFunc">Used to inject a function in order not to override this class</param>
        /// <param name="partitionNameFunc">Used to inject a function in order not to override this class</param>
        public TimeDimension(
            string desc,
            int partCount = DEFAULT_PART_COUNT,
            PartitionFunc<double> partitionFunc = null,
            PartitionNameFunc partitionNameFunc = null)
            : base(desc, partCount, partitionFunc, partitionNameFunc)
        {
        }

        #region Public

            /// <summary>
            /// Maps a value into partition index
            /// </summary>
            public new int this[double value]
            {
                get { return m_PartitionFunc == null ? DoPartition(value) : m_PartitionFunc(this, value); }
            }

        #endregion

        #region Protected

            protected override int DoPartition(double v)
            {
                if (v < 0.000010) return 0;
                if (v <= 0.000025) return Convert.ToInt32(Math.Round((v - 0.000010) * 1000000.0) / 5) + 0;
                if (v <= 0.000100) return Convert.ToInt32(Math.Round((v - 0.000025) * 1000000.0) / 25) + 3;
                return DEFAULT_PART_COUNT;
            }

            protected override string DoPartitionName(int i)
            {
                switch (i)
                {
                    case 0: return "<10us";
                    case 1: return "<15us";
                    case 2: return "<20us";
                    case 3: return "<25us";
                    case 4: return "<50us";
                    case 5: return "<75us";
                    case 6: return "<100us";
                    case 7: return ">100us";
                    default: throw new ArgumentException(); // impossible
                }
            }

        #endregion

    }
}
