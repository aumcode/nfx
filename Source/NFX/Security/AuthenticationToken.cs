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

namespace NFX.Security
{

    /// <summary>
    /// Represents security provider-internal ID that SecurityManager assigns into User object on authentication.
    /// These tokens can be used in place of Credentials to re-authenticate users or to requery user rights.
    /// External parties should never be supplied with this struct as it is backend-internal
    /// </summary>
    [Serializable]
    public struct AuthenticationToken
    {

        public AuthenticationToken(string realm, object data)
        {
           m_Realm = realm;
           m_Data = data;
        }

        private string m_Realm;
        private object m_Data;


        /// <summary>
        /// Provides information about back-end security source (realm) that perfomed authentication, i.e. LDAP instance, Database name etc...
        /// </summary>
        public string Realm
        {
          get {return m_Realm;}
        }

        /// <summary>
        /// Provides provider-specific key/id that uniquely identifies the user in the realm
        /// </summary>
        public object Data
        {
          get { return m_Data; }
        }

        public override string ToString()
        {
          return "AuthToken({0}::{1})".Args(m_Realm, m_Data);
        }

    }
}
