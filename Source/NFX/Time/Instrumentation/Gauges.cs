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

using NFX.Instrumentation;

namespace NFX.Time.Instrumentation
{
   [Serializable]
    public abstract class TimeInstrumentationLongGauge : LongGauge, ISchedulingInstrument
    {
      protected TimeInstrumentationLongGauge(long value) : base(null, value) { }
    }

    [Serializable]
    public class EventCount : TimeInstrumentationLongGauge
    {
        protected EventCount(long value) : base(value) {}

        public static void Record(long value)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new EventCount(value));
        }


        public override string Description { get{ return "Timer event count"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_EVENT; }}


        protected override Datum MakeAggregateInstance()
        {
            return new EventCount(0);
        }
    }

    [Serializable]
    public class FiredEventCount : TimeInstrumentationLongGauge
    {
        protected FiredEventCount(long value) : base(value) {}

        public static void Record(long value)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new FiredEventCount(value));
        }


        public override string Description { get{ return "Count of timer events that have fired since last measurement"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_EVENT; }}


        protected override Datum MakeAggregateInstance()
        {
            return new FiredEventCount(0);
        }
    }

}
