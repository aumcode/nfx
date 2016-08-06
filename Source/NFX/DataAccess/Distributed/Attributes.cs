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

using NFX.Environment;

namespace NFX.DataAccess.Distributed
{

    /// <summary>
    /// Provides information about targetname->table name, sequence name(and possibly other) mappings
    /// </summary>
    public struct TargetTableMapping : INamed
    {
      public const string CONFIG_GDID_SEQUENCE_NAME_ATTR = "gdid-sequence";

      public TargetTableMapping(IConfigSectionNode node)
      {
        m_Name = node.Name;
        m_TableName = node.Value;
        m_GDIDSequenceName = node.AttrByName(CONFIG_GDID_SEQUENCE_NAME_ATTR).Value;
      }

      private string m_Name;
      private string m_TableName;
      private string m_GDIDSequenceName;
      //...more props in future

      public string Name { get { return m_Name;} }
      public string TableName { get { return m_TableName;} }
      public string GDIDSequenceName { get { return m_GDIDSequenceName;} }
    }



    /// <summary>
    /// Decorates Pacel-derivative classes specifying distributed data store options.
    /// Unlike the CRUD family of metadata attributes this attributed is NOT TARGETABLE on purpose
    /// beacause different sharding definitions would have affected the properties of the parcel which could have been
    /// very complex to maintain/account for. So, every parcel has ONLY ONE set opf metadata definition.
    /// In case when different parcel definitions needed a new parcel type should be created which can reuse the payload - this is much
    ///  easier to implement (two parcels) than targeting within the same parcel.
    /// Table mappings are targetable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class DataParcelAttribute : Attribute, ICachePolicy
    {

        /// <summary>
        /// Returns DataParcelAttribute for a parcel type. Use Parcel.MetadataAttribute to
        ///  obtain the attribute instance polymorphically for instance.
        ///  If parcel is not decorated by the attribute then exception is thrown
        /// </summary>
        public static DataParcelAttribute GetParcelAttr(Type tparcel)
        {
          if (tparcel==null || !typeof(Parcel).IsAssignableFrom(tparcel))
            throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+typeof(DataParcelAttribute).FullName+".GetParcelAttr(tparcel=null|!Parcel)");

          var result = getParcelAttrCore(tparcel);
          if (result==null)
            throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_MISSING_ATTRIBUTE_ERROR.Args(tparcel));

          return result;
        }


        private static Dictionary<Type, DataParcelAttribute> s_Cache = new Dictionary<Type, DataParcelAttribute>();
        internal static DataParcelAttribute getParcelAttrCore(Type tparcel)
        {
          DataParcelAttribute result;
          if (!s_Cache.TryGetValue(tparcel, out result))
          {
            var attrs = tparcel.GetCustomAttributes(typeof(DataParcelAttribute), false);
            if (attrs.Length>0) result= attrs[0] as DataParcelAttribute;
            if (result!=null)
            {
              var dict = new Dictionary<Type,DataParcelAttribute>(s_Cache);
              dict[tparcel] = result;
              s_Cache = dict;//atomic
            }
          }
          return result;
        }



        public DataParcelAttribute(
                             string schemaName,
                             string areaName,

                             bool supportsMerge = false,
                             Type shardingParcel = null,
                             string replicationChannel = null,
                             string cacheTableName = null,
                             int cacheWriteMaxAgeSec = -1,
                             int cacheReadMaxAgeSec = -1,
                             int cachePriority = -1,
                             string targetTableMappings = null)
        {
            if (schemaName.IsNullOrWhiteSpace() ||
                areaName.IsNullOrWhiteSpace()
                )
             throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(schemaName|areaName=null|empty)");

            SchemaName = schemaName;
            AreaName = areaName;

            if (shardingParcel!=null)
             if (!typeof(Parcel).IsAssignableFrom(shardingParcel))
              throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(shardingParcel='"+shardingParcel.FullName+"' isnot Parcel-derived)");

            SupportsMerge = supportsMerge;
            ShardingParcel = shardingParcel;
            ReplicationChannel = replicationChannel;
            CacheTableName = cacheTableName;
            CacheWriteMaxAgeSec = cacheWriteMaxAgeSec <0 ? (int?)null : cacheWriteMaxAgeSec;
            CacheReadMaxAgeSec  = cacheReadMaxAgeSec  <0 ? (int?)null : cacheReadMaxAgeSec;
            CachePriority       = cachePriority       <0 ? (int?)null : cachePriority;

            if (targetTableMappings.IsNotNullOrWhiteSpace())
            {
               var data = ("mappings{"+targetTableMappings+"}").AsLaconicConfig();
               if (data==null) data = targetTableMappings.AsLaconicConfig();
               if (data==null)
                throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(targetTableMappings='"+targetTableMappings+"' Laconic parse error)");

               foreach(var cn in data.Children)
                m_TableMappings.Register( new TargetTableMapping( cn ) );
            }
        }


        private Registry<TargetTableMapping> m_TableMappings = new Registry<TargetTableMapping>();

        /// <summary>
        /// Returns true if parcel supports merge with other versions.
        /// Server may merge multiple parcel versions to resolve versioning conflict.
        /// Default implementation returns false
        /// </summary>
        public bool SupportsMerge { get; private set;}


        /// <summary>
        /// Specifies the type of Parcel that is used for sharding. By default this parameter is null, so
        /// parcels are sharded on their own instance types, however there are cases when a parcel overrides ShardingID
        ///  and specifies ShardingParcel type via this member so it gets stored along with the specified parcel
        /// </summary>
        public Type ShardingParcel { get; private set;}


        /// <summary>
        /// Specifies the name for logical schema that parcel decorated by this attribute belongs to.
        /// The exact location within the data store is detailed further with AreaName property.
        /// The value is required and is always specified or exception is thrown in .ctor
        /// </summary>
        public string SchemaName { get; private set;}


        /// <summary>
        /// Specifies the Areaname - the logical subdivision in the store where parcels reside within the schema(see SchemaName): i.e.  'commonClinicalData'.
        /// Depending on a particular store setup this logical name may or may not represent physical servers/locations in the store.
        /// Every store implementation maps logical paths to physical servers and/or shards. This feature provides very high degree
        ///  of distributed database design flexibility, as some parcels may represent common/dictionary data that does not need to be sharded,
        /// whereas others may represent purely transactional high-volume data that needs to be sharded.
        /// The value is required and is always specified or exception is thrown in .ctor
        /// </summary>
        /// <remarks>
        /// Keep in mind that in a distributed database system various parcels may be stored not only in different locations, be optionally sharded,
        /// and even be stored in different back-end technologies (i.e. NoSQL/RDBMS/flat files)
        /// </remarks>
        public string AreaName { get; private set;}


        /// <summary>
        /// Specifies the name of the replication channel used to pump data between servers/data centers/locations
        /// </summary>
        public string ReplicationChannel { get; private set;}


        /// <summary>
        /// Specifies for how long should this parcel be cached in RAM after a write (after a saved change).
        /// This property acts as a default, the runtime first checks parcel instance properties then reverts to this attribute
        /// </summary>
        public int? CacheWriteMaxAgeSec { get; private set; }

        /// <summary>
        /// Specifies the maximum age of parcel instance in cache to be suitable for reading
        /// This property acts as a default, the runtime first checks parcel instance properties then reverts to this attribute
        /// </summary>
        public int? CacheReadMaxAgeSec { get; private set; }

        /// <summary>
        /// Specifies the relative cache priority of this parcel
        /// This property acts as a default, the runtime first checks parcel instance properties then reverts to this attribute
        /// </summary>
        public int? CachePriority { get; private set; }


        /// <summary>
        /// Specifies the name of the cache table for this parcel
        /// This property acts as a default, the runtime first checks parcel instance properties then reverts to this attribute
        /// </summary>
        public string CacheTableName { get; private set;}


        /// <summary>
        /// This property can not be set on the attribute level and always returns null
        /// </summary>
        public DateTime? CacheAbsoluteExpirationUTC { get {return null; }}

        /// <summary>
        /// Returns mappins of target->table attributes. Pass in [DataParcel(targetTableMappings = "targetName1=tableName1{atr1=v1 atr2=v2...} targetName2=tableName2{atr1=v1 atr2=v2...}...")];
        /// </summary>
        public IRegistry<TargetTableMapping> TableMappings { get{ return m_TableMappings;}}
    }
}
