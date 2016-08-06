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

using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Web.Social
{
  /// <summary>
  /// Defines constants and helper methods that facilitate VKontakte functionality
  /// </summary>
  public class VKontakte : SocialNetwork
  {
    #region CONSTS
      //do not localize

      public const string VK_PUB_SERVICE_URL = "https://www.vk.com";

      private const string LOGIN_LINK_TEMPLATE = "https://oauth.vk.com/authorize?client_id={0}&redirect_uri={1}&response_type=code";

      private const string ACCESSTOKEN_BASEURL = "https://oauth.vk.com/access_token";
      private const string ACCESSTOKEN_CLIENTID_PARAMNAME = "client_id";
      private const string ACCESSTOKEN_CLIENTSECRET_PARAMNAME = "client_secret";
      private const string ACCESSTOKEN_CODE_PARAMNAME = "code";
      private const string ACCESSTOKEN_REDIRECTURL_PARAMNAME = "redirect_uri";
      private const string ACCESSTOKEN_PARAMNAME = "access_token";
      private const string ACCESSTOKEN_USERID_PARAMNAME = "user_id";

      private const string GETUSERINFO_BASEURL = "https://api.vk.com/method/users.get";

      private const string USERINFO_USERID_PARAMNAME = "uids";
      private const string USERINFO_FIELDS_PARAMNAME = "fields";
      private const string USERINFO_FIELDS_PARAMVALUE = "uid,first_name,last_name,nickname,screen_name,sex,bdate,city,country,timezone,photo";

      private const string USERINFO_FIRSTNAME_PARAMNAME = "first_name";
      private const string USERINFO_LASTNAME_PARAMNAME = "last_name";
      private const string USERINFO_PHOTO_PARAMNAME = "photo";
      private const string USERINFO_NICKNAME_PARAMNAME = "nickname";

      private const string WALL_GET_BASEURL = "https://api.vk.com/method/wall.get";
      private const string WALL_POST_BASEURL = "https://api.vk.com/method/wall.post";
      private const string MESSAGE_PARAMNAME = "message";

    #endregion

    #region Static

      private static object s_Lock = new object();
      private static VKontakte s_Instance;

      /// <summary>
      /// Returns a singleton instance of the social network provider
      /// </summary>
      public static VKontakte Instance
      {
        get
        {
          if (s_Instance != null) return s_Instance;
          lock (s_Lock)
          {
            if (s_Instance == null) s_Instance = new VKontakte();
          }
          return s_Instance;
        }
      }

      private VKontakte(string name = null, IConfigSectionNode cfg = null) : base(name, cfg) { }

    #endregion

    #region Properties

      /// <summary>
      /// Globally uniquelly identifies social network architype
      /// </summary>
      public sealed override SocialNetID ID { get { return SocialNetID.VKT; } }

      [NFX.Environment.Config]
      public string ClientCode { get; set; }

      [NFX.Environment.Config]
      public string ClientSecret { get; set; }

      /// <summary>
      /// Returns service description
      /// </summary>
      public override string Description { get { return "VKontakte"; } }

      /// <summary>
      /// Specifies how service takes user credentials
      /// </summary>
      public override CredentialsEntryMethod CredentialsEntry { get { return CredentialsEntryMethod.Browser; } }

      /// <summary>
      /// Returns the root public URL for the service
      /// </summary>
      public override string ServiceURL { get { return VK_PUB_SERVICE_URL; } }

    #endregion

    #region Public

      public override SocialUserInfo CreateSocialUserInfo(SocialUserInfoToken? existingToken = null)
      {
        return new VKontakteSocialUserInfo(this);
      }

      public override string GetExternalLoginReference(string returnURL)
      { // dlatushkin 2014/02/06: only standalone apps are supported to wall post
        return Uri.EscapeUriString(LOGIN_LINK_TEMPLATE.Args(ClientCode, PrepareReturnURLParameter(returnURL)));
        //return Uri.EscapeUriString( LOGIN_LINK_TEMPLATE.Args(ClientCode, "https://oauth.vk.com/blank.html"));
      }

    #endregion

    #region Protected

      protected override void DoObtainTokens(SocialUserInfo userInfo, JSONDataMap request, string returnPageURL)
      {
        var code = request[ACCESSTOKEN_CODE_PARAMNAME].AsString();

        if (code.IsNullOrWhiteSpace())
          throw new NFXException( StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetUserInfo(request should contain code)");

        VKontakteSocialUserInfo vkUserInfo = userInfo as VKontakteSocialUserInfo;

        string returnURL = PrepareReturnURLParameter(returnPageURL);

        dynamic accessTokenObj = getAccessTokenObj( code, returnURL);

        vkUserInfo.AccessToken = accessTokenObj[ACCESSTOKEN_PARAMNAME];
        vkUserInfo.ID = (accessTokenObj[ACCESSTOKEN_USERID_PARAMNAME]).ToString();
      }

      protected override void DoRetrieveLongTermTokens(SocialUserInfo userInfo) {}

      protected override void DoRetrieveUserInfo(SocialUserInfo userInfo)
      {
        var vkUserInfo = userInfo as VKontakteSocialUserInfo;
        getUserInfo(vkUserInfo);
      }

      // dlatushkin 2014/02/06: only standalone apps are supported to wall post
      //public override void PostMessage(string text, SocialUserInfo userInfo)
      //{
      //  VKontakteSocialUserInfo vkUserInfo = userInfo as VKontakteSocialUserInfo;

      //  wallPost( vkUserInfo.AccessToken, text);
      //}

    #endregion

    #region .pvt. impl.

      private dynamic getAccessTokenObj(string code, string redirectURI)
      {
        return WebClient.GetJsonAsDynamic( ACCESSTOKEN_BASEURL, this, HTTPRequestMethod.GET,
          new Dictionary<string, string>() {
            {ACCESSTOKEN_CLIENTID_PARAMNAME, ClientCode},
            {ACCESSTOKEN_CLIENTSECRET_PARAMNAME, ClientSecret},
            {ACCESSTOKEN_CODE_PARAMNAME, code},
            {ACCESSTOKEN_REDIRECTURL_PARAMNAME, redirectURI}
          });
      }

      private void getUserInfo(VKontakteSocialUserInfo vkUserInfo)
      {
        dynamic responseCommon = WebClient.GetJsonAsDynamic(GETUSERINFO_BASEURL, this, HTTPRequestMethod.GET, new Dictionary<string, string>() {
          {USERINFO_USERID_PARAMNAME, vkUserInfo.ID.ToString()},
          {USERINFO_FIELDS_PARAMNAME, USERINFO_FIELDS_PARAMVALUE},
          {ACCESSTOKEN_PARAMNAME, vkUserInfo.AccessToken}
        });

        dynamic responseObj = responseCommon.response[0];

        vkUserInfo.LoginState = SocialLoginState.LoggedIn;
        vkUserInfo.FirstName = responseObj[USERINFO_FIRSTNAME_PARAMNAME];
        vkUserInfo.LastName = responseObj[USERINFO_LASTNAME_PARAMNAME];
        vkUserInfo.Nickname = responseObj[USERINFO_NICKNAME_PARAMNAME];
        vkUserInfo.Photo = responseObj[USERINFO_PHOTO_PARAMNAME];
      }

      private void wallGet(string accessToken)
      {
        dynamic responseCommon = WebClient.GetJsonAsDynamic(WALL_GET_BASEURL, this, HTTPRequestMethod.GET, new Dictionary<string, string>() {
          {ACCESSTOKEN_PARAMNAME, accessToken}
        });

        //dynamic responseObj = responseCommon.response[0];
      }

      private void wallPost(string accessToken, string text)
      {
        dynamic responseCommon = WebClient.GetJsonAsDynamic(WALL_POST_BASEURL, this, HTTPRequestMethod.GET, new Dictionary<string, string>() {
          {ACCESSTOKEN_PARAMNAME, accessToken},
          {MESSAGE_PARAMNAME, text}
        });

        //dynamic responseObj = responseCommon.response[0];
      }

    #endregion
  }

  [Serializable]
  public class VKontakteSocialUserInfo: SocialUserInfo
  {
    public VKontakteSocialUserInfo(VKontakte issuer) : base(issuer) {}

    /// <summary>
    /// Token to perform VKontakte operations like post
    /// </summary>
    public string AccessToken { get; internal set; }

    ///// <summary>
    ///// VKontakte first name field
    ///// </summary>
    //public string FirstName { get; internal set; }

    ///// <summary>
    ///// VKontakte last name field
    ///// </summary>
    //public string LastName { get; internal set; }

    /// <summary>
    /// VKontakte nickname field
    /// </summary>
    public string Nickname { get; internal set; }

    /// <summary>
    /// VKontakte nickname field
    /// </summary>
    public string Photo { get; internal set; }

    public override string DisplayName { get { return "{0} {1}".Args(LastName, FirstName);}}

    public override string DebugInfo { get { return "{0}, {1}, {2}".Args(ID, Nickname, Photo);}}

    public override string LongTermProviderToken
    {
      get { return AccessToken; }
      internal set
      {
        AccessToken = value;
        base.LongTermProviderToken = value;
      }
    }
  }

}
