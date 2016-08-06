using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.Messaging
{
  public sealed class NOPMessageSink:MessageSink
  {
    #region .ctor
    public NOPMessageSink(MessageService director)
      : base(director)
    {
    }
    #endregion

    #region Protected
    protected override void DoSendMsg(Message msg)
    {
    }
    #endregion
  }
}
