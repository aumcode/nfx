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
    ///  string-based addressing (getting instances by object instance name)
    /// </summary>
    public interface INamed
    {
      string Name { get; }
    }

    /// <summary>
    /// Denotes an entity that has an Order property
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
        /// Returns true if when this registry contains the specified name
        /// </summary>
        bool ContainsName(string name);

        /// <summary>
        /// Returns the count of items registered in this instance
        /// </summary>
        int Count{ get;}
    }
    
    
    
    
    /// <summary>
    /// Represents a dictionary of string-named objects. Name search is case-insensitive
    /// </summary>
    [Serializable]
    internal class RegistryDictionary<T> : Dictionary<string, T> , IRegistry<T> where T : INamed
    {
       public RegistryDictionary() : base(StringComparer.OrdinalIgnoreCase)
       {

       }
       
       public RegistryDictionary(IDictionary<string, T> other) : base(other, StringComparer.OrdinalIgnoreCase)
       {

       }

       protected RegistryDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
       {

       }

       public IEnumerable<string> Names
       {
           get { return this.Keys; }
       }

       public new IEnumerable<T> Values
       {
           get { return base.Values; }
       }

       public bool ContainsName(string name)
       {
           return ContainsKey(name);
       }

       public new IEnumerator<T> GetEnumerator()
       {
           return Values.GetEnumerator();
       }
    }


    /// <summary>
    /// Represents a thread-safe registry of T. This class is efficient for concurrent read access and is not designed for cases when frequent modifications happen.
    /// It is ideal for lookup of named instances that have much longer time span than components that look them up
    /// </summary>
    [Serializable]
    public class Registry<T> : IRegistry<T> where T : INamed
    {
          public Registry()
          {

          }

          public Registry(IEnumerable<T> other)
          {
            foreach(var i in other) m_Data[i.Name] = i;
          }
          
          protected object m_Sync = new object();
          private volatile RegistryDictionary<T> m_Data = new RegistryDictionary<T>();
         
         
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

                var data = new RegistryDictionary<T>(m_Data);
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
            
            lock(m_Sync)
            {
                var data = new RegistryDictionary<T>(m_Data);
                
                if (data.TryGetValue(item.Name, out existing))
                {
                   data[item.Name] = item;
                   JustReplaced(existing, item);
                }
                else
                {
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

                var data = new RegistryDictionary<T>(m_Data);
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

                var data = new RegistryDictionary<T>(m_Data);
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
             m_Data = new RegistryDictionary<T>(); //atomic
          }

          /// <summary>
          /// Tries to find an item by name, and returns it if it is found, otherwise calls a factory function supplying context value and registers the obtained
          ///  new item. The operation is performed atomically under lock
          /// </summary>
          public T GetOrRegister<TContext>(string name, Func<TContext, T> regFactory, TContext context)
          {
            lock(m_Sync)
            {
                var data = m_Data;
                T result;
                if (data.TryGetValue(name, out result)) return result;

                result = regFactory( context );
                Register( result );
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
    /// It is ideal for lookup of named instances that have much longer time span than components that look them up
    /// </summary>
    [Serializable]
    public class OrderedRegistry<T> : Registry<T> where T : INamed, IOrdered
    {
          public OrderedRegistry()
          {

          }

          private List<T> m_OrderedValues = new List<T>();

          /// <summary>
          /// Returns items that registry contains ordered by their Order property.
          /// The returned sequence is pre-sorted during alteration of registry, so this property access is efficient
          /// </summary>
          public IEnumerable<T> OrderedValues
          {
            get { return m_OrderedValues; }
          }

          /// <summary>
          /// Tries to return an item by its position index in ordered set of items that this registry keeps.
          /// Null is returned when index is out of bounds
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
