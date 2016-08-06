using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD.Subscriptions
{

  /// <summary>
  /// Thrown by provider to indicate that such a subscribtion can not be established in principle,
  /// i.e. you may have passed a zero size of populus group, the server may respond with this error
  /// to indicate the principal error in request
  /// </summary>
  public class InvalidSubscriptionRequestException : CRUDException
  {
    public InvalidSubscriptionRequestException(string message, Exception inner) : base(message, inner) {}
    protected InvalidSubscriptionRequestException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }

  /// <summary>
  /// Represents a subscription to the CRUD Data.
  /// Call Dispose() to terminate the subscription.
  /// </summary>
  public abstract class Subscription : SubscriptionAppComponent, INamed
  {
    protected Subscription(ICRUDSubscriptionStoreImplementation store, string name, Query query, Mailbox mailbox, object correlate) : base(store)
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
      Correlate = correlate;

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

    private InvalidSubscriptionRequestException m_InvalidSubscriptionException;
    private ICRUDSubscriptionStoreImplementation m_Store;
    private string  m_Name;
    private bool    m_IsLoaded;
    private Query   m_Query;
    private Mailbox m_Mailbox;

    public ICRUDSubscriptionStore Store{ get{ return m_Store;}}
    public string    Name     { get { return m_Name; } }
    public bool      IsLoaded { get { return m_IsLoaded; } }
    public Query     Query    { get { return m_Query; } }
    public Mailbox   Mailbox  { get { return m_Mailbox; } }


    /// <summary>
    /// If this property is not null then this subscription is !IsValid
    /// </summary>
    public InvalidSubscriptionRequestException InvalidSubscriptionException
    {
      get{ return m_InvalidSubscriptionException;}
    }

    /// <summary>
    /// Returns false when this subscription experienced InvalidSubscriptionException
    /// </summary>
    public bool IsValid{ get{ return m_InvalidSubscriptionException==null;}}

    /// <summary>
    /// Allows to attach arbtrary bject for correlation
    /// </summary>
    public object Correlate{get; set;}

    /// <summary>
    /// Called by descendants to invalidate this subscription
    /// </summary>
    protected void Invalidate(InvalidSubscriptionRequestException error)
    {
      m_InvalidSubscriptionException = error;
    }

    /// <summary>
    /// Call after subscription has been initialized
    /// </summary>
    protected void HasLoaded()
    {
      m_IsLoaded = true;
    }
  }

}
