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
    /// Implements a destination that is based on another instance of LogService, which provides asynchronous buffering and failover capabilities.
    /// </summary>
    public class LogServiceDestination : Destination
    {
       #region .ctor
        public LogServiceDestination() : base(null)
        {

        }

        public LogServiceDestination(string name) : base(name)
        {
        }

        protected override void Destructor()
        {
          base.Destructor();
        }
      #endregion

       #region Pvt Fields

        private LogService  m_Service = new LogService(null);
      #endregion


      #region Properties


      #endregion

      #region Public


           public override void Open()
           {
               base.Open();
               m_Service.Start();
           }

           public override void Close()
           {
               m_Service.WaitForCompleteStop();
               base.Close();
           }

      #endregion



        #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
            base.DoConfigure(node);
            m_Service.Configure(node);
        }


        protected internal override void DoSend(Message msg)
        {
           m_Service.Write(msg);
        }

      #endregion
    }
}
