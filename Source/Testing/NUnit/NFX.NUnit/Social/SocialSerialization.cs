/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NUnit.Framework;

using NFX.Environment;
using NFX.Web.Social;

namespace NFX.NUnit.Social
{
  [TestFixture]
  public class SocialSerialization
  {
    [Test]
    public void SerializeDeserializeFB()
    {
      using(var app = new ApplicationModel.TestApplication(m_RootCfg))
      {
        var ui = new FacebookSocialUserInfo(Facebook.Instance);

        initSocialUserInfo(ui);

        ui.UserName = "Pupkin Vasya";

        var s = ui.SerializeToString();
        var ui1 = SocialUserInfo.DeserializeFromString<FacebookSocialUserInfo>(s);
        Assert.AreEqual(ui, ui1);
      }
    }

    [Test]
    public void SerializeDeserializeTWT()
    {
      using (var app = new ApplicationModel.TestApplication(m_RootCfg))
      {
        var ui = new TwitterSocialUserInfo(Twitter.Instance);

        initSocialUserInfo(ui);

        ui.UserName = "Pupkin Vasya";
        ui.OAuthRequestToken = "rMd67-J5c";
        ui.OAuthRequestTokenSecret = "brDM-00zeq6";
        ui.OAuthVerifier = "J78iOPz";

        var s = ui.SerializeToString();
        var ui1 = SocialUserInfo.DeserializeFromString<TwitterSocialUserInfo>(s);
        Assert.AreEqual(ui, ui1);
      }
    }

    [TestFixtureSetUp]
    public void SetUp()
    {
      m_RootCfg = initConf();
    }

    private void initSocialUserInfo(SocialUserInfo ui)
    { 
      ui.Email = "pupkin@mail.com";
      ui.FirstName = "Vasya";
      ui.LastName = "Pupkin";
      ui.MiddleName = "S";
      ui.Gender = Gender.MALE;
      ui.BirthDate = new DateTime(1980, 01, 10);
      ui.Locale = "en-us";
      ui.TimezoneOffset = -120;
      ui.LongTermProviderToken = "aBX567-mm78Da/Yhj78-iik";
      ui.LoginState = SocialLoginState.LongTermTokenObtained;
      ui.LastError = new NFXException("ex-outer", new NullReferenceException());
    }

    #region LACONF

      private const string NFX_SOCIAL = "NFX_SOCIAL";

      private ConfigSectionNode initConf()
      {


//social 
//{ 
//  provider {
//    name="Gooogle+"
//    type='NFX.Web.Social.GooglePlus, NFX.Web' 
//    client-code='111111111111' client-secret='11111111111-111111111111' 
//    web-service-call-timeout-ms='20000' keep-alive='false' pipelined='false'} 
//  provider {
//    name="Facebook"
//    type='NFX.Web.Social.Facebook, NFX.Web' 
//    client-code='1111111111111111' client-secret='11111111111111111111111111111111' app-accesstoken='1111111111111111|111111111111111111111111111'} 
//  provider {
//    name="Twitter"
//    type='NFX.Web.Social.Twitter, NFX.Web' client-code='1111111111111111111111' client-secret='111111111111111111111111111111111111111111'} 
//  provider {
//    name="VKontakte"
//    type='NFX.Web.Social.VKontakte, NFX.Web' client-code='1111111' client-secret='11111111111111111111'} 
//  provider {
//    name="LinkedIn"
//    type='NFX.Web.Social.LinkedIn, NFX.Web' api-key='11111111111111' secret-key='1111111111111111'} 
//}

        string envVarStr;
        try
        {
          envVarStr = System.Environment.GetEnvironmentVariable(NFX_SOCIAL);
        }
        catch (Exception ex)
        {
          throw new Exception( 
            "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added".Args(
              NFX_SOCIAL,
              "social { provider {type='NFX.Web.Social.GooglePlus, NFX.Web' client-code='111111111111' client-secret='agh222ppppp-1pppppppPppp' web-service-call-timeout-ms='20000' keep-alive='false' pipelined='false' } }"), 
          ex);
        }

        var laconfStr = "nfx {{ web-settings {{ {0} }} }}".Args(envVarStr);
        var cfg = Configuration.ProviderLoadFromString(laconfStr, Configuration.CONFIG_LACONIC_FORMAT);
        return cfg.Root;
      }

      private ConfigSectionNode m_RootCfg;
      
    #endregion
  }
}
