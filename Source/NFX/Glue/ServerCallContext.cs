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

using NFX.Glue.Protocol;

namespace NFX.Glue
{

    /// <summary>
    /// Provides access to server call context. Use to access Headers
    /// </summary>
    public static class ServerCallContext
    {
        [ThreadStatic]
        private static RequestMsg ts_Request;

        [ThreadStatic]
        private static Headers ts_ResponseHeaders;


        /// <summary>
        /// Returns RequestMsg which is being processed. Access incoming headers through Request.Headers
        /// </summary>
        public static RequestMsg Request
        {
          get { return ts_Request; }
        }

        /// <summary>
        /// Returns Headers instance that will be appended to response
        /// </summary>
        public static Headers ResponseHeaders
        {
          get
          {
            if (ts_ResponseHeaders==null) ts_ResponseHeaders = new Headers();
            return ts_ResponseHeaders;
          }
        }



        /// <summary>
        /// Internal framework-only method to bind thread-level context
        /// </summary>
        public static void __SetThreadLevelContext(RequestMsg request)
        {
          ts_Request = request;
        }

        /// <summary>
        /// Internal framework-only method to clear thread-level context
        /// </summary>
        public static void __ResetThreadLevelContext()
        {
          ts_Request = null;
          ts_ResponseHeaders = null;
        }

        public static Headers GetResponseHeadersOrNull()
        {
          return ts_ResponseHeaders;
        }

    }

}
