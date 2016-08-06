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

    public interface ICachePolicy
    {
        /// <summary>
        /// Specifies for how long should this parcel be cached in RAM after a write (after a parcel gets saved).
        /// </summary>
        int? CacheWriteMaxAgeSec { get;  }

        /// <summary>
        /// Specifies the maximum age of parcel instance in cache to be suitable for reading
        /// </summary>
        int? CacheReadMaxAgeSec { get;  }

        /// <summary>
        /// Specifies the relative cache priority of this parcel
        /// </summary>
        int? CachePriority { get;  }

        /// <summary>
        /// Specifies the absolute time when items expires in cache
        /// </summary>
        DateTime? CacheAbsoluteExpirationUTC { get;}

        /// <summary>
        /// Specifies the name of cache table
        /// </summary>
        string CacheTableName { get;}
    }

    /// <summary>
    /// Holds data per IParcelCachePolicy contract
    /// </summary>
    public struct CachePolicyData : ICachePolicy
    {
      public int? CacheWriteMaxAgeSec {  get; internal set;  }
      public int? CacheReadMaxAgeSec  {  get; internal set;  }
      public int? CachePriority       {  get; internal set;  }
      public string CacheTableName    {  get; internal set;  }
      public DateTime? CacheAbsoluteExpirationUTC {  get; internal set;  }
    }


}
