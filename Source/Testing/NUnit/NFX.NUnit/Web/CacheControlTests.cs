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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.Web;
using NFX.Web.Social;
using NFX.Wave.MVC;

namespace NFX.NUnit.Web
{
  [TestFixture]
  public class CacheControlTests
  {
    [Test]
    public void CacheControlConfig()
    {
      var conf = @"
cache-control {
  cacheability=public
  max-age-sec=60
  shared-max-age-sec=120

  no-store=true
  no-transform=true
  must-revalidate=true
  proxy-revalidate=true
}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      var cc = CacheControl.FromConfig(conf);

      Assert.AreEqual(CacheControl.Type.Public, cc.Cacheability);
      Assert.AreEqual(60, cc.MaxAgeSec);
      Assert.AreEqual(120, cc.SharedMaxAgeSec);
      Assert.IsTrue(cc.NoStore);
      Assert.IsTrue(cc.NoTransform);
      Assert.IsTrue(cc.MustRevalidate);
      Assert.IsTrue(cc.ProxyRevalidate);

      Console.WriteLine(cc.HTTPCacheControl);
      Assert.IsTrue(cc.HTTPCacheControl.EqualsOrdIgnoreCase("public, no-transform, max-age=60, s-maxage=120, must-revalidate, proxy-revalidate"));
    }
  }
}
