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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Serialization.JSON;

namespace NFX.Instrumentation
{
    /// <summary>
    /// Base class for single measurement events (datums) reported to instrumentation
    /// </summary>
    [Serializable]
    public abstract class Datum : IJSONWritable
    {
       public const string UNSPECIFIED_SOURCE = "*";
       public const string FRAMEWORK_SOURCE = "Frmwrk";
       public const string BUSINESS_SOURCE = "BsnsLgc";


       protected Datum()
       {
         m_UTCTime = App.TimeSource.UTCNow;
       }

       protected Datum(string source)
       {
         m_Source = source;
         m_UTCTime = App.TimeSource.UTCNow;
       }

       protected Datum(string source, DateTime utcDateTime)
       {
         m_Source = source;
         m_UTCTime = utcDateTime;
       }

       protected string m_Source;
       protected int m_Count;
       protected DateTime m_UTCTime;
       protected DateTime m_UTCEndTime;

       /// <summary>
       /// Returns UTC time stamp when event happened
       /// </summary>
       public DateTime UTCTime
       {
         get { return m_UTCTime;}
       }

       /// <summary>
       /// Returns UTC time stamp when event happened. This property may be gotten only if IsAggregated==true, otherwise UTCTime value is returned
       /// </summary>
       public DateTime UTCEndTime
       {
         get { return m_Count==0 ? m_UTCTime : m_UTCEndTime;}
       }


       /// <summary>
       /// Indicates whether this instance represents a rollup/aggregation of multiple events
       /// </summary>
       public bool IsAggregated
       {
         get { return m_Count>0;}
       }

       /// <summary>
       /// Returns count of measurements. This property may be gotten only if IsAggregated==true, otherwise zero is returned
       /// </summary>
       public int Count
       {
         get
         {
           return m_Count;
         }
       }

       /// <summary>
       /// Returns datum source. Data are rolled-up by type of recorded datum instances and source
       /// </summary>
       public string Source
       {
         get {return m_Source ?? UNSPECIFIED_SOURCE; }
       }

       /// <summary>
       /// Returns rate of occurrence string
       /// </summary>
       public string Rate
       {
         get
         {
           if (m_Count<=0) return CoreConsts.UNKNOWN;

           var span = m_UTCEndTime - m_UTCTime;

           if (m_Count==1 && span.TotalMilliseconds<0.1) return string.Empty;

           var rate = m_Count / span.TotalSeconds;

           if (rate>1.0)
            return string.Format("{0:0.00}/sec.", rate);

           rate = rate * 1000;
           return string.Format("{0:0.00}/msec.", rate);
         }
       }


       /// <summary>
       /// Returns description for data that this datum represents. Base implementation returns full type name of this instance
       /// </summary>
       public virtual string Description
       {
         get { return GetType().FullName; }
       }


       /// <summary>
       /// Provides access to value polymorphically
       /// </summary>
       public abstract object ValueAsObject { get;}

       /// <summary>
       /// Provides numeric value used for charts
       /// </summary>
       public virtual object PlotValue { get { return ValueAsObject; } }

       /// <summary>
       /// Provides name for units that value is measured in
       /// </summary>
       public abstract string ValueUnitName { get; }



                       private static Dictionary<Type, IEnumerable<Type>> s_ViewGroupInterfaces = new Dictionary<Type,IEnumerable<Type>>();

       /// <summary>
       /// Returns Datum classification interfaces marked with InstrumentViewGroup attribute. The implementation is cached fr efficiency
       /// </summary>
       public IEnumerable<Type> ViewGroupInterfaces { get { return GetViewGroupInterfaces(GetType());} }

       /// <summary>
       /// Returns Datum classification interfaces marked with InstrumentViewGroup attribute. The implementation is cached fr efficiency
       /// </summary>
       public static IEnumerable<Type> GetViewGroupInterfaces(Type tDatum)
       {
           var dict = s_ViewGroupInterfaces;//atomic

           IEnumerable<Type> result = null;
           if (dict.TryGetValue(tDatum, out result)) return result;

           result = tDatum.GetInterfaces().Where(i => Attribute.IsDefined(i, typeof(InstrumentViewGroup)));

           dict = new Dictionary<Type,IEnumerable<Type>>(dict);
           dict[tDatum] = result;

           s_ViewGroupInterfaces = dict;//atomic

           return result;
       }



       /// <summary>
       /// Aggregates events, for example from multiple threads into one
       /// </summary>
       public Datum Aggregate(IEnumerable<Datum> many)
       {
            var start = DateTime.MaxValue;
            var end = DateTime.MinValue;
            var cnt = 0;

            var result = MakeAggregateInstance();

            foreach(var e in many)
            {
              cnt++;
              if (e.UTCTime < start) start = e.UTCTime;
              if (e.UTCTime > end) end = e.UTCTime;
              result.AggregateEvent(e);
            }

            result.m_Count = cnt;
            result.m_UTCTime = start;
            result.m_UTCEndTime = end;
            result.SummarizeAggregation();

            return result;
       }


       /// <summary>
       /// Override to set a new source value which is less-specific than existing source.
       /// ReductionLevel specifies how much detail should be lost. The function is idempotent, that is - calling more than once with the same arg does not
       /// change the state of the object.
       /// The default implementation removes all source details (unspecified source) when reductionLevel less than zero.
       /// Example:
       ///  TotalBytesSent("mpx://45.12.123.19:7823 -> MySystem.Contracts.IDoSomething.SomeMethod1()")
       ///  ReduceSourceDetail(0) -> yields original string
       ///  ReduceSourceDetail(1) - > "MySystem.Contracts.IDoSomething.SomeMethod1()"
       ///  ReduceSourceDetail(2) - > "MySystem.Contracts.IDoSomething"
       ///  ReduceSourceDetail(3) - > ""
       /// </summary>
       public virtual void ReduceSourceDetail(int reductionLevel)
       {
         if (reductionLevel<0) m_Source = string.Empty;
       }



       protected abstract Datum MakeAggregateInstance();
       protected virtual void AggregateEvent(Datum dat) { }
       protected virtual void SummarizeAggregation() { }

       public override string ToString()
       {
         var t = GetType().FullName;
         var d = Description;
         if (t!=d) t = t + " '" + d + "'";
         var r = Rate;
         if (r.IsNotNullOrWhiteSpace())
            return "[{0}] {1} {2} @ {3} Value: {4} {5}".Args(t, Source, Count, r, ValueAsObject, ValueUnitName);
         else
            return "[{0}] {1} {2} Value: {3} {4}".Args(t, Source, Count, ValueAsObject, ValueUnitName);
       }


       public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
       {
         var data = new Dictionary<string, object>();

         WriteJSONFields( data, options );

         JSONWriter.WriteMap(wri, data, nestingLevel, options);
       }

       protected virtual void WriteJSONFields(IDictionary<string, object> data, JSONWritingOptions options)
       {
          data["cnt"]  = this.Count;
          data["sd"]   = this.UTCTime;
          data["ed"]   = this.UTCEndTime;
          data["rate"] = this.Rate;
          data["val"]  = this.ValueAsObject;
          data["plt"] = this.PlotValue;

          if (options!=null && options.Purpose >= JSONSerializationPurpose.Marshalling)
          {
            data["src"]   = this.m_Source;
            data["aggr"]  = this.IsAggregated;
            data["tp"]    = this.GetType().FullName;
            data["descr"] = this.Description;
            data["unit"]  = this.ValueUnitName;
          }
       }

    }
}
