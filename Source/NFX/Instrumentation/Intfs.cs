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

using NFX.Time;
using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Instrumentation
{
    /// <summary>
    /// Stipulates instrumentation contract
    /// </summary>
    public interface IInstrumentation : IApplicationComponent, ILocalizedTimeProvider
    {

      /// <summary>
      /// Indicates whether instrumentation is enabled
      /// </summary>
      bool Enabled { get;}

      /// <summary>
      /// Returns true to indicate that instrumentation does not have any space left to record more data at the present moment
      /// </summary>
      bool Overflown { get;}

      /// <summary>
      /// Returns current record count in the instance
      /// </summary>
      int RecordCount { get;}

      /// <summary>
      /// Gets/Sets the maximum record count that this instance can store
      /// </summary>
      int MaxRecordCount{ get; }

      /// <summary>
      /// Specifies how often aggregation is performed
      /// </summary>
      int ProcessingIntervalMS { get; }

      /// <summary>
      /// Specifies how often OS instrumentation such as CPU and RAM is sampled.
      /// Value of zero disables OS sampling
      /// </summary>
      int OSInstrumentationIntervalMS { get; }

      /// <summary>
      /// When true, outputs instrumentation data about the self (how many datum buffers, etc.)
      /// </summary>
      bool SelfInstrumented{ get;}

      /// <summary>
      /// Returns the size of the ring buffer where result (aggregated) instrumentation records are kept in memory.
      /// The maximum buffer capacity is returned, not how many results have been buffered so far.
      ///  If this property is less than or equal to zero then result buffering in memory is disabled
      /// </summary>
      int ResultBufferSize { get; }


      /// <summary>
      /// Enumerates distinct types of Datum ever recorded in the instance. This property may be used to build
      ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED
      /// </summary>
      IEnumerable<Type> DataTypes { get;}

      /// <summary>
      /// Enumerates sources per Datum type ever recorded by the instance. This property may be used to build
      ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED.
      ///  Returns default instance so caller may get default description/unit name
      /// </summary>
      IEnumerable<string> GetDatumTypeSources(Type datumType, out Datum defaultInstance);


      /// <summary>
      /// Records instrumentation datum
      /// </summary>
      void Record(Datum datum);

      /// <summary>
      /// Returns the specified number of samples from the ring result buffer in the near-chronological order,
      /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
      ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
      /// The enumeration is empty if ResultBufferSize is less or equal to zero entries.
      /// If count is less or equal to zero then the system returns all results available.
      /// </summary>
      IEnumerable<Datum> GetBufferedResults(int count=0);

      /// <summary>
      /// Returns samples starting around the the specified UTCdate in the near-chronological order,
      /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
      ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
      /// The enumeration is empty if ResultBufferSize is less or equal to zero entries
      /// </summary>
      IEnumerable<Datum> GetBufferedResultsSince(DateTime utcStart);
    }


    /// <summary>
    /// Stipulates instrumentation contract
    /// </summary>
    public interface IInstrumentationImplementation : IInstrumentation, IDisposable, IConfigurable, IInstrumentable
    {

    }


    /// <summary>
    /// Denotes an entity that can be instrumented
    /// </summary>
    public interface IInstrumentable : IExternallyParameterized
    {
      /// <summary>
      /// Turns on/off instrumentation
      /// </summary>
      bool InstrumentationEnabled { get; set;}
    }


}
