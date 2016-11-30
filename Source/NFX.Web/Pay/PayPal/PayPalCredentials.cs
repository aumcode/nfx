/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using NFX.Security;

namespace NFX.Web.Pay.PayPal
{
    /// <summary>
    /// Represents basic PayPal credentials for registered application
    /// which include Business account email, client ID and client secret.
    /// </summary>
    public class PayPalCredentials : Credentials
    {
        public PayPalCredentials(string clientID, string clientSecret)
        {
            m_ClientID = clientID;
            m_ClientSecret = clientSecret;
        }

        private readonly string m_ClientID;
        private readonly string m_ClientSecret;

        public string ClientID { get { return m_ClientID; } }
        public string ClientSecret { get { return m_ClientSecret; } }
    }
}
