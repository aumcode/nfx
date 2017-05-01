using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Security;

namespace NFX.Wave.CMS
{
  /// <summary>
  /// Provides abstract base for all CMS resources (i.e. templates, menus, content blocks, files etc.)
  /// </summary>
  [Serializable]
  public abstract class Resource : INamed
  {



    [NonSerialized]
    internal Portal m_Portal;
    private FID m_VersionID;
    private string m_Path;
    private string m_FSPath;

    private DateTime? m_CreationTimestamp;
    private DateTime? m_ModificationTimestamp;
    private DateTime? m_ApprovalTimestamp;

    private User m_CreationUser;
    private User m_ModificationUser;
    private User m_ApprovalUser;


    /// <summary>
    /// Gets full logic resource name starting with [PORTAL]://...path...
    /// </summary>
    public string Name{ get { return m_Portal.Name+"://"+Path;}}

    //Portal that this resource is in
    public Portal Portal { get { return m_Portal;} }

    public FID VersionID{ get{ return m_VersionID;} }

    /// <summary>
    /// Gets logical path to resources (excluding portal)
    /// </summary>
    public string Path{ get { return m_Path;} }

    /// <summary>
    /// Returns full FS path
    /// </summary>
    public string FSPath{ get { return m_FSPath;} }

    public DateTime? CreationTimestamp     { get{ return m_CreationTimestamp;    } }
    public DateTime? ModificationTimestamp { get{ return m_ModificationTimestamp;} }
    public DateTime? ApproveTimestamp      { get{ return m_ApprovalTimestamp;    } }

    public User CreationUser    { get{ return m_CreationUser;     } }
    public User ModificationUser{ get{ return m_ModificationUser; } }
    public User ApproveUser     { get{ return m_ApprovalUser;     } }

   // public bool IsReadOnly { get; }

  }
}
