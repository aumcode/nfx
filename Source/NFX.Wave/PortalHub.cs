using System.Linq;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.IO.FileSystem;

namespace NFX.Wave
{
  /// <summary>
  /// Portal hub - a registry of portals, It establishes a context for portal inter-operation (i.e. so one portal may locate another by name)
  ///  when some settings need to be cloned. This is an app-started singleton instance class
  /// </summary>
  public sealed class PortalHub : ApplicationComponent, IApplicationStarter, IApplicationFinishNotifiable
  {
    public const string CONFIG_PORTAL_SECTION = "portal";
    public const string CONFIG_CONTENT_FS_SECTION = "content-file-system";
    public const string CONFIG_FS_CONNECT_PARAMS_SECTION = "connect-params";
    public const string CONFIG_FS_ROOT_PATH_ATTR = "root-path";

    private static object s_Lock = new object();
    internal static volatile PortalHub s_Instance;


    /// <summary>
    /// True if instance is allocated
    /// </summary>
    public static bool InstanceAvailable
    {
      get{ return s_Instance!=null;}
    }

    /// <summary>
    /// Returns singleton instance
    /// </summary>
    public static PortalHub Instance
    {
      get
      {
         lock(s_Lock)
         {
           if (s_Instance==null)
             throw new WaveException(StringConsts.PORTAL_HUB_INSTANCE_IS_NOT_AVAILABLE_ERROR);

           return s_Instance;
         }
      }
    }

    internal PortalHub() : base()
    {
      lock(s_Lock)
      {
        if (s_Instance!=null)
             throw new WaveException(StringConsts.PORTAL_HUB_INSTANCE_IS_ALREADY_AVAILABLE_ERROR);

        m_Portals = new Registry<Portal>(false);

        s_Instance = this;
      }
    }

    protected override void Destructor()
    {
      lock(s_Lock)
      {
        if (s_Instance != null)
        {
          s_Instance = null;

          foreach(var portal in m_Portals)
            portal.Dispose();

          DisposableObject.DisposeAndNull(ref m_ContentFS);

          base.Destructor();
        }
      }
    }


    private FileSystemSessionConnectParams m_ContentFSConnect;
    private FileSystem m_ContentFS;
    private string m_ContentFSRootPath;

    internal Registry<Portal> m_Portals;


    /// <summary>
    /// Registry of all portals in the hub
    /// </summary>
    public IRegistry<Portal> Portals{ get{ return m_Portals;} }


    /// <summary>
    /// Returns file system that serves static content for portals
    /// </summary>
    public IFileSystem ContentFileSystem{ get{ return m_ContentFS;}}

    public FileSystemSessionConnectParams ContentFileSystemConnectParams{ get{ return m_ContentFSConnect;}}

    /// <summary>
    /// Returns root path for content file system
    /// </summary>
    public string ContentFileSystemRootPath{ get{ return m_ContentFSRootPath;}}

    /// <summary>
    /// Returns first portal which is not Offline and marked as default
    /// </summary>
    public Portal DefaultOnline
    {
      get
      {
        return m_Portals.FirstOrDefault(p => !p.Offline && p.Default);
      }
    }


    bool IApplicationStarter.ApplicationStartBreakOnException { get { return true; } }

    void IApplicationStarter.ApplicationStartBeforeInit(IApplication application) { }

    void IApplicationStarter.ApplicationStartAfterInit(IApplication application)
    {
      application.RegisterAppFinishNotifiable(this);
    }

    void IConfigurable.Configure(IConfigSectionNode node)
    {
      if (node==null || !node.Exists)
        throw new WaveException(StringConsts.CONFIG_PORTAL_HUB_NODE_ERROR);

      foreach(var cn in node.Children.Where(cn=>cn.IsSameName(CONFIG_PORTAL_SECTION)))
        m_Portals.Register( FactoryUtils.Make<Portal>(cn, typeof(Portal), args: new object[]{ cn }));

      //Make File System
      var fsNode =  node[CONFIG_CONTENT_FS_SECTION];

      m_ContentFS = FactoryUtils.MakeAndConfigure<FileSystem>(fsNode,
                                                       typeof(NFX.IO.FileSystem.Local.LocalFileSystem),
                                                       args: new object[]{GetType().Name, fsNode});
      var fsPNode = fsNode[CONFIG_FS_CONNECT_PARAMS_SECTION];

      if (fsPNode.Exists)
      {
        m_ContentFSConnect = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(fsPNode);
      }
      else
      {
        m_ContentFSConnect = new FileSystemSessionConnectParams(){ User = NFX.Security.User.Fake};
      }

      m_ContentFSRootPath = fsNode.AttrByName(CONFIG_FS_ROOT_PATH_ATTR).Value;
      if (m_ContentFSRootPath.IsNullOrWhiteSpace())
       throw new WaveException(StringConsts.CONFIG_PORTAL_HUB_FS_ROOT_PATH_ERROR.Args(CONFIG_CONTENT_FS_SECTION, CONFIG_FS_ROOT_PATH_ATTR));
    }

    public string Name { get { return GetType().Name; } }

    void IApplicationFinishNotifiable.ApplicationFinishBeforeCleanup(IApplication application)
    {
      Dispose();
    }

    void IApplicationFinishNotifiable.ApplicationFinishAfterCleanup(IApplication application) { }

    /// <summary>
    /// Generates file version path segment suitable for usage in file name.
    /// This method is slow as it does byte file sig calculation
    /// </summary>
    public string GenerateContentFileVersionSegment(string filePath)
    {
       if (m_ContentFS==null) return null;
       if (m_ContentFSConnect==null) return null;
       if (filePath.IsNullOrWhiteSpace()) return null;

       using(var session = m_ContentFS.StartSession(m_ContentFSConnect))
       {
         var buf = new byte[8*1024];
         var fName = m_ContentFS.CombinePaths(m_ContentFSRootPath, filePath);
         var fsFile = session[fName] as FileSystemFile;
         if (fsFile==null) return null;
         long sz = 0;
         var csum = new NFX.IO.ErrorHandling.Adler32();
         using(var stream = fsFile.FileStream)
         while(true)
         {
           var got = stream.Read(buf, 0, buf.Length);
           if (got<=0) break;
           sz += got;
           csum.Add(buf, 0, got);
         }

         var data = (ulong)sz << 32 | (ulong)csum.Value;
         return data.ToString("X").ToLowerInvariant();
       }

    }


  }//hub

}
