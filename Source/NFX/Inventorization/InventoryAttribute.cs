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

namespace NFX.Inventorization
{

    /// <summary>
    /// Designates system architecture tiers
    /// </summary>
    [Flags]
    public enum SystemTiers {
                               Unspecified=0,
                               GUI = 1,
                               AppServer = 2,
                               DBServer = 4,
                               SystemServer = 8,
                               NotificationBus = 32,
                               All = GUI | AppServer | DBServer | SystemServer | NotificationBus
                              }


    /// <summary>
    /// Designates item concerns
    /// </summary>
    [Flags]
    public enum SystemConcerns {
                               Unspecified=0,
                               Security = 1,
                               Development = 2,
                               Testing = 4,
                               Deployment = 8,
                               Release = 16,
                               Testability = 32,
                               Configuration = 64,
                               Performance = 128,
                               Maintainability = 256,
                               MissionCriticality = 512,
                               Licensing = 1024,
                               Features = 2048,
                               Requirements = 4096,
                               Luck = 8192//yes, luck too
                             }



    /// <summary>
    /// Defines an inventory-related set of data which is used by automatic code/component discovery
    /// such as database schema generation tools and system-global registry servers
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited=true, AllowMultiple=true)]
    public sealed class InventoryAttribute : Attribute
    {

       /// <summary>
       /// Designates what tier this entry is for
       /// </summary>
       public SystemTiers Tiers { get; set; }

       /// <summary>
       /// Indicates what concerns this entry relates to
       /// </summary>
       public SystemConcerns Concerns { get; set; }

       /// <summary>
       /// A type of implementaton that this inventory entry refers to, i.e. "Oracle", "MongoDB", "Riak"
       /// </summary>
       public string Technology { get; set; }

       /// <summary>
       /// Provides a name of schema for this item, for example [Inventory(Technology="MongoDB", Schema="UserTransactions")]
       /// </summary>
       public string Schema { get; set; }

       /// <summary>
       /// Provides an optional name of an applicable tool [Inventory(Tool="ErlangModuleGenerator")]
       /// </summary>
       public string Tool { get; set; }

        /// <summary>
       /// Provides an optional start date i.e. when this item will start to be used
       /// </summary>
       public DateTime? StartDate { get; set; }


       /// <summary>
       /// Provides an optional end date i.e. when this item will be phased out
       /// </summary>
       public DateTime? EndDate { get; set; }

       /// <summary>
       /// Parameters for particular inventory entry. This can be used as params for auto-generating tools that scan inventories
       /// </summary>
       public string Parameters { get; set; }
    }
}
