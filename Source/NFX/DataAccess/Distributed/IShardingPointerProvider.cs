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
