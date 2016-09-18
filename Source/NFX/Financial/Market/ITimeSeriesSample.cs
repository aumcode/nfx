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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Financial.Market
{
  /// <summary>
  /// Represents a sample of a TimeSeries stream
  /// </summary>
  public interface ITimeSeriesSample
  {
    /// <summary>
    /// Timestamp of the sample
    /// </summary>
    DateTime TimeStamp{ get; }

    /// <summary>
    /// Associates an arbitrary data
    /// </summary>
    object AssociatedData { get;}

    /// <summary>
    /// Makes aggregate instance
    /// </summary>
    ITimeSeriesSample MakeAggregateInstance();

    /// <summary>
    /// Adds a sample to this aggregation instance
    /// </summary>
    void AggregateSample(ITimeSeriesSample sample);

    /// <summary>
    /// Summarizes aggregation on this isntance
    /// </summary>
    void SummarizeAggregation();
  }


  /// <summary>
  /// Represents a sample of a TimeSeries stream
  /// </summary>
  public abstract class TimeSeriesSampleBase : ITimeSeriesSample
  {
    protected TimeSeriesSampleBase(DateTime timeStamp)
    {
      m_TimeStamp = timeStamp;
    }

    private DateTime m_TimeStamp;

    public DateTime TimeStamp{ get{ return m_TimeStamp;} }

    /// <summary>
    /// Associates an arbitrary data
    /// </summary>
    public object AssociatedData { get; set;}


    public virtual ITimeSeriesSample MakeAggregateInstance()
    {
      throw new NotImplementedException(GetType().FullName+".MakeAggregateInstance()");
    }

    public virtual void AggregateSample(ITimeSeriesSample sample)
    {
      throw new NotImplementedException(GetType().FullName+".AggregateSample(sample)");
    }

    public virtual void SummarizeAggregation() { }
  }


}
