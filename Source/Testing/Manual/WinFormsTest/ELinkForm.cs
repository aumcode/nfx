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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NFX.DataAccess.Distributed;

using NFX;
using NFX.Security.CAPTCHA;

namespace WinFormsTest
{
    public partial class ELinkForm : Form
    {
        public ELinkForm()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
          var lnk = new ELink(new GDID((uint)sbEra.Value, 12,(ulong)tbID.Text.AsLong(0)), null);
          tbELink.Text = lnk.Link;

          var lnk2 = new ELink( lnk.Link );

          tbResult.Text = lnk2.GDID.ToString();
        }

        private void sb_Scroll(object sender, ScrollEventArgs e)
        {
            tbID.Text = sb.Value.ToString();
            btnCalculate_Click(null ,null);
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            var lnk = new ELink((ulong)tbID.Text.AsLong(0), null);
            tbELink.Text = lnk.Link;

            var lnk2 = new ELink( lnk.Link );

            tbResult.Text = lnk2.ID.ToString(); 
        }

        private void btnBigID_Click(object sender, EventArgs e)
        {
            ulong id = 182500000000;// ulong.MaxValue;
            
            var lnk = new ELink(id, null);// new byte[]{ 123, 18});
            tbELink.Text = lnk.Link;
        }

        private void btnDecode_Click(object sender, EventArgs e)
        {
            var lnk2 = new ELink( tbELink.Text );

            tbResult.Text = lnk2.GDID.ToString();
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
           Text = "Score: {0}   Easy%: {1}  Normal%: {2} Hard%: {3}".Args(
                       NFX.Security.PasswordUtils.PasswordStrengthScore(tbPassword.Text),
                       NFX.Security.PasswordUtils.PasswordStrengthPercent(tbPassword.Text, NFX.Security.PasswordUtils.TOP_SCORE_EASY),
                       NFX.Security.PasswordUtils.PasswordStrengthPercent(tbPassword.Text, NFX.Security.PasswordUtils.TOP_SCORE_NORMAL),
                       NFX.Security.PasswordUtils.PasswordStrengthPercent(tbPassword.Text, NFX.Security.PasswordUtils.TOP_SCORE_HARD));

        }

       
        private void sbv_Scroll(object sender, ScrollEventArgs e)
        {
          var img = cb2.Checked ? pb3.Image : pb1.Image;
          pb2.Image = img.NormalizeCenteredImage(sbh.Value, sbv.Value);
          Text = "{0} x {1}".Args(sbh.Value, sbv.Value);
        }

        private void ELinkForm_Load(object sender, EventArgs e)
        {
          sbv_Scroll(null, null);
        }

        private void sbEra_Scroll(object sender, ScrollEventArgs e)
        {
         button1_Click(null, null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
           var lnk = new ELink((ulong)tbID.Text.AsLong(0), null);
           lnk.Encode(1);
           tbELink.Text = lnk.Link;

           var lnk2 = new ELink( lnk.Link );

           tbResult.Text = lnk2.ID.ToString(); 
        }

        
        public static string a1 = ExternalRandomGenerator.Instance.NextRandomWebSafeString().Substring(0, 2);
        string a2;

        private void btnPuzzle_Click(object sender, EventArgs e)
        {
           var pk = new PuzzleKeypad(NFX.Parsing.NaturalTextGenerator.Generate(16));
           var img = pk.DefaultRender(Color.White, false);
           pic.Image = img;

          // a1 = "a";
           a2 = tbPassword.Text;

           Text = "'{0}' ref eq '{1}' is {2}, == is {3} ".Args(a1, a2, object.ReferenceEquals(a1,a2), a1==a2);
        }

        
    }
}
