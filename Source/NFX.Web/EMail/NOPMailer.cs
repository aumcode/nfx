using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.ApplicationModel;
using NFX.ServiceModel;

namespace NFX.Web.EMail
{
  /// <summary>
  /// Implements NOP Mailer that does nothing
  /// </summary>
  public sealed class NOPMailer : Service, IMailerImplementation
  {
    public NOPMailer() : base(){}

    public void SendMsg(MailMsg msg, SendPriority? handlingPriority = null)
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
