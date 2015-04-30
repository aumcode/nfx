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

using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.Stripe
{
  public class StripeConnectionParameters: PayConnectionParameters
  {
    public const string STRIPE_REALM = "stripe";

    public const string CONFIG_EMAIL_ATTR = "email";
    public const string CONFIG_SECRETKEY_ATTR = "secret-key";
    public const string CONFIG_PUBLISHABLEKEY_ATTR = "publishable-key";

    public StripeConnectionParameters(): base() {}
    public StripeConnectionParameters(IConfigSectionNode node): base(node) {}
    public StripeConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) {}

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;
      var secretKey = node.AttrByName(CONFIG_SECRETKEY_ATTR).Value;
      var publishableKey = node.AttrByName(CONFIG_PUBLISHABLEKEY_ATTR).Value;

      var cred = new StripeCredentials(email, secretKey, publishableKey);
      var at = new AuthenticationToken(STRIPE_REALM, publishableKey);

      User = new User(cred, at, UserStatus.User, publishableKey, publishableKey, Rights.None);
    }

  } //StripeSystemSessionParameters

}
