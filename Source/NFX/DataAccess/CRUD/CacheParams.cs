using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD
{
  /// <summary>
  /// Implements ICacheParams - supplied caching parameters
  /// </summary>
  public struct CacheParams : ICacheParams
  {
    /// <summary>
    /// CacheParams that dont use cache
    /// </summary>
    public static CacheParams NoCache
    {
      get { return new CacheParams{ ReadCacheMaxAgeSec = -1, WriteCacheMaxAgeSec = -1, WriteCachePriority = 0}; }
    }

    /// <summary>
    /// CacheParams that use cache's default
    /// </summary>
    public static CacheParams DefaultCache
    {
      get { return new CacheParams{ ReadCacheMaxAgeSec = 0, WriteCacheMaxAgeSec = 0, WriteCachePriority = 0, CacheAbsentData = true}; }
    }

    /// <summary>
    /// Cache params with the same read/write interval
    /// </summary>
    public static CacheParams ReadWriteSec(int sec, int priority = 0)
    {
      return new CacheParams{ ReadCacheMaxAgeSec = sec, WriteCacheMaxAgeSec = sec, WriteCachePriority = priority, CacheAbsentData = true};
    }

    /// <summary>
    /// Cache params with no read but write caching interval and priority
    /// </summary>
    public static CacheParams ReadFreshWriteSec(int sec, int priority = 0)
    {
      return new CacheParams{ ReadCacheMaxAgeSec = -1, WriteCacheMaxAgeSec = sec, WriteCachePriority = priority, CacheAbsentData = true};
    }


    /// <summary>
    /// If greater than 0 then would allow reading a cached result for up-to the specified number of seconds.
    /// If =0 uses cache's default span.
    /// Less than 0 does not try to read from cache
    /// </summary>
    public int ReadCacheMaxAgeSec
    {
      get; set;
    }

    /// <summary>
    /// If greater than 0 then writes to cache with the expiration.
    /// If =0 uses cache's default life span.
    /// Less than 0 does not write to cache
    /// </summary>
    public int WriteCacheMaxAgeSec
    {
      get; set;
    }

    /// <summary>
    /// Relative cache priority which is used when WriteCacheMaxAgeSec>=0
    /// </summary>
    public int WriteCachePriority
    {
      get; set;
    }

    /// <summary>
    /// When true would cache the instance of AbsentData to signify the absence of data in the backend for key
    /// </summary>
    public bool CacheAbsentData
    {
      get; set;
    }
  }
}
