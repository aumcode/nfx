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

using NFX.Security;

namespace NFX.Glue.Protocol
{
    /// <summary>
    /// Marshalls user authentication information
    /// </summary>
    [Serializable]
    public sealed class AuthenticationHeader : Header
    {
        private AuthenticationToken m_Token;
        private Credentials m_Credentials;

        /// <summary>
        /// Returns AuthenticationToken
        /// </summary>
        public AuthenticationToken Token { get { return m_Token;} }


        /// <summary>
        /// Returns Credentials
        /// </summary>
        public Credentials Credentials { get { return m_Credentials;} }


        public AuthenticationHeader(AuthenticationToken token)
        {
            m_Token = token;
        }

        /// <summary>
        /// Inits header with Credentials instance.
        /// Note: passing IDPasswordCredentials over public wire is not a good practice,
        /// pass AuthenticationToken instead
        /// </summary>
        public AuthenticationHeader(Credentials credentials)
        {
            m_Credentials = credentials;
        }

    }
}
