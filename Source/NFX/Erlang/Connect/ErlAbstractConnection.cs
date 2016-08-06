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
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;

namespace NFX.Erlang
{
  /// <summary>
  /// Maintains a connection between a C# process and a remote Erlang,
  /// C# or C node. The object maintains connection state and allows
  /// data to be sent to and received from the peer
  /// </summary>
  /// <remarks>
  /// This abstract class provides the neccesary methods to maintain
  /// the actual connection and encode the messages and headers in the
  /// proper format according to the Erlang distribution protocol.
  /// Subclasses can use these methods to provide a more or less
  /// transparent communication channel as desired.
  ///
  /// Note that no receive methods are provided. Subclasses must
  /// provide methods for message delivery, and may implement their own
  /// receive methods.
  ///
  /// If an exception occurs in any of the methods in this class, the
  /// connection will be closed and must be reopened in order to resume
  /// communication with the peer. This will be indicated to the subclass
  /// by passing the exception to its delivery() method
  /// </remarks>
  public abstract class ErlAbstractConnection : DisposableObject
  {
  #region CONSTS

    protected const int HEADER_LEN = 2048; // more than enough
    protected const int DEFAULT_MAX_PAYLOAD_LENGTH = 128 * 1024 * 1024; // max size of the payload sent by peer

    protected const byte PASS_THROUGH           = 0x70;

    // MD5 challenge messsage tags
    protected const int CHALLENGE_REPLY         = 'r';
    protected const int CHALLENGE_ACK           = 'a';
    protected const int CHALLENGE_STATUS        = 's';

    private static int      s_ConnectTimeout    = 5000;
    protected static Random s_Random            = new Random();

  #endregion

  #region .ctor

    private ErlAbstractConnection(ErlLocalNode home, ErlRemoteNode peer, IErlTransport s, ErlAtom? cookie = null)
    {
      m_Peer = peer;
      m_Home = home;
      m_Transport = s;
      m_Cookie = !cookie.HasValue || cookie.Value == ErlAtom.Null
          ? (peer.Cookie == ErlAtom.Null ? home.Cookie : peer.Cookie) : cookie.Value;
      m_SentBytes = 0;
      m_ReceivedBytes = 0;
      m_MaxPayloadLength = DEFAULT_MAX_PAYLOAD_LENGTH;
      if (m_Transport != null)
        m_Transport.Trace += (o, t, d, msg) => home.OnTrace(t, d, msg);
    }

    /// <summary>
    /// Accept an incoming connection from a remote node. Used by ErlLocalNode.Accept
    /// to create a connection
    /// based on data received when handshaking with the peer node, when
    /// the remote node is the connection intitiator.
    /// </summary>
    protected ErlAbstractConnection(ErlLocalNode home, IErlTransport s)
        : this(home, new ErlRemoteNode(home), s, home.Cookie)
    {
      setSockOpts();
      //this.socket.ReceiveTimeout = 5000;

      home.OnTrace(ErlTraceLevel.Handshake, Direction.Inbound, () =>
              StringConsts.ERL_CONN_ACCEPT_FROM.Args(
                  IPAddress.Parse(s.RemoteEndPoint.ToString()).ToString(),
                  (s.RemoteEndPoint as IPEndPoint).Port.ToString()));

      // get his info
      recvName(m_Peer);

      // now find highest common dist value
      if ((m_Peer.Proto != home.Proto) || (home.DistHigh < m_Peer.DistLow) || (home.DistLow > m_Peer.DistHigh))
      {
        Close();
        throw new ErlException(StringConsts.ERL_CONN_NO_COMMON_PROTO_ERROR);
      }

      // highest common protocol version
      m_Peer.DistChoose = Math.Min(m_Peer.DistHigh, home.DistHigh);

      doAccept();
    }

    /// <summary>
    /// Intiate and open a connection to a remote node
    /// </summary>
    protected ErlAbstractConnection(ErlLocalNode self, ErlRemoteNode other, ErlAtom? cookie = null, bool connect = true)
        : this(self, other, null, cookie)
    {
      // now find highest common dist value
      if ((m_Peer.Proto != self.Proto) || (self.DistHigh < m_Peer.DistLow) || (self.DistLow > m_Peer.DistHigh))
        throw new ErlException(StringConsts.ERL_CONN_NO_COMMON_PROTO_ERROR);

      // highest common version: min(m_Peer.distHigh, self.distHigh)
      m_Peer.DistChoose = Math.Min(m_Peer.DistHigh, self.DistHigh);

      m_Connected = false;

      if (connect)
        Connect();
    }

    protected override void Destructor()
    {
      base.Destructor();
      Close();
    }

  #endregion

  #region Fields

    protected bool          m_Connected = false;  // connection status
    protected IErlTransport m_Transport;          // communication channel
    protected ErlRemoteNode m_Peer;               // who are we connected to
    protected ErlLocalNode  m_Home;               // the local node this connection is attached to
    protected ErlAtom       m_Cookie;

    private long m_SentBytes;                     // Total number of bytes sent
    private long m_ReceivedBytes;                 // Total number of bytes received
    private long m_SentMsgs;                      // Total number of messages sent
    private long m_ReceivedMsgs;                  // Total number of messages received
    private int  m_MaxPayloadLength;

    private byte[] m_Buf2 = new byte[2];
    private byte[] m_BufN = new byte[64 * 1024];
    protected bool m_CookieOk = false; // already checked the cookie for this connection
    protected bool m_ShouldSendCookie = true; // Send cookies in messages?

  #endregion

  #region Props

    /// <summary>
    /// Set the trace level for this connection. Normally tracing is off by default
    /// </summary>
    public ErlTraceLevel TraceLevel { get { return m_Home.TraceLevel; } }

    public static int ConnectTimeout
    {
      get { return s_ConnectTimeout; }
      set { s_ConnectTimeout = value; }
    }

    /// <summary>
    /// Total number of bytes sent through connection
    /// </summary>
    public long SentBytes { get { return m_SentBytes; } }

    /// <summary>
    /// Total number of bytes received from connection
    /// </summary>
    public long ReceivedBytes { get { return m_ReceivedBytes; } }

    /// <summary>
    /// Total number of messages sent through connection
    /// </summary>
    public long SentMsgs { get { return m_SentMsgs; } }

    /// <summary>
    /// Total number of messages received from connection
    /// </summary>
    public long ReceivedMsgs { get { return m_ReceivedMsgs; } }

    /// <summary>
    /// Cookie to send along with each distribution message
    /// </summary>
    protected ErlAtom SendCookie { get { return m_ShouldSendCookie ? m_Cookie : ErlAtom.Null; } }

    /// <summary>
    /// Max size of the message accepted from the peer.
    /// The connection will be closed if a message is received of size greater than this.
    /// </summary>
    public int MaxPayloadLength
    {
      get { return m_MaxPayloadLength; }
      set { m_MaxPayloadLength = value; }
    }

    public ErlLocalNode LocalNode { get { return m_Home; } }
    public ErlRemoteNode RemoteNode { get { return m_Peer; } }

    public ErlAtom Name { get { return m_Peer.NodeName; } }

    public ErlAtom Cookie { get { return m_Cookie; } }

    /// <summary>
    /// Determine if the connection is still alive. Note that this method
    /// only reports the status of the connection, and that it is
    /// possible that there are unread messages waiting in the receive
    /// queue
    /// </summary>
    public bool Connected { get { return m_Connected; } }

  #endregion

  #region Public

    /// <summary>
    /// Deliver communication exceptions to the recipient
    /// </summary>
    protected abstract void Deliver(ErlConnectionException e);

    /// <summary>
    /// Deliver messages to the recipient
    /// </summary>
    protected abstract void Deliver(ErlMsg msg);

    /// <summary>
    /// Close the connection to the remote node
    /// </summary>
    protected virtual void Close()
    {
      m_Connected = false;
      if (m_Transport != null)
        lock (m_Transport)
        {
          if (m_Transport != null)
          {
            m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, "CLOSE");
            try { m_Transport.Close(); }
            catch { }
            finally { m_Transport = null; }
          }
        }
    }

    /// <summary>
    /// Send an RPC request to the remote Erlang node
    /// </summary>
    /// <param>mod the name of the Erlang module containing the function to be called</param>
    /// <param>fun the name of the function to call</param>
    /// <param>args a list of Erlang terms, to be used as arguments to the function</param>
    /// <remarks>This convenience function creates the following message
    /// and sends it to 'rex' on the remote node:
    ///
    /// <code>
    /// { Self, { call, Mod, Fun, Args, user }}
    /// </code>
    ///
    /// Note that this method has unpredicatble results if the remote
    /// node is not an Erlang node
    /// </remarks>
    public void SendRPC(ErlPid from, string mod, string fun, ErlList args)
    {
      IErlObject rpc = Internal.ErlRpcServer.EncodeRPC(from, mod, fun, args, ConstAtoms.User);
      var msg = ErlMsg.RegSend(from, ConstAtoms.Rex, rpc);
      Send(msg);
    }

    /// <summary>
    /// Send an RPC cast request to the remote Erlang node
    /// </summary>
    public void SendRPCcast(ErlPid from, string mod, string fun, ErlList args)
    {
      IErlObject rpc = Internal.ErlRpcServer.EncodeRPCcast(from, mod, fun, args, ConstAtoms.User);
      var msg = ErlMsg.RegSend(from, ConstAtoms.Rex, rpc);
      Send(msg);
    }

  #endregion

  #region Protected / Internal

    internal void Send(ErlMsg msg)
    {
      var ctrl = msg.ToCtrlTuple(SendCookie);
      sendBuf(ctrl, msg.Payload==null ? null : new ErlOutputStream(msg.Payload));
    }

    internal void Connect()
    {
      if (m_Connected)
        throw new ErlException(StringConsts.ERL_CONN_ALREADY_CONNECTED_ERROR);

      if (m_Transport != null)
        m_Transport.Trace += (o, t, d, msg) => m_Home.OnTrace(t, d, msg);

      // now get a connection between the two...
      int port = 0;

      using (var ps = ErlTransportPasswordSource.StartPasswordSession(m_Peer.NodeName, m_Peer.SSHUserName))
      {
        // now get a connection between the two...
        port = ErlEpmd.LookupPort(LocalNode, m_Peer, true);

        if (port == 0)
          throw new ErlException(StringConsts.ERL_EPMD_INVALID_PORT_ERROR.Args(m_Peer.NodeName));

        doConnect(port);
      }

      m_Peer.Port = port;

      m_Connected = true;
    }

    // used by Send and SendReg (message types with payload)
    protected void DoSend(ErlOutputStream header, ErlOutputStream payload = null)
    {
      bool tw = ErlApp.TraceEnabled(TraceLevel, ErlTraceLevel.Wire);
      bool ts = ErlApp.TraceEnabled(TraceLevel, ErlTraceLevel.Send);

      if (tw || ts)
      {
        var h = header.InputStream(5).Read(true);

        m_Home.OnTrace(ErlTraceLevel.Wire, Direction.Outbound, () =>
        {
          var hb = header.ToBinary();
          return "{0} {1} (header_sz={2}{3})\n   Header: {4}{5}"
              .Args(HeaderType(h), h.ToString(), hb.Length,
                  payload == null ? string.Empty : ", msg_sz={0}".Args(payload.Length),
                  hb.ToBinaryString(),
                  payload == null ? string.Empty : "\n   Msg:    {0}".Args(payload.ToBinaryString()));
        });

        m_Home.OnTrace(ErlTraceLevel.Send, Direction.Outbound, () =>
        {
          var o = payload == null ? "" : payload.InputStream(0).Read(true).ToString();
          return "{0} {1} {2}".Args(HeaderType(h), h.ToString(), o);
        });
      }

      var written = (int)header.Length;

      lock (m_Transport)
      {
        try
        {
          header.WriteTo(m_Transport.GetStream());
          if (payload != null)
          {
            written += (int)payload.Length;
            payload.WriteTo(m_Transport.GetStream());
          }

          m_SentBytes += written;
          m_SentMsgs++;
        }
        catch (Exception e)
        {
          Close();
          throw e;
        }
      }

      onReadWrite(Direction.Outbound, written, m_SentBytes, m_SentMsgs);
    }

    protected string HeaderType(IErlObject h)
    {
      var tag = h is ErlTuple
          ? (ErlMsg.Tag)(((ErlTuple)h)[0].ValueAsInt)
          : ErlMsg.Tag.Undefined;

      return tag.ToString().ToUpper();
    }

    /// <summary>
    /// Read data from socket
    /// </summary>
    /// <remarks>
    /// This method now throws exception if we don't get full read
    /// </remarks>
    protected int ReadSock(byte[] b, int sz, bool readingPayload)
    {
      int got = 0;
      int len = sz;
      int i;
      Stream sock = null;

      lock (m_Transport)
          sock = m_Transport.GetStream();

      while (got < len && sock.CanRead)
      {
        i = sock.Read(b, got, len - got);

        if (i < 0)
          throw new ErlException(StringConsts.ERL_CONN_EOF_AFTER_N_BYTES_ERROR, got, len);
        else if (i == 0)
          throw new ErlException(StringConsts.ERL_CONN_REMOTE_CLOSED_ERROR);

        got += i;
      }

      m_ReceivedBytes += got;
      if (readingPayload)
      {
        m_ReceivedMsgs++;
        onReadWrite(Direction.Inbound, got + 4 /* header len */, m_ReceivedBytes, m_ReceivedMsgs);
      }
      return got;
    }

  #endregion

  #region .pvt

    private void setSockOpts()
    {
      m_Transport.NoDelay = RemoteNode.TcpNoDelay;
      if (RemoteNode.TcpRcvBufSize > 0) m_Transport.ReceiveBufferSize = RemoteNode.TcpRcvBufSize;
      if (RemoteNode.TcpSndBufSize > 0) m_Transport.SendBufferSize = RemoteNode.TcpSndBufSize;

      // Use keepalive timer
      m_Transport.SetSocketOption(
          System.Net.Sockets.SocketOptionLevel.Socket,
          System.Net.Sockets.SocketOptionName.KeepAlive, RemoteNode.TcpKeepAlive);
      // Close socket without waiting for it to deliver all data
      m_Transport.SetSocketOption(
          System.Net.Sockets.SocketOptionLevel.Socket,
          System.Net.Sockets.SocketOptionName.DontLinger, !RemoteNode.TcpLinger);

      //set SSH params
      RemoteNode.AppendSSHParamsToTransport(m_Transport);
    }

    private void onReadWrite(Direction op, int lastBytes, long totalBytes, long totalMsgs)
    {
      m_Home.OnReadWrite(this, op, lastBytes, totalBytes, totalMsgs);
    }

    private void sendBuf(ErlTuple ctrl, ErlOutputStream payload = null)
    {
      if (!m_Connected)
        throw new ErlException(StringConsts.ERL_CONN_NOT_CONNECTED_ERROR);

      var header = new ErlOutputStream(writeVersion: false, capacity: HEADER_LEN);

      // preamble: 4 byte length + "PASS_THROUGH" tag + version
      header.Write4BE(0); // reserve space for length
      header.Write1(PASS_THROUGH);
      header.Write1((byte)ErlExternalTag.Version);

      // header info
      header.WriteTuple(ctrl);

      // version for payload
      //header.Write1((byte)ErlExternalTag.Version); // Note that it's already written in the payload

      if (payload == null)
      {
        header.Poke4BE(0, (int)(header.Length - 4));
        DoSend(header);
      }
      else
      {
        header.Poke4BE(0, (int)(header.Length + payload.Length - 4));
        DoSend(header, payload);
      }
    }

    private void doAccept()
    {
      try
      {
        sendStatus("ok");
        int ourChallenge = genChallenge();
        sendChallenge(m_Peer.DistChoose, ErlAbstractNode.NodeCompatibility.Flags, ourChallenge);
        int herChallenge = recvChallengeReply(ourChallenge);
        byte[] ourDigest = genDigest(herChallenge, m_Cookie);
        sendChallengeAck(ourDigest);
        m_Connected = true;
        m_CookieOk = true;
        m_ShouldSendCookie = false;
      }
      catch (ErlException)
      {
        Close();
        throw;
      }
      catch (Exception e)
      {
        Close();
        throw new ErlException(StringConsts.ERL_CONN_ACCEPT_ERROR.Args(m_Peer.NodeName.Value), e);
      }

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Inbound, () => "MD5 ACCEPTED " + m_Peer.Host);
    }

    private void doConnect(int port)
    {
      try
      {
        m_Transport = ErlTransportFactory.Create(RemoteNode.TransportClassName, RemoteNode.NodeName.Value);

        m_Transport.Trace += (o, t, d, msg) => m_Home.OnTrace(t, d, msg);

        setSockOpts();

        m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Outbound, () =>
                "MD5 CONNECT TO {0}:{1}".Args(m_Peer.Host, port));

        //m_TcpClient.ReceiveTimeout = 5000;
        m_Transport.Connect(m_Peer.Host, port, ConnectTimeout);

        m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Outbound, () =>
                "MD5 CONNECTED TO {0}:{1}".Args(m_Peer.Host, port));

        sendName(m_Peer.DistChoose, ErlAbstractNode.NodeCompatibility.Flags);
        recvStatus();
        int herChallenge = recvChallenge();
        byte[] our_digest = genDigest(herChallenge, m_Cookie);
        int ourChallenge = genChallenge();
        sendChallengeReply(ourChallenge, our_digest);
        recvChallengeAck(ourChallenge);
        m_CookieOk = true;
        m_ShouldSendCookie = false;
      }
      catch (ErlAuthException ae)
      {
        Close();
        throw ae;
      }
      catch (Exception e)
      {
        Close();
        throw new ErlConnectionException(Name,
            StringConsts.ERL_CONN_CANT_CONNECT_TO_NODE_ERROR.Args(
                "{0}:{1}{2}: {3}".Args(m_Peer.Host, port,
                    m_Peer.NodeName == ErlAtom.Null ? "" : " [{0}]".Args(m_Peer.NodeName.Value),
                    e.Message)));
      }
    }

    // FIXME: This is no good as a challenge
    private static int genChallenge()
    {
      return s_Random.Next();
    }


    // Used to debug print a message digest
    private static string hex0(byte x)
    {
      return x.ToString("x2");
    }

    private static string hex(byte[] b)
    {
      return string.Concat(b.Select(v => v.ToString("x2")).ToArray());
    }

    private byte[] genDigest(int challenge, ErlAtom cookie)
    {
      long ch2 = challenge < 0
          ? 1L << 31 | (long)(challenge & 0x7FFFFFFFL)
          : (long)challenge;

      return new MD5CryptoServiceProvider().ComputeHash(
          Encoding.UTF8.GetBytes(cookie.Value + Convert.ToString(ch2)));
    }

    private void sendName(int dist, ErlAbstractNode.NodeCompatibility flags)
    {
      string str = m_Home.NodeName.Value;
      var obuf = new ErlOutputStream(writeVersion: false, writePktSize: false, capacity: 7 + str.Length);
      obuf.Write2BE(str.Length + 7); // 7 bytes + nodename
      obuf.Write1((int)ErlAbstractNode.NodeType.Ntype_R6);
      obuf.Write2BE((short)dist);
      obuf.Write4BE((int)flags);
      obuf.Write(Encoding.ASCII.GetBytes(str));

      obuf.WriteTo(m_Transport.GetStream());

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Outbound, () =>
          "sendName(flags({0:X2})={1}, dist={2}, local={3}".Args(
              (int)flags, flags, dist, m_Home.AliveName));
    }

    private void sendChallenge(int dist, ErlAbstractNode.NodeCompatibility flags, int challenge)
    {
      ErlAtom str = m_Home.NodeName;
      var obuf = new ErlOutputStream(writeVersion: false, writePktSize: false, capacity: 11 + str.Length);
      obuf.Write2BE((short)str.Length + 11); // 11 bytes + nodename
      obuf.Write1((byte)ErlAbstractNode.NodeType.Ntype_R6);
      obuf.Write2BE((short)dist);
      obuf.Write4BE((int)flags);
      obuf.Write4BE(challenge);
      obuf.Write(Encoding.ASCII.GetBytes(str.Value));

      obuf.WriteTo(m_Transport.GetStream());

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Outbound, () =>
          "sendChallenge(flags({0:X2})={1}, dist={2}, challenge={3}, local={4}".Args(
              (int)flags, flags, dist, challenge, m_Home.AliveName));
    }

    private ErlInputStream getInputStream()
    {
      ReadSock(m_Buf2, m_Buf2.Length, false);
      int idx = 0;
      int len = m_Buf2.ReadBEShort(ref idx);
      var tmpbuf = len <= m_BufN.Length ? m_BufN : new byte[len];
      ReadSock(tmpbuf, len, true);
      return new ErlInputStream(tmpbuf, 0, len, checkVersion: false);
    }


    private void recvName(ErlRemoteNode peer)
    {
      string hisname = string.Empty;
      ErlInputStream ibuf;

      try
      {
        ibuf = getInputStream();
      }
      catch
      {
        throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_DATA_ERROR);
      }

      int len = (int)ibuf.Length;
      m_Peer.Ntype = (ErlAbstractNode.NodeType)ibuf.Read1();
      if (m_Peer.Ntype != ErlAbstractNode.NodeType.Ntype_R6)
        throw new ErlConnectionException(Name, StringConsts.ERL_CONN_UNKNOWN_REMOTE_NODE_ERROR);
      m_Peer.DistLow = m_Peer.DistHigh = ibuf.Read2BE();
      if (m_Peer.DistLow < ErlAbstractNode.DIST_LOW)
        throw new ErlConnectionException(Name, StringConsts.ERL_CONN_UNKNOWN_REMOTE_NODE_ERROR);

      m_Peer.Flags = (ErlAbstractNode.NodeCompatibility)ibuf.Read4BE();
      hisname = ibuf.ReadBytesAsString(len - 7);
      // Set the old nodetype parameter to indicate hidden/normal status
      // When the old handshake is removed, the ntype should also be.
      m_Peer.Ntype = (m_Peer.Flags & ErlAbstractNode.NodeCompatibility.Published) != 0
          ? ErlAbstractNode.NodeType.Ntype_R4_Erlang
          : ErlAbstractNode.NodeType.Ntype_R4_Hidden;

      if ((m_Peer.Flags & ErlAbstractNode.NodeCompatibility.ExtendedReferences) == 0)
        throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_REF_ERROR);

      if (ErlApp.UseExtendedPidsPorts && (m_Peer.Flags & ErlAbstractNode.NodeCompatibility.ExtendedPidsPorts) == 0)
        throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_EXT_PIDS_ERROR);

      m_Peer.SetNodeName(hisname);

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Inbound, () =>
          "ntype({0})={1}, dist={2}, remote={3}".Args(
              (int)m_Peer.Ntype, m_Peer.Ntype, (int)m_Peer.DistHigh, m_Peer));
    }

    private int recvChallenge()
    {
      int challenge;
      ErlInputStream ibuf;

      try { ibuf = getInputStream(); }
      catch { throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_DATA_ERROR); }

      m_Peer.Ntype = (ErlAbstractNode.NodeType)ibuf.Read1();
      if (m_Peer.Ntype != ErlAbstractNode.NodeType.Ntype_R6)
        throw new System.IO.IOException("Unexpected peer type");

      m_Peer.DistLow = m_Peer.DistHigh = ibuf.Read2BE();
      m_Peer.Flags = (ErlAbstractNode.NodeCompatibility)ibuf.Read4BE();
      challenge = ibuf.Read4BE();
      var hisname = ibuf.ReadBytesAsString((int)ibuf.Length - 11);
      m_Peer.SetNodeName(hisname);

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Inbound, () =>
          "recvChallenge from={0} challenge={1} local={2}".Args(
              m_Peer.NodeName.Value, challenge, m_Home.NodeName.Value));

      return challenge;
    }

    private void sendChallengeReply(int challenge, byte[] digest)
    {
      var obuf = new ErlOutputStream(writeVersion: false, writePktSize: false, capacity: 23);
      obuf.Write2BE(21);
      obuf.Write1(CHALLENGE_REPLY);
      obuf.Write4BE(challenge);
      obuf.Write(digest);
      obuf.WriteTo(m_Transport.GetStream());

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Outbound, () =>
              "sendChallengeReply(challenge={0}, digest={1}, local={2}".Args(
                  challenge, hex(digest), m_Home.NodeName.Value));
    }

    private int recvChallengeReply(int our_challenge)
    {
      int challenge;
      byte[] her_digest = new byte[16];

      try
      {
        var ibuf = getInputStream();
        int tag = ibuf.Read1();
        if (tag != CHALLENGE_REPLY)
          throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_DATA_ERROR);
        challenge = ibuf.Read4BE();
        ibuf.ReadN(her_digest);
        byte[] our_digest = genDigest(our_challenge, m_Cookie);
        if (!her_digest.SequenceEqual(our_digest))
          throw new ErlAuthException(Name, StringConsts.ERL_CONN_PEER_AUTH_ERROR);
      }
      catch (ErlConnectionException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new ErlConnectionException(
            Name, StringConsts.ERL_CONN_HANDSHAKE_FAILED_ERROR.Args(e.Message));
      }

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Inbound, () =>
          "recvChallengeReply(from={0}, challenge={1}, digest={2}, local={3}".Args(
              m_Peer.NodeName.Value, challenge, hex(her_digest), m_Home.NodeName.Value));

      return challenge;
    }

    private void sendChallengeAck(byte[] digest)
    {
      var obuf = new ErlOutputStream(writeVersion: false, writePktSize: false, capacity: 19);
      obuf.Write2BE(17);
      obuf.Write1(CHALLENGE_ACK);
      obuf.Write(digest);

      obuf.WriteTo(m_Transport.GetStream());

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Outbound, () =>
          "sendChallengeAck(digest={1}, local={1}".Args(
              hex(digest), m_Home.NodeName.Value));
    }

    private void recvChallengeAck(int our_challenge)
    {
      byte[] her_digest = new byte[16];

      try
      {
        var ibuf = getInputStream();
        int tag = ibuf.Read1();
        if (tag != CHALLENGE_ACK)
          throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_DATA_ERROR);
        ibuf.ReadN(her_digest);
        byte[] our_digest = genDigest(our_challenge, m_Cookie);
        if (!her_digest.SequenceEqual(our_digest))
          throw new ErlAuthException(Name, StringConsts.ERL_CONN_PEER_AUTH_ERROR);
      }
      catch (ErlConnectionException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new ErlConnectionException(
            Name, StringConsts.ERL_CONN_HANDSHAKE_FAILED_ERROR.Args(e.Message));
      }

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Inbound, () =>
          "recvChallengeAck(from={0}, digest={1}, local={2}".Args(
              m_Peer.NodeName.Value, hex(her_digest), m_Home.NodeName.Value));
    }

    private void sendStatus(string status)
    {
      var obuf = new ErlOutputStream(writeVersion: false, writePktSize: false, capacity: 19);
      obuf.Write2BE(status.Length + 1);
      obuf.Write1(CHALLENGE_STATUS);
      obuf.Write(Encoding.ASCII.GetBytes(status));

      obuf.WriteTo(m_Transport.GetStream());

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Outbound, () =>
          "sendStatus(status={0}, local={1}".Args(status, m_Home.NodeName.Value));
    }

    private void recvStatus()
    {
      string status;

      try
      {
        var ibuf = getInputStream();
        int tag = ibuf.Read1();
        if (tag != CHALLENGE_STATUS)
          throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_DATA_ERROR);
        status = ibuf.ReadBytesAsString((int)ibuf.Length - 1);
      }
      catch (ErlConnectionException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new ErlConnectionException(Name, StringConsts.ERL_CONN_HANDSHAKE_FAILED_ERROR.Args(e.Message));
      }

      if (!string.Equals(status, "ok", StringComparison.InvariantCulture))
        throw new ErlConnectionException(Name, StringConsts.ERL_CONN_WRONG_STATUS_ERROR.Args(status));

      m_Home.OnTrace(ErlTraceLevel.Handshake, Direction.Inbound, () =>
          "sendStatus(status={0}, local={1}".Args(status, m_Home.NodeName.Value));
    }

  #endregion
  }
}
