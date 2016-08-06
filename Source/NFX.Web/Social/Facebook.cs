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
    /// Defines constants and helper methods that facilitate Facebook functionality
    /// </summary>
    public class Facebook : SocialNetwork
    {
      #region CONSTS
        //do not localize

        public const string FACEBOOK_PUB_SERVICE_URL = "https://www.facebook.com";

        private const string LOGIN_LINK_TEMPLATE = "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&response_type=code&scope=publish_stream+email";

        private const string ACCESSTOKEN_BASEURL = "https://graph.facebook.com/oauth/access_token";
        private const string ACCESSTOKEN_CODE_PARAMNAME = "code";
        private const string ACCESSTOKEN_CLIENTID_PARAMNAME = "client_id";
        private const string ACCESSTOKEN_CLIENTSECRET_PARAMNAME = "client_secret";
        private const string ACCESSTOKEN_REDIRECTURL_PARAMNAME = "redirect_uri";
        private const string ACCESSTOKEN_PARAMNAME = "access_token";
        private const string FBEXCHANGETOKEN_PARAMNAME = "fb_exchange_token";
        private const string GRANTTYPE_PARAMNAME = "grant_type";
        private const string GRANTTYPE_FBEXCHANGETOKEN_PARAMVALUE = FBEXCHANGETOKEN_PARAMNAME;

        private const string GETUSERINFO_BASEURL = "https://graph.facebook.com/me";

        private const string GETUSERINFO_FIELDS_PARAMNAME = "fields";

        private const string USER_ID_PARAMNAME = "id";
        private const string USER_EMAIL_PARAMNAME = "email";
        private const string USER_NAME_PARAMNAME = "name"; // person's full name
        private const string USER_FIRSTNAME_PARAMNAME = "first_name";
        private const string USER_LASTNAME_PARAMNAME = "last_name";
        private const string USER_MIDDLENAME_PARAMNAME = "middle_name";
        private const string USER_GENDER_PARAMNAME = "gender";
          private const string USER_GENDER_MALE = "male";
          private const string USER_GENDER_FEMALE = "female";
        private const string USER_BIRTHDAY_PARAMNAME = "birthday"; // person's birthday in the format MM/DD/YYYY.
        private const string USER_LOCALE_PARAMNAME = "locale";
        private const string USER_TIMEZONE_PARAMNAME = "timezone"; // int, user's current timezone offset from UTC
        private const string USER_PICTURE_PARAMNAME = "picture";
          private const string USER_PICTURE_DATA_PARAMNAME = "data";
          private const string USER_PICTURE_URL_PARAMNAME = "url";

        private const string PUBLISH_BASEURL_PATTERN = "https://graph.facebook.com/{0}/feed";

        private const string MESSAGE_PARAMNAME = "message";

      #endregion

      #region Static

        private static object s_Lock = new object();
        private static Facebook s_Instance;

        /// <summary>
        /// Returns a singleton instance of the social network provider
        /// </summary>
        public static Facebook Instance
        {
          get
          {
            if (s_Instance != null) return s_Instance;
            lock (s_Lock)
            {
              if (s_Instance == null) s_Instance = new Facebook();
            }
            return s_Instance;
          }
        }

        private Facebook(string name = null, IConfigSectionNode cfg = null) : base(name, cfg) { }

      #endregion


      #region Properties

        /// <summary>
        /// Globally uniquelly identifies social network architype
        /// </summary>
        public sealed override SocialNetID ID { get { return SocialNetID.FBK; } }

        [NFX.Environment.Config]
        public string AppID { get; set; }

        [NFX.Environment.Config]
        public string ClientSecret { get; set; }

      #endregion

      #region Protected

        public override SocialUserInfo CreateSocialUserInfo(SocialUserInfoToken? existingToken = null)
        {
          return new FacebookSocialUserInfo(this);
        }

        public override string GetExternalLoginReference(string returnURL)
        {
          return LOGIN_LINK_TEMPLATE.Args(AppID, PrepareReturnURLParameter(returnURL));
        }

        protected override void DoObtainTokens(SocialUserInfo userInfo, JSONDataMap request, string returnPageURL)
        {
          var code = request[ACCESSTOKEN_CODE_PARAMNAME].AsString();

          if (code.IsNullOrWhiteSpace())
            throw new NFXException( StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetUserInfo(request should contain code)");

          FacebookSocialUserInfo fbUserInfo = userInfo as FacebookSocialUserInfo;

          string returnURL = PrepareReturnURLParameter(returnPageURL);

          fbUserInfo.AccessToken = getAccessToken( code, returnURL);
        }

        protected override void DoRetrieveLongTermTokens(SocialUserInfo userInfo)
        {
          var fbUserInfo = userInfo as FacebookSocialUserInfo;
          fbUserInfo.LongTermAccessToken = getLongTermAccessToken(fbUserInfo.AccessToken);
        }

        protected override void DoRetrieveUserInfo(SocialUserInfo userInfo)
        {
          var fbUserInfo = userInfo as FacebookSocialUserInfo;
          fillUserInfo(fbUserInfo);
        }

        protected override void DoPostMessage(string text, SocialUserInfo userInfo)
        {
          if (userInfo.LoginState != SocialLoginState.LoggedIn)
            return;

          FacebookSocialUserInfo fbUserInfo = userInfo as FacebookSocialUserInfo;

          publish( fbUserInfo.ID, fbUserInfo.AccessToken, text);
        }

        /// <summary>
        /// Returns service description
        /// </summary>
        public override string Description { get { return "Facebook";} }

        /// <summary>
        /// Specifies how service takes user credentials
        /// </summary>
        public override CredentialsEntryMethod CredentialsEntry {  get { return CredentialsEntryMethod.Browser; } }

        /// <summary>
        /// Returns the root public URL for the service
        /// </summary>
        public override string ServiceURL { get { return FACEBOOK_PUB_SERVICE_URL; } }

        public override bool CanPost { get { return true;} }

      #endregion

      #region .pvt. impl.

        private string getAccessToken(string code, string redirectURI)
        {
          var response = WebClient.GetValueMap(ACCESSTOKEN_BASEURL, this, HTTPRequestMethod.GET,
            new Dictionary<string, string>() {
              {ACCESSTOKEN_CLIENTID_PARAMNAME, AppID},
              {ACCESSTOKEN_REDIRECTURL_PARAMNAME, redirectURI},
              {ACCESSTOKEN_CLIENTSECRET_PARAMNAME, ClientSecret},
              {ACCESSTOKEN_CODE_PARAMNAME, code}
            });

          return response[ACCESSTOKEN_PARAMNAME].AsString();
        }

        private void fillUserInfo(FacebookSocialUserInfo userInfo)
        {
          var fields = string.Join(",", USER_ID_PARAMNAME, USER_EMAIL_PARAMNAME, USER_NAME_PARAMNAME, USER_FIRSTNAME_PARAMNAME,
            USER_LASTNAME_PARAMNAME, USER_MIDDLENAME_PARAMNAME, USER_GENDER_PARAMNAME, USER_BIRTHDAY_PARAMNAME,
            USER_LOCALE_PARAMNAME, USER_TIMEZONE_PARAMNAME, USER_PICTURE_PARAMNAME);

          dynamic responseObj = WebClient.GetJsonAsDynamic(GETUSERINFO_BASEURL, this, HTTPRequestMethod.GET, new Dictionary<string, string>() {
            {ACCESSTOKEN_PARAMNAME, userInfo.AccessToken},
            {GETUSERINFO_FIELDS_PARAMNAME, fields}
          });

          userInfo.LoginState = SocialLoginState.LoggedIn;
          userInfo.ID = responseObj[USER_ID_PARAMNAME];
          userInfo.UserName = responseObj[USER_NAME_PARAMNAME];
          userInfo.Email = responseObj[USER_EMAIL_PARAMNAME];
          userInfo.FirstName = responseObj[USER_FIRSTNAME_PARAMNAME];
          userInfo.LastName = responseObj[USER_LASTNAME_PARAMNAME];
          userInfo.MiddleName = responseObj[USER_MIDDLENAME_PARAMNAME];

          var genderStr = responseObj[USER_GENDER_PARAMNAME];
          if (genderStr == USER_GENDER_MALE)
            userInfo.Gender = Gender.MALE;
          else if (genderStr == USER_GENDER_MALE)
            userInfo.Gender = Gender.FEMALE;

          var birthDateStr = responseObj[USER_BIRTHDAY_PARAMNAME];
          DateTime birthDate;
          if (DateTime.TryParseExact(birthDateStr, "MM/DD/YYYY",
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out birthDate))
              userInfo.BirthDate = birthDate;

          userInfo.Locale = responseObj[USER_LOCALE_PARAMNAME];
          userInfo.TimezoneOffset = ((int)responseObj[USER_TIMEZONE_PARAMNAME]) * 60 * 60;

          userInfo.PictureLink = responseObj[USER_PICTURE_PARAMNAME][USER_PICTURE_DATA_PARAMNAME][USER_PICTURE_URL_PARAMNAME];
        }

        private string getLongTermAccessToken(string accessToken)
        {
          var response = WebClient.GetValueMap(ACCESSTOKEN_BASEURL, this, HTTPRequestMethod.GET,
            new Dictionary<string, string>() {
              {GRANTTYPE_PARAMNAME, GRANTTYPE_FBEXCHANGETOKEN_PARAMVALUE},
              {ACCESSTOKEN_CLIENTID_PARAMNAME, AppID},
              {ACCESSTOKEN_CLIENTSECRET_PARAMNAME, ClientSecret},
              {FBEXCHANGETOKEN_PARAMNAME, accessToken}
            });

          return response[ACCESSTOKEN_PARAMNAME].AsString();
        }

        // you can post a new wall post on current user's wall by issuing a POST request to https://graph.facebook.com/[USER_ID]/feed:
        private void publish(string userId, string accessToken, string message)
        {
          string url = PUBLISH_BASEURL_PATTERN.Args(userId);

          dynamic responseObj = WebClient.GetJsonAsDynamic(url, this, HTTPRequestMethod.POST,
            queryParameters: new Dictionary<string, string>() {
              {MESSAGE_PARAMNAME, message},
              {ACCESSTOKEN_PARAMNAME, accessToken}
            }
          );
        }

      #endregion

    }




  [Serializable]
  public class FacebookSocialUserInfo: SocialUserInfo
  {
    public FacebookSocialUserInfo(Facebook issuer, SocialUserInfoToken? existingToken = null) : base(issuer) { }

    /// <summary>
    /// Facebook user name field
    /// </summary>
    public string UserName { get; internal set; }

    /// <summary>
    /// Token to perform Facebook operations like post (CAN expire in 20 minutes)
    /// </summary>
    public string AccessToken { get; internal set; }

    /// <summary>
    /// Token to perform Facebook operations like post (expires in two months)
    /// </summary>
    public string LongTermAccessToken
    {
      get;
      internal set;
    }

    public override string LongTermProviderToken
    {
      get { return LongTermAccessToken; }
      internal set
      {
        LongTermAccessToken = value;
        base.LongTermProviderToken = value;
      }
    }

    public override string DisplayName { get { return "{0}".Args(UserName);}}

    public override string DebugInfo { get { return "{0}, {1}, {2}".Args(ID, AccessToken, LongTermAccessToken);} }

    #region Object overrides

      public override bool Equals(object obj)
      {
        return base.Equals(obj) && UserName == ((FacebookSocialUserInfo)obj).UserName;
      }

      public override int GetHashCode()
      {
        return UserName.GetHashCode();
      }

    #endregion
  }


}
