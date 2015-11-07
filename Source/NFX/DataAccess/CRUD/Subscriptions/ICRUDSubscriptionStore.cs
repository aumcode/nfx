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
    /// <returns>The subscription. It may be ended by calling .Dispose()</returns>
    Subscription Subscribe(string name, Query query, Recipient recipient);

    /// <summary>
    /// Returns registry of all active subscriptions
    /// </summary>
    IRegistry<Subscription> Subscriptions { get;}

    /// <summary>
    /// Returns registry of all recipients
    /// </summary>
    IRegistry<Recipient> Recipients { get;}

    /// <summary>
    /// Returns registry of type mappings that map remote data to CLR row types
    /// </summary>
    IRegistry<RowTypeMapping> TypeMappings{ get; }

  }

  public interface ICRUDSubscriptionStoreImplementation : IDataStoreImplementation
  {

  }

}
