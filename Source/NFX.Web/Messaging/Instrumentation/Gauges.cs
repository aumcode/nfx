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

using NFX.Instrumentation;

namespace NFX.Web.Messaging.Instrumentation
{
    [Serializable]
    public abstract class MessagingSinkLongGauge : LongGauge, IWebInstrument
    {
      protected MessagingSinkLongGauge(string source, long value) : base(source, value) { }
    }

    [Serializable]
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
    public class MessagingSinkErrorCount : MessagingSinkLongGauge
    {
      protected MessagingSinkErrorCount(string source, long value) : base(source, value) { }

      public static void Record(string source, long value)
      {
        var inst = App.Instrumentation;
        if (inst.Enabled)
          inst.Record(new MessagingSinkErrorCount(source, value));
      }

      public override string Description { get { return "Messsages error count"; } }
      public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

      protected override Datum MakeAggregateInstance()
      {
        return new MessagingSinkErrorCount(this.Source, 0);
      }
    }
}
