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

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.NUnit.Integration.IO;
using NFX.Web.Pay;

namespace NFX.NUnit.Integration.Web.Pay
{
  [TestFixture]
  public class AutostarterTest: ExternalCfg
  {
    [Test]
    public void StripeAutostarted()
    {
      psAutostarted("Stripe");
    }

    [Test]
    public void MockAutostarted()
    {
      psAutostarted("Mock");
    }

    private void psAutostarted(string name)
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var ps = PaySystem.Instances[name];

        Assert.IsNotNull(ps, name + " wasn't created");
        Assert.AreEqual(name, ps.Name);
      }
    }

  }
}
