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


// Author: Serge Aleynikov
// Created: 2013-05-02
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Throttling
{
    /// <summary>
    /// Base class defining throttle interface
    /// </summary>
    public abstract class Throttle : ApplicationComponent, IConfigurable, IThrottle
    {
        #region CONSTS

            internal const string DEFAULT_UNITS = "times";

        #endregion

        #region .ctor

            protected Throttle() : base()
            {
            }

            /// <summary>
            /// Constructor that identifies throttle by name, and optionally defines the
            /// unit of measurement for this throttle instance
            /// </summary>
            protected Throttle(string name, string unit = null):base()
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new NFXException(StringConsts.ARGUMENT_ERROR + "Throttle.ctor(name == NULL)");
                m_Name = name;
                m_Unit = unit ?? DEFAULT_UNITS;
            }

            protected override void Destructor()
            {
                Unregister();
                base.Destructor();
            }

        #endregion

        #region Private

            private IThrottlingImplementation m_ThrottlingImpl; internal void __setThrottling(IThrottlingImplementation ti) { m_ThrottlingImpl = ti; }

            [Config("$name")]
            private string m_Name;

            [Config("$unit", "times")]
            private string m_Unit;

        #endregion

        #region Public/Props

            /// <summary>
            /// References the Throttling that this instance is registered with
            /// </summary>
            public IThrottling Throttling
            {
                get { return m_ThrottlingImpl; }
            }

            /// <summary>
            /// Returns the unique name of this instance
            /// </summary>
            public string Name { get { return m_Name; } }

            /// <summary>
            /// Returns the name of unit of measurement for this instance
            /// </summary>
            public string Unit { get { return m_Unit; } }

            /// <summary>
            /// Checks whether the current throttle rate limit has not been reached
            /// per one unit of measurement.  This method is usually used to throttle based
            /// on pass count
            /// </summary>
            public virtual bool Try() { return Try(App.TimeSource.UTCNow, 1.0); }

            /// <summary>
            /// Checks whether the current throttle rate limit has not been reached
            /// for the provided value.  This method is usually used to throttle based
            /// on values like financial amounts, data volumes, user counts, etc.
            /// </summary>
            public virtual bool Try(double value) { return Try(App.TimeSource.UTCNow, value); }

            /// <summary>
            /// Reset the internal state of the throttle
            /// </summary>
            public abstract void Reset();

            /// <summary>
            /// Register this instance with throttling container
            /// </summary>
            public void Register(IThrottling throttling)
            {
                throttling.RegisterThrottle(this);
            }

            /// <summary>
            /// Unregister this instance from throttling container
            /// </summary>
            public bool Unregister()
            {
                var impl = m_ThrottlingImpl;
                if (impl != null)
                    return impl.UnregisterThrottle(this);
                return false;
            }

            public void Configure(IConfigSectionNode node)
            {
                ConfigAttribute.Apply(this, node);
                DoConfigure(node);
            }

        #endregion

        #region Protected

            /// <summary>
            /// Update current throttle timestamp
            /// </summary>
            internal abstract bool Try(DateTime time, double value);

            protected virtual void DoConfigure(IConfigSectionNode node) {}

        #endregion
    }
}
