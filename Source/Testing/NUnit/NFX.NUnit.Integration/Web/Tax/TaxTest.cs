using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Web.Pay.Tax;
using NFX.Financial;
using NFX.Web.Pay;
using NFX.Web.Pay.Tax.TaxJar;

namespace NFX.NUnit.Integration.Web.Tax
{
  [TestFixture]
  public class TaxTest : ExternalCfg
  {
    public static readonly IAddress IL_ADDRESS = new Address()
                                                            {
                                                              Address1 = "5844 South Oak Street",
                                                              Address2 = "1234 Lemon Street",
                                                              Country = "US",
                                                              City = "Chicago",
                                                              PostalCode = "60667",
                                                              Region = "IL",
                                                              EMail = "wholeseller@gmail.com",
                                                              Phone = "(309) 123-4567"
                                                            };

    public static readonly IAddress CA_ADDRESS = new Address()
                                                            {
                                                              Address1 = "30 Mortensen Avenue",
                                                              Address2 = "",
                                                              Country = "US",
                                                              City = "Salinas",
                                                              PostalCode = "93905",
                                                              Region = "CA",
                                                              EMail = "retailer@gmail.com",
                                                              Phone = "(831) 758-7214"
                                                            };

    public static readonly IAddress TX_ADDRESS = new Address()
                                                            {
                                                              Address1 = "711-2880 Nulla St.",
                                                              Address2 = null,
                                                              Country = "US",
                                                              City = "Mankato Mississippi",
                                                              PostalCode = "96522",
                                                              Region = "TX",
                                                              EMail = "shipper@gmail.com",
                                                              Phone = "(257) 563-7401"
                                                            };

    [Test]
    public void Tax01_NOPRegistry()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        Assert.IsNotNull(nop);
        Assert.IsNotNull(nop.DefaultSesssionConnectParamsCfg);
      }
    }

    [Test]
    public void Tax02_NOPSession()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          Assert.AreEqual("tax_nop_user@mail.com", sess.User.Name);
        }
      }
    }

    [Test]
    public void Tax03_NOPCalc_ILCATX()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          var result = sess.Calc(IL_ADDRESS, new [] {"IL"}, CA_ADDRESS, new [] {"CA"}, new string[] {}, TX_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsTrue(result.IsNone);
        }
      }
    }

    [Test]
    public void Tax04_NOPCalc_ILCACA()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          var result = sess.Calc(IL_ADDRESS, new [] {"IL"}, CA_ADDRESS, new [] {"CA"}, new string[] {}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsFalse(result.IsNone);
          Assert.AreEqual("CA", result.Recipient);
          Assert.IsNotNull(result.RetailerTax);
          Assert.IsNull(result.WholesellerTax);
        }
      }
    }

    [Test]
    public void Tax05_NOPCalc_CAILCA_NoCAResaleCertificate()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          var result = sess.Calc(CA_ADDRESS, new [] {"CA"}, IL_ADDRESS, new [] {"IL"}, new string[] {}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsFalse(result.IsNone);
          Assert.AreEqual("CA", result.Recipient);
          Assert.IsNull(result.RetailerTax);
          Assert.IsNotNull(result.WholesellerTax);
        }
      }
    }

    [Test]
    public void Tax06_NOPCalc_CAILCA_CAResaleCertificate()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          var result = sess.Calc(CA_ADDRESS, new [] {"CA"}, IL_ADDRESS, new [] {"IL"}, new string[] {"CA"}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsTrue(result.IsNone);
        }
      }
    }

    [Test]
    public void Tax07_NOPCalc_CAILCA_CAResaleCertificate_RetailerNexusInCA()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          var result = sess.Calc(CA_ADDRESS, new [] {"CA"}, IL_ADDRESS, new [] {"IL", "CA"}, new string[] {"CA"}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsFalse(result.IsNone);
          Assert.AreEqual("CA", result.Recipient);
          Assert.IsNotNull(result.RetailerTax);
          Assert.IsNull(result.WholesellerTax);
        }
      }
    }

    [Test]
    public void Tax08_NOPCalc_CACACA_NoCAResaleCertificate()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          var result = sess.Calc(CA_ADDRESS, new [] {"CA"}, CA_ADDRESS, new [] {"CA"}, new string[] {}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsFalse(result.IsNone);
          Assert.AreEqual("CA", result.Recipient);
          Assert.IsNotNull(result.RetailerTax);
          Assert.IsNotNull(result.WholesellerTax);
        }
      }
    }

    [Test]
    public void Tax09_NOPCalc_CACACA_CAResaleCertificate()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var nop = TaxCalculator.Instances["NOPCalculator"];
        using (var sess = nop.StartSession())
        {
          var result = sess.Calc(CA_ADDRESS, new [] {"CA"}, CA_ADDRESS, new [] {"CA"}, new string[] {"CA"}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsFalse(result.IsNone);
          Assert.AreEqual("CA", result.Recipient);
          Assert.IsNotNull(result.RetailerTax);
          Assert.IsNull(result.WholesellerTax);
        }
      }
    }

    [Test]
    public void Tax21_TaxJarRegistry()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var tj = TaxCalculator.Instances["TaxJar"];
        Assert.IsNotNull(tj);
        Assert.IsNotNull(tj.DefaultSesssionConnectParamsCfg);
      }
    }

    [Test]
    public void Tax22_TaxJarCalc_ILCACA()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var tj = TaxCalculator.Instances["TaxJar"];
        using (var sess = tj.StartSession())
        {
          var result = sess.Calc(IL_ADDRESS, new [] {"IL"}, CA_ADDRESS, new [] {"CA"}, new string[] {}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
          Assert.NotNull(result);
          Assert.IsFalse(result.IsNone);
          Assert.AreEqual("CA", result.Recipient);
          Assert.IsNotNull(result.RetailerTax);
          Assert.IsTrue(0 < result.RetailerTax.Total);
          Assert.IsNull(result.WholesellerTax);
        }
      }
    }

    [Test]
    [ExpectedException(typeof(TaxException))]
    public void Tax23_TaxJarCalc_WrongApiKey()
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var tj = TaxCalculator.Instances["TaxJar"];
        var sessionPrms = new TaxJarConnectionParameters() { ApiKey = "" };
        using (var sess = tj.StartSession(sessionPrms))
        {
          sess.Calc(IL_ADDRESS, new [] {"IL"}, CA_ADDRESS, new [] {"CA"}, new string[] {}, CA_ADDRESS, new Amount("usd", 30M), new Amount("usd", 50M), new Amount("usd", 0M));
        }
      }
    }
  }
}
