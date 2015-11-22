using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD.Subscriptions
{
  
  /// <summary>
  /// Describes event handlers that get called when data arrives
  /// </summary>
  public delegate void SubscriptionReceiptEventHandler(Subscription subscription, Mailbox recipient, RowChange data, Exception error);
  
  /// <summary>
  /// Represents CRUD row data recipient
  /// </summary>
  public class Mailbox : SubscriptionAppComponent, INamed
  {
    #region .ctor
      protected internal Mailbox(ICRUDSubscriptionStoreImplementation store, string name) : base(store)
      {
        m_Store = store;
        m_Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
     
        var reg = m_Store.Mailboxes as Registry<Mailbox>;
        Mailbox existing;
        if (!reg.RegisterOrReplace(this, out existing))
         existing.Dispose();
      }

      protected override void Destructor()
      {
        if (m_Store!=null)
          ((Registry<Mailbox>)m_Store.Mailboxes).Unregister(this);
        
        foreach(var subscription in m_Subscriptions)
          subscription.Dispose();
        
        m_Subscriptions.Clear();

        base.Destructor();
      }
    #endregion

    #region Fields
      private ICRUDSubscriptionStoreImplementation m_Store;
      private string  m_Name;

      private Registry<Subscription> m_Subscriptions = new Registry<Subscription>();

      private LinkedList<RowChange> m_Buffer = new LinkedList<RowChange>();
      private volatile int m_BufferCapacity;
    #endregion

    #region Events/Props
      public event SubscriptionReceiptEventHandler Receipt;

      public ICRUDSubscriptionStore Store{ get{ return m_Store;}}
      public string  Name { get { return m_Name; } }

      public IRegistry<Subscription> Subscriptions{ get{ return m_Subscriptions;}}

      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
      public int BufferCapacity
      {
        get{ return m_BufferCapacity; }
        set{ m_BufferCapacity = value; }
      }

      /// <summary>
      /// Returns all buffered data in chronological order
      /// </summary>
      public RowChange[] Buffered
      {
        get
        {
          lock(m_Buffer)
            return m_Buffer.ToArray();
        }
      }

      /// <summary>
      /// Returns how much is currently buffered
      /// </summary>
      public int BufferedCount
      {
        get{ lock(m_Buffer) return m_Buffer.Count; }
      }

    #endregion

    #region Public
      
      
      /// <summary>
      /// Delivers data to the mailbox. This method is called by subscription
      /// </summary>
      public bool Deliver(Subscription subscription, RowChange data)
      {
        if (subscription.Store!=this.Store) return false;

        var cap = m_BufferCapacity;
        if (cap>0)
          lock(m_Buffer)
          {
            m_Buffer.AddLast(data);
            if (m_Buffer.Count>cap)
              m_Buffer.RemoveFirst();
          } 
        
        OnReceipt(subscription, data, null);

        return true;
      }

      public bool DeliverError(Subscription subscription, Exception error)
      {
        if (subscription.Store!=this.Store) return false;
        
        OnReceipt(subscription, default(RowChange), error);

        return true;
      }

    #endregion

    #region Protected

      /// <summary>
      /// Tries to take the specified number of samples in chronological order
      /// </summary>
      public List<RowChange> FetchBuffered(int count, bool keep = false)
      {
       var result = new List<RowChange>();

       if (count<=0) return result;
       
       lock(m_Buffer)
        for(var i=0; i<count; i++)
        {
          var first = m_Buffer.First;
          if (first==null) break;
          result.Add(first.Value);
          if (keep) continue;
          m_Buffer.RemoveFirst();
        } 

        return result;
      }

      protected virtual void OnReceipt(Subscription subscription, RowChange data, Exception error)
      {
        if (Receipt!=null)
          Receipt(subscription, this, data, error);
      }
    #endregion
  }

}
