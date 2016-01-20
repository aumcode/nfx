using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD
{
  internal struct iListReadOnly : IList
  {
     public iListReadOnly(List<Row> data){  m_Data = data;  }

     private List<Row> m_Data;


     public int Add(object value)
     {
       throw new NotImplementedException();
     }

     public void Clear()
     {
       throw new NotImplementedException();
     }

     public bool Contains(object value)
     {
       return m_Data.Contains((Row)value);
     }

     public int IndexOf(object value)
     {
       return m_Data.IndexOf((Row)value);
     }

     public void Insert(int index, object value)
     {
       throw new NotImplementedException();
     }

     public bool IsFixedSize
     {
       get { return true; }
     }

     public bool IsReadOnly
     {
       get { return true; }
     }

     public void Remove(object value)
     {
       throw new NotImplementedException();
     }

     public void RemoveAt(int index)
     {
       throw new NotImplementedException();
     }

     public object this[int index]
     {
       get
       {
         return m_Data[index];
       }
       set
       {
         throw new NotImplementedException();
       }
     }

     public void CopyTo(Array array, int index)
     {
       throw new NotImplementedException();
     }

     public int Count
     {
       get { return m_Data.Count; }
     }

     public bool IsSynchronized
     {
       get { return false; }
     }

     public object SyncRoot
     {
       get { return this; }
     }

     public IEnumerator GetEnumerator()
     {
       return m_Data.GetEnumerator();
     }
  }
}
