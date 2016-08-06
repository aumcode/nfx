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
using NFX.ApplicationModel;
using NFX.Instrumentation;

namespace NFX.Instrumentation.Self
{

    [Serializable]
    public abstract class SelfInstrumentationLongGauge : LongGauge, IInstrumentationInstrument
    {
      protected SelfInstrumentationLongGauge(long value) : base(null, value) { }
    }

    [Serializable]
    public class RecordCount : SelfInstrumentationLongGauge, IMemoryInstrument
    {
        protected RecordCount(long value) : base(value) {}

        public static void Record(long value)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new RecordCount(value));
        }


        public override string Description { get{ return "Datum record count"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_RECORD; }}


        protected override Datum MakeAggregateInstance()
        {
            return new RecordCount(0);
        }
    }

    [Serializable]
    public class RecordLoad : SelfInstrumentationLongGauge, IMemoryInstrument
    {
        protected RecordLoad(long value) : base(value) {}

        public static void Record(long value)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new RecordLoad(value));
        }


        public override string Description { get{ return "Instrumentation load percentage: recordCount / maxRecordCount"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_PERCENT; }}


        protected override Datum MakeAggregateInstance()
        {
            return new RecordLoad(0);
        }
    }

    [Serializable]
    public class ProcessingInterval : SelfInstrumentationLongGauge
    {
        protected ProcessingInterval(long value) : base(value) {}

        public static void Record(long value)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new ProcessingInterval(value));
        }


        public override string Description { get{ return "Instrumentation processing interval in milliseconds"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_MSEC; }}


        protected override Datum MakeAggregateInstance()
        {
            return new ProcessingInterval(0);
        }
    }


    [Serializable]
    public class BufferMaxAge : SelfInstrumentationLongGauge, IMemoryInstrument
    {
        protected BufferMaxAge(long value) : base(value) {}

        public static void Record(long value)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new BufferMaxAge(value));
        }


        public override string Description { get{ return "Age of the oldest datum in result buffer in seconds"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_SEC; }}


        protected override Datum MakeAggregateInstance()
        {
            return new BufferMaxAge(0);
        }
    }




}
