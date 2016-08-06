using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.Instrumentation;
using NFX.DataAccess.Distributed;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Represents a cache of expiring objects, which are identified by a key and stored in a pile.
  /// Pile allows to store hundreds of millions of objects without overloading the managed GC.
  /// The cache may be local or distributed
  /// </summary>
  public interface ICache : IApplicationComponent
  {
    /// <summary>
    /// Returns whether the cache key:value mappings are local or distributed
    /// </summary>
    LocalityKind Locality { get;}

    /// <summary>
    /// Returns the model of key:value mapping persistence that this cache supports
    /// </summary>
    ObjectPersistence Persistence { get;}

    /// <summary>
    /// Returns the status of the pile where object are stored
    /// </summary>
    IPileStatus PileStatus { get;}


    /// <summary>
    /// Tables that this cache contains
    /// </summary>
    IRegistry<ICacheTable> Tables { get; }

    /// <summary>
    /// Returns existing table by name, if it does not exist creates a new table.
    /// For existing table the types must be identical to the ones used at creation
    /// </summary>
    ICacheTable<TKey> GetOrCreateTable<TKey>(string tableName, IEqualityComparer<TKey> keyComparer = null);

    /// <summary>
    /// Returns existing table by name, if it does not exist creates a new table.
    /// For existing table the types must be identical to the ones used at creation
    /// </summary>
    ICacheTable<TKey> GetOrCreateTable<TKey>(string tableName, out bool createdNew, IEqualityComparer<TKey> keyComparer = null);

    /// <summary>
    /// Returns existing table by name, if it does not exist thorws.
    /// The TKey must correspond to existing table
    /// </summary>
    ICacheTable<TKey> GetTable<TKey>(string tableName);


    /// <summary>
    /// Returns how many records are kept in cache
    /// </summary>
    long Count{get;}


    /// <summary>
    /// Removes all data from all tables stored in the cache
    /// </summary>
    void PurgeAll();
  }

  public interface ICacheTable : INamed
  {
    /// <summary>
    /// References cache that this table is under
    /// </summary>
    ICache Cache{ get;}

    /// <summary>
    /// Returns how many records are kept in a table
    /// </summary>
    long Count{get;}

    /// <summary>
    /// Returns how many slots/entries allocated in a table
    /// </summary>
    long Capacity{get;}

    /// <summary>
    /// Returns the percentage of occupied table slots.
    /// When this number exceeds high-water-mark threshold the table is grown,
    /// otheriwse if the number falls below the low-water-mark level then the table is shrunk
    /// </summary>
    double LoadFactor{ get;}

    /// <summary>
    /// Cache table options in effect
    /// </summary>
    TableOptions Options { get;}


    /// <summary>
    /// Removes all data from table
    /// </summary>
    void Purge();
  }

  /// <summary>
  /// Denotes statuses of cache table Put
  /// </summary>
  public enum PutResult
  {
    /// <summary>
    /// The item could not be put because it collides with existing data
    /// that can not be overwritten because it has higher priority and there is no extra space
    /// </summary>
    Collision = 0,

    /// <summary>
    /// The item was inserted into cache table anew
    /// </summary>
    Inserted,

    /// <summary>
    /// The item replaced an existing item with the same key
    /// </summary>
    Replaced,

    /// <summary>
    /// The item was inserted instead of an existing item with lower or equal priority
    /// </summary>
    Overwritten
  }

  public interface ICacheTable<TKey> : ICacheTable
  {
     /// <summary>
    /// Returns equality comparer for keys, or null to use default Equals
    /// </summary>
    IEqualityComparer<TKey> KeyComparer{ get ;}



    /// <summary>
    /// Returns true if cache has object with the key, optionally filtering-out objects older than ageSec param if it is &gt; zero.
    /// Returns false if there is no object with the specified key or it is older than ageSec limit.
    /// </summary>
    bool ContainsKey(TKey key, int ageSec = 0);


    /// <summary>
    /// Returns the size of stored object if cache has object with the key, optionally filtering-out objects older than ageSec param if it is &gt; zero.
    /// </summary>
    long SizeOfValue(TKey key, int ageSec = 0);

    /// <summary>
    /// Gets cache object by key, optionally filtering-out objects older than ageSec param if it is &gt; zero.
    /// Returns null if there is no object with the specified key or it is older than ageSec limit.
    /// </summary>
    object Get(TKey key, int ageSec = 0);

    /// <summary>
    /// Puts an object identified by a key into cache returning the result of the put.
    /// For example, put may have added nothing if the table is capped and the space is occupied with data of higher priority
    /// </summary>
    /// <param name="key">A table-wide unique obvject key</param>
    /// <param name="obj">An object to put</param>
    /// <param name="maxAgeSec">If null then the default maxAgeSec is taken from Options property, otherwise specifies the length of items life in seconds</param>
    /// <param name="priority">The priority of this item. If there is no space in future the items with lower priorities will not evict existing data with highr priorities</param>
    /// <param name="absoluteExpirationUTC">Optional UTC timestamp of object eviction from cache</param>
    /// <returns>The status of put - whether item was inserted/replaced(if key exists)/overwritten or collided with higher-prioritized existing data</returns>
    PutResult Put(TKey key, object obj, int? maxAgeSec = null, int priority = 0, DateTime? absoluteExpirationUTC = null);


    /// <summary>
    /// Removes data by key returning true if found and removed
    /// </summary>
    bool Remove(TKey key);

    /// <summary>
    /// Resets inetrnal object age returning true of object was found and rejuvenated
    /// </summary>
    bool Rejuvenate(TKey key);


    /// <summary>
    /// Atomically tries to get object by key if it exists, otherwise calls a factory method under lock and puts the data with the specified parameters.
    /// 'newPutResult' returns the result of the put after factory method call.
    /// Keep in mind, that even if a factory method created a new object, there may be a case when the value
    /// could not be physically inserted in the cache because of a collision (data with higher priority occupies space and space is capped), so check for
    /// 'newPutResult' value which is null in case of getting an existing item.
    /// Returns object that was gotten or created anew
    /// </summary>
    object GetOrPut(TKey key,
                  Func<ICacheTable<TKey>, TKey, object, object> valueFactory,
                  object factoryContext,
                  out PutResult? newPutResult,
                  int ageSec = 0,
                  int putMaxAgeSec = 0,
                  int putPriority = 0,
                  DateTime? putAbsoluteExpirationUTC = null);
  }

  public interface ICacheImplementation : ICache, ServiceModel.IService, IInstrumentable
  {
    /// <summary>
    /// Imposes a limit on maximum number of bytes that a pile can allocate of the system heap.
    /// The default value of 0 means no limit, meaning - the pile will keep allocating objects
    /// until the system allows. If the limit is reached, then the cache will start deleting
    /// older objects to releave the memory load even if they are not due for expiration yet
    /// </summary>
    long PileMaxMemoryLimit { get; set;}

    /// <summary>
    /// Defines modes of allocation: space/time tradeoff
    /// </summary>
    AllocationMode PileAllocMode{ get; set;}


    /// <summary>
    /// Returns table options - used for table creation
    /// </summary>
    Registry<TableOptions> TableOptions { get; }

    /// <summary>
    /// Sets default options for a table which is not found in TableOptions collection.
    /// If this property is null then every table assumes the set of constant values defined in Table class
    /// </summary>
     TableOptions DefaultTableOptions { get; set;}
  }


}
