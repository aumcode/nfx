/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX.Instrumentation;
using NFX.Serialization.BSON;

namespace NFX.Web.Messaging.Instrumentation
{
  [Serializable]
  public abstract class MessagingSinkLongGauge : LongGauge, IWebInstrument
  {
    protected MessagingSinkLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  [BSONSerializable("AF384279-1916-4A74-B4EC-E426435D8E48")]
  public class MessagingSinkCount : MessagingSinkLongGauge
  {
    public MessagingSinkCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new MessagingSinkCount(source, value));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingSinkCount(this.Source, 0);
    }

    public override string Description { get { return "Messages count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_MESSAGE; } }
  }

  [Serializable]
  [BSONSerializable("352086BB-36DB-4B27-9F85-41933FB8BD6E")]
  public class MessagingFallbackCount : MessagingSinkLongGauge
  {
    public MessagingFallbackCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new MessagingFallbackCount(source, value));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingFallbackCount(this.Source, 0);
    }

    public override string Description { get { return "Fallbacks count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_MESSAGE; } }
  }

  [Serializable]
  [BSONSerializable("FDED631F-1C0D-4C2D-8F86-3400D203CA7F")]
  public class MessagingSinkErrorCount : MessagingSinkLongGauge
  {
    protected MessagingSinkErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new MessagingSinkErrorCount(source, value));
    }

    public override string Description { get { return "Messages error count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingSinkErrorCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("D277AA37-EAD1-45F0-8FB4-6A96D0E74CD7")]
  public class MessagingFallbackErrorCount : MessagingSinkLongGauge
  {
    protected MessagingFallbackErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new MessagingFallbackErrorCount(source, value));
    }

    public override string Description { get { return "Fallbacks error count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingFallbackErrorCount(this.Source, 0);
    }
  }
}
