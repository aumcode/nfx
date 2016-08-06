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
using System.Runtime.CompilerServices;
using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Erlang
{
  public delegate void NodeStatusChangeEventHandler(
    ErlLocalNode sender, ErlAtom node, bool up, object info);

  public delegate void ConnAttemptEventHandler(
    ErlLocalNode sender, ErlAtom node, Direction dir, object info);

  public delegate void UnhandledMsgEventHandler(
    ErlLocalNode sender, ErlConnection conn, ErlMsg msg);

  public delegate void IoOutputEventHandler(
    ErlLocalNode sender, ErlAtom encoding, IErlObject output);

  public delegate void EpmdFailedConnAttemptEventHandler(
    ErlLocalNode sender, ErlAtom node, object info);

  /// <summary>
  /// Delegate called on read/write from socket
  /// </summary>
  public delegate void ReadWriteEventHandler(
    ErlLocalNode sender, ErlAbstractConnection con, Direction op, int lastBytes,
    long totalBytes, long totalMsgs);

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
    public ErlLocalNode(string node, bool shortName = true, bool acceptConns = true)
      : this(node, ErlAtom.Null, shortName, acceptConns) {}

    /// <summary>
    /// Create a node with the given name, cookie, and short name indicator
    /// </summary>
    public ErlLocalNode(string node, ErlAtom cookie, bool shortName = true,
                        bool acceptConns = true)
      : base(node, cookie, shortName)
    {
      m_AcceptConnections = acceptConns;
      TraceLevel          = ErlApp.DefaultTraceLevel;
    }

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

    private int m_PidCount = 0;
    private int m_PortCount = 0;
    private uint[] m_RefIds = new uint[] {0, 0, 0};

    private bool m_AcceptConnections = true;
    private string m_AcceptAddressPort = string.Empty;

    // Since the space of unique Pids is limited, we cache mailboxes used for RPC calls
    private ConcurrentQueue<ErlMbox> m_MboxFreelist =
      new ConcurrentQueue<ErlMbox>();

    // Manages incoming connections
    private Internal.ErlAcceptor m_Acceptor;
    // Manages distributed I/O sent from remote Erlang nodes
    private Internal.ErlIoServer m_IoServer;
    // Processes all external RPC calls
    private Internal.ErlRpcServer m_RpcServer;

    // keep track of all connections
    private ConcurrentDictionary<ErlAtom, ErlConnection> m_Connections;

    // keep track of all mailboxes
    internal Internal.MboxRegistry m_Mailboxes;

    private ErlMbox m_GroupLeader;

    /// <summary>
    /// Delegate invoked on read/write from socket
    /// </summary>
    protected ReadWriteEventHandler m_OnReadWrite;

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

    internal Internal.MboxRegistry Mailboxes
    {
      get { return m_Mailboxes; }
    }

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
    public ErlMbox GroupLeader
    {
      get { return m_GroupLeader; }
    }

    /// <summary>
    /// Set the trace level for this connection. Normally tracing is off by default
    /// </summary>
    [Config("$trace")]
    public ErlTraceLevel TraceLevel { get; set; }

    /// <summary>
    /// Save trace events to log
    /// </summary>
    [Config]
    public bool TraceToLog { get; set; }

    /// <summary>
    /// Record unhandled msgs to log
    /// </summary>
    [Config]
    public bool LogUnhandledMsgs { get; set; }

    /// <summary>
    /// Configs for remote nodes
    /// </summary>
    public ConfigSectionNode AllNodeConfigs { get; internal set; }

    public IEnumerable<ConfigSectionNode> RemoteNodeConfigs
    {
      get
      {
        return AllNodeConfigs.Children
                             .Where(n =>  n.Name.EqualsSenseCase("node")
                                      && !n.AttrByName(ErlConsts.CONFIG_IS_LOCAL_ATTR).ValueAsBool());
      }
    }

    /// <summary>
    /// Get configuration settings for a given remote node
    /// </summary>
    public ConfigSectionNode RemoteNodeConfig(string remoteNodeName)
    {
      return AllNodeConfigs.NavigateSection("/node[{0}]".Args(remoteNodeName));
    }

    /// <summary>
    /// Trace callback executed if connection tracing is enabled
    /// </summary>
    public event TraceEventHandler Trace;

    public event NodeStatusChangeEventHandler NodeStatusChange;
    public event ConnAttemptEventHandler ConnectAttempt;
    public event UnhandledMsgEventHandler UnhandledMsg;
    public event ReadWriteEventHandler ReadWrite;
    public event IoOutputEventHandler IoOutput;
    public event EpmdFailedConnAttemptEventHandler EpmdFailedConnectAttempt;

    #endregion

    #region Public

    public ErlConnection Connection(string node, ErlAtom? cookie = null)
    {
      var nodeName = ErlAbstractNode.NodeShortName(node, UseShortName);
      return Connection(nodeName, cookie);
    }

    internal ErlConnection Connection(string toNode, IConfigSectionNode config,
                                      ErlAtom? cookie = null)
    {
      var peer = new ErlRemoteNode(this, toNode);
        // This will convert toNode to a valid ErlAtom

      lock (m_Connections)
      {
        ErlConnection c;

        if (m_Connections.TryGetValue(peer.NodeName, out c))
          return c;

        peer.Configure(config); // Fetch all proper TCP and other settings

        try
        {
          c = new ErlConnection(this, peer, true);
        }
        catch
        {
          return null;
        } // TODO: maybe add return reason for connection failure?

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

      //try to append config to remote node
      TryAppendConfigToRemoteNode(peer);

      lock (m_Connections)
      {
        if (m_Connections.TryGetValue(toNode, out c))
          return c;

        try
        {
          c = new ErlConnection(this, peer, true);
        }
        catch
        {
          return null;
        }

        m_Connections.TryAdd(toNode, c);

        return c;
      }
    }

    public bool Disconnect(string fromNode)
    {
        ErlConnection c;
        if (m_Connections.TryRemove(fromNode, out c))
        {
          c.Dispose();
          return true;
        }
        return false;
    }


    /// <summary>
    /// Add a connection to collection
    /// </summary>
    /// <returns>Returns false if this connection was already previously added</returns>
    public bool Add(ErlConnection conn)
    {
      var res = m_Connections.TryAdd(conn.Name, conn);
      if (res)
        OnNodeStatusChange(conn.Name, true, null);
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
    /// Create a new named mailbox (emulates spawning a new Pid)
    /// </summary>
    public ErlMbox CreateMbox(string name = null)
    {
      CheckServiceActiveOrStarting();
      return name.IsNullOrWhiteSpace() ? m_Mailboxes.Create()
                                       : CreateMbox(new ErlAtom(name));
    }


    /// <summary>
    /// Create a new named mailbox (emulates spawning a new Pid)
    /// </summary>
    public ErlMbox CreateMbox(ErlAtom name)
    {
      CheckServiceActiveOrStarting();
      return m_Mailboxes.Create(name);
    }

    /// <summary>
    /// Close the given mailbox
    /// </summary>
    public void CloseMbox(ErlMbox mbox)
    {
      if (m_Mailboxes != null)
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

    public IErlObject RPC(ErlAtom node, ErlAtom mod, ErlAtom fun, ErlList args,
                          int timeout)
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
          ? new IPEndPoint(0, 0)
          : m_AcceptAddressPort.ToIPEndPoint();

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


    protected internal virtual void OnNodeStatusChange(ErlAtom node, bool up, object info)
    {
      if (NodeStatusChange != null) NodeStatusChange(this, node, up, info);
    }

    protected internal virtual void OnConnectAttempt(ErlAtom node, Direction dir,
                                                     object info)
    {
      if (ConnectAttempt != null) ConnectAttempt(this, node, dir, info);
    }

    protected internal virtual void OnUnhandledMsg(ErlConnection conn, ErlMsg msg,
                                                   [CallerFilePath]  string file = null,
                                                   [CallerLineNumber]int    line = 0)
    {
      if (UnhandledMsg != null) UnhandledMsg(this, conn, msg);
      if (LogUnhandledMsgs)
        App.Log.Write(new NFX.Log.Message(null, file, line)
        {
          Type = Log.MessageType.TraceErl,
          Topic = CoreConsts.ERLANG_TOPIC,
          From = this.ToString(),
          Text = "Connection {0} couldn't find mbox for: {1}".Args(conn.Name, msg)
        });
    }

    protected internal virtual void OnReadWrite(ErlAbstractConnection conn, Direction dir,
                                                int lastBytes, long totalBytes,
                                                long totalMsgs)
    {
      if (ReadWrite != null) ReadWrite(this, conn, dir, lastBytes, totalBytes, totalMsgs);
    }

    protected internal virtual void OnIoOutput(ErlAtom encoding, IErlObject output)
    {
      if (IoOutput != null) IoOutput(this, encoding, output);
    }

    protected internal virtual void OnTrace(ErlTraceLevel type, Direction dir, Func<string> msgFunc,
                                            [CallerFilePath]  string file = null,
                                            [CallerLineNumber]int    line = 0)
    {
      if (!ErlApp.TraceEnabled(type, TraceLevel)) return;
      var msg = msgFunc();
      OnTraceCore(type, dir, msg, file, line);
    }

    protected internal void OnTrace(ErlTraceLevel type, Direction dir, string msg,
                                    [CallerFilePath]  string file = null,
                                    [CallerLineNumber]int line = 0)
    {
      if (!ErlApp.TraceEnabled(type, TraceLevel)) return;
      OnTraceCore(type, dir, msg, file, line);
    }

    protected virtual void OnTraceCore(ErlTraceLevel type, Direction dir, string msg,
                                       string file, int line)
    {
      if (Trace != null) Trace(this, type, dir, msg);
      if (TraceToLog && ErlApp.TraceEnabled(type, TraceLevel))
        App.Log.Write(new NFX.Log.Message(null, file, line)
        {
          Type = Log.MessageType.TraceErl,
          Topic = "{0}.{1}".Args(CoreConsts.ERLANG_TOPIC, type),
          From = this.ToString(),
          Text = msg
        });
    }

    protected internal void OnEpmdFailedConnectAttempt(ErlAtom node, object info)
    {
      if (EpmdFailedConnectAttempt != null)
        EpmdFailedConnectAttempt(this, node, info);
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

    public void ApplicationFinishAfterCleanup(IApplication application) {}

    internal bool Deliver(ErlConnectionException e)
    {
      var fromNode = e.Node;
      OnNodeStatusChange(fromNode, false, e.Message);

      ErlConnection c;
      m_Connections.TryRemove(fromNode, out c);

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

      try
      {
        conn.Send(msg);
      }
      catch
      {
        return false;
      }
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

      OnTrace(ErlTraceLevel.Send, Direction.Inbound, () =>
                                                       "{0}{1} got: {2}".Args(
                                                         mbox.Self,
                                                         mbox.Name.Empty
                                                           ? string.Empty
                                                           : " ({0})".Args(
                                                             mbox.Name.ToString()),
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
    internal ConcurrentQueue<ErlMbox> MboxFreelist
    {
      get { return m_MboxFreelist; }
    }

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
      repeat:
      int n = Interlocked.Increment(ref m_PortCount);

      while (n > 0x0fffffff)
      {
        Interlocked.CompareExchange(ref m_PortCount, 0, n);
        goto repeat;
      }

      return new ErlPort(NodeName, n, m_Creation);
    }

    #endregion

    #region .pvt

    private void TryAppendConfigToRemoteNode(ErlRemoteNode peer)
    {
      if (AllNodeConfigs == null) return;

      var cfg = RemoteNodeConfig(peer.NodeName);

      if (cfg.Exists)
        peer.Configure(cfg);
    }

    #endregion
  }
}