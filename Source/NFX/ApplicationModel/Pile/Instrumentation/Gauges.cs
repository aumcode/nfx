/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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

namespace NFX.ApplicationModel.Pile.Instrumentation
{
    /// <summary>
    /// Provides base for pile long gauges
    /// </summary>
    [Serializable]
    public abstract class PileLongGauge : LongGauge, ICacheInstrument, IMemoryInstrument
    {
      protected PileLongGauge(string src, long value) : base(src, value) {}
    }

    /// <summary>
    /// Provides base for pile double gauges
    /// </summary>
    [Serializable]
    public abstract class PileDoubleGauge : DoubleGauge, ICacheInstrument, IMemoryInstrument
    {
      protected PileDoubleGauge(string src, double value) : base(src, value) {}
    }


    /// <summary>
    /// Provides object count in the instance
    /// </summary>
    [Serializable]
    public class ObjectCount : PileLongGauge
    {
        internal ObjectCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides object count in the instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_OBJECT; } }

        protected override Datum MakeAggregateInstance() { return new ObjectCount(this.Source, 0); }
    }


    /// <summary>
    /// Provides segment count in the instance
    /// </summary>
    [Serializable]
    public class SegmentCount : PileLongGauge
    {
        internal SegmentCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides segment count in the instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_SEGMENT; } }

        protected override Datum MakeAggregateInstance() { return new SegmentCount(this.Source, 0); }
    }

    /// <summary>
    /// Number of bytes allocated by the instance from system memory
    /// </summary>
    [Serializable]
    public class AllocatedMemoryBytes : PileLongGauge
    {
        internal AllocatedMemoryBytes(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Bytes allocated by the instance from system memory";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_BYTES; } }

        protected override Datum MakeAggregateInstance() { return new AllocatedMemoryBytes(this.Source, 0); }
    }


    /// <summary>
    /// Average capacity of free memory that the system has left
    /// </summary>
    [Serializable]
    public class MemoryCapacityBytes : PileLongGauge
    {
        internal MemoryCapacityBytes(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Average capacity of free memory that the system has left";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_BYTES; } }

        protected override Datum MakeAggregateInstance() { return new MemoryCapacityBytes(this.Source, 0); }
    }


    /// <summary>
    /// Number of bytes allocated for object storage within AllocatedMemoryBytes
    /// </summary>
    [Serializable]
    public class UtilizedBytes : PileLongGauge
    {
        internal UtilizedBytes(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Number of bytes allocated for object storage within AllocatedMemoryBytes";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_BYTES; } }

        protected override Datum MakeAggregateInstance() { return new UtilizedBytes(this.Source, 0); }
    }


    /// <summary>
    /// Number of extra bytes used by pile metadata currently occupied by object stored in this pile
    /// </summary>
    [Serializable]
    public class OverheadBytes : PileLongGauge
    {
        internal OverheadBytes(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Number of extra bytes used by pile metadata currently occupied by object stored in this pile";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_BYTES; } }

        protected override Datum MakeAggregateInstance() { return new OverheadBytes(this.Source, 0); }
    }


    /// <summary>
    /// Number of bytes for average object
    /// </summary>
    [Serializable]
    public class AverageObjectSizeBytes : PileLongGauge
    {
        internal AverageObjectSizeBytes(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Number of bytes for average object";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_BYTES; } }

        protected override Datum MakeAggregateInstance() { return new AverageObjectSizeBytes(this.Source, 0); }
    }


    /// <summary>
    /// Count of Put() calls
    /// </summary>
    [Serializable]
    public class PutCount : PileLongGauge
    {
        internal PutCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Count of Put() calls";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new PutCount(this.Source, 0); }
    }

    /// <summary>
    /// Count of Delete() calls
    /// </summary>
    [Serializable]
    public class DeleteCount : PileLongGauge
    {
        internal DeleteCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Count of Delete() calls";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new DeleteCount(this.Source, 0); }
    }


    /// <summary>
    /// Count of Get() calls
    /// </summary>
    [Serializable]
    public class GetCount : PileLongGauge
    {
        internal GetCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Count of Get() calls";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new GetCount(this.Source, 0); }
    }

    /// <summary>
    /// Number of free clots(chunks) in the free list
    /// </summary>
    [Serializable]
    public class FreeListCapacity : PileLongGauge
    {
        internal FreeListCapacity(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Number of free clots(chunks) in the free list";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_SLOT; } }

        protected override Datum MakeAggregateInstance() { return new FreeListCapacity(this.Source, 0); }
    }




}
