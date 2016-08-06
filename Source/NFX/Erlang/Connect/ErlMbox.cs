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
using System.Linq;
using System.Threading;
using NFX.Erlang.Internal;

namespace NFX.Erlang
{
  /// <summary>
  /// Indicates arrival of a message to a given mailbox.
  /// The msg is of type ErlMsg, ErlExit, ErlDown, or ErlException.
  /// <returns>
  /// If returns true, the message is handled, and will not be put in the mailbox's queue.
  /// Otherwise, the message will be enqueued in the mailbox's queue.
  /// </returns>
  /// </summary>
  public delegate bool MailboxMsgEventHandler(ErlMbox mailbox, IQueable msg);

  /// <summary>
  /// Provides a simple mechanism for exchanging messages with Erlang
  /// processes or other instances of this class
  /// </summary>
  /// <remarks>
  /// Each mailbox is associated with a unique <see cref="ErlPid"/>
  /// that contains information necessary for delivery of messages.
  /// When sending messages to named processes or mailboxes, the sender
  /// pid is made available to the recipient of the message. When sending
  /// messages to other mailboxes, the recipient can only respond if the
  /// sender includes the pid as part of the message contents. The sender
  /// can determine his own pid by calling <see cref="Self"/>.
  ///
  /// Mailboxes can be named, either at creation or later. Messages
  /// can be sent to named mailboxes and named Erlang processes without
  /// knowing the <see cref="ErlPid"/> that identifies the mailbox.
  /// This is neccessary in order to set up initial communication between
  /// parts of an application. Each mailbox can have at most one name.
  ///
  /// Messages to remote nodes are externalized for transmission, and
  /// as a result the recipient receives a <b>copy</b> of the original
  /// C# object. To ensure consistent behaviour when messages are sent
  /// between local mailboxes, such messages are cloned before delivery.
  ///
  /// Additionally, mailboxes can be linked in much the same way as
  /// Erlang processes. If a link is active when a mailbox is closed
  /// any linked Erlang processes or ErlMbox's will be
  /// sent an exit signal. As well, exit signals will be (eventually)
  /// sent if a mailbox goes out of scope and its Dispose method called.
  /// However due to the nature of
  /// finalization (i.e. C# makes no guarantees about when Dispose
  /// will be called) it is recommended that you
  /// always explicitly close mailboxes if you are using links instead of
  /// relying on finalization to notify other parties in a timely manner.
  ///
  /// When retrieving messages from a mailbox that has received an exit
  /// signal, an <see cref="ErlExit"/> exception will be
  /// raised. Note that the exception is queued in the mailbox along with
  /// other messages, and will not be raised until it reaches the head of
  /// the queue and is about to be retrieved
  /// </remarks>
  public class ErlMbox : DisposableObject
  {
  #region CONSTS

    private const int SLEEP_GRANULARITY_MSEC = 5000;

  #endregion

  #region .ctor

    /// <summary>
    /// Create a mailbox with optional name
    /// </summary>
    internal ErlMbox(ErlLocalNode home, ErlPid self, string name = null)
        : this(home, self, name == null ? ErlAtom.Null : new ErlAtom(name))
    { }

    internal ErlMbox(ErlLocalNode home, ErlPid self, ErlAtom name)
    {
      m_Self = self;
      m_Node = home;
      m_RegName = name;
      m_Queue = new ErlBlockingQueue<IQueable>();
      m_Links = new ErlLinks();
      m_Monitors = new ErlMonitors(this);
    }

    protected override void Destructor()
    {
      base.Destructor();
      Close();
      m_Queue.Dispose();
    }

  #endregion

  #region Fields

    private ErlLocalNode m_Node;
    private ErlPid m_Self;
    private ErlBlockingQueue<IQueable> m_Queue;
    private ErlAtom m_RegName;
    private ErlLinks m_Links;
    private ErlMonitors m_Monitors;

  #endregion

  #region Events

    /// <summary>
    /// If this event is assigned, it will be called on arrival of a message, but
    /// the messages will not be put in the queue
    /// </summary>
    public event MailboxMsgEventHandler MailboxMessage;

  #endregion

  #region Proterties

    /// <summary>
    /// Get the Pid identifying associated with this mailbox
    /// </summary>
    public ErlPid Self { get { return m_Self; } }

    public ErlLocalNode Node { get { return m_Node; } }

    /// <summary>
    /// Get the registered name of this mailbox, or string.Empty if the
    /// mailbox doesn't have a registered name
    /// </summary>
    /// <returns></returns>
    public ErlAtom Name { get { return m_RegName; } }

    /// <summary>
    /// Return true if there are no messages waiting in the receive queue
    /// of this connection
    /// </summary>
    public bool Empty { get { return m_Queue.Empty; } }

    /// <summary>
    /// Return the number of messages currently waiting in the receive
    /// queue of this connection
    /// </summary>
    public int MsgCount { get { return m_Queue.Count; } }

    /// <summary>
    /// Queue not empty status notification handle
    /// </summary>
    public WaitHandle Handle { get { return m_Queue.Handle; } }

    /// <summary>
    /// Timestamps when this mailbox was last used - this is used internally
    /// by mailbox caching
    /// </summary>
    internal DateTime LastUsed { get; set; }

    public bool QueueActive{ get{ return m_Queue.Active;}}

  #endregion

  #region Public

    public void Clear()
    {
      m_Queue.Clear();
    }

    /// <summary>
    /// Register a name for this mailbox. Registering a
    /// name for a mailbox enables others to send messages without
    /// knowing the <see cref="ErlPid"/> of the mailbox. A mailbox
    /// can have at most one name; if the mailbox already had a name,
    /// calling this method will supercede that name
    /// </summary>
    /// <returns>true if the name was available, or false otherwise</returns>
    public bool Register(string name)
    {
      return Register(new ErlAtom(name));
    }

    public bool Register(ErlAtom name)
    {
      var res = m_Node.Mailboxes.Register(name, this);
      if (res)
        m_RegName = name;
      return res;
    }

    /// <summary>
    /// Receive a message and match it against a given pattern
    /// </summary>
    /// <param name="pattern">Pattern to match the message against</param>
    /// <param name="timeoutMsec">Timeout in milliseconds</param>
    /// <returns>Return a tuple containing the received message and variable
    /// binding object. On timeout the first element of the tuple is null.
    /// On unsuccessful match the second element of the tuple is null</returns>
    public Tuple<IErlObject, ErlVarBind> ReceiveMatch(IErlObject pattern, int timeoutMsec = -1)
    {
      var m = Receive(timeoutMsec);
      if (m == null) return Tuple.Create<IErlObject, ErlVarBind>(null, null);

      var binding = new ErlVarBind();
      bool res = m.Match(pattern, binding);
      return Tuple.Create(m, res ? binding : null);
    }

    /// <summary>
    /// Receive a message and match it against a given pattern
    /// </summary>
    /// <param name="pm">Patterns to match the message against</param>
    /// <param name="timeoutMsec">Timeout in milliseconds</param>
    /// <returns>Return a tuple containing the received message and index of the
    /// pattern in the pm instance that was successfully matched.
    /// On timeout the 2nd element of the tuple is -2 and 1st element of the tuple is null.
    /// On unsuccessful match the second element of the tuple is -1</returns>
    public Tuple<IErlObject, int> ReceiveMatch(ErlPatternMatcher pm, int timeoutMsec = -1)
    {
      var m = Receive(timeoutMsec);
      if (m == null) return Tuple.Create<IErlObject, int>(null, -2);

      int n = pm.Match(ref m);
      return Tuple.Create(m, n);
    }

    /// <summary>
    /// Wait for a message to arrive for this mailbox. On timeout return null
    /// </summary>
    /// <param name="timeout">Timeout time in milliseconds</param>
    /// <returns></returns>
    public IErlObject Receive(int timeout = -1)
    {
      var m = receiveMsg(timeout);
      return m == null ? null : m.Msg;
    }

    /// <summary>
    /// Send a message to a named mailbox created from another node
    /// </summary>
    public bool Send(ErlAtom node, ErlAtom name, IErlObject msg)
    {
      return m_Node.Deliver(node, ErlMsg.RegSend(m_Self, name, msg));
    }

    public IErlObject RPC(ErlAtom node, string mod, string fun, ErlList args, ErlAtom? remoteCookie = null)
    {
      return RPC(node, mod, fun, args, -1, remoteCookie);
    }

    public IErlObject RPC(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args, ErlAtom? remoteCookie = null)
    {
      return RPC(node, mod, fun, args, -1, remoteCookie);
    }

    public IErlObject RPC(ErlAtom node, string mod, string fun, ErlList args, int timeout, ErlAtom? remoteCookie = null)
    {
      return this.RPC(node, new ErlAtom(mod), new ErlAtom(fun), args, timeout, remoteCookie);
    }

    public IErlObject RPC(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args, int timeout, ErlAtom? remoteCookie = null)
    {
      AsyncRPC(node, mod, fun, args, remoteCookie);
      var r = m_Monitors.Add(node);
      using (Scope.OnExit(() => m_Monitors.Remove(r)))
      {
        return ReceiveRPC(timeout);
      }
    }

    public void AsyncRPC(ErlAtom node, string mod, string fun, ErlList args, ErlAtom? remoteCookie = null)
    {
      AsyncRPC(node, new ErlAtom(mod), new ErlAtom(fun), args, remoteCookie);
    }

    public void AsyncRPC(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args, ErlAtom? remoteCookie = null)
    {
      AsyncRPC(node, mod, fun, args, (IErlObject)m_Node.GroupLeader.Self, remoteCookie);
    }

    public void AsyncRPC(ErlAtom node, string mod, string fun, ErlList args, ErlPid ioServer,
                         ErlAtom? remoteCookie = null)
    {
      AsyncRPC(node, new ErlAtom(mod), new ErlAtom(fun), args, (IErlObject)ioServer, remoteCookie);
    }

    /// <summary>
    /// Send RPC call to a given node.
    /// </summary>
    /// <param name="node">Destination node for this RPC call</param>
    /// <param name="mod">Module name to call</param>
    /// <param name="fun">Function name to call</param>
    /// <param name="args">Function arguments</param>
    /// <param name="ioServer">Either a PID or an Atom containing registered I/O server's name.</param>
    /// <param name="remoteCookie">Remote cookie</param>
    public void AsyncRPC(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args,
                         IErlObject ioServer, ErlAtom? remoteCookie = null)
    {
      if (node.Equals(m_Node.NodeName))
        throw new ErlException(StringConsts.ERL_CONN_LOCAL_RPC_ERROR);
      else
      {
        IErlObject msg = Internal.ErlRpcServer.EncodeRPC(m_Self, mod, fun, args, ioServer);

        var conn = m_Node.Connection(node, remoteCookie);
        if (conn == null)
          throw new ErlConnectionException( node, StringConsts.ERL_CONN_CANT_CONNECT_TO_NODE_ERROR.Args(node));
        conn.Send(m_Self, ConstAtoms.Rex, msg);
      }
    }

    public void RPCcast(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args)
    {
      RPCcast(node, mod, fun, args, ConstAtoms.User);
    }

    public void RPCcast(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args, IErlObject ioServer)
    {
      if (node.Equals(m_Node.NodeName))
        throw new ErlException(StringConsts.ERL_CONN_CANT_RPC_TO_LOCAL_NODE_ERROR);
      else
      {
        var msg = Internal.ErlRpcServer.EncodeRPCcast(m_Self, mod, fun, args, ioServer);
        var conn = m_Node.Connection(node);
        if (conn == null)
          throw new ErlConnectionException(
              node, StringConsts.ERL_CONN_CANT_CONNECT_TO_NODE_ERROR.Args(node));
        conn.Send(m_Self, ConstAtoms.Rex, msg);
      }
    }

    public IErlObject ReceiveRPC(int timeout = -1)
    {
      var result = Receive(timeout);

      if (result == null)
        return result;
      else
      {
        var rpcReply = Internal.ErlRpcServer.DecodeRPC(result);
        return rpcReply == null ? result : rpcReply;
      }
    }

    public void Down(ErlRef eref, ErlPid pid, ErlAtom reason)
    {
      // TODO
      throw new NotImplementedException();
    }

    /// <summary>
    /// Link to a remote mailbox or Erlang process. Links are
    /// idempotent, calling this method multiple times will not result in
    /// more than one link being created
    /// </summary>
    /// <remarks>
    /// If the remote process subsequently exits or the mailbox is
    /// closed, a subsequent attempt to retrieve a message through this
    /// mailbox will cause an {@link Exit Exit}
    /// exception to be raised. Similarly, if the sending mailbox is
    /// closed, the linked mailbox or process will receive an exit
    /// signal.
    ///
    /// If the remote process cannot be reached in order to set the
    /// link, the exception is raised immediately.
    /// </remarks>
    public void Link(ErlPid to)
    {
      Debug.Assert(to != m_Self);
      if (m_Node.Deliver(ErlMsg.Link(m_Self, to)))
        m_Links.Add(to);
    }

    /// <summary>
    /// Remove a link to a remote mailbox or Erlang process. This
    /// method removes a link created with <see cref="ErlLink"/>
    /// Links are idempotent; calling this method once will remove all
    /// links between this mailbox and the remote <see cref="ErlPid"/>
    /// </summary>
    public void Unlink(ErlPid to)
    {
      m_Links.Remove(to);
      m_Node.Deliver(ErlMsg.Unlink(m_Self, to));
    }

    /// <summary>
    /// Determine if two mailboxes are equal
    /// </summary>
    public override bool Equals(object o)
    {
      if (!(o is ErlMbox))
        return false;

      ErlMbox m = (ErlMbox)o;
      return m.m_Self.Equals(m_Self);
    }

    public override int GetHashCode()
    {
      return m_Self.GetHashCode();
    }

    #endregion

  #region Protected / Internal

    protected void Enqueue(IQueable data)
    {
      if (!OnMailboxMessage(data))
        m_Queue.Enqueue(data);
    }

    protected IQueable Dequeue(int timeout = -1)
    {
      return m_Queue.Dequeue(timeout);
    }

    /// <summary>
    /// Close this mailbox
    /// </summary>
    /// <remarks>
    /// After this operation, the mailbox will no longer be able to
    /// receive messages. Any delivered but as yet unretrieved messages
    /// can still be retrieved however.
    ///
    /// If there are links from this mailbox to other <see cref="ErlPid"/>
    /// pids they will be broken when this method is
    /// called and exit signals will be sent.
    /// </remarks>
    internal void Close()
    {
      // Notify all registered monitors that this pid is closing
      foreach (var monitor in m_Monitors)
      {
        var msg = ErlMsg.MonitorPexit(m_Self, monitor.Value, monitor.Key, ErlAtom.Normal);
        m_Node.Deliver(monitor.Value.Node, msg);
      }
      m_Node.CloseMbox(this);
      m_RegName = ErlAtom.Null;
    }

    internal void Deliver(ErlConnectionException e)
    {
      Enqueue(e);
    }

    /// <summary>
    /// Called to deliver message to this mailbox
    /// </summary>
    internal void Deliver(ErlMsg m)
    {
      Debug.Assert((m.Recipient is ErlPid  && m.RecipientPid == m_Self)
                || (m.Recipient is ErlAtom && m.RecipientName == m_RegName));

      switch (m.Type)
      {
        case ErlMsg.Tag.Link:
          m_Links.Add(m.SenderPid);
          break;

        case ErlMsg.Tag.Unlink:
          m_Links.Remove(m.SenderPid);
          break;

        case ErlMsg.Tag.Exit:
        case ErlMsg.Tag.Exit2:
          m_Monitors.Remove(m.Ref);
          if (m_Links.Remove(m.SenderPid))
            Enqueue(m);
          break;

        case ErlMsg.Tag.MonitorP:
          m_Monitors.Add(m.Ref, m.SenderPid);
          break;

        case ErlMsg.Tag.DemonitorP:
          m_Monitors.Remove(m.Ref);
          break;

        case ErlMsg.Tag.MonitorPexit:
          m_Links.Remove(m.SenderPid);
          if (m_Monitors.Remove(m.Ref))
            Enqueue(new ErlDown(m.Ref, m.SenderPid, m.Reason));
          break;

        case ErlMsg.Tag.Undefined:
          break;

        default:
          Enqueue(m);
          break;
      }

    }

    /// <summary>
    /// Used to break all known links to this mbox
    /// </summary>
    internal void BreakLinks(ErlAtom fromNode, IErlObject reason)
    {
      var links = m_Links.Remove(fromNode);

      foreach (var link in links)
        if (link.HasPid)
          m_Node.Deliver(ErlMsg.Exit(m_Self, link.Pid, reason));
        else
          m_Node.Deliver(new ErlConnectionException(fromNode, reason));

      foreach (var m in m_Monitors.Where(o => o.Value.Node == fromNode)
                                  .Where(m => m_Monitors.Remove(m.Key)))
        Deliver(new ErlConnectionException(fromNode, reason));
    }

    /// <summary>
    /// Used to break all known links to this mbox
    /// </summary>
    internal void BreakLinks(ErlAtom reason)
    {
      foreach (var l in m_Links.Clear())
        m_Node.Deliver(ErlMsg.Exit(m_Self, l.Pid, reason));
    }

    protected virtual bool OnMailboxMessage(IQueable msg)
    {
      return MailboxMessage != null && MailboxMessage(this, msg);
    }

  #endregion

  #region .pvt

    /// <summary>
    /// Block until a message arrives for this mailbox
    /// </summary>
    /// <returns>a stream representing the still-encoded body of the next
    /// message waiting in this mailbox</returns>
    private ErlInputStream receiveBuf()
    {
      return receiveBuf(-1);
    }

    /// <summary>
    /// Wait for a message to arrive for this mailbox
    /// </summary>
    /// <param name="timeout">time in milliseconds</param>
    /// <returns>a stream representing the still-encoded body of the next
    /// message waiting in this mailbox, or null on timeout</returns>
    public ErlInputStream receiveBuf(int timeout)
    {
      var m = receiveMsg(timeout);
      return m == null ? null : m.Paybuf;
    }

    /*
    * Block until a message arrives for this mailbox.
    *
    * @return an {@link ErlMsg ErlMsg} containing the header
    * information as well as the body of the next message waiting in
    * this mailbox.
    *
    * @exception Exit if a linked {@link Pid pid} has
    * exited or has sent an exit signal to this mailbox.
    *
    **/
    private ErlMsg receiveMsg()
    {
      return receiveMsg(-1);
    }

    /// <summary>
    /// Receive a message complete with sender and recipient information
    /// from a remote process. This method blocks at most for the specified
    /// time, until a valid message is received or an exception is raised
    /// </summary>
    /// <param name="timeout">Time in milliseconds</param>
    /// <returns>Erlang distributed message or null on timeout</returns>
    private ErlMsg receiveMsg(int timeout)
    {
      var msg = Dequeue(timeout);

      if (msg == null)
        return null;

      if (msg is ErlMsg)
      {
        var m = (ErlMsg)msg;
        switch (m.Type)
        {
          case ErlMsg.Tag.Exit:
          case ErlMsg.Tag.Exit2:
            var o = m.Msg;
            throw new ErlExit(m.SenderPid, o);
        }
        return m;
      }
      else if (msg is ErlExit)
        throw (ErlExit)msg;
      else if (msg is ErlAuthException)
        throw (ErlAuthException)msg;
      else if (msg is ErlConnectionException)
        throw (ErlConnectionException)msg;
      else if (msg is ErlException)
        throw (ErlException)msg;
      else
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR.Args(msg.ToString()));
    }

  #endregion
  }
}
