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

namespace NFX.DataAccess.Cache.Instrumentation
{
    /// <summary>
    /// Provides base for cache long gauges
    /// </summary>
    [Serializable]
    public abstract class CacheLongGauge : LongGauge, ICacheInstrument, IMemoryInstrument
    {
      protected CacheLongGauge(string src, long value) : base(src, value) {}
    }

    /// <summary>
    /// Provides base for cache double gauges
    /// </summary>
    [Serializable]
    public abstract class CacheDoubleGauge : DoubleGauge, ICacheInstrument, IMemoryInstrument
    {
      protected CacheDoubleGauge(string src, double value) : base(src, value) {}
    }

    /// <summary>
    /// Provides record count in the instance
    /// </summary>
    [Serializable]
    public class RecordCount : CacheLongGauge
    {
        internal RecordCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides record count in the instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_RECORD; } }

        protected override Datum MakeAggregateInstance() { return new RecordCount(this.Source, 0); }
    }


    /// <summary>
    /// Provides page count in the instance
    /// </summary>
    [Serializable]
    public class PageCount : CacheLongGauge
    {
        internal PageCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides page count in the instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_PAGE; } }

        protected override Datum MakeAggregateInstance() { return new PageCount(this.Source, 0); }
    }


    /// <summary>
    /// Provides the ratio of how many buckets are loaded with pages vs. bucket count
    /// </summary>
    [Serializable]
    public class BucketPageLoadFactor : CacheDoubleGauge
    {
        internal BucketPageLoadFactor(string src, double value) : base(src, value) {}

        public override string Description { get{ return "Provides the ratio of how many buckets are loaded with pages vs. bucket count";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_PAGE_PER_BUCKET; } }

        protected override Datum MakeAggregateInstance() { return new BucketPageLoadFactor(this.Source, 0); }
    }


    /// <summary>
    /// How many times Get() resulted in cache hit
    /// </summary>
    [Serializable]
    public class HitCount : CacheLongGauge
    {
        internal HitCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times Get() resulted in cache hit";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new HitCount(this.Source, 0); }
    }

    /// <summary>
    /// How many times Get() resulted in cache miss
    /// </summary>
    [Serializable]
    public class MissCount : CacheLongGauge
    {
        internal MissCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times Get() resulted in cache miss";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new MissCount(this.Source, 0); }
    }

    /// <summary>
    /// How many times factory func was called from GetOrPut()
    /// </summary>
    [Serializable]
    public class ValueFactoryCount : CacheLongGauge
    {
        internal ValueFactoryCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times factory func was called from GetOrPut()";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_CALL; } }

        protected override Datum MakeAggregateInstance() { return new ValueFactoryCount(this.Source, 0); }
    }


    /// <summary>
    /// How many times tables were swept
    /// </summary>
    [Serializable]
    public class SweepTableCount : CacheLongGauge
    {
        internal SweepTableCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times tables were swept";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_SWEEP; } }

        protected override Datum MakeAggregateInstance() { return new SweepTableCount(this.Source, 0); }
    }

    /// <summary>
    /// How many pages swept
    /// </summary>
    [Serializable]
    public class SweepPageCount : CacheLongGauge
    {
        internal SweepPageCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many pages swept";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_SWEEP; } }

        protected override Datum MakeAggregateInstance() { return new SweepPageCount(this.Source, 0); }
    }

    /// <summary>
    /// How many records removed by sweep
    /// </summary>
    [Serializable]
    public class SweepRemoveCount : CacheLongGauge
    {
        internal SweepRemoveCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many records removed by sweep";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_RECORD; } }

        protected override Datum MakeAggregateInstance() { return new SweepRemoveCount(this.Source, 0); }
    }

    /// <summary>
    /// How many times Put() was called
    /// </summary>
    [Serializable]
    public class PutCount : CacheLongGauge
    {
        internal PutCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times Put() was called";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new PutCount(this.Source, 0); }
    }

    /// <summary>
    /// How many times a call to Put() resulted in insert
    /// </summary>
    [Serializable]
    public class PutInsertCount : CacheLongGauge
    {
        internal PutInsertCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times a call to Put() resulted in insert";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new PutInsertCount(this.Source, 0); }
    }

    /// <summary>
    /// How many times a call to Put() resulted in relacement of existing item by key without collision
    /// </summary>
    [Serializable]
    public class PutReplaceCount : CacheLongGauge
    {
        internal PutReplaceCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times a call to Put() resulted in relacement of existing item by key without collision";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new PutReplaceCount(this.Source, 0); }
    }

    /// <summary>
    /// How many times a call to Put() resulted in bucket collision that created a page
    /// </summary>
    [Serializable]
    public class PutPageCreateCount : CacheLongGauge
    {
        internal PutPageCreateCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times a call to Put() resulted in bucket collision that created a page";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_PAGE; } }

        protected override Datum MakeAggregateInstance() { return new PutPageCreateCount(this.Source, 0); }
    }

    /// <summary>
    /// How many times a call to Put() resulted in new value overriding existing because of collision (old value lost)
    /// </summary>
    [Serializable]
    public class PutCollisionCount : CacheLongGauge
    {
        internal PutCollisionCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times a call to Put() resulted in new value overriding existing because of collision (old value lost)";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new PutCollisionCount(this.Source, 0); }
    }


    /// <summary>
    /// How many times a call to Put() could have resulted in new value overriding existing one because of collision, however the situation was prevented
    /// because existing item had higher priority than the newer one
    /// </summary>
    [Serializable]
    public class PutPriorityPreventedCollisionCount : CacheLongGauge
    {
        internal PutPriorityPreventedCollisionCount(string src, long value) : base(src, value) {}

        public override string Description
        {
         get{ return
          "How many times a call to Put() could have resulted in new value overriding existing one because of collision, "+
          "however the situation was prevented because existing item had higher priority than the newer one"; }
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new PutPriorityPreventedCollisionCount(this.Source, 0); }
    }

    /// <summary>
    /// How many pages have been deleted, a page gets deleted when there are no records stored in it
    /// </summary>
    [Serializable]
    public class RemovePageCount : CacheLongGauge
    {
        internal RemovePageCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many pages have been deleted, a page gets deleted when there are no records stored in it";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_PAGE; } }

        protected override Datum MakeAggregateInstance() { return new RemovePageCount(this.Source, 0); }
    }

    /// <summary>
    /// How many records have been found and removed
    /// </summary>
    [Serializable]
    public class RemoveHitCount : CacheLongGauge
    {
        internal RemoveHitCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many records have been found and removed";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_RECORD; } }

        protected override Datum MakeAggregateInstance() { return new RemoveHitCount(this.Source, 0); }
    }

    /// <summary>
    /// How many records have been sought to be removed but were not found
    /// </summary>
    [Serializable]
    public class RemoveMissCount : CacheLongGauge
    {
        internal RemoveMissCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many records have been sought to be removed but were not found";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new RemoveMissCount(this.Source, 0); }
    }




}
