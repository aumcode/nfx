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
using System.Runtime.Serialization;
using System.Text;

namespace NFX
{

    /// <summary>
    /// Denotes an entity that has a Name property.
    /// This interface is primarily used with Registry[INamed] class that allows for
    ///  string-based addressing (getting instances by object instance name).
    /// The names are ideal for many system functions, like naming components in configs and admin tools
    /// </summary>
    public interface INamed
    {
      string Name { get; }
    }

    /// <summary>
    /// Denotes an entity that has a relative Order property within a collection of entities
    /// </summary>
    public interface IOrdered
    {
      int Order { get; }
    }



    /// <summary>
    /// Provides read-only named object lookup capabilities
    /// </summary>
    public interface IRegistry<out T> : IEnumerable<T> where T : INamed
    {
        IEnumerable<string> Names { get; }
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Returns item by name or default item (such as null) if the named instance could not be found
        /// </summary>
        T this[string name] { get;}

        /// <summary>
        /// Returns true if the instance differentiates names by case
        /// </summary>
        bool IsCaseSensitive{ get;}

        /// <summary>
        /// Returns true if when this registry contains the specified name
        /// </summary>
        bool ContainsName(string name);

        /// <summary>
        /// Returns the count of items registered in this instance
        /// </summary>
        int Count{ get;}
    }

    /// <summary>
    /// Provides read-only named ordered object lookup capabilities
    /// </summary>
    public interface IOrderedRegistry<out T> : IRegistry<T> where T : INamed, IOrdered
    {
          /// <summary>
          /// Returns items that registry contains ordered by their Order property.
          /// The returned sequence is pre-sorted during alteration of registry, so this property access is efficient.
          /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
          ///  of ordered items which may capture items that have already/not yet been added to the registry
          /// </summary>
          IEnumerable<T> OrderedValues{ get;}

          /// <summary>
          /// Tries to return an item by its position index in ordered set of items that this registry keeps.
          /// Null is returned when index is out of bounds.
          /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
          ///  of ordered items which may capture items that have already/not yet been added to the registry
          /// </summary>
          T this[int index] { get;}
    }



    /// <summary>
    /// Internal dictionary of string-named objects
    /// </summary>
    [Serializable]
    internal class RegistryDictionary<T> : Dictionary<string, T> where T : INamed
    {
       public RegistryDictionary(bool caseSensitive) : base(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
       {
       }

       public RegistryDictionary(bool caseSensitive, IDictionary<string, T> other) : base(other, caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
       {
       }

       protected RegistryDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
       {
       }
    }


    /// <summary>
    /// Represents a thread-safe registry of T. This class is efficient for concurrent read access and is not designed for cases when frequent modifications happen.
    /// It is ideal for lookup of named instances (such as components) that have much longer time span than components that look them up.
    /// Registry performs lock-free lookup which speeds-up many concurrent operations that need to map names into objects.
    /// The enumeration over registry makes a snapshot of its data, hence a registry may be modified by other threads while being enumerated.
    /// </summary>
    [Serializable]
    public class Registry<T> : IRegistry<T> where T : INamed
    {
          public Registry() : this(false)
          {

          }

          public Registry(bool caseSensitive)
          {
            m_CaseSensitive = caseSensitive;
            m_Data = new RegistryDictionary<T>(caseSensitive);
          }

          public Registry(IEnumerable<T> other) : this(other, false)
          {

          }

          public Registry(IEnumerable<T> other, bool caseSensitive) : this(caseSensitive)
          {
            foreach(var i in other)
              m_Data[i.Name] = i;
          }



          [NonSerialized]
          protected object m_Sync = new object();

          private bool m_CaseSensitive;
          private volatile RegistryDictionary<T> m_Data;



          /// <summary>
          /// Returns true if the instance differentiates names by case
          /// </summary>
          public bool IsCaseSensitive{ get{ return m_CaseSensitive;} }

          /// <summary>
          /// Returns a value by name or null if not found
          /// </summary>
          public T this[string name]
          {
            get
            {
             var data = m_Data;//atomic
             T result;
             if (data.TryGetValue(name, out result)) return result;
             return default(T);
            }
          }


          /// <summary>
          /// Returns the number of entries in the registry
          /// </summary>
          public int Count
          {
            get{ return m_Data.Count; }
          }


          /// <summary>
          /// Registers item and returns true if it was registered, false if this named instance already existed in the list
          /// </summary>
          public bool Register(T item)
          {
            lock(m_Sync)
            {
                if (m_Data.ContainsKey(item.Name)) return false;

                var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);
                data.Add(item.Name, item);

                JustRegistered(item);

                m_Data = data;
            }

            return true;
          }

          /// <summary>
          /// Registers item and returns true if it was registered, false if this named instance already existed and was replaced
          /// </summary>
          public bool RegisterOrReplace(T item)
          {
            T existing;
            return RegisterOrReplace(item, out existing);
          }

          /// <summary>
          /// Registers item and returns true if it was registered, false if this named instance already existed and was replaced
          /// </summary>
          public bool RegisterOrReplace(T item, out T existing)
          {
            lock(m_Sync)
            {
                var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);

                if (data.TryGetValue(item.Name, out existing))
                {
                   data[item.Name] = item;
                   JustReplaced(existing, item);
                }
                else
                {
                   existing = default(T);//safeguard
                   data.Add(item.Name, item);
                   JustRegistered(item);
                }

                m_Data = data;
            }

            return existing == null;
          }

          /// <summary>
          /// Unregisters item and returns true if it was unregistered, false if it did not exist
          /// </summary>
          public bool Unregister(T item)
          {
            lock(m_Sync)
            {
                if (!m_Data.ContainsKey(item.Name)) return false;

                var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);
                data.Remove(item.Name);

                JustUnregistered(item);

                m_Data = data;
            }

            return true;
          }

          /// <summary>
          /// Unregisters item by name and returns true if it was unregistered, false if it did not exist
          /// </summary>
          public bool Unregister(string name)
          {
            lock(m_Sync)
            {
                T item;
                if (!m_Data.TryGetValue(name, out item)) return false;

                var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);
                data.Remove(name);

                JustUnregistered(item);

                m_Data = data;
            }

            return true;
          }

          /// <summary>
          /// Deletes all items from registry
          /// </summary>
          public virtual void Clear()
          {
             m_Data = new RegistryDictionary<T>(m_CaseSensitive); //atomic
          }


          /// <summary>
          /// Tries to find an item by name, and returns it if it is found, otherwise calls a factory function supplying context value and registers the obtained
          ///  new item. The first lookup is performed in a lock-free way and if an item is found then it is immediately returned.
          ///  The second check and factory call operation is performed atomically under the lock to ensure consistency
          /// </summary>
          public T GetOrRegister<TContext>(string name, Func<TContext, T> regFactory, TContext context)
          {
            bool wasAdded;
            return this.GetOrRegister<TContext>(name, regFactory, context, out wasAdded);
          }


          /// <summary>
          /// Tries to find an item by name, and returns it if it is found, otherwise calls a factory function supplying context value and registers the obtained
          ///  new item. The first lookup is performed in a lock-free way and if an item is found then it is immediately returned.
          ///  The second check and factory call operation is performed atomically under the lock to ensure consistency
          /// </summary>
          public T GetOrRegister<TContext>(string name, Func<TContext, T> regFactory, TContext context, out bool wasAdded)
          {
            //1st check - lock-free lookup attempt
            var data = m_Data;
            T result;
            if (data.TryGetValue(name, out result))
            {
               wasAdded = false;
               return result;
            }

            lock(m_Sync)
            {
                //2nd check under lock
                data = m_Data;//must re-read the reference
                if (data.TryGetValue(name, out result))
                {
                  wasAdded = false;
                  return result;
                }
                result = regFactory( context );
                Register( result );
                wasAdded = true;
                return result;
            }
          }



          public IEnumerator<T> GetEnumerator()
          {
              var data = m_Data;//atomic

              return data.Values.GetEnumerator();
          }

          System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
          {
              return this.GetEnumerator();
          }



           public IEnumerable<string> Names
           {
               get
               {
                 var data = m_Data;
                 return data.Keys;
               }
           }


           public IEnumerable<T> Values
           {
               get
               {
                 var data = m_Data;
                 return data.Values;
               }
           }


           public bool ContainsName(string name)
           {
               var data = m_Data;
               return data.ContainsKey(name);
           }



           public bool TryGetValue(string name, out T value)
           {
               var data = m_Data;
               return data.TryGetValue(name, out value);
           }


           protected virtual void JustRegistered(T item) {}

           protected virtual void JustReplaced(T existingItem, T newItem) {}

           protected virtual void JustUnregistered(T item) {}

    }


    /// <summary>
    /// Represents a thread-safe registry of T which is ordered by Order property.
    /// This class is efficient for concurrent read access and is not designed for cases when frequent modifications happen.
    /// It is ideal for lookup of named instances that have much longer time span than components that look them up.
    /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
    ///  of ordered items which may capture items that have already/not yet been added to the registry
    /// </summary>
    [Serializable]
    public class OrderedRegistry<T> : Registry<T>, IOrderedRegistry<T> where T : INamed, IOrdered
    {
          public OrderedRegistry() : this(false)
          {

          }

          public OrderedRegistry(bool caseSensitive) : base(caseSensitive)
          {
            m_OrderedValues = new List<T>();
          }


          private List<T> m_OrderedValues;

          /// <summary>
          /// Returns items that registry contains ordered by their Order property.
          /// The returned sequence is pre-sorted during alteration of registry, so this property access is efficient.
          /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
          ///  of ordered items which may capture items that have already/not yet been added to the registry
          /// </summary>
          public IEnumerable<T> OrderedValues
          {
            get { return m_OrderedValues; }
          }

          /// <summary>
          /// Tries to return an item by its position index in ordered set of items that this registry keeps.
          /// Null is returned when index is out of bounds.
          /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
          ///  of ordered items which may capture items that have already/not yet been added to the registry
          /// </summary>
          public T this[int index]
          {
            get
            {
                var lst = m_OrderedValues;//thread-safe copy
                if (index>=0 && index<lst.Count)
                    return lst[index];

                return default(T);
            }
          }

           /// <summary>
          /// Deletes all items from ordered registry
          /// </summary>
          public override void Clear()
          {
             lock(m_Sync)
             {
               base.Clear();
               m_OrderedValues = new List<T>();
             }
          }


          protected override void JustRegistered(T item)
          {
            var list = new List<T>(m_OrderedValues);
            list.Add(item);
            list.Sort( (l, r) => l.Order.CompareTo(r.Order) );

            m_OrderedValues = list;
          }

          protected override void JustReplaced(T existingItem, T newItem)
          {
            var list = new List<T>(m_OrderedValues);
            list.Remove(existingItem);
            list.Add(newItem);
            list.Sort( (l, r) => l.Order.CompareTo(r.Order) );

            m_OrderedValues = list;
          }

          protected override void JustUnregistered(T item)
          {
            var list = new List<T>(m_OrderedValues);
            list.Remove(item);

            m_OrderedValues = list;
          }

   }


}
