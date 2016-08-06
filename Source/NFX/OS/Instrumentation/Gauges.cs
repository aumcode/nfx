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

namespace NFX.OS.Instrumentation
{
    [Serializable]
    public abstract class OSLongGauge : LongGauge
    {
      protected OSLongGauge(string src, long value) : base(src, value) { }
    }

    [Serializable]
    public abstract class OSDoubleGauge : DoubleGauge
    {
      protected OSDoubleGauge(string src, double value) : base(src, value) {}
    }


    [Serializable]
    public class CPUUsage : OSLongGauge, ICPUInstrument
    {
        protected CPUUsage(string src, long value) : base(src, value) {}

        public static void Record(long value, string src=null)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new CPUUsage(src, value));
        }


        public override string Description { get{ return "CPU Usage %"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_PERCENT; }}


        protected override Datum MakeAggregateInstance()
        {
            return new CPUUsage(this.Source, 0);
        }
    }

    [Serializable]
    public class RAMUsage : OSLongGauge, IMemoryInstrument
    {
        protected RAMUsage(long value) : base(null, value) {}

        public static void Record(long value)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new RAMUsage(value));
        }


        public override string Description { get{ return "RAM Usage %"; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_PERCENT; }}


        protected override Datum MakeAggregateInstance()
        {
            return new RAMUsage(0);
        }
    }

    [Serializable]
    public class AvailableRAM : OSLongGauge, IMemoryInstrument
    {
        protected AvailableRAM(string src, long value) : base(src, value) {}

        public static void Record(long value, string src=null)
        {
           var inst = App.Instrumentation;
           if (inst.Enabled)
             inst.Record(new AvailableRAM(src, value));
        }


        public override string Description { get{ return "Available RAM mb."; }}
        public override string ValueUnitName { get{ return CoreConsts.UNIT_NAME_MB; }}


        protected override Datum MakeAggregateInstance()
        {
            return new AvailableRAM(this.Source, 0);
        }
    }

}
