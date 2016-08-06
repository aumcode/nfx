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

using NFX.DataAccess.Distributed;

namespace NFX.DataAccess.Cache
{
    /// <summary>
    /// Provides hashing strategy for keys that are not scalar uint64.
    /// This is a companion class for CacheStore which only understands uint64 as keys for efficiency purposes.
    /// One may extend this class to implement custom resolution of complex keys into uint64 hashes
    /// and provide collision handling. The base implementation uses object.GetHashCode() with chaining for collision resolution
    /// </summary>
    public class ComplexKeyHashingStrategy
    {
        /// <summary>
        /// How many times hash will be adjusted while chaining
        /// </summary>
        public const int MAX_CACHE_CHAIN_LENGTH = 5;


        public ComplexKeyHashingStrategy(CacheStore store)
        {
          Store = store;
        }


        /// <summary>
        /// References cachestore that this strategy works with
        /// </summary>
        public readonly CacheStore Store;

        /// <summary>
        /// Default implementation that converts complex object to uint64 key.
        /// The default implementation returns the hashcode of the object unless the key is string
        /// in which case it calls DefaultStringKeyToCacheKey() or IULongHashProvider(GDID and parcels).
        /// The function is not deterministic due to the use of .GetHashCode() that may change between release for some objects (depends on object implementation),
        /// so the returned value should not be persisted
        /// </summary>
        public static ulong DefaultComplexKeyToCacheKey(object key)
        {
            if (key==null) return 0;
            if (key is string)
              return DefaultStringKeyToCacheKey((string)key);

            if (key is IDistributedStableHashProvider)
              return ((IDistributedStableHashProvider)key).GetDistributedStableHash();//covers GDID and parcels as well

            return (ulong)key.GetHashCode();
        }

        /// <summary>
        /// Provides default implementation for converting string keys into uint64 hash.
        /// This functions provides the best selectivity for strings that are 8 chars long or less.
        /// The function is NOT deterministic as it uses .GetHashCode(), so the returned value should not be persisted
        /// </summary>
        public static ulong DefaultStringKeyToCacheKey(string key)
        {
            if (key==null) return 0;
            var sl = key.Length;
            if (sl==0) return 0;

            ulong hash1 = 0;
            for(int i=sl-1; i>sl-1-sizeof(ulong) && i>=0; i--)//take 8 chars from end (string suffix), for most string the
            {                                                 //string tail is the most changing part (i.e. 'Alex Kozloff'/'Alex Richardson'/'System.A'/'System.B'
              if (i<sl-1) hash1 <<= 8;
              var c = key[i];
              var b1 = (c & 0xff00) >> 8;
              var b2 = c & 0xff;
              hash1 |= (byte)(b1 ^ b2);
            }

            ulong hash2 = 1566083941ul * (ulong)key.GetHashCode();

            return hash1 ^ hash2;
        }


        /// <summary>
        /// Override to convert complex object to uint64 key.
        /// The default implementation returns DefaultComplexKeyToCacheKey(key)
        /// </summary>
        public virtual ulong ComplexKeyToCacheKey(object key)
        {
            return DefaultComplexKeyToCacheKey(key);
        }


        /// <summary>
        /// Override to put a value keyed on a non-uint64 scalar into cache. Returns table.Put() result
        /// </summary>
        public virtual bool Put(string tableName, object key, object value, int maxAgeSec = 0, int priority = 0, DateTime? absoluteExpirationUTC = null)
        {
            if (key==null)
             throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().FullName+"Put(key=null)");

            ulong id = ComplexKeyToCacheKey(key);

            var tbl = Store[tableName];

            int cnt = 0;
            while(cnt<MAX_CACHE_CHAIN_LENGTH)//key collision
            {
              var crec = tbl.Get(id);
              if (crec==null) break;
              if (key.Equals(crec.Metadata)) break;
              id++;
              cnt++;
            }

            CacheRec rec;
            var result = tbl.Put(id, value, out rec, maxAgeSec, priority, absoluteExpirationUTC);
            rec.Metadata = key;
            return result;
        }

        /// <summary>
        /// Override to get a value keyed on a non-uint64 scalar from cache. Returns null when item does not exist
        /// </summary>
        public virtual object Get(string tableName, object key, int ageSec = 0)
        {
          if (key==null)
             throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().FullName+"Get(key=null)");

          ulong id = ComplexKeyToCacheKey(key);

          var tbl = Store[tableName];

          int cnt = 0;
          while(cnt<MAX_CACHE_CHAIN_LENGTH)//key collision
          {
            var result = tbl.Get(id, ageSec);
            if (result!=null)
            {
             if (key.Equals(result.Metadata))
             {
               System.Threading.Interlocked.Increment(ref tbl.stat_ComplexHitCount);
               return result.Value;
             }
            }
            id++;
            cnt++;
          }

          System.Threading.Interlocked.Increment(ref tbl.stat_ComplexMissCount);
          return null;//not found
        }


        /// <summary>
        /// Override to remove an item keyed on a non-uint64 scalar from cache. Returns true if remove found and removed item from table
        /// </summary>
        public virtual bool Remove(string tableName, object key)
        {
          if (key==null)
             throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().FullName+"Remove(key=null)");

          ulong id = ComplexKeyToCacheKey(key);

          var tbl = Store[tableName];

          int cnt = 0;
          bool removed = false;
          while(cnt<MAX_CACHE_CHAIN_LENGTH)//key collision
          {
            var result = tbl.Get(id);
            if (result!=null)
            {
             if (key.Equals(result.Metadata))
             {
               removed = tbl.Remove(id);
               break;
             }
            }
            id++;
            cnt++;
          }

          if (removed)
              System.Threading.Interlocked.Increment(ref tbl.stat_ComplexHitCount);
          else
              System.Threading.Interlocked.Increment(ref tbl.stat_ComplexMissCount);

          return removed;
        }


    }
}
