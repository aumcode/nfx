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

namespace NFX.Wave.Instrumentation
{
    /// <summary>
    /// Provides base for Wave long gauges
    /// </summary>
    [Serializable]
    public abstract class WaveLongGauge : LongGauge, IWebInstrument
    {
      protected WaveLongGauge(string src, long value) : base(src, value) {}
    }

    /// <summary>
    /// Provides base for Wave double gauges
    /// </summary>
    [Serializable]
    public abstract class WaveDoubleGauge : DoubleGauge, IWebInstrument
    {
      protected WaveDoubleGauge(string src, double value) : base(src, value) {}
    }

    /// <summary>
    /// Provides request count received by server
    /// </summary>
    [Serializable]
    public class ServerRequest : WaveLongGauge, INetInstrument
    {
        internal ServerRequest(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides request count received by server";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_REQUEST; } }

        protected override Datum MakeAggregateInstance() { return new ServerRequest(this.Source, 0); }
    }

    /// <summary>
    /// Provides request count received by server portal
    /// </summary>
    [Serializable]
    public class ServerPortalRequest : WaveLongGauge, INetInstrument
    {
        internal ServerPortalRequest(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides request count received by server portal";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_REQUEST; } }

        protected override Datum MakeAggregateInstance() { return new ServerPortalRequest(this.Source, 0); }
    }


    /// <summary>
    /// Provides request count that were denied by the server gate
    /// </summary>
    [Serializable]
    public class ServerGateDenial : WaveLongGauge, INetInstrument, ISecurityInstrument, IWarningInstrument
    {
        internal ServerGateDenial(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides request count that were denied by the server gate";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_REQUEST; } }

        protected override Datum MakeAggregateInstance() { return new ServerGateDenial(this.Source, 0); }
    }


    /// <summary>
    /// Provides the count of exceptions that server had to handle as no other memebr of processing chain did
    /// </summary>
    [Serializable]
    public class ServerHandleException : WaveLongGauge, IErrorInstrument
    {
        internal ServerHandleException(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides the count of exceptions that server had to handle as no other memebr of processing chain did";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_EXCEPTION; } }

        protected override Datum MakeAggregateInstance() { return new ServerHandleException(this.Source, 0); }
    }


    /// <summary>
    /// Provides the count of exceptions that filter handled
    /// </summary>
    [Serializable]
    public class FilterHandleException : WaveLongGauge, IErrorInstrument
    {
        internal FilterHandleException(string src, long value) : base(src, value) {}

        public override string Description { get{ return "Provides the count of exceptions that filter handled";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_EXCEPTION; } }

        protected override Datum MakeAggregateInstance() { return new FilterHandleException(this.Source, 0); }
    }


    /// <summary>
    /// The current level of taken request accept semaphore slots
    /// </summary>
    [Serializable]
    public class ServerAcceptSemaphoreCount : WaveLongGauge, INetInstrument, ICPUInstrument
    {
        internal ServerAcceptSemaphoreCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "The current level of taken request accept semaphore slots";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_SLOT; } }

        protected override Datum MakeAggregateInstance() { return new ServerAcceptSemaphoreCount(this.Source, 0); }
    }

    /// <summary>
    /// The current level of taken request work semaphore slots
    /// </summary>
    [Serializable]
    public class ServerWorkSemaphoreCount : WaveLongGauge, INetInstrument, ICPUInstrument
    {
        internal ServerWorkSemaphoreCount(string src, long value) : base(src, value) {}

        public override string Description { get{ return "The current level of taken request work semaphore slots";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_SLOT; } }

        protected override Datum MakeAggregateInstance() { return new ServerWorkSemaphoreCount(this.Source, 0); }
    }


    /// <summary>
    /// How many response object were written into
    /// </summary>
    [Serializable]
    public class WorkContextWrittenResponse : WaveLongGauge, INetInstrument
    {
        internal WorkContextWrittenResponse(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many response object were written into";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_RESPONSE; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextWrittenResponse(this.Source, 0); }
    }

    /// <summary>
    /// How many response object were buffered
    /// </summary>
    [Serializable]
    public class WorkContextBufferedResponse : WaveLongGauge, INetInstrument, IMemoryInstrument
    {
        internal WorkContextBufferedResponse(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many response object were buffered";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_RESPONSE; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextBufferedResponse(this.Source, 0); }
    }


    /// <summary>
    /// How many response bytes were buffered
    /// </summary>
    [Serializable]
    public class WorkContextBufferedResponseBytes : WaveLongGauge, INetInstrument, IMemoryInstrument
    {
        internal WorkContextBufferedResponseBytes(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many response bytes were buffered";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_BYTE; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextBufferedResponseBytes(this.Source, 0); }
    }


    /// <summary>
    /// How many work contexts were created
    /// </summary>
    [Serializable]
    public class WorkContextCtor : WaveLongGauge
    {
        internal WorkContextCtor(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many work contexts were created";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextCtor(this.Source, 0); }
    }

    /// <summary>
    /// How many work contexts were destroyed
    /// </summary>
    [Serializable]
    public class WorkContextDctor : WaveLongGauge
    {
        internal WorkContextDctor(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many work contexts were destroyed";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextDctor(this.Source, 0); }
    }

    /// <summary>
    /// How many times work semaphore was released
    /// </summary>
    [Serializable]
    public class WorkContextWorkSemaphoreRelease : WaveLongGauge,  INetInstrument, ICPUInstrument
    {
        internal WorkContextWorkSemaphoreRelease(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many times work semaphore was released";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_TIME; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextWorkSemaphoreRelease(this.Source, 0); }
    }

    /// <summary>
    /// How many work contexts got aborted
    /// </summary>
    [Serializable]
    public class WorkContextAborted : WaveLongGauge
    {
        internal WorkContextAborted(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many work contexts got aborted";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextAborted(this.Source, 0); }
    }

    /// <summary>
    /// How many work contexts got handled
    /// </summary>
    [Serializable]
    public class WorkContextHandled : WaveLongGauge
    {
        internal WorkContextHandled(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many work contexts got handled";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextHandled(this.Source, 0); }
    }

    /// <summary>
    /// How many work contexts requested not to be closed at the end of the initial request processing cycle
    /// </summary>
    [Serializable]
    public class WorkContextNoDefaultClose : WaveLongGauge, INetInstrument
    {
        internal WorkContextNoDefaultClose(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many work contexts requested not to be closed at the end of the initial request processing cycle";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextNoDefaultClose(this.Source, 0); }
    }


    /// <summary>
    /// How many work contexts requested session state
    /// </summary>
    [Serializable]
    public class WorkContextNeedsSession : WaveLongGauge
    {
        internal WorkContextNeedsSession(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many work contexts requested session state";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

        protected override Datum MakeAggregateInstance() { return new WorkContextNeedsSession(this.Source, 0); }
    }

    /// <summary>
    /// How many new sessions created
    /// </summary>
    [Serializable]
    public class SessionNew : WaveLongGauge
    {
        internal SessionNew(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many new sessions created";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_SESSION; } }

        protected override Datum MakeAggregateInstance() { return new SessionNew(this.Source, 0); }
    }

    /// <summary>
    /// How many existing sessions found
    /// </summary>
    [Serializable]
    public class SessionExisting : WaveLongGauge
    {
        internal SessionExisting(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many existing sessions found";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_SESSION; } }

        protected override Datum MakeAggregateInstance() { return new SessionExisting(this.Source, 0); }
    }

    /// <summary>
    /// How many sessions ended by request
    /// </summary>
    [Serializable]
    public class SessionEnd : WaveLongGauge
    {
        internal SessionEnd(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many sessions ended by request";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_SESSION; } }

        protected override Datum MakeAggregateInstance() { return new SessionEnd(this.Source, 0); }
    }


    /// <summary>
    /// How many sessions supplied invalid identifier (by client)
    /// </summary>
    [Serializable]
    public class SessionInvalidID : WaveLongGauge
    {
        internal SessionInvalidID(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many sessions supplied invalid identifier (by client)";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_SESSION; } }

        protected override Datum MakeAggregateInstance() { return new SessionInvalidID(this.Source, 0); }
    }


    /// <summary>
    /// How many geo lookups have been requested
    /// </summary>
    [Serializable]
    public class GeoLookup : WaveLongGauge
    {
        internal GeoLookup(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many geo lookups have been requested";} }

        public override string ValueUnitName  { get { return NFX.CoreConsts.UNIT_NAME_GEO_LOOKUP; } }

        protected override Datum MakeAggregateInstance() { return new GeoLookup(this.Source, 0); }
    }


    /// <summary>
    /// How many geo lookups have resulted in finding the geo location
    /// </summary>
    [Serializable]
    public class GeoLookupHit : WaveLongGauge
    {
        internal GeoLookupHit(string src, long value) : base(src, value) {}

        public override string Description { get{ return "How many geo lookups have resulted in finding the geo location";} }

        public override string ValueUnitName { get { return NFX.CoreConsts.UNIT_NAME_GEO_LOOKUP_HIT; } }

        protected override Datum MakeAggregateInstance() { return new GeoLookupHit(this.Source, 0); }
    }


}
