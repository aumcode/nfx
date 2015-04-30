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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.RecordModel
{
    /// <summary>
    /// Implements a list of T that is used by ListField class
    /// </summary>
    [Serializable]
    public class ListOf<T> : IList<T>
    {
       #region .ctor
          internal ListOf(ListField<T> field)
          {
            m_Field = field;
            m_List = new List<T>();
          }
       
       #endregion


       #region Private Fields
         internal ListField<T> m_Field;
         internal List<T> m_List;
       #endregion



       #region IList Contract
       
        public int IndexOf(T item)
        {
            return m_List.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            m_Field.EnsureCanModifyValue();
            
            
            m_Field.OnFieldDataChanging(new FieldDataChangeEventArgs(this, true, null));
            m_List.Insert(index, item);
            m_Field.__internalSetValue(this);
            m_Field.OnFieldDataChanged(new FieldDataChangeEventArgs(this, true, null));
        }

        public void RemoveAt(int index)
        {
            m_Field.EnsureCanModifyValue();
            
            m_Field.OnFieldDataChanging(new FieldDataChangeEventArgs(this, true, null));
            m_List.RemoveAt(index);
            m_Field.__internalSetValue(this);
            m_Field.OnFieldDataChanged(new FieldDataChangeEventArgs(this, true, null));
        }

        public T this[int index]
        {
            get
            {
                return m_List[index];
            }
            set
            {
                m_Field.EnsureCanModifyValue();

                m_Field.OnFieldDataChanging(new FieldDataChangeEventArgs(this, true, null));
                m_List[index] = value;
                m_Field.__internalSetValue(this);
                m_Field.OnFieldDataChanged(new FieldDataChangeEventArgs(this, true, null));
            }
        }

        public void Add(T item)
        {
            m_Field.EnsureCanModifyValue();

            m_Field.OnFieldDataChanging(new FieldDataChangeEventArgs(this, true, null));
            m_List.Add(item);
            m_Field.__internalSetValue(this);
            m_Field.OnFieldDataChanged(new FieldDataChangeEventArgs(this, true, null));
        }


        public void AddFrom(ListOf<T> list)
        {
            m_Field.EnsureCanModifyValue();

            m_Field.OnFieldDataChanging(new FieldDataChangeEventArgs(this, true, null));
           
            foreach(var item in list)
              m_List.Add(item);
           
            m_Field.__internalSetValue(this);
            m_Field.OnFieldDataChanged(new FieldDataChangeEventArgs(this, true, null));
        }

        public void Clear()
        {
            m_Field.EnsureCanModifyValue();

            m_Field.OnFieldDataChanging(new FieldDataChangeEventArgs(this, true, null));
            m_List.Clear();
            m_Field.__internalSetValue(this);
            m_Field.OnFieldDataChanged(new FieldDataChangeEventArgs(this, true, null));
        }

        public bool Contains(T item)
        {
            return m_List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return m_List.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return !m_Field.IsMutable;
            }
        }

        public bool Remove(T item)
        {
            m_Field.EnsureCanModifyValue();

            m_Field.OnFieldDataChanging(new FieldDataChangeEventArgs(this, true, null));
            var result = m_List.Remove(item);
            m_Field.__internalSetValue(this);
            m_Field.OnFieldDataChanged(new FieldDataChangeEventArgs(this, true, null));

            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_List.GetEnumerator();
        }
      #endregion



    }





    /// <summary> Represents fields that maintain list of generic objects </summary>
  [Serializable]     //20110111 DKh
  public sealed class ListField<T> : TypedReferenceTypeField<ListOf<T>>, IField<ListOf<T>>
  {
    #region .ctors
    public ListField() : base()
    {
       __internalSetValue(new ListOf<T>(this));
    }
     
    #endregion

    /// <summary>
    /// Same as .Value
    /// </summary>
    public ListOf<T> List
    {
      get { return Value; }
    }

    

    public override void Clear()
    {
        EnsureCanModifyValue();
        
        OnFieldDataChanging(new FieldDataChangeEventArgs(null, false, __sourceBinding));
        Value.Clear();
        OnFieldDataChanged(new FieldDataChangeEventArgs(null, false, __sourceBinding));

    }

    protected override void SetInternalValue(ref ListOf<T> value)
    {
       if (Value != null)
         throw new RecordModelException(string.Format(StringConsts.REFERENCE_TYPE_VALUE_SET_ERROR, FieldName));
   
       base.SetInternalValue(ref value);
    }

    protected override int CompareFieldValues(ListOf<T> v1, ListOf<T> v2)
    { //v1 and v2 are never null here

      if (object.ReferenceEquals(v1, v2)) return 0;

      IComparable cmp = v1 as IComparable;
      if (cmp != null)
        return cmp.CompareTo(v2);
      else
        return -1; // no particular comparison possible 
    }


    protected internal override void CreatePreEditStateCopy()
    {
        base.CreatePreEditStateCopy();
        var list = new ListOf<T>(this);
       
        //copy over, we do not want to call list.AddFrom as it triggers events on Field
        foreach(var item in m_Value)
         list.m_List.Add(item);
       
        m_Value = list;
    }

    protected internal override void DropPreEditStateCopy()
    {
        base.DropPreEditStateCopy();
    }

    protected internal override void RevertToPreEditState()
    {
        base.RevertToPreEditState();
    }


  }




}
