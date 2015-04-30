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
