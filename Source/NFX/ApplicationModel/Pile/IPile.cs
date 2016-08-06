using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.Instrumentation;


namespace NFX.ApplicationModel.Pile
{

  /// <summary>
  /// Provides information about the pile - number of objects, allocated bytes, etc.
  /// </summary>
  public interface IPileStatus
  {

    /// <summary>
    /// Returns whether pile is local or distributed
    /// </summary>
    LocalityKind Locality { get;}

    /// <summary>
    /// Returns the model of object persistence that this pile supports
    /// </summary>
    ObjectPersistence Persistence { get;}

    /// <summary>
    /// Returns whether this instance supports object expiration
    /// </summary>
    bool SupportsObjectExpiration{ get;}

    /// <summary>
    /// Returns the number of allocated objects in this pile
    /// </summary>
    long ObjectCount{ get;}

    /// <summary>
    /// Returns the number of bytes allocated by this pile from system memory
    /// </summary>
    long AllocatedMemoryBytes{ get;}

    /// <summary>
    /// Returns an approximate capacity of free memory that the system has left
    /// </summary>
    long MemoryCapacityBytes{ get;}

    /// <summary>
    /// Returns the number of bytes allocated for object storage within AllocatedMemoryBytes
    /// </summary>
    long UtilizedBytes{ get;}

    /// <summary>
    /// Returns the number of extra bytes used by pile metadata currently occupied by object stored in this pile
    /// </summary>
    long OverheadBytes { get; }

    /// <summary>
    /// Returns the number of segments allocated
    /// </summary>
    long SegmentCount { get;}

    /// <summary>
    /// Returns the number of nodes(servers) that service this distributed pile.
    /// If this pile locality is local then returns 1
    /// </summary>
    int NodeCount { get;}
  }


  /// <summary>
  /// Represents a pile of objects - a custom memory heap that can store native CLR objects in a tightly-serialized form.
  /// Piles can be either local (allocate local RAM on the server), or distributed (allocate RAM on many servers).
  /// This class is designed primarily for applications that need to store/cache very many (100s of millions on local, billions on distributed) of objects in RAM
  /// (and/or possibly on disk) without causing the local CLR's GC scans of huge object graphs.
  /// Implementors of this interface are custom memory managers that favor the GC performance in apps with many objects at the cost of higher CPU usage.
  /// The implementor must be thread-safe for all operations unless stated otherwise on a member level.
  /// The memory represented by this class as a whole is not synchronizable, that is - it does not support functions like Interlocked-family, Lock, MemoryBarriers and the like
  ///  that regular RAM supports. Should a need arise to interlock within the pile - a custom CLR-based lock must be used to syncronize access to pile as a whole, for example:
  ///   a Get does not impose a lock on ALL concurrent writes throught the pile (a write does not block all gets either).
  /// </summary>
  public interface IPile : IPileStatus, IApplicationComponent
  {
    /// <summary>
    /// Puts a CLR object into the pile and returns a newly-allocated pointer.
    /// Throws out-of-space if there is not enough space in the pile and limits are set.
    /// Optional lifeSpanSec will auto-delete object after the interval elapses if
    ///  the pile SupportsObjectExpiration and SweepExpireObjects is set to true
    /// </summary>
    PilePointer Put(object obj, uint lifeSpanSec = 0);

    /// <summary>
    /// Returns a CLR object by its pointer or throws access violation if pointer is invalid
    /// </summary>
    object Get(PilePointer ptr);

    /// <summary>
    /// Returns a raw byte[] occupied by the object payload, only payload is returned along with serializer flag
    /// which tells what kind of serializer was used.
    /// This method is rarely used, it is needed for debugging and special-case "direct" memory access on read
    /// to bypass the de-serialization process altogether
    /// </summary>
    byte[] GetRawBuffer(PilePointer ptr, out byte serializerFlag);

    /// <summary>
    /// Deletes object from pile by its pointer returning true if there is no access violation
    /// and pointer is pointing to the valid object, throws otherwise unless
    /// throwInvalid is set to false
    /// </summary>
    bool Delete(PilePointer ptr, bool throwInvalid = true);

    /// <summary>
    /// If pile supports expiration, resets object age to zero.
    /// Returns true if object was found and reset. N/A for pile that do not support expiration
    /// </summary>
    bool Rejuvenate(PilePointer ptr);

    /// <summary>
    /// Returns the size of pointed-to object in bytes or throws access violation if pointer is invalid.
    /// The serialized object size is returned, not the CLR object size.
    /// </summary>
    int SizeOf(PilePointer ptr);


    /// <summary>
    /// Deletes all objects freeing all segment memory buffers.
    /// This method may require the caller to have special rights
    /// </summary>
    void Purge();

    /// <summary>
    /// Tries to delete extra capacity which is allocated but not currently needed.
    /// Returns the number of bytes freed back to the system
    /// </summary>
    long Compact();
  }


  public interface IPileImplementation : IPile, ServiceModel.IService, IInstrumentable
  {
    /// <summary>
    /// Defines modes of allocation: space/time tradeoff
    /// </summary>
    AllocationMode AllocMode{ get; set;}


    /// <summary>
    /// Control whether the instance respects object life spans
    /// if they are set
    /// </summary>
    bool SweepExpiredObjects{ get; set;}

    /// <summary>
    /// Imposes a limit on maximum number of bytes that a pile can allocate of the system heap.
    /// The default value of 0 means no limit, meaning - the pile will keep allocating objects
    /// until the system allows
    /// </summary>
    long MaxMemoryLimit { get; set;}



    /// <summary>
    /// Gets the maximum count of segments that this pile can have.
    /// The property is not thread-safe for set and can only be set if pile is inactive.
    /// The value of zero means no limit
    /// </summary>
    int MaxSegmentLimit { get; set;}

    /// <summary>
    /// Gets the segment size in bytes, up to (2^31)-1
    /// The property is not thread-safe for set and can only be set if pile is inactive
    /// </summary>
    int SegmentSize { get; set;}
  }






}
