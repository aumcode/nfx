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

namespace NFX.Instrumentation
{
    /// <summary>
    /// Denotes interface types that participate in instrumentation data grouping.
    /// Records that implement several classification interfaces which are decorated by this attribute
    /// get listed in each decorated interface/group
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited=false)]
    public sealed class InstrumentViewGroup : Attribute { public InstrumentViewGroup(){}}




    /// <summary>
    /// Root marker interface for instrumentation data classification.
    /// Instruments are primarily classified by their derivation from Datum ancestor, however this interface-based
    ///  scheme allows for alternate classification (a la multiple inheritance)
    /// </summary>
    public interface IInstrumentClass  { }

        /// <summary>
        /// A class of problem that decorate instrument represents
        /// </summary>
        public interface IProblemClass : IInstrumentClass { }

            /// <summary>
            /// Data of this class indicates some abnormality in operation, i.e. "LowDisk"
            /// </summary>
            [InstrumentViewGroup]
            public interface IWarningInstrument : IProblemClass { }


            /// <summary>
            /// Data of this class indicates a definite abnormality, however the system is going to continue functioning
            /// </summary>
            [InstrumentViewGroup]
            public interface IErrorInstrument : IProblemClass {  }

            /// <summary>
            /// Data of this class indicates an abnormality that will most likely lead to system inability to continue functioning as expected
            /// </summary>
            [InstrumentViewGroup]
            public interface ICatastropyInstrument : IProblemClass {  }




        /// <summary>
        /// A class of operations that instrument measures
        /// </summary>
        public interface IOperationClass : IInstrumentClass { }

            /// <summary>
            /// IO-related operations
            /// </summary>
            public interface IIOInstrumentClass : IOperationClass { }

                /// <summary>
                /// Disk operations (i.e. % drive free)
                /// </summary>
                [InstrumentViewGroup]
                public interface IDiskInstrument : IIOInstrumentClass { }

                /// <summary>
                /// Network operations (i.e. sockets)
                /// </summary>
                [InstrumentViewGroup]
                public interface INetInstrument : IIOInstrumentClass { }

            /// <summary>
            /// Instruments that measure instrumentation itself
            /// </summary>
            [InstrumentViewGroup]
            public interface IInstrumentationInstrument : IOperationClass { }


            /// <summary>
            /// CPU-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface ICPUInstrument : IOperationClass { }

            /// <summary>
            /// Memory-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface IMemoryInstrument : IOperationClass { }


            /// <summary>
            /// Cache-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface ICacheInstrument : IOperationClass { }


            /// <summary>
            /// Locking/coordination-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface ILockingInstrument : IOperationClass { }

            /// <summary>
            /// DB-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface IDatabaseInstrument : IOperationClass { }


            /// <summary>
            /// Web-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface IWebInstrument : IOperationClass { }

            /// <summary>
            /// Security-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface ISecurityInstrument : IOperationClass { }


            /// <summary>
            /// GDID-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface IGDIDInstrument : IOperationClass { }

            /// <summary>
            /// Timer/scheduling-related operations
            /// </summary>
            [InstrumentViewGroup]
            public interface ISchedulingInstrument : IOperationClass { }




            /// <summary>
            /// A class of operations related to business logic that instrument measures
            /// </summary>
            public interface IBusinessLogic : IOperationClass { }

                /// <summary>
                /// A class of operations related to financial transactions / business logic
                /// </summary>
                [InstrumentViewGroup]
                public interface IFinancialLogic : IBusinessLogic { }


                /// <summary>
                /// A class of operations related to social operations / business logic
                /// </summary>
                [InstrumentViewGroup]
                public interface ISocialLogic : IBusinessLogic { }



}
