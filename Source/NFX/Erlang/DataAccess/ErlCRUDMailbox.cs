using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.CRUD.Subscriptions;

namespace NFX.DataAccess.Erlang
{
  /// <summary>
  /// Implements mailboxes that receive data from Erlang CRUD data stores
  /// </summary>
  public class ErlCRUDMailbox : Mailbox
  {
    internal ErlCRUDMailbox(ErlDataStore store, string name) : base(store, name)
    {

    }
  }
}
