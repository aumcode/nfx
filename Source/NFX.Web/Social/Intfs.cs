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
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Instrumentation;
using NFX.Serialization.JSON;

namespace NFX.Web.Social
{
  /// <summary>
  /// Flags that specify how a network takes user credentials
  /// </summary>
  [Flags]
  public enum CredentialsEntryMethod
  {
      /// <summary>
      /// Specifies that the service does not support any forms of security integration
      /// </summary>
      None = 0,

      /// <summary>
      /// Specifies that the service only allows users to enter credentials via browser, in a popup or IFrame
      /// </summary>
      Browser = 1,

      /// <summary>
      /// Specifies that the service has server API that a client (web app) can call and pass the credentials
      /// </summary>
      ServerAPI = 2
  }

  /// <summary>
  /// Globally uniquelly identifies social network architypes
  /// </summary>
  public enum SocialNetID
  {
    UNS=0, // Unspecified

    TWT=1, // Twitter
    FBK=2, // Facebook
    GPS=3, // Google+
    LIN=4, // LinkedIn
    IGM=5, // Instagram
    PIN=6, // Pinterest

    VKT=100, // VKontakte
    ODN=101, // Odnoklassniki

    OTH=1000000 // Other
  }

  /// <summary>
  /// Describes an entity that can perform social functions (i.e. login, post)
  /// </summary>
  public interface ISocialNetwork: INamed
  {
    /// <summary>
    /// Globally uniquelly identifies social network architype
    /// </summary>
    SocialNetID ID { get; }

    /// <summary>
    /// Provides social network description, this default implementation returns the name of the class
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Returns the root public URL for the service
    /// </summary>
    string ServiceURL { get; }

    /// <summary>
    /// Specifies how service takes user credentials
    /// </summary>
    CredentialsEntryMethod CredentialsEntry { get; }

    /// <summary>
    /// Defines if a meeesage can be post to this social network
    /// </summary>
    bool CanPost { get; }

    /// <summary>
    /// Returns href to login via social system/site
    /// </summary>
    string GetExternalLoginReference(string returnURL);

    /// <summary>
    /// Returns social service login URL for "two-stage" login networks.
    /// Currently twitter only requires this
    /// </summary>
    /// <param name="returnURL">Social site redirects browser here after login</param>
    /// <param name="userInfo">Context</param>
    /// <returns>Social site login URL</returns>
    string GetSpecifiedExternalLoginReference(SocialUserInfo userInfo, string returnURL);

    /// <summary>
    /// Specifies if this provider requires to obtain temporary token before redirecting to social network login page.
    /// Currently only Twitter requires this routine
    /// </summary>
    bool RequiresSpecifiedExternalLoginReference { get; }

    /// <summary>
    /// Fills user info with values from social network
    /// </summary>
    /// <param name="userInfo">Context user info</param>
    /// <param name="request">Context http request</param>
    /// <param name="returnURL">Social network login URL (sometimes needed by social site just to ensure correct call)</param>
    void ObtainTokensAndFillInfo(SocialUserInfo userInfo, JSONDataMap request, string returnURL);

    /// <summary>
    /// Refreshes long term tokens (if provider needs them).
    /// Should be used in scenario like background server-side token renew routine
    /// </summary>
    void RenewLongTermTokens(SocialUserInfo userInfo);

    /// <summary>
    /// Retrieves all user fields (e.g. screen name, email) but tokens.
    /// </summary>
    void RetrieveUserInfo(SocialUserInfo userInfo);

    /// <summary>
    /// Create an instance of social user info class.
    /// If parameters are null then creates new non-logged-in instance, otherwise, if parameters are set,
    /// then connects to network and tries to re-initializes SocialUser info with fresh data
    /// from the network (i.e. name, gender etc.) using the supplied net tokens, or throws if tokens are invalid (i.e. expired).
    /// This returned instance is usually stored in session for later use
    /// </summary>
    /// <returns>SocialUserInfo instance</returns>
    SocialUserInfo CreateSocialUserInfo(SocialUserInfoToken? existingToken = null);

    /// <summary>
    /// Returns user profile image data along with content type or null if no image available.
    /// Picture kind specifies classification of pictures within profile i.e. "main", "small-icon" etc.
    /// </summary>
    byte[] GetPictureData(SocialUserInfo userInfo, out string contentType, string pictureKind = null);

    /// <summary>
    /// Returns user profile image or null if no image available.
    /// Picture kind specifies classification of pictures within profile i.e. "main", "small-icon" etc.
    /// </summary>
    System.Drawing.Image GetPicture(SocialUserInfo userInfo, out string contentType, string pictureKind = null);

    /// <summary>
    /// Post message to social network
    /// </summary>
    /// <param name="userInfo">Context social user info</param>
    /// <param name="text">Message to send</param>
    void PostMessage(SocialUserInfo userInfo, string text);

#pragma warning disable 1570
    /// <summary>
    /// Takes unescaped regular URL and transforms it in a single escaped parameter string suitable
    /// for submission to social network.
    /// For example, incoming "https://aa.bb?nonce=FDAC25&target=123" -> "https%3a%2f%2faa.com%3fnonce%3dFDAC25%26target%3d123"
    /// </summary>
#pragma warning restore 1570
    string PrepareReturnURLParameter(string returnURL, bool escape = true);
  }

  public interface ISocialNetworkImplementation: ISocialNetwork, IConfigurable, IInstrumentable
  {
  }
}
