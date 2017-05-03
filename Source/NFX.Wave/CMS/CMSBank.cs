using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.IO.FileSystem;
using NFX.ServiceModel;

namespace NFX.Wave.CMS
{
  /*
   *  CMS is based on a file system that supports versioning, creation stamps, and security
   *  the content is kept in files most of which are laconic config (.cms) extension
   *
   *  The CMS consists of hierarchical namespaces - folders with *.ns.
   *  The top-most folder is a portal folder that ends with *.p
   *
   *  /portal.p
   *     /root.ns
   *       /buyer.ns
   *       /seller.ns
   *
   *
   * Namespaces have resources in the files in laconic format:
   *   page.vw - view/page mappable to portal type//<--- a chto eto?
   *   main.mnu - menu
   *   value.map - key -> value nls maps
   *   named.iso.tpl - template interpretable at run-time
   *   ....binary files... (js, images, fonts, text)
   *
   *
   *
   *
   *
   *
   *
   */


  /// <summary>
  /// Provides CMS backend access services.
  /// This class is injected via PortalHub.Instance.CMSBank
  /// </summary>
  public class CMSBank : ServiceWithInstrumentationBase<object>, ICMSBankImplementation
  {
    public const int VERSION_HISTORY_LENGTH = 8;
    public const int REFRESH_VERSION_INTERVAL_SEC = 5 * 60;

    #region .ctor
      public CMSBank() : base()
      {
      }

      protected override void Destructor()
      {
        base.Destructor();
      }
    #endregion


    #region Fields
      private FileSystemSessionConnectParams m_ContentFSConnect;
      private FileSystem m_ContentFS;
      private string m_ContentFSRootPath;

      private object m_VersionLock = new object();
      private DateTime m_LastVersionTimestamp;
      private IFileSystemVersion m_LatestVersion;
      private List<IFileSystemVersion> m_Versions = new List<IFileSystemVersion>();
    #endregion

    #region Properties

      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public override bool InstrumentationEnabled{ get; set; }

      /// <summary>
      /// Returns file system that serves static content for portals
      /// </summary>
      public IFileSystem ContentFileSystem{ get{ return m_ContentFS;}}

      public FileSystemSessionConnectParams ContentFileSystemConnectParams{ get{ return m_ContentFSConnect;}}

      /// <summary>
      /// Returns root path for content file system
      /// </summary>
      public string ContentFileSystemRootPath{ get{ return m_ContentFSRootPath;}}


      public IEnumerable<IFileSystemVersion> Versions
      {
        get
        {
          if (!m_ContentFS.InstanceCapabilities.SupportsVersioning)
           return Enumerable.Empty<IFileSystemVersion>();

          lock(m_VersionLock)
          {
            var now = DateTime.UtcNow;
            if ((now-m_LastVersionTimestamp).TotalSeconds < REFRESH_VERSION_INTERVAL_SEC)
              return m_Versions.ToArray();
            m_LastVersionTimestamp = now;

            using(var fsSession = m_ContentFS.StartSession( m_ContentFSConnect))
            {
              m_LatestVersion = fsSession.LatestVersion;
              var newVersions = fsSession.GetVersions(m_LatestVersion, VERSION_HISTORY_LENGTH);
              var lst = new List<IFileSystemVersion>( m_Versions );
              foreach(var newVersion in newVersions)
              {
                if (!lst.Any( v => v == newVersion)) //list can only grow
                 lst.Add(newVersion);
              }
              m_Versions = lst;//atomic
            }
          }
          return m_Versions.ToArray();
        }
      }

      public IFileSystemVersion LatestVersion
      {
        get
        {
          if (!m_ContentFS.InstanceCapabilities.SupportsVersioning)
            return null;

          var versions = Versions;
          return m_LatestVersion;
        }
      }

      private int getVersionId(IFileSystemVersion version)
      {
        if (version==null) return -1;
        var lst = m_Versions;
        return lst.IndexOf(version);
      }

    #endregion

    #region Public


      public IFileSystemVersion GetVersionByID(int verID)
      {
        var lst = m_Versions;
        if (verID>=0 && verID<lst.Count)
         return lst[verID];
        return null;
      }

      public ICMSContext GetContext(Portal portal, IFileSystemVersion version)
      {
        if (version==null)
         version = LatestVersion;

        var versionId = getVersionId(version);


        //todo sdelat context

        return null;
      }

    #endregion

    #region protected

      //todo  DObavit cash i vsjy logiku po rabote s filami


      protected override void DoConfigure(IConfigSectionNode node)
      {

        //Make File System
        var fsNode =  node[PortalHub.CONFIG_CONTENT_FS_SECTION];

        m_ContentFS = FactoryUtils.MakeAndConfigure<FileSystem>(fsNode,
                                                         typeof(NFX.IO.FileSystem.Local.LocalFileSystem),
                                                         args: new object[]{GetType().Name, fsNode});
        var fsPNode = fsNode[PortalHub.CONFIG_FS_CONNECT_PARAMS_SECTION];

        if (fsPNode.Exists)
        {
          m_ContentFSConnect = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(fsPNode);
        }
        else
        {
          m_ContentFSConnect = new FileSystemSessionConnectParams(){ User = NFX.Security.User.Fake};
        }

        m_ContentFSRootPath = fsNode.AttrByName(PortalHub.CONFIG_FS_ROOT_PATH_ATTR).Value;
        if (m_ContentFSRootPath.IsNullOrWhiteSpace())
         throw new WaveException(StringConsts.CONFIG_CMS_BANK_FS_ROOT_PATH_ERROR.Args(PortalHub.CONFIG_CONTENT_FS_SECTION, PortalHub.CONFIG_FS_ROOT_PATH_ATTR));

      }

      protected override void DoStart()
      {
        base.DoStart();
      }

      protected override void DoSignalStop()
      {
        base.DoSignalStop();
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();
      }

    #endregion


  }

}
