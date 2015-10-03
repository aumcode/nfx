using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NFX.WinForms.Controls.ChartKit.Temporal
{
  /// <summary>
  /// Defines model for time series data which is always sorted by time ascending.
  /// The class is optimized for list traversal and addition of new points to the right
  /// </summary>
  public abstract class TimeSeries : Series
  {
      public const int MAX_SAMPLES_DEFAULT = 1024;
      public const int MAX_SAMPLES_MIN = 16;

          /// <summary>
          /// Represents a sample of a TimeSeries stream
          /// </summary>
          public abstract class Sample
          {
            protected Sample(DateTime timeStamp)
            {
              m_TimeStamp = timeStamp;
            }
    
            private DateTime m_TimeStamp;

            public DateTime TimeStamp{ get{ return m_TimeStamp;} }

            /// <summary>
            /// Associates an arbitrary data
            /// </summary>
            public object AssociatedData { get; set;}
          }


      protected TimeSeries(string name, int order, TimeSeries parent = null) : base(name, order, parent)
      {
      }

      protected override void Destructor()
      {
      }

      private LinkedList<TimeSeries.Sample> m_Data = new LinkedList<Sample>();
      private int m_MaxSamples = MAX_SAMPLES_DEFAULT;

      /// <summary>
      /// Imposes a limit on the maximum size of the time series
      /// </summary>
      public int MaxSamples
      {
         get{ return m_MaxSamples;}
         set{ m_MaxSamples = value < MAX_SAMPLES_MIN ? MAX_SAMPLES_MIN : value; }
      }

      /// <summary>
      /// Returns the number of samples in this series
      /// </summary>
      protected override int SampleCount
      {
	       get { return m_Data.Count; }
      }

      /// <summary>
      /// Returns data in the series in natural time order
      /// </summary>
      public IEnumerable<TimeSeries.Sample> Data { get{ return m_Data;}}

      /// <summary>
      /// Returns data in the series in the reveresed time order
      /// </summary>
      public IEnumerable<TimeSeries.Sample> DataReveresed 
      {
        get
        { 
           var current = m_Data.Last;

           while(current!=null)
           {
             yield return current.Value;
             current = current.Previous;
           }
        }
      }


      /// <summary>
      /// Returns last X data samples in the series in natural order. It may return less samples than X
      /// </summary>
      public IEnumerable<TimeSeries.Sample> LastXSamplesData(int x)
      {
        var current = m_Data.Last;
        if (current==null) yield break;
        for(var i=0; i<x; i++)
        {
          var prior = current.Previous;
          if (prior==null) break;
          current = prior;
        }

        while(current!=null)
        {
          yield return current.Value;
          current = current.Next;
        }
      }


      /// <summary>
      /// Adds sample to the series at the appropriate position.
      /// This method respects MaxSamples and first deletes older samples making room for new additions
      /// </summary>
      protected void Add(Sample sample)
      {
        if (sample==null) throw new WFormsException(StringConsts.ARGUMENT_ERROR+"TimeSeries.Add(sample==null)");
       
        //remove data over max samples
        while(m_Data.Count >= m_MaxSamples) m_Data.RemoveFirst();


        var dt = sample.TimeStamp;
        var head = m_Data.First;
        if (head==null || head.Value.TimeStamp >= dt)
        {
           m_Data.AddFirst(sample);
           return;
        }

        var last = m_Data.Last;
        if (last.Value.TimeStamp <= dt)
        {
          m_Data.AddLast(sample);
          return;
        }


        var d1 = dt - head.Value.TimeStamp;
        var d2 = last.Value.TimeStamp - dt;

        if (d1<d2)
        {
            var node = head;
            while(node!=null)
            {
              if (node.Value.TimeStamp>=dt)
              {
                m_Data.AddBefore(node, sample);
                return;
              }
              node = node.Next;
            }
            m_Data.AddLast(sample);
        }
        else
        {
            var node = last;
            while(node!=null)
            {
              if (node.Value.TimeStamp<=dt)
              {
                m_Data.AddAfter(node, sample);
                return;
              }
              node = node.Previous;
            }
            m_Data.AddFirst(sample);
        }
      }

      /// <summary>
      /// Deletes sample from the set. This method is not efficient as it does linear list scan
      /// </summary>
      public bool Delete(Sample sample)
      {
        return m_Data.Remove(sample);
      }

  }



  public abstract class TimeSeries<TSample> : TimeSeries where TSample : TimeSeries.Sample
  {

    protected TimeSeries(string name, int order, TimeSeries parent = null) : base(name, order, parent)
    {
    }
    
    public void Add(TSample sample)
    {
      base.Add( sample );
    }

    public new IEnumerable<TSample> Data { get{ return base.Data.Cast<TSample>(); } }

    public new IEnumerable<TSample> DataReveresed { get{ return base.DataReveresed.Cast<TSample>(); } }

    public new IEnumerable<TimeSeries.Sample> LastXSamplesData(int x)
    {
       return base.LastXSamplesData(x).Cast<TSample>();
    }
  }

}
