using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD.Subscriptions
{
  /// <summary>
  /// Describes a CRUD store that can send ROW data via an established subscriptions into local recipients.
  /// Subscriptions are really stateless, they do not capture the data, however the Recipients do.
  /// Recipients can either support pull (caller has to fetch data), or reactive (calling delegate) modes
  /// </summary>
  public interface ICRUDSubscriptionStore : IDataStore
  {

    /// <summary>
    /// Subscribes the local recipient to the remote data store by executing a Query.
    /// The subscription ends by calling a .Dispose()
    /// </summary>
    /// <param name="name">The store-wide-unique subscription name</param>
    /// <param name="query">The query that informs the remote store what data to send back</param>
    /// <param name="recipient">The local Recipient which will receive remote data</param>
    /// <param name="correlate">Adhock correlation object</param>
    /// <returns>The subscription. It may be ended by calling .Dispose()</returns>
    Subscription Subscribe(string name, Query query, Mailbox recipient, object correlate = null);

    /// <summary>
    /// Returns existing mailbox by name (case-insensitive) or creates a new named mailbox
    /// </summary>
    Mailbox OpenMailbox(string name);

    /// <summary>
    /// Returns registry of all active subscriptions
    /// </summary>
    IRegistry<Subscription> Subscriptions { get;}

    /// <summary>
    /// Returns registry of all mailboxes
    /// </summary>
    IRegistry<Mailbox> Mailboxes { get;}

  }

  public interface ICRUDSubscriptionStoreImplementation : ICRUDSubscriptionStore, IDataStoreImplementation
  {

  }

}
