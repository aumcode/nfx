using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD.Subscriptions
{

  /// <summary>
  /// Represents a subscription to the CRUD Data
  /// </summary>
  public sealed class Subscription : SubscriptionAppComponent, INamed
  {
    public Subscription(ICRUDSubscriptionStore store) : base(store)
    {

    }

    protected override void Destructor()
    {
      base.Destructor();
    }
    
    public string Name
    {
      get { throw new NotImplementedException(); }
    }
  }

}
