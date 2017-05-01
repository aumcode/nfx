using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using NFX.Environment;
using NFX.IO.FileSystem;

namespace NFX.Wave.CMS
{
  /// <summary>
  /// Represents NOPCMSBank that does nothing - indicating the absense of any CMS system
  /// </summary>
  public sealed class NOPCMSBank : ICMSBankImplementation
  {

    public static readonly NOPCMSBank Instance = new NOPCMSBank();

    private NOPCMSBank(){}

    public IFileSystemVersion LatestVersion{ get { return null;} }

    public IEnumerable<IFileSystemVersion> Versions { get { return Enumerable.Empty<IFileSystemVersion>(); } }

    public ICMSContext GetContext(Portal portal, IO.FileSystem.IFileSystemVersion version)
    {
      return new NOPCMSContext(portal, version);
    }

    public void Dispose(){ }

    public void Configure(Environment.IConfigSectionNode node){ }

    public bool InstrumentationEnabled {  get {return false; } set{ } }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters
    {
      get{ return Enumerable.Empty<KeyValuePair<string, Type>>(); }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return Enumerable.Empty<KeyValuePair<string, Type>>();
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      value = null;
      return false;
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      value = null;
      return false;
    }

    public IFileSystemVersion GetVersionByID(int verID)
    {
      return null;
    }
  }

  /// <summary>
  /// Represents CMS context that does nothing. Returned by NOPCMSBank
  /// </summary>
  public sealed class NOPCMSContext : ICMSContext
  {
    internal NOPCMSContext(Portal portal, IFileSystemVersion version)
    {
      m_Portal = portal;
      m_Version = version;
    }

    private Portal m_Portal;
    private IFileSystemVersion m_Version;

    public Portal Portal { get { return m_Portal; } }

    public IFileSystemVersion Version { get { return m_Version; } }

    public Resource TryNavigate(string path, DataAccess.CRUD.ICacheParams caching = null)
    {
      return null;
    }

    public void Dispose(){ }


    public DirectoryResource Root
    {
      get { return null; }
    }


    public int VersionID
    {
      get { return 0; }
    }
  }

}
