/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.ServiceModel;

namespace NFX.Web.Messaging
{

  /// <summary>
  /// Marker interface for injection into MailerSerive.Sink property from code
  /// </summary>
  public interface IMessageSink
  {
      MessageService ComponentDirector{ get;}
  }

  /// <summary>
  /// Base for ALL implementations that work under MailerService
  /// </summary>
  public abstract class MessageSink : Service<MessageService>, IMessageSink, IConfigurable
  {
      protected MessageSink(MessageService director) : base(director)
      {

      }


      /// <summary>
      /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread
      /// </summary>
      public void SendMsg(Message msg)
      {
        if (!Running) return;
        DoSendMsg(msg);
      }

      /// <summary>
      /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread
      /// </summary>
      protected abstract void DoSendMsg(Message msg);

  }

}
