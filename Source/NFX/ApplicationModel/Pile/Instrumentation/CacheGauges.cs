using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Instrumentation;

namespace NFX.ApplicationModel.Pile.Instrumentation
{
    /// <summary>
    /// Provides base for cache long gauges
    /// </summary>
    [Serializable]
    public abstract class CacheLongGauge : PileLongGauge
    {
      protected CacheLongGauge(string src, long value) : base(src, value) {}
    }

    /// <summary>
    /// Provides base for cache double gauges
    /// </summary>
    [Serializable]
    public abstract class CacheDoubleGauge : PileDoubleGauge
    {
      protected CacheDoubleGauge(string src, double value) : base(src, value) {}
    }

    /// <summary>
    /// Provides base for cache events
    /// </summary>
    [Serializable]
    public abstract class CacheEvent : Event
    {
      protected CacheEvent(string src) : base(src) {}
    }

    /// <summary>
    /// Provides table count in the cache instance
    /// </summary>
    [Serializable]
    public class CacheTableCount : CacheLongGauge
    {
        internal CacheTableCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides table count in the cache instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TABLE; } }

        protected override Datum MakeAggregateInstance() { return new CacheTableCount(this.Source, 0); }
    }


    /// <summary>
    /// Provides object count in the cache instance
    /// </summary>
    [Serializable]
    public class CacheCount : CacheLongGauge
    {
        internal CacheCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides object count in the cache instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_OBJECT; } }

        protected override Datum MakeAggregateInstance() { return new CacheCount(this.Source, 0); }
    }

    /// <summary>
    /// Provides entry/slot count in the cache instance
    /// </summary>
    [Serializable]
    public class CacheCapacity : CacheLongGauge
    {
        internal CacheCapacity(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides entry/slot count in the cache instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_OBJECT; } }

        protected override Datum MakeAggregateInstance() { return new CacheCapacity(this.Source, 0); }
    }

    /// <summary>
    /// Provides load factor percentage
    /// </summary>
    [Serializable]
    public class CacheLoadFactor : CacheDoubleGauge
    {
        internal CacheLoadFactor(string src, double value) : base(src, value) {}

        public override string Description { get{ return "Provides object count in the cache instance";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_PERCENT; } }

        protected override Datum MakeAggregateInstance() { return new CacheLoadFactor(this.Source, 0d); }
    }

    /// <summary>
    /// How many times put resulted in new object insertion in cache with or without overwriting the existing item
    /// </summary>
    [Serializable]
    public class CachePut : CacheLongGauge
    {
        internal CachePut(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times put resulted in new object insertion in cache with or without overwriting the existing item";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CachePut(this.Source, 0); }
    }

    /// <summary>
    /// How many times put could not insert new object in cache because there was no room and existing data could not be overwritten
    ///  due to higher priority
    /// </summary>
    [Serializable]
    public class CachePutCollision : CacheLongGauge
    {
        internal CachePutCollision(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times put could not insert new object in cache because there was no room and existing data could not be overwritten due to higher priority";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CachePutCollision(this.Source, 0); }
    }

    /// <summary>
    /// How many times put inserted new object in cache by overwriting existing value with lower priority
    /// </summary>
    [Serializable]
    public class CachePutOverwrite : CacheLongGauge
    {
        internal CachePutOverwrite(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times put inserted new object in cache by overwriting existing value with lower priority";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CachePutOverwrite(this.Source, 0); }
    }

    /// <summary>
    /// How many times put replaced existing object in cache 
    /// </summary>
    [Serializable]
    public class CachePutReplace : CacheLongGauge
    {
        internal CachePutReplace(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times put replaced existing object in cache";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CachePutReplace(this.Source, 0); }
    }

    /// <summary>
    /// How many times key was found and object removed
    /// </summary>
    [Serializable]
    public class CacheRemoveHit : CacheLongGauge
    {
        internal CacheRemoveHit(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times key was found and object removed";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheRemoveHit(this.Source, 0); }
    }

    /// <summary>
    /// How many times key was not found and object not removed
    /// </summary>
    [Serializable]
    public class CacheRemoveMiss : CacheLongGauge
    {
        internal CacheRemoveMiss(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return " How many times key was not found and object not removed";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheRemoveMiss(this.Source, 0); }
    }


    /// <summary>
    /// How many entries/objects were removed by sweep
    /// </summary>
    [Serializable]
    public class CacheSweep : CacheLongGauge
    {
        internal CacheSweep(string src, long value) : base(src, value) {}

        public override string Description{  get{ return " How many slots/objects were removed by sweep";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_OBJECT; } }

        protected override Datum MakeAggregateInstance() { return new CacheSweep(this.Source, 0); }
    }

    /// <summary>
    /// How long the sweeping took (examination + removal of expired) 
    /// </summary>
    [Serializable]
    public class CacheSweepDuration : CacheLongGauge
    {
        internal CacheSweepDuration(string src, long value) : base(src, value) {}

        public override string Description{  get{ return "How long the sweeping took (examination + removal of expired)";} }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_MSEC; } }

        protected override Datum MakeAggregateInstance() { return new CacheSweepDuration(this.Source, 0); }
    }


    /// <summary>
    /// Cache table was swept
    /// </summary>
    [Serializable]
    public class CacheTableSwept : CacheEvent
    {
       protected CacheTableSwept(string src) : base(src) {}
        
        public static void Happened(string tableName)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new CacheTableSwept(tableName)); 
        }

       public override string Description { get{ return "Cache table was swept"; }}

       protected override Datum MakeAggregateInstance()
       {
            return new CacheTableSwept(this.Source); 
       }
    }


    /// <summary>
    /// How many times key entry was found and its age reset to zero
    /// </summary>
    [Serializable]
    public class CacheRejuvenateHit : CacheLongGauge
    {
        internal CacheRejuvenateHit(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times key entry was found and its age reset to zero";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheRejuvenateHit(this.Source, 0); }
    }

    /// <summary>
    /// How many times key entry was not found for resetting its age
    /// </summary>
    [Serializable]
    public class CacheRejuvenateMiss : CacheLongGauge
    {
        internal CacheRejuvenateMiss(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times key entry was not found for resetting its age";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheRejuvenateMiss(this.Source, 0); }
    }


    /// <summary>
    /// How many times cached object was found and gotten by its key
    /// </summary>
    [Serializable]
    public class CacheGetHit : CacheLongGauge
    {
        internal CacheGetHit(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times cached object was found and gotten by its key";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheGetHit(this.Source, 0); }
    }

    /// <summary>
    /// How many times cached object was tried to be gotten but not found by its key
    /// </summary>
    [Serializable]
    public class CacheGetMiss : CacheLongGauge
    {
        internal CacheGetMiss(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times cached object was tried to be gotten but not found by its key";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheGetMiss(this.Source, 0); }
    }


    /// <summary>
    /// How many times cache has to increase its capacity
    /// </summary>
    [Serializable]
    public class CacheGrew : CacheLongGauge
    {
        internal CacheGrew(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times cache has to increase its capacity";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheGrew(this.Source, 0); }
    }

    /// <summary>
    /// How many times cache has to decrease its capacity
    /// </summary>
    [Serializable]
    public class CacheShrunk : CacheLongGauge
    {
        internal CacheShrunk(string src, long value) : base(src, value) {}

        public override string Description 
        {
         get{ return "How many times cache has to decrease its capacity";} 
        }

        public override string ValueUnitName  { get { return CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new CacheShrunk(this.Source, 0); }
    }



}
