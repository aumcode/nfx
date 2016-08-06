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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NFX.Environment;

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Infrastructure class - not for app developers.
    /// Resolves Query objects into query handlers. Query names are case-insensitive.
    /// This class is thread-safe
    /// </summary>
    public sealed class QueryResolver : ICRUDQueryResolver, IConfigurable
    {
        #region CONSTS
            public const string CONFIG_QUERY_RESOLVER_SECTION = "query-resolver";
            public const string CONFIG_HANDLER_LOCATION_SECTION = "handler-location";
            public const string CONFIG_SCRIPT_ASM_ATTR = "script-assembly";
            public const string CONFIG_NS_ATTR = "ns";
        #endregion

        #region .ctor
            public QueryResolver(ICRUDDataStoreImplementation dataStore)
            {
                m_DataStore = dataStore;
            }
        #endregion

        #region Fields
            private volatile bool m_Started;

            private string m_ScriptAssembly;
            private ICRUDDataStoreImplementation m_DataStore;
            private List<string> m_Locations = new List<string>();
            private Registry<ICRUDQueryHandler> m_Handlers = new Registry<ICRUDQueryHandler>();

        #endregion


        #region Properties

            /// <summary>
            /// Gets sets name of assembly that query scripts resolve from
            /// </summary>
            public string ScriptAssembly
            {
                get { return m_ScriptAssembly;}
                set
                {
                  checkNotStarted();
                  m_ScriptAssembly = value;
                }
            }

            public IList<string> HandlerLocations
            {
                get { return m_Locations;}
            }

            public IRegistry<ICRUDQueryHandler> Handlers
            {
                get { return m_Handlers;}
            }

        #endregion


        #region Public

            /// <summary>
            /// Registers handler location. The Resolver must be not started yet. This method is NOT thread safe
            /// </summary>
            public void RegisterHandlerLocation(string location)
            {
              checkNotStarted();
              if (location.IsNullOrWhiteSpace() || m_Locations.Contains(location, StringComparer.InvariantCultureIgnoreCase )) return;
              m_Locations.Add(location);
            }

            /// <summary>
            /// Unregisters handler location returning true if it was found and removed. The Resolve must be not started yet. This method is NOT thread safe
            /// </summary>
            public bool UnregisterHandlerLocation(string location)
            {
              checkNotStarted();
              if (location.IsNullOrWhiteSpace()) return false;
              return m_Locations.RemoveAll((s) => s.EqualsIgnoreCase(location)) > 0;
            }


            public ICRUDQueryHandler Resolve(Query query)
            {
                m_Started = true;
                var name = query.Name;
                try
                {
                    var result = m_Handlers[name];
                    if (result!=null) return result;

                    result = searchForType(name);

                    if (result==null)//did not find handler yet
                        result = searchForScript(name);

                    if (result==null)//did not find handler yet
                        throw new CRUDException(StringConsts.CRUD_QUERY_RESOLUTION_NO_HANDLER_ERROR);

                    m_Handlers.Register(result);
                    return result;
                }
                catch(Exception error)
                {
                    throw new CRUDException(StringConsts.CRUD_QUERY_RESOLUTION_ERROR.Args(name, error.ToMessageWithType()), error);
                }
            }

            public void Configure(IConfigSectionNode node)
            {
                checkNotStarted();

                m_ScriptAssembly = node.AttrByName(CONFIG_SCRIPT_ASM_ATTR).ValueAsString( Assembly.GetCallingAssembly().FullName );
                foreach(var lnode in node.Children.Where(cn => cn.IsSameName(CONFIG_HANDLER_LOCATION_SECTION)))
                {
                  var loc = lnode.AttrByName(CONFIG_NS_ATTR).Value;
                  if (loc.IsNotNullOrWhiteSpace())
                    m_Locations.Add( loc );
                  else
                    App.Log.Write(Log.MessageType.Warning, StringConsts.CRUD_CONFIG_EMPTY_LOCATIONS_WARNING, "CRUD", "QueryResolver.Configure()");
                }

            }

        #endregion

        #region .pvt

            private void checkNotStarted()
            {
                if (m_Started)
                 throw new CRUDException(StringConsts.CRUD_QUERY_RESOLVER_ALREADY_STARTED_ERROR);
            }


            private ICRUDQueryHandler searchForType(string name)
            {
                foreach(var loc in m_Locations)
                {
                    var ns = loc;
                    var asm = string.Empty;
                    var ic = loc.IndexOf(',');
                    if(ic>0)
                    {
                        ns = loc.Substring(0, ic);
                        asm = loc.Substring(ic+1);
                    }

                    var tname = asm.IsNullOrWhiteSpace() ? "{0}.{1}".Args(ns, name) : "{0}.{1}, {2}".Args(ns, name, asm);
                    var t = Type.GetType(tname, false, true);
                    if (t!=null)
                    {
                        if (typeof(ICRUDQueryHandler).IsAssignableFrom(t))
                        {
                            return Activator.CreateInstance(t, m_DataStore) as ICRUDQueryHandler;
                        }
                    }
                }
                return null;
            }

            private ICRUDQueryHandler searchForScript(string name)
            {
                var asm = Assembly.Load(m_ScriptAssembly);
                var asmname = asm.FullName;
                var ic = asmname.IndexOf(',');
                if (ic>0)
                 asmname = asmname.Substring(0, ic);
                var resources = asm.GetManifestResourceNames();

                name = name + m_DataStore.ScriptFileSuffix;

                var res = resources.FirstOrDefault(r => r.EqualsIgnoreCase(name) || r.EqualsIgnoreCase(asmname+"."+name));

                if (res!=null)
                {
                    using (var stream = asm.GetManifestResourceStream(res))
                      using (var reader = new StreamReader(stream))
                      {
                         var script = reader.ReadToEnd();
                         var qsource = new QuerySource(name, script);
                         return m_DataStore.MakeScriptQueryHandler(qsource);
                      }
                }
                return null;
            }

        #endregion
    }


}
