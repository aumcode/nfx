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
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.Mock
{
  public class MockConnectionParameters: PayConnectionParameters
  {
    #region Consts

      public const string MOCK_REALM = "MOCK_PAY_REALM";

      public const string CONFIG_ACCOUNTS_SECTION = "accounts";
      public const string CONFIG_ACCOUNT_ACTUAL_DATA_NODE = "account-actual-data";

      public const string CONFIG_EMAIL_ATTR = "email";

      public const string CONFIG_CARD_NUMBER_ATTR = "number";
      public const string CONFIG_CARD_EXPYEAR_ATTR = "exp-year";
      public const string CONFIG_CARD_EXPMONTH_ATTR = "exp-month";
      public const string CONFIG_CARD_CVC_ATTR = "cvc";

      public const string CONFIG_ACCOUNT_NUMBER_ATTR = "account-number";
      public const string CONFIG_ROUTING_NUMBER_ATTR = "routing-number";
      public const string CONFIG_ACCOUNT_TYPE_ATTR = "account-type";

    #endregion

    public MockConnectionParameters(): base() {}
    public MockConnectionParameters(IConfigSectionNode node): base(node) {}
    public MockConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) {}

    //private MockActualAccountData[] m_AccountActualDatas;

    //public IEnumerable<MockActualAccountData> AccountActualDatas { get { return m_AccountActualDatas; } }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;
      var cred = new MockCredentials(email);
      var at = new AuthenticationToken(MOCK_REALM, email);

      User = new User(cred, at, email, Rights.None);

      //var nAccounts = node[CONFIG_ACCOUNTS_SECTION];
      //configureAccounts(nAccounts);
    }

    //private void configureAccounts(IConfigSectionNode nAccountActualDatas)
    //{
    //  var accountActualDatas = new List<MockActualAccountData>();

    //  foreach (var accountActualDataConf in nAccountActualDatas.Children.Where(c => c.IsSameName(CONFIG_ACCOUNT_ACTUAL_DATA_NODE)))
    //  {
    //    var actualAccountData = MockActualAccountData.MakeAndConfigure(accountActualDataConf);

    //    accountActualDatas.Add(actualAccountData);
    //  }

    //  m_AccountActualDatas = accountActualDatas.ToArray();
    //}

  } //MockConnectionParameters

}
