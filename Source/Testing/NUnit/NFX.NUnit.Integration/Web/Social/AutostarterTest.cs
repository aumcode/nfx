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

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.ApplicationModel;
using NFX.Web.Social;

namespace NFX.NUnit.Integration.Web.Social
{
  [TestFixture]
  public class AutostarterTest: ExternalCfg
  {
    [Test]
    public void SocialAutostarted()
    {
      psAutostarted(NFX_SOCIAL_PROVIDER_GP);
      psAutostarted(NFX_SOCIAL_PROVIDER_FB);
      psAutostarted(NFX_SOCIAL_PROVIDER_TWT);
      psAutostarted(NFX_SOCIAL_PROVIDER_LIN);
      psAutostarted(NFX_SOCIAL_PROVIDER_VK);
    }

    private void psAutostarted(string name)
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var sn = SocialNetwork.Instances[name];

        Assert.IsNotNull(sn, name + " wasn't created");
        Assert.AreEqual(name, sn.Name);
      }
    }
  }
}
