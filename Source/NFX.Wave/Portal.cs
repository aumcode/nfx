using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Log;
using NFX.Serialization.JSON;
using NFX.ApplicationModel;
using NFX.Instrumentation;
using NFX.Environment;

namespace NFX.Wave
{
  /// <summary>
  /// Represents a web portal that controls the mapping of types and themes within the site.
  /// Portals allow to host differently-looking/behaving sites in the same web application
  /// </summary>
  public abstract class Portal : ApplicationComponent, INamed, IInstrumentable
  {
    #region CONSTS
      public const string CONFIG_THEME_SECTION = "theme";
      public const string CONFIG_LOCALIZATION_SECTION = "localization";
      public const string CONFIG_CONTENT_SECTION = "content";
      public const string CONFIG_RECORD_MODEL_SECTION = "record-model-generator";

      public const string CONFIG_MSG_FILE_ATTR = "msg-file";
      public const string LOC_ANY_SCHEMA_KEY = "--ANY-SCHEMA--";
      public const string LOC_ANY_FIELD_KEY = "--ANY-FIELD--";

      public const string CONFIG_DESCR_ATTR = "description";
      public const string CONFIG_DISPLAY_NAME_ATTR = "display-name";
      public const string CONFIG_OFFLINE_ATTR = "offline";
      public const string CONFIG_DEFAULT_ATTR = "default";
      public const string CONFIG_PRIMARY_ROOT_URI_ATTR = "primary-root-uri";
      public const string CONFIG_PARENT_NAME_ATTR = "parent-name";
    #endregion

    #region Inner Types

      public enum MoneyFormat{WithCurrencySymbol, WithoutCurrencySymbol}

      public enum DateTimeFormat{ShortDate, LongDate, ShortDateTime, LongDateTime}

    #endregion

    #region .ctor
      /// <summary>
      /// Makes portal from config.
      /// Due to the nature of Portal object there is no need to create other parametrized ctors
      /// </summary>
      protected Portal(IConfigSectionNode conf) : base(PortalHub.Instance)
      {
        const string PORTAL = "portal";

        m_Name = conf.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        if (m_Name.IsNullOrWhiteSpace())
        {
          m_Name = this.GetType().Name;
          if (m_Name.EndsWith(PORTAL, StringComparison.OrdinalIgnoreCase) && m_Name.Length>PORTAL.Length)
           m_Name = m_Name.Substring(0, m_Name.Length-PORTAL.Length);
        }

        //Register with the Hub
        if (!PortalHub.Instance.m_Portals.Register( this ))
          throw new WaveException(StringConsts.PORTAL_HUB_INSTANCE_ALREADY_CONTAINS_PORTAL_ERROR.Args(m_Name));



        m_Description = conf.AttrByName(CONFIG_DESCR_ATTR).ValueAsString(m_Name);
        m_Offline = conf.AttrByName(CONFIG_OFFLINE_ATTR).ValueAsBool(false);
        m_Default = conf.AttrByName(CONFIG_DEFAULT_ATTR).ValueAsBool(false);

        var puri = conf.AttrByName(CONFIG_PRIMARY_ROOT_URI_ATTR).Value;

        try{ m_PrimaryRootUri = new Uri(puri, UriKind.Absolute); }
        catch(Exception error)
        {
          throw new WaveException(StringConsts.CONFIG_PORTAL_ROOT_URI_ERROR.Args(m_Name, error.ToMessageWithType()), error);
        }

        m_DisplayName = conf.AttrByName(CONFIG_DISPLAY_NAME_ATTR).Value;

        if (m_DisplayName.IsNullOrWhiteSpace())
          m_DisplayName = m_PrimaryRootUri.ToString();

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

        m_ParentName = conf.AttrByName(CONFIG_PARENT_NAME_ATTR).Value;

        ConfigAttribute.Apply(this, conf);

        m_LocalizableContent = new Dictionary<string,string>(GetLocalizableContent(), StringComparer.InvariantCultureIgnoreCase);
        foreach(var atr in conf[CONFIG_LOCALIZATION_SECTION][CONFIG_CONTENT_SECTION].Attributes)
         m_LocalizableContent[atr.Name] = atr.Value;

        var gen = conf[CONFIG_RECORD_MODEL_SECTION];
        m_RecordModelGenerator = FactoryUtils.Make<Client.RecordModelGenerator>(gen,
                                                                                typeof(Client.RecordModelGenerator),
                                                                                new object[]{gen});

        m_RecordModelGenerator.ModelLocalization += recGeneratorLocalization;

        m_LocalizationData = conf[CONFIG_LOCALIZATION_SECTION];
        var msgFile = m_LocalizationData.AttrByName(CONFIG_MSG_FILE_ATTR).Value;
        if (msgFile.IsNotNullOrWhiteSpace())
        try
        {
           m_LocalizationData = Configuration.ProviderLoadFromFile(msgFile).Root;
        }
        catch(Exception fileError)
        {
           throw new WaveException(StringConsts.CONFIG_PORTAL_LOCALIZATION_FILE_ERROR.Args(m_Name, msgFile, fileError.ToMessageWithType()), fileError);
        }
      }//.ctor

      protected override void Destructor()
      {
        var hub = PortalHub.s_Instance;
        if (hub!=null)
          hub.m_Portals.Unregister( this );

        base.Destructor();
      }

    #endregion


    #region Fields
      private string m_Name;
      private string m_Description;
      private string m_DisplayName;
      private bool m_InstrumentationEnabled;

      private bool m_Offline;
      private bool m_Default;

      private Uri m_PrimaryRootUri;

      private Theme m_DefaultTheme;
      private Registry<Theme> m_Themes;

      private string m_ParentName;
      private Dictionary<string, string> m_LocalizableContent;

      private Client.RecordModelGenerator m_RecordModelGenerator;

      private IConfigSectionNode m_LocalizationData;
    #endregion


    #region Properties

      /// <summary>
      /// Globally-unique portal name/ID
      /// </summary>
      public string Name{ get { return m_Name;  } }


      /// <summary>
      /// Primary site display name in primary language
      /// </summary>
      public string DisplayName{ get{ return m_DisplayName;}}

      /// <summary>
      /// English/primary language description
      /// </summary>
      public string Description{ get { return m_Description;  } }


      /// <summary>
      /// Primary root URL used to access this portal
      /// </summary>
      public Uri PrimaryRootUri{ get{ return m_PrimaryRootUri;}}


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



      /// <summary>
      /// Points to the parent portal instance where some settings can be cloned (i.e. localization strings)
      /// </summary>
      public string ParentName
      {
        get { return m_ParentName;  }
      }


      /// <summary>
      /// Returns parent portal as identified by ParentName or null.
      /// Makes sure that there is no cycle in portal derivation
      /// </summary>
      public Portal Parent
      {
        get
        {
          const int MAX_DEPTH = 3;
          Portal portal = this;
          Portal myParent = null;
          var depth = 0;
          while(portal.m_ParentName.IsNotNullOrWhiteSpace())
          {
            portal = PortalHub.Instance.Portals[m_ParentName];
            if (portal==null)
              throw new WaveException(StringConsts.PORTAL_PARENT_INVALID_ERROR.Args(this.Name, this.m_ParentName));

            if (myParent==null) myParent = portal;

            depth++;
            if (depth>MAX_DEPTH)
              throw new WaveException(StringConsts.PORTAL_PARENT_DEPTH_ERROR.Args(this.Name, this.m_ParentName, MAX_DEPTH));
          }

          return myParent;
        }
      }

      /// <summary>
      /// Returns default language ISO code for this portal
      /// </summary>
      public abstract string DefaultLanguageISOCode{ get; }

      /// <summary>
      /// Returns default currency ISO code for this portal
      /// </summary>
      public abstract string DefauISOCurrency{ get; }


      /// <summary>
      /// Returns record model generator that creates JSON for WV.JS library form server-supplied metadata
      /// </summary>
      public Client.RecordModelGenerator RecordModelGenerator
      {
        get{ return m_RecordModelGenerator;}
      }

      /// <summary>
      /// Set to true to capture the localization errors in log - used for development
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_LOG)]
      public bool DumpLocalizationErrors{ get; set;}

    #endregion

    #region Public


      /// <summary>
      /// Translates the named content into desired language trying to infer language from work context/locality/session.
      /// The search is first done in this portal then in inherited portals.
      /// Returns an empty string if no translation is possible
      /// </summary>
      public virtual string TranslateContent(string contentKey, string isoLang = null, WorkContext work = null)
      {
        if (isoLang.IsNullOrWhiteSpace())
          isoLang = GetLanguageISOCode(work);

        string result;

        var portal = this;

        while(portal!=null)
        {
          var content = portal.m_LocalizableContent;

          if (content.TryGetValue(contentKey+"_"+isoLang, out result)) return result;
          if (!isoLang.EqualsOrdIgnoreCase(portal.DefaultLanguageISOCode))
          {
            if (content.TryGetValue(contentKey+"_"+portal.DefaultLanguageISOCode, out result)) return result;
          }
          if (content.TryGetValue(contentKey, out result)) return result;

          portal = portal.Parent;
        }

        return string.Empty;
      }

      /// <summary>
      /// Tries to determine session/work context lang and returns it or DefaultLanguageISOCode
      /// </summary>
      public virtual string GetLanguageISOCode(WorkContext work = null)
      {
        string lang = null;

        if (work==null)
          work = ExecutionContext.Request as WorkContext;

        if (work==null)
        {
          var session = ExecutionContext.Session;
          if (session!=null)
           lang = session.LanguageISOCode;
        }
        else
        {
          var session = work.Session;
          if (session!=null)
            lang = session.LanguageISOCode;

          if (lang.IsNullOrWhiteSpace() && work.GeoEntity!=null && work.GeoEntity.Location.HasValue)
          {
            var country = work.GeoEntity.CountryISOName;
            lang =  CountryISOCodeToLanguageISOCode(country);
          }
        }

        return lang.IsNullOrWhiteSpace() ? this.DefaultLanguageISOCode : lang;
      }


      /// <summary>
      /// Converts country code into language code per this portal
      /// </summary>
      public abstract string CountryISOCodeToLanguageISOCode(string countryISOCode);


      /// <summary>
      /// Converts financial amount in portals.default currency to string per portal
      /// </summary>
      public virtual string AmountToString(decimal amount,
                                   MoneyFormat format = MoneyFormat.WithCurrencySymbol,
                                   ISession session = null)
      {
        return AmountToString(new Financial.Amount(this.DefauISOCurrency, amount), format, session);
      }

      /// <summary>
      /// Converts financial amount to string per portal
      /// </summary>
      public abstract string AmountToString(Financial.Amount amount,
                                   MoneyFormat format = MoneyFormat.WithCurrencySymbol,
                                   ISession session = null);

      /// <summary>
      /// Converts datetime to string per portal
      /// </summary>
      public abstract string DateTimeToString(DateTime dt,
                                      DateTimeFormat format = DateTimeFormat.LongDateTime,
                                      ISession session = null);


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

      public override string ToString()
      {
        return "{0}('{1}')".Args(GetType().Name, m_Name);
      }

    #endregion

    #region Protected

      /// <summary>
      /// Override to add localizable system content blocks.
      /// Each block has a key of the form:  'keyname_isoLang'. I.e. {"mnuStart_deu", "Anfangen"}...
      /// </summary>
      protected abstract Dictionary<string, string> GetLocalizableContent();


      private string recGeneratorLocalization(Client.RecordModelGenerator gen, string schema, string field, string value, string isoLang)
      {
        return DoLocalizeRecordModel(schema, field, value, isoLang);
      }

      /// <summary>
      /// Localizes record model schema:field:value
      /// </summary>
      protected virtual string DoLocalizeRecordModel(string schema, string field, string value, string isoLang)
      {
        if (value.IsNullOrWhiteSpace()) return value;

        if (!m_LocalizationData.Exists) return value;//nowhere to lookup

        if (isoLang.IsNullOrWhiteSpace())
        {
          var session = ExecutionContext.Session;
          if (session==null) return value;

          isoLang = session.LanguageISOCode;
        }

        if (isoLang.IsNullOrWhiteSpace())
         isoLang = DefaultLanguageISOCode;

        if (isoLang.EqualsOrdIgnoreCase(CoreConsts.ISO_LANG_ENGLISH)) return value;

        if (schema.IsNullOrWhiteSpace()) schema = LOC_ANY_SCHEMA_KEY;
        if (field.IsNullOrWhiteSpace())  field = LOC_ANY_FIELD_KEY;
        bool exists;
        var lv = DoLookupLocalizationValue(isoLang, schema, field, value, out exists);

        //Use this to find out what strings need translation
        if (DumpLocalizationErrors && !exists)
        {
            App.Log.Write( new Message{
              Type = MessageType.InfoZ,
              From = "lookup",
              Topic = CoreConsts.LOCALIZATION_TOPIC,
              Text = "Need localization",
              Parameters = (new {iso = isoLang, sch = schema, fld = field, val = value }).ToJSON()
            });
        }

        return lv;
      }

      protected virtual string DoLookupLocalizationValue(string isoLang, string schema, string field, string value, out bool exists)
      {
        exists = false;

        var nlang = m_LocalizationData[isoLang];
        if (!nlang.Exists) return value;
        var nschema = nlang[schema, LOC_ANY_SCHEMA_KEY];
        if (!nschema.Exists) return value;
        var nfield = nschema[field, LOC_ANY_FIELD_KEY];
        if (!nfield.Exists) return value;

        var nvalue = nfield.Attributes.FirstOrDefault(a=>a.Name == value);//case SENSITIVE search
        if (nvalue==null) return value;
        var lv = nvalue.Value;

        if (lv.IsNotNullOrWhiteSpace())
        {
           exists = true;
           return lv;
        }

        return value;
      }

    #endregion
  }

}
