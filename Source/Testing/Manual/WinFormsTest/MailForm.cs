using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NFX.Web.Messaging;

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
            var message = new NFX.Web.Messaging.Message(null)
            {
                FROMName = tbFROMName.Text,
                FROMAddress = tbFROMAddress.Text,
                TOName = tbTOName.Text,
                TOAddress = tbTOAddress.Text,
                Subject = tbSubject.Text,
                Body = tbBody.Text,
                HTMLBody = tbHTML.Text
            };

            MessageService.Instance.SendMsg(message);
        }
    }
}
