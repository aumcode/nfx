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
using NFX.Financial;
using NFX.Serialization.BSON;

namespace NFX.Web.Pay.Instrumentation
{
  [Serializable]
  public abstract class PayLongGauge : LongGauge, IFinancialLogic, IWebInstrument
  {
    protected PayLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  public abstract class PayAmountGauge : AmountGauge, IWebInstrument
  {
    protected PayAmountGauge(string src, Amount value) : base(src, value) { }
  }

  #region Charge
  [Serializable]
  [BSONSerializable("6F40A9E0-2CCD-48C4-B61E-596D64EE755C")]
  public class ChargeCount : PayLongGauge
  {
    protected ChargeCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ChargeCount(source, value));
    }


    public override string Description { get { return "Charge count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ChargeCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("E06AF526-A4DB-46A4-939B-7565DE1C3140")]
  public class ChargeErrorCount : PayLongGauge
  {
    protected ChargeErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ChargeErrorCount(source, value));
    }


    public override string Description { get { return "Charge error count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ChargeErrorCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("92E92250-D659-4131-8080-1B397824EE07")]
  public class ChargeAmount : PayAmountGauge
  {
    protected ChargeAmount(string source, Amount value) : base(source, value) { }

    public static void Record(string source, Amount value)
    {
      var instr = App.Instrumentation;
      if (instr.Enabled)
        instr.Record(new ChargeAmount(source, value));
    }

    public override string Description { get { return "Capture amount"; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ChargeAmount(this.Source, new Amount(this.Value.CurrencyISO, 0m));
    }
  }
  #endregion

  #region Capture
  [Serializable]
  [BSONSerializable("3ED404F3-B083-4BCF-A70C-1C82E89657D5")]
  public class CaptureCount : PayLongGauge
  {
    protected CaptureCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new CaptureCount(source, value));
    }


    public override string Description { get { return "Capture count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance()
    {
      return new CaptureCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("3126CDD0-9CA3-41E9-BEB5-091B40B4141F")]
  public class CaptureErrorCount : PayLongGauge
  {
    protected CaptureErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new CaptureErrorCount(source, value));
    }


    public override string Description { get { return "Capture error count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new CaptureErrorCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("B36FFFFF-32C0-4863-9C8C-F61886665E14")]
  public class CaptureAmount : PayAmountGauge
  {
    protected CaptureAmount(string source, Amount value) : base(source, value) { }

    public static void Record(string source, Amount value)
    {
      var instr = App.Instrumentation;
      if (instr.Enabled)
        instr.Record(new CaptureAmount(source, value));
    }

    public override string Description { get { return "Capture amount"; } }

    protected override Datum MakeAggregateInstance()
    {
      return new CaptureAmount(this.Source, new Amount(this.Value.CurrencyISO, 0m));
    }
  }
  #endregion

  #region Refund
  [Serializable]
  [BSONSerializable("AF8ED1E7-074D-4B8B-985A-24299320ADDC")]
  public class RefundCount : PayLongGauge
  {
    protected RefundCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new RefundCount(source, value));
    }


    public override string Description { get { return "Refund count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance()
    {
      return new RefundCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("C97AE435-A503-4A4C-ABE1-6A94211F271C")]
  public class RefundErrorCount : PayLongGauge
  {
    protected RefundErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var instr = App.Instrumentation;
      if (instr.Enabled)
        instr.Record(new RefundErrorCount(source, value));
    }

    public override string Description { get { return "Refund error count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new RefundErrorCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("16A7C68E-5D6C-42D2-AA92-56BD8D521630")]
  public class RefundAmount : PayAmountGauge
  {
    protected RefundAmount(string source, Amount value) : base(source, value) { }

    public static void Record(string source, Amount value)
    {
      var instr = App.Instrumentation;
      if (instr.Enabled)
        instr.Record(new RefundAmount(source, value));
    }

    public override string Description { get { return "Refund amount"; } }

    protected override Datum MakeAggregateInstance()
    {
      return new RefundAmount(this.Source, new Amount(this.Value.CurrencyISO, 0m));
    }
  }
  #endregion

  #region Transfer
  [Serializable]
  [BSONSerializable("139AC796-93D0-4472-AACC-5017FE22D710")]
  public class TransferCount : PayLongGauge
  {
    protected TransferCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new TransferCount(source, value));
    }


    public override string Description { get { return "Transfer total count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance()
    {
      return new TransferCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("CB033FF0-77B3-4E8D-896C-3F0D4A8DCE80")]
  public class TransferErrorCount : PayLongGauge
  {
    protected TransferErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var instr = App.Instrumentation;
      if (instr.Enabled)
        instr.Record(new TransferErrorCount(source, value));
    }

    public override string Description { get { return "Transfer total error count"; } }
    public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new TransferErrorCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("0581E6E0-658E-4970-BF38-8DF4BD7FD284")]
  public class TransferAmount : PayAmountGauge
  {
    protected TransferAmount(string source, Amount value) : base(source, value) { }

    public static void Record(string source, Amount value)
    {
      var instr = App.Instrumentation;
      if (instr.Enabled)
        instr.Record(new TransferAmount(source, value));
    }

    public override string Description { get { return "Transfer total amount"; } }

    protected override Datum MakeAggregateInstance()
    {
      return new TransferAmount(this.Source, new Amount(Value.CurrencyISO, 0m));
    }
  }
  #endregion
}
