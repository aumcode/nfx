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
using System.Net;
using System.Text;

using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Web.Social
{
  /// <summary>
  /// Defines constants and helper methods that facilitate GooglePlus functionality
  /// </summary>
  public class GooglePlus : SocialNetwork
  {
    #region CONSTS
      //do not localize

      public const string GOOGLE_PLUS_PUB_SERVICE_URL = "http://plus.google.com";

      //private const string LOGIN_LINK_TEMPLATE = "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&scope=https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile";
      private const string LOGIN_LINK_TEMPLATE = "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&scope=https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile+https://www.googleapis.com/auth/plus.login";

    //https://accounts.google.com/o/oauth2/auth?
      //redirect_uri=https://developers.google.com/oauthplayground&
      //response_type=code&
      //client_id=407408718192.apps.googleusercontent.com&
      //scope=https://www.googleapis.com/auth/userinfo.email+
        //https://www.googleapis.com/auth/plus.me+
        //https://www.googleapis.com/auth/userinfo.email+
        //https://www.googleapis.com/auth/plus.me+
        //https://www.googleapis.com/auth/userinfo.email+
        //https://www.googleapis.com/auth/plus.circles.read+
        //https://www.googleapis.com/auth/plus.me+
        //https://www.googleapis.com/auth/userinfo.email&
        //approval_prompt=force&
        //access_type=offline

      private const string BEARER = "Bearer";

      private const string ACCESSTOKEN_BASEURL = "https://accounts.google.com/o/oauth2/token";
      private const string ACCESSTOKEN_CODE_PARAMNAME = "code";
      private const string ACCESSTOKEN_CLIENTID_PARAMNAME = "client_id";
      private const string ACCESSTOKEN_CLIENTSECRET_PARAMNAME = "client_secret";
      private const string ACCESSTOKEN_REDIRECTURL_PARAMNAME = "redirect_uri";
      private const string ACCESSTOKEN_GRANTTYPE_PARAMNAME = "grant_type";
      private const string ACCESSTOKEN_GRANTTYPE_PARAMVALUE = "authorization_code";
      private const string ACCESSTOKEN_PARAMNAME = "access_token";

      private const string POST_TOKEN_URL = "https://accounts.google.com/o/oauth2/token";
      private const string GETUSERINFO_BASEURL = "https://www.googleapis.com/oauth2/v1/userinfo";

      private const string INSERTMOMENT_BASEURL_PATTERN = "https://www.googleapis.com/plus/v1/people/{0}/moments/vault";
      private const string LISTACTIVITIES_BASEURL_PATTERN = "https://www.googleapis.com/plusDomains/v1/people/{0}/activities/user";
      private const string LISTMOMENTS_BASEURL_PATTERN = " https://www.googleapis.com/plus/v1/people/me/moments/vault";

      private const string USERID_EMAIL_PARAMNAME = "id";
      private const string USERINFO_EMAIL_PARAMNAME = "email";
      private const string USERINFO_NAME_PARAMNAME = "name";

    #endregion

    #region Static

      private static object s_Lock = new object();
      private static GooglePlus s_Instance;

      /// <summary>
      /// Returns a singleton instance of the social network provider
      /// </summary>
      public static GooglePlus Instance
      {
        get
        {
          if (s_Instance != null) return s_Instance;
          lock (s_Lock)
          {
            if (s_Instance == null) s_Instance = new GooglePlus();
          }
          return s_Instance;
        }
      }

      private GooglePlus(string name = null, IConfigSectionNode cfg = null) : base(name, cfg) { }

    #endregion

    #region Properties

      /// <summary>
      /// Globally uniquelly identifies social network architype
      /// </summary>
      public sealed override SocialNetID ID { get { return SocialNetID.GPS; } }

      [NFX.Environment.Config]
      public string ClientCode { get; set; }

      [NFX.Environment.Config]
      public string ClientSecret { get; set; }

      /// <summary>
      /// Returns service description
      /// </summary>
      public override string Description { get { return "Google+"; } }

      /// <summary>
      /// Specifies how service takes user credentials
      /// </summary>
      public override CredentialsEntryMethod CredentialsEntry { get { return CredentialsEntryMethod.Browser; } }

      /// <summary>
      /// Returns the root public URL for the service
      /// </summary>
      public override string ServiceURL { get { return GOOGLE_PLUS_PUB_SERVICE_URL; } }

    #endregion

    #region Public

      public override SocialUserInfo CreateSocialUserInfo(SocialUserInfoToken? existingToken = null)
      {
        return new GooglePlusSocialUserInfo(this);
      }

      public override string GetExternalLoginReference(string returnURL)
      {
        return LOGIN_LINK_TEMPLATE.Args(ClientCode, PrepareReturnURLParameter(returnURL));
      }

    #endregion

    #region Protected

      protected override void DoObtainTokens(SocialUserInfo userInfo, JSONDataMap request, string returnPageURL)
      {
        var code = request[ACCESSTOKEN_CODE_PARAMNAME].AsString();

        if (code.IsNullOrWhiteSpace())
          throw new NFXException( StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetUserInfo(request should contain code)");

        string returnURL = PrepareReturnURLParameter(returnPageURL);

        var googleUserInfo = userInfo as GooglePlusSocialUserInfo;

        string accessToken = getAccessToken( code, returnURL);
        googleUserInfo.AccessToken = accessToken;
      }

      protected override void DoRetrieveLongTermTokens(SocialUserInfo userInfo) {}

      protected override void DoRetrieveUserInfo(SocialUserInfo userInfo)
      {
        var googleUserInfo = userInfo as GooglePlusSocialUserInfo;
        getUserInfo(googleUserInfo);
      }

    #endregion

    #region .pvt. impl.

      private string getAccessToken(string code, string redirectURI)
      {
        dynamic responseObj = WebClient.GetJsonAsDynamic(ACCESSTOKEN_BASEURL, this, HTTPRequestMethod.POST,
          bodyParameters: new Dictionary<string, string>() {
          {ACCESSTOKEN_CODE_PARAMNAME, code},
          {ACCESSTOKEN_CLIENTID_PARAMNAME, ClientCode},
          {ACCESSTOKEN_CLIENTSECRET_PARAMNAME, ClientSecret},
          {ACCESSTOKEN_REDIRECTURL_PARAMNAME, redirectURI},
          {ACCESSTOKEN_GRANTTYPE_PARAMNAME, ACCESSTOKEN_GRANTTYPE_PARAMVALUE}
        });

        return responseObj[ACCESSTOKEN_PARAMNAME];
      }

      private void getUserInfo(GooglePlusSocialUserInfo userInfo)
      {
        dynamic responseObj = WebClient.GetJsonAsDynamic(GETUSERINFO_BASEURL, this, HTTPRequestMethod.GET,
          headers: getAuthorizationHeader(userInfo.AccessToken)
        );

        userInfo.LoginState = SocialLoginState.LoggedIn;
        userInfo.ID = responseObj[USERID_EMAIL_PARAMNAME];
        userInfo.AccessToken = userInfo.AccessToken;
        userInfo.Email = responseObj[USERINFO_EMAIL_PARAMNAME];
        userInfo.UserName = responseObj[USERINFO_NAME_PARAMNAME];
      }

      private Dictionary<string, string> getAuthorizationHeader(string accessToken)
      {
        string headerValue = "{0} {1}".Args(BEARER, accessToken);
        return new Dictionary<string, string>() { {HttpRequestHeader.Authorization.ToString(), headerValue}};
      }

    #endregion
  }


  [Serializable]
  public class GooglePlusSocialUserInfo: SocialUserInfo
  {
    public GooglePlusSocialUserInfo(GooglePlus issuer) : base(issuer) {}

    /// <summary>
    /// Google+ user name field
    /// </summary>
    public string UserName { get; internal set; }

    /// <summary>
    /// Token to perform googl+ operations like post
    /// </summary>
    public string AccessToken { get; internal set; }

    public override string DisplayName { get { return "{0}".Args(UserName);}}

    public override string DebugInfo { get { return "{0}, {1}".Args(ID, AccessToken);}}

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
