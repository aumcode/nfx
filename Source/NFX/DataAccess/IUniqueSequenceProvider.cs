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

namespace NFX.DataAccess
{

      /// <summary>
      /// Provides basic information about a named sequence.
      /// Warning!!! This class represents informational-only data which CAN NOT be used for real identification
      /// </summary>
      public interface ISequenceInfo : INamed
      {
        /// <summary>
        /// Current Era in which the IDs are generated. This CAN NOT be used to obtain real ID, just the info
        /// </summary>
        uint Era{get;}

        /// <summary>
        /// Approximate current ID. This CAN NOT be used to obtain real ID, just the info
        /// </summary>
        ulong ApproximateCurrentValue{get;}

        /// <summary>
        /// The size of block (if any) that was pre-allocated (instead of generating IDs every time)
        /// </summary>
        int TotalPreallocation{get;}

        /// <summary>
        /// Remaining IDs from the preallocated count
        /// </summary>
        int RemainingPreallocation{get;}

        /// <summary>
        /// The name of issuing authority
        /// </summary>
        string IssuerName{get;}

        /// <summary>
        /// Time stamp of issue
        /// </summary>
        DateTime IssueUTCDate{get;}
      }


      /// <summary>
      /// Represents a starting ID along with the number of consecutive generated IDs of the sequence
      /// </summary>
      public struct ConsecutiveUniqueSequenceIDs
      {
        public ConsecutiveUniqueSequenceIDs(ulong startInclusive, int count)
        {
          this.StartInclusive = startInclusive;
          this.Count = count;
        }

        public readonly ulong StartInclusive;
        public readonly int Count;
      }

      /// <summary>
      /// Represents an entity that provides unique identifiers via named sequences
      /// </summary>
      public interface IUniqueSequenceProvider : INamed
      {

        /// <summary>
        /// Returns the list of all scope names in the instance
        /// </summary>
        IEnumerable<string> SequenceScopeNames { get; }


        /// <summary>
        /// Returns sequnce information enumerable for all sequences in the named scope
        /// </summary>
        IEnumerable<ISequenceInfo> GetSequenceInfos(string scopeName);

        /// <summary>
        /// Generates one ID for the supplied sequence name.
        /// Note: do not confuse with block pre-allocation, which is an internal optimization.
        /// Even if 100 IDs are pre-allocated the method returns one unique ID
        /// </summary>
        /// <param name="scopeName">The name of scope where sequences are kept</param>
        /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
        /// <param name="blockSize">If >0 specifies how many sequence values to pre-allocate, otherwise provider would use its default setting</param>
        /// <param name="vicinity">The location on ID counter scale, the issuing authority may disregard this parameter</param>
        /// <param name="noLWM">
        ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
        ///  This may not be desired in some short-lived processes.
        ///  The provider may disregard this flag
        /// </param>
        /// <returns>The new ULONG sequence value</returns>
        ulong GenerateOneSequenceID(string scopeName,
                                    string sequenceName,
                                    int blockSize = 0,
                                    ulong? vicinity = ulong.MaxValue,
                                    bool noLWM = false);


        /// <summary>
        /// Tries to generate many consecutive IDs. If the reserved block gets exhausted, then the returned ID count may be less than requested.
        /// </summary>
        /// <param name="scopeName">The name of scope where sequences are kept</param>
        /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
        /// <param name="idCount">How many Consecutive IDs should the system try to reserve</param>
        /// <param name="vicinity">The location on ID counter scale, the issuing authority may disregard this parameter</param>
        /// <param name="noLWM">
        ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
        ///  This may not be desired in some short-lived processes.
        ///  The provider may disregard this flag
        /// </param>
        /// <returns>The first uniqueID along with the number of Consecutive IDs that the system could allocate which can be less than requested number of IDs</returns>
        ConsecutiveUniqueSequenceIDs TryGenerateManyConsecutiveSequenceIDs(string scopeName,
                                                                           string sequenceName,
                                                                           int idCount,
                                                                           ulong? vicinity = ulong.MaxValue,
                                                                           bool noLWM = false);
      }

      /// <summary>
      /// Represents an entity that provides unique Global Distributed IDs (GDIDs) via named sequences.
      /// Note: GDID.Zero is never returned as it indicates the absence of a value
      /// </summary>
      public interface IGDIDProvider : IUniqueSequenceProvider
      {
        /// <summary>
        /// Generates Globally-Unique distributed ID (GDID) for the supplied sequence name.
        /// Note: do not confuse with block pre-allocation, which is an internal optimization.
        /// Even if 100 IDs are pre-allocated the method returns one unique GDID
        /// </summary>
        /// <param name="scopeName">The name of scope where sequences are kept</param>
        /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
        /// <param name="blockSize">If >0 specifies how many sequence values to pre-allocate, otherwise provider would use its default setting</param>
        /// <param name="vicinity">The location on ID counter scale, the authority may disregard this parameter</param>
        /// <param name="noLWM">
        ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
        ///  This may not be desired in some short-lived processes.
        ///  The provider may disregard this flag
        /// </param>
        /// <returns>The GDID instance</returns>
        Distributed.GDID GenerateOneGDID(string scopeName,
                                         string sequenceName,
                                         int blockSize = 0,
                                         ulong? vicinity = Distributed.GDID.COUNTER_MAX,
                                         bool noLWM = false);


        /// <summary>
        /// Tries to generate many consecutive Globally-Unique distributed ID (GDID) from the same authority for the supplied sequence name.
        /// If the reserved block gets exhausted, then the returned ID array length may be less than requested
        /// </summary>
        /// <param name="scopeName">The name of scope where sequences are kept</param>
        /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
        /// <param name="gdidCount">How many Consecutive GDIDs from the same authority should the system try to reserve</param>
        /// <param name="vicinity">The location on ID counter scale, the authority may disregard this parameter</param>
        /// <param name="noLWM">
        ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
        ///  This may not be desired in some short-lived processes.
        ///  The provider may disregard this flag
        /// </param>
        /// <returns>The GDID[] instance which may have less elements than requested by gdidCount</returns>
        Distributed.GDID[] TryGenerateManyConsecutiveGDIDs(string scopeName,
                                                           string sequenceName,
                                                           int gdidCount,
                                                           ulong? vicinity = Distributed.GDID.COUNTER_MAX,
                                                           bool noLWM = false);

        /// <summary>
        /// Gets/sets Authority Glue Node for testing. It can only be set once in the testing app container init before the first call to
        ///  Generate is made. When this setting is set then any cluster authority nodes which would have been normally used will be
        ///  completely bypassed during block allocation
        /// </summary>
        string TestingAuthorityNode { get; set;}
      }
}
