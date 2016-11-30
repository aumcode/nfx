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
using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.PayPal
{
  public class PayPalConnectionParameters : ConnectionParameters
  {
    #region .ctor
    public PayPalConnectionParameters() : base() { }
    public PayPalConnectionParameters(IConfigSectionNode node) : base(node) { }
    public PayPalConnectionParameters(string connectionString, string format = Configuration.CONFIG_LACONIC_FORMAT)
        : base(connectionString, format) { }
    #endregion

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      User = User.Fake;

      var clientID = node.AttrByName("client-id").Value;
      if (clientID.IsNullOrWhiteSpace()) return;
      var clientSecret = node.AttrByName("client-secret").Value;
      if (clientSecret.IsNullOrWhiteSpace()) return;

      var token = new AuthenticationToken(PayPalSystem.PAYPAL_REALM, null); // OAuth token is empty at start
      User = new User(new PayPalCredentials(clientID, clientSecret), token, string.Empty, Rights.None);
    }
  }
}
