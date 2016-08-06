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
using System.Net;
using System.Net.Sockets;

using NFX.Environment;

namespace NFX.Log.Syslog
{

    /// <summary>
    /// Implements SYSLOG UDP client
    /// </summary>
    public sealed class SyslogClient : DisposableObject, IConfigurable
    {
       #region CONSTS

         public const int SYSLOG_PORT = 514;

       #endregion


       #region .ctor

         public SyslogClient()
         {

         }

         public SyslogClient(string host): this(host, SYSLOG_PORT)
         {

         }

         public SyslogClient(string host, int port)
         {
            m_Host = host;
            m_Port = port;
         }

         protected override void Destructor()
         {
             close();
             base.Destructor();
         }


       #endregion


       #region Private Fields

         private string m_Host;
         private int m_Port = SYSLOG_PORT;

         private object m_SocketSync = new Object();
         private UdpClient m_UdpClient;
       #endregion


       #region Properties

         [Config("$host")]
         public string Host
         {
           get { return m_Host;}
           set
           {
             if (m_Host != value)
             {
                m_Host = value;
                close();
             }
           }
         }

         [Config("$port")]
         public int Port
         {
           get { return m_Port;}
           set
           {
             if (m_Port != value)
             {
                m_Port = value;
                close();
             }
           }
         }


       #endregion


       #region Public


         public void Configure(IConfigSectionNode node)
         {
             ConfigAttribute.Apply(this, node);
         }


         public void Send(SyslogMessage message)
         {
           var dgram = string.Format(
                                    "<{0}>{1}: {2} {3}",
                                    message.Priority,
                                    message.LocalTimeStamp.ToString("MMM dd HH:mm:ss"),
                                    System.Environment.MachineName,
                                    message.Text);


           var buf = System.Text.Encoding.ASCII.GetBytes(dgram);

           lock(m_SocketSync)
           {
             open();
             m_UdpClient.Send(buf, buf.Length);
           }
         }


         public void Close()
         {
           close();
         }


       #endregion



       #region .pvt .impl

         private void close()
         {
           lock (m_SocketSync)
           {
             if (m_UdpClient!=null)
             {
               m_UdpClient.Close();
               m_UdpClient = null;
             }
           }
         }

         private void open()
         {
           lock (m_SocketSync)
           {
             if (m_UdpClient==null)
             {
               var cl  = new UdpClient();
               cl.Connect(m_Host, m_Port);
               m_UdpClient = cl;
             }
           }
         }



       #endregion



    }

}
