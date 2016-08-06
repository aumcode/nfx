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

using NFX.Financial;


namespace NFX.Instrumentation
{

    /// <summary>
    /// Represents a general-purpose long integer measurement datum
    /// </summary>
    [Serializable]
    public abstract class LongGauge : Gauge
    {
       protected LongGauge(long value) : base()
       {
         m_Value = value;
       }

       protected LongGauge(string source, long value) : base(source)
       {
         m_Value = value;
       }

       protected LongGauge(string source, long value, DateTime utcDateTime) : base(source, utcDateTime)
       {
         m_Value = value;
       }


       private long m_Value;


       /// <summary>
       /// Gets gauge value
       /// </summary>
       public long Value { get {return m_Value;} }


       public override object ValueAsObject
       {
           get { return m_Value; }
       }

       public override string ValueUnitName
       {
           get { return CoreConsts.UNIT_NAME_UNSPECIFIED; }
       }

          [NonSerialized]
          private long m_Sum;

        protected override void AggregateEvent(Datum dat)
        {
            var dg = dat as LongGauge;
            if (dg==null) return;

            m_Sum += dg.Value;
        }

        protected override void SummarizeAggregation()
        {
            m_Value = m_Sum / m_Count;
        }


    }


    /// <summary>
    /// Represents a general-purpose double measurement datum
    /// </summary>
    [Serializable]
    public abstract class DoubleGauge : Gauge
    {

       protected DoubleGauge(string source, double value) : base(source)
       {
         m_Value = value;
       }

       protected DoubleGauge(string source, double value, DateTime utcDateTime) : base(source, utcDateTime)
       {
         m_Value = value;
       }


       private double m_Value;


       /// <summary>
       /// Gets gauge value
       /// </summary>
       public double Value { get {return m_Value;} }


       public override object ValueAsObject
       {
           get { return m_Value; }
       }

       public override string ValueUnitName
       {
           get { return CoreConsts.UNIT_NAME_UNSPECIFIED; }
       }



          [NonSerialized]
          private double m_Sum;

        protected override void AggregateEvent(Datum dat)
        {
            var dg = dat as DoubleGauge;
            if (dg==null) return;

            m_Sum += dg.Value;
        }

        protected override void SummarizeAggregation()
        {
            if (m_Count>0)
                m_Value = m_Sum / (double)m_Count;
        }


    }




    /// <summary>
    /// Represents a general-purpose decimal measurement datum
    /// </summary>
    [Serializable]
    public abstract class DecimalGauge : Gauge
    {
       protected DecimalGauge(string source, decimal value) : base(source)
       {
         m_Value = value;
       }

       protected DecimalGauge(string source, decimal value, DateTime utcDateTime) : base(source, utcDateTime)
       {
         m_Value = value;
       }


       private decimal m_Value;


       /// <summary>
       /// Gets gauge value
       /// </summary>
       public decimal Value { get {return m_Value;} }


       public override object ValueAsObject
       {
           get { return m_Value; }
       }

       public override string ValueUnitName
       {
           get { return CoreConsts.UNIT_NAME_UNSPECIFIED; }
       }



          [NonSerialized]
          private decimal m_Sum;


        protected override void AggregateEvent(Datum dat)
        {
            var dg = dat as DecimalGauge;
            if (dg==null) return;

            m_Sum += dg.Value;
        }

        protected override void SummarizeAggregation()
        {
            if (m_Count>0)
                m_Value = m_Sum / (decimal)m_Count;
        }


    }



    /// <summary>
    /// Represents a general-purpose financial Amount measurement datum
    /// </summary>
    [Serializable]
    public abstract class AmountGauge : Gauge, IFinancialLogic
    {
       public const string CURRENCY_DELIM = "::";

       private static string makeSource(string source, Amount value)
       {
         var prefix = value.CurrencyISO + CURRENCY_DELIM;
         if (source == null || !source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
           return prefix + source;
         else
           return source;
       }

       protected AmountGauge(string source, Amount value) : base( makeSource(source, value))
       {
         m_Value = value;
         m_Sum = new Amount(value.CurrencyISO, 0M);
       }

       protected AmountGauge(string source, Amount value, DateTime utcDateTime, bool skipSourceConstruction = false) : base(makeSource(source, value), utcDateTime)
       {
         m_Value = value;
         m_Sum = new Amount(value.CurrencyISO, 0M);
       }


       private Amount m_Value;


       /// <summary>
       /// Gets gauge value
       /// </summary>
       public Amount Value { get {return m_Value;} }


       public override object ValueAsObject
       {
           get { return m_Value; }
       }

       public override object PlotValue
       {
         get { return m_Value.Value; }
       }

       public override string ValueUnitName
       {
          get { return CoreConsts.UNIT_NAME_MONEY; }
       }



          [NonSerialized]
          private Amount m_Sum;


        protected override void AggregateEvent(Datum dat)
        {
            var dg = dat as AmountGauge;
            if (dg==null) return;

            m_Sum = m_Sum + dg.Value;
        }

        protected override void SummarizeAggregation()
        {
            if (m_Count>0)
                m_Value = m_Sum / (double)m_Count;
        }


    }

}
