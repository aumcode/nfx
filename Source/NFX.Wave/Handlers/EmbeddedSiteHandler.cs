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
using System.IO;
using System.Net;

using NFX.Web;
using NFX.Environment;
using NFX.Wave.Templatization;

namespace NFX.Wave.Handlers
{
  /// <summary>
  /// Implements handler that serves content from assembly-embedded resources and class actions.
  /// Inherit from this class to implement actual handler that serves from particular assembly/namespace root
  /// </summary>
  public abstract class EmbeddedSiteHandler : WorkHandler
  {
     #region CONSTS

           public const string SITE_ACTION = "site";
           public const string DEFAULT_SITE_PATH = "Home.htm";

           public const string VAR_PATH = "path";

           public const int DEFAULT_CACHE_MAX_AGE_SEC = 24  * //hrs
                                                        60  * //min
                                                        60;   //sec

     #endregion

     #region Inner Classes
          /// <summary>
          /// Represents an action that can be dispatched by a EmbeddedSiteHandler.
          /// The instance of this interface implementor is shared between requests (must be thread-safe)
          /// </summary>
          public interface IAction : INamed
          {
            /// <summary>
            /// Performs the action - by performing action work
            /// </summary>
            void Perform(WorkContext context);
          }


     #endregion


     #region .ctor

      protected EmbeddedSiteHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                          : base(dispatcher, name, order, match)
      {
         foreach(var action in GetActions()) m_Actions.Register(action);
      }

      protected EmbeddedSiteHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
      {
         ConfigAttribute.Apply(this, confNode);
         foreach(var action in GetActions()) m_Actions.Register(action);
      }

     #endregion

     #region Fields

      private Registry<IAction> m_Actions = new Registry<IAction>();

      [Config(Default=DEFAULT_CACHE_MAX_AGE_SEC)]
      private int m_CacheMaxAgeSec = DEFAULT_CACHE_MAX_AGE_SEC;

      [Config]
      private string m_VersionSegmentPrefix;

     #endregion

     #region Properties

        /// <summary>
        /// Returns actions that this site can perform
        /// </summary>
        public IRegistry<IAction> Actions { get { return m_Actions;} }

        /// <summary>
        /// Returns name for action that serves embedded site
        /// </summary>
        public virtual string SiteAction { get { return SITE_ACTION;}}

        /// <summary>
        /// Returns default site path that serves site root
        /// </summary>
        public virtual string DefaultSitePath { get { return DEFAULT_SITE_PATH;}}

        /// <summary>
        /// Returns resource path root, i.e. namespace prefixes where resources reside
        /// </summary>
        public abstract string RootResourcePath{ get; }


        /// <summary>
        /// Override in sites that do not support named environment sub-folders in resource structure.
        /// True by default
        /// </summary>
        public virtual bool SupportsEnvironmentBranching
        {
          get { return true;}
        }

        /// <summary>
        /// When set indicates the case-insensitive prefix of a path segment that should be ignored by the handler path resolver.
        /// Version prefixes are used for attaching a surrogate path "folder" that makes resource differ based on their content.
        /// For example when prefix is "@",  path '/embedded/img/@767868768768/picture.png' resolves to actual '/embedded/img/picture.png'
        /// </summary>
        public string VersionSegmentPrefix
        {
          get { return m_VersionSegmentPrefix;}
          set { m_VersionSegmentPrefix = value;}
        }

        /// <summary>
        /// Specifies the maximum age in cache in seconds for resource file content. Zero means - do not cache the file
        /// </summary>
        public int CacheMaxAgeSec
        {
           get {return m_CacheMaxAgeSec;}
           set {m_CacheMaxAgeSec = value<0 ? 0 : value;}
        }

     #endregion


     #region Protected

      /// <summary>
      /// Override to declare what actions this site can perform
      /// </summary>
      /// <returns></returns>
      protected abstract IEnumerable<IAction> GetActions();

      /// <summary>
      /// Override to set specific cache header set per resource name
      /// </summary>
      protected virtual void SetResourceCacheHeader(WorkContext work, string sitePath, string resName)
      {
         if (m_CacheMaxAgeSec>0)
          work.Response.Headers[HttpResponseHeader.CacheControl] = "private, max-age={0}, must-revalidate".Args(m_CacheMaxAgeSec);
         else
          work.Response.Headers[HttpResponseHeader.CacheControl] = "no-cache";
      }


      protected override void DoHandleWork(WorkContext work)
      {
        string path;
        var action = ParseWork(work, out path);

        DispatchAction(work, action, path);
      }

            /// <summary>
            /// Override to extract action and path form request
            /// </summary>
            protected virtual string ParseWork(WorkContext work, out string path)
            {
              path = string.Empty;
              var fullPath = work.MatchedVars[VAR_PATH].AsString();
              if (fullPath.IsNullOrWhiteSpace()) return SiteAction;

              string action = null;

              var segs = fullPath.Split(DELIMS);

              if(segs.Length>1)
              {
                action = segs[0];
                path = string.Join("/",segs, 1, segs.Length-1);
              }
              else
               if (segs.Length==1) action=segs[0];

              if (action.IsNullOrWhiteSpace())
                action = SiteAction;

              return action;
            }


            /// <summary>
            /// Dispatched appropriate action
            /// </summary>
            protected virtual void DispatchAction(WorkContext work, string action, string path)
            {
              if (string.Equals(SiteAction, action, StringComparison.InvariantCultureIgnoreCase))
                serveSite(work, path);
               else
                DispatchNonSiteAction(work, action);
            }

            /// <summary>
            /// Dispatches an action which is not a site-serving one
            /// </summary>
            protected virtual void DispatchNonSiteAction(WorkContext context, string action)
            {
              var actionInstance = m_Actions[action];
              if (actionInstance!=null)
                   actionInstance.Perform(context);
              else
              {
                   context.Response.StatusCode = WebConsts.STATUS_500;
                   context.Response.StatusDescription = WebConsts.STATUS_500_DESCRIPTION;
                   context.Response.Write(StringConsts.DONT_KNOW_ACTION_ERROR + action);
              }
            }
     #endregion


     #region .pvt


            private char[] DELIMS = new char[]{'/','\\'};

            private void serveSite(WorkContext work, string sitePath)
            {
              if (sitePath.IsNullOrWhiteSpace())
                sitePath = DefaultSitePath;

              var assembly = this.GetType().Assembly;

              //Cut the surrogate out of path, i.e. '/static/img/@@767868768768/picture.png' -> '/static/img/picture.png'
              sitePath = FileDownloadHandler.CutVersionSegment(sitePath, m_VersionSegmentPrefix);

              var resName = getResourcePath(sitePath);



              using (Stream strm = assembly.GetManifestResourceStream(resName))
               if (strm!=null)
               {
                 work.Response.ContentType = mapContentType(resName);
                 SetResourceCacheHeader(work, sitePath, resName);
                 work.Response.WriteStream(strm);
               }
               else throw new HTTPStatusException(WebConsts.STATUS_404, WebConsts.STATUS_404_DESCRIPTION, StringConsts.NOT_FOUND_ERROR + resName);
            }

            private string getResourcePath(string sitePath)
            {
              var root = RootResourcePath;
              if (!root.EndsWith("."))
                root += '.';

              if (SupportsEnvironmentBranching)
              {
                var envp = this.Dispatcher.ComponentDirector.EnvironmentName;

                if (envp.IsNotNullOrWhiteSpace())
                {
                  root += envp;
                  if (!root.EndsWith("."))
                  root += '.';
                }
              }

              //adjust namespace names
              string[] segments = sitePath.Split(DELIMS);
              for(var i=0; i<segments.Length-1; i++) //not the resource name
                segments[i] = segments[i].Replace('-','_').Replace(' ','_');


              var result = new StringBuilder();
              var first = true;
              foreach(var seg in segments)
              {
                if (!first)
                  result.Append('.');
                else
                  first = false;
                result.Append(seg);
              }

              return root + result;
            }

            private string mapContentType(string res)
            {
              if (res==null)
                 return ContentType.HTML;

              var i = res.LastIndexOf('.');

              if (i<0 || i>res.Length-1)
                 return ContentType.HTML;

              var ext = res.Substring(i+1);

              return ContentType.ExtensionToContentType(ext);
            }

     #endregion

  }
}
