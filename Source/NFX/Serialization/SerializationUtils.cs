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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Linq.Expressions;
using NFX.Serialization.JSON;

namespace NFX.Serialization
{
    /// <summary>
    /// Provides misc serialization-related functions that are really low-level and not intended to be used by developers.
    /// Methods are thread-safe
    /// </summary>
    public static class SerializationUtils
    {

        /// <summary>
        /// Returns .ctor(SerializationInfo, StreamingContext) that complies with ISerializable concept, or null.
        /// </summary>
        public static ConstructorInfo GetISerializableCtorInfo(Type type)
        {
           return type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                             null,
                                             new Type[] { typeof(SerializationInfo), typeof(StreamingContext)},
                                             null);
        }


             //20150201 Added caching
             private static Dictionary<Type, Func<object>> s_CreateFuncCache = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Create new object instance for type, calling default ctor
        /// </summary>
        //20150201 Started using cached lambda for .ctor, inling gives 2360 -> 2415 ops/sec in given test
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static object MakeNewObjectInstance(Type type)
        {
           Func<object> f;
           if (!s_CreateFuncCache.TryGetValue(type, out f))
           {
             var ctorEmpty = type.GetConstructor(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance,
                                             null,
                                             Type.EmptyTypes,
                                             null);

             //20150717 DKh added SlimDeserializationCtorSkipAttribute
             var skipAttr = ctorEmpty!=null ?
                              ctorEmpty.GetCustomAttributes<Slim.SlimDeserializationCtorSkipAttribute>(false).FirstOrDefault()
                              : null;


             //20150715 DKh look for ISerializable .ctor
             var ctorSer = GetISerializableCtorInfo(type);

             //20150715 DKh, the empty .ctor SHOULD NOT be called for types that have SERIALIZABLE .ctor which is called later(after object init)
             if (ctorEmpty!=null && skipAttr==null && ctorSer==null)
               f = Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
             else
               f = () => FormatterServices.GetUninitializedObject(type);

             var cache = new Dictionary<Type, Func<object>>(s_CreateFuncCache);
             cache[type] = f;
             s_CreateFuncCache = cache;//atomic
           }

           return f();
        }


                //20150124 Added caching
                private static Dictionary<Type, FieldInfo[]> s_FieldCache = new Dictionary<Type, FieldInfo[]>();
                private static FieldInfo[] getGetSerializableFields(Type type)
                {
                    //20140926 DKh +DeclaredOnly
                    var local = type.GetFields(BindingFlags.DeclaredOnly |
                                                BindingFlags.Instance  |
                                                BindingFlags.NonPublic |
                                                BindingFlags.Public)
                                    .Where(fi => !fi.IsNotSerialized) //DKh 20130801 removed readonly constraint
                                    .OrderBy( fi => fi.Name )//DKh 20130730
                                    .ToArray();

                    var bt = type.BaseType;//null for Object

                    if (bt==null || bt==typeof(object)) return local;

                    return GetSerializableFields(type.BaseType).Concat(local).ToArray();//20140926 DKh parent+child reversed order, was: child+parent
                }

        /// <summary>
        /// Gets all serializable fields for type in parent->child declaraion order, sub-ordered by case
        ///  within the segment
        /// </summary>
        public static IEnumerable<FieldInfo> GetSerializableFields(Type type)
        {
          FieldInfo[] result;
          if (!s_FieldCache.TryGetValue(type, out result))
          {
            result = getGetSerializableFields(type);
            var dict = new Dictionary<Type, FieldInfo[]>(s_FieldCache);
            dict[type] = result;
            s_FieldCache = dict;//atomic
          }
          return result;
        }



        /// <summary>
        /// Finds methods decorated by [On(De)Seriali(zing/zed)]
        /// </summary>
        /// <param name="t">A type whose methods to search</param>
        /// <param name="atype">Attribute type to search</param>
        /// <returns>List(MethodInfo) that qualifies or NULL if none found</returns>
        public static List<MethodInfo> FindSerializationAttributedMethods(Type t, Type atype)
        {
            var list = new List<MethodInfo>();

            while(t!=null && t!=typeof(object))
            {
                var methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                .Where(mi => Attribute.IsDefined( mi, atype) &&
                                                mi.ReturnType==typeof(void) &&
                                                mi.GetParameters().Length==1 &&
                                                mi.GetParameters()[0].ParameterType == typeof(StreamingContext));
                foreach(var m in methods)
                list.Add(m);

                t = t.BaseType;
            }

            if (list.Count>0)
            {
                list.Reverse(); //because we harvest methods from child -> parent but need to call parent->child order
                return list;
            }

            return null;
        }

        /// <summary>
        /// Calls method in the list that was returned by a call to FindSerializationAttributedMethods
        /// </summary>
        /// <param name="methods">list that was returned by a call to FindSerializationAttributedMethods</param>
        /// <param name="instance">Instance to invoke mathods on</param>
        /// <param name="streamingContext">Streaming Context</param>
        public static void InvokeSerializationAttributedMethods(List<MethodInfo> methods, object instance, StreamingContext streamingContext)
        {
            if (instance==null) return;

            for(var i=0; i<methods.Count; i++)
             try
             {
              methods[i].Invoke(instance, new object[] { streamingContext });
             }
             catch(TargetInvocationException ie)
             {  //20131219 DKh
                if (ie.InnerException!=null) throw ie.InnerException;
                throw;
             }
        }


        /// <summary>
        /// Performs an action on each element of a possibly mutidimensional array
        /// </summary>
        public static void WalkArrayWrite(Array arr, Action<object> each)
        {
            var rank = arr.Rank;

            if (rank==1)
            {
                var i = arr.GetLowerBound(0);
                var top = arr.GetUpperBound(0);
                for(; i<=top; i++)
                  each(arr.GetValue(i));
                return;
            }


            var idxs = new int[rank];
            doDimensionGetValue(arr, idxs, 0, each);
        }

         /// <summary>
        /// Performs an action on each element of a possibly mutidimensional array
        /// </summary>
        public static void WalkArrayRead<T>(Array arr, Func<T> each)
        {
            var rank = arr.Rank;

            if (rank==1)
            {
                var i = arr.GetLowerBound(0);
                var top = arr.GetUpperBound(0);
                for(; i<=top; i++)
                  arr.SetValue(each(), i);
                return;
            }


            var idxs = new int[rank];
            doDimensionSetValue<T>(arr, idxs, 0, each);
        }

        /// <summary>
        /// Navigates through JSON datamap by subsequent node names.
        /// </summary>
        /// <returns>
        /// null if navigation path is not exists.
        /// JSONDataMap if navigation ends up with non-leaf node.
        /// object if navigation ends up with leaf node.</returns>
        public static object GetNodeByPath(this JSONDataMap json, params string[] nodeNames)
        {
            object node = null;
            for (int i=0; i<nodeNames.Length; i++)
            {
                if (json == null || !json.TryGetValue(nodeNames[i], out node))
                    return null;

                if (i == nodeNames.Length - 1)
                    return node;

                json = node as JSONDataMap;
            }

            return null;
        }


        private static void doDimensionGetValue(Array arr, int[] idxs, int di, Action<object> each)
        {
            var bot =  arr.GetLowerBound(di);
            var top =  arr.GetUpperBound(di);
            for(idxs[di] = bot; idxs[di] <= top; idxs[di]++)
            {
                if (di<idxs.Length-1)
                    doDimensionGetValue(arr, idxs, di + 1, each);
                else
                    each( arr.GetValue(idxs) );
            }
            idxs[di]=top;
        }

        private static void doDimensionSetValue<T>(Array arr, int[] idxs, int di, Func<T> each)
        {
            var bot =  arr.GetLowerBound(di);
            var top =  arr.GetUpperBound(di);
            for(idxs[di] = bot; idxs[di] <= top; idxs[di]++)
            {
                if (di<idxs.Length-1)
                    doDimensionSetValue(arr, idxs, di + 1, each);
                else
                    arr.SetValue( each(), idxs );
            }
            idxs[di]=top;
        }









    }
}
