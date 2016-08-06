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
using System.Linq;
using System.Text;

namespace NFX.ApplicationModel
{




    /// <summary>
    /// Provides marker contract requirement for an ApplicationComponent.
    /// This interface must be implemented only by ApplicationComponent class
    /// </summary>
    public interface IApplicationComponent : IDisposable
    {
        /// <summary>
        /// Returns process/instance unique app component system id
        /// </summary>
        ulong ComponentSID { get; }


        /// <summary>
        /// Returns a reference to an object that this app component services/operates under, or null
        /// </summary>
        object ComponentDirector { get;}

        /// <summary>
        /// Returns the common name used to identify the component, for example "Glue" for various IGlue implementations.
        /// This name is searched-by some management tools that allow to find component by this name that does not change between
        /// application restarts like ComponentSID does. Subordinate (non-root) components return null
        /// </summary>
        string ComponentCommonName { get; }
    }



    /// <summary>
    /// An abstraction of a disposable application component - major implementation/functionality part of an app.
    /// Components logically subdivide application container so their instances may be discovered
    ///  by other parties, for example: one may iterate over all components in an application that support instrumentation and logging.
    ///  Services are sub-types of components.
    /// Use "ApplicationComponent.AllComponents" to access all components in the container
    /// </summary>
    public abstract class ApplicationComponent : DisposableObject, IApplicationComponent
    {
        #region .ctor

          protected ApplicationComponent()
          {
             ctor();
          }

          protected ApplicationComponent(object director)
          {
             m_ComponentDirector = director;
             ctor();
          }

          protected override void Destructor()
          {
            lock(s_Instances)
             s_Instances.Remove(m_ComponentSID);
          }

          private void ctor()
          {
            m_ComponentStartTime = DateTime.Now;
            lock(s_Instances)
            {
              s_SIDSeed++;
              m_ComponentSID = s_SIDSeed;
              s_Instances.Add(m_ComponentSID, this);
            }
          }

        #endregion

        #region Private Fields

          private static ulong s_SIDSeed;
          private static Dictionary<ulong, ApplicationComponent> s_Instances = new Dictionary<ulong, ApplicationComponent>();

          [NonSerialized] private DateTime m_ComponentStartTime;
          [NonSerialized] private ulong m_ComponentSID;
          [NonSerialized] private object m_ComponentDirector;
        #endregion

        #region Properties

          /// <summary>
          /// Returns a thread-safe enumerable( a snapshot) of all known component instances
          /// </summary>
          public static IEnumerable<ApplicationComponent> AllComponents
          {
            get
            {
              ApplicationComponent[] result;
              lock(s_Instances)
               result = s_Instances.Values.ToArray();
              return result;
            }
          }

          /// <summary>
          /// Looks-up an existing application component instance by its ComponentSID or null
          /// </summary>
          public static ApplicationComponent GetAppComponentBySID(ulong sid)
          {
            ApplicationComponent result;

            lock(s_Instances)
             if (s_Instances.TryGetValue(sid, out result)) return result;

            return null;
          }

          /// <summary>
          /// Looks-up an existing application component instance by its ComponentCommonName or null. The search is case-insensitive
          /// </summary>
          public static ApplicationComponent GetAppComponentByCommonName(string name)
          {
            if (name.IsNullOrWhiteSpace()) return null;

            name = name.Trim();

            ApplicationComponent result;

            lock(s_Instances)
             result = s_Instances.Values.FirstOrDefault( c => c.ComponentCommonName!=null && name.EqualsIgnoreCase(c.ComponentCommonName.Trim()));

            return result;
          }


          /// <summary>
          /// Returns process/instance unique app component system id
          /// </summary>
          public ulong ComponentSID { get { return m_ComponentSID;}}


          /// <summary>
          /// Returns local computer time of component start (not from application container time)
          /// </summary>
          public DateTime ComponentStartTime { get {return m_ComponentStartTime; }}

          /// <summary>
          /// Returns the common name used to identify the component, for example "Glue" for various IGlue implementations.
          /// This name is searched-by some management tools that allow to find component by this name that does not change between
          /// application restarts like ComponentSID does. Subordinate (non-root) components return null
          /// </summary>
          public virtual string ComponentCommonName { get {return null;}}


          /// <summary>
          /// Returns a reference to an object that this app component services/operates under, or null
          /// </summary>
          public object ComponentDirector
          {
            get { return m_ComponentDirector; }
          }
          internal void __setComponentDirector(object director) { m_ComponentDirector = director; }


        #endregion
    }
}
