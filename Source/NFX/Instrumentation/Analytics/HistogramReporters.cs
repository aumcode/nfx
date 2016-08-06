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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NFX.Instrumentation.Analytics
{
    /// <summary>
    /// Maps histogram data into some other representation (such as text, HTML, etc)
    /// </summary>
    public static class HistogramReporters
    {
        public static string ToStringReport(this IHistogram hist)
        {
            const string s_Count = "Count";

            SortedList<HistogramKeys, HistogramEntry> m_Data =
                new SortedList<HistogramKeys, HistogramEntry>();
            foreach (var entry in hist)
                m_Data.Add(entry.BucketKeys, entry);

            int[] maxDimTitleLen = new int[hist.DimensionCount];
            foreach (var d in hist.Dimensions)
                maxDimTitleLen[d.Index] = d.Name.Length + 1;

            int maxTitleLen = 0;
            int maxValue = Convert.ToInt32(Math.Pow(10, s_Count.Length));

            // Calculate max legth of partition titles in each dimension
            foreach (var item in m_Data)
            {
                int titleLen = 0;
                foreach (var d in hist.Dimensions)
                {
                    int i = d.Index;
                    int n = hist.GetPartitionName(i, item.Key[i]).Length + 1;
                    if (n > maxDimTitleLen[i])
                        maxDimTitleLen[i] = n;
                    else
                        n = maxDimTitleLen[i];
                    titleLen += n;
                }
                if (titleLen > maxTitleLen)
                    maxTitleLen = titleLen;
                if (item.Value.Count > maxValue)
                    maxValue = item.Value.Count;
            }

            int maxValueLen = maxValue.ToString().Length;

            var result = new StringBuilder();

            result.AppendFormat("{0}\n", hist.Title);

            foreach (var d in hist.Dimensions)
                result.AppendFormat("|{0}",
                    hist.GetDimention(d.Index).Name.PadLeft(maxDimTitleLen[d.Index]-1));
            result.AppendFormat("|{0}|     %|Total%|\n", s_Count.PadLeft(maxValueLen));

            foreach (var d in hist.Dimensions)
                result.AppendFormat("|{0," + (maxDimTitleLen[d.Index]-1) + "}",
                    new String('-', maxDimTitleLen[d.Index]-1));
            result.AppendFormat("|{0," + maxValueLen + "}|------|------|\n", new String('-', maxValueLen));

            double tot = 0;

            foreach (var item in m_Data)
            {
                var title = new StringBuilder();
                foreach (var d in hist.Dimensions)
                    title.AppendFormat("|{0}",
                        hist.GetPartitionName(d.Index, item.Key[d.Index]).PadLeft(maxDimTitleLen[d.Index]-1));

                double pcnt = 100.0 * (double)item.Value.Count / hist.TotalSamples;
                tot += pcnt;
                result.AppendFormat(
                    "{0}|{1," + maxValueLen + "}|{2,6:##0.00}|{3,6:##0.00}|\n",
                    title.ToString().PadLeft(maxTitleLen), item.Value.Count, pcnt, tot);
            }

            return result.ToString();
        }
    }
}
