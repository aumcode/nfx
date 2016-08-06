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
using System.IO;
using System.Net;
using System.Net.Sockets;

using NFX.Environment;
using NFX.ServiceModel;

namespace NFX.Erlang
{


  /// <summary>
  /// Represents an OTP node
  /// </summary>
  /// <remarks>
  /// About nodenames: Erlang nodenames consist of two components, an
  /// alivename and a hostname separated by '@'. Additionally, there are
  /// two nodename formats: short and long. Short names are of the form
  /// "alive@hostname", while long names are of the form
  /// "alive@host.fully.qualified.domainname". Erlang has special
  /// requirements regarding the use of the short and long formats, in
  /// particular they cannot be mixed freely in a network of
  /// communicating nodes, however Jinterface makes no distinction. See
  /// the Erlang documentation for more information about nodenames.
  ///
  /// The constructors for the AbstractNode classes will create names
  /// exactly as you provide them as long as the name contains '@'. If
  /// the string you provide contains no '@', it will be treated as an
  /// alivename and the name of the local host will be appended,
  /// resulting in a shortname. Nodenames longer than 255 characters will
  /// be truncated without warning.
  ///
  /// Upon initialization, this class attempts to read the file
  /// .erlang.cookie in the user's home directory, and uses the trimmed
  /// first line of the file as the default cookie by those constructors
  /// lacking a cookie argument. If for any reason the file cannot be
  /// found or read, the default cookie will be set to the empty string
  /// (""). The location of a user's home directory is determined using
  /// the system environment "HOME", which may not be automatically set
  /// on all platforms
  /// </remarks>
  public abstract class ErlAbstractNode : Service
  {
  #region CONSTS / Enums

    // Node types
    internal enum NodeType
    {
      Ntype_R6 = 110,   // 'n' post-r5, all nodes
      Ntype_R4_Erlang = 109,   // 'm' Only for source compatibility
      Ntype_R4_Hidden = 104,   // 'h' Only for source compatibility
    }

    // Node capability flags
    [Flags]
    internal enum NodeCompatibility
    {
      Published           = 1,
      AtomCache           = 2,
      ExtendedReferences  = 4,
      DistMonitor         = 8,
      FunTags             = 16,
      ExtendedPidsPorts   = 256, // pshaffer
      BitBinaries         = 1024,
      NewFloats           = 2048,
      Flags               = ExtendedReferences | ExtendedPidsPorts
                          | BitBinaries | NewFloats | DistMonitor,  // pshaffer
    }

    internal const NodeType NTYPE  = NodeType.Ntype_R6;   // pshaffer
    internal const int DIST_HIGH   = 5; // Cannot talk to nodes before R6
    internal const int DIST_LOW    = 5; // Cannot talk to nodes before R6

  #endregion

  #region Static

    internal static readonly string LocalHost;
    internal static ErlAtom s_DefaultCookie = ErlAtom.Null;
    internal static bool s_UseShortNames = false;

    static ErlAbstractNode()
    {
      try   { LocalHost = System.Net.Dns.GetHostName(); }
      catch { LocalHost = "localhost"; }

      s_DefaultCookie = tryReadDefaultCookieFromFile();
    }

    private static ErlAtom tryReadDefaultCookieFromFile()
    {
      var filename = System.Environment.GetEnvironmentVariable("HOME")
                      + System.IO.Path.DirectorySeparatorChar
                      + ".erlang.cookie";
      var s = tryReadStringFromFile(filename);
      return s.IsNullOrEmpty() ? ErlAtom.Null : new ErlAtom(s);
    }

    protected static string tryReadStringFromFile(string filename)
    {
      StreamReader br = null;
      try
      {
        var dotCookieFile = new FileInfo(filename);
        br = new StreamReader(new StreamReader(dotCookieFile.FullName).BaseStream);
        return br.ReadLine().Trim();
      }
      catch
      {
        return null;
      }
      finally
      {
        if (br != null)
          br.Close();
      }
    }

  #endregion

  #region .ctor

    /// <summary>
    /// Create a node with the given name and the default cookie
    /// </summary>
    protected internal ErlAbstractNode(string node, bool shortName)
        : this(node, ErlAtom.Null, shortName)
    { }

    /// <summary>
    /// Create a node with the given name and cookie
    /// </summary>
    /// <param name="name">Node name in the form "name" or "name@hostname"</param>
    /// <param name="cookie">Security cookie used to connect to this/other node(s)</param>
    /// <param name="shortName">Use short/long host names</param>
    protected ErlAbstractNode(string name, ErlAtom cookie, bool shortName)
        : base(null)
    {
      m_Cookie = new ErlAtom(cookie.Empty ? s_DefaultCookie : cookie);
      m_UseShortName = shortName;

      SetNodeName(name);
    }

    /// <summary>
    /// This constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="config"></param>
    protected ErlAbstractNode(string name, IConfigSectionNode config)
        : base(null)
    {
      m_Cookie = s_DefaultCookie;
      ConfigAttribute.Apply(this, config);
    }

  #endregion

  #region Fields

    private ErlAtom     m_NodeName;
    private string      m_Host;
    private string      m_Alive;
    private ErlAtom     m_Cookie;
    private ErlAtom     m_LongName;
    private bool        m_UseShortName;

    protected int       m_Creation  = 0;
    private int         m_Proto     = 0; // tcp/ip
    private NodeType    m_Ntype    = NTYPE;
    private int         m_DistHigh  = DIST_HIGH;
    private int         m_DistLow   = DIST_LOW;

    [Config("$tcp-no-delay")]
    protected bool      m_TcpNoDelay    = true;
    [Config("$tcp-rcv-buf-size")]
    protected int       m_TcpRcvBufSize = 0;
    [Config("$tcp-snd-buf-size")]
    protected int       m_TcpSndBufSize = 0;
    [Config("$tcp-keep-alive")]
    protected bool      m_TcpKeepAlive  = true;
    [Config("$tcp-linger")]
    protected bool      m_TcpLinger     = false;

  #endregion

  #region Public

    /// <summary>
    /// Get the name of this node (short or long depending on UseShortName)
    /// </summary>
    public ErlAtom NodeName { get { return m_UseShortName ? m_NodeName : m_LongName; } }

    /// <summary>
    /// Get the long name of this node
    /// </summary>
    public ErlAtom NodeLongName { get { return m_LongName; } }

    /// <summary>
    /// Get the hostname part of the nodename. Nodenames are composed of
    /// two parts, an alivename and a hostname, separated by '@'. This
    /// method returns the part of the nodename following the '@'
    /// </summary>
    public string Host { get { return m_Host; } }

    /// <summary>
    /// Get the alivename part of the hostname. Nodenames are composed of
    /// two parts, an alivename and a hostname, separated by '@'. This
    /// method returns the part of the nodename preceding the '@'
    /// </summary>
    public string AliveName { get { return m_Alive; } }

    /// <summary>
    /// Get the authorization cookie used by this node
    /// </summary>
    [Config("$cookie")]
    public ErlAtom Cookie { get { return m_Cookie; } set { m_Cookie = value; } }

    /// <summary>
    /// Get/Set the Epmd socket after publishing this nodes listen port to Epmd
    /// </summary>
    internal IErlTransport Epmd { get; set; }

    /// <summary>
    /// Get the port number used by this node.
    /// For local node the value may be 0 if the node was not registered with
    /// the EPMD port mapper
    /// </summary>
    public int Port { get; internal set; }

    public override string ToString() { return NodeName; }

    [Config("../$" + ErlConsts.ERLANG_SHORT_NAME_ATTR)]
    internal bool UseShortName
    {
      get { return m_UseShortName; }
      set { m_UseShortName = value; }
    }

    internal NodeCompatibility Flags{ get; set; }
    internal NodeType   Ntype       { get { return m_Ntype      ; } set { m_Ntype    = value; } }
    internal int        Proto       { get { return m_Proto      ; } set { m_Proto    = value; } }
    internal int        DistHigh    { get { return m_DistHigh   ; } set { m_DistHigh = value; } }
    internal int        DistLow     { get { return m_DistLow    ; } set { m_DistLow  = value; } }
    internal int        DistChoose  { get; set; }

    public bool TcpNoDelay      { get { return m_TcpNoDelay;    } set { m_TcpNoDelay    = value; } }
    public int  TcpSndBufSize   { get { return m_TcpSndBufSize; } set { m_TcpSndBufSize = value; } }
    public int  TcpRcvBufSize   { get { return m_TcpRcvBufSize; } set { m_TcpRcvBufSize = value; } }
    public bool TcpKeepAlive    { get { return m_TcpKeepAlive;  } set { m_TcpKeepAlive  = value; } }
    public bool TcpLinger       { get { return m_TcpLinger;     } set { m_TcpLinger     = value; } }

  #endregion

  #region Protected / Internal

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      ConfigAttribute.Apply(this, node);
    }

    internal void SetNodeName(string name)
    {
      splitNodeName(name, UseShortName, out m_Alive, out m_Host, out m_NodeName, out m_LongName);
    }

    internal static ErlAtom NodeShortName(string nodeName, bool shortName)
    {
      string alive, host;
      ErlAtom name, longName;
      splitNodeName(nodeName, shortName, out alive, out host, out name, out longName);
      return name;
    }

  #endregion

  #region .pvt

    internal static void splitNodeName(string name, bool shortName,
        out string alive, out string host, out ErlAtom nodeName, out ErlAtom longName)
    {
      int i = name.IndexOf('@');
      if (i < 0)
      {
        alive = name;
        host = LocalHost;
      }
      else
      {
        alive = name.Substring(0, i);
        host = name.Substring(i + 1);
      }

      if (alive.Length > AtomTable.MAX_ATOM_LEN)
        alive = alive.Substring(0, AtomTable.MAX_ATOM_LEN);

      longName = new ErlAtom(alive + "@" + host);
      nodeName = node(longName, shortName);
    }

    private static ErlAtom node(string name, bool shortName)
    {
      if (!shortName)
        return new ErlAtom(name);

      int i = name.IndexOf('@');
      i = i < 0 ? 0 : i + 1;

      IPAddress ip;
      if (IPAddress.TryParse(name.Substring(i), out ip))
        return new ErlAtom(name);

      int j = name.IndexOf('.', i);
      return new ErlAtom(j < 0 ? name : name.Substring(0, i + j - 2));
    }

  #endregion
  }
}
