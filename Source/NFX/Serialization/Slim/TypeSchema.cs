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
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;

using NFX.IO;


namespace NFX.Serialization.Slim
{
    internal delegate void dyn_serialize(TypeSchema schema, SlimWriter writer, TypeRegistry tregistry, RefPool refs, object instance, StreamingContext context);
    internal delegate void dyn_deserialize(TypeSchema schema, SlimReader reader, TypeRegistry tregistry, RefPool refs, ref object instance, StreamingContext context);


    /// <summary>
    /// Type descriptor dynamicaly compiles serialization/deserialization expressions for a particular type
    /// </summary>
    internal class TypeDescriptor
    {
      public TypeDescriptor(TypeSchema schema, Type type)
      {
        if (Attribute.IsDefined(type, typeof(SlimSerializationProhibitedAttribute)))
          throw new SlimSerializationProhibitedException(type);

        Schema = schema;
        Format = schema.Format;
        Type = type;

        IsPrimitive = Format.IsTypeSupported(type);
        IsArray = type.IsArray;
        IsPrimitiveArray = IsArray && IsPrimitive;

        if (!IsArray)
            Fields = SerializationUtils.GetSerializableFields(type).ToArray();


        //If type implements ISerializable than that should be used instead of dyn methods
        if (type.GetInterfaces().Contains(typeof(ISerializable)))
        {
          CustomIsSerializable = true;
        }
        else
        {
           m_Serialize = makeSerialize();
           m_Deserialize = makeDeserialize();
        }

        //Query for "On..." family of attributes
        m_MethodsOnSerializing   = findAttributedMethods(typeof(OnSerializingAttribute));
        m_MethodsOnSerialized    = findAttributedMethods(typeof(OnSerializedAttribute));
        m_MethodsOnDeserializing = findAttributedMethods(typeof(OnDeserializingAttribute));
        m_MethodsOnDeserialized  = findAttributedMethods(typeof(OnDeserializedAttribute));

      }

      public readonly TypeSchema Schema;
      public readonly SlimFormat Format;
      public readonly Type Type;
      public readonly FieldInfo[] Fields;
      public readonly bool CustomIsSerializable;
      public readonly bool IsPrimitive;
      public readonly bool IsArray;
      public readonly bool IsPrimitiveArray;

      private dyn_serialize m_Serialize;
      private dyn_deserialize m_Deserialize;


      private List<MethodInfo> m_MethodsOnSerializing;
      private List<MethodInfo> m_MethodsOnSerialized;
      private List<MethodInfo> m_MethodsOnDeserializing;
      private List<MethodInfo> m_MethodsOnDeserialized;

      public void SerializeInstance(SlimWriter writer, TypeRegistry registry, RefPool refs, object instance, StreamingContext streamingContext)
      {
        if (m_MethodsOnSerializing!=null)
          invokeAttributedMethods(m_MethodsOnSerializing, instance, streamingContext);

            if (m_Serialize!=null)
            {
                 m_Serialize(Schema, writer, registry, refs, instance, streamingContext);
            }
            else
            {
                ISerializable isz = instance as ISerializable;
                var info = new SerializationInfo(Type, new FormatterConverter());
                isz.GetObjectData(info, streamingContext);

                serializeInfo(writer, registry, refs, info, streamingContext);
            }

        if (m_MethodsOnSerialized!=null)
          invokeAttributedMethods(m_MethodsOnSerialized, instance, streamingContext);
      }

      public void DeserializeInstance(SlimReader reader, TypeRegistry registry, RefPool refs, ref object instance, StreamingContext streamingContext)
      {
         if (m_MethodsOnDeserializing!=null)
          invokeAttributedMethods(m_MethodsOnDeserializing, instance, streamingContext);

            if (m_Deserialize!=null)
            {
                 m_Deserialize(Schema, reader, registry, refs, ref instance, streamingContext);
            }
            else
            {
                var info = deserializeInfo(reader, registry, refs, streamingContext);
                refs.AddISerializableFixup(instance, info);
            }

        if (m_MethodsOnDeserialized!=null)
        {
           refs.AddOnDeserializedCallback(instance, this);
        }
      }


      public void InvokeOnDeserializedCallbak(object instance, StreamingContext streamingContext )
      {
        if (m_MethodsOnDeserialized!=null)
           invokeAttributedMethods(m_MethodsOnDeserialized, instance, streamingContext);
      }


           private void invokeAttributedMethods(List<MethodInfo> methods, object instance, StreamingContext streamingContext)
           {
                //20130820 DKh refactored into common code
                SerializationUtils.InvokeSerializationAttributedMethods(methods, instance, streamingContext);
           }

           private List<MethodInfo> findAttributedMethods(Type atype)
           {
                //20130820 DKh refactored into common code
                return SerializationUtils.FindSerializationAttributedMethods(Type, atype);
           }

           private dyn_serialize makeSerialize()
           {
             var _walkArrayWrite = typeof(TypeDescriptor).GetMethod("walkArrayWrite", BindingFlags.NonPublic | BindingFlags.Static);

              var pSchema   = Expression.Parameter(typeof(TypeSchema));
              var pWriter   = Expression.Parameter(typeof(SlimWriter));
              var pTReg     = Expression.Parameter(typeof(TypeRegistry));
              var pRefs     = Expression.Parameter(typeof(RefPool));
              var pInstance = Expression.Parameter(typeof(object));
              var pStreamingContext = Expression.Parameter(typeof(StreamingContext));

              var expressions = new List<Expression>();

              var instance = Expression.Variable(Type, "instance");

              expressions.Add( Expression.Assign(instance, Expression.Convert(pInstance, Type)));


              if (IsPrimitive)
              {
                expressions.Add(  Expression.Call(pWriter,
                                       Format.GetWriteMethodForType(Type),
                                       instance) );
              }
              else if (IsArray)
              {
                 var elmType = Type.GetElementType();

                 if (Format.IsTypeSupported(elmType))//array element type
                 {  //spool whole array into writer using primitive types

                    var pElement = Expression.Parameter(typeof(object));
                    expressions.Add( Expression.Call(_walkArrayWrite,
                                                     instance,
                                                     Expression.Lambda(Expression.Call(pWriter,
                                                                                      Format.GetWriteMethodForType(elmType),
                                                                                      Expression.Convert(pElement, elmType)), pElement)
                                                    )
                     );
                 }
                 else
                 {  //spool whole array using TypeSchema because objects may change type
                    var pElement = Expression.Parameter(typeof(object));

                    if (!elmType.IsValueType)//reference type
                     expressions.Add( Expression.Call(_walkArrayWrite,
                                                     instance,
                                                     Expression.Lambda(Expression.Call(pSchema,
                                                                       typeof(TypeSchema).GetMethod("writeRefMetaHandle"),
                                                                        pWriter,
                                                                        pTReg,
                                                                        pRefs,
                                                                        Expression.Convert( pElement, typeof(object) ),
                                                                        pStreamingContext),
                                                                        pElement)
                                                    )
                                    );
                    else
                     expressions.Add( Expression.Call(_walkArrayWrite,
                                                     instance,
                                                     Expression.Lambda(Expression.Call(pSchema,
                                                                       typeof(TypeSchema).GetMethod("Serialize"),
                                                                       pWriter,
                                                                       pTReg,
                                                                       pRefs,
                                                                       pElement,
                                                                       pStreamingContext,
                                                                       Expression.Constant(elmType)//valueType
                                                                       ), pElement)
                                              )
                     );
                 }
              }
              else
              {
                foreach(var field in Fields)
                {
                  Expression expr = null;
                  Type t = field.FieldType;

                  if (Format.IsTypeSupported(t))
                  {
                    expr = Expression.Call(pWriter,
                                           Format.GetWriteMethodForType(t),
                                           Expression.Field(instance, field));
                  }
/* 20150316 DKh, introduction of Metahandle.InlinedTypeValue
                  else
                  if (t == typeof(Type))
                  {
                    expr = Expression.IfThenElse(
                                  Expression.Equal(Expression.Field(instance, field), Expression.Constant(null, typeof(Type))),

                                //then
                                  Expression.Call(pWriter,
                                           Format.GetWriteMethodForType(typeof(string)),
                                           Expression.Constant(null, typeof(string))),
                                //else
                                  Expression.Call(pWriter,
                                           typeof(SlimWriter).GetMethod("Write", new Type[] {typeof(string)}),
                                           Expression.Call(pTReg,
                                                           typeof(TypeRegistry).GetMethod("GetTypeHandle"),
                                                           Expression.Field(instance, field)))
                    );//ifthenelse
                  }
 */
                  else
                  if (t.IsEnum)
                  {
                    expr = Expression.Call(pWriter,
                                           Format.GetWriteMethodForType(typeof(int)),
                                           Expression.Convert(Expression.Field(instance, field), typeof(int)));

                  }
                  else // complex type ->  struct or reference
                  {
                    if (!t.IsValueType)//reference type -> write metahandle
                    {
                      expr = Expression.Call(pSchema,
                                               typeof(TypeSchema).GetMethod("writeRefMetaHandle"),
                                               pWriter,
                                               pTReg,
                                               pRefs,
                                               Expression.Convert( Expression.Field(instance, field), typeof(object) ),
                                               pStreamingContext);
                    }
                    else
                    expr = Expression.Call(pSchema,
                                           typeof(TypeSchema).GetMethod("Serialize"),
                                           pWriter,
                                           pTReg,
                                           pRefs,
                                           Expression.Convert( Expression.Field(instance, field), typeof(object) ),
                                           pStreamingContext,
                                           Expression.Constant(field.FieldType));//valueType

                  }

                  expressions.Add(expr);
                }//foreach
               }

              var body = Expression.Block(new ParameterExpression[]{instance}, expressions);

              return Expression.Lambda<dyn_serialize>(body, pSchema, pWriter, pTReg, pRefs, pInstance, pStreamingContext).Compile();
           }

             private void serializeInfo(SlimWriter writer, TypeRegistry registry, RefPool refs, SerializationInfo info, StreamingContext streamingContext)
             {
                 writer.Write(info.MemberCount);

                 var senum = info.GetEnumerator();
                 while(senum.MoveNext())
                 {
                   writer.Write(senum.Name);
                   writer.Write( registry.GetTypeHandle( senum.ObjectType ) );
                   Schema.Serialize(writer, registry, refs, senum.Value, streamingContext);
                 }
             }

             private SerializationInfo deserializeInfo(SlimReader reader, TypeRegistry registry, RefPool refs, StreamingContext streamingContext)
             {
                 var info = new SerializationInfo(Type, new FormatterConverter());

                 var cnt = reader.ReadInt();

                 for(int i=0; i<cnt; i++)
                 {
                   var name = reader.ReadString();

                   var vis = reader.ReadVarIntStr();
                   var type = registry[ vis ];
                   var obj = Schema.Deserialize(reader, registry, refs, streamingContext);

                   info.AddValue(name, obj, type);
                 }

                 return info;
             }





             private static void walkArrayWrite(Array arr, Action<object> each)
             {
                //20130816 DKh refactored into SerializationUtils
                SerializationUtils.WalkArrayWrite(arr, each);
             }


             private static void walkArrayRead<T>(Array arr, Func<T> each)
             {
                 //20130816 DKh refactored into SerializationUtils
                 SerializationUtils.WalkArrayRead<T>(arr, each);
             }




        private dyn_deserialize makeDeserialize()
        {



              var pSchema   = Expression.Parameter(typeof(TypeSchema));
              var pReader   = Expression.Parameter(typeof(SlimReader));
              var pTReg     = Expression.Parameter(typeof(TypeRegistry));
              var pRefs     = Expression.Parameter(typeof(RefPool));
              var pInstance = Expression.Parameter(typeof(object).MakeByRefType());
              var pStreamingContext = Expression.Parameter(typeof(StreamingContext));

              var expressions = new List<Expression>();

              var instance = Expression.Variable(Type, "instance");

              expressions.Add( Expression.Assign(instance, Expression.Convert(pInstance, Type)));


              if (IsPrimitive)
              {
                expressions.Add( Expression.Assign
                                         (instance,  Expression.Call(pReader, Format.GetReadMethodForType(Type)) )
                               );

              }
              else if (IsArray)
              {
                 var elmType = Type.GetElementType();
                 var _walkArrayRead = typeof(TypeDescriptor).GetMethod("walkArrayRead", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elmType);

                 if (Format.IsTypeSupported(elmType))//array element type
                 {
                    //spool whole array from reader using primitive types
                    expressions.Add( Expression.Call(_walkArrayRead,
                                                     instance,
                                                     Expression.Lambda(Expression.Call(pReader, Format.GetReadMethodForType(elmType)))
                                                    )
                     );
                 }
                 else
                 {  //spool whole array using TypeSchema because objects may change type
                    if (!elmType.IsValueType)//reference type
                     expressions.Add( Expression.Call(_walkArrayRead,
                                                     instance,
                                                     Expression.Lambda(
                                                                     Expression.Convert(
                                                                                       Expression.Call(pSchema,
                                                                                                   typeof(TypeSchema).GetMethod("readRefMetaHandle"),
                                                                                                    pReader,
                                                                                                    pTReg,
                                                                                                    pRefs,
                                                                                                    pStreamingContext),
                                                                                        elmType
                                                                                       )
                                                                      )
                                                    )
                                    );
                    else
                     expressions.Add( Expression.Call(_walkArrayRead,
                                                     instance,
                                                     Expression.Lambda(
                                                                     Expression.Convert(
                                                                                       Expression.Call(pSchema,
                                                                                               typeof(TypeSchema).GetMethod("Deserialize"),
                                                                                               pReader,
                                                                                               pTReg,
                                                                                               pRefs,
                                                                                               pStreamingContext,
                                                                                               Expression.Constant(elmType)),//valueType
                                                                                      elmType
                                                                                      )
                                                                       )
                                                    )
                                    );
                 }
              }
              else//loop through fields
              {
                foreach(var field in Fields)
                {
                          Expression expr = null;
                          Type t = field.FieldType;

                          Expression assignmentTargetExpression;
                          if (field.IsInitOnly)//readonly fields must be assigned using reflection
                          {
                            assignmentTargetExpression =  Expression.Variable(t, "readonlyFieldValue");
                          }
                          else
                            assignmentTargetExpression = Expression.Field(instance, field);


                          if (Format.IsTypeSupported(t))
                          {
                            expr =  Expression.Assign(
                                                        assignmentTargetExpression,
                                                        Expression.Call(pReader, Format.GetReadMethodForType(t))
                                                      );
                          }
/* 20150316 DKh, introduction of Metahandle.InlinedTypeValue
                          else
                          if (t == typeof(Type))
                          {
                            var strv = Expression.Variable(typeof(string));
                            var stra = Expression.Assign(strv, Expression.Call(pReader, Format.GetReadMethodForType(typeof(string))) );


                            var ifte = Expression.IfThenElse(
                                          Expression.Equal( strv, Expression.Constant(null, typeof(string))),

                                        //then
                                          Expression.Assign(
                                                        assignmentTargetExpression,
                                                        Expression.Constant( null, typeof(Type))
                                                      ),
                                        //else
                                          Expression.Assign(
                                                        assignmentTargetExpression,
                                                        Expression.Call(pTReg,
                                                                   typeof(TypeRegistry).GetMethod("GetByHandle"),
                                                                   strv)
                                                      )
                                       );//if-then-else



                            expr = Expression.Block(new ParameterExpression[]{strv}, stra, ifte);
                          }
 */
                          else
                          if (t.IsEnum)
                          {
                            expr =  Expression.Assign(
                                                        assignmentTargetExpression,
                                                        Expression.Convert(
                                                             Expression.Call(pReader, Format.GetReadMethodForType(typeof(int))),
                                                             field.FieldType
                                                             )
                                                      );
                          }
                          else // complex type ->  struct or reference
                          {
                            if (!t.IsValueType)//reference type -> read metahandle
                            {
                              expr =  Expression.Assign(
                                                        assignmentTargetExpression,

                                                        Expression.Convert(
                                                                Expression.Call(pSchema,
                                                                           typeof(TypeSchema).GetMethod("readRefMetaHandle"),
                                                                           pReader,
                                                                           pTReg,
                                                                           pRefs,
                                                                           pStreamingContext),
                                                            field.FieldType)
                                                       );
                            }
                            else
                            {
                              expr =  Expression.Assign(
                                                        assignmentTargetExpression,
                                                        Expression.Convert(
                                                               Expression.Call(pSchema,
                                                                               typeof(TypeSchema).GetMethod("Deserialize"),
                                                                               pReader,
                                                                               pTReg,
                                                                               pRefs,
                                                                               pStreamingContext,
                                                                               Expression.Constant(field.FieldType)),//valueType
                                                               field.FieldType)
                                                       );
                           }
                          }

                          if (assignmentTargetExpression is ParameterExpression)//readonly fields
                          {
                            if (Type.IsValueType)//20150405DKh added
                            {
                              var vBoxed = Expression.Variable(typeof(object), "vBoxed");
                              var box = Expression.Assign(vBoxed, Expression.TypeAs( instance, typeof(object)));//box the value type
                              var setField =  Expression.Call( Expression.Constant(field),
                                                                  typeof(FieldInfo).GetMethod("SetValue", new Type[]{typeof(object), typeof(object)}),
                                                                  vBoxed, //on boxed struct
                                                                  Expression.Convert( assignmentTargetExpression, typeof(object))
                                                           );
                              var swap = Expression.Assign( instance,  Expression.Unbox(vBoxed, Type));
                              expressions.Add(
                                  Expression.Block
                                  (new ParameterExpression[]{(ParameterExpression)assignmentTargetExpression, vBoxed},
                                    box,
                                    expr,
                                    setField,
                                    swap
                                  )
                              );
                            }
                            else
                            {

                              var setField =  Expression.Call( Expression.Constant(field),
                                                                  typeof(FieldInfo).GetMethod("SetValue", new Type[]{typeof(object), typeof(object)}),
                                                                  instance,
                                                                  Expression.Convert( assignmentTargetExpression, typeof(object))
                                                           );
                              expressions.Add( Expression.Block(new ParameterExpression[]{(ParameterExpression)assignmentTargetExpression}, expr, setField) );
                            }
                          }
                          else
                           expressions.Add(expr);
                }//foreach
               }//loop through fields


           expressions.Add( Expression.Assign(pInstance, Expression.Convert(instance, typeof(object))));

           var body = Expression.Block(new ParameterExpression[]{instance}, expressions);
//////////Debug dump 20150405 Dkh
////////Console.WriteLine(Type.FullName);
////////Console.WriteLine("--------------------------");
////////Console.WriteLine(body.ToDebugView());
           return Expression.Lambda<dyn_deserialize>(body, pSchema, pReader, pTReg, pRefs, pInstance, pStreamingContext).Compile();
        }







    }
   //TypeDef-----------------------------------------------------------------------------------------------------------------------------------------
   //TypeDef-----------------------------------------------------------------------------------------------------------------------------------------
   //TypeDef-----------------------------------------------------------------------------------------------------------------------------------------
   //TypeDef-----------------------------------------------------------------------------------------------------------------------------------------
   //TypeDef-----------------------------------------------------------------------------------------------------------------------------------------


    internal class TypeSchema
    {

       private SlimFormat m_Format;

       private object m_DictLock = new object();
       private Dictionary<Type, TypeDescriptor> m_Dict = new Dictionary<Type,TypeDescriptor>();


                       private TypeDescriptor getTypeDescriptorCachedOrMake(Type tp)
                       {
                         TypeDescriptor result;
                         if (!m_Dict.TryGetValue(tp, out result))
                         {
                           lock(m_DictLock)
                           {
                             if (!m_Dict.TryGetValue(tp, out result))
                             {
                               result = new TypeDescriptor(this, tp);
                               var dict = new Dictionary<Type,TypeDescriptor>(m_Dict);
                               dict[tp] = result;
                               m_Dict = dict;//atomic
                             }
                           }
                         }
                         return result;
                       }




       public TypeSchema(SlimFormat format)
       {
         m_Format = format;
       }


       public SlimFormat Format
       {
         get { return m_Format; }
       }


       public void Serialize(SlimWriter writer, TypeRegistry registry, RefPool refs, object instance, StreamingContext streamingContext, Type valueType = null)
       {

          Type type = valueType;

          VarIntStr typeHandle = new VarIntStr(0);

          if (type==null)
          {
              if (instance==null)
              {
                writer.Write(TypeRegistry.NULL_HANDLE);//object type null
                return;
              }

              type = instance.GetType();

              //Write type name. Full or compressed. Full Type names are assembly-qualified strings, compressed are string in form of
              // $<name_table_index> i.e.  $1 <--- get string[1]
              typeHandle = registry.GetTypeHandle(type);
              writer.Write( typeHandle );
          }

          //we get here if we have a boxed value of directly-handled type
          var wa = Format.GetWriteActionForType(type) ?? Format.GetWriteActionForRefType(type);//20150503 DKh fixed root byte[] slow
          if (wa!=null)
          {
            wa(writer, instance);
            return;
          }

          TypeDescriptor td = getTypeDescriptorCachedOrMake(type);

          if (td.IsArray) //need to write array dimensions
          {
            writer.Write( Arrays.ArrayToDescriptor((Array)instance, type, typeHandle) );
          }

          td.SerializeInstance(writer, registry, refs, instance, streamingContext);
       }


       public void writeRefMetaHandle(SlimWriter writer, TypeRegistry registry, RefPool refs, object instance, StreamingContext streamingContext)
       {
           Type tInstance;

           var mh = refs.GetHandle(instance, registry, Format, out tInstance);
           writer.Write(mh);

           if (mh.IsInlinedValueType)
           {
             var wa = Format.GetWriteActionForType(tInstance);
             if (wa!=null)
               wa(writer, instance);
             else
               this.Serialize(writer, registry, refs, instance, streamingContext);
           }
           else
           if (mh.IsInlinedRefType)
           {
             var wa = Format.GetWriteActionForRefType(tInstance);
             if (wa!=null)
              wa(writer, instance);
             else
              throw new SlimSerializationException("Internal error writeRefMetaHandle: no write action for ref type, but ref mhandle is inlined");
           }
       }

       public object readRefMetaHandle(SlimReader reader, TypeRegistry registry, RefPool refs, StreamingContext streamingContext)
       {
           var mh = reader.ReadMetaHandle();

           if (mh.IsInlinedValueType)
           {
             var tboxed = registry[ mh.Metadata.Value ];//adding this type to registry if it is not there yet

             var ra = Format.GetReadActionForType(tboxed);
             if (ra!=null)
              return ra(reader);
             else
              return this.Deserialize(reader, registry, refs, streamingContext);
           }

           return refs.HandleToReference(mh, registry, Format, reader);
       }

       public object Deserialize(SlimReader reader, TypeRegistry registry, RefPool refs, StreamingContext streamingContext, Type valueType = null)
       {
         return DeserializeRootOrInner(reader, registry, refs, streamingContext, false, valueType);
       }

       public object DeserializeRootOrInner(SlimReader reader, TypeRegistry registry, RefPool refs, StreamingContext streamingContext, bool root, Type valueType = null)
       {
         Type type = valueType;
         if (type==null)
         {
             var thandle = reader.ReadVarIntStr();
             if (thandle.StringValue!=null)//need to search for possible array descriptor
             {
                var ip = thandle.StringValue.IndexOf('|');//array descriptor start
                if (ip>0)
                {
                  var tname =  thandle.StringValue.Substring(0, ip);
                  if (TypeRegistry.IsNullHandle(tname)) return null;
                  type = registry[ tname ];
                }
                else
                {
                  if (TypeRegistry.IsNullHandle(thandle)) return null;
                  type = registry[ thandle ];
                }
             }
             else
             {
                if (TypeRegistry.IsNullHandle(thandle)) return null;
                type = registry[ thandle ];
             }
         }

         //we get here if we have a boxed value of directly-handled type
         var ra = Format.GetReadActionForType(type) ?? Format.GetReadActionForRefType(type);//20150503 DKh fixed root byte[] slow
         if (ra!=null)
           return ra(reader);


         TypeDescriptor td = getTypeDescriptorCachedOrMake(type);

         object instance = null;

         if (td.IsArray)
           instance = Arrays.DescriptorToArray( reader.ReadString(), type);
         else
           instance = SerializationUtils.MakeNewObjectInstance(type);

         if (root)
             if (!type.IsValueType )//if this is a reference type
             {
               refs.Add(instance);
             }

         td.DeserializeInstance(reader, registry, refs, ref instance, streamingContext);


         return instance;
       }


       public void DeserializeRefTypeInstance(object instance, SlimReader reader, TypeRegistry registry, RefPool refs, StreamingContext streamingContext)
       {
         if (instance==null) throw new SlimDeserializationException("DeserRefType(null)");

         var type = instance.GetType();

          reader.ReadVarIntStr();//skip type as we already know it from prior-allocated metahandle


         TypeDescriptor td = getTypeDescriptorCachedOrMake(type);

         if (type.IsArray)
           reader.ReadString();//skip array descriptor as we already know it from prior-allocated metahandle

         td.DeserializeInstance(reader, registry, refs, ref instance, streamingContext);
       }

    }



}
