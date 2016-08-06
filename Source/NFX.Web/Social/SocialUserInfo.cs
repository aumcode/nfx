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
using System.Dynamic;
using System.Linq;
using System.Text;


using NFX.Serialization.Slim;
using System.IO;

namespace NFX.Web.Social
{


  /// <summary>
  /// Represents user (as represented by SocialUserInfo object) login states
  /// </summary>
  public enum SocialLoginState
  {
    /// <summary>
    /// User not connected/logged-in
    /// </summary>
    NotLoggedIn = 0,

    /// <summary>
    /// Social network provider returned access token
    /// </summary>
    TokenObtained = 1,

    /// <summary>
    /// Social network provider returned long term access token
    /// </summary>
    LongTermTokenObtained = 2,

    /// <summary>
    /// User connected/init with login token and logged-in
    /// </summary>
    LoggedIn = 3
  };


  /// <summary>
  /// Represents a vector of Network-provided params that can be used to reconstruct SocialUserInfo object
  /// </summary>
  public struct SocialUserInfoToken
  {
    public SocialUserInfoToken(string netUserID, string netLongToken) : this()
    {
      NetUserID = netUserID;
      NetLongToken = netLongToken;
    }

    public string NetUserID { get; internal set; }
    public string NetLongToken { get; internal set; }
  }


  /// <summary>
  /// Defines gender
  /// </summary>
  public enum Gender { MALE=0, FEMALE=1, UNSPECIFIED=100};

  /// <summary>
  /// Represents social network user common information
  /// </summary>
  [Serializable]
  public abstract class SocialUserInfo
  {
    protected SocialUserInfo(SocialNetwork issuer, SocialUserInfoToken? existingToken = null)
    {
      m_IssuerNetworkName = issuer.Name;
      if (existingToken.HasValue)
      {
        ID = existingToken.Value.NetUserID;
        LongTermProviderToken = existingToken.Value.NetLongToken;
        issuer.RetrieveUserInfo(this);
      }
    }

    private string m_IssuerNetworkName;//not a readonly field because of possible serialization issues

    /// <summary>
    /// Returns the name of the network that issued this info, i.e "FacebookOld".
    /// DO NOT confuse it with network ID. One can obtain netID by getting IssuerSocialNetworkID
    /// </summary>
    public string IssuerNetworkName { get { return m_IssuerNetworkName;} }

    /// <summary>
    /// Returns the SocialNetID for the social network instance that issued this user info object
    /// </summary>
    public SocialNetID IssuerNetworkID
    {
      get
      {
         var net = WebSettings.SocialNetworks[m_IssuerNetworkName];
         if (net==null) return SocialNetID.UNS;

         return net.ID;
      }
    }

    /// <summary>
    /// Returns the Sociatextual description for the social network instance that issued this user info object
    /// </summary>
    public string IssuerNetworkDescription
    {
      get
      {
         var net = WebSettings.SocialNetworks[m_IssuerNetworkName];
         if (net==null) return string.Empty;

         return net.Description;
      }
    }



    /// <summary>
    /// UserID in appropriate social Network
    /// Sample ID's are:
    ///   Google+:   100454735382076872928
    ///   Facebook:  100007030231661
    ///   Twitter:   2227913354
    ///   VKontakte: 229735500
    ///   Linked In: h2u4-ixYiC
    /// </summary>
    public string ID { get; internal set; }

    /// <summary>
    /// Pseudo-field (no social network has this field. It's composed from other fields in specific way for each network.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Not all social network supports this field
    /// </summary>
    public string Email { get; internal set; }

    /// <summary>
    /// Not all social network supports this field
    /// </summary>
    public string FirstName { get; internal set; }

    /// <summary>
    /// Not all social network supports this field
    /// </summary>
    public string LastName { get; internal set; }

    /// <summary>
    /// Not all social network supports this field
    /// </summary>
    public string MiddleName { get; internal set; }

    /// <summary>
    /// Not all social network supports this field
    /// </summary>
    public Gender? Gender { get; internal set; }

    /// <summary>
    /// Not all social network supports this field
    /// </summary>
    public DateTime? BirthDate { get; internal set; }

    /// <summary>
    /// Locale in form "en-gb".
    /// Not all social network supports this field
    /// </summary>
    public string Locale { get; internal set; }

    /// <summary>
    /// UTC offset in seconds.
    /// Not all social network supports this field
    /// </summary>
    public int? TimezoneOffset { get; internal set; }

    /// <summary>
    /// Web link to user profile picture ().
    /// Not all social network supports this field
    /// </summary>
    public string PictureLink { get; internal set; }

    /// <summary>
    /// Information used to perform social network operations like message post.
    /// Can be ordinal string (e.g. Facebook) or pair of strings (e.g. Twitter)
    /// </summary>
    public virtual string LongTermProviderToken
    {
      get { return string.Empty; }
      internal set { LoginState = SocialLoginState.LongTermTokenObtained; }
    }

    /// <summary>
    /// Returns a vector of Network-provided params that can be used to reconstruct SocialUserInfo object
    /// </summary>
    public SocialUserInfoToken Token { get { return new SocialUserInfoToken(this.ID, this.LongTermProviderToken);} }


    /// <summary>
    /// Indicates if user currently logged in
    /// </summary>
    public SocialLoginState LoginState { get; internal set; }

    /// <summary>
    /// Stores last exception of operations for this user
    /// </summary>
    public Exception LastError { get; internal set; }

    /// <summary>
    /// Social network specific debug info (for example, ID)
    /// </summary>
    public virtual string DebugInfo { get { return "{0}".Args(ID);}}

    /// <summary>
    /// Returns user profile image or null if no image available.
    /// Picture kind specifies classification of pictures within profile i.e. "main", "small-icon" etc.
    /// </summary>
    public System.Drawing.Image GetPicture(out string contentType, string pictureKind = null)
    {
      contentType = string.Empty;

      var net = WebSettings.SocialNetworks[m_IssuerNetworkName];
      if (net == null) return null;

      return net.GetPicture(this, out contentType, pictureKind);
    }

    /// <summary>
    /// Returns user profile image or null if no image available.
    /// Picture kind specifies classification of pictures within profile i.e. "main", "small-icon" etc.
    /// </summary>
    public System.Drawing.Image GetPicture(string pictureKind = null)
    {
      string dummy;
      return this.GetPicture(out dummy, pictureKind);
    }

    /// <summary>
    /// Posts message to social network
    /// </summary>
    public void PostMessage(string text)
    {
      var net = WebSettings.SocialNetworks[m_IssuerNetworkName];
      if (net == null) return;

      net.PostMessage(this, text);
    }

    #region Object overrides

      public override bool Equals(object obj)
      {
        var other = obj as SocialUserInfo;
        if (other == null) return false;

        var isEqual = this.IssuerNetworkID == other.IssuerNetworkID && this.ID == other.ID;

        isEqual &= string.Compare(this.Email, other.Email, true) == 0;
        isEqual &= string.Compare(this.FirstName, other.FirstName, false) == 0;
        isEqual &= string.Compare(this.LastName, other.LastName, false) == 0;
        isEqual &= string.Compare(this.MiddleName, other.MiddleName, false) == 0;
        isEqual &= this.Gender == other.Gender;
        isEqual &= this.BirthDate == other.BirthDate;
        isEqual &= string.Compare(this.Locale, other.Locale, false) == 0;
        isEqual &= this.TimezoneOffset == other.TimezoneOffset;
        isEqual &= string.Compare(this.LongTermProviderToken, other.LongTermProviderToken, false) == 0;
        isEqual &= this.LoginState == other.LoginState;

        return isEqual;
      }

      //public override string ToString()
      //{
      //  return "{0}, ID='{1}', DisplayName='{2}'".Args(IssuerNetworkID, ID, DisplayName);
      //}

      public override int GetHashCode()
      {
        return IssuerNetworkID.GetHashCode() ^ ID.GetHashCode();
      }

    #endregion


    #region Serialization to String

      /// <summary>
      /// Serializes current instance as string, i.e. this may be needed to store the instance in the database VARCHAR column
      /// </summary>
      public string SerializeToString()
      {
        using(var ms = new MemoryStream())
        {
          var ser = makeStringIntermediateSerializer();
          ser.Serialize(ms, this);
          return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Position);
        }
      }

      /// <summary>
      /// Deserializes instance from string, i.e. from database VARCHAR column
      /// </summary>
      public static T DeserializeFromString<T>(string data) where T: SocialUserInfo
      {
        if (data==null) return null;
        var buf = Convert.FromBase64String(data);
        var ser = makeStringIntermediateSerializer();
        return ser.Deserialize(new MemoryStream(buf)) as T;
      }

      private static SlimSerializer makeStringIntermediateSerializer()
      {
        return new SlimSerializer(TypeRegistry.BoxedCommonTypes, TypeRegistry.BoxedCommonNullableTypes, TypeRegistry.CommonCollectionTypes);
      }

    #endregion



  }//SocialUserInfo

}
