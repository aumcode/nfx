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
using System.Threading;

using NFX.Web;
using NFX.Serialization.JSON;
using NFX.Environment;
using NFX.Wave.Templatization;
using ErrorPage=NFX.Wave.Templatization.StockContent.Error;

namespace NFX.Wave.Filters
{
  /// <summary>
  /// Intercepts error that arise during processing and displays an error page for exceptions and error codes
  /// </summary>
  public sealed class ErrorFilter : WorkFilter
  {
    #region CONSTS
      public const string CONFIG_SHOW_DUMP_SECTION = "show-dump";
      public const string CONFIG_LOG_SECTION = "log";

    #endregion

    #region .ctor
      public ErrorFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order)
      {
      }

      public ErrorFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode)
      {
        ConfigureMatches(confNode, m_ShowDumpMatches, m_LogMatches, GetType().FullName);
        ConfigAttribute.Apply(this, confNode);
      }

      public ErrorFilter(WorkHandler handler, string name, int order) : base(handler, name, order)
      {
      }

      public ErrorFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode)
      {
        ConfigureMatches(confNode, m_ShowDumpMatches, m_LogMatches, GetType().FullName);
        ConfigAttribute.Apply(this, confNode);
      }

    #endregion

    #region Fields

      private OrderedRegistry<WorkMatch> m_ShowDumpMatches = new OrderedRegistry<WorkMatch>();
      private OrderedRegistry<WorkMatch> m_LogMatches = new OrderedRegistry<WorkMatch>();

      private string m_SecurityRedirectURL;
      private Type m_CustomErrorPageType;

    #endregion

    #region Properties

      /// <summary>
      /// Returns matches used by the filter to determine whether exception details should be shown
      /// </summary>
      public OrderedRegistry<WorkMatch> ShowDumpMatches { get{ return m_ShowDumpMatches;}}

      /// <summary>
      /// Returns matches used by the filter to determine whether exception details should be logged
      /// </summary>
      public OrderedRegistry<WorkMatch> LogMatches { get{ return m_LogMatches;}}


      /// <summary>
      /// When set redirects response to the specified URL if security exceptions are thrown
      /// </summary>
      [Config]
      public string SecurityRedirectURL
      {
        get{return m_SecurityRedirectURL ?? string.Empty;}
        set{ m_SecurityRedirectURL = value;}
      }

      /// <summary>
      /// Specifies a type for custom error page. Must be WebTemplate-derived type
      /// </summary>
      [Config]
      public string CustomErrorPageType
      {
        get{return m_CustomErrorPageType!=null ? m_CustomErrorPageType.AssemblyQualifiedName : string.Empty ;}
        set
        {
          if (value.IsNullOrWhiteSpace())
          {
            m_CustomErrorPageType = null;
            return;
          }

          try
          {
            var tp = Type.GetType(value, true);
            if (!typeof(WaveTemplate).IsAssignableFrom(tp))
              throw new WaveException("not WaveTemplate");
            m_CustomErrorPageType = tp;
          }
          catch(Exception tErr)
          {
            throw new WaveException(StringConsts.ERROR_PAGE_TEMPLATE_TYPE_ERROR.Args(value, tErr.ToMessageWithType()));
          }

        }
      }

    #endregion


    #region Public

      /// <summary>
      /// Handles the exception by responding appropriately with error page with conditional level of details and logging
      /// </summary>
      public static void HandleException(WorkContext work,
                                         Exception error,
                                         OrderedRegistry<WorkMatch> showDumpMatches,
                                         OrderedRegistry<WorkMatch> logMatches,
                                         string securityRedirectURL = null,
                                         Type customPageType = null
                                         )
      {
          if (work==null || error==null) return;

          var showDump = showDumpMatches != null ?
                         showDumpMatches.OrderedValues.Any(m => m.Make(work)!=null) : false;

          if (work.Response.Buffered)
            work.Response.CancelBuffered();

          var json = false;

          if (work.Request!=null && work.Request.AcceptTypes!=null)//if needed for some edge HttpListener cases when Request or Request.AcceptTypes are null
               json = work.Request.AcceptTypes.Any(at=>at.EqualsIgnoreCase(ContentType.JSON));

          var actual = error;
          if (actual is FilterPipelineException)
            actual = ((FilterPipelineException)actual).RootException;

          if (actual is MVCActionException)
            actual = ((MVCActionException)actual).InnerException;


          var securityError = actual is NFX.Security.AuthorizationException || actual.InnerException is NFX.Security.AuthorizationException;

          if (actual is HTTPStatusException)
          {
            var se = (HTTPStatusException)actual;
            work.Response.StatusCode = se.StatusCode;
            work.Response.StatusDescription = se.StatusDescription;
          }
          else
          {
            if (securityError)
            {
              work.Response.StatusCode = WebConsts.STATUS_403;
              work.Response.StatusDescription = WebConsts.STATUS_403_DESCRIPTION;
            }
            else
            {
              work.Response.StatusCode = WebConsts.STATUS_500;
              work.Response.StatusDescription = WebConsts.STATUS_500_DESCRIPTION;
            }
          }


          if (json)
          {
             work.Response.ContentType = ContentType.JSON;
             work.Response.WriteJSON(error.ToClientResponseJSONMap(showDump));
          }
          else
          {
            if (securityRedirectURL.IsNotNullOrWhiteSpace() && securityError)
              work.Response.RedirectAndAbort(securityRedirectURL);
            else
            {
              WaveTemplate errorPage = null;

              if (customPageType!=null)
                try
                {
                  errorPage = Activator.CreateInstance(customPageType) as WaveTemplate;
                  if (errorPage==null) throw new WaveException("not WaveTemplate");
                }
                catch(Exception actErr)
                {
                  work.Log(Log.MessageType.Error,
                            StringConsts.ERROR_PAGE_TEMPLATE_TYPE_ERROR.Args(customPageType.FullName, actErr.ToMessageWithType()),
                            typeof(ErrorFilter).FullName+".ctor(customPageType)",
                            actErr);
                }

              if (errorPage==null)
              {
                errorPage =  new ErrorPage(work, error, showDump);
                errorPage.Render(work, error);
              }
              else
                errorPage.Render(work, actual);
            }
          }

          if (logMatches!=null && logMatches.Count>0)
          {
            JSONDataMap matched = null;
            foreach(var match in logMatches.OrderedValues)
            {
              matched = match.Make(work);
              if (matched!=null) break;
            }
            if (matched!=null)
              work.Log(Log.MessageType.Error, error.ToMessageWithType(), typeof(ErrorFilter).FullName, pars: matched.ToJSON(JSONWritingOptions.CompactASCII));
          }

      }


    #endregion

    #region Protected

      internal static void ConfigureMatches(IConfigSectionNode confNode,
                                            OrderedRegistry<WorkMatch> showDumpMatches,
                                            OrderedRegistry<WorkMatch> logMatches,
                                            string from)
      {
        foreach(var cn in confNode[CONFIG_SHOW_DUMP_SECTION].Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!showDumpMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "{0}.ShowDump".Args(from)));

        foreach(var cn in confNode[CONFIG_LOG_SECTION].Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!logMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "{0}.Log".Args(from)));
      }

      protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
        try
        {
          this.InvokeNextWorker(work, filters, thisFilterIndex);
        }
        catch(Exception error)
        {
          HandleException(work, error, m_ShowDumpMatches, m_LogMatches, m_SecurityRedirectURL, m_CustomErrorPageType);
          if (Server.m_InstrumentationEnabled)
               Interlocked.Increment(ref Server.m_Stat_FilterHandleException);
        }
       }

    #endregion
  }

}
