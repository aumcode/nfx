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

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Implements msg sink based on SMTPClient
  /// </summary>
  public class SMTPMessageSink : MessageSink
  {
    #region CONSTS
         public const int DEFAULT_SMTP_PORT = 587;
    #endregion

    #region .ctor

        public SMTPMessageSink(MessageService director) : base(director)
        {
          SmtpPort = DEFAULT_SMTP_PORT;
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

        [Config(Default=DEFAULT_SMTP_PORT)]
        public int SmtpPort { get; set; }

        [Config]
        public bool SmtpSSL { get; set; }

        [Config]
        public string CredentialsID { get; set; }

        [Config]
        public string CredentialsPassword { get; set; }

        [Config]
        public string DefaultFromAddress { get; set; }

        [Config]
        public string DefaultFromName { get; set; }

        [Config]
        public string DropFolder { get; set; }


      #endregion


        protected override void DoStart()
        {
          m_Smtp = new SmtpClient();

          if (DropFolder.IsNotNullOrWhiteSpace() && System.IO.Directory.Exists(DropFolder))
          {
             m_Smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
             m_Smtp.PickupDirectoryLocation = DropFolder;
          }
          else
          {
             if (SmtpHost.IsNullOrWhiteSpace())
             throw new WebException(StringConsts.MAILER_SINK_SMTP_IS_NOT_CONFIGURED_ERROR+"SmtpHost==null|empty|0");


             m_Smtp.Host = this.SmtpHost;
             m_Smtp.Port = this.SmtpPort;
             m_Smtp.EnableSsl = this.SmtpSSL;
             m_Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
             m_Smtp.UseDefaultCredentials = false;
             m_Smtp.Credentials = new NetworkCredential(this.CredentialsID, this.CredentialsPassword);
          }
        }

        protected override void DoWaitForCompleteStop()
        {
          if (m_Smtp!=null)
          {
             m_Smtp.Dispose();
             m_Smtp = null;
          }
        }


    protected override void DoSendMsg(Message msg)
    {
        if (msg==null || msg.TOAddress.IsNullOrWhiteSpace()) return;

           var fa = msg.FROMAddress;
           var fn = msg.FROMName;

           if (fa.IsNullOrWhiteSpace()) fa = DefaultFromAddress;
           if (fn.IsNullOrWhiteSpace()) fn = DefaultFromName;


           var from = new MailAddress(fa, fn);
           var to = new MailAddress(msg.TOAddress, msg.TOName);


           using (var email = new MailMessage(from, to))
           {

                email.Subject = msg.Subject;

                if (msg.CC.IsNotNullOrWhiteSpace())  email.CC.Add(msg.CC);

                if (msg.BCC.IsNotNullOrWhiteSpace()) email.Bcc.Add(msg.BCC);

                if (msg.HTMLBody.IsNullOrWhiteSpace())
                {
                  email.Body = msg.Body;
                }
                else
                {
                  if (msg.Body.IsNullOrWhiteSpace())
                  {
                    email.IsBodyHtml = true;
                    email.Body = msg.HTMLBody;
                  }
                  else
                  {
                    email.Body = msg.Body;
                    var alternateHTML = AlternateView.CreateAlternateViewFromString(msg.HTMLBody, new System.Net.Mime.ContentType(ContentType.HTML));
                    email.AlternateViews.Add(alternateHTML);
                  }
                }

                if (msg.Attachments!=null)
                 foreach(var att in msg.Attachments.Where(a => a.Content != null))
                 {
                   var ema = new Attachment(new System.IO.MemoryStream(att.Content), new System.Net.Mime.ContentType( att.ContentType ));

                   if (att.Name.IsNotNullOrWhiteSpace())
                     ema.ContentDisposition.FileName = att.Name;

                   email.Attachments.Add( ema );
                 }

                m_Smtp.Send(email);
           }
    }
  }
}
