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

namespace NFX.Erlang
{
    /// <summary>
    /// Provides non-localizable Erlang constants
    /// </summary>
    public static class ErlConsts
    {
        internal const string CONFIG_IS_LOCAL_ATTR = "is-local";

        internal const string ERLANG_ACCEPT_ATTR        = "accept";
        internal const string ERLANG_ADDRESS_ATTR       = "address";
        internal const string ERLANG_CONFIG_SECTION     = "erlang";
        internal const string ERLANG_CONFIG_TRANSPORT_SECTION = "transport";
        internal const string ERLANG_COOKIE_ATTR        = "cookie";
        internal const string ERLANG_CREATION_FILE_ATTR = "creation-file";
        internal const string ERLANG_CONN_TIMEOUT_ATTR  = "connect-timeout-msec";
        internal const string ERLANG_NODE_SECTION       = "node";
        internal const string ERLANG_SHORT_NAME_ATTR    = "short-name";
        internal const string ERLANG_CONNECT_ON_STARUP  = "connect-on-startup";

        public const string ANY       = "_";
        public const string TRUE      = "true";
        public const string FALSE     = "false";
        public const string UNDEFINED = "undefined";

        /// <summary>
        /// The largest value that can be encoded as an integer
        /// </summary>
        public static readonly int ERL_INT_MAX = (1 << 27) - 1;

        /// <summary>
        /// The smallest value that can be encoded as an integer
        /// </summary>
        public static readonly int ERL_INT_MIN = -(1 << 27);
    }
}
