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
using System.Reflection;

using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Security
{

    /// <summary>
    /// Invoked by permission checker to get session
    /// </summary>
    public delegate ISession GetSessionFunc();


    /// <summary>
    /// Represents a general permission abstraction - where permission type represents the path/name of the permission
    ///  in User's rights and .ctor takes specific parameters to check while authorizing user.
    ///  Permission-derived class represents a certain permission type, whereas its instance is a check for particular desired level.
    ///  To authorize certain actions, one creates an instance of Permission-derived class passing in its .ctor required
    ///   access levels, then calls a Check() method that returns true if action is authorized.
    ///
    /// This scheme provides a great deal of flexibility, i.e. for very complex security cases developers may inherit leaf-level permissions from intermediate ones
    ///   that have logic tied to session-level variables, this way user's access may vary by permission/session state, i.e. a user may have
    ///    "Patient.Master" level 4 access in database "A", while having acess denied to the same named permission in database "B".
    /// User's database, or system instance is a flag in user-session context
    /// </summary>
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Interface |
                    AttributeTargets.Constructor |
                    AttributeTargets.Method |
                    AttributeTargets.Field |
                    AttributeTargets.Property, Inherited = true, AllowMultiple=true)]
    public abstract class Permission : Attribute
    {
        #region CONSTS

          public const string CONFIG_PERMISSIONS_SECTION = "permissions";
          public const string CONFIG_PERMISSION_SECTION = "permission";

        #endregion

        #region Static

           /// <summary>
           /// Checks the action represented by MemberInfo by checking the permission-derived attributes and returns false if
           /// any of authorization attributes do not pass
           /// </summary>
           public static bool AuthorizeAction(MemberInfo actionInfo, ISession session = null, GetSessionFunc getSessionFunc = null)
           {
             return FindAuthorizationFailingPermission(actionInfo, session, getSessionFunc) == null;
           }


                  private static Dictionary<MemberInfo, Permission[]> s_AttrCache = new Dictionary<MemberInfo,Permission[]>();


           /// <summary>
           /// Checks the action represented by MemberInfo by checking the permission-derived attributes and returns false if
           /// any of authorization attributes do not pass
           /// </summary>
           public static Permission FindAuthorizationFailingPermission(MemberInfo actionInfo, ISession session = null, GetSessionFunc getSessionFunc = null)
           { //20150124 DKh - added caching instead of reflection. Glue inproc binding speed improved 20%
             Permission[] permissions;
             if (!s_AttrCache.TryGetValue(actionInfo, out permissions))
             {
               permissions = actionInfo.GetCustomAttributes(typeof(Permission), true).Cast<Permission>().ToArray();
               var dict = new Dictionary<MemberInfo,Permission[]>(s_AttrCache);
               dict[actionInfo] = permissions;
               s_AttrCache = dict;//atomic
             }

             for(var i=0; i<permissions.Length; i++)
             {
               var permission = permissions[i];
               if (i==0 && session==null && getSessionFunc!=null) session = getSessionFunc();
               if (!permission.Check(session)) return permission;
             }
             return null;
           }

           ////20140124 DKh - added caching instead of reflection. Glue inproc binding speed improved 20%
           ///// <summary>
           ///// Checks the action represented by MemberInfo by checking the permission-derived attributes and returns false if
           ///// any of authorization attributes do not pass
           ///// </summary>
           //public static Permission FindAuthorizationFailingPermission(MemberInfo actionInfo, ISession session = null, GetSessionFunc getSessionFunc = null)
           //{
           //  var attrs = actionInfo.GetCustomAttributes(typeof(Permission), true).Cast<Permission>();

           //  var first = true;
           //  foreach(var attr in attrs)
           //  {
           //    if (first && session==null && getSessionFunc!=null) session = getSessionFunc();
           //    first = false;
           //    if (!attr.Check(session)) return attr;
           //  }
           //  return null;
           //}

           /// <summary>
           /// Guards the action represented by MemberInfo by checking the permission-derived attributes and throwing exception if
           /// any of authorization attributes do not pass
           /// </summary>
           public static void AuthorizeAndGuardAction(MemberInfo actionInfo, ISession session = null, GetSessionFunc getSessionFunc = null)
           {
             var failed = FindAuthorizationFailingPermission(actionInfo, session, getSessionFunc);

             if (failed!=null)
               throw new AuthorizationException(string.Format(StringConsts.SECURITY_AUTHROIZATION_ERROR, failed,  actionInfo.ToDescription()));
           }

           /// <summary>
           /// Guards the action represented by enumerable of permissions by checking all permissions and throwing exception if
           /// any of authorization attributes do not pass
           /// </summary>
           public static void AuthorizeAndGuardAction(IEnumerable<Permission> permissions, string actionName, ISession session = null, GetSessionFunc getSessionFunc = null)
           {
             if (permissions==null) return;


             if (session==null && permissions.Any() && getSessionFunc!=null) session = getSessionFunc();

             var failed = permissions.FirstOrDefault(perm => !perm.Check(session));

             if (failed!=null)
               throw new AuthorizationException(string.Format(StringConsts.SECURITY_AUTHROIZATION_ERROR, failed,  actionName ?? CoreConsts.UNKNOWN));
           }

           /// <summary>
           /// Makes multiple permissions from conf node
           /// </summary>
           public static IEnumerable<Permission> MultipleFromConf(IConfigSectionNode node,
                                                                  string shortNodeName = null,
                                                                  string typePattern = null
                                                                  )
           {
              if (node==null || !node.Exists) return Enumerable.Empty<Permission>();
              var result = new List<Permission>();
              foreach(var pnode in node.Children
                                       .Where(cn => cn.IsSameName(CONFIG_PERMISSION_SECTION) ||
                                                                  (shortNodeName.IsNotNullOrWhiteSpace() && cn.IsSameName(shortNodeName) )
                                             ))
                 result.Add( FactoryUtils.MakeUsingCtor<Permission>(pnode, typePattern) );
              return result;
           }


        #endregion


        #region .ctor

           /// <summary>
           /// Creates the check instance against the minimum access level for this permission
           /// </summary>
           protected Permission(int level)
           {
             m_Level = level;
           }

        #endregion


        #region Fields
            private int m_Level;
        #endregion

        #region Properties


            /// <summary>
            /// Returns the permission name - the last segment of the path
            /// </summary>
            public abstract string Name
            {
                get;
            }

            /// <summary>
            /// Returns the permission description - base implementation returns permission name
            /// </summary>
            public virtual string Description
            {
                get { return Name;}
            }

            /// <summary>
            /// Returns a top-rooted path to this permission (without name)
            /// </summary>
            public abstract string Path
            {
                get;
            }

            /// <summary>
            /// Returns full permission path - a concatenation of its path and name
            /// </summary>
            public string FullPath
            {
               get
               {
                 var path = Path;
                 if (path.EndsWith("/"))
                   return path + Name;
                 else
                   return path + "/" + Name;
               }
            }

            /// <summary>
            /// Specifies the minimum access level for the permission check to pass
            /// </summary>
            public int Level
            {
               get { return m_Level; }
            }

        #endregion

        #region Public

            /// <summary>
            /// Shortcut method that creates a temp/mock BaseSession object thus checking permission in mock BaseSession context
            /// </summary>
            public bool Check(User user)
            {
              if (user==null || !user.IsAuthenticated) return false;
              var session = new BaseSession(Guid.NewGuid());
              session.User = user;
              return this.Check(session);
            }

            /// <summary>
            /// Checks the permission for requested action as specified in particular permission .ctor.
            /// The check is performed in the scope of supplied session, or if no session was supplied then
            ///  current execution context session is assumed
            /// </summary>
            /// <returns>True when action is authorized, false otherwise</returns>
            public virtual bool Check(ISession sessionInstance = null)
            {
              var session = sessionInstance ?? ExecutionContext.Session ?? NOPSession.Instance;
              var user = session.User;

              //System user passes all permission checks
              if (user.Status==UserStatus.System) return true;

              var manager = App.SecurityManager;

              var access = manager.Authorize(user, this);

              if (access==null) return false;

              return DoCheckAccessLevel(session, access);
            }

            public override string ToString()
            {
                return FullPath;
            }

        #endregion

        #region Protected

            /// <summary>
            /// Override to perform access level checks per user's AccessLevel instance.
            /// True if  accessLevel satisfies permission requirements.
            /// The default implementation checks the access.Level
            /// </summary>
            protected virtual bool DoCheckAccessLevel(ISession session, AccessLevel access)
            {
              return access.Level >= m_Level;
            }

        #endregion

    }

    /// <summary>
    /// A general ancestor for all typed permissions - the ones declared in code
    /// </summary>
    public abstract class TypedPermission : Permission
    {
        public const string PERMISSION_SUFFIX = "Permission";//do not localize

        #region .ctor

           /// <summary>
           /// Creates the check instance against the minimum access level for this typed permission
           /// </summary>
           protected TypedPermission(int level) : base (level)
           {

           }


        #endregion


        #region Properties
            public override string Name
            {
                get
                {
                    var name = GetType().Name;
                    if (name.EndsWith(PERMISSION_SUFFIX) && name.Length > PERMISSION_SUFFIX.Length)//do not localize
                      name = name.Remove(name.Length - PERMISSION_SUFFIX.Length);

                    return name;
                }
            }

            public override string Path
            {
                get { return '/' + GetType().Namespace.Replace('.', '/'); }
            }
        #endregion
    }


    /// <summary>
    /// Represents a permission check instance which is a-typical and is based on string arguments
    /// </summary>
    public sealed class AdHocPermission : Permission
    {
        #region .ctor
            public AdHocPermission(string path, string name, int level) : base (level)
            {
              path = path ?? "/";
              name = name ?? CoreConsts.UNKNOWN;

              if (!path.StartsWith("/")) path = '/' + path;

              m_Path = path.Replace('.', '/').Replace('\\', '/');
              m_Name = name;
            }
        #endregion

        #region Fields

          private string m_Name;
          private string m_Path;

        #endregion

        #region Properties


            /// <summary>
            /// Returns the permission name - the last segment of the path
            /// </summary>
            public override string Name
            {
                get { return m_Name; }
            }

            /// <summary>
            /// Returns a top-rooted path to this permission (without name)
            /// </summary>
            public override string Path
            {
                get { return m_Path; }
            }

        #endregion

    }


}
