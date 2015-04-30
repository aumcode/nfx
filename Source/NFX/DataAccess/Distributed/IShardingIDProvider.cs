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

namespace NFX.DataAccess.Distributed
{

    /// <summary>
    /// Denotes an entity which provides a sharding ID that can be used to determine 
    /// data location via conversion of this id into physical shard #(particular server) that this entity represents
    /// </summary>
    public interface IShardingIDProvider
    {
        /// <summary>
        /// Returns the ID used for sharding. This id is converted into physical shard # (particular server) where
        /// data represented by this entity resides. 
        /// WARNING! The ShardingID is immutable during the lifecycle of the entity. See Parcel.ShardingID
        /// </summary>
        object ShardingID { get; }
    }


}
