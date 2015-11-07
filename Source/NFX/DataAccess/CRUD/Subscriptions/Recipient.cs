using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD.Subscriptions
{
  
  /// <summary>
  /// Describes event handlers that get called when data arrives
  /// </summary>
  public delegate void SubscriptionDataReceiptEventHandler(Subscription subscription, Recipient recipient, Row data);
  
  
  /// <summary>
  /// Represents CRUD row data recipient
  /// </summary>
  public class Recipient : SubscriptionAppComponent, INamed
  {
    public Recipient(ICRUDSubscriptionStore store) : base(store)
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


    public event SubscriptionDataReceiptEventHandler DataReceipt;

    protected virtual void OnDataReceipt(Subscription subscription, Row data)
    {
      if (DataReceipt!=null)
       DataReceipt(subscription, this, data);
    }

  }


}
