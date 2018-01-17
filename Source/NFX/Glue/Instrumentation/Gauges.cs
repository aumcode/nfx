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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFX.ApplicationModel;
using NFX.Instrumentation;
using NFX.Serialization.BSON;

namespace NFX.Glue.Instrumentation
{
  [Serializable]
  public abstract class GlueLongGauge : LongGauge
  {
    protected GlueLongGauge(string src, long value) : base(src, value) { }
  }

  [Serializable]
  public abstract class GlueDoubleGauge : DoubleGauge
  {
    protected GlueDoubleGauge(string src, double value) : base(src, value) { }
  }


  [Serializable]
  public abstract class ServerGauge : GlueLongGauge
  {
    protected ServerGauge(string src, long value) : base(src, value) { }
  }

  [Serializable]
  public abstract class ClientGauge : GlueLongGauge
  {
    protected ClientGauge(string src, long value) : base(src, value) { }
  }

  [Serializable]
  public abstract class ClientDoubleGauge : GlueDoubleGauge
  {
    protected ClientDoubleGauge(string src, double value) : base(src, value) { }
  }

  [Serializable]
  [BSONSerializable("C7B8A9D0-ED17-4DCC-9FD6-871E3F994ECF")]
  public class ServerTransportCount : ServerGauge, INetInstrument
  {
    protected ServerTransportCount(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerTransportCount(node.ToString(), value));
    }


    public override string Description { get { return "How many instances of server transport (listeners) are open"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TRANSPORT; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerTransportCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("D9604EFA-DBB6-4C6E-B0D4-E9B24D4B386D")]
  public class ServerTransportChannelCount : ServerGauge, INetInstrument
  {
    protected ServerTransportChannelCount(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerTransportChannelCount(node.ToString(), value));
    }


    public override string Description { get { return "How many instances of server transport channels (i.e. sockets) are open"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_CHANNEL; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerTransportChannelCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("CD8C6236-BC02-4B83-8925-D5C6AE4318AD")]
  public class ServerBytesReceived : ServerGauge, INetInstrument
  {
    protected ServerBytesReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerBytesReceived(node.ToString(), value));
    }


    public override string Description { get { return "How many bytes server received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerBytesReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("2DD3E764-EC46-4A7A-951F-1CB4AC63702A")]
  public class ServerTotalBytesReceived : ServerGauge, INetInstrument
  {
    protected ServerTotalBytesReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerTotalBytesReceived(node.ToString(), value));
    }


    public override string Description { get { return "Total of how many bytes server received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerTotalBytesReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("980D3C93-2807-4542-8951-2D6049771C84")]
  public class ServerBytesSent : ServerGauge, INetInstrument
  {
    protected ServerBytesSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerBytesSent(node.ToString(), value));
    }

    public override string Description { get { return "How many bytes server sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerBytesSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("9686462A-3C58-4320-A59B-6C48FDCA1945")]
  public class ServerTotalBytesSent : ServerGauge, INetInstrument
  {
    protected ServerTotalBytesSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerTotalBytesSent(node.ToString(), value));
    }

    public override string Description { get { return "Total of how many bytes server sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerTotalBytesSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("C2DEA729-C96E-4AC0-8301-5A40A11F51B0")]
  public class ServerMsgReceived : ServerGauge, INetInstrument
  {
    protected ServerMsgReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerMsgReceived(node.ToString(), value));
    }

    public override string Description { get { return "How many messages server received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ServerMsgReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("5B687239-5F41-4E7A-B0A3-A2E5D10CF900")]
  public class ServerTotalMsgReceived : ServerGauge, INetInstrument
  {
    protected ServerTotalMsgReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerTotalMsgReceived(node.ToString(), value));
    }

    public override string Description { get { return "Total of how many messages server received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ServerTotalMsgReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("86BBBD73-6DCB-42D9-913E-AA7012A77CBE")]
  public class ServerMsgSent : ServerGauge, INetInstrument
  {
    protected ServerMsgSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerMsgSent(node.ToString(), value));
    }


    public override string Description { get { return "How many messages server sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ServerMsgSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("FC5FD27D-9128-49FA-AEA2-794F6C4F6BEE")]
  public class ServerTotalMsgSent : ServerGauge, INetInstrument
  {
    protected ServerTotalMsgSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerTotalMsgSent(node.ToString(), value));
    }


    public override string Description { get { return "Total of how many messages server sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ServerTotalMsgSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("F23CB11E-910C-4516-8C14-A099340481D5")]
  public class ServerErrors : ServerGauge, INetInstrument, IErrorInstrument
  {
    protected ServerErrors(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerErrors(node.ToString(), value));
    }

    public override string Description { get { return "How many times server errors happened"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_ERROR; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerErrors(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("BAD91F35-EDD2-4579-8BC6-59D9CF834FA6")]
  public class ServerTotalErrors : ServerGauge, INetInstrument, IErrorInstrument
  {
    protected ServerTotalErrors(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ServerTotalErrors(node.ToString(), value));
    }

    public override string Description { get { return "Total of how many server errors happened"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_ERROR; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ServerTotalErrors(this.Source, 0);
    }
  }

  //----------------------------------------------------------------------------------------

  [Serializable]
  [BSONSerializable("F5C0CF2C-800B-4D71-A7AE-18EFCD91A454")]
  public class ClientTransportCount : ClientGauge, INetInstrument
  {
    protected ClientTransportCount(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientTransportCount(node.ToString(), value));
    }


    public override string Description { get { return "How many instances of client transports are open"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TRANSPORT; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ClientTransportCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("46264004-2A2A-403A-B7D5-C078C5850C23")]
  public class ClientBytesReceived : ClientGauge, INetInstrument
  {
    protected ClientBytesReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientBytesReceived(node.ToString(), value));
    }

    public override string Description { get { return "How many bytes client received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientBytesReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("26612E5E-76B4-49A5-9E68-AEFC988B9888")]
  public class ClientTotalBytesReceived : ClientGauge, INetInstrument
  {
    protected ClientTotalBytesReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientTotalBytesReceived(node.ToString(), value));
    }

    public override string Description { get { return "Total of how many bytes client received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientTotalBytesReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("91F9555D-32AF-4F96-B279-1AC2DD2AC6C6")]
  public class ClientBytesSent : ClientGauge, INetInstrument
  {
    protected ClientBytesSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientBytesSent(node.ToString(), value));
    }


    public override string Description { get { return "How many bytes client sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ClientBytesSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("DFFB5E98-B80E-40AE-9AA9-6EB5AD143638")]
  public class ClientTotalBytesSent : ClientGauge, INetInstrument
  {
    protected ClientTotalBytesSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientTotalBytesSent(node.ToString(), value));
    }


    public override string Description { get { return "Total of how many bytes client sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ClientTotalBytesSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("E9DB37EF-4DE9-458A-B9C4-88E4F3636B1A")]
  public class ClientTimedOutCallSlotsRemoved : ClientGauge, INetInstrument
  {
    protected ClientTimedOutCallSlotsRemoved(long value) : base(null, value) { }

    public static void Record(long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientTimedOutCallSlotsRemoved(value));
    }


    public override string Description { get { return "How many call slots have timed out and got removed by Glue runtime"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SLOT; } }


    protected override Datum MakeAggregateInstance()
    {
      return new ClientTimedOutCallSlotsRemoved(0);
    }
  }

  [Serializable]
  [BSONSerializable("A3FCB8F4-AA23-4854-B7F9-531728D29518")]
  public class ClientMsgReceived : ClientGauge, INetInstrument
  {
    protected ClientMsgReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientMsgReceived(node.ToString(), value));
    }

    public override string Description { get { return "How many messages client received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientMsgReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("5817F3DB-CDB6-45FC-8AD8-CE1D756D6E5C")]
  public class ClientTotalMsgReceived : ClientGauge, INetInstrument
  {
    protected ClientTotalMsgReceived(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientTotalMsgReceived(node.ToString(), value));
    }

    public override string Description { get { return "Total of how many messages client received"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientTotalMsgReceived(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("F8B30E3C-A03A-4522-8B0A-2D47448BF252")]
  public class ClientMsgSent : ClientGauge, INetInstrument
  {
    protected ClientMsgSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientMsgSent(node.ToString(), value));
    }

    public override string Description { get { return "How many messages client sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientMsgSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("3637958D-222B-4527-937C-E6D444344E89")]
  public class ClientTotalMsgSent : ClientGauge, INetInstrument
  {
    protected ClientTotalMsgSent(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientTotalMsgSent(node.ToString(), value));
    }

    public override string Description { get { return "Total of how many messages client sent"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientTotalMsgSent(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("B26AB41A-E4AB-4858-A0C8-32CE9C6B9195")]
  public class ClientErrors : ClientGauge, INetInstrument, IErrorInstrument
  {
    protected ClientErrors(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientErrors(node.ToString(), value));
    }

    public override string Description { get { return "How many times client errors happened"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientErrors(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("EC7EAC3E-CB04-4364-BEDC-D6FE3C83542A")]
  public class ClientTotalErrors : ClientGauge, INetInstrument, IErrorInstrument
  {
    protected ClientTotalErrors(string src, long value) : base(src, value) { }

    public static void Record(Node node, long value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientTotalErrors(node.ToString(), value));
    }

    public override string Description { get { return "Total of how many times client errors happened"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientTotalErrors(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("F4DC23D9-A52C-4C58-9876-11994748029D")]
  public class ClientCallRoundtripTime : ClientDoubleGauge, INetInstrument
  {
    protected ClientCallRoundtripTime(string src, double value) : base(src, value) { }

    public static void Record(string key, double value)
    {
      var inst = ExecutionContext.Application.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ClientCallRoundtripTime(key, value));
    }

    public override string Description { get { return "How long does a two-way last known call take"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MSEC; } }

    protected override Datum MakeAggregateInstance()
    {
      return new ClientCallRoundtripTime(this.Source, 0);
    }
  }
}
