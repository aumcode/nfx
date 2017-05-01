using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Instrumentation;
using NFX.Environment;
using NFX.IO.FileSystem;
using NFX.DataAccess.CRUD;

namespace NFX.Wave.CMS
{
  /// <summary>
  /// Represents the primary singleton interface to CMS backend accessible via PortalHub.Instance.CMS
  /// </summary>
  public interface ICMSBank
  {
    IFileSystemVersion LatestVersion{ get;}
    IEnumerable<IFileSystemVersion> Versions{ get;}
    IFileSystemVersion GetVersionByID(int verID);

    ICMSContext GetContext(Portal portal, IFileSystemVersion version);
  }

  public interface ICMSBankImplementation : ICMSBank, IDisposable, IConfigurable, IInstrumentable
  {

  }


  /// <summary>
  /// Describes context under which CMS operations take place.
  /// The context should be obrtained via a call to Portal.GetCMSContext()
  /// </summary>
  public interface ICMSContext : IDisposable
  {
    /// <summary>
    /// References a portal for which the operations are performed
    /// </summary>
    Portal Portal{ get; }

    /// <summary>
    /// Returns an ID of the version that this CMS context "sees".
    /// Use CMSBank.GetVersionByID() to get IFileSystemVersion
    /// </summary>
    int VersionID{ get;}

    /// <summary>
    /// Returns version that this CMS context "sees".
    /// </summary>
    IFileSystemVersion Version{ get;}

    /// <summary>
    /// Gets root namespace for portal
    /// </summary>
    DirectoryResource Root{ get; }

    /// <summary>
    /// Retrievs CMS Item by path or null if not found
    /// </summary>
    Resource TryNavigate(string path, ICacheParams caching = null);
  }


  /// <summary>
  /// Provides extension methods for ICMSContext
  /// </summary>
  public static class CMSContextExtensions
  {
    public static TResource Navigate<TResource>(this ICMSContext ctx, string path, ICacheParams caching = null) where TResource : Resource
    {
      var result = ctx.TryNavigate(path, caching) as TResource;
      if (result==null)
        throw new CMSException(StringConsts.CMS_NAVIGATE_T_ERROR.Args(ctx.Portal, typeof(TResource), path));
      return result;
    }

  }


}
