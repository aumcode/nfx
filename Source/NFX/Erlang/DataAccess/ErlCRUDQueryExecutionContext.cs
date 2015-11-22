using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.CRUD;
using NFX.Erlang;

namespace NFX.DataAccess.Erlang
{
  /// <summary>
    /// Provides query execution environment in Erlang context 
    /// </summary>
    public struct ErlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
        public ErlCRUDQueryExecutionContext(ErlDataStore store, ErlMbox erlMBox = null)
        {
          this.DataStore = store;
          ErlMailBox = erlMBox;
        }
       
        public readonly ErlDataStore  DataStore;
        public readonly ErlMbox ErlMailBox;
    }
}
