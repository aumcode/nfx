/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
