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

namespace NFX.ApplicationModel
{

    /// <summary>
    /// Provides access to execution context - an entity that groups Request, Response and Session object.
    /// This class has nothing to do with Web. Execution contexts are supported as easily in console, service, web,  and Windows forms apps.
    ///  All objects may be either application-global or thread level. This class is useful for technology-agnostic implementations
    /// of Record Models i.e. in a Record class one may write:  " if (ExecutionContext.Session.User.Kind==UserKind.Administrator)..."
    /// </summary>
    public static class ExecutionContext
    {
        [ThreadStatic]
        private static object ts_Request;

        [ThreadStatic]
        private static object ts_Response;

        [ThreadStatic]
        private static ISession ts_Session;

        private static IApplication s_Application;
        private static object s_Request;
        private static object s_Response;
        private static ISession s_Session;


        /// <summary>
        /// Returns global application context
        /// </summary>
        public static IApplication Application
        {
          get { return s_Application ?? NOPApplication.Instance; }
        }

        /// <summary>
        /// Returns Request object for current thread, or if it is null, app-global-level object is returned
        /// </summary>
        public static object Request
        {
          get { return ts_Request ?? s_Request; }
        }

        /// <summary>
        /// Returns Response object for current thread, or if it is null, app-global-level object is returned
        /// </summary>
        public static object Response
        {
          get { return ts_Response ?? s_Response; }
        }

        /// <summary>
        /// Returns Session object for current thread, or if it is null, app-global-level object is returned
        /// </summary>
        public static ISession Session
        {
          get { return ts_Session ?? s_Session ?? NOPSession.Instance; }
        }

        /// <summary>
        /// Returns true when thread-level session object is available and not a NOPSession instance
        /// </summary>
        public static bool HasThreadContextSession
        {
          get { return ts_Session != null && ts_Session.GetType()!=typeof(NOPSession); }
        }

        /// <summary>
        /// Internal framework-only method to bind application-level context
        /// </summary>
        public static void __SetApplicationLevelContext(IApplication application, object request, object response, ISession session)
        {
          if (s_Application==null || !(application is NOPApplication))
            s_Application = application;

          s_Request = request;
          s_Response = response;
          s_Session = session;
        }

        /// <summary>
        /// Internal framework-only method to bind thread-level context
        /// </summary>
        public static void __SetThreadLevelContext(object request, object response, ISession session)
        {
          ts_Request = request;
          ts_Response = response;
          ts_Session = session;
        }

        /// <summary>
        /// Internal framework-only method to bind thread-level context
        /// </summary>
        public static void __SetThreadLevelSessionContext(ISession session)
        {
          ts_Session = session;
        }

    }

}
