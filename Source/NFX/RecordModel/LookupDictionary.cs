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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.RecordModel
{
  /// <summary>
  /// Represents a collection of strings with lookup key/description dictionary functionality
  /// </summary>
  [Serializable]
  public class LookupDictionary : IList<string>
  {
  
    #region .ctor
      public LookupDictionary(Field field)
      {
        m_Field = field;
      }
    #endregion
  
  
    #region Private Fields
     
     private Field m_Field;
     private List<string> m_List = new List<string>();
     
     private char m_Separator = ',';
  
    #endregion
  
    #region Properties
  
  
        /// <summary>
        /// Separator character that delimits key and description
        /// </summary>
        public char Separator
        {
          get { return m_Separator;}
          set { m_Separator = value; }
        }
        
        
        /// <summary>
        /// Finds description by key or null
        /// </summary>
        public string this[string key]
        {
          get
          {
            foreach (string entry in this)
            {
              string[] parts = entry.Trim().Split(m_Separator);
              if (parts.Length > 0)
              {
                if (parts[0] == key)
                {
                  if (parts.Length>1)
                    return parts[1];
                  else
                    return parts[0];
                }
              }  
            }
            return null;
          }
        }
        
        /// <summary>
        /// Returns all keys enumeration
        /// </summary>
        public IEnumerable<string> Keys
        {
          get
          {
            foreach(string entry in this)
            {
              string [] parts = entry.Trim().Split(m_Separator);
              if (parts.Length>0) 
                yield return parts[0];
            }
          }
        }
    #endregion
    
    
    
    #region Public
    
      /// <summary>
      /// Tries to find a key and returns true if key was found
      /// </summary>
      public bool FindKey(string key)
      {
         foreach (string item in Keys)
             if (item.EqualsOrdIgnoreCase(key)) return true;
          
         return false; 
      }

      /// <summary>
      /// Adds items to dictionary
      /// </summary>
      public void Add(params string[] items)
      {
        foreach (var item in items) Add(item);
      }
      
      
      /// <summary>
      /// Includes choice in dictionary if it was not already included
      /// </summary>
      public bool Include(string value)
      {
        if (!Contains(value))
        {
          Add(value); 
          return true;
        }  
        return false;
      }

      /// <summary>
      /// Excludes choice from dictionary if it was already included 
      /// </summary>
      public bool Exclude(string value)
      {
        if (Contains(value))
        {
          Remove(value);
          return true;
        }
        return false;
      }
    
    #endregion

      #region IList<string> Members

      public int IndexOf(string item)
      {
        return m_List.IndexOf(item);
      }

      public void Insert(int index, string item)
      {
        m_List.Insert(index, item);
        notify();
      }

      public void RemoveAt(int index)
      {
        m_List.RemoveAt(index);
        notify();
      }

      public string this[int index]
      {
        get
        {
          return m_List[index];
        }
        set
        {
          m_List[index] = value;
          notify();
        }
      }

      #endregion

      #region ICollection<string> Members


      
      
      public void Add(string item)
      {
        m_List.Add(item);
        notify();
      }
      
      public void AddRange(IEnumerable<string> items)
      {
        m_List.AddRange(items);
        notify();
      }

      public void Clear()
      {
        m_List.Clear();
        notify();
      }

      public bool Contains(string item)
      {
        return m_List.Contains(item);
      }

      public void CopyTo(string[] array, int arrayIndex)
      {
        m_List.CopyTo(array, arrayIndex);
      }

      public int Count
      {
        get { return m_List.Count; }
      }

      public bool IsReadOnly
      {
        get { return false; }
      }

      public bool Remove(string item)
      {
        var result = m_List.Remove(item);
        notify();
        
        return result;
      }

      #endregion

      #region IEnumerable<string> Members

      public IEnumerator<string> GetEnumerator()
      {
        return m_List.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      IEnumerator IEnumerable.GetEnumerator()
      {
        return m_List.GetEnumerator();
      }

      #endregion
      
      
      #region .pvt .impl
      
        private void notify()
        {
          if (!m_Field.Constructed) return;
          m_Field.DisableBindings();

          m_Field.AddNotification(new DataEntryTypeChangeNotification(m_Field));
          m_Field.Validate();
        
          m_Field.EnableBindings();
        }
      
      #endregion
      
  }
}
