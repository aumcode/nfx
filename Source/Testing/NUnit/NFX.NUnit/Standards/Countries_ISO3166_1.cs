using NFX.Standards;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.NUnit.Standards
{
  [TestFixture]
  class Countries_ISO3166_1_Test
  {
    [TestCase]
    public void ToAlpha2()
    {
      Assert.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.ToAlpha2("___")));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2("___", "US").EqualsOrdIgnoreCase("US"));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase("US"));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_RUSSIA).EqualsOrdIgnoreCase("RU"));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_GERMANY).EqualsOrdIgnoreCase("DE"));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_MEXICO).EqualsOrdIgnoreCase("MX"));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_CANADA).EqualsOrdIgnoreCase("CA"));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_FRANCE).EqualsOrdIgnoreCase("FR"));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha2(null).EqualsOrdIgnoreCase(null));
    }
    [TestCase]
    public void ToAlpha3()
    {
      Assert.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.ToAlpha3("__")));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3("__", CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3("US").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3("RU").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_RUSSIA));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3("DE").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_GERMANY));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3("MX").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_MEXICO));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3("CA").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_CANADA));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3("FR").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_FRANCE));
      Assert.IsTrue(Countries_ISO3166_1.ToAlpha3(null).EqualsOrdIgnoreCase(null));
    }
    [TestCase]
    public void Normalize2()
    {
      Assert.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.Normalize2("___")));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2("__", "US").EqualsOrdIgnoreCase("US"));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2("US").EqualsOrdIgnoreCase("US"));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2("RU").EqualsOrdIgnoreCase("RU"));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2("DE").EqualsOrdIgnoreCase("DE"));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2("MX").EqualsOrdIgnoreCase("MX"));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2("CA").EqualsOrdIgnoreCase("CA"));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2("FR").EqualsOrdIgnoreCase("FR"));
      Assert.IsTrue(Countries_ISO3166_1.Normalize2(null).EqualsOrdIgnoreCase(null));
    }
    [TestCase]
    public void Normalize3()
    {
      Assert.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.Normalize3("__")));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3("___", CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_RUSSIA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_RUSSIA));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_GERMANY).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_GERMANY));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_MEXICO).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_MEXICO));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_CANADA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_CANADA));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_FRANCE).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_FRANCE));
      Assert.IsTrue(Countries_ISO3166_1.Normalize3(null).EqualsOrdIgnoreCase(null));
    }
  }
}
