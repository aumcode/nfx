/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NFX;
using NFX.Web.Messaging;
using WinFormsTest.Properties;

namespace WinFormsTest
{
    public partial class MailForm : Form
    {
        public MailForm()
        {
            InitializeComponent();
        }

        private void btSend_Click(object sender, EventArgs e)
        {
          // note that ToAddress has complex structure - laconic config, look MessageBuilder.Addressee
          var message = new NFX.Web.Messaging.Message(null)
          {
            AddressFrom = tbFROMAddress.Text,
            AddressTo = tbTOAddress.Text,
            Subject = tbSubject.Text,
            Body = tbBody.Text,
            RichBody = tbHTML.Text
          };

          if (includeAttachments.Checked)
          {
            ImageConverter converter = new ImageConverter();
            var imageBytes = (byte[])converter.ConvertTo(Resources._20140601_204233, typeof(byte[]));

            message.Attachments = new NFX.Web.Messaging.Message.Attachment[]
                          {new NFX.Web.Messaging.Message.Attachment("photo1", imageBytes, NFX.Web.ContentType.JPEG), new NFX.Web.Messaging.Message.Attachment("photo2", imageBytes, NFX.Web.ContentType.JPEG)};
          }

          MessageService.Instance.SendMsg(message);
        }
    }
}
