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
  public class ErlRemoteNode : ErlAbstractNode
  {
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
        : base(toNode, cookie.HasValue ? cookie.Value : home.Cookie, home.UseShortName)
    {
      ctor(home);
    }

    /// <summary>
    /// Create a peer node
    /// </summary>
    public ErlRemoteNode(ErlLocalNode home, ErlAtom toNode, ErlAtom? cookie = null)
        : base(toNode, cookie.HasValue ? cookie.Value : home.Cookie, home.UseShortName)
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
      m_TcpKeepAlive = home.TcpKeepAlive;
      m_TcpLinger = home.TcpLinger;
      m_TcpNoDelay = home.TcpNoDelay;
      m_TcpRcvBufSize = home.TcpRcvBufSize;
      m_TcpSndBufSize = home.TcpSndBufSize;
    }
  }
}
