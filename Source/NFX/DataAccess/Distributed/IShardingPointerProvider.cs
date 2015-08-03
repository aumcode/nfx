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

using NFX;
using NFX.DataAccess.Cache;

namespace NFX.DataAccess.Distributed
{

    /// <summary>
    /// Denotes entities that provide ULONG STABLE hash code for use in a distributed (large scale) system.
    /// This is needed primarily for cluster/large datasets to properly compute 64bit sharding addresses and to differentiate
    /// from GetHashCode() that returns 32 bits unstable hash for local object location in hashtables.
    /// DO not confuse with object.GetHashCode() which is un-suitable for long-term persistence
    /// </summary>
    public interface IDistributedStableHashProvider
    {
      /// <summary>
      /// Provides 64 bit STABLE hash suitable for distributed system application.
      /// This hash may NOT depend on platform as it is used for storage.
      /// Warning! DO NOT CALL object.GetHashCode() as it may not be suitable for storage
      /// </summary>
      ulong GetDistributedStableHash();
    }



    /// <summary>
    /// Contains information about sharding parcel type and ID
    /// which is used to calculate the physical location of parcel data
    /// </summary>
    public struct ShardingPointer 
    {
      public ShardingPointer(Type tParcel, object id)
      {
         if (tParcel==null || id==null)
          throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+"ShardingPointer.ctor(parcel|id==null)");
     
         if (!typeof(Parcel).IsAssignableFrom(tParcel))
          throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+"ShardingPointer.ctor(shardingParcel='"+tParcel.FullName+"' isnot Parcel-derived");
                

         ParcelType = tParcel;
         ID = id;
      }
      
      public readonly Type ParcelType;
      public readonly object ID;


      public bool IsAssigned{ get{ return  this.ParcelType!=null && ID!=null;} }

      public override string ToString()
      {
        return IsAssigned ? ParcelType.DisplayNameWithExpandedGenericArgs() + "::" + ID.ToString() : "<not assigned>";
      }
    }
    
    /// <summary>
    /// Used to return ID data from multiple elements, i.e. multiple parcel fields so sharding framework may obtain
    /// ULONG sharding key. You can not compare or equate instances (only reference comparison of data buffer)
    /// </summary>
    public struct CompositeShardingID : IDistributedStableHashProvider, IEnumerable<object>
    {

      public CompositeShardingID(params object[] data)
      {
        if (data==null || data.Length==0)
          throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+"CompositeShardingID.ctor(data==null|empty)");

        m_Data = data;

        m_HashCode = 0ul;

        for(var i=0; i<m_Data.Length; i++)
        {
         var elm = m_Data[i];
         
         ulong ehc;
         
         if (elm!=null)
         {
             var ulhp = elm as IDistributedStableHashProvider;
             if (ulhp!=null) 
              ehc = ulhp.GetDistributedStableHash();
             else
              ehc = (ulong)elm.GetHashCode();
         }
         else
          ehc = 0xaa018055ul;

         m_HashCode <<= 1;
         m_HashCode ^= ehc;
        }
      }


      private ulong m_HashCode;
      private object[] m_Data;

      public int Count{ get{ return m_Data==null ? 0 : m_Data.Length;} }
      public object this[int i] { get{ return m_Data==null ? null : ( i>=0 && i<m_Data.Length ? m_Data[i] : null );}}

      public ulong GetDistributedStableHash()
      {
         return m_HashCode;
      }

      public override string ToString()
      {
        if (m_Data==null) return "[]";

        var sb = new StringBuilder("CompositeShardingID[");
        for(var i=0; i<m_Data.Length; i++)
        {
          if (i>0) sb.Append(", ");

          var elm = m_Data[i];

          if (elm==null)
           sb.Append("<null>");
          else
           sb.Append(elm.ToString());
        }
        sb.Append(']');

        return sb.ToString();
      }

      public IEnumerator<object> GetEnumerator() { return m_Data!=null ? ((IEnumerable<object>)m_Data).GetEnumerator() : Enumerable.Empty<object>().GetEnumerator(); }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){ return m_Data!=null ? m_Data.GetEnumerator() : Enumerable.Empty<object>().GetEnumerator(); }
    }
    
    
    /// <summary>
    /// Denotes an entity which provides a sharding parcel type along with sharding ID (ShardingPointer) that can be used to determine 
    /// data location via conversion of this id into physical shard #(particular server) that this entity represents
    /// </summary>
    public interface IShardingPointerProvider
    {
        /// <summary>
        /// Returns the parcel type and ID used for sharding. 
        /// This pointer is converted into physical shard # (particular server) where data represented by this entity resides. 
        /// WARNING! The ShardingPointer is immutable during the lifecycle of the entity. See Parcel.ShardingPointer
        /// </summary>
        ShardingPointer ShardingPointer { get; }
    }


}
