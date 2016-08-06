using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.CRUD;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Provides read/write-through extensions
  /// </summary>
  public static class CacheExtensions
  {
    /// <summary>
    /// Fetches an existing item from cache or null
    /// </summary>
    public static TResult FetchFrom<TKey, TResult>(this ICache cache, TKey key, string tblCache, ICacheParams caching) where TResult : class
    {
      if (caching==null) caching = CacheParams.DefaultCache;

      TResult result = null;

      if (caching.ReadCacheMaxAgeSec>=0)
      {
        ICacheTable<TKey> tbl = cache.GetOrCreateTable<TKey>(tblCache);
        var cached = tbl.Get(key, caching.ReadCacheMaxAgeSec);
        if (!(cached is AbsentValue))
          result = cached as TResult;
      }

      return result;
    }

    /// <summary>
    /// Fetches an item through cache
    /// </summary>
    public static TResult FetchThrough<TKey, TResult>(this ICache cache, TKey key, string tblCache, ICacheParams caching, Func<TKey, TResult> fFetch, Func<TKey, TResult, TResult> fFilter = null) where TResult : class
    {
      ICacheTable<TKey> tbl = null;

      if (caching==null) caching = CacheParams.DefaultCache;

      if (caching.ReadCacheMaxAgeSec>=0)
        tbl = cache.GetOrCreateTable<TKey>(tblCache);

      TResult result = null;

      if (tbl!=null)
      {
        var cached = tbl.Get(key, caching.ReadCacheMaxAgeSec);
        if (cached is AbsentValue)
          return null;
        else
          result = cached as TResult;

        if (fFilter != null)
          result = fFilter(key, result);
      }

      if (result!=null) return result;

      result = fFetch(key);

      if (result==null && !caching.CacheAbsentData) return null;

      var wAge = caching.WriteCacheMaxAgeSec;
      if (tbl!=null && wAge>=0)
        tbl.Put(key, (object)result ?? AbsentValue.Instance, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);

      return result;
    }

    public static bool DeleteThrough<TKey>(this ICache cache, TKey key, string tblCache, ICacheParams caching, Func<TKey, bool> fDelete)
    {
      if (caching==null) caching = CacheParams.DefaultCache;

      var wAge = caching.WriteCacheMaxAgeSec;
      var tbl = cache.GetOrCreateTable<TKey>(tblCache);

      if (caching.CacheAbsentData && wAge>=0)
        tbl.Put(key, AbsentValue.Instance, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);
      else
        tbl.Remove(key);

      return fDelete(key);
    }

    public static int SaveThrough<TKey, TData>(this ICache cache, TKey key, TData data, string tblCache, ICacheParams caching, Func<TKey, TData, int> fSave) where TData : class
    {
      if (data==null) return 0;

      if (caching==null) caching = CacheParams.DefaultCache;

      var result = fSave(key, data);

      var wAge = caching.WriteCacheMaxAgeSec;
      if (wAge>=0)
      {
        var tbl = cache.GetOrCreateTable<TKey>(tblCache);
        tbl.Put(key, data, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);
      }

      return result;
    }

  }
}
