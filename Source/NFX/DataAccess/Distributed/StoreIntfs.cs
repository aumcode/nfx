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
using System.Threading.Tasks;

using NFX.ApplicationModel;

namespace NFX.DataAccess.Distributed
{
    /// <summary>
    /// Represents a data store that works with large distributed systems that support OLTP-style processing
    /// and provide data querying, caching, partitioning/sharding, failover and replication.
    /// These systems are designed to handle billions of rows that need to be accessed by millions of active concurrent users,
    ///  so the design is specific to this model that scales horizontally. There is no need to use this technology for medium and smaller data stores
    ///  as it imposes specific requirements on how application is written/interacts with the backend system. This technology is based on the idea
    ///  of Parcels - an atomic unit of data change. Parcels get replicated between hosts for failover and performance reasons.
    /// Note:
    ///  NFX library does not provide the implementation for this technology, only marker interfaces so developers can plan for distributed backends
    ///  in future
    /// </summary>
    /// <remarks>
    /// The structure of distributed data store:
    ///
    ///  +------------------------------------------------------------------------------------------------------------+
    ///  |                                            Data Store                                                      |
    ///  |  +-------------------------------------------------------------------------+      +---------------------+  |
    ///  |  |                             Bank = N 1 (Schema A)                       |      |      Bank   =  N 2  |  |
    ///  |  | +----------+ +--------------------------------------------------------+ |      |      (Schema A)     |  |
    ///  |  | |   Area   | +                 Area "UserData"                        | | ...  |                     |  |
    ///  |  | | "common" | +-----------+-----------+-----------+-----------+--------+ |      |                     |  |
    ///  |  | |          | +  Shard 1  |  Shard 2  |  Shard 3  |  Shard 4  |          |      |                     |  |
    ///  |  | +----------+ |           |           |           |           |          |      +---------------------+  |
    ///  |  |              +-----------+           |           |           |          |      +---------------------+  |
    ///  |  | +----------------------+ |           +-----------+           |          |      |      Bank   =  N X  |  |
    ///  |  | |Area "clinicalData"   | |           |           |           |          |      |      (Schema B)     |  |
    ///  |  | |       /doctors       | |           |           |           |          | ...  |                     |  |
    ///  |  | |       /codes         | |           |           |           |          |      |                     |  |
    ///  |  | |       /diagnoses     | |           |           +-----------+          |      |                     |  |
    ///  |  | +----------------------+ |           |                                  |      +---------------------+  |
    ///  |  |                          +-----------+                                  |                               |
    ///  |  +-------------------------------------------------------------------------+                               |
    ///  +------------------------------------------------------------------------------------------------------------+
    /// Distributed data stores hold single or multiple named data banks instances: Bank = global data bank name, a named instance of a distributed database.
    /// Banks are logical isolation containers in large datasets. Store implementations may use it for physical isolation as well.
    /// Every bank implements a particular schema - a structure suitable for some business purpose.
    /// A store may support multiple schemas, but every particular database bank implements only one schema.
    /// Named instances of database banks with the same schema may be used to house data for different clients or environments.
    /// Bank name example: "PROD-Data", "DEV-Data", "EnterpriseA", "CustomerX" etc.
    ///
    /// Every Bank is further broken down by Areas that can be accessed/addressed by their names.
    /// Areas contain shards that partition large volumes of data horizontally, they define how data is partitioned and where it is stored
    /// </remarks>
    public interface IDistributedDataStore : IDataStore
    {
        /// <summary>
        /// Returns names of database bank schemas supported by the store.
        /// Every bank implements a particular schema
        /// </summary>
        IEnumerable<string> SchemaNames { get; }

        /// <summary>
        /// Returns names of database bank instances in the store that implement the specified schema
        /// </summary>
        IEnumerable<string> BankNames(string schemaName);

        /// <summary>
        /// Returns Bank object by name within schema
        /// </summary>
        IBank GetBank(string schemaName, string name);
    }


    public interface IDistributedDataStoreImplementation : IDistributedDataStore, IDataStoreImplementation
    {

    }

    /// <summary>
    /// Provides information about schema of data store banks. Schema defines areas of the bank, where every area
    ///  defines what parcel types can be stored. Each bank implements only one bank schema
    /// </summary>
    public interface ISchema : INamed
    {
        /// <summary>
        /// References data store that this schema is a part of
        /// </summary>
        IDistributedDataStore DataStore{ get;}


        /// <summary>
        /// Returns registry of named schema areas
        /// </summary>
        IRegistry<IArea>  Areas { get;}

        /// <summary>
        /// Returns target name suffix which is added at the end.
        /// This allows for detailed targeting of metadata for particular schema.
        /// In most cases this property returns null which means no specific schema targeting
        /// </summary>
        string TargetSuffix { get;}
    }

    /// <summary>
    /// Provides information about an area of a bank schema. This information does not depend on a particular bank instance,
    ///  as it is common for all banks that implement the same schema.
    /// Area provides configuration information for parcels that it can store.
    /// Every instance of this (interface-implementer) class has a corresponding IAreaInstance instance that stores information
    ///  for every particular bank, i.e. what distribution policies are applied (such as sharding) to the parcels stored in this area
    /// </summary>
    public interface IArea : INamed
    {

        /// <summary>
        /// Returns schema that this area is in
        /// </summary>
        ISchema Schema { get;}


        /// <summary>
        /// Returns area description
        /// </summary>
        string Description { get;}

        /// <summary>
        /// Returns the type of device driver that loads/stores data
        /// </summary>
        Type DeviceType { get;}
    }

    /// <summary>
    /// Stipulates levels of data fidality, the higher the level - the more accurately verified data is provided
    /// </summary>
    public enum DataVeracity
    {
      /// <summary>
      /// The highest level of data accuracy - the backend will compare data from multiple storage mediums/devices to calculate the most accurate/latest data.
      /// This mode is the slowest among the others
      /// </summary>
      Maximum = 0,

      /// <summary>
      /// The backend will return the data which is stored in more than one device/medium
      /// </summary>
      BackedUp,

      /// <summary>
      /// The backend will return the fist available data as soon as it finds it. This mode is the fastest however it may return
      /// data that has since been overwritten in some other storage devices/mediums.
      /// This mode may be used in cases when performance is paramount but 100% data accuracy is not really needed, i.e. when showing
      /// message/forum post comments on a social web site
      /// </summary>
      Any
    }


    /// <summary>
    /// Stipulates where data gets cached
    /// </summary>
    public enum DataCaching
    {
      /// <summary>
      /// Does not get cached
      /// </summary>
      None = -1,

      /// <summary>
      /// Gets cached on the level where data is always accurate and never gets stale, i.e. in the BDB server that reflects most recent changes (can not get out of date)
      /// </summary>
      LatestData = 0,


      /// <summary>
      /// Gets cached everywhere including system tiers where data can get stale
      /// </summary>
      Everywhere
    }


    /// <summary>
    /// Provides abstraction for Global Database Bank instance
    /// </summary>
    public interface IBank : INamed
    {
        /// <summary>
        /// References data store that this data bank is a part of
        /// </summary>
        IDistributedDataStore DataStore{ get;}


        /// <summary>
        /// Returns the schema that this bank implements
        /// </summary>
        ISchema Schema { get;}

        /// <summary>
        /// Returns registry of named bank schema area instances
        /// </summary>
        IRegistry<IAreaInstance>  Areas { get;}


        /// <summary>
        /// Returns database description
        /// </summary>
        string Description { get;}

        /// <summary>
        /// Returns database description for the specific culture.
        /// This method is needed because end users may need to see the description of the database instance in their native language
        /// </summary>
        string GetDescription(string culture);



        /// <summary>
        /// Returns sequence provider that generates unique identifiers in the store
        /// </summary>
        IGDIDProvider IDGenerator { get; }



        /// <summary>
        /// Generates version replication information for the parcel instance which is being sealed
        /// </summary>
        IReplicationVersionInfo GenerateReplicationVersionInfo(Parcel parcel);



        /// <summary>
        /// Returns ULONG for an object so any object (i.e. a string) may be used as a sharding key.
        /// Suppose a string needs to be used for sharding, this method translates a string into a 64 bit hash expressed as ulong
        /// </summary>
        /// <param name="key">An object used for sharding ID translation</param>
        /// <returns>UInt64 that represents the sharding ID</returns>
        UInt64 ObjectToShardingID(object key);


        /// <summary>
        /// Loads parcel by fetching it from the bank backened by its primary id
        /// </summary>
        /// <param name="tParcel">Parcel type to load</param>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.GetParcelAttr(typeof(T)).ShardingParcel
        /// </param>
        /// <param name="veracity">The level of data veracity</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cacheMaxAgeSec">The maximum acceptable age of cached instance, cached data will be re-queried from backend if it is older</param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        Parcel Load(Type tParcel,
                      GDID id,
                      object shardingID = null,
                      DataVeracity veracity = DataVeracity.Maximum,
                      DataCaching cacheOpt = DataCaching.LatestData,
                      int? cacheMaxAgeSec = null,
                      ISession session = null);


        /// <summary>
        /// Async version: Loads parcel by fetching it from the bank backened by its primary id
        /// </summary>
        /// <param name="tParcel">Parcel type to load</param>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.GetParcelAttr(typeof(T)).ShardingParcel
        /// </param>
        /// <param name="veracity">The level of data veracity</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cacheMaxAgeSec">The maximum acceptable age of cached instance, cached data will be re-queried from backend if it is older</param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        Task<Parcel> LoadAsync(Type tParcel,
                                 GDID id,
                                 object shardingID = null,
                                 DataVeracity veracity = DataVeracity.Maximum,
                                 DataCaching cacheOpt = DataCaching.LatestData,
                                 int? cacheMaxAgeSec = null,
                                 ISession session = null);


        /// <summary>
        /// Loads parcel by fetching it from the bank backened by its primary id
        /// </summary>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.GetParcelAttr(typeof(T)).ShardingParcel
        /// </param>
        /// <param name="veracity">The level of data veracity</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cacheMaxAgeSec">The maximum acceptable age of cached instance, cached data will be re-queried from backend if it is older</param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        T Load<T>(GDID id,
                      object shardingID = null,
                      DataVeracity veracity = DataVeracity.Maximum,
                      DataCaching cacheOpt = DataCaching.LatestData,
                      int? cacheMaxAgeSec = null,
                      ISession session = null) where T : Parcel;


        /// <summary>
        /// Async version: Loads parcel by fetching it from the bank backened by its primary id
        /// </summary>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.GetParcelAttr(typeof(T)).ShardingParcel
        /// </param>
        /// <param name="veracity">The level of data veracity</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cacheMaxAgeSec">The maximum acceptable age of cached instance, cached data will be re-queried from backend if it is older</param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        Task<T> LoadAsync<T>(GDID id,
                                 object shardingID = null,
                                 DataVeracity veracity = DataVeracity.Maximum,
                                 DataCaching cacheOpt = DataCaching.LatestData,
                                 int? cacheMaxAgeSec = null,
                                 ISession session = null) where T : Parcel;

        /// <summary>
        /// Loads result object by executing a query command in the bank backend fetching necessary parcels/records/documents and aggregating the result.
        /// The query may return parcel/s that can be modified and saved back into the store
        /// </summary>
        /// <param name="command">The command object that contains command name and parameters to query the datastore</param>
        /// <param name="veracity">The level of data veracity</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cacheMaxAgeSec">The maximum acceptable age of cached instance, cached data will be re-queried from backend if it is older</param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        object Query(Command command,
                           DataVeracity veracity = DataVeracity.Maximum,
                           DataCaching cacheOpt = DataCaching.LatestData,
                           int? cacheMaxAgeSec = null,
                           ISession session = null);

        /// <summary>
        /// Async version: Loads result object by executing a query command in the bank backend fetching necessary parcels/records/documents and aggregating the result.
        /// The query may return parcel/s that can be modified and saved back into the store
        /// </summary>
        /// <param name="command">The command object that contains command name and parameters to query the datastore</param>
        /// <param name="veracity">The level of data veracity</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cacheMaxAgeSec">The maximum acceptable age of cached instance, cached data will be re-queried from backend if it is older</param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        Task<object> QueryAsync(Command command,
                                      DataVeracity veracity = DataVeracity.Maximum,
                                      DataCaching cacheOpt = DataCaching.LatestData,
                                      int? cacheMaxAgeSec = null,
                                      ISession session = null);


        /// <summary>
        /// Saves/sends the parcel into this bank
        /// </summary>
        /// <param name="parcel">parcel instance to save</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cachePriority">
        /// The relative priority of the item in cache.
        /// If null is passed then the value is obtained from the parcel instance
        /// </param>
        /// <param name="cacheMaxAgeSec">
        /// Specifies the duration of parcel lifespan in cache devices.
        /// If null is passed then the value is obtained from the parcel instance
        /// </param>
        ///<param name="cacheAbsoluteExpirationUTC">
        /// Specifies absolute expiration time for this parcel instance in cache.
        /// If null is passed then the value is obtained from the parcel instance
        /// </param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        void Save(Parcel parcel,
                  DataCaching cacheOpt = DataCaching.Everywhere,
                  int? cachePriority = null,
                  int? cacheMaxAgeSec = null,
                  DateTime? cacheAbsoluteExpirationUTC = null,
                  ISession session = null);


        /// <summary>
        /// Async version: Saves/sends the parcel into this bank
        /// </summary>
        /// <param name="parcel">parcel instance to save</param>
        /// <param name="cacheOpt">The cache control options</param>
        /// <param name="cachePriority">
        /// The relative priority of the item in cache.
        /// If null is passed then the value is obtained from the parcel instance
        /// </param>
        /// <param name="cacheMaxAgeSec">
        /// Specifies the duration of parcel lifespan in cache devices.
        /// If null is passed then the value is obtained from the parcel instance
        /// </param>
        ///<param name="cacheAbsoluteExpirationUTC">
        /// Specifies absolute expiration time for this parcel instance in cache.
        /// If null is passed then the value is obtained from the parcel instance
        /// </param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        Task SaveAsync(Parcel parcel,
                       DataCaching cacheOpt = DataCaching.Everywhere,
                       int? cachePriority = null,
                       int? cacheMaxAgeSec = null,
                       DateTime? cacheAbsoluteExpirationUTC = null,
                       ISession session = null);


        /// <summary>
        /// Removes the parcel from the bank returning true if parcel was found and removed
        /// </summary>
        /// <param name="tParcel">Type of parcel to remove</param>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.ShardingParcel
        /// </param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        bool Remove(Type tParcel,
                       GDID id,
                       object shardingID = null,
                       ISession session = null);

        /// <summary>
        /// Async version: Removes the parcel from the bank returning true if parcel was found and removed
        /// </summary>
        /// <param name="tParcel">Type of parcel to remove</param>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.ShardingParcel
        /// </param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        Task<bool> RemoveAsync(Type tParcel,
                                  GDID id,
                                  object shardingID = null,
                                  ISession session = null);

        /// <summary>
        /// Removes the parcel from the bank returning true if parcel was found and removed
        /// </summary>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.ShardingParcel
        /// </param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        bool Remove<T>(GDID id,
                       object shardingID = null,
                       ISession session = null) where T : Parcel;

        /// <summary>
        /// Async version: Removes the parcel from the bank returning true if parcel was found and removed
        /// </summary>
        /// <param name="id">The unique GDID of the parcel</param>
        /// <param name="shardingID">
        /// The ID of the entity used for sharding,
        /// i.e. a message may use ID of the item that the message relates to, so messages get sharded in the same location as their "parent" record.
        /// The parcel type T specifies the DataParcelAttirbute.ShardingParcel
        /// </param>
        /// <param name="session">User session, if null session will be taken from execution context. The session may be needed for policy filtering</param>
        Task<bool> RemoveAsync<T>(GDID id,
                                  object shardingID = null,
                                  ISession session = null) where T : Parcel;
    }

    /// <summary>
    /// Represents data for a concrete instance of schema area within banks, this depends on a concrete bank instance (unlike it's complementary part IArea)
    /// </summary>
    public interface IAreaInstance : INamed
    {
        /// <summary>
        /// Returns schema area that this bank area represents - a prototype of this instance
        /// </summary>
        IArea Area { get;}

        /// <summary>
        /// Returns bank instance that this area is in
        /// </summary>
        IBank Bank { get;}
    }





}
