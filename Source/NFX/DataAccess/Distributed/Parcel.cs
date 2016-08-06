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
using System.Runtime.Serialization;

using NFX.Serialization;
using NFX.Serialization.Slim;
using NFX.DataAccess.Cache;

namespace NFX.DataAccess.Distributed
{

    /// <summary>
    /// Represents data parcel states - creating, modifying, sealed
    /// </summary>
    public enum ParcelState {Sealed = 0, Creating, Modifying}


    /// <summary>
    /// Describes a data parcel - a piece of logically-grouped data that gets fetched from/comitted into a distributed backend system.
    /// Parcels represent an atomic unit of change, a changeset that gets replicated between failover hosts.
    /// Every parcel has a Payload property that stores business data of interest that the parcel contains.
    /// This class is not thread-safe. Use DeepClone() to create 100% copies for working in multiple threads.
    /// This particular class serves as a very base for all Parcel implementations
    /// </summary>
    [Serializable]
    public abstract class Parcel : DisposableObject, IReplicatable, ICachePolicy, IDistributedStableHashProvider, IShardingPointerProvider
    {
        #region CONSTS


          /// <summary>
          /// Default priority of replication
          /// </summary>
          public static int DEFAULT_REPLICATION_PRIORITY = 4;


          /// <summary>
          /// Defines well-known frequently used types for slim serializer compression
          /// </summary>
          public static readonly TypeRegistry STANDARD_KNOWN_SERIALIZER_TYPES = new TypeRegistry(
                                                                 TypeRegistry.BoxedCommonTypes,
                                                                 TypeRegistry.BoxedCommonNullableTypes,
                                                                 TypeRegistry.CommonCollectionTypes,
                                                                 TypeRegistry.DataAccessCRUDTypes
                                                                 );
        #endregion


        #region .ctor / static


            /// <summary>
            /// Obtains parcel cache name from either it's associated DataParcelAttribute or
            ///  if not defined, from parcel type full name
            /// </summary>
            public static string GetParcelCacheTableName(Type tparcel)
            {
              if (tparcel==null || !typeof(Parcel).IsAssignableFrom(tparcel))
                  throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+"Parcel.GetParcelCacheTableName(tparcel=null|!Parcel)");

              string result = null;

              var attr = DataParcelAttribute.getParcelAttrCore(tparcel);
              if (attr!=null)
               result = attr.CacheTableName;

              if (result.IsNullOrWhiteSpace())
               result = tparcel.FullName;

              return result;
            }


            /// <summary>
            /// Used for serialization
            /// </summary>
            protected Parcel(){ }

            /// <summary>
            /// This ctor is never public, used with __ctor__injectID
            /// </summary>
            protected Parcel(object payload)
            {
               if (payload==null)
                 throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(payload==null)");

               m_State = ParcelState.Creating;
               m_Payload = payload;
            }


            /// <summary>
            /// Called when creating new Parcel instances by the original author.
            /// The new instance is in 'ParcelState.Creating' state
            /// </summary>
            protected Parcel(GDID id, object payload)
            {
                m_GDID = id;
                if (payload==null)
                 throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(payload==null)");

                m_State = ParcelState.Creating;
                m_Payload = payload;
            }

            /// <summary>
            /// Called by device to load parcel from storage.
            /// The new instance is in 'ParcelState.Sealed' state.
            /// Business logic devs - do not call
            /// </summary>
            protected Parcel(GDID id, object payload, IReplicationVersionInfo versInfo)
            {
               m_GDID = id;

               if (versInfo==null)
                 throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(versInfo==null)");

               if (payload==null && !versInfo.VersionDeleted)
                 throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(payload==null)");

               m_State = ParcelState.Sealed;
               m_Payload = payload;
               m_ReplicationVersionInfo = versInfo;
            }



        #endregion

        #region Fields


            private GDID m_GDID;
               /// <summary>
               /// Internal framework method, developers: never call
               /// </summary>
               protected void __ctor__injectID(GDID id){ m_GDID = id;}


            private IReplicationVersionInfo m_ReplicationVersionInfo;//created by Seal()

            private object m_Payload;


            [NonSerialized]
            private ParcelState m_State;

            [NonSerialized]
            private bool m_NewlySealed;

            [NonSerialized]
            protected List<Exception> m_ValidationExceptions;

        #endregion



        #region Properties

            /// <summary>
            /// Returns GDID for data that this parcel represents
            /// </summary>
            public GDID GDID
            {
                get { return m_GDID;}
            }


            /// <summary>
            /// Returns the ShardingPointer(type:ID) used for sharding, default implementation returns this parcel type and GDID.
            /// Override ShardingID property to return another sharding key, i.e. a social comment msg may use parent item (that the msg relates to) ID
            /// as the shard key so msgs are co-located with related items. Use CompositeShardingID to return multiple values.
            ///  IMPORTANT!!! ShardingPointer must return an immutable value, the one that CAN NOT be changed during parcel payload life
            /// </summary>
            public ShardingPointer ShardingPointer
            {
                get { return new ShardingPointer( MetadataAttribute.ShardingParcel ?? this.GetType() , this.ShardingID ); }
            }

            /// <summary>
            /// Returns the ShardingID used for sharding, default implementation returns this parcel type and GDID.
            /// Override to return another sharding key, i.e. a social comment msg may use parent item (that the msg relates to) ID
            /// as the shard key so msgs are co-located with related items. Use CompositeShardingID to return multiple values.
            ///  IMPORTANT!!! ShardingID must return an immutable value, the one that CAN NOT be changed during parcel payload life
            /// </summary>
            public virtual object ShardingID
            {
               get { return m_GDID;}
            }

            /// <summary>
            /// Returns replication priority, override to make replication priority dependent on instance, i.e. a user profile data for
            ///  a celebrity user may need to have higher replication rate. The lower value indicates the higher priority
            /// </summary>
            public virtual int ReplicationPriority
            {
                get { return DEFAULT_REPLICATION_PRIORITY;}
            }


            /// <summary>
            /// Returns payload of this parcel. This may be null for deleted parcels (VersionDeleted).
            /// WARNING!!! Although parcels do not allow to set Payload property if they are sealed, one can still mutate/modify the payload object graph
            /// even on a sealed parcel instance, i.e. one may write:
            ///   <code>mySealedParcel.Payload.DueDates.Add(DateTime.Now) (given DueDates of type List(DateTime))</code>.
            /// This case is considered to be a bug in the calling-code. The framework has no way of preventing such an inadvertent behavior as there is no
            /// way to intercept a mutation via transitive or direct references/functors in an object graph referenced by Payload property because payload type
            ///  does not impose (and should not) any constraints on what can be a payload.
            /// In Aum language we will use static type checker that will detect possible property access via Payload BEFORE calling Open()
            /// </summary>
            public object Payload { get { return m_Payload;}}


            /// <summary>
            /// Indicates whether the data may be altered.
            /// ReadOnly parcels can not be Opened after that have been Sealed by their creator
            /// </summary>
            public abstract bool ReadOnly { get; }


            /// <summary>
            /// Returns the state of the parcel: Creating|Modifying|Sealed
            /// </summary>
            public ParcelState State { get { return m_State;} }


            /// <summary>
            /// Returns true when parcel was just sealed after a call to Open or .ctor/create.
            /// Datastore may check this flag and disallow saving of parcel instances that have not changed
            /// </summary>
            public bool NewlySealed { get{return m_NewlySealed;} }

            /// <summary>
            /// Implements IReplicatable interface
            /// </summary>
            public IReplicationVersionInfo ReplicationVersionInfo { get { return m_ReplicationVersionInfo; } }


            /// <summary>
            /// Implements IParcelCachePolicy contract.
            /// The default implementation returns null.
            /// Override to supply a value for maximum length of this isntance stay in cache
            ///  that may depend on particular parcel payload state (i.e. field values)
            /// </summary>
            public virtual int? CacheWriteMaxAgeSec
            {
                get { return null; }
            }

            /// <summary>
            /// Implements IParcelCachePolicy contract.
            /// The default implementation returns null.
            /// Override to supply a value for maximum validity span of cached parcel
            ///  that may depend on particular parcel payload state (i.e. field values).
            /// This property may be used to obtain a value before parcel is re-read from the store
            /// </summary>
            public virtual int? CacheReadMaxAgeSec
            {
                get { return null; }
            }

            /// <summary>
            /// Implements IParcelCachePolicy contract.
            /// The default implementation returns null.
            /// Override to supply a relative cache priority of this parcel
            ///  that may depend on particular parcel payload state (i.e. field values).
            /// </summary>
            public virtual int? CachePriority
            {
                get { return null; }
            }

            /// <summary>
            /// Implements IParcelCachePolicy contract.
            /// The implementation returns null for parcel.
            /// </summary>
            string ICachePolicy.CacheTableName
            {
                get { return null; }
            }

            /// <summary>
            /// Implements IParcelCachePolicy contract.
            /// The default implementation returns null.
            /// Override to supply a different absolute cache expiration UTC timestamp for this parcel
            ///  that may depend on particular parcel payload state (i.e. field values).
            /// </summary>
            public DateTime? CacheAbsoluteExpirationUTC
            {
                get { return null; }
            }

            /// <summary>
            /// Returns validation exceptions - populated by a call to Validate
            /// </summary>
            public IEnumerable<Exception> ValidationExceptions { get { return m_ValidationExceptions ?? Enumerable.Empty<Exception>();}}


                         [NonSerialized] private DataParcelAttribute m_MetadataAttribute;
            /// <summary>
            /// Returns DataParcelAttribute that describes this parcel. Every parcel MUST be decorated by the DataParcel attribute
            /// </summary>
            public DataParcelAttribute MetadataAttribute
            {
              get
              {
                if (m_MetadataAttribute==null)
                  m_MetadataAttribute = DataParcelAttribute.GetParcelAttr(this.GetType());
                return m_MetadataAttribute;
              }
            }

            /// <summary>
            /// Returns effective cache policy the one that is calculated from attribute and overidden by the instance
            /// </summary>
            public ICachePolicy EffectiveCachePolicy
            {
              get
              {
                 var result =  new CachePolicyData
                 {
                   CacheReadMaxAgeSec  = this.CacheReadMaxAgeSec ?? MetadataAttribute.CacheReadMaxAgeSec,
                   CacheWriteMaxAgeSec = this.CacheWriteMaxAgeSec ?? MetadataAttribute.CacheWriteMaxAgeSec,
                   CachePriority       = this.CachePriority ?? MetadataAttribute.CachePriority,
                   CacheTableName      = MetadataAttribute.CacheTableName,
                   CacheAbsoluteExpirationUTC = this.CacheAbsoluteExpirationUTC ?? MetadataAttribute.CacheAbsoluteExpirationUTC
                 };

                 if (result.CacheTableName.IsNullOrWhiteSpace())
                   result.CacheTableName = this.GetType().FullName;

                 return result;
              }
            }

        #endregion

        #region Public



            /// <summary>
            /// Opens parcel for modification. Parcel must be in the Sealed state for this call and must not be ReadOnly.
            /// Once open for modification, a parcel can not be "UnOpened", only Seal()-ed. This is because there is no way to track
            /// whether some part of payload object has changed by the calling code. Use DeepClone() before calling Open to retain a copy of Sealed parcel
            /// to revert to the cloned instance instead
            /// </summary>
            public void Open()
            {
                if (m_State!=ParcelState.Sealed)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Open", GetType().FullName, m_State) );
                if (ReadOnly)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Open", GetType().FullName+"(ReadOnly)", m_State) );

                if (ReplicationVersionInfo!=null && ReplicationVersionInfo.VersionDeleted)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Open", GetType().FullName+"(VersionDeleted)", m_State) );

                DoOpen();
                m_State = ParcelState.Modifying;
            }

            /// <summary>
            /// Seal parcel after modification
            /// May call this method on parcels in either Creating or Modifying states.
            /// Bank is used to create the ReplicationVersionInfo which is depends on store/bank implementation.
            /// Parcels can replicate only within stores/technologies that have sealed them
            /// </summary>
            public void Seal(IBank bank)
            {
               if (bank==null)
                 throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+"Parcel.Seal(bank==null)");

               if (m_State!= ParcelState.Creating && m_State!=ParcelState.Modifying)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Seal", GetType().FullName, m_State) );

               Validate(bank);

               if (ValidationExceptions.Any())
                 throw ParcelSealValidationException.ForErrors(GetType().FullName, ValidationExceptions);


               m_ReplicationVersionInfo = bank.GenerateReplicationVersionInfo(this);

               DoSeal(bank);

               m_State = ParcelState.Sealed;
               m_NewlySealed = true;
            }

            /// <summary>
            /// Merges other parcel instances into this one.
            /// A parcel type may support merging (when DataParcel attribute SupportsMerge is set to true) of data from other parcels/versions into this instance.
            /// This parcel must not be sealed as merge may change the payload in which case TRUE is returned. The parcel needs to be sealed again after the change.
            /// If a call to this method returns false, then nothing was changed as this instance already contains the latest data/could not be merged.
            /// Merging is used for version conflict resolution: servers check if the type of the updated parcel instance SupportsMerge, then if it does, Opens parcel
            /// and calls this method passing other versions in question to this method. The framework does not impose a limit on the supplied parcel types, however
            /// most of the parcel types support merge only with the same-typed parcel instances.
            /// IMPORTANT: The ordering of parcel versions is not guaranteed.
            /// </summary>
            /// <param name="others">Other parcel versions/data</param>
            /// <returns>True if Merge() generated newer version/changed payload. False when Merge did not/could not change existing parcel</returns>
            public bool Merge(IEnumerable<Parcel> others)
            {
              if (others==null)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Merge", GetType().FullName, "others==null") );

              if (m_State!= ParcelState.Creating && m_State!=ParcelState.Modifying)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Merge", GetType().FullName, m_State) );

              if (!MetadataAttribute.SupportsMerge)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Merge", GetType().FullName, "SupportMerge==false") );

              return DoMerge(others);
            }

            /// <summary>
            /// Performs validation of the data. Does not throw but populates validation exceptions if data is not valid.
            /// Bank context may be used to run sub-queries during consistency/crosschecks
            /// May call this method on parcels in either Creating or Modifying states
            /// </summary>
            public void Validate(IBank bank)
            {
                if (m_State!= ParcelState.Creating && m_State!=ParcelState.Modifying)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Validate", GetType().FullName, m_State) );

                m_ValidationExceptions = new List<Exception>();
                DoValidate(bank);
            }

            /// <summary>
            /// Duplicates this parcel by doing a complete deep-clone of its state via serialization.
            /// This method is useful for making copies of the same parcel for different threads as it is thread-safe while no other thread mutates the instance,
            /// however Parcel instances are NOT THREAD-SAFE for parallel changes.
            /// The existing parcel MUST be SEALED (otherwise it can not be serialized).
            /// This method is also used before a call to Open() if parcel needs to be "un-opened" the cloned instance may be reverted to
            /// </summary>
            public Parcel DeepClone()
            {
              if (m_State!=ParcelState.Sealed)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("DeepClone", GetType().FullName, m_State) );

              using(var ms = new System.IO.MemoryStream(4*1024))
              {
                var serializer = new SlimSerializer( STANDARD_KNOWN_SERIALIZER_TYPES );

                serializer.Serialize(ms, this);
                ms.Position = 0;
                return serializer.Deserialize(ms) as Parcel;
              }
            }

            /// <summary>
            /// Tests for parcel equality based on the same type and GDID value
            /// </summary>
            public override bool Equals(object obj)
            {
              var other = obj as Parcel;
              if (other==null) return false;
              if (this.GetType() != other.GetType()) return false;
              if (this.m_GDID != other.m_GDID) return false;
              return true;
            }

            /// <summary>
            /// Generates hash code based on parcel type and GDID value
            /// </summary>
            public override int GetHashCode()
            {
              return this.GetType().GetHashCode() ^  m_GDID.GetHashCode();
            }

            public ulong GetDistributedStableHash()
            {
              return m_GDID.GetDistributedStableHash();
            }

            public override string ToString()
            {
              return "{0}[{1}:{2}]({3})".Args(
                            GetType().DisplayNameWithExpandedGenericArgs(),
                            m_GDID,
                            m_State,
                            (m_Payload != null) ? m_Payload.GetType().DisplayNameWithExpandedGenericArgs() : "<null>"
                         );
            }


        #endregion


        #region Protected

            /// <summary>
            /// Override to perform actions when parcel is unsealed (opened) for modification
            /// </summary>
            protected virtual void DoOpen()
            {

            }


            /// <summary>
            /// Override to seal the parcel instance, i.e. generate some calculated fields.
            /// The version information is already generated by the time this method is called
            /// </summary>
            protected virtual void DoSeal(IBank bank)
            {

            }


            /// <summary>
            /// Override to merge other parcels into this one. Return true if merge changed the payload. See Merge().
            /// This implementation throws an exception
            /// </summary>
            protected virtual bool DoMerge(IEnumerable<Parcel> others)
            {
              throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_MERGE_NOT_IMPLEMENTED_ERROR.Args(GetType().FullName) );
            }

            /// <summary>
            /// Override to perform parcel/payload validation after modification. Add validation exceptions into m_ValidationExceptions field.
            /// Use bank for cross-checks (i.e. may run queries to check consistency)
            /// </summary>
            protected abstract void DoValidate(IBank bank);




        #endregion

        #region .pvt .impl

           [OnSerializing]
           private void OnSerializing(StreamingContext context)
           {
              if (m_State!=ParcelState.Sealed)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("OnSerializing", GetType().FullName, m_State) );
           }

        #endregion
    }//Parcel

    /// <summary>
    /// Describes a data parcel - a piece of logically-grouped data that gets fetched from/comitted into a distributed backend system.
    /// Parcels represent an atomic unit of change, a changeset that gets replicated between failover hosts.
    /// Every parcel has a Payload property that stores business data of interest that the parcel contains.
    /// This class is not thread-safe.
    /// This particular class serves as a very base for distributed data store implementations
    /// </summary>
    /// <typeparam name="TPayload">Type of payload that parcel carries</typeparam>
    [Serializable]
    public abstract class Parcel<TPayload> : Parcel where TPayload : class
    {
        #region .ctor


            /// <summary>
            /// Used for serialization
            /// </summary>
            protected Parcel():base(){ }

            /// <summary>
            /// This ctor is never public, used with __ctor__injectID
            /// </summary>
            protected Parcel(TPayload payload) : base(payload)
            {

            }

            protected Parcel(GDID id, TPayload payload) : base(id, payload)
            {
            }

            protected Parcel(GDID id, TPayload payload, IReplicationVersionInfo versInfo) : base(id, payload, versInfo)
            {

            }

        #endregion

        #region Properties
           /// <summary>
           /// Returns payload of this parcel. May be null for deleted parcels (VersionDeleted)
           /// </summary>
           public new TPayload Payload
           {
              get{ return (TPayload)base.Payload; }
           }
        #endregion
    }



}
