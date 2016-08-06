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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.3  2009.10.12
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Runtime.Serialization;

namespace NFX.Security
{
      /// <summary>
      /// Provides base user functionality. Particular security manager implementations may return users derived from this class
      /// </summary>
      [Serializable]
      public class User : IIdentity, IPrincipal, IDeserializationCallback
      {
        #region Static
          private static readonly User s_FakeUserInstance = new User(BlankCredentials.Instance,
                                              new AuthenticationToken(),
                                              UserStatus.Invalid,
                                                "John Doe",
                                                "Fake user",
                                                NFX.Security.Rights.None);

          /// <summary>
          /// Returns default instance of the fake user that has no rights
          /// </summary>
          public static User Fake{ get{ return s_FakeUserInstance; } }
        #endregion


        #region .ctor
          private User() { } //for quicker serialization

          public User(Credentials credentials,
                      AuthenticationToken token,
                      UserStatus status,
                      string name,
                      string descr,
                      Rights rights)
          {
              m_Credentials = credentials;
              m_AuthenticationToken = token;
              m_Status = status;
              m_Name = name;
              m_Description = descr;
              m_Rights = rights;
              m_StatusTimeStampUTC = App.TimeSource.UTCNow;
          }

          public User(Credentials credentials,
                      AuthenticationToken token,
                      string name,
                      Rights rights) : this(credentials, token, UserStatus.User, name, null, rights)
          {

          }
        #endregion


        #region Private Props/Members

          private DateTime m_StatusTimeStampUTC;

          private Credentials m_Credentials;
          private AuthenticationToken m_AuthenticationToken;

          private UserStatus m_Status;
          private string m_Name;

          private string m_Description;

          [NonSerialized]//Important, rights are NOT serializable
          private Rights m_Rights;

        #endregion


        #region Properties

          /// <summary>
          /// Captures timestamp when this user was set to current status (created/set rights)
          /// Security managers may elect to reftech user rights after some period
          /// </summary>
          public DateTime StatusTimeStampUTC
          {
            get{ return m_StatusTimeStampUTC;}
          }

          public Credentials Credentials
          {
            get { return m_Credentials ?? BlankCredentials.Instance; }
          }

          public AuthenticationToken AuthToken
          {
            get { return m_AuthenticationToken; }
          }

          public string Name
          {
            get { return m_Name ?? string.Empty; }
          }

          public string Description
          {
            get { return m_Description ?? string.Empty; }
          }

          public UserStatus Status
          {
            get { return m_Status; }
          }


          /// <summary>
          /// Returns data bag that contains user rights. This is a framework-only internal property
          ///  which should not be used by application developers. This bag may get populated fully-or-partially
          ///   by ISecurityManager implementation. Use User[permission] indexer or Application.SecurityManager.Authorize()
          ///    to obtain AccessLevel
          /// </summary>
          public Rights Rights
          {
            get { return m_Rights ?? NFX.Security.Rights.None; }
          }

          /// <summary>
          /// Authorizes user to specified permission.
          /// Note: this authorization call returns AccessLevel object that may contain a complex data structure.
          /// The final assertion of user's ability to perform a certain action is encapsulated in Permission.Check() method.
          /// Call Permission.AuthorizeAndGuardAction(MemberInfo, ISession) to guard classes and methods from unauthorized access
          /// </summary>
          public AccessLevel this[Permission permission]
          {
              get { return App.SecurityManager.Authorize(this, permission); }
          }
        #endregion

        #region Pub

          /// <summary>
          /// Makes user invalid
          /// </summary>
          public void Invalidate()
          {
            m_Status = UserStatus.Invalid;
          }

          /// <summary>
          /// Framework-internal. Do not call
          /// </summary>
          public void ___update_status(
                        UserStatus status,
                        string name,
                        string descr,
                        Rights rights)
            {
                if (object.ReferenceEquals(this, s_FakeUserInstance)) return;//Fake user is immutable
                m_Status = status;
                m_Name = name;
                m_Description = descr;
                m_Rights = rights;
                m_StatusTimeStampUTC = App.TimeSource.UTCNow;
            }


          public override string ToString()
          {
            return "[{0}]{1},{2}".Args(Status, Name, Description);
          }


        #endregion

    #region IIdentity Members

      /// <summary>
      /// Implementation of IIdentity interface
      /// </summary>
      public string AuthenticationType
      {
        get { return m_AuthenticationToken.Realm ?? string.Empty; }
      }

      /// <summary>
      /// Implementation of IIdentity interface. Checks to see if user is not in invalid status
      /// </summary>
      public bool IsAuthenticated
      {
        get { return m_Status != UserStatus.Invalid; }
      }

    #endregion

    #region IPrincipal Members

        /// <summary>
        /// Implementation of IPrincipal interface
        /// </summary>
        public IIdentity Identity
        {
          get { return this; }
        }

        /// <summary>
        /// Determines whether the current principal belongs to the specified role.
        /// This method implements IPrincipal and has little application in NFX framework context
        /// as NFX permissions are more granular than just boolean. This method really checks user kind (User/Admin/Sys).
        /// Confusion comes from the fact that what Microsoft calls role really is just a single named permission -
        ///  a role is a named permission set in NFX.
        /// </summary>
        public bool IsInRole(string role)
        {
          return m_Status.ToString().ToUpperInvariant() == role.ToUpperInvariant();
        }

    #endregion

    #region IDeserializationCallback
        public void OnDeserialization(object sender)
        {
            //Re-authorizes user re-fetches Rights (Rights are not serializable)
            App.SecurityManager.Authenticate(this);
        }
    #endregion
      }
}
