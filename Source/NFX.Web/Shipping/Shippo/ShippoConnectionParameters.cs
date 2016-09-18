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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Shipping.Shippo
{
  public class ShippoConnectionParameters : ShippingConnectionParameters
  {
    #region ctor

      public ShippoConnectionParameters() : base() { }

      public ShippoConnectionParameters(IConfigSectionNode node) : base(node) { }

      public ShippoConnectionParameters(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
        : base(connStr, format) {}

    #endregion

    public string CarrierID { get; set; }


    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var privateToken = node.AttrByName("private-token").ValueAsString();
      if (privateToken.IsNullOrWhiteSpace())
        User = User.Fake;

      var publicToken = node.AttrByName("public-token").ValueAsString();
      if (publicToken.IsNullOrWhiteSpace())
        User = User.Fake;

      var carrierID = node.AttrByName("carrier-id").ValueAsString();
      if (carrierID.IsNotNullOrWhiteSpace())
        CarrierID = carrierID;

      var cred = new ShippoCredentials(privateToken, publicToken);
      var token = new AuthenticationToken(ShippoSystem.SHIPPO_REALM, null);
      User = new User(cred, token, null, Rights.None);
    }
  }
}
