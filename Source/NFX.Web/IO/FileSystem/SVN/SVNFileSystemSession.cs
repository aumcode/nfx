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
using System.Collections.Generic;

using NFX.Environment;
using NFX.Security;

using WSC = NFX.Web.StringConsts;

namespace NFX.IO.FileSystem.SVN
{
  /// <summary>
  /// Provides connection parameters specific to SVNFIleSystemSession
  /// </summary>
  public class SVNFileSystemSessionConnectParams : FileSystemSessionConnectParams
  {
    public const string CONFIG_SERVERURL_ATTR = "server-url";
    public const string CONFIG_UNAME_ATTR = "user-name";
    public const string CONFIG_UPWD_ATTR  = "user-password";
    public const string CONFIG_TIMEOUT_MS_ATTR = "timeout-ms";

    public SVNFileSystemSessionConnectParams(): base() {}
    public SVNFileSystemSessionConnectParams(IConfigSectionNode node): base(node) {}
    public SVNFileSystemSessionConnectParams(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) {}

    private int m_TimeoutMs;

    /// <summary>
    /// SVN Server URL i.e. 'http://assembla.com/svn/myRepository'
    /// </summary>
    [Config]public string ServerURL{ get; set;}

    /// <summary>
    /// Request timeout milliseconds
    /// </summary>
    [Config]public int TimeoutMs
    {
      get { return m_TimeoutMs; }
      set { m_TimeoutMs = value < 0 ? 0 : value; }
    }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var unm = node.AttrByName(CONFIG_UNAME_ATTR).Value;
      var upwd = node.AttrByName(CONFIG_UPWD_ATTR).Value;

      if (unm.IsNotNullOrWhiteSpace())
      {
        var cred = new IDPasswordCredentials(unm, upwd);
        var at = new AuthenticationToken(ServerURL, unm);
        User = new User(cred, at, UserStatus.User, unm, unm, Rights.None);
      }
    }
  }

  /// <summary>
  /// Provides SVN file system connection session
  /// </summary>
  public sealed class SVNFileSystemSession : FileSystemSession
  {
    internal SVNFileSystemSession(SVNFileSystem fs, IFileSystemHandle handle, SVNFileSystemSessionConnectParams cParams)
                      : base(fs, handle, cParams)
    {
      m_ServerURL = cParams.ServerURL;

      var cred = cParams.User.Credentials as IDPasswordCredentials;

      if (cred == null)
	      m_WebDAV = new WebDAV(cParams.ServerURL, cParams.TimeoutMs);
      else
        m_WebDAV = new WebDAV(cParams.ServerURL, cParams.TimeoutMs, cred.ID, cred.Password);
    }

    private string m_ServerURL;
    private WebDAV m_WebDAV;

    /// <summary>
    /// SVN Server URL i.e. 'http://assembla.com/svn/myRepository'
    /// </summary>
    public string ServerURL{get{ return m_ServerURL;}}

    /// <summary>
    /// Extracts user ID from User
    /// </summary>
    public string UserID
    {
      get
      {
        if (m_User==null || m_User==User.Fake) return string.Empty;
        var cred = m_User.Credentials as IDPasswordCredentials;
        if (cred==null) return string.Empty;
        return cred.ID;
      }
    }

    /// <summary>
    /// Extracts user Password from User
    /// </summary>
    public string UserPassword
    {
      get
      {
        if (m_User==null || m_User==User.Fake) return string.Empty;
        var cred = m_User.Credentials as IDPasswordCredentials;
        if (cred==null) return string.Empty;
        return cred.Password;
      }
    }

    public WebDAV WebDAV { get { return m_WebDAV; }}

    public override IFileSystemVersion LatestVersion { get { return m_WebDAV.GetHeadRootVersion(); }}

    public override IFileSystemVersion Version
    {
      get { return m_WebDAV.CurrentVersion; }
      set { m_WebDAV.CurrentVersion = (WebDAV.Version)value; }
    }

    public override IEnumerable<IFileSystemVersion> GetVersions(IFileSystemVersion from, int countBack)
    {
      int fromVersionNumber = from.Name.AsInt();
      int listVersionFirst = fromVersionNumber - countBack;
      int listVersionLast = fromVersionNumber - 1;

      IEnumerable<IFileSystemVersion> versions = m_WebDAV.ListVersions(listVersionFirst.ToString(), listVersionLast.ToString());

      return versions;
    }

    protected override void ValidateConnectParams(FileSystemSessionConnectParams cParams)
    {
      var svnp = (SVNFileSystemSessionConnectParams)cParams;

      if (svnp.ServerURL.IsNullOrWhiteSpace())
      throw new NFXIOException(WSC.FS_SVN_PARAMS_SERVER_URL_ERROR);
    }

  }
}
