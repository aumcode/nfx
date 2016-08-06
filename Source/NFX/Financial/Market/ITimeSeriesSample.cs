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
