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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NFX.Environment;

namespace NFX.Erlang
{
  /// <summary>
  /// General interface of TCP transports (i.e. usual TCP channel or SSH tunneled TCP channel)
  /// </summary>
  public interface IErlTransport : IDisposable
  {
    /// <summary>
    /// Set receive buffer size
    /// </summary>
    int ReceiveBufferSize { get; set; }

    /// <summary>
    /// Set send buffer size
    /// </summary>
    int SendBufferSize { get; set; }

    /// <summary>
    /// Network stream
    /// </summary>
    Stream GetStream();

    /// <summary>
    /// Connects to remote host:port
    /// </summary>
    void Connect(string host, int port);

    /// <summary>
    /// Connects to remote host:port with a timeout in milliseconds
    /// </summary>
    void Connect(string host, int port, int timeoutMsec);

    /// <summary>
    /// Close connection
    /// </summary>
    void Close();

    /// <summary>
    /// Remote endpoint
    /// </summary>
    EndPoint RemoteEndPoint { get; }

    /// <summary>
    /// Sets cocket options
    /// </summary>
    void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName,
                         bool optionValue);

    /// <summary>
    /// NoDelay socket
    /// </summary>
    bool NoDelay { get; set; }

    /// <summary>
    /// Trace event
    /// </summary>
    event TraceEventHandler Trace;

    /// <summary>
    /// Erlang node name
    /// </summary>
    string NodeName { get; set; }

    /// <summary>
    /// ConnectTimeout in milliseconds
    /// </summary>
    int ConnectTimeout { get; set; }

    /// <summary>
    /// Port of SSH server
    /// </summary>
    int SSHServerPort { get; set; }

    /// <summary>
    /// SSH user name
    /// </summary>
    string SSHUserName { get; set; }

    /// <summary>
    /// Private key file path (only for AuthenticationType = PublicKey)
    /// Required SSH2 ENCRYPTED PRIVATE KEY format.
    /// </summary>
    string SSHPrivateKeyFilePath { get; set; }

    /// <summary>
    /// Type of auth on SSH server.
    /// Valid values: "PublicKey", "Password".
    /// </summary>
    string SSHAuthenticationType { get; set; }
  }
}