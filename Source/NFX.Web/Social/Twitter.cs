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
using NFX.Web.IO;

namespace NFX.Web.Social
{
  /// <summary>
  /// Defines constants and helper methods that facilitate Twitter functionality
  /// </summary>
  public partial class Twitter : SocialNetwork
  {
    #region CONSTS

      public const string TWITTER_PUB_SERVICE_URL = "https://api.twitter.com";

      public const string TWITTER_LOGIN_URL_PATTERN = "https://api.twitter.com/oauth/authenticate?oauth_token={0}";
      public const string OAUTH_REQUEST_TOKEN_URL = "https://api.twitter.com/oauth/request_token";
      public const string OAUTH_ACCESS_TOKEN_URL = "https://api.twitter.com/oauth/access_token";
      public const string USER_SHOW_URL = "https://api.twitter.com/1.1/users/show.json";
      public const string UPDATE_STATUS_INFO_URL = "https://api.twitter.com/1.1/statuses/update.json";

      private const string OAUTH_CALLBACK_PARAMNAME = "oauth_callback";
      private const string OAUTH_CONSUMERKEY_PARAMNAME = "oauth_consumer_key";
      private const string OAUTH_NONCE_PARAMNAME = "oauth_nonce";
      private const string OAUTH_SIGNATURE_PARAMNAME = "oauth_signature";
      private const string OAUTH_SIGNATUREMETHOD_PARAMNAME = "oauth_signature_method";
      private const string OAUTH_TIMESTAMP_PARAMNAME = "oauth_timestamp";
      private const string OAUTH_VERSION_PARAMNAME = "oauth_version";
      private const string OAUTH_SIGNATUREMETHOD_PARAMVALUE = "HMAC-SHA1";
      private const string OAUTH_VERSION_PARAMVALUE = "1.0";

      private const string OAUTH_HEADER_NAME = "OAuth";

      private const string OAUTH_TOKEN_PARAMNAME = "oauth_token";
      private const string OAUTH_TOKENSECRET_PARAMNAME = "oauth_token_secret";
      private const string OAUTH_VERIFIER_PARAMNAME = "oauth_verifier";
      private const string USER_ID_PARAMNAME = "user_id";
      private const string USER_SCREENNAME_PARAMNAME = "screen_name";
      private const string USER_DEFAULTPROFILEIMAGE_PARAMNAME = "default_profile_image";
      private const string USER_PROFILEIMAGEURL_PARAMNAME = "profile_image_url";
      private const string USER_LANG_PARAMNAME = "lang";
      private const string USER_UTCOFFSET_PARAMNAME = "utc_offset";

      private const string STATUS_PARAMNAME = "status";

    #endregion

    #region Static

      private static object s_Lock = new object();
      private static Twitter s_Instance;

      /// <summary>
      /// Returns a singleton instance of the social network provider
      /// </summary>
      public static Twitter Instance
      {
        get
        {
          if (s_Instance != null) return s_Instance;
          lock (s_Lock)
          {
            if (s_Instance == null) s_Instance = new Twitter();
          }
          return s_Instance;
        }
      }

      private Twitter(string name = null, IConfigSectionNode cfg = null) : base(name, cfg) { }

    #endregion

    #region Properties

      /// <summary>
      /// Globally uniquelly identifies social network architype
      /// </summary>
      public sealed override SocialNetID ID { get { return SocialNetID.TWT; } }

      [NFX.Environment.Config]
      public string ClientCode { get; set; }

      [NFX.Environment.Config]
      public string ClientSecret { get; set; }

      /// <summary>
      /// Returns service description
      /// </summary>
      public override string Description { get { return "Twitter"; } }

      /// <summary>
      /// Specifies how service takes user credentials
      /// </summary>
      public override CredentialsEntryMethod CredentialsEntry { get { return CredentialsEntryMethod.Browser; } }

      /// <summary>
      /// Returns the root public URL for the service
      /// </summary>
      public override string ServiceURL { get { return TWITTER_PUB_SERVICE_URL; } }

      public override bool CanPost { get { return true; } }

    #endregion

    #region Public

      public override string GetExternalLoginReference(string returnURL)
      {
        return PrepareReturnURLParameter(returnURL, false);
      }

      public override string GetSpecifiedExternalLoginReference(SocialUserInfo userInfo, string returnURL)
      {
        TwitterSocialUserInfo twitterUserInfo = userInfo as TwitterSocialUserInfo;

        string solializedReturnURL = PrepareReturnURLParameter(returnURL, false);

        getOAuthRequestToken(solializedReturnURL, twitterUserInfo);

        return TWITTER_LOGIN_URL_PATTERN.Args(twitterUserInfo.OAuthRequestToken);
      }

      public override bool RequiresSpecifiedExternalLoginReference { get { return true; } }

    #endregion

    #region Protected

      protected override void DoObtainTokens(SocialUserInfo userInfo, JSONDataMap request, string returnURL)
      {
        var twitterUserInfo = userInfo as TwitterSocialUserInfo;
        twitterUserInfo.OAuthVerifier = extractVerifier(request);
        getOAuthAccessToken( twitterUserInfo);
      }

      protected override void DoRetrieveLongTermTokens(SocialUserInfo userInfo) {}

      protected override void DoRetrieveUserInfo(SocialUserInfo userInfo)
      {
        var twitterUserInfo = userInfo as TwitterSocialUserInfo;
        fillShowUserInfo(twitterUserInfo);
        twitterUserInfo.LoginState = SocialLoginState.LoggedIn;
      }

      public override SocialUserInfo CreateSocialUserInfo(SocialUserInfoToken? existingToken = null)
      {
        return new TwitterSocialUserInfo(this);
      }

      protected override void DoPostMessage(string text, SocialUserInfo userInfo)
      {
        if (userInfo.LoginState != SocialLoginState.LoggedIn)
          return;

        TwitterSocialUserInfo twitterUserInfo = userInfo as TwitterSocialUserInfo;

        twit(twitterUserInfo.OAuthAccessToken, twitterUserInfo.OAuthAccessTokenSecret, text);
      }

    #endregion

    #region .pvt .impl

      private void getOAuthRequestToken(string returnURL, TwitterSocialUserInfo twUserInfo)
      {
        Dictionary<string, string> oauthHeaderDictionary = new Dictionary<string, string>() {
          {OAUTH_CALLBACK_PARAMNAME, returnURL},
          {OAUTH_CONSUMERKEY_PARAMNAME, ClientCode},
          {OAUTH_NONCE_PARAMNAME, GenerateNonce()},
          {OAUTH_SIGNATUREMETHOD_PARAMNAME, OAUTH_SIGNATUREMETHOD_PARAMVALUE},
          {OAUTH_TIMESTAMP_PARAMNAME, MiscUtils.ToSecondsSinceUnixEpochStart(DateTime.Now).ToString()},
          {OAUTH_VERSION_PARAMNAME, OAUTH_VERSION_PARAMVALUE}
        };

        string rawOAuthHeaderStr = GetRawOAuthHeaderStr(oauthHeaderDictionary);
        string requestStringToSign = AddMethodAndBaseURL(rawOAuthHeaderStr, HTTPRequestMethod.POST, OAUTH_REQUEST_TOKEN_URL);
        string signature = CalculateSignature(requestStringToSign, ClientSecret);

        oauthHeaderDictionary.Add(OAUTH_SIGNATURE_PARAMNAME, signature);

        string oauthHeaderStr = "{0} {1}".Args(OAUTH_HEADER_NAME, GetOAuthHeaderString(oauthHeaderDictionary));

        var responseDictionary = WebClient.GetValueMap(OAUTH_REQUEST_TOKEN_URL, this, HTTPRequestMethod.POST,
          headers: new Dictionary<string, string>() { { HttpRequestHeader.Authorization.ToString(), oauthHeaderStr} }
        );

        twUserInfo.OAuthRequestToken = responseDictionary[OAUTH_TOKEN_PARAMNAME].AsString();
        twUserInfo.OAuthRequestTokenSecret = responseDictionary[OAUTH_TOKENSECRET_PARAMNAME].AsString();
      }

      private void getOAuthAccessToken(TwitterSocialUserInfo twUserInfo)
      {
        Dictionary<string, string> oauthHeaderDictionary = new Dictionary<string, string>() {
          {OAUTH_CONSUMERKEY_PARAMNAME, ClientCode},
          {OAUTH_NONCE_PARAMNAME, GenerateNonce()},
          {OAUTH_SIGNATUREMETHOD_PARAMNAME, OAUTH_SIGNATUREMETHOD_PARAMVALUE},
          {OAUTH_TIMESTAMP_PARAMNAME, MiscUtils.ToSecondsSinceUnixEpochStart(DateTime.Now).ToString()},
          {OAUTH_TOKEN_PARAMNAME, twUserInfo.OAuthRequestToken},
          {OAUTH_VERSION_PARAMNAME, OAUTH_VERSION_PARAMVALUE}
        };

        Dictionary<string, string> signDictionary = new Dictionary<string,string>( oauthHeaderDictionary);
        signDictionary.Add(OAUTH_VERIFIER_PARAMNAME, twUserInfo.OAuthVerifier);

        string rawOAuthHeaderStr = GetRawOAuthHeaderStr(signDictionary);
        string requestStringToSign = AddMethodAndBaseURL(rawOAuthHeaderStr, HTTPRequestMethod.POST, OAUTH_ACCESS_TOKEN_URL);
        string signature = CalculateSignature(requestStringToSign, ClientSecret, twUserInfo.OAuthRequestTokenSecret);

        oauthHeaderDictionary.Add(OAUTH_SIGNATURE_PARAMNAME, signature);

        string oauthHeaderStr = "{0} {1}".Args(OAUTH_HEADER_NAME, GetOAuthHeaderString(oauthHeaderDictionary));

        var responseDictionary = WebClient.GetValueMap(OAUTH_ACCESS_TOKEN_URL, this, HTTPRequestMethod.POST,
          bodyParameters: new Dictionary<string, string>() { { OAUTH_VERIFIER_PARAMNAME, twUserInfo.OAuthVerifier} },
          headers: new Dictionary<string, string>() { { HttpRequestHeader.Authorization.ToString(), oauthHeaderStr} }
        );

        twUserInfo.OAuthAccessToken = responseDictionary[OAUTH_TOKEN_PARAMNAME].AsString();
        twUserInfo.OAuthAccessTokenSecret = responseDictionary[OAUTH_TOKENSECRET_PARAMNAME].AsString();
        twUserInfo.ID = responseDictionary[USER_ID_PARAMNAME].AsString();
        twUserInfo.ScreenName = responseDictionary[USER_SCREENNAME_PARAMNAME].AsString();
      }

      private string extractVerifier(JSONDataMap inboundParams)
      {
        var oauthToken = inboundParams[OAUTH_TOKEN_PARAMNAME].AsString();
        var oauthVerifier = inboundParams[OAUTH_VERIFIER_PARAMNAME].AsString();

        if (oauthToken.IsNullOrWhiteSpace() || oauthVerifier.IsNullOrWhiteSpace())
            throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name
              + ".ExtractVerifier(inboundParams.Contains({0}&{1}))".Args(OAUTH_TOKEN_PARAMNAME, OAUTH_VERIFIER_PARAMNAME));

        return oauthVerifier;
      }

      // Gets user info.
      // For details see https://dev.twitter.com/rest/reference/get/users/show
      private void fillShowUserInfo(TwitterSocialUserInfo twUserInfo)
      {
        Dictionary<string, string> oauthHeaderDictionary = new Dictionary<string, string>() {
          {OAUTH_CONSUMERKEY_PARAMNAME, ClientCode},
          {OAUTH_NONCE_PARAMNAME, GenerateNonce()},
          {OAUTH_SIGNATUREMETHOD_PARAMNAME, OAUTH_SIGNATUREMETHOD_PARAMVALUE},
          {OAUTH_TIMESTAMP_PARAMNAME, MiscUtils.ToSecondsSinceUnixEpochStart(DateTime.Now).ToString()},
          {OAUTH_TOKEN_PARAMNAME, twUserInfo.OAuthAccessToken},
          {OAUTH_VERSION_PARAMNAME, OAUTH_VERSION_PARAMVALUE}
        };

        Dictionary<string, string> signDictionary = new Dictionary<string, string>(oauthHeaderDictionary);
        signDictionary.Add(USER_ID_PARAMNAME, twUserInfo.ID);

        string rawOAuthHeaderStr = GetRawOAuthHeaderStr(signDictionary);
        string requestStringToSign = AddMethodAndBaseURL(rawOAuthHeaderStr, HTTPRequestMethod.GET, USER_SHOW_URL);
        string signature = CalculateSignature(requestStringToSign, ClientSecret, twUserInfo.OAuthAccessTokenSecret);

        oauthHeaderDictionary.Add(OAUTH_SIGNATURE_PARAMNAME, signature);

        string oauthHeaderStr = "{0} {1}".Args(OAUTH_HEADER_NAME, GetOAuthHeaderString(oauthHeaderDictionary));

        dynamic response = WebClient.GetJsonAsDynamic(USER_SHOW_URL, this, HTTPRequestMethod.GET,
          queryParameters: new Dictionary<string, string>() { { USER_ID_PARAMNAME, twUserInfo.ID } },
          headers: new Dictionary<string, string>() { { HttpRequestHeader.Authorization.ToString(), oauthHeaderStr } }
        );

        try
        {
          bool hasDefaultProfileImage = response[USER_DEFAULTPROFILEIMAGE_PARAMNAME];
          if (!hasDefaultProfileImage)
            twUserInfo.PictureLink = response[USER_PROFILEIMAGEURL_PARAMNAME];

          twUserInfo.Locale = response[USER_LANG_PARAMNAME];

          twUserInfo.TimezoneOffset = (int?)response[USER_UTCOFFSET_PARAMNAME];
        }
        catch (Exception ex)
        {
          throw new NFXException(SocialStringConsts.POSTFAILED_ERROR + this.GetType().Name + ".twit", ex);
        }
      }

      // Updates the authenticating user's status, AKA tweeting
      private long twit(string acessToken, string tokenSecret, string text)
      {
          Dictionary<string, string> oauthHeaderDictionary = new Dictionary<string, string>() {
          {OAUTH_CONSUMERKEY_PARAMNAME, ClientCode},
          {OAUTH_NONCE_PARAMNAME, GenerateNonce()},
          {OAUTH_SIGNATUREMETHOD_PARAMNAME, OAUTH_SIGNATUREMETHOD_PARAMVALUE},
          {OAUTH_TIMESTAMP_PARAMNAME, MiscUtils.ToSecondsSinceUnixEpochStart(DateTime.Now).ToString()},
          {OAUTH_TOKEN_PARAMNAME, acessToken},
          {OAUTH_VERSION_PARAMNAME, OAUTH_VERSION_PARAMVALUE}
        };

        Dictionary<string, string> signDictionary = new Dictionary<string,string>( oauthHeaderDictionary);
        signDictionary.Add(STATUS_PARAMNAME, text);

        string rawOAuthHeaderStr = GetRawOAuthHeaderStr(signDictionary);
        string requestStringToSign = AddMethodAndBaseURL(rawOAuthHeaderStr, HTTPRequestMethod.POST, UPDATE_STATUS_INFO_URL);
        string signature = CalculateSignature(requestStringToSign, ClientSecret, tokenSecret);

        oauthHeaderDictionary.Add(OAUTH_SIGNATURE_PARAMNAME, signature);

        string oauthHeaderStr = "{0} {1}".Args(OAUTH_HEADER_NAME, GetOAuthHeaderString(oauthHeaderDictionary));

        dynamic response = WebClient.GetJsonAsDynamic(UPDATE_STATUS_INFO_URL, this, HTTPRequestMethod.POST,
          bodyParameters: new Dictionary<string, string>() { { STATUS_PARAMNAME, RFC3986.Encode( text)}},
          headers: new Dictionary<string, string>() { { HttpRequestHeader.Authorization.ToString(), oauthHeaderStr} }
        );

        try
        {
          return (long)response.Data["id"];
        }
        catch (Exception ex)
        {
          throw new NFXException(SocialStringConsts.POSTFAILED_ERROR + this.GetType().Name + ".twit", ex);
        }
      }

    #endregion
  }

  [Serializable]
  public class TwitterSocialUserInfo: SocialUserInfo
  {
    public TwitterSocialUserInfo(Twitter issuer) : base(issuer) {}

    /// <summary>
    /// Twitter user name
    /// </summary>
    public string UserName { get; internal set; }

    /// <summary>
    /// Request token to obtain access token
    /// </summary>
    public string OAuthRequestToken { get; internal set; }

    /// <summary>
    /// Token secret to sign query to obtain access token
    /// </summary>
    public string OAuthRequestTokenSecret { get; internal set; }

    /// <summary>
    /// Field is used to obtain regular access token
    /// </summary>
    public string OAuthVerifier { get; internal set; }

    /// <summary>
    /// Token to perform Twitter operations like post
    /// </summary>
    public string OAuthAccessToken { get; internal set; }


    /// <summary>
    /// Token secret to sign Twitter operations like post
    /// </summary>
    public string OAuthAccessTokenSecret { get; internal set; }

    public override string LongTermProviderToken
    {
      get
      {
        return OAuthAccessToken + "/" + OAuthAccessTokenSecret;
      }
      internal set
      {
        var parts = value.Split('/');
        OAuthAccessToken = parts[0];
        OAuthAccessTokenSecret = parts[1];
        base.LongTermProviderToken = value;
      }
    }

    /// <summary>
    /// Twitter screen name field
    /// </summary>
    public string ScreenName;

    public override string DisplayName { get { return ScreenName;}}

    public override string DebugInfo { get { return "{0}, {1}, {2}, {3}".Args(ID, OAuthAccessToken, OAuthAccessTokenSecret, ScreenName);} }

    #region Object overrides

      public override bool Equals(object obj)
      {
        if (!base.Equals(obj)) return false;

        var other = obj as TwitterSocialUserInfo;
        return this.UserName == other.UserName &&
          this.OAuthAccessToken == other.OAuthAccessToken &&
          this.OAuthAccessTokenSecret == other.OAuthAccessTokenSecret &&
          this.OAuthVerifier == other.OAuthVerifier;
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

    #endregion
  }


}
