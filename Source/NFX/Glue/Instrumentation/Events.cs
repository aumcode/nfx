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
using NFX.ApplicationModel;
using NFX.Instrumentation;

namespace NFX.Glue.Instrumentation
{
    [Serializable]
    public abstract class GlueEvent : Event
    {
      protected GlueEvent(string src) : base(src) {}
    }

    [Serializable]
    public abstract class ServerEvent : GlueEvent
    {
      protected ServerEvent(string src) : base(src) {}
    }

    [Serializable]
    public abstract class ClientEvent : GlueEvent
    {
      protected ClientEvent(string src) : base(src) {}
    }

    [Serializable]
    public abstract class ServerTransportErrorEvent : ServerEvent, IErrorInstrument, INetInstrument
    {
      protected ServerTransportErrorEvent(string src) : base(src) {}
    }

    [Serializable]
    public abstract class ClientTransportErrorEvent : ClientEvent, IErrorInstrument, INetInstrument
    {
      protected ClientTransportErrorEvent(string src) : base(src) {}
    }

    [Serializable]
    public class ServerDeserializationErrorEvent : ServerTransportErrorEvent
    {
        protected ServerDeserializationErrorEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ServerDeserializationErrorEvent(node.ToString()));
        }

        public override string Description { get{ return "Server-side errors while deserializing messages"; }}


        protected override Datum MakeAggregateInstance()
        {
            return new ServerDeserializationErrorEvent(this.Source);
        }
    }


    [Serializable]
    public class ClientDeserializationErrorEvent : ClientTransportErrorEvent
    {
       protected ClientDeserializationErrorEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ClientDeserializationErrorEvent(node.ToString()));
        }

       public override string Description { get{ return "Client-side errors while deserializing messages"; }}

       protected override Datum MakeAggregateInstance()
       {
            return new ClientDeserializationErrorEvent(this.Source);
       }
    }


    [Serializable]
    public class ServerGotOverMaxMsgSizeErrorEvent : ServerTransportErrorEvent
    {
        protected ServerGotOverMaxMsgSizeErrorEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ServerGotOverMaxMsgSizeErrorEvent(node.ToString()));
        }

        public override string Description { get{ return "Server-side errors getting messages with sizes over limit"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new ServerGotOverMaxMsgSizeErrorEvent(this.Source);
        }
    }

    [Serializable]
    public class ClientGotOverMaxMsgSizeErrorEvent : ClientTransportErrorEvent
    {
        protected ClientGotOverMaxMsgSizeErrorEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ClientGotOverMaxMsgSizeErrorEvent(node.ToString()));
        }


        public override string Description { get{ return "Client-side errors getting messages with sizes over limit"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new ClientGotOverMaxMsgSizeErrorEvent(this.Source);
        }
    }


    [Serializable]
    public class ServerSerializedOverMaxMsgSizeErrorEvent : ServerTransportErrorEvent
    {
        protected ServerSerializedOverMaxMsgSizeErrorEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ServerSerializedOverMaxMsgSizeErrorEvent(node.ToString()));
        }


        public override string Description { get{ return "Server-side errors serializing messages with sizes over limit"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new ServerSerializedOverMaxMsgSizeErrorEvent(this.Source);
        }
    }

    [Serializable]
    public class ClientSerializedOverMaxMsgSizeErrorEvent : ClientTransportErrorEvent
    {
        protected ClientSerializedOverMaxMsgSizeErrorEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ClientSerializedOverMaxMsgSizeErrorEvent(node.ToString()));
        }

        public override string Description { get{ return "Client-side errors serializing messages with sizes over limit"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new ClientSerializedOverMaxMsgSizeErrorEvent(this.Source);
        }
    }


    [Serializable]
    public class ServerListenerErrorEvent : ServerTransportErrorEvent
    {
        protected ServerListenerErrorEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ServerListenerErrorEvent(node.ToString()));
        }

        public override string Description { get{ return "Server-side listener errors"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new ServerListenerErrorEvent(this.Source);
        }
    }


    [Serializable]
    public class InactiveClientTransportClosedEvent : ClientEvent, INetInstrument
    {
        protected InactiveClientTransportClosedEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new InactiveClientTransportClosedEvent(node.ToString()));
        }

        public override string Description { get{ return "Client closed inactive transport"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new InactiveClientTransportClosedEvent(this.Source);
        }
    }

    [Serializable]
    public class InactiveServerTransportClosedEvent : ServerEvent, INetInstrument
    {
        protected InactiveServerTransportClosedEvent(string src) : base(src) {}

        public static void Happened(Node node)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new InactiveServerTransportClosedEvent(node.ToString()));
        }

        public override string Description { get{ return "Server closed inactive transport"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new InactiveServerTransportClosedEvent(this.Source);
        }
    }



    [Serializable]
    public class CallSlotNotFoundErrorEvent : ClientEvent, IErrorInstrument, INetInstrument
    {
        protected CallSlotNotFoundErrorEvent() : base(Datum.UNSPECIFIED_SOURCE) {}

        public static void Happened()
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new CallSlotNotFoundErrorEvent());
        }

        public override string Description { get{ return "Client could not find call slot"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new CallSlotNotFoundErrorEvent();
        }
    }



    [Serializable]
    public class ClientConnectedEvent : ServerEvent, INetInstrument
    {
        protected ClientConnectedEvent(string from) : base(from ?? Datum.UNSPECIFIED_SOURCE) {}

        public static void Happened(string from)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ClientConnectedEvent(from));
        }

        public override string Description { get{ return "Client connected to server"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new ClientConnectedEvent(this.Source);
        }
    }

    [Serializable]
    public class ClientDisconnectedEvent : ServerEvent, INetInstrument
    {
        protected ClientDisconnectedEvent(string from) : base(from ?? Datum.UNSPECIFIED_SOURCE) {}

        public static void Happened(string from)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new ClientDisconnectedEvent(from));
        }

        public override string Description { get{ return "Client disconnected from server"; }}

        protected override Datum MakeAggregateInstance()
        {
            return new ClientDisconnectedEvent(this.Source);
        }
    }


}
