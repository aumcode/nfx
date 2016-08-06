using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Instrumentation;
using NFX.Environment;

namespace NFX.Wave
{
  /// <summary>
  /// Represents a portal theme. Theme groups various resources (such as css, scripts etc..)
  /// whitin a portal. Do not inherit your themes from this class directly, instead use Theme(TPortal)
  /// </summary>
  public abstract class Theme : INamed
  {

    protected Theme(Portal portal, IConfigSectionNode conf)
    {
      m_Portal = portal;
      m_Name = conf.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
      if (m_Name.IsNullOrWhiteSpace())
       throw new WaveException(StringConsts.CONFIG_PORTAL_THEME_NO_NAME_ERROR.Args(portal.Name));


      m_Description = conf.AttrByName(Portal.CONFIG_DESCR_ATTR).ValueAsString(m_Name);
      m_Default = conf.AttrByName(Portal.CONFIG_DEFAULT_ATTR).ValueAsBool(false);

      ConfigAttribute.Apply(this, conf);
    }


    private Portal m_Portal;
    private string m_Name;
    private string m_Description;
    private bool m_Default;

    /// <summary>
    /// Parent portal that this theme is under
    /// </summary>
    public Portal Portal{ get{ return m_Portal;}}

    /// <summary>
    /// Globally-unique portal name/ID
    /// </summary>
    public string Name{ get { return m_Name;  } }

    /// <summary>
    /// English/primary language description
    /// </summary>
    public string Description{ get { return m_Description;  } }

    /// <summary>
    /// If true, gets matched when no other suites
    /// </summary>
    public bool Default { get { return m_Default;  } }

    public override string ToString()
    {
      return "{0}('{1}', '{2}')".Args(GetType().Name, m_Portal.Name, m_Name);
    }
  }


  /// <summary>
  /// Represents a portal theme. Theme groups various resources (such as css, scripts etc..)
  /// whitin a portal. Inherit your themes from this class
  /// </summary>
  public abstract class Theme<TPortal> : Theme where TPortal : Portal
  {
    protected Theme(TPortal portal, IConfigSectionNode conf) : base(portal, conf){ }

    /// <summary>
    /// Parent portal that this theme is under
    /// </summary>
    public new TPortal Portal{ get{ return (TPortal)base.Portal;}}
  }
}
