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
using System.Net.Sockets;
using System.Threading;

namespace NFX.Erlang
{
  /// <summary>
  /// Maintains a connection between a C# process and a remote
  /// Erlang, C# or C node. The object maintains connection state and
  /// allows data to be sent to and received from the peer
  /// </summary>
  /// <remarks>
  /// In current implementation each connection creates a thread
  /// </remarks>
  public class ErlConnection : ErlAbstractConnection
  {
  #region .ctor

    private void _ctor(string threadName)
    {
      m_Thread = new Thread(threadSpin);
      m_Thread.IsBackground = true;
      m_Thread.Name = threadName;
      m_Thread.Start();
    }

    /// <summary>
    /// Accept an incoming connection from a remote node
    /// </summary>
    public ErlConnection(ErlLocalNode node, TcpClient peer)
        : base(node, new ErlTcpTransport(peer))
    {
      _ctor(StringConsts.ERL_CONNECTION.Args(m_Home.NodeName.Value, "<-", peer.ToString()));
    }

    /// <summary>
    /// Intiate and open a connection to a remote node
    /// </summary>
    internal ErlConnection(ErlLocalNode node, ErlRemoteNode peer, bool connect = true)
        : base(node, peer, connect: connect)
    {
      _ctor(StringConsts.ERL_CONNECTION.Args(m_Home.NodeName.Value, "->", peer.NodeName.Value));
    }

  #endregion

  #region Fields

    private Thread m_Thread;
    protected bool m_Done = false;

  #endregion

  #region Public

    /// <summary>
    /// Send an Erlang term to a Pid on a local or remote node
    /// </summary>
    public void Send(ErlPid dest, IErlObject msg)
    {
      base.Send(ErlMsg.Send(dest, msg, SendCookie));
    }

    /*
    * send to remote name
    * dest is recipient's registered name, the nodename is implied by
    * the choice of connection.
    */
    public void Send(ErlPid from, ErlAtom dest, IErlObject msg)
    {
      // encode and send the message
      base.Send(ErlMsg.RegSend(from, dest, msg, SendCookie));
    }

  #endregion

  #region Protected

    protected override void Close()
    {
      m_Done = true;

      base.Close();

      m_Home.BreakLinks(Name, ConstAtoms.NoConnection);

      var t = m_Thread;
      if (t == null) return;
      m_Thread = null;

      t.Join();
    }

    /// <summary>
    /// Deliver communication exceptions to the recipient
    /// </summary>
    protected override void Deliver(ErlConnectionException e)
    {
      m_Home.Deliver(e);
    }

    /// <summary>
    /// Deliver messages to the recipient
    /// </summary>
    protected override void Deliver(ErlMsg msg)
    {
      if (!m_Home.Deliver(msg))
        m_Home.OnUnhandledMsg(this, msg);
    }

  #endregion

  #region .pvt

    private void threadSpin()
    {
      try
      {
        threadSpinCore();
      }
      catch(Exception error)
      {
        var em = new NFX.Log.Message
        {
          Type = Log.MessageType.CatastrophicError,
          Topic = CoreConsts.ERLANG_TOPIC,
          From = GetType().Name + "threadSpin()",
          Text = "threadSpinCore leaked: " + error.ToMessageWithType(),
          Exception = error
        };
        App.Log.Write(em);
      }
    }

    private void threadSpinCore()
    {
      if (!m_Connected)
      {
        Deliver(new ErlConnectionException(Name, StringConsts.ERL_CONN_NOT_CONNECTED_ERROR));
        return;
      }

      int len;
      byte[] header = new byte[4];
      byte[] tock = new byte[] { 0, 0, 0, 0 };
      byte[] payloadBuf = new byte[1024 * 1024];

      try
      {
        while (m_Home.Running && !this.DisposeStarted && !m_Done)
        {
          // don't return until we get a real message
          // or a failure of some kind (e.g. EXIT)
          // read length and read buffer must be atomic!
          do
          {
            // read 4 bytes - get length of incoming packet
            // socket.getInputStream().read(lbuf);
            int n;
            if ((n = ReadSock(header, header.Length, false)) < header.Length)
              throw new ErlException(StringConsts.ERL_CONN_READ_TOO_SHORT_ERROR, n, header.Length);

            len = header.ReadBEInt32();

            //  received tick? send tock!
            if (len == 0)
              lock (this)
              {
                if (m_Transport != null)
                  (m_Transport.GetStream()).Write(tock, 0, tock.Length);
              }
          }
          while (len == 0); // tick_loop

          if (len > DEFAULT_MAX_PAYLOAD_LENGTH)
            throw new ErlException(
                StringConsts.ERL_CONN_MSG_SIZE_TOO_LONG_ERROR, DEFAULT_MAX_PAYLOAD_LENGTH, len);

          // got a real message (maybe) - read len bytes
          byte[] tmpbuf = new byte[len]; // len > payloadBuf.Length ? new byte[len] : payloadBuf;
                                         // i = socket.getInputStream().read(tmpbuf);
          int m = ReadSock(tmpbuf, len, true);
          if (m != len)
            throw new ErlException(StringConsts.ERL_CONN_READ_TOO_SHORT_ERROR, m, len);

          var ibuf = new ErlInputStream(tmpbuf, 0, len, checkVersion: false);

          if (ibuf.Read1() != PASS_THROUGH)
            goto receive_loop_brk;

          try
          {
            // decode the header
            var tmp = ibuf.Read(true);
            if (!(tmp is ErlTuple))
              goto receive_loop_brk;

            var head = (ErlTuple)tmp;
            if (!(head[0] is ErlByte))
              goto receive_loop_brk;

            // lets see what kind of message this is
            ErlMsg.Tag tag = (ErlMsg.Tag)head[0].ValueAsInt;

            switch (tag)
            {
              case ErlMsg.Tag.Send:
              case ErlMsg.Tag.SendTT:
                {
                  // { SEND, Cookie, ToPid, TraceToken }
                  if (!m_CookieOk)
                  {
                    // we only check this once, he can send us bad cookies later if he likes
                    if (!(head[1] is ErlAtom))
                      goto receive_loop_brk;
                    var cookie = (ErlAtom)head[1];
                    if (m_ShouldSendCookie)
                    {
                      if (!cookie.Equals(m_Cookie))
                        cookieError(m_Home, cookie);
                    }
                    else if (!cookie.Empty)
                      cookieError(m_Home, cookie);
                    m_CookieOk = true;
                  }

                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                  {
                    long mark = ibuf.Position;
                    var traceobj = ibuf.Read(true);
                    ibuf.Position = mark;
                    return "{0} {1} trace: {2}".Args(
                            HeaderType(head), head.ToString(),
                            traceobj == null ? "(null)" : traceobj.ToString());
                  });

                  var to = (ErlPid)head[2];

                  Deliver(new ErlMsg(tag, ErlPid.Null, to, paybuf: ibuf));
                  break;
                }
              case ErlMsg.Tag.RegSend:
              case ErlMsg.Tag.RegSendTT:
                {
                  // { REG_SEND, FromPid, Cookie, ToName, TraceToken }
                  if (!m_CookieOk)
                  {
                    // we only check this once, he can send us bad cookies later if he likes
                    if (!(head[2] is ErlAtom))
                      goto receive_loop_brk;
                    var cookie = (ErlAtom)head[2];
                    if (m_ShouldSendCookie)
                    {
                      if (!cookie.Equals(m_Cookie))
                        cookieError(m_Home, cookie);
                    }
                    else if (!cookie.Empty)
                      cookieError(m_Home, cookie);
                    m_CookieOk = true;
                  }

                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                  {
                    long mark = ibuf.Position;
                    var traceobj = ibuf.Read(true);
                    ibuf.Position = mark;
                    return "{0} {1} trace: {2}".Args(
                            HeaderType(head), head.ToString(),
                            traceobj == null ? "(null)" : traceobj.ToString());
                  });

                  var from = (ErlPid)head[1];
                  var toName = (ErlAtom)head[3];

                  Deliver(new ErlMsg(tag, from, toName, paybuf: ibuf));
                  break;
                }
              case ErlMsg.Tag.Exit:
              case ErlMsg.Tag.Exit2:
                {
                  // { EXIT2, FromPid, ToPid, Reason }
                  if (!(head[3] is ErlAtom))
                    goto receive_loop_brk;

                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                      "{0} {1}".Args(HeaderType(head), head.ToString()));

                  var from = (ErlPid)head[1];
                  var to = (ErlPid)head[2];
                  var reason = (ErlAtom)head[3];

                  Deliver(new ErlMsg(tag, from, to, reason: reason));
                  break;
                }
              case ErlMsg.Tag.ExitTT:
              case ErlMsg.Tag.Exit2TT:
                {
                  // { EXIT2, FromPid, ToPid, TraceToken, Reason }
                  // as above, but bifferent element number
                  if (!(head[4] is ErlAtom))
                    goto receive_loop_brk;

                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                          "{0} {1}".Args(HeaderType(head), head.ToString()));
                  // TODO: print TraceToken

                  var from = (ErlPid)head[1];
                  var to = (ErlPid)head[2];
                  var trace = (ErlTrace)head[3];
                  var reason = (ErlAtom)head[4];

                  Deliver(new ErlMsg(tag, from, to, reason: reason, trace: trace));
                  break;
                }
              case ErlMsg.Tag.Link:
              case ErlMsg.Tag.Unlink:
                {
                  // {UNLINK, FromPid, ToPid}
                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                      "{0} {1}".Args(HeaderType(head), head.ToString()));

                  var from = (ErlPid)head[1];
                  var to = (ErlPid)head[2];

                  Deliver(new ErlMsg(tag, from, to));
                  break;
                }
              case ErlMsg.Tag.GroupLeader:
              case ErlMsg.Tag.NodeLink:
                {
                  // No idea what to do with these, so we ignore them...
                  // {GROUP_LEADER, FromPid, ToPid},  { NODELINK }
                  // (just show trace)
                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                      "{0} {1}".Args(HeaderType(head), head.ToString()));
                  break;
                }
              case ErlMsg.Tag.MonitorP:
              case ErlMsg.Tag.DemonitorP:
                {
                  // {MONITOR_P, FromPid, ToProc, Ref}
                  // {DEMONITOR_P, FromPid, ToProc, Ref}
                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                      "{0} {1}".Args(HeaderType(head), head.ToString()));
                  var from = (ErlPid)head[1];
                  var to = (ErlPid)head[2];
                  var eref = (ErlRef)head[3];

                  Deliver(new ErlMsg(tag, from, to, eref));
                  break;
                }
              case ErlMsg.Tag.MonitorPexit:
                {
                  // {MONITOR_P_EXIT, FromPid, ToProc, Ref, Reason}
                  m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                      "{0} {1}".Args(HeaderType(head), head.ToString()));
                  var from = (ErlPid)head[1];
                  var to = (ErlPid)head[2];
                  var eref = (ErlRef)head[3];
                  var reason = head[4];

                  Deliver(ErlMsg.MonitorPexit(from, to, eref, reason));
                  break;
                }
              default:
                // garbage?
                m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
                    StringConsts.ERL_CONN_UNKNOWN_TAG_ERROR.Args(
                        HeaderType(head), head.ToString()));
                goto receive_loop_brk;
            }
          }
          catch (Exception e)
          {
            // we have received garbage
            m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, e.ToString());
            Deliver(new ErlBadDataException(
                Name, /* "Remote has closed connection or sending garbage: " + */
                e.Message));
          }
        }

      receive_loop_brk:
        // end receive_loop

        // this section reachable only with break
        // connection went down or we have received garbage from peer
        Deliver(new ErlConnectionException(Name, StringConsts.ERL_CONN_INVALID_DATA_FROM_PEER_ERROR));
      }
      catch (ErlAuthException e)
      {
        m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () => e.ToString());
        Deliver(e);
      }
      catch (Exception e)
      {
        m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () => e.ToString());
        Deliver(new ErlConnectionException(
            Name, /* "Remote has closed connection or sending garbage: " + */
            e.Message));
      }
      finally
      {
        m_Thread = null;
        Close();
        m_Home.OnTrace(ErlTraceLevel.Ctrl, Direction.Inbound, () =>
            "Exiting connection (thread {0})".Args(Thread.CurrentThread.Name));
      }
    }

    /// <summary>
    /// Send an auth error to peer because he sent a bad cookie
    /// The auth error uses his cookie (not revealing ours).
    /// This is just like send_reg otherwise
    /// </summary>
    private void cookieError(ErlLocalNode local, ErlAtom cookie)
    {
      var header = new ErlOutputStream(writeVersion: false, capacity: HEADER_LEN);

      // preamble: 4 byte length + "PASS_THROUGH" tag + version
      header.Write4BE(0); // reserve space for length
      header.Write1(PASS_THROUGH);
      header.Write1((byte)ErlExternalTag.Version);

      header.WriteTupleHead(4);
      header.WriteLong((long)ErlMsg.Tag.RegSend);
      header.WritePid(local.CreatePid()); // disposable pid
      header.WriteAtom(cookie); // important: his cookie, not mine...
      header.WriteAtom("auth");

      // version for payload written later by the payload stream
      //header.Write1((byte)ErlExternalTag.Version);

      // the payload

      // the no_auth message (copied from Erlang) Don't change this (Erlang will crash)
      // {$gen_cast, {print, "~n** Unauthorized cookie ~w **~n", [foo@aule]}}

      var msg = ErlObject.Parse(
          "{'$gen_cast', {print, \"\n** Unauthorized cookie ~w **\n\", [" + local.NodeName.Value + "]}}");

      var payload = new ErlOutputStream(msg, writeVersion: true);

      // fix up length in preamble
      header.Poke4BE(0, (int)(header.Length + payload.Length - 4));

      try
      {
        DoSend(header, payload);
      }
      catch (Exception e)
      {
        Close();
        throw new ErlException(StringConsts.ERL_CONN_UNAUTH_COOKIE_ERROR.Args(cookie.Value), e);
      }
    }

  #endregion
  }
}
