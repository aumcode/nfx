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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Log;
using NFX.ServiceModel;

namespace NFX.Throttling
{
    /// <summary>
    /// A service that maintains throttling components in an application and
    /// allows to turn on/off global throttling functionality
    /// </summary>
    [ConfigMacroContext]
    public class ThrottlingService : Service, IThrottlingImplementation
    {
        #region CONSTS

            // private const string THREAD_NAME = "ThrottlingSvc Thread";
            public const string CONFIG_THROTTLE_SECTION = "throttle";

        #endregion

        #region .ctor

            public ThrottlingService() : base(null) {}

            protected override void Destructor()
            {
                base.Destructor();
            }

        #endregion

        #region Private Fields

            private ConcurrentDictionary<string,Throttle> m_Throttles =
                new ConcurrentDictionary<string,Throttle>(StringComparer.InvariantCultureIgnoreCase);

        #endregion

        #region Properties

            public override string ComponentCommonName { get { return "throttling"; }}

            public bool Enabled { get { return true; } }

        #endregion

        #region Public

            /// <summary>
            /// Resolve a throttle instance by name
            /// </summary>
            public IThrottle this[string name]
            {
                get
                {
                    if (Status != ControlStatus.Active)
                        return NOPThrottle.Instance;

                    Throttle result;
                    if (m_Throttles.TryGetValue(name, out result))
                        return result;
                    return NOPThrottle.Instance;
                }
            }

            /// <summary>
            /// Resolve a typed throttle by name
            /// </summary>
            public T Get<T>(string name) where T : Throttle
            {
                return this[name] as T;
            }

            /// <summary>
            /// Register a throttle with this throttling container
            /// </summary>
            public void RegisterThrottle(Throttle throttle)
            {
                if (throttle == null)
                    throw new NFXException(StringConsts.ARGUMENT_ERROR + "RegisterThrottle(NULL)");
                lock (m_Throttles)
                {
                    if (!m_Throttles.TryAdd(throttle.Name, throttle))
                        throw new NFXException(StringConsts.ARGUMENT_ERROR + throttle.Name + " throttle already exists");
                    throttle.__setThrottling(this);
                }
            }

            /// <summary>
            /// Unregister a throttle from this throttling container
            /// </summary>
            /// <returns>True if throttle was successfully unregistered</returns>
            public bool UnregisterThrottle(Throttle throttle)
            {
                if (throttle == null)
                    throw new NFXException(StringConsts.ARGUMENT_ERROR + "UnregisterThrottle(NULL)");
                Throttle value;
                lock (m_Throttles)
                {
                    bool ok = m_Throttles.TryRemove(throttle.Name, out value);
                    if (ok)
                        throttle.__setThrottling(null);
                    return ok;
                }
            }

        #endregion

        #region Protected

            protected override void DoConfigure(IConfigSectionNode node)
            {
                base.DoConfigure(node);

                foreach (ConfigSectionNode tnode in
                    node.Children.Where(
                        n => string.Equals(
                            n.Name, CONFIG_THROTTLE_SECTION, StringComparison.OrdinalIgnoreCase) )
                    )
                {
                    Throttle thr = FactoryUtils.MakeAndConfigure(tnode) as Throttle;

                    this.RegisterThrottle(thr);
                }
            }

            protected override void DoStart()
            {
                base.DoStart();

                lock(m_Throttles)
                    foreach (var t in m_Throttles)
                        t.Value.Reset();
            }

        #endregion

        #region .pvt. impl.

            private void log(MessageType type, string message, string parameters)
            {
                App.Log.Write(
                    new Log.Message
                    {
                        Text = message ?? string.Empty,
                        Type = type,
                        From = this.Name,
                        Topic = CoreConsts.THROTTLINGSVC_TOPIC,
                        Parameters = parameters ?? string.Empty
                    }
                );
        }

        #endregion

    }
}
