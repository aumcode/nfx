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
using System.IO;
using System.Runtime.Serialization;

using NFX.IO;
using System.Reflection;

namespace NFX.Serialization.Slim
{
    /// <summary>
    /// Implements Slim serialization algorithm that relies on an injectable SlimFormat-derivative (through .ctor) paremeter.
    /// This class was designed for highly-efficient serialization of types without versioning.
    /// SlimSerializer supports a concept of "known types" that save space by not emitting their names into stream.
    /// Performance note:
    /// This serializer yields on average 1/4 serialization and 1/2 deserialization times while compared to BinaryFormatter.
    /// Serialization of Record-instances usually takes 1/6 of BinaryFormatter time.
    /// Format takes 1/10 space for records and 1/2 for general object graphs.
    /// Such performance is achieved because of dynamic compilation of type-specific serialization/deserialization methods.
    /// This type is thread-safe for serializations/deserializations when TypeMode is set to "PerCall"
    /// </summary>
    public class SlimSerializer : ISlimSerializer
    {
        #region CONSTS

           public const ushort HEADER = (ushort)0xCAFE; 

        #endregion

        #region .ctor
            
            
            public SlimSerializer() : this(SlimFormat.Instance)
            {
               
            }

            public SlimSerializer(params IEnumerable<Type>[] globalTypes) : this(SlimFormat.Instance, globalTypes)
            {

            }
            
            public SlimSerializer(SlimFormat format)
            {
              if (format==null)
               throw new SlimException(StringConsts.ARGUMENT_ERROR + "SlimSerializer.ctor(format=null)");

              m_Format = format;
              m_TypeMode = TypeRegistryMode.PerCall;
            }
            
            public SlimSerializer(SlimFormat format, params IEnumerable<Type>[] globalTypes) : this(format)
            {
              m_GlobalTypes = globalTypes;
            }

            
            internal SlimSerializer(TypeRegistry global, SlimFormat format) : this(format)
            {
              __globalTypeRegistry = global;
              m_GlobalTypes = new IEnumerable<Type>[]{ global.ToArray() };
              m_SkipTypeRegistryCrosschecks = true;
            }


        #endregion

        #region Fields

          internal readonly TypeRegistry __globalTypeRegistry;

          private SlimFormat m_Format;
          private IEnumerable<Type>[] m_GlobalTypes;
          private TypeRegistryMode m_TypeMode;

          private TypeRegistry m_BatchTypeRegistry;
          private int m_BatchTypeRegistryPriorCount;

          private bool m_SkipTypeRegistryCrosschecks;

          /// <summary>
          /// Associates arbitrary owner object with this instance. Slim serializer does not use this field internally for any purpose
          /// </summary>
          public object Owner;

        #endregion

        #region Properties

          

          public SlimFormat Format { get { return m_Format;} }
          
          /// <summary>
          /// Gets/sets how serializer handles type information between calls to Serialize/Deserialize.
          /// Setting this to "Batch" makes this serializer instance not thread-safe for calling Serialize/Deserialize.
          /// This property itself is not thread-safe, that is - it should be only set once by control/initiating thread
          /// </summary>
          public TypeRegistryMode TypeMode
          {
             get{ return m_TypeMode;}
             set
             {
               if (m_TypeMode==value) return;
               
               if (value == TypeRegistryMode.Batch)
               {
                  m_BatchTypeRegistry = new TypeRegistry(m_GlobalTypes);
                  m_BatchTypeRegistryPriorCount = m_BatchTypeRegistry.Count;
               }
               else
                m_BatchTypeRegistry= null;

               m_TypeMode = value;
             }
          }  

          /// <summary>
          /// Returns true when TypeMode is "PerCall"
          /// </summary>
          public bool IsThreadSafe { get{ return m_TypeMode == TypeRegistryMode.PerCall;}}


          /// <summary>
          /// Returns true if last call to Serialize or Deserialize in batch mode added more types to type registry.
          /// This call is only valid in TypeMode = "Batch" and is inherently not thread-safe
          /// </summary>
          public bool BatchTypesAdded
          {
             get { return m_TypeMode==TypeRegistryMode.Batch && m_BatchTypeRegistryPriorCount!=m_BatchTypeRegistry.Count;}
          }

          /// <summary>
          /// ADVANCED FEATURE! Developers do not use. 
          /// Returns type registry used in batch.
          /// This call is only valid in TypeMode = "Batch" and is inherently not thread-safe.
          /// Be careful not to mutate the returned object
          /// </summary>
          public TypeRegistry BatchTypeRegistry
          {
             get { return m_BatchTypeRegistry;}
          }

        #endregion


        #region Public
          
            /// <summary>
            /// Resets type registry to initial state (which is based on global types) for TypeMode = "Batch",
            /// otherwise does nothing. This method is not thread-safe
            /// </summary>
            public void ResetCallBatch()
            {
              if (m_TypeMode!=TypeRegistryMode.Batch) return;
              m_BatchTypeRegistry = new TypeRegistry(m_GlobalTypes);
              m_BatchTypeRegistryPriorCount = m_BatchTypeRegistry.Count;
            }

               private int m_SerializeNestLevel;
               private SlimWriter m_CachedWriter;

            public void Serialize(Stream stream, object root)
            {
                try
                {
                    var singleThreaded = m_TypeMode == TypeRegistryMode.Batch;
                    
                    SlimWriter writer;
                    
                    if (!singleThreaded || m_SerializeNestLevel >0)
                     writer = m_Format.MakeWritingStreamer();
                    else
                    {
                      writer = m_CachedWriter;
                      if (writer==null) 
                      {
                        writer = m_Format.MakeWritingStreamer();
                        m_CachedWriter = writer;
                      }
                    }
                    
                    var pool = reservePool();
                    try
                    {
                     m_SerializeNestLevel++;
                     writer.BindStream( stream );

                     serialize(writer, root, pool);
                    }
                    finally
                    {
                      writer.UndindStream();
                      m_SerializeNestLevel--;
                      releasePool(pool);
                    }
                }
                catch(Exception error)
                {
                    throw new SlimSerializationException(StringConsts.SLIM_SERIALIZATION_EXCEPTION_ERROR + error.ToMessageWithType(), error);
                }
            }

               private int m_DeserializeNestLevel;
               private SlimReader m_CachedReader;

            public object Deserialize(Stream stream)
            {
                try
                {
                    var singleThreaded = m_TypeMode == TypeRegistryMode.Batch;
                    
                    SlimReader reader;
                    
                    if (!singleThreaded || m_DeserializeNestLevel>0)
                     reader = m_Format.MakeReadingStreamer();
                    else
                    {
                      reader = m_CachedReader;
                      if (reader==null) 
                      {
                        reader = m_Format.MakeReadingStreamer();
                        m_CachedReader = reader;
                      }
                    }

                    var pool = reservePool();
                    try
                    {
                       m_DeserializeNestLevel++;
                       reader.BindStream( stream );

                       return deserialize(reader, pool);
                    }
                    finally
                    {
                      reader.UndindStream();
                      m_DeserializeNestLevel--;
                      releasePool(pool);
                    }
                }
                catch(Exception error)
                {
                    throw new SlimDeserializationException(StringConsts.SLIM_DESERIALIZATION_EXCEPTION_ERROR + error.ToMessageWithType(), error);
                }
            }

        #endregion



        #region .pvt .impl

            [ThreadStatic]
            private static RefPool[] ts_pools;
            [ThreadStatic]
            private static int ts_poolFreeIdx;

            private static RefPool reservePool()
            {
              RefPool result = null;
              if (ts_pools==null)
              {
                ts_pools = new RefPool[8];
                for(var i=0; i<ts_pools.Length; i++)
                 ts_pools[i] = new RefPool(); 
                ts_poolFreeIdx = 0;
              }

              if (ts_poolFreeIdx<ts_pools.Length)
              {
                result = ts_pools[ts_poolFreeIdx];
                ts_poolFreeIdx++;
              }
              else
               result = new RefPool();

              return result;
            }

            private static void releasePool(RefPool pool)
            {
              if (ts_poolFreeIdx==0) return;
              pool.Reuse();
              ts_poolFreeIdx--;
              ts_pools[ts_poolFreeIdx] = pool;
            }


            private void serialize(SlimWriter writer, object root, RefPool pool)
            {                           
               if (root is Type)
                 root = new rootTypeBox{ TypeValue = (Type)root};

               var scontext = new StreamingContext();
               var registry = (m_TypeMode == TypeRegistryMode.PerCall) ? new TypeRegistry(m_GlobalTypes) : m_BatchTypeRegistry;
               var type = root!=null? root.GetType() : typeof(object);
               var isValType = type.IsValueType;
                               
               
                 writeHeader(writer);
                 var rcount = registry.Count;
                 m_BatchTypeRegistryPriorCount = rcount;
                
                 if (!m_SkipTypeRegistryCrosschecks)
                 {
                    writer.Write( (uint)rcount );
                    writer.Write( registry.CSum );
                 }      
                                            

                 //Write root in pool if it is reference type
                 if (!isValType && root!=null)
                   pool.Add(root);
                 m_Format.TypeSchema.Serialize(writer, registry, pool, root, scontext);

                 
                 if (root==null) return;

                 var i = 1;

                 if (!isValType) i++;
                     
                 //Write all the rest of objects. The upper bound of this loop may increase as objects are written
                 //0 = NULL
                 //1 = root IF root is ref type
                 var ts = m_Format.TypeSchema;
                 for(; i<pool.Count; i++)
                 {
                    var instance = pool[i];
                    var tinst = instance.GetType();
                    if (!m_Format.IsRefTypeSupported(tinst))
                      ts.Serialize(writer, registry, pool, instance, scontext); 
                 }

            }

            private object deserialize(SlimReader reader, RefPool pool)
            {
               object root = null;

               var scontext = new StreamingContext();
               var registry = (m_TypeMode == TypeRegistryMode.PerCall) ? new TypeRegistry(m_GlobalTypes) : m_BatchTypeRegistry;
          
               {
                 var rcount = registry.Count;
                 m_BatchTypeRegistryPriorCount = rcount;
                 
                 readHeader(reader);
                 if (!m_SkipTypeRegistryCrosschecks)
                 {
                     if (reader.ReadUInt()!=rcount)
                        throw new SlimDeserializationException(StringConsts.SLIM_TREG_COUNT_ERROR);
                     if (reader.ReadULong()!= registry.CSum)
                        throw new SlimDeserializationException(StringConsts.SLIM_TREG_CSUM_ERROR);
                 }

                 //Read root                  
                 //Deser will add root to pool[1] if its ref-typed
                 //------------------------------------------------
                 root = m_Format.TypeSchema.DeserializeRootOrInner(reader, registry, pool, scontext, root: true );
                 if (root==null) return null;
                 if (root is rootTypeBox) return ((rootTypeBox)root).TypeValue;
                 

                 var type = root.GetType();
                 var isValType = type.IsValueType;

                 var i = 1;

                 if (!isValType) i++;
                     
                 //Read all the rest of objects. The upper bound of this loop may increase as objects are read and their references added to pool
                 //0 = NULL
                 //1 = root IF root is ref type
                 //-----------------------------------------------
                 var ts = m_Format.TypeSchema;
                 for(; i<pool.Count; i++)
                 {
                    var instance = pool[i];
                    var tinst = instance.GetType();
                    if (!m_Format.IsRefTypeSupported(tinst))
                      ts.DeserializeRefTypeInstance(instance, reader, registry, pool, scontext); 
                 }

               }

               //perform fixups for ISerializable
               //---------------------------------------------
               var fxps = pool.Fixups;
               for(var i=0; i<fxps.Count;i++)
               {                            
                 var fixup = fxps[i];  
                 var t = fixup.Instance.GetType();
                 var ctor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                                             null,
                                             new Type[] { typeof(SerializationInfo), typeof(StreamingContext)},
                                             null);
                 if (ctor==null)
                  throw new SlimDeserializationException(StringConsts.SLIM_ISERIALIZABLE_MISSING_CTOR_ERROR + t.FullName); 
                 ctor.Invoke(fixup.Instance, new object[]{ fixup.Info, scontext} );
               }  


               //20150214 DD - fixing deserialization problem of Dictionary(InvariantStringComparer)
               //before 20150214 this was AFTER OnDeserialization
               //invoke OnDeserialized-decorated methods
               //--------------------------------------------
               var odc = pool.OnDeserializedCallbacks;
               for(int i=0; i<odc.Count; i++)
               {
                 var cb = odc[i];
                 cb.Descriptor.InvokeOnDeserializedCallbak(cb.Instance, scontext);
               }

               //before 20150214 this was BEFORE OnDeserializedCallbacks
               //invoke IDeserializationCallback
               //---------------------------------------------
               for(int i = 1; i<pool.Count; i++)//[0]=null
               {
                 var dc = pool[i] as IDeserializationCallback;
                 if (dc!=null)
                 try
                 {
                    dc.OnDeserialization(this);
                 }
                 catch(Exception error)
                 {
                    throw new SlimDeserializationException(StringConsts.SLIM_DESERIALIZE_CALLBACK_ERROR + error.ToMessageWithType(), error);
                 }
               }
               
               

               return root;
            }



                        private void writeHeader(SlimWriter writer)
                        {
                           writer.Write((byte)0);
                           writer.Write((byte)0);
                           writer.Write((byte)((HEADER >> 8) & 0xff));
                           writer.Write((byte)(HEADER & 0xff));
                        }

                        private void readHeader(SlimReader reader)
                        {
                           if (reader.ReadByte() != 0 ||
                               reader.ReadByte() != 0 ||
                               reader.ReadByte() != (byte)((HEADER >> 8) & 0xff) ||
                               reader.ReadByte() != (byte)(HEADER & 0xff)
                              ) throw new SlimDeserializationException(StringConsts.SLIM_BAD_HEADER_ERROR);
                        }


                        private struct rootTypeBox
                        {
                          public Type TypeValue;
                        }

        #endregion
   
    }


    internal class _ISerializableFixup
    {
       public object Instance;
       public SerializationInfo Info;
    } 

    internal class _OnDeserializedCallback
    {
       public object Instance;
       public TypeDescriptor Descriptor;
    } 



    internal class RefPool 
    {
       public RefPool()
       {
         m_List.Add(null);// null must be at index ZERO
       }

       //resets inner state so the instance may be re-used
       public void Reuse()
       {
         m_List.Clear();
         m_List.Add(null);// null must be at index ZERO
         m_Dict.Clear();
         m_Fixups.Clear();
         m_OnDeserializedCallbacks.Clear();
       }

       
       private List<object> m_List = new List<object>(128);
       private Dictionary<object, int> m_Dict = new Dictionary<object,int>(128, ReferenceEqualityComparer<object>.Instance);
       private List<_ISerializableFixup> m_Fixups = new List<_ISerializableFixup>();
       private List<_OnDeserializedCallback> m_OnDeserializedCallbacks = new List<_OnDeserializedCallback>(); 
        
       public int Count { get { return m_List.Count;} }

       public List<_ISerializableFixup> Fixups { get { return m_Fixups; }}

       public List<_OnDeserializedCallback> OnDeserializedCallbacks { get { return m_OnDeserializedCallbacks; }}
       
       public object this[int i]
       {
         get { return m_List[i]; }
         set { m_List[i] = value; }
       }
       
       public bool Add(object reference)
       {
         bool added;
         getIndex(reference, out added);
         return added;
       }

       public void AddISerializableFixup(object instance, SerializationInfo info)
       {
         m_Fixups.Add( new _ISerializableFixup{ Instance = instance, Info = info } );
       }

       public void AddOnDeserializedCallback(object instance, TypeDescriptor descriptor)
       {
         m_OnDeserializedCallbacks.Add( new _OnDeserializedCallback{ Instance = instance, Descriptor = descriptor } );
       }

       /// <summary>
       /// Emits MetaHandle that contains type handle for reference handle only when this referenced is added to pool for the first time.
       /// Emits inlined string for strings and inlined value types for boxed objects.
       /// Emits additional array dimensions info for array refernces who's types are emitted for the first time
       /// </summary>
       public MetaHandle GetHandle(object reference, TypeRegistry treg, SlimFormat format, out Type type)
       {
         if (reference==null)
         {
           type = null;
           return new MetaHandle(0);
         }
         
         if (reference is string)
         {
           type = typeof(string);
           return MetaHandle.InlineString(reference as string);
         }

         if (reference is Type)
         {
           type = typeof(Type);
           var thandle = treg.GetTypeHandle(reference as Type); 
           return MetaHandle.InlineTypeValue(thandle);
         }

         type = reference.GetType();
         if (type.IsValueType)
         {
           var vth = treg.GetTypeHandle(type);
           return MetaHandle.InlineValueType(vth);
         }
         
         bool added;

         uint handle = (uint)getIndex(reference, out added);
         string th = added ? treg.GetTypeHandle(type) : null; 

         if (added)
         {
              if (format.IsRefTypeSupported(type))//20150305 Refhandle inline
                return MetaHandle.InlineRefType(th);

              if (reference is Array)//write array header like so:  "System.int[,]|0~10,0~12" or "$3|0~10,0~12"
              {
                //DKh 20130712 Removed repetitive code that was refactored into Arrays class
                var arr = (Array)reference;
                th = Arrays.ArrayToDescriptor(arr, treg, type, th);
              }
         }
         return new MetaHandle(handle, th);
       } 


       /// <summary>
       /// Returns object reference for supplied metahandle
       /// </summary>
       public object HandleToReference(MetaHandle handle, TypeRegistry treg, SlimFormat format, SlimReader reader)
       {
         if (handle.IsInlinedString) return handle.Metadata;
         if (handle.IsInlinedTypeValue) 
         {
           var tref = treg.GetByHandle(handle.Metadata);//adding this type to registry if it is not there yet
           return tref;
         }

         if (handle.IsInlinedRefType)
         {
             var tref = treg.GetByHandle(handle.Metadata);//adding this type to registry if it is not there yet
             var ra = format.GetReadActionForRefType(tref);
             if (ra!=null)
             {
               var inst = ra(reader);
               m_List.Add(inst);
               m_Dict.Add(inst, m_List.Count - 1);
               return inst;
             }
             else
              throw new SlimDeserializationException("Internal error HandleToReference: no read action for ref type, but ref mhandle is inlined");
         }

         
         int idx = (int)handle.Handle;
         if (idx<m_List.Count) return m_List[idx];
         
         if (string.IsNullOrEmpty(handle.Metadata))
          throw new SlimDeserializationException(StringConsts.SLIM_HNDLTOREF_MISSING_TYPE_NAME_ERROR + handle.ToString());

         var metadata = handle.Metadata;
         var ip = metadata.IndexOf('|');
         //var segments = metadata.Split('|');
         var th = ip>0 ? metadata.Substring(0, ip) : metadata;

         //20140701 DKh 
         var type = treg[th];//segments[0]];
         
         object instance = null;
         
         if (type.IsArray)
              //DKh 20130712 Removed repetitive code that was refactored into Arrays class
              instance = Arrays.DescriptorToArray(metadata, treg, type); 
         else
              //20130715 DKh
              instance = SerializationUtils.MakeNewObjectInstance(type);

         m_List.Add(instance);
         m_Dict.Add(instance, m_List.Count - 1);
         return instance;
       }

       private int getIndex(object reference, out bool added)
       {
           added = false;
           if (reference==null) return 0;

           int idx = -1;
           if (m_Dict.TryGetValue(reference, out idx)) return idx;
                      

           added = true;
           m_List.Add(reference);
           idx = m_List.Count - 1;
           m_Dict.Add(reference, idx);
           return idx;
      }



    }

}
