/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Threading.Tasks;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Indicates where pile implementation stores data: local vs distributed
  /// </summary>
  public enum LocalityKind
  {
    /// <summary>
    /// The pile resides on this machine and is limited by the RAM on the local server
    /// </summary>
    Local = 0,

    /// <summary>
    /// The pile is distributed - it runs across many machines
    /// </summary>
    Distributed,

    /// <summary>
    /// The pile is distributed - it runs across many machines using Aum Cluster
    /// </summary>
    AumCluster
  }

  /// <summary>
  /// Denotes modes of object persistence
  /// </summary>
  public enum ObjectPersistence
  {
    /// <summary>
    /// The data is kept in memory in a format that prohibits the preservation of data between object layout changes,
    ///  for example, the Slim serializer does not support versioning, hence this mode is beneficial for maximum performance
    ///  of local in-process heaps
    /// </summary>
    Memory = 0,

    /// <summary>
    /// The data is kept in memory in a format that allows to change the object structure (serialization versioning), i.e.
    ///  a distributed node may keep objects usable even after client's software changes
    /// </summary>
    UpgreadableMemory,

    /// <summary>
    /// The data is kept on disk. The data is in object-upgreadable format (support changes of object structure)
    /// </summary>
    Disk,

    /// <summary>
    /// The data is kept on disk and cached in memory (i.e. memory-mapped file).
    /// The data is in object-upgreadable format (support changes of object structure)
    /// </summary>
    MemoryDisk
  }

  /// <summary>
  /// Defines modes of allocation: space/time tradeoff
  /// </summary>
  public enum AllocationMode
  {
    /// <summary>
    /// The pile will try to reuse ram at the cost of possibly slower allocations
    /// </summary>
    ReuseSpace = 0,

    /// <summary>
    /// The pile may use more ram in some cases but allocate faster
    /// </summary>
    FavorSpeed
  }
}
