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
using System.Net.Mail;

using NFX.Environment;

namespace NFX.Log.Destinations
{

    /// <summary>
    /// Implements log destination that sends emails
    /// </summary>
    public class SMTPDestination : Destination
    {
        #region CONSTS


             public const int DEFAULT_SMTP_PORT = 587;



        #endregion



        #region .ctor

            /// <summary>
            /// Creates a new instance of destination that sends EMails
            /// </summary>
            public SMTPDestination() : base(null)
            {
              SmtpPort = DEFAULT_SMTP_PORT;
            }

            /// <summary>
            /// Creates a new instance of destination that sends EMails
            /// </summary>
            public SMTPDestination(string name, string host, int port, bool ssl) : base(name)
            {
              SmtpHost = host;
              SmtpPort = port;
              SmtpSSL = ssl;
            }

            protected override void Destructor()
            {
              base.Destructor();
            }
      #endregion


      #region Private Fields

        private SmtpClient m_Smtp;

      #endregion


      #region Properties

        [Config]
        public string SmtpHost { get; set; }
        [Config]
        public int SmtpPort { get; set; }

        [Config]
        public bool SmtpSSL { get; set; }



        [Config]
        public string FromAddress { get; set; }
        [Config]
        public string FromName { get; set; }

        [Config]
        public string ToAddress { get; set; }

        [Config]
        public string CredentialsID { get; set; }
        [Config]
        public string CredentialsPassword { get; set; }


        [Config]
        public string Subject { get; set; }

        [Config]
        public string Body { get; set; }

      #endregion

      #region Public


       public override void Open()
       {
           base.Open();

           m_Smtp =
               new SmtpClient
               {
                   Host = this.SmtpHost,
                   Port = this.SmtpPort,
                   EnableSsl = this.SmtpSSL,
                   DeliveryMethod = SmtpDeliveryMethod.Network,
                   UseDefaultCredentials = false,
                   Credentials = new NetworkCredential(this.CredentialsID, this.CredentialsPassword)
               };
       }

       public override void Close()
       {
           if (m_Smtp!=null)
           {
             m_Smtp.Dispose();
             m_Smtp = null;
           }
           base.Close();
       }

      #endregion


      #region Protected

        protected internal override void DoSend(Message entry)
        {
           if (string.IsNullOrEmpty(ToAddress)) return;

           var from = new MailAddress(this.FromAddress, this.FromName);
           var to = new MailAddress(ToAddress);


           using (var email = new MailMessage(from, to))
           {

                email.Subject = this.Subject ?? entry.Topic;
                email.Body = (this.Body??string.Empty) + entry.ToString();//for now
                email.CC.Add(ToAddress);

                m_Smtp.Send(email);
           }
        }

      #endregion

    }
}
