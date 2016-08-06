using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.CRUD;
using NFX.DataAccess.CRUD.Subscriptions;
using NFX.Erlang;

namespace NFX.DataAccess.Erlang
{
  /// <summary>
    /// Provides query execution environment in Erlang context
    /// </summary>
    public struct ErlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
        public ErlCRUDQueryExecutionContext(ErlDataStore store, ErlMbox erlMBox = null, DataTimeStamp? ts = null)
        {
          this.DataStore = store;
          ErlMailBox = erlMBox;
          SubscriptionTimestamp = ts;
        }

        public readonly ErlDataStore  DataStore;
        public readonly ErlMbox ErlMailBox;
        public readonly DataTimeStamp? SubscriptionTimestamp;
    }
}
