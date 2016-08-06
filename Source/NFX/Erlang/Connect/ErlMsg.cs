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

namespace NFX.Erlang
{
  /// <summary>
  /// Represents a interface that messages enqueued to ErlMbox must support
  /// </summary>
  public interface IQueable { };

  /// <summary>
  /// Provides a distributed carrier for Erlang messages
  /// </summary>
  /// <remarks>
  /// Instances of this class are created to package header and
  /// payload information in received Erlang messages so that the
  /// recipient can obtain both parts with a single call to
  /// <see cref="ErlMbox.receiveMsg()"/>
  ///
  /// The header information that is available is as follows:
  /// <ul>
  /// <li>a tag indicating the type of message</li>
  /// <li>the intended recipient of the message, either as a
  ///     <see cref="ErlPid"/> or as a <see cref="ErlAtom"/>, but never both.</li>
  /// <li>(sometimes) the sender of the message. Due to some eccentric
  ///     characteristics of the Erlang distribution protocol, not
  ///     all messages have information about the sending process. In
  ///     particular, only messages whose tag is <see cref="ErlMsg.Tag.RegSend"/>
  ///     contain sender information.</li>
  /// </ul>
  ///
  /// Message are sent using the Erlang external format (see separate
  /// documentation). When a message is received and delivered to the
  /// recipient <see cref="ErlMbox"/>, the body of the message is still
  /// in this external representation until <see cref="Msg"/>
  /// is called, at which point the message is decoded. A copy of the
  /// decoded message is stored in the OtpMsg so that subsequent calls to
  /// <see cref="Msg"/> do not require that the message be decoded
  /// a second time.
  /// </remarks>
  public class ErlMsg : IQueable
  {
  #region CONSTS / Enums

    /// <summary>
    /// Erlang message header tags
    /// </summary>
    public enum Tag
    {
      Undefined       = -1,
      Link            = 1,
      Send            = 2,
      Exit            = 3,
      Unlink          = 4,
      NodeLink        = 5,
      RegSend         = 6,
      GroupLeader     = 7,
      Exit2           = 8,
      SendTT          = 12,
      ExitTT          = 13,
      RegSendTT       = 16,
      Exit2TT         = 18,
      MonitorP        = 19,
      DemonitorP      = 20,
      MonitorPexit    = 21
    };

  #endregion

  #region .ctor / Factory
    // All message types are described here:
    // http://www.erlang.org/doc/apps/erts/erl_dist_protocol.html#id91435

    internal static ErlMsg Link(ErlPid from, ErlPid dest)
    {
      return new ErlMsg(Tag.Link, from, dest);
    }

    internal static ErlMsg Send(ErlPid dest, IErlObject msg, ErlAtom? cookie = null)
    {
      return new ErlMsg(Tag.Send, ErlPid.Null, dest, payload: msg, cookie: cookie);
    }

    internal static ErlMsg Exit(ErlPid from, ErlPid dest, IErlObject reason)
    {
      return new ErlMsg(Tag.Exit, from, dest, reason: reason);
    }

    internal static ErlMsg Unlink(ErlPid from, ErlPid dest)
    {
      return new ErlMsg(Tag.Unlink, from, dest);
    }

    internal static ErlMsg NodeLink()
    {
      return new ErlMsg(Tag.NodeLink, ErlPid.Null, ErlPid.Null);
    }

    internal static ErlMsg RegSend(ErlPid from, ErlAtom dest, IErlObject msg, ErlAtom? cookie = null)
    {
      return new ErlMsg(Tag.RegSend, from, dest, payload: msg, cookie: cookie);
    }

    internal static ErlMsg GroupLeader(ErlPid from, ErlPid dest)
    {
      return new ErlMsg(Tag.GroupLeader, from, dest);
    }

    internal static ErlMsg Exit2(ErlPid from, ErlPid dest, IErlObject reason)
    {
      return new ErlMsg(Tag.Exit2, from, dest, reason: reason);
    }

    internal static ErlMsg SendTT(ErlPid dest, IErlObject msg, ErlTrace traceToken, ErlAtom? cookie = null)
    {
      return new ErlMsg(Tag.SendTT, ErlPid.Null, dest, payload: msg, trace: traceToken, cookie: cookie);
    }

    internal static ErlMsg ExitTT(ErlPid from, ErlPid dest, IErlObject reason, ErlTrace traceToken)
    {
      return new ErlMsg(Tag.ExitTT, from, dest, reason: reason, trace: traceToken);
    }

    internal static ErlMsg RegSendTT(ErlPid from, ErlAtom dest, IErlObject msg,
        ErlTrace trace, ErlAtom? cookie = null)
    {
      return new ErlMsg(Tag.RegSend, from, dest, payload: msg, cookie: cookie);
    }

    internal static ErlMsg Exit2TT(ErlPid from, ErlPid dest, IErlObject reason, ErlTrace traceToken)
    {
      return new ErlMsg(Tag.Exit2TT, from, dest, reason: reason, trace: traceToken);
    }

    internal static ErlMsg MonitorP(ErlPid from, IErlObject /* Pid or Atom */ dest, ErlRef eref)
    {
      return new ErlMsg(Tag.MonitorP, from, dest, eref: eref);
    }

    internal static ErlMsg DemonitorP(ErlPid from, IErlObject /* Pid or Atom */ dest, ErlRef eref)
    {
      return new ErlMsg(Tag.DemonitorP, from, dest, eref: eref);
    }

    internal static ErlMsg MonitorPexit(IErlObject from, ErlPid dest, ErlRef eref, IErlObject reason)
    {
      return new ErlMsg(Tag.MonitorPexit, from, dest, eref: eref, reason: reason);
    }

    /*
    internal ErlMsg(Tag tag, ErlMsg msg)
        : this(tag, msg.Sender, msg.Recipient, msg.Ref, msg.Reason, msg.Payload,
               msg.m_Paybuf, msg.TraceToken)
    {}
    */

    internal ErlMsg(Tag tag, IErlObject /* Pid or Atom */ from,
        IErlObject /* Pid or Atom */ to, ErlRef? eref = null, IErlObject reason = null,
        IErlObject payload = null, ErlInputStream paybuf = null,
        ErlAtom? cookie = null, ErlTrace trace = null)
    {
      if (!(from is ErlPid || from is ErlAtom))
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_TYPE_ERROR.Args("sender", from.GetType().Name));
      if (!(to is ErlPid || to is ErlAtom))
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_TYPE_ERROR.Args("recipient", from.GetType().Name));

      m_From      = from;
      m_To        = to;
      Type        = tag;
      Paybuf      = paybuf;
      m_Payload   = payload;
      Reason      = reason;
      Ref         = eref.HasValue ? eref.Value : ErlRef.Null;
      Cookie      = cookie.HasValue ? cookie.Value : ErlAtom.Null;
      TraceToken  = trace;
    }

  #endregion

  #region Fields

    private IErlObject m_From; // ErlPid or ErlAtom
    private IErlObject m_To;   // ErlPid or ErlAtom
    private IErlObject m_Payload;

    /// <summary>
    /// The type of message
    /// </summary>
    public readonly Tag Type; // what type of message is this

    /// <summary>
    /// Returns reference contained in this message
    /// </summary>
    public readonly ErlRef Ref;

    /// <summary>
    /// Reason associated with some messages
    /// </summary>
    public readonly IErlObject Reason;

    /// <summary>
    /// Contains cookie if it is to be sent or was received in the message
    /// </summary>
    public readonly ErlAtom Cookie;

    /// <summary>
    /// Trace token delivered in the message
    /// </summary>
    public readonly ErlTrace TraceToken;

    /// <summary>
    /// The payload of this deserialized message
    /// </summary>
    public readonly ErlInputStream Paybuf;

  #endregion

  #region Props

    /// <summary>
    /// Deserialized Erlang message contained in this instance
    /// </summary>
    public IErlObject Msg
    {
      get
      {
        if (m_Payload != null)
          return m_Payload;
        if (Paybuf == null)
          return ErlList.Empty;
        m_Payload = Paybuf.Read(true);
        return m_Payload;
      }
    }

    internal IErlObject Payload { get { return m_Payload; } }

    /// <summary>
    /// Get the name of the recipient for this message
    /// </summary>
    /// <remarks>
    /// Messages are sent to Pids or names. If this message was sent
    /// to a name then the name is returned by this method
    /// </remarks>
    public ErlAtom RecipientName { get { return (ErlAtom)m_To; } }

    /// <summary>
    /// Get the Pid of the recipient for this message, if it is a Send message
    /// </summary>
    /// <remarks>
    /// Messages are sent to Pids or names. If this message was sent
    /// to a Pid then the Pid is returned by this method. The recipient
    /// Pid is also available for link, unlink and exit messages
    /// </remarks>
    public ErlPid RecipientPid { get { return (ErlPid)m_To; } }

    /// <summary>
    /// Get the recipient for this message, as a Pid or a registered name (Atom)
    /// </summary>
    public IErlObject Recipient { get { return m_To; } }

    /// <summary>
    /// Returns pid of the sender (may be empty, i.e. SenderPid.Empty == true)
    /// </summary>
    public ErlPid SenderPid { get { return (ErlPid)m_From; } }
    public ErlAtom SenderName { get { return (ErlAtom)m_From; } internal set { m_From = value; } }

    /// <summary>
    /// Get the sender for this message, as a Pid or a registered name (Atom)
    /// </summary>
    public IErlObject Sender { get { return m_From; } }

    /// <summary>
    /// Returns true if the message has a non-empty sender
    /// </summary>
    public bool HasSender
    {
      get
      {
        return (m_From is ErlPid && !((ErlPid)m_From).Empty)
            || (m_From is ErlAtom && !((ErlAtom)m_From).Empty);
      }
    }

    public override string ToString()
    {
      return "#ErlMsg{{{0}, {1}to={2}, msg={3}}}".Args(
          Type, HasSender ? "from={0}, ".Args(m_From) : string.Empty, m_To, Msg.ToString());
    }

  #endregion

  #region Internal

    internal static ErlTuple ToCtrlTuple(ErlMsg msg)
    {
      return msg.ToCtrlTuple();
    }

    internal ErlTuple ToCtrlTuple()
    {
      return ToCtrlTuple(Cookie);
    }

    internal ErlTuple ToCtrlTuple(ErlAtom cookie)
    {
      switch (Type)
      {
        case Tag.Link           : return new ErlTuple((int)Type, m_From, m_To);
        case Tag.Send           : return new ErlTuple((int)Type, cookie, m_To);
        case Tag.Exit           : return new ErlTuple((int)Type, m_From, m_To, Reason);
        case Tag.Unlink         : return new ErlTuple((int)Type, m_From, m_To);
        case Tag.NodeLink       : return new ErlTuple((int)Type);
        case Tag.RegSend        : return new ErlTuple((int)Type, m_From, cookie, m_To);
        case Tag.GroupLeader    : return new ErlTuple((int)Type, m_From, m_To);
        case Tag.Exit2          : return new ErlTuple((int)Type, m_From, m_To, Reason);
        case Tag.SendTT         : return new ErlTuple((int)Type, cookie, m_To, TraceToken);
        case Tag.Exit2TT        :
        case Tag.ExitTT         : return new ErlTuple((int)Type, m_From, m_To, TraceToken, Reason);
        case Tag.RegSendTT      : return new ErlTuple((int)Type, m_From, cookie, m_To, TraceToken);
        case Tag.MonitorP       :
        case Tag.DemonitorP     : return new ErlTuple((int)Type, m_From, Recipient, Ref);
        case Tag.MonitorPexit   : return new ErlTuple((int)Type, Sender, m_To, Ref, Reason);
        default                 : throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR.Args(Type));
      }
    }

  #endregion
  }
}
