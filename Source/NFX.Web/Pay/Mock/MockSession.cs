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
using System.Threading.Tasks;

namespace NFX.Web.Pay.Mock
{
  public class MockSession: PaySession
  {
    public MockSession(PaySystem paySystem, MockConnectionParameters cParams)
      : base(paySystem, cParams)
    {
      //AccountActualDatas = cParams.AccountActualDatas;
    }

    public string Email
    {
      get
      {
        if (m_User == null || m_User == NFX.Security.User.Fake) return string.Empty;
        var cred = m_User.Credentials as MockCredentials;
        if (cred == null) return string.Empty;
        return cred.Email;
      }
    }

    //public readonly IEnumerable<MockActualAccountData> AccountActualDatas;
  }
}
