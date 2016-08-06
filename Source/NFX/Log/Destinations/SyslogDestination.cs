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

using NFX.Log.Syslog;

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Implements destination that sends messages to UNIX syslog using UDP datagrams
    /// </summary>
    public class SyslogDestination : Destination
    {

        #region .ctor

        /// <summary>
        /// Creates a new instance of destination that sends messages to .nix SYSLOG
        /// </summary>
        public SyslogDestination() : base(null)
        {
          m_Client = new SyslogClient();
        }

        /// <summary>
        /// Creates a new instance of destination that sends messages to .nix SYSLOG
        /// </summary>
        public SyslogDestination(string name, string host, int port) : base(name)
        {
          m_Client = new SyslogClient(host, port);
        }

        protected override void Destructor()
        {
          m_Client.Dispose();
          base.Destructor();
        }
      #endregion


      #region Private Fields

        private SyslogClient m_Client;

      #endregion


      #region Properties

       /// <summary>
       /// References the underlying syslog client instance
       /// </summary>
       public SyslogClient Client
       {
         get { return m_Client; }
       }

      #endregion

      #region Public


       public override void Close()
       {
           m_Client.Close();
           base.Close();
       }

      #endregion


      #region Protected


        protected override void DoConfigure(Environment.IConfigSectionNode node)
        {
            base.DoConfigure(node);
            m_Client.Configure(node);
        }

        protected internal override void DoSend(Message entry)
        {
           m_Client.Send(new SyslogMessage(entry));
        }

      #endregion

    }
}
