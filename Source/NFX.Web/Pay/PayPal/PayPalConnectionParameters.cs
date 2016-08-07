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
using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.PayPal
{
    public class PayPalConnectionParameters : PayConnectionParameters
    {
        public const string CFG_EMAIL = "email";
        public const string CFG_CLIENT_ID = "client-id";
        public const string CFG_CLIENT_SECRET = "client-secret";

        #region .ctor

        public PayPalConnectionParameters() : base()
        {
        }

        public PayPalConnectionParameters(IConfigSectionNode node) : base(node)
        {
        }

        public PayPalConnectionParameters(string connectionString, string format = Configuration.CONFIG_LACONIC_FORMAT)
            : base(connectionString, format)
        {
        }

        #endregion

        public override void Configure(IConfigSectionNode node)
        {
            base.Configure(node);

            var email = node.AttrByName(CFG_EMAIL).Value;
            var clientID = node.AttrByName(CFG_CLIENT_ID).Value;
            var clientSecret = node.AttrByName(CFG_CLIENT_SECRET).Value;

            var credentials = new PayPalCredentials(email, clientID, clientSecret);
            var token = new AuthenticationToken(PayPalSystem.PAYPAL_REALM, null); // OAuth token is empty at start
            User = new User(credentials, token, email, Rights.None);
        }
    }
}
