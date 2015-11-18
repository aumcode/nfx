using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Instrumentation;
using NFX.Environment;

namespace NFX.Wave
{
  /// <summary>
  /// Represents a web portal that controls the mapping of types and themes within the site.
  /// Portals allow to host differently-looking/behaving sites in the same web application
  /// </summary>
  public class Portal : INamed, IInstrumentable
  {
    public const string DEFAULT_STATIC_PATH =  "/static";
    public const string DEFAULT_IMG_PATH    =  "img";
    public const string DEFAULT_STL_PATH    =  "stl";
    public const string DEFAULT_FNT_PATH    =  "fnt";
    public const string DEFAULT_ELM_PATH    =  "elm";
    public const string DEFAULT_SCR_PATH    =  "scr";
    public const string DEFAULT_STOCK_PATH  =  "stock/site";


    private string m_Name;
    private string m_Description;

    private bool m_Offline;
    private bool m_Default;

    private Theme m_DefaultTheme;
    private Registry<Theme> m_Themes;

    private Uri m_PrimaryRootUri;

    private string m_StaticRootPath;
    private string m_ImagePath;
    private string m_StylePath;
    private string m_FontPath;
    private string m_ElementPath;
    private string m_ScriptPath;
    private string m_StockPath;



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
    public bool Offline
    {
      get { return m_Offline;  } 
      set { m_Offline = value;}
    }

    /// <summary>
    /// If true, gets matched when no othe suites
    /// </summary>
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

    [Config]
    public string StaticRootPath
    {
      get{ return m_StaticRootPath ?? DEFAULT_STATIC_PATH;}
      set { m_StaticRootPath = value;}
    }

    [Config]
    public string StaticImagePath
    {
      get{ return m_ImagePath ?? DEFAULT_IMG_PATH;}
      set { m_ImagePath = value;}
    }

    [Config]
    public string StaticStylePath
    {
      get{ return m_StylePath ?? DEFAULT_STL_PATH;}
      set { m_StylePath = value;}
    }

    [Config]
    public string StaticFontPath
    {
      get{ return m_FontPath ?? DEFAULT_FNT_PATH;}
      set { m_FontPath = value;}
    }

    [Config]
    public string StaticElementPath
    {
      get{ return m_ElementPath ?? DEFAULT_ELM_PATH;}
      set { m_ElementPath = value;}
    }

    [Config]
    public string StaticScriptPath
    {
      get{ return m_ScriptPath ?? DEFAULT_SCR_PATH;}
      set { m_ScriptPath = value;}
    }

    [Config]
    public string StaticStockPath
    {
      get{ return m_StockPath ?? DEFAULT_STOCK_PATH;}
      set { m_StockPath = value;}
    }



    #region IInstrumentable
    public bool InstrumentationEnabled
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters
    {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      throw new NotImplementedException();
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      throw new NotImplementedException();
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      throw new NotImplementedException();
    }
    #endregion
  }

  /// <summary>
  /// Represents a portal theme. Theme resources are located in the thema-named subfolder i.e.: /static/bees/stl/main.css
  /// </summary>
  /// <remarks>
  /// /static
  ///   /elm - common elements
  ///   /stock/site - stock resources
  ///   
  ///   /bees  - theme name
  ///      /img
  ///      /stl
  ///      /fnt
  ///      /scr
  ///    
  ///   /sunny - theme name
  ///      /img
  ///      /stl
  ///      /fnt
  ///      /scr
  ///  ...  
  /// </remarks>
  public class Theme : INamed
  {
    
    private Portal m_Portal;
    private string m_Name;
    private string m_Description;
    
    private string m_StaticRootPath;
    private string m_ImagePath;
    private string m_StylePath;
    private string m_FontPath;
    private string m_ElementPath;
    private string m_ScriptPath;
    private string m_StockPath;

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


    [Config]
    public string StaticRootPath
    {
      get{ return m_StaticRootPath ?? Portal.StaticRootPath;}
      set { m_StaticRootPath = value;}
    }

    [Config]
    public string StaticImagePath
    {
      get{ return m_ImagePath ?? Portal.StaticImagePath;}
      set { m_ImagePath = value;}
    }

    [Config]
    public string StaticStylePath
    {
      get{ return m_StylePath ?? Portal.StaticStylePath;}
      set { m_StylePath = value;}
    }

    [Config]
    public string StaticFontPath
    {
      get{ return m_FontPath ?? Portal.StaticFontPath;}
      set { m_FontPath = value;}
    }

    [Config]
    public string StaticElementPath
    {
      get{ return m_ElementPath ?? Portal.StaticElementPath;}
      set { m_ElementPath = value;}
    }

    [Config]
    public string StaticScriptPath
    {
      get{ return m_ScriptPath ?? Portal.StaticScriptPath;}
      set { m_ScriptPath = value;}
    }

    [Config]
    public string StaticStockPath
    {
      get{ return m_StockPath ?? Portal.StaticStockPath;}
      set { m_StockPath = value;}
    }



    /// <summary>
    /// Returns the rooted static image path
    /// </summary>
    public virtual string Image(string path)
    {
      return Path.Combine(StaticRootPath, m_Name, StaticImagePath, path);
    }

    /// <summary>
    /// Returns the rooted static style path
    /// </summary>
    public virtual string Style(string path)
    {
      return Path.Combine(StaticRootPath, m_Name, StaticStylePath, path);
    }


    /// <summary>
    /// Returns the rooted static font path
    /// </summary>
    public virtual string Font(string path)
    {
      return Path.Combine(StaticRootPath, m_Name, StaticFontPath, path);
    }

    /// <summary>
    /// Returns the rooted static element (arbitrary files) path
    /// Unlike other path accessors the default implemntation does not prepend theme name
    /// </summary>
    public virtual string Element(string path)
    {
      return Path.Combine(StaticRootPath, StaticElementPath, path);
    }

    /// <summary>
    /// Returns the rooted static script path
    /// </summary>
    public virtual string Script(string path)
    {
      return Path.Combine(StaticRootPath, m_Name, StaticScriptPath, path);
    }

    /// <summary>
    /// Returns the rooted static stock (usually pre-compiled DLL resource) path.
    /// Unlike other path accessors the default implemntation does not prepend theme name
    /// </summary>
    public virtual string Stock(string path)
    {
      return Path.Combine(StaticRootPath, StaticStockPath, path);
    }
  }


}
