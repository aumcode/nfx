using System;
using System.Collections.Generic;

namespace NFX.Web.Pay.Braintree
{
  public class BraintreeWebTerminal : PayWebTerminal
  {
    public BraintreeWebTerminal(BraintreeSystem paySystem)
      : base(paySystem) {}

    public override object GetPayInit()
    {
      using (var session = PaySystem.StartSession())
      {
        var btSession = session as BraintreeSession;
        if (btSession == null) return string.Empty;
        return new { publicKey = btSession.ClientToken };
      }
    }
  }
}
