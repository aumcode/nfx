/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using NFX.Serialization.BSON;

namespace NFX.ApplicationModel.Pile.Instrumentation
{
  /// <summary>
  /// Provides base for cache long gauges
  /// </summary>
  [Serializable]
  public abstract class CacheLongGauge : PileLongGauge
  {
    protected CacheLongGauge(string src, long value) : base(src, value) { }
  }

  /// <summary>
  /// Provides base for cache double gauges
  /// </summary>
  [Serializable]
  public abstract class CacheDoubleGauge : PileDoubleGauge
  {
    protected CacheDoubleGauge(string src, double value) : base(src, value) { }
  }

  /// <summary>
  /// Provides base for cache events
  /// </summary>
  [Serializable]
  public abstract class CacheEvent : Event
  {
    protected CacheEvent(string src) : base(src) { }
  }

  /// <summary>
  /// Provides table count in the cache instance
  /// </summary>
  [Serializable]
  [BSONSerializable("1C1E2248-0665-425B-8788-33A604BDB9C8")]
  public class CacheTableCount : CacheLongGauge
  {
    internal CacheTableCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides table count in the cache instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TABLE; } }

    protected override Datum MakeAggregateInstance() { return new CacheTableCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides object count in the cache instance
  /// </summary>
  [Serializable]
  [BSONSerializable("70E4120E-6EC8-46C0-B60D-59D11F4ED903")]
  public class CacheCount : CacheLongGauge
  {
    internal CacheCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides object count in the cache instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_OBJECT; } }

    protected override Datum MakeAggregateInstance() { return new CacheCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides entry/slot count in the cache instance
  /// </summary>
  [Serializable]
  [BSONSerializable("CEE6A3C2-B876-4171-8B10-A70456D2F95A")]
  public class CacheCapacity : CacheLongGauge
  {
    internal CacheCapacity(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides entry/slot count in the cache instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_OBJECT; } }

    protected override Datum MakeAggregateInstance() { return new CacheCapacity(this.Source, 0); }
  }

  /// <summary>
  /// Provides load factor percentage
  /// </summary>
  [Serializable]
  [BSONSerializable("8FF1C20B-3A23-4CE7-92F6-116C366FB59D")]
  public class CacheLoadFactor : CacheDoubleGauge
  {
    internal CacheLoadFactor(string src, double value) : base(src, value) { }

    public override string Description { get { return "Provides object count in the cache instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PERCENT; } }

    protected override Datum MakeAggregateInstance() { return new CacheLoadFactor(this.Source, 0d); }
  }

  /// <summary>
  /// How many times put resulted in new object insertion in cache with or without overwriting the existing item
  /// </summary>
  [Serializable]
  [BSONSerializable("F98541C5-9C2C-487D-8B2F-B8916CA914D3")]
  public class CachePut : CacheLongGauge
  {
    internal CachePut(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times put resulted in new object insertion in cache with or without overwriting the existing item"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CachePut(this.Source, 0); }
  }

  /// <summary>
  /// How many times put could not insert new object in cache because there was no room and existing data could not be overwritten
  ///  due to higher priority
  /// </summary>
  [Serializable]
  [BSONSerializable("5541FEF0-FCB2-4C6E-9FCB-1BC7FE00441C")]
  public class CachePutCollision : CacheLongGauge
  {
    internal CachePutCollision(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times put could not insert new object in cache because there was no room and existing data could not be overwritten due to higher priority"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CachePutCollision(this.Source, 0); }
  }

  /// <summary>
  /// How many times put inserted new object in cache by overwriting existing value with lower priority
  /// </summary>
  [Serializable]
  [BSONSerializable("85B3755A-3C0D-4079-8E8C-01B10A3F0613")]
  public class CachePutOverwrite : CacheLongGauge
  {
    internal CachePutOverwrite(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times put inserted new object in cache by overwriting existing value with lower priority"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CachePutOverwrite(this.Source, 0); }
  }

  /// <summary>
  /// How many times put replaced existing object in cache
  /// </summary>
  [Serializable]
  [BSONSerializable("E73361E0-DCD3-4557-A44B-E89F5EB9733A")]
  public class CachePutReplace : CacheLongGauge
  {
    internal CachePutReplace(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times put replaced existing object in cache"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CachePutReplace(this.Source, 0); }
  }

  /// <summary>
  /// How many times key was found and object removed
  /// </summary>
  [Serializable]
  [BSONSerializable("17A730D2-E409-4DDB-8979-E8DE913D72E2")]
  public class CacheRemoveHit : CacheLongGauge
  {
    internal CacheRemoveHit(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times key was found and object removed"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheRemoveHit(this.Source, 0); }
  }

  /// <summary>
  /// How many times key was not found and object not removed
  /// </summary>
  [Serializable]
  [BSONSerializable("88DE0E76-475C-41DE-961F-E735737B1F00")]
  public class CacheRemoveMiss : CacheLongGauge
  {
    internal CacheRemoveMiss(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return " How many times key was not found and object not removed"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheRemoveMiss(this.Source, 0); }
  }


  /// <summary>
  /// How many entries/objects were removed by sweep
  /// </summary>
  [Serializable]
  [BSONSerializable("DCDEE4A4-BE23-4B2E-BE9C-9D3F970ECDC9")]
  public class CacheSweep : CacheLongGauge
  {
    internal CacheSweep(string src, long value) : base(src, value) { }

    public override string Description { get { return " How many slots/objects were removed by sweep"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_OBJECT; } }

    protected override Datum MakeAggregateInstance() { return new CacheSweep(this.Source, 0); }
  }

  /// <summary>
  /// How long the sweeping took (examination + removal of expired)
  /// </summary>
  [Serializable]
  [BSONSerializable("FD0C832E-0FC5-46A3-BBA3-5689E8004A34")]
  public class CacheSweepDuration : CacheLongGauge
  {
    internal CacheSweepDuration(string src, long value) : base(src, value) { }

    public override string Description { get { return "How long the sweeping took (examination + removal of expired)"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MSEC; } }

    protected override Datum MakeAggregateInstance() { return new CacheSweepDuration(this.Source, 0); }
  }


  /// <summary>
  /// Cache table was swept
  /// </summary>
  [Serializable]
  [BSONSerializable("6360EF29-C14D-4CCD-81A1-433F3F533A0B")]
  public class CacheTableSwept : CacheEvent
  {
    protected CacheTableSwept(string src) : base(src) { }

    public static void Happened(string tableName)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new CacheTableSwept(tableName));
    }

    public override string Description { get { return "Cache table was swept"; } }

    protected override Datum MakeAggregateInstance() { return new CacheTableSwept(this.Source); }
  }


  /// <summary>
  /// How many times key entry was found and its age reset to zero
  /// </summary>
  [Serializable]
  [BSONSerializable("A322FCA5-C07C-499D-AF9C-96EC7E7857A6")]
  public class CacheRejuvenateHit : CacheLongGauge
  {
    internal CacheRejuvenateHit(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times key entry was found and its age reset to zero"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheRejuvenateHit(this.Source, 0); }
  }

  /// <summary>
  /// How many times key entry was not found for resetting its age
  /// </summary>
  [Serializable]
  [BSONSerializable("19BC6E0B-AFC8-4EE5-A077-1F5C414C5035")]
  public class CacheRejuvenateMiss : CacheLongGauge
  {
    internal CacheRejuvenateMiss(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times key entry was not found for resetting its age"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheRejuvenateMiss(this.Source, 0); }
  }


  /// <summary>
  /// How many times cached object was found and gotten by its key
  /// </summary>
  [Serializable]
  [BSONSerializable("87AFAACD-6526-4E54-811C-4AC616FEE408")]
  public class CacheGetHit : CacheLongGauge
  {
    internal CacheGetHit(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times cached object was found and gotten by its key"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheGetHit(this.Source, 0); }
  }

  /// <summary>
  /// How many times cached object was tried to be gotten but not found by its key
  /// </summary>
  [Serializable]
  [BSONSerializable("24D12137-96AD-4428-B8B8-A6B58EE10EE9")]
  public class CacheGetMiss : CacheLongGauge
  {
    internal CacheGetMiss(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times cached object was tried to be gotten but not found by its key"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheGetMiss(this.Source, 0); }
  }


  /// <summary>
  /// How many times cache has to increase its capacity
  /// </summary>
  [Serializable]
  [BSONSerializable("EB07D59D-67FB-4AF8-8311-4299F66B5749")]
  public class CacheGrew : CacheLongGauge
  {
    internal CacheGrew(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times cache has to increase its capacity"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheGrew(this.Source, 0); }
  }

  /// <summary>
  /// How many times cache has to decrease its capacity
  /// </summary>
  [Serializable]
  [BSONSerializable("F9474DA5-9E5F-463E-AAD3-8F9A2629B2BD")]
  public class CacheShrunk : CacheLongGauge
  {
    internal CacheShrunk(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "How many times cache has to decrease its capacity"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new CacheShrunk(this.Source, 0); }
  }
}
