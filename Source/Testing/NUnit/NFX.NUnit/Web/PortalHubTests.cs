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
using NFX.Environment;
using NFX.ApplicationModel;
using NFX.Wave;


namespace NFX.NUnit.Web
{
  [TestFixture]
  public class PortalHubTests
  {
    private const string CONF1=@"
app{   
    starters
    {
       starter
       {
         name='PortalHub'
         type='NFX.Wave.PortalHub, NFX.Wave'
         
         content-file-system
         {
            type='NFX.IO.FileSystem.Local.LocalFileSystem, NFX'
            connect-params{}
            root-path=$'c:\'
         }

              portal
              {
                name='Paris' type='NFX.NUnit.Web.MockPortalFrench, NFX.NUnit' 
                primary-root-uri='http://paris.for.me'
                default=true

                theme{ name='Eiffel'  default=true  type='NFX.NUnit.Web.EuroTheme, NFX.NUnit' resource-path='paris'}
              }
              portal
              {
                name='Berlin' type='NFX.NUnit.Web.MockPortalGerman, NFX.NUnit' 
                primary-root-uri='http://berlin.for.me'
                
                theme{ name='Merkel'  default=true  type='NFX.NUnit.Web.EuroTheme, NFX.NUnit' resource-path='ausgang'}
              }
       }//PortalHub
    }

}//app
";


    [Test]
    public void Test1()
    {
      using(var app = new ServiceBaseApplication(null, CONF1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
      {
          Assert.IsNotNull(PortalHub.Instance);

          var paris = PortalHub.Instance.Portals["PARIS"];
          Assert.IsNotNull(paris);

          var berlin = PortalHub.Instance.Portals["BERLIN"];
          Assert.IsNotNull(berlin);
          
          var onlineDefault = PortalHub.Instance.DefaultOnline;
          Assert.IsNotNull(onlineDefault);
          Assert.IsTrue( onlineDefault.Name.EqualsOrdIgnoreCase("PARIS"));


          Assert.IsInstanceOf<MockPortalFrench>(paris);
          Assert.IsInstanceOf<MockPortalGerman>(berlin);

          Assert.AreEqual(CoreConsts.ISO_LANG_FRENCH, paris.DefaultLanguageISOCode);
          Assert.AreEqual(CoreConsts.ISO_LANG_GERMAN, berlin.DefaultLanguageISOCode);

          Assert.IsNotNull(paris.DefaultTheme);
          Assert.AreEqual("Eiffel", paris.DefaultTheme.Name);

          Assert.IsNotNull(berlin.DefaultTheme);
          Assert.AreEqual("Merkel", berlin.DefaultTheme.Name);
      }
    }
     
  }


  public class MockPortalFrench : Portal
  {

    protected MockPortalFrench(IConfigSectionNode conf) : base(conf){}

    public override string DefaultLanguageISOCode { get{ return CoreConsts.ISO_LANG_FRENCH;}}

    public override string DefauISOCurrency { get{ return CoreConsts.ISO_CURRENCY_EUR;}}

    public override string CountryISOCodeToLanguageISOCode(string countryISOCode)
    {
      return CoreConsts.ISO_LANG_FRENCH;
    }

    public override string AmountToString(NFX.Financial.Amount amount, Portal.MoneyFormat format = MoneyFormat.WithCurrencySymbol, ISession session = null)
    {
      return amount.Value.ToString();
    }

    public override string DateTimeToString(DateTime dt, Portal.DateTimeFormat format = DateTimeFormat.LongDateTime, ISession session = null)
    {
      return dt.ToString();
    }

    protected override Dictionary<string, string> GetLocalizableContent()
    {
      return new Dictionary<string,string>
      {
        {"Hello", "Bonjour"}
      };
    }
  }

  public class MockPortalGerman : Portal
  {

    protected MockPortalGerman(IConfigSectionNode conf) : base(conf){}

    public override string DefaultLanguageISOCode { get{ return CoreConsts.ISO_LANG_GERMAN;}}

    public override string DefauISOCurrency { get{ return CoreConsts.ISO_CURRENCY_EUR;}}

    public override string CountryISOCodeToLanguageISOCode(string countryISOCode)
    {
      return CoreConsts.ISO_LANG_GERMAN;
    }

    public override string AmountToString(NFX.Financial.Amount amount, Portal.MoneyFormat format = MoneyFormat.WithCurrencySymbol, ISession session = null)
    {
      return amount.Value.ToString();
    }

    public override string DateTimeToString(DateTime dt, Portal.DateTimeFormat format = DateTimeFormat.LongDateTime, ISession session = null)
    {
      return dt.ToString();
    }

    protected override Dictionary<string, string> GetLocalizableContent()
    {
      return new Dictionary<string,string>
      {
        {"Hello", "Hello"}
      };
    }
  }


  public class EuroTheme : Theme<Portal>
  {
    protected EuroTheme(Portal portal, IConfigSectionNode conf) : base(portal, conf){ }
  }



}
