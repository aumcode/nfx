using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Instrumentation;
using NFX.Environment;

namespace NFX.Wave
{
  /// <summary>
  /// Represents a web portal that controls the mapping of types and themes within the site.
  /// Portals allow to host differently-looking/behaving sites in the same web application
  /// </summary>
  public abstract class Portal : INamed, IInstrumentable
  {
    public const string CONFIG_THEME_SECTION = "theme";
    

    public const string CONFIG_DESCR_ATTR = "description";
    public const string CONFIG_OFFLINE_ATTR = "offline";
    public const string CONFIG_DEFAULT_ATTR = "default";
    public const string CONFIG_PRIMARY_ROOT_URI_ATTR = "primary-root-uri";


    /// <summary>
    /// Makes portal from config.
    /// Due to the nature of Portal object there is no need to create other parametrized ctors
    /// </summary>
    protected Portal(IConfigSectionNode conf)
    {
      const string PORTAL = "portal";

      m_Name = conf.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
      if (m_Name.IsNullOrWhiteSpace())
      {
        m_Name = this.GetType().Name;
        if (m_Name.EndsWith(PORTAL, StringComparison.OrdinalIgnoreCase) && m_Name.Length>PORTAL.Length)
         m_Name = m_Name.Substring(0, m_Name.Length-PORTAL.Length);
      }

      m_Description = conf.AttrByName(CONFIG_DESCR_ATTR).ValueAsString(m_Name);
      m_Offline = conf.AttrByName(CONFIG_OFFLINE_ATTR).ValueAsBool(false);
      m_Default = conf.AttrByName(CONFIG_DEFAULT_ATTR).ValueAsBool(false);

      var puri = conf.AttrByName(CONFIG_PRIMARY_ROOT_URI_ATTR).Value;

      try{ m_PrimaryRootUri = new Uri(puri, UriKind.Absolute); }
      catch(Exception error)
      {
        throw new WaveException(StringConsts.CONFIG_PORTAL_ROOT_URI_ERROR.Args(m_Name, error.ToMessageWithType()), error);
      }

      m_Themes = new Registry<Theme>();
      var nthemes = conf.Children.Where(c => c.IsSameName(CONFIG_THEME_SECTION));
      foreach(var ntheme in nthemes)
      {
        var theme = FactoryUtils.Make<Theme>(ntheme, args: new object[]{this, ntheme});
        if(!m_Themes.Register(theme))
          throw new WaveException(StringConsts.CONFIG_PORTAL_DUPLICATE_THEME_NAME_ERROR.Args(theme.Name, m_Name)); 
      }

      if (m_Themes.Count==0)
        throw new WaveException(StringConsts.CONFIG_PORTAL_NO_THEMES_ERROR.Args(m_Name)); 

      m_DefaultTheme = m_Themes.FirstOrDefault(t => t.Default);
      if (m_DefaultTheme==null)
        throw new WaveException(StringConsts.CONFIG_PORTAL_NO_DEFAULT_THEME_ERROR.Args(m_Name)); 

      ConfigAttribute.Apply(this, conf);
    }//.ctor
    
     

    private string m_Name;
    private string m_Description;
    private bool m_InstrumentationEnabled;

    private bool m_Offline;
    private bool m_Default;

    private Uri m_PrimaryRootUri;

    private Theme m_DefaultTheme;
    private Registry<Theme> m_Themes;

   

    /// <summary>
    /// Globally-unique portal name/ID
    /// </summary>
    public string Name{ get { return m_Name;  } }


    /// <summary>
    /// English/primary language description
    /// </summary>
    public string Description{ get { return m_Description;  } }


    /// <summary>
    /// Primary root URL used to access this portal
    /// </summary>
    public Uri PrimaryRootUri{ get{ return m_PrimaryRootUri;}}


    /// <summary>
    /// If true, does not get matched per request
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB)] 
    public bool Offline
    {
      get { return m_Offline;  } 
      set { m_Offline = value;}
    }

    /// <summary>
    /// If true, matches this portal when no other suites
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB)] 
    public bool Default
    {
      get { return m_Default;  } 
      set { m_Default = value;}
    }


    /// <summary>
    /// Returns the default theme used
    /// </summary>
    public Theme DefaultTheme{ get{ return m_DefaultTheme;}}

    /// <summary>
    /// Themes tha this portal supports
    /// </summary>
    public IRegistry<Theme> Themes{ get{ return m_Themes;} }


    public override string ToString()
    {
      return "{0}('{1}')".Args(GetType().Name, m_Name);
    }


    #region IInstrumentable
      /// <summary>
      /// Implements IInstrumentable
      /// </summary>
      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)] 
      public bool InstrumentationEnabled
      {
        get { return m_InstrumentationEnabled;}
        set { m_InstrumentationEnabled = value;}
      }

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      { 
        return ExternalParameterAttribute.GetParameters(this, groups); 
      }

      /// <summary>
      /// Gets external parameter value returning true if parameter was found
      /// </summary>
      public virtual bool ExternalGetParameter(string name, out object value, params string[] groups)
      {
          return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
      }
          
      /// <summary>
      /// Sets external parameter value returning true if parameter was found and set
      /// </summary>
      public virtual bool ExternalSetParameter(string name, object value, params string[] groups)
      {
        return ExternalParameterAttribute.SetParameter(this, name, value, groups);
      }
    #endregion
  }

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
