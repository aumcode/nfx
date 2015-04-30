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
    /// This class is designed in such way that Payload property (unwrapped data) is not serialized, instead a byte[](wrapped data) is serialized.
    /// This is needed because an instance of Parcel may travel between many hosts that do not need to serialize/deserialize possibly complex business
    /// object that parcel contains.
    /// Parcels wrap their payload - when parcels get transported between hosts only parcel metadata (Parcel class fields) get serialized/deserialized by 
    /// hosts in the data supply chain, so metadata is available for tasks like parcel routing and cache policy adjustment, but the business data (the payload) must be 
    /// unwrapped first before it can be used. This design promotes efficient storage in distributed cache systems, i.e. the data origination host may keep cached version
    /// in an unwrapped state (not serialized) so business payload is ready for access right away without deserialization. 
    /// On the other hand, the intermediary parcel relays do not need to deserialize/serialize payload every time as it may be complex and waste significant processing time.
    /// This class is not thread-safe. Use DeepClone() to create 100% copies for working in multiple threads.
    /// This particular class serves as a very base for all Parcel implementations 
    /// </summary>
    [Serializable]
    public abstract class Parcel : DisposableObject, IReplicatable, IParcelCachePolicy, IULongHashProvider, IShardingIDProvider 
    {
        #region CONSTS

          
          /// <summary>
          /// Default priority of replication
          /// </summary>
          public static int DEFAULT_REPLICATION_PRIORITY = 4;
          
          
          /// <summary>
          /// Denotes a payload serialization format that uses Slim with standard(to this class) type registry
          /// </summary>
          public const string STANDARD_SLIM_PAYLOAD_FORMAT = "std.slim";

          /// <summary>
          /// Denotes a payload serialization format that uses PODSlim (Portable Object Document + Slim) with standard(to this class) type registry
          /// </summary>
          public const string STANDARD_PODSLIM_PAYLOAD_FORMAT = "std.PODslim";

          /// <summary>
          /// Defines well-known frequently used types for slim serializer compression
          /// </summary>
          public static readonly TypeRegistry STANDARD_KNOWN_SERIALIZER_TYPES = new TypeRegistry(
                                                                 TypeRegistry.BoxedCommonTypes,
                                                                 TypeRegistry.BoxedCommonNullableTypes,
                                                                 TypeRegistry.CommonCollectionTypes,
                                                                 TypeRegistry.DataAccessCRUDTypes
                                                                 ); 

          public const int DEFAULT_WRAPPED_PAYLOAD_BUFFER_SIZE = 1 * 1024;

          public const int REALLOC_EXCESS_THRESHOLD = 0xff;

        #endregion
        
        
        #region .ctor / static
            
            
            /// <summary>
            /// Obtains parcel cache name from either it's associated DataParcelAttribute or 
            ///  if not defined, from parcel type full name. Keep in mind, that a parcel instance may dynamicaly sypply a
            ///  different name for CacheTable so the store may need to search in alternative cache tables
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
               result = tparcel.GetType().FullName;

              return result;
            }
            
            
            
            /// <summary>
            /// Used by serialization
            /// </summary>
            protected Parcel()
            {       

            }

            /// <summary>
            /// Called when creating new Parcel instances by the original author.
            /// The new instance is in 'ParcelState.Creating' state
            /// </summary>
            protected Parcel(GDID id, object payload = null)
            {
                m_GDID = id;
                m_State = ParcelState.Creating;
                m_PayloadUnwrapped = true;
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
               m_State = ParcelState.Sealed;
               m_PayloadUnwrapped = true;
               m_Payload = payload;
               m_ReplicationVersionInfo = versInfo;
            }


            
        #endregion
        
        #region Fields
            

            private GDID m_GDID;

            private string m_WrappedPayloadFormat; //attributes such as: serialization format slim/POD IF this parcel has ParcelPayloadWrappingMode==Wrapped, null otherwise
            private object m_PayloadData;//Either a byte[] if ParcelPayloadWrappingMode==Wrapped, or a payload object itself (which can also be a byte[] but it will not be deserialized)

            private IReplicationVersionInfo m_ReplicationVersionInfo;//created by Seal()

           

            [NonSerialized]
            protected bool m_PayloadUnwrapped;//even if m_Payload is null we need a flag to know that we obtained null
            [NonSerialized]
            protected object m_Payload;//cached unwrapped data

            
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
            /// Returns the ID used for sharding, default implementation returns GDID. override to return another sharding key, i.e.
            ///  a social comment msg may use parent item (that the msg relates to) ID as the shard key so msgs are co-located with related items.
            ///  IMPORTANT!!! ShardingID must return an immutable value, the one that CAN NOT be changed during parcel payload life.  
            /// </summary>
            public virtual object ShardingID
            {
                get { return m_GDID; }
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
            /// Returns payload of this parcel.
            /// If the parcel is sealed and payload has not been unwrapped yet it will be unwrapped first then cached for subsequent access.
            /// May set payload only if parcel is in Creating or Modifying state (opened/not sealed).
            /// WARNING!!! Although parcels do not allow to set Payload property if they are sealed, one can still mutate/modify the payload object graph 
            /// even on a sealed parcel instance, i.e. one may write: 
            ///   <code>mySealedParcel.Payload.DueDates.Add(DateTime.Now) (given DueDates of type List(DateTime))</code>.
            /// This case is considered to be a bug in the calling-code. The framework has no way of preventing such an inadvertent behavior as there is no
            /// way to intercept a mutation via transitive or direct references/functors in an object graph referenced by Payload property because payload type
            ///  does not impose (and should not) any constraints on what can be a payload.
            /// In Aum language we will use static type checker that will detect possible property access via Payload BEFORE calling Open() 
            /// </summary>
            public object Payload
            {
                get           
                {
                  if (m_State==ParcelState.Sealed)
                    EnsurePayloadUnwrapped();
                 
                  return m_Payload;
                }
                set
                {
                  if (m_State!=ParcelState.Creating && m_State!=ParcelState.Modifying) 
                   throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("Payload.set()", GetType().FullName, m_State) );
                  
                  m_Payload = value;
                }
            }

            /// <summary>
            /// Returns true when payload data is already cached - either deserialized from internal byte[] prepped for transmission, or was created locally
            /// </summary>
            public bool PayloadUnwrapped  
            {
                get { return m_PayloadUnwrapped;}
            }


            /// <summary>
            /// Returns payload wrapped as byte[] for transmission.
            /// If payload has not been wrapped yet it will be wrapped first (serialized into byte[]) then cached for subsequent access.
            /// Parcel must be in a Sealed state for this call. Returns null if ParcelPayloadWrappingMode==NotWrapped
            /// </summary>
            internal byte[] WrappedPayload  
            {
                get
                {
                  if (MetadataAttribute.PayloadWrappingMode== ParcelPayloadWrappingMode.Wrapped)
                  {
                    EnsurePayloadWrappedCopy();
                    return m_PayloadData as byte[];
                  }

                  return null;
                }
            }

            /// <summary>
            /// Returns true to indicate that internal wrapped payload buffer is available (does not need to be wrapped)
            /// </summary>
            internal bool HasWrappedPayload    
            {
                get{ return MetadataAttribute.PayloadWrappingMode==ParcelPayloadWrappingMode.Wrapped && m_PayloadData is byte[];}
            } 


            /// <summary>
            /// Returns pauload wrap format - a type of serialization used to wrap the payload
            /// Parcel must be in a Sealed state for this call. Returns null if ParcelPayloadWrappingMode==NotWrapped
            /// </summary>
            public string WrappedPayloadFormat
            {
               get
               {
                 if (MetadataAttribute.PayloadWrappingMode== ParcelPayloadWrappingMode.Wrapped)
                 { 
                   EnsurePayloadWrappedCopy();
                   return m_WrappedPayloadFormat;
                 }
                 return null;
               }
            }

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
            /// The default implementation returns null.
            /// Override to supply a different name of caching table
            ///  that may depend on particular parcel payload state (i.e. field values). 
            /// Example: store 'ultra hot' items in a dedicated cache table 
            /// </summary>
            public virtual string CacheTableName
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
            /// Returns DataParcelAttribute that describes this parcel
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
            public IParcelCachePolicy EffectiveCachePolicy
            {
              get
              {
                 var result =  new ParcelCachePolicyData
                 {
                   CacheReadMaxAgeSec  = this.CacheReadMaxAgeSec.HasValue ? this.CacheReadMaxAgeSec.Value : MetadataAttribute.CacheReadMaxAgeSec,
                   CacheWriteMaxAgeSec = this.CacheWriteMaxAgeSec.HasValue ? this.CacheWriteMaxAgeSec.Value : MetadataAttribute.CacheWriteMaxAgeSec,
                   CachePriority       = this.CachePriority.HasValue ? this.CachePriority : MetadataAttribute.CachePriority,
                   CacheTableName      = this.CacheTableName.IsNotNullOrWhiteSpace() ? this.CacheTableName : MetadataAttribute.CacheTableName,
                   CacheAbsoluteExpirationUTC = this.CacheAbsoluteExpirationUTC.HasValue ? this.CacheAbsoluteExpirationUTC.Value : MetadataAttribute.CacheAbsoluteExpirationUTC
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

                EnsurePayloadUnwrapped();
                m_PayloadData = null;
                m_WrappedPayloadFormat = null;
                
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
            /// Forgets wrapped payload copy by deallocating byte[] that stores serialized payload.
            /// If parcel is ParcelPayloadWrappingMode.NotWrapped then forgets a reference to payload data which is transmitted(serialized).
            /// This method is usually used by business code after Parcel instance gets sent to backend, then this method is called, then
            ///  the instance gets written into local in-memory field/cache (same process address space).
            /// WARNING!!! Parcel instances are NOT thread-safe, they can not be mutated by multiple threads at the same time, so
            ///  if a parcel instance needs to be cached for subsequent parallel modifications then DeepClone() should be used.
            ///  DeepClone() will recreate the wrapped payload buffer, so this method does not need to be called in this case
            /// Parcel must be in a Sealed state for this call
            /// </summary>
            public void ForgetWrappedPayloadCopy()     
            {
                if (m_State!=ParcelState.Sealed)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("ForgetWrappedPayloadCopy", GetType().FullName, m_State) );
                EnsurePayloadUnwrapped();
                m_PayloadData = null;
                m_WrappedPayloadFormat = null;
            }

            /// <summary>
            /// Forgets the unwrapped payload object graph.
            /// This method is used by cache store that holds data in byte[] anyway because it needs to do DeepClone() for cache hits,
            ///  consequently the redundant object graph is not needed.
            /// Parcel must be in a Sealed state for this call 
            /// </summary>
            public void ForgetPayloadCopy() 
            {
                if (m_State!=ParcelState.Sealed)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("ForgetPayloadCopy", GetType().FullName, m_State) );
                EnsurePayloadWrappedCopy();
                m_Payload = null;
                m_PayloadUnwrapped = false;
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

              using(var ms = new System.IO.MemoryStream(GetEstimatedWrappedPayloadBufferSize()))
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

            public ulong GetULongHash()
            {
              return (ulong)this.GetType().GetHashCode() ^ m_GDID.GetULongHash();
            }

            public override string ToString()
            {
              return "{0}[{1}:{2}]({3})".Args(
                            GetType().DisplayNameWithExpandedGenericArgs(),
                            m_GDID,
                            m_State,
                            (m_Payload != null) ? m_Payload.GetType().DisplayNameWithExpandedGenericArgs() : "<null/wrapped>"
                         );
            }      


        #endregion


        #region Protected
           
            /// <summary>
            /// Checks to see if payload is already unwrapped and does nothing if it is.
            /// Otherwise, unwraps the payload by deserializing byte[] stream into payload object.
            /// If this parcel is ParcelPayloadWrappingMode.NotWrapped then just swaps references.
            /// Parcel must be in a Sealed state for this call
            /// </summary>
            protected void EnsurePayloadUnwrapped()
            {
              if (m_State!=ParcelState.Sealed)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("EnsurePayloadUnwrapped", GetType().FullName, m_State) );
              if (m_PayloadUnwrapped) return;
              
              if (MetadataAttribute.PayloadWrappingMode==ParcelPayloadWrappingMode.Wrapped)
              {
                  OnBeforePayloadUnwrap();
              
                  m_Payload = DoUnwrapPayload();
                  m_PayloadUnwrapped = true;

                  OnAfterPayloadUnwrap();
              }
              else
              {
                  m_Payload = m_PayloadData;
                  m_PayloadUnwrapped = true;
              }
            }
            
            /// <summary>
            /// Checks to see if payload wrapped copy is present and does nothing if it is.
            /// Otherwise, wraps the payload by serializing payload into a byte[], thus creating payload copy for wire transmission.
            /// If this parcel is ParcelPayloadWrappingMode.NotWrapped then just swaps references.
            /// Parcel must be in a Sealed state for this call
            /// </summary>
            protected void EnsurePayloadWrappedCopy()
            {
              if (m_State!=ParcelState.Sealed)
                 throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR.Args("EnsurePayloadWrappedCopy", GetType().FullName, m_State) );
              if (m_PayloadData!=null) return;
             
              if (MetadataAttribute.PayloadWrappingMode==ParcelPayloadWrappingMode.Wrapped)
              {

                OnBeforePayloadWrap();

                m_PayloadData = DoWrapPayloadCopy(out m_WrappedPayloadFormat);
              
                OnAfterPayloadWrap();
              }
              else
              {
                m_PayloadData = m_Payload;
                m_WrappedPayloadFormat = null;
              }
            }
            
            
            
            /// <summary>
            /// Called only if this parcel is ParcelPayloadWrappingMode.Wrapped.
            /// Override to perform custom content deserialization, i.e. when particular store may use special format for data marshalling.
            /// Base implementation understands  Parcel.STANDARD_SLIM_PAYLOAD_FORMAT and Parcel.STANDARD_PODSLIM_PAYLOAD_FORMAT formats
            /// </summary>
            protected virtual object DoUnwrapPayload()
            {
                var wrapped = m_PayloadData as byte[];
                if (wrapped==null) return null;


                ISerializer serializer = null;
                if (m_WrappedPayloadFormat==STANDARD_SLIM_PAYLOAD_FORMAT)
                  serializer = new SlimSerializer( STANDARD_KNOWN_SERIALIZER_TYPES );
                else if (m_WrappedPayloadFormat==STANDARD_PODSLIM_PAYLOAD_FORMAT)
                  serializer = new PODSlimSerializer();
                else
                  throw new DistributedDataParcelSerializationException(
                              StringConsts.DISTRIBUTED_DATA_PARCEL_UNWRAP_FORMAT_ERROR.Args(GetType().FullName, m_WrappedPayloadFormat ?? StringConsts.NULL_STRING));
                
                try
                {
                  using(var ms = new System.IO.MemoryStream( wrapped ))
                    return serializer.Deserialize( ms );
                }
                catch(Exception error)
                {
                  throw new DistributedDataParcelSerializationException(
                             StringConsts.DISTRIBUTED_DATA_PARCEL_UNWRAP_DESER_ERROR.Args(GetType().FullName, error.ToMessageWithType())
                             , error);
                } 
            }
            
            /// <summary>
            /// Called only if this parcel is ParcelPayloadWrappingMode.Wrapped.
            /// Override to perform custom content serialization, i.e. when particular store may use special format for data marshalling.
            /// Base implementation uses  Parcel.STANDARD_SLIM_PAYLOAD_FORMAT with SlimSerializer
            /// </summary>
            protected virtual byte[] DoWrapPayloadCopy(out string format)
            {
                if (m_Payload==null)
                {
                  format = null;
                  return null;
                }
                
                try
                {
                  ISerializer serializer = new SlimSerializer( STANDARD_KNOWN_SERIALIZER_TYPES );
                  using(var ms = new System.IO.MemoryStream(GetEstimatedWrappedPayloadBufferSize())) //todo Instrument!!!  
                  {
                    
                    serializer.Serialize(ms, m_Payload );

                    format = STANDARD_SLIM_PAYLOAD_FORMAT;

                    var buffer = ms.GetBuffer();
                    if (ms.Capacity - ms.Length < REALLOC_EXCESS_THRESHOLD)
                     return buffer;//do not make a copy
 //todo Instrument!!!  PREALLOCATIOn vs ACTUAL SIZE at the end!!!!
                    //make a trimmed buffer copy
                    var data = new byte[ms.Length];
                    Buffer.BlockCopy(buffer, 0, data, 0, (int)ms.Length);
                    return data;
                  }
                }
                catch(Exception error)
                {
                  throw new DistributedDataParcelSerializationException(
                             StringConsts.DISTRIBUTED_DATA_PARCEL_UNWRAP_DESER_ERROR.Args(GetType().FullName, error.ToMessageWithType())
                             , error);
                }
            }

            /// <summary>
            /// Override to provide a better estimate for buffer size needed to serialize the Payload instance into WrappedPayload which is a byte[].
            /// Used by cloning and wrapped payload serialization. 
            /// This implementation returns default buffer size if wrapped payload is null
            /// </summary>
            protected virtual int GetEstimatedWrappedPayloadBufferSize()
            {
              if (m_PayloadData!=null &&
                  MetadataAttribute.PayloadWrappingMode==ParcelPayloadWrappingMode.Wrapped)
               return ((byte[])m_PayloadData).Length;

              return DEFAULT_WRAPPED_PAYLOAD_BUFFER_SIZE;
            }

            /// <summary>
            /// Override to initialize internal Parcel state right before payload gets unwrapped - deserialized from byte[], 
            ///  i.e.  this may be used to clear some internal fields. Base implementation does nothing
            /// </summary>
            protected virtual void OnBeforePayloadUnwrap()
            {

            }
           
            /// <summary>
            /// Override to initialize internal Parcel state right after payload gets unwrapped - deserialized from byte[], 
            ///  i.e.  this may be used to cache or clear some internal fields. Base implementation does nothing
            /// </summary>
            protected virtual void OnAfterPayloadUnwrap()
            {

            }

            /// <summary>
            /// Override to initialize internal Parcel state right before payload gets wrapped - serialized into byte[], 
            ///  i.e.  this may be used to set some internal fields used for caching, i.e. copy user rating field from complex user object(payload)
            /// which will be wrapped into byte[] into separate field that may influence Parcel caching policy. Base implementation does nothing  
            /// </summary>
            protected virtual void OnBeforePayloadWrap()
            {

            }
            
            /// <summary>
            /// Override to initialize internal Parcel state right after payload gets wrapped - serialized into byte[], 
            ///  i.e.  this may be used to clear some internal fields. Base implementation does nothing
            /// </summary>
            protected virtual void OnAfterPayloadWrap()
            {

            }



            
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
              EnsurePayloadWrappedCopy();
           }

        #endregion


           
    }

    /// <summary>
    /// Describes a data parcel - a piece of logically-grouped data that gets fetched from/comitted into a distributed backend system.
    /// Parcels represent an atomic unit of change, a changeset that gets replicated between failover hosts.
    /// Every parcel has a Payload property that stores business data of interest that the parcel contains.
    /// This class is designed in such way that Payload property (unwrapped data) is not serialized, instead a byte[](wrapped data) is serialized.
    /// This is needed because an instance of Parcel may travel between many hosts that do not need to serialize/deserialize possibly complex business
    /// object that parcel contains.
    /// Parcels wrap their payload - when parcels get transported between hosts only parcel metadata (Parcel class fields) get serialized/deserialized by 
    /// hosts in the data supply chain, so metadata is available for tasks like parcel routing and cache policy adjustment, but the business data (the payload) must be 
    /// unwrapped first before it can be used. This design promotes efficient storage in distributed cache systems, i.e. the data origination host may keep cached version
    /// in an unwrapped state (not serialized) so business payload is ready for access right away without deserialization. 
    /// On the other hand, the intermediary parcel relays do not need to deserialize/serialize payload every time as it may be complex and waste significant processing time.
    /// This class is not thread-safe.
    /// This particular class serves as a very base for distributed data store implementations
    /// </summary>
    /// <typeparam name="TPayload">Type of payload that parcel carries</typeparam>
    [Serializable]
    public abstract class Parcel<TPayload> : Parcel where TPayload : class
    {
        #region .ctor

            protected Parcel(GDID id, TPayload payload = null) : base(id, payload)
            {
            }

            protected Parcel(GDID id, TPayload payload, IReplicationVersionInfo versInfo) : base(id, payload, versInfo)
            {

            }

        #endregion
        
        #region Properties
           /// <summary>
           /// Returns payload of this parcel.
           /// If the parcel is sealed and payload has not been unwrapped yet it will be unwrapped first then cached for subsequent access.
           /// May set payload only if parcel is in Creating or Modifying state (opened/not sealed)
           /// </summary>
           public new TPayload Payload
           {
              get{ return (TPayload)base.Payload; }
              set { base.Payload = value;}
           }
        #endregion
    }


}
