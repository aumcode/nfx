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
using System.IO;
using System.Reflection;
using System.Net;

using NFX.ApplicationModel;
using NFX.Templatization;
using NFX.Environment;

namespace NFX.Wave.Handlers
{
    /// <summary>
    /// Represents a base handler for all handlers that dynamicaly resolve type that performs actual work
    /// </summary>
    public abstract class TypeLookupHandler<TTarget> : WorkHandler where TTarget : class
    {
       #region CONSTS
         public const string VAR_TARGET_TYPE = "type";
         public const string VAR_INSTANCE_ID = "instanceID";

         public const string CONFIG_DEFAULT_TYPE_ATTR = "default-type";
         public const string CONFIG_CLOAK_TYPE_ATTR = "cloak-type";
         public const string CONFIG_NOT_FOUND_REDIRECT_URL_ATTR = "not-found-redirect-url";

       #endregion


       #region .ctor

         protected TypeLookupHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                              : base(dispatcher, name, order, match)
         {

         }

         protected TypeLookupHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
         {
          if (confNode==null)
            throw new WaveException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(confNode==null)");

          foreach(var ntl in confNode.Children.Where(cn=>cn.IsSameName(TypeLocation.CONFIG_TYPE_LOCATION_SECTION)))
            m_TypeLocations.Register( FactoryUtils.Make<TypeLocation>(ntl, typeof(TypeLocation), args: new object[]{ ntl }) );

          m_DefaultTypeName = confNode.AttrByName(CONFIG_DEFAULT_TYPE_ATTR).Value;
          m_CloakTypeName = confNode.AttrByName(CONFIG_CLOAK_TYPE_ATTR).Value;
          m_NotFoundRedirectURL = confNode.AttrByName(CONFIG_NOT_FOUND_REDIRECT_URL_ATTR).Value;
         }

       #endregion

       #region Fields

         private string m_DefaultTypeName;
         private string m_CloakTypeName;
         private string m_NotFoundRedirectURL;
         private TypeLookup m_Lookup = new TypeLookup();
         private TypeLocations m_TypeLocations = new TypeLocations();

       #endregion


       #region Properties

            /// <summary>
            /// Indicates whether instance IDs are supported in requests. Default is false.
            /// Override to return true for handlers that support target instance state between requests
            /// </summary>
            public virtual bool SupportsInstanceID
            {
              get { return false; }
            }

            /// <summary>
            /// Returns a registry of type locations
            /// </summary>
            public TypeLocations TypeLocations
            {
              get { return m_TypeLocations;}
            }

            /// <summary>
            /// Provides default type name
            /// </summary>
            public string DefaultTypeName
            {
              get { return m_DefaultTypeName ?? string.Empty;}
              set { m_DefaultTypeName = value;}
            }

            /// <summary>
            /// Provides type name which is used if the prior one was not found. This allows to block 404 errors,
            /// i.e. if page with requested name is not found then always return the specified page
            /// </summary>
            public string CloakTypeName
            {
              get { return m_CloakTypeName ?? string.Empty;}
              set { m_CloakTypeName = value;}
            }

            /// <summary>
            /// Provides redirect URL where the user gets redirected when type name could not be resolved.
            /// Note: CloakTypeName is used first when set.
            /// </summary>
            public string NotFoundRedirectURL
            {
              get { return NotFoundRedirectURL ?? string.Empty;}
              set { NotFoundRedirectURL = value;}
            }


       #endregion


       #region Protected

            /// <summary>
            /// Sealed. Override DoTargetWork(TTarget, WorkContext) to do actual work
            /// </summary>
            protected sealed override void DoHandleWork(WorkContext work)
            {
               Exception error = null;
               Guid iid = Guid.Empty;
               bool needInstanceCheckin = false;

               TTarget target = null;

               try
               {
                  try
                  {
                       if (SupportsInstanceID && GetTargetInstanceID(work, ref iid))
                       {
                         //iid has object ID of instance
                         target = GetTargetInstanceByID(work, iid);
                         needInstanceCheckin = true;

                         if (target!=null)
                            Security.Permission.AuthorizeAndGuardAction(target.GetType(), work.Session, ()=> work.NeedsSession() );
                       }


                       if (target==null)
                       {
                           var tt = GetTargetType(work);

                           if (tt==null || tt.IsAbstract)
                           {
                             if (m_NotFoundRedirectURL.IsNotNullOrWhiteSpace())
                             {
                                work.Response.RedirectAndAbort(m_NotFoundRedirectURL);
                                return;
                             }

                             error = Do404(work);
                             return;
                           }

                           Security.Permission.AuthorizeAndGuardAction(tt, work.Session, ()=> work.NeedsSession() );

                           target = CreateTargetInstance(work, tt);
                           if (SupportsInstanceID)
                           {
                              iid = Guid.NewGuid();
                              needInstanceCheckin = true;
                           }

                       }

                       if (target==null)
                       {
                           error = Do404(work);
                           return;
                       }


                       DoTargetWork(target, work);

                  }
                  catch(Exception err)
                  {
                     error = err;
                  }
               }
               finally
               {
                 try
                 {
                   if (error!=null)
                     DoError(work, error);
                 }
                 finally
                 {
                   if (target!=null && needInstanceCheckin)
                   {
                     var end = false;
                     if (target is IEndableInstance)
                      end = ((IEndableInstance)target).IsEnded;

                     if (end)
                        DeleteTargetInstanceByID(work, iid);
                     else
                        PutTargetInstanceByID(work, iid, target);
                   }
                 }
               }
            }



           /// <summary>
           /// Performs work on the target instance
           /// </summary>
           protected abstract void DoTargetWork(TTarget target, WorkContext work);



           /// <summary>
           /// Override to extract instance ID from WorkContext
           /// </summary>
           protected virtual bool GetTargetInstanceID(WorkContext work, ref Guid id)
           {
             return getTargetInstanceID(work, ref id);
           }


           /// <summary>
           /// Override to resolve route/URL parameters to type
           /// </summary>
           protected virtual Type GetTargetType(WorkContext work)
           {
             var tname = GetTargetTypeNameFromWorkContext(work);

             var result = getTargetType(work, tname);
             if(result!=null && !result.IsAbstract) return result;

             if (m_CloakTypeName.IsNotNullOrWhiteSpace())
              result = getTargetType(work, m_CloakTypeName);

             return result;
           }

           /// <summary>
           /// Override to get type name from WorkContext. Default implementation looks for MatchedVars[VAR_TARGET_TYPE]
           /// </summary>
           protected virtual string GetTargetTypeNameFromWorkContext(WorkContext work)
           {
             var result = work.MatchedVars[VAR_TARGET_TYPE].AsString();

             if (result.IsNullOrWhiteSpace())
              result = DefaultTypeName;

             //20160217 DKh
             var match = work.Match;
             if (match!=null && match.TypeNsPrefix.IsNotNullOrWhiteSpace())
             {
                var pfx = match.TypeNsPrefix;

                if (pfx[pfx.Length-1]!='/' && pfx[pfx.Length-1]!='\\') pfx = pfx + '/';

                result = pfx + result;
             }

             return result;
           }


           /// <summary>
           /// Factory method - Override to create and init more particular template implementation (i.e. based on model)
           /// </summary>
           protected virtual TTarget CreateTargetInstance(WorkContext work, Type tt)
           {
              return  Activator.CreateInstance(tt) as TTarget;
           }

           /// <summary>
           /// Retrieves target instance from id. Default implementation uses Application.ObjectStore.
           /// If this method is called by the framework then complementary matching PutTargetInstanceWithID() is guaranteed to be called as well
           /// </summary>
           protected virtual TTarget GetTargetInstanceByID(WorkContext work, Guid id)
           {
              return App.ObjectStore.CheckOut(id) as TTarget;
           }

           /// <summary>
           /// Puts target instance with id into store. Default implementation uses Application.ObjectStore.
           /// This method is called by the framework to complement GetTargetInstanceFromID()
           /// </summary>
           protected virtual void PutTargetInstanceByID(WorkContext work, Guid id, TTarget target, int msTimeout = 0)
           {
              App.ObjectStore.CheckIn(id, target, msTimeout);
           }

           /// <summary>
           /// Deletes target instance with id from store. Default implementation uses Application.ObjectStore.
           /// </summary>
           protected virtual void DeleteTargetInstanceByID(WorkContext work, Guid id)
           {
              App.ObjectStore.Delete(id);
           }

           /// <summary>
           /// Override to handle 404 condition, i.e. may write into response instead of generating a exception.
           /// The default implementation returns a HttpStatusException with 404 code
           /// </summary>
           protected virtual HTTPStatusException Do404(WorkContext context)
           {
              return new HTTPStatusException(Web.WebConsts.STATUS_404, Web.WebConsts.STATUS_404_DESCRIPTION);
           }

           /// <summary>
           /// Override to handle error processing, i.e. may elect to write error data into response.
           /// The dafault implementation throws the error. It is recommended to handle errors with filters instead
           /// </summary>
           protected virtual void DoError(WorkContext work, Exception error)
           {
              throw error;
           }

       #endregion


       #region .pvt .impl

            //extracts guid from type name '(guid)' pattern
            private bool getTargetInstanceID(WorkContext work, ref Guid id)
            {
              string name = GetTargetTypeNameFromWorkContext(work);

              var j = name.IndexOf('(');
              if (j<0) return false;
              var k = name.IndexOf(')');
              if (k<j) return false;

              if (k-j < 32) return false;

              name = name.Substring(j+1, k-j-2);

              if (Guid.TryParse(name, out id)) return true;

              return false;
            }


            private Type getTargetType(WorkContext work, string typeName)
            {
              const string PORTAL_PREFIX = @"!#PORTAL\";

              if (typeName.IsNullOrWhiteSpace()) return null;
              Type result = null;
              string key;

              if (work.Portal==null)
                key = typeName;
              else
                key = PORTAL_PREFIX+work.Portal.Name+typeName;


              //1 Lookup in cache
              if (m_Lookup.TryGetValue(key, out result)) return result;

              //2 Lookup in locations
              result = lookupTargetInLocations(work, typeName);
              if (result!=null)
              {
                var lookup = new TypeLookup(m_Lookup);//thread safe copy

                lookup[key] =  result;//other thread may have added already
                m_Lookup = lookup;//atomic
                return result;
              }

              return null;//404 error - type not found
            }


            private Type lookupTargetInLocations(WorkContext work, string typeName)
            {
              if (!isValidTypeNameKey(typeName)) return null;

              string portal = null;
              if (work.Portal!=null)
               portal = work.Portal.Name;

              var clrTName = getCLRTypeName(typeName);

              while(true)
              {
                  foreach(var loc in  m_TypeLocations.OrderedValues)
                  {
                      if (portal!=null)
                      {
                        if (!portal.EqualsOrdIgnoreCase(loc.Portal)) continue;
                      }
                      else
                      {
                        if (loc.Portal.IsNotNullOrWhiteSpace()) continue;
                      }

                      var asm = loc.Assembly;
                      if (asm==null)
                        asm = Assembly.LoadFrom(loc.AssemblyName);

                      var namespaces = loc.Namespaces;
                      if (namespaces!=null && namespaces.Any())
                      {
                        foreach(var ns in loc.Namespaces)
                        {
                          var nsn = ns.Trim();
                          if (!nsn.EndsWith("."))
                            nsn+='.';
                          var result = asm.GetType(nsn + clrTName, false, true);
                          if (result!=null) return result;
                        }
                      }
                      else
                      {
                          return asm.GetType(clrTName, false, true);
                      }
                  }

                  if (portal == null) break;
                  portal = null;
              }//while


              return null;
            }


            private bool isValidTypeNameKey(string key)
            {
              if (key==null) return false;

              for(var i=0; i<key.Length; i++)
              {
                var c = key[i];
                if (c < '-') return false;
                if (c > '9' && c < 'A') return false;
                if (c > 'Z' && c < 'a' && c != '\\') return false;
                if (c > 'z' && c < 'À') return false;
              }

              return true;
            }

            private string getCLRTypeName(string key)
            {
              var cname = Path.GetFileNameWithoutExtension(key);
              var ns = Path.GetDirectoryName(key);

              ns = ns.Replace('/','.').Replace('\\','.').Trim('.');

              var fullName =  string.IsNullOrWhiteSpace(ns)? cname :  ns + '.'+ cname;

              return fullName.Replace('-', '_');
            }



       #endregion
    }
}
