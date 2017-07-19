using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Log;

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Registry of different sinks. Purpose - main sink for message service
  /// </summary>
  public class CompositeMessageSink : MessageSink
  {
    public CompositeMessageSink(MessageService director) : base(director)
    {
    }

    private Registry<MessageSink> m_Sinks;

    #region Properties

    public Registry<MessageSink> Sinks
    {
      get { return m_Sinks; }
      set
      {
        CheckServiceInactive();
        if (m_Sinks != null && m_Sinks.Any() && m_Sinks.First().ComponentDirector != this.ComponentDirector)
          throw new WebException(StringConsts.MESSAGE_SINK_IS_NOT_OWNED_ERROR);
        m_Sinks = value;
      }
    }

    public override MsgChannels SupportedChannels
    {
      get
      {
        MsgChannels result = MsgChannels.Unspecified;
        if (m_Sinks == null || !m_Sinks.Any()) return result;
        foreach (var sink in m_Sinks)
          result = result | SupportedChannels;
        return result;
      }
    }

    public override IEnumerable<string> SupportedChannelNames
    {
      get
      {
        return m_Sinks.SelectMany(s => s.SupportedChannelNames).Distinct();
      }
    }

    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (m_Sinks == null) m_Sinks = new Registry<MessageSink>();
      foreach (var sect in node.Children)
      {
        var sink = FactoryUtils.MakeAndConfigure<MessageSink>(sect, args: new object[] { this.ComponentDirector });
        m_Sinks.Register(sink);
      }
    }

    protected override void DoStart()
    {
      try
      {
        if (m_Sinks == null || !m_Sinks.Any())
          throw new WebException("Message sink registry is empty");

        foreach (var sink in m_Sinks)
          sink.Start();
      }
      catch (Exception error)
      {
        AbortStart();
        App.Log.Write(MessageType.CatastrophicError, error.ToMessageWithType(), "DoStart() exception", "CompositeMessageSink");
        throw error;
      }
    }

    protected override void DoSignalStop()
    {
      try
      {
        if (m_Sinks == null || !m_Sinks.Any()) return;
        foreach (var sink in m_Sinks)
          sink.SignalStop();
      }
      catch (Exception error)
      {
        App.Log.Write(MessageType.CatastrophicError, error.ToMessageWithType(), "DoSignalStop() exception", "CompositeMessageSink");
        throw error;
      }
    }

    protected override void DoWaitForCompleteStop()
    {
      try
      {
        if (m_Sinks == null || !m_Sinks.Any()) return;
        foreach (var sink in m_Sinks)
          sink.WaitForCompleteStop();
      }
      catch (Exception error)
      {
        App.Log.Write(MessageType.CatastrophicError, error.ToMessageWithType(), "DoWaitForCompleteStop() exception", "CompositeMessageSink");
        throw error;
      }
    }

    protected override bool DoSendMsg(Message msg)
    {
      var sent = false;
      foreach (var sink in m_Sinks)
        try
        {
          var wasSentNow = sink.SendMsg(msg);

          sent |= wasSentNow;
        }
        catch (Exception error)
        {
          var et = error.ToMessageWithType();
          Log(MessageType.Error, "{0}.DoSendMsg(msg): {1}".Args(this.GetType().FullName, et), et);
        }

      return sent;
    }

    #endregion
  }
}
