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
/*
 * Author: Dmitriy Khmaladze, Spring 2015  dmitriy@itadapter.com
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


using NFX.IO;
using NFX.ServiceModel;
using NFX.Serialization.Slim;
using NFX.Environment;


namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Provides default implementation of IPile which stores objects in the local machine RAM
  /// </summary>
  [SlimSerializationProhibited]
  public sealed class DefaultPile : DefaultPileBase
  {
    #region .ctor

      public DefaultPile(string name = null) : base(name)
      {
      }

      public DefaultPile(object director, string name = null) : base(director, name)
      {
      }

    #endregion

    #region Properties

      /// <summary>
      /// Returns PilePersistence.Memory
      /// </summary>
      public override ObjectPersistence Persistence { get{ return ObjectPersistence.Memory; }}
    #endregion

    #region Protected

      /// <summary>
      /// Creates a segment that stores data in local memory array byte buffers
      /// </summary>
      internal override DefaultPileBase._segment MakeSegment(int segmentNumber)
      {
        var memory = new LocalMemory(SegmentSize);
        var result = new DefaultPileBase._segment(this, memory, true);
        return result;
      }
    #endregion

  }
}
