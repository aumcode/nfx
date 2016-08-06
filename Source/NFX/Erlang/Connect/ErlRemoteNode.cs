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

using NFX.Environment;
namespace NFX.Erlang
{
  public class ErlRemoteNode : ErlAbstractNode
  {
    #region CONSTS

    private const int DEFAULT_SSH_PORT = 22;
    private const int DEFAULT_CONNECT_TIMEOUT = 5000;//ms

    #endregion;

    #region Fields

    [Config("$transport-type")]
    string m_TransportType;
    [Config("$ssh-server-port")]
    int    m_SSHServerPort              = DEFAULT_SSH_PORT;
    [Config("$ssh-user-name")]
    string m_SSHUserName;
    [Config("$ssh-private-key-file")]
    string m_SSHPrivateKeyFilePath;
    [Config("$connect-timeout")]
    int    m_ConnectTimeout             = DEFAULT_CONNECT_TIMEOUT;
    [Config("$ssh-authentication-type")]
    string m_SSHAuthenticationType      = "Password";

    #endregion

    /// <summary>
    /// Constructor used for creating a remote node by the Acceptor of incoming connections
    /// </summary>
    /// <param name="home"></param>
    internal ErlRemoteNode(ErlLocalNode home) : this(home, ErlAtom.Null)
    { }

    /// <summary>
    /// Create a peer node
    /// </summary>
    public ErlRemoteNode(ErlLocalNode home, string toNode, ErlAtom? cookie = null)
        : base(toNode, cookie ?? home.Cookie, home.UseShortName)
    {
      ctor(home);
    }

    /// <summary>
    /// Create a peer node
    /// </summary>
    public ErlRemoteNode(ErlLocalNode home, ErlAtom toNode, ErlAtom? cookie = null)
        : base(toNode, cookie ?? home.Cookie, home.UseShortName)
    {
      ctor(home);
    }


    /// <summary>
    /// Create a peer node
    /// </summary>
    public ErlRemoteNode(ErlLocalNode home, ErlAtom toNode, IConfigSettings config, ErlAtom? cookie = null)
        : base(toNode, cookie ?? home.Cookie, home.UseShortName)
    {
        ctor(home);
    }

    /// <summary>
    /// Create a connection to a remote node
    /// </summary>
    /// <param name="self">the local node from which you wish to connect</param>
    /// <returns>A connection to the remote node</returns>
    public ErlConnection Connect(ErlLocalNode self)
    {
      return new ErlConnection(self, this);
    }

    private void ctor(ErlLocalNode home)
    {
      m_TcpKeepAlive  = home.TcpKeepAlive;
      m_TcpLinger     = home.TcpLinger;
      m_TcpNoDelay    = home.TcpNoDelay;
      m_TcpRcvBufSize = home.TcpRcvBufSize;
      m_TcpSndBufSize = home.TcpSndBufSize;
    }

    #region Public

    /// <summary>
    /// Full name of transport class (if not specified - it uses ErlTcpTransport)
    /// </summary>
    public string TransportClassName { get { return m_TransportType; } set { m_TransportType = value; } }
    /// <summary>
    /// Port of SSH server
    /// </summary>
    public int SSHServerPort { get { return m_SSHServerPort; } set { m_SSHServerPort = value; } }
    /// <summary>
    /// SSH user name
    /// </summary>
    public string SSHUserName { get { return m_SSHUserName; } set { m_SSHUserName = value; } }
    /// <summary>
    /// Private key file path (only for AuthenticationType = PublicKey)
    /// Required SSH2 ENCRYPTED PRIVATE KEY format.
    /// </summary>
    public string SSHPrivateKeyFilePath { get { return m_SSHPrivateKeyFilePath; } set { m_SSHPrivateKeyFilePath = value; } }
    /// <summary>
    /// Connect timeout, ms
    /// </summary>
    public int ConnectTimeout { get { return m_ConnectTimeout; } set { m_ConnectTimeout = value; } }
    /// <summary>
    /// Type of auth on SSH server
    /// </summary>
    public string SSHAuthenticationType { get { return m_SSHAuthenticationType; } set { m_SSHAuthenticationType = value; } }

    public void AppendSSHParamsToTransport(IErlTransport transport)
    {
        //set SSH params
        transport.SSHAuthenticationType = SSHAuthenticationType;
        transport.SSHPrivateKeyFilePath = SSHPrivateKeyFilePath;
        transport.SSHServerPort         = SSHServerPort;
        transport.ConnectTimeout        = ConnectTimeout;
        transport.SSHUserName           = SSHUserName;
    }

    #endregion
  }
}
