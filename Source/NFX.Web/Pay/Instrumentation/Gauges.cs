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
using NFX.Financial;

namespace NFX.Web.Pay.Tax.Instrumentation
{
  [Serializable]
  public abstract class TaxLongGauge : LongGauge, IFinancialLogic, IWebInstrument
  {
    protected TaxLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  public abstract class TaxAmountGauge : AmountGauge, IWebInstrument
  {
    protected TaxAmountGauge(string src, Amount value) : base(src, value) { }
  }

  #region Charge

    [Serializable]
    public class TaxCaclulationCount : TaxLongGauge
    {
      protected TaxCaclulationCount(string source, long value) : base(source, value) { }

      public static void Record(string source, long value)
      {
        var inst = App.Instrumentation;
        if (inst.Enabled)
          inst.Record(new TaxCaclulationCount(source, value));
      }


      public override string Description { get { return "Tax calculation count"; } }
      public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_TIME; } }

      protected override Datum MakeAggregateInstance()
      {
        return new TaxCaclulationCount(this.Source, 0);
      }
    }

    [Serializable]
    public class TaxCaclulcationErrorCount : TaxLongGauge
    {
      protected TaxCaclulcationErrorCount(string source, long value) : base(source, value) { }

      public static void Record(string source, long value)
      {
        var inst = App.Instrumentation;
        if (inst.Enabled)
          inst.Record(new TaxCaclulcationErrorCount(source, value));
      }


      public override string Description { get { return "Tax caluclation error count"; } }
      public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

      protected override Datum MakeAggregateInstance()
      {
        return new TaxCaclulcationErrorCount(this.Source, 0);
      }
    }

    [Serializable]
    public class RetailerTaxAmount : TaxAmountGauge
    {
      protected RetailerTaxAmount(string source, Amount value): base(source, value) { }

      public static void Record(string source, Amount value)
      {
        var instr = App.Instrumentation;
        if (instr.Enabled)
          instr.Record(new RetailerTaxAmount(source, value));
      }

      public override string Description { get { return "Retailer tax amount"; } }

      protected override Datum MakeAggregateInstance()
      { 
        return new RetailerTaxAmount(this.Source, new Amount(this.Value.CurrencyISO, 0m)); 
      }
    }

    [Serializable]
    public class WholesellerTaxAmount : TaxAmountGauge
    {
      protected WholesellerTaxAmount(string source, Amount value): base(source, value) { }

      public static void Record(string source, Amount value)
      {
        var instr = App.Instrumentation;
        if (instr.Enabled)
          instr.Record(new WholesellerTaxAmount(source, value));
      }

      public override string Description { get { return "Wholeseller tax amount"; } }

      protected override Datum MakeAggregateInstance()
      { 
        return new WholesellerTaxAmount(this.Source, new Amount(this.Value.CurrencyISO, 0m)); 
      }
    }

  #endregion
}
