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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Wave.Filters
{
  /// <summary>
  /// Manages injection of portal into the work context
  /// </summary>
  public class PortalFilter : WorkFilter
  {
    #region CONSTS
      public const string VAR_PORTAL_NAME = "portal-name";
      public const string CONF_THEME_COOKIE_NAME_ATTR = "theme-cookie-name";
      public const string CONF_USE_THEME_COOKIE_ATTR = "use-theme-cookie";
      public const string DEFAULT_THEME_COOKIE_NAME = "UIT";
    #endregion

    #region .ctor
      public PortalFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public PortalFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) {ctor(confNode);}
      public PortalFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
      public PortalFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) {ctor(confNode);}

      private void ctor(IConfigSectionNode confNode)
      {

        m_UseThemeCookie = confNode.AttrByName(CONF_USE_THEME_COOKIE_ATTR).ValueAsBool(true);
        m_ThemeCookieName = confNode.AttrByName(CONF_THEME_COOKIE_NAME_ATTR).ValueAsString(DEFAULT_THEME_COOKIE_NAME);

        //read matches
        foreach(var cn in confNode.Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!m_PortalMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "{0}".Args(GetType().FullName)));
      }

    #endregion

    #region Fields

     private OrderedRegistry<WorkMatch> m_PortalMatches = new OrderedRegistry<WorkMatch>();

     private bool m_UseThemeCookie;
     private string m_ThemeCookieName = DEFAULT_THEME_COOKIE_NAME;

    #endregion

    #region Properties

      /// <summary>
      /// OrderedRegistry of matches used by the filter to determine whether work should match a portal
      /// </summary>
      public OrderedRegistry<WorkMatch> PortalMatches { get{ return m_PortalMatches;}}


      /// <summary>
      /// Specifies true to interpret ThemeCookieName
      /// </summary>
      public bool UseThemeCookie
      {
        get { return m_UseThemeCookie;}
        set { m_UseThemeCookie = value; }
      }

      /// <summary>
      /// Specifies theme cookie name
      /// </summary>
      public string ThemeCookieName
      {
        get { return m_ThemeCookieName ?? DEFAULT_THEME_COOKIE_NAME;}
        set { m_ThemeCookieName = value; }
      }
    #endregion

    #region Protected

      protected sealed override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
        if (work.m_PortalFilter==null)
        {
          try
          {
            work.m_PortalFilter = this;

            foreach(var match in m_PortalMatches.OrderedValues)
            {
              var matched = match.Make(work);
              if (matched!=null)
              {
                var portalName = matched[VAR_PORTAL_NAME].AsString();
                if (portalName.IsNotNullOrWhiteSpace())
                {
                  var portal = PortalHub.Instance.Portals[portalName];
                  if (portal!=null && !portal.Offline)
                  {
                    work.m_Portal = portal;
                    work.m_PortalMatch = match;
                    work.m_PortalMatchedVars = matched;
                  }
                  break;
                }
              }
            }

            if (work.m_Portal==null)
            {
              var defaultPortal = PortalHub.Instance.DefaultOnline;
              if (defaultPortal!=null)
              {
                 work.m_Portal = defaultPortal;
              }
            }

            if (m_UseThemeCookie && work.m_Portal!=null)
            {
              //Use regular cookies so client JS can set it up
              var tcv = work.Request.Cookies[m_ThemeCookieName];//work.Response.GetClientVar(m_ThemeCookieName);
              if (tcv!=null && tcv.Value.IsNotNullOrWhiteSpace())
              {
                var theme = work.m_Portal.Themes[tcv.Value];
                if (theme!=null)
                 work.m_PortalTheme = theme;
              }
            }

            if (Server.m_InstrumentationEnabled &&
                work.m_Portal!=null &&
                work.m_Portal.InstrumentationEnabled)
            {
              Server.m_Stat_PortalRequest.IncrementLong(work.m_Portal.Name);
            }

            this.InvokeNextWorker(work, filters, thisFilterIndex);
          }
          finally
          {
            work.m_PortalFilter = null;
            work.m_Portal = null;
            work.m_PortalMatch = null;
            work.m_PortalMatchedVars = null;
            work.PortalTheme = null;
          }
        }
        else this.InvokeNextWorker(work, filters, thisFilterIndex);
      }

    #endregion


  }

}
