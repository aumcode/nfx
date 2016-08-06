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
using System.Text;
using System.Net.Sockets;

namespace NFX.Erlang
{
  /// <summary>
  /// Provides methods for registering, unregistering and looking up
  /// nodes with the Erlang portmapper daemon (Epmd). For each registered
  /// node, Epmd maintains information about the port on which incoming
  /// connections are accepted, as well as which versions of the Erlang
  /// communication protocolt the node supports
  /// </summary>
  /// <remarks>
  /// Nodes wishing to contact other nodes must first request
  /// information from Epmd before a connection can be set up, however
  /// this is done automatically by
  /// <see cref="ErlLocalNode.Connection(string, ErlAtom?)"/>
  /// when necessary.
  ///
  /// The methods <see cref="PublishPort(ErlLocalNode)"/> and
  /// <see cref="UnPublishPort"/> will fail if an
  /// Epmd process is not running on the localhost. Additionally
  /// <see cref="LookupPort"/> will fail if there is no Epmd
  /// process running on the host where the specified node is running.
  /// See the Erlang documentation for information about starting Epmd.
  ///
  /// This class contains only static methods, there are no
  /// constructors.
  /// </remarks>
  internal static class ErlEpmd
  {
  #region CONSTS

    public const int EPMD_PORT = 4369;

    private enum Indicator
    {
      StopReq         = 115,
      Port3req        = 112,
      Publish3req     = 97,
      Publish3ok      = 89,
      Port4req        = 122,
      Port4resp       = 119,
      Publish4req     = 120,
      Publish4resp    = 121,
    }

  #endregion

    /// <summary>
    /// Determine what port a node listens for incoming connections on
    /// </summary>
    /// <param name="home">Local node</param>
    /// <param name="node">Remote lode for which to look up the port number from remote EPMD</param>
    /// <param name="closeSocket">If true, close the connection to remote EPMD at return</param>
    /// <returns>the listen port for the specified node, or 0 if the node
    /// was not registered with Epmd</returns>
    public static int LookupPort(ErlLocalNode home, ErlRemoteNode node,
        bool closeSocket = false)
    {
      IErlTransport s = null;

      try
      {
        var obuf = new ErlOutputStream(writeVersion: false, capacity: 4 + 3 + node.AliveName.Length + 1);
        s = node.Epmd;

        if (s == null)
        {
          //create transport
          s = ErlTransportFactory.Create(node.TransportClassName, node.NodeName.Value);

          s.Trace += (o, t, d, msg) => home.OnTrace(t, d, msg);

          //append SSH params to transport
          node.AppendSSHParamsToTransport(s);

          //connect (I am not sure)
          s.Connect(node.Host, EPMD_PORT, node.ConnectTimeout);
        }

        // build and send epmd request
        // length[2], tag[1], alivename[n] (length = n+1)
        obuf.Write2BE(node.AliveName.Length + 1);
        obuf.Write1((byte)Indicator.Port4req);
        //UPGRADE_NOTE: This code will be optimized in the future;
        byte[] buf = Encoding.ASCII.GetBytes(node.AliveName);
        obuf.Write(buf);

        // send request
        obuf.WriteTo(s.GetStream());

        home.OnTrace(ErlTraceLevel.Epmd, Direction.Outbound,
                StringConsts.ERL_EPMD_LOOKUP_R4.Args(node.NodeName.Value));

        // receive and decode reply
        // resptag[1], result[1], port[2], ntype[1], proto[1],
        // disthigh[2], distlow[2], nlen[2], alivename[n],
        // elen[2], edata[m]
        buf = new byte[100];

        int n = s.GetStream().Read(buf, 0, buf.Length);

        if (n < 0)
          throw new ErlException(StringConsts.ERL_EPMD_NOT_RESPONDING.Args(node.Host, node.AliveName));

        var ibuf = new ErlInputStream(buf, 0, n, checkVersion: false);

        if (Indicator.Port4resp == (Indicator)ibuf.Read1())
        {
          if (0 == ibuf.Read1())
          {
            node.Port = ibuf.Read2BE();

            node.Ntype = (ErlAbstractNode.NodeType)ibuf.Read1();
            node.Proto = ibuf.Read1();
            node.DistHigh = ibuf.Read2BE();
            node.DistLow = ibuf.Read2BE();
            // ignore rest of fields
          }
        }

        if (!closeSocket)
        {
          node.Epmd = s;
          s = null;
        }
      }
      catch (Exception e)
      {
        home.OnTrace(ErlTraceLevel.Epmd, Direction.Inbound, StringConsts.ERL_EPMD_INVALID_RESPONSE_ERROR.Args(node.Host, node.AliveName, e.ToString()));
        throw new ErlException(StringConsts.ERL_EPMD_NOT_RESPONDING.Args(node.Host, node.AliveName, e.ToString()));
      }
      finally
      {
        if (s != null)
          try { s.Close(); } catch { }
        s = null;
      }

      home.OnTrace(ErlTraceLevel.Epmd, Direction.Inbound, () =>
          node.Port == 0 ? StringConsts.ERL_EPMD_NOT_FOUND : StringConsts.ERL_EPMD_PORT.Args(node.Port));

      return node.Port;
    }

    /// <summary>
    /// Publish node's port at local EPMD, so that other nodes can connect to it.
    ///
    /// On failure to connect to EPMD the function may throw if the value of
    /// ErlApp.IgnoreLocalEpmdConnectErrors variable is true.
    ///
    /// On failed connection attempt the function calls
    /// node.OnEpmdFailedConnectAttempt delegate
    /// </summary>
    /// <remarks>
    /// This function will get an exception if it tries to talk to an r3
    /// epmd, or if something else happens that it cannot forsee. In both
    /// cases we return an exception (and the caller should try again, using
    /// the r3 protocol).
    ///
    /// If we manage to successfully communicate with an r4 epmd, we return
    /// either the socket, or null, depending on the result
    /// </remarks>
    public static bool PublishPort(ErlLocalNode node)
    {
      Exception error = null;
      IErlTransport s = null;

      try
      {
        ErlOutputStream obuf = new ErlOutputStream(writeVersion: false, capacity: node.AliveName.Length + 20);
        s = node.Epmd ?? new ErlTcpTransport(ErlLocalNode.LocalHost, EPMD_PORT) { NodeName = node.NodeName.Value };

        obuf.Write2BE(node.AliveName.Length + 13);

        obuf.Write1((byte)Indicator.Publish4req);
        obuf.Write2BE(node.Port);

        obuf.Write1((byte)node.Ntype);

        obuf.Write1((byte)node.Proto);
        obuf.Write2BE(node.DistHigh);
        obuf.Write2BE(node.DistLow);

        obuf.Write2BE(node.AliveName.Length);
        var buf = Encoding.ASCII.GetBytes(node.AliveName);
        //UPGRADE_NOTE: This code will be optimized in the future;
        obuf.Write(buf);
        obuf.Write2BE(0); // No extra

        // send request
        obuf.WriteTo(s.GetStream());

        node.OnTrace(ErlTraceLevel.Epmd, Direction.Outbound, () =>
                StringConsts.ERL_EPMD_PUBLISH.Args(node.AliveName, node.Port, "r4"));

        // get reply
        buf = new byte[100];
        int n = s.GetStream().Read(buf, 0, buf.Length);

        if (n < 0)
          // this was an r3 node => not a failure (yet)
          throw new ErlException(StringConsts.ERL_EPMD_NOT_RESPONDING.Args(node.Host, node.AliveName));

        ErlInputStream ibuf = new ErlInputStream(buf, 0, n, checkVersion: false);

        if (Indicator.Publish4resp == (Indicator)ibuf.Read1())
        {
          int result = ibuf.Read1();
          if (result == 0)
          {
            node.Creation = (byte)ibuf.Read2BE();
            node.OnTrace(ErlTraceLevel.Epmd, Direction.Inbound, StringConsts.ERL_EPMD_OK);

            node.Epmd = s;
            return true; // success
          }
        }
        error = new ErlException("Cannot register node '{0}' (not unique?)".Args(node.AliveName));
      }
      catch (Exception e)
      {
        error = e;
      }

      // epmd closed the connection = fail
      if (s != null)
        try { s.Close(); } catch { }

      node.Epmd = null;

      node.OnTrace(ErlTraceLevel.Epmd, Direction.Inbound, StringConsts.ERL_EPMD_NO_RESPONSE);


      node.OnEpmdFailedConnectAttempt(node.NodeName, error.Message);

      node.OnTrace(ErlTraceLevel.Epmd, Direction.Inbound, StringConsts.ERL_EPMD_FAILED_TO_CONNECT_ERROR);

      if (!ErlApp.IgnoreLocalEpmdConnectErrors)
        throw new ErlException(StringConsts.ERL_EPMD_FAILED_TO_CONNECT_ERROR + ": " + error.Message);

      node.Creation = 0;
      return false;
    }

    /// <summary>
    /// Unregister from Epmd.
    /// Other nodes wishing to connect will no longer be able to
    /// </summary>
    public static void UnPublishPort(ErlLocalNode node)
    {
      IErlTransport s = null;

      try
      {
        s = node.Epmd ?? new ErlTcpTransport(ErlLocalNode.LocalHost, EPMD_PORT) { NodeName = node.NodeName.Value };
        ErlOutputStream obuf = new ErlOutputStream(
            writeVersion: false, capacity: node.AliveName.Length + 8);
        obuf.Write2BE(node.AliveName.Length + 1);
        obuf.Write1((byte)Indicator.StopReq);
        var buf = Encoding.ASCII.GetBytes(node.AliveName);
        obuf.Write(buf);
        obuf.WriteTo(s.GetStream());

        node.OnTrace(ErlTraceLevel.Epmd, Direction.Outbound, () =>
                StringConsts.ERL_EPMD_UNPUBLISH.Args(node.NodeName.Value, node.Port));
      }
      catch
      {
        s = null;
      }
      finally
      {
        if (s != null)
          try { s.Close(); } catch { }
        node.Epmd = null;
      }
    }
  }
}
