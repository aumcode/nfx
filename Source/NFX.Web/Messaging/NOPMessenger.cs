using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.ApplicationModel;
using NFX.ServiceModel;

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Implements NOP Mailer that does nothing
  /// </summary>
  public sealed class NOPMessenger : Service, IMessengerImplementation
  {
    public NOPMessenger() : base(){}

    public void SendMsg(Message msg)
    {

    }

    void IApplicationFinishNotifiable.ApplicationFinishBeforeCleanup(IApplication application)
    {

    }

    void IApplicationFinishNotifiable.ApplicationFinishAfterCleanup(IApplication application)
    {

    }
  }
}
