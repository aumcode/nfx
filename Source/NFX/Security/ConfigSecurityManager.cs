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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.Serialization.JSON;

namespace NFX.Security
{
  /// <summary>
  /// Provides security manager implementation that authenticates and authorizes users from configuration
  /// </summary>
  public class ConfigSecurityManager : ServiceWithInstrumentationBase<object>, ISecurityManagerImplementation
  {
    #region CONSTS
      public const string CONFIG_USERS_SECTION = "users";
      public const string CONFIG_USER_SECTION = "user";

      public const string CONFIG_RIGHTS_SECTION = Rights.CONFIG_ROOT_SECTION;
      public const string CONFIG_PERMISSION_SECTION = "permission";
      public const string CONFIG_PASSWORD_MANAGER_SECTION = "password-manager";

      public const string CONFIG_DESCRIPTION_ATTR = "description";
      public const string CONFIG_STATUS_ATTR = "status";
      public const string CONFIG_ID_ATTR = "id";
      public const string CONFIG_PASSWORD_ATTR = "password";
    #endregion

    #region .ctor
      /// <summary>
      /// Constructs security manager that authenticates users listed in application configuration
      /// </summary>
      public ConfigSecurityManager() : base() { }

      /// <summary>
      /// Constructs security manager that authenticates users listed in the supplied configuration section
      /// </summary>
      public ConfigSecurityManager(object director) : base(director) { }
    #endregion

    #region Fields
      private IConfigSectionNode m_Config;
      private IPasswordManagerImplementation m_PasswordManager;
      private bool m_InstrumentationEnabled;
    #endregion

    #region Properties

      [Config(Default = false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_PAY)]
      public override bool InstrumentationEnabled
      {
        get { return m_InstrumentationEnabled; }
        set { m_InstrumentationEnabled = value; }
      }

      public override string ComponentCommonName { get { return "secman"; } }

      /// <summary>
      /// Returns config node that this instance is configured from.
      /// If null is returned then manager performs authentication from application configuration
      /// </summary>
      public IConfigSectionNode Config { get { return m_Config; } }

      public IPasswordManager PasswordManager { get { return m_PasswordManager; } }

      [Config(Default = SecurityLogMask.Custom)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
      public SecurityLogMask LogMask { get; set;}

      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
      public Log.MessageType LogLevel { get; set;}


    #endregion

    #region Public

      public IConfigSectionNode GetUserLogArchiveDimensions(IIdentityDescriptor identity)
      {
        if (identity==null) return null;

        var cfg = new MemoryConfiguration();
        cfg.Create("ad");
        cfg.Root.AddAttributeNode("un", identity.IdentityDescriptorName);

        return cfg.Root;
      }

      public void LogSecurityMessage(SecurityLogAction action, Log.Message msg, IIdentityDescriptor identity = null)
      {
        if ((LogMask & action.ToMask()) == 0) return;
        if (msg==null) return;
        if (LogLevel > msg.Type) return;
        if (msg.ArchiveDimensions.IsNullOrWhiteSpace())
        {
          if (identity==null)
            identity = ExecutionContext.Session.User;

          msg.ArchiveDimensions = GetUserLogArchiveDimensions(identity).ToLaconicString();
        }

        logSecurityMessage(msg);
      }

      public User Authenticate(Credentials credentials)
      {
        var sect = m_Config ?? App.ConfigRoot[CommonApplicationLogic.CONFIG_SECURITY_SECTION];
        if (sect.Exists && credentials is IDPasswordCredentials)
        {
          var idpass = (IDPasswordCredentials)credentials;

          var usern = findUserNode(sect, idpass);

          if (usern.Exists)
          {
            var name = usern.AttrByName(CONFIG_NAME_ATTR).ValueAsString(string.Empty);
            var descr = usern.AttrByName(CONFIG_DESCRIPTION_ATTR).ValueAsString(string.Empty);
            var status = usern.AttrByName(CONFIG_STATUS_ATTR).ValueAsEnum<UserStatus>(UserStatus.Invalid);

            var rights = Rights.None;

            var rightsn = usern[CONFIG_RIGHTS_SECTION];

            if (rightsn.Exists)
            {
              var data = new MemoryConfiguration();
              data.CreateFromNode(rightsn);
              rights = new Rights(data);
            }

            return new User(credentials,
                            credToAuthToken(idpass),
                            status,
                            name,
                            descr,
                            rights);
          }
        }

        return new User(credentials,
                        new AuthenticationToken(),
                        UserStatus.Invalid,
                        StringConsts.SECURITY_NON_AUTHENTICATED,
                        StringConsts.SECURITY_NON_AUTHENTICATED,
                        Rights.None);
      }

      public User Authenticate(AuthenticationToken token)
      {
        var idpass = authTokenToCred(token);
        return Authenticate(idpass);
      }

      public void Authenticate(User user)
      {
        if (user == null) return;
        var token = user.AuthToken;
        var reuser = Authenticate(token);

        user.___update_status(reuser.Status, reuser.Name, reuser.Description, reuser.Rights);
      }

      public AccessLevel Authorize(User user, Permission permission)
      {
        if (user == null || permission == null)
          throw new SecurityException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Authorize(user==null|permission==null)");

        var node = user.Rights.Root.NavigateSection(permission.FullPath);
        return new AccessLevel(user, permission, node);
      }
    #endregion

    #region Protected
      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);
        m_Config = node;
        m_PasswordManager = FactoryUtils.MakeAndConfigure<IPasswordManagerImplementation>(node[CONFIG_PASSWORD_MANAGER_SECTION], typeof(DefaultPasswordManager), new object[] { this });
      }

      protected override void DoStart()
      {
        m_PasswordManager.Start();
      }

      protected override void DoSignalStop()
      {
        m_PasswordManager.SignalStop();
      }

      protected override void DoWaitForCompleteStop()
      {
        m_PasswordManager.WaitForCompleteStop();
      }
    #endregion

    #region Private
      private IConfigSectionNode findUserNode(IConfigSectionNode securityRootNode, IDPasswordCredentials cred)
      {
        var users = securityRootNode[CONFIG_USERS_SECTION];

        using (var password = cred.SecurePassword)
        {
          bool needRehash = false;
          return users.Children.FirstOrDefault(cn => cn.IsSameName(CONFIG_USER_SECTION)
                                                  && string.Equals(cn.AttrByName(CONFIG_ID_ATTR).Value, cred.ID, StringComparison.InvariantCulture)
                                                  && m_PasswordManager.Verify(password, HashedPassword.FromString(cn.AttrByName(CONFIG_PASSWORD_ATTR).Value), out needRehash)
                                              ) ?? users.Configuration.EmptySection;

        }
      }

      private AuthenticationToken credToAuthToken(IDPasswordCredentials cred)
      {
        return new AuthenticationToken(this.GetType().FullName, "{0}\n{1}".Args(cred.ID, cred.Password));
      }

      private IDPasswordCredentials authTokenToCred(AuthenticationToken token)
      {
        if (token.Data == null)
          return new IDPasswordCredentials(string.Empty, string.Empty);

        var seg = token.Data.ToString().Split('\n');

        if (seg.Length < 2)
          return new IDPasswordCredentials(string.Empty, string.Empty);

        return new IDPasswordCredentials(seg[0], seg[1]);
      }


      private void logSecurityMessage(Log.Message msg)
      {
        msg.Channel = CoreConsts.LOG_CHANNEL_SECURITY;
        msg.From = "{0}.{1}".Args(GetType().Name, msg.From);
        App.Log.Write(msg);
      }

    #endregion




  }
}
