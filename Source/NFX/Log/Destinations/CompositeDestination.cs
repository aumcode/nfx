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

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Provides an abstraction of a wrap around another destinations
    /// </summary>
    public class CompositeDestination : Destination
    {
       #region .ctor

            public CompositeDestination()
            {
            }

            public CompositeDestination(params Destination[] inner)
            {
              m_Destinations.AddRange(inner);
            }

            public CompositeDestination(string name, params Destination[] inner) : base (name)
            {
              m_Destinations.AddRange(inner);
            }

            protected override void Destructor()
            {
              base.Destructor();
            }

       #endregion


       #region Pvt/Protected Fields

            private LogServiceBase.DestinationList m_Destinations = new LogServiceBase.DestinationList();

       #endregion


       #region Properties
            /// <summary>
            /// Returns destinations that this destination wraps. This call is thread safe
            /// </summary>
            public IEnumerable<Destination> Destinations
            {
              get { lock(m_Destinations) return m_Destinations.ToList(); }
            }

       #endregion

       #region Public

            public override void Open()
            {
                base.Open();

                lock(m_Destinations)
                 foreach(var dest in m_Destinations)
                    dest.Open();
            }

            public override void Close()
            {
               lock(m_Destinations)
                 foreach(var dest in m_Destinations)
                    dest.Close();
                base.Close();
            }

            public override void TimeChanged()
            {
               lock(m_Destinations)
                 foreach(var dest in m_Destinations)
                    dest.TimeChanged();
            }

            public override void SettingsChanged()
            {
                lock(m_Destinations)
                 foreach(var dest in m_Destinations)
                    dest.SettingsChanged();
            }


            /// <summary>
            /// Adds a destination to this wrapper
            /// </summary>
            public void RegisterDestination(Destination dest)
            {
              lock (m_Destinations)
              {
                if (!m_Destinations.Contains(dest))
                {
                  m_Destinations.Add(dest);
                  dest.m_Owner = this;
                }
              }
            }

            /// <summary>
            /// Removes a destiantion from this wrapper, returns true if destination was found and removed
            /// </summary>
            public bool UnRegisterDestination(Destination dest)
            {
              lock (m_Destinations)
              {
                bool r = m_Destinations.Remove(dest);
                if (r) dest.m_Owner = null;
                return r;
              }
            }

       #endregion


      #region Protected

            protected override void DoConfigure(Environment.IConfigSectionNode node)
            {
                base.DoConfigure(node);

                  foreach (var dnode in node.Children.Where(n => n.Name.EqualsIgnoreCase(LogServiceBase.CONFIG_DESTINATION_SECTION)))
                  {
                    var dest = FactoryUtils.MakeAndConfigure(dnode) as Destination;
                    this.RegisterDestination(dest);
                  }

            }



            protected internal override void DoSend(Message entry)
            {
                lock(m_Destinations)
                   foreach(var dest in m_Destinations)
                        dest.DoSend(entry);
            }

      #endregion

    }
}
