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
using System;
using System.Linq;

using NFX.Log;
using NFX.Environment;
using NFX.Web.Social;

namespace NFX.Web
{

  /// <summary>
  /// Facilitates fast access to important web-related config settings that update their values when underlying config changes
  /// </summary>
  public class WebSettings: IConfigSettings
  {
    #region CONSTS

      public const string CONFIG_WEBSETTINGS_SECTION = "web-settings";

      public const string CONFIG_SOCIAL_SECTION = "social";
      public const string CONFIG_SOCIAL_PROVIDER_SECTION = "provider";

      public const string CONFIG_TAX_SECTION = "tax";
      public const string CONFIG_TAX_CALCULATOR_SECTION = "calculator";

      public const string CONFIG_LOGTYPE_ATTR = "log-type";
      public const string CONFIG_DEFAULT_TIMEOUT_MS_ATTR = "default-timeout-ms";
      public const int WEBDAV_DEFAULT_TIMEOUT_MS_DEFAULT = 30 * 1000;

      public const string CONFIG_SESSION_TIMEOUT_MS_ATTR = "timeout-ms";
      public const int DEFAULT_SESSION_TIMEOUT_MS = 5/*min*/ * 60/*sec*/ * 1000;
      public const int MIN_SESSION_TIMEOUT_MS = 10*1000;

      public const string CONFIG_SERVICEPOINTMANAGER_SECTION = "service-point-manager";

      public const string CONFIG_WEBDAV_SECTION = "web-dav";

    #endregion

    #region .ctor and static

      private WebSettings() {}

      private static WebSettings s_Instance = new WebSettings();
      private static Guid s_RegisteredAppId;
      private static object s_Lock = new object();

      private static WebSettings Instance
      {
        get
        {
          if (s_RegisteredAppId != NFX.App.InstanceID)
          {
            lock(s_Lock)
              if (s_RegisteredAppId != NFX.App.InstanceID)
              {
                s_Instance.ConfigChanged(null);
                NFX.App.Instance.RegisterConfigSettings(s_Instance);
                s_RegisteredAppId = NFX.App.InstanceID;
              }
          }
          return s_Instance;
        }
      }

    #endregion

    #region Private fields

      private IRegistry<SocialNetwork> m_SocialNetworks;

      private MessageType? m_WebDavLogType;
      private int m_WebDavDefaultTimeoutMs = WEBDAV_DEFAULT_TIMEOUT_MS_DEFAULT;

    #endregion

    #region Settings Properties

      /// <summary>
      /// Social network providers currently present in the system
      /// </summary>
      public static IRegistry<SocialNetwork> SocialNetworks { get { return Instance.m_SocialNetworks ?? new Registry<SocialNetwork>(); }}

      ///// <summary>
      ///// Social network providers currently present in the system
      ///// </summary>
      //public static IRegistry<TaxCalculator> TaxCalculators { get { return Instance.m_TaxCalculators ?? new Registry<TaxCalculator>(); }}

      /// <summary>
      /// When set turns on WebDAV logging
      /// </summary>
      public static MessageType? WebDavLogType { get { return Instance.m_WebDavLogType; }}

      /// <summary>
      /// Sets default timeout for WebDAV requests
      /// 0 means indefinite
      /// </summary>
      public static int WebDavDefaultTimeoutMs { get { return Instance.m_WebDavDefaultTimeoutMs; } }

      /// <summary>
      /// Provides settings related to Http traffic handling
      /// </summary>
      public static ServicePointManagerConfigurator ServicePointManager
      {
        get
        {
          RequireInitializedSettings();
          return ServicePointManagerConfigurator.s_Instance;
        }
      }

    #endregion

    #region Public


      /// <summary>
      /// Called by framework components that rely on initialized web settings.
      /// If the settings have been initialized that this method just does a lock-free check and returns
      /// </summary>
      public static void RequireInitializedSettings()
      {
        var i = Instance;
      }

      public static void ChangeConfig(IConfigSectionNode atNode)
      {
        Instance.ConfigChanged(atNode);
      }

      public void ConfigChanged(IConfigSectionNode atNode)
      {
        var webSettingsSection = App.ConfigRoot[CONFIG_WEBSETTINGS_SECTION];

        var nSocial = webSettingsSection[CONFIG_SOCIAL_SECTION];
        configSocial(nSocial);

        var webDavSection = webSettingsSection[CONFIG_WEBDAV_SECTION];
        m_WebDavLogType = webDavSection.AttrByName(CONFIG_LOGTYPE_ATTR).ValueAsNullableEnum<MessageType>();
        m_WebDavDefaultTimeoutMs = webDavSection.AttrByName(CONFIG_DEFAULT_TIMEOUT_MS_ATTR).ValueAsInt();
        if (m_WebDavDefaultTimeoutMs < 0) m_WebDavDefaultTimeoutMs = 0;

        var servicePointManagerSection = webSettingsSection[CONFIG_SERVICEPOINTMANAGER_SECTION];
        ((IConfigurable)(ServicePointManagerConfigurator.s_Instance)).Configure(servicePointManagerSection);
      }

    #endregion

    #region Pvt.

      private void configSocial(IConfigSectionNode nSocial)
      {
        var networks = new Registry<SocialNetwork>();
        var nProviders = nSocial.Children.Where(n => n.IsSameName(CONFIG_SOCIAL_PROVIDER_SECTION));

        foreach (var nProv in nProviders)
        {
          var provider = FactoryUtils.MakeAndConfigure<SocialNetwork>(nProv, typeof(SocialNetwork), new object[] { null, nProv});
          networks.Register(provider);
        }

        m_SocialNetworks = networks;
      }

    #endregion
  }
}
