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
      /// Provides basic information about a named sequence
      /// </summary>
      public interface ISequenceInfo : INamed
      {
        ulong ApproximateCurrentValue{get;}
        int TotalPreallocation{get;}
        int RemainingPreallocation{get;}
        string IssuerName{get;}
        DateTime IssueUTCDate{get;}
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
        /// Generates ID for the supplied sequence name
        /// </summary>
        /// <param name="scopeName">The name of scope where sequences are kept</param>
        /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
        /// <param name="blockSize">If >0 specifies how many sequence values to pre-allocate, otherwise provider would use its default setting</param>
        /// <param name="vicinity">The location on ID counter scale, the issuing authority may disregard this parameter</param>
        /// <returns>The new ULONG sequence value</returns>
        ulong GenerateSequenceID(string scopeName, string sequenceName, int blockSize = 0, ulong? vicinity = ulong.MaxValue);
      }

      /// <summary>
      /// Represents an entity that provides unique Global Distributed IDs (GDIDs) via named sequences
      /// </summary>
      public interface IGDIDProvider : IUniqueSequenceProvider
      {
        /// <summary>
        /// Generates Globally-Unique distributed ID (GDID) for the supplied sequence name
        /// </summary>
        /// <param name="scopeName">The name of scope where sequences are kept</param>
        /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
        /// <param name="blockSize">If >0 specifies how many sequence values to pre-allocate, otherwise provider would use its default setting</param>
        /// <param name="vicinity">The location on ID counter scale, the authority may disregard this parameter</param>
        /// <returns>The GDID instance</returns>
        Distributed.GDID GenerateGDID(string scopeName, string sequenceName, int blockSize = 0, ulong? vicinity = Distributed.GDID.COUNTER_MAX);

        /// <summary>
        /// Gets/sets Authority Glue Node for testing. It can only be set once in the testing app container init before the first call to
        ///  Generate is made. When this setting is set then any cluster authority nodes which would have been normally used will be 
        ///  completely bypassed during block allocation
        /// </summary>
        string TestingAuthorityNode { get; set;}
      }
}
