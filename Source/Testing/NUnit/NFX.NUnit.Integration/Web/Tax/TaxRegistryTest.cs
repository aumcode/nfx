using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Web.Pay.Tax;

namespace NFX.NUnit.Integration.Web.Tax
{
  [TestFixture]
  public class TaxRegistryTest : ExternalCfg
  {
    [Test]
    public void TaxRegistry_01()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        Assert.IsNotNull(TaxCalculator.Instances["NOPCalculator"]);
      }
    }
  }
}
