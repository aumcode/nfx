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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Net;

using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Erlang
{
  public delegate void NodeStatusCallback(ErlAtom node, bool up, object info);
  public delegate void ConnAttemptCallback(ErlAtom node, Direction dir, object info);
  public delegate void UnhandledMsgCallback(ErlConnection conn, ErlMsg msg);
  public delegate void IoOutputCallback(ErlAtom encoding, IErlObject output);

  /// <summary>
  /// Delegate called on read/write from socket
  /// </summary>
  public delegate void ReadWriteCallback(
      ErlAbstractConnection con, Direction op, int lastBytes, long totalBytes, long totalMsgs);

  public class ErlLocalNode : ErlAbstractNode, IApplicationFinishNotifiable
  {
  #region .ctor

    /// <summary>
    /// Makes local node name based on app id and local host name
    /// </summary>
    public static string MakeLocalNodeForThisAppOnThisHost()
    {
      var hndl = Math.Abs(App.InstanceID.GetHashCode()).ToString("X8");
      return "{0}@{1}".Args(hndl, System.Environment.MachineName);
    }


    /// <summary>
    /// Create a node with the given name and the default cookie
    /// </summary>
    public ErlLocalNode(string node, bool shortName = true)
        : this(node, ErlAtom.Null, shortName)
    { }

    /// <summary>
    /// Create a node with the given name, cookie, and short name indicator
    /// </summary>
    public ErlLocalNode(string node, ErlAtom cookie, bool shortName = true)
        : base(node, cookie, shortName)
    { }

    internal ErlLocalNode(string node, IConfigSectionNode config)
        : base(node, config)
    {
      var addr = m_AcceptAddressPort.Split(':').FirstOrDefault();
      if (node.IndexOf('@') < 0 && addr != null)
        node = "{0}@{1}".Args(node, addr);

      SetNodeName(node);
    }

  #endregion

  #region Fields

    private int     m_PidCount  = 0;
    private int     m_PortCount = 0;
    private uint[]  m_RefIds = new uint[] { 0,0,0 };

    private bool    m_AcceptConnections = true;
    private string  m_AcceptAddressPort = string.Empty;

    // Since the space of unique Pids is limited, we cache mailboxes used for RPC calls
    private ConcurrentQueue<ErlMbox> m_MboxFreelist =
        new ConcurrentQueue<ErlMbox>();
        
    // Manages incoming connections
    private Internal.ErlAcceptor    m_Acceptor;
    // Manages distributed I/O sent from remote Erlang nodes
    private Internal.ErlIoServer    m_IoServer;
    // Processes all external RPC calls
    private Internal.ErlRpcServer   m_RpcServer;

    // keep track of all connections
    private ConcurrentDictionary<ErlAtom, ErlConnection> m_Connections;

    // keep track of all mailboxes
    internal Internal.MboxRegistry m_Mailboxes;

    private ErlMbox m_GroupLeader;

    private TraceCallback        m_OnTrace;
    private NodeStatusCallback   m_OnNodeStatus;
    private ConnAttemptCallback  m_OnConnectAttempt;
    private UnhandledMsgCallback m_OnUnhandledMsg;
    private IoOutputCallback     m_OnIoOutput;

    /// <summary>
    /// Delegate invoked on read/write from socket
    /// </summary>
    protected ReadWriteCallback m_OnReadWrite;

  #endregion

  #region Props

    /// <summary>
    /// Contains node creation bits that facilitate Pid uniqueness
    /// upon node restart
    /// </summary>
    public byte Creation { get; internal set; }

    public IEnumerator<KeyValuePair<ErlAtom, ErlConnection>> Connections
    {
      get { return m_Connections.GetEnumerator(); }
    }

    internal Internal.MboxRegistry Mailboxes { get { return m_Mailboxes; } }

    /// <summary>
    /// If true local node will start a listener
    /// </summary>
    [Config("$accept")]
    public bool AcceptConnections
    {
      get { return m_AcceptConnections; }
      set { m_AcceptConnections = value; }
    }

    /// <summary>
    /// Configuration Address and Port information for the listener in
    /// the "address:port" format 
    /// </summary>
    [Config("$address")]
    internal string AcceptAddressPort
    {
      get { return m_AcceptAddressPort; }
      set { m_AcceptAddressPort = value; }
    }

    /// <summary>
    /// Mailbox for handling all I/O directed from remote nodes
    /// </summary>
    public ErlMbox GroupLeader { get { return m_GroupLeader; } }

    /// <summary>
    /// Set the trace level for this connection. Normally tracing is off by default
    /// </summary>
    [Config("$trace")]
    public ErlTraceLevel TraceLevel { get; set; }

    /// <summary>
    /// Trace callback executed if connection tracing is enabled
    /// </summary>
    public TraceCallback        OnTrace         { set { m_OnTrace = value; } }

    public NodeStatusCallback   OnNodeStatus    { set { m_OnNodeStatus = value; } }
    public ConnAttemptCallback  OnConnectAttempt{ set { m_OnConnectAttempt = value; } }
    public UnhandledMsgCallback OnUnhandledMsg  { set { m_OnUnhandledMsg = value; } }
    public ReadWriteCallback    OnReadWrite     { set { m_OnReadWrite = value; } }
    public IoOutputCallback     OnIoOutput      { set { m_OnIoOutput = value; } }

  #endregion

  #region Public

    public ErlConnection Connection(string node, ErlAtom? cookie = null)
    {
      var nodeName = ErlAbstractNode.NodeShortName(node, UseShortName);
      return Connection(nodeName, cookie);
    }

    internal ErlConnection Connection(string toNode, IConfigSectionNode config)
    {
      var peer = new ErlRemoteNode(this, toNode); // This will convert toNode to a valid ErlAtom

      lock (m_Connections)
      {
        ErlConnection c;

        if (m_Connections.TryGetValue(peer.NodeName, out c))
          return c;

        peer.Configure(config); // Fetch all proper TCP and other settings

        try { c = new ErlConnection(this, peer, true); }
        catch { return null; }

        m_Connections.TryAdd(peer.NodeName, c);

        return c;
      }
    }

    public ErlConnection Connection(ErlAtom toNode, ErlAtom? cookie = null)
    {
      ErlConnection c;
      if (m_Connections.TryGetValue(toNode, out c))
        return c;

      var peer = new ErlRemoteNode(this, toNode, cookie);

      lock (m_Connections)
      {
        if (m_Connections.TryGetValue(toNode, out c))
          return c;

        try { c = new ErlConnection(this, peer, true); }
        catch { return null; }

        m_Connections.TryAdd(toNode, c);

        return c;
      }
    }

    /// <summary>
    /// Add a connection to collection
    /// </summary>
    /// <returns>Returns false if this connection was already previously added</returns>
    public bool Add(ErlConnection conn)
    {
      var res = m_Connections.TryAdd(conn.Name, conn);
      if (res)
        NodeStatus(conn.Name, true, null);
      return res;
    }

    /// <summary>
    /// Remove a connection from collection
    /// </summary>
    public void Remove(ErlConnection conn)
    {
      ErlConnection value;
      m_Connections.TryRemove(conn.Name, out value);
    }

    /// <summary>
    /// Create a new mailbox (emulates spawning a new Pid)
    /// </summary>
    public ErlMbox CreateMbox() { return m_Mailboxes.Create(); }

    /// <summary>
    /// Create a new named mailbox (emulates spawning a new Pid)
    /// </summary>
    public ErlMbox CreateMbox(string name) { return CreateMbox(new ErlAtom(name)); }

    /// <summary>
    /// Create a new named mailbox (emulates spawning a new Pid)
    /// </summary>
    public ErlMbox CreateMbox(ErlAtom name) { return m_Mailboxes.Create(name); }

    /// <summary>
    /// Close the given mailbox
    /// </summary>
    public void CloseMbox(ErlMbox mbox)
    {
      m_Mailboxes.Unregister(mbox);
    }

    /// <summary>
    /// Determine the mailbox corresponding to a
    /// registered name on this <see cref="ErlLocalNode"/>
    /// </summary>
    public ErlMbox FindMbox(ErlAtom name)
    {
      return m_Mailboxes[name];
    }

    /// <summary>
    /// Send a message to a remote <see cref="ErlPid"/>, representing
    /// either another <see cref="ErlMbox"/> or an Erlang process
    /// </summary>
    /// <returns>true if message was sent successfully</returns>
    public bool Send(ErlPid to, IErlObject msg)
    {
      return Deliver(ErlMsg.Send(to, msg));
    }

    /// <summary>
    /// Send a message to a named mailbox on a given remote node
    /// </summary>
    public bool Send(ErlPid from, ErlAtom remoteNode, ErlAtom toName, IErlObject msg)
    {
      return Deliver(remoteNode, ErlMsg.RegSend(from, toName, msg));
    }

    /// <summary>
    /// Send a message to a named mailbox on local node
    /// </summary>
    public bool Send(ErlPid from, ErlAtom toName, IErlObject msg)
    {
      return Deliver(ErlMsg.RegSend(from, toName, msg));
    }

    /// <summary>
    /// Create an Erlang {@link Ref reference}. Erlang
    /// references are based upon some node specific information; this
    /// method creates a reference using the information in this node.
    /// Each call to this method produces a unique reference
    /// </summary>
    public ErlRef CreateRef()
    {
      lock (m_RefIds)
      {
        // increment ref ids (3 ints: 18 + 32 + 32 bits)
        m_RefIds[0]++;
        if (m_RefIds[0] > 0x3ffff)
        {
          m_RefIds[0] = 0;
          m_RefIds[1]++;
          if (m_RefIds[1] == 0)
            m_RefIds[2]++;
        }

        return new ErlRef(NodeName, m_RefIds, m_Creation);
      }
    }

    public IErlObject RPC(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args, int timeout)
    {
      var mbox = m_Mailboxes.Create(true);
      try
      {
        var res = mbox.RPC(node, mod, fun, args, timeout);
        return res;
      }
      finally
      {
        m_Mailboxes.Unregister(mbox);
      }
    }

    public int WaitAny(params ErlMbox[] mboxes)
    {
      return WaitAny(mboxes, -1);
    }

    public int WaitAny(int msecTimeout, params ErlMbox[] mboxes)
    {
      return WaitAny(mboxes, msecTimeout);
    }

    public int WaitAny(IEnumerable<ErlMbox> mboxes, int msecTimeout = -1)
    {
      return WaitAny(mboxes.ToArray(), msecTimeout);
    }

    /// <summary>
    /// Wait for arrival of messages in any one of the given mailboxes
    /// </summary>
    /// <param name="mboxes">Mailboxes to wait for messages in</param>
    /// <param name="msecTimeout">Timeout in milliseconds</param>
    /// <returns>Index of the first non-empty mailbox or -1 on timeout</returns>
    public int WaitAny(ErlMbox[] mboxes, int msecTimeout = -1)
    {
      for (int i = 0; i < mboxes.Length; i++)
        if (!mboxes[i].Empty)
          return i;
      if (msecTimeout == 0)
        return -1;
      int n = WaitHandle.WaitAny(mboxes.Select(m => m.Handle).ToArray(), msecTimeout);
      return n == WaitHandle.WaitTimeout ? -1 : n;
    }

  #endregion

  #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
    }

    /// <summary>
    /// Start current node. This optionally creates a socket listener
    /// and I/O server
    /// </summary>
    protected override void DoStart()
    {
      m_Connections = new ConcurrentDictionary<ErlAtom, ErlConnection>();
      m_Mailboxes = new Internal.MboxRegistry(this);

      //bool acceptConnections = true, short port = 0, IPAddress addr = null
      if (m_IoServer != null)
        throw new ErlException("Already started!");

      m_GroupLeader = m_Mailboxes.Create(ConstAtoms.User);

      if (m_AcceptConnections)
      {
        var addr = m_AcceptAddressPort.IsNullOrEmpty()
                 ? new IPEndPoint(0, 0) : m_AcceptAddressPort.ToIPEndPoint();

        m_Acceptor = new Internal.ErlAcceptor(this, addr.Port, addr.Address);
      }

      m_IoServer = new Internal.ErlIoServer(this);
      m_RpcServer = new Internal.ErlRpcServer(this);
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();

      if (m_Acceptor != null)
        m_Acceptor.Dispose();
      if (m_IoServer != null)
        m_IoServer.Dispose();
      if (m_RpcServer != null)
        m_RpcServer.Dispose();

      if (m_Connections != null)
      {
        foreach (var c in m_Connections)
          c.Value.Dispose();

        m_Connections.Clear();
      }

      if (m_Mailboxes != null)
      {
        foreach (var m in m_Mailboxes)
          CloseMbox(m.Value);
        ErlMbox mb;
        while (MboxFreelist.TryDequeue(out mb)) ;
      }
    }

  #endregion

  #region Internal

    //---------------------------------------------------------------------
    // Application model shutdown
    //---------------------------------------------------------------------
    public void ApplicationFinishBeforeCleanup(IApplication application)
    {
      ErlApp.Node = null;
    }

    public void ApplicationFinishAfterCleanup(IApplication application)
    { }

    internal bool Deliver(ErlConnectionException e)
    {
      var fromNode = e.Node;
      NodeStatus(fromNode, false, e.Message);

      var conn = Connection(fromNode);
      if (conn != null)
      {
        ErlConnection c;
        m_Connections.TryRemove(fromNode, out c);
      }

      BreakLinks(fromNode, new ErlString(e.Message));
      return true;
    }

    internal bool Deliver(ErlAtom node, ErlMsg msg)
    {
      if (node == NodeName)
        return Deliver(msg);

      var conn = Connection(node);
      if (conn == null)
        return false;

      try { conn.Send(msg); }
      catch { return false; }
      return true;
    }

    internal bool Deliver(ErlMsg msg)
    {
      if (msg.Recipient is ErlPid)
      {
        var node = msg.RecipientPid.Node;
        if (node != NodeName)
          return Deliver(node, msg);
      }

      var mbox = msg.Type == ErlMsg.Tag.RegSend || msg.Type == ErlMsg.Tag.RegSendTT
          ? m_Mailboxes[msg.RecipientName]
          : m_Mailboxes[msg.RecipientPid];

      if (mbox == null)
        return false;

      Trace(ErlTraceLevel.Send, Direction.Inbound, () =>
          "{0}{1} got: {2}".Args(
              mbox.Self, mbox.Name.Empty ? string.Empty : " ({0})".Args(mbox.Name.ToString()),
              msg.Msg));

      mbox.Deliver(msg);

      return true;
    }

    /// <summary>
    /// Break links of all pids linked to pids on the fromNode node
    /// </summary>
    internal void BreakLinks(ErlAtom fromNode, IErlObject reason)
    {
      foreach (var m in m_Mailboxes)
        m.Value.BreakLinks(fromNode, reason);
    }

    /// <summary>
    /// Cache of freed mailboxes that can be reused for RPC calls
    /// </summary>
    /// <remarks>This is needed because the unique Pid space is limited to 2^28</remarks>
    internal ConcurrentQueue<ErlMbox> MboxFreelist { get { return m_MboxFreelist; } }

    /// <summary>
    /// Create an Erlang <see cref="ErlPid"/> that belongs to current node
    /// </summary>
    internal static ErlPid CreateNullPid(ErlAtom node)
    {
      return new ErlPid(node, 0, 0);
    }

    /// <summary>
    /// Create an Erlang <see cref="ErlPid"/>. Erlang pids are based
    /// upon some node specific information; this method creates a pid
    /// using the information in this node. Each call to this method
    /// produces a unique pid
    /// </summary>
    internal ErlPid CreatePid()
    {
      int n;

      // The label handles conditional jump in a very rare case of an overflow
      while (true)
      {
        n = Interlocked.Increment(ref m_PidCount);

        if (n < (1 << 28))
          break;

        if (Interlocked.CompareExchange(ref m_PidCount, 1, n) == n)
        {
          n = 1;
          break;
        }
      }

      // TODO: need to deal with pid numbering in case of large number of pids
      // when ID+Serial exceeds 18 bits.

      return new ErlPid(NodeName, n, m_Creation);
    }

    /// <summary>
    /// Create an Erlang <see cref="ErlPort"/>. Erlang ports are
    /// based upon some node specific information; this method creates a
    /// port using the information in this node. Each call to this method
    /// produces a unique port. It may not be meaningful to create a port
    /// in a non-Erlang environment, but this method is provided for
    /// completeness
    /// </summary>
    internal ErlPort CreatePort()
    {
    // The label handles conditional jump in a very rare case of an overflow
    repeat: int n = Interlocked.Increment(ref m_PortCount);

      while (n > 0x0fffffff)
      {
        Interlocked.CompareExchange(ref m_PortCount, 0, n);
        goto repeat;
      }

      return new ErlPort(NodeName, n, m_Creation);
    }

    internal void NodeStatus(ErlAtom node, bool up, object info)
    {
      if (m_OnNodeStatus != null)
        m_OnNodeStatus(node, up, info);
    }

    internal void ConnectAttempt(ErlAtom node, Direction dir, object info)
    {
      if (m_OnConnectAttempt != null)
        m_OnConnectAttempt(node, dir, info);
    }

    internal void UnhandledMsg(ErlConnection conn, ErlMsg msg)
    {
      if (m_OnUnhandledMsg != null)
        m_OnUnhandledMsg(conn, msg);
    }

    internal void ReadWrite(ErlAbstractConnection conn, Direction dir,
        int lastBytes, long totalBytes, long totalMsgs)
    {
      if (m_OnReadWrite != null)
        m_OnReadWrite(conn, dir, lastBytes, totalBytes, totalMsgs);
    }

    internal void IoOutput(ErlAtom encoding, IErlObject output)
    {
      if (m_OnIoOutput != null)
        m_OnIoOutput(encoding, output);
    }

    internal void Trace(ErlTraceLevel type, Direction dir, Func<string> msgFunc)
    {
      if (m_OnTrace != null && ErlApp.TraceEnabled(type, TraceLevel))
        m_OnTrace(type, dir, msgFunc());
    }

    internal void Trace(ErlTraceLevel type, Direction dir, string msg)
    {
      if (m_OnTrace != null && ErlApp.TraceEnabled(type, TraceLevel))
        m_OnTrace(type, dir, msg);
    }

  #endregion

  #region .pvt
  #endregion
  }
}
