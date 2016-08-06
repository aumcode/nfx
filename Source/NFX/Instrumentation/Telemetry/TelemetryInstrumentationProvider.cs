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

using NFX.Log;
using NFX.Environment;

namespace NFX.Instrumentation.Telemetry
{
    /// <summary>
    /// Represents a provider that writes aggregated datums into remote telemetry receiver
    /// </summary>
    public class TelemetryInstrumentationProvider : LogInstrumentationProvider
    {
       #region .ctor

        public TelemetryInstrumentationProvider() : base()
        {

        }

        protected override void Destructor()
        {
            cleanupClient();
            base.Destructor();
        }

       #endregion

       #region Properties

        /// <summary>
        /// Determines whether to write to log as well
        /// </summary>
        [Config]
        public bool UseLog { get; set;}

        /// <summary>
        /// Provides remote telemetry receiver node
        /// </summary>
        [Config]
        public string ReceiverNode { get; set; }

        /// <summary>
        /// Provides name for reporting site, if this property is blank then App.Name is used instead
        /// </summary>
        [Config]
        public string SiteName { get; set; }

       #endregion





        private TelemetryReceiverClient m_Client;


        public override void Write(Datum aggregatedDatum)
        {
           if (!App.Available) return;

           if (UseLog)
             base.Write(aggregatedDatum);

           var node = ReceiverNode;

           if (node.IsNullOrWhiteSpace()) return;


           string site;
           if (SiteName.IsNotNullOrWhiteSpace())
             site = SiteName;
           else
             site = "{0}::{1}@{2}".Args(App.Name, App.InstanceID, System.Environment.MachineName);

           try
           {
               if (m_Client==null)
                   m_Client = new TelemetryReceiverClient(ReceiverNode);

               m_Client.Send(site, aggregatedDatum);
           }
           catch(Exception error)
           {
               cleanupClient();
               WriteLog(MessageType.Error, error.ToMessageWithType(), from: "{0}.{1}".Args(GetType().Name, "Write(datum)") );
           }
        }

        private void cleanupClient()
        {
           var cl = m_Client;
           if (cl==null) return;

           try
           {
               m_Client = null;
               cl.Dispose();
           }
           catch(Exception error)
           {
              WriteLog(MessageType.Error, error.ToMessageWithType(), from: "{0}.{1}".Args(GetType().Name, "cleanupClient()") );
           }
        }

    }
}
