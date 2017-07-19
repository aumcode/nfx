/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using NFX.Log;
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
  public abstract class MessageSink : ServiceWithInstrumentationBase<MessageService>, IMessageSink, IConfigurable
  {
    private const string LOG_TOPIC = "Messaging.MessageSink";

    protected MessageSink(MessageService director) : base(director)
    {

    }

    #region Properties

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled{ get; set;}

    public abstract MsgChannels SupportedChannels { get; }

    public virtual IEnumerable<string> SupportedChannelNames { get { yield return Name; } }


    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING)]
    public SendMessageErrorHandling ErrorHandlingMode{ get ; set; }

    #endregion

    /// <summary>
    /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread
    /// </summary>
    public bool SendMsg(Message msg)
    {
      if (msg == null) throw new WebException(StringConsts.ARGUMENT_ERROR + "MessageSink.SendMsg(msg==null)");

      if (!Running) return false;
      if (!Filter(msg)) return false;

      var sent = DoSendMsg(msg);
      if (!sent && ErrorHandlingMode == SendMessageErrorHandling.Throw)
        throw new WebException(StringConsts.SENDING_MESSAGE_HAS_NOT_SUCCEEDED.Args(Name));

      return sent;
    }

    #region Protected

      protected virtual bool Filter(Message msg)
      {
        return msg.MessageAddress.MatchNamedChannel(this.SupportedChannelNames);
      }

      /// <summary>
      /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread
      /// </summary>
      protected abstract bool DoSendMsg(Message msg);

      protected Guid Log(MessageType type,
                         string from,
                         string message,
                         Exception error = null,
                         Guid? relatedMessageID = null,
                         string parameters = null)
      {
        if (type < ComponentDirector.LogLevel) return Guid.Empty;

        var logMessage = new Log.Message
        {
          Topic = LOG_TOPIC,
          Text = message ?? string.Empty,
          Type = type,
          From = "{0}.{1}".Args(this.GetType().Name, from),
          Exception = error,
          Parameters = parameters
        };
        if (relatedMessageID.HasValue) logMessage.RelatedTo = relatedMessageID.Value;

        App.Log.Write(logMessage);

        return logMessage.Guid;
      }
    #endregion
  }
}
