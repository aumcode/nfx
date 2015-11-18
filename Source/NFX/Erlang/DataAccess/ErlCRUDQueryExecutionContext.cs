using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.CRUD;

namespace NFX.DataAccess.Erlang
{
  /// <summary>
    /// Provides query execution environment in Erlang context 
    /// </summary>
    public struct ErlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
        public ErlCRUDQueryExecutionContext(ErlDataStore store)
        {
          this.DataStore = store;
        }
       
        public readonly ErlDataStore  DataStore;
    }
}
