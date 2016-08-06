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
    /// Denotes a distributed entity that can be replicated to a different location/site/instance.
    /// Normally only Parcel and its derivatives should implement this interface
    /// </summary>
    public interface IReplicatable
    {
        /// <summary>
        /// Provides the information about this version of data that this isntance represents.
        /// Usually this object contains the name of the user who made a change, server/cluster node ID and/or machine name
        /// </summary>
        IReplicationVersionInfo ReplicationVersionInfo { get; }


        /// <summary>
        /// Returns the relative priority of replication, where 0=highest priority
        /// </summary>
        int ReplicationPriority { get;}
    }

    public interface IReplicationVersionInfo : IComparable
    {

        /// <summary>
        /// Returns true for items that have been marked for deletion
        /// </summary>
        bool VersionDeleted { get;}

        /// <summary>
        /// Provides a UTC timestamp for this version - when last change was made
        /// </summary>
        DateTime VersionUTCTimestamp { get; }
    }

}
