using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD.Subscriptions
{

  /// <summary>
  /// Represents a subscription to the CRUD Data.
  /// Call Dispose() to terminate the subscription.
  /// </summary>
  public abstract class Subscription : SubscriptionAppComponent, INamed
  {
    protected Subscription(ICRUDSubscriptionStoreImplementation store, string name, Query query, Mailbox mailbox) : base(store)
    {
      if (store==null ||
          query==null||
          mailbox==null) 
        throw new CRUDException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(args...null)");

      if (mailbox.Store!=store)
        throw new CRUDException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(mailbox.Store!=this.Store)");

      m_Store = store;
      m_Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
      m_Query = query;
      m_Mailbox = mailbox;

      var reg = m_Store.Subscriptions as Registry<Subscription>;
      Subscription existing;
      if (!reg.RegisterOrReplace(this, out existing))
       existing.Dispose();

      ((Registry<Subscription>)mailbox.Subscriptions).Register(this);
    }

    protected override void Destructor()
    {
      if (m_Mailbox!=null)
       ((Registry<Subscription>)m_Mailbox.Subscriptions).Unregister(this);
      
      if (m_Store!=null)
       ((Registry<Subscription>)m_Store.Subscriptions).Unregister(this);

      base.Destructor();
    }

    private ICRUDSubscriptionStoreImplementation m_Store;
    private string  m_Name;
    private Query   m_Query;
    private Mailbox m_Mailbox;

    public ICRUDSubscriptionStore Store{ get{ return m_Store;}}
    public string    Name     { get { return m_Name; } }
    public Query     Query    { get { return m_Query; } }
    public Mailbox   Mailbox  { get { return m_Mailbox; } }
  }

}
