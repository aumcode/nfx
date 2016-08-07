/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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
    /// Represents PayPal pay session
    /// </summary>
    public class PayPalSession : PaySession
    {
        public PayPalSession(PayPalSystem paySystem, PayPalConnectionParameters cParams)
            : base(paySystem, cParams)
        {
            m_ConnectionParameters = cParams;
        }

        private readonly PayPalConnectionParameters m_ConnectionParameters;
        public PayPalConnectionParameters ConnectionParameters
        {
            get { return m_ConnectionParameters; }
        }

        public PayPalOAuthToken AuthorizationToken
        {
            get
            {
                if (m_User == null || m_User == User.Fake) return null;
                return m_User.AuthToken.Data as PayPalOAuthToken;
            }
        }
    }
}
