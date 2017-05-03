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
using NFX.Instrumentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Shipping.Instrumentation
{
  [Serializable]
  public abstract class ShippingLongGauge : LongGauge, IFinancialLogic, IWebInstrument
  {
    protected ShippingLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  public class LabelCount : ShippingLongGauge
  {
    protected LabelCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new LabelCount(source, value));
    }


    public override string Description { get { return "Labels count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance()
    {
      return new LabelCount(this.Source, 0);
    }
  }

  [Serializable]
  public class LabelErrorCount : ShippingLongGauge
  {
    protected LabelErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new LabelErrorCount(source, value));
    }


    public override string Description { get { return "Label creation error count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new LabelErrorCount(this.Source, 0);
    }
  }
}
